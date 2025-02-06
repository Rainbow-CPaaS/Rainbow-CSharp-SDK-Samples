using Rainbow.SimpleJSON;

namespace BotLibrary.Model
{
    public class ExeSettings
    {
        public Boolean UseAudioVideo { get; set; }

        public String FfmpegLibFolderPath { get; set; }

        public String LogFolderPath { get; set; }
        
        public ExeSettings()
        {
            UseAudioVideo = false;
            FfmpegLibFolderPath = "";
            LogFolderPath = ".\\logs";
        }

        public static Boolean FromJsonNode(JSONNode jsonNode, out ExeSettings settings)
        {
            settings = new ();
            if (jsonNode is not null)
            {
                settings.UseAudioVideo = jsonNode["useAudioVideo"];
                settings.FfmpegLibFolderPath = jsonNode["ffmpegLibFolderPath"];

                settings.LogFolderPath = jsonNode["logFolderPath"];

                // Check validity
                if (settings.UseAudioVideo)
                {
                    if (String.IsNullOrEmpty(settings.FfmpegLibFolderPath))
                    {
                        Util.WriteErrorToConsole($"You want to use audio/video media but ffmpeg binaries folder path is not defined - config path file used:[{settings.LogFolderPath}]");
                        return false;
                    }

                    if(!Directory.Exists(settings.FfmpegLibFolderPath))
                    {
                        Util.WriteErrorToConsole($"You define ffmpeg binaries folder path but it has not be found - config path file used:[{settings.LogFolderPath}]");
                        return false;
                    }
                }

                if (String.IsNullOrEmpty(settings.LogFolderPath))
                {
                    settings.LogFolderPath = ".\\logs";
                    Util.WriteErrorToConsole($"You didn't defined a folder for log files - config path file used:[{settings.LogFolderPath}]");
                }

                try
                {
                    Directory.CreateDirectory(settings.LogFolderPath);
                }
                catch (Exception ex)
                {
                    Util.WriteErrorToConsole($"Cannot create directory to store log files - path:[{settings.LogFolderPath}] - Exception:[{ex}]");
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