using Microsoft.Extensions.Logging;
using Rainbow;
using Rainbow.Consts;
using System.Text;

// --------------------------------------------------

//  /!\ Define you Rainbow settings below:

String appId = ""; // To set according your settings
String appSecretKey = ""; // To set according your settings
String hostName = ""; // To set according your settings

String login = ""; // To set according your settings
String password = ""; // To set according your settings

// --------------------------------------------------

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

// Add default logger
NLogConfigurator.AddLogger();

// Create Rainbow SDK objects
var RbApplication = new Application();

// We want to receive events from SDK
RbApplication.AuthenticationFailed += RbApplication_AuthenticationFailed;       // Triggered when the authentication process will fail
RbApplication.AuthenticationSucceeded += RbApplication_AuthenticationSucceeded; // Triggered when the authentication process will succeed
RbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;   // Triggered when the Connection State will change

// Set global configuration info
RbApplication.SetApplicationInfo(appId, appSecretKey);
RbApplication.SetHostInfo(hostName);

WriteBlue($"{CR}Use [ESC] at anytime to quit\r\n");

// Start login
WriteWhite("Starting login ...");
var taskSdkResult = RbApplication.LoginAsync(login, password); // Here we don't wait the Login process before to continue

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

#region Events received from the SDK
void RbApplication_ConnectionStateChanged(Rainbow.Model.ConnectionState connectionState)
{
    // We log connection state in the console - use differnte color according the status
    ConsoleColor color;
    switch (connectionState.Status)
    {
        case ConnectionStatus.Connected:
            color = ConsoleColor.Blue;
            break;

        case ConnectionStatus.Disconnected:
            color = ConsoleColor.Red;
            break;

        case ConnectionStatus.Connecting:
        default:
            color = ConsoleColor.Green;
            break;

    }
    WriteToConsole($"{CR}Event Application.ConnectionStateChanged triggered - Connection Status: [{connectionState.Status}]", color);
}

void RbApplication_AuthenticationSucceeded()
{
    // Authentication Succeeded- we display in the console the info
    WriteBlue($"{CR}Event Application.AuthenticationSucceeded triggered");
}

void RbApplication_AuthenticationFailed(SdkError sdkError)
{
    // Authentication failed - we display in the console the reason
    WriteRed($"{CR}Event Application.AuthenticationFailed triggered - SdkError:{sdkError}");
}
#endregion Events received from the SDK

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
