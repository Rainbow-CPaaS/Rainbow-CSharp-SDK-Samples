using NLog.Config;
using System;
using System.IO;
using System.Threading;

namespace BotAdaptiveCards
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

            // Create a Bot
            var rainbowBotBase = new RainbowBotConnect();

            // Get the dot graph of this Bot - it's a String which can be used to represent the state machine diagram on a online tool like https://dreampuf.github.io/GraphvizOnline/
            string graph = rainbowBotBase.ToDotGraph();

            // Configure the bot
            if (rainbowBotBase.Configure(
                    RainbowApplicationInfo.APP_ID, RainbowApplicationInfo.APP_SECRET_KEY,
                    RainbowApplicationInfo.HOST_NAME,
                    RainbowApplicationInfo.LOGIN_BOT, RainbowApplicationInfo.PASSWORD_BOT,
                    RainbowApplicationInfo.LOGIN_MASTER_BOT,
                    RainbowApplicationInfo.USER_LIST_FOR_MCQ
                    ))
            {
                // Start login
                if (rainbowBotBase.StartLogin())
                {
                    Boolean canContinue = true;
                    RainbowBotConnect.Trigger trigger;
                    RainbowBotConnect.State state;

                    // We loop until we can no more continue
                    while (canContinue)
                    {
                        Thread.Sleep(20);

                        // Get the curent state of the bot and the trigger used to reach it
                        (state, trigger) = rainbowBotBase.GetStateAndTrigger();

                        // Two states are important here:
                        //  - NotConnected: the bot is not connected (or no more connected), we need to add logic according uour needs
                        //  - Created: the bot is not connected because credentials are not correct
                        // All other states are managed in the bot logic itself

                        // Check if bot is in NotConnected state
                        if (state == RainbowBotConnect.State.NotConnected)
                        {
                            switch (trigger)
                            {
                                case RainbowBotConnect.Trigger.Disconnect:
                                    // The bot has received the "stop message" from the bot master.
                                    Console.WriteLine("The bot has received the \"stop message\" from the bot master...");
                                    canContinue = false;
                                    return;

                                case RainbowBotConnect.Trigger.ServerNotReachable:
                                    // The server has not been reached.
                                    // Need to add logic to try again OR something else ?
                                    Console.WriteLine("The bot cannot reach the server ... You need to had your own logic to manage this case.");
                                    canContinue = false;
                                    return;

                                case RainbowBotConnect.Trigger.TooManyAttempts:
                                    // Bot was logged at least once but since we can reach the server after several attempts
                                    // Need to add logic to try again OR something else ?
                                    Console.WriteLine("The bot was connected but after several attempts we can't reach the server ... You need to had your own logic to manage this case.");
                                    canContinue = false;
                                    break;
                            }
                        }
                        else if (state == RainbowBotConnect.State.Created)
                        {
                            switch (trigger)
                            {
                                case RainbowBotConnect.Trigger.AllMCQTestPassed:
                                    // Bot has finished - all MCQ has been performed - a test can also be set as finished if email provided is not correct/not found.
                                    Console.WriteLine("Bot has finished - all MCQ has been performed - a test can also be set as finished if email provided is not correct/not found.");
                                    canContinue = false;
                                    break;
                                case RainbowBotConnect.Trigger.IncorrectCredentials:
                                    Console.WriteLine("The bot is not connected because the credentials are not correct ...");
                                    canContinue = false;
                                    break;
                            }
                        }
                    }
                }
                else
                    Console.WriteLine("Cannot start the login process ...");
            }
            else
                Console.WriteLine("Cannot configure the bot ...");
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

            if (!IsValueValid(RainbowApplicationInfo.APP_ID))
            {
                message += "\r\n\t APP_ID has not been defined";
                result = false;
            }

            if (!IsValueValid(RainbowApplicationInfo.APP_SECRET_KEY))
            {
                message += "\r\n\t APP_SECRET_KEY has not been defined";
                result = false;
            }

            if (!IsValueValid(RainbowApplicationInfo.HOST_NAME))
            {
                message += "\r\n\t HOST_NAME has not been defined";
                result = false;
            }

            if (!IsValueValid(RainbowApplicationInfo.LOGIN_MASTER_BOT))
            {
                message += "\r\n\t LOGIN_MASTER_BOT has not been defined";
                result = false;
            }

            if (!IsValueValid(RainbowApplicationInfo.LOGIN_BOT))
            {
                message += "\r\n\t LOGIN_BOT has not been defined";
                result = false;
            }

            if (!IsValueValid(RainbowApplicationInfo.PASSWORD_BOT))
            {
                message += "\r\n\t PASSWORD_BOT has not been defined";
                result = false;
            }

            if (!result)
                Console.WriteLine(message);

            return result;
        }

        static private Boolean IsValueValid(String? value)
        {
            if (String.IsNullOrEmpty(value))
                return false;

            if (value.Equals(RainbowApplicationInfo.DEFAULT_VALUE, StringComparison.InvariantCultureIgnoreCase))
                return false;

            return true;
        }
    }
}
