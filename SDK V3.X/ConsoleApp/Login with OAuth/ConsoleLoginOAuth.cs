
using Rainbow;
using Rainbow.Consts;

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

if ( (credentials.ServerConfig.OAuthPorts is null) 
    || (credentials.ServerConfig.OAuthPorts.Count == 0) )
{
    Util.WriteRed($"No OAuthPorts defined in credentials.json");
    return;
}

String CR = Rainbow.Util.CR; // Get carriage return;

Util.WriteGreen($"{CR}Application used: Id:[{credentials.ServerConfig.AppId}] - HostName:[{credentials.ServerConfig.HostName}]");
Util.WriteGreen($"{CR}OAuthPorts used: [{String.Join(", ", credentials.ServerConfig.OAuthPorts)}]");

// --------------------------------------------------

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
var RbContacts = RbApplication.GetContacts();

// We want to receive events from SDK
RbApplication.AuthenticationFailed += RbApplication_AuthenticationFailed;       // Triggered when the authentication process will fail
RbApplication.AuthenticationSucceeded += RbApplication_AuthenticationSucceeded; // Triggered when the authentication process will succeed
RbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;   // Triggered when the Connection State will change

RbAutoReconnection.Cancelled += RbAutoReconnection_Cancelled;                       // Triggered when AutoReonnection service is cancelled
RbAutoReconnection.Started += RbAutoReconnection_Started;                           // Triggered when AutoReonnection service is started
RbAutoReconnection.MaxNbAttemptsReached += RbAutoReconnection_MaxNbAttemptsReached; // Triggered when AutoReonnection service reachde the mawimun number of attempts tryig to connect to the server
RbAutoReconnection.TokenExpired += RbAutoReconnection_TokenExpired;                 // Triggered when the Security Token is expired

// Set global configuration info
RbApplication.Restrictions.LogRestRequest = true;
RbApplication.SetApplicationInfo(credentials.ServerConfig.AppId, credentials.ServerConfig.AppSecret);
RbApplication.SetHostInfo(credentials.ServerConfig.HostName);


// MaxNbAttempts: By default it's 50. We reduce here the number so it will take less time to reach the limit.
// The goal here is to understand how events are triggered by the AutoReconnection service
RbAutoReconnection.MaxNbAttempts = 10;

// ------------------------------------

// Try to create a small local web server using one the port specified
List<int> ports = credentials.ServerConfig.OAuthPorts;
SystemBrowser? systemBrowser = null;
try
{
    systemBrowser = new(ports);
}
catch (Exception exc)
{
    Util.WriteRed($"Cannot create systemBrowser - Exception:[{exc}]");
    return;
}

if(systemBrowser is null)
{
    Util.WriteRed($"Cannot start local HTTP server using ports:[{String.Join(", ", ports)}]");
    return;
}

String redirectUri = $"http://127.0.0.1:{systemBrowser.Port}";

string oAuthAuthorizationUrl = RbApplication.GetOAuthAuthorizationEndPoint(true, redirectUri);
Util.WriteGreen($"{CR}OAuthAuthorizationUrl used: [{oAuthAuthorizationUrl}]");

CancellationToken cancellationToken = default; // We don't use here an cancellationToken
var browserResult = await systemBrowser.InvokeAsync(oAuthAuthorizationUrl, cancellationToken);
if (browserResult.IsError)
{
    Util.WriteRed($"SSO Failed");
}
else
{
    if (browserResult.Parameters.ContainsKey("code"))
    {
        string code = browserResult.Parameters["code"];
        Util.WriteBlue($"SSO done successfully{CR}\tAuthorization Code:{code}");

        if (!String.IsNullOrEmpty(code))
        {
            var sdkResult = await RbApplication.LoginWithOauthAuthorizationCodeAsync(code, redirectUri);
            if(sdkResult.Success)
            {
                Util.WriteBlue($"Login to RB server done successfully");
            }
            else
            {
                Util.WriteRed($"LoginWithOauthAuthorizationCodeAsync - Error:[{sdkResult.Result}]");
                return;
            }
        }
    }
    else
    {
        String msg = "";
        foreach (var key in browserResult.Parameters.Keys)
            msg += $"\t{key}:{browserResult.Parameters[key]}";

        Util.WriteRed($"Authentication failed:{CR}{msg}");

        // We wait here a little to ensure to display info to the browser
        await Task.Delay(500);
        return;
    }
}


// ------------------------------------

Util.WriteGreen($"{CR}Use [ESC] at anytime to quit");

Util.WriteGreen($"{CR}Use [Q] at anytime once loggued to quit using Logout");
Util.WriteGreen($"Use [A] at anytime once loggued to cancel/stop AutoReconnection service");

Util.WriteBlue($"{CR}You can change your network connection settgins while the process is running to see how auto-reconnection is working");

// Start login
Util.WriteWhite($"{CR}Starting login ...");

var canContinue = true;

do
{
    await Task.Delay(200);
    await CheckInputKey();
} while (canContinue);

await Task.Delay(5000);

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

            case ConsoleKey.I:
                var sdkResultContact =  await RbContacts.GetContactByIdAsync(RbContacts.GetCurrentContact().Peer.Id);
                if(sdkResultContact.Success)
                    Util.WriteRed($"{CR}GetContactByIdAsync done.");
                else
                    Util.WriteRed($"{CR}GetContactByIdAsync - Error:[{sdkResultContact.Result}]");
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
    Util.WriteToConsole($"{CR}Event Application.ConnectionStateChanged triggered - Connection Status: [{connectionState.Status}]", color);

    // If we are disconnected and the AutoReconnection is stopped, nothing more wille happpen
    // So we quit the process
    if ((connectionState.Status == ConnectionStatus.Disconnected) && (!RbAutoReconnection.IsStarted))
    {
        Util.WriteYellow($"{CR}We will quit the process since AutoReconnection is stopped and we are disconnected");
        canContinue = false;
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

