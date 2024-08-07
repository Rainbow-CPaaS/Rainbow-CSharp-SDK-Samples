using Microsoft.Extensions.Logging;
using Rainbow;
using System.Text;

//// --------------------------------------------------

////  /!\ Define you Rainbow settings below:

String appId = ""; // To set according your settings
String appSecretKey = ""; // To set according your settings
String hostName = ""; // To set according your settings

// We define the first account
String login_1 = ""; // To set according your settings
String password_1 = ""; // To set according your settings

// We define the second account
String login_2 = ""; // To set according your settings
String password_2 = ""; // To set according your settings

//// --------------------------------------------------

Console.OutputEncoding = Encoding.UTF8; // We want to display UTF8 on the console
Boolean canUseEmoji = true;             // Set to false if Emoji cannot be displayed in your console

Object consoleLockObject = new(); // To lock until the current console display is performed
String CR = Rainbow.Util.CR; // Get carriage return;

// Check if information are set
if (String.IsNullOrEmpty(appId)
    || String.IsNullOrEmpty(appSecretKey)
    || String.IsNullOrEmpty(hostName)
    || String.IsNullOrEmpty(login_1)
    || String.IsNullOrEmpty(password_1)
    || String.IsNullOrEmpty(login_2)
    || String.IsNullOrEmpty(password_2))
{
    WriteRed("Ensure to set appId / appSecretKey / hostName and login / password");
    Environment.Exit(0);
}

// We want to log files from SDK
// But also from this example

// Each time you create a Rainbow.Application a logger prefix is used

// By default the prefix is null.
// Since we have two accounts, we create 2 loggers with diffrent prefix
String prefix_1 = "BOT1_";
String prefix_2 = "BOT2_";
NLogConfigurator.AddLogger(prefix_1);
NLogConfigurator.AddLogger(prefix_2);

/// This logger use NLog but you can use any logger based on Microsoft Extension Logging (MEL)
/// See https://developers.openrainbow.com/doc/sdk/csharp/core/lts/guides/130_application_logging for more details
/// By default, two log files are created "RainbowSdk.log" and "RainbowSdk_WebRTC.log" (if WebRTC is used) in the foder where this process is running
/// See more details in <see cref="NLogConfigurator"/> object.


/// Create Rainbow SDK objects

// We need to ahe IniFileName different for each Rainbow Application
var iniFileName_1 = prefix_1 + "file.ini";
var iniFileName_2 = prefix_2 + "file.ini";

var iniFolderPath = ".\\"; // We will store iniFile name in current folder

var RbApplication_1 = new Application(iniFolderFullPathName: iniFolderPath, iniFileName: iniFileName_1, loggerPrefix: prefix_1); // we create the first Rainbow Applicaiotn with the first prefix
var RbApplication_2 = new Application(iniFolderFullPathName: iniFolderPath, iniFileName: iniFileName_2, loggerPrefix: prefix_2); // we create the second Rainbow Applicaiotn with the second prefix

WriteYellow($"{CR}Log files of the Rainbow SDK have been created");
WriteYellow($"For {login_1}:");
WriteYellow($"\tLogFile name is [{prefix_1}RainbowSdk.log] and stored in folder:[{Path.GetFullPath(".\\")}]");
WriteYellow($"\tIniFile name is [{iniFileName_1}] and stored in folder:[{Path.GetFullPath(iniFolderPath)}]");

WriteYellow($"{CR}\tFor {login_2}:");
WriteYellow($"\tLogFile name is [{prefix_2}RainbowSdk.log] and stored in folder:[{Path.GetFullPath(".\\")}]");
WriteYellow($"\tIniFile name is [{iniFileName_2}] and stored in folder:[{Path.GetFullPath(iniFolderPath)}]");

/// You can also add log entries in a different for log entries created direclty in this consoleApp

/// We create a ILogger from the LogFactory using "ConsoleApp" as CategoryName
var prefix_consoleApp = "ConsoleApp";

NLogConfigurator.AddLogger(prefix_consoleApp);
ILogger log = Rainbow.LogFactory.CreateLogger(prefix_consoleApp);

/// And to log info use these
log.LogDebug("Add log as Debug from my console app");
log.LogInformation("Add log as Information from my console app");
log.LogWarning("Add log as Warning from my console app");
log.LogError("Add log as Error from my console app");
log.LogCritical("Add log as Critical from my console app");

WriteYellow($"{CR}We have added several log entries from this console application");
WriteYellow($"\tLogFile name is [{prefix_consoleApp}RainbowSdk.log] and stored in folder:[{Path.GetFullPath(".\\")}]");

// Set global configuration info
RbApplication_1.SetApplicationInfo(appId, appSecretKey);
RbApplication_1.SetHostInfo(hostName);

RbApplication_2.SetApplicationInfo(appId, appSecretKey);
RbApplication_2.SetHostInfo(hostName);

// Try to log using login_1
WriteWhite($"{CR}Starting login with {login_1}...");

var sdkResult = await RbApplication_1.LoginAsync(login_1, password_1);
if (sdkResult.Success)
{
    // We are connected and SDK is ready
    WriteGreen($"{CR}Connected to Rainbow Server using {login_1}");
}
else
{
    // We are not connected - Display why:
    WriteRed($"{CR}Connection failed using {login_1} - SdkError:{sdkResult.Result}");
}

// Try to log using login_2
WriteWhite($"{CR}Starting login with {login_2}...");

sdkResult = await RbApplication_2.LoginAsync(login_2, password_2);
if (sdkResult.Success)
{
    // We are connected and SDK is ready
    WriteGreen($"{CR}Connected to Rainbow Server using {login_2}");
}
else
{
    // We are not connected - Display why:
    WriteRed($"{CR}Connection failed using {login_2} - SdkError:{sdkResult.Result}");
}


WriteBlue($"{CR}Use [ESC] at anytime to quit");

WriteBlue($"{CR}Try also this example with incorrect appId / appSecretKey / hostName AND/OR incorrect login / password");

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
                WriteYellow($"Asked to end process using [ESC] key");
                Environment.Exit(0);
                return;
        }
    }
}


#region UTILITY METHODS

void WriteYellow(String message)
{
    WriteToConsole(message, ConsoleColor.Yellow);
}

void WriteWhite(String message)
{
    WriteToConsole(message, ConsoleColor.White);
}

void WriteRed(String message)
{
    WriteToConsole(message, ConsoleColor.Red);
}

void WriteGreen(String message)
{
    WriteToConsole(message, ConsoleColor.Green);
}

void WriteBlue(String message)
{
    WriteToConsole(message, ConsoleColor.Blue);
}

void WriteToConsole(String message, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null)
{
    if (String.IsNullOrEmpty(message))
        return;

    lock (consoleLockObject)
    {
        if (foregroundColor != null)
            Console.ForegroundColor = foregroundColor.Value;

        if (backgroundColor != null)
            Console.BackgroundColor = backgroundColor.Value;

        Console.WriteLine(message);
        Console.ResetColor();
    }
}
#endregion UTILITY METHODS
