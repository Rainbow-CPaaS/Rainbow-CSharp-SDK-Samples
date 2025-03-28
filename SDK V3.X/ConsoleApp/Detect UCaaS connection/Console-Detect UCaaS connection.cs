
using LoginSSO;
using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;
using System.Text;

using Util = Rainbow.Console.Util;
using Rainbow.SimpleJSON;
using Rainbow.Example.Common;

ExeSettings? exeSettings = null;
Credentials? credentialsUsers = null;
Credentials? credentialsAdmin = null;

if ((!ReadExeSettings()) || (exeSettings is null))
    return;

credentialsUsers = ReadCredentials("credentials.json");
if (credentialsUsers is null)
    return;

credentialsAdmin = ReadCredentials("credentials-Admin.json");
if (credentialsAdmin is null)
    return;

// --------------------------------------------------

Object consoleLockObject = new(); // To lock until the current console display is performed

Console.OutputEncoding = Encoding.UTF8; // We want to display UTF8 on the console

List<RainbowBot> rainbowBots = new ();


// Set folder / directory path
NLogConfigurator.Directory = exeSettings.LogFolderPath;
var logFullPath = Path.GetFullPath(exeSettings.LogFolderPath);
Util.WriteBlue($"Logs files will be stored in folder:[{logFullPath}]{Rainbow.Util.CR}");

Util.WriteBlue($"Use [ESC] at anytime to quit{Rainbow.Util.CR}");

// Create admin bot
var adminBot = new RainbowAdminBot(credentialsAdmin.ServerConfig, credentialsAdmin.UsersConfig[0]);
adminBot.ConnectionStateChanged += (connectionState) => RainbowBot_ConnectionStateChanged(adminBot.RainbowAccount, connectionState);
adminBot.ConnectionFailed += (sdkError) => RainbowBot_ConnectionFailed(adminBot.RainbowAccount, sdkError);
adminBot.SIPDeviceStatusChanged += AdminBot_SIPDeviceStatusChanged;



foreach (var account in credentialsUsers.UsersConfig)
{
    var rainbowBot = new RainbowBot(credentialsUsers.ServerConfig, account);
    rainbowBots.Add(rainbowBot);

    // Set events we want to follow 
    rainbowBot.ConnectionStateChanged += (connectionState) => RainbowBot_ConnectionStateChanged(rainbowBot.RainbowAccount, connectionState);
    rainbowBot.ConnectionFailed += (sdkError) => RainbowBot_ConnectionFailed(rainbowBot.RainbowAccount, sdkError);
    
    rainbowBot.AccountUsedOnAnotherDevice += (accountUsedOnAnotherDevice) => RainbowBot_AccountUsedOnAnotherDevice(rainbowBot.RainbowAccount, accountUsedOnAnotherDevice);
}


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
        }
    }
}

#region EVENTS TRIGGERED BY RainbowAdminBot

void AdminBot_SIPDeviceStatusChanged(SIPDeviceStatus sipDeviceStatus)
{
    var log = $"Device status changed: Registered[{sipDeviceStatus.Registered}] - ShortNumber:[{sipDeviceStatus.DeviceShortNumber}] - Peer:[{sipDeviceStatus.Peer.ToString("small")}]";
    if (sipDeviceStatus.Registered)
        Util.WriteDarkYellow(log);
    else
        Util.WriteRed(log);
}
#endregion EVENTS TRIGGERED BY RainbowAdminBot

#region EVENTS TRIGGERED BY RainbowBot

void RainbowBot_ConnectionStateChanged(UserConfig rainbowAccount, ConnectionState connectionState)
{
    switch(connectionState.Status)
    {
        case ConnectionStatus.Connected:
            Util.WriteDarkYellow($"Bot using [{rainbowAccount.Login}] is connected{Rainbow.Util.CR}");
            break;

        case ConnectionStatus.Connecting:
            Util.WriteBlue($"Bot using [{rainbowAccount.Login}] is connecting ...{Rainbow.Util.CR}");
            break;

        case ConnectionStatus.Disconnected:
            Util.WriteRed ($"Bot using [{rainbowAccount.Login}] is disconnected{Rainbow.Util.CR}");
            break;
    }
}

void RainbowBot_ConnectionFailed(UserConfig rainbowAccount, SdkError sdkError)
{
    Util.WriteRed($"Bot using [{rainbowAccount.Login}] is not connected - Error:[{sdkError}]{Rainbow.Util.CR}");
}

void RainbowBot_AccountUsedOnAnotherDevice(UserConfig rainbowAccount, Boolean accountUsedOnAnotherDevice)
{
    if (accountUsedOnAnotherDevice)
        Util.WriteDarkYellow($"[{rainbowAccount.Login}] is connected using another device{Rainbow.Util.CR}");
    else
        Util.WriteRed($"[{rainbowAccount.Login}] is NOT connected using another device{Rainbow.Util.CR}");
}


#endregion EVENTS TRIGGERED BY RainbowBot


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
