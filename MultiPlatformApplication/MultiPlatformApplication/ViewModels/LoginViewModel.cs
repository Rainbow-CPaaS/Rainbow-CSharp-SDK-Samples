using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Rainbow.Model;

using MultiPlatformApplication.Effects;
using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;

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

            Helper.SdkWrapper.ConnectionStateChanged += RbApplication_ConnectionStateChanged;
            Helper.SdkWrapper.InitializationPerformed += RbApplication_InitializationPerformed;
        }

        public void Initialize()
        {
            LoginModel.IsBusy = false;
            LoginModel.Connect = Helper.SdkWrapper.GetLabel("continue");

            LoginModel.AskingLogin = true;

            LoginModel.LoginLabel = Helper.SdkWrapper.GetLabel("enterAuthID");
            LoginModel.PasswordLabel = Helper.SdkWrapper.GetLabel("enterAuthPwd");

            LoginModel.Login = Helper.SdkWrapper.GetUserLoginFromCache();
            LoginModel.Password = Helper.SdkWrapper.GetUserPasswordFromCache();
        }

        public void LoginFieldFocused()
        {
            LoginModel.AskingLogin = true;
            LoginModel.Connect = Helper.SdkWrapper.GetLabel("continue");
        }

        private void SetToBusy(bool busy)
        {
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => SetToBusy(busy));
                return;
            }

            if (busy)
            {
                if(LoginModel.AskingLogin)
                    LoginModel.Connect = Helper.SdkWrapper.GetLabel("continue");
                else
                    LoginModel.Connect = Helper.SdkWrapper.GetLabel("connecting");
                LoginModel.IsBusy = true;

                Popup.ShowDefaultActivityIndicator();
            }
            else
            {
                if (LoginModel.AskingLogin)
                    LoginModel.Connect = Helper.SdkWrapper.GetLabel("continue");
                else
                    LoginModel.Connect = Helper.SdkWrapper.GetLabel("connect");
                LoginModel.IsBusy = false;

                Popup.HideDefaultActivityIndicator();
            }

            // Update button display
            LoginModel.ButtonConnectCommand.RaiseCanExecuteChanged();
        }

        private void RbApplication_ConnectionStateChanged(object sender, Rainbow.Events.ConnectionStateEventArgs e)
        {
            switch (e.State)
            {
                case ConnectionState.Connected:
                case ConnectionState.Connecting:
                    SetToBusy(true);
                    break;

                case ConnectionState.Disconnected:
                    SetToBusy(false);
                    break;
            }
        }

        private void RbApplication_InitializationPerformed(object sender, EventArgs e)
        {
            Task task = new Task(() =>
            {
                // Get all conversations and all bubbles in parallel
                ManualResetEvent manualEventBubbles = new ManualResetEvent(false);
                ManualResetEvent manualEventConversations = new ManualResetEvent(false);

                Helper.SdkWrapper.GetAllConversations(callback =>
                {
                    manualEventConversations.Set();
                });

                Helper.SdkWrapper.GetAllBubbles(callback =>
                {
                    manualEventBubbles.Set();
                });

                // Wait all manual events before to continue
                WaitHandle.WaitAll(new WaitHandle[] { manualEventBubbles, manualEventConversations }, 5000);

                // No more in busy state
                SetToBusy(false);

                // Display conversations pages
                ShowMainPage();
            });
            task.Start();
        }

        async void ShowMainPage()
        {
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => ShowMainPage());
                return;
            }

            
            await XamarinApplication.NavigationService.ReplaceCurrentPageAsync("MainPage");
        }


        private void AskPassword()
        {
            if(!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => AskPassword());
                return;
            }

            SetToBusy(false);

            LoginModel.Connect = Helper.SdkWrapper.GetLabel("connect");

            LoginModel.AskingLogin = false;
        }

        private void StepAskingLogin()
        {
            SetToBusy(true);

            Task task = new Task(() =>
            {
                Helper.SdkWrapper.GetAuthenticationSSOUrls(LoginModel.Login, callback =>
                {
                    if (callback.Result.Success)
                    {
                        List<AuthenticationSSOUrl> urls = callback.Data;

                        // Do we have at least one URL ?
                        if ( (urls == null) || (urls?.Count == 0) )
                        {
                            AskPassword();
                            return;
                        }

                        // If we have several URLs, the first one different of RAINBOW is taken into account
                        AuthenticationSSOUrl authUrl = urls.FirstOrDefault(x => (x.Type.ToUpper() != "RAINBOW"));

                        // Do we have found one ?
                        if(authUrl == null)
                        {
                            AskPassword();
                            return;
                        }

                        // So we start SSO
                        Uri uri = new Uri(authUrl.LoginUrl);
                        Uri redirectUri = new Uri("rainbow://callback/");

                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            try
                            {
                                var authResult = await WebAuthenticator.AuthenticateAsync(uri, redirectUri);

                                if (authResult.Properties.ContainsKey("tkn"))
                                {
                                    String token = authResult.Properties["tkn"];
                                    Helper.SdkWrapper.LoginWithToken(token, callbackLoginToken =>
                                    {
                                        if(!callbackLoginToken.Result.Success)
                                            SetToBusy(false);
                                    });
                                }
                                else
                                    SetToBusy(false);
                            }
                            catch (Exception exc)
                            {
                                // ERROR OCCURS
                                SetToBusy(false);
                            }
                        });
                    }
                    else
                    {
                        SetToBusy(false);
                    }

                });
            });
            task.Start();
        }

        private void StepAskingPassword()
        {

        }

        private void ButtonConnectCommand(object obj)
        {
            if (Helper.SdkWrapper.ConnectionState() == ConnectionState.Disconnected)
            {
                if (LoginModel.AskingLogin)
                    StepAskingLogin();
                else
                    StepAskingPassword();

                
            }
            else
                Helper.SdkWrapper.Logout();
        }
    }
}
