using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

using Rainbow;
using Rainbow.Common;

using MultiPlatformApplication.Assets;
using MultiPlatformApplication.Helpers;

using NLog;


namespace MultiPlatformApplication
{
    public partial class App : Xamarin.Forms.Application
    {         
        private static Logger log;

        internal SdkWrapper SdkWrapper;


        // To store the current conversation followed
        internal String CurrentConversationId = null;

        internal String CurrentThemeName = "light";

        // Define Pages used in the Xamarin Application
        internal ConversationsPage ConversationsPage = null; // Page used to display the list of Conversations
        internal Dictionary<String, ConversationStreamPage> ConversationStreamPageList = null; // Pages used to display the conversation stream - we will store 5 of them - use PeerID as Key

        public App()
        {
            InitializeComponent();

            // As soon as possible we want the device density
            double density = Helper.GetDensity();

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

            if (ApplicationInfo.UseTestEnvironment)
                SdkWrapper = new TestSdkWrapper();
            else
                SdkWrapper = new RainbowSdkWrapper();


            this.ModalPopped += App_ModalPopped;
            this.ModalPopping += App_ModalPopping;

            this.ModalPushed += App_ModalPushed;
            this.ModalPushing += App_ModalPushing;

            MainPage = new NavigationPage(new LoginPage());
        }

        private void App_ModalPopped(object sender, ModalPoppedEventArgs e)
        {

        }


        private void App_ModalPopping(object sender, ModalPoppingEventArgs e)
        {

        }

        private void App_ModalPushed(object sender, ModalPushedEventArgs e)
        {
            
        }

        private void App_ModalPushing(object sender, ModalPushingEventArgs e)
        {

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

