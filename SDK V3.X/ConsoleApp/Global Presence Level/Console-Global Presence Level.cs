
using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;
using System.Text;


using Rainbow.SimpleJSON;
using Rainbow.Example.Common;

ExeSettings? exeSettings = null;
Credentials? credentials = null;

if ((!ReadExeSettings()) || (exeSettings is null))
    return;

credentials = ReadCredentials("credentials.json");
if (credentials is null)
    return;

// --------------------------------------------------

// Check if a Callback URL has been set
if (String.IsNullOrEmpty(exeSettings.S2SCallbackURL))
{
    ConsoleAbstraction.WriteRed($"No Callback URL has been set. This example needs one to create a WebHook.\r\n Ensure to set one using 'exeSettings.json' file and 's2sCallbackURL' property.");
    return;
}
else
    ConsoleAbstraction.WriteBlue($"Callback URL used to create the WebHook:{exeSettings.S2SCallbackURL}");


// Set folder / directory path
NLogConfigurator.Directory = exeSettings.LogFolderPath;
var logFullPath = Path.GetFullPath(exeSettings.LogFolderPath);
ConsoleAbstraction.WriteBlue($"Logs files will be stored in folder:[{logFullPath}]{Rainbow.Util.CR}");

ConsoleAbstraction.WriteBlue($"Use [ESC] at anytime to quit{Rainbow.Util.CR}");
ConsoleAbstraction.WriteBlue($"Use [S] to have Company Event Subscription Status (once created)");

// Create admin bot
var adminBot = new RainbowAdminBot(credentials.ServerConfig, credentials.UsersConfig[0], exeSettings.S2SCallbackURL);
adminBot.ConnectionStateChanged += (connectionState) => AdminBot_ConnectionStateChanged(adminBot.RainbowAccount, connectionState);
adminBot.ConnectionFailed += (sdkError) => AdminBot_ConnectionFailed(adminBot.RainbowAccount, sdkError);

do
{
    CheckInputKey();
    await Task.Delay(200);
} while (true);


void CheckInputKey()
{
    while (ConsoleAbstraction.KeyAvailable)
    {
        var userInput = ConsoleAbstraction.ReadKey();

        switch (userInput?.Key)
        {
            case ConsoleKey.Escape:
                ConsoleAbstraction.WriteYellow($"Asked to end process using [ESC] key");
                System.Environment.Exit(0);
                return;

            case ConsoleKey.S:
                Task.Run(async () =>
                {
                    var sdkResult = await adminBot.GetCompanyEventSubscriptionStatusAsync();
                    if (sdkResult.Success)
                        ConsoleAbstraction.WriteGreen($"GetCompanyEventSubscriptionStatusAsync:[{sdkResult.Data}]");
                    else
                        ConsoleAbstraction.WriteRed($"GetCompanyEventSubscriptionStatusAsync - Error:[{sdkResult.Success}]");
                });
                return;
        }
    }
}

#region EVENTS TRIGGERED BY RainbowAdminBot

void AdminBot_ConnectionStateChanged(UserConfig rainbowAccount, ConnectionState connectionState)
{
    switch (connectionState.Status)
    {
        case ConnectionStatus.Connected:
            ConsoleAbstraction.WriteDarkYellow($"Bot using [{rainbowAccount.Login}] is connected{Rainbow.Util.CR}");
            break;

        case ConnectionStatus.Connecting:
            ConsoleAbstraction.WriteBlue($"Bot using [{rainbowAccount.Login}] is connecting ...{Rainbow.Util.CR}");
            break;

        case ConnectionStatus.Disconnected:
            ConsoleAbstraction.WriteRed($"Bot using [{rainbowAccount.Login}] is disconnected{Rainbow.Util.CR}");
            break;
    }
}

void AdminBot_ConnectionFailed(UserConfig rainbowAccount, SdkError sdkError)
{
    ConsoleAbstraction.WriteRed($"Bot using [{rainbowAccount.Login}] is not connected - Error:[{sdkError}]{Rainbow.Util.CR}");
}


#endregion EVENTS TRIGGERED BY RainbowAdminBot

Boolean ReadExeSettings()
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

    if (ExeSettings.FromJsonNode(jsonNode["exeSettings"], out exeSettings))
    {
        // Set where log files must be stored
        NLogConfigurator.Directory = exeSettings.LogFolderPath;
    }
    else
    {
        ConsoleAbstraction.WriteRed($"Cannot read 'exeSettings' object OR invalid/missing data - file:'{exeSettingsFilePath}'.");
        return false;
    }

    return true;
}

Credentials? ReadCredentials(string fileName = "credentials.json")
{
    var credentialsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}{fileName}";
    if (!File.Exists(credentialsFilePath))
    {
        ConsoleAbstraction.WriteRed($"The file '{credentialsFilePath}' has not been found.");
        return null;
    }

    String jsonConfig = File.ReadAllText(credentialsFilePath);
    var jsonNode = JSON.Parse(jsonConfig);

    var credentials = Credentials.FromJsonNode(jsonNode["credentials"]);
    if (credentials?.IsValid() != true)
    {
        ConsoleAbstraction.WriteRed($"Cannot read 'credentials' object OR invalid/missing data in file:[{fileName}].");
        return null;
    }

    return credentials;
}
