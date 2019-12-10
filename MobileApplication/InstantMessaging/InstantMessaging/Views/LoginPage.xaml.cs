using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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

            ViewModel.Connect = "Connect";
            ViewModel.IsBusy = false;

            XamarinApplication = (InstantMessaging.App) Xamarin.Forms.Application.Current;

            // Set default values for login / pwd
            EntryLogin.Text = XamarinApplication.LOGIN_USER1;
            EntryPassword.Text = XamarinApplication.PASSWORD_USER1;

            XamarinApplication.RbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;
            XamarinApplication.RbApplication.InitializationPerformed += RbApplication_InitializationPerformed;
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

                ViewModel.IsBusy = true;

                XamarinApplication.RbApplication.Login(XamarinApplication.LOGIN_USER1, XamarinApplication.PASSWORD_USER1, callback =>
                {
                    //TODO - manage error
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
                    ViewModel.Connect = "Connecting";
                    break;

                case ConnectionState.Connecting:
                    ViewModel.Connect = "Connecting";
                    break;

                case ConnectionState.Disconnected:
                    ViewModel.Connect = "Connect";
                    break;
            }
        }

        private void RbApplication_InitializationPerformed(object sender, EventArgs e)
        {
            Task task = new Task(() =>
            {
                ManualResetEvent manualEventBubbles = new ManualResetEvent(false);
                ManualResetEvent manualEventConversations = new ManualResetEvent(false);

                XamarinApplication.RbApplication.GetConversations().GetAllConversations(callback =>
                {
                    manualEventConversations.Set();
                });

                XamarinApplication.RbApplication.GetBubbles().GetAllBubbles(callback =>
                {
                    manualEventBubbles.Set();
                });

                // Wait all manual events before to continue
                WaitHandle.WaitAll(new WaitHandle[] { manualEventBubbles, manualEventConversations }, 5000);
                
                ShowConversationsPage();
            });
            task.Start();
        }

    }
}