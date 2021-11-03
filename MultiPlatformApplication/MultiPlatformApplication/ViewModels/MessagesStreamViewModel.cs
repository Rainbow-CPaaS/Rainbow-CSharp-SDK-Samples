using MultiPlatformApplication.Controls;
using MultiPlatformApplication.Effects;
using MultiPlatformApplication.Events;
using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using MultiPlatformApplication.Services;
using NLog;
using Rainbow;
using Rainbow.Events;
using Rainbow.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class MessagesStreamViewModel
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(MessagesStreamViewModel));
        private static readonly int NB_MESSAGE_LOADED_BY_ROW = 20;

        private FilesUpload FilesUpload;

        private View rootView;
        private MessageInput messageInput;

        private Boolean capabilityFileSharing;
        private Boolean capabilityFileStorage;
        private Boolean capabilityMessageUrgencyReception;
        private Boolean capabilityReadReceipt;
        private Boolean capabilityMessageUrgencySending;

        ContextMenuModel ContextMenuModelMessageAction = new ContextMenuModel();
        Controls.ContextMenu ContextMenuMessageAction = null;

        ContextMenuModel ContextMenuModelUrgency = new ContextMenuModel();
        Controls.ContextMenu ContextMenuUrgency = null;

        private ContentView contentViewPlatformSpecific;
        private RefreshView refreshView;
        private StackLayout stackLayout;
        
        private ScrollView scrollView;
        private DateTime dateTimeLastScrolled;

        private String lastMessageIdOfCurrentUser;
        private MessageElementModel actionDoneOnMessage;

        private String conversationId = null;
        private String currentContactId;
        private String currentContactJid;
        private Conversation rbConversation = null; // Rainbow Conversation object

        private Object lockObservableMessagesList = new Object(); // To lock access to the observable collection: 'MessagesList'

        private List<String> PeerJidTyping = new List<string>();

        private List<MessageElementModel> MessagesList { get; set; } = new List<MessageElementModel>();

        private Object lockUploadObjects = new Object();
        private List<String> filesDescriptorIdListUploading = new List<string>(); // To store the list of FileDescriptorId in a upload process


#region BINDINGS used in XAML
        public DynamicStreamModel DynamicStream { get; private set; } = new DynamicStreamModel();

#endregion BINDINGS used in XAML

        public MessagesStreamViewModel(View rootView)
        {
            this.rootView = rootView;

            capabilityFileSharing = Helper.SdkWrapper.IsCapabilityAvailable(Rainbow.Model.Contact.Capability.FileSharing);
            capabilityFileStorage = Helper.SdkWrapper.IsCapabilityAvailable(Rainbow.Model.Contact.Capability.FileStorage);
            capabilityMessageUrgencyReception = Helper.SdkWrapper.IsCapabilityAvailable(Rainbow.Model.Contact.Capability.MessageUrgencyReception);
            capabilityReadReceipt = Helper.SdkWrapper.IsCapabilityAvailable(Rainbow.Model.Contact.Capability.ReadReceipt);
            capabilityMessageUrgencySending = Helper.SdkWrapper.IsCapabilityAvailable(Rainbow.Model.Contact.Capability.MessageUrgencySending);

            // Need to deal with filesUpload
            FilesUpload = FilesUpload.Instance;
            FilesUpload.FileDescriptorCreated += FilesUpload_FileDescriptorCreated;
            FilesUpload.FileDescriptorNotCreated += FilesUpload_FileDescriptorNotCreated;
            FilesUpload.UploadNotPerformed += FilesUpload_UploadNotPerformed;

            // We need to follow file upload progress
            Helper.SdkWrapper.FileUploadUpdated += SdkWrapper_FileUploadUpdated;

            messageInput = rootView.FindByName<MessageInput>("MessageInput");
            if (messageInput != null)
            {
                messageInput.MessageUrgencyClicked += MessageInput_MessageUrgencyClicked;
                messageInput.MessageToSend += MessageInput_MessageToSend;
                messageInput.UserIsTyping += MessageInput_UserIsTyping;
                messageInput.SizeChanged += MessageInput_SizeChanged;
            }

            // Need to know if message edition is stopped/finished
            Helper.SdkWrapper.StopMessageEdition += SdkWrapper_StopMessageEdition;
            
            // Need to know if we need to start file download
            Helper.SdkWrapper.StartFileDownload += SdkWrapper_StartFileDownload;

            contentViewPlatformSpecific = rootView.FindByName<ContentView>("ContentViewPlatformSpecific");
            if (contentViewPlatformSpecific == null)
                return;
            // Set Binding Context
            contentViewPlatformSpecific.Content.BindingContext = DynamicStream;

            // Do we have a RefresView as content ?
            Type type = contentViewPlatformSpecific.Content.GetType();
            if (type == typeof(RefreshView))
            {
                refreshView = contentViewPlatformSpecific.FindByName<RefreshView>("RefreshViewMobile");
                scrollView = contentViewPlatformSpecific.FindByName<ScrollView>("ScrollViewMobile");
                stackLayout = contentViewPlatformSpecific.FindByName<StackLayout>("StackLayoutMobile");
            }
            else
            {
                refreshView = null;
                scrollView = contentViewPlatformSpecific.FindByName<ScrollView>("ScrollViewDesktop");
                stackLayout = contentViewPlatformSpecific.FindByName<StackLayout>("StackLayoutDesktop");
            }

            scrollView.Scrolled += ScrollView_Scrolled;
            scrollView.Content.SizeChanged += ScrollContent_SizeChanged;


            DynamicStream.AskMoreItemsCommand = new RelayCommand<object>(new Action<object>(AskMoreMessagesCommand));
        }

        public void Initialize(String conversationId)
        {
            if(this.conversationId != conversationId)
            {
                this.conversationId = conversationId;

                InitializeSdkObjectsAndEvents();

                Task task = new Task(() =>
                {
                    Thread.Sleep(300);
                    AskMoreMessagesCommand(null);
                });
                task.Start();
            }
        }

        private void InitializeSdkObjectsAndEvents()
        {
            // Manage event(s) from InstantMessaging
            Helper.SdkWrapper.MessageReceived += SdkWrapper_MessageReceived;
            Helper.SdkWrapper.UserTypingChanged += SdkWrapper_UserTypingChanged;

            currentContactId = Helper.SdkWrapper.GetCurrentContactId();
            currentContactJid = Helper.SdkWrapper.GetCurrentContactJid();

            if (!String.IsNullOrEmpty(conversationId))
            {
                // Get Rainbow Conversation object
                rbConversation = Helper.SdkWrapper.GetConversationByIdFromCache(conversationId);
            }
        }
 
        private void AskMoreMessagesCommand(object obj)
        {
            Popup.HideCurrentContextMenu();
            DynamicStream.AskingMoreItems = true;

            Task task = new Task(() =>
            {
                // Do we have already some messages ?
                if (MessagesList.Count > 0)
                {
                    LoadMoreMessages();
                }
                else
                {
                    Boolean loadmoreMsg = true;

                    // Get messages from cache
                    List<Rainbow.Model.Message> rbMessagesList = Helper.SdkWrapper.GetAllMessagesFromConversationIdFromCache(conversationId);
                    if (rbMessagesList?.Count > 0)
                    {
                        AddToModelRbMessages(rbMessagesList);
                        if (rbMessagesList.Count >= NB_MESSAGE_LOADED_BY_ROW)
                            loadmoreMsg = false;
                    }

                    if (loadmoreMsg)
                        LoadMoreMessages();
                }
            });
            task.Start();
        }

        private void LoadMoreMessages()
        {
            Helper.SdkWrapper.GetMessagesFromConversationId(conversationId, NB_MESSAGE_LOADED_BY_ROW, callback =>
            {
                if (callback.Result.Success)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (callback.Data.Count == 0)
                        {
                            // No more items will be added in this dynamic list
                            DynamicStream.MoreItemsAvailable = false;
                            DynamicStream.AskingMoreItems = false;
                            DynamicStream.CodeAskingToScroll = false;
                        }
                        else
                            AddToModelRbMessages(callback.Data);
                    });
                }
                else
                {
                    // TODO
                }
            });
        }

        private void UpdateModelWithMessage(Rainbow.Model.Message rbMessage)
        {
            String replaceId = rbMessage.ReplaceId;

            int index = GetIndexUsingMessageId(rbMessage.Id);

            if (index > -1)
            {
                // We need to
                //  - remove the previous view from the stack layout
                //  - create the new view using new message


                // We need to use the date of the originale message
                MessageElementModel previousMessageBeforeReplace = MessagesList[index];
                rbMessage.Date = previousMessageBeforeReplace.Date;

                // Create message element model using the RB Message
                MessageElementModel message = GetMessageFromRBMessage(rbMessage, rbConversation.Type);
                
                // Get from the list, the message just before
                MessageElementModel previousMessageInList = null;
                if (index > 0)
                    previousMessageInList = MessagesList[index - 1];

                // Using previous and current message, check if we need to have avatar or not ?
                NeedDisplayNameSeparator(message, previousMessageInList);

                // Create UI element using message element model
                ContentView element = GetContentViewAccordingMessage(message);

                // Now update the display list: remove older message and add the new one
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    StoreScrollingPosition();
                    DynamicStream.CodeAskingToScroll = true;
                            
                    stackLayout?.Children.RemoveAt(index);
                    MessagesList.RemoveAt(index);

                    stackLayout?.Children.Insert(index, element);
                    MessagesList.Insert(index, message);

                    // We need to wait a little before to scroll on correct position
                    WaitAndScrollToCorrectPosition();
                });
            }
        }

        private int GetIndexUsingMessageId(String messageId)
        {
            int result = -1;
            if (MessagesList?.Count > 0)
            {
                int nb = MessagesList.Count;
                for (int index = nb - 1; index >= 0; index--)
                {
                    if (MessagesList[index].Id == messageId)
                        return index;
                }
            }
            return result;
        }

        private ContentView GetContentViewAccordingMessage(MessageElementModel message)
        {
            if (message == null)
                return null;

            ContentView element;

            if (message.Content.Type == "date")
                element = new MessageSeparatorDate();

            else if (message.Content.Type == "event")
                element = new MessageSeparatorEvent();

            else if (message.Content.Type == "displayName")
                element = new MessageSeparatorDisplayName();

            else if (message.Peer.Jid != currentContactJid)
            {
                if (message.Content.WithAvatar)
                {
                    element = new MessageOtherUserWithAvatar();
                    ((MessageOtherUserWithAvatar)element).ActionMenuToDisplay += MessagesStreamViewModel_ActionMenuToDisplay;
                }
                else
                {
                    element = new MessageOtherUser();
                    ((MessageOtherUser)element).ActionMenuToDisplay += MessagesStreamViewModel_ActionMenuToDisplay;
                }
            }
            else
            {
                if (message.Content.WithAvatar)
                {
                    element = new MessageCurrentUserWithDate();
                    ((MessageCurrentUserWithDate)element).ActionMenuToDisplay += MessagesStreamViewModel_ActionMenuToDisplay;
                }
                else
                {
                    element = new MessageCurrentUser();
                    ((MessageCurrentUser)element).ActionMenuToDisplay += MessagesStreamViewModel_ActionMenuToDisplay;
                }
            }
            element.BindingContext = message;

            return element;
        }

        private void AddToModelRbMessages(List<Rainbow.Model.Message> rbMessagesList, bool atTheEnd = false, String fileAction = "")
        {
            if (rbMessagesList == null)
                return;

            if (rbMessagesList.Count == 0)
                return;

            // We reverse the list
            List<Rainbow.Model.Message> messagesList = rbMessagesList.ToList<Rainbow.Model.Message>();
            messagesList.Reverse();

            MessageElementModel msg = null;
            MessageElementModel previousMessage = null;

            List<MessageElementModel> messagesElementList = new List<MessageElementModel>();


            if (atTheEnd)
                previousMessage = MessagesList.LastOrDefault();

            foreach (Rainbow.Model.Message rbMessage in messagesList)
            {
                if (rbMessage == null)
                    continue;

                msg = GetMessageFromRBMessage(rbMessage, rbConversation.Type, fileAction);

                if (msg != null)
                {
                    // Need to add "date" message as separator ?
                    if (!AreBothMessagesFromSameDay(msg, previousMessage))
                    {
                        previousMessage = new MessageElementModel();
                        previousMessage.Date = msg.Date;
                        previousMessage.Content.Type = "date";
                        previousMessage.Content.Body = msg.Date.ToString("dddd dd MMMM").ToUpper();

                        messagesElementList.Add(previousMessage);
                    }

                    // Need to add "displayMessage" as separator ?
                    if (NeedDisplayNameSeparator(msg, previousMessage))
                    {
                        previousMessage = new MessageElementModel();
                        previousMessage.Date = msg.Date;
                        previousMessage.Content.Type = "displayName";
                        previousMessage.Peer = msg.Peer;
                        messagesElementList.Add(previousMessage);
                    }

                    // Add message to the list
                    messagesElementList.Add(msg);

                    // Now store the current msg as the "previous one"
                    previousMessage = msg;
                }
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                ContentView element;
                lock (lockObservableMessagesList)
                {
                    DynamicStream.UserAskingToScroll = false;
                    DynamicStream.CodeAskingToScroll = true;

                    if (MessagesList.Count != 0)
                    {
                        if (!atTheEnd)
                        {
                            // Remove first element displayed of the current list if it's a "date" message separator and the last element added is fro the same day
                            MessageElementModel firstElementDisplayed = MessagesList[0];
                            MessageElementModel lastElementToAdd = messagesElementList.Last();
                            if (AreBothMessagesFromSameDay(lastElementToAdd, firstElementDisplayed))
                            {
                                if (MessagesList[0].Content.Type == "date")
                                {
                                    MessagesList.RemoveAt(0);
                                    stackLayout.Children.RemoveAt(0);
                                }
                            }
                        }
                        else
                        {
                            
                            MessageElementModel firstElementToAdd = messagesElementList[0];

                            // We check if we have already a message with same File descriptor id in upload scenario process
                            String id = firstElementToAdd?.Content?.Attachment?.Id;
                            if ( (id != null)  && filesDescriptorIdListUploading.Contains(id))
                            {
                                // Remove this id from the list
                                filesDescriptorIdListUploading.Remove(id);

                                // Get the element already displayd using this id (we start from end of the list)
                                if (MessagesList.Count > 0)
                                {
                                    for (int index = MessagesList.Count-1; index>0; index--)
                                    {
                                        if(MessagesList[index]?.Content?.Attachment?.Id == id)
                                        {
                                            // Remove this element
                                            stackLayout.Children.RemoveAt(index);
                                            MessagesList.RemoveAt(index);
                                            break;
                                        }
                                    }
                                }
                            }

                            MessageElementModel lastElementDisplayed = MessagesList.Last();
                            // If last element displayed in the list as the same sender than the new one added, perhaps we need to:
                            //          - Avoid to display Avatar on each message
                            //          - Another case ? (non for the moment=
                            if (lastElementDisplayed.Peer.Jid == firstElementToAdd.Peer.Jid)
                            {
                                //  Avoid to display Avatar on each message
                                if (firstElementToAdd.Content.WithAvatar)
                                {
                                    // Avoid avatar on previous message if the delay is less than one minute
                                    if (firstElementToAdd.Date.Subtract(lastElementDisplayed.Date).TotalMinutes <= 1)
                                    {
                                        // Remove the previous one
                                        stackLayout.Children.RemoveAt(stackLayout.Children.Count - 1);
                                        MessagesList.RemoveAt(MessagesList.Count - 1);

                                        // Update the new one
                                        lastElementDisplayed.Content.WithAvatar = false;
                                        if (lastElementDisplayed.Peer.Jid == currentContactJid)
                                            element = new MessageCurrentUser();
                                        else
                                            element = new MessageOtherUser();

                                        element.BindingContext = lastElementDisplayed;
                                        stackLayout?.Children.Add(element);
                                        MessagesList.Add(lastElementDisplayed);
                                    }
                                }
                            }

                        }
                    }

                    // NEED TO STORE SCROLLING INFO TO SCROLL TO SAME POSITION ONCE ELEMENTS ARE ADDED
                    StoreScrollingPosition();
                    
                    int indexInsert;

                    if (atTheEnd)
                        indexInsert = MessagesList.Count;
                    else
                        indexInsert = 0;

                    foreach (MessageElementModel message in messagesElementList)
                    {
                        // Store File Descriptor Id since we are uploading a file
                        if (message?.Content?.Attachment?.Action == "upload")
                            filesDescriptorIdListUploading.Add(message.Content.Attachment.Id);

                        // Store Message Id of the last message sent by current user
                        if ((message.Peer.Jid == currentContactJid) && (indexInsert == MessagesList.Count))
                        {
                            // it's the last message but EDIT action is possible only if we have a valid content
                            if (message?.Content?.Body?.Length > 0)
                            {
                                if (!(message.Content.Body.StartsWith("/code", StringComparison.InvariantCultureIgnoreCase) || message.Content.Body.StartsWith("/img", StringComparison.InvariantCultureIgnoreCase)))
                                    lastMessageIdOfCurrentUser = message.Id;
                            }
                        }

                        element = GetContentViewAccordingMessage(message);

                        stackLayout?.Children.Insert(indexInsert,element);
                        MessagesList.Insert(indexInsert,message);
                        indexInsert++;
                    }

                    DynamicStream.AskingMoreItems = false;
                }
            });
        }

        private Boolean AreBothMessagesFromSameDay(MessageElementModel msg, MessageElementModel previousMsg)
        {
            if (previousMsg == null)
                return false;

            if (msg.Date.Subtract(previousMsg.Date).TotalDays > 1)
                return false;

            if (msg.Date.Day == previousMsg.Date.Day)
                return true;

            return false;
        }

        // Check if a display name separator must be used 
        // But also to know if an avatar must be displayed for this message and/or for the previous one
        private Boolean NeedDisplayNameSeparator(MessageElementModel msg, MessageElementModel previousMsg)
        {
            Boolean result = false;

            if ((msg.Content.Type == "event") || (msg.Content.Type == "date"))
                return false;

            if (previousMsg == null)
            {
                msg.Content.WithAvatar = true;
                result = true;
            }

            if (!result)
            {
                if ((previousMsg.Content.Type == "event") || (previousMsg.Content.Type == "date"))
                {
                    msg.Content.WithAvatar = true;
                    result = true;
                }
                else
                {
                    msg.Content.WithAvatar = true;
                    if (previousMsg.Peer.Jid != msg.Peer.Jid)
                        result = true;
                    else
                    {
                        if (msg.Date.Subtract(previousMsg.Date).TotalMinutes > 1)
                            result = true;
                        else
                        {
                            // We must not display avatar on previous message
                            previousMsg.Content.WithAvatar = false;
                        }
                    }
                }
            }

            if (result)
            {
                // Never display name in One to One conversation
                if (rbConversation.Type == Rainbow.Model.Conversation.ConversationType.User)
                    result = false;
                // Never display name of current user
                else if (msg.Peer.Id == currentContactId)
                    result = false;
            }

            return result;
        }

        private MessageElementModel GetMessageFromRBMessage(Rainbow.Model.Message rbMessage, String conversationType, String fileAction = "")
        {
            MessageElementModel message = null;
            if (rbMessage != null)
            {
                //log.Debug("[GetMessageFromRBMessage] Message.Id:[{0}] - Message.ReplaceId:[{1}] - DateTime:[{2}] - Content:[{3}]", rbMessage.Id, rbMessage.ReplaceId, rbMessage.Date.ToString("o"), rbMessage.Content);

                message = new MessageElementModel();

                message.Id = rbMessage.Id;

                message.ConversationType = conversationType;
                message.ConversationId = conversationId;

                message.Peer.Jid = rbMessage.FromJid;
                message.Peer.Type = Rainbow.Model.Conversation.ConversationType.User;

                // Store replace Id
                message.ReplaceId = rbMessage.ReplaceId;

                // Store forward info
                message.IsForwarded = rbMessage.IsForwarded;

                message.Date = rbMessage.Date;
                message.DateDisplayed = Helper.HumanizeDateTime(rbMessage.Date) + (String.IsNullOrEmpty(message.ReplaceId) ? "" : " - " + Helper.SdkWrapper.GetLabel("modified") );

                Rainbow.Model.Contact contact = Helper.SdkWrapper.GetContactFromContactJid(rbMessage.FromJid);
                if (contact != null)
                {
                    message.Peer.Id = contact.Id;
                    message.Peer.DisplayName = Rainbow.Util.GetContactDisplayName(contact);
                }
                else
                {
                    log.Debug("[GetMessageFromRBMessage] Unknown Contact - Jid :[{0}] - MessageId:[{1}]", rbMessage.FromJid, rbMessage.Id);

                    // We ask to have more info about this contact usin AvatarPool
                    Helper.SdkWrapper.AddUnknownContactToPoolByJid(rbMessage.FromJid);
                }

                // Receipt
                message.Receipt = rbMessage.Receipt.ToString();
                message.NeedReceipt = capabilityReadReceipt;

                // Is-it an "Event message" ?
                if (!String.IsNullOrEmpty(rbMessage.BubbleEvent))
                {
                    message.Content.Type = "event";
                    message.Content.Body = rbMessage.BubbleEvent; // Store bubble event - will be used in "MessageSeparatorEvent" class
                }
                // Is-it an "CallLog message" ?
                else if (rbMessage.CallLogAttachment != null)
                {
                    message.Content.Type = "event";
                    message.CallLogAttachment = rbMessage.CallLogAttachment;
                }
                else
                {
                    // Display Emergency message s Std message if user has not the right to see them
                     
                    if ( (rbMessage.Urgency == UrgencyType.High) && (!capabilityMessageUrgencyReception) )
                        message.Content.Urgency = UrgencyType.Std;
                    else
                        message.Content.Urgency = rbMessage.Urgency;

                    if (!String.IsNullOrEmpty(rbMessage.Content))
                    {
                        message.Content.Type = "message";
                        message.Content.Body = rbMessage.Content;
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(rbMessage.ReplaceId))
                        {
                            message.Content.Type = "deletedMessage";
                            message.Content.Body = "";

                            // In this case we don't display "modified"
                            message.DateDisplayed = Helper.HumanizeDateTime(rbMessage.Date);
                        }
                    }

                    // Reply part
                    if (rbMessage.ReplyMessage != null)
                    {
                        message.Reply = new MessageReplyModel();

                        // Store Id and Stamp of this reply message
                        message.Reply.Id = rbMessage.ReplyMessage.Id;
                        message.Reply.Stamp = rbMessage.ReplyMessage.Stamp;
                    }

                    // FileAttachment
                    if (rbMessage.FileAttachment != null)
                    {
                        message.Content.Attachment = new MessageAttachmentModel()
                        {
                            Id = rbMessage.FileAttachment.Id,
                            Name = rbMessage.FileAttachment.Name,
                            Size = Helper.HumanizeFileSize(rbMessage.FileAttachment.Size),
                            Action = fileAction
                        };
                    }
                }
            }
            return message;
        }

#region MESSAGE INPUT STUFF

        private async void MessageInput_MessageToSend(object sender, StringEventArgs e)
        {
            // Get urgency
            UrgencyType urgencyType = GetMessageUrgencySelection();

            MessageInput messageInput = (MessageInput)sender;

            // Manage message as TEXT
            String text = messageInput.GetMessageContent();
            if(!String.IsNullOrEmpty(text))
                text = text.Trim();

            // Manage message with files
            List<FileResult> attachments = messageInput.GetFilesToSend();

            // Clear messsage input
            messageInput.Clear();

            // Set to standard urgency if at least we have to send something
            if ((!String.IsNullOrEmpty(text)) || (attachments?.Count > 0))
            {
                String std = UrgencyType.Std.ToString();
                messageInput?.SetUrgencySelection(std);
                ContextMenuUrgency?.SetSelectedItemId(std);
            }

            // Do we have a replyId ?
            if (String.IsNullOrEmpty(e.Value))
            {
                // Send text message
                if (!String.IsNullOrEmpty(text))
                    Helper.SdkWrapper.SendMessageToConversationId(conversationId, text, urgencyType);

                // Send File message using FilesUpload service
                if (attachments?.Count > 0)
                {
                    List<FileUploadModel> filesUploadList = new List<FileUploadModel>();
                    FileUploadModel fileUpload;

                    foreach (FileResult attachment in attachments)
                    {
                        try
                        {
                            fileUpload = new FileUploadModel();

                            fileUpload.PeerId = rbConversation.PeerId;
                            fileUpload.PeerType = rbConversation.Type;
                            fileUpload.Urgency = urgencyType;

                            // /!\ Can we store a lot of Stream objects ???

                            fileUpload.Stream = await attachment.OpenReadAsync();
                            fileUpload.FileFullPath = attachment.FullPath;
                            fileUpload.FileName = attachment.FileName;
                            fileUpload.FileSize = fileUpload.Stream.Length;

                            filesUploadList.Add(fileUpload);
                        }
                        catch
                        {
                            log.Warn("[MessageInput_MessageToSend] Cannot create stream object for this file:[{0}]", attachment.FullPath);
                        }
                    }
                    if (filesUploadList.Count > 0)
                        FilesUpload.AddFiles(filesUploadList);
                }
            }
            else
            {
                // Send text message
                if (!String.IsNullOrEmpty(text))
                {
                    Helper.SdkWrapper.ReplyToMessage(conversationId, e.Value, text, callback =>
                    {
                        // TODO - manage error
                    });
                }  

            }
        }

        private void MessageInput_UserIsTyping(object sender, Rainbow.Events.BooleanEventArgs e)
        {
            Helper.SdkWrapper.SendIsTypingInConversationById(conversationId, e.Value );
        }

        private void MessageInput_MessageUrgencyClicked(object sender, RectEventArgs e)
        {
            // Ensure to be on Main UI Thread
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => MessageInput_MessageUrgencyClicked(sender, e));
                return;
            }

            DisplayMessageUrgencyContextMenu(e.Rect);
        }

        private void MessageInput_SizeChanged(object sender, EventArgs e)
        {
            double deltaYFromBottom = scrollView.Content.Height - (scrollView.Height + scrollView.ScrollY);

            //if(deltaYFromBottom < 100)
                scrollView.ScrollToAsync(0, scrollView.Content.Height - scrollView.Height, false);
        }

#endregion MESSAGE INPUT STUFF

#region SCROLLING STUFF

        private void ScrollContent_SizeChanged(object sender, EventArgs e)
        {
            ScrollToCorrectPosition();
        }

        private void StoreScrollingPosition()
        {
            if (scrollView == null)
                return;

            // Get delta Y from the bottom before loading new items
            double v = scrollView.Content.Height - (scrollView.Height + scrollView.ScrollY);
            DynamicStream.DeltaYFromTheBottom = (v > 10) ? v : 0;
        }

        private double GetPositionToScroll()
        {
            double contentHeight = scrollView.Content.Height;
            double height = scrollView.Height;
            double y = -1;

            if (contentHeight > height)
                y = contentHeight - height - DynamicStream.DeltaYFromTheBottom;

            return y;
        }

        private void WaitAndScrollToCorrectPosition()
        {
            Task task = new Task(() =>
            {
                Thread.Sleep(100);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ScrollToCorrectPosition();
                });
            });
            task.Start();
        }

        private void ScrollToCorrectPosition()
        {
            if (DynamicStream.UserAskingToScroll)
                return;

            double y = GetPositionToScroll();
            if (y > 0)
            {
                double diff = (scrollView.ScrollY - y);
                if ( (diff > 2) || (diff < -2) )
                {
                    DynamicStream.CodeAskingToScroll = true;
                    scrollView.ScrollToAsync(0, y, false);
                }
                else
                    DynamicStream.CodeAskingToScroll = false;
            }
        }

        private void ScrollView_Scrolled(object sender, ScrolledEventArgs e)
        {
            dateTimeLastScrolled = DateTime.UtcNow;

            // If we scroll we hide any context menu
            Popup.HideCurrentContextMenu();

            // We managed scrolling in two ways
            // - If we are Asking (adding) more items to scroll to the correct position
            // - To load need items (if a refresh view is not used)
            if (DynamicStream.AskingMoreItems || DynamicStream.CodeAskingToScroll)
            {
                ScrollToCorrectPosition();
            }
            else
            {
                // It's the user who wants to scroll content
                DynamicStream.UserAskingToScroll = true;

                if (refreshView == null)
                {
                    // We load more items
                    if (e.ScrollY < 10)
                    {
                        AskMoreMessagesCommand(null);
                    }
                }
            }
        }

#endregion SCROLLING STUFF

#region ACTION CONTEXT MENU STUFF

        private void CreateContextMenuMessageAction()
        {
            if (ContextMenuMessageAction != null)
                return;

            Color color;
            String colorHex;
            String imageSourceId;

            ContextMenuModelMessageAction = new ContextMenuModel();

            color = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageOtherUserFont");
            colorHex = color.ToHex();

            imageSourceId = "Font_PencilAlt|" + colorHex;
            ContextMenuModelMessageAction.Add(new ContextMenuItemModel() { Id = "edit", ImageSourceId = imageSourceId, Title = Helper.SdkWrapper.GetLabel("edit") });

            imageSourceId = "Font_FileDownload|" + colorHex;
            ContextMenuModelMessageAction.Add(new ContextMenuItemModel() { Id = "download", ImageSourceId = imageSourceId, Title = Helper.SdkWrapper.GetLabel("download") });

            imageSourceId = "Font_Reply|" + colorHex;
            ContextMenuModelMessageAction.Add(new ContextMenuItemModel() { Id = "reply", ImageSourceId = imageSourceId, Title = Helper.SdkWrapper.GetLabel("replyToMessage") });

            imageSourceId = "Font_ArrowRight|" + colorHex;
            ContextMenuModelMessageAction.Add(new ContextMenuItemModel() { Id = "forward", ImageSourceId = imageSourceId, Title = Helper.SdkWrapper.GetLabel("forwardMessage") });

            imageSourceId = "Font_Copy|" + colorHex;
            ContextMenuModelMessageAction.Add(new ContextMenuItemModel() { Id = "copy", ImageSourceId = imageSourceId, Title = Helper.SdkWrapper.GetLabel("copy") });

            imageSourceId = "Font_TrashAlt|" + colorHex;
            ContextMenuModelMessageAction.Add(new ContextMenuItemModel() { Id = "delete", ImageSourceId = imageSourceId, Title = Helper.SdkWrapper.GetLabel("delete") });

            imageSourceId = "Font_CloudDownloadAlt|" + colorHex;
            ContextMenuModelMessageAction.Add(new ContextMenuItemModel() { Id = "save", ImageSourceId = imageSourceId, Title = Helper.SdkWrapper.GetLabel("save") });

            ContextMenuMessageAction = new Controls.ContextMenu { AutomationId = "ContextMenuMessageAction", WidthRequest = 120, StoreSelection = false };
            ContextMenuMessageAction.BindingContext = ContextMenuModelMessageAction;

            ContextMenuMessageAction.Command = new RelayCommand<object>(new Action<object>(ContextMenuMessageActionCommand));

            Popup.Add(null, ContextMenuMessageAction, PopupType.ContextMenu);
        }

        private void ChangeMessageActionVisibility(String id, Boolean isVisible)
        {
            foreach(var item in ContextMenuModelMessageAction.Items)
            {
                if (item.Id == id)
                {
                    item.IsVisible = isVisible;
                    break;
                }
            }
        }

		private void DisplayActionContextMenu(Rect rect)
        {
            Popup.Show("ContextMenuMessageAction", rect);
        }

        public async void ContextMenuMessageActionCommand(Object obj)
        {
            if (obj is String id)
            {
                String action = id;

                // Check if we have a valid message
                if (actionDoneOnMessage == null)
                    return;

                GetMessageContext(actionDoneOnMessage, out Boolean isCurrentUser, out Boolean withFileAttachment, out Boolean withBodyContent, out Boolean isLastMessageOfCurrentUser);

                // TODO - perform action
                switch (action)
                {
                    case "edit":
                        Helper.SdkWrapper.OnStartMessageEdition(this, new IdEventArgs(actionDoneOnMessage?.Id));
                        break;

                    case "download":
                        StartFileDownload(actionDoneOnMessage?.Content?.Attachment?.Id);
                        break;

                    case "reply":
                        MessageElementModel messageElementModel = new MessageElementModel();
                        messageElementModel.Id = "newReply";
                        messageElementModel.ConversationId = rbConversation.Id;
                        messageElementModel.ConversationType = rbConversation.Type;
                        messageElementModel.Reply = new MessageReplyModel();
                        messageElementModel.Reply.Id = actionDoneOnMessage.Id;
                        messageElementModel.Reply.Peer = actionDoneOnMessage.Peer;

                        messageElementModel.Reply.Content = new MessageContentModel();
                        messageElementModel.Reply.Content.Body = actionDoneOnMessage.Content?.Body;
                        messageElementModel.Reply.Content.Attachment = actionDoneOnMessage.Content?.Attachment;

                        messageInput.SetReplyMessage(messageElementModel);
                        break;

                    case "forward":
                        break;

                    case "copy":
                        String text = actionDoneOnMessage?.Content?.Body;
                        if (!String.IsNullOrEmpty(text))
                            await Clipboard.SetTextAsync(text);
                        break;

                    case "delete":
                        if (withFileAttachment)
                        {
                            Helper.SdkWrapper.RemoveFileDescriptor(actionDoneOnMessage.Content.Attachment.Id, callback =>
                            {
                                // TODO - manage error
                            });
                        }
                        else
                        {
                            Helper.SdkWrapper.DeleteMessage(conversationId, actionDoneOnMessage.Id, callback =>
                            {
                                // TODO - manage error
                            });
                        }
                        break;

                    case "save":
                        Helper.SdkWrapper.CopyFileToPersonalStorage(actionDoneOnMessage?.Content?.Attachment?.Id, callback =>
                        {
                            // TODO - manage error
                        });
                        break;
                }
            }
        }

        private void SetActionOptionsModel(Boolean isCurrentUser, Boolean withFileAttachment, Boolean withBodyContent, Boolean isLastMessageOfCurrentUser)
        {
            // Create (if necessary) Context Menu with Message Action
            CreateContextMenuMessageAction();

            // Actions possible: (to display in this order)
            //      - Edit      => Context: Last message of current user
            //      - Download  => Context: File Attachment
            //      - Reply     => Context: Always
            //      - Forward   => Context: For message only Always, With file needs Capability.FileSharing
            //      - Copy      => Context: Body.Content not null
            //      - Delete    => Context: Last message of current user OR (File Attachment + Current user + Capability.FileStorage)
            //      - Save      => Context: File Attachment + Other User + Capability.FileSharing

            ChangeMessageActionVisibility("edit", isLastMessageOfCurrentUser);

            ChangeMessageActionVisibility("download", withFileAttachment);

            //ChangeMessageActionVisibility("forward", (withFileAttachment && fileSharing) || (!withFileAttachment));
            ChangeMessageActionVisibility("forward", false); // => // TODO - Implement Forward action

            ChangeMessageActionVisibility("copy", withBodyContent);

            ChangeMessageActionVisibility("delete", (isLastMessageOfCurrentUser || (withFileAttachment && isCurrentUser && capabilityFileStorage)));

            ChangeMessageActionVisibility("save", (capabilityFileSharing && withFileAttachment && (!isCurrentUser)));
        }

        private void GetMessageContext(MessageElementModel messageElementModel, out Boolean isCurrentUser, out Boolean withFileAttachment, out Boolean withBodyContent, out Boolean isLastMessageOfCurrentUser)
        {
            isCurrentUser = messageElementModel?.Peer?.Jid == currentContactJid;
            withFileAttachment = messageElementModel?.Content?.Attachment != null;
            withBodyContent = messageElementModel?.Content?.Body?.Length > 0;
            isLastMessageOfCurrentUser = messageElementModel?.Id == lastMessageIdOfCurrentUser;
        }

        private void MessagesStreamViewModel_ActionMenuToDisplay(object sender, EventArgs e)
        {
            // We will display Action menu if we are not aksing more items
            if (DynamicStream.AskingMoreItems)
                return;

            // We will display Action menu if we haven't scroll to recently the list
            if (DateTime.UtcNow.Subtract(dateTimeLastScrolled).TotalMilliseconds < Helper.DELAY_FOR_LONG_PRESS)
                return;

            // Ensure to be on Main UI Thread
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => MessagesStreamViewModel_ActionMenuToDisplay(sender, e));
                return;
            }

            if ((sender != null) && (sender is ContentView element))
            {
                actionDoneOnMessage = (MessageElementModel)element.BindingContext;
                if (actionDoneOnMessage == null)
                    return;

                if (e is RectEventArgs rectEventArgs)
                {
                    // Add Haptic Feedback Long Press
                    Helper.HapticFeedbackLongPress();

                    GetMessageContext(actionDoneOnMessage, out Boolean isCurrentUser, out Boolean withFileAttachment, out Boolean withBodyContent, out Boolean isLastMessageOfCurrentUser);

                    SetActionOptionsModel(isCurrentUser, withFileAttachment, withBodyContent, isLastMessageOfCurrentUser);

                    DisplayActionContextMenu(rectEventArgs.Rect);
                }
            }
        }

        private void StartFileDownload(String fileDescriptorId)
        {
            if(fileDescriptorId != null)
            {
                FileDescriptor fileDescriptor = Helper.SdkWrapper.GetFileDescriptorFromCache(fileDescriptorId);

                if(fileDescriptor != null)
                {
                    // TODO - A specific folder (choose by end user) should be use instead on this one
                    String dstFolder = Helper.SdkWrapper.GetFolderPathForFilesStorage();

                    // TODO - enhance the way to deal with files downloaded several times
                    // For the momet we delete the previous one
                    String fileFullPath = Path.Combine(dstFolder, fileDescriptor.Name);
                    if (File.Exists(fileFullPath))
                        File.Delete(fileFullPath);

                    Helper.SdkWrapper.DownloadFile(fileDescriptor.Id, dstFolder, fileDescriptor.Name, callback =>
                    {
                        // TODO - Manage error
                    });
                }
            }
        }

        private void SdkWrapper_StopMessageEdition(object sender, StringListEventArgs e)
        {
            if ( (e?.Values?.Count > 0) && (actionDoneOnMessage != null)  && (e?.Values[0] == actionDoneOnMessage.Id) )
            {
                // Do we have a message ?
                if(e.Values.Count > 1)
                {
                    String newText = e.Values[1];
                    Helper.SdkWrapper.EditMessage(conversationId, actionDoneOnMessage.Id, newText, callback =>
                    { 
                        // TODO - manage error
                    });
                }
            }
        }

        private void SdkWrapper_StartFileDownload(object sender, IdEventArgs e)
        {
            StartFileDownload(e.Id);
        }

#endregion ACTION CONTEXT MENU STUFF

#region MESSAGE URGENCY CONTEXT MENU STUFF

        private void CreateContextMenuUrgency()
        {
            if (ContextMenuUrgency != null)
                return;

            Color color;
            Color backgroundColor;
            String title;
            String label;
            String imageSourceId;

            ContextMenuItemModel ContextMenuModel;

            String urgencyType;

            urgencyType = UrgencyType.High.ToString();
            Helper.GetUrgencyInfo(urgencyType, out backgroundColor, out color, out title, out label, out imageSourceId);
            ContextMenuModel = new ContextMenuItemModel() { Id = urgencyType, ImageSourceId = imageSourceId, Title = title, Description = label, TextColor = color, isVisible = capabilityMessageUrgencySending };
            ContextMenuModelUrgency.Add(ContextMenuModel);

            urgencyType = UrgencyType.Middle.ToString();
            Helper.GetUrgencyInfo(urgencyType, out backgroundColor, out color, out title, out label, out imageSourceId);
            ContextMenuModel = new ContextMenuItemModel() { Id = urgencyType, ImageSourceId = imageSourceId, Title = title, Description = label, TextColor = color };
            ContextMenuModelUrgency.Add(ContextMenuModel);

            urgencyType = UrgencyType.Low.ToString();
            Helper.GetUrgencyInfo(urgencyType, out backgroundColor, out color, out title, out label, out imageSourceId);
            ContextMenuModel = new ContextMenuItemModel() { Id = urgencyType, ImageSourceId = imageSourceId, Title = title, Description = label, TextColor = color };
            ContextMenuModelUrgency.Add(ContextMenuModel);

            urgencyType = UrgencyType.Std.ToString();
            Helper.GetUrgencyInfo(urgencyType, out backgroundColor, out color, out title, out label, out imageSourceId);
            ContextMenuModel = new ContextMenuItemModel() { Id = urgencyType, ImageSourceId = imageSourceId, Title = title, Description = label, TextColor = color, isSelected = true };
            ContextMenuModelUrgency.Add(ContextMenuModel);

            ContextMenuUrgency = new Controls.ContextMenu { AutomationId = "ContextMenuUrgency", WidthRequest = 280, StoreSelection = true };
            ContextMenuUrgency.BindingContext = ContextMenuModelUrgency;

            ContextMenuUrgency.Command = new RelayCommand<object>(new Action<object>(ContextMenuUrgencyCommand));

            Popup.Add(null, ContextMenuUrgency, PopupType.ContextMenu);
        }

		private void DisplayMessageUrgencyContextMenu(Rect rect)
        {
            // Create (if necessary) Context Menu with Urgency
            CreateContextMenuUrgency();

            Popup.Show("ContextMenuUrgency", rect);
        }

        private UrgencyType GetMessageUrgencySelection()
        {
            UrgencyType result = UrgencyType.Std;

            String id = ContextMenuUrgency?.GetSelectedItemId();
            if (!String.IsNullOrEmpty(id))
            {
                if (Enum.TryParse<UrgencyType>(id, out UrgencyType parse))
                    result = parse;
            }
            return result;
        }

        private void ContextMenuUrgencyCommand(Object obj)
        {
            if(obj is String id)
                messageInput?.SetUrgencySelection(id);
        }

#endregion MESSAGE URGENCY CONTEXT MENU STUFF

#region EVENTS FROM SDKWRAPPER

        private void SdkWrapper_MessageReceived(object sender, Rainbow.Events.MessageEventArgs e)
        {
            if (e.ConversationId == conversationId)
            {
                log.Debug("[SdkWrapper_MessageReceived] - FromJId:[{0}] - ToJid:[{1}] - CarbonCopy:[{2}] - Message.Id:[{3}] - Message.ReplaceId:[{4}]", e.Message.FromJid, e.Message.ToJid, e.CarbonCopy, e.Message.Id, e.Message.ReplaceId);

                // Mark  the message as read
                if(e.Message.FromJid != currentContactJid)
                    Helper.SdkWrapper.MarkMessageAsRead(conversationId, e.Message.Id);


                // Manage incoming REPLACE message
                if (!String.IsNullOrEmpty(e.Message.ReplaceId))
                {
                    UpdateModelWithMessage(e.Message);
                }
                else
                {
                    List<Message> list = new List<Message>();
                    list.Add(e.Message);
                    AddToModelRbMessages(list, true);
                }
            }
        }

        private void SdkWrapper_UserTypingChanged(object sender, Rainbow.Events.UserTypingEventArgs e)
        {
            if(e.ConversationId == conversationId)
            {
                if (String.IsNullOrEmpty(e.ContactJid))
                    return;

                if (e.ContactJid == currentContactJid)
                    return;

                bool updateDone = false;

                if(PeerJidTyping.Contains(e.ContactJid))
                {
                    if (!e.IsTyping)
                    {
                        updateDone = true;
                        PeerJidTyping.Remove(e.ContactJid);
                    }
                }
                else
                {
                    if (e.IsTyping)
                    {
                        updateDone = true;
                        PeerJidTyping.Add(e.ContactJid);
                    }
                }

                if (updateDone)
                    messageInput.UpdateUsersTyping(PeerJidTyping);
            }
        }

#endregion EVENTS FROM SDKWRAPPER


#region EVENTS ABOUT UPLOAD STUFF

        private void FilesUpload_FileDescriptorCreated(object sender, Rainbow.Events.StringListEventArgs e)
        {
            // 1) A FileDescriptor is created when a file is sent in a conversation
            if (e.Values?.Count >= 2)
            {
                // Check if it's related to this conversation
                if (e.Values[0] == rbConversation?.PeerId)
                {
                    log.Debug("[FilesUpload_FileDescriptorCreated] PeerId:[{0}] - FileDescriptorId:[{1}]", e.Values[0], e.Values[1]);

                    // Get FileUploadModel
                    FileUploadModel fileUploadModel = FilesUpload.GetFileUploadByFileDescriptorId(e.Values[1]);

                    // Get RbMessage
                    Rainbow.Model.Message RbMessage = fileUploadModel?.RbMessage;

                    // Update the UI
                    AddToModelRbMessages(new List<Message>() { RbMessage }, true, "upload");
                }
            }
        }

        private void SdkWrapper_FileUploadUpdated(object sender, Rainbow.Events.FileUploadEventArgs e)
        {
            // 2) After FileDescriptor is created, the upload process starts. Here we have info updated about this process

            // Check if it's related to this conversation
            if (e.PeerId == rbConversation?.PeerId)
            {
                log.Debug("[SdkWrapper_FileUploadUpdated] PeerId:[{0}] - InProgress:[{1}] - Completed:[{2}] - SizeUploaded:[{3}]", e.PeerId, e.InProgress, e.Completed, e.SizeUploaded);
            }
        }

        private void FilesUpload_FileDescriptorNotCreated(object sender, Rainbow.Events.StringListEventArgs e)
        {
            // 3) Unfortunatly, it's not possible to create a FileDescriptor ... 
            if (e.Values?.Count >= 2)
            {
                // Check if it's related to this conversation
                if (e.Values[0] == rbConversation?.PeerId)
                {
                    log.Warn("[FilesUpload_FileDescriptorCreated] PeerId:[{0}] - FileName:[{1}]", e.Values[0], e.Values[1]);
                }
            }
        }

        private void FilesUpload_UploadNotPerformed(object sender, Rainbow.Events.StringListEventArgs e)
        {
            // 4) Unfortunatly, it's not possible to start the upload process ...
            if (e.Values?.Count >= 3)
            {
                // Check if it's related to this conversation
                if (e.Values[0] == rbConversation?.PeerId)
                {
                    log.Warn("[FilesUpload_FileDescriptorCreated] PeerId:[{0}] - FileName:[{1}] - FileDescriptorId:[{2}]", e.Values[0], e.Values[1], e.Values[2]);
                }
            }
        }

#endregion EVENTS ABOUT UPLOAD STUFF

    }
}
