using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;

using Rainbow.Model;
using Xamarin.Essentials;

namespace MultiPlatformApplication.Controls
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MessageSeparatorEvent : ContentView
	{
        String body;
        Boolean isCallLogEvent = false;

        String  peerJid;
        CallLogAttachment callLogAttachment;


        Boolean manageDisplay = false;

        public MessageSeparatorEvent ()
		{
			InitializeComponent ();

            this.BindingContextChanged += MessageSeparatorEvent_BindingContextChanged;
		}

        private void MessageSeparatorEvent_BindingContextChanged(object sender, EventArgs e)
        {
            if (BindingContext != null)
            {
                MessageElementModel message = (MessageElementModel)BindingContext;
                if ((message != null) && (message.Peer != null) && (message.Peer.Type == Rainbow.Model.Conversation.ConversationType.User))
                {
                    peerJid = message.Peer.Jid;
                    body = message.Content.Body;
                    if (message.CallLogAttachment != null)
                    {
                        String currentContactJid = Helper.SdkWrapper.GetCurrentContactJid();
                        callLogAttachment = message.CallLogAttachment;
                        isCallLogEvent = true;

                        // Store PeerJid
                        if (callLogAttachment.Callee != currentContactJid)
                            peerJid = callLogAttachment.Callee;
                        else
                            peerJid = callLogAttachment.Caller;

                    }
                    ManageDisplay();
                }
            }
        }

        private void ManageDisplay()
        {
            if (!manageDisplay)
            {
                manageDisplay = true;

                Helper.SdkWrapper.PeerInfoChanged += SdkWrapper_PeerInfoChanged;
                Helper.SdkWrapper.PeerAdded += SdkWrapper_PeerInfoChanged;

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

            if (String.IsNullOrEmpty(peerJid))
                return;

            Rainbow.Model.Contact contact = Helper.SdkWrapper.GetContactFromContactJid(peerJid);
            if (contact != null)
            {
                if(isCallLogEvent)
                    Label.Text = Helper.GetCallLogEventMessageBody(callLogAttachment);
                else
                    Label.Text = Helper.GetBubbleEventMessageBody(contact, body);
            }
        }

        private void SdkWrapper_PeerInfoChanged(object sender, Rainbow.Events.PeerEventArgs e)
        {
            if (e.Peer.Jid == peerJid)
            {
                UpdateDisplay();
            }
        }
    }
}