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

        static internal String appId = DEFAULT_VALUE;
        static internal String appSecret = DEFAULT_VALUE;
        static internal String hostname = DEFAULT_VALUE;

        static internal List<Account>? botsVideoBroadcaster = null;
        static internal Account? botVideoOrchestrator = null;
        static internal Boolean botVideoOrchestratorAutoJoinConference = false;

        static internal int nbMaxVideoBroadcaster = -1;

        static internal String labelOrchestrator = "Orchestrator";
        static internal String labelTitle = DEFAULT_VALUE;
        static internal String labelTitleEnd = DEFAULT_VALUE;
        static internal String labelSet = DEFAULT_VALUE;
        static internal String labelStop = DEFAULT_VALUE;
        
        static internal String labelNone = DEFAULT_VALUE;
        static internal String labelNoVideo = DEFAULT_VALUE;
        static internal String labelNoConferenceInProgress = DEFAULT_VALUE;
        static internal String labelUseSharingStream = DEFAULT_VALUE;

        static internal List<String>? labelVideosUriName = null;
        static internal List<String>? labelBotsVideoBroadcasterName = null;

        static internal List<Video>? videos = null;
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
