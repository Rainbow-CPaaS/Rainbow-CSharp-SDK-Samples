using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;

using InstantMessaging.Helpers;
using InstantMessaging.Model;

using Rainbow;
using Rainbow.Helpers;
using Rainbow.Model;

using log4net;

namespace InstantMessaging
{
    public class ConversationStreamViewModel : ObservableObject
    {
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(ConversationStreamViewModel));

        private static readonly int NB_MESSAGE_LOADED_BY_ROW = 40;

        private readonly String currentContactJid;
        private readonly String conversationId; // ConversationId of this Conversation
        private readonly Conversation rbConversation = null; // Rainbow Conversation object

        private readonly AvatarPool avatarPool;
        private readonly InstantMessaging.App XamarinApplication;

        private readonly Object lockObservableMessagesList = new Object(); // To lock access to the observable collection: 'MessagesList'

        private readonly Object lockContactsListInvolved = new Object(); // To lock access to: 'contactsListInvolved'
        private readonly List<String> contactsListInvolved = new List<String>(); // To store list of contacts involved in this Conversation (by Jid)

        private readonly Object lockRepliedContactsListInvolved = new Object(); // To lock access to: 'repliedContactsListInvolved'
        private readonly List<String> repliedContactsListInvolved = new List<String>(); // To store list of contacts involved in reply context in this Conversation (by Jid)

        private readonly Object lockUnknownRepliedMessagesListInvolved = new Object(); // To lock access to: 'repliedMessagesListInvolved'
        private readonly Dictionary<String, String> unknownRepliedMessagesListInvolved = new Dictionary<String, String>(); // To store list of messages involved in reply context in this Conversation (by message Id / message Stamp) but unknown for the moment
        private BackgroundWorker backgroundWorkerUnknownrepliedMessage = null;

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

            currentContactJid = XamarinApplication.RbContacts.GetCurrentContactJid();

            // Store conversation Id
            this.conversationId = conversationId;

            // Create default ConversationStream object
            ConversationStream = new ConversationStream
            {
                LoadingIndicatorIsVisible = "False",
                ListViewIsEnabled = "True"
            };

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
            {
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
                });
            });
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

        private void AddToModelRbMessages(List<Rainbow.Model.Message> rbMessagesList)
        {
            List<InstantMessaging.Model.Message> messagesList = new List<InstantMessaging.Model.Message>();
            foreach (Rainbow.Model.Message rbMessage in rbMessagesList)
            {
                InstantMessaging.Model.Message msg = GetMessageFromRBMessage(rbMessage, rbConversation.Type);
                if (msg != null)
                {
                    messagesList.Insert(0, msg);
                    msg.AvatarSource = Helper.GetContactAvatarImageSource(msg.PeerId);
                }
            }

            lock (lockObservableMessagesList)
            {
                if (MessagesList.Count == 0)
                    MessagesList.ReplaceRange(messagesList);
                else
                    MessagesList.AddRange(messagesList, System.Collections.Specialized.NotifyCollectionChangedAction.Reset, 0);
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

        private void AddUnknownRepliedMessageInvolved(String msgId, String msgStamp)
        {
            lock (lockUnknownRepliedMessagesListInvolved)
            {
                if ((!String.IsNullOrEmpty(msgId)) && (!unknownRepliedMessagesListInvolved.ContainsKey(msgId)))
                {
                    unknownRepliedMessagesListInvolved.Add(msgId, msgStamp);
                }
            }

            if (backgroundWorkerUnknownrepliedMessage == null)
            {
                backgroundWorkerUnknownrepliedMessage = new BackgroundWorker();
                backgroundWorkerUnknownrepliedMessage.DoWork += BackgroundWorkerUnknownrepliedMessage_DoWork;
                backgroundWorkerUnknownrepliedMessage.WorkerSupportsCancellation = true;
                backgroundWorkerUnknownrepliedMessage.RunWorkerCompleted += BackgroundWorkerUnknownrepliedMessage_RunWorkerCompleted; ;
            }

            if (!backgroundWorkerUnknownrepliedMessage.IsBusy)
                backgroundWorkerUnknownrepliedMessage.RunWorkerAsync();

        }

        private void BackgroundWorkerUnknownrepliedMessage_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (unknownRepliedMessagesListInvolved.Count > 0)
                AddUnknownRepliedMessageInvolved(null, null);
        }

        private void BackgroundWorkerUnknownrepliedMessage_DoWork(object sender, DoWorkEventArgs e)
        {
            bool canContinue;
            int index = 0;
            do
            {
                if(unknownRepliedMessagesListInvolved.Count > 0)
                {
                    if (index > unknownRepliedMessagesListInvolved.Count)
                        index = 0;

                    String msgId = unknownRepliedMessagesListInvolved.Keys.ToList()[index];
                    String msgStamp = unknownRepliedMessagesListInvolved[msgId];
                    if (AskMessageInfo(msgId, msgStamp))
                        unknownRepliedMessagesListInvolved.Remove(msgId);
                    else
                        index++;
                }
                canContinue = (unknownRepliedMessagesListInvolved.Count > 0);
            }
            while (canContinue);
        }

        private bool AskMessageInfo(String msgId, String msgStamp)
        {
            Boolean result = false;
            ManualResetEvent manualEvent = new ManualResetEvent(false);

            XamarinApplication.RbInstantMessaging.GetOneMessageFromConversationId(this.conversationId, msgId, msgStamp, callback =>
            {
                if(callback.Result.Success)
                {
                    result = true;
                    Rainbow.Model.Message rbMessage = callback.Data;
                    if (rbMessage != null)
                    {
                        List<Model.Message> messagesList = GetMessagesByReplyId(msgId);
                        foreach (Model.Message message in messagesList)
                        {
                            SetReplyPartOfMessage(message, rbMessage);
                        }
                    }
                }
                manualEvent.Set();
            });

            manualEvent.WaitOne();
            manualEvent.Dispose();

            return result;
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

        private List<InstantMessaging.Model.Message> GetMessagesByReplyPeerJid(String peerJid)
        {
            List<InstantMessaging.Model.Message> result = new List<InstantMessaging.Model.Message>();
            lock (lockObservableMessagesList)
            {
                foreach (InstantMessaging.Model.Message msg in MessagesList)
                {
                    if (msg.ReplyPeerJid == peerJid)
                        result.Add(msg);
                }
            }
            return result;
        }

        private List<InstantMessaging.Model.Message> GetMessagesByReplyId(String replyId)
        {
            List<InstantMessaging.Model.Message> result = new List<InstantMessaging.Model.Message>();
            lock (lockObservableMessagesList)
            {
                foreach (InstantMessaging.Model.Message msg in MessagesList)
                {
                    if (msg.ReplyId == replyId)
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
                                message.BackgroundColor = Color.FromHex(Rainbow.Helpers.AvatarPool.GetColorFromDisplayName(message.PeerDisplayName));
                                message.ReplyBackgroundColor = Color.FromHex(Rainbow.Helpers.AvatarPool.GetDarkerColorFromDisplayName(message.PeerDisplayName));
                            }
                        }
                    }
                }
                else
                    log.WarnFormat("[UpdateMessagesForJid] peerJid:[{0}] found but related contact not found", peerJid);
            }
        }

        private void UpdateRepliedMessagesForJid(String peerJid)
        {
            if(repliedContactsListInvolved.Contains(peerJid))
            {
                Contact contactReply = XamarinApplication.RbContacts.GetContactFromContactJid(peerJid);
                if (contactReply != null)
                {
                    String displayName = Util.GetContactDisplayName(contactReply, AvatarPool.Instance.GetFirstNameFirst());

                    List<InstantMessaging.Model.Message> messages = GetMessagesByReplyPeerJid(peerJid);
                    lock (lockObservableMessagesList)
                    {
                        foreach (InstantMessaging.Model.Message message in messages)
                        {
                            log.DebugFormat("[UpdateRepliedMessagesForJid] peerJid:[{0}] - message.Id:[{1}] - displayName:[{2}]", peerJid, message.Id, displayName);

                            message.ReplyPartIsVisible = "True";

                            message.ReplyPeerId = contactReply.Id;
                            message.ReplyPeerDisplayName = displayName;
                        }
                    }
                }
                else
                    log.WarnFormat("[UpdateRepliedMessagesForJid] peerJid:[{0}] found but related contact not found", peerJid);
            }
        }

        private InstantMessaging.Model.Message GetMessageFromRBMessage(Rainbow.Model.Message rbMessage, String conversationType)
        {
            InstantMessaging.Model.Message message = null;
            if (rbMessage != null)
            {
                InstantMessaging.App XamarinApplication = (InstantMessaging.App)Xamarin.Forms.Application.Current;
                AvatarPool avatarPool = AvatarPool.Instance;

                //log.DebugFormat("[GetMessageFromRBMessage] Message.Id:[{0}] - Message.ReplaceId:[{1}] - DateTime:[{2}] - Content:[{3}]", rbMessage.Id, rbMessage.ReplaceId, rbMessage.Date.ToString("o"), rbMessage.Content);

                message = new InstantMessaging.Model.Message();
                message.EventMessageBodyPart2Color = Color.Black; // Set default value

                Rainbow.Model.Contact contact = XamarinApplication.RbContacts.GetContactFromContactJid(rbMessage.FromJid);
                if (contact != null)
                {
                    message.PeerId = contact.Id;

                    message.PeerDisplayName = Util.GetContactDisplayName(contact, avatarPool.GetFirstNameFirst());
                    message.BackgroundColor = Color.FromHex(Rainbow.Helpers.AvatarPool.GetColorFromDisplayName(message.PeerDisplayName));
                }
                else
                {
                    message.BackgroundColor = Color.FromHex(Rainbow.Helpers.AvatarPool.GetColorFromDisplayName(""));

                    // We ask to have more info about this contact usin AvatarPool
                    avatarPool.AddUnknownContactToPoolByJid(rbMessage.FromJid);
                }

                // We have the display name only in Room / Bubble context
                message.PeerDisplayNameIsVisible = (conversationType == Rainbow.Model.Conversation.ConversationType.Room) ? "True" : "False";

                message.Id = rbMessage.Id;
                message.PeerJid = rbMessage.FromJid;

                message.MessageDateTime = rbMessage.Date;
                message.MessageDateDisplay = Helper.HumanizeDateTime(rbMessage.Date);

                // Is-it an "Event message" ?
                String content;
                if (!String.IsNullOrEmpty(rbMessage.BubbleEvent))
                {
                    message.IsEventMessage = "True";
                    message.EventMessageBodyPart1 = Helper.GetBubbleEventMessageBody(contact, rbMessage.BubbleEvent);
                }
                // Is-it an "CallLog message" ?
                else if (rbMessage.CallLogAttachment != null)
                {
                    SetCallLogPartOfMessage(message, rbMessage.CallLogAttachment);
                }
                else
                {
                    message.IsEventMessage = "False";
                    message.BodyFontAttributes = FontAttributes.None;
                    message.BodyColor = (message.PeerJid == currentContactJid) ? Color.Black : Color.White;

                    if (!String.IsNullOrEmpty(rbMessage.Content))
                    {
                        content = rbMessage.Content;
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(rbMessage.ReplaceId))
                            content = "";
                        else
                        {
                            content = "This message was deleted.";
                            message.BodyFontAttributes = FontAttributes.Italic;
                            message.BodyColor = (message.PeerJid == currentContactJid) ? Color.Gray : Color.FromHex(Rainbow.Helpers.AvatarPool.GetDarkerColorFromDisplayName(message.PeerDisplayName));
                        }
                    }

                    // Reply part
                    // By default is not displayed
                    message.ReplyPartIsVisible = "False";
                    if (rbMessage.ReplyMessage != null)
                    {
                        // Store Id of this reply message
                        message.ReplyId = rbMessage.ReplyMessage.Id;

                        // Set background color
                        message.ReplyBackgroundColor = Color.FromHex(Rainbow.Helpers.AvatarPool.GetDarkerColorFromDisplayName(message.PeerDisplayName));

                        // We need to get Name and text of the replied message ...
                        Rainbow.Model.Message rbRepliedMessage = XamarinApplication.RbInstantMessaging.GetOneMessageFromConversationIdFromCache(this.conversationId, rbMessage.ReplyMessage.Id);
                        if (rbRepliedMessage != null)
                            SetReplyPartOfMessage(message, rbRepliedMessage);
                        else
                            AddUnknownRepliedMessageInvolved(rbMessage.ReplyMessage.Id, rbMessage.ReplyMessage.Stamp);
                    }
                    else
                    {
                        // Set default color to avoir warning
                        message.ReplyBackgroundColor = Color.White;
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

                // We store info about this contact in message context
                AddContactInvolved(message.PeerJid);
            }
            return message;
        }

        private void SetReplyPartOfMessage(Model.Message message, Rainbow.Model.Message rbRepliedMessage)
        {
            // Store Jid of the message sender
            message.ReplyPeerJid = rbRepliedMessage.FromJid;
            if (String.IsNullOrEmpty(rbRepliedMessage.Content))
            {
                if (rbRepliedMessage.FileAttachment != null)
                    message.ReplyBody = rbRepliedMessage.FileAttachment.Name;
                else
                    message.ReplyBody = rbRepliedMessage.Id; // Bad display but should permit to debug this situation
            }
            else
                message.ReplyBody = rbRepliedMessage.Content;


            log.DebugFormat("[SetReplyPartOfMessage] - message.Id:[{0}] - replyMsgId:[{1}] - replyBody:[{2}] - ContactJid:[{3}]", message.Id, rbRepliedMessage.Id, message.ReplyBody, rbRepliedMessage.FromJid);

            Rainbow.Model.Contact contactReply = XamarinApplication.RbContacts.GetContactFromContactJid(rbRepliedMessage.FromJid);
            if (contactReply != null)
            {
                // Reply part is visible
                message.ReplyPartIsVisible = "True";

                message.ReplyPeerId = contactReply.Id;
                message.ReplyPeerDisplayName = Util.GetContactDisplayName(contactReply, avatarPool.GetFirstNameFirst());
            }
            else
            {
                log.DebugFormat("[SetReplyPartOfMessage] - message.Id:[{0}] - replyMsgId:[{1}] - UnknownContactJid[{2}]", message.Id, rbRepliedMessage.Id, rbRepliedMessage.FromJid);
                // We ask to have more info about this contact using AvatarPool
                avatarPool.AddUnknownContactToPoolByJid(rbRepliedMessage.FromJid);
            }

            // We store info about this contact in reply context
            AddRepliedContactInvolved(rbRepliedMessage.FromJid);
        }

        private void SetCallLogPartOfMessage(Model.Message message, CallLogAttachment callLogAttachment)
        {
            if(callLogAttachment != null)
            {
                message.IsEventMessage = "True";

                message.CallDuration = callLogAttachment.Duration;
                message.CallState = callLogAttachment.State.ToString();
                message.CallType = callLogAttachment.Type.ToString();

                if (callLogAttachment.Caller == currentContactJid)
                {
                    message.CallOriginator = "True";
                    message.CallOtherJid = callLogAttachment.Callee;
                }
                else
                {
                    message.CallOriginator = "False";
                    message.CallOtherJid = callLogAttachment.Caller;
                }

                SetEventPartFromCallLog(message);
            }
        }

        private void SetEventPartFromCallLog(Model.Message message)
        {
            Rainbow.Model.Contact contact = XamarinApplication.RbContacts.GetContactFromContactJid(message.CallOtherJid);
            if (contact != null)
            {
                String displayName = Util.GetContactDisplayName(contact, AvatarPool.Instance.GetFirstNameFirst());
                if (message.CallState == CallLog.LogState.ANSWERED.ToString())
                {
                    if (message.CallOriginator == "True")
                        message.EventMessageBodyPart1 = "You called " + displayName + ".";
                    else
                        message.EventMessageBodyPart1 = displayName  + " has called you.";

                    double nbSecs = Math.Round((double)message.CallDuration / 1000);
                    int mns = (int) (nbSecs / 60);
                    int sec = (int)Math.Round(nbSecs - (mns * 60));
                    message.EventMessageBodyPart2 = "Duration: " + ( (mns>0) ? mns + ((mns>1) ? "mns " : "mn ") : "") + ((sec > 0) ? sec + "s" : "");
                }
                else
                {
                    if (message.CallOriginator == "True")
                        message.EventMessageBodyPart1 = "You called " + displayName + ".";
                    else
                        message.EventMessageBodyPart1 = displayName + " has called you.";

                    message.EventMessageBodyPart2 = (message.CallState == CallLog.LogState.MISSED.ToString()) ? "Missed call" : "Failed call";
                    message.EventMessageBodyPart2Color = Color.Red;
                }
            }
            else
            {
                log.DebugFormat("[SetEventPartFromCallLog] - message.Id:[{0}] - UnknowContactJid:[{1}]", message.Id, message.CallOtherJid);
                // We ask to have more info about this contact using AvatarPool
                avatarPool.AddUnknownContactToPoolByJid(message.CallOtherJid);
            }
        }

#endregion PRIVATE METHOD

#region EVENTS FROM RAINBOW SDK
        private void RbInstantMessaging_MessageReceived(object sender, Rainbow.Events.MessageEventArgs e)
        {
            if (e.ConversationId == this.conversationId)
            {
                //log.DebugFormat("[RbInstantMessaging_MessageReceived] - FromJId:[{0}] - ToJid:[{1}] - CarbonCopy:[{2}] - Message.Id:[{3}]", e.Message.FromJid, e.Message.ToJid, e.CarbonCopy, e.Message.Id);
                InstantMessaging.Model.Message newMsg = GetMessageFromRBMessage(e.Message, rbConversation.Type);
                if (newMsg == null)
                {
                    log.WarnFormat("[RbInstantMessaging_MessageReceived] - Impossible to have Model.Message from XMPP Message - Message.Id:[{3}]", e.Message.Id);
                    return;
                }

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
                        bool newMsgInserted = false;
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
                                newMsgInserted = true;
                                break;
                            }
                        }
                        // If we don't have already added the new message, we insert it to the first place
                        if (!newMsgInserted)
                            MessagesList.Insert(0, newMsg);
                    }
                }
            }
        }

        private void RbContacts_ContactInfoChanged(object sender, Rainbow.Events.JidEventArgs e)
        {
            // Need to update this contact in each messages (not avatar part)
            UpdateMessagesForJid(e.Jid, false, true);

            // Need to update this contact in each replied part
            UpdateRepliedMessagesForJid(e.Jid);
        }

        private void RbContacts_ContactAdded(object sender, Rainbow.Events.JidEventArgs e)
        {
            // Need to update this contact in each messages (avatar + display name)
            UpdateMessagesForJid(e.Jid, true, true);

            // Need to update this contact in each replied part
            UpdateRepliedMessagesForJid(e.Jid);
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
