using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using InstantMessaging.Helpers;
using InstantMessaging.Pool;
using InstantMessaging.View;
using InstantMessaging.ViewModel;

using Rainbow;
using Rainbow.Events;
using Rainbow.Model;

using log4net;

namespace InstantMessaging.Model
{
    class MessagesModel
    {
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(LoginModel));

        private static readonly int NB_MESSAGE_LOADED_BY_ROW = 40;

        App CurrentApplication = null;
        MainView ApplicationMainWindow = null;
        String CurrentConversationId = null;
        Rainbow.Model.Conversation CurrentConversation = null;

        private ScrollViewer ScrollViewer;

        private Bubbles RbBubbles = null;
        private Contacts RbContacts = null;
        private Conversations RbConversations = null;
        private Rainbow.InstantMessaging RbInstantMessaging = null;

        private readonly AvatarPool AvatarPool; // To manage avatars
        private readonly FilePool FilePool; // To manage avatars


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


        public RangeObservableCollection<MessageViewModel> MessagesList { get; set; } // Need to be public - Used as Binding from XAML

        public MessagesModel()
        {
            // Get Application and Main Window
            CurrentApplication = (App)System.Windows.Application.Current;
            ApplicationMainWindow = CurrentApplication.ApplicationMainWindow;

            // Create Observable list of Message
            MessagesList = new RangeObservableCollection<MessageViewModel>();

            // Chesk if we use dummy data (to test UI)
            if (CurrentApplication.USE_DUMMY_DATA)
            {
                LoadFakeMessages();
                return;
            }

            // Get Rainbow Objects
            RbBubbles = CurrentApplication.RbBubbles;
            RbContacts = CurrentApplication.RbContacts;
            RbConversations = CurrentApplication.RbConversations;
            RbInstantMessaging = CurrentApplication.RbInstantMessaging;

            // Get Pools objects
            AvatarPool = InstantMessaging.Pool.AvatarPool.Instance;
            FilePool = InstantMessaging.Pool.FilePool.Instance;

            // Manage event(s) from ApplicationMainWindow
            ApplicationMainWindow.ConversationSelectedFromConversationsList += ApplicationMainWindow_ConversationSelectedFromConversationsList;
            ApplicationMainWindow.FavoriteSelectedFromFavoritesList += ApplicationMainWindow_FavoriteSelectedFromFavoritesList;

            // Manage event(s) from FilePool
            FilePool.FileDescriptorAvailable += FilePool_FileDescriptorAvailable; ;
            FilePool.ThumbnailAvailable += FilePool_ThumbnailAvailable; ;

            // Manage event(s) from AvatarPool
            AvatarPool.ContactAvatarChanged += AvatarPool_ContactAvatarChanged; ;
            AvatarPool.BubbleAvatarChanged += AvatarPool_BubbleAvatarChanged; ;

            // Manage event(s) from InstantMessaging
            RbInstantMessaging.MessageReceived += RbInstantMessaging_MessageReceived;
            RbInstantMessaging.ReceiptReceived += RbInstantMessaging_ReceiptReceived;
            RbInstantMessaging.MessagesAllRead += RbInstantMessaging_MessagesAllRead;
            RbInstantMessaging.UserTypingChanged += RbInstantMessaging_UserTypingChanged;

            // Manage event(s) from Contacts
            RbContacts.ContactAdded += RbContacts_ContactAdded; ;
            RbContacts.ContactInfoChanged += RbContacts_ContactInfoChanged;
        }

        public void SetScrollViewer(ScrollViewer sv)
        {
            ScrollViewer = sv;
        }

        #region EVENTS FIRED BY RAINBOW SDK
        private void RbContacts_ContactInfoChanged(object sender, JidEventArgs e)
        {
            if (CurrentConversation == null)
                return;

            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    // Need to update this contact in each messages (not avatar part)
                    UpdateMessagesForJid(e.Jid, false, true);

                    // Need to update this contact in each replied part
                    UpdateRepliedMessagesForJid(e.Jid);
                }));
            }
        }

        private void RbContacts_ContactAdded(object sender, JidEventArgs e)
        {
            if (CurrentConversation == null)
                return;

            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    // Need to update this contact in each messages (avatar + display name)
                    UpdateMessagesForJid(e.Jid, true, true);

                    // Need to update this contact in each replied part
                    UpdateRepliedMessagesForJid(e.Jid);
                }));
            }
        }

        private void RbInstantMessaging_UserTypingChanged(object sender, UserTypingEventArgs e)
        {
            if (CurrentConversation == null)
                return;
            //TO DO
        }

        private void RbInstantMessaging_MessagesAllRead(object sender, IdEventArgs e)
        {
            // Set to ClientRead all messages in the list
            if (e.Id == CurrentConversationId)
            {
                log.DebugFormat("[RbInstantMessaging_MessagesAllRead] conversationId:[{0}]", CurrentConversationId);
                if (System.Windows.Application.Current != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        lock (lockObservableMessagesList)
                        {
                            foreach (MessageViewModel message in MessagesList)
                            {
                                if (message.PeerJid == CurrentApplication.CurrentUserJid)
                                    SetReceiptPartOfMessage(message, ReceiptType.ClientRead);
                            }
                        }
                    }));
                }
            }
        }

        private void RbInstantMessaging_ReceiptReceived(object sender, ReceiptReceivedEventArgs e)
        {
            if (e.ConversationId == CurrentConversationId)
            {
                log.DebugFormat("[RbInstantMessaging_ReceiptReceived] MessageId:[{0}] - ReceiptType:[{1}]", e.MessageId, e.ReceiptType);
                MessageViewModel message = GetMessageByMessageId(e.MessageId);
                if (message != null)
                {
                    if (System.Windows.Application.Current != null)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            SetReceiptPartOfMessage(message, e.ReceiptType);
                        }));
                    }
                }
            }
        }

        private void RbInstantMessaging_MessageReceived(object sender, MessageEventArgs e)
        {
            if (e.ConversationId == CurrentConversationId)
            {
                if (System.Windows.Application.Current != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        
                    log.DebugFormat("[RbInstantMessaging_MessageReceived] - FromJId:[{0}] - ToJid:[{1}] - CarbonCopy:[{2}] - Message.Id:[{3}] - Message.ReplaceId:[{4}]", e.Message.FromJid, e.Message.ToJid, e.CarbonCopy, e.Message.Id, e.Message.ReplaceId);

                    MessageViewModel newMsg = GetMessageViewModelFromRBMessage(e.Message, CurrentConversation.Type);
                    if (newMsg == null)
                    {
                        log.WarnFormat("[RbInstantMessaging_MessageReceived] - Impossible to have Model.Message from XMPP Message - Message.Id:[{3}]", e.Message.Id);
                        return;
                    }

                    // Since message is not null, set the Avatar Source
                    newMsg.AvatarImageSource = Helper.GetContactAvatarImageSource(newMsg.PeerId);

                    // Manage incoming REPLACE message
                    if (!String.IsNullOrEmpty(e.Message.ReplaceId))
                    {
                        MessageViewModel previousMessage = GetMessageByMessageId(e.Message.Id);
                        if (previousMessage == null)
                        {
                            // We do nothing in this case ... Message not in the cache, so not visible ...
                            return;
                        }

                        previousMessage.Body = newMsg.Body;
                        previousMessage.BodyIsVisible = String.IsNullOrEmpty(previousMessage.Body) ? Visibility.Collapsed : Visibility.Visible;

                        previousMessage.EditedIsVisible = Visibility.Visible;
                    }
                    // Manage incoming NEW message
                    else
                    {
                        // It's a new message to add in the list
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
                                MessageViewModel storedMsg;
                                bool newMsgInserted = false;
                                int nb = MessagesList.Count - 1;

                                for (int i = nb; i > 0; i--)
                                {
                                    storedMsg = MessagesList[i];
                                    if (newMsg.MessageDateTime > storedMsg.MessageDateTime)
                                    {
                                        if (i == nb)
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

                    // Mark the message as read
                    // TODO: LATER MUST CHECK THAT THIS CONVERSATION IS CURRENTLY DISPLAYED / FOCUSED
                        RbInstantMessaging.MarkMessageAsRead(CurrentConversationId, newMsg.Id, null);
                    }));
                }
            }
        }

#endregion EVENTS FIRED BY RAINBOW SDK

#region EVENTS FIRED BY AVATAR POOL
        private void AvatarPool_BubbleAvatarChanged(object sender, IdEventArgs e)
        {
            if (CurrentConversation == null)
                return;

            // TODO: need to update Bubble Avatar (if necessary)
        }

        private void AvatarPool_ContactAvatarChanged(object sender, IdEventArgs e)
        {
            if (CurrentConversation == null)
                return;

            // TODO: need to update Bubble Avatar (if necessary)

            // Need to update this contact in each messages (just avatar part)
            Contact contact = RbContacts.GetContactFromContactId(e.Id);
            if (contact != null)
            {
                if (System.Windows.Application.Current != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        UpdateMessagesForJid(contact.Jid_im, true, false);
                    }));
                }
                
            }
        }

#endregion EVENTS FIRED BY AVATAR POOL

#region EVENTS FIRED BY FILE POOL
        private void FilePool_ThumbnailAvailable(object sender, IdEventArgs e)
        {
            String conversationId = FilePool.GetConversationIdForFileDescriptorId(e.Id);
            if (conversationId == CurrentConversationId)
            {
                MessageViewModel message = GetMessageByFileDescriptorId(e.Id);
                if (message != null)
                {
                    if (System.Windows.Application.Current != null)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            SetFileAttachmentSourceOfMessage(message, e.Id);
                        }));
                    }
                }
            }
        }

        private void FilePool_FileDescriptorAvailable(object sender, IdEventArgs e)
        {
            String conversationId = FilePool.GetConversationIdForFileDescriptorId(e.Id);
            if (conversationId == CurrentConversationId)
            {
                MessageViewModel message = GetMessageByFileDescriptorId(e.Id);
                if (message != null)
                {
                    // TODO
                }
            }
        }

#endregion EVENTS FIRED BY FILE POOL

#region EVENTS RAISED BY ApplicationMainWindow

        private void ApplicationMainWindow_FavoriteSelectedFromFavoritesList(object sender, IdEventArgs e)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    // Need to get/create the conversation Id associated to this Favorite Id
                }));
            }
        }

        private void ApplicationMainWindow_ConversationSelectedFromConversationsList(object sender, IdEventArgs e)
        {
            
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ResetMessageListUsingConversationId(e.Id);

                    ScrollViewer.ScrollToBottom();
                }));
            }
        }

#endregion EVENTS RAISED BY ApplicationMainWindow

        private void AddContactInvolved(String peerJid)
        {
            lock (lockContactsListInvolved)
            {
                if ((!String.IsNullOrEmpty(peerJid)) && (!contactsListInvolved.Contains(peerJid)))
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
                if (unknownRepliedMessagesListInvolved.Count > 0)
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

            RbInstantMessaging.GetOneMessageFromConversationId(CurrentConversationId, msgId, msgStamp, callback =>
            {
                if (callback.Result.Success)
                {
                    result = true;
                    Rainbow.Model.Message rbMessage = callback.Data;
                    if (rbMessage != null)
                    {
                        List<MessageViewModel> messagesList = GetMessagesByReplyId(msgId);
                        if ((messagesList != null) && (messagesList.Count > 0))
                            {
                            if (System.Windows.Application.Current != null)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    foreach (MessageViewModel message in messagesList)
                                    {
                                        SetReplyPartOfMessage(message, rbMessage);
                                    }
                                }));
                            }
                        }
                    }
                }
                manualEvent.Set();
            });

            manualEvent.WaitOne();
            manualEvent.Dispose();

            return result;
        }

        private void UpdateMessagesForJid(String peerJid, bool updateAvatar, bool updateDisplayName)
        {
            if (contactsListInvolved.Contains(peerJid))
            {
                Contact contact = RbContacts.GetContactFromContactJid(peerJid);
                if (contact != null)
                {
                    log.DebugFormat("[UpdateMessagesForJid] peerJid:[{0}] - peerId:[{1}] - updateAvatar[{2}] - updateDisplayName:[{3}]", peerJid, contact.Id, updateAvatar, updateDisplayName);
                    BitmapImage imageSource;
                    String displayName;

                    if (updateAvatar)
                        imageSource = Helper.GetContactAvatarImageSource(contact.Id);
                    else
                        imageSource = null;

                    if (updateDisplayName)
                        displayName = Util.GetContactDisplayName(contact, AvatarPool.Instance.GetFirstNameFirst());
                    else
                        displayName = null;

                    List<MessageViewModel> messages = GetMessagesByPeerJid(peerJid);
                    lock (lockObservableMessagesList)
                    {
                        foreach (MessageViewModel message in messages)
                        {
                            if (updateAvatar)
                                message.AvatarImageSource = imageSource;

                            if (updateDisplayName)
                            {
                                message.PeerDisplayName = displayName;
                                message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;
                                message.ReplyBackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetDarkerColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;
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
            if (repliedContactsListInvolved.Contains(peerJid))
            {
                Contact contactReply = RbContacts.GetContactFromContactJid(peerJid);
                if (contactReply != null)
                {
                    String displayName = Util.GetContactDisplayName(contactReply, AvatarPool.Instance.GetFirstNameFirst());

                    List<MessageViewModel> messages = GetMessagesByReplyPeerJid(peerJid);
                    lock (lockObservableMessagesList)
                    {
                        foreach (MessageViewModel message in messages)
                        {
                            log.DebugFormat("[UpdateRepliedMessagesForJid] peerJid:[{0}] - message.Id:[{1}] - displayName:[{2}]", peerJid, message.Id, displayName);

                            message.ReplyPartIsVisible = Visibility.Visible;

                            message.ReplyPeerId = contactReply.Id;
                            message.ReplyPeerDisplayName = displayName;
                        }
                    }
                }
                else
                    log.WarnFormat("[UpdateRepliedMessagesForJid] peerJid:[{0}] found but related contact not found", peerJid);
            }
        }
        private List<MessageViewModel> GetMessagesByReplyId(String replyId)
        {
            List<MessageViewModel> result = new List<MessageViewModel>();
            lock (lockObservableMessagesList)
            {
                foreach (MessageViewModel msg in MessagesList)
                {
                    if (msg.ReplyId == replyId)
                        result.Add(msg);
                }
            }
            return result;
        }

        private List<MessageViewModel> GetMessagesByPeerJid(String peerJid)
        {
            List<MessageViewModel> result = new List<MessageViewModel>();
            lock (lockObservableMessagesList)
            {
                foreach (MessageViewModel msg in MessagesList)
                {
                    if (msg.PeerJid == peerJid)
                        result.Add(msg);
                }
            }
            return result;
        }

        private List<MessageViewModel> GetMessagesByReplyPeerJid(String peerJid)
        {
            List<MessageViewModel> result = new List<MessageViewModel>();
            lock (lockObservableMessagesList)
            {
                foreach (MessageViewModel msg in MessagesList)
                {
                    if (msg.ReplyPeerJid == peerJid)
                        result.Add(msg);
                }
            }
            return result;
        }

        private MessageViewModel GetMessageByMessageId(String messageId)
        {
            MessageViewModel result = null;
            lock (lockObservableMessagesList)
            {
                foreach (MessageViewModel msg in MessagesList)
                {
                    if (msg.Id == messageId)
                    {
                        result = msg;
                        break;
                    }
                }
            }
            return result;
        }

        private MessageViewModel GetMessageByFileDescriptorId(String fileId)
        {
            MessageViewModel result = null;
            lock (lockObservableMessagesList)
            {
                foreach (MessageViewModel msg in MessagesList)
                {
                    if (msg.FileId == fileId)
                    {
                        result = msg;
                        break;
                    }
                }
            }
            return result;
        }

        public void SetFileAttachmentSourceOfMessage(MessageViewModel message, String fileId)
        {
            string filePath = FilePool.GetThumbnailFullFilePath(fileId);
            if (filePath != null)
            {
                try
                {
                    log.DebugFormat("[SetFileAttachmentSourceOfMessage] FileId:[{0}] - Use filePath:[{1}]", fileId, filePath);
                    using (Stream stream = new MemoryStream(File.ReadAllBytes(filePath)))
                    {
                        System.Drawing.Size size = AvatarPool.GetSize(stream);
                        double density = AvatarPool.GetDensity();

                        message.FileAttachmentImageSource = Helper.BitmapImageFromFile(filePath);
                        message.FileAttachmentImageWidth = (int)Math.Round((double)size.Width / density);
                        message.FileAttachmentImageHeight = (int)Math.Round((double)size.Height / density);
                        message.FileAttachmentIsVisible = Visibility.Visible;
                        message.FileInfoIsVisible = Visibility.Collapsed;
                    }
                }
                catch { }
            }
        }

        public void SetReceiptPartOfMessage(MessageViewModel message, Rainbow.Model.ReceiptType receiptType)
        {
            String receipt = receiptType.ToString();
            if (message.ReceiptType == receipt)
                return;

            message.ReceiptType = receipt;
            switch (receiptType)
            {
                case ReceiptType.ServerReceived:
                    message.ReceiptImageSource = Helper.GetBitmapImageFromResource("msg_check.png");
                    break;
                case ReceiptType.ClientReceived:
                    message.ReceiptImageSource = Helper.GetBitmapImageFromResource("msg_not_read.png");
                    break;
                case ReceiptType.ClientRead:
                    message.ReceiptImageSource = Helper.GetBitmapImageFromResource("msg_read.png");
                    break;
                case ReceiptType.None:
                    message.ReceiptImageSource = Helper.GetBitmapImageFromResource("msg_sending.png");
                    break;
            }
        }

        private void SetReplyPartOfMessage(MessageViewModel message, Rainbow.Model.Message rbRepliedMessage)
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

            Rainbow.Model.Contact contactReply = RbContacts.GetContactFromContactJid(rbRepliedMessage.FromJid);
            if (contactReply != null)
            {
                // Reply part is visible
                message.ReplyPartIsVisible = Visibility.Visible;

                message.ReplyPeerId = contactReply.Id;
                message.ReplyPeerDisplayName = Util.GetContactDisplayName(contactReply, AvatarPool.GetFirstNameFirst());
            }
            else
            {
                log.DebugFormat("[SetReplyPartOfMessage] - message.Id:[{0}] - replyMsgId:[{1}] - UnknownContactJid[{2}]", message.Id, rbRepliedMessage.Id, rbRepliedMessage.FromJid);
                // We ask to have more info about this contact using AvatarPool
                AvatarPool.AddUnknownContactToPoolByJid(rbRepliedMessage.FromJid);
            }

            // We store info about this contact in reply context
            AddRepliedContactInvolved(rbRepliedMessage.FromJid);
        }

        private void SetCallLogPartOfMessage(MessageViewModel message, CallLogAttachment callLogAttachment)
        {
            if (callLogAttachment != null)
            {
                message.IsEventMessage = true;

                message.CallDuration = callLogAttachment.Duration;
                message.CallState = callLogAttachment.State.ToString();
                message.CallType = callLogAttachment.Type.ToString();

                if (callLogAttachment.Caller == CurrentApplication.CurrentUserJid)
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

        private void SetEventPartFromCallLog(MessageViewModel message)
        {
            Rainbow.Model.Contact contact = RbContacts.GetContactFromContactJid(message.CallOtherJid);
            if (contact != null)
            {
                String displayName = Util.GetContactDisplayName(contact, AvatarPool.Instance.GetFirstNameFirst());
                if (message.CallState == CallLog.LogState.ANSWERED.ToString())
                {
                    if (message.CallOriginator == "True")
                        message.EventMessageBodyPart1 = "You called " + displayName + ".";
                    else
                        message.EventMessageBodyPart1 = displayName + " has called you.";

                    double nbSecs = Math.Round((double)message.CallDuration / 1000);
                    int mns = (int)(nbSecs / 60);
                    int sec = (int)Math.Round(nbSecs - (mns * 60));
                    message.EventMessageBodyPart2 = "Duration: " + ((mns > 0) ? mns + ((mns > 1) ? "mns " : "mn ") : "") + ((sec > 0) ? sec + "s" : "");
                }
                else
                {
                    if (message.CallOriginator == "True")
                        message.EventMessageBodyPart1 = "You called " + displayName + ".";
                    else
                        message.EventMessageBodyPart1 = displayName + " has called you.";

                    message.EventMessageBodyPart2 = (message.CallState == CallLog.LogState.MISSED.ToString()) ? "Missed call" : "Failed call";
                    message.EventMessageBodyPart2Color = Brushes.Red;
                }
            }
            else
            {
                log.DebugFormat("[SetEventPartFromCallLog] - message.Id:[{0}] - UnknowContactJid:[{1}]", message.Id, message.CallOtherJid);
                // We ask to have more info about this contact using AvatarPool
                AvatarPool.AddUnknownContactToPoolByJid(message.CallOtherJid);
            }
        }

        private MessageViewModel GetMessageViewModelFromRBMessage(Rainbow.Model.Message rbMessage, String conversationType)
        {
            MessageViewModel message = null;
            if (rbMessage != null)
            {
                //log.DebugFormat("[GetMessageViewModelFromRBMessage] Message.Id:[{0}] - Message.ReplaceId:[{1}] - DateTime:[{2}] - Content:[{3}]", rbMessage.Id, rbMessage.ReplaceId, rbMessage.Date.ToString("o"), rbMessage.Content);

                message = new MessageViewModel();
                message.EventMessageBodyPart2Color = Brushes.Gray; // Set default value

                Rainbow.Model.Contact contact = RbContacts.GetContactFromContactJid(rbMessage.FromJid);

                if (contact != null)
                {
                    message.PeerId = contact.Id;

                    if (message.PeerId == CurrentApplication.CurrentUserId)
                    {
                        message.PeerDisplayName = Util.GetContactDisplayName(contact, AvatarPool.GetFirstNameFirst());
                        message.BackgroundColor = Brushes.LightGray;
                    }
                    else
                    {
                        message.PeerDisplayName = Util.GetContactDisplayName(contact, AvatarPool.GetFirstNameFirst());
                        message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;
                    }
                }
                else
                {
                    // We ask to have more info about this contact usin AvatarPool
                    AvatarPool.AddUnknownContactToPoolByJid(rbMessage.FromJid);

                    // Try to get info from pool
                    AvatarsData.LightContact lightContact = AvatarPool.GetLightContact(null, rbMessage.FromJid);

                    if (lightContact != null)
                    {
                        message.PeerId = lightContact.Id;
                        message.PeerDisplayName = lightContact.DisplayName;
                        message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;
                    }
                    else
                    {
                        message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName("?")) as SolidColorBrush;
                    }
                }

                // We have the display name only in Room / Bubble context
                message.PeerDisplayNameIsVisible = (conversationType == Rainbow.Model.Conversation.ConversationType.Room) ? Visibility.Visible : Visibility.Collapsed;

                message.Id = rbMessage.Id;
                message.PeerJid = rbMessage.FromJid;

                message.MessageDateTime = rbMessage.Date;
                message.MessageDateDisplay = Helper.HumanizeDateTime(rbMessage.Date);

                // Receipt
                SetReceiptPartOfMessage(message, rbMessage.Receipt);

                // Is-it an "Event message" ?
                String content;
                if (!String.IsNullOrEmpty(rbMessage.BubbleEvent))
                {
                    message.IsEventMessage = true;
                    message.EventMessageBodyPart1 = Helper.GetBubbleEventMessageBody(contact, rbMessage.BubbleEvent);
                }
                // Is-it an "CallLog message" ?
                else if (rbMessage.CallLogAttachment != null)
                {
                    SetCallLogPartOfMessage(message, rbMessage.CallLogAttachment);
                }
                else
                {
                    message.IsEventMessage = false;
                    message.BodyFontStyle = FontStyles.Normal;
                    message.BodyColor = (message.PeerJid == CurrentApplication.CurrentUserJid) ? Brushes.Black : Brushes.White;

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
                            message.BodyFontStyle = FontStyles.Italic;
                            message.BodyColor = (message.PeerJid == CurrentApplication.CurrentUserJid) ? Brushes.Gray : new BrushConverter().ConvertFromString(AvatarPool.GetDarkerColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;
                        }
                    }

                    // Edited text visible ?
                    message.EditedIsVisible = String.IsNullOrEmpty(rbMessage.ReplaceId) ? Visibility.Collapsed : Visibility.Visible;

                    // Reply part
                    // By default is not displayed
                    message.ReplyPartIsVisible = Visibility.Collapsed;
                    if (rbMessage.ReplyMessage != null)
                    {
                        // Store Id of this reply message
                        message.ReplyId = rbMessage.ReplyMessage.Id;

                        // Set background color
                        if (message.PeerId == CurrentApplication.CurrentUserId)
                            message.ReplyBackgroundColor = Brushes.Gray;
                        else
                            message.ReplyBackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetDarkerColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

                        // We need to get Name and text of the replied message ...
                        Rainbow.Model.Message rbRepliedMessage = RbInstantMessaging.GetOneMessageFromConversationIdFromCache(CurrentConversationId, rbMessage.ReplyMessage.Id);
                        if (rbRepliedMessage != null)
                            SetReplyPartOfMessage(message, rbRepliedMessage);
                        else
                            AddUnknownRepliedMessageInvolved(rbMessage.ReplyMessage.Id, rbMessage.ReplyMessage.Stamp);
                    }
                    else
                    {
                        // Set default color to avoir warning
                        message.ReplyBackgroundColor = Brushes.White;
                    }

                    // FileAttachment
                    if (rbMessage.FileAttachment != null)
                    {
                        // Set Global info
                        message.FileAttachmentIsVisible = Visibility.Visible;
                        message.FileInfoIsVisible = Visibility.Visible;
                        message.FileId = rbMessage.FileAttachment.Id;
                        message.FileName = rbMessage.FileAttachment.Name;
                        message.FileSize = Helper.HumanizeFileSize(rbMessage.FileAttachment.Size);

                        if (FilePool.IsThumbnailFileAvailable(CurrentConversationId, rbMessage.FileAttachment.Id, rbMessage.FileAttachment.Name))
                        {
                            SetFileAttachmentSourceOfMessage(message, rbMessage.FileAttachment.Id);
                        }
                        else
                        {
                            // Ask more info about this file
                            FilePool.AskFileDescriptorDownload(CurrentConversationId, rbMessage.FileAttachment.Id);

                            // Set default icon
                            message.FileAttachmentImageSource = Helper.GetBitmapImageFromResource("icon_unknown_blue.png");
                            message.FileAttachmentImageWidth = 60;
                            message.FileAttachmentImageHeight = 30;
                        }
                    }
                    else
                    {
                        message.FileAttachmentIsVisible = Visibility.Collapsed;
                        message.FileInfoIsVisible = Visibility.Collapsed;

                        message.FileAttachmentImageWidth = 0;
                        message.FileAttachmentImageHeight = 0;
                    }

                    message.Body = content;
                    message.BodyIsVisible = String.IsNullOrEmpty(message.Body) ? Visibility.Collapsed : Visibility.Visible;
                }

                // We store info about this contact in message context
                AddContactInvolved(message.PeerJid);
            }
            return message;
        }

        private void AddToModelRbMessages(List<Rainbow.Model.Message>  rbMessagesList)
        {
            // We must be on UI Thread to update Messages object
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    lock (lockObservableMessagesList)
                    {
                        foreach (Rainbow.Model.Message rbMessage in rbMessagesList)
                        {
                            MessageViewModel msg = GetMessageViewModelFromRBMessage(rbMessage, CurrentConversation.Type);
                            if (msg != null)
                            {
                                msg.AvatarImageSource = Helper.GetContactAvatarImageSource(msg.PeerId);

                                MessagesList.Insert(0, msg);
                            }
                        }
                    }
                }));
            }
        }

        public void LoadMoreMessages()
        {
            // We are starting to load older messages
            loadingOlderMessages = true;

            // Display "loading indicator"
            //ConversationStream.LoadingIndicatorIsVisible = "True";
            //ConversationStream.ListViewIsEnabled = "False";

            Task task = new Task(() =>
            {
                RbInstantMessaging.GetMessagesFromConversationId(CurrentConversationId, NB_MESSAGE_LOADED_BY_ROW, callback =>
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
                            //ConversationStream.LoadingIndicatorIsVisible = "False";
                            //ConversationStream.ListViewIsEnabled = "True";
                        }
                        else
                        {
                            // We store the fact that we will now wait to scroll to message before to ask older messages
                            // Must been done before to add element to model
                            waitingScrollingDueToLoadingOlderMessages = true;

                            AddToModelRbMessages(callback.Data);

                            // We have stopped to load older messages
                            loadingOlderMessages = false;
                        }
                    }
                    else
                    {
                        //TODO
                    }
                });
            });
            task.Start();
        }

        private void ResetMessageListUsingConversationId(String id)
        {
            if(CurrentConversationId != id)
            {
                // Store Conversation info
                CurrentConversationId = id;
                CurrentConversation = RbConversations.GetConversationByIdFromCache(id);

                // Clear the MessagesList displayed
                MessagesList.Clear();

                // Get 'new' Messages list (from cache first then load some of them if necessary)
                List<Rainbow.Model.Message> rbMessagesList = RbInstantMessaging.GetAllMessagesFromConversationIdFromCache(CurrentConversationId);
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

        public void LoadFakeMessages()
        {
            double nbSecs;
            int mns;
            int sec;

            DateTime utcNow = DateTime.UtcNow;
            MessageViewModel message;

            #region FAKE CONVERSATIONS - TEST 'OTHER USER' DISPLAY

            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , short body
            message = new MessageViewModel();
            message.Id = "1";

            message.PeerId = "peerId";
            message.PeerJid = "peerJid";
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "message body";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.White;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Collapsed;

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = false;

            MessagesList.Add(message);


            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , short body, edited
            message = new MessageViewModel();
            message.Id = "2";

            message.PeerId = "peerId";
            message.PeerJid = "peerJid";
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "message body";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.White;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Visible; // EDITED

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = false;

            MessagesList.Add(message);


            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , short body, reply part
            message = new MessageViewModel();
            message.Id = "3";

            message.PeerId = "peerId";
            message.PeerJid = "peerJid";
            message.PeerDisplayName = "Frederic Reard";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "message body";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.White;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Visible; // EDITED

            message.ReplyPartIsVisible = Visibility.Visible;
            message.ReplyId = "1";
            message.ReplyPeerDisplayName = "Christophe Irles";
            message.ReplyBackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetDarkerColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;
            message.ReplyPeerId = CurrentApplication.CurrentUserId;
            message.ReplyPeerJid = CurrentApplication.CurrentUserJid;
            message.ReplyBody = "reply message body";

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = false;

            MessagesList.Add(message);


            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , long body, long reply part
            message = new MessageViewModel();
            message.Id = "5";

            message.PeerId = "peerId";
            message.PeerJid = "peerJid";
            message.PeerDisplayName = "Frederic Reard";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "Lorem ipsum dolor sit amet, libero amet, aliquam sed quis nam a, interdum massa est, pede aenean. Luctus felis. Nam accumsan consectetuer dui enim nostra et, amet odio consequatur, quis lacinia ipsum vel diam. Pellentesque rem fermentum felis, orci risus vel augue amet odio quam, sit eu quis donec scelerisque urna, urna dictum. Felis cursus lorem eget sagittis sed ante, ultrices varius non pulvinar elit in justo, lectus consectetuer tempus ornare tempor libero, per suspendisse mauris vel nec. Per lorem ante etiam aliquet cursus nec, ac lobortis. Eget metus. Mus urna in habitant ut tincidunt libero, et sit, urna interdum eget purus nec odio a, id sodales in. Tincidunt sit amet mattis aliquam sed risus, orci mollis lacus ullamcorper vel.";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.White;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Visible; // EDITED

            message.ReplyPartIsVisible = Visibility.Visible;
            message.ReplyId = "1";
            message.ReplyPeerDisplayName = "Christophe Irles";
            message.ReplyBackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetDarkerColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;
            message.ReplyPeerId = CurrentApplication.CurrentUserId;
            message.ReplyPeerJid = CurrentApplication.CurrentUserJid;
            message.ReplyBody = "Lorem ipsum dolor sit amet, libero amet, aliquam sed quis nam a, interdum massa est, pede aenean. Luctus felis. Nam accumsan consectetuer dui enim nostra et, amet odio consequatur, quis lacinia ipsum vel diam. Pellentesque rem fermentum felis, orci risus vel augue amet odio quam, sit eu quis donec scelerisque urna, urna dictum. Felis cursus lorem eget sagittis sed ante, ultrices varius non pulvinar elit in justo, lectus consectetuer tempus ornare tempor libero, per suspendisse mauris vel nec. Per lorem ante etiam aliquet cursus nec, ac lobortis. Eget metus. Mus urna in habitant ut tincidunt libero, et sit, urna interdum eget purus nec odio a, id sodales in. Tincidunt sit amet mattis aliquam sed risus, orci mollis lacus ullamcorper vel.";

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = false;

            MessagesList.Add(message);

            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , short body, File Attachment + File Info
            message = new MessageViewModel();
            message.Id = "4";

            message.PeerId = "peerId";
            message.PeerJid = "peerJid";
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "message body";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.White;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Visible; // EDITED

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Visible;
            message.FileAttachmentImageSource = Helper.GetBitmapImageFromResource("icon_unknown_blue.png");
            message.FileAttachmentImageHeight = 60;
            message.FileAttachmentImageWidth = 60;

            message.FileInfoIsVisible = Visibility.Visible;
            message.FileId = "1";
            message.FileName = "My word document.ppt";
            message.FileSize = Helper.HumanizeFileSize(10248);

            message.IsEventMessage = false;

            MessagesList.Add(message);

            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , short body, File Attachment without File Info
            message = new MessageViewModel();
            message.Id = "5";

            message.PeerId = "peerId";
            message.PeerJid = "peerJid";
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "message body";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.White;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Visible; // EDITED

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Visible;
            message.FileAttachmentImageSource = Helper.GetBitmapImageFromResource("icon_unknown_blue.png");
            message.FileAttachmentImageHeight = 80;
            message.FileAttachmentImageWidth = 80;

            message.FileInfoIsVisible = Visibility.Collapsed;
            message.FileId = "1";

            message.IsEventMessage = false;

            MessagesList.Add(message);


            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , simulate delete message
            message = new MessageViewModel();
            message.Id = "1";

            message.PeerId = "peerId";
            message.PeerJid = "peerJid";
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "This message was deleted";
            message.BodyFontStyle = FontStyles.Italic; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = new BrushConverter().ConvertFromString(AvatarPool.GetDarkerColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Collapsed;

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = false;

            MessagesList.Add(message);

            #endregion FAKE CONVERSATIONS - TEST 'OTHER USER' DISPLAY

            #region FAKE CONVERSATIONS - TEST 'CURRENT USER' DISPLAY

            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , short body
            message = new MessageViewModel();
            message.Id = "1";

            message.PeerId = CurrentApplication.CurrentUserId;
            message.PeerJid = CurrentApplication.CurrentUserJid;
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = Brushes.LightGray;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "message body";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.Black;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Collapsed;

            message.ReceiptImageSource = Helper.GetBitmapImageFromResource("msg_sending.png");

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = false;

            MessagesList.Add(message);


            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , short body, edited
            message = new MessageViewModel();
            message.Id = "2";

            message.PeerId = CurrentApplication.CurrentUserId;
            message.PeerJid = CurrentApplication.CurrentUserJid;
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = Brushes.LightGray;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "message body";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.Black;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Visible; // EDITED

            message.ReceiptImageSource = Helper.GetBitmapImageFromResource("msg_not_read.png");

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = false;

            MessagesList.Add(message);


            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , short body, reply part
            message = new MessageViewModel();
            message.Id = "3";

            message.PeerId = CurrentApplication.CurrentUserId;
            message.PeerJid = CurrentApplication.CurrentUserJid;
            message.PeerDisplayName = "Frederic Reard";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = Brushes.LightGray;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "message body";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.Black;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Visible; // EDITED

            message.ReceiptImageSource = Helper.GetBitmapImageFromResource("msg_read.png");

            message.ReplyPartIsVisible = Visibility.Visible;
            message.ReplyId = "1";
            message.ReplyPeerDisplayName = "Christophe Irles";
            message.ReplyBackgroundColor = Brushes.Gray;
            message.ReplyPeerId = CurrentApplication.CurrentUserId;
            message.ReplyPeerJid = CurrentApplication.CurrentUserJid;
            message.ReplyBody = "reply message body";

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = false;

            MessagesList.Add(message);


            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , long body, long reply part
            message = new MessageViewModel();
            message.Id = "3";

            message.PeerId = CurrentApplication.CurrentUserId;
            message.PeerJid = CurrentApplication.CurrentUserJid;
            message.PeerDisplayName = "Frederic Reard";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = Brushes.LightGray;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "Lorem ipsum dolor sit amet, libero amet, aliquam sed quis nam a, interdum massa est, pede aenean. Luctus felis. Nam accumsan consectetuer dui enim nostra et, amet odio consequatur, quis lacinia ipsum vel diam. Pellentesque rem fermentum felis, orci risus vel augue amet odio quam, sit eu quis donec scelerisque urna, urna dictum. Felis cursus lorem eget sagittis sed ante, ultrices varius non pulvinar elit in justo, lectus consectetuer tempus ornare tempor libero, per suspendisse mauris vel nec. Per lorem ante etiam aliquet cursus nec, ac lobortis. Eget metus. Mus urna in habitant ut tincidunt libero, et sit, urna interdum eget purus nec odio a, id sodales in. Tincidunt sit amet mattis aliquam sed risus, orci mollis lacus ullamcorper vel.";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.Black;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Visible; // EDITED

            message.ReceiptImageSource = Helper.GetBitmapImageFromResource("msg_read.png");

            message.ReplyPartIsVisible = Visibility.Visible;
            message.ReplyId = "1";
            message.ReplyPeerDisplayName = "Christophe Irles";
            message.ReplyBackgroundColor = Brushes.Gray;
            message.ReplyPeerId = CurrentApplication.CurrentUserId;
            message.ReplyPeerJid = CurrentApplication.CurrentUserJid;
            message.ReplyBody = "Lorem ipsum dolor sit amet, libero amet, aliquam sed quis nam a, interdum massa est, pede aenean. Luctus felis. Nam accumsan consectetuer dui enim nostra et, amet odio consequatur, quis lacinia ipsum vel diam. Pellentesque rem fermentum felis, orci risus vel augue amet odio quam, sit eu quis donec scelerisque urna, urna dictum. Felis cursus lorem eget sagittis sed ante, ultrices varius non pulvinar elit in justo, lectus consectetuer tempus ornare tempor libero, per suspendisse mauris vel nec. Per lorem ante etiam aliquet cursus nec, ac lobortis. Eget metus. Mus urna in habitant ut tincidunt libero, et sit, urna interdum eget purus nec odio a, id sodales in. Tincidunt sit amet mattis aliquam sed risus, orci mollis lacus ullamcorper vel.";

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = false;

            MessagesList.Add(message);

            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , short body, File Attachment + File Info
            message = new MessageViewModel();
            message.Id = "4";

            message.PeerId = CurrentApplication.CurrentUserId;
            message.PeerJid = CurrentApplication.CurrentUserJid;
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = Brushes.LightGray;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "message body";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.Black;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Visible; // EDITED

            message.ReceiptImageSource = Helper.GetBitmapImageFromResource("msg_check.png");

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Visible;
            message.FileAttachmentImageSource = Helper.GetBitmapImageFromResource("icon_unknown_blue.png");
            message.FileAttachmentImageHeight = 60;
            message.FileAttachmentImageWidth = 60;

            message.FileInfoIsVisible = Visibility.Visible;
            message.FileId = "1";
            message.FileName = "My word document.ppt";
            message.FileSize = Helper.HumanizeFileSize(10248);

            message.IsEventMessage = false;

            MessagesList.Add(message);

            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , short body, File Attachment without File Info
            message = new MessageViewModel();
            message.Id = "5";

            message.PeerId = CurrentApplication.CurrentUserId;
            message.PeerJid = CurrentApplication.CurrentUserJid;
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = Brushes.LightGray;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "message body";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.Black;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Visible; // EDITED

            message.ReceiptImageSource = Helper.GetBitmapImageFromResource("msg_sending.png");

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Visible;
            message.FileAttachmentImageSource = Helper.GetBitmapImageFromResource("icon_unknown_blue.png");
            message.FileAttachmentImageHeight = 80;
            message.FileAttachmentImageWidth = 80;

            message.FileInfoIsVisible = Visibility.Collapsed;
            message.FileId = "1";

            message.IsEventMessage = false;

            MessagesList.Add(message);


            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , simulate delete message
            message = new MessageViewModel();
            message.Id = "1";

            message.PeerId = CurrentApplication.CurrentUserId;
            message.PeerJid = CurrentApplication.CurrentUserJid;
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = Brushes.LightGray;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "This message was deleted";
            message.BodyFontStyle = FontStyles.Italic; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.Gray;

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Collapsed;

            message.ReceiptImageSource = Helper.GetBitmapImageFromResource("msg_read.png");

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = false;

            MessagesList.Add(message);

            #endregion FAKE CONVERSATIONS - TEST 'CURRENT USER' DISPLAY

            #region FAKE CONVERSATIONS - TEST 'EVENT' DISPLAY

            // --------------------------------------------------------------------
            // TEST BASIC MSG: event - start conference
            message = new MessageViewModel();
            message.Id = "1";

            message.PeerId = "peerId";
            message.PeerJid = "peerJid";
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Collapsed;

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Collapsed;

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = true;
            message.EventMessageBodyPart1 = message.PeerDisplayName + " has started the conference";
            message.EventMessageBodyPart2IsVisible = String.IsNullOrEmpty(message.EventMessageBodyPart2) ? Visibility.Collapsed : Visibility.Visible;

            MessagesList.Add(message);

            // --------------------------------------------------------------------
            // TEST BASIC MSG: event - end conference
            message = new MessageViewModel();
            message.Id = "1";

            message.PeerId = "peerId";
            message.PeerJid = "peerJid";
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Collapsed;

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Collapsed;

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = true;
            message.EventMessageBodyPart1 = message.PeerDisplayName + " has ended the conference";
            message.EventMessageBodyPart2IsVisible = String.IsNullOrEmpty(message.EventMessageBodyPart2) ? Visibility.Collapsed : Visibility.Visible;

            MessagesList.Add(message);


            // --------------------------------------------------------------------
            // TEST BASIC MSG: event -'call log' - answered
            message = new MessageViewModel();
            message.Id = "1";

            message.PeerId = "peerId";
            message.PeerJid = "peerJid";
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Collapsed;

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Collapsed;

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = true;
            message.EventMessageBodyPart1 = message.PeerDisplayName + " has called you.";
            nbSecs = 658;
            mns = (int)(nbSecs / 60);
            sec = (int)Math.Round(nbSecs - (mns * 60));
            message.EventMessageBodyPart2 = "Duration: " + ((mns > 0) ? mns + ((mns > 1) ? "mns " : "mn ") : "") + ((sec > 0) ? sec + "s" : "");
            message.EventMessageBodyPart2Color = Brushes.Gray;
            message.EventMessageBodyPart2IsVisible = String.IsNullOrEmpty(message.EventMessageBodyPart2) ? Visibility.Collapsed : Visibility.Visible;

            MessagesList.Add(message);

            // --------------------------------------------------------------------
            // TEST BASIC MSG: event -'call log' - not answered / failed
            message = new MessageViewModel();
            message.Id = "1";

            message.PeerId = "peerId";
            message.PeerJid = "peerJid";
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Collapsed;

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Collapsed;

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = true;
            message.EventMessageBodyPart1 = message.PeerDisplayName + " has called you.";
            message.EventMessageBodyPart2 = "Missed call";
            message.EventMessageBodyPart2Color = Brushes.Red;
            message.EventMessageBodyPart2IsVisible = String.IsNullOrEmpty(message.EventMessageBodyPart2) ? Visibility.Collapsed : Visibility.Visible;

            MessagesList.Add(message);

            #endregion FAKE CONVERSATIONS - TEST 'EVENT' DISPLAY
        }
    }
}


