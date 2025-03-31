using Microsoft.Extensions.Logging;
using Rainbow.Example.Common;
using Rainbow.SimpleJSON;
using System;
using System.IO;
using System.Windows;
using WpfSSOSamples.View;

namespace WpfSSOSamples
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private readonly String LogFolderName = "Rainbow.CSharp.SDK";

        private static ILogger log;

        // Define all Rainbow objects we use
        internal Rainbow.Application RbApplication = null;

        internal LoginView LoginWindow = null;

        internal ExeSettings? exeSettings = null;
        internal Credentials? credentials = null;

    #region EVENTS/OVERRIDE OF APPLICATION OBJECT
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if ((!ReadExeSettings()) || exeSettings is null)
            {
                // TODO
            }

            if ((!ReadCredentials()) || credentials is null)
            {
                // TODO
            }

            // Get prefix and use it to add a logger
            var prefix = credentials.UsersConfig[0].Prefix;
            NLogConfigurator.AddLogger(prefix);

            log = Rainbow.LogFactory.CreateLogger<App>(prefix);

            RbApplication = new Rainbow.Application(exeSettings.LogFolderPath, prefix + ".ini", prefix);
            RbApplication.Restrictions.LogRestRequest = true;

            RbApplication.SetApplicationInfo(credentials.ServerConfig.AppId, credentials.ServerConfig.AppSecret);
            RbApplication.SetHostInfo(credentials.ServerConfig.HostName);

            LoginWindow = new LoginView();
            LoginWindow.Show();

        }
#endregion EVENTS/OVERRIDE OF APPLICATION OBJECT

        private Boolean ReadExeSettings()
        {
            String exeSettingsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}exeSettings.json";
            if (!File.Exists(exeSettingsFilePath))
                return false;

            String jsonConfig = File.ReadAllText(exeSettingsFilePath);
            var jsonNode = JSON.Parse(jsonConfig);

            if ((jsonNode is null) || (!jsonNode.IsObject))
                return false;

            if (ExeSettings.FromJsonNode(jsonNode["exeSettings"], out exeSettings))
            {
                // Set where log files must be stored
                NLogConfigurator.Directory = exeSettings.LogFolderPath;
            }
            else
            {
                return false;
            }

            return true;
        }

        private Boolean ReadCredentials(string fileName = "credentials.json")
        {
            var credentialsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}{fileName}";
            if (!File.Exists(credentialsFilePath))
                return false;

            String jsonConfig = File.ReadAllText(credentialsFilePath);
            var jsonNode = JSON.Parse(jsonConfig);

            if (!Credentials.FromJsonNode(jsonNode["credentials"], out credentials))
                return false;

            return true;
        }


    }
}
