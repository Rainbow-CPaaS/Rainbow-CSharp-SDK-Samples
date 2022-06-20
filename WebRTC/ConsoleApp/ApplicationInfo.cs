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

        // IF YOU WANT TO MANAGE ONLY HEADSET AND MICROPHONE
        static internal readonly Boolean USE_ONLY_MICROPHONE_OR_HEADSET = false; // If you want to use ONLY HEADSET AND MICROPHONE - in this case FFmpeg library is not necessary

        // DEFINE FOLDER PATH TO FFmpeg binaries/libraries - can be a relative path or an absolute path
        static internal readonly string FFMPEG_LIB_FOLDER_PATH = DEFAULT_VALUE; // @"C:\ffmpeg-4.4.1-full_build-shared\bin";

        // To use an audio file instead of an audio input device - the audio file path can be the same than the video file path
        // If specified (i.e. not nulll/empty) the path must be valid

        //static internal readonly String AUDIO_FILE_PATH = @"c:\media\file_example_WAV_5MG.wav";
        //static internal readonly String AUDIO_FILE_PATH = @"c:\media\big_buck_bunny.mp4";
        //static internal readonly String AUDIO_FILE_PATH = @"https://upload.wikimedia.org/wikipedia/commons/3/36/Cosmos_Laundromat_-_First_Cycle_-_Official_Blender_Foundation_release.webm";
        //static internal readonly String AUDIO_FILE_PATH = @"rtsp://wowzaec2demo.streamlock.net/vod/mp4:BigBuckBunny_115k.mov";
        static internal readonly String AUDIO_FILE_PATH = @"http://84.138.84.42:8083/cgi-bin/faststream.jpg";

        // To use a video file instead of an video input device
        // If specified (i.e. not nulll/empty) the path must be valid
        //static internal readonly String VIDEO_FILE_PATH = @"c:\media\big_buck_bunny.mp4";
        //static internal readonly String VIDEO_FILE_PATH = @"https://upload.wikimedia.org/wikipedia/commons/3/36/Cosmos_Laundromat_-_First_Cycle_-_Official_Blender_Foundation_release.webm";
        //static internal readonly String VIDEO_FILE_PATH = @"rtsp://wowzaec2demo.streamlock.net/vod/mp4:BigBuckBunny_115k.mov";
        static internal readonly String VIDEO_FILE_PATH = @"http://84.138.84.42:8083/cgi-bin/faststream.jpg";


        //// DEFINE YOUR APP_ID,  APP_SECRET_KEY and HOST_NAME used
        static internal readonly string APP_ID = DEFAULT_VALUE;
        static internal readonly string APP_SECRET_KEY = DEFAULT_VALUE;
        static internal readonly string HOST_NAME = DEFAULT_VALUE;


        //// DEFINE THE ACCOUNT USED
        static internal readonly string LOGIN_USER = DEFAULT_VALUE;
        static internal readonly string PASSWORD_USER = DEFAULT_VALUE;

    }
}
