using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Rainbow;
using Rainbow.Model;

using MultiPlatformApplication.Helpers;

using NLog;
using System.Collections.ObjectModel;
using MultiPlatformApplication.Models;
using System.Windows.Input;
using System.Threading.Tasks;

namespace MultiPlatformApplication.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ConversationsViewModel : ConversationModel
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(ConversationsViewModel));

        private App XamarinApplication;
        
        private Boolean firstInitialization = true;

        private readonly Object lockObservableConversations = new Object(); // To lock access to the observable collection: 'Conversations'

        // To know the dateTime of the most recent message received - needed to update the layout
        private DateTime mostRecentDateTimeMessage = DateTime.MinValue;
        private CancelableDelay delayUpdateForDateTimePurpose = null;

#region BINDINGS used by XAML

        // We have a dynamic list of 'ConversationModel' to manage
        public DynamicListModel<ConversationModel> DynamicList { get; private set; } = new DynamicListModel<ConversationModel>();

#endregion


#region PUBLIC METHODS

        public ConversationsViewModel()
        {
            // Initialize DynamicList commands
            DynamicList.AskMoreItemsCommand = new RelayCommand<object>(new Action<object>(ConversationLoadMoreItems));
        }

        public void Initialize()
        {
            if(firstInitialization)
            {
                firstInitialization = false;
                InitializeSdkObjectsAndEvents();

                // Need to manage some events of the collection view
                if(DynamicList.ListView != null)
                    DynamicList.ListView.ItemSelected += ListView_ItemSelected;

                // Now ask to load more items
                DynamicList.AskingMoreItems = true;
                DynamicList.ListView.BeginRefresh();
            }

            // Reset any selection
            if (DynamicList.ListView != null)
                DynamicList.ListView.SelectedItem = null;
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            if (e.SelectedItem is ConversationModel)
            {
                String conversationId = ((ConversationModel)e.SelectedItem)?.Id;
                if (String.IsNullOrEmpty(conversationId))
                    return;

                XamarinApplication.CurrentConversationId = conversationId;
                
                ConversationStreamPage conversationStreamPage;
                conversationStreamPage = new ConversationStreamPage(conversationId);

                App.Current.MainPage.Navigation.PushAsync(conversationStreamPage, false);
                //App.Current.MainPage.Navigation.PushModalAsync(conversationStreamPage, false);
            }
        }


#endregion PUBLIC METHODS


#region EVENTS FROM CollectionView

        private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count == 0)
                return;

            if (e.CurrentSelection[0] == null)
                return;

            if (e.CurrentSelection[0] is ConversationModel)
            {
                String conversationId = ((ConversationModel)e.CurrentSelection[0])?.Id;
                if (String.IsNullOrEmpty(conversationId))
                    return;

                XamarinApplication.CurrentConversationId = conversationId;

                if (XamarinApplication.ConversationStreamPageList == null)
                    XamarinApplication.ConversationStreamPageList = new Dictionary<String, ConversationStreamPage>();

                ConversationStreamPage conversationStreamPage;
                if (XamarinApplication.ConversationStreamPageList.ContainsKey(conversationId))
                    conversationStreamPage = XamarinApplication.ConversationStreamPageList[conversationId];
                else
                {
                    conversationStreamPage = new ConversationStreamPage(conversationId);
                    XamarinApplication.ConversationStreamPageList.Add(conversationId, conversationStreamPage);

                    //TODO - avoid to have too many Page in this list
                }

                App.Current.MainPage.Navigation.PushAsync(conversationStreamPage, false);
                //App.Current.MainPage.Navigation.PushModalAsync(conversationStreamPage, false);
            }
        }

#endregion EVENTS FROM CollectionView


#region MANAGE COMMANDS FROM DynamicList

        void ConversationLoadMoreItems(Object obj)
        {
            List<Rainbow.Model.Conversation> rbConversations = XamarinApplication.SdkWrapper.GetAllConversationsFromCache();
            if (rbConversations != null)
            {
                ResetModelWithRbConversations(rbConversations);

                // No more items will be added in this dynamic list
                DynamicList.NoMoreItemsAvailable = true;
            }
        }

#endregion MANAGE COMMANDS FROM DynamicList


#region PRIVATE METHODS

        private void InitializeSdkObjectsAndEvents()
        {
            // Get Xamarin Application
            XamarinApplication = (App)Xamarin.Forms.Application.Current;

            // Manage event(s) from Rainbow SDK about CONVERSATIONS
            XamarinApplication.SdkWrapper.ConversationCreated += RbConversations_ConversationCreated;
            XamarinApplication.SdkWrapper.ConversationRemoved += RbConversations_ConversationRemoved;
            XamarinApplication.SdkWrapper.ConversationUpdated += RbConversations_ConversationUpdated;

            // Manage event(s) from Rainbow SDK about CONTACTS
            XamarinApplication.SdkWrapper.ContactPresenceChanged += RbContacts_ContactPresenceChanged;
            XamarinApplication.SdkWrapper.ContactAdded += RbContacts_ContactAdded;
            XamarinApplication.SdkWrapper.ContactInfoChanged += RbContacts_ContactInfoChanged;

            // Manage event(s) from Rainbow SDK about BUBBLES
            XamarinApplication.SdkWrapper.BubbleInfoUpdated += RbBubbles_BubbleInfoUpdated;

            // Manage event(s) from AvatarPool
            XamarinApplication.SdkWrapper.ContactAvatarUpdated += RbAvatars_ContactAvatarUpdated;
            XamarinApplication.SdkWrapper.BubbleAvatarUpdated += RbAvatars_BubbleAvatarUpdated;
        }

        /// <summary>
        /// Replace the current ViewModel by all Rainbow Conversations provided
        /// </summary>
        /// <param name="rbConversations">List of Rainbow Conversations</param>
        private void ResetModelWithRbConversations(List<Rainbow.Model.Conversation> rbConversations)
        {
            List<ConversationModel> conversationList = new List<ConversationModel>();
            ConversationModel newConversation;
            bool itemAdded;

            foreach (Rainbow.Model.Conversation rbConversation in rbConversations)
            {
                newConversation = Helper.GetConversationFromRBConversation(rbConversation);
                if (newConversation != null)
                {
                    // /!\ We must not start Avatar dwl here. We must only do it once they are stored in "Conversations" collection
                    var nb = conversationList.Count;
                    if (nb == 0)
                        conversationList.Add(newConversation);
                    else
                    {
                        itemAdded = false;
                        for (int i = 0; i < nb; i++)
                        {
                            if (newConversation.LastMessageDateTime.ToUniversalTime() > conversationList[i].LastMessageDateTime.ToUniversalTime())
                            {
                                conversationList.Insert(i, newConversation);
                                itemAdded = true;
                                break;
                            }
                        }
                        if (!itemAdded)
                            conversationList.Add(newConversation);
                    }
                }
            }

            lock (lockObservableConversations)
            {
                DynamicList.ReplaceRangeAndScroll(conversationList, ScrollTo.START);

                // Now we can now start Avatar download
                foreach (ConversationModel conversation in DynamicList.Items)
                    conversation.AvatarFilePath = Helper.GetConversationAvatarFilePath(conversation);
            }

            log.Debug("[ResetModelWithRbConversations] nb RBConversations:[{0}] - nb conversationList:[{1}] - nb Conversations:[{2}]", rbConversations?.Count, conversationList.Count, DynamicList.Items.Count);


           

            // Check if we need to update the view due to DateTime purpose
            //if (conversationList.Count > 0)
            //    CheckIfUpdateModelForDateTimePurpose(conversationList[0].LastMessageDateTime);
        }

        /// <summary>
        /// Add one RB Conversation in the ViewModel
        /// </summary>
        /// <param name="rbConversation">a Rainbow Conversation object</param>
        private void AddRBConversationToModel(Rainbow.Model.Conversation rbConversation)
        {
            if (rbConversation != null)
            {
                // Now add it to the model
                ConversationModel newConversation;
                newConversation = Helper.GetConversationFromRBConversation(rbConversation);
                if (newConversation != null)
                {
                    AddConversationToModel(newConversation);
                    // Get Avatar only once the conversation has been added to the model
                    newConversation.AvatarFilePath = Helper.GetConversationAvatarFilePath(newConversation);
                }
            }
        }

        private void UpdateRBConversationToModel(Rainbow.Model.Conversation rbConversation)
        {
            log.Debug("[UpdateRBConversationToModel] - IN");
            if (rbConversation != null)
            {
                // Now add it to the model
                ConversationModel newConversation = Helper.GetConversationFromRBConversation(rbConversation);
                ConversationModel oldConversation = GetConversationById(newConversation.Id);

                if (oldConversation == null)
                    AddConversationToModel(newConversation);
                else
                {
                    lock (lockObservableConversations)
                    {
                        Boolean needChangeOrder = (oldConversation.LastMessageDateTime != newConversation.LastMessageDateTime);

                        oldConversation.Id = newConversation.Id;
                        oldConversation.Jid = newConversation.Jid;
                        oldConversation.PeerId = newConversation.PeerId;
                        oldConversation.Name = newConversation.Name;
                        oldConversation.PresenceSource = newConversation.PresenceSource;

                        oldConversation.NbMsgUnread = newConversation.NbMsgUnread;

                        oldConversation.LastMessage = newConversation.LastMessage;
                        oldConversation.LastMessageDateTime = newConversation.LastMessageDateTime;

                        if (needChangeOrder)
                        {
                            int oldIndex = 0;
                            int newIndex = -1;

                            int nb = DynamicList.Items.Count;

                            // Get old Index
                            for (int i = 0; i < nb; i++)
                            {
                                if (DynamicList.Items[i].Id == oldConversation.Id)
                                {
                                    oldIndex = i;
                                    break;
                                }
                            }

                            // Try to update order only if are not already the first row
                            if (oldIndex != 0)
                            {
                                // Get new Index
                                for (int i = 0; i < nb; i++)
                                {
                                    if (oldConversation.LastMessageDateTime.ToUniversalTime() > DynamicList.Items[i].LastMessageDateTime.ToUniversalTime())
                                    {
                                        newIndex = i;
                                        break;
                                    }
                                }

                                // If we have found no new index, we need to move it to the end
                                if (newIndex == -1)
                                    newIndex = nb - 1;

                                // Update order only if indexes are different
                                if (oldIndex != newIndex)
                                    DynamicList.Items.Move(oldIndex, newIndex);

                                log.Debug("[UpdateRBConversationToModel] MOVED - oldIndex:[{0}] - newIndex:[{1}] - nb:[{2}] - TopDate:[{3}] - LastMessageDateTime:[{4}]", oldIndex, newIndex, nb, DynamicList.Items[0].LastMessageDateTime.ToString("o"), oldConversation.LastMessageDateTime.ToString("o"));
                            }
                            else
                                log.Debug("[UpdateRBConversationToModel] NOT MOVED - oldIndex:[{0}] - nb:[{1}] - TopDate:[{2}] - LastMessageDateTime:[{3}]", oldIndex, nb, DynamicList.Items[0].LastMessageDateTime.ToString("o"), oldConversation.LastMessageDateTime.ToString("o"));
                        }
                    }

                    // Check if we need to update the view due to DateTime purpose
                    CheckIfUpdateModelForDateTimePurpose(newConversation.LastMessageDateTime);
                }
            }
            log.Debug("[UpdateRBConversationToModel] - OUT");
        }

        /// <summary>
        /// Remove one RB Conversation (by ID) from the ViewModel
        /// </summary>
        /// <param name="id">Rainbow Conversation's ID</param>
        private void RemoveRbConversationFromModel(String id)
        {
            ConversationModel conversation = GetConversationById(id);

            lock (lockObservableConversations)
                DynamicList.Items.Remove(conversation);
        }

        private Boolean UpdateConversationNameByPeerId(string peerId, String name)
        {
            Boolean result = false;

            ConversationModel conversation = GetConversationByPeerId(peerId);
            if (conversation != null)
            {
                result = true;
                conversation.Name = name;
            }
            return result;
        }

        /// <summary>
        /// Update the ViewModel using the Presence Level of the specified JID
        /// </summary>
        /// <param name="jid">Jid of the contact</param>
        /// <param name="presence">Presence of the contact</param>
        private void UpdateModelUsingPresence(String jid, Presence presence)
        {
            ConversationModel conversation = GetConversationByJid(jid);
            if (conversation != null)
                conversation.PresenceSource = Helpers.Helper.GetPresenceSourceFromPresence(presence);
        }

        /// <summary>
        /// Add one Conversation Model in the ViewModel
        /// </summary>
        /// <param name="rbConversation">a Rainbow Conversation object</param>
        private void AddConversationToModel(ConversationModel newConversation)
        {
            if (newConversation != null)
            {
                // First remove this conversation if already in the model
                RemoveRbConversationFromModel(newConversation.Id);

                lock (lockObservableConversations)
                {
                    Boolean itemAdded = false;
                    int nb = DynamicList.Items.Count;
                    for (int i = 0; i < nb; i++)
                    {
                        if (newConversation.LastMessageDateTime.ToUniversalTime() > DynamicList.Items[i].LastMessageDateTime.ToUniversalTime())
                        {
                            DynamicList.Items.Insert(i, newConversation);
                            itemAdded = true;
                            break;
                        }
                    }
                    if (!itemAdded)
                    {
                        DynamicList.Items.Add(newConversation);
                    }

                    // Check if we need to update the view due to DateTime purpose
                    CheckIfUpdateModelForDateTimePurpose(newConversation.LastMessageDateTime);
                }
            }
        }

        private ConversationModel GetConversationById(String id)
        {
            ConversationModel conversationFound = null;
            lock (lockObservableConversations)
            {
                foreach (ConversationModel conversation in DynamicList.Items)
                {
                    if (conversation.Id == id)
                    {
                        conversationFound = conversation;
                        break;
                    }
                }
            }
            return conversationFound;
        }

        private ConversationModel GetConversationByJid(String jid)
        {
            ConversationModel conversationFound = null;
            lock (lockObservableConversations)
            {
                foreach (ConversationModel conversation in DynamicList.Items)
                {
                    if (conversation.Jid == jid)
                    {
                        conversationFound = conversation;
                        break;
                    }
                }
            }
            return conversationFound;
        }

        private ConversationModel GetConversationByPeerId(String peerId)
        {
            ConversationModel conversationFound = null;
            lock (lockObservableConversations)
            {
                foreach (ConversationModel conversation in DynamicList.Items)
                {
                    if (conversation.PeerId == peerId)
                    {
                        conversationFound = conversation;
                        break;
                    }
                }
            }
            return conversationFound;
        }

        private void CheckIfUpdateModelForDateTimePurpose(DateTime dt)
        {
            int delay;

            // do we have a DateTime more recent ?
            if (dt.ToUniversalTime() > mostRecentDateTimeMessage.ToUniversalTime())
            {
                delay = Helper.GetRefreshDelay(dt);
                mostRecentDateTimeMessage = dt;

                // If there is not already a delay in progress, we ask a new one
                if (delayUpdateForDateTimePurpose == null)
                {
                    log.Debug("[CheckIfUpdateModelForDateTimePurpose] - Start Delay:[{0}] - DateTime:[{1}] - CASE MORE RECENT", delay, dt.ToString("o"));
                    delayUpdateForDateTimePurpose = CancelableDelay.StartAfter(delay * 1000, () => UpdateModelForDateTimePurpose());
                }
                // There is already a delay running. So we need to cancel it and update display now
                else
                {
                    log.Debug("[CheckIfUpdateModelForDateTimePurpose] - Stop current delay and ask update now - DateTime:[{0}]", dt.ToString("o"));

                    // We stop the current "update delay"
                    delayUpdateForDateTimePurpose.Cancel();

                    // We ask the update now 
                    UpdateModelForDateTimePurpose();
                }
            }
            // We have not a more recent DateTime
            else
            {
                delay = Helper.GetRefreshDelay(mostRecentDateTimeMessage);
                // If there is not already a delay in progress, we ask a new one
                if ((delayUpdateForDateTimePurpose == null) || (!delayUpdateForDateTimePurpose.IsRunning()))
                {
                    log.Debug("[CheckIfUpdateModelForDateTimePurpose] - Start Delay:[{0}] - DateTime:[{1}] - CASE OLDER", delay, dt.ToString("o"));
                    delayUpdateForDateTimePurpose = CancelableDelay.StartAfter(delay * 1000, () => UpdateModelForDateTimePurpose());
                }
            }
        }

        private void UpdateModelForDateTimePurpose()
        {
            log.Debug("[UpdateModelForDateTimePurpose] - IN");
            //lock (lockObservableConversations)
            //{
            //    foreach (ConversationModel conversation in Conversations)
            //        conversation.MessageTimeDisplay = Helpers.Helper.HumanizeDateTime(conversation.LastMessageDateTime);
            //}

            // We ask the next update 
            CheckIfUpdateModelForDateTimePurpose(mostRecentDateTimeMessage);
            log.Debug("[UpdateModelForDateTimePurpose] - OUT");
        }

#endregion PRIVATE METHODS


#region EVENTS FIRED BY RAINBOW SDK

        private void RbBubbles_BubbleInfoUpdated(object sender, Rainbow.Events.BubbleInfoEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
           {
               UpdateConversationNameByPeerId(e.BubbleId, e.Name);
           });
        }

        private void RbContacts_ContactAdded(object sender, Rainbow.Events.JidEventArgs e)
        {
            Rainbow.Model.Contact contact = XamarinApplication.SdkWrapper.GetContactFromContactJid(e.Jid);
            if (contact != null)
            {
                Device.BeginInvokeOnMainThread(() =>
               {
                   UpdateConversationNameByPeerId(contact.Id, Rainbow.Util.GetContactDisplayName(contact));
               });
            }
        }

        private void RbContacts_ContactInfoChanged(object sender, Rainbow.Events.JidEventArgs e)
        {
            Rainbow.Model.Contact contact = XamarinApplication.SdkWrapper.GetContactFromContactJid(e.Jid);
            if (contact != null)
            {
                Device.BeginInvokeOnMainThread(() =>
               {
                   UpdateConversationNameByPeerId(contact.Id, Rainbow.Util.GetContactDisplayName(contact));
               });
            }
        }

        private void RbContacts_ContactPresenceChanged(object sender, Rainbow.Events.PresenceEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
           {
               UpdateModelUsingPresence(e.Jid, e.Presence);
           });
        }

        private void RbConversations_ConversationUpdated(object sender, Rainbow.Events.ConversationEventArgs e)
        {
            if (e.Conversation != null)
            {
                Device.BeginInvokeOnMainThread(() =>
               {
                   UpdateRBConversationToModel(e.Conversation);
               });
            }
        }

        private void RbConversations_ConversationRemoved(object sender, Rainbow.Events.ConversationEventArgs e)
        {
            if (e.Conversation != null)
            {
                Device.BeginInvokeOnMainThread(() =>
               {
                   RemoveRbConversationFromModel(e.Conversation.Id);
               });
            }
        }

        private void RbConversations_ConversationCreated(object sender, Rainbow.Events.ConversationEventArgs e)
        {
            if (e.Conversation != null)
            {
                Device.BeginInvokeOnMainThread(() =>
               {
                   AddRBConversationToModel(e.Conversation);
               });
            }
        }

#endregion EVENTS FIRED BY RAINBOW SDK


#region AVATAR POOL EVENTS

        private void RbAvatars_BubbleAvatarUpdated(object sender, Rainbow.Events.IdEventArgs e)
        {
            ConversationModel conversation = GetConversationByPeerId(e.Id);
            if (conversation != null)
            {
                Device.BeginInvokeOnMainThread(() =>
               {
                   conversation.AvatarFilePath = Helper.GetConversationAvatarFilePath(conversation);
                   log.Debug("[RbAvatars_BubbleAvatarUpdated] - bubbleId:[{0}] - conversationId:[{1}] - AvatarFilePath:[{2}]", e.Id, conversation.Id, conversation.AvatarFilePath);
               });
            }
            else
                log.Debug("[RbAvatars_BubbleAvatarChanged] No conversation found:[{1}]", e.Id);
        }

        private void RbAvatars_ContactAvatarUpdated(object sender, Rainbow.Events.IdEventArgs e)
        {
            ConversationModel conversation = GetConversationByPeerId(e.Id);
            if (conversation != null)
            {

                Device.BeginInvokeOnMainThread(() =>
               {
                   conversation.AvatarFilePath = Helper.GetConversationAvatarFilePath(conversation);
                   log.Debug("[RbAvatars_ContactAvatarChanged] - contactId:[{0}] - conversationId:[{1}] - AvatarFilePath:[{2}]", e.Id, conversation.Id, conversation.AvatarFilePath);
               });
            }
            else
            {
                log.Debug("[RbAvatars_ContactAvatarChanged] - Avatar contactId:[{0}] but no conversation found ...", e.Id);
            }
        }

#endregion AVATAR POOL EVENTS

    }
}
