using System;
using System.Collections.Generic;
using System.Threading;

using Xamarin.Forms;

using MvvmHelpers;

using Rainbow;
using Rainbow.Helpers;
using Rainbow.Model;

using InstantMessaging.Helpers;

using log4net;
using System.IO;

namespace InstantMessaging
{
    public class ConversationsViewModel : BaseViewModel
    {
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(ConversationsViewModel));

        public ObservableRangeCollection<ConversationLight> Conversations { get; } // Need to be public - Used as Binding from XAML

        // To manage avatar
        private AvatarPool avatarPool;

        private InstantMessaging.App XamarinApplication;

        private Object lockObservableConversations = new Object(); // To lock access to the observable collection: 'Conversations'

        // To know the dateTime of the most receent message received - needed to update the layout
        private DateTime mostRecentDateTimeMessage = DateTime.MinValue;
        private CancelableDelay delayUpdateForDateTimePurpose = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public ConversationsViewModel()
        {
            Conversations = new ObservableRangeCollection<ConversationLight>();

            // Get Xamarin Application
            XamarinApplication = (InstantMessaging.App)Xamarin.Forms.Application.Current;

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
            List<ConversationLight> conversationList = new List<ConversationLight>();
            ConversationLight newConversation;
            bool itemAdded;

            foreach (Rainbow.Model.Conversation rbConversation in rbConversations)
            {
                newConversation = Helper.GetConversationFromRBConversation(rbConversation);
                if (newConversation != null)
                {
                    newConversation.AvatarSource = Helper.GetConversationAvatarImageSource(newConversation);
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
                Conversations.ReplaceRange(conversationList);

            log.DebugFormat("[ResetModelWithRbConversations] nb RBConversations:[{0}] - nb conversationList:[{1}] - nb Conversations:[{2}]", rbConversations?.Count, conversationList.Count, Conversations.Count);

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
            if(rbConversation != null)
            {
                // Now add it to the model
                ConversationLight newConversation;
                newConversation = Helper.GetConversationFromRBConversation(rbConversation);
                if (newConversation != null)
                {
                    newConversation.AvatarSource = Helper.GetConversationAvatarImageSource(newConversation);
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
                ConversationLight newConversation = Helper.GetConversationFromRBConversation(rbConversation);
                ConversationLight oldConversation = GetConversationById(newConversation.Id);

                if(oldConversation == null)
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

                        int nb = Conversations.Count;

                        // Get old Index
                        for (int i = 0; i < nb; i++)
                        {
                            if(Conversations[i].Id == oldConversation.Id)
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
                                if (oldConversation.LastMessageDateTime.ToUniversalTime() > Conversations[i].LastMessageDateTime.ToUniversalTime())
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
                                Conversations.Move(oldIndex, newIndex);

                            log.DebugFormat("[UpdateRBConversationToModel] MOVED - oldIndex:[{0}] - newIndex:[{1}] - nb:[{2}] - TopDate:[{3}] - LastMessageDateTime:[{4}]", oldIndex, newIndex, nb, Conversations[0].LastMessageDateTime.ToString("o"), oldConversation.LastMessageDateTime.ToString("o"));
                        }
                        else
                            log.DebugFormat("[UpdateRBConversationToModel] NOT MOVED - oldIndex:[{0}] - nb:[{1}] - TopDate:[{2}] - LastMessageDateTime:[{3}]", oldIndex, nb, Conversations[0].LastMessageDateTime.ToString("o"), oldConversation.LastMessageDateTime.ToString("o"));
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
            ConversationLight conversation = GetConversationById(id);

            lock (lockObservableConversations)
                Conversations.Remove(conversation);
        }

        public Boolean UpdateConversationNameByPeerId(string peerId, String name)
        {
            Boolean result = false;

            ConversationLight conversation = GetConversationByPeerId(peerId);
            if(conversation != null)
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
            ConversationLight conversation = GetConversationByJid(jid);
            if (conversation != null)
                conversation.PresenceSource = InstantMessaging.Helpers.Helper.GetPresenceSourceFromPresence(presence);
        }
#endregion PUBLIC METHODS

#region PRIVATE METHODS
        /// <summary>
        /// Add one Conversation Model in the ViewModel
        /// </summary>
        /// <param name="rbConversation">a Rainbow Conversation object</param>
        private void AddConversationToModel(ConversationLight newConversation)
        {
            if (newConversation != null)
            {
                // First remove this conversation if already in the model
                RemoveRbConversationFromModel(newConversation.Id);

                lock (lockObservableConversations)
                {
                    Boolean itemAdded = false;
                    int nb = Conversations.Count;
                    for (int i = 0; i < nb; i++)
                    {
                        if (newConversation.LastMessageDateTime.ToUniversalTime() > Conversations[i].LastMessageDateTime.ToUniversalTime())
                        {
                            Conversations.Insert(i, newConversation);
                            itemAdded = true;
                            break;
                        }
                    }
                    if (!itemAdded)
                    {
                        Conversations.Add(newConversation);
                    }

                    // Check if we nee to update the view due to DateTime purpose
                    CheckIfUpdateModelForDateTimePurpose(newConversation.LastMessageDateTime);
                }
            }
        }

        private ConversationLight GetConversationById(String id)
        {
            ConversationLight conversationFound = null;
            lock (lockObservableConversations)
            {
                foreach (ConversationLight conversation in Conversations)
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

        private ConversationLight GetConversationByJid(String jid)
        {
            ConversationLight conversationFound = null;
            lock (lockObservableConversations)
            {
                foreach (ConversationLight conversation in Conversations)
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

        private ConversationLight GetConversationByPeerId(String peerId)
        {
            ConversationLight conversationFound = null;
            lock (lockObservableConversations)
            {
                foreach (ConversationLight conversation in Conversations)
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

        private ConversationLight GetConversationFromRBConversation(Rainbow.Model.Conversation rbConversation)
        {
            ConversationLight conversation = null;

            if (rbConversation != null)
            {
                conversation = new ConversationLight();


                if (rbConversation.Type == Rainbow.Model.Conversation.ConversationType.Room)
                {
                    conversation.Name = rbConversation.Name;
                    conversation.Jid = rbConversation.Jid_im;
                    conversation.PresenceSource = "";

                }
                else if (rbConversation.Type == Rainbow.Model.Conversation.ConversationType.User)
                {
                    // Get Display name of this user
                    Rainbow.Model.Contact contact = XamarinApplication.RbContacts.GetContactFromContactId(rbConversation.PeerId);
                    if (contact != null)
                    {
                        conversation.Name = Util.GetContactDisplayName(contact, avatarPool.GetFirstNameFirst());
                        conversation.Jid = contact.Jid_im;

                        Presence presence = XamarinApplication.RbContacts.GetPresenceFromContactId(rbConversation.PeerId);
                        conversation.PresenceSource = InstantMessaging.Helpers.Helper.GetPresenceSourceFromPresence(presence);

                    }
                    else
                    {
                        log.DebugFormat("[GetConversationFromRBConversation] - unknown contact - contactId:[{0}]", rbConversation.PeerId);
                        XamarinApplication.RbContacts.GetContactFromContactIdFromServer(rbConversation.PeerId, null);
                    }
                }
                else
                {
                    //TODO ( bot case)
                    log.DebugFormat("[GetConversationFromRBConversation] Conversation from model not created - Id:[{0}]", rbConversation.Id);
                    return null;
                }

                conversation.Id = rbConversation.Id;
                conversation.PeerId = rbConversation.PeerId;

                conversation.Type = rbConversation.Type;
                conversation.NbMsgUnread = rbConversation.UnreadMessageNumber;

                conversation.LastMessage = rbConversation.LastMessageText;
                conversation.LastMessageDateTime = rbConversation.LastMessageDate;

                // Humanized the DateTime
                conversation.MessageTimeDisplay = Helpers.Helper.HumanizeDateTime(conversation.LastMessageDateTime);
            }
            return conversation;
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
                if ( (delayUpdateForDateTimePurpose == null) || (!delayUpdateForDateTimePurpose.IsRunning()))
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
                foreach (ConversationLight conversation in Conversations)
                    conversation.MessageTimeDisplay = Helpers.Helper.HumanizeDateTime(conversation.LastMessageDateTime);
            }

            // We ask the next update 
            CheckIfUpdateModelForDateTimePurpose(mostRecentDateTimeMessage);
            log.DebugFormat("[UpdateModelForDateTimePurpose] - OUT");
        }

        private void SetConversationAvatar(ConversationLight conversation)
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
                        conversation.AvatarSource = ImageSource.FromFile(filePath);
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
            ConversationLight conversation = GetConversationByPeerId(e.Id);
            if (conversation != null)
                conversation.AvatarSource = Helper.GetConversationAvatarImageSource(conversation);
        }

        private void AvatarPool_ContactAvatarChanged(object sender, Rainbow.Events.IdEventArgs e)
        {
            ConversationLight conversation = GetConversationByPeerId(e.Id);
            if (conversation != null)
            {
                log.DebugFormat("[AvatarPool_ContactAvatarChanged] - contactId:[{0}] - conversationId:[{1}]", e.Id, conversation.Id);
                conversation.AvatarSource = Helper.GetConversationAvatarImageSource(conversation);
            }
            else
            {
                log.DebugFormat("[AvatarPool_ContactAvatarChanged] - Avatar contactId:[{0}] but no conversation found ...", e.Id);
            }
        }
#endregion AVATAR POOL EVENTS


    }
}
