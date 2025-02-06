using BotLibrary;
using BotLibrary.Model;
using Rainbow.SimpleJSON;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BotBasic
{
    internal class ProgramBotBasicMessages
    {
        private static ExeSettings? _exeSettings;
        private static BotBasicMessages? _botBasic;

        static async Task Main()
        {
            Util.WriteDebugToConsole($"{Global.ProductName()} v{Global.FileVersion()}");
            Util.WriteDebugToConsole($"[ESC] To stop the bot");

            if (!ReadExeSettings())
                return;

            if (!await ConfigureBot())
                return;

            if ((_botBasic is null) || (_exeSettings is null))
                return;

            // Get Dot Graph on the Bot and store it in a file
            var dotGraph = _botBasic.ToDotGraph();
            var dotGraphFile = Path.Combine(_exeSettings.LogFolderPath, _botBasic.BotName + ".dotgraph");
            try
            {
                File.WriteAllText(dotGraphFile, dotGraph);
            }
            catch {}

            if (!_botBasic.Login())
                Util.WriteErrorToConsole("Cannot start login process");

            var isStopped = false;
            while (!isStopped)
            {
                Thread.Sleep(500);

                while (Console.KeyAvailable)
                {
                    var userInput = Console.ReadKey(true);

                    // If [ESC] is used, we ask the bot to log out
                    if (userInput.Key == ConsoleKey.Escape)
                        _botBasic.Logout();
                }

                (isStopped, var sdkError) = _botBasic.IsStopped();
                if(isStopped)
                    Util.WriteErrorToConsole($"Bot as stopped:{Rainbow.Util.CR}{sdkError}");
            }
        }

        static async Task<Boolean> ConfigureBot()
        {

            String botCredentialsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}botCredentials.json";
            if (!File.Exists(botCredentialsFilePath))
            {
                BotLibrary.Util.WriteErrorToConsole($"The file '{botCredentialsFilePath}' has not been found.");
                return false;
            }
            String jsonConfig = File.ReadAllText(botCredentialsFilePath);
            var jsonNode = JSON.Parse(jsonConfig);

            if (jsonNode?["botCredentials"]?.IsObject != true)
            {
                BotLibrary.Util.WriteErrorToConsole($"Cannot get JSON object 'botCredentials' from file '{botCredentialsFilePath}'.");
                return false;
            }
            var jsonNodeBotSettings = jsonNode["botCredentials"];


            String botConfigurationFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}botConfiguration.json";
            if (!File.Exists(botConfigurationFilePath))
            {
                BotLibrary.Util.WriteErrorToConsole($"The file '{botConfigurationFilePath}' has not been found.");
                return false;
            }

            jsonConfig = File.ReadAllText(botConfigurationFilePath);
            jsonNode = JSON.Parse(jsonConfig);
            if (jsonNode?["botConfiguration"]?.IsObject != true)
            {
                BotLibrary.Util.WriteErrorToConsole($"Cannot get JSON object 'botConfiguration' from file '{botCredentialsFilePath}'.");
                return false;
            }
            var jsonNodeBotConfiguration = jsonNode["botConfiguration"];

            _botBasic = new();
            if (!(await _botBasic.Configure(jsonNodeBotSettings, jsonNodeBotConfiguration)))
            {
                BotLibrary.Util.WriteErrorToConsole($"Cannot configure bot");
                return false;
            }

            return true;
        }

        static Boolean ReadExeSettings()
        {
            String exeSettingsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}exeSettings.json";
            if (!File.Exists(exeSettingsFilePath))
            {
                BotLibrary.Util.WriteErrorToConsole($"The file '{exeSettingsFilePath}' has not been found.");
                return false;
            }

            String jsonConfig = File.ReadAllText(exeSettingsFilePath);
            var jsonNode = JSON.Parse(jsonConfig);

            if ((jsonNode is null) || (!jsonNode.IsObject))
            {
                BotLibrary.Util.WriteErrorToConsole($"Cannot get JSON data from file '{exeSettingsFilePath}'.");
                return false;
            }

            if (ExeSettings.FromJsonNode(jsonNode["exeSettings"], out _exeSettings))
            {
                // Set where log files must be stored
                NLogConfigurator.Directory = _exeSettings.LogFolderPath;
            }
            else
            {
                BotLibrary.Util.WriteErrorToConsole($"Cannot read 'exeSettings' object OR invalid/missing data - file:'{exeSettingsFilePath}'.");
                return false;
            }

            return true;
        }
    }
}
