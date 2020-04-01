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
        App currentApplication = (App)System.Windows.Application.Current;

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
            }
        }

#endregion PROPERTIES

#region PUBLIC METHODS
        public LoginModel()
        {
            // Define Rainbow events used in this View
            currentApplication.RbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;
            currentApplication.RbApplication.InitializationPerformed += RbApplication_InitializationPerformed;
        }

        public void Init()
        {
            // Set default values
            IsNotBusy = true;
            Connect = "Connect";

            Login = currentApplication.RbApplication.GetUserLoginFromCache();
            Password = currentApplication.RbApplication.GetUserPasswordFromCache();
        }

        public void RBLogin(String login, String password)
        {
            Task task = new Task(() =>
            {
                // We set to Busy
                IsNotBusy = false;

                if (currentApplication.RbApplication.ConnectionState() == ConnectionState.Disconnected)
                {
                    /*
                    IPEndPoint ipEndPoint;
                    var addresses = Dns.GetHostAddresses(currentApplication.HOST_NAME);
                    ipEndPoint = new IPEndPoint(addresses[0], 0);
                    currentApplication.RbApplication.SetIpEndPoint(ipEndPoint);
                    */

                    currentApplication.RbApplication.Login(login, password, callback =>
                    {
                        //TODO - manage error
                    });
                }
                else
                    currentApplication.RbApplication.Logout();
            });
            task.Start();
        }
#endregion PUBLIC METHODS

#region PRIVATE METHODS
        private void ShowMainWindow()
        {
            // First hide login window
            currentApplication.HideLoginWindow();

            // Now show Main Window
            currentApplication.ShowApplicationMainWindow();
        }

#endregion PRIVATE METHODS

#region Events fired by Rainbow SDK
        private void RbApplication_InitializationPerformed(object sender, EventArgs e)
        {
            if (!currentApplication.RbApplication.IsInitialized())
                return;

            // Once the initialization is performed, we wants 
            //      - to know all conversations
            //      - to know Bubbles
            Task task = new Task(() =>
            {
                ManualResetEvent manualEventBubbles = new ManualResetEvent(false);
                ManualResetEvent manualEventConversations = new ManualResetEvent(false);

                currentApplication.RbConversations.GetAllConversations(callback =>
                {
                    if (!callback.Result.Success)
                    {
                        // TO DO
                    }
                    manualEventConversations.Set();
                });

                currentApplication.RbBubbles.GetAllBubbles(callback =>
                {
                    if (!callback.Result.Success)
                    {
                        // TO DO
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