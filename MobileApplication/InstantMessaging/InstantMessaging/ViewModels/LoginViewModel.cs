using System;
using System.ComponentModel;
using Xamarin.Forms;

using Rainbow.Helpers;

namespace InstantMessaging
{
    public class LoginViewModel : ObservableObject
    {
        string connect;
        Boolean isNotBusy;
        Boolean isBusy;

        public String Connect
        {
            get { return connect; }
            set { SetProperty(ref connect, value); }
        }

        public Boolean IsNotBusy
        {
            get { return isNotBusy; }
            set { SetProperty(ref isNotBusy, value);
                if (IsBusy == isNotBusy)
                    IsBusy = !isNotBusy;
            }
        }

        public Boolean IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value);
                if (IsNotBusy == isBusy)
                    IsNotBusy = !isBusy;
            }
        }
    }
}
