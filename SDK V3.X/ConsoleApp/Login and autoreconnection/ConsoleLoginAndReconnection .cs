
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
var RbApplication = new Application(iniFolderFullPathName: logFolderPath, iniFileName: logPrefix +".ini", loggerPrefix: logPrefix);
var RbAutoReconnection = RbApplication.GetAutoReconnection();
var RbBubbles = RbApplication.GetBubbles();

// We want to receive events from SDK
RbApplication.AuthenticationFailed += RbApplication_AuthenticationFailed;           // Triggered when the authentication process will fail
RbApplication.AuthenticationSucceeded += RbApplication_AuthenticationSucceeded;     // Triggered when the authentication process will succeed
RbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;       // Triggered when the Connection State will change

RbAutoReconnection.Cancelled += RbAutoReconnection_Cancelled;                       // Triggered when AutoReonnection service is cancelled
RbAutoReconnection.Started += RbAutoReconnection_Started;                           // Triggered when AutoReonnection service is started
RbAutoReconnection.MaxNbAttemptsReached += RbAutoReconnection_MaxNbAttemptsReached; // Triggered when AutoReonnection service reachde the mawimun number of attempts tryig to connect to the server
RbAutoReconnection.TokenExpired += RbAutoReconnection_TokenExpired;                 // Triggered when the Security Token is expired

RbBubbles.BubbleAffiliationsPerformed += RbBubbles_BubbleAffiliationsPerformed;     // Triggered when all bubbles have been affiliated

// Set global configuration info
RbApplication.SetApplicationInfo(credentials.ServerConfig.AppId, credentials.ServerConfig.AppSecret);
RbApplication.SetHostInfo(credentials.ServerConfig.HostName);


// MaxNbAttempts: By default it's 50. We reduce here the number so it will take less time to reach the limit.
// The goal here is to understand how events are triggered by the AutoReconnection service
RbAutoReconnection.MaxNbAttempts = 10; 

Util.WriteGreen($"{CR}Use [ESC] at anytime to quit");

Util.WriteGreen($"{CR}Use [Q] at anytime once loggued to quit using Logout");
Util.WriteGreen($"Use [A] at anytime once loggued to cancel/stop AutoReconnection service");

Util.WriteBlue($"{CR}You can change your network connection settgins while the process is running to see how auto-reconnection is working");

// Start login
Util.WriteWhite($"{CR}Starting login ...");
var _ = RbApplication.LoginAsync(credentials.UsersConfig[0].Login, credentials.UsersConfig[0].Password);
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
                Util.WriteYellow($"Asked to end process using [ESC] key");
                Environment.Exit(0);
                return;

            case ConsoleKey.Q:
                if(RbApplication.IsConnected())
                {
                    // We logout - here we don't wait the result
                    var taskSdkResult = await RbApplication.LogoutAsync();
                    if (taskSdkResult.Success)
                    {
                        // Nothing to do here - on success, event Cancelled from AutoReconnexion is triggered
                    }
                    else
                        Util.WriteRed($"LogoutAsync - Error:[{taskSdkResult.Result}]");
                }
                return;


            case ConsoleKey.A:
                if (RbAutoReconnection.IsStarted)
                {
                    Util.WriteRed($"{CR}AutoReconnection - Service cancelled/Stopped");
                    RbAutoReconnection.Cancel();
                }
                return;
        }
    }
}

#region Events received from the SDK
void RbApplication_ConnectionStateChanged(Rainbow.Model.ConnectionState connectionState)
{
    // Display the CurrentNbAttempts
    if (connectionState.Status == ConnectionStatus.Connecting)
        Util.WriteYellow($"{CR}AutoReconnection.CurrentNbAttempts: [{RbAutoReconnection.CurrentNbAttempts}]");

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

    // If we are disconnected and the AutoReconnection is stopped, nothing more wille happpen
    // So we quit the process
    if ((connectionState.Status == ConnectionStatus.Disconnected) && (!RbAutoReconnection.IsStarted))
    {
        Util.WriteYellow($"{CR}We quit the process since AutoReconnection is stopped and we are disconnected");
        Environment.Exit(0);
    }
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

void RbBubbles_BubbleAffiliationsPerformed()
{
    // Bubble Affiliations Performed - we display in the console the info
    Util.WriteBlue($"{CR}Event Bubbles_BubbleAffiliationsPerformed triggered");

}

void RbAutoReconnection_TokenExpired()
{
    Util.WriteRed($"{CR}Event AutoReconnection.TokenExpired triggered");
}

void RbAutoReconnection_MaxNbAttemptsReached()
{
    Util.WriteRed($"{CR}Event AutoReconnection.MaxNbAttemptsReached triggered");
}

void RbAutoReconnection_Started()
{
    Util.WriteBlue($"{CR}Event AutoReconnection.Started triggered");
}

void RbAutoReconnection_Cancelled(SdkError sdkError)
{
    if (sdkError.Type == Rainbow.Enums.SdkErrorType.NoError)
    {
        // The service has been cancelled/stopped voluntarily
        Util.WriteYellow($"{CR}Event AutoReconnection.Cancelled triggered - Done using the SDK voluntarily");
    }
    else 
    {
        // The service has been cancelled/stopped involuntarily - display the reason
        Util.WriteBlue($"{CR}Event AutoReconnection.Cancelled triggered - SdkError(Exception]:[{sdkError}]");
    }

    Util.WriteWhite($"{CR}We quit since the AutoReconnection has been Cancelled");
    Environment.Exit(0);
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

