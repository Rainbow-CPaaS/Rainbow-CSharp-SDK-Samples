using System;
using System.Threading;
using System.Threading.Tasks;

using InstantMessaging.Helpers;

using Rainbow;
using Rainbow.Model;

using Microsoft.Extensions.Logging;
using System.Windows;

namespace InstantMessaging.Model
{
    public class LoginModel: ObservableObject
    {
        private static readonly ILogger log = Rainbow.LogFactory.CreateLogger<LoginModel>();
        App CurrentApplication = (App)System.Windows.Application.Current;
        IniFileParser iniFileParser;

#region PROPERTIES

        string m_login;
        public String Login
        {
            get { return m_login; }
            set { 
                SetProperty(ref m_login, value); 
            }
        }

        string m_password;
        public string Password
        {
            get { return m_password; }
            set { SetProperty(ref m_password, value); }
        }

        string m_connect;
        public String Connect
        {
            get { return m_connect; }
            set { SetProperty(ref m_connect, value); }
        }

        Boolean m_isNotBusy;
        public Boolean IsNotBusy
        {
            get { return m_isNotBusy; }
            set
            {
                SetProperty(ref m_isNotBusy, value);
                if (IsBusy == IsNotBusy)
                    IsBusy = (!IsNotBusy);
            }
        }

        Boolean m_isBusy;
        public Boolean IsBusy
        {
            get { return m_isBusy; }
            set
            {
                SetProperty(ref m_isBusy, value);
                if (IsBusy == IsNotBusy)
                    IsNotBusy = (!IsBusy);
            }
        }

        Boolean m_autoLogin;
        public Boolean AutoLogin
        {
            get { return m_autoLogin; }
            set { SetProperty(ref m_autoLogin, value); }
        }

        String m_errorString;
        public String ErrorString
        {
            get { return m_errorString; }
            set { SetProperty(ref m_errorString, value); }
        }

        Visibility m_errorIsVisible;
        public Visibility ErrorIsVisible
        {
            get { return m_errorIsVisible; }
            set { SetProperty(ref m_errorIsVisible, value); }
        }

#endregion PROPERTIES

#region PUBLIC METHODS
        public LoginModel()
        {
            // Define Rainbow events used in this View
            CurrentApplication.RbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;
            CurrentApplication.RbApplication.InitializationPerformed += RbApplication_InitializationPerformed;

            Init();
        }

        public void Init()
        {
            // Set default values
            IsNotBusy = true;
            Connect = "Connect";
            ErrorIsVisible = Visibility.Collapsed;
            ErrorString = "";

            // Get Login / Password from cache (i.e. INI FILE)
            Login = CurrentApplication.RbApplication.GetUserLoginFromCache();
            Password = CurrentApplication.RbApplication.GetUserPasswordFromCache();

            // Get AutoLogin value (specific value from INI FILE)
            iniFileParser = CurrentApplication.RbApplication.GetIniFileParser();
            String autoLoginString = iniFileParser.GetValue("AutoLogin", "Application", "False");
            AutoLogin = autoLoginString.ToLower() == "true";

            if (AutoLogin)
                RBLogin(Login, Password);

            // FOR TEST:
            //ErrorIsVisible = Visibility.Visible;
            //ErrorString = "Lorem ipsum dolor sit amet, libero amet, aliquam sed quis nam a, interdum massa est, pede aenean. Luctus felis. Nam accumsan consectetuer dui enim nostra et, amet odio consequatur, quis lacinia ipsum vel diam. Pellentesque rem fermentum felis, orci risus vel augue amet odio quam, sit eu quis donec scelerisque urna, urna dictum. Felis cursus lorem eget sagittis sed ante, ultrices varius non pulvinar elit in justo, lectus consectetuer tempus ornare tempor libero, per suspendisse mauris vel nec. Per lorem ante etiam aliquet cursus nec, ac lobortis. Eget metus. Mus urna in habitant ut tincidunt libero, et sit, urna interdum eget purus nec odio a, id sodales in. Tincidunt sit amet mattis aliquam sed risus, orci mollis lacus ullamcorper vel.";
        }

        public void RBLogin(String login, String password)
        {
            if (IsBusy)
                return;

            // We set to Busy
            IsNotBusy = false;

            // Hide error (if any)
            ErrorIsVisible = Visibility.Collapsed;
            ErrorString = "";


            Task task = new Task(() =>
            {
                // Save AutoLogin option
                iniFileParser.WriteValue("AutoLogin", "Application", AutoLogin ? "True" : "False");
                iniFileParser.Save();

                if (CurrentApplication.RbApplication.ConnectionState() == ConnectionState.Disconnected)
                {
                    /*
                    IPEndPoint ipEndPoint;
                    var addresses = Dns.GetHostAddresses(currentApplication.HOST_NAME);
                    ipEndPoint = new IPEndPoint(addresses[0], 0);
                    currentApplication.RbApplication.SetIpEndPoint(ipEndPoint);
                    */

                    CurrentApplication.RbApplication.Login(login, password, callback =>
                    {
                        if(!callback.Result.Success)
                        {
                            if (System.Windows.Application.Current != null)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    String strError = "";
                                    if (callback.Result.ExceptionError != null)
                                        strError = "Exception: " + callback.Result.ExceptionError.ToString();
                                    else if (callback.Result.IncorrectUseError != null)
                                    {
                                        strError = callback.Result.IncorrectUseError.ErrorMsg; 
                                        if(!String.IsNullOrEmpty(callback.Result.IncorrectUseError.ErrorDetails))
                                            strError += ": " + callback.Result.IncorrectUseError.ErrorDetails;
                                    }
                                    IsNotBusy = true;
                                    ErrorIsVisible = Visibility.Visible;
                                    ErrorString = strError;
                                }));
                            }
                        }
                        
                    });
                }
                else
                    CurrentApplication.RbApplication.Logout();
            });
            task.Start();
        }
#endregion PUBLIC METHODS

#region PRIVATE METHODS
        private void ShowMainWindow()
        {
            // First hide login window
            CurrentApplication.HideLoginWindow();

            // Now show Main Window
            CurrentApplication.ShowApplicationMainWindow();
        }

#endregion PRIVATE METHODS

#region Events fired by Rainbow SDK
        private void RbApplication_InitializationPerformed(object sender, EventArgs e)
        {
            if (!CurrentApplication.RbApplication.IsInitialized())
                return;

            // Store current user ID / Jid
            CurrentApplication.CurrentUserId = CurrentApplication.RbContacts.GetCurrentContactId();
            CurrentApplication.CurrentUserJid = CurrentApplication.RbContacts.GetCurrentContactJid();

            // Once the initialization is performed, we wants 
            //      - to know all conversations
            //      - to know Bubbles
            Task task = new Task(() =>
            {
                ManualResetEvent manualEventBubbles = new ManualResetEvent(false);
                ManualResetEvent manualEventConversations = new ManualResetEvent(false);

                CurrentApplication.RbConversations.GetAllConversations(callback =>
                {
                    if (!callback.Result.Success)
                    {
                        log.LogWarning("[RbApplication_InitializationPerformed] Cannot get all conversations");
                    }
                    manualEventConversations.Set();
                });

                CurrentApplication.RbBubbles.GetAllBubbles(callback =>
                {
                    if (!callback.Result.Success)
                    {
                        log.LogWarning("[RbApplication_InitializationPerformed] Cannot get all bubbles");
                    }
                    manualEventBubbles.Set();
                });

                // Wait all manual events before to continue
                WaitHandle.WaitAll(new WaitHandle[] { manualEventBubbles, manualEventConversations }, 5000);

                // No we are fully operationnal: Login DONE + Init DONE
                Connect = "Disconnect";
                IsNotBusy = true;

                // Show the main Window
                ShowMainWindow();
            });
            task.Start();
        }

        private void RbApplication_ConnectionStateChanged(object sender, Rainbow.Events.ConnectionStateEventArgs e)
        {
            switch (e.State)
            {
                case ConnectionState.Connected: // We need to wait also  "InitializationPErformed" event to be sure to be fully connected/ready
                case ConnectionState.Connecting:
                    Connect = "Connecting";
                    IsNotBusy = false;
                    break;

                case ConnectionState.Disconnected:
                    Connect = "Connect";
                    IsNotBusy = true;
                    break;
            }
        }
#endregion Events fired by Rainbow SDK

    }
}