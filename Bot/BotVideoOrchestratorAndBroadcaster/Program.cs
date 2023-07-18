using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace BotVideoOrchestratorAndBroadcaster
{
    internal class Program
    {
        static void Main()
        {
            Util.WriteDebugToConsole($"{Global.ProductName()} v{Global.FileVersion()}");

            // Check done on values defined in file RainbowApplicationInfo.cs AND in config.json
            if (!CheckApplicationInfoValues())
                return;

            // Check done on values defined in file RainbowApplicationInfo.cs AND in labels file (by default labels_EN.json)
            if (!ReadLabels(RainbowApplicationInfo.labelsFilePath))
                return;

            if(RainbowApplicationInfo.botVideoOrchestrator == null)
            {
                Util.WriteErrorToConsole($"[{RainbowApplicationInfo.labelOrchestrator}] No login/pwd specify - Check 'config.json' file");
                return;
            }

            // Init log configuration
            InitLogsWithNLog();



            // Create RainbowBotVideoOrchestrator and RainbowBotVideoBroadcaster (for this one just it's just to have the dot graph)
            RainbowBotVideoOrchestrator botVideoOrchestrator = new RainbowBotVideoOrchestrator();
            RainbowBotVideoBroadcaster botVideoBroadcaster = new RainbowBotVideoBroadcaster();

            // Get the dot graph of this Bot - it's a String which can be used to represent the state machine diagram on a online tool like https://dreampuf.github.io/GraphvizOnline/
            string graphOrchestrator = botVideoOrchestrator.ToDotGraph();
            string graphBroadcaster = botVideoBroadcaster.ToDotGraph();

            // Configure the botVideoOrchestrator
            if (!botVideoOrchestrator.Configure(
                    RainbowApplicationInfo.appId, RainbowApplicationInfo.appSecret,
                    RainbowApplicationInfo.hostname,
                    RainbowApplicationInfo.botVideoOrchestrator.Login, RainbowApplicationInfo.botVideoOrchestrator.Password,
                    RainbowApplicationInfo.labelOrchestrator,
                    RainbowApplicationInfo.botManagers,
                    "./",
                    "botVideoOrchestrator.ini"
                    ))
            {
                Util.WriteErrorToConsole($"[{RainbowApplicationInfo.labelOrchestrator}] Cannot configure - Check 'config.json' file");
                return;
            }

            // Start login
            if (botVideoOrchestrator.StartLogin())
            {
                Boolean canContinue = true;
                RainbowBotVideoOrchestrator.Trigger trigger;
                RainbowBotVideoOrchestrator.State state;

                // We loop until we can no more continue
                while (canContinue)
                {
                    Thread.Sleep(20);

                    // Get the curent state of the bot and the trigger used to reach it
                    (state, trigger) = botVideoOrchestrator.GetStateAndTrigger();

                    // Two states are important here:
                    //  - NotConnected: the bot is not connected (or no more connected), we need to add logic according uour needs
                    //  - Created: the bot is not connected because credentials are not correct
                    // All other states are managed in the bot logic itself

                    // Check if bot is in NotConnected state
                    if (state == RainbowBotVideoOrchestrator.State.NotConnected)
                    {
                        switch (trigger)
                        {
                            case RainbowBotVideoOrchestrator.Trigger.Disconnect:
                                // The bot has received the "stop message" from the bot master.
                                Util.WriteErrorToConsole($"[{RainbowApplicationInfo.labelOrchestrator}] 'STOP message' received - We quit");
                                canContinue = false;
                                return;

                            case RainbowBotVideoOrchestrator.Trigger.ServerNotReachable:
                                // The server has not been reached.
                                // Need to add logic to try again OR something else ?
                                Util.WriteErrorToConsole($"[{RainbowApplicationInfo.labelOrchestrator}] Cannot reach the server ... - We quit");
                                canContinue = false;
                                return;

                            case RainbowBotVideoOrchestrator.Trigger.TooManyAttempts:
                                // Bot was logged at least once but since we can reach the server after several attempts
                                // Need to add logic to try again OR something else ?
                                Util.WriteErrorToConsole($"[{RainbowApplicationInfo.labelOrchestrator}] The bot was connected but after several attempts we can't reach the server ... - We quit");
                                canContinue = false;
                                break;
                        }
                    }
                    else if (state == RainbowBotVideoOrchestrator.State.Created)
                    {
                        switch (trigger)
                        {
                            case RainbowBotVideoOrchestrator.Trigger.IncorrectCredentials:
                                Util.WriteErrorToConsole($"[{RainbowApplicationInfo.labelOrchestrator}] The bot is not connected because the credentials are not correct... - We quit");
                                canContinue = false;
                                break;
                        }
                    }
                }
            }
            else
                Util.WriteErrorToConsole($"[{RainbowApplicationInfo.labelOrchestrator}] Cannot start the login process ... Check 'config.json' file");
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
                var json = JsonConvert.DeserializeObject<dynamic>(jsonConfig);

                if (json == null)
                {
                    Util.WriteErrorToConsole($"Cannot get JSON data from file '{configFilePath}'.");
                    return false;
                }

                if (json["ffmpegLibFolderPath"] != null)
                {
                    RainbowApplicationInfo.ffmpegLibFolderPath = json["ffmpegLibFolderPath"].ToString();
                    Rainbow.Medias.Helper.InitExternalLibraries(RainbowApplicationInfo.ffmpegLibFolderPath);
                }

                if (json["labelsFilePath"] != null)
                    RainbowApplicationInfo.labelsFilePath = $".{Path.DirectorySeparatorChar}Resources{Path.DirectorySeparatorChar}" + json["labelsFilePath"].ToString();

                if (json["videosUri"] != null)
                {
                    JArray list = (JArray)json["videosUri"];
                    RainbowApplicationInfo.videosUri = new List<String>();
                    foreach (var item in list)
                        RainbowApplicationInfo.videosUri.Add(item.ToString());
                }

                if (json["serverConfig"] != null)
                {
                    var jobject = json["serverConfig"];
                    if (jobject["appId"] != null)
                        RainbowApplicationInfo.appId = jobject["appId"].ToString();

                    if (jobject["appSecret"] != null)
                        RainbowApplicationInfo.appSecret = jobject["appSecret"].ToString();

                    if (jobject["hostname"] != null)
                        RainbowApplicationInfo.hostname = jobject["hostname"].ToString();
                }

                if (json["botVideoOrchestrator"] != null)
                {
                    String login = "";
                    String pwd = "";

                    var jobject = json["botVideoOrchestrator"];
                    if (jobject["login"] != null)
                        login = jobject["login"].ToString();

                    if (jobject["password"] != null)
                        pwd = jobject["password"].ToString();

                    if (jobject["autoJoinConference"] != null)
                        RainbowApplicationInfo.botVideoOrchestratorAutoJoinConference = (Boolean)jobject["autoJoinConference"];

                    if (jobject["commandMenu"] != null)
                        RainbowApplicationInfo.commandMenu = jobject["commandMenu"].ToString();

                    if (jobject["commandStop"] != null)
                        RainbowApplicationInfo.commandStop = jobject["commandStop"].ToString();


                    if ((!String.IsNullOrEmpty(login)) && (!String.IsNullOrEmpty(pwd)))
                    {
                        RainbowApplicationInfo.botVideoOrchestrator = new Account(login, pwd);
                    }
                }

                if (json["nbMaxVideoBroadcaster"] != null)
                    RainbowApplicationInfo.nbMaxVideoBroadcaster = (int)json["nbMaxVideoBroadcaster"];


                if (json["botManagers"] != null)
                {
                    JArray list = (JArray)json["botManagers"];
                    RainbowApplicationInfo.botManagers = new List<BotManager>();

                    foreach (var item in list)
                    {
                        String? login = item["login"]?.ToString();
                        String? jid = item["jid"]?.ToString();
                        String? id = item["id"]?.ToString();

                        if (!String.IsNullOrEmpty(login))
                            RainbowApplicationInfo.botManagers.Add(new BotManager(login, id, jid));
                    }
                }

                if (json["botsVideoBroadcaster"] != null)
                {
                    JArray list = (JArray)json["botsVideoBroadcaster"];
                    RainbowApplicationInfo.botsVideoBroadcaster = new List<Account>();

                    foreach (var item in list)
                    {
                        String? login = item["login"]?.ToString();
                        String? pwd = item["password"]?.ToString();

                        if ((!String.IsNullOrEmpty(login)) && (!String.IsNullOrEmpty(pwd)))
                            RainbowApplicationInfo.botsVideoBroadcaster.Add(new Account(login, pwd));
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


            if (!IsConfigValueValid(RainbowApplicationInfo.commandMenu))
            {
                message += "\r\n\t botVideoOrchestrator.commandMenu has not been defined";
                result = false;
            }

            if(!IsConfigValueValid(RainbowApplicationInfo.commandStop))
            {
                message += "\r\n\t botVideoOrchestrator.commcommandStopandMenu has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.labelsFilePath))
            {
                message += "\r\n\t labelsFilePath has not been defined";
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

            if (RainbowApplicationInfo.botVideoOrchestrator == null)
            {
                message += "\r\n\t botVideoOrchestrator has not been defined";
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

        static private Boolean ReadLabels(String filepath)
        {
            String message = $"You must define this variable(s) in the file RainbowApplicationInfo.cs OR in file '{filepath}'";
            Boolean result = true;

            if (!File.Exists(filepath))
            {
                Util.WriteErrorToConsole($"The file '{filepath}' has not been found. It must be stored in the same fodler than this application.");
                return false;
            }

            try
            {
                String jsonConfig = File.ReadAllText(filepath);
                var json = JsonConvert.DeserializeObject<dynamic>(jsonConfig);

                if (json == null)
                {
                    Util.WriteErrorToConsole($"Cannot get JSON data from file '{filepath}'.");
                    return false;
                }

                if (json["labelTitle"] != null)
                    RainbowApplicationInfo.labelTitle = json["labelTitle"];

                if (json["labelTitleEnd"] != null)
                    RainbowApplicationInfo.labelTitleEnd = json["labelTitleEnd"];

                if (json["labelSet"] != null)
                    RainbowApplicationInfo.labelSet = json["labelSet"];

                if (json["labelStop"] != null)
                    RainbowApplicationInfo.labelStop = json["labelStop"];
                
                if (json["labelNone"] != null)
                    RainbowApplicationInfo.labelNone = json["labelNone"];

                if (json["labelNoVideo"] != null)
                    RainbowApplicationInfo.labelNoVideo = "**"+json["labelNoVideo"]+"**";

                if (json["labelNoConferenceInProgress"] != null)
                    RainbowApplicationInfo.labelNoConferenceInProgress = json["labelNoConferenceInProgress"];

                if (json["labelUseSharingStream"] != null)
                    RainbowApplicationInfo.labelUseSharingStream = json["labelUseSharingStream"];

                if (json["labelVideosUriName"] != null)
                {
                    JArray list = (JArray)json["labelVideosUriName"];
                    RainbowApplicationInfo.labelVideosUriName = new List<String>();

                    foreach (var item in list)
                    {
                        RainbowApplicationInfo.labelVideosUriName.Add(item.ToString());
                    }
                }

                if (json["labelBotsVideoBroadcasterName"] != null)
                {
                    JArray list = (JArray)json["labelBotsVideoBroadcasterName"];
                    RainbowApplicationInfo.labelBotsVideoBroadcasterName = new List<String>();

                    foreach (var item in list)
                    {
                        RainbowApplicationInfo.labelBotsVideoBroadcasterName.Add(item.ToString());
                    }
                }
            }
            catch (Exception exc)
            {
                Util.WriteErrorToConsole($"Exception occurs when reading '{filepath}' - Exception:[{Rainbow.Util.SerializeException(exc)}]");
                return false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.labelTitle))
            {
                message += "\r\n\t labelTitle has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.labelTitleEnd))
            {
                message += "\r\n\t labelTitleEnd has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.labelSet))
            {
                message += "\r\n\t labelSet has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.labelStop))
            {
                message += "\r\n\t labelStop has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.labelNone))
            {
                message += "\r\n\t labelNone has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.labelNoVideo))
            {
                message += "\r\n\t labelNoVideo has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.labelNoConferenceInProgress))
            {
                message += "\r\n\t labelNoConferenceInProgress has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.labelUseSharingStream))
            {
                message += "\r\n\t labelUseSharingStream has not been defined";
                result = false;
            }

            return result;

            /*
 * 	
    "labelTitle": "Bot video broadcaster settings",
    "labelSet": "Set",
    "labelNone": "None",
    "labelNoVideo": "No broadcast",
    "labelUseSharingStream": "Use sharing stream on:",
*/
        }
    }
}
