using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using WpfSSOSamples.Helpers;
using WpfSSOSamples.ViewModel;

using Rainbow;
using Rainbow.Model;

using WpfSSOSamples;

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

