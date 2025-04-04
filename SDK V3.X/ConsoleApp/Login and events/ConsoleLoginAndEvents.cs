using Rainbow;
using Rainbow.Consts;

using Rainbow.Example.Common;
using Util = Rainbow.Example.Common.Util;
using Rainbow.SimpleJSON;

// --------------------------------------------------

ExeSettings? exeSettings = null;
Credentials? credentials = null;

if ((!ReadExeSettings()) || (exeSettings is null))
    return;

if ((!ReadCredentials()) || (credentials is null))
    return;

Util.WriteRed($"Account used: [{credentials.UsersConfig[0].Login}]");

// --------------------------------------------------

Object consoleLockObject = new(); // To lock until the current console display is performed
String CR = Rainbow.Util.CR; // Get carriage return;

// In "exeSettings.json" using "logFolderPath" property, we defined a folder where the logs must be stored
String logFolderPath = exeSettings.LogFolderPath;

// In "credentials.json" using "userConfig" object, we defined a prefix used as logger prefix (this prefix permits to have logs stored in specific file for this "userConfig")
String logPrefix = credentials.UsersConfig[0].Prefix;

// Using NLogConfigurator, we specify the folder where log will be stored
NLogConfigurator.Directory = logFolderPath;

// Using NLogConfigurator, we add a logger using the preix
NLogConfigurator.AddLogger(logPrefix);

Rainbow.Util.SetLogAnonymously(false);

// Create Rainbow SDK objects
var RbApplication = new Application(loggerPrefix: logPrefix);

// We want to receive events from SDK
RbApplication.AuthenticationFailed += RbApplication_AuthenticationFailed;       // Triggered when the authentication process will fail
RbApplication.AuthenticationSucceeded += RbApplication_AuthenticationSucceeded; // Triggered when the authentication process will succeed
RbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;   // Triggered when the Connection State will change

// Set global configuration info
RbApplication.SetApplicationInfo(credentials.ServerConfig.AppId, credentials.ServerConfig.AppSecret);
RbApplication.SetHostInfo(credentials.ServerConfig.HostName);


Util.WriteBlue($"{CR}Use [ESC] at anytime to quit\r\n");

// Start login
Util.WriteWhite("Starting login ...");
var sdkResult = await RbApplication.LoginAsync(credentials.UsersConfig[0].Login, credentials.UsersConfig[0].Password);

do
{
    await Task.Delay(200);
    CheckInputKey();
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
                Environment.Exit(0);
                return;
        }
    }
}

#region Events received from the SDK
void RbApplication_ConnectionStateChanged(Rainbow.Model.ConnectionState connectionState)
{
    // We log connection state in the console - use differnte color according the status
    String color;
    switch (connectionState.Status)
    {
        case ConnectionStatus.Connected:
            color = Util.BLUE;
            break;

        case ConnectionStatus.Disconnected:
            color = Util.RED;
            break;

        case ConnectionStatus.Connecting:
        default:
            color = Util.GREEN;
            break;

    }
    Util.WriteToConsole($"{CR}{color}Event Application.ConnectionStateChanged triggered - Connection Status: [{connectionState.Status}]");
}

void RbApplication_AuthenticationSucceeded()
{
    // Authentication Succeeded- we display in the console the info
    Util.WriteBlue($"{CR}Event Application.AuthenticationSucceeded triggered");
}

void RbApplication_AuthenticationFailed(SdkError sdkError)
{
    // Authentication failed - we display in the console the reason
    Util.WriteRed($"{CR}Event Application.AuthenticationFailed triggered - SdkError:{sdkError}");
}
#endregion Events received from the SDK

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

Boolean ReadCredentials(string fileName = "credentials.json")
{
    var credentialsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}{fileName}";
    if (!File.Exists(credentialsFilePath))
    {
        Util.WriteRed($"The file '{credentialsFilePath}' has not been found.");
        return false;
    }

    String jsonConfig = File.ReadAllText(credentialsFilePath);
    var jsonNode = JSON.Parse(jsonConfig);

    if (!Credentials.FromJsonNode(jsonNode["credentials"], out credentials))
    {
        Util.WriteRed($"Cannot read 'credentials' object OR invalid/missing data in file:[{fileName}].");
        return false;
    }

    return true;
}

