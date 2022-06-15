using System;
using System.Collections.Generic;
using System.Text;

namespace Sample_P2PandConference
{
    public static class ApplicationInfo
    {
        static internal readonly string DEFAULT_VALUE = "TO DEFINE";  // /!\ DON'T MODIFY THIS

        // NLog configuration file used to log SDK debug information
        static internal readonly String NLOG_CONFIG_FILE_PATH = @"..\..\..\..\..\..\NLogConfiguration.xml";

        // Paht to FFmpeg libraries / binaries
        static internal readonly String FFMPEG_LIB_PATH = DEFAULT_VALUE; // @"C:\ffmpeg-4.4.1-full_build-shared\bin";

        // DEFINE YOUR APP_ID,  APP_SECRET_KEY and HOST_NAME used
        static internal readonly string APP_ID = DEFAULT_VALUE;
        static internal readonly string APP_SECRET_KEY = DEFAULT_VALUE;
        static internal readonly string HOST_NAME = DEFAULT_VALUE;

        // DEFINE THE ACCOUNT USED
        static internal string LOGIN_USER = DEFAULT_VALUE;
        static internal string PASSWORD_USER = DEFAULT_VALUE;
    }
}
