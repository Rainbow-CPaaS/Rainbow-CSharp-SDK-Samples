using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Sample_Contacts
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
            Application.Run(new SampleContactForm());
        }

        static void InitLogs()
        {
            String logFileName = "RainbowSampleContacts.log"; // File name of the log file
            String archiveLogFileName = "RainbowSampleContacts_{###}.log"; // File name of the archive log file
            String logConfigFilePath = @".\..\..\NLogConfiguration.xml"; // File path to log configuration

            String logConfigContent; // Content of the log file configuration
            String logFolderPath = ""; // Folder path which will contains log files;
            String logFullPathFileName = ""; // Full path to log file
            String archiveLogFullPathFileName; ; // Full path to archive log file 

            try
            {
                // Set folder path where log files are stored
                logFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

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
                targetElement.SetAttribute("name", Rainbow.LogConfigurator.GetRepositoryName());    // Set target name equals to RB repository name
                targetElement.SetAttribute("fileName", logFullPathFileName);                        // Set full path to log file
                targetElement.SetAttribute("archiveFileName", archiveLogFullPathFileName);          // Set full path to archive log file

                XmlElement loggerElement = doc["nlog"]["rules"]["logger"];
                loggerElement.SetAttribute("writeTo", Rainbow.LogConfigurator.GetRepositoryName()); // Write to RB repository name

                // Set the configuration
                Rainbow.LogConfigurator.Configure(doc.OuterXml);
            }
            catch { }

        }
    }
}
