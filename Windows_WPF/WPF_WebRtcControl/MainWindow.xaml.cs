using System.IO;
using System.Windows;

using SDK.WpfApp.ViewModel;

using Rainbow;

using NLog;

namespace SDK.WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(MainWindow));

        private AppViewModel AppViewModel = null;

        private App currentApplication;

        private Rainbow.Application rbApplication;
        private Rainbow.Common.Avatars rbAvatars;
        


    #region CONSTRUCTOR
        public MainWindow()
        {
            // Get currentWpf Application
            currentApplication = System.Windows.Application.Current as App;

            // Get Rainbow Application Object
            rbApplication = currentApplication.rbApplication;

            // Create View Model
            AppViewModel = new AppViewModel();

            InitializeComponent();

            // Specify the Rainbow.Application object in the WebRtc Control
            webRtcControl.SetApplication(rbApplication);

            // Initialize avatar service
            InitAvatarService();

            // Specify Host Name - it's displayed when using sharing - if host name is invalid a default one is used instead
            webRtcControl.SetHostName("csharp-sample");

            AppViewModel.SetRbApplication(rbApplication);
            AppViewModel.SetWebRtcControl(webRtcControl);

            // TODO - remove this
            AppViewModel.LoginInfoModel.Login = App.LOGIN_USER1;
            AppViewModel.LoginInfoModel.Password = App.PASSWORD_USER1;

            this.Loaded += MainWindow_Loaded;
        }

    #endregion CONSTRUCTOR

        private void InitAvatarService()
        {
            // We create Avatars service
            rbAvatars = Rainbow.Common.Avatars.Instance;
            rbAvatars.SetApplication(rbApplication);
            rbAvatars.SetFolderPathUsedToStoreAvatars(Path.Combine(webRtcControl.GetResourcesFolderPath(), "Avatars"));
            rbAvatars.AllowAvatarDownload(true);
            rbAvatars.AllowToAskInfoForUnknownContact(true);
            rbAvatars.AllowToAskInfoAboutUnknowBubble(true);

            rbAvatars.ContactAvatarUpdated += Avatars_ContactAvatarUpdated;

            if (!rbAvatars.Initialize())
                log.Error("CANNOT initialize Avatars service ...");

            // We can now specify the avatars path
            webRtcControl.SetContactAvatarFolderPath(rbAvatars.GetAvatarsFolderPath(Rainbow.Common.Avatars.AvatarType.ROUNDED, true));
            webRtcControl.SetUnknownAvatarFilePath(rbAvatars.GetUnknwonAvatarFilePath()); ;
        }

    #region EVENT FROM AVATARS SERVICE
        private void Avatars_ContactAvatarUpdated(object sender, Rainbow.Events.IdEventArgs e)
        {
            if (e.Id == AppViewModel.UsersModel.CurrentContactId)
                webRtcControl.UpdateContactAvatarDisplay(e.Id);
        }

    #endregion EVENT FROM AVATARS SERVICE

    #region CURRENT WINDOW EVENTS

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = AppViewModel;
        }
    
#endregion CURRENT WINDOW EVENTS

    }
}
