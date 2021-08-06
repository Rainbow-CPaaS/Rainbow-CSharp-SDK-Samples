using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using MultiPlatformApplication.ViewModels;
using Rainbow;
using Rainbow.Events;
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

        public event EventHandler<EventArgs> UpdateParentLayout; // To ask parent to update the layout when the size of this UI component has changed

        public event EventHandler<IdEventArgs> UserIsTyping;
        public event EventHandler<IdEventArgs> MessageSendClicked;
        public event EventHandler<EventArgs> MessageUrgencyClicked;

#region BINDINGS used in XAML
        public MessageInputModel Message { get; private set; } = new MessageInputModel();

#endregion BINDINGS used in XAML

        public MessageInput()
        {
            Message.AttachmentCommand = new RelayCommand<object>(new Action<object>(MessageInputAttachmentCommand));
            Message.UrgencyCommand = new RelayCommand<object>(new Action<object>(MessageInputUrgencyCommand));
            Message.SendCommand = new RelayCommand<object>(new Action<object>(MessageInputSendCommand));

            Message.ValidationKeyModifier = "shift";
            Message.ValidationCommand = new RelayCommand<object>(new Action<object>(MessageInputValidationCommand));

            InitializeComponent();
            BindingContext = this;

            MessageInputAttachments.UpdateParentLayout += MessageInputAttachments_UpdateParentLayout;
            
            ButtonTyping.PropertyChanged += ButtonTyping_PropertyChanged;

            EntryMessage.Placeholder = Helper.GetLabel("enterTextHere");
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

        private void MessageInputAttachments_UpdateParentLayout(object sender, EventArgs e)
        {
            // Need to force the layout of the parent ...
            UpdateParentLayout?.Raise(this, null);
        }

        private void ButtonTyping_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsVisible")
            {
                // Need to force the layout of the parent ...
                UpdateParentLayout?.Raise(this, null);
                //((Layout)this.Parent)?.ForceLayout();
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

        private void StartUserTypingAnimationAnimation()
        {
            if (!ButtonTyping.IsVisible)
            {
                ButtonTyping.IsVisible = true;
                userTypingAnimation.Commit(this, "UserTypingAnimation", length: 4000, repeat: () => true);
            }
        }

        private void StopUserTypingAnimationAnimation()
        {
            if (ButtonTyping.IsVisible)
            {
                this.AbortAnimation("UserTypingAnimation");
                ButtonTyping.IsVisible = false;
                LabelTyping.Text = " ";
            }
        }

        public void UpdateUsersTyping(List<String> peerJidTyping)
        {
            Boolean start = false;
            String label = " ";

            if ((peerJidTyping == null) || (peerJidTyping?.Count == 0) )
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
                    if(!String.IsNullOrEmpty(displayName))
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
                        label = Helper.GetLabel("isTyping");
                    else
                        label = Helper.GetLabel("areTyping");


                    label = "<b>" + displayNameList + "</b> " + label;
                    start = true;
                }
           }

            // Update UI using correct thread
            Device.BeginInvokeOnMainThread(() =>
            {
                LabelTyping.Text = label;

                if (start)
                    StartUserTypingAnimationAnimation();
                else
                    StopUserTypingAnimationAnimation();
            });

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
                    UserIsTyping?.Raise(this, new IdEventArgs("true"));
                }
            }
            else
            {
                if (userIsTyping)
                {
                    userIsTyping = false;
                    UserIsTyping?.Raise(this, new IdEventArgs("false"));
                }
            }
        }

        public void SetUrgencySelection(String id)
        {
            Color textColor;
            Color bgdColor;
            String label;
            switch (id)
            {
                case "Emergency":
                    textColor = Helper.GetResourceDictionaryById<Color>("ColorUrgencyEmergency");
                    bgdColor = Helper.GetResourceDictionaryById<Color>("ColorBackgroundUrgencyEmergency");
                    label = Helper.GetLabel("emergencyAlert");
                    break;

                case "Important":
                    textColor = Helper.GetResourceDictionaryById<Color>("ColorUrgencyImportant");
                    bgdColor = Helper.GetResourceDictionaryById<Color>("ColorBackgroundUrgencyImportant");
                    label = Helper.GetLabel("warningAlert");
                    break;

                case "Information":
                    textColor = Helper.GetResourceDictionaryById<Color>("ColorUrgencyInformation");
                    bgdColor = Helper.GetResourceDictionaryById<Color>("ColorBackgroundUrgencyInformation");
                    label = Helper.GetLabel("notifyAlert");
                    break;

                case "Standard":
                default:
                    textColor = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageOtherUserFont");
                    bgdColor = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageOtherUserBackGround");
                    label = "";
                    break;
            }

            if (String.IsNullOrEmpty(label))
                StackLayoutUrgency.Padding = new Thickness(0);
            else
                StackLayoutUrgency.Padding = new Thickness(2);

            LabelUrgency.Text = label;
            LabelUrgency.TextColor = textColor;
            FrameUrgency.BackgroundColor = bgdColor;

            EntryMessage.SetFocus();

            // Need to force the layout of the parent ...
            UpdateParentLayout?.Raise(this, null);
            //((Layout)this.Parent).ForceLayout();
        }

        private void MessageInputAttachmentCommand(object obj)
        {
            MessageInputAttachments.PickFiles();
        }

        private void MessageInputUrgencyCommand(object obj)
        {
            MessageUrgencyClicked?.Raise(this, null);
        }

        private void MessageInputSendCommand(object obj)
        {
            String text = EntryMessage.GetEditorText();

            if (text?.Length > 0)
            {
                //Get Content and trim it
                string content = text.Trim();

                // Clear text: as consequence this component size will be changed and eventually we ask parent to update layout
                EntryMessage.SetEditorText("");

                // Ask to send this message
                MessageSendClicked?.Raise(this, new IdEventArgs(content));
            }
        }

        private void MessageInputValidationCommand(object obj)
        {
            MessageInputSendCommand(obj);
        }
    }
}