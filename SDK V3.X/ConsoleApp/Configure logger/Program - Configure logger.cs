
using Microsoft.Extensions.Logging;
using Rainbow;
using System.Text;

// --------------------------------------------------

//  /!\ Define you Rainbow settings below:

String appId = ""; // To set according your settings
String appSecretKey = ""; // To set according your settings
String hostName = ""; // To set according your settings

String login = ""; // To set according your settings
String password = ""; // To set according your settings

// --------------------------------------------------

Console.OutputEncoding = Encoding.UTF8; // We want to display UTF8 on the console
Boolean canUseEmoji = true;             // Set to false if Emoji cannot be displayed in your console

Object consoleLockObject = new(); // To lock until the current console display is performed
String CR = Rainbow.Util.CR; // Get carriage return;

// Check if information are set
if (String.IsNullOrEmpty(appId)
    || String.IsNullOrEmpty(appSecretKey)
    || String.IsNullOrEmpty(hostName)
    || String.IsNullOrEmpty(login)
    || String.IsNullOrEmpty(password))
{
    WriteRed("Ensure to set appId / appSecretKey / hostName and login / password");
    Environment.Exit(0);
}

// We want to log files from SDK
// But also from this example

// Each time you create a Rainbow.Application a logger prefix is used
// By default the prefix is null. It's why here we create a logger with no prefix
NLogConfigurator.AddLogger();

/// This logger use NLog but you can use any logger based on Microsoft Extension Logging (MEL)
/// See https://developers.openrainbow.com/doc/sdk/csharp/core/lts/guides/130_application_logging for more details
/// By default, two log files are created "RainbowSdk.log" and "RainbowSdk_WebRTC.log" (if WebRTC is used) in the foder where this process is running
/// See more details in <see cref="NLogConfigurator"/> object.


/// Create Rainbow SDK objects
var RbApplication = new Application(); // Even if it's not specified a default loggerPrefix is used here

/// Since we have created a logger and a Rainbow.Application with the same logger prefix, the SDK will add log entries automatically.
WriteYellow($"{CR}Log files have been created");

/// You can also add log entries in same files if you wish.
/// We create a ILogger from the LogFactory using "ConsoleApp" as CategoryName
ILogger log = Rainbow.LogFactory.CreateLogger("ConsoleApp");

/// And to log info use these
log.LogDebug("Add log as Debug from my console app");
log.LogInformation("Add log as Information from my console app");
log.LogWarning("Add log as Warning from my console app");
log.LogError("Add log as Error from my console app");
log.LogCritical("Add log as Critical from my console app");

WriteYellow($"{CR}We have added several log entries from this console application");

// Set global configuration info
RbApplication.SetApplicationInfo(appId, appSecretKey);
RbApplication.SetHostInfo(hostName);

// Start login
WriteWhite($"{CR}Starting login ...");
var sdkResult = await RbApplication.LoginAsync(login, password);

if (sdkResult.Success)
{
    // We are connected and SDK is ready
    WriteGreen($"{CR}Connected to Rainbow Server");
}
else
{
    // We are not connected - Display why:
    WriteRed($"{CR}Connection failed - SdkError:{sdkResult.Result}");
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
