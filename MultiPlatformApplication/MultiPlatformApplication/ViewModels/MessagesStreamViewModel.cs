using MultiPlatformApplication.Controls;
using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using NLog;
using Rainbow;
using Rainbow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class MessagesStreamViewModel
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(MessagesStreamViewModel));
        private static readonly int NB_MESSAGE_LOADED_BY_ROW = 20;

        private View rootView;
        private MessageInput messageInput;
        private ListView contextMenuMessageUrgencyListView;
        private ContentView contentViewPlatformSpecific;
        private View contextMenuMessageUrgency;
        private RefreshView refreshView;
        private StackLayout stackLayout;
        private ScrollView scrollView;

        private String conversationId = null;
        private String currentContactId;
        private String currentContactJid;
        private Conversation rbConversation = null; // Rainbow Conversation object

        private Object lockObservableMessagesList = new Object(); // To lock access to the observable collection: 'MessagesList'

        private List<MessageElementModel> MessagesList { get; set; } = new List<MessageElementModel>();

#region BINDINGS used in XAML
        public DynamicStreamModel DynamicStream { get; private set; } = new DynamicStreamModel();

        public ContextMenuModel MessageUrgency { get; private set; } = new ContextMenuModel();

#endregion BINDINGS used in XAML

        public MessagesStreamViewModel(View rootView)
        {
            this.rootView = rootView;

            messageInput = rootView.FindByName<MessageInput>("MessageInput");
            if (messageInput != null)
            {
                messageInput.MessageUrgencyClicked += MessageInput_MessageUrgencyClicked;
                messageInput.MessageAttachmentClicked += MessageInput_MessageAttachmentClicked;
                messageInput.MessageSendClicked += MessageInput_MessageSendClicked;
            }

            contextMenuMessageUrgencyListView = rootView.FindByName<ListView>("ContextMenuMessageUrgencyListView");
            if(contextMenuMessageUrgencyListView != null)
            contextMenuMessageUrgencyListView.ItemSelected += ContextMenuMessageUrgencyListView_ItemSelected;

            contentViewPlatformSpecific = rootView.FindByName<ContentView>("ContentViewPlatformSpecific");

            contextMenuMessageUrgency = rootView.FindByName<Grid>("ContextMenuMessageUrgency");
            contextMenuMessageUrgency.BindingContext = this;


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

                SetMessageUrgencyModel();
                // Set default selection
                SetMessageUrgencySelectedItem(3);

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
                    // Get messages from cache
                    List<Rainbow.Model.Message> rbMessagesList = Helper.SdkWrapper.GetAllMessagesFromConversationIdFromCache(conversationId);
                    if (rbMessagesList?.Count > 0)
                    {
                        AddToModelRbMessages(rbMessagesList);
                    }
                    else
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
                    Device.BeginInvokeOnMainThread(() =>
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

        private void AddToModelRbMessages(List<Rainbow.Model.Message> rbMessagesList, bool atTheEnd = false)
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
                msg = GetMessageFromRBMessage(rbMessage, rbConversation.Type);

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

            Device.BeginInvokeOnMainThread(() =>
            {
                lock (lockObservableMessagesList)
                {
                    DynamicStream.UserAskingToScroll = false;
                    DynamicStream.CodeAskingToScroll = true;

                    if (MessagesList.Count != 0)
                    {
                        // Remove first element of current list if it's a "date" message separator
                        MessageElementModel lastElementAdded = messagesElementList.Last();
                        MessageElementModel firstElement = MessagesList[0];
                        if (AreBothMessagesFromSameDay(lastElementAdded, firstElement))
                        {
                            if (MessagesList[0].Content.Type == "date")
                            {
                                MessagesList.RemoveAt(0);
                                stackLayout.Children.RemoveAt(0);
                            }
                        }
                    }

                    // NEED TO STORE SCROLLING INFO TO SCROLL TO SAME POSITION ONCE ELEMENTS ARE ADDED
                    StoreScrollingPosition();

                    ContentView element;
                    int indexInsert;

                    if (atTheEnd)
                        indexInsert = MessagesList.Count;
                    else
                        indexInsert = 0;

                    foreach (MessageElementModel message in messagesElementList)
                    {
                        if (message.Content.Type == "date")
                            element = new MessageSeparatorDate();

                        else if (message.Content.Type == "event")
                            element = new MessageSeparatorEvent();

                        else if (message.Content.Type == "displayName")
                            element = new MessageSeparatorDisplayName();

                        else if (message.Peer.Id != currentContactId)
                        {
                            if (message.Content.WithAvatar)
                                element = new MessageOtherUserWithAvatar();
                            else
                                element = new MessageOtherUser();
                        }
                        else
                        {
                            if (message.Content.WithAvatar)
                                element = new MessageCurrentUserWithDate();
                            else
                                element = new MessageCurrentUser();
                        }
                        element.BindingContext = message;

                        
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

        private MessageElementModel GetMessageFromRBMessage(Rainbow.Model.Message rbMessage, String conversationType)
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

                message.Date = rbMessage.Date;
                message.DateDisplayed = Helper.HumanizeDateTime(rbMessage.Date) + (String.IsNullOrEmpty(message.ReplaceId) ? "" : " - " + Helper.GetLabel("modified") );

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
                            message.Content.Body = Helper.GetLabel("messageReceivedDeleted");
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
                            Size = Helper.HumanizeFileSize(rbMessage.FileAttachment.Size)
                        };
                    }
                }
            }
            return message;
        }

#region MESSAGE INPUT STUFF

        private void MessageInput_MessageSendClicked(object sender, Rainbow.Events.IdEventArgs e)
        {
            // TODO
        }

        private void MessageInput_MessageAttachmentClicked(object sender, EventArgs e)
        {
            // TODO
        }

        private void MessageInput_MessageUrgencyClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                SetMessageUrgencyModel();
                DisplayMessageUrgencyContextMenu();
            });
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
            double v = scrollView.Content.Height - scrollView.Height;
            DynamicStream.DeltaYFromTheBottom = (v > 0) ? v : 0;
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

#region MESSAGE URGENCY CONTEXT MENU STUFF

        private void HideMessageUrgencyContextMenu()
        {
			contextMenuMessageUrgency.IsVisible = false;
		}

		private void DisplayMessageUrgencyContextMenu()
        {
			contextMenuMessageUrgency.IsVisible = true;
		}

		private void SetMessageUrgencySelectedItem(int selectedIndex)
        {
			if (selectedIndex == -1)
				return;

			if(MessageUrgency?.Items?.Count > selectedIndex)
            {
                for (int i = 0; i < MessageUrgency.Items.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        messageInput?.SetUrgencySelection(MessageUrgency.Items[i].Id);
                        MessageUrgency.Items[i].IsSelected = true;
                    }
                    else
                        MessageUrgency.Items[i].IsSelected = false;
                }
			}
        }

        private void SetMessageUrgencyModel()
        {
			MessageUrgency.Clear();

			Color color;
			ContextMenuItemModel  messageUrgencyModelItem;

			color = Helper.GetResourceDictionaryById<Color>("ColorUrgencyEmergency");
			messageUrgencyModelItem = new ContextMenuItemModel () { Id = "Emergency", ImageSourceId = "Font_Fire|" + color.ToHex(), Title = Helper.GetLabel("emergencyAlert"), Description = Helper.GetLabel("emergencyAlertInfo"), TextColor = color };
			MessageUrgency.Add(messageUrgencyModelItem);

			color = Helper.GetResourceDictionaryById<Color>("ColorUrgencyImportant");
			messageUrgencyModelItem = new ContextMenuItemModel () { Id = "Important", ImageSourceId = "Font_ExclamationTriangle|" + color.ToHex(), Title = Helper.GetLabel("warningAlert"), Description = Helper.GetLabel("warningAlertInfo"), TextColor = color };
			MessageUrgency.Add(messageUrgencyModelItem);

			color = Helper.GetResourceDictionaryById<Color>("ColorUrgencyInformation");
			messageUrgencyModelItem = new ContextMenuItemModel () { Id = "Information", ImageSourceId = "Font_Lightbulb|" + color.ToHex(), Title = Helper.GetLabel("notifyAlert"), Description = Helper.GetLabel("notifyAlertInfo"), TextColor = color };
			MessageUrgency.Add(messageUrgencyModelItem);

            color = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageOtherUserFont");
            messageUrgencyModelItem = new ContextMenuItemModel () { Id = "Standard", ImageSourceId = "Font_CommentAlt|" + color.ToHex(), Title = Helper.GetLabel("standardAlert"), Description = Helper.GetLabel("standardAlertInfo"), TextColor = color };
			MessageUrgency.Add(messageUrgencyModelItem);
		}

        private void ContextMenuMessageUrgencyListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            SetMessageUrgencySelectedItem(e.SelectedItemIndex);
            contextMenuMessageUrgencyListView.SelectedItem = null;

            HideMessageUrgencyContextMenu();
        }


#endregion MESSAGE URGENCY CONTEXT MENU STUFF

#region EVENTS FROM SDKWRAPPER

        private void SdkWrapper_MessageReceived(object sender, Rainbow.Events.MessageEventArgs e)
        {
            if (e.ConversationId == conversationId)
            {
                log.Debug("[SdkWrapper_MessageReceived] - FromJId:[{0}] - ToJid:[{1}] - CarbonCopy:[{2}] - Message.Id:[{3}] - Message.ReplaceId:[{4}]", e.Message.FromJid, e.Message.ToJid, e.CarbonCopy, e.Message.Id, e.Message.ReplaceId);

                // Mark  the message as read
                Helper.SdkWrapper.MarkMessageAsRead(conversationId, e.Message.Id);


                // Manage incoming REPLACE message
                if (!String.IsNullOrEmpty(e.Message.ReplaceId))
                {
                    // TO DO
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
            // TODO
        }

#endregion EVENTS FROM SDKWRAPPER

    }
}
