using Microsoft.Extensions.Logging;
using Rainbow;
using System.Text;


using Rainbow.SimpleJSON;
using Rainbow.Example.Common;

//// --------------------------------------------------

ExeSettings? exeSettings = null;
Credentials? credentials = null;

if ((!ReadExeSettings()) || (exeSettings is null))
    return;

credentials = ReadCredentials("credentials.json");
if (credentials is null)
    return;


ConsoleAbstraction.WriteRed($"Account used: [{credentials.UsersConfig[0].Login}]");
ConsoleAbstraction.WriteRed($"Account used: [{credentials.UsersConfig[1].Login}]");

// --------------------------------------------------

//// --------------------------------------------------

Object consoleLockObject = new(); // To lock until the current console display is performed
String CR = Rainbow.Util.CR; // Get carriage return;


// We want to log files from SDK
// But also from this example

// In "exeSettings.json" using "logFolderPath" property, we defined a folder where the logs must be stored
String logFolderPath = exeSettings.LogFolderPath;

// In "credentials.json" and in "credentials2.json" using "userConfig" object, we defined a prefix used as logger prefix for each "userConfig"
String logPrefix = credentials.UsersConfig[0].Prefix;
String logPrefix2 = credentials.UsersConfig[1].Prefix;

// Using NLogConfigurator, we specify the folder where log will be stored
NLogConfigurator.Directory = logFolderPath;

// Using NLogConfigurator, we add loggers using their prefix
NLogConfigurator.AddLogger(logPrefix);
NLogConfigurator.AddLogger(logPrefix2);

// Each time you create a Rainbow.Application a logger prefix is used

/// This logger use NLog but you can use any logger based on Microsoft Extension Logging (MEL)
/// See https://developers.openrainbow.com/doc/sdk/csharp/core/lts/guides/130_application_logging for more details
/// By default, two log files are created "RainbowSdk.log" and "RainbowSdk_WebRTC.log" (if WebRTC is used) in the foder where this process is running
/// See more details in <see cref="NLogConfigurator"/> object.

/// Create Rainbow SDK objects

// We want to have an IniFileName different for each Rainbow Application
var iniFileName = logPrefix + "_file.ini";
var iniFileName2 = logPrefix2 + "_file.ini";

var iniFolderPath = logFolderPath; // We will store iniFile in same folder than logs

var RbApplication_1 = new Application(iniFolderFullPathName: iniFolderPath, iniFileName: iniFileName, loggerPrefix: logPrefix); // we create the first Rainbow Applicaiotn with the first prefix
var RbApplication_2 = new Application(iniFolderFullPathName: iniFolderPath, iniFileName: iniFileName2, loggerPrefix: logPrefix2); // we create the second Rainbow Applicaiotn with the second prefix

ConsoleAbstraction.WriteYellow($"{CR}Log files of the Rainbow SDK have been created");
ConsoleAbstraction.WriteYellow($"For {credentials.UsersConfig[0].Login}:");
ConsoleAbstraction.WriteYellow($"\tLogFile name is [{logPrefix}RainbowSdk.log] and stored in folder:[{Path.GetFullPath(".\\")}]");
ConsoleAbstraction.WriteYellow($"\tIniFile name is [{iniFileName}] and stored in folder:[{Path.GetFullPath(iniFolderPath)}]");

ConsoleAbstraction.WriteYellow($"{CR}For {credentials.UsersConfig[1].Login}:");
ConsoleAbstraction.WriteYellow($"\tLogFile name is [{logPrefix2}RainbowSdk.log] and stored in folder:[{Path.GetFullPath(".\\")}]");
ConsoleAbstraction.WriteYellow($"\tIniFile name is [{iniFileName2}] and stored in folder:[{Path.GetFullPath(iniFolderPath)}]");

/// You can also add log entries in a different for log entries created direclty in this consoleApp

/// We create a ILogger from the LogFactory using "ConsoleApp" as CategoryName
var prefix_consoleApp = "ConsoleApp";

NLogConfigurator.AddLogger(prefix_consoleApp);
ILogger log = Rainbow.LogFactory.CreateLogger("ConsoleApp", prefix: prefix_consoleApp);

/// And to log info use these
log.LogDebug("Add log as Debug from my console app");
log.LogInformation("Add log as Information from my console app");
log.LogWarning("Add log as Warning from my console app");
log.LogError("Add log as Error from my console app");
log.LogCritical("Add log as Critical from my console app");

ConsoleAbstraction.WriteYellow($"{CR}We have added several log entries from this console application");
ConsoleAbstraction.WriteYellow($"\tLogFile name is [{prefix_consoleApp}RainbowSdk.log] and stored in folder:[{Path.GetFullPath(".\\")}]");

// Set global configuration info
RbApplication_1.SetApplicationInfo(credentials.ServerConfig.AppId, credentials.ServerConfig.AppSecret);
RbApplication_1.SetHostInfo(credentials.ServerConfig.HostName);

RbApplication_2.SetApplicationInfo(credentials.ServerConfig.AppId, credentials.ServerConfig.AppSecret);
RbApplication_2.SetHostInfo(credentials.ServerConfig.HostName);

// Try to log using login_1
ConsoleAbstraction.WriteWhite($"{CR}Starting login with {credentials.UsersConfig[0].Login}...");

var sdkResult = await RbApplication_1.LoginAsync(credentials.UsersConfig[0].Login, credentials.UsersConfig[0].Password);
if (sdkResult.Success)
{
    // We are connected and SDK is ready
    ConsoleAbstraction.WriteGreen($"{CR}Connected to Rainbow Server using {credentials.UsersConfig[0].Login}");
}
else
{
    // We are not connected - Display why:
    ConsoleAbstraction.WriteRed($"{CR}Connection failed using {credentials.UsersConfig[0].Login} - SdkError:{sdkResult.Result}");
}

// Try to log using login_2
ConsoleAbstraction.WriteWhite($"{CR}Starting login with {credentials.UsersConfig[1].Login}...");

sdkResult = await RbApplication_2.LoginAsync(credentials.UsersConfig[1].Login, credentials.UsersConfig[1].Password);
if (sdkResult.Success)
{
    // We are connected and SDK is ready
    ConsoleAbstraction.WriteGreen($"{CR}Connected to Rainbow Server using {credentials.UsersConfig[1].Login}");
}
else
{
    // We are not connected - Display why:
    ConsoleAbstraction.WriteRed($"{CR}Connection failed using {credentials.UsersConfig[1].Login} - SdkError:{sdkResult.Result}");
}


ConsoleAbstraction.WriteBlue($"{CR}Use [ESC] at anytime to quit");

ConsoleAbstraction.WriteBlue($"{CR}Try also this example with incorrect appId / appSecretKey / hostName AND/OR incorrect login / password");

ConsoleAbstraction.WriteBlue($"{CR}Nothing more is expected - You can quit");
do
{
    await Task.Delay(200);
    CheckInputKey();
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