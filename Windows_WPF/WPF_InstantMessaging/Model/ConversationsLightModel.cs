using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

using Rainbow;
using Rainbow.Model;

using InstantMessaging.Helpers;
using InstantMessaging.Pool;
using InstantMessaging.ViewModel;
using log4net;

namespace InstantMessaging.Model
{
    public class ConversationsLightModel : ObservableObject
    {
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(ConversationsLightModel));
        private readonly App currentApplication = (App)System.Windows.Application.Current;

        public ObservableRangeCollection<ConversationLightViewModel> ConversationsLightList { get; set; } // Need to be public - Used as Binding from XAML

        // To manage avatar
        private readonly AvatarPool avatarPool;

        private readonly Object lockObservableConversations = new Object(); // To lock access to the observable collection: 'Conversations'

        // To know the dateTime of the most recent message received - needed to update the layout
        private DateTime mostRecentDateTimeMessage = DateTime.MinValue;
        private CancelableDelay delayUpdateForDateTimePurpose = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public ConversationsLightModel()
        {
            ConversationsLightList = new ObservableRangeCollection<ConversationLightViewModel>();

            LoadFakeConversations();

            // Manage event(s) from AvatarPool
            avatarPool = AvatarPool.Instance;
            avatarPool.ContactAvatarChanged += AvatarPool_ContactAvatarChanged;
            avatarPool.BubbleAvatarChanged += AvatarPool_BubbleAvatarChanged;
        }

        #region PUBLIC METHODS

        /// <summary>
        /// Replace the current ViewModel by all Rainbow Conversations provided
        /// </summary>
        /// <param name="rbConversations">List of Rainbow Conversations</param>
        public void ResetModelWithRbConversations(List<Rainbow.Model.Conversation> rbConversations)
        {
            List<ConversationLightViewModel> conversationList = new List<ConversationLightViewModel>();
            ConversationLightViewModel newConversation;
            bool itemAdded;

            foreach (Rainbow.Model.Conversation rbConversation in rbConversations)
            {
                newConversation = Helper.GetConversationFromRBConversation(rbConversation);
                if (newConversation != null)
                {
                    newConversation.AvatarImageSource = Helper.GetConversationAvatarImageSource(newConversation);
                    //Helper.SetAvatarImageSource(newConversation, newConversation.AvatarSource);
                    //newConversation.AvatarSource = Helper.GetAvatarImageSource(newConversation);

                    var nb = conversationList.Count;
                    if (nb == 0)
                    {
                        conversationList.Add(newConversation);
                    }
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
                ConversationsLightList.ReplaceRange(conversationList);

            log.DebugFormat("[ResetModelWithRbConversations] nb RBConversations:[{0}] - nb conversationList:[{1}] - nb Conversations:[{2}]", rbConversations?.Count, conversationList.Count, ConversationsLightList.Count);

            // Check if we nee to update the view due to DateTime purpose
            if (conversationList.Count > 0)
                CheckIfUpdateModelForDateTimePurpose(conversationList[0].LastMessageDateTime);
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
            log.DebugFormat("[UpdateRBConversationToModel] - IN");
            if (rbConversation != null)
            {
                // Now add it to the model
                ConversationLightViewModel newConversation = Helper.GetConversationFromRBConversation(rbConversation);
                ConversationLightViewModel oldConversation = GetConversationById(newConversation.Id);

                if (oldConversation == null)
                    AddConversationToModel(newConversation);
                else
                {
                    lock (lockObservableConversations)
                    {
                        oldConversation.Id = newConversation.Id;
                        oldConversation.Jid = newConversation.Jid;
                        oldConversation.PeerId = newConversation.PeerId;
                        oldConversation.Name = newConversation.Name;
                        oldConversation.PresenceSource = newConversation.PresenceSource;
                        oldConversation.NbMsgUnread = newConversation.NbMsgUnread;

                        oldConversation.LastMessage = newConversation.LastMessage;
                        oldConversation.LastMessageDateTime = newConversation.LastMessageDateTime;
                        oldConversation.MessageTimeDisplay = newConversation.MessageTimeDisplay;

                        int oldIndex = 0;
                        int newIndex = -1;

                        int nb = ConversationsLightList.Count;

                        // Get old Index
                        for (int i = 0; i < nb; i++)
                        {
                            if (ConversationsLightList[i].Id == oldConversation.Id)
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
                                if (oldConversation.LastMessageDateTime.ToUniversalTime() > ConversationsLightList[i].LastMessageDateTime.ToUniversalTime())
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
                                ConversationsLightList.Move(oldIndex, newIndex);

                            log.DebugFormat("[UpdateRBConversationToModel] MOVED - oldIndex:[{0}] - newIndex:[{1}] - nb:[{2}] - TopDate:[{3}] - LastMessageDateTime:[{4}]", oldIndex, newIndex, nb, ConversationsLightList[0].LastMessageDateTime.ToString("o"), oldConversation.LastMessageDateTime.ToString("o"));
                        }
                        else
                            log.DebugFormat("[UpdateRBConversationToModel] NOT MOVED - oldIndex:[{0}] - nb:[{1}] - TopDate:[{2}] - LastMessageDateTime:[{3}]", oldIndex, nb, ConversationsLightList[0].LastMessageDateTime.ToString("o"), oldConversation.LastMessageDateTime.ToString("o"));
                    }

                    // Check if we need to update the view due to DateTime purpose
                    CheckIfUpdateModelForDateTimePurpose(newConversation.LastMessageDateTime);
                }
            }
            log.DebugFormat("[UpdateRBConversationToModel] - OUT");
        }

        /// <summary>
        /// Remove one RB Conversation (by ID) from the ViewModel
        /// </summary>
        /// <param name="id">Rainbow Conversation's ID</param>
        public void RemoveRbConversationFromModel(String id)
        {
            ConversationLightViewModel conversation = GetConversationById(id);

            lock (lockObservableConversations)
                ConversationsLightList.Remove(conversation);
        }

        public Boolean UpdateConversationNameByPeerId(string peerId, String name)
        {
            Boolean result = false;

            ConversationLightViewModel conversation = GetConversationByPeerId(peerId);
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
        public void UpdateModelUsingPresence(String jid, Presence presence)
        {
            ConversationLightViewModel conversation = GetConversationByJid(jid);
            if (conversation != null)
                conversation.PresenceSource = InstantMessaging.Helpers.Helper.GetPresenceSourceFromPresence(presence);
        }
        #endregion PUBLIC METHODS

        #region PRIVATE METHODS
        /// <summary>
        /// Add one Conversation Model in the ViewModel
        /// </summary>
        /// <param name="rbConversation">a Rainbow Conversation object</param>
        private void AddConversationToModel(ConversationLightViewModel newConversation)
        {
            if (newConversation != null)
            {
                // First remove this conversation if already in the model
                RemoveRbConversationFromModel(newConversation.Id);

                lock (lockObservableConversations)
                {
                    Boolean itemAdded = false;
                    int nb = ConversationsLightList.Count;
                    for (int i = 0; i < nb; i++)
                    {
                        if (newConversation.LastMessageDateTime.ToUniversalTime() > ConversationsLightList[i].LastMessageDateTime.ToUniversalTime())
                        {
                            ConversationsLightList.Insert(i, newConversation);
                            itemAdded = true;
                            break;
                        }
                    }
                    if (!itemAdded)
                    {
                        ConversationsLightList.Add(newConversation);
                    }

                    // Check if we nee to update the view due to DateTime purpose
                    CheckIfUpdateModelForDateTimePurpose(newConversation.LastMessageDateTime);
                }
            }
        }

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

        private void UpdateModelForDateTimePurpose()
        {
            log.DebugFormat("[UpdateModelForDateTimePurpose] - IN");
            lock (lockObservableConversations)
            {
                foreach (ConversationLightViewModel conversation in ConversationsLightList)
                    conversation.MessageTimeDisplay = Helpers.Helper.HumanizeDateTime(conversation.LastMessageDateTime);
            }

            // We ask the next update 
            CheckIfUpdateModelForDateTimePurpose(mostRecentDateTimeMessage);
            log.DebugFormat("[UpdateModelForDateTimePurpose] - OUT");
        }

        private void SetConversationAvatar(ConversationLightViewModel conversation)
        {
            if (conversation == null)
                return;

            String filePath = null;

            try
            {
                if (conversation.Type == Rainbow.Model.Conversation.ConversationType.User)
                    filePath = avatarPool.GetContactAvatarPath(conversation.PeerId);
                else if (conversation.Type == Rainbow.Model.Conversation.ConversationType.Room)
                    filePath = avatarPool.GetBubbleAvatarPath(conversation.PeerId);
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

        #endregion PRIVATE METHODS

        #region AVATAR POOL EVENTS
        private void AvatarPool_BubbleAvatarChanged(object sender, Rainbow.Events.IdEventArgs e)
        {
            ConversationLightViewModel conversation = GetConversationByPeerId(e.Id);
            if (conversation != null)
                conversation.AvatarImageSource = Helper.GetConversationAvatarImageSource(conversation);
        }

        private void AvatarPool_ContactAvatarChanged(object sender, Rainbow.Events.IdEventArgs e)
        {
            ConversationLightViewModel conversation = GetConversationByPeerId(e.Id);
            if (conversation != null)
            {
                log.DebugFormat("[AvatarPool_ContactAvatarChanged] - contactId:[{0}] - conversationId:[{1}]", e.Id, conversation.Id);
                conversation.AvatarImageSource = Helper.GetConversationAvatarImageSource(conversation);
            }
            else
            {
                log.DebugFormat("[AvatarPool_ContactAvatarChanged] - Avatar contactId:[{0}] but no conversation found ...", e.Id);
            }
        }
        #endregion AVATAR POOL EVENTS


        private void LoadFakeConversations()
        {
            List<ConversationLightViewModel> conversationList = new List<ConversationLightViewModel>();

            ConversationLightViewModel conversationLight = null;

            // SET NEW FAKE Conversation Light
            conversationLight = new ConversationLightViewModel();
            conversationLight.Id = "convID1";
            conversationLight.Jid = "test1@test.fr";
            conversationLight.PeerId = "peerId1";
            conversationLight.Type = "user";
            conversationLight.Name = "User Name1";
            conversationLight.Topic = "";
            conversationLight.PresenceSource = "presence_online.png";
            conversationLight.NbMsgUnread = 1;
            conversationLight.LastMessage = "Last message";
            conversationLight.LastMessageDateTime = DateTime.UtcNow;
            conversationLight.MessageTimeDisplay = "18:39";
            conversationLight.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            conversationList.Add(conversationLight);

            // SET NEW FAKE Conversation Light
            conversationLight = new ConversationLightViewModel();
            conversationLight.Id = "convID2";
            conversationLight.Jid = "test2@test.fr";
            conversationLight.PeerId = "peerId2";
            conversationLight.Type = "user";
            conversationLight.Name = "User Name2 avec tres tres tres tres long";
            conversationLight.Topic = "";
            conversationLight.PresenceSource = "presence_busy.png";
            conversationLight.NbMsgUnread = 15;
            conversationLight.LastMessage = "Last message very veryveryveryveryvery long";
            conversationLight.LastMessageDateTime = DateTime.UtcNow;
            conversationLight.MessageTimeDisplay = "17:39";
            conversationLight.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            conversationList.Add(conversationLight);

            // SET NEW FAKE Conversation Light
            conversationLight = new ConversationLightViewModel();
            conversationLight.Id = "convID3";
            conversationLight.Jid = "test3@test.fr";
            conversationLight.PeerId = "peerId3";
            conversationLight.Type = "group";
            conversationLight.Name = "Group Name3";
            conversationLight.Topic = "";
            conversationLight.PresenceSource = "";
            conversationLight.NbMsgUnread = 115;
            conversationLight.LastMessage = "Last message";
            conversationLight.LastMessageDateTime = DateTime.UtcNow;
            conversationLight.MessageTimeDisplay = "15:09";
            conversationLight.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            conversationList.Add(conversationLight);

            // SET NEW FAKE Conversation Light
            conversationLight = new ConversationLightViewModel();
            conversationLight.Id = "convID4";
            conversationLight.Jid = "test4@test.fr";
            conversationLight.PeerId = "peerId4";
            conversationLight.Type = "group";
            conversationLight.Name = "Group Name4 tres tres tres long";
            conversationLight.Topic = "";
            conversationLight.PresenceSource = "";
            conversationLight.NbMsgUnread = 115;
            conversationLight.LastMessage = "Last message";
            conversationLight.LastMessageDateTime = DateTime.UtcNow;
            conversationLight.MessageTimeDisplay = "30 Mars";
            conversationLight.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            conversationList.Add(conversationLight);

            // SET NEW FAKE Conversation Light
            conversationLight = new ConversationLightViewModel();
            conversationLight.Id = "convID5";
            conversationLight.Jid = "test6@test.fr";
            conversationLight.PeerId = "peerId6";
            conversationLight.Type = "group";
            conversationLight.Name = "Group Name6";
            conversationLight.Topic = "";
            conversationLight.PresenceSource = "";
            conversationLight.NbMsgUnread = 115;
            conversationLight.LastMessage = "";
            conversationLight.LastMessageDateTime = DateTime.UtcNow;
            conversationLight.MessageTimeDisplay = "29 Mars";
            conversationLight.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            conversationList.Add(conversationLight);

            // SET NEW FAKE Conversation Light
            conversationLight = new ConversationLightViewModel();
            conversationLight.Id = "convID6";
            conversationLight.Jid = "test6@test.fr";
            conversationLight.PeerId = "peerId6";
            conversationLight.Type = "group";
            conversationLight.Name = "Group Name6";
            conversationLight.Topic = "";
            conversationLight.PresenceSource = "";
            conversationLight.NbMsgUnread = 115;
            conversationLight.LastMessage = "Last message";
            conversationLight.LastMessageDateTime = DateTime.UtcNow;
            conversationLight.MessageTimeDisplay = "31/03";
            conversationLight.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            conversationList.Add(conversationLight);


            // SET NEW FAKE Conversation Light
            conversationLight = new ConversationLightViewModel();
            conversationLight.Id = "convID7";
            conversationLight.Jid = "test7@test.fr";
            conversationLight.PeerId = "peerId7";
            conversationLight.Type = "group";
            conversationLight.Name = "Group Name7 tres tres tres long";
            conversationLight.Topic = "";
            conversationLight.PresenceSource = "";
            conversationLight.NbMsgUnread = 115;
            conversationLight.LastMessage = "Last message";
            conversationLight.LastMessageDateTime = DateTime.UtcNow;
            conversationLight.MessageTimeDisplay = "31/03/19";
            conversationLight.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            conversationList.Add(conversationLight);

            ConversationsLightList.AddRange(conversationList);
        }

    }
}
