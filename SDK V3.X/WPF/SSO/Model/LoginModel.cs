using System;

using WpfSSOSamples.Helpers;

using System.Windows;

namespace WpfSSOSamples.Model
{
    public class LoginModel: ObservableObject
    {
#region PROPERTIES

        string m_login;
        public String Login
        {
            get { return m_login; }
            set { 
                SetProperty(ref m_login, value); 
            }
        }

        string m_password;
        public string Password
        {
            get { return m_password; }
            set { SetProperty(ref m_password, value); }
        }

        string m_connect;
        public String Connect
        {
            get { return m_connect; }
            set { SetProperty(ref m_connect, value); }
        }

        Boolean m_isBusy;
        public Boolean IsBusy
        {
            get { return m_isBusy; }
            set { SetProperty(ref m_isBusy, value); }
        }

        Boolean m_askingPassword;
        public Boolean AskingPassword
        {
            get { return m_askingPassword; }
            set { SetProperty(ref m_askingPassword, value); }
        }


        String m_errorString;
        public String ErrorString
        {
            get { return m_errorString; }
            set { SetProperty(ref m_errorString, value); }
        }

        Visibility m_errorIsVisible;
        public Visibility ErrorIsVisible
        {
            get { return m_errorIsVisible; }
            set { SetProperty(ref m_errorIsVisible, value); }
        }

#endregion PROPERTIES

        public LoginModel()
        {

        }
    }
}