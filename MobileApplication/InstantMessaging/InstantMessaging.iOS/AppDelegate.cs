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
            String archiveLogFileName = "rainbowsdk_{###}.log"; // File name of the archive log file
            String logConfigFilePath = "NLogConfiguration.xml"; // File path to log configuration

            String logConfigContent; // Content of the log file configuration
            String logFolderPath; // Folder path which will contains log files;
            String logFullPathFileName; // Full path to log file
            String archiveLogFullPathFileName; ; // Full path to archive log file 

            try
            {
                // Set folder path where log files are stored
                logFolderPath = Helper.GetTempFolder();

                // Set full path to log file name
                logFullPathFileName = Path.Combine(logFolderPath, logFileName);
                archiveLogFullPathFileName = Path.Combine(logFolderPath, archiveLogFileName);

                //// Delete previous log
                //if (File.Exists(logFullPathFileName))
                //{
                //    String content = File.ReadAllText(logFullPathFileName);
                //    File.Delete(logFullPathFileName);
                //}

                // Get content of the log file configuration
                using (StreamReader sr = new StreamReader(logConfigFilePath))
                {
                    logConfigContent = sr.ReadToEnd();
                }

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
            catch (Exception e)
            {

            }
        }
    }
}
