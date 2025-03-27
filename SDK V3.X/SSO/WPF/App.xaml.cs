using Microsoft.Extensions.Logging;
using NLog.Config;
using System;
using System.IO;
using System.Windows;
using WpfSSOSamples.Helpers;
using WpfSSOSamples.View;

namespace WpfSSOSamples
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private readonly String LogFolderName = "Rainbow.CSharp.SDK";

        private static ILogger log;

        // Define all Rainbow objects we use
        internal Rainbow.Application RbApplication = null;

        internal LoginView LoginWindow = null;

#region EVENTS/OVERRIDE OF APPLICATION OBJECT
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitLogsWithNLog();
            log = Rainbow.LogFactory.CreateLogger<App>();

            string appFolderPath = Path.Combine(Helper.GetTempFolder(), LogFolderName);

            RbApplication = new Rainbow.Application(appFolderPath);
            RbApplication.SetTimeout(10000);
            RbApplication.Restrictions.LogRestRequest = true;

            RbApplication.SetApplicationInfo(AppConfiguration.APP_ID, AppConfiguration.APP_SECRET_KEY);
            RbApplication.SetHostInfo(AppConfiguration.HOST_NAME);
            RbApplication.SetWebProxy(null);

            LoginWindow = new LoginView();
            LoginWindow.Show();

        }
        #endregion EVENTS/OVERRIDE OF APPLICATION OBJECT

        static Boolean InitLogsWithNLog()
        {
            try
            {
                // Get content of the log file configuration
                String logConfigContent = null;
                Stream stream = Helper.GetMemoryStreamFromResource("NLogConfiguration.xml");
                using (StreamReader sr = new StreamReader(stream))
                {
                    logConfigContent = sr.ReadToEnd();
                }
                stream.Dispose();

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
            catch { }

            return false;
        }

    }
}
