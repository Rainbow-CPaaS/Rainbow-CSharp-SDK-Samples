﻿using MultiPlatformApplication.Effects;
using MultiPlatformApplication.Events;
using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using MultiPlatformApplication.ViewModels;
using Rainbow;
using Rainbow.Events;
using Rainbow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MessageInput : ContentView
    {
        String editMessageId = null;
        Boolean userIsTyping = false;
        Boolean capabilityFileSharing = false;
        List<String> peerJidTyping = new List<string>();

        Animation userTypingAnimation;

        String replyToMessageId = null;

        public event EventHandler<BooleanEventArgs> UserIsTyping;
        public event EventHandler<RectEventArgs> MessageUrgencyClicked;
        public event EventHandler<StringEventArgs> MessageToSend;
        public event EventHandler<StringEventArgs> MessageToEdit;

        #region BINDINGS used in XAML
        public MessageInputModel Message { get; private set; } = new MessageInputModel();
#endregion BINDINGS used in XAML

        public MessageInput()
        {
            // /!\ Message object must be defined FIRST !!!
            Message.AttachmentCommand = new RelayCommand<object>(new Action<object>(MessageInputAttachmentCommand));
            Message.UrgencyCommand = new RelayCommand<object>(new Action<object>(MessageInputUrgencyCommand));
            Message.SendCommand = new RelayCommand<object>(new Action<object>(MessageInputSendCommand));

            Message.BreakLineModifier = "shift";
            Message.ValidationCommand = new RelayCommand<object>(new Action<object>(MessageInputValidationCommand));

            InitializeComponent();
            BindingContext = this;

            // Check if user can send file
            capabilityFileSharing = Helper.SdkWrapper.IsCapabilityAvailable(Rainbow.Model.Contact.Capability.FileSharing);
            
            FrameBeforeButtonAttachment.IsVisible = capabilityFileSharing;
            ButtonAttachment.IsVisible = capabilityFileSharing;

            MessageContentReplyButton.Command = new RelayCommand<object>(new Action<object>(MessageContentReplyButtonCommand));

            EntryMessage.Placeholder = Helper.SdkWrapper.GetLabel("enterTextHere");
            EntryMessage.TextChanged += EntryMessage_TextChanged;
            EntryMessage.KeyboardRectChanged += EntryMessage_KeyboardRectChanged;

            var opacityToZero = new Animation((d) => { ButtonTyping.Opacity = d; }, 0.6, 0.4, Easing.Linear);
            var opacityToOne = new Animation((d) => { ButtonTyping.Opacity = d; }, 0.4, 0.6, Easing.Linear);

            userTypingAnimation = new Animation();
            userTypingAnimation.Add(0.25, 0.5, opacityToZero);
            userTypingAnimation.Add(0.75, 1, opacityToOne);

            // We need to update UserIsTyping viex if contact are updated
            Helper.SdkWrapper.PeerInfoChanged += SdkWrapper_PeerUpdated;
            Helper.SdkWrapper.PeerAdded += SdkWrapper_PeerUpdated;
        }

        public String GetMessageContent()
        {
            return EntryMessage.GetEditorText();
        }

        public List<FileResult> GetFilesToSend()
        {
            return MessageInputAttachments.GetFilesAttached();
        }

        public void Clear()
        {
            // Clear text: as consequence this component's size will be changed and eventually we ask parent to update layout
            EntryMessage.SetEditorText("");

            // Clear attachment
            MessageInputAttachments.Clear();
        }

        public void UpdateUsersTyping(List<String> peerJidTyping)
        {
            // Ensure to be on Main UI Thread
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => UpdateUsersTyping(peerJidTyping));
                return;
            }

            Boolean start = false;
            String label = " ";

            if ((peerJidTyping == null) || (peerJidTyping?.Count == 0))
            {
                this.peerJidTyping.Clear();
            }
            else
            {
                // Copy content
                this.peerJidTyping = new List<string>(peerJidTyping);

                // Get list of display name
                String displayNameList = null;
                String displayName;
                int nbAccount = 0;
                foreach (String jid in peerJidTyping)
                {

                    displayName = Helper.GetContactDisplayName(jid);
                    if (!String.IsNullOrEmpty(displayName))
                    {
                        nbAccount++;

                        if (displayNameList == null)
                            displayNameList = displayName;
                        else
                            displayNameList += ", " + displayName;
                    }
                }

                if (nbAccount > 0)
                {
                    if (nbAccount == 1)
                        label = Helper.SdkWrapper.GetLabel("isTyping");
                    else
                        label = Helper.SdkWrapper.GetLabel("areTyping");


                    label = "<b>" + displayNameList + "</b> " + label;
                    start = true;
                }
            }

            LabelTyping.Text = label;

            if (start)
                StartUserTypingAnimation();
            else
                StopUserTypingAnimation();
        }

        public String GetEditMessageId()
        {
            return editMessageId;
        }

        public void StopEditionMode()
        {
            // Remove message Id
            editMessageId = null;

            ButtonUrgency.IsVisible = true;

            // Attachments are now available
            FrameBeforeButtonAttachment.IsVisible = capabilityFileSharing;
            ButtonAttachment.IsVisible = capabilityFileSharing;
            ButtonAttachment.ImageSourceId = "Font_PaperClip|ColorButtonForeground";

            Color backgroundColor = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageOtherUserBackGround");
            Color textColor = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageOtherUserFont");
            String text = "";

            EntryMessage.SetEditorText("");

            SetContext(text, textColor, backgroundColor, false);
        }

        public void StartEditionMode(String messageId, String messageBody)
        {
            // Store message Id
            editMessageId = messageId;

            ButtonUrgency.IsVisible = false;
            // Attachment is used to stop edition mode
            FrameBeforeButtonAttachment.IsVisible = true;
            ButtonAttachment.IsVisible = true;
            ButtonAttachment.ImageSourceId = "Font_Times|ColorButtonForeground";

            Color backgroundColor = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageCurrentUserBackGround"); ;
            Color textColor = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageCurrentUserFont");
            String text = Helper.SdkWrapper.GetLabel("edit");

            EntryMessage.SetEditorText(messageBody);

            SetContext(text, textColor, backgroundColor, true);
        }

        public void SetUrgencySelection(String urgencyTypeString, Boolean setFocus)
        {
            Color backgroundColor;
            Color textColor;
            String text;

            if (urgencyTypeString == UrgencyType.Std.ToString())
            {
                textColor = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageOtherUserFont");
                backgroundColor = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageOtherUserBackGround");
                text = "";
            }
            else
                Helper.GetUrgencyInfo(urgencyTypeString, out backgroundColor, out textColor, out text, out String label, out String imageSourceId);

            SetContext(text, textColor, backgroundColor, setFocus);
        }

        private void SetContext(String text, Color textColor, Color backgroundColor, Boolean setFocus)
        {
            if (String.IsNullOrEmpty(text))
                StackLayoutUrgency.Padding = new Thickness(0);
            else
                StackLayoutUrgency.Padding = new Thickness(2);

            LabelUrgency.Text = text;
            LabelUrgency.TextColor = textColor;
            FrameUrgency.BackgroundColor = backgroundColor;

            if(setFocus)
                EntryMessage.SetFocus();
        }

        public void SetReplyMessage(MessageElementModel messageElementModel)
        {
            // Ensure to be on Main UI Thread
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => SetReplyMessage(messageElementModel));
                return;
            }

            // We can't add attachments
            FrameBeforeButtonAttachment.IsVisible = false;
            ButtonAttachment.IsVisible = false;

            replyToMessageId = messageElementModel?.Reply?.Id;

            MessageContentReply.SetUsageMode(true);
            MessageContentReply.BindingContext = messageElementModel;

            MessageContentReplyElement.IsVisible = true;
        }

        private void SdkWrapper_PeerUpdated(object sender, PeerEventArgs e)
        {
            if(peerJidTyping?.Count > 0)
            {
                if (peerJidTyping.Contains(e.Peer.Jid))
                    UpdateUsersTyping(peerJidTyping);
            }
        }

        private void StartUserTypingAnimation()
        {
            if (!ButtonTyping.IsVisible)
            {
                ButtonTyping.IsVisible = true;
                userTypingAnimation.Commit(this, "UserTypingAnimation", length: 4000, repeat: () => true);
            }
        }

        private void StopUserTypingAnimation()
        {
            if (ButtonTyping.IsVisible)
            {
                this.AbortAnimation("UserTypingAnimation");
                ButtonTyping.IsVisible = false;
                LabelTyping.Text = " ";
            }
        }

        private void EntryMessage_KeyboardRectChanged(object sender, Rect e)
        {
            BoxView.HeightRequest = e.Height;
        }

        private void EntryMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue?.Length > 0)
            {
                if (!userIsTyping)
                {
                    userIsTyping = true;
                    UserIsTyping?.Raise(this, new BooleanEventArgs(true));
                }
            }
            else
            {
                if (userIsTyping)
                {
                    userIsTyping = false;
                    UserIsTyping?.Raise(this, new BooleanEventArgs(false));
                }
            }
        }

        private void MessageInputAttachmentCommand(object obj)
        {
            if(String.IsNullOrEmpty(editMessageId))
                MessageInputAttachments.PickFiles();
            else
                StopEditionMode();
        }

        private void MessageInputUrgencyCommand(object obj)
        {
            if (obj is View view)
            {
                Rect rect = Popup.GetRectOfView(view);
                MessageUrgencyClicked?.Raise(this, new RectEventArgs(rect));
            }
        }

        private void MessageInputValidationCommand(object obj)
        {
            MessageInputSendCommand(obj);
        }

        private void MessageInputSendCommand(object obj)
        {
            if (String.IsNullOrEmpty(editMessageId))
            {
                MessageToSend?.Raise(this, new StringEventArgs(replyToMessageId));

                // Simulate that we close Reply part
                MessageContentReplyButtonCommand(null);
            }
            else
            {
                MessageToEdit?.Raise(this, new StringEventArgs(editMessageId));
                StopEditionMode();
            }
        }

        private void MessageContentReplyButtonCommand(object obj)
        {
            // Ensure to be on Main UI Thread
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => MessageContentReplyButtonCommand(obj));
                return;
            }

            // Clear value
            replyToMessageId = null;

            // Attachments are now available
            FrameBeforeButtonAttachment.IsVisible = capabilityFileSharing;
            ButtonAttachment.IsVisible = capabilityFileSharing;

            MessageContentReplyElement.IsVisible = false;
        }

    }
}