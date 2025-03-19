using System;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using Rainbow.Model;

using WpfSSOSamples.Helpers;
using WpfSSOSamples.Model;

using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WpfWebView;
using Rainbow.Consts;

namespace WpfSSOSamples.ViewModel
{
    public class LoginViewModel : ObservableObject
    {
        App CurrentApplication = (App)System.Windows.Application.Current;

        public LoginModel Model { get; private set; } // Need to be public - Used as Binding from XAML

        private EmbeddedBrowser browser = null;
        private Boolean SSOInProgress = false;

    #region PROPERTIES

        BitmapImage m_logo;
        public BitmapImage Logo
        {
            get { return m_logo; }
            set { SetProperty(ref m_logo, value); }
        }

        ICommand m_ButtonLoginCommand;
        public ICommand ButtonLoginCommand
        {
            get { return m_ButtonLoginCommand; }
            set { m_ButtonLoginCommand = value; }
        }

        ICommand m_EntryLoginLostFocusCommand;
        public ICommand EntryLoginLostFocusCommand
        {
            get { return m_EntryLoginLostFocusCommand; }
            set { m_EntryLoginLostFocusCommand = value; }
        }

        ICommand m_EntryLoginGotFocusCommand;
        public ICommand EntryLoginGotFocusCommand
        {
            get { return m_EntryLoginGotFocusCommand; }
            set { m_EntryLoginGotFocusCommand = value; }
        }

    #endregion PROPERTIES

    #region CONSTRUCTOR
        public LoginViewModel()
        {
            // Init / Define properties not managed by the Model Object
            Logo = Helper.GetBitmapImageFromResource("splash_logo.png");
            
            m_ButtonLoginCommand = new RelayCommand<object>(new Action<object>(CommandButtonLogin));

            m_EntryLoginLostFocusCommand = new RelayCommand<object>(new Action<object>(CommandEntryLoginLostFocus));

            m_EntryLoginGotFocusCommand = new RelayCommand<object>(new Action<object>(CommandEntryLoginGotFocus));

            // Create Model object associated to the current ViewModel object
            Model = new LoginModel();

            Init();
        }
    #endregion CONSTRUCTOR


    #region PUBLIC METHODS
        public void ViewGotFocus()
        {
            if (SSOInProgress)
            {
                browser?.Show();
                browser?.Activate();
            }
        }

        public void ViewClosing()
        {
            browser?.Close();
        }

    #endregion PUBLIC METHODS

    #region PRIVATE METHODS
        private void Init()
        {
            // Set default values
            Model.IsBusy = false;
            Model.AskingPassword = false;
            Model.Connect = "Continue";
            SetErrorMessage(null);

            // Get Login / Password from cache (i.e. INI FILE)
            Model.Login = CurrentApplication.RbApplication.GetUserLogin();
            Model.Password = CurrentApplication.RbApplication.GetUserPassword();

            CurrentApplication.RbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;


            // FOR TEST:
            //Model.ErrorIsVisible = Visibility.Visible;
            //Model.ErrorString = "Lorem ipsum dolor sit amet, libero amet, aliquam sed quis nam a, interdum massa est, pede aenean. Luctus felis. Nam accumsan consectetuer dui enim nostra et, amet odio consequatur, quis lacinia ipsum vel diam. Pellentesque rem fermentum felis, orci risus vel augue amet odio quam, sit eu quis donec scelerisque urna, urna dictum. Felis cursus lorem eget sagittis sed ante, ultrices varius non pulvinar elit in justo, lectus consectetuer tempus ornare tempor libero, per suspendisse mauris vel nec. Per lorem ante etiam aliquet cursus nec, ac lobortis. Eget metus. Mus urna in habitant ut tincidunt libero, et sit, urna interdum eget purus nec odio a, id sodales in. Tincidunt sit amet mattis aliquam sed risus, orci mollis lacus ullamcorper vel.";
        }

        private void RBLogin(String login, String password)
        {
            if (Model.IsBusy)
                return;

            // We set to Busy
            Model.IsBusy = true;

            // Hide error (if any)
            Model.ErrorIsVisible = Visibility.Collapsed;
            Model.ErrorString = "";

            Task task = new Task(() =>
            {
                if (!CurrentApplication.RbApplication.IsConnected())
                {
                    CurrentApplication.RbApplication.LoginAsync(login, password).ContinueWith(async task =>
                    {
                        var sdkResult = await task;
                        if (!sdkResult.Success)
                        {
                            if (System.Windows.Application.Current != null)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    String strError = "";
                                    if (sdkResult.Result.ExceptionError != null)
                                        strError = "Exception: " + sdkResult.Result.ExceptionError.ToString();
                                    else if (sdkResult.Result.IncorrectUseError != null)
                                    {
                                        strError = sdkResult.Result.IncorrectUseError.ErrorMsg; 
                                        if(!String.IsNullOrEmpty(sdkResult.Result.IncorrectUseError.ErrorDetails))
                                            strError += ": " + sdkResult.Result.IncorrectUseError.ErrorDetails;
                                    }
                                    Model.IsBusy = false;
                                    Model.ErrorIsVisible = Visibility.Visible;
                                    Model.ErrorString = strError;
                                }));
                            }
                        }
                        
                    });
                }
                else
                    CurrentApplication.RbApplication.LogoutAsync();
            });
            task.Start();
        }

    #endregion PRIVATE METHODS

    #region Events fired by Rainbow SDK

        private void RbApplication_ConnectionStateChanged(ConnectionState connectionState)
        {
            switch (connectionState.Status)
            {
                case ConnectionStatus.Connected:
                    var contacts = CurrentApplication.RbApplication.GetContacts();
                    var currentContact = contacts.GetCurrentContact();
                    SetErrorMessage($"User well connected: {currentContact.Peer.DisplayName}");

                    SetToBusy(false);
                    break;

                case ConnectionStatus.Connecting:
                    SetToBusy(true);
                    break;

                case ConnectionStatus.Disconnected:
                    Model.AskingPassword = false;
                    SetToBusy(false);
                    break;
            }
        }

    #endregion Events fired by Rainbow SDK


    #region COMMANDS RAISED BY THE VIEW

        private Boolean IsEmailAddress(String str)
        {
            return new EmailAddressAttribute().IsValid(str);
        }

        private void SetErrorMessage(String errMsg)
        {
            // Ensure to be on UI Thread
            if (System.Windows.Application.Current.Dispatcher.Thread != Thread.CurrentThread)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => { SetErrorMessage(errMsg); }));
                return;
            }

            if (String.IsNullOrEmpty(errMsg))
            {
                Model.ErrorIsVisible = Visibility.Collapsed;
                Model.ErrorString = "";
            }
            else
            {
                Model.ErrorIsVisible = Visibility.Visible;
                Model.ErrorString = errMsg;
            }
        }

        private void SetToBusy(bool busy)
        {
            // Ensure to be on UI Thread
            if (System.Windows.Application.Current.Dispatcher.Thread != Thread.CurrentThread)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => { SetToBusy(busy); }));
                return;
            }

            Model.IsBusy = busy;
            if (busy)
            {
                if (Model.AskingPassword)
                    Model.Connect = "Connecting";
                else
                    Model.Connect = "Continue";
            }
            else
            {
                if (Model.AskingPassword)
                {
                    if (CurrentApplication.RbApplication.IsConnected())
                        Model.Connect = "Logout";
                    else
                        Model.Connect = "Connect";
                }
                else
                {
                    if (CurrentApplication.RbApplication.IsConnected())
                        Model.Connect = "Logout";
                    else
                        Model.Connect = "Continue";
                }
            }
        }
        
        private async void StartSSO(String uri, String redirectUri)
        {
            // Ensure to be on UI Thread
            if (System.Windows.Application.Current.Dispatcher.Thread != Thread.CurrentThread)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => { StartSSO(uri, redirectUri); }));
                return;
            }


            try
            {
                SSOInProgress = true;

                if (browser == null)
                {
                    browser = new EmbeddedBrowser();
                    browser.Icon = Logo;
                    browser.Title = "Single Sign On";
                }

                BrowserResult browserResult;
                browserResult = await browser.InvokeAsync(uri, redirectUri);

                SSOInProgress = false;
                if (browserResult.Error)
                {
                    if (browserResult.CancelByUser)
                        SetErrorMessage("The sign-in window was closed before authorization was completed.");
                    else
                        SetErrorMessage("Authentication failed");
                    SetToBusy(false);
                }
                else
                {
                    SetErrorMessage("SSO has been performed well. Now using Token to login on RB Server ...");

                    String token = browserResult.Token;

                    // Now start login with this token
                    var sdkResult = await CurrentApplication.RbApplication.LoginWithTokenAsync(token);
                    if (!sdkResult.Success)
                    {
                        SetErrorMessage("Login to RB using Token failed");
                        SetToBusy(false);
                    }
                }
            }
            catch (Exception ex)
            {
                SSOInProgress = false;
                SetErrorMessage($"Exception occurred: [{ex.Message}]");

                // ERROR OCCURS or USER CANCELLED
                SetToBusy(false);
            }

        }

        private void CommandButtonLogin(object obj)
        {
            // Hide any error displayed
            SetErrorMessage(null);

            if (CurrentApplication.RbApplication.IsConnected())
            {
                SetErrorMessage("Log out in progress ...");
                SetToBusy(true);
                CurrentApplication.RbApplication.LogoutAsync().ContinueWith(async task =>
                {
                    var sdkResult = await task;
                    if (sdkResult.Success)
                        SetErrorMessage("Log out done");
                    else
                        SetErrorMessage($"Error: [{sdkResult.Result}]");
                });
            }
            else
            {
                // Are we asking the password ?
                if (Model.AskingPassword)
                {

                    if (String.IsNullOrEmpty(Model.Password))
                    {
                        SetErrorMessage("Invalid password");
                        SetToBusy(false);
                    }
                    else
                    {
                        CurrentApplication.RbApplication.LoginAsync(Model.Login, Model.Password).ContinueWith(async task =>
                        {
                            var sdkResult = await task;
                            if (!sdkResult.Success)
                            {
                                if (sdkResult.Result.HttpStatusCode != 0)
                                {
                                    if (sdkResult.Result.IncorrectUseError != null)
                                        SetErrorMessage(String.Format("{0}: [{1}]", sdkResult.Result.IncorrectUseError.ErrorDetails, sdkResult.Result.IncorrectUseError.ErrorCode));
                                    else
                                        SetErrorMessage(String.Format("Exception occurs: [{0}]", sdkResult.Result.ExceptionError.Message));
                                }
                                else
                                {
                                    SetErrorMessage("Cannot contact the server. Please check your network settings.");
                                }
                            }
                        });
                    }

                }

                // We are not asking password
                else
                {
                    // Check email address
                    if (IsEmailAddress(Model.Login))
                    {
                        SetToBusy(true);

                        // We ask server for SSO URL
                        CurrentApplication.RbApplication.GetAuthenticationSSOUrlsAsync(Model.Login).ContinueWith(async task =>
                        {
                            var sdkResult = await task;
                            if (sdkResult.Success)
                            {
                                List<AuthenticationSSOUrl> urls = sdkResult.Data;

                                // Do we have at least one URL ?
                                if ((urls == null) || (urls?.Count == 0))
                                {
                                    Model.AskingPassword = true;
                                    SetToBusy(false);
                                    return;
                                }

                                // If we have several URLs, the first one different of RAINBOW is taken into account
                                AuthenticationSSOUrl authUrl = urls.FirstOrDefault(x => (x.Type.ToUpper() != "RAINBOW"));

                                // Do we have found one ?
                                if (authUrl == null)
                                {
                                    Model.AskingPassword = true;
                                    SetToBusy(false);
                                    return;
                                }

                                // So we start SSO
                                String uri = authUrl.LoginUrl;
                                String redirectUri = String.Format("{0}://{1}/", AppConfiguration.URI_SCHEME_FOR_SSO, AppConfiguration.URI_PATH_FOR_SSO); // /!\  This callback must be set on the property "SsoAuthenticationRedirectUrl" on the RB Application used by this SDK
                                StartSSO(uri, redirectUri);
                            }
                            else
                            {
                                SetToBusy(false);

                                if (sdkResult.Result.HttpStatusCode != 0)
                                {
                                    if (sdkResult.Result.IncorrectUseError != null)
                                        SetErrorMessage(String.Format("{0}: [{1}]", sdkResult.Result.IncorrectUseError.ErrorDetails, sdkResult.Result.IncorrectUseError.ErrorCode));
                                    else
                                        SetErrorMessage(String.Format("Exception occurs: [{0}]", sdkResult.Result.ExceptionError.Message));
                                }
                                else
                                {
                                    SetErrorMessage("Cannot contact the server. Please check your network settings.");
                                }
                            }
                        });
                    }
                    else
                    {
                        SetErrorMessage("Not a valid email address");

                        Model.AskingPassword = false;
                        Model.Connect = "Continue";
                    }

                }
            }
        }

        private void CommandEntryLoginLostFocus(object obj)
        {
            // Nothing to do here
        }

        private void CommandEntryLoginGotFocus(object obj)
        {
            Model.AskingPassword = false;
            Model.Connect = "Continue";
        }

#endregion COMMANDS RAISED BY THE VIEW


    }
}
