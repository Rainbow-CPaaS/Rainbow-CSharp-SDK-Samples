using System;
using System.Threading;
using System.Threading.Tasks;

using InstantMessaging.Helpers;

using Rainbow;
using Rainbow.Model;

using Microsoft.Extensions.Logging;

namespace InstantMessaging.Model
{
    public class MainModel
    {
        private static readonly ILogger log = Rainbow.LogFactory.CreateLogger<MainModel>();
        App currentApplication = (App)System.Windows.Application.Current;

        public MainModel()
        {

        }
    }
}
