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
        Boolean userIsTyping = false;
        List<String> peerJidTyping = new List<string>();

        Animation userTypingAnimation;

        String replyToMessageId = null;

        public event EventHandler<EventArgs> UpdateParentLayout; // To ask parent to update the layout when the size of this UI component has changed

        public event EventHandler<BooleanEventArgs> UserIsTyping;
        public event EventHandler<RectEventArgs> MessageUrgencyClicked;

        public event EventHandler<StringEventArgs> MessageToSend;

#region BINDINGS used in XAML
        public MessageInputModel Message { get; private set; } = new MessageInputModel();
#endregion BINDINGS used in XAML

        public MessageInput()
        {
            Message.AttachmentCommand = new RelayCommand<object>(new Action<object>(MessageInputAttachmentCommand));
            Message.UrgencyCommand = new RelayCommand<object>(new Action<object>(MessageInputUrgencyCommand));
            Message.SendCommand = new RelayCommand<object>(new Action<object>(MessageInputSendCommand));

            Message.BreakLineModifier = "shift";
            Message.ValidationCommand = new RelayCommand<object>(new Action<object>(MessageInputValidationCommand));

            InitializeComponent();
            BindingContext = this;

            MessageInputAttachments.UpdateParentLayout += MessageInputAttachments_UpdateParentLayout;
            
            ButtonTyping.PropertyChanged += ButtonTyping_PropertyChanged;

            MessageContentReplyButton.Command = new RelayCommand<object>(new Action<object>(MessageContentReplyButtonCommand));
            MessageContentReplyElement.PropertyChanged += MessageContentReplyElement_PropertyChanged;

            EntryMessage.Placeholder = Helper.SdkWrapper.GetLabel("enterTextHere");
            EntryMessage.PropertyChanged += EntryMessage_PropertyChanged;
            EntryMessage.TextChanged += EntryMessage_TextChanged;

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

            // Update UI using correct thread
            Device.BeginInvokeOnMainThread(() =>
            {
                LabelTyping.Text = label;

                if (start)
                    StartUserTypingAnimation();
                else
                    StopUserTypingAnimation();
            });

        }

        public void SetUrgencySelection(String urgencyTypeString)
        {
            Helper.GetUrgencyInfo(urgencyTypeString, out Color backgroundColor, out Color color, out String title, out String label, out String imageSourceId);

            if (urgencyTypeString == UrgencyType.Std.ToString())
                label = "";

            if (String.IsNullOrEmpty(label))
            {
                StackLayoutUrgency.Padding = new Thickness(0);
                color = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageOtherUserFont");
                backgroundColor = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageOtherUserBackGround");
                title = "";
            }
            else
                StackLayoutUrgency.Padding = new Thickness(2);

            LabelUrgency.Text = title;
            LabelUrgency.TextColor = color;
            FrameUrgency.BackgroundColor = backgroundColor;

            EntryMessage.SetFocus();

            // Need to force the layout of the parent ...
            UpdateParentLayout?.Raise(this, null);
            //((Layout)this.Parent).ForceLayout();
        }

        public void SetReplyMessage(MessageElementModel messageElementModel)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                // We can't add attachments
                ButtonAttachment.IsVisible = false;

                replyToMessageId = messageElementModel?.Reply?.Id;

                MessageContentReply.SetUsageMode(true);
                MessageContentReply.BindingContext = messageElementModel;

                MessageContentReplyElement.IsVisible = true;

                // Need to force the layout of the parent ...
                UpdateParentLayout?.Raise(this, null);
            });
        }

        private void MessageInputAttachments_UpdateParentLayout(object sender, EventArgs e)
        {
            // Need to force the layout of the parent ...
            UpdateParentLayout?.Raise(this, null);
        }

        private void MessageContentReplyElement_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ( (e.PropertyName == "IsVisible") || (e.PropertyName == "Height") )
            {
                // Need to force the layout of the parent ...
                UpdateParentLayout?.Raise(this, null);
            }
        }

        private void ButtonTyping_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsVisible")
            {
                // Need to force the layout of the parent ...
                UpdateParentLayout?.Raise(this, null);
            }
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
        
        private void EntryMessage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Height")
            {
                // Need to force the layout of the parent ...
                UpdateParentLayout?.Raise(this, null);
                //((Layout)this.Parent).ForceLayout();
            }
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
            MessageInputAttachments.PickFiles();
        }

        private void MessageInputUrgencyCommand(object obj)
        {
            if (obj is VisualElement visualElement)
            {
                Rect rect = Helper.GetRelativePosition(visualElement, typeof(RelativeLayout));

                MessageUrgencyClicked?.Raise(this, new RectEventArgs(rect));
            }
        }

        private void MessageInputValidationCommand(object obj)
        {
            MessageInputSendCommand(obj);
        }

        private void MessageInputSendCommand(object obj)
        {
            MessageToSend?.Raise(this, new StringEventArgs(replyToMessageId));
            
            // Simulate that we close Reply part
            MessageContentReplyButtonCommand(null);
        }

        private void MessageContentReplyButtonCommand(object obj)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                // Clear value
                replyToMessageId = null;

                // Attachments are now available
                ButtonAttachment.IsVisible = true;

                MessageContentReplyElement.IsVisible = false;

                // Need to force the layout of the parent ...
                UpdateParentLayout?.Raise(this, null);
            });
        }

    }
}