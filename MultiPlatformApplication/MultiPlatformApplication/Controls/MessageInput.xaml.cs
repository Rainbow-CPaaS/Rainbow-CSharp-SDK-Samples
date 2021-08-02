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

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MessageInput : ContentView
    {
        Boolean userIsTyping = false;

        public event EventHandler<IdEventArgs> UserIsTyping;
        public event EventHandler<IdEventArgs> MessageSendClicked;
        public event EventHandler<EventArgs> MessageUrgencyClicked;
        public event EventHandler<EventArgs> MessageAttachmentClicked;

#region BINDINGS used in XAML
        public MessageInputModel Message { get; private set; } = new MessageInputModel();

#endregion BINDINGS used in XAML


        public MessageInput()
        {
            Message.AttachmentCommand = new RelayCommand<object>(new Action<object>(MessageInputAttachmentCommand));
            Message.UrgencyCommand = new RelayCommand<object>(new Action<object>(MessageInputUrgencyCommand));
            Message.SendCommand = new RelayCommand<object>(new Action<object>(MessageInputSendCommand));

            InitializeComponent();
            BindingContext = this;


            EntryMessage.PropertyChanged += EntryMessage_PropertyChanged;

            EntryMessage.TextChanged += EntryMessage_TextChanged;
        }

        private void EntryMessage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Height")
            {
                // Need to force the layout of the parent ...
                ((Layout)this.Parent).ForceLayout();
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

            EntryMessage.Focus();

            // Need to force the layout of the parent ...
            ((Layout)this.Parent).ForceLayout();
        }


        private void MessageInputAttachmentCommand(object obj)
        {
            MessageAttachmentClicked?.Raise(this, null);
        }

        private void MessageInputUrgencyCommand(object obj)
        {
            MessageUrgencyClicked?.Raise(this, null);
        }

        private void MessageInputSendCommand(object obj)
        {
            if (EntryMessage.Text?.Length > 0)
                MessageSendClicked?.Raise(this, new IdEventArgs(EntryMessage.Text));
        }
    }
}