using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Extensions.Logging;
using Rainbow.SimpleJSON;
using System;
using System.IO;
using System.Windows.Forms;

namespace Sample_Telephony
{
    static class Program
    {
        const string _configFileName = "config.json";

        static public String NlogConfigFilePath = "";
        static public RainbowServer RainbowServer;
        static public RainbowAccount RainbowAccount;

        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ParseConfigFile();
            InitLogsWithNLog();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SampleTelephonyForm(RainbowServer, RainbowAccount));
        }

        private static String GetConfigFilePath()
        {
            return Path.GetFullPath($".{Path.DirectorySeparatorChar}{_configFileName}");
        }

        private static void ParseConfigFile()
        {
            String message = "";
            String configFilePath = GetConfigFilePath();

            if (!File.Exists(configFilePath))
                throw new Exception("Cannot found configuration file");

            try
            {
                String jsonConfig = File.ReadAllText(configFilePath);
                var json = JSON.Parse(jsonConfig) ?? throw new Exception("Cannot parse configuration file as JSON");

                // NLog
                if (json["nlogConfigFilePath"] != null)
                    NlogConfigFilePath = Rainbow.UtilJson.AsString(json, "nlogConfigFilePath");
                else
                    message += $"\r\nCannot read 'nlogConfigFilePath' property";


                // Parse RainbowServer config
                RainbowServer = new RainbowServer();
                if (json["serverConfig"]?.IsObject == true)
                {
                    var jobject = json["serverConfig"];
                    if (!RainbowServer.TryJsonParse(jobject, out RainbowServer))
                        message += $"\r\nNo 'serverConfig' object parsed";
                }
                else
                    message += $"\r\nCannot read 'serverConfig' object";

                // Parse RainbowAccount config
                RainbowAccount = new RainbowAccount();
                if (json["userConfig"]?.IsObject == true)
                {
                    var jobject = json["userConfig"];
                    if (!RainbowAccount.TryJsonParse(jobject, out RainbowAccount))
                        message += $"\r\nNo 'userConfig' object parsed";
                }
                else
                    message += $"\r\nCannot read 'userConfig' object";

            }
            catch (Exception exc)
            {
                message += $"\r\nException:[{Rainbow.Util.SerializeException(exc)}]";

            }

            if (String.IsNullOrEmpty(message))
                return;

            message = $"Pb occured while reading/parsing this file:\r\n\t{configFilePath}" + message;
            throw new Exception(message);
        }

        static Boolean InitLogsWithNLog()
        {
            String logConfigFilePath = NlogConfigFilePath; // File path to log configuration (or you could alos use an embedded resource)

            try
            {
                if (File.Exists(logConfigFilePath))
                {
                    // Get content of the log file configuration
                    String logConfigContent = File.ReadAllText(logConfigFilePath, System.Text.Encoding.UTF8);

                    // Create NLog configuration using XML file content
                    XmlLoggingConfiguration config = XmlLoggingConfiguration.CreateFromXmlString(logConfigContent);
                    if (config.InitializeSucceeded == true)
                    {
                        // Create Logger factory
                        var factory = LoggerFactory.Create(builder => builder.AddNLog(config));

                        // Set Logger factory to Rainbow SDK
                        Rainbow.LogFactory.Set(factory);

                        return true;
                    }
                }
            }
            catch { }

            return false;
        }
    }
}
