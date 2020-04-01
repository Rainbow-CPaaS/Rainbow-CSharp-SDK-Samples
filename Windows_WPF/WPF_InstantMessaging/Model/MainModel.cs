using System;
using System.Threading;
using System.Threading.Tasks;

using InstantMessaging.Helpers;

using Rainbow;
using Rainbow.Model;

using log4net;

namespace InstantMessaging.Model
{
    public class MainModel : ObservableObject
    {
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(LoginModel));
        App currentApplication = (App)System.Windows.Application.Current;

        public MainModel()
        {

        }
    }
}
