using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using Rainbow;
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
	public partial class MessageSeparatorDisplayName : ContentView
	{
        CancelableDelay TaskCheckCacheWithJid = null;

        PeerModel peer;
        Boolean manageDisplay = false;

        public MessageSeparatorDisplayName ()
		{
			InitializeComponent();

            this.BindingContextChanged += MessageSeparatorDisplayName_BindingContextChanged;
		}

        private void MessageSeparatorDisplayName_BindingContextChanged(object sender, EventArgs e)
        {
            if (BindingContext != null)
            {
                MessageElementModel message = (MessageElementModel)BindingContext;
                if ( message?.Peer?.Jid != null )
                {
                    peer = message.Peer;
                    ManageDisplay();
                }
            }
        }

        private void ManageDisplay()
        {
            if (!manageDisplay)
            {
                if (peer.Type == Rainbow.Model.Conversation.ConversationType.User)
                {
                    manageDisplay = true;

                    Helper.SdkWrapper.PeerInfoChanged += SdkWrapper_PeerInfoChanged;
                    Helper.SdkWrapper.PeerAdded += SdkWrapper_PeerInfoChanged;

                    UpdateDisplay();
                }
            }
        }



        private void CheckCacheWithJid()
        {
            var contact = Helper.SdkWrapper.GetContactFromContactJid(peer.Jid);
            if (contact != null)
            {
                peer.Id = contact.Id;
                UpdateDisplay();
                return;
            }

            if (TaskCheckCacheWithJid == null)
                TaskCheckCacheWithJid = CancelableDelay.StartAfter(300, () => CheckCacheWithJid());
            else
                TaskCheckCacheWithJid.PostPone();
        }


        private void UpdateDisplay()
        {
            if (String.IsNullOrEmpty(peer.Id))
            {
                // Event PeerAdded could have been raised too soon. It's why we can have a PeerId null.
                // So we have to check the cache
                CheckCacheWithJid();
                return;
            }

            // Ensure to be on Main UI Thread
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => UpdateDisplay());
                return;
            }

            String displayName = Helper.GetContactDisplayName(peer.Jid);
            Label.Text = displayName;
        }

         private void SdkWrapper_PeerInfoChanged(object sender, Rainbow.Events.PeerEventArgs e)
        {
            if (e.Peer.Jid == peer.Jid)
                UpdateDisplay();
        }

    }
}