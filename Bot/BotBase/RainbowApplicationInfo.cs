using System;
using System.Collections.Generic;
using System.Text;

namespace RainbowBotBase
{
    internal static class RainbowApplicationInfo
    {
        static internal readonly string DEFAULT_VALUE = "TO DEFINE";  // /!\ DON'T MODIFY THIS

        // NLog configuration file used to log SDK debug information
        static internal readonly String NLOG_CONFIG_FILE_PATH = @"..\..\..\..\..\NLogConfiguration.xml";

        static internal readonly String AUTOMATIC_ANSWER_P2P = "Automatic answer ! I'm a Bot using SDK C# :). Thanks for your message ! [P2P]";
        static internal readonly String AUTOMATIC_ANSWER_BUBBLE = "Automatic answer ! I'm a Bot using SDK C# :). Thanks for your message ! [Bubble]";
        static internal readonly int MESSAGE_READ_BY_SECOND = 10; // nb messages READ by seconds. We will wait the necessary delay to read the next one. No message are lost. They are just read step by step avoiding a flood

        // DEFINE YOUR APP_ID,  APP_SECRET_KEY,  HOST_NAME etc...
        static internal readonly string APP_ID = DEFAULT_VALUE;
        static internal readonly string APP_SECRET_KEY = DEFAULT_VALUE;
        static internal readonly string HOST_NAME = DEFAULT_VALUE;

        static internal readonly string LOGIN_MASTER_BOT = DEFAULT_VALUE; // This account can send the STOP_MESSAGE to the BOT to stop the process
        static internal readonly String STOP_MESSAGE = "Bot please stop";

        static internal readonly string LOGIN_BOT = DEFAULT_VALUE;
        static internal readonly string PASSWORD_BOT = DEFAULT_VALUE;
    }
}
