using System;
using System.Collections.Generic;
using System.Text;

namespace BotVideoBroadcaster
{
    internal static class RainbowApplicationInfo
    {
        static internal readonly string DEFAULT_VALUE = "TO DEFINE";  // /!\ DON'T MODIFY THIS

        // NLog configuration file used to log SDK debug information
        static internal readonly String NLOG_CONFIG_FILE_PATH = @"..\..\..\..\..\NLogConfiguration.xml";

        // DEFINE FOLDER PATH TO FFmpeg binaries/libraries - can be a relative path or an absolute path
        static internal readonly string FFMPEG_LIB_FOLDER_PATH = DEFAULT_VALUE;

        static internal readonly String APP_ID = DEFAULT_VALUE;
        static internal readonly String APP_SECRET_KEY = DEFAULT_VALUE;
        static internal readonly String HOST_NAME = DEFAULT_VALUE;            // For example "openrainbow.net" or "openrainbow.com"

        static internal readonly String LOGIN_MASTER_BOT = DEFAULT_VALUE;     // email of the master bot

        static internal readonly String STOP_MESSAGE_BOT_01 = DEFAULT_VALUE;  // For example "bot01 stop"
        static internal readonly String NAME_BOT_01 = DEFAULT_VALUE;          // For example "Video 01"
        static internal readonly String LOGIN_BOT_01 = DEFAULT_VALUE;
        static internal readonly String PASSWORD_BOT_01 = DEFAULT_VALUE;

        static internal readonly String STOP_MESSAGE_BOT_02 = DEFAULT_VALUE;  // For example "bot02 stop"
        static internal readonly String NAME_BOT_02 = DEFAULT_VALUE;          // For example "Video 02"
        static internal readonly String LOGIN_BOT_02 = DEFAULT_VALUE;
        static internal readonly String PASSWORD_BOT_02 = DEFAULT_VALUE;

        static internal readonly String VIDEO_URI_01 = DEFAULT_VALUE;         // For example a valid path to an video file, or valid URI to a CCTV
        static internal readonly String VIDEO_URI_02 = DEFAULT_VALUE;         // For example a valid path to an video file, or valid URI to a CCTV
        static internal readonly String VIDEO_URI_03 = DEFAULT_VALUE;         // For example a valid path to an video file, or valid URI to a CCTV
        static internal readonly String VIDEO_URI_04 = DEFAULT_VALUE;         // For example a valid path to an video file, or valid URI to a CCTV


    }
}
