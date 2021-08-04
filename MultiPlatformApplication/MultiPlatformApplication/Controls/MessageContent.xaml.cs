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
        private MessageElementModel message = null;

        MessageContentUrgency messageContentUrgency = null;
        MessageContentReply messageContentReply = null;
        MessageContentBody messageContentBody = null;
        MessageContentBodyWithImg messageContentBodyWithImg = null;
        MessageContentBodyWithCode messageContentBodyWithCode = null;
        MessageContentAttachment messageContentAttachment = null;
        MessageContentDeleted messageContentDeleted = null;

        public MessageContent()
        {
            InitializeComponent();
        }


        protected override void OnBindingContextChanged()
        {
            if( (BindingContext != null) && (message == null) )
            {
                message = (MessageElementModel)BindingContext;
                if(message != null)
                {
                    String backgroundColorKey;
                    if (message.Peer.Id == Helper.SdkWrapper.GetCurrentContactId())
                        backgroundColorKey = "ColorConversationStreamMessageCurrentUserBackGround";
                    else
                        backgroundColorKey = "ColorConversationStreamMessageOtherUserBackGround";
                    Frame.BackgroundColor = Helper.GetResourceDictionaryById<Color>(backgroundColorKey);


                    // Add Urgency element ?
                    if (message.Content.Urgency != UrgencyType.Std)
                    {
                        if (messageContentUrgency == null)
                        {
                            messageContentUrgency = new MessageContentUrgency();
                            messageContentUrgency.BindingContext = message;
                            MainGrid.Children.Add(messageContentUrgency, 0, 0);
                        }
                    }

                    // Add Reply element ?
                    if (message.Reply != null)
                    {
                        if (messageContentReply == null)
                        {
                            messageContentReply = new MessageContentReply();
                            messageContentReply.BindingContext = message;
                            MainGrid.Children.Add(messageContentReply, 0, 1);
                        }
                    }

                    // Add Body element ?
                    if (!String.IsNullOrEmpty(message.Content?.Body))
                    {
                        if(message.Content.Body.StartsWith("/img", StringComparison.InvariantCultureIgnoreCase) )
                        {
                            if (messageContentBodyWithImg == null)
                            {
                                messageContentBodyWithImg = new MessageContentBodyWithImg();
                                messageContentBodyWithImg.BindingContext = message;
                                MainGrid.Children.Add(messageContentBodyWithImg, 0, 2);
                            }
                        }
                        else if (message.Content.Body.StartsWith("/code", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (messageContentBodyWithCode == null)
                            {
                                messageContentBodyWithCode = new MessageContentBodyWithCode();
                                messageContentBodyWithCode.BindingContext = message;
                                MainGrid.Children.Add(messageContentBodyWithCode, 0, 2);
                            }
                        }
                        else if (messageContentBody == null)
                        {
                            messageContentBody = new MessageContentBody();
                            messageContentBody.BindingContext = message;
                            MainGrid.Children.Add(messageContentBody, 0, 2);
                        }
                    }
                    else if(message.Content.Type == "deletedMessage")
                    {
                        // Create MessageContentDeleted
                        messageContentDeleted = new MessageContentDeleted();
                        messageContentDeleted.BindingContext = message;
                        MainGrid.Children.Add(messageContentDeleted, 0, 2);
                    }

                    // Add Attachment element ?
                    if (message.Content.Attachment != null)
                    {
                        if (messageContentAttachment == null)
                        {
                            messageContentAttachment = new MessageContentAttachment();
                            messageContentAttachment.BindingContext = message;
                            MainGrid.Children.Add(messageContentAttachment, 0, 3);
                        }
                    }
                }
            }
        }
    }
}