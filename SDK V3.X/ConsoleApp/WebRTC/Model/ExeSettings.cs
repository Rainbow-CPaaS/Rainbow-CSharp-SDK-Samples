using Rainbow.SimpleJSON;

namespace ConsoleWebRTC.Model
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
            LogFolderPath = ".\\";
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
                        Util.WriteRed($"You want to use audio/video media but ffmpeg binaries folder path is not defined - config path file used:[{settings.LogFolderPath}]");
                        return false;
                    }

                    if(!Directory.Exists(settings.FfmpegLibFolderPath))
                    {
                        Util.WriteRed($"You define ffmpeg binaries folder path but it has not be found - config path file used:[{settings.LogFolderPath}]");
                        return false;
                    }
                }

                if (String.IsNullOrEmpty(settings.LogFolderPath))
                {
                    Util.WriteRed($"You didn't defined a folder for log files - config path file used:[{settings.LogFolderPath}]");
                    return false;
                }

                try
                {
                    Directory.CreateDirectory(settings.LogFolderPath);
                }
                catch (Exception ex)
                {
                    Util.WriteRed($"Cannot create directory to store log files - path:[{settings.LogFolderPath}] - Exception:[{ex}]");
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