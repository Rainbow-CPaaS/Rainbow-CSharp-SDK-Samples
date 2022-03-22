using NLog.Config;
using Rainbow;
using System.Reflection;
using System.Xml;

namespace SampleChannels
{
    public partial class FormLogin : Form
    {
        // Define delegates to update forms elements from any thread
        public delegate void BoolArgReturningVoidDelegate(bool value);
        public delegate void ImageArgReturningVoidDelegate(System.Drawing.Image value);
        public delegate void StringArgReturningVoidDelegate(string value);
        public delegate void VoidDelegate();

        // Define Rainbow SDK objects
        Rainbow.Application rbApplication;

        FormChannel formChannel;

        public FormLogin()
        {
            InitializeComponent();

            if (CheckApplicationInfo())
            {
                InitLogsWithNLog(); // To initialize log using NLog XML configuration file with SDK 2.6.0 and more
                // InitLogsInOldWay(); // To initialize log using NLog XML configuration file BEFORE SDK 2.6.0
            }
            else
                btnConnect.Enabled = false;
                
            tbHostName.Text = ApplicationInfo.HOST_NAME;
            tbLogin.Text = ApplicationInfo.LOGIN_USER;
            tbPassword.Text = ApplicationInfo.PASSWORD_USER;


            // Create Rainbow.Application object
            rbApplication = new Rainbow.Application();

            // Set main values
            rbApplication.SetApplicationInfo(ApplicationInfo.APP_ID, ApplicationInfo.APP_SECRET_KEY);
            rbApplication.SetHostInfo(ApplicationInfo.HOST_NAME);

            // DEfine events we want to manage
            rbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;
            rbApplication.InitializationPerformed += RbApplication_InitializationPerformed;
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

        // Check if mandatory information have been set in file ApplicationInfo.cs
        private Boolean CheckApplicationInfo()
        {
            Boolean result = true;

            if ( ! (ApplicationInfo.APP_ID?.Length > 0 && ApplicationInfo.APP_ID != ApplicationInfo.DEFAULT_VALUE) )
            {
                AddInformation("ApplicationInfo.APP_ID has not been defined correctly");
                result = false;
            }

            if (!(ApplicationInfo.APP_SECRET_KEY?.Length > 0 && ApplicationInfo.APP_SECRET_KEY != ApplicationInfo.DEFAULT_VALUE))
            {
                AddInformation("ApplicationInfo.APP_SECRET_KEY has not been defined correctly");
                result = false;
            }

            if (!(ApplicationInfo.HOST_NAME?.Length > 0 && ApplicationInfo.HOST_NAME != ApplicationInfo.DEFAULT_VALUE))
            {
                AddInformation("ApplicationInfo.HOST_NAME has not been defined correctly");
                result = false;
            }

            if (ApplicationInfo.NLOG_CONFIG_FILE_PATH?.Length > 0)
            {
                if(!File.Exists(ApplicationInfo.NLOG_CONFIG_FILE_PATH))
                {
                    AddInformation("ApplicationInfo.NLOG_CONFIG_FILE_PATH has been defined but the path is invalid");
                    result = false;
                }
            }
            else
            {
                AddInformation("ApplicationInfo.NLOG_CONFIG_FILE_PATH has not been defined correctly");
                result = false;
            }

            return result;
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

        private Boolean InitLogsWithNLog() // To initialize log using NLog XML configuration file with SDK 2.6.0 and more
        {
            String logConfigFilePath = ApplicationInfo.NLOG_CONFIG_FILE_PATH; // File path to log configuration (or you could also use an embedded resource)

            try
            {
                if (File.Exists(logConfigFilePath))
                {
                    // Get content of the log file configuration
                    String logConfigContent = File.ReadAllText(logConfigFilePath, System.Text.Encoding.UTF8);

                    // Create NLog configuration using XML file content
                    XmlLoggingConfiguration config = XmlLoggingConfiguration.CreateFromXmlString(logConfigContent);
                    if (config.InitializeSucceeded == true)
                    {
                        // Set NLog configuration
                        NLog.LogManager.Configuration = config;

                        // Create Logger factory
                        var factory = new NLog.Extensions.Logging.NLogLoggerFactory();

                        // Set Logger factory to Rainbow SDK
                        Rainbow.LogFactory.Set(factory);

                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        //private void InitLogsInOldWay() // To initialize log using NLog XML configuration file BEFORE SDK 2.6.0
        //{
        //    String logFileName = "RainbowSampleConferences.log"; // File name of the log file
        //    String archiveLogFileName = "RainbowSampleConferences_{###}.log"; // File name of the archive log file
        //    String logConfigFilePath = @"..\..\..\..\..\NLogConfigurationOldWay.xml"; // File path to log configuration

        //    String logConfigContent; // Content of the log file configuration
        //    String logFolderPath = ""; // Folder path which will contains log files;
        //    String logFullPathFileName = ""; // Full path to log file
        //    String archiveLogFullPathFileName; ; // Full path to archive log file 

        //    try
        //    {
        //        // Set folder path where log files are stored
        //        logFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        //        // Set full path to log file name
        //        logFullPathFileName = Path.Combine(logFolderPath, logFileName);
        //        archiveLogFullPathFileName = Path.Combine(logFolderPath, archiveLogFileName);

        //        // Get content of the log file configuration
        //        using (FileStream stream = File.OpenRead(logConfigFilePath))
        //        {
        //            logConfigContent = File.ReadAllText(logConfigFilePath, System.Text.Encoding.UTF8);
        //        }

        //        // Load XML in XMLDocument
        //        XmlDocument doc = new XmlDocument();
        //        doc.LoadXml(logConfigContent);

        //        // Set full path to log file in XML element
        //        XmlElement targetElement = doc["nlog"]["targets"]["target"];
        //        targetElement.SetAttribute("name", Rainbow.LogConfigurator.GetRepositoryName());    // Set target name equals to RB repository name
        //        targetElement.SetAttribute("fileName", logFullPathFileName);                        // Set full path to log file
        //        targetElement.SetAttribute("archiveFileName", archiveLogFullPathFileName);          // Set full path to archive log file

        //        XmlElement loggerElement = doc["nlog"]["rules"]["logger"];
        //        loggerElement.SetAttribute("writeTo", Rainbow.LogConfigurator.GetRepositoryName()); // Write to RB repository name

        //        // Set the configuration
        //        Rainbow.LogConfigurator.Configure(doc.OuterXml);
        //    }
        //    catch { }
        //}

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

        private void btnOpenTestForm_Click(object sender, EventArgs e)
        {
            if ( (formChannel == null) || (formChannel?.IsDisposed == true) )
            {
                formChannel = new FormChannel();
                formChannel.Initialize(rbApplication);
            }

            if(!formChannel.Visible)
                formChannel.Show(this);
        }

    #endregion EVENTS RAISED BY Forms Elements


    }
}