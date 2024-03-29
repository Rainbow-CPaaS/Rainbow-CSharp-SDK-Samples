﻿using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Extensions.Logging;
using Org.BouncyCastle.Asn1.X509;
using Rainbow;
using Rainbow.Medias;
using Rainbow.SimpleJSON;
using Rainbow.WebRTC.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace BotVideoBroadcaster
{
    internal class Program
    {
        private static List<BotVideoBroadcasterInfo>? botVideoBroadcasterInfos;
        private static Boolean TestLoginAndInitializationStepsOnly = false;

        static void Main()
        {
            int iteration = 0;

            Util.WriteDebugToConsole($"{Global.ProductName()} v{Global.FileVersion()}");

            DeleteUnnecessaryFiles();

            // Check done on values defined in file RainbowApplicationInfo.cs AND in config.json
            if (!CheckApplicationInfoValues())
                return;

            // Init log configuration
            InitLogsWithNLog();

            var mechanisms = new List<String>() {
            { "SCRAM-SHA-512" } };
            Application.SetXMPPAuthenticationMechanisms(mechanisms);

            IVideoStreamTrack? videoStreamTrackToUse = null;
            if (RainbowApplicationInfo.useOnlyOneStream && RainbowApplicationInfo.videosUri?.Count > 0)
            {
                var uri = RainbowApplicationInfo.videosUri[0];
                var uriDevice = new InputStreamDevice("videoStream", "videoStream", uri, true, false, true);
                var RbWebRTCFactory = new Rainbow.WebRTC.Desktop.WebRTCFactory();
                var streamToUse = new MediaInput(uriDevice);
                if (!streamToUse.Init(true))
                {
                    Util.WriteErrorToConsole($"Cannot init COMMON Video Stream using this URI:{uri}].");
                    return;
                }
                videoStreamTrackToUse = RbWebRTCFactory.CreateVideoTrack(streamToUse);
            }


            // Create BotVideoBroadcaster 01 && 02
            RainbowBotVideoBroadcaster botVideoBroadcaster = new RainbowBotVideoBroadcaster();

            // Get the dot graph of this Bot - it's a String which can be used to represent the state machine diagram on a online tool like https://dreampuf.github.io/GraphvizOnline/
            string graph = botVideoBroadcaster.ToDotGraph();


            botVideoBroadcasterInfos = new List<BotVideoBroadcasterInfo>();
            int index = 0;
            int nbBots = 0;
            foreach (var account in RainbowApplicationInfo.botsVideoBroadcaster)
            {
                var botVideoBroadcasterInfo = new BotVideoBroadcasterInfo(account.Login, account.Password, videoStreamTrackToUse);
                botVideoBroadcasterInfo.Name = $"BOT{(index + 1).ToString("00")}";

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
            nbBots = index;

            RainbowBotVideoBroadcaster.State botState;
            Boolean botCanContinue;
            String botMessage;


        START_BOT:

            if (TestLoginAndInitializationStepsOnly)
                Util.WriteErrorToConsole($"[{DateTime.Now.ToString("o")}]  STARTING ITERATION: [{++iteration}]");
                
            // Check if a Bot has not been started / Created
            foreach (var bot in botVideoBroadcasterInfos)
            {
                (botState, botCanContinue, botMessage) = bot.CheckBotStatus();

                // If a bot has just been created, we configure it and start login
                if (botState == RainbowBotVideoBroadcaster.State.Created)
                {
                    bot.Configure(RainbowApplicationInfo.commandStop, RainbowApplicationInfo.botManagers);

                    Thread.Sleep(30);
                    //do
                    //{
                    //    Thread.Sleep(30);
                    //    (botState, botCanContinue, botMessage) = bot.CheckBotStatus();
                    //    if (!botCanContinue)
                    //    {
                    //        Util.WriteErrorToConsole($"[{bot.Name}] Cannot connect to the server ...");
                    //        return;
                    //    }
                    //    else
                    //    {
                    //        if (botState == RainbowBotVideoBroadcaster.State.Initialized)
                    //            break;
                    //    }

                    //} while (true);
                }
                else if (botState == RainbowBotVideoBroadcaster.State.AutoReconnection)
                {
                    bot.StartLogin();
                    Thread.Sleep(30);
                }
            }

            Boolean canContinue = true;
            do
            {
                index = 0;
                foreach (var bot in botVideoBroadcasterInfos)
                {
                    (botState, botCanContinue, botMessage) = bot.CheckBotStatus();

                    if (TestLoginAndInitializationStepsOnly && (botState == RainbowBotVideoBroadcaster.State.Initialized))
                    {
                        index++;
                        if (index == nbBots)
                        {
                            canContinue = false;
                            Util.WriteErrorToConsole($"ALL BOTS ARE CONNECTED / INITIALIZED");
                        }
                    }
                }

                Thread.Sleep(30);

            } while (canContinue);

            if (TestLoginAndInitializationStepsOnly)
            {

                foreach (var bot in botVideoBroadcasterInfos)
                {
                    bot.StopBot();
                }

                do
                {
                    index = 0;
                    foreach (var bot in botVideoBroadcasterInfos)
                    {
                        (botState, botCanContinue, botMessage) = bot.CheckBotStatus();

                        if (botState == RainbowBotVideoBroadcaster.State.AutoReconnection)
                        {
                            index++;
                            if (index == nbBots)
                                canContinue = false;
                        }
                        else
                        {

                        }
                    }

                    Thread.Sleep(30);

                } while (canContinue);


                if (TestLoginAndInitializationStepsOnly)
                    Util.WriteErrorToConsole($"END ITERATION: [{iteration}]");
                //Thread.Sleep(2000);

                goto START_BOT;
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
                string logLoggerFilePath = RainbowApplicationInfo.NLOG_CONFIG_FILE_LOGGER;
                string logTargetFilePath = RainbowApplicationInfo.NLOG_CONFIG_FILE_TARGET;

                if (File.Exists(logConfigFilePath))
                {
                    // Get content of the log file configuration
                    String logConfigContent = File.ReadAllText(logConfigFilePath, System.Text.Encoding.UTF8);
                    String logLoggerContent = File.ReadAllText(logLoggerFilePath, System.Text.Encoding.UTF8);
                    String logTargetContent = File.ReadAllText(logTargetFilePath, System.Text.Encoding.UTF8);

                    string loggers = "";
                    string targets = "";

                    if (RainbowApplicationInfo.botsVideoBroadcaster?.Count > 0)
                    {
                        int nb = RainbowApplicationInfo.botsVideoBroadcaster.Count;
                        for (int i = 1; i<=nb; i++)
                        {
                            loggers += logLoggerContent.Replace("[$PREFIX_WEBRTC]", $"BOT{i.ToString("00")}_")
                                        .Replace("[$PREFIX]", $"BOT{i.ToString("00")}_")
                                        + "\r\n";

                            targets += logTargetContent.Replace("[$PREFIX_WEBRTC]", $"BOT{i.ToString("00")}_")
                                        .Replace("[$PREFIX]", $"BOT{i.ToString("00")}_")
                                        + "\r\n";
                        }
                    }

                    // ADD DEFAULT Loggers and Targets
                    loggers += logLoggerContent.Replace("[$PREFIX_WEBRTC]", "")
                                        .Replace("[$PREFIX]", $"Rainbow")
                                        .Replace("RainbowRAINBOW_DEFAULT_LOGGER", "RAINBOW_DEFAULT_LOGGER")
                                        + "\r\n";

                    targets += logTargetContent.Replace("[$PREFIX_WEBRTC]", "")
                                .Replace("[$PREFIX]", "")
                                + "\r\n";

                    logConfigContent = logConfigContent.Replace("<!--RULES-->", loggers)
                                        .Replace("<!--TARGETS-->", targets);

                    // Create NLog configuration using XML file content
                    XmlLoggingConfiguration config = XmlLoggingConfiguration.CreateFromXmlString(logConfigContent);
                    if (config.InitializeSucceeded == true)
                    {
                        // Create Logger factory
                        var factory = LoggerFactory.Create(builder => builder.AddNLog(config));

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

                RainbowApplicationInfo.useOnlyOneStream = UtilJson.AsBoolean(json, "useOnlyOneStream", false);

                RainbowApplicationInfo.createDataChannel = UtilJson.AsBoolean(json, "createDataChannel", false);

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

        static private void DeleteUnnecessaryFiles()
        {
            var oldLogFiles = Directory.GetFiles(".\\", "BOT??_*.log");
            var oldIniFiles = Directory.GetFiles(".\\", "BOT??.ini");
            var oldSdpFiles = Directory.GetFiles(".\\", "SDK_CSHARP_SDP_TEMPORARY_FILE_*.txt");

            var oldFiles = oldLogFiles.Concat(oldIniFiles).Concat(oldSdpFiles);

            foreach (var oldFile in oldFiles)
            {
                try
                {
                    File.Delete(oldFile);
                }
                catch { }
            }
        }
    }
}
