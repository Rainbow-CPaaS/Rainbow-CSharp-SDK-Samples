using Microsoft.Extensions.Logging;
using Rainbow;
using System.Text;

using Rainbow.Console;
using Util = Rainbow.Console.Util;
using Rainbow.SimpleJSON;
using Rainbow.Example.Common;

// --------------------------------------------------

ExeSettings? exeSettings = null;
Credentials? credentials = null;

if ((!ReadExeSettings()) || (exeSettings is null))
    return;

if ((!ReadCredentials()) || (credentials is null))
    return;

Util.WriteRed($"Account used: [{credentials.UsersConfig[0].Login}]");

// --------------------------------------------------

Console.OutputEncoding = Encoding.UTF8; // We want to display UTF8 on the console

Object consoleLockObject = new(); // To lock until the current console display is performed
String CR = Rainbow.Util.CR; // Get carriage return;

// We want to log files from SDK
// But also from this example

// In "exeSettings.json" using "logFolderPath" property, we defined a folder where the logs must be stored
String logFolderPath = exeSettings.LogFolderPath;

// In "credentials.json" using "userConfig" object, we defined a prefix used as logger prefix (this prefix permits to have logs stored in specific file for this "userConfig")
String logPrefix = credentials.UsersConfig[0].Prefix;

// Using NLogConfigurator, we specify the folder where log will be stored
NLogConfigurator.Directory = logFolderPath;

// Using NLogConfigurator, we add a logger using the preix
NLogConfigurator.AddLogger(logPrefix);

/// This logger use NLog but you can use any logger based on Microsoft Extension Logging (MEL)
/// See https://developers.openrainbow.com/doc/sdk/csharp/core/lts/guides/130_application_logging for more details
/// By default, two log files are created "RainbowSdk.log" and "RainbowSdk_WebRTC.log" (if WebRTC is used) in the foder where this process is running
/// See more details in <see cref="NLogConfigurator"/> object.

/// Create Rainbow SDK objects - using the prefix
var RbApplication = new Application(loggerPrefix: logPrefix);

/// Since we have created a logger and a Rainbow.Application with the same logger prefix, the SDK will add log entries automatically.
Util.WriteYellow($"{CR}Log files have been created\r\nStored here:{Path.GetFullPath(logFolderPath)}");

/// You can also add log entries in same files if you wish.
/// We create a ILogger from the LogFactory using "ConsoleApp" as CategoryName
/// AND with the same prefix 
ILogger log = Rainbow.LogFactory.CreateLogger("ConsoleApp", logPrefix);

/// And to log info use these
log.LogDebug("Add log as Debug from my console app");
log.LogInformation("Add log as Information from my console app");
log.LogWarning("Add log as Warning from my console app");
log.LogError("Add log as Error from my console app");
log.LogCritical("Add log as Critical from my console app");

Util.WriteYellow($"{CR}We have added several log entries from this console application");

// Set global configuration info
RbApplication.SetApplicationInfo(credentials.ServerConfig.AppId, credentials.ServerConfig.AppSecret);
RbApplication.SetHostInfo(credentials.ServerConfig.HostName);

// Start login
Util.WriteWhite($"{CR}Starting login ...");
var sdkResult = await RbApplication.LoginAsync(credentials.UsersConfig[0].Login, credentials.UsersConfig[0].Password);

if (sdkResult.Success)
{
    // We are connected and SDK is ready
    Util.WriteGreen($"{CR}Connected to Rainbow Server");
}
else
{
    // We are not connected - Display why:
    Util.WriteRed($"{CR}Connection failed - SdkError:{sdkResult.Result}");
}

Util.WriteBlue($"{CR}Use [ESC] at anytime to quit");

Util.WriteBlue($"{CR}Try also this example with incorrect appId / appSecretKey / hostName AND/OR incorrect login / password");

Util.WriteBlue($"{CR}Nothing more is expected - You can quit");

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