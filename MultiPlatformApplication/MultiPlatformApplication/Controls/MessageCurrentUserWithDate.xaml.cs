﻿using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using Rainbow;
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
	public partial class MessageCurrentUserWithDate : ContentView
	{
        public event EventHandler<EventArgs> ActionMenuToDisplay;

        String messageId;
        String conversationId;
        String receipt;
        String receiptReceived;
        Boolean displayReceipt = false;
        Boolean receiptEventManaged = false;

		public MessageCurrentUserWithDate ()
		{
			InitializeComponent ();

            this.BindingContextChanged += MessageCurrentUserWithDate_BindingContextChanged;

            MessageContent.ActionMenuToDisplay += MessageContent_ButtonActionUsed;
        }

        private void MessageContent_ButtonActionUsed(object sender, EventArgs e)
        {
            ActionMenuToDisplay?.Raise(sender, e);
        }

        private void MessageCurrentUserWithDate_BindingContextChanged(object sender, EventArgs e)
        {
            if (BindingContext != null)
            {
                MessageElementModel message = (MessageElementModel)BindingContext;
                if ((message != null) && (message.ConversationType == Rainbow.Model.Conversation.ConversationType.User))
                {
                    if(!receiptEventManaged)
                    {
                        receiptEventManaged = true;

                        messageId = message.Id;
                        conversationId = message.ConversationId;
                        receipt = message.Receipt;
                        displayReceipt = message.NeedReceipt; // Do we have to display receipt ?

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
                receiptReceived = receipt = Rainbow.Model.ReceiptType.ClientRead.ToString();
                UpdateDisplay();
            }
        }

        private void SdkWrapper_ReceiptReceived(object sender, Rainbow.Events.ReceiptReceivedEventArgs e)
        {
            if( (e.ConversationId == conversationId) && (e.MessageId == messageId))
            {
                receiptReceived = e.ReceiptType.ToString();
                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            // Ensure to be on Main UI Thread
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => UpdateDisplay());
                return;
            }

            String imageSourceId = Helper.GetReceiptSourceIdFromReceiptType(receipt, receiptReceived);
            if (imageSourceId != null)
            {
                receipt = receiptReceived;
                Receipt.Source = Helper.GetImageSourceFromIdOrFilePath(imageSourceId);
                Receipt.IsVisible = displayReceipt;
            }
        }
    }
}