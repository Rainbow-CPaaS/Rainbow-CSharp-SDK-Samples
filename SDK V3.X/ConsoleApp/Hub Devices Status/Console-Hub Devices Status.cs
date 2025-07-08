
using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;
using System.Text;

using Util = Rainbow.Example.Common.Util;
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

Console.OutputEncoding = Encoding.UTF8; // We want to display UTF8 on the console

// Check if a Callback URL has been set
if (String.IsNullOrEmpty(exeSettings.S2SCallbackURL))
{
    Util.WriteRed($"No Callback URL has been set. This example needs one to create a WebHook.\r\n Ensure to set one using 'exeSettings.json' file and 's2sCallbackURL' property.");
    return;
}
else
    Util.WriteBlue($"Callback URL used to create the WebHook:{exeSettings.S2SCallbackURL}");


// Set folder / directory path
NLogConfigurator.Directory = exeSettings.LogFolderPath;
var logFullPath = Path.GetFullPath(exeSettings.LogFolderPath);
Util.WriteBlue($"Logs files will be stored in folder:[{logFullPath}]{Rainbow.Util.CR}");

Util.WriteBlue($"Use [ESC] at anytime to quit{Rainbow.Util.CR}");
Util.WriteBlue($"Use [S] to have Company Event Subscription Status (once created)");

// Create admin bot
var adminBot = new RainbowAdminBot(credentials.ServerConfig, credentials.UsersConfig[0], exeSettings.S2SCallbackURL);
adminBot.ConnectionStateChanged += (connectionState) => AdminBot_ConnectionStateChanged(adminBot.RainbowAccount, connectionState);
adminBot.ConnectionFailed += (sdkError) => AdminBot_ConnectionFailed(adminBot.RainbowAccount, sdkError);
adminBot.SIPDeviceStatusChanged += AdminBot_SIPDeviceStatusChanged;

do
{
    CheckInputKey();
    await Task.Delay(200);
} while (true);


void CheckInputKey()
{
    while (Console.KeyAvailable)
    {
        var userInput = Console.ReadKey(true);

        switch (userInput.Key)
        {
            case ConsoleKey.Escape:
                Util.WriteYellow($"Asked to end process using [ESC] key");
                System.Environment.Exit(0);
                return;

            case ConsoleKey.S:
                Task.Run(async () =>
                {
                    var sdkResult = await adminBot.GetCompanyEventSubscriptionStatusAsync();
                    if (sdkResult.Success)
                        Util.WriteGreen($"GetCompanyEventSubscriptionStatusAsync:[{sdkResult.Data}]");
                    else
                        Util.WriteRed($"GetCompanyEventSubscriptionStatusAsync - Error:[{sdkResult.Success}]");
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
            Util.WriteDarkYellow($"Bot using [{rainbowAccount.Login}] is connected{Rainbow.Util.CR}");
            break;

        case ConnectionStatus.Connecting:
            Util.WriteBlue($"Bot using [{rainbowAccount.Login}] is connecting ...{Rainbow.Util.CR}");
            break;

        case ConnectionStatus.Disconnected:
            Util.WriteRed($"Bot using [{rainbowAccount.Login}] is disconnected{Rainbow.Util.CR}");
            break;
    }
}

void AdminBot_ConnectionFailed(UserConfig rainbowAccount, SdkError sdkError)
{
    Util.WriteRed($"Bot using [{rainbowAccount.Login}] is not connected - Error:[{sdkError}]{Rainbow.Util.CR}");
}

void AdminBot_SIPDeviceStatusChanged(SIPDeviceStatus sipDeviceStatus)
{
    var log = $"Device status changed: Registered[{sipDeviceStatus.Registered}] - ShortNumber:[{sipDeviceStatus.DeviceShortNumber}] - Peer:[{sipDeviceStatus.Peer.ToString("small")}]";
    if (sipDeviceStatus.Registered)
        Util.WriteDarkYellow(log);
    else
        Util.WriteRed(log);
}

#endregion EVENTS TRIGGERED BY RainbowAdminBot

Boolean ReadExeSettings()
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

    if (ExeSettings.FromJsonNode(jsonNode["exeSettings"], out exeSettings))
    {
        // Set where log files must be stored
        NLogConfigurator.Directory = exeSettings.LogFolderPath;
    }
    else
    {
        Util.WriteRed($"Cannot read 'exeSettings' object OR invalid/missing data - file:'{exeSettingsFilePath}'.");
        return false;
    }

    return true;
}

Credentials? ReadCredentials(string fileName = "credentials.json")
{
    var credentialsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}{fileName}";
    if (!File.Exists(credentialsFilePath))
    {
        Util.WriteRed($"The file '{credentialsFilePath}' has not been found.");
        return null;
    }

    String jsonConfig = File.ReadAllText(credentialsFilePath);
    var jsonNode = JSON.Parse(jsonConfig);

    if (!Credentials.FromJsonNode(jsonNode["credentials"], out Credentials credentials))
    {
        Util.WriteRed($"Cannot read 'credentials' object OR invalid/missing data in file:[{fileName}].");
        return null;
    }

    return credentials;
}
