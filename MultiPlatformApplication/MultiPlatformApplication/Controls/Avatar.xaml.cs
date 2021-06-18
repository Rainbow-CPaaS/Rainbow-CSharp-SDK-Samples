using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
            //var control = (Avatar)bindable;
            //TODO
        }

        public String DisplayPresence
        {
            get
            {
                var obj = base.GetValue(DisplayPresenceProperty);
                if (obj is String)
                    return (String)obj;
                return null;
            }
            set
            {
                base.SetValue(DisplayPresenceProperty, value);
            }
        }

#endregion DisplayPresenceProperty

        private SdkWrapper SdkWrapper;

        public Avatar()
        {
            InitializeComponent();

            // Get SdkWrapper
            App XamarinApplication = (App)Xamarin.Forms.Application.Current;
            SdkWrapper = XamarinApplication.SdkWrapper;

            SdkWrapper.ContactAvatarUpdated += SdkWrapper_ContactAvatarUpdated;
            SdkWrapper.ContactPresenceChanged += SdkWrapper_ContactPresenceChanged;

            SdkWrapper.BubbleAvatarUpdated += SdkWrapper_BubbleAvatarUpdated;
        }

        private static void UpdateAvatarImageDisplay(Avatar control)
        {
            if (String.IsNullOrEmpty(control.PeerId))
                return;

            if(control.PeerType == "user")
                control.Image.Source = ImageSource.FromFile(Helper.GetContactAvatarFilePath(control.PeerId));
            else if (control.PeerType == "room")
                control.Image.Source = ImageSource.FromFile(Helper.GetBubbleAvatarFilePath(control.PeerId));
        }

        private static void UpdatePresenceDisplay(Avatar avatar)
        {

        }

#region EVENTS FROM SDK WRAPPER

        private void SdkWrapper_ContactPresenceChanged(object sender, Rainbow.Events.PresenceEventArgs e)
        {
            //TODO
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