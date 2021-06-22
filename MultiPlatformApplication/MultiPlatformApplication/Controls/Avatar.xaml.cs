using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Rainbow.Model;

using MultiPlatformApplication.Helpers;


namespace MultiPlatformApplication.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Avatar : Frame
    {
        
#region PeerIdProperty

        public static readonly BindableProperty PeerIdProperty =
            BindableProperty.Create(nameof(PeerId),
            typeof(String),
            typeof(Avatar),
            defaultValue: null,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: PeerIdChanged);

        private static void PeerIdChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (Avatar)bindable;
            UpdateAvatarImageDisplay(control);
            UpdatePresenceDisplay(control);
        }

        public String PeerId
        {
            get
            {
                var obj = base.GetValue(PeerIdProperty);
                if (obj is String)
                    return (String)obj;
                return null;
            }
            set
            {
                base.SetValue(PeerIdProperty, value);
            }
        }

#endregion PeerIdProperty
       
#region PeerTypeProperty

        public static readonly BindableProperty PeerTypeProperty =
            BindableProperty.Create(nameof(PeerType),
            typeof(String),
            typeof(Avatar),
            defaultValue: "user",
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: PeerTypeChanged);

        private static void PeerTypeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (Avatar)bindable;
            UpdateAvatarImageDisplay(control);
            UpdatePresenceDisplay(control);
        }

        public String PeerType
        {
            get
            {
                var obj = base.GetValue(PeerTypeProperty);
                if (obj is String)
                    return (String)obj;
                return null;
            }
            set
            {
                base.SetValue(PeerTypeProperty, value);
            }
        }

#endregion PeerTypeProperty

#region DisplayPresenceProperty

        public static readonly BindableProperty DisplayPresenceProperty =
            BindableProperty.Create(nameof(DisplayPresence),
            typeof(Boolean),
            typeof(Avatar),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: DisplayPresenceChanged);

        private static void DisplayPresenceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (Avatar)bindable;
            UpdatePresenceDisplay((Avatar)bindable);
        }

        public Boolean DisplayPresence
        {
            get
            {
                var obj = base.GetValue(DisplayPresenceProperty);
                if (obj is Boolean)
                    return (Boolean)obj;
                return false;
            }
            set
            {
                base.SetValue(DisplayPresenceProperty, value);
            }
        }

#endregion DisplayPresenceProperty

        private String peerJid;

        public Avatar()
        {
            InitializeComponent();

            Helper.SdkWrapper.ContactAvatarUpdated += SdkWrapper_ContactAvatarUpdated;
            Helper.SdkWrapper.ContactAggregatedPresenceChanged += SdkWrapper_ContactAggregatedPresenceChanged;

            Helper.SdkWrapper.BubbleAvatarUpdated += SdkWrapper_BubbleAvatarUpdated;
        }

        private static void UpdateAvatarImageDisplay(Avatar control)
        {
            if (String.IsNullOrEmpty(control.PeerId))
                return;

            if ( (control.PeerType == "user") && (!String.IsNullOrEmpty(control.PeerId)) )
            {
                control.Image.Source = ImageSource.FromFile(Helper.GetContactAvatarFilePath(control.PeerId));

                // Need to store peerJid if it's a user to manage presence update
                Contact contact = Helper.SdkWrapper.GetContactFromContactId(control.PeerId);
                control.peerJid = contact?.Jid_im;

            }
            else if (control.PeerType == "room")
                control.Image.Source = ImageSource.FromFile(Helper.GetBubbleAvatarFilePath(control.PeerId));
        }

        private static void UpdatePresenceDisplay(Avatar control)
        {
            Boolean displayFrameForPresence = false;
            if (control.DisplayPresence)
            {

                if ((control.PeerType == "user") && (!String.IsNullOrEmpty(control.PeerId)))
                {
                    Presence presence = Helper.SdkWrapper.GetAggregatedPresenceFromContactId(control.PeerId);

                    if (presence == null)
                    {
                        control.ImagePresence.Source = null;
                        displayFrameForPresence = false;
                    }
                    else
                    {
                        control.ImagePresence.Source = ImageSource.FromResource(Helper.GetPresenceSourceFromPresence(presence));
                        displayFrameForPresence = true;
                    }
                }
            }

            control.FrameForPesence.IsVisible = displayFrameForPresence;

        }

#region EVENTS FROM SDK WRAPPER
        private void SdkWrapper_ContactAggregatedPresenceChanged(object sender, Rainbow.Events.PresenceEventArgs e)
        {
            if (DisplayPresence && (PeerType == "user") && (peerJid == e.Jid))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    UpdatePresenceDisplay(this);
                });
            }
        }

        private void SdkWrapper_ContactAvatarUpdated(object sender, Rainbow.Events.IdEventArgs e)
        {
            if ((PeerType == "user") && (PeerId == e.Id))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    UpdateAvatarImageDisplay(this);
                });
            }
        }

        private void SdkWrapper_BubbleAvatarUpdated(object sender, Rainbow.Events.IdEventArgs e)
        {
            if ((PeerType == "room") && (PeerId == e.Id))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    UpdateAvatarImageDisplay(this);
                });
            }
        }

#endregion EVENTS FROM SDK WRAPPER

    }
}