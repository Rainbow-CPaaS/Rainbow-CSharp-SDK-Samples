using System;
using System.IO;
using System.Collections.Generic;

using System.Threading.Tasks;

using Xamarin.Forms;

using MvvmHelpers;

using InstantMessaging.Helpers;
using InstantMessaging.Model;

using Rainbow;
using Rainbow.Helpers;
using Rainbow.Model;

using log4net;

namespace InstantMessaging
{
    public class ConversationStreamViewModel : BaseViewModel
    {
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(ConversationStreamViewModel));

        private static int NB_MESSAGE_LOADED_BY_ROW = 40;

        private String conversationId; // ConversationId of this Conversation
        private Conversation rbConversation = null; // Rainbow Conversation object

        private AvatarPool avatarPool;
        private InstantMessaging.App XamarinApplication;

        private Object lockObservableMessagesList = new Object(); // To lock access to the observable collection: 'MessagesList'

        private Object lockContactsListInvolved = new Object(); // To lock access to: 'contactsListInvolved'
        private List<String> contactsListInvolved = new List<String>(); // To store list of contacts involved in this Conversation (by Jid)

        private Object lockRepliedContactsListInvolved = new Object(); // To lock access to: 'repliedContactsListInvolved'
        private List<String> repliedContactsListInvolved = new List<String>(); // To store list of contacts involved in reply context in this Conversation (by Jid)

        private Object lockRepliedMessagesListInvolved = new Object(); // To lock access to: 'repliedMessagesListInvolved'
        private List<String> repliedMessagesListInvolved = new List<String>(); // To store list of messages involved in reply context in this Conversation (by message Id)

        // Define attributes to manage loading/scrolling of messages list view
        private Boolean noMoreOlderMessagesAvailable = false;
        private Boolean loadingOlderMessages = false;
        private Boolean waitingScrollingDueToLoadingOlderMessages = false;
        private int nbOlderMessagesFound = 0;
        private Boolean newMessageAdded = false;

#region XAML BINDING ELEMENTS

        public ConversationLight Conversation { get; } // Need to be public - Used as Binding from XAML

        public ObservableRangeCollection<InstantMessaging.Model.Message> MessagesList { get; } // Need to be public - Used as Binding from XAML

        public ConversationStream ConversationStream { get; }  // Need to be public - Used as Binding from XAML

#endregion XAML BINDING ELEMENTS

#region PUBLIC METHODS

        /// <summary>
        /// Constructor
        /// </summary>
        public ConversationStreamViewModel(String conversationId)
        {
            // Get Xamarin Application
            XamarinApplication = (InstantMessaging.App)Xamarin.Forms.Application.Current;

            // Manage event(s) from AvatarPool
            avatarPool = AvatarPool.Instance;
            avatarPool.ContactAvatarChanged += AvatarPool_ContactAvatarChanged;
            avatarPool.BubbleAvatarChanged += AvatarPool_BubbleAvatarChanged;

            // Manage event(s) from InstantMessaging
            XamarinApplication.RbInstantMessaging.MessageReceived += RbInstantMessaging_MessageReceived;

            // Manage event(s) from Contacts
            XamarinApplication.RbContacts.ContactAdded += RbContacts_ContactAdded;
            XamarinApplication.RbContacts.ContactInfoChanged += RbContacts_ContactInfoChanged;

            // Store conversation Id
            this.conversationId = conversationId;

            // Create default ConversationStream object
            ConversationStream = new ConversationStream();
            ConversationStream.LoadingIndicatorIsVisible = "False";
            ConversationStream.ListViewIsEnabled = "True";

            // Create default MessagesList  object
            MessagesList = new ObservableRangeCollection<InstantMessaging.Model.Message>();

            if (string.IsNullOrEmpty(this.conversationId))
            {
                //TODO
                Conversation = new ConversationLight();
            }
            else
            {
                // Get Rainbow Conversation object
                rbConversation = XamarinApplication.RbConversations.GetConversationByIdFromCache(this.conversationId);

                // Get Conversation Model Object using Rainbow Conversation
                Conversation = Helper.GetConversationFromRBConversation(rbConversation);
                if (Conversation != null)
                    Conversation.AvatarSource = Helper.GetConversationAvatarImageSource(Conversation);

                // Get Messages
                List<Rainbow.Model.Message> rbMessagesList = XamarinApplication.RbInstantMessaging.GetAllMessagesFromConversationIdFromCache(this.conversationId);
                if (rbMessagesList != null)
                {
                    if (rbMessagesList.Count > 0)
                        AddToModelRbMessages(rbMessagesList);

                    if (rbMessagesList.Count < NB_MESSAGE_LOADED_BY_ROW)
                        LoadMoreMessages();
                }
                else
                    LoadMoreMessages();
            }
        }

        public void SendMessage(String content)
        {
            XamarinApplication.RbInstantMessaging.SendMessageToConversationId(this.conversationId, content);
        }

        public void ScrollingDueToLoadingOlderMessagesDone()
        {
            waitingScrollingDueToLoadingOlderMessages = false;
            ConversationStream.LoadingIndicatorIsVisible = "False";
            ConversationStream.ListViewIsEnabled = "True";
        }

        public bool WaitingScrollingDueToLoadingOlderMessages()
        {
            return waitingScrollingDueToLoadingOlderMessages;
        }

        public int NbOlderMessagesFound()
        {
            return nbOlderMessagesFound;
        }

        public Boolean CanLoadMoreMessages()
        {
            if (noMoreOlderMessagesAvailable || loadingOlderMessages || waitingScrollingDueToLoadingOlderMessages)
                return false;
            return true;
        }

        public void LoadMoreMessages()
        {
            // We are starting to load older messages
            loadingOlderMessages = true;

            // Display "loading indicator"
            ConversationStream.LoadingIndicatorIsVisible = "True";
            ConversationStream.ListViewIsEnabled = "False";

            Task task = new Task(() =>
                XamarinApplication.RbInstantMessaging.GetMessagesFromConversationId(conversationId, NB_MESSAGE_LOADED_BY_ROW, callback =>
                {
                    if (callback.Result.Success)
                    {
                        // We store the number of older message found
                        nbOlderMessagesFound = callback.Data.Count;

                        if (callback.Data.Count == 0)
                        {
                            // We store the fact that we have no more older message to found
                            noMoreOlderMessagesAvailable = true;

                            // We hide loading indicator
                            ConversationStream.LoadingIndicatorIsVisible = "False";
                            ConversationStream.ListViewIsEnabled = "True";
                        }
                        else
                        {
                            // We store the fact that we will now wait to scroll to message before to ask older messages
                            // Must been done before to add element to model
                            waitingScrollingDueToLoadingOlderMessages = true;

                            AddToModelRbMessages(callback.Data);
                        }
                        // We have stopped to load older messages
                        loadingOlderMessages = false;
                    }
                    else
                    {
                        //TODO
                    }
                }) );
            task.Start();
        }

        public Boolean NewMessageAdded()
        {
            return newMessageAdded;
        }

        public void ScrollToNewMessageAddedDone()
        {
            newMessageAdded = false;
        }

#endregion PUBLIC METHODS

#region PRIVATE METHOD

        private int AddToModelRbMessages(List<Rainbow.Model.Message> rbMessagesList)
        {
            List<InstantMessaging.Model.Message> messagesList = new List<InstantMessaging.Model.Message>();
            foreach (Rainbow.Model.Message rbMessage in rbMessagesList)
            {
                InstantMessaging.Model.Message msg = GetMessageFromRBMessage(rbMessage, rbConversation.Type);
                if (msg != null)
                {
                    messagesList.Insert(0, msg);
                    msg.AvatarSource = Helper.GetContactAvatarImageSource(msg.PeerId);
                    AddContactInvolved(msg.PeerJid);
                    
                    // For reply management
                    if(msg.ReplyPeerJid != null)
                        AddRepliedContactInvolved(msg.ReplyPeerJid);
                    else
                        AddRepliedMessageInvolved(msg.ReplyId);
                }
            }

            lock (lockObservableMessagesList)
            {
                if (MessagesList.Count == 0)
                {
                    MessagesList.ReplaceRange(messagesList);
                    return 0;
                }
                else
                {
                    int nb = messagesList.Count - 1;
                    for (int i = nb; i > 0; i--)
                    {
                        MessagesList.Insert(0, messagesList[i]);
                    }
                    return nb;
                }
            }
        }

        private void AddContactInvolved(String peerJid)
        {
            lock (lockContactsListInvolved)
            {
                if((!String.IsNullOrEmpty(peerJid)) && (!contactsListInvolved.Contains(peerJid)))
                {
                    //log.DebugFormat("[AddContactInvolved] - ContactJid:[{0}]", peerJid);
                    contactsListInvolved.Add(peerJid);
                }
            }
        }

        private void AddRepliedContactInvolved(String peerJid)
        {
            lock (lockRepliedContactsListInvolved)
            {
                if ((!String.IsNullOrEmpty(peerJid)) && (!repliedContactsListInvolved.Contains(peerJid)))
                {
                    repliedContactsListInvolved.Add(peerJid);
                }
            }
        }

        private void AddRepliedMessageInvolved(String msgId)
        {
            lock (lockRepliedMessagesListInvolved)
            {
                if ((!String.IsNullOrEmpty(msgId)) && (!repliedMessagesListInvolved.Contains(msgId)))
                {
                    repliedMessagesListInvolved.Add(msgId);
                }
            }
        }
        
        private List<InstantMessaging.Model.Message> GetMessagesByPeerJid(String peerJid)
        {
            List<InstantMessaging.Model.Message> result = new List<InstantMessaging.Model.Message>();
            lock (lockObservableMessagesList)
            {
                foreach(InstantMessaging.Model.Message msg  in MessagesList)
                {
                    if (msg.PeerJid == peerJid)
                        result.Add(msg);
                }
            }
            return result;
        }

        private void UpdateMessagesForJid(String peerJid, bool updateAvatar, bool updateDisplayName )
        {
            if (contactsListInvolved.Contains(peerJid))
            {
                Contact contact = XamarinApplication.RbContacts.GetContactFromContactJid(peerJid);
                if (contact != null)
                {
                    log.DebugFormat("[UpdateMessagesForJid] peerJid:[{0}] - peerId:[{1}] - updateAvatar[{2}] - updateDisplayName:[{3}]", peerJid, contact.Id, updateAvatar, updateDisplayName);
                    ImageSource imageSource;
                    String displayName;
                    
                    if (updateAvatar)
                        imageSource = Helper.GetContactAvatarImageSource(contact.Id);
                    else
                        imageSource = null;

                    if (updateDisplayName)
                        displayName = Util.GetContactDisplayName(contact, AvatarPool.Instance.GetFirstNameFirst());
                    else
                        displayName = null;

                    List<InstantMessaging.Model.Message> messages = GetMessagesByPeerJid(peerJid);
                    lock (lockObservableMessagesList)
                    {
                        foreach (InstantMessaging.Model.Message message in messages)
                        {
                            if (updateAvatar)
                                message.AvatarSource = imageSource;

                            if (updateDisplayName)
                            {
                                message.PeerDisplayName = displayName;
                                message.BackgroundColor = Rainbow.Helpers.AvatarPool.GetColorFromDisplayName(message.PeerDisplayName);
                                message.ReplyBackgroundColor = Rainbow.Helpers.AvatarPool.GetDarkerColorFromDisplayName(message.PeerDisplayName);
                            }
                        }
                    }
                }
                else
                    log.WarnFormat("[UpdateMessagesForJid] peerJid:[{0}] found but related contact not found", peerJid);
            }
            else
                log.WarnFormat("[UpdateMessagesForJid] peerJid:[{0}] not found in this conversationId:[{1}]", peerJid, this.conversationId);
        }

        public InstantMessaging.Model.Message GetMessageFromRBMessage(Rainbow.Model.Message rbMessage, String conversationType)
        {
            InstantMessaging.Model.Message message = null;
            if (rbMessage != null)
            {
                InstantMessaging.App XamarinApplication = (InstantMessaging.App)Xamarin.Forms.Application.Current;
                AvatarPool avatarPool = AvatarPool.Instance;

                message = new InstantMessaging.Model.Message();

                Rainbow.Model.Contact contact = XamarinApplication.RbContacts.GetContactFromContactJid(rbMessage.FromJid);
                if (contact != null)
                {
                    message.PeerId = contact.Id;

                    message.PeerDisplayName = Util.GetContactDisplayName(contact, avatarPool.GetFirstNameFirst());
                    message.BackgroundColor = Rainbow.Helpers.AvatarPool.GetColorFromDisplayName(message.PeerDisplayName);
                }
                else
                {
                    message.BackgroundColor = Rainbow.Helpers.AvatarPool.GetColorFromDisplayName("");

                    // We ask to have more info about this contact usin AvatarPool
                    avatarPool.AddUnknownContactToPoolByJid(rbMessage.FromJid);
                }

                // We have the display name only in Room / Bubble context
                message.PeerDisplayNameIsVisible = (conversationType == Rainbow.Model.Conversation.ConversationType.Room) ? "True" : "False";

                message.Id = rbMessage.Id;
                message.PeerJid = rbMessage.FromJid;

                message.MessageDateTime = rbMessage.Date;
                message.MessageDateDisplay = Helper.HumanizeDateTime(rbMessage.Date);

                // Set default content
                String content;
                if (!String.IsNullOrEmpty(rbMessage.BubbleEvent))
                {
                    content = "EVENT: " + rbMessage.BubbleEvent;
                }
                else if (rbMessage.Content != null)
                {
                    content = rbMessage.Content;
                }
                else
                {
                    content = "";
                }

                // Reply part
                // By default is not displayed
                message.ReplyPartIsVisible = "False";
                if (rbMessage.ReplyMessage != null)
                {
                    // Store Id of this reply message
                    message.ReplyId = rbMessage.ReplyMessage.Id;

                    // Set background color
                    message.ReplyBackgroundColor = Rainbow.Helpers.AvatarPool.GetDarkerColorFromDisplayName(message.PeerDisplayName);

                    // We need to get Name and text of the replied message ...
                    Rainbow.Model.Message rbRepliedMessage = XamarinApplication.RbInstantMessaging.GetOneMessageFromConversationIdFromCache(this.conversationId, rbMessage.ReplyMessage.Id);
                    if(rbRepliedMessage != null)
                    {
                        // Store Jid of the message sender
                        message.ReplyPeerJid = rbRepliedMessage.FromJid;

                        Rainbow.Model.Contact contactReply = XamarinApplication.RbContacts.GetContactFromContactJid(rbRepliedMessage.FromJid);
                        if (contactReply != null)
                        {
                            // Reply part is visible
                            message.ReplyPartIsVisible = "True";

                            message.ReplyPeerId = contactReply.Id;
                            message.ReplyPeerDisplayName = Util.GetContactDisplayName(contactReply, avatarPool.GetFirstNameFirst());
                            message.ReplyBody = rbRepliedMessage.Content;
                        }
                        else
                        {
                            // We ask to have more info about this contact using AvatarPool
                            avatarPool.AddUnknownContactToPoolByJid(rbRepliedMessage.FromJid);
                        }
                    }
                    else
                    {
                        //TODO - need to have info about this reply message ...
                    }
                }
                

                // Replace
                if (!String.IsNullOrEmpty(rbMessage.ReplaceId))
                {
                    if (!String.IsNullOrEmpty(content))
                        content += "\r\n";
                    content += String.Format("ReplaceId: [{0}]", rbMessage.ReplaceId);
                }

                // FileAttachment
                if (rbMessage.FileAttachment != null)
                {
                    if (!String.IsNullOrEmpty(content))
                        content += "\r\n";
                    content += String.Format("File: \"{0}\"\r\nSize: {1}", rbMessage.FileAttachment.Name, Helper.HumanizeFileSize(rbMessage.FileAttachment.Size));
                }
                message.Body = content;

            }
            return message;
        }

#endregion PRIVATE METHOD

#region EVENTS FROM RAINBOW SDK
        private void RbInstantMessaging_MessageReceived(object sender, Rainbow.Events.MessageEventArgs e)
        {
            if (e.ConversationId == this.conversationId)
            {
                InstantMessaging.Model.Message newMsg = GetMessageFromRBMessage(e.Message, rbConversation.Type);
                if (newMsg == null)
                    return;

                newMessageAdded = true;

                lock (lockObservableMessagesList)
                {
                    // Do we have already some message ?
                    if (MessagesList.Count == 0)
                    {
                        MessagesList.Add(newMsg);
                    }
                    else
                    {
                        // Add to the list but need to check date
                        InstantMessaging.Model.Message storedMsg;
                        bool newMsgAdded = false;
                        int nb = MessagesList.Count - 1;

                        for (int i = nb; i>0; i--)
                        {
                            storedMsg = MessagesList[i];
                            if(newMsg.MessageDateTime > storedMsg.MessageDateTime)
                            {
                                if(i == nb)
                                    MessagesList.Add(newMsg);
                                else
                                    MessagesList.Insert(i, newMsg);
                                newMsgAdded = true;
                                break;
                            }
                        }
                        // If we don't have already added the new message, we insert it to the first place
                        if (!newMsgAdded)
                            MessagesList.Insert(0, newMsg);
                    }
                }
            }
        }

        private void RbContacts_ContactInfoChanged(object sender, Rainbow.Events.JidEventArgs e)
        {
            // Need to update this contact in each messages (not avatar part)
            UpdateMessagesForJid(e.Jid, false, true);
        }

        private void RbContacts_ContactAdded(object sender, Rainbow.Events.JidEventArgs e)
        {
            // Need to update this contact in each messages (avatar + display name)
            UpdateMessagesForJid(e.Jid, true, true);
        }

#endregion EVENTS FROM RAINBOW SDK

#region EVENTS FROM AVATAR POOL

        private void AvatarPool_BubbleAvatarChanged(object sender, Rainbow.Events.IdEventArgs e)
        {
            if( (rbConversation != null) 
                && (rbConversation.Type == Rainbow.Model.Conversation.ConversationType.Room)
                && (rbConversation.Id == e.Id) )
            {
                if (Conversation != null)
                    Conversation.AvatarSource = Helper.GetConversationAvatarImageSource(Conversation);
            }
        }

        private void AvatarPool_ContactAvatarChanged(object sender, Rainbow.Events.IdEventArgs e)
        {
            // Check Conversation Avatar
            if ((rbConversation != null)
                && (rbConversation.Type == Rainbow.Model.Conversation.ConversationType.User)
                && (rbConversation.Id == e.Id))
            {
                if (Conversation != null)
                    Conversation.AvatarSource = Helper.GetConversationAvatarImageSource(Conversation);
            }

            // Need to update this contact in each messages (just avatar part)
            Contact contact = XamarinApplication.RbContacts.GetContactFromContactId(e.Id);
            if (contact != null)
                UpdateMessagesForJid(contact.Jid_im, true, false);
        }

#endregion EVENTS FROM AVATAR POOL
    }
}
