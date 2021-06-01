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

            InitLogs();

            Application.Run(new MassProvisioningForm());
        }

        static void InitLogs()
        {
            String logFileName = "RainbowMassProvisioning.log"; // File name of the log file
            String archiveLogFileName = "RainbowMassProvisioning_{###}.log"; // File name of archive log file

            //String logConfigFilePath = @"..\..\..\log4netConfiguration.xml"; // File path to log configuration
            String logConfigFilePath = @"..\..\..\..\NLogConfiguration.xml"; // File path to log configuration

            String logConfigContent; // Content of the log file configuration
            String logFolderPath = ""; // Folder path which will contains log files;
            String logFullPathFileName = ""; // Full path to log file
            String archiveLogFullPathFileName = ""; // Full path to archive file

            try
            {
                // Set folder path where log files are stored
                logFolderPath = Path.GetDirectoryName(Application.ExecutablePath);

                // Set full path to log file name
                logFullPathFileName = Path.Combine(logFolderPath, logFileName);
                archiveLogFullPathFileName = Path.Combine(logFolderPath, archiveLogFileName);

                // Get content of the log file configuration
                using (FileStream stream = File.OpenRead(logConfigFilePath))
                {
                    logConfigContent = File.ReadAllText(logConfigFilePath, System.Text.Encoding.UTF8);
                }

                // Load XML in XMLDocument
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(logConfigContent);

                // Set full path to log file in XML element
                XmlElement targetElement = doc["nlog"]["targets"]["target"];
                targetElement.SetAttribute("name", Rainbow.LogConfigurator.GetRepositoryName());
                targetElement.SetAttribute("fileName", logFullPathFileName);
                targetElement.SetAttribute("archiveFileName", archiveLogFullPathFileName);

                XmlElement loggerElement = doc["nlog"]["rules"]["logger"];
                loggerElement.SetAttribute("writeTo", Rainbow.LogConfigurator.GetRepositoryName());

                Boolean intitlog = Rainbow.LogConfigurator.Configure(doc.OuterXml);
            }
            catch { }

        }
    }
}
