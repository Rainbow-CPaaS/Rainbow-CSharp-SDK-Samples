using MultiPlatformApplication.Effects;
using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using Rainbow.Model;
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
    public partial class MessageContent : ContentView
    {
        public static double MINIMAL_MESSAGE_WIDTH = 120;

        private MessageElementModel message = null;

        private MessageContentUrgency messageContentUrgency = null;
        private MessageContentReply messageContentReply = null;
        private MessageContentBody messageContentBody = null;
        private MessageContentBodyWithImg messageContentBodyWithImg = null;
        private MessageContentBodyWithCode messageContentBodyWithCode = null;
        private MessageContentAttachment messageContentAttachment = null;
        private MessageContentDeleted messageContentDeleted = null;
        private MessageContentForward messageContentForward = null;

        // Define commands used for  Mouse Over / Mouse Out purpose
        public MouseOverAndOutModel mouseCommands { get; set; }
        private Boolean needActionButton = false;

        public MessageContent()
        {
            InitializeComponent();

            // Add Mouse out/oVer management only for some platforms
            if ((Device.RuntimePlatform == Device.UWP)
                || (Device.RuntimePlatform == Device.WPF)
                || (Device.RuntimePlatform == Device.macOS))
            {
                needActionButton = true;

                mouseCommands = new MouseOverAndOutModel();
                mouseCommands.MouseOverCommand = new RelayCommand<object>(new Action<object>(MouseOverCommand));
                mouseCommands.MouseOutCommand = new RelayCommand<object>(new Action<object>(MouseOutCommand));

                //Add mouse over effect
                MouseOverEffect.SetCommand(RootGrid, mouseCommands.MouseOverCommand);

                //Add mouse out effect
                MouseOutEffect.SetCommand(RootGrid, mouseCommands.MouseOutCommand);

                BtnAction.Command = new RelayCommand<object>(new Action<object>(BtnActionCommand));
            }

            BowViewForMinimalWidth.WidthRequest = MINIMAL_MESSAGE_WIDTH;
        }

        private void BtnActionCommand(object obj)
        {
            // TODO
        }

        private void MouseOverCommand(object obj)
        {
            if (needActionButton)
            {
                BtnAction.IsVisible = true;
            }
        }

        private void MouseOutCommand(object obj)
        {
            if (needActionButton)
            {
                BtnAction.IsVisible = false;
            }
        }

        protected override void OnBindingContextChanged()
        {
            if ((BindingContext != null) && (message == null))
            {
                message = (MessageElementModel)BindingContext;
                if (message != null)
                {
                    Color color;
                    Color backgroundColor;

                    if (message.Peer.Id == Helper.SdkWrapper.GetCurrentContactId())
                    {
                        color = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageCurrentUserFont");
                        backgroundColor = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageCurrentUserBackGround");
                    }
                    else
                    {
                        color = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageOtherUserFont");
                        backgroundColor = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageOtherUserBackGround");
                    }

                    Frame.BackgroundColor = backgroundColor;
                    if (needActionButton)
                    {
                        BtnAction.ImageSourceId = "Font_EllipsisV|" + color.ToHex();
                        BtnAction.BackgroundColor = backgroundColor;
                        BtnAction.BackgroundColorOnMouseOver = ColorInterpolator.InterpolateBetween(color, backgroundColor, 0.5);
                    }

                    // Add Urgency element ?
                    if (message.Content.Urgency != UrgencyType.Std)
                    {
                        if (messageContentUrgency == null)
                        {
                            messageContentUrgency = new MessageContentUrgency();
                            messageContentUrgency.BindingContext = message;
                            HeaderGrid.Children.Add(messageContentUrgency, 0, 0);

                            // Need to define here the BackgroundColor of the HeaderGrid to ensure to have correct display if the message body is short
                            Helper.GetUrgencyInfo(message.Content.Urgency.ToString(), out backgroundColor, out color, out String title, out String label, out String imageSourceId);
                            HeaderGrid.BackgroundColor = backgroundColor;
                        }
                    }

                    // Add forward header
                    if (message.IsForwarded)
                    {
                        if (messageContentForward == null)
                        {
                            messageContentForward = new MessageContentForward();
                            messageContentForward.BindingContext = message;
                            HeaderGrid.Children.Add(messageContentForward, 0, 0);
                        }
                    }

                    // Add Reply element ?
                    if (message.Reply != null)
                    {
                        if (messageContentReply == null)
                        {
                            messageContentReply = new MessageContentReply();
                            messageContentReply.BindingContext = message;
                            HeaderGrid.Children.Add(messageContentReply, 0, 1);
                        }
                    }

                    // Add Body element ?
                    if (!String.IsNullOrEmpty(message.Content?.Body))
                    {
                        if (message.Content.Body.StartsWith("/img", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (messageContentBodyWithImg == null)
                            {
                                messageContentBodyWithImg = new MessageContentBodyWithImg();
                                messageContentBodyWithImg.BindingContext = message;
                                ContentGrid.Children.Add(messageContentBodyWithImg, 0, 0);
                            }
                        }
                        else if (message.Content.Body.StartsWith("/code", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (messageContentBodyWithCode == null)
                            {
                                messageContentBodyWithCode = new MessageContentBodyWithCode();
                                messageContentBodyWithCode.BindingContext = message;
                                ContentGrid.Children.Add(messageContentBodyWithCode, 0, 0);
                            }
                        }
                        else if (messageContentBody == null)
                        {
                            messageContentBody = new MessageContentBody();
                            messageContentBody.BindingContext = message;
                            ContentGrid.Children.Add(messageContentBody, 0, 0);
                        }
                    }
                    else if (message.Content.Type == "deletedMessage")
                    {
                        // We don't need action button in this case
                        needActionButton = false;

                        // Create MessageContentDeleted
                        messageContentDeleted = new MessageContentDeleted();
                        messageContentDeleted.BindingContext = message;
                        ContentGrid.Children.Add(messageContentDeleted, 0, 0);
                    }

                    // Add Attachment element ?
                    if (message.Content.Attachment != null)
                    {
                        if (messageContentAttachment == null)
                        {
                            messageContentAttachment = new MessageContentAttachment();
                            messageContentAttachment.BindingContext = message;
                            ContentGrid.Children.Add(messageContentAttachment, 0, 1);
                        }
                    }
                }
            }
        }
    }
}