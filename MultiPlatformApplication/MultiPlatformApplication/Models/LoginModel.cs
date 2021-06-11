using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms.Xaml;

using System.Windows.Input;
using MultiPlatformApplication.Helpers;

namespace MultiPlatformApplication.Models
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class LoginModel : ObservableObject
    {
        string m_connect;
        Boolean m_isBusy;
        string m_login;
        string m_password;

        RelayCommand<object> m_buttonConnectCommand;

        public String Connect
        {
            get { return m_connect; }
            set { SetProperty(ref m_connect, value); }
        }

        public Boolean IsBusy
        {
            get { return m_isBusy; }
            set { SetProperty(ref m_isBusy, value); }
        }

        public String Login
        {
            get { return m_login; }
            set { SetProperty(ref m_login, value); }
        }

        public String Password
        {
            get { return m_password; }
            set { SetProperty(ref m_password, value); }
        }

        public RelayCommand<object> ButtonConnectCommand
        {
            get { return m_buttonConnectCommand; }
            set { SetProperty(ref m_buttonConnectCommand, value); }
        }

        //public LoginModel()
        //{
        //    Connect = "connect";
        //    IsBusy = false;
        //    Login = "";
        //    Password = "";
        //}

    }
}
