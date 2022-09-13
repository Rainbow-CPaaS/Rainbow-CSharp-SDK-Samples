using System;
using System.Collections.Generic;
using System.IO;

namespace BotVideoOrchestratorAndBroadcaster
{

    internal static class RainbowApplicationInfo
    {
        static internal readonly string DEFAULT_VALUE = "TO DEFINE";  // /!\ DON'T MODIFY THIS

        // NLog configuration file used to log SDK debug information
        static internal readonly String NLOG_CONFIG_FILE_PATH = $".{Path.DirectorySeparatorChar}Resources{Path.DirectorySeparatorChar}NLogConfiguration.xml";

        static internal String ffmpegLibFolderPath = DEFAULT_VALUE;

        static internal List<BotManager>? botManagers = null;
        static internal String loginMasterBot = DEFAULT_VALUE;
        static internal String commandMenu = DEFAULT_VALUE;
        static internal String commandStop = DEFAULT_VALUE;

        static internal String labelsFilePath = DEFAULT_VALUE;

        static internal List<String>? videosUri = null;

        static internal String appId = DEFAULT_VALUE;
        static internal String appSecret = DEFAULT_VALUE;
        static internal String hostname = DEFAULT_VALUE;

        static internal List<Account>? botsVideoBroadcaster = null;
        static internal Account? botVideoOrchestrator = null;
        static internal Boolean botVideoOrchestratorAutoJoinConference = false;

        static internal int nbMaxVideoBroadcaster = -1;

        static internal String labelOrchestrator = "Orchestrator";
        static internal String labelTitle = DEFAULT_VALUE;
        static internal String labelSet = DEFAULT_VALUE;
        static internal String labelSetAction = DEFAULT_VALUE;
        static internal String labelNone = DEFAULT_VALUE;
        static internal String labelNoVideo = DEFAULT_VALUE;
        static internal String labelNoConferenceInProgress = DEFAULT_VALUE;
        static internal String labelUseSharingStream = DEFAULT_VALUE;

        static internal List<String>? labelVideosUriName = null;
        static internal List<String>? labelBotsVideoBroadcasterName = null;


        /////////////////////////////////////////////////////////
        // NET ENVIRONMENT

        //static internal String FFMPEG_LIB_FOLDER_PATH = @"C:\ffmpeg-4.4.1-full_build-shared\bin";

        //static internal String LOGIN_MASTER_BOT = "irlesuser1@sophia.com";

        //static internal String LABELS_FILE_PATH = "label_EN.json";

        //static internal List<String> VIDEOS = new List<string>() { "http://5.48.50.194:8081/mjpg/video.mjpg", "http://78.230.0.54:81/mjpg/video.mjpg", "http://178.194.8.37:9000/mjpg/video.mjpg", "http://212.114.24.142/cgi-bin/faststream.jpg", "http://80.14.131.195/mjpg/video.mjpg" };

        //static internal String APP_ID = "6f8c5910725b11e9b55c81be00bebc2c";
        //static internal String APP_SECRET_KEY = "5CkS7sRKU5NgGjZ8DfFjHnKkHv0LrpcVqVcunCJ5XQvi3ZbQdNvuUMgYUYk0KCnQ";
        //static internal String HOST_NAME = "openrainbow.net";

        //static internal Account BOT_VIDEO_ORCHESTRATOR = new Account("cctvmoderator@compirles.com", "Rainbow1!");

        //static internal int NB_MAX_BOT_VIDEO_BROADCASTER = -1;

        //static internal List<Account> LIST_BOT_VIDEO_BROADCASTER = new List<Account>() { new Account("cctvstream01@compirles.com", "Rainbow1!"),
        //                                                                                    new Account("cctvstream02@compirles.com", "Rainbow1!"),
        //                                                                                    new Account("cctvstream03@compirles.com", "Rainbow1!"),
        //                                                                                    new Account("cctvstream04@compirles.com", "Rainbow1!")};

    }

    internal class Account
    {
        public String Login;
        public String Password;

        public Account(String login, String password)
        {
            Login = login;
            Password = password;
        }
    }
}
