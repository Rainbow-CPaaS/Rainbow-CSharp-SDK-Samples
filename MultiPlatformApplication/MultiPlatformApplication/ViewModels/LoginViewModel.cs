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
using MultiPlatformApplication.Views;

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
                ButtonConnectCommand = new RelayCommand<object>(new Action<object>(ButtonConnectCommand))
            };

            XamarinApplication = (App)Xamarin.Forms.Application.Current;

            XamarinApplication.SdkWrapper.ConnectionStateChanged += RbApplication_ConnectionStateChanged;
            XamarinApplication.SdkWrapper.InitializationPerformed += RbApplication_InitializationPerformed;
        }

        public void Initialize()
        {
            LoginModel.IsBusy = false;
            LoginModel.Connect = Helper.GetLabel("connect");

            LoginModel.Login = XamarinApplication.SdkWrapper.GetUserLoginFromCache();
            LoginModel.Password = XamarinApplication.SdkWrapper.GetUserPasswordFromCache();
        }

        private void RbApplication_ConnectionStateChanged(object sender, Rainbow.Events.ConnectionStateEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                switch (e.State)
                {
                    case ConnectionState.Connected:
                    case ConnectionState.Connecting:
                        LoginModel.Connect = LoginModel.Connect = Helper.GetLabel("connecting");
                        LoginModel.IsBusy = true;
                        break;

                    case ConnectionState.Disconnected:
                        LoginModel.Connect = LoginModel.Connect = Helper.GetLabel("connect");
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

                // Display conversations pages
                ShowConversationsPage();

            });
            task.Start();
        }

        void ShowConversationsPage()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await XamarinApplication.NavigationService.NavigateModalAsync("ConversationsPage");
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
    }
}
