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

using NLog;


[assembly: ExportFont("FontAwesomeSolid.5.15.3Enhanced.otf", Alias = "FontAwesomeSolid5")]

namespace MultiPlatformApplication
{
    public partial class App : Xamarin.Forms.Application
    {         
        private static Logger log;

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

            InitLogs(appFolderPath);
            log = LogConfigurator.GetLogger(typeof(App));
            log.Info("==============================================================");
            log.Info("Application started - App Folder Path: [{0}] - Density:[{1}]", appFolderPath, density);

            switch(Device.RuntimePlatform)
            {
                case Device.iOS:
                case Device.Android:
                case Device.UWP:
                    log.Info("Device Info - Platform:[{0}] - Model:[{1}] - OS Version:[{2}] - Type:[{3}]", DeviceInfo.Platform, DeviceInfo.Model, DeviceInfo.VersionString, DeviceInfo.DeviceType);
                    break;
                case Device.WPF:
                    // TODO: add more info
                    log.Info("Device Info - Platform: WPF");
                    break;
                case Device.macOS:
                    // TODO: add more info
                    log.Info("Device Info - Platform: macOS");
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
            //var mainPage = NavigationService.SetRootPage("TestPage");

            var mainPage = NavigationService.SetRootPage("LoginPage");

            MainPage = mainPage;
        }


        public void ChangeTheme(String themeName)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
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

            });
        }

        private void InitLogs(String folder)
        {
            String logFileName = "MultiPlatformApplication.log"; // File name of the log file
            String archiveLogFileName = "MultiPlatformApplication_{###}.log"; // File name of the archive log file
            String logConfigFileName = "NLogConfiguration.xml"; // File path to log configuration

            String logConfigContent; // Content of the log file configuration
            String logFullPathFileName; // Full path to log file
            String archiveLogFullPathFileName; ; // Full path to archive log file 
            try
            {
                //String tempFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
                Directory.CreateDirectory(folder);

                // Set full path to log file name
                logFullPathFileName = Path.Combine(folder, logFileName);
                archiveLogFullPathFileName = Path.Combine(folder, archiveLogFileName);

                // Get content of the log file configuration
                logConfigContent = Helper.GetContentOfEmbeddedResource(logConfigFileName, Encoding.UTF8);

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
                Boolean logInit = Rainbow.LogConfigurator.Configure(doc.OuterXml);
            }
            catch
            {
            }
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

