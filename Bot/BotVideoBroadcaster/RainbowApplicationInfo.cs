using System;
using System.Collections.Generic;
using System.IO;

namespace BotVideoBroadcaster
{

    internal static class RainbowApplicationInfo
    {
        static internal readonly string DEFAULT_VALUE = "TO DEFINE";  // /!\ DON'T MODIFY THIS

        // NLog configuration file used to log SDK debug information
        static internal readonly String NLOG_CONFIG_FILE_PATH = $".{Path.DirectorySeparatorChar}Resources{Path.DirectorySeparatorChar}NLogConfiguration.xml";
        static internal readonly String NLOG_CONFIG_FILE_LOGGER = $".{Path.DirectorySeparatorChar}Resources{Path.DirectorySeparatorChar}NLogConfiguration-logger.txt";
        static internal readonly String NLOG_CONFIG_FILE_TARGET = $".{Path.DirectorySeparatorChar}Resources{Path.DirectorySeparatorChar}NLogConfiguration-target.txt";

        static internal String ffmpegLibFolderPath = DEFAULT_VALUE;

        static internal List<BotManager>? botManagers = null;
        static internal String commandStop = DEFAULT_VALUE;

        static internal Boolean useOnlyOneStream = false;
        static internal Boolean createDataChannel = true;

        static internal List<String>? videosUri = null;

        static internal String appId = DEFAULT_VALUE;
        static internal String appSecret = DEFAULT_VALUE;
        static internal String hostname = DEFAULT_VALUE;

        static internal List<Account>? botsVideoBroadcaster = null;
        static internal int nbMaxVideoBroadcaster = -1;
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
