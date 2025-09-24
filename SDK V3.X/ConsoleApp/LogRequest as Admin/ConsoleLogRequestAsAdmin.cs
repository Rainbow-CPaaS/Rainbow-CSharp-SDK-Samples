
using Rainbow;
using System.Text;

using Rainbow.Example.Common;
using Util = Rainbow.Example.Common.Util;
using Rainbow.SimpleJSON;
using Rainbow.Model;

// --------------------------------------------------

ExeSettings? exeSettings = null;
Credentials? credentials = null;

if ((!ReadExeSettings()) || (exeSettings is null))
    return;

if ((!ReadCredentials(credentialsCanBeEmpty: false)) || (credentials is null))
    return;

// --------------------------------------------------

Console.OutputEncoding = Encoding.UTF8; // We want to display UTF8 on the console
String CR = Rainbow.Util.CR; // Get carriage return;

// In "exeSettings.json" using "logFolderPath" property, we defined a folder where the logs must be stored
String logFolderPath = exeSettings.LogFolderPath;

// In "credentials.json" using "userConfig" object, we defined a prefix used as logger prefix (this prefix permits to have logs stored in specific file for this "userConfig")
String logPrefix = credentials.UsersConfig[0].Prefix;

// Using NLogConfigurator, we specify the folder where log will be stored
NLogConfigurator.Directory = logFolderPath;

// Using NLogConfigurator, we add a logger using the prefix
NLogConfigurator.AddLogger(logPrefix);

Rainbow.Util.SetLogAnonymously(false);

// Create Rainbow SDK objects
var RbApplication = new Application(iniFolderFullPathName: logFolderPath, loggerPrefix: logPrefix);
var RbContacts = RbApplication.GetContacts();
var RbCustomerCare = RbApplication.GetCustomerCare();

Dictionary<String, String> logRequestsStorage = []; // Key: LogRequestId / Value: ContactId

// Set global configuration info
RbApplication.SetApplicationInfo(credentials.ServerConfig.AppId, credentials.ServerConfig.AppSecret);
RbApplication.SetHostInfo(credentials.ServerConfig.HostName);

RbApplication.Restrictions.LogRestRequest = true;

// Set event we want to manage
RbCustomerCare.LogRequestAccepted += RbCustomerCare_LogRequestAccepted;
RbCustomerCare.LogRequestRejected += RbCustomerCare_LogRequestRejected;

Util.WriteGreen($"Starting login using credentials stored in file [.\\config\\credentials.json]");
var sdkResult = await RbApplication.LoginAsync(credentials.UsersConfig[0].Login, credentials.UsersConfig[0].Password);

if (!sdkResult.Success)
{
    Util.WriteRed($"{CR}Cannot loggued - Error:[{sdkResult.Result}]");

    Util.WriteWhite($"{CR}Press any key to quit");
    var userInput = Console.ReadKey(true);
    return;
}

var currentContact = RbContacts.GetCurrentContact();

Util.WriteWhite($"{CR}Account is now loggued:");
Util.WriteWhite($"\t- login:[{currentContact.LoginEmail}]");
Util.WriteWhite($"\t- id:[{currentContact.Peer.Id}]");
Util.WriteWhite($"\t- resource:[{RbApplication.GetResource()}]");

DisplayInfoMenu();

void DisplayInfoMenu()
{

    Util.WriteBlue($"{CR}[ESC] to quit");
    Util.WriteBlue($"[L] to ask Log Request");
}

async Task AskLogRequestAsync()
{
    Util.WriteBlue($"{CR}Ask log request:");
    Util.WriteBlue($"Enter Contact Id:");
    var contactId = Console.ReadLine();
    contactId ??= "";

    Util.WriteBlue($"Enter resource used by the contact:");
    var resource = Console.ReadLine();

    Util.WriteBlue($"Enter description to provide for the log request:");
    var description = Console.ReadLine();

    Util.WriteGreen($"{CR}Asking log request ...");
    var sdkResultLogRequest = await RbCustomerCare.AskLogRequestForUserAsync(Peer.FromContactId(contactId), resource, description);
    if(sdkResultLogRequest.Success)
    {
        var logRequestId = sdkResultLogRequest.Data.Id;
        logRequestsStorage[logRequestId] = contactId;
        Util.WriteWhite($"{CR}Log request asked successfully - logRequestId:[{logRequestId}]");
    }
    else
    {
        Util.WriteRed($"{CR}Cannot ask log request - Error:[{sdkResultLogRequest.Result}]");
    }

    DisplayInfoMenu();
}

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
                System.Environment.Exit(0);
                return;

            case ConsoleKey.L:
                var _ = AskLogRequestAsync();
                return;
        }
    }
}

#region EVENTS TRIGGERED BY THE SDK

void RbCustomerCare_LogRequestRejected(string logRequestId)
{
    Util.WriteRed($"{CR}A Log request has been rejected by the user - logRequestId:[{logRequestId}]");
}

void RbCustomerCare_LogRequestAccepted(string logRequestId)
{
    Util.WriteGreen($"{CR}A Log request has been accepted by the user - logRequestId:[{logRequestId}]");

    Action action = async () =>
    {
        if (logRequestsStorage.TryGetValue(logRequestId, out var contactId))
        {
            Util.WriteGreen($"{CR}Asking log request details...");
            var sdkResult = await RbCustomerCare.GetLogRequestAsync(Peer.FromContactId(contactId), logRequestId);
            if(sdkResult.Success)
            {
                var logRequest = sdkResult.Data;
                Util.WriteBlue($"{CR}Log request details:{logRequest.ToString(Rainbow.Consts.DetailsLevel.Full)}");
            }
            else
            {
                Util.WriteRed($"{CR}Cannot get log request details - Error:[{sdkResult.Result}]");
            }
        }
    };

    Task.Run(action);
}

#endregion EVENTS TRIGGERED BY THE SDK

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

Boolean ReadCredentials(string fileName = "credentials.json", Boolean credentialsCanBeEmpty = false)
{
    var credentialsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}{fileName}";
    if (!File.Exists(credentialsFilePath))
    {
        Util.WriteRed($"The file '{credentialsFilePath}' has not been found.");
        return false;
    }

    String jsonConfig = File.ReadAllText(credentialsFilePath);
    var jsonNode = JSON.Parse(jsonConfig);

    if (!Credentials.FromJsonNode(jsonNode["credentials"], out credentials, credentialsCanBeEmpty))
    {
        Util.WriteRed($"Cannot read 'credentials' object OR invalid/missing data in file:[{fileName}].");
        return false;
    }

    return true;
}
