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

        // DEFINE YOUR APP_ID,  APP_SECRET_KEY,  HOST_NAME etc ...
        static internal readonly string APP_ID = DEFAULT_VALUE;
        static internal readonly string APP_SECRET_KEY = DEFAULT_VALUE;
        static internal readonly string HOST_NAME = DEFAULT_VALUE;

        static internal readonly string LOGIN_MASTER_BOT = DEFAULT_VALUE;

        static internal readonly string LOGIN_BOT = DEFAULT_VALUE;
        static internal readonly string PASSWORD_BOT = DEFAULT_VALUE;

        static internal readonly List<String> USER_LIST_FOR_MCQ = new List<String>() { "email1@myCompany.com", "email2@myCompany.com" };
    }
}
