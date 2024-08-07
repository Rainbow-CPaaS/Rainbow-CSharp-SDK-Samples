using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MassProvisioning
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            InitLogsWithNLog();

            Application.Run(new MassProvisioningForm());
        }

        static Boolean InitLogsWithNLog()
        {
            String logConfigFilePath = @"..\..\..\NLogConfiguration.xml"; // File path to log configuration (or you could alos use an embedded resource)

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
