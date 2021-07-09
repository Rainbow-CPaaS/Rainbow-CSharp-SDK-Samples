using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using Rainbow;
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
	public partial class MessageSeparatorDisplayName : ContentView
	{
        PeerModel peer;
        Boolean manageDisplay = false;

        String displayName;

        public MessageSeparatorDisplayName ()
		{
			InitializeComponent();

            this.BindingContextChanged += MessageSeparatorDisplayName_BindingContextChanged;
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

                    CheckPeerId();
                }
            }
        }

        private void MessageSeparatorDisplayName_BindingContextChanged(object sender, EventArgs e)
        {
            if (BindingContext != null)
            {
                MessageElementModel message = (MessageElementModel)BindingContext;
                if ( (message != null) && (message.Peer != null) )
                {
                    peer = message.Peer;
                    displayName = peer.DisplayName;
                    ManageDisplay();
                    
                }
            }
        }

        private void CheckPeerId()
        {
            if (String.IsNullOrEmpty(peer?.Jid))
                return;

            Contact contact = Helper.SdkWrapper.GetContactFromContactJid(peer?.Jid);
            if (contact != null)
            {
                displayName = contact.DisplayName;
                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Label.Text = displayName;
            });
        }

         private void SdkWrapper_PeerInfoChanged(object sender, Rainbow.Events.PeerEventArgs e)
        {
            if (e.Peer.Jid == peer?.Jid)
            {
                peer.Id = e.Peer.Id;
                displayName = e.Peer.DisplayName;
                UpdateDisplay();
            }
        }

    }
}