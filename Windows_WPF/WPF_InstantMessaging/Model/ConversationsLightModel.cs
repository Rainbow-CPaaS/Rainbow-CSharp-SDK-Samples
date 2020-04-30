using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;

using Rainbow;
using Rainbow.Model;

using InstantMessaging.Helpers;
using InstantMessaging.Pool;
using InstantMessaging.ViewModel;

using log4net;

namespace InstantMessaging.Model
{
    public class ConversationsLightModel
    {
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(ConversationsLightModel));
        private readonly App CurrentApplication = (App)System.Windows.Application.Current;
        
        private Bubbles RbBubbles = null;
        private Contacts RbContacts = null;
        private Conversations RbConversations = null;

        public RangeObservableCollection<ConversationLightViewModel> ConversationsLightList { get; set; } // Need to be public - Used as Binding from XAML

        private readonly AvatarPool AvatarPool; // To manage avatars

        private readonly Object lockObservableConversations = new Object(); // To lock access to the observable collection: 'Conversations'

        // To know the dateTime of the most recent message received - needed to update the layout
        private DateTime mostRecentDateTimeMessage = DateTime.MinValue;
        private CancelableDelay delayUpdateForDateTimePurpose = null;

        /// Define all commands used in ListView
        /// Left Click on Item
        ICommand m_ItemLeftClick;
        public ICommand ItemLeftClick
        {
            get { return m_ItemLeftClick; }
            set { m_ItemLeftClick = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ConversationsLightModel()
        {
            // Init commands
            m_ItemLeftClick = new RelayCommand<object>(new Action<object>(ItemLeftClickCommand));

            //ConversationsLightList = new SortableObservableCollection<ConversationLightViewModel>(new ConversationLightViewModel.ConversationLightViewModelComparer(), true);
            ConversationsLightList = new RangeObservableCollection<ConversationLightViewModel>();


            if (CurrentApplication.USE_DUMMY_DATA)
            {
                LoadFakeConversations();
                return;
            }

            // Get Rainbow SDK Objects
            RbBubbles = CurrentApplication.RbBubbles;
            RbContacts = CurrentApplication.RbContacts;
            RbConversations = CurrentApplication.RbConversations;

            // Manage event(s) from AvatarPool
            AvatarPool = AvatarPool.Instance;
            AvatarPool.ContactAvatarChanged += AvatarPool_ContactAvatarChanged;
            AvatarPool.BubbleAvatarChanged += AvatarPool_BubbleAvatarChanged;

            // Manage event(s) from Rainbow SDK about CONVERSATIONS
            RbConversations.ConversationCreated += RbConversations_ConversationCreated;
            RbConversations.ConversationRemoved += RbConversations_ConversationRemoved;
            RbConversations.ConversationUpdated += RbConversations_ConversationUpdated;

            // Manage event(s) from Rainbow SDK about CONTACTS
            RbContacts.ContactPresenceChanged += RbContacts_ContactPresenceChanged;
            RbContacts.ContactAdded += RbContacts_ContactAdded;
            RbContacts.ContactInfoChanged += RbContacts_ContactInfoChanged;

            // Manage event(s) from Rainbow SDK about BUBBLES
            RbBubbles.BubbleInfoUpdated += RbBubbles_BubbleInfoUpdated;

            // Set conversations list using cache
            ResetModelWithRbConversations(RbConversations.GetAllConversationsFromCache());

            // Now allow Avatar download
            AvatarPool.AllowAvatarDownload(true);

            // Now allow to ask server info about unknown contacts
            AvatarPool.AllowToAskInfoForUnknownContact(true);
        }

        #region COMMANDS RAISED BY THE VIEW

        public void ItemLeftClickCommand(object obj)
        {
            if ((obj != null) && (obj is ConversationLightViewModel))
            {
                ConversationLightViewModel conversation = obj as ConversationLightViewModel;
                if(conversation != null)
                    CurrentApplication.ApplicationMainWindow.SetConversationIdSelectionFromConversationsList(conversation.Id);
                else
                    log.WarnFormat("[ItemLeftClickCommand] Referenced object is null...");
            }
            else
                log.WarnFormat("[ItemLeftClickCommand] Referenced object is null or it's an unknown type.");

        }

        #endregion COMMANDS RAISED BY THE VIEW

        #region MODEL UPDATED IN THESE METHODS

        /// <summary>
        /// Replace the current ViewModel by all Rainbow Conversations provided
        /// </summary>
        /// <param name="rbConversations">List of Rainbow Conversations</param>
        public void ResetModelWithRbConversations(List<Rainbow.Model.Conversation> rbConversations)
        {
            lock (lockObservableConversations)
            {
                // Clear the list
                ConversationsLightList.Clear();

                foreach (Rainbow.Model.Conversation rbConversation in rbConversations)
                    AddRBConversationToModel(rbConversation);
            }

            //TODO - need to check dates to update display
            //CheckIfUpdateModelForDateTimePurpose(conversationList[0].LastMessageDateTime);
        }

        /// <summary>
        /// Add one RB Conversation in the ViewModel
        /// </summary>
        /// <param name="rbConversation">a Rainbow Conversation object</param>
        public void AddRBConversationToModel(Rainbow.Model.Conversation rbConversation)
        {
            if (rbConversation != null)
            {
                // Now add it to the model
                ConversationLightViewModel newConversation;
                newConversation = Helper.GetConversationFromRBConversation(rbConversation);
                if (newConversation != null)
                {
                    newConversation.AvatarImageSource = Helper.GetConversationAvatarImageSource(newConversation);
                    AddConversationToModel(newConversation);
                }
            }
        }

        public void UpdateRBConversationToModel(Rainbow.Model.Conversation rbConversation)
        {
            if (rbConversation != null)
            {
                ConversationLightViewModel newConversation = Helper.GetConversationFromRBConversation(rbConversation);
                if (newConversation != null)
                {
                    newConversation.AvatarImageSource = Helper.GetConversationAvatarImageSource(newConversation);
                    UpdateConversationToModel(newConversation);
                }
            }
        }

        /// <summary>
        /// Remove one RB Conversation (by ID) from the ViewModel
        /// </summary>
        /// <param name="id">Rainbow Conversation's ID</param>
        public void RemoveRBConversationFromModel(String id)
        {
            ConversationLightViewModel result = GetConversationById(id);
            if (result != null)
            {
                lock (lockObservableConversations)
                    ConversationsLightList.Remove(result);
            }
        }

        public void UpdateConversationNameByPeerId(string peerId, String name)
        {
            ConversationLightViewModel conversation = GetConversationByPeerId(peerId);
            if (conversation != null)
                conversation.Name = name;
        }

        /// <summary>
        /// Update the ViewModel using the Presence Level of the specified JID
        /// </summary>
        /// <param name="jid">Jid of the contact</param>
        /// <param name="presence">Presence of the contact</param>
        public void UpdateModelUsingPresence(String jid, Presence presence)
        {
            ConversationLightViewModel conversation = GetConversationByJid(jid);
            if (conversation != null)
                conversation.PresenceSource = InstantMessaging.Helpers.Helper.GetPresenceSourceFromPresence(presence, jid == CurrentApplication.CurrentUserJid);
        }

        private void AddConversationToModel(ConversationLightViewModel newConversation)
        {
            if (newConversation != null)
            {
                // First remove this conversation if already in the model
                RemoveRBConversationFromModel(newConversation.Id);

                lock (lockObservableConversations)
                {
                    Boolean itemAdded = false;
                    int nb = ConversationsLightList.Count;
                    for (int i = 0; i < nb; i++)
                    {
                        if (newConversation.LastMessageDateTime.ToUniversalTime() > ConversationsLightList[i].LastMessageDateTime.ToUniversalTime())
                        {
                            ConversationsLightList.Insert(i, newConversation);
                            log.DebugFormat("[AddConversationToModel] INSERT Conversation.id:[{0}] IN index:[{1}]", newConversation.Id, i);
                            itemAdded = true;
                            break;
                        }
                    }
                    if (!itemAdded)
                    {
                        ConversationsLightList.Add(newConversation);
                        log.DebugFormat("[AddConversationToModel] ADD Conversation.id:[{0}] ", newConversation.Id);
                    }

                    // Check if we nee to update the view due to DateTime purpose
                    //CheckIfUpdateModelForDateTimePurpose(newConversation.LastMessageDateTime);
                }
            }
        }

        private void UpdateConversationToModel(ConversationLightViewModel newConversation)
        {
            if (newConversation != null)
            {
                ConversationLightViewModel oldConversation = GetConversationById(newConversation.Id);

                // If it's an unknown conversation remove it
                if (oldConversation == null)
                {
                    AddConversationToModel(newConversation);
                    return;
                }

                lock (lockObservableConversations)
                {
                    oldConversation.Id = newConversation.Id;
                    oldConversation.Jid = newConversation.Jid;
                    oldConversation.PeerId = newConversation.PeerId;
                    oldConversation.Type = newConversation.Type;
                    oldConversation.Name = newConversation.Name;
                    oldConversation.Topic = newConversation.Topic;
                    oldConversation.LastMessage = newConversation.LastMessage;
                    oldConversation.PresenceSource = newConversation.PresenceSource;
                    oldConversation.NbMsgUnread = newConversation.NbMsgUnread;

                    // Do we need to sort the list ? Only if DateTime is different
                    if (oldConversation.LastMessageDateTime != newConversation.LastMessageDateTime)
                    {
                        int nb = ConversationsLightList.Count;
                        int oldIndex = -1;
                        int newIndex = -1;

                        // Get old Index
                        for (int i = 0; i < nb; i++)
                        {
                            if (ConversationsLightList[i].Id == oldConversation.Id)
                            {
                                oldIndex = i;
                                break;
                            }
                        }

                        // Check  old index found
                        if(oldIndex == -1)
                        {
                            log.ErrorFormat("[UpdateConversationToModel] There is a pb to get old index ...");
                            return;
                        }

                        // Get new index
                        for (int i = 0; i < nb; i++)
                        {
                            if (newConversation.LastMessageDateTime.ToUniversalTime() > ConversationsLightList[i].LastMessageDateTime.ToUniversalTime())
                            {
                                newIndex = i;
                                break;
                            }
                        }

                        if (newIndex == -1)
                            newIndex = nb - 1;

                        // Now set the new date
                        oldConversation.LastMessageDateTime = newConversation.LastMessageDateTime;

                        if (oldIndex != newIndex)
                        {
                            log.InfoFormat("[UpdateConversationToModel] Move conversation from oldIndex:[{0}] to newIndex:[{1}]", oldIndex, newIndex);

                            // Move to corretc index
                            ConversationsLightList.Move(oldIndex, newIndex);
                        }
                        else
                            log.InfoFormat("[UpdateConversationToModel] DON'T Move conversation - same indexes - oldIndex:[{0}] - newIndex:[{1}]", oldIndex, newIndex);

                    }
                }
            }
        }

        private void UpdateModelForDateTimePurpose()
        {
            log.DebugFormat("[UpdateModelForDateTimePurpose] - IN");
            lock (lockObservableConversations)
            {
                foreach (ConversationLightViewModel conversation in ConversationsLightList)
                    conversation.LastMessageTimeDisplay = Helpers.Helper.HumanizeDateTime(conversation.LastMessageDateTime);
            }

            // We ask the next update 
            //CheckIfUpdateModelForDateTimePurpose(mostRecentDateTimeMessage);
            //log.DebugFormat("[UpdateModelForDateTimePurpose] - OUT");
        }

        private void SetConversationAvatar(ConversationLightViewModel conversation)
        {
            if (conversation == null)
                return;

            String filePath = null;

            try
            {
                if (conversation.Type == Rainbow.Model.Conversation.ConversationType.User)
                    filePath = AvatarPool.GetContactAvatarPath(conversation.PeerId);
                else if (conversation.Type == Rainbow.Model.Conversation.ConversationType.Room)
                    filePath = AvatarPool.GetBubbleAvatarPath(conversation.PeerId);
            }
            catch (Exception exc)
            {
                log.WarnFormat("[SetConversationAvatar] PeerId:[{0}] - exception occurs to create avatar:[{1}]", conversation.PeerId, Util.SerializeException(exc));
            }

            if (!String.IsNullOrEmpty(filePath))
            {
                if (File.Exists(filePath))
                {
                    log.DebugFormat("[SetConversationAvatar] - file used - filePath:[{0}] - PeerId:[{1}]", filePath, conversation.PeerId);
                    try
                    {
                        conversation.AvatarImageSource = Helper.BitmapImageFromFile(filePath);
                    }
                    catch (Exception exc)
                    {
                        log.WarnFormat("[SetConversationAvatar] PeerId:[{0}] - exception occurs to display avatar[{1}]", conversation.PeerId, Util.SerializeException(exc));
                    }
                }
                else
                    log.WarnFormat("[SetConversationAvatar] - file not found - filePath:[{0}] - PeerId:[{1}]", filePath, conversation.PeerId);
            }
        }

        #endregion  MODEL UPDATED IN THESE METHODS

        #region PRIVATE METHODS
        /// <summary>
        /// Add one Conversation Model in the ViewModel
        /// </summary>
        /// <param name="rbConversation">a Rainbow Conversation object</param>

        private ConversationLightViewModel GetConversationById(String id)
        {
            ConversationLightViewModel conversationFound = null;
            lock (lockObservableConversations)
            {
                foreach (ConversationLightViewModel conversation in ConversationsLightList)
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

        private ConversationLightViewModel GetConversationByJid(String jid)
        {
            ConversationLightViewModel conversationFound = null;
            lock (lockObservableConversations)
            {
                foreach (ConversationLightViewModel conversation in ConversationsLightList)
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

        private ConversationLightViewModel GetConversationByPeerId(String peerId)
        {
            ConversationLightViewModel conversationFound = null;
            lock (lockObservableConversations)
            {
                foreach (ConversationLightViewModel conversation in ConversationsLightList)
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
                    log.DebugFormat("[CheckIfUpdateModelForDateTimePurpose] - Start Delay:[{0}] - DateTime:[{1}] - CASE MORE RECENT", delay, dt.ToString("o"));
                    delayUpdateForDateTimePurpose = CancelableDelay.StartAfter(delay * 1000, () => UpdateModelForDateTimePurpose());
                }
                // There is already a delay running. So we need to cancel it and update display now
                else
                {
                    log.DebugFormat("[CheckIfUpdateModelForDateTimePurpose] - Stop current delay and ask update now - DateTime:[{0}]", dt.ToString("o"));

                    // We stop the current "update delay"
                    delayUpdateForDateTimePurpose.Cancel();

                    // We ask the update now 
                    //TO DO - must be a UI THREAD TO YPDATE THE MODEL !!!
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
                    log.DebugFormat("[CheckIfUpdateModelForDateTimePurpose] - Start Delay:[{0}] - DateTime:[{1}] - CASE OLDER", delay, dt.ToString("o"));
                    delayUpdateForDateTimePurpose = CancelableDelay.StartAfter(delay * 1000, () => UpdateModelForDateTimePurpose());
                }
            }
        }

        #endregion PRIVATE METHODS

        #region AVATAR POOL EVENTS
        private void AvatarPool_BubbleAvatarChanged(object sender, Rainbow.Events.IdEventArgs e)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ConversationLightViewModel conversation = GetConversationByPeerId(e.Id);
                    if (conversation != null)
                    {
                        log.DebugFormat("[AvatarPool_BubbleAvatarChanged] - conversationId:[{0}]", conversation.Id);
                        conversation.AvatarImageSource = Helper.GetConversationAvatarImageSource(conversation);
                    }
                    else
                    {
                        log.DebugFormat("[AvatarPool_BubbleAvatarChanged] - no conversation found:[{0}]", e.Id);
                    }
                }));
            }
        }

        private void AvatarPool_ContactAvatarChanged(object sender, Rainbow.Events.IdEventArgs e)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ConversationLightViewModel conversation = GetConversationByPeerId(e.Id);
                    if (conversation != null)
                    {
                        log.DebugFormat("[AvatarPool_ContactAvatarChanged] - contactId:[{0}] - conversationId:[{1}]", e.Id, conversation.Id);
                        conversation.AvatarImageSource = Helper.GetConversationAvatarImageSource(conversation);
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

        private void RbBubbles_BubbleInfoUpdated(object sender, Rainbow.Events.BubbleInfoEventArgs e)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    UpdateConversationNameByPeerId(e.BubbleId, e.Name);
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
                        UpdateConversationNameByPeerId(contact.Id, Util.GetContactDisplayName(contact, AvatarPool.GetFirstNameFirst()));
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
                        UpdateConversationNameByPeerId(contact.Id, Util.GetContactDisplayName(contact, AvatarPool.GetFirstNameFirst()));
                }));
            }
        }

        private void RbContacts_ContactPresenceChanged(object sender, Rainbow.Events.PresenceEventArgs e)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    UpdateModelUsingPresence(e.Jid, e.Presence);
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
                        UpdateRBConversationToModel(e.Conversation);
                }));
            }
        }

        private void RbConversations_ConversationRemoved(object sender, Rainbow.Events.ConversationEventArgs e)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (e.Conversation != null)
                    RemoveRBConversationFromModel(e.Conversation.Id);
                }));
            }
        }

        private void RbConversations_ConversationCreated(object sender, Rainbow.Events.ConversationEventArgs e)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (e.Conversation != null)
                    AddRBConversationToModel(e.Conversation);
                }));
            }
        }

        #endregion EVENTS FIRED BY RAINBOW SDK

        private void LoadFakeConversations()
        {
            DateTime utcNow = DateTime.UtcNow;

            ConversationLightViewModel conversationLight = null;

            // SET NEW FAKE Conversation Light
            conversationLight = new ConversationLightViewModel();
            conversationLight.Id = "convID1";
            conversationLight.Jid = "test1@test.fr";
            conversationLight.PeerId = "peerId1";
            conversationLight.Type = "user";
            conversationLight.Name = conversationLight.Id + "User Name1";
            conversationLight.Topic = "";
            conversationLight.PresenceSource = "presence_online.png";
            conversationLight.NbMsgUnread = 1;
            conversationLight.LastMessage = "Last message";
            conversationLight.LastMessageDateTime = utcNow;
            //conversationLight.LastMessageTimeDisplay = Helpers.Helper.HumanizeDateTime(conversationLight.LastMessageDateTime);
            conversationLight.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            ConversationsLightList.Add(conversationLight);

            // SET NEW FAKE Conversation Light
            conversationLight = new ConversationLightViewModel();
            conversationLight.Id = "convID2";
            conversationLight.Jid = "test2@test.fr";
            conversationLight.PeerId = "peerId2";
            conversationLight.Type = "user";
            conversationLight.Name = conversationLight.Id + "User Name2 avec tres tres tres tres long";
            conversationLight.Topic = "";
            conversationLight.PresenceSource = "presence_busy.png";
            conversationLight.NbMsgUnread = 15;
            conversationLight.LastMessage = "-01:00 Last message very veryveryveryveryvery veryvery veryvery veryvery veryvery veryvery veryvery veryvery veryverylong";
            conversationLight.LastMessageDateTime = utcNow.AddHours(-1);
            //conversationLight.LastMessageTimeDisplay = Helpers.Helper.HumanizeDateTime(conversationLight.LastMessageDateTime);
            conversationLight.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar2.jpg");

            ConversationsLightList.Add(conversationLight);

            // SET NEW FAKE Conversation Light
            conversationLight = new ConversationLightViewModel();
            conversationLight.Id = "convID3";
            conversationLight.Jid = "test3@test.fr";
            conversationLight.PeerId = "peerId3";
            conversationLight.Type = "group";
            conversationLight.Name = conversationLight.Id + "Group Name3";
            conversationLight.Topic = "";
            conversationLight.PresenceSource = "";
            conversationLight.NbMsgUnread = 0;
            conversationLight.LastMessage = "+01:00 Last message";
            conversationLight.LastMessageDateTime = utcNow.AddHours(1);
            //conversationLight.LastMessageTimeDisplay = Helpers.Helper.HumanizeDateTime(conversationLight.LastMessageDateTime);
            conversationLight.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar3.jpg");

            ConversationsLightList.Add(conversationLight);

            // SET NEW FAKE Conversation Light
            conversationLight = new ConversationLightViewModel();
            conversationLight.Id = "convID4";
            conversationLight.Jid = "test4@test.fr";
            conversationLight.PeerId = "peerId4";
            conversationLight.Type = "group";
            conversationLight.Name = conversationLight.Id + "Group Name4 tres tres tres long";
            conversationLight.Topic = "";
            conversationLight.PresenceSource = "";
            conversationLight.NbMsgUnread = 99;
            conversationLight.LastMessage = "-02:00 Last message";
            conversationLight.LastMessageDateTime = utcNow.AddHours(-2);
            //conversationLight.LastMessageTimeDisplay = Helpers.Helper.HumanizeDateTime(conversationLight.LastMessageDateTime);
            conversationLight.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar4.jpg");

            ConversationsLightList.Add(conversationLight);

            // SET NEW FAKE Conversation Light
            conversationLight = new ConversationLightViewModel();
            conversationLight.Id = "convID5";
            conversationLight.Jid = "test6@test.fr";
            conversationLight.PeerId = "peerId6";
            conversationLight.Type = "group";
            conversationLight.Name = conversationLight.Id + "Group Name6";
            conversationLight.Topic = "";
            conversationLight.PresenceSource = "";
            conversationLight.NbMsgUnread = 99;
            conversationLight.LastMessage = "-1 day";
            conversationLight.LastMessageDateTime = utcNow.AddDays(-1);
            //conversationLight.LastMessageTimeDisplay = Helpers.Helper.HumanizeDateTime(conversationLight.LastMessageDateTime);
            conversationLight.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            ConversationsLightList.Add(conversationLight);

            // SET NEW FAKE Conversation Light
            conversationLight = new ConversationLightViewModel();
            conversationLight.Id = "convID6";
            conversationLight.Jid = "test6@test.fr";
            conversationLight.PeerId = "peerId6";
            conversationLight.Type = "group";
            conversationLight.Name = conversationLight.Id + "Group Name6 tres tres tres tres long";
            conversationLight.Topic = "";
            conversationLight.PresenceSource = "";
            conversationLight.NbMsgUnread = 25;
            conversationLight.LastMessage = "-1 Year Last message";
            conversationLight.LastMessageDateTime = utcNow.AddYears(-1);
            //conversationLight.LastMessageTimeDisplay = Helpers.Helper.HumanizeDateTime(conversationLight.LastMessageDateTime);
            conversationLight.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar2.jpg");

            ConversationsLightList.Add(conversationLight);


            // SET NEW FAKE Conversation Light
            conversationLight = new ConversationLightViewModel();
            conversationLight.Id = "convID7";
            conversationLight.Jid = "test7@test.fr";
            conversationLight.PeerId = "peerId7";
            conversationLight.Type = "group";
            conversationLight.Name = conversationLight.Id + "Group Name7 tres tres tres long";
            conversationLight.Topic = "";
            conversationLight.PresenceSource = "";
            conversationLight.NbMsgUnread = 0;
            conversationLight.LastMessage = "-1 Month Last message";
            conversationLight.LastMessageDateTime = utcNow.AddMonths(-1);
            //conversationLight.LastMessageTimeDisplay = Helpers.Helper.HumanizeDateTime(conversationLight.LastMessageDateTime);
            conversationLight.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar3.jpg");

            ConversationsLightList.Add(conversationLight);
        }

        private ConversationLightViewModel GetConversationLightCopy(ConversationLightViewModel conversationLightViewModel)
        {
            ConversationLightViewModel result = new ConversationLightViewModel();

            result.Id = conversationLightViewModel.Id;
            result.Jid = conversationLightViewModel.Jid;
            result.PeerId = conversationLightViewModel.PeerId;
            result.Type = conversationLightViewModel.Type;
            result.Name = conversationLightViewModel.Name;
            result.Topic = conversationLightViewModel.Topic;
            result.LastMessage = conversationLightViewModel.LastMessage;
            result.PresenceSource = conversationLightViewModel.PresenceSource;
            result.NbMsgUnread = conversationLightViewModel.NbMsgUnread;
            result.LastMessageDateTime = conversationLightViewModel.LastMessageDateTime;

            result.AvatarImageSource = conversationLightViewModel.AvatarImageSource;

            return result;
        }

    }
}
