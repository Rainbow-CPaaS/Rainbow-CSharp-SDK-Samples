
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

using NLog;

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

        private static Logger log;

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
            InitLogs(appFolderPath);

            log = LogConfigurator.GetLogger(typeof(App));
            log.Info("==============================================================");
            log.Info("Application started: [{0}]", LogFolderName);
            log.Info("Windows version:[{0}]", RuntimeInformation.OSDescription);

            // Create Rainbow Application
            rbApplication = new Rainbow.Application(appFolderPath);
            rbApplication.SetApplicationInfo(APP_ID, APP_SECRET_KEY);
            rbApplication.SetHostInfo(HOST_NAME);

            // We use WebRTC Feature
            rbApplication.Restrictions.UseWebRTC = true;
        }

        private void InitLogs(String folder)
        {
            String logFileName = LogfileName; // File name of the log file
            String archiveLogFileName = LogArchivefileName; // File name of the archive log file
            String logConfigFileName = LogconfigurationFile; // File path to log configuration

            String logConfigContent; // Content of the log file configuration
            String logFullPathFileName; // Full path to log file
            String archiveLogFullPathFileName; // Full path to archive log file 
            try
            {
                //String tempFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
                Directory.CreateDirectory(folder);

                // Set full path to log file name
                logFullPathFileName = Path.Combine(folder, logFileName);
                archiveLogFullPathFileName = Path.Combine(folder, archiveLogFileName);

                // Get content of the log file configuration
                logConfigContent = WpfUtil.GetContentFromResource(logConfigFileName);

                // Load XML in XMLDocument
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(logConfigContent);

                // Set full path to log file in XML element
                XmlElement targetElement = doc["nlog"]["targets"]["target"];
                targetElement.SetAttribute("name", Rainbow.LogConfigurator.GetRepositoryName());    // Set target name equals to RB repository name
                targetElement.SetAttribute("fileName", logFullPathFileName);                        // Set full path to log file
                targetElement.SetAttribute("archiveFileName", archiveLogFullPathFileName);          // Set full path to archive log file

                XmlElement loggerElement = doc["nlog"]["rules"]["logger"];
                loggerElement.SetAttribute("writeTo", Rainbow.LogConfigurator.GetRepositoryName()); // Write to RB repository name

                // Set the configuration
                Rainbow.LogConfigurator.Configure(doc.OuterXml);
            }
            catch (Exception exc)
            { 
            }
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
