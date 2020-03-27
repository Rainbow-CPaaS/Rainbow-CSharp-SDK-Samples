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

using InstantMessaging.Helpers;
using InstantMessaging.Model;

using Rainbow;
using Rainbow.Model;



namespace InstantMessaging.View
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : System.Windows.Window
    {
        App currentApplication = (App)System.Windows.Application.Current;

        public LoginView()
        {
            InitializeComponent();

            // Define the Icon of the Window
            this.Icon = Helper.GetBitmapFrameFromResource("icon_32x32.png");
        }
    }
}

