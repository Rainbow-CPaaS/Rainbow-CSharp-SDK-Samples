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
        string m_loginLabel;
        string m_password;
        string m_passwordLabel;
        Boolean m_askingLogin;

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

        public Boolean AskingLogin
        {
            get { return m_askingLogin; }
            set { SetProperty(ref m_askingLogin, value); }
        }

        public String LoginLabel
        {
            get { return m_loginLabel; }
            set { SetProperty(ref m_loginLabel, value); }
        }

        public String Login
        {
            get { return m_login; }
            set { SetProperty(ref m_login, value); }
        }

        public String PasswordLabel
        {
            get { return m_passwordLabel; }
            set { SetProperty(ref m_passwordLabel, value); }
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
