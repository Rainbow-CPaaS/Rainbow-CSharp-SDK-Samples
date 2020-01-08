using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Foundation;
using InstantMessaging.Helpers;
using InstantMessaging.iOS.Helpers;
using Rainbow.Helpers;
using UIKit;

namespace InstantMessaging.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

            InitLogs();

            AvatarPool avatarPool = AvatarPool.Instance;
            avatarPool.SetImageManagement(new ImageManagement());

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        private void InitLogs()
        {
            String logFileName = "rainbowsdk.log"; // File name of the log file
            String logConfigFilePath = "log4netConfiguration.xml"; // File path to log configuration

            String logConfigContent; // Content of the log file configuration
            String logFolderPath; // Folder path which will contains log files;
            String logFullPathFileName; // Full path to log file

            try
            {
                // Set folder path where log files are stored
                logFolderPath = Helper.GetTempFolder();

                // Set full path to log file name
                logFullPathFileName = Path.Combine(logFolderPath, logFileName);

                // Delete previous log
                if (File.Exists(logFullPathFileName))
                {
                    String content = File.ReadAllText(logFullPathFileName);
                    File.Delete(logFullPathFileName);
                }

                // Get content of the log file configuration
                using (StreamReader sr = new StreamReader(logConfigFilePath))
                {
                    logConfigContent = sr.ReadToEnd();
                }

                // Load XML in XMLDocument
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(logConfigContent);

                // Set full path to log file in XML element
                XmlElement fileElement = doc["log4net"]["appender"]["file"];
                fileElement.SetAttribute("value", logFullPathFileName);

                log4net.Repository.ILoggerRepository repository = Rainbow.LogConfigurator.GetRepository();
                log4net.Config.XmlConfigurator.Configure(repository, doc.DocumentElement);
            }
            catch (Exception e)
            {

            }
        }
    }
}
