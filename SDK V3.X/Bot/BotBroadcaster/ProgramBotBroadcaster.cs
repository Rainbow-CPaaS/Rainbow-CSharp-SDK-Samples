﻿using BotLibrary;
using Microsoft.Extensions.Logging;
using Rainbow;
using Rainbow.Example.Common;
using Rainbow.SimpleJSON;
using System;
using System.IO;
using System.Threading.Tasks;
using Util = Rainbow.Example.Common.Util;

namespace BotBroadcaster
{
    internal class Program
    {
        private static ExeSettings? _exeSettings;
        private static BotBroadcaster? _botBroadcaster;

        static async Task Main()
        {
            Util.WriteGreen($"{Global.ProductName()} v{Global.FileVersion()}");

            Util.WriteGreen($"[ESC] To stop the bot");

            if (!ReadExeSettings())
                return;

            if (!await ConfigureBot())
                return;

            if ( (_botBroadcaster is null) || (_exeSettings is null) )
                return;

            // Get Dot Graph and store it in a file
            var dotGraph = _botBroadcaster.ToDotGraph();
            var dotGraphFile = Path.Combine(_exeSettings.LogFolderPath, _botBroadcaster.BotName + ".dotgraph");
            try
            {
                File.WriteAllText(dotGraphFile, dotGraph);
            }
            catch //(Exception ex)
            {

            }

            if (!_botBroadcaster.Login())
                Util.WriteRed("Cannot start login process");

            var isStopped = false;
            SdkError? sdkError = null;
            while (!isStopped)
            {
                await Task.Delay(1000);

                while (Console.KeyAvailable)
                {
                    var userInput = Console.ReadKey(true);

                    // If [ESC] is used, we ask the bot to log out
                    if (userInput.Key == ConsoleKey.Escape)
                        _botBroadcaster.Logout();
                }

                (isStopped, sdkError) = _botBroadcaster.IsStopped();
                    
            }
            Util.WriteRed($"Bot as stopped:{Rainbow.Util.CR}{sdkError}");
        }

        static async Task<Boolean> ConfigureBot()
        {

            String credentialsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}credentials.json";
            if (!File.Exists(credentialsFilePath))
            {
                Util.WriteRed($"The file '{credentialsFilePath}' has not been found.");
                return false;
            }
            String jsonConfig = File.ReadAllText(credentialsFilePath);
            var jsonNode = JSON.Parse(jsonConfig);

            if (jsonNode?["credentials"]?.IsObject != true )
            {
                Util.WriteRed($"Cannot get JSON object 'credentials' from file '{credentialsFilePath}'.");
                return false;
            }
            var jsonNodeBotSettings = jsonNode["credentials"];


            String botConfigurationFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}botConfiguration.json";
            if (!File.Exists(botConfigurationFilePath))
            {
                Util.WriteRed($"The file '{botConfigurationFilePath}' has not been found.");
                return false;
            }

            jsonConfig = File.ReadAllText(botConfigurationFilePath);
            jsonNode = JSON.Parse(jsonConfig);
            if (jsonNode?["botConfiguration"]?.IsObject != true)
            {
                Util.WriteRed($"Cannot get JSON object 'botConfiguration' from file '{credentialsFilePath}'.");
                return false;
            }
            var jsonNodeBotConfiguration = jsonNode["botConfiguration"];

            _botBroadcaster = new();
            if (!(await _botBroadcaster.Configure(jsonNodeBotSettings, jsonNodeBotConfiguration)))
            {
                Util.WriteRed($"Cannot configure bot");
                return false;
            }

            var log = Rainbow.LogFactory.CreateLogger("FFmpeg", _botBroadcaster.BotName + "_");
            Rainbow.Medias.Helper.SetFFmpegLog(log, LogLevel.Error);

            return true;
        }

        static Boolean ReadExeSettings()
        {
            String exeSettingsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}exeSettings.json";
            if (!File.Exists(exeSettingsFilePath))
            {
                Util.WriteRed($"The file '{exeSettingsFilePath}' has not been found.");
                return false;
            }

            String jsonConfig = File.ReadAllText(exeSettingsFilePath);
            var jsonNode = JSON.Parse(jsonConfig);

            if ((jsonNode is null) || (!jsonNode.IsObject))
            {
                Util.WriteRed($"Cannot get JSON data from file '{exeSettingsFilePath}'.");
                return false;
            }

            var jsonExeSettings = jsonNode["exeSettings"];
            if (ExeSettings.FromJsonNode(jsonNode["exeSettings"], out _exeSettings))
            {
                // Set where log files must be stored
                NLogConfigurator.Directory = _exeSettings.LogFolderPath;

                // Init external librairies: FFmpeg and SDL2
                if(_exeSettings.UseAudioVideo)
                    Rainbow.Medias.Helper.InitExternalLibraries(_exeSettings.FfmpegLibFolderPath);
            }
            else
            {
                Util.WriteRed($"Cannot read 'exeSettings' object OR invalid/missing data - file:'{exeSettingsFilePath}'.");
                return false;
            }

            return true;
        }
    }
}
