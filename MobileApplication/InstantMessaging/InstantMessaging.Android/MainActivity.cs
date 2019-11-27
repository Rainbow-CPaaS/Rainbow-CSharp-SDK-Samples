using System;
using System.IO;
using System.Xml;

using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using InstantMessaging.Helpers;


using Rainbow.Helpers;
using Rainbow.Droid.Helpers;


namespace InstantMessaging.Droid
{
    [Activity(Label = "InstantMessaging", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            InitLogs();

            AvatarPool avatarPool = AvatarPool.Instance;
            avatarPool.SetImageManagement(new ImageManagement());

            LoadApplication(new App());
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
                AssetManager assets = this.Assets;
                using (StreamReader sr = new StreamReader(assets.Open(logConfigFilePath)))
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
            catch { }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}