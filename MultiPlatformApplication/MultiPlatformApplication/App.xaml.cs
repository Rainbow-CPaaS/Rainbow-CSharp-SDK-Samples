using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

using Rainbow;

using MultiPlatformApplication.Assets;
using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Services;

using Microsoft.Extensions.Logging;
using NLog.Config;

[assembly: ExportFont("FontAwesomeSolid.5.15.3Enhanced.otf", Alias = "FontAwesomeSolid5")]

namespace MultiPlatformApplication
{
    public partial class App : Xamarin.Forms.Application
    {
        private static ILogger log;

        // Create the navigation service
        internal NavigationService NavigationService { get; } = new NavigationService();

        // Create the FilesUpload service
        internal FilesUpload FilesUpload { get; } = FilesUpload.Instance;

        // To store the current conversation followed
        internal String CurrentConversationId = null;

        // To store the current theme used
        internal String CurrentThemeName = "light";


        public App()
        {
            InitializeComponent();

            // As soon as possible we want the device density
            double density = Helper.GetDensity();

            // Get app foldr path in order to init logs
            string appFolderPath = Helper.GetAppFolderPath();

            InitLogsWithNLog(appFolderPath);
            log = Rainbow.LogFactory.CreateLogger<App>();
            log.LogInformation("==============================================================");
            log.LogInformation("Application started - App Folder Path: [{0}] - Density:[{1}]", appFolderPath, density);

            switch(Device.RuntimePlatform)
            {
                case Device.iOS:
                case Device.Android:
                case Device.UWP:
                    log.LogInformation("Device Info - Platform:[{0}] - Model:[{1}] - OS Version:[{2}] - Type:[{3}]", DeviceInfo.Platform, DeviceInfo.Model, DeviceInfo.VersionString, DeviceInfo.DeviceType);
                    break;
                case Device.WPF:
                    // TODO: add more info
                    log.LogInformation("Device Info - Platform: WPF");
                    break;
                case Device.macOS:
                    // TODO: add more info
                    log.LogInformation("Device Info - Platform: macOS");
                    break;
            }

            // Set the wrapper according the environment to use/test
            if (ApplicationInfo.UseTestEnvironment)
                Helper.SdkWrapper = new TestSdkWrapper();
            else
                Helper.SdkWrapper = new RainbowSdkWrapper();


            // Set language to EN
            Helper.SdkWrapper.SetLanguageId("en");

            // Register views in the Navigation servcice
            NavigationService.Configure("LoginPage", typeof(MultiPlatformApplication.Views.LoginPage));
            NavigationService.Configure("MainPage", typeof(MultiPlatformApplication.Views.MainPage));
            NavigationService.Configure("ConversationStreamPage", typeof(MultiPlatformApplication.Views.ConversationStreamPage));

            //NavigationService.Configure("TestPage", typeof(MultiPlatformApplication.Views.TestPage));
            //NavigationService.Configure("TestPage2", typeof(MultiPlatformApplication.Views.TestPage2));
            //NavigationService.Configure("TestPage3", typeof(MultiPlatformApplication.Views.TestPage3));

            MainPage = new NavigationPage( NavigationService.GetPage("LoginPage") );
        }

        public void ChangeTheme(String themeName)
        {            
            // Ensure to be on Main UI Thread
            if(!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => ChangeTheme(themeName) );
                return;
            }
            try
            {
                if (String.IsNullOrEmpty(themeName))
                    return;

                String validThemeName = themeName.ToLower();
                if (validThemeName == CurrentThemeName)
                    return;

                ResourceDictionary resourceDictionary = null;
                switch (validThemeName)
                {
                    case "light":
                        resourceDictionary = new ThemeLight();
                        break;

                    case "dark":
                        resourceDictionary = new ThemeDark();
                        break;

                    default:
                        validThemeName = "light";
                        resourceDictionary = new ThemeLight();
                        break;

                }

                // Clear and set new theme
                ThemeDictionary.Clear();
                ThemeDictionary.Add(resourceDictionary);

                // Store specified theme name
                CurrentThemeName = validThemeName;
            }
            catch { }
        }

        static Boolean InitLogsWithNLog(String folder)
        {
            String logConfigFileName = "NLogConfiguration.xml";

            String logFileName = "MultiPlatformApplication"; // File name of the log file
            String suffixWebRtc = "_WebRTC";
            String suffixArchive = "_{###}";
            String extension = ".log";

            try
            {
                // Get content of the log file configuration
                String logConfigContent = Helper.GetContentOfEmbeddedResource(logConfigFileName, Encoding.UTF8);

                // Load XML in XMLDocument
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(logConfigContent);

                XmlElement targetElement = doc["nlog"]["targets"];
                var targetsList = targetElement.ChildNodes;
                foreach(var target in targetsList)
                {
                    if(target is XmlElement xmlElement)
                    {
                        var name = xmlElement.GetAttribute("name");
                        if(name?.Contains("WEBRTC") == true)
                        {
                            var filename = Path.Combine(folder, logFileName + suffixWebRtc + extension);
                            var archiveFilename = Path.Combine(folder, logFileName + suffixWebRtc + suffixArchive + extension);
                            xmlElement.SetAttribute("fileName", filename);
                            xmlElement.SetAttribute("archiveFileName", archiveFilename);
                        }
                        else
                        {
                            var filename = Path.Combine(folder, logFileName + extension);
                            var archiveFilename = Path.Combine(folder, logFileName + suffixArchive + extension);
                            xmlElement.SetAttribute("fileName", filename);
                            xmlElement.SetAttribute("archiveFileName", archiveFilename);
                        }
                    }
                }

                // Create NLog configuration using XML file content
                XmlLoggingConfiguration config = XmlLoggingConfiguration.CreateFromXmlString(doc.OuterXml);
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

        

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

    }
}

