using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Rainbow.Model;

using MultiPlatformApplication.Models;
using MultiPlatformApplication.Helpers;


namespace MultiPlatformApplication.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class LoginViewModel
    {
        private App XamarinApplication;

        public LoginModel LoginModel { get; set; }

        public LoginViewModel()
        {
            LoginModel = new LoginModel()
            {
                ButtonConnectCommand = new RelayCommand<object>(new Action<object>(ButtonConnectCommand), new Predicate<object>(ButtonConnectCommandCanExecute))
            };

            XamarinApplication = (App)Xamarin.Forms.Application.Current;

            XamarinApplication.SdkWrapper.ConnectionStateChanged += RbApplication_ConnectionStateChanged;
            XamarinApplication.SdkWrapper.InitializationPerformed += RbApplication_InitializationPerformed;
        }

        public void Initialize()
        {
            LoginModel.IsBusy = false;
            LoginModel.Connect = "Connect";

            LoginModel.Login = XamarinApplication.SdkWrapper.GetUserLoginFromCache();
            LoginModel.Password = XamarinApplication.SdkWrapper.GetUserPasswordFromCache();

            // Update button display
            LoginModel.ButtonConnectCommand.RaiseCanExecuteChanged();
        }

        private void RbApplication_ConnectionStateChanged(object sender, Rainbow.Events.ConnectionStateEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                switch (e.State)
                {
                    case ConnectionState.Connected:
                    case ConnectionState.Connecting:
                        LoginModel.Connect = "Connecting";
                        LoginModel.IsBusy = true;
                        break;

                    case ConnectionState.Disconnected:
                        LoginModel.Connect = "Connect";
                        LoginModel.IsBusy = false;
                        break;
                }

                // Update button display
                LoginModel.ButtonConnectCommand.RaiseCanExecuteChanged();
            });
        }

        private void RbApplication_InitializationPerformed(object sender, EventArgs e)
        {
            Task task = new Task(() =>
            {

                // Get all conversations and all bubbles in parallel

                ManualResetEvent manualEventBubbles = new ManualResetEvent(false);
                ManualResetEvent manualEventConversations = new ManualResetEvent(false);

                XamarinApplication.SdkWrapper.GetAllConversations(callback =>
                {
                    manualEventConversations.Set();
                });

                XamarinApplication.SdkWrapper.GetAllBubbles(callback =>
                {
                    manualEventBubbles.Set();
                });

                // Wait all manual events before to continue
                WaitHandle.WaitAll(new WaitHandle[] { manualEventBubbles, manualEventConversations }, 5000);

                // Updte display 
                Device.BeginInvokeOnMainThread(() =>
                {
                    LoginModel.Connect = "Disconnect";
                    LoginModel.IsBusy = false;

                    // Update button display
                    LoginModel.ButtonConnectCommand.RaiseCanExecuteChanged();
                });

                // Display conversations pages
                ShowConversationsPage();

            });
            task.Start();
        }

        void ShowConversationsPage()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (XamarinApplication.ConversationsPage == null)
                    XamarinApplication.ConversationsPage = new ConversationsPage();

                App.Current.MainPage.Navigation.PushAsync(XamarinApplication.ConversationsPage, false);
                //App.Current.MainPage.Navigation.PushModalAsync(XamarinApplication.ConversationsPage, false);
            });
        }

        public void ButtonConnectCommand(object obj)
        {

            if (XamarinApplication.SdkWrapper.ConnectionState() == ConnectionState.Disconnected)
            {
                XamarinApplication.SdkWrapper.Login(LoginModel.Login, LoginModel.Password, callback =>
                {
                    //TODO - manage error
                });
            }
            else
                XamarinApplication.SdkWrapper.Logout();
        }

        public Boolean ButtonConnectCommandCanExecute(object obj)
        {
            return !LoginModel.IsBusy;
        }
    }
}
