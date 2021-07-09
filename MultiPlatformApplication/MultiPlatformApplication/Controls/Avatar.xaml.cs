using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Rainbow.Model;

using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using NLog;
using Rainbow;

namespace MultiPlatformApplication.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Avatar : ContentView
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(Avatar));

        PeerModel peer;
        Boolean manageAvatarDisplay = false;
        Boolean managePresenceDisplay = false;

        public Avatar()
        {
            InitializeComponent();

            this.BindingContextChanged += Avatar_BindingContextChanged;
        }

        private void Avatar_BindingContextChanged(object sender, EventArgs e)
        {
            if (BindingContext != null)
            {
                peer = (PeerModel)BindingContext;
                if ((peer != null) && (peer.Jid != null))
                {
                    ManageAvatarDisplay();
                    ManagePresenceDisplay();
                }
            }
        }
        private void ManageAvatarDisplay()
        {
            if (!manageAvatarDisplay)
            {
                manageAvatarDisplay = true;

                Helper.SdkWrapper.PeerAvatarUpdated += SdkWrapper_PeerAvatarUpdated;
                Helper.SdkWrapper.PeerAdded += SdkWrapper_PeerAdded;
                
            }
            UpdateAvatarImageDisplay();
        }

        private void SdkWrapper_PeerAdded(object sender, Rainbow.Events.PeerEventArgs e)
        {
            if (e.Peer.Jid == peer.Jid)
            {
                peer.Id = e.Peer.Id;
                ManageAvatarDisplay();
            }
        }

        private void ManagePresenceDisplay()
        {
            if (!managePresenceDisplay)
            {
                if ((!String.IsNullOrEmpty(peer?.Id)) && (peer.Type == Rainbow.Model.Conversation.ConversationType.User) )
                {
                    managePresenceDisplay = true;
                    Helper.SdkWrapper.ContactAggregatedPresenceChanged += SdkWrapper_ContactAggregatedPresenceChanged;
                    UpdatePresenceDisplay();
                }
            }
        }

        private void UpdateAvatarImageSource(String filePath)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Image.Source = ImageSource.FromFile(filePath);
            });
        }

        private void UpdatePresenceImageSource(String resource)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (resource == null)
                {
                    ImagePresence.Source = null;
                    FrameForPesence.IsVisible = false;
                }
                else
                {
                    ImagePresence.Source = ImageSource.FromResource(resource, typeof(Helper).Assembly);
                    FrameForPesence.IsVisible = true;
                }
            });
        }

        private void UpdateAvatarImageDisplay()
        {
            if (String.IsNullOrEmpty(peer.Id))
                return;

            if (peer.Type == Rainbow.Model.Conversation.ConversationType.User)
            {
                UpdateAvatarImageSource(Helper.GetContactAvatarFilePath(peer.Id));
            }
            else if (peer.Type == Rainbow.Model.Conversation.ConversationType.Room)
            {
                UpdateAvatarImageSource(Helper.GetBubbleAvatarFilePath(peer.Id));
            }
        }

        private void UpdatePresenceDisplay()
        {
            if (String.IsNullOrEmpty(peer?.Id))
                return;

            if ( (peer?.DisplayPresence == true) && (peer?.Type == Rainbow.Model.Conversation.ConversationType.User))
            {
                Presence presence = Helper.SdkWrapper.GetAggregatedPresenceFromContactId(peer.Id);

                if (presence == null)
                    UpdatePresenceImageSource(null);
                else
                    UpdatePresenceImageSource(Helper.GetPresenceSourceFromPresence(presence));
            }
        }

#region EVENTS FROM SDK WRAPPER

        private void SdkWrapper_PeerAvatarUpdated(object sender, Rainbow.Events.PeerEventArgs e)
        {
            if (peer.Jid == e.Peer.Jid)
            {
                peer.Id = e.Peer.Id;

                log.Debug("[SdkWrapper_PeerAvatarUpdated] Jid:[{0}]", peer.Jid);
                UpdateAvatarImageDisplay();
            }
        }

        private void SdkWrapper_ContactAggregatedPresenceChanged(object sender, Rainbow.Events.PresenceEventArgs e)
        {
            if ((peer?.DisplayPresence == true) && (peer?.Type == Rainbow.Model.Conversation.ConversationType.User) && (peer?.Jid == e.Jid))
            {
                UpdatePresenceDisplay();
            }
        }

    #endregion EVENTS FROM SDK WRAPPER

    }
}