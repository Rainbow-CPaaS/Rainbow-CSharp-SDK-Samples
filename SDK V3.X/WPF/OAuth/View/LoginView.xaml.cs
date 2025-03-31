using System.Windows;

using WpfSSOSamples.Helpers;
using WpfSSOSamples.ViewModel;

namespace WpfSSOSamples.View
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : System.Windows.Window
    {
        App currentApplication = (App)System.Windows.Application.Current;

        LoginViewModel model = null;

        public LoginView()
        {
            InitializeComponent();

            // Define the Icon of the Window
            this.Icon = Helper.GetBitmapFrameFromResource("icon_32x32.png");

            this.Loaded += LoginView_Loaded;
            this.Closing += LoginView_Closing;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
         
            if(e.Property.Name == "IsActive")
                model?.ViewGotFocus();
        }

        
        //private void LoginView_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    model?.ViewGotFocus();
        //}

        private void LoginView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            model?.ViewClosing();
        }

        private void LoginView_Loaded(object sender, RoutedEventArgs e)
        {
            model = new LoginViewModel();
            DataContext = model;
        }
    }
}

