using Rainbow.SimpleJSON;

namespace Rainbow.Example.Common
{
    public class ExeSettings
    {
        public Boolean UseAudioVideo { get; set; }

        public String FfmpegLibFolderPath { get; set; }

        public String LogFolderPath { get; set; }

        public Boolean LogOnConsole { get; set; }

        public String S2SCallbackURL { get; set; }

        public String CultureInfo { get; set; }

        public ExeSettings()
        {
            UseAudioVideo = false;
            FfmpegLibFolderPath = "";
            LogFolderPath = "./";
            LogOnConsole = false;
            S2SCallbackURL = "";
            CultureInfo = "";
        }

        public static Boolean FromJsonNode(JSONNode jsonNode, out ExeSettings settings)
        {
            settings = new();
            if (jsonNode is not null)
            {
                settings.UseAudioVideo = jsonNode["useAudioVideo"];
                settings.FfmpegLibFolderPath = jsonNode["ffmpegLibFolderPath"];

                settings.LogFolderPath = jsonNode["logFolderPath"];
                settings.LogOnConsole = jsonNode["logOnConsole"];

                settings.S2SCallbackURL = jsonNode["s2sCallbackURL"];

                settings.CultureInfo = jsonNode["cultureInfo"];

                // Check validity
                if (settings.UseAudioVideo)
                {
                    if (String.IsNullOrEmpty(settings.FfmpegLibFolderPath))
                        return false;

                    if (!Directory.Exists(settings.FfmpegLibFolderPath))
                        return false;
                }

                if (String.IsNullOrEmpty(settings.LogFolderPath))
                    return false;

                try
                {
                    Directory.CreateDirectory(settings.LogFolderPath);
                }
                catch
                {
                    return false;
                }
                return true;
            }
            else
                settings = new();

            return false;
        }

    }
}