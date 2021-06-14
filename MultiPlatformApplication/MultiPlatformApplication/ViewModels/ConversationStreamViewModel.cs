using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;

using Rainbow;
using Rainbow.Common;
using Rainbow.Model;

using NLog;


namespace MultiPlatformApplication.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ConversationStreamViewModel : MessageModel
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(ConversationStreamViewModel));


        private ConversationModel conversationModel;

#region BINDINGS used in XAML

        public NavigationModel Navigation { get; private set; } = new NavigationModel();

        public ConversationModel Conversation {
                get { return conversationModel; }
                set { SetProperty(ref conversationModel, value); }
            }

        public DynamicListModel<MessageModel> DynamicList { get; private set; } = new DynamicListModel<MessageModel>();
#endregion BINDINGS used in XAML

        private static readonly int NB_MESSAGE_LOADED_BY_ROW = 20;

        private String conversationId;
        private Boolean firstInitialization = true;
        private String currentContactJid;
        private Conversation rbConversation = null; // Rainbow Conversation object

        private App XamarinApplication;

        private Object lockObservableMessagesList = new Object(); // To lock access to the observable collection: 'MessagesList'

        private Object lockContactsListInvolved = new Object(); // To lock access to: 'contactsListInvolved'
        private List<String> contactsListInvolved = new List<String>(); // To store list of contacts involved in this Conversation (by Jid)

        private Object lockRepliedContactsListInvolved = new Object(); // To lock access to: 'repliedContactsListInvolved'
        private List<String> repliedContactsListInvolved = new List<String>(); // To store list of contacts involved in reply context in this Conversation (by Jid)

        private Object lockUnknownRepliedMessagesListInvolved = new Object(); // To lock access to: 'repliedMessagesListInvolved'
        private Dictionary<String, String> unknownRepliedMessagesListInvolved = new Dictionary<String, String>(); // To store list of messages involved in reply context in this Conversation (by message Id / message Stamp) but unknown for the moment
        private BackgroundWorker backgroundWorkerUnknownrepliedMessage = null;

        // Define attributes to manage loading/scrolling of messages list view
        private Boolean noMoreOlderMessagesAvailable = false;
        private Boolean loadingOlderMessages = false;
        private Boolean waitingScrollingDueToLoadingOlderMessages = false;
        private int nbOlderMessagesFound = 0;
        private Boolean newMessageAdded = false;

#region PUBLIC METHODS

        /// <summary>
        /// Constructor
        /// </summary>
        public ConversationStreamViewModel()
        {
            Navigation.BackCommand = new RelayCommand<object>(new Action<object>(BackCommand));

            // Define commands
            DynamicList.AskMoreItemsCommand = new RelayCommand<object>(new Action<object>(AskMoreMessagesCommand));
        }

        ~ConversationStreamViewModel()
        {

        }

        private void InitializeSdkObjectsAndEvents()
        {
            // Get Xamarin Application
            XamarinApplication = (App)Xamarin.Forms.Application.Current;

            // Manage event(s) from FilePool
            XamarinApplication.SdkWrapper.FileDescriptorAvailable += FilePool_FileDescriptorAvailable;
            XamarinApplication.SdkWrapper.ThumbnailAvailable += FilePool_ThumbnailAvailable;

            // Manage event(s) from AvatarPool
            XamarinApplication.SdkWrapper.ContactAvatarUpdated += RbAvatars_ContactAvatarUpdated;
            XamarinApplication.SdkWrapper.BubbleAvatarUpdated += RbAvatars_BubbleAvatarUpdated;

            // Manage event(s) from InstantMessaging
            XamarinApplication.SdkWrapper.MessageReceived += RbInstantMessaging_MessageReceived;
            XamarinApplication.SdkWrapper.ReceiptReceived += RbInstantMessaging_ReceiptReceived;
            XamarinApplication.SdkWrapper.MessagesAllRead += RbInstantMessaging_MessagesAllRead;
            XamarinApplication.SdkWrapper.UserTypingChanged += RbInstantMessaging_UserTypingChanged;

            // Manage event(s) from Contacts
            XamarinApplication.SdkWrapper.ContactAdded += RbContacts_ContactAdded;
            XamarinApplication.SdkWrapper.ContactInfoChanged += RbContacts_ContactInfoChanged;

            currentContactJid = XamarinApplication.SdkWrapper.GetCurrentContactJid();

            if (!String.IsNullOrEmpty(conversationId))
            {
                // Get Rainbow Conversation object
                rbConversation = XamarinApplication.SdkWrapper.GetConversationByIdFromCache(conversationId);

                // Get Conversation Model Object using Rainbow Conversation
                Conversation = Helper.GetConversationFromRBConversation(rbConversation);
                if (Conversation != null)
                    Conversation.AvatarFilePath = Helper.GetConversationAvatarFilePath(Conversation);
            }
        }

        public void Initialize(String conversationId)
        {
            if (firstInitialization)
            {
                firstInitialization = false;
                this.conversationId = conversationId;

                InitializeSdkObjectsAndEvents();

                // Now ask to load items
                DynamicList.AskingMoreItems = true;
                DynamicList.ListView.BeginRefresh();
            }
            else
            {
                // We want to scroll to last element
                DynamicList.ScrollToEnd();
            }

            // Mark all msg as read
            MarkAllAsRead();

            // Reset selection (if any)
            DynamicList.ListView.SelectedItem = null;
        }

        public void Uninitialize()
        {
            UnnitializeSdkObjectsAndEvents();

            DynamicList.AskMoreItemsCommand = null;

            DynamicList.Dispose();
            DynamicList = null;
        }

         private void UnnitializeSdkObjectsAndEvents()
        {
            // Unmanage event(s) from FilePool
            XamarinApplication.SdkWrapper.FileDescriptorAvailable -= FilePool_FileDescriptorAvailable;
            XamarinApplication.SdkWrapper.ThumbnailAvailable -= FilePool_ThumbnailAvailable;

            // Unmanage event(s) from AvatarPool
            XamarinApplication.SdkWrapper.ContactAvatarUpdated -= RbAvatars_ContactAvatarUpdated;
            XamarinApplication.SdkWrapper.BubbleAvatarUpdated -= RbAvatars_BubbleAvatarUpdated;

            // Unmanage event(s) from InstantMessaging
            XamarinApplication.SdkWrapper.MessageReceived -= RbInstantMessaging_MessageReceived;
            XamarinApplication.SdkWrapper.ReceiptReceived -= RbInstantMessaging_ReceiptReceived;
            XamarinApplication.SdkWrapper.MessagesAllRead -= RbInstantMessaging_MessagesAllRead;
            XamarinApplication.SdkWrapper.UserTypingChanged -= RbInstantMessaging_UserTypingChanged;
        }

#region EVENTS FROM CollectionView

#region  COMMANDS

        private async void BackCommand(object obj)
        {
            await XamarinApplication.NavigationService.GoBack();
        }

        // Command used when asking to load more messages
        private void AskMoreMessagesCommand(object obj)
        {
            // Do we have already some messages ?
            if (DynamicList.Items.Count > 0)
            {
                LoadMoreMessages();
            }
            else
            {
                // Get messages from cache
                List<Rainbow.Model.Message> rbMessagesList = XamarinApplication.SdkWrapper.GetAllMessagesFromConversationIdFromCache(Conversation.Id);
                if (rbMessagesList?.Count > 0)
                {
                    AddToModelRbMessages(rbMessagesList);

                    if (rbMessagesList.Count < NB_MESSAGE_LOADED_BY_ROW)
                    {
                        // No more items will be added in this dynamic list
                        DynamicList.NoMoreItemsAvailable = true;
                    }
                }
                else
                    LoadMoreMessages();
            }


            //// Get Messages
            //List<Rainbow.Model.Message> rbMessagesList = XamarinApplication.SdkWrapper.GetAllMessagesFromConversationIdFromCache(Conversation.Id);
            //if (rbMessagesList != null)
            //{
            //    if (rbMessagesList.Count > 0)
            //    {
            //        AddToModelRbMessages(rbMessagesList);

            //        // No more items will be added in this dynamic list
            //        DynamicList.NoMoreItemsAvailable = true;

            //        // Now scroll to the index we want
            //        //DynamicList.ScrollCollectionViewToIndex(DynamicList.ScrolltoIndex);
            //    }

            //    //if (rbDynamicList.Items.Count < NB_MESSAGE_LOADED_BY_ROW)
            //    //    LoadMoreMessages();
            //}
            ////else
            ////    LoadMoreMessages();
        }

        private void MessageSelectionChanged(object obj)
        {
            // Necessary ?
        }

#endregion  COMMANDS

 
#endregion EVENTS FROM CollectionView

        public void MarkAllAsRead()
        {
            try
            {
                XamarinApplication.SdkWrapper.MarkAllMessagesAsRead(Conversation?.Id);
            }
            catch
            {
            }
        }

        public void SetIsTyping(bool isTyping)
        {
            XamarinApplication.SdkWrapper.SendIsTypingInConversationById(Conversation.Id, isTyping);
        }

        public void SendMessage(String content)
        {
            XamarinApplication.SdkWrapper.SendMessageToConversationId(Conversation.Id, content, UrgencyType.Std);
            SetIsTyping(false);
        }

        public void ScrollingDueToLoadingOlderMessagesDone()
        {
            waitingScrollingDueToLoadingOlderMessages = false;
        }

        public Boolean WaitingScrollingDueToLoadingOlderMessages()
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
            Task task = new Task(() =>
            {
                XamarinApplication.SdkWrapper.GetMessagesFromConversationId(Conversation.Id, NB_MESSAGE_LOADED_BY_ROW, callback =>
                {
                    if (callback.Result.Success)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            if (callback.Data.Count == 0 )
                            {
                                // No more items will be added in this dynamic list
                                DynamicList.NoMoreItemsAvailable = true;
                            }

                            AddToModelRbMessages(callback.Data);
                        });
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
            if (rbMessagesList == null)
                return;
            
            if (rbMessagesList.Count == 0)
                return;

            DynamicList.AskingMoreItems = true;

            List<MessageModel> messagesList = new List<MessageModel>();
            foreach (Rainbow.Model.Message rbMessage in rbMessagesList)
            {
                MessageModel msg = GetMessageFromRBMessage(rbMessage, rbConversation.Type);
                if (msg != null)
                {
                    messagesList.Insert(0, msg);
                    msg.AvatarFilePath = Helper.GetContactAvatarFilePath(msg.PeerId);

                    AddContactInvolved(msg.PeerJid);
                }
            }

            lock (lockObservableMessagesList)
            {
                if (DynamicList.Items.Count == 0)
                {
                    // Replace range and scroll to the end
                    DynamicList.ReplaceRangeAndScroll(messagesList, ScrollTo.END);
                }
                else
                {
                    // Add range and scroll to the end of elements added
                    DynamicList.AddRangeAndScroll(messagesList, 0, ScrollTo.END_OF_RANGE_ADDED);
                }

            }
        }

        private void AddContactInvolved(String peerJid)
        {
            lock (lockContactsListInvolved)
            {
                if((!String.IsNullOrEmpty(peerJid)) && (!contactsListInvolved.Contains(peerJid)))
                {
                    log.Debug("[AddContactInvolved] - ContactJid:[{0}]", peerJid);
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
                    if (index >= unknownRepliedMessagesListInvolved.Count)
                        index = 0;
                    try
                    {
                        String msgId = unknownRepliedMessagesListInvolved.ElementAt(index).Key;
                        String msgStamp = unknownRepliedMessagesListInvolved.ElementAt(index).Value;

                        if (AskMessageInfo(msgId, msgStamp))
                            unknownRepliedMessagesListInvolved.Remove(msgId);
                        else
                            index++;
                    }
                    catch
                    {
                        index++;
                    }
                }
                canContinue = (unknownRepliedMessagesListInvolved.Count > 0);
            }
            while (canContinue);
        }

        private bool AskMessageInfo(String msgId, String msgStamp)
        {
            Boolean result = false;
            ManualResetEvent manualEvent = new ManualResetEvent(false);

            XamarinApplication.SdkWrapper.GetOneMessageFromConversationId(Conversation.Id, msgId, msgStamp, callback =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (callback.Result.Success)
                    {

                        result = true;
                        Rainbow.Model.Message rbMessage = callback.Data;
                        if (rbMessage != null)
                        {
                            List<MessageModel> messagesList = GetMessagesByReplyId(msgId);
                            foreach (MessageModel message in messagesList)
                            {
                                SetReplyPartOfMessage(message, rbMessage);
                            }
                        }
                    }
                    manualEvent.Set();
                });
            });

            manualEvent.WaitOne();
            manualEvent.Dispose();

            return result;
        }

        private List<MessageModel> GetMessagesByPeerJid(String peerJid)
        {
            List<MessageModel> result = new List<MessageModel>();
            lock (lockObservableMessagesList)
            {
                foreach(MessageModel msg  in DynamicList.Items)
                {
                    if (msg.PeerJid == peerJid)
                        result.Add(msg);
                }
            }
            return result;
        }

        private List<MessageModel> GetMessagesByReplyPeerJid(String peerJid)
        {
            List<MessageModel> result = new List<MessageModel>();
            lock (lockObservableMessagesList)
            {
                foreach (MessageModel msg in DynamicList.Items)
                {
                    if (msg.ReplyPeerJid == peerJid)
                        result.Add(msg);
                }
            }
            return result;
        }

        private List<MessageModel> GetMessagesByReplyId(String replyId)
        {
            List<MessageModel> result = new List<MessageModel>();
            lock (lockObservableMessagesList)
            {
                foreach (MessageModel msg in DynamicList.Items)
                {
                    if (msg.ReplyId == replyId)
                        result.Add(msg);
                }
            }
            return result;
        }

        private MessageModel GetMessageByMessageId(String messageId)
        {
            MessageModel result = null;
            lock (lockObservableMessagesList)
            {
                foreach (MessageModel msg in DynamicList.Items)
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

        private MessageModel GetMessageByFileDescriptorId(String fileId)
        {
            MessageModel result = null;
            lock (lockObservableMessagesList)
            {
                foreach (MessageModel msg in DynamicList.Items)
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

        private void UpdateMessagesForJid(String peerJid, bool updateAvatar, bool updateDisplayName )
        {
            if (contactsListInvolved.Contains(peerJid))
            {
                Contact contact = XamarinApplication.SdkWrapper.GetContactFromContactJid(peerJid);
                if (contact != null)
                {
                    log.Debug("[UpdateMessagesForJid] peerJid:[{0}] - peerId:[{1}] - updateAvatar[{2}] - updateDisplayName:[{3}]", peerJid, contact.Id, updateAvatar, updateDisplayName);
                    
                    String displayName;
                    if (updateDisplayName)
                        displayName = Rainbow.Util.GetContactDisplayName(contact);
                    else
                        displayName = null;

                    List<MessageModel> messages = GetMessagesByPeerJid(peerJid);
                    lock (lockObservableMessagesList)
                    {
                        foreach (MessageModel message in messages)
                        {
                            if (updateAvatar)
                                message.AvatarFilePath = Helper.GetContactAvatarFilePath(contact.Id);

                            if (displayName != null)
                            {
                                message.PeerDisplayName = displayName;
                                message.BackgroundColor = Color.FromHex(SdkWrapper.GetColorFromDisplayName(message.PeerDisplayName));
                                message.ReplyBackgroundColor = Color.FromHex(SdkWrapper.GetDarkerColorFromDisplayName(message.PeerDisplayName));
                            }
                        }
                    }
                }
                else
                    log.Warn("[UpdateMessagesForJid] peerJid:[{0}] found but related contact not found", peerJid);
            }
        }

        private void UpdateRepliedMessagesForJid(String peerJid)
        {
            if(repliedContactsListInvolved.Contains(peerJid))
            {
                Contact contactReply = XamarinApplication.SdkWrapper.GetContactFromContactJid(peerJid);
                if (contactReply != null)
                {
                    String displayName = Rainbow.Util.GetContactDisplayName(contactReply);

                    List<MessageModel> messages = GetMessagesByReplyPeerJid(peerJid);
                    lock (lockObservableMessagesList)
                    {
                        foreach (MessageModel message in messages)
                        {
                            log.Debug("[UpdateRepliedMessagesForJid] peerJid:[{0}] - message.Id:[{1}] - displayName:[{2}]", peerJid, message.Id, displayName);

                            message.ReplyPartIsVisible = "True";

                            message.ReplyPeerId = contactReply.Id;
                            message.ReplyPeerDisplayName = displayName;
                        }
                    }
                }
                else
                    log.Warn("[UpdateRepliedMessagesForJid] peerJid:[{0}] found but related contact not found", peerJid);
            }
        }

        private MessageModel GetMessageFromRBMessage(Rainbow.Model.Message rbMessage, String conversationType)
        {
            MessageModel message = null;
            if (rbMessage != null)
            {
                App XamarinApplication = (App)Xamarin.Forms.Application.Current;

                //log.Debug("[GetMessageFromRBMessage] Message.Id:[{0}] - Message.ReplaceId:[{1}] - DateTime:[{2}] - Content:[{3}]", rbMessage.Id, rbMessage.ReplaceId, rbMessage.Date.ToString("o"), rbMessage.Content);

                message = new MessageModel();
                message.EventMessageBodyPart2Color = Color.Black; // Set default value

                Rainbow.Model.Contact contact = XamarinApplication.SdkWrapper.GetContactFromContactJid(rbMessage.FromJid);
                if (contact != null)
                {
                    message.PeerId = contact.Id;

                    message.PeerDisplayName = Rainbow.Util.GetContactDisplayName(contact);
                    message.BackgroundColor = Color.FromHex(SdkWrapper.GetColorFromDisplayName(message.PeerDisplayName));
                }
                else
                {
                    message.BackgroundColor = Color.FromHex(SdkWrapper.GetColorFromDisplayName(""));

                    log.Debug("[GetMessageFromRBMessage] Unknown Contact - Jid :[{0}]", rbMessage.FromJid);

                    // We ask to have more info about this contact usin AvatarPool
                    XamarinApplication.SdkWrapper.AddUnknownContactToPoolByJid(rbMessage.FromJid);
                }

                // We have the display name only in Room / Bubble context
                message.PeerDisplayNameIsVisible = (conversationType == Rainbow.Model.Conversation.ConversationType.Room) ? "True" : "False";

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
                            message.BodyColor = (message.PeerJid == currentContactJid) ? Color.Gray : Color.FromHex(SdkWrapper.GetDarkerColorFromDisplayName(message.PeerDisplayName));
                        }
                    }

                    // Edited text visible ?
                    message.EditedIsVisible = String.IsNullOrEmpty(rbMessage.ReplaceId) ? "False" : "True";

                    // Reply part
                    // By default is not displayed
                    message.ReplyPartIsVisible = "False";
                    if (rbMessage.ReplyMessage != null)
                    {
                        // Store Id of this reply message
                        message.ReplyId = rbMessage.ReplyMessage.Id;

                        // Set background color
                        message.ReplyBackgroundColor = Color.FromHex(SdkWrapper.GetDarkerColorFromDisplayName(message.PeerDisplayName));

                        // We need to get Name and text of the replied message ...
                        Rainbow.Model.Message rbRepliedMessage = XamarinApplication.SdkWrapper.GetOneMessageFromConversationIdFromCache(Conversation.Id, rbMessage.ReplyMessage.Id);
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
                        // Set Global info
                        message.FileAttachmentIsVisible = "True";
                        message.FileDefaultInfoIsVisible = "True";
                        message.FileId = rbMessage.FileAttachment.Id;
                        message.FileName = rbMessage.FileAttachment.Name;
                        message.FileSize = Helper.HumanizeFileSize(rbMessage.FileAttachment.Size);

                        if (XamarinApplication.SdkWrapper.IsThumbnailFileAvailable(Conversation.Id, rbMessage.FileAttachment.Id, rbMessage.FileAttachment.Name))
                        {
                            SetFileAttachmentSourceOfMessage(message, rbMessage.FileAttachment.Id);
                        }
                        else
                        {
                            // Ask more info about this file
                            XamarinApplication.SdkWrapper.AskFileDescriptorDownload(Conversation.Id, rbMessage.FileAttachment.Id);

                            // Set default icon
                            String defaultIconPath = Helpers.Helper.GetFileAttachmentSourceFromFileName(message.FileName);
                            if (!String.IsNullOrEmpty(defaultIconPath))
                                message.FileAttachmentImageSource = ImageSource.FromResource(defaultIconPath, typeof(Helpers.Helper).Assembly);
                            else
                                message.FileAttachmentImageSource = null;


                            message.FileAttachmentSourceWidth = 50;
                            message.FileAttachmentSourceHeight = 50;
                        }
                    }
                    else
                    {
                        message.FileAttachmentIsVisible = "False";
                        message.FileDefaultInfoIsVisible = "False";

                        message.FileAttachmentSourceWidth = 0;
                        message.FileAttachmentSourceHeight = 0;
                    }

                    message.Body = content;
                    message.BodyIsVisible = String.IsNullOrEmpty(message.Body) ? "False" : "True";
                }

                // We store info about this contact in message context
                AddContactInvolved(message.PeerJid);
            }
            return message;
        }

        public void SetFileAttachmentSourceOfMessage(MessageModel message, String fileId)
        {
            string filePath = XamarinApplication.SdkWrapper.GetThumbnailFullFilePath(fileId);
            try
            {
                log.Debug("[SetFileAttachmentSourceOfMessage] FileId:[{0}] - Use filePath:[{1}]", fileId, filePath);
                System.Drawing.Size size = ImageTools.GetImageSize(filePath);
                if ((size.Width > 0) && (size.Height > 0))
                {
                    double density = Helper.GetDensity();

                    // Avoid to have a thumbnail too big
                    float scaleWidth = (float)(XamarinApplication.SdkWrapper.MaxThumbnailWidth * density) / (float)size.Width;
                    float scaleHeight = (float)(XamarinApplication.SdkWrapper.MaxThumbnailHeight * density) / (float)size.Height;
                    float scale = Math.Min(scaleHeight, scaleWidth);

                    // Don't increase size of the thumbnail 
                    if (scale > 1)
                        scale = 1;

                    // Calculate size of the thumbnail
                    int w = (int)(size.Width * scale);
                    int h = (int)(size.Height * scale);

                    message.FileAttachmentSourceWidth = (int)Math.Round(w / density);
                    message.FileAttachmentSourceHeight = (int)Math.Round(h / density);

                    message.FileAttachmentImageSource = ImageSource.FromFile(filePath);

                    message.FileDefaultInfoIsVisible = "False";
                }
            }
            catch { }
        }

        public void SetReceiptPartOfMessage(MessageModel message, ReceiptType receiptType)
        {
            String receipt = receiptType.ToString();
            if (message.ReceiptType == receipt)
                return;

            message.ReceiptType = receipt;
            message.ReceiptSource = Helper.GetReceiptSourceFromReceiptType(receiptType);
        }

        private void SetReplyPartOfMessage(MessageModel message, Rainbow.Model.Message rbRepliedMessage)
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


            log.Debug("[SetReplyPartOfMessage] - message.Id:[{0}] - replyMsgId:[{1}] - replyBody:[{2}] - ContactJid:[{3}]", message.Id, rbRepliedMessage.Id, message.ReplyBody, rbRepliedMessage.FromJid);

            Rainbow.Model.Contact contactReply = XamarinApplication.SdkWrapper.GetContactFromContactJid(rbRepliedMessage.FromJid);
            if (contactReply != null)
            {
                // Reply part is visible
                message.ReplyPartIsVisible = "True";

                message.ReplyPeerId = contactReply.Id;
                message.ReplyPeerDisplayName = Rainbow.Util.GetContactDisplayName(contactReply);
            }
            else
            {
                log.Debug("[SetReplyPartOfMessage] - message.Id:[{0}] - replyMsgId:[{1}] - Unknown Contact Jid[{2}]", message.Id, rbRepliedMessage.Id, rbRepliedMessage.FromJid);

                // We ask to have more info about this contact using AvatarPool
                XamarinApplication.SdkWrapper.AddUnknownContactToPoolByJid(rbRepliedMessage.FromJid);
            }

            // We store info about this contact in reply context
            AddRepliedContactInvolved(rbRepliedMessage.FromJid);
        }

        private void SetCallLogPartOfMessage(MessageModel message, CallLogAttachment callLogAttachment)
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

        private void SetEventPartFromCallLog(MessageModel message)
        {
            Rainbow.Model.Contact contact = XamarinApplication.SdkWrapper.GetContactFromContactJid(message.CallOtherJid);
            if (contact != null)
            {
                String displayName = Rainbow.Util.GetContactDisplayName(contact);
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
                log.Debug("[SetEventPartFromCallLog] - message.Id:[{0}] - Unknown Contact Jid:[{1}]", message.Id, message.CallOtherJid);

                // We ask to have more info about this contact using AvatarPool
                XamarinApplication.SdkWrapper.AddUnknownContactToPoolByJid(message.CallOtherJid);
            }
        }

#endregion PRIVATE METHOD


#region EVENTS FROM RAINBOW SDK

        private void RbInstantMessaging_UserTypingChanged(object sender, Rainbow.Events.UserTypingEventArgs e)
        {
            //TO DO
        }

        private void RbInstantMessaging_MessagesAllRead(object sender, Rainbow.Events.IdEventArgs e)
        {
            // Set to ClientRead all messages in the list
            if (e.Id == Conversation.Id)
            {
                Device.BeginInvokeOnMainThread( () =>
                {
                    log.Debug("[RbInstantMessaging_MessagesAllRead] conversationId:[{0}]", Conversation.Id);
                    lock (lockObservableMessagesList)
                    {
                        foreach (MessageModel message in DynamicList.Items)
                        {
                            if (message.PeerJid == currentContactJid)
                                SetReceiptPartOfMessage(message, Rainbow.Model.ReceiptType.ClientRead);
                        }
                    }
                });
            }
        }

        private void RbInstantMessaging_MessageReceived(object sender, Rainbow.Events.MessageEventArgs e)
        {
            if (e.ConversationId == Conversation.Id)
            {
                Device.BeginInvokeOnMainThread( () =>
                {
                    log.Debug("[RbInstantMessaging_MessageReceived] - FromJId:[{0}] - ToJid:[{1}] - CarbonCopy:[{2}] - Message.Id:[{3}] - Message.ReplaceId:[{4}]", e.Message.FromJid, e.Message.ToJid, e.CarbonCopy, e.Message.Id, e.Message.ReplaceId);

                    MessageModel newMsg = GetMessageFromRBMessage(e.Message, rbConversation.Type);
                    if (newMsg == null)
                    {
                        log.Warn("[RbInstantMessaging_MessageReceived] - Impossible to have Model.Message from XMPP Message - Message.Id:[{3}]", e.Message.Id);
                        return;
                    }

                    // Since message is not null, set the Avatar Source
                    newMsg.AvatarFilePath = Helper.GetContactAvatarFilePath(newMsg.PeerId);

                    // Manage incoming REPLACE message
                    if (!String.IsNullOrEmpty(e.Message.ReplaceId))
                    {
                        MessageModel previousMessage = GetMessageByMessageId(e.Message.Id);
                        if (previousMessage == null)
                        {
                            // We do nothing in this case ... Message not in the cache, so not visible ...
                            return;
                        }

                        previousMessage.Body = newMsg.Body;
                        previousMessage.BodyIsVisible = String.IsNullOrEmpty(previousMessage.Body) ? "False" : "True";

                        previousMessage.EditedIsVisible = "True";
                    }
                    // Manage incoming NEW message
                    else
                    {
                        // It's a new message to add in the list
                        newMessageAdded = true;

                        lock (lockObservableMessagesList)
                        {
                            // Do we have already some message ?
                            if (DynamicList.Items.Count == 0)
                            {
                                DynamicList.Items.Add(newMsg);
                            }
                            else
                            {
                                // Add to the list but need to check date
                                MessageModel storedMsg;
                                bool newMsgInserted = false;
                                int nb = DynamicList.Items.Count - 1;

                                for (int i = nb; i > 0; i--)
                                {
                                    storedMsg = DynamicList.Items[i];
                                    if (newMsg.MessageDateTime > storedMsg.MessageDateTime)
                                    {
                                        if (i == nb)
                                            DynamicList.Items.Add(newMsg);
                                        else
                                            DynamicList.Items.Insert(i, newMsg);
                                        newMsgInserted = true;
                                        break;
                                    }
                                }
                                // If we don't have already added the new message, we insert it to the first place
                                if (!newMsgInserted)
                                    DynamicList.Items.Insert(0, newMsg);
                            }
                        }
                    }

                    // Mark the message as read
                    if (XamarinApplication.CurrentConversationId == Conversation.Id)
                        XamarinApplication.SdkWrapper.MarkMessageAsRead(Conversation.Id, newMsg.Id);
                });
            }
        }

        private void RbInstantMessaging_ReceiptReceived(object sender, Rainbow.Events.ReceiptReceivedEventArgs e)
        {
            Device.BeginInvokeOnMainThread( () =>
            {
                if (e.ConversationId == Conversation.Id)
                {
                    log.Debug("[RbInstantMessaging_ReceiptReceived] MessageId:[{0}] - ReceiptType:[{1}]", e.MessageId, e.ReceiptType);
                    MessageModel message = GetMessageByMessageId(e.MessageId);
                    if (message != null)
                        SetReceiptPartOfMessage(message, e.ReceiptType);
                }
            });
        }

        private void RbContacts_ContactInfoChanged(object sender, Rainbow.Events.JidEventArgs e)
        {
            Device.BeginInvokeOnMainThread( () =>
            {
                // Need to update this contact in each messages (not avatar part)
                UpdateMessagesForJid(e.Jid, false, true);

                // Need to update this contact in each replied part
                UpdateRepliedMessagesForJid(e.Jid);
            });
        }

        private void RbContacts_ContactAdded(object sender, Rainbow.Events.JidEventArgs e)
        {
            Device.BeginInvokeOnMainThread( () =>
            {
                // Need to update this contact in each messages (avatar + display name)
                UpdateMessagesForJid(e.Jid, true, true);

                // Need to update this contact in each replied part
                UpdateRepliedMessagesForJid(e.Jid);
            });
        }

#endregion EVENTS FROM RAINBOW SDK

#region EVENTS FROM AVATAR POOL

        private void RbAvatars_BubbleAvatarUpdated(object sender, Rainbow.Events.IdEventArgs e)
        {
            if( (rbConversation != null) 
                && (rbConversation.Type == Rainbow.Model.Conversation.ConversationType.Room)
                && (rbConversation.Id == e.Id) )
            {
                if (Conversation != null)
                {
                    Device.BeginInvokeOnMainThread( () =>
                    {
                        Conversation.AvatarFilePath = Helper.GetConversationAvatarFilePath(Conversation);
                    });
                }
            }
        }

        private void RbAvatars_ContactAvatarUpdated(object sender, Rainbow.Events.IdEventArgs e)
        {
            Device.BeginInvokeOnMainThread( () =>
            {
                // Check Conversation Avatar
                if ((rbConversation != null)
                && (rbConversation.Type == Rainbow.Model.Conversation.ConversationType.User)
                && (rbConversation.Id == e.Id))
                {
                    if (Conversation != null)
                        Conversation.AvatarFilePath = Helper.GetConversationAvatarFilePath(Conversation);
                }

                // Need to update this contact in each messages (just avatar part)
                Contact contact = XamarinApplication.SdkWrapper.GetContactFromContactId(e.Id);
                if (contact != null)
                    UpdateMessagesForJid(contact.Jid_im, true, false);
                else
                    log.Warn("[RbAvatars_ContactAvatarUpdated] Contact avatar updated but no related Contact object found");
            });
        }

#endregion EVENTS FROM AVATAR POOL

#region EVENTS FROM FILE POOL

        private void FilePool_ThumbnailAvailable(object sender, Rainbow.Events.IdEventArgs e)
        {
            String conversationId = XamarinApplication.SdkWrapper.GetConversationIdByFileDescriptorId(e.Id);
            if(conversationId == Conversation.Id)
            {
                MessageModel message = GetMessageByFileDescriptorId(e.Id);
                if (message != null)
                {
                    Device.BeginInvokeOnMainThread( () =>
                    {
                        SetFileAttachmentSourceOfMessage(message, e.Id);
                    });
                }
                else
                    log.Warn("[FilePool_ThumbnailAvailable] Thumbnail Available (FileId:[{0}]) but no related message found ...", e.Id);
            }
        }

        private void FilePool_FileDescriptorAvailable(object sender, Rainbow.Events.IdEventArgs e)
        {
            // For the moment nothing to do.
            // Thumbnail is automatically downloaded if the file descriptor concerns an image
        }

#endregion EVENTS FROM FILE POOL
    }
}
