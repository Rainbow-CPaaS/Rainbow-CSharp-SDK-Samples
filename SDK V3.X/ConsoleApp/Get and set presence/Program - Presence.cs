
using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;
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

int presenceCounter = 0; // Use to know which presence to set (one by one)
int maxPresenceCounter = 10; // Use to know when to reset presence counter

// Check if information are set
if (String.IsNullOrEmpty(appId)
    || String.IsNullOrEmpty(appSecretKey)
    || String.IsNullOrEmpty(hostName)
    || String.IsNullOrEmpty(login)
    || String.IsNullOrEmpty(password))
{
    WriteRed("Ensure to set appId / appSecretKey / hostName and login / password");
    System.Environment.Exit(0);
}

Rainbow.Util.SetLogAnonymously(false);

// Add default logger
NLogConfigurator.AddLogger();

// Create Rainbow SDK objects
var RbApplication = new Application();
var RbAutoReconnection = RbApplication.GetAutoReconnection();
var RbContacts = RbApplication.GetContacts();

// We want to receive events from SDK
RbApplication.AuthenticationFailed += RbApplication_AuthenticationFailed;       // Triggered when the authentication process will fail
RbApplication.AuthenticationSucceeded += RbApplication_AuthenticationSucceeded; // Triggered when the authentication process will succeed
RbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;   // Triggered when the Connection State will change

RbAutoReconnection.Cancelled += RbAutoReconnection_Cancelled;                       // Triggered when AutoReonnection service is cancelled
RbAutoReconnection.Started += RbAutoReconnection_Started;                           // Triggered when AutoReonnection service is started
RbAutoReconnection.MaxNbAttemptsReached += RbAutoReconnection_MaxNbAttemptsReached; // Triggered when AutoReonnection service reachde the mawimun number of attempts tryig to connect to the server
RbAutoReconnection.TokenExpired += RbAutoReconnection_TokenExpired;                 // Triggered when the Security Token is expired

RbContacts.ContactAggregatedPresenceUpdated += RbContacts_ContactAggregatedPresenceUpdated; // Triggered when aggregated/unified presence of a contact has changed
RbContacts.ContactPresenceUpdated += RbContacts_ContactPresenceUpdated;                     // Triggered when presence of a contact on of of this devices has changed

// Set global configuration info
RbApplication.SetApplicationInfo(appId, appSecretKey);
RbApplication.SetHostInfo(hostName);

// MaxNbAttempts: By default it's 50. We reduce here the number so it will take less time to reach the limit.
// The goal here is to understand how events are triggered by the AutoReconnection service
RbAutoReconnection.MaxNbAttempts = 10; 

WriteGreen($"{CR}Use [ESC] at anytime to quit");

WriteGreen($"{CR}Use [Q] at anytime once loggued to quit using Logout");
WriteGreen($"Use [A] at anytime once loggued to cancel/stop AutoReconnection service");

WriteGreen($"{CR}Use [P] at anytime once loggued to change your presence");

WriteGreen($"{CR}Using another account on Rainbow Web client and change his presence to see Presence events");

// Start login
WriteWhite($"{CR}Starting login ...");
var taskSdkResult = RbApplication.LoginAsync(login, password); // Here we don't wait the Login process before to continue

do
{
    await Task.Delay(200);
    await CheckInputKey();
} while (true);

async Task CheckInputKey()
{
    while (Console.KeyAvailable)
    {
        var userInput = Console.ReadKey(true);

        switch (userInput.Key)
        {
            case ConsoleKey.Escape:
                WriteYellow($"Asked to end process using [ESC] key");
                System.Environment.Exit(0);
                return;

            case ConsoleKey.Q:
                if(RbApplication.IsConnected())
                {
                    // We logout - here we don't wait the result
                    var taskSdkResult = await RbApplication.LogoutAsync();
                    if (taskSdkResult.Success)
                    {
                        // We do nothing special here - we manage event Cancelled from AutoReconnexion
                    }
                    else
                        WriteRed($"LogoutAsync - Error:[{taskSdkResult.Result}]");
                }
                return;


            case ConsoleKey.A:
                if (RbAutoReconnection.IsStarted)
                {
                    WriteRed($"{CR}AutoReconnection - Service cancelled/Stopped");
                    RbAutoReconnection.Cancel();
                }
                return;

            case ConsoleKey.P:
                await ChangePresence();
                return;
        }
    }
}

async Task ChangePresence()
{
    // presenceCounter - case 0 to 4: change presence to Away, Away / Inactive, Xa / Invisible, Dnd and Online
    // These case can be manually set using classic Rainbow Client

    // Other cases, cannot be set manually, but are calculated according the situation: in a call with Audio, video , etc ...
    // Using the SDK you can set them according your needs

    Presence newPresence = null;
    switch(presenceCounter)
    {
        case 0:
            // Create presence object to Away
            newPresence = RbContacts.CreatePresence(PresenceLevel.Away);
            break;

        case 1:
            // Create presence object to Away / Inactive
            newPresence = RbContacts.CreatePresence(PresenceLevel.Away, PresenceDetails.Inactive);
            break;

        case 2:
            // Create presence object to Xa / Invisible
            newPresence = RbContacts.CreatePresence(PresenceLevel.Xa);
            break;

        case 3:
            // Create presence object to Dnd
            newPresence = RbContacts.CreatePresence(PresenceLevel.Dnd);
            break;

        case 4:
            // Create presence object to Online
            newPresence = RbContacts.CreatePresence(PresenceLevel.Dnd);
            break;

        case 5:
            // Create presence object to Busy - Audio
            newPresence = RbContacts.CreatePresence(PresenceLevel.Busy, PresenceDetails.Audio);
            break;

        case 6:
            // Create presence object to Busy - Video
            newPresence = RbContacts.CreatePresence(PresenceLevel.Busy, PresenceDetails.Video);
            break;

        case 7:
            // Create presence object to Busy - Sharing
            newPresence = RbContacts.CreatePresence(PresenceLevel.Busy, PresenceDetails.Sharing);
            break;

        case 8:
            // Create presence object to Dnd - Teams
            newPresence = RbContacts.CreatePresence(PresenceLevel.Dnd, PresenceDetails.Teams);
            break;

        case 9:
            // Create presence object to Dnd - Presentation
            newPresence = RbContacts.CreatePresence(PresenceLevel.Dnd, PresenceDetails.Presentation);
            break;
    }

    if (newPresence != null)
    {
        // We now ask the server to change my presence so other contacts can see it
        var sdkResult = await RbContacts.SetPresenceLevelAsync(newPresence);
        if (!sdkResult.Success)
        {
            if (sdkResult.Result.IncorrectUseError != null)
            {
                // We try to set an invalid presence
                WriteRed($"SetPresenceLevel - SdkError:[{sdkResult.Result}]");
            }
            else
            {
                // Pb with the server
                WriteRed($"SetPresenceLevel - SdkError:[{sdkResult.Result}]");
            }
        }
    }

    // Increase counter and reset is if necessary
    presenceCounter++;
    if (presenceCounter == maxPresenceCounter)
        presenceCounter = 0;
}


#region Events received from the SDK
void RbApplication_ConnectionStateChanged(Rainbow.Model.ConnectionState connectionState)
{
    // Display the CurrentNbAttempts
    if (connectionState.Status == ConnectionStatus.Connecting)
        WriteYellow($"{CR}AutoReconnection.CurrentNbAttempts: [{RbAutoReconnection.CurrentNbAttempts}]");

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

    // If we are disconnected and the AutoReconnection is stopped, nothing more wille happpen
    // So we quit the process
    if ((connectionState.Status == ConnectionStatus.Disconnected) && (!RbAutoReconnection.IsStarted))
    {
        WriteYellow($"{CR}We quit the process since AutoReconnection is stopped and we are disconnected");
        System.Environment.Exit(0);
    }
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

void RbAutoReconnection_TokenExpired()
{
    WriteRed($"{CR}Event AutoReconnection.TokenExpired triggered");
}

void RbAutoReconnection_MaxNbAttemptsReached()
{
    WriteRed($"{CR}Event AutoReconnection.MaxNbAttemptsReached triggered");
}

void RbAutoReconnection_Started()
{
    WriteBlue($"{CR}Event AutoReconnection.Started triggered");
}

void RbAutoReconnection_Cancelled(SdkError sdkError)
{
    if (sdkError.Type == Rainbow.Enums.SdkErrorType.NoError)
    {
        // The service has been cancelled/stopped voluntarily
        WriteYellow($"{CR}Event AutoReconnection.Cancelled triggered - Done using the SDK voluntarily");
    }
    else 
    {
        // The service has been cancelled/stopped involuntarily - display the reason
        WriteBlue($"{CR}Event AutoReconnection.Cancelled triggered - SdkError(Exception]:[{sdkError}]");
    }

    WriteWhite($"{CR}We quit since the AutoReconnection has been Cancelled");
    System.Environment.Exit(0);
}

void RbContacts_ContactPresenceUpdated(Rainbow.Model.Presence presence)
{
    // Presence of a Contact on one of his device has changed
    WriteYellow($"{CR}Event RbContacts_ContactPresenceUpdated triggered - Contact:[{presence.Contact.ToString(DetailsLevel.Small)}] - Presence:[{presence.ToString(DetailsLevel.Medium)}]");
}

void RbContacts_ContactAggregatedPresenceUpdated(Rainbow.Model.Presence presence)
{
    // Aggregated / Unified presence of a Contact has changed
    WriteBlue($"{CR}Event RbContacts_ContactAggregatedPresenceUpdated triggered - Contact:[{presence.Contact.ToString(DetailsLevel.Small)}] - Presence:[{presence.ToString(DetailsLevel.Small)}]");
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