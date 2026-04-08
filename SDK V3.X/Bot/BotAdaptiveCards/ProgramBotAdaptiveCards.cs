using BotLibrary;
using Rainbow.Example.Common;
using Rainbow.SimpleJSON;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace BotAdaptiveCards
{
    internal class ProgramBotBasicMessages
    {
        private static ExeSettings? _exeSettings;
        private static BotBasicMessages? _botBasic;

        static async Task Main()
        {
            ConsoleAbstraction.WriteGreen($"{Global.ProductName()} v{Global.FileVersion()}");
            ConsoleAbstraction.WriteGreen($"[ESC] To stop the bot");

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
                ConsoleAbstraction.WriteRed("Cannot start login process");

            var isStopped = false;
            while (!isStopped)
            {
                Thread.Sleep(500);

                while (ConsoleAbstraction.KeyAvailable)
                {
                    var userInput = ConsoleAbstraction.ReadKey();

                    // If [ESC] is used, we ask the bot to log out
                    if (userInput?.Key == ConsoleKey.Escape)
                        _botBasic.Logout();
                }

                (isStopped, var sdkError) = _botBasic.IsStopped();
                if(isStopped)
                    ConsoleAbstraction.WriteRed($"Bot as stopped:{Rainbow.Util.CR}{sdkError}");
            }
        }

        static async Task<Boolean> ConfigureBot()
        {

            String credentialsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}credentials.json";
            if (!File.Exists(credentialsFilePath))
            {
                ConsoleAbstraction.WriteRed($"The file '{credentialsFilePath}' has not been found.");
                return false;
            }
            String jsonConfig = File.ReadAllText(credentialsFilePath);
            var jsonNode = JSON.Parse(jsonConfig);

            if (jsonNode?["credentials"]?.IsObject != true)
            {
                ConsoleAbstraction.WriteRed($"Cannot get JSON object 'credentials' from file '{credentialsFilePath}'.");
                return false;
            }
            var jsonNodeBotSettings = jsonNode["credentials"];


            String botConfigurationFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}botConfiguration.json";
            if (!File.Exists(botConfigurationFilePath))
            {
                ConsoleAbstraction.WriteRed($"The file '{botConfigurationFilePath}' has not been found.");
                return false;
            }

            jsonConfig = File.ReadAllText(botConfigurationFilePath);
            jsonNode = JSON.Parse(jsonConfig);
            if (jsonNode?["botConfiguration"]?.IsObject != true)
            {
                ConsoleAbstraction.WriteRed($"Cannot get JSON object 'botConfiguration' from file '{credentialsFilePath}'.");
                return false;
            }
            var jsonNodeBotConfiguration = jsonNode["botConfiguration"];

            _botBasic = new();
            if (!(await _botBasic.Configure(jsonNodeBotSettings, jsonNodeBotConfiguration)))
            {
                ConsoleAbstraction.WriteRed($"Cannot configure bot");
                return false;
            }

            return true;
        }

        static Boolean ReadExeSettings()
        {
            String exeSettingsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}exeSettings.json";
            if (!File.Exists(exeSettingsFilePath))
            {
                ConsoleAbstraction.WriteRed($"The file '{exeSettingsFilePath}' has not been found.");
                return false;
            }

            String jsonConfig = File.ReadAllText(exeSettingsFilePath);
            var jsonNode = JSON.Parse(jsonConfig);

            if ((jsonNode is null) || (!jsonNode.IsObject))
            {
                ConsoleAbstraction.WriteRed($"Cannot get JSON data from file '{exeSettingsFilePath}'.");
                return false;
            }

            if (ExeSettings.FromJsonNode(jsonNode["exeSettings"], out _exeSettings))
            {
                // Set where log files must be stored
                NLogConfigurator.Directory = _exeSettings.LogFolderPath;
            }
            else
            {
                ConsoleAbstraction.WriteRed($"Cannot read 'exeSettings' object OR invalid/missing data - file:'{exeSettingsFilePath}'.");
                return false;
            }

            return true;
        }
    }
}
