using System;
using System.IO;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Rainbow;
using Rainbow.Events;
using Rainbow.Model;

using InstantMessaging.Helpers;


namespace InstantMessaging
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoginPage : ContentPage
    {
        InstantMessaging.App XamarinApplication;

        public LoginPage ()
		{
			InitializeComponent ();

            XamarinApplication = (InstantMessaging.App) Xamarin.Forms.Application.Current;

            // Set default values for login / pwd
            EntryLogin.Text = XamarinApplication.LOGIN_USER1;
            EntryPassword.Text = XamarinApplication.PASSWORD_USER1;

            XamarinApplication.RbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;
            XamarinApplication.RbApplication.InitializationPerformed += RbApplication_InitializationPerformed;

            XamarinApplication.RbContacts.ContactPresenceChanged += RbContacts_ContactPresenceChanged;
        }

        private void RbContacts_ContactPresenceChanged(object sender, PresenceEventArgs e)
        {
            string presence = e.Presence.PresenceLevel + (String.IsNullOrEmpty(e.Presence.PresenceDetails) ? "" : $" ({e.Presence.PresenceDetails})");

            if (Util.GetJidNode(e.Jid) == XamarinApplication.JID_NODE_USER1)
            {
                ViewModel.Info = ($"Current User - Presence level changed [{presence}]");
            }
            else
            {
                Contact contact = XamarinApplication.RbContacts.GetContactFromContactJid(e.Jid);
                if (contact != null)
                {
                    string name = contact.LastName + " " + contact.FirstName;
                    ViewModel.Info = ($"[{name}] - Presence level changed [{presence}]");
                }
            }
        }

        private void ButtonConnect_Clicked(object sender, EventArgs e)
        {
            if (XamarinApplication.RbApplication.ConnectionState() == ConnectionState.Disconnected)
            {
                /*IPEndPoint ipEndPoint;
                var addresses = Dns.GetHostAddresses(XamarinApplication.HOST_NAME);
                ipEndPoint = new IPEndPoint(addresses[0], 0);

                ViewModel.Info = addresses[0].ToString();

                XamarinApplication.RbApplication.SetIpEndPoint(ipEndPoint);*/

                XamarinApplication.RbApplication.Login(XamarinApplication.LOGIN_USER1, XamarinApplication.PASSWORD_USER1, callback =>
                {
                    ViewModel.Info = Util.SerialiseSdkError(callback.Result);
                });
            }
            else
                XamarinApplication.RbApplication.Logout();
        }

        void ShowConversationsPage()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (XamarinApplication.ConversationsPage == null)
                    XamarinApplication.ConversationsPage = new ConversationsPage();

                await Navigation.PushAsync(XamarinApplication.ConversationsPage, false);
                Navigation.RemovePage(this);
            });
            
        }

        private void RbApplication_ConnectionStateChanged(object sender, Rainbow.Events.ConnectionStateEventArgs e)
        {
            switch (e.State)
            {
                case ConnectionState.Connected:
                    ViewModel.Status = "Connected";
                    ViewModel.Connect = "Disconnect";
                    break;

                case ConnectionState.Connecting:
                    ViewModel.Status = "Connecting";
                    ViewModel.Connect = "Connecting";
                    break;

                case ConnectionState.Disconnected:
                    ViewModel.Status = "Disconnected";
                    ViewModel.Connect = "Connect";
                    ViewModel.Info = " ";
                    break;
            }
        }

        private void RbApplication_InitializationPerformed(object sender, EventArgs e)
        {
            ShowConversationsPage();
        }
    }
}