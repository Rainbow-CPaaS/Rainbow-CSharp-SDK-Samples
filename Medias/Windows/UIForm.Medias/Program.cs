using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDK.UIForm.WebRTC
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Create Configuration object and load config from file
            var configuration = Configuration.Instance;

            // We have to init external libraries
            if (configuration.Initialized)
            {
                Rainbow.Medias.Helper.InitExternalLibraries(configuration.FFmpegLibPath);

                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FormMediaInputStreams());
            }
        }
    }
}
