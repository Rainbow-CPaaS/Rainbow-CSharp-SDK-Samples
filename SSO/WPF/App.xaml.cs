using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using NLog;
using Rainbow;
using WpfSSOSamples.Helpers;
using WpfSSOSamples.View;

namespace WpfSSOSamples
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private readonly String LogfileName = "Rainbow_CSharp_WPF_Example.log";
        private readonly String LogArchivefileName = "RaveAlert_{###}.log";
        private readonly String LogConfigurationfileName = "NLogConfiguration.xml";
        private readonly String LogFolderName = "Rainbow.CSharp.SDK";

        private Logger log;

        // Define all Rainbow objects we use
        internal Rainbow.Application RbApplication = null;

        internal LoginView LoginWindow = null;

#region EVENTS/OVERRIDE OF APPLICATION OBJECT
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitLogs();
            log = LogConfigurator.GetLogger(typeof(App));

            string appFolderPath = Path.Combine(Helper.GetTempFolder(), LogFolderName);

            RbApplication = new Rainbow.Application(appFolderPath);
            RbApplication.SetTimeout(10000);

            RbApplication.SetApplicationInfo(AppConfiguration.APP_ID, AppConfiguration.APP_SECRET_KEY);
            RbApplication.SetHostInfo(AppConfiguration.HOST_NAME);
            RbApplication.SetWebProxyInfo(null);

            LoginWindow = new LoginView();
            LoginWindow.Show();

        }
#endregion EVENTS/OVERRIDE OF APPLICATION OBJECT


        private void InitLogs()
        {
            String logFileName = LogfileName; // File name of the log file
            String archiveLogFileName = LogArchivefileName; // File name of the archive log file
            String logConfigFileName = LogConfigurationfileName; // File path to log configuration

            String logConfigContent; // Content of the log file configuration
            String logFullPathFileName; // Full path to log file
            String archiveLogFullPathFileName; ; // Full path to archive log file 

            try
            {
                String folder = Helper.GetTempFolder();
                Directory.CreateDirectory(folder);

                // Set full path to log file name
                logFullPathFileName = Path.Combine(folder, logFileName);
                archiveLogFullPathFileName = Path.Combine(folder, archiveLogFileName);

                // Get content of the log file configuration
                Stream stream = Helper.GetMemoryStreamFromResource(logConfigFileName);
                using (StreamReader sr = new StreamReader(stream))
                {
                    logConfigContent = sr.ReadToEnd();
                }
                stream.Dispose();

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
            catch { }
        }

    }
}
