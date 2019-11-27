using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace InstantMessaging
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        string connect;
        string status;
        string info;

        ImageSource avatarSource;

        public ImageSource AvatarSource
        {
            set {
                if (avatarSource != value)
                {
                    avatarSource = value;
                    OnPropertyChanged("AvatarSource");
                }
            }
            get { return avatarSource; }
        }

        public LoginViewModel()
        {
            Connect = "Connect";
            Status = " ";
            Info = " ";
        }

        public String Connect
        {
            set
            {
                if (connect != value)
                {
                    connect = value;
                    OnPropertyChanged("Connect");
                }
            }
            get
            {
                return connect;
            }
        }

        public String Status
        {
            set
            {
                if (status != value)
                {
                    status = value;
                    OnPropertyChanged("Status");
                }
            }
            get
            {
                return status;
            }
        }

        public String Info
        {
            set
            {
                if (info != value)
                {
                    info = value;
                    OnPropertyChanged("Info");
                }
            }
            get
            {
                return info;
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
