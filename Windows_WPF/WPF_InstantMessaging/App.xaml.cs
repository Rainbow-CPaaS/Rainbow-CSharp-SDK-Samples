using System;
using System.IO;
using System.Windows;
using System.Xml;

using InstantMessaging.Helpers;
using InstantMessaging.Pool;
using InstantMessaging.View;

using Rainbow;

using log4net;

namespace InstantMessaging
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private readonly String LogfileName = "Rainbow_CSharp_WPF_Example.log";
        private readonly String Log4NetConfigurationfileName = "log4netConfiguration.xml";
        private readonly String LogFolderName = "Rainbow.CSharp.SDK";

        private ILog log;

        // Define all Rainbow objects we use
        internal Rainbow.Application RbApplication = null;
        internal Rainbow.Bubbles RbBubbles = null;
        internal Rainbow.Contacts RbContacts = null;
        internal Rainbow.Conversations RbConversations = null;
        internal Rainbow.Favorites RbFavorites = null;
        internal Rainbow.InstantMessaging RbInstantMessaging = null;
        internal Rainbow.FileStorage RbFileStorage = null;

        internal String CurrentUserId;
        internal String CurrentUserJid;

        // Define a way to use dummy data
        internal Boolean USE_DUMMY_DATA = false;
        internal Boolean USE_LOGIN_FORM_WITH_DUMMY_DATA = false;
        internal Boolean USE_AVATAR_CACHE = true;
        internal Boolean USE_FILE_CACHE = false;

        // Define global windows
        internal LoginView LoginWindow = null;
        internal MainView ApplicationMainWindow = null;

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

            RbBubbles = RbApplication.GetBubbles();
            RbContacts = RbApplication.GetContacts();
            RbConversations = RbApplication.GetConversations();
            RbFavorites = RbApplication.GetFavorites();
            RbInstantMessaging = RbApplication.GetInstantMessaging();
            RbFileStorage = RbApplication.GetFileStorage();

            InitAvatarPool();
            InitFilePool();


            if ((USE_DUMMY_DATA && USE_LOGIN_FORM_WITH_DUMMY_DATA)
                || (!USE_DUMMY_DATA))
            {
                LoginWindow = new LoginView();
                LoginWindow.Show();
            }
            else if (USE_DUMMY_DATA)
            {
                ApplicationMainWindow = new MainView();
                ApplicationMainWindow.Show();
            }
        }
        #endregion EVENTS/OVERRIDE OF APPLICATION OBJECT

        #region INIT METHODS - Logs / Avatar / FilePool
        private void InitLogs()
        {
            String logFileName = LogfileName; // File name of the log file
            String logConfigFileName = Log4NetConfigurationfileName; // File path to log configuration

            String logConfigContent; // Content of the log file configuration
            String logFullPathFileName; // Full path to log file

            try
            {
                // Set full path to log file name
                logFullPathFileName = Path.Combine(Helper.GetTempFolder(), LogFolderName, logFileName);

                // FOR TEST PURPOSE ONLY
                //if (File.Exists(logFullPathFileName))
                //{
                //    String content = File.ReadAllText(logFullPathFileName);
                //    File.Delete(logFullPathFileName);
                //}

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
                XmlElement fileElement = doc["log4net"]["appender"]["file"];
                fileElement.SetAttribute("value", logFullPathFileName);

                log4net.Repository.ILoggerRepository repository = Rainbow.LogConfigurator.GetRepository();
                log4net.Config.XmlConfigurator.Configure(repository, doc.DocumentElement);
            }
            catch { }
        }

        private void InitAvatarPool()
        {
            // Get AvatarPool and initialize it
            string avatarsFolderPath = Path.Combine(Helper.GetTempFolder(), LogFolderName, "Avatars");

            if (!USE_AVATAR_CACHE)
            {
                if (Directory.Exists(avatarsFolderPath))
                    Directory.Delete(avatarsFolderPath, true);
            }

            AvatarPool avatarPool = AvatarPool.Instance;
            avatarPool.SetAvatarSize(60);
            avatarPool.SetFont("Arial", 11);

            avatarPool.SetFolderPath(avatarsFolderPath);
            avatarPool.SetApplication(ref RbApplication);
            
            avatarPool.SetImageManagement(new ImageManagement());
        }

        private void InitFilePool()
        {
            // Get FilePool and initialize it
            string filePoolFolderPath = Path.Combine(Helper.GetTempFolder(), LogFolderName, "FileStorage");

            FilePool filePool = FilePool.Instance;

            if(!USE_FILE_CACHE)
            {
                if (Directory.Exists(filePoolFolderPath))
                    Directory.Delete(filePoolFolderPath, true);
            }

            filePool.SetFolderPath(filePoolFolderPath);
            filePool.SetApplication(ref RbApplication);

            filePool.AllowAutomaticThumbnailDownload = true;
            filePool.AutomaticDownloadLimitSizeForImages = 250 * 1024; // 250 Ko
        }
        #endregion INIT METHODS - Logs / Avatar / FilePool

        #region UI APPLICATION METHODS
        internal void HideLoginWindow()
        {
            if (LoginWindow != null)
            {
                // Need to be on UI Thread
                LoginWindow.Dispatcher.Invoke(new Action(() =>
                {
                    LoginWindow.Hide();
                }));
            }
            else
                log.ErrorFormat("[HideLoginWindow] LoginWindow is null !");
        }

        internal void ShowApplicationMainWindow()
        {
            App currentApplication = (App)System.Windows.Application.Current;

            // Does the ApplicationMainWindow has been already created ?
            if (currentApplication.ApplicationMainWindow == null) 
            {
                // Need to be on UI Thread
                currentApplication.LoginWindow.Dispatcher.Invoke(new Action(() =>
                {
                    currentApplication.ApplicationMainWindow = new MainView();
                    currentApplication.ApplicationMainWindow.Show();
                }));
            }
            // Does the ApplicationMainWindow has been already created BUT has beend previoulsy closed ?
            //else if (!currentApplication.ApplicationMainWindow.IsLoaded)
            //{
            //    // Need to be on UI Thread
            //    currentApplication.LoginWindow.Dispatcher.Invoke(new Action(() =>
            //    {
            //        currentApplication.ApplicationMainWindow = null;
            //        currentApplication.ApplicationMainWindow = new MainView();
            //        currentApplication.ApplicationMainWindow.Show();
            //    }));
            //}
            // The ApplicationMainWindow exists and just need to be displayd in foreground
            else
            {
                // Need to be on UI Thread
                currentApplication.ApplicationMainWindow.Dispatcher.Invoke(new Action(() =>
                {
                    if (currentApplication.ApplicationMainWindow.IsVisible)
                    {
                        if (currentApplication.ApplicationMainWindow.WindowState == WindowState.Minimized)
                            currentApplication.ApplicationMainWindow.WindowState = WindowState.Normal;
                        currentApplication.ApplicationMainWindow.Activate();
                    }
                    else
                        currentApplication.ApplicationMainWindow.Show();
                }));
            }
        }
        #endregion UI APPLICATION METHODS
    }
}
