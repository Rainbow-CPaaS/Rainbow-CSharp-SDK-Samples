using NLog.Config;
using Rainbow;
using Rainbow.SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace BotVideoBroadcaster
{
    internal class Program
    {
        private static List<BotVideoBroadcasterInfo>? botVideoBroadcasterInfos;

        static void Main()
        {

            Util.WriteDebugToConsole($"{Global.ProductName()} v{Global.FileVersion()}");

            // Check done on values defined in file RainbowApplicationInfo.cs AND in config.json
            if (!CheckApplicationInfoValues())
                return;

            // Init log configuration
            InitLogsWithNLog();

            // Create BotVideoBroadcaster 01 && 02
            RainbowBotVideoBroadcaster botVideoBroadcaster = new RainbowBotVideoBroadcaster();

            // Get the dot graph of this Bot - it's a String which can be used to represent the state machine diagram on a online tool like https://dreampuf.github.io/GraphvizOnline/
            string graph = botVideoBroadcaster.ToDotGraph();


            botVideoBroadcasterInfos = new List<BotVideoBroadcasterInfo>();
            int index = 0;
            foreach (var account in RainbowApplicationInfo.botsVideoBroadcaster)
            {
                var botVideoBroadcasterInfo = new BotVideoBroadcasterInfo(account.Login, account.Password);
                botVideoBroadcasterInfo.Name = $"CCTV {index + 1}";

                // Set Both default URI
                if (RainbowApplicationInfo.videosUri?.Count > 0)
                {
                    if (RainbowApplicationInfo.videosUri.Count > index)
                        botVideoBroadcasterInfo.Uri = RainbowApplicationInfo.videosUri[index];
                    else
                        botVideoBroadcasterInfo.Uri = RainbowApplicationInfo.videosUri[index % RainbowApplicationInfo.videosUri.Count];
                }

                botVideoBroadcasterInfos.Add(botVideoBroadcasterInfo);
                index++;
                if ((RainbowApplicationInfo.nbMaxVideoBroadcaster > 0) && (index == RainbowApplicationInfo.nbMaxVideoBroadcaster))
                    break;
            }


            do
            {

                // Check if a Bot has not been started / Created
                foreach (var bot in botVideoBroadcasterInfos)
                {
                    RainbowBotVideoBroadcaster.State botState;
                    Boolean botCanContinue;
                    String botMessage;

                    (botState, botCanContinue, botMessage) = bot.CheckBotStatus();

                    // If a bot has just been created, we configure it and start login
                    if (botState == RainbowBotVideoBroadcaster.State.Created)
                    {
                        bot.Configure(RainbowApplicationInfo.commandStop, RainbowApplicationInfo.botManagers);

                        do
                        {
                            Thread.Sleep(20);
                            (botState, botCanContinue, botMessage) = bot.CheckBotStatus();
                            if(!botCanContinue)
                            {
                                Util.WriteErrorToConsole($"[{bot.Name}] Cannot connect to the server ...");
                                return;
                            }
                            else
                            {
                                if (botState == RainbowBotVideoBroadcaster.State.Connected)
                                    break;
                            }

                        } while (true);
                    }
                    // If this bot is not already connected, we don't try to check another one
                    else if (botState != RainbowBotVideoBroadcaster.State.Connected)
                    {
                        break;
                    }
                }

                // Here all bots are connected
                Thread.Sleep(20);

            } while (true);
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
            String configFilePath = $".{Path.DirectorySeparatorChar}Resources{Path.DirectorySeparatorChar}config.json";

            if (!File.Exists(configFilePath))
            {
                Util.WriteErrorToConsole($"The file '{configFilePath}' has not been found. It must be stored in the same folder than this application.");
                return false;
            }

            try
            {
                String jsonConfig = File.ReadAllText(configFilePath);
                var json = JSON.Parse(jsonConfig);

                if (json == null)
                {
                    Util.WriteErrorToConsole($"Cannot get JSON data from file '{configFilePath}'.");
                    return false;
                }

                if (json["ffmpegLibFolderPath"] != null)
                    RainbowApplicationInfo.ffmpegLibFolderPath = UtilJson.AsString(json, "ffmpegLibFolderPath");

                if (json["videosUri"] != null)
                {
                    var list = UtilJson.AsStringList(json, "videosUri");
                    RainbowApplicationInfo.videosUri = new List<String>();
                    foreach (var item in list)
                        RainbowApplicationInfo.videosUri.Add(item);
                }

                if (json["serverConfig"] != null)
                {
                    var jobject = json["serverConfig"];
                    if (jobject["appId"] != null)
                        RainbowApplicationInfo.appId = UtilJson.AsString(jobject, "appId");

                    if (jobject["appSecret"] != null)
                        RainbowApplicationInfo.appSecret = UtilJson.AsString(jobject, "appSecret");

                    if (jobject["hostname"] != null)
                        RainbowApplicationInfo.hostname = UtilJson.AsString(jobject, "hostname");
                }

                if (json["command"] != null)
                {
                    var jobject = json["command"];

                    if (jobject["stop"] != null)
                        RainbowApplicationInfo.commandStop = UtilJson.AsString(jobject, "stop");
                }

                if (json["nbMaxVideoBroadcaster"] != null)
                    RainbowApplicationInfo.nbMaxVideoBroadcaster = UtilJson.AsInt(json, "nbMaxVideoBroadcaster");


                if (json["botManagers"] != null)
                {
                    var list = json["botManagers"];
                    if (list?.IsArray == true)
                    {
                        RainbowApplicationInfo.botManagers = new List<BotManager>();
                        foreach (JSONNode jsonItem in list)
                        {
                            String login = UtilJson.AsString(jsonItem, "login");
                            String jid = UtilJson.AsString(jsonItem, "jid");
                            String id = UtilJson.AsString(jsonItem, "id");

                            if (!String.IsNullOrEmpty(login))
                                RainbowApplicationInfo.botManagers.Add(new BotManager(login, id, jid));
                        }
                    }
                }

                if (json["botsVideoBroadcaster"] != null)
                {
                    var list = json["botsVideoBroadcaster"];
                    if (list?.IsArray == true)
                    {
                        RainbowApplicationInfo.botsVideoBroadcaster = new List<Account>();
                        foreach (JSONNode jsonItem in list)
                        {
                            String login = UtilJson.AsString(jsonItem, "login");
                            String pwd = UtilJson.AsString(jsonItem, "password");

                            if ((!String.IsNullOrEmpty(login)) && (!String.IsNullOrEmpty(pwd)))
                                RainbowApplicationInfo.botsVideoBroadcaster.Add(new Account(login, pwd));
                        }
                    }
                }

            }
            catch (Exception exc)
            {
                Util.WriteErrorToConsole($"Exception occurs when reading '{configFilePath}' - Exception:[{Rainbow.Util.SerializeException(exc)}]");
                return false;
            }

            String message = $"You must define this variable(s) in the file RainbowApplicationInfo.cs OR in file '{configFilePath}'";
            Boolean result = true;

            if (!IsConfigValueValid(RainbowApplicationInfo.ffmpegLibFolderPath))
            {
                message += "\r\n\t ffmpegLibFolderPath has not been defined";
                result = false;
            }

            Rainbow.Medias.Helper.InitExternalLibraries(RainbowApplicationInfo.ffmpegLibFolderPath);

            if (!IsConfigValueValid(RainbowApplicationInfo.commandStop))
            {
                message += "\r\n\t botVideoOrchestrator.commcommandStopandMenu has not been defined";
                result = false;
            }

            if (RainbowApplicationInfo.videosUri == null)
            {
                message += "\r\n\t videosUri has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.appId))
            {
                message += "\r\n\t appId has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.appSecret))
            {
                message += "\r\n\t appSecret has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.hostname))
            {
                message += "\r\n\t hostname has not been defined";
                result = false;
            }

            if (RainbowApplicationInfo.botManagers == null)
            {
                message += "\r\n\t botManagers has not been defined";
                result = false;
            }

            if (RainbowApplicationInfo.botsVideoBroadcaster == null)
            {
                message += "\r\n\t botsVideoBroadcaster has not been defined";
                result = false;
            }

            if (!result)
                Util.WriteErrorToConsole(message);

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
