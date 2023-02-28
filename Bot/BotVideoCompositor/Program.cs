using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Collections;
using Rainbow.Medias;
using Microsoft.Extensions.Configuration;
using System.Xml.Linq;

namespace BotVideoCompositor
{
    internal class Program
    {
        static void Main()
        {
            Util.WriteBlueToConsole($"{Global.ProductName()} {Global.FileVersion()}\r\n");

            // Check done on values defined in file RainbowApplicationInfo.cs AND in config.json
            if (!CheckApplicationInfoValues())
                return;

            if (Util.CreateBasicAdaptiveCardsData() == null)
            {
                Util.WriteRedToConsole($"Cannot create data for the adaptive cards ...");
                return;
            }

            // Init log configuration
            InitLogsWithNLog();

            // Create BotVideoBroadcaster 01 && 02
            RainbowBotVideoCompositor botVideoCompositor = new RainbowBotVideoCompositor();

            // Get the dot graph of this Bot - it's a String which can be used to represent the state machine diagram on a online tool like https://dreampuf.github.io/GraphvizOnline/
            string graph = botVideoCompositor.ToDotGraph();

            string Name = "BotVideoCompositor";

            if (!botVideoCompositor.Configure(
                        RainbowApplicationInfo.appId, RainbowApplicationInfo.appSecret,
                        RainbowApplicationInfo.hostname,
                        RainbowApplicationInfo.account.Login,
                        RainbowApplicationInfo.account.Password,
                        Name,
                        RainbowApplicationInfo.botManagers,
                        RainbowApplicationInfo.commandStop,
                        RainbowApplicationInfo.commandStart,
                        RainbowBotVideoCompositor.AUTOJOIN_BUBBLE_ID,
                        "./",
                        Name + ".ini"
                        ))
            {
                Util.WriteRedToConsole($"[{Name} Cannot configure this bot - check 'config.json' file !");
                return;
            }

            if (!botVideoCompositor.StartLogin())
            {
                Util.WriteRedToConsole($"[{Name} Cannot start login with this bot - check 'config.json' file !");
                return;
            }

            // We loop until we cannot continue
            RainbowBotVideoCompositor.State state;
            String? message;
            String Id = "";
            Boolean canContinue = true;
            while (canContinue)
            {
                Thread.Sleep(20);

                (state, canContinue, message) = CheckBotStatus(botVideoCompositor);

                // Get ID of the RB contact used by this bot
                if (canContinue && String.IsNullOrEmpty(Id) && (state == RainbowBotVideoCompositor.State.Connected))
                {
                    //if (botVideoCompositor != null)
                    {
                        var contact = botVideoCompositor.GetCurrentContact();
                        if (contact != null)
                        {
                            Id = contact.Id;
                            Util.WriteBlueToConsole($"[{botVideoCompositor.BotName}] We get id[{Id}] of the bot[{Name}]");
                        }
                    }
                }
            }
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
                Util.WriteRedToConsole($"The file '{configFilePath}' has not been found. It must be stored in the same folder than this application.");
                return false;
            }

            try
            {
                String jsonConfig = File.ReadAllText(configFilePath);
                var json = JsonConvert.DeserializeObject<dynamic>(jsonConfig);

                if (json == null)
                {
                    Util.WriteRedToConsole($"Cannot get JSON data from file '{configFilePath}'.");
                    return false;
                }

                if (json["ffmpegLibFolderPath"] != null)
                    RainbowApplicationInfo.ffmpegLibFolderPath = json["ffmpegLibFolderPath"].ToString();

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

                if (json["botVideoCompositor"] != null)
                {
                    String login = "";
                    String password = "";

                    var jobject = json["botVideoCompositor"];
                    if (jobject["login"] != null)
                        login = jobject["login"].ToString();

                    if (jobject["password"] != null)
                        password = jobject["password"].ToString();

                    if ((!String.IsNullOrEmpty(login)) && (!String.IsNullOrEmpty(password)))
                        RainbowApplicationInfo.account = new Account(login, password);
                }

                if (json["command"] != null)
                {
                    var jobject = json["command"];

                    if (jobject["stop"] != null)
                        RainbowApplicationInfo.commandStop = jobject["stop"].ToString();

                    if (jobject["start"] != null)
                        RainbowApplicationInfo.commandStart = jobject["start"].ToString();
                }

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

                // Calculate outputs size available
                if ((json["outputRatio"] != null) && (json["outputWidth"] != null) && (json["outputMaxHeight"] != null))
                {
                    JArray jList;

                    // Get all ratios
                    List<Size> ratiosList = new List<Size>();
                    jList = (JArray)json["outputRatio"];
                    foreach (var item in jList)
                        ratiosList.Add(new Size(item.ToString()));

                    // Get all width
                    List<int> widthList = new List<int>();
                    jList = (JArray)json["outputWidth"];
                    foreach (var item in jList)
                        widthList.Add(item.ToObject<int>());

                    // Get Max Height
                    int maxHeight = (int)json["outputMaxHeight"];

                    // Loop on all width and create different size
                    Dictionary<String, String> ratiosAvailable = new Dictionary<string, string>();
                    int? height;
                    Size ratioToAdd;
                    int maxStringLength = 0;
                    String str;
                    foreach (var width in widthList)
                        foreach (var ratio in ratiosList)
                        {
                            height = Util.GetHeightAccordingRatio(width, ratio);
                            if ((height != null) && (height <= maxHeight) && (height % 2 == 0))
                            {
                                ratioToAdd = new Size(width, height.Value);
                                ratiosAvailable.Add(ratioToAdd.ToString(), ratio.ToString(true));

                                // Need to get max string length to have a better display
                                str = ratioToAdd.ToString();
                                if (str.Length > maxStringLength)
                                    maxStringLength = str.Length;
                            }
                        }

                    Dictionary<String, String> ratiosAvailableForDisplay = new Dictionary<string, string>();
                    foreach (var item in ratiosAvailable)
                    {
                        var key = item.Key;
                        var value = Util.AddString(item.Key, "_", maxStringLength) + "____" + item.Value;
                        ratiosAvailableForDisplay.Add(key, value);
                    }

                    // Store result
                    RainbowApplicationInfo.outputs = ratiosAvailableForDisplay;
                }

                // Calculate vignettes/overlay size available
                if ((json["vignetteRatio"] != null) && (json["vignetteWidth"] != null) && (json["vignetteMaxHeight"] != null))
                {
                    JArray jList;

                    // Get all ratios
                    List<Size> ratiosList = new List<Size>();
                    jList = (JArray)json["vignetteRatio"];
                    foreach (var item in jList)
                        ratiosList.Add(new Size(item.ToString()));

                    // Get all width
                    List<int> widthList = new List<int>();
                    jList = (JArray)json["vignetteWidth"];
                    foreach (var item in jList)
                        widthList.Add(item.ToObject<int>());

                    // Get Max Height
                    int maxHeight = (int)json["vignetteMaxHeight"];

                    // Loop on all widht and create different size
                    Dictionary<String, String> ratiosAvailable = new Dictionary<string, string>();
                    int? height;
                    Size ratioToAdd;
                    int maxStringLength = 0;
                    String str;
                    foreach (var width in widthList)
                        foreach (var ratio in ratiosList)
                        {
                            height = Util.GetHeightAccordingRatio(width, ratio);
                            if ((height != null) && (height <= maxHeight) && (height % 2 == 0))
                            {
                                ratioToAdd = new Size(width, height.Value);
                                ratiosAvailable.Add(ratioToAdd.ToString(), ratio.ToString(true));

                                // Need to get max string length to have a better display
                                str = ratioToAdd.ToString();
                                if (str.Length > maxStringLength)
                                    maxStringLength = str.Length;
                            }
                        }

                    Dictionary<String, String> ratiosAvailableForDisplay = new Dictionary<string, string>();
                    foreach (var item in ratiosAvailable)
                    {
                        var key = item.Key;
                        var value = Util.AddString(item.Key, "_", maxStringLength) + "____" + item.Value;
                        ratiosAvailableForDisplay.Add(key, value);
                    }

                    // Store result
                    RainbowApplicationInfo.vignettes = ratiosAvailableForDisplay;
                }

                // Get fps
                if (json["fps"] != null)
                {
                    // Get all ratios
                    List<int> fpsList = new List<int>();
                    JArray jList = (JArray)json["fps"];
                    foreach (var item in jList)
                        fpsList.Add((int)item);

                    // Store result
                    RainbowApplicationInfo.fps = fpsList;
                }

                // Get labels
                if (json["labels"] != null)
                {
                    string labels = json["labels"].ToString();

                    String labelsFilePath = $".{Path.DirectorySeparatorChar}Resources{Path.DirectorySeparatorChar}{labels}";
                    if(File.Exists(labelsFilePath))
                        RainbowApplicationInfo.labels = File.ReadAllText(labelsFilePath);
                }

                
            }
            catch (Exception exc)
            {
                Util.WriteRedToConsole($"Exception occurs when reading '{configFilePath}' - Exception:[{Rainbow.Util.SerializeException(exc)}]");
                return false;
            }

            String message = $"You must define this variable(s) in the file RainbowApplicationInfo.cs OR in file '{configFilePath}'";
            Boolean result = true;

            if (!IsConfigValueValid(RainbowApplicationInfo.ffmpegLibFolderPath))
            {
                message += "\r\n\t ffmpegLibFolderPath has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.commandStop))
            {
                message += "\r\n\t command.stop has not been defined";
                result = false;
            }

            if (!IsConfigValueValid(RainbowApplicationInfo.commandStart))
            {
                message += "\r\n\t command.start has not been defined";
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

            if (RainbowApplicationInfo.account == null)
            {
                message += "\r\n\t botVideoCompositor has not been defined";
                result = false;
            }

            if (RainbowApplicationInfo.botManagers == null)
            {
                message += "\r\n\t botManagers has not been defined";
                result = false;
            }

            if (RainbowApplicationInfo.fps == null)
            {
                message += "\r\n\t fps has not been defined";
                result = false;
            }

            if (RainbowApplicationInfo.vignettes == null)
            {
                message += "\r\n\t vignetteRatio, vignetteWidth and/or vignetteMaxHeight have not been defined";
                result = false;
            }

            if (RainbowApplicationInfo.outputs == null)
            {
                message += "\r\n\t outputRatio, outputWidth and/or outputMaxHeight have not been defined";
                result = false;
            }

            if (RainbowApplicationInfo.labels == null)
            {
                message += "\r\n\t labels hase not been defined OR file paht is incorrect";
                result = false;
            }

            if (!result)
                Util.WriteRedToConsole(message);

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

        static private (RainbowBotVideoCompositor.State state, Boolean canContinue, String message) CheckBotStatus(RainbowBotVideoCompositor botVideoCompositor)
        {
            Boolean canContinue = true;
            String message = "";

            // Get the curent state of the bot and the trigger used to reach it
            (RainbowBotVideoCompositor.State state, RainbowBotVideoCompositor.Trigger trigger) = botVideoCompositor.GetStateAndTrigger();

            // Two states are important here:
            //  - NotConnected: the bot is not connected (or no more connected), we need to add logic according uour needs
            //  - Created: the bot is not connected because credentials are not correct
            // All other states are managed in the bot logic itself

            // Check if bot is in NotConnected state
            if (state == RainbowBotVideoCompositor.State.NotConnected)
            {
                switch (trigger)
                {
                    case RainbowBotVideoCompositor.Trigger.Disconnect:
                        // The bot has received the "stop message" from the bot master.
                        message = $"The bot [{botVideoCompositor.BotName}] has received the \"STOP message\" from the bot master...";
                        canContinue = false;
                        break;

                    case RainbowBotVideoCompositor.Trigger.ServerNotReachable:
                        // The server has not been reached.
                        // Need to add logic to try again OR something else ?
                        message = $"The bot [{botVideoCompositor.BotName}] cannot reach the server ...";
                        canContinue = false;
                        break;

                    case RainbowBotVideoCompositor.Trigger.TooManyAttempts:
                        // Bot was logged at least once but since we can reach the server after several attempts
                        // Need to add logic to try again OR something else ?
                        message = $"The bot [{botVideoCompositor.BotName}] was connected but after several attempts it can't reach the server anymore...";
                        canContinue = false;
                        break;
                }
            }
            else if (state == RainbowBotVideoCompositor.State.Created)
            {
                switch (trigger)
                {
                    case RainbowBotVideoCompositor.Trigger.IncorrectCredentials:
                        message = $"The bot [{botVideoCompositor.BotName}] is not connected because the credentials are not correct ...";
                        canContinue = false;
                        break;
                }
            }

            return (state, canContinue, message);
        }
    }
}
