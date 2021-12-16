
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

using Rainbow;

using Microsoft.Extensions.Logging;
using NLog.Config;

namespace SDK.WpfApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        //   /!\ INFORMATION TO PROVIDE ACCORDING YOUR ENVIRONMENT - Cf: https://hub.openrainbow.com/#/documentation/doc/sdk/csharp/guides/001_getting_started
        public const string APP_ID = "TO-DEFINE";
        public const string APP_SECRET_KEY = "TO-DEFINE";
        public const string HOST_NAME = "TO-DEFINE";

        public const string LOGIN_USER1 = "TO-DEFINE";
        public const string PASSWORD_USER1 = "TO-DEFINE";

        private static ILogger log;

        private readonly String LogfileName = "WpfApp.log";
        private readonly String LogArchivefileName = "WpfApp_{###}.log";
        private readonly String LogconfigurationFile = "NLogConfiguration.xml";
        private readonly String LogFolderName = "SDKWpfApp";

        public String appFolderPath;

        public Rainbow.Application rbApplication;

        public App()
        {
            if (SingleApplicationDetector.IsRunning())
            {
                ExitApplication();
                return;
            }

            System.Windows.Application.Current.Exit += Application_Exit;

            appFolderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            appFolderPath = Path.Combine(appFolderPath, LogFolderName);
            InitLogsWithNLog();

            log = Rainbow.LogFactory.CreateLogger<App>();
            log.LogInformation("==============================================================");
            log.LogInformation("Application started: [{0}]", LogFolderName);
            log.LogInformation("Windows version:[{0}]", RuntimeInformation.OSDescription);

            // Create Rainbow Application
            rbApplication = new Rainbow.Application(appFolderPath);
            rbApplication.SetApplicationInfo(APP_ID, APP_SECRET_KEY);
            rbApplication.SetHostInfo(HOST_NAME);

            // We use WebRTC Feature
            rbApplication.Restrictions.UseWebRTC = true;
        }

        static Boolean InitLogsWithNLog()
        {
            try
            {
                String logConfigContent = WpfUtil.GetContentFromResource("NLogConfiguration.xml");

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

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            SingleApplicationDetector.Close();
        }

        private void ExitApplication()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
