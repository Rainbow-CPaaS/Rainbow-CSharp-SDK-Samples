using System;
using System.Collections.Generic;
using System.IO;

namespace BotVideoOrchestratorAndRemoteControl
{

    internal static class RainbowApplicationInfo
    {
        static internal readonly string DEFAULT_VALUE = "TO DEFINE";  // /!\ DON'T MODIFY THIS

        // NLog configuration file used to log SDK debug information
        static internal readonly String NLOG_CONFIG_FILE_PATH = $".{Path.DirectorySeparatorChar}Resources{Path.DirectorySeparatorChar}NLogConfiguration.xml";

        static internal String ffmpegLibFolderPath = DEFAULT_VALUE;

        static internal ServerConfig? serverConfig = null;

        static internal List<Manager>? managers = null;
        static internal List<Broadcaster>? broadcasters = null;
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
