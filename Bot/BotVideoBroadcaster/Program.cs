using NLog.Config;
using System;
using System.IO;
using System.Threading;

namespace BotVideoBroadcaster
{
    internal class Program
    {
        static void Main()
        {
            // Check done on values defined in file RainbowApplicationInfo.cs
            if (!CheckApplicationInfoValues())
                return;

            // Init log configuration
            InitLogsWithNLog();

            // Create BotVideoBroadcaster 01 && 02
            RainbowBotVideoBroadcaster botVideoBroadcaster01 = new RainbowBotVideoBroadcaster();
            RainbowBotVideoBroadcaster botVideoBroadcaster02 = new RainbowBotVideoBroadcaster();

            // Get the dot graph of this Bot - it's a String which can be used to represent the state machine diagram on a online tool like https://dreampuf.github.io/GraphvizOnline/
            string graph = botVideoBroadcaster01.ToDotGraph();

            // Configure the BotVideoBroadcaster 01
            if (!botVideoBroadcaster01.Configure(
                    RainbowApplicationInfo.APP_ID, RainbowApplicationInfo.APP_SECRET_KEY,
                    RainbowApplicationInfo.HOST_NAME,
                    RainbowApplicationInfo.LOGIN_BOT_01, RainbowApplicationInfo.PASSWORD_BOT_01,
                    RainbowApplicationInfo.NAME_BOT_01,
                    RainbowApplicationInfo.LOGIN_MASTER_BOT,
                    RainbowApplicationInfo.STOP_MESSAGE_BOT_01,
                    null,
                    RainbowApplicationInfo.VIDEO_URI_01,
                    RainbowApplicationInfo.VIDEO_URI_02,
                    "./",
                    RainbowApplicationInfo.NAME_BOT_01+ ".ini"
                    ))
            {
                Console.WriteLine("Cannot configure Bot 01 - We quit");
                return;
            }

            // Configure the BotVideoBroadcaster 02
            if (!botVideoBroadcaster02.Configure(
                    RainbowApplicationInfo.APP_ID, RainbowApplicationInfo.APP_SECRET_KEY,
                    RainbowApplicationInfo.HOST_NAME,
                    RainbowApplicationInfo.LOGIN_BOT_02, RainbowApplicationInfo.PASSWORD_BOT_02,
                    RainbowApplicationInfo.NAME_BOT_02,
                    RainbowApplicationInfo.LOGIN_MASTER_BOT,
                    RainbowApplicationInfo.STOP_MESSAGE_BOT_02,
                    null,
                    RainbowApplicationInfo.VIDEO_URI_03,
                    null,
                    "./",
                    RainbowApplicationInfo.NAME_BOT_02 + ".ini"
                    ))
            {
                Console.WriteLine("Cannot configure Bot 02 - We quit");
                return;
            }

            if (! botVideoBroadcaster01.StartLogin())
            {
                Console.WriteLine("Cannot Start login with Bot 01 - We quit");
                return;
            }

            if (!botVideoBroadcaster02.StartLogin())
            {
                Console.WriteLine("Cannot Start login with Bot 02 - We quit");
                return;
            }

            Boolean canContinueBot01 = true;
            Boolean canContinueBot02 = true;

            // We loop until both bot cannot continue
            while (canContinueBot01 || canContinueBot02)
            {
                Thread.Sleep(20);

                if(canContinueBot01)
                    canContinueBot01 = CheckBotStatus(botVideoBroadcaster01);

                if (canContinueBot02)
                    canContinueBot02 = CheckBotStatus(botVideoBroadcaster02);
            }
        }

        static Boolean CheckBotStatus(RainbowBotVideoBroadcaster botVideoBroadcaster)
        {
            Boolean canContinue = true;

            // Get the curent state of the bot and the trigger used to reach it
            (RainbowBotVideoBroadcaster.State state, RainbowBotVideoBroadcaster.Trigger trigger) = botVideoBroadcaster.GetStateAndTrigger();

            // Two states are important here:
            //  - NotConnected: the bot is not connected (or no more connected), we need to add logic according uour needs
            //  - Created: the bot is not connected because credentials are not correct
            // All other states are managed in the bot logic itself

            // Check if bot is in NotConnected state
            if (state == RainbowBotVideoBroadcaster.State.NotConnected)
            {
                switch (trigger)
                {
                    case RainbowBotVideoBroadcaster.Trigger.Disconnect:
                        // The bot has received the "stop message" from the bot master.
                        Console.WriteLine($"The bot [{botVideoBroadcaster.BotName}] has received the \"stop message\" from the bot master...");
                        canContinue = false;
                        break;

                    case RainbowBotVideoBroadcaster.Trigger.ServerNotReachable:
                        // The server has not been reached.
                        // Need to add logic to try again OR something else ?
                        Console.WriteLine($"The bot [{botVideoBroadcaster.BotName}] cannot reach the server ... You need to had your own logic to manage this case.");
                        canContinue = false;
                        break;

                    case RainbowBotVideoBroadcaster.Trigger.TooManyAttempts:
                        // Bot was logged at least once but since we can reach the server after several attempts
                        // Need to add logic to try again OR something else ?
                        Console.WriteLine($"The bot [{botVideoBroadcaster.BotName}] was connected but after several attempts we can't reach the server ... You need to had your own logic to manage this case.");
                        canContinue = false;
                        break;
                }
            }
            else if (state == RainbowBotVideoBroadcaster.State.Created)
            {
                switch (trigger)
                {
                    case RainbowBotVideoBroadcaster.Trigger.IncorrectCredentials:
                        Console.WriteLine($"The bot [{botVideoBroadcaster.BotName}] is not connected because the credentials are not correct ...");
                        canContinue = false;
                        break;
                }
            }

            return canContinue;
        }

        static Boolean InitLogsWithNLog()
        {
            try
            {
                string logConfigFilePath = RainbowApplicationInfo.NLOG_CONFIG_FILE_PATH;

                if (File.Exists(logConfigFilePath))
                {
                    // Get content of the log file configuration
                    String logConfigContent = File.ReadAllText(logConfigFilePath, System.Text.Encoding.UTF8);

                    // Create NLog configuration using XML file content
                    XmlLoggingConfiguration config = XmlLoggingConfiguration.CreateFromXmlString(logConfigContent);
                    if (config.InitializeSucceeded == true)
                    {
                        // Set NLog configuration
                        NLog.LogManager.Configuration = config;

                        // Create Logger factory
                        var factory = new NLog.Extensions.Logging.NLogLoggerFactory();

                        // Set Logger factory to Rainbow SDK
                        Rainbow.LogFactory.Set(factory);

                        return true;
                    }
                }
                else
                    Console.WriteLine("NLOG_CONFIG_FILE_PATH is not a correct valid path ! The process will continue but you will have no log file.");

            }
            catch { }

            return false;
        }

        static private Boolean CheckApplicationInfoValues()
        {
            String message = "You must define this variable(s) in the file RainbowApplicationInfo.cs:";
            Boolean result = true;

            if (!IsConfigValueValid(RainbowApplicationInfo.FFMPEG_LIB_FOLDER_PATH))
            {
                message += "\r\n\t FFMPEG_LIB_FOLDER_PATH has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.APP_ID))
            {
                message += "\r\n\t APP_ID has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.APP_SECRET_KEY))
            {
                message += "\r\n\t APP_SECRET_KEY has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.HOST_NAME))
            {
                message += "\r\n\t HOST_NAME has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.LOGIN_MASTER_BOT))
            {
                message += "\r\n\t LOGIN_MASTER_BOT has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.STOP_MESSAGE_BOT_01))
            {
                message += "\r\n\t STOP_MESSAGE_BOT_01 has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.NAME_BOT_01))
            {
                message += "\r\n\t NAME_BOT_01 has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.LOGIN_BOT_01))
            {
                message += "\r\n\t LOGIN_BOT_01 has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.PASSWORD_BOT_01))
            {
                message += "\r\n\t PASSWORD_BOT_01 has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.STOP_MESSAGE_BOT_02))
            {
                message += "\r\n\t STOP_MESSAGE_BOT_02 has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.NAME_BOT_02))
            {
                message += "\r\n\t NAME_BOT_02 has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.LOGIN_BOT_02))
            {
                message += "\r\n\t LOGIN_BOT_02 has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.VIDEO_URI_01))
            {
                message += "\r\n\t VIDEO_URI_01 has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.VIDEO_URI_02))
            {
                message += "\r\n\t VIDEO_URI_02 has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.VIDEO_URI_03))
            {
                message += "\r\n\t VIDEO_URI_03 has not been defined";
                result = false;
            }

            if (!result)
                Console.WriteLine(message);

            return result;
        }

        static private Boolean IsConfigValueValid(String? value)
        {
            if (String.IsNullOrEmpty(value))
                return false;

            if (value.Equals(RainbowApplicationInfo.DEFAULT_VALUE, StringComparison.InvariantCultureIgnoreCase))
                return false;

            return true;
        }
    }
}
