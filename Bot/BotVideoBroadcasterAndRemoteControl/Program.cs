using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using WebSocketSharp;

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
                var json = JsonConvert.DeserializeObject<dynamic>(jsonConfig);

                if (json == null)
                {
                    Util.WriteErrorToConsole($"Cannot get JSON data from file '{configFilePath}'.");
                    return false;
                }

                if (json["ffmpegLibFolderPath"] != null)
                    RainbowApplicationInfo.ffmpegLibFolderPath = json["ffmpegLibFolderPath"].ToString();


                if (json["serverConfig"] != null)
                {
                    var jobject = json["serverConfig"];

                    RainbowApplicationInfo.serverConfig = new ServerConfig();
                    RainbowApplicationInfo.serverConfig.AppId = jobject["appId"].ToString();
                    RainbowApplicationInfo.serverConfig.AppSecretKey = jobject["appSecret"].ToString();
                    RainbowApplicationInfo.serverConfig.HostName = jobject["hostname"].ToString();
                }

                if (json["botManagers"] != null)
                {
                    JArray list = (JArray)json["botManagers"];
                    RainbowApplicationInfo.managers = new List<Manager>();

                    foreach (var item in list)
                    {
                        String? email = item["email"]?.ToString();
                        String? jid = item["jid"]?.ToString();
                        String? id = item["id"]?.ToString();

                        if (!String.IsNullOrEmpty(email))
                            RainbowApplicationInfo.managers.Add(new Manager(email, id, jid));
                    }
                }

                if (json["botsVideoBroadcaster"] != null)
                {
                    
                    JArray list = (JArray)json["botsVideoBroadcaster"];
                    RainbowApplicationInfo.broadcasters = new List<Broadcaster>();

                    foreach (var item in list)
                    {
                        var broadcaster = new Broadcaster();
                        broadcaster.Name = item["name"]?.ToString();
                        broadcaster.Login = item["login"]?.ToString();
                        broadcaster.Pwd = item["password"]?.ToString();
                        broadcaster.VideoURI = item["videoUri"]?.ToString();
                        broadcaster.SharingURI = item["sharingUri"]?.ToString();
                        if (item["autoJoinConference"] != null)
                            broadcaster.AutoJoinConference = item["autoJoinConference"].Value<Boolean>();
                        else
                            broadcaster.AutoJoinConference = false;
                        broadcaster.OnlyJoinBubbleJid = item["onlyJoinBubbleJid"]?.ToString();
                     
                        
                        if((!String.IsNullOrEmpty(broadcaster.Login)) && (!String.IsNullOrEmpty(broadcaster.Pwd)))
                        {
                            RainbowApplicationInfo.broadcasters.Add(broadcaster);

                            broadcaster.ServerConfig = RainbowApplicationInfo.serverConfig;
                            broadcaster.Managers = RainbowApplicationInfo.managers;
                            broadcaster.CommandsFromIM = ParseActions(item["commandsFromIM"], item);
                            broadcaster.CommandsFromAC = ParseActions(item["commandsFromAC"], item);
                        }
                        else
                        {
                            Util.WriteErrorToConsole($"Cannot create a Broadcaster since login and/or Pwd is null/empty");
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

        static public List<Action>? ParseActions(JToken? actions, JToken root)
        {
            List<Action>? result = null;

            if(actions != null)
            {
                if(actions is JArray jArray)
                {
                    foreach(var item in jArray)
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

        static public Action? ParseAction(JToken? action, JToken root)
        {
            if (action != null)
            {
                Action result = new Action();
                result.Trigger = action["trigger"]?.ToString();
                result.Type = action["type"]?.ToString();

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

        static public ActionDetails? ParseDetails(JToken? details, JToken root)
        {
            ActionDetails result = null;
            if (details != null)
            {
                foreach (var entry in details)
                {
                    var property = entry as JProperty;

                    if (property != null)
                    {
                        if (!String.IsNullOrEmpty(property.Name))
                        {
                            if (property.Name == "nextAction")
                            {
                                if (result == null)
                                    result = new ActionDetails();

                                result.NextAction = ParseAction(details["nextAction"], root);
                            }
                            else
                            {
                                var value = ParseValue(property.Value.ToString(), root);

                                if (!String.IsNullOrEmpty(value))
                                {
                                    if (result == null)
                                        result = new ActionDetails();

                                    result.Info.Add(property.Name, value);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        static public String ParseValue(String value, JToken root)
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
