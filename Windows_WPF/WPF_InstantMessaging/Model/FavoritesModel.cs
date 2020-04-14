using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Rainbow;
using Rainbow.Model;

using InstantMessaging.Helpers;
using InstantMessaging.Pool;
using InstantMessaging.ViewModel;

using log4net;

namespace InstantMessaging.Model
{
    public class FavoritesModel : ObservableObject
    {
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(LoginModel));
        App CurrentApplication = (App)System.Windows.Application.Current;

        private Bubbles RbBubbles = null;
        private Contacts RbContacts = null;
        private Conversations RbConversations = null;
        private Favorites RbFavorites = null;

        private readonly AvatarPool AvatarPool; // To manage avatars

        private readonly Object lockObservableFavorites = new Object(); // To lock access to the observable collection: 'Conversations'

        public RangeObservableCollection<FavoriteViewModel> FavoritesList { get; set; } // Need to be public - Used as Binding from XAML

        public FavoritesModel()
        {
            //FavoritesList = new RangeObservableCollection<FavoriteViewModel>(new FavoriteViewModel.FavoriteViewModelComparer());
            FavoritesList = new RangeObservableCollection<FavoriteViewModel>();

            if (CurrentApplication.USE_DUMMY_DATA)
            {
                LoadFakeConversations();
                return;
            }

            // Get Rainbow SDK Objects
            RbBubbles = CurrentApplication.RbBubbles;
            RbContacts = CurrentApplication.RbContacts;
            RbConversations = CurrentApplication.RbConversations;
            RbFavorites = CurrentApplication.RbFavorites;

            // Manage event(s) from AvatarPool
            AvatarPool = AvatarPool.Instance;
            AvatarPool.ContactAvatarChanged += AvatarPool_ContactAvatarChanged;
            AvatarPool.BubbleAvatarChanged += AvatarPool_BubbleAvatarChanged;

            // Manage event(s) from Rainbow SDK about FAVORITES
            RbFavorites.FavoriteCreated += RbFavorites_FavoriteCreated;
            RbFavorites.FavoriteRemoved += RbFavorites_FavoriteRemoved;
            RbFavorites.FavoriteUpdated += RbFavorites_FavoriteUpdated;

            // Manage event(s) from Rainbow SDK about CONVERSATIONS
            // RbConversations.ConversationRemoved += RbConversations_ConversationRemoved; // A converation removed doesn't mean the favorties is removed
            RbConversations.ConversationUpdated += RbConversations_ConversationUpdated;

            // Manage event(s) from Rainbow SDK about CONTACTS
            RbContacts.ContactPresenceChanged += RbContacts_ContactPresenceChanged;
            RbContacts.ContactAdded += RbContacts_ContactAdded;
            RbContacts.ContactInfoChanged += RbContacts_ContactInfoChanged;

            // Manage event(s) from Rainbow SDK about BUBBLES
            RbBubbles.BubbleInfoUpdated += RbBubbles_BubbleInfoUpdated;

            // Set favorites list using cache
            Task task = new Task(() =>
            {
                // Get favorites list
                List<Favorite> list = RbFavorites.GetFavorites();

                if ((list != null) && (list.Count > 0))
                {
                    // We need to update UI so we nend to be on correct Thread
                    if (System.Windows.Application.Current != null)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            ResetModelWithRbFavorites(list);
                        }));
                    }
                }
            });
            task.Start();
        }


        #region MODEL UPDATED IN THESE METHODS

        public void ResetModelWithRbFavorites(List<Rainbow.Model.Favorite> rbFavorites)
        {
            // First clear the list
            FavoritesList.Clear();

            foreach (Rainbow.Model.Favorite favorite in rbFavorites)
                AddRbFavoriteToModel(favorite);
        }

        public void AddRbFavoriteToModel(Rainbow.Model.Favorite favorite)
        {
            if (favorite != null)
            {
                // Now add it to the model
                FavoriteViewModel newFavorite;
                newFavorite = Helper.GetFavoriteFromRbFavorite(favorite);
                if (newFavorite != null)
                {
                    newFavorite.AvatarImageSource = Helper.GetConversationAvatarImageSourceByPeerId(favorite.PeerId);
                    AddFavoriteToModel(newFavorite);
                }
            }
        }

        public void RemoveRBFavoriteFromModelByFavoriteId(String favoriteId)
        {
            FavoriteViewModel result = GetFavoriteById(favoriteId);

            if (result != null)
            {
                lock (lockObservableFavorites)
                    FavoritesList.Remove(result);
            }
        }

        public void RemoveRBFavoriteFromModelByPeerId(String peerId)
        {
            FavoriteViewModel result = GetFavoriteByPeerId(peerId);

            if (result != null)
            {
                lock (lockObservableFavorites)
                    FavoritesList.Remove(result);
            }
        }

        private void AddFavoriteToModel(FavoriteViewModel newFavorite)
        {
            if (newFavorite != null)
            {
                // First remove this favorite if already in the model
                RemoveRBFavoriteFromModelByFavoriteId(newFavorite.Id);

                lock (lockObservableFavorites)
                {
                    Boolean itemAdded = false;
                    int nb = FavoritesList.Count;
                    for (int i = 0; i < nb; i++)
                    {
                        if (newFavorite.Position < FavoritesList[i].Position)
                        {
                            FavoritesList.Insert(i, newFavorite);
                            log.DebugFormat("[AddFavoriteToModel] INSERT Favorite.id:[{0}] IN index:[{1}]", newFavorite.Id, i);
                            itemAdded = true;
                            break;
                        }
                    }
                    if (!itemAdded)
                    {
                        FavoritesList.Add(newFavorite);
                        log.DebugFormat("[AddFavoriteToModel] ADD Favorite.id:[{0}] ", newFavorite.Id);
                    }
                }
            }
        }

        public void UpdateFavoriteNameByPeerId(string peerId, String name)
        {
            FavoriteViewModel result = GetFavoriteByPeerId(peerId);
            if (result != null)
                result.Name = name;
        }

        public void UpdateFavoriteUsingPresence(String jid, Presence presence)
        {
            FavoriteViewModel result = GetFavoriteByJid(jid);
            if (result != null)
                result.PresenceSource = InstantMessaging.Helpers.Helper.GetPresenceSourceFromPresence(presence, jid == CurrentApplication.CurrentUserJid);
        }

        public void UpdateFavoriteUsingUpdatedConversation(Conversation conversation)
        {
            FavoriteViewModel result = GetFavoriteByPeerId(conversation.PeerId);
            if (result != null)
            {
                result.NbMsgUnread = conversation.UnreadMessageNumber;
            }
        }


        #endregion MODEL UPDATED IN THESE METHODS

        #region AVATAR POOL EVENTS
        private void AvatarPool_BubbleAvatarChanged(object sender, Rainbow.Events.IdEventArgs e)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    FavoriteViewModel result = GetFavoriteByPeerId(e.Id);
                    if (result != null)
                    {
                        log.DebugFormat("[AvatarPool_BubbleAvatarChanged] - bubbleId:[{0}] - favoriteId:[{1}]", e.Id, result.Id);
                        result.AvatarImageSource = Helper.GetBubbleAvatarImageSource(e.Id);
                    }
                    //else
                    //{
                    //    log.DebugFormat("[AvatarPool_BubbleAvatarChanged] - no favorite found:[{0}]", e.Id);
                    //}
                }));
            }
        }

        private void AvatarPool_ContactAvatarChanged(object sender, Rainbow.Events.IdEventArgs e)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    FavoriteViewModel result = GetFavoriteByPeerId(e.Id);
                    if (result != null)
                    {
                        log.DebugFormat("[AvatarPool_ContactAvatarChanged] - contactId:[{0}] - favoriteId:[{1}]", e.Id, result.Id);
                        result.AvatarImageSource = Helper.GetContactAvatarImageSource(e.Id);
                    }
                    //else
                    //{
                    //    log.DebugFormat("[AvatarPool_ContactAvatarChanged] - Avatar contactId:[{0}] but no conversation found ...", e.Id);
                    //}
                }));
            }
        }
        #endregion AVATAR POOL EVENTS

        #region EVENTS FIRED BY RAINBOW SDK

        private void RbFavorites_FavoriteUpdated(object sender, Rainbow.Events.FavoriteEventArgs e)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    AddRbFavoriteToModel(e.Favorite);
                }));
            }
        }

        private void RbFavorites_FavoriteRemoved(object sender, Rainbow.Events.FavoriteEventArgs e)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    RemoveRBFavoriteFromModelByFavoriteId(e.Favorite.Id);
                }));
            }
        }

        private void RbFavorites_FavoriteCreated(object sender, Rainbow.Events.FavoriteEventArgs e)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    AddRbFavoriteToModel(e.Favorite);
                }));
            }
        }

        private void RbBubbles_BubbleInfoUpdated(object sender, Rainbow.Events.BubbleInfoEventArgs e)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    UpdateFavoriteNameByPeerId(e.BubbleId, e.Name);
                }));
            }
        }

        private void RbContacts_ContactAdded(object sender, Rainbow.Events.JidEventArgs e)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Rainbow.Model.Contact contact = RbContacts.GetContactFromContactJid(e.Jid);
                    if (contact != null)
                        UpdateFavoriteNameByPeerId(contact.Id, Util.GetContactDisplayName(contact, AvatarPool.GetFirstNameFirst()));
                }));
            }
        }

        private void RbContacts_ContactInfoChanged(object sender, Rainbow.Events.JidEventArgs e)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Rainbow.Model.Contact contact = RbContacts.GetContactFromContactJid(e.Jid);
                    if (contact != null)
                        UpdateFavoriteNameByPeerId(contact.Id, Util.GetContactDisplayName(contact, AvatarPool.GetFirstNameFirst()));
                }));
            }
        }

        private void RbContacts_ContactPresenceChanged(object sender, Rainbow.Events.PresenceEventArgs e)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    UpdateFavoriteUsingPresence(e.Jid, e.Presence);
                }));
            }
        }

        private void RbConversations_ConversationUpdated(object sender, Rainbow.Events.ConversationEventArgs e)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (e.Conversation != null)
                        UpdateFavoriteUsingUpdatedConversation(e.Conversation);
                }));
            }
        }

        #endregion EVENTS FIRED BY RAINBOW SDK

        private FavoriteViewModel GetFavoriteByPeerId(String peerId)
        {
            FavoriteViewModel result = null;
            lock (lockObservableFavorites)
            {
                foreach (FavoriteViewModel favorite in FavoritesList)
                {
                    if (favorite.PeerId == peerId)
                    {
                        result = favorite;
                        break;
                    }
                }
            }
            return result;
        }

        private FavoriteViewModel GetFavoriteById(String id)
        {
            FavoriteViewModel result = null;
            lock (lockObservableFavorites)
            {
                foreach (FavoriteViewModel favorite in FavoritesList)
                {
                    if (favorite.Id == id)
                    {
                        result = favorite;
                        break;
                    }
                }
            }
            return result;
        }

        private FavoriteViewModel GetFavoriteByJid(String jid)
        {
            FavoriteViewModel result = null;
            lock (lockObservableFavorites)
            {
                foreach (FavoriteViewModel favorite in FavoritesList)
                {
                    if (favorite.Jid == jid)
                    {
                        result = favorite;
                        break;
                    }
                }
            }
            return result;
        }

        private void LoadFakeConversations()
        {
            //List<FavoriteViewModel> favoritesList = new List<FavoriteViewModel>();

            FavoriteViewModel favorite = null;

            // SET NEW FAKE Conversation Light
            favorite = new FavoriteViewModel();
            favorite.Id = "convID1";
            favorite.Name = "User Name1";
            favorite.PresenceSource = "presence_online.png";
            favorite.NbMsgUnread = favorite.Position = 1;
            favorite.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            FavoritesList.Add(favorite);

            // SET NEW FAKE Conversation Light
            favorite = new FavoriteViewModel();
            favorite.Id = "convID2";
            favorite.Name = "User Name2 avec tres tres tres tres long";
            favorite.PresenceSource = "presence_busy.png";
            favorite.NbMsgUnread = favorite.Position = 2;
            favorite.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            FavoritesList.Add(favorite);

            // SET NEW FAKE Conversation Light
            favorite = new FavoriteViewModel();
            favorite.Id = "convID3";
            favorite.Name = "Group Name3";
            favorite.PresenceSource = "";
            favorite.NbMsgUnread = favorite.Position = 3;
            favorite.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            FavoritesList.Add(favorite);

            // SET NEW FAKE Conversation Light
            favorite = new FavoriteViewModel();
            favorite.Id = "convID4";
            favorite.Name = "Group Name4 tres tres tres long";
            favorite.PresenceSource = "";
            favorite.NbMsgUnread = favorite.Position = 6;
            favorite.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            FavoritesList.Add(favorite);

            // SET NEW FAKE Conversation Light
            favorite = new FavoriteViewModel();
            favorite.Id = "convID5";
            favorite.Name = "Group Name6";
            favorite.PresenceSource = "";
            favorite.NbMsgUnread = favorite.Position = 5;
            favorite.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            FavoritesList.Add(favorite);

            // SET NEW FAKE Conversation Light
            favorite = new FavoriteViewModel();
            favorite.Id = "convID6";
            favorite.Name = "Group Name6";
            favorite.PresenceSource = "";
            favorite.NbMsgUnread = favorite.Position = 4;
            favorite.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            FavoritesList.Add(favorite);

            // SET NEW FAKE Conversation Light
            favorite = new FavoriteViewModel();
            favorite.Id = "convID7";
            favorite.Name = "Group Name7 tres tres tres long";
            favorite.PresenceSource = "";
            favorite.NbMsgUnread = favorite.Position = 0;
            favorite.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            FavoritesList.Add(favorite);
        }
    }
}
