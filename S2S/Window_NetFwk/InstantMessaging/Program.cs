using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Sample_InstantMessaging
{
    static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            InitLogs();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SampleInstantMessagingForm());
        }

        static void InitLogs()
        {
            String logFileName = "RainbowSampleInstantMessaging.log"; // File name of the log file
            String logConfigFilePath = @".\..\..\log4netConfiguration.xml"; // File path to log configuration

            String logConfigContent; // Content of the log file configuration
            String logFolderPath = ""; // Folder path which will contains log files;
            String logFullPathFileName = ""; // Full path to log file

            try
            {

                // Set folder path where log files are stored
                logFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                // Set full path to log file name
                logFullPathFileName = Path.Combine(logFolderPath, logFileName);

                // Get content of the log file configuration
                using (FileStream stream = File.OpenRead(logConfigFilePath))
                {
                    logConfigContent = File.ReadAllText(logConfigFilePath, System.Text.Encoding.UTF8);
                }

                // Load XML in XMLDocument
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(logConfigContent);

                // Set full path to log file in XML element
                XmlElement fileElement = doc["log4net"]["appender"]["file"];
                fileElement.SetAttribute("value", logFullPathFileName);

                log4net.Repository.ILoggerRepository repository = Rainbow.LogConfigurator.GetRepository();
                log4net.Config.XmlConfigurator.Configure(repository, doc.DocumentElement);
            }
            catch { }

        }
    }
}
