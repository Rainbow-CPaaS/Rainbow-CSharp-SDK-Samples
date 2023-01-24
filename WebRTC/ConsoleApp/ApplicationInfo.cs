using System;
using System.Collections.Generic;
using System.Text;

namespace SDK.ConsoleApp.WebRTC
{
    public static class ApplicationInfo
    {
        static internal readonly string DEFAULT_VALUE = "TO DEFINE";  // /!\ DON'T MODIFY THIS

        // NLog configuration file used to log SDK debug information
        static internal readonly String NLOG_CONFIG_FILE_PATH = @"..\..\..\..\..\NLogConfiguration.xml";

        // DEFINE FOLDER PATH TO FFmpeg binaries/libraries - can be a relative path or an absolute path
        static internal readonly string FFMPEG_LIB_FOLDER_PATH = DEFAULT_VALUE; //@"C:\ffmpeg-5.1.1-full_build-shared\bin";

        // To use an audio file instead of an audio input device - the audio file path can be the same than the video file path
        // If specified (i.e. not nulll/empty) the path must be valid

        static internal readonly String AUDIO_FILE_PATH = DEFAULT_VALUE; //@"c:\media\Joy_and_Heron.webm";

        // To use a video file instead of an video input device
        // If specified (i.e. not nulll/empty) the path must be valid
        static internal readonly String VIDEO_FILE_PATH = DEFAULT_VALUE; //@"c:\media\Joy_and_Heron.webm";


        // DEFINE YOUR APP_ID,  APP_SECRET_KEY and HOST_NAME used
        static internal string APP_ID = DEFAULT_VALUE;
        static internal string APP_SECRET_KEY = DEFAULT_VALUE;
        static internal string HOST_NAME = DEFAULT_VALUE;


        //// DEFINE THE ACCOUNT USED
        static internal string LOGIN_USER = DEFAULT_VALUE;
        static internal string PASSWORD_USER = DEFAULT_VALUE;
    }
}
