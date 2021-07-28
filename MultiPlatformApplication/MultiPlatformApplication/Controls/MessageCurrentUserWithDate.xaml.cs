using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
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
	public partial class MessageCurrentUserWithDate : ContentView
	{
        String messageId;
        String conversationId;
        String receipt;
        Boolean receiptManaged = false;

		public MessageCurrentUserWithDate ()
		{
			InitializeComponent ();

            this.BindingContextChanged += MessageCurrentUserWithDate_BindingContextChanged;
		}

        private void MessageCurrentUserWithDate_BindingContextChanged(object sender, EventArgs e)
        {
            if (BindingContext != null)
            {
                MessageElementModel message = (MessageElementModel)BindingContext;
                if ((message != null) && (message.ConversationType == Rainbow.Model.Conversation.ConversationType.User))
                {
                    if(!receiptManaged)
                    {
                        receiptManaged = true;

                        messageId = message.Id;
                        conversationId = message.ConversationId;
                        receipt = message.Receipt;

                        UpdateDisplay();

                        Helper.SdkWrapper.ReceiptReceived += SdkWrapper_ReceiptReceived;
                        Helper.SdkWrapper.MessagesAllRead += SdkWrapper_MessagesAllRead;
                    }
                }
            }
        }

        private void SdkWrapper_MessagesAllRead(object sender, Rainbow.Events.IdEventArgs e)
        {
            if (e.Id == conversationId)
            {
                receipt = Rainbow.Model.ReceiptType.ClientRead.ToString();
                UpdateDisplay();
            }
        }

        private void SdkWrapper_ReceiptReceived(object sender, Rainbow.Events.ReceiptReceivedEventArgs e)
        {
            if( (e.ConversationId == conversationId) && (e.MessageId == messageId))
            {
                receipt = e.ReceiptType.ToString();
                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                String imageSourceId = Helper.GetReceiptSourceIdFromReceiptType(receipt);
                Receipt.Source = Helper.GetImageSourceFromIdOrFilePath(imageSourceId);
                Receipt.IsVisible = true;
            });
        }
    }
}