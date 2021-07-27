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

namespace MultiPlatformApplication.Controls
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MessageSeparatorEvent : ContentView
	{
        PeerModel peer;
        String body;
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
                if ((message != null) && (message.Peer != null))
                {
                    peer = message.Peer;
                    body = message.Content.Body;
                    ManageDisplay();
                }
            }
        }

        private void ManageDisplay()
        {
            if (!manageDisplay)
            {
                if (peer?.Type == Rainbow.Model.Conversation.ConversationType.User)
                {
                    manageDisplay = true;

                    Helper.SdkWrapper.PeerInfoChanged += SdkWrapper_PeerInfoChanged;
                    Helper.SdkWrapper.PeerAdded += SdkWrapper_PeerInfoChanged;

                    UpdateDisplay();
                }
            }
        }


        private void UpdateDisplay()
        {
            if (String.IsNullOrEmpty(peer?.Jid))
                return;

            Contact contact = Helper.SdkWrapper.GetContactFromContactJid(peer?.Jid);
            if (contact != null)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Label.Text = Helper.GetBubbleEventMessageBody(contact, body);
                });
            }
        }

        private void SdkWrapper_PeerInfoChanged(object sender, Rainbow.Events.PeerEventArgs e)
        {
            if (e.Peer.Jid == peer?.Jid)
            {
                peer.Id = e.Peer.Id;
                UpdateDisplay();
            }
        }
    }
}