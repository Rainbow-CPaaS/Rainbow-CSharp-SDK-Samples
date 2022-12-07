using Newtonsoft.Json.Linq;
using NLog.Config;
using Rainbow;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

using static System.Net.Mime.MediaTypeNames;

namespace SDK.UIForm.WebRTC
{
    public partial class FormLogin : Form
    {
        // Define delegates to update forms elements from any thread
        public delegate void StringArgReturningVoidDelegate(string value);
        public delegate void VoidDelegate();

        // Define Rainbow SDK objects
        Rainbow.Application rbApplication;

        FormWebRTC? _formWebRTC;

        public FormLogin()
        {
            this.HandleCreated += Form_HandleCreated;
            
            InitializeComponent();

            
        }

        private void Form_HandleCreated(object sender, EventArgs e)
        {
            var configuration = Configuration.Instance;

            // Fill form with Configuration info
            tbHostName.Text = configuration.HostName;
            tbLogin.Text = configuration.UserLogin;
            tbPassword.Text = configuration.UserPassword;
            cb_AutoLogin.Checked = configuration.AutoLogin;

            // Create Rainbow.Application object
            rbApplication = new Rainbow.Application();

            // Set main values
            rbApplication.SetApplicationInfo(configuration.AppId, configuration.AppSecret);
            rbApplication.SetHostInfo(configuration.HostName);
            rbApplication.SetTimeout(10000);

            // Define events we want to manage
            rbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;
            rbApplication.InitializationPerformed += RbApplication_InitializationPerformed;

            // Start Login process (if necessary)
            if (configuration.AutoLogin)
            {
                CancelableDelay.StartAfter(200, () =>
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        btnConnect_Click(null, null);
                    }));
                });
            }
        }

        // Used to add information message in "lbInformation" List Box element
        private void AddInformation(String info)
        {
            if (this.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(AddInformation);
                this.Invoke(d, new object[] { info });
            }
            else
            {
                if (!String.IsNullOrEmpty(info))
                {
                    if (!info.StartsWith("\r\n"))
                        info = "\r\n" + info;

                    tbInformation.Text += "\n" + info;

                    // Auto scrool to the end
                    tbInformation.SelectionStart = tbInformation.Text.Length;
                    tbInformation.ScrollToCaret();
                }
            }
        }

        private void UpdateFormAccodringRainbowConnectionStatus()
        {
            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateFormAccodringRainbowConnectionStatus);
                this.Invoke(d, new object[] {  });
            }
            else
            {
                var connectionState = rbApplication.ConnectionState();
                var isInitialized = rbApplication.IsInitialized();

                switch(connectionState)
                {
                    case Rainbow.Model.ConnectionState.Connected:
                        // If initialization is also done, we can allow to open the form used for tests purpose
                        if (isInitialized)
                        {
                            btnOpenTestForm.Enabled = true;

                            btnConnect.Enabled = true;
                            btnConnect.Text = "Disconnect";

                            btnOpenTestForm_Click(this, null);
                        }
                        else // We must wait until the initialization is performed
                        {
                            btnOpenTestForm.Enabled = false;

                            btnConnect.Enabled = false;
                            btnConnect.Text = "Connecting";
                        }
                        break;

                    case Rainbow.Model.ConnectionState.Connecting:
                        btnOpenTestForm.Enabled = false;

                        btnConnect.Enabled = false;
                        btnConnect.Text = "Connecting";
                        break;

                    case Rainbow.Model.ConnectionState.Disconnected:
                        btnOpenTestForm.Enabled = false;

                        btnConnect.Enabled = true;
                        btnConnect.Text = "Connect";
                        break;

                }

                
            }
            
        }

    #region EVENTS RAISED BY SDK

        private void RbApplication_InitializationPerformed(object? sender, EventArgs e)
        {
            AddInformation($"InitializationPerformed");

            UpdateFormAccodringRainbowConnectionStatus();
        }

        private void RbApplication_ConnectionStateChanged(object? sender, Rainbow.Events.ConnectionStateEventArgs e)
        {
            AddInformation($"ConnectionStateChanged[{e.State}]");

            UpdateFormAccodringRainbowConnectionStatus();
        }

    #endregion EVENTS RAISED BY SDK

    #region EVENTS RAISED BY Forms Elements
        
        private void btnConnect_Click(object sender, EventArgs e)
        {
            // Do we want to connect or disconnect ?
            if (rbApplication.IsConnected())
            {
                btnConnect.Enabled = false;

                rbApplication.Logout(callback =>
                {
                    if (callback.Result.Success)
                        AddInformation($"Logout has succeded.");
                    else
                    {
                        AddInformation($"Logout failed:[{Util.SerializeSdkError(callback.Result)}");

                        UpdateFormAccodringRainbowConnectionStatus();
                    }
                });
            }
            else
            {
                btnConnect.Enabled = false;
                btnConnect.Text = "Connecting";

                rbApplication.Restrictions.UseWebRTC = true; // We want to use WebRTC features

                rbApplication.Login(tbLogin.Text, tbPassword.Text, callback =>
                {
                    if (callback.Result.Success)
                        AddInformation($"Login has succeded. Waiting for full initialization ...");
                    else
                    {
                        AddInformation($"Login failed:[{Util.SerializeSdkError(callback.Result)}");

                        UpdateFormAccodringRainbowConnectionStatus();
                    }
                });
            }
        }

        private void btnOpenTestForm_Click(object sender, EventArgs? e)
        {
            if (_formWebRTC == null)
            {
                _formWebRTC = new FormWebRTC();
                _formWebRTC.Initialize(rbApplication, Configuration.Instance.FFmpegLibPath);
            }

            _formWebRTC.WindowState = FormWindowState.Normal;
            _formWebRTC.Show();
        }

    #endregion EVENTS RAISED BY Forms Elements


    }
}
