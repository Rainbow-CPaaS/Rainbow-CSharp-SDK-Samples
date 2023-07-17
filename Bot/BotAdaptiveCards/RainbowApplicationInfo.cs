using System;
using System.Collections.Generic;
using System.Text;

namespace BotAdaptiveCards
{
    internal static class RainbowApplicationInfo
    {
        static internal readonly string DEFAULT_VALUE = "TO DEFINE";  // /!\ DON'T MODIFY THIS

        // NLog configuration file used to log SDK debug information
        static internal readonly String NLOG_CONFIG_FILE_PATH = @"..\..\..\..\..\NLogConfiguration.xml";

        static internal readonly Boolean USE_ALWAYS_SAME_ADAPTIVE_CARD = true;

        //// DEFINE YOUR APP_ID,  APP_SECRET_KEY,  HOST_NAME etc ...
        static internal string APP_ID = DEFAULT_VALUE;
        static internal string APP_SECRET_KEY = DEFAULT_VALUE;
        static internal string HOST_NAME = DEFAULT_VALUE;

        static internal readonly string LOGIN_MASTER_BOT = DEFAULT_VALUE; // This account can send the STOP_MESSAGE to the BOT to stop the process
        static internal readonly String STOP_MESSAGE = "Bot please stop";

        static internal readonly string LOGIN_BOT = DEFAULT_VALUE;      // Login to use for the BOT 
        static internal readonly string PASSWORD_BOT = DEFAULT_VALUE;   // Pwd of the BOT

        // List account which must pass the MCQ test - they must be known by the BOT
        static internal readonly List<String> USER_LIST_FOR_MCQ = new List<String>() { "email1@mycompany.com", "email1@mycompany.com" };
    }
}
