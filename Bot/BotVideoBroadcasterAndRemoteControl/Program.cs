using NLog.Config;
using Rainbow;
using Rainbow.SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace BotVideoOrchestratorAndRemoteControl
{
    internal class Program
    {
        static void Main()
        {
            Util.WriteDebugToConsole($"{Global.ProductName()} v{Global.FileVersion()}");

            // Init log configuration
            InitLogsWithNLog();


            // Read config.json file
            if (!ReadConfig())
                return;

            List<RainbowBotVideoBroadcaster> bots = new List<RainbowBotVideoBroadcaster>();

            foreach(var broadcaster in RainbowApplicationInfo.broadcasters)
            {
                var bot = new RainbowBotVideoBroadcaster();

                if(bot.Configure(broadcaster))
                {
                    if(bot.StartLogin())
                        bots.Add(bot);
                }
                else
                {
                    Util.WriteErrorToConsole($"Cannot Configure Bot - Name:[{broadcaster.Name}");
                }
            }

            // We loop until all bots cannot continue
            while (bots.Count > 0)
            {
                Thread.Sleep(20);

                var firstBot = bots.ElementAt(0);

                if (!CheckBotStatus(firstBot))
                    bots.RemoveAt(0);
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
                        Util.WriteErrorToConsole($"The bot [{botVideoBroadcaster.BotName}] has received the \"stop message\" from the bot master...");
                        canContinue = false;
                        break;

                    case RainbowBotVideoBroadcaster.Trigger.ServerNotReachable:
                        // The server has not been reached.
                        // Need to add logic to try again OR something else ?
                        Util.WriteErrorToConsole($"The bot [{botVideoBroadcaster.BotName}] cannot reach the server ... You need to had your own logic to manage this case.");
                        canContinue = false;
                        break;

                    case RainbowBotVideoBroadcaster.Trigger.TooManyAttempts:
                        // Bot was logged at least once but since we can reach the server after several attempts
                        // Need to add logic to try again OR something else ?
                        Util.WriteErrorToConsole($"The bot [{botVideoBroadcaster.BotName}] was connected but after several attempts we can't reach the server ... You need to had your own logic to manage this case.");
                        canContinue = false;
                        break;
                }
            }
            else if (state == RainbowBotVideoBroadcaster.State.Created)
            {
                switch (trigger)
                {
                    case RainbowBotVideoBroadcaster.Trigger.IncorrectCredentials:
                        Util.WriteErrorToConsole($"The bot [{botVideoBroadcaster.BotName}] is not connected because the credentials are not correct ...");
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
                    Util.WriteWarningToConsole("NLOG_CONFIG_FILE_PATH is not a correct valid path ! The process will continue but you will have no log file.");

            }
            catch { }

            return false;
        }
        
        static private Boolean IsConfigValueValid(String? value)
        {
            if (String.IsNullOrEmpty(value))
                return false;

            if (value.Equals(RainbowApplicationInfo.DEFAULT_VALUE, StringComparison.InvariantCultureIgnoreCase))
                return false;

            return true;
        }

        static private Boolean ReadConfig()
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
                {
                    RainbowApplicationInfo.ffmpegLibFolderPath = UtilJson.AsString(json, "ffmpegLibFolderPath");
                    Rainbow.Medias.Helper.InitExternalLibraries(RainbowApplicationInfo.ffmpegLibFolderPath);
                }


                if (json["serverConfig"] != null)
                {
                    var jobject = json["serverConfig"];

                    RainbowApplicationInfo.serverConfig = new ServerConfig();
                    RainbowApplicationInfo.serverConfig.AppId = UtilJson.AsString(jobject, "appId");
                    RainbowApplicationInfo.serverConfig.AppSecretKey = UtilJson.AsString(jobject, "appSecret");
                    RainbowApplicationInfo.serverConfig.HostName = UtilJson.AsString(jobject, "hostname");
                }

                if (json["botManagers"] != null)
                {
                    var list = json["botManagers"];
                    if (list?.IsArray == true)
                    {
                        RainbowApplicationInfo.managers = new List<Manager>();

                        foreach (var item in list)
                        {
                            String? email = UtilJson.AsString(item, "email");
                            String? jid = UtilJson.AsString(item, "jid");
                            String? id = UtilJson.AsString(item, "id");

                            if (!String.IsNullOrEmpty(email))
                                RainbowApplicationInfo.managers.Add(new Manager(email, id, jid));
                        }
                    }
                }

                if (json["botsVideoBroadcaster"] != null)
                {
                    var list = json["botsVideoBroadcaster"];
                    if (list?.IsArray == true)
                    {
                        RainbowApplicationInfo.broadcasters = new List<Broadcaster>();

                        foreach (var item in list)
                        {
                            var broadcaster = new Broadcaster();
                            broadcaster.Name = UtilJson.AsString(item, "name");
                            broadcaster.Login = UtilJson.AsString(item, "login");
                            broadcaster.Pwd = UtilJson.AsString(item, "password");
                            broadcaster.VideoURI = UtilJson.AsString(item, "videoUri");
                            broadcaster.SharingURI = UtilJson.AsString(item, "sharingUri");
                            broadcaster.AutoJoinConference = UtilJson.AsBoolean(item, "autoJoinConference");
                            broadcaster.OnlyJoinBubbleJid = UtilJson.AsString(item, "onlyJoinBubbleJid");


                            if ((!String.IsNullOrEmpty(broadcaster.Login)) && (!String.IsNullOrEmpty(broadcaster.Pwd)))
                            {
                                RainbowApplicationInfo.broadcasters.Add(broadcaster);

                                broadcaster.ServerConfig = RainbowApplicationInfo.serverConfig;
                                broadcaster.Managers = RainbowApplicationInfo.managers;
                                broadcaster.CommandsFromIM = ParseActions(item.Value["commandsFromIM"], item.Value);
                                broadcaster.CommandsFromAC = ParseActions(item.Value["commandsFromAC"], item.Value);
                            }
                            else
                            {
                                Util.WriteErrorToConsole($"Cannot create a Broadcaster since login and/or Pwd is null/empty");
                            }
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

            if (!IsConfigValueValid(RainbowApplicationInfo.serverConfig.AppId))
            {
                message += "\r\n\t appId has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.serverConfig.AppSecretKey))
            {
                message += "\r\n\t appSecret has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.serverConfig.HostName))
            {
                message += "\r\n\t hostname has not been defined";
                result = false;
            }

            if (RainbowApplicationInfo.managers == null)
            {
                message += "\r\n\t botManagers has not been defined";
                result = false;
            }

            
            if (RainbowApplicationInfo.broadcasters == null)
            {
                message += "\r\n\t botsVideoBroadcaster has not been defined";
                result = false;
            }

            if (!result)
                Util.WriteErrorToConsole(message);

            return result;
        }

        static public List<Action>? ParseActions(JSONNode? actions, JSONNode root)
        {
            List<Action>? result = null;

            if(actions != null)
            {
                if(actions.IsArray)
                {
                    foreach(var item in actions)
                    {
                        var action = ParseAction(item, root);
                        if(action != null)
                        {
                            if (result == null)
                                result = new List<Action>();
                            result.Add(action);
                        }
                    }
                }
            }

            return result;
        }

        static public Action? ParseAction(JSONNode? action, JSONNode root)
        {
            if (action != null)
            {
                Action result = new Action();
                result.Trigger = UtilJson.AsString(action, "trigger");
                result.Type = UtilJson.AsString(action, "type");

                if ( String.IsNullOrEmpty(result.Type))
                        return null;

                var details = ParseDetails(action["details"], root);
                if (details != null)
                {
                    result.Details = details;
                }
                return result;
            }

            return null;
        }

        static public ActionDetails? ParseDetails(JSONNode? details, JSONNode root)
        {
            ActionDetails result = null;
            if (details != null)
            {
                foreach (var entry in details)
                {
                    if (!String.IsNullOrEmpty(entry.Key))
                    {
                        if (entry.Key == "nextAction")
                        {
                            if (result == null)
                                result = new ActionDetails();

                            result.NextAction = ParseAction(details["nextAction"], root);
                        }
                        else
                        {
                            var str = UtilJson.AsString(entry, entry.Key);
                            var value =  ParseValue(entry.Value.Value, root);
                            if (!String.IsNullOrEmpty(value))
                            {
                                if (result == null)
                                    result = new ActionDetails();

                                result.Info.Add(entry.Key, value);
                            }
                        }
                    }
                }
            }

            return result;
        }

        static public String ParseValue(String value, JSONNode root)
        {
            if (!String.IsNullOrEmpty(value))
            { 
                if (value.StartsWith("${") && value.EndsWith("}"))
                { 
                    var v = value.Substring(2, value.Length - 3);
                    if (root[v] != null)
                        return root[v].ToString();
                }
            }
            return value;
        }
    }
}
