using System;
using System.Threading;
using System.Threading.Tasks;

using InstantMessaging.Helpers;

using Rainbow;
using Rainbow.Model;

using log4net;

namespace InstantMessaging.Model
{
    public class LoginModel: ObservableObject
    {
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(LoginModel));
        App CurrentApplication = (App)System.Windows.Application.Current;

#region PROPERTIES

        string m_login;
        public String Login
        {
            get { return m_login; }
            set { SetProperty(ref m_login, value); }
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

            Login = CurrentApplication.RbApplication.GetUserLoginFromCache();
            Password = CurrentApplication.RbApplication.GetUserPasswordFromCache();
        }

        public void RBLogin(String login, String password)
        {
            if (IsBusy)
                return;

            Task task = new Task(() =>
            {
                // We set to Busy
                IsNotBusy = false;

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
                        //TODO - manage error
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
                        log.WarnFormat("[RbApplication_InitializationPerformed] Cannot get all conversations");
                    }
                    manualEventConversations.Set();
                });

                CurrentApplication.RbBubbles.GetAllBubbles(callback =>
                {
                    if (!callback.Result.Success)
                    {
                        log.WarnFormat("[RbApplication_InitializationPerformed] Cannot get all bubbles");
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