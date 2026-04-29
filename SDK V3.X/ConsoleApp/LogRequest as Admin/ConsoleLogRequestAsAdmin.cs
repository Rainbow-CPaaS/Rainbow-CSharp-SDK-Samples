
using Rainbow;
using Rainbow.Enums;
using Rainbow.Example.Common;
using Rainbow.Model;
using Rainbow.SimpleJSON;
using System.Text;


// --------------------------------------------------

ExeSettings? exeSettings = null;
Credentials? credentials = null;

if ((!ReadExeSettings()) || (exeSettings is null))
    return;

if ((!ReadCredentials(credentialsCanBeEmpty: false)) || (credentials is null))
    return;

// --------------------------------------------------

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

// Set restrictions
Restrictions restrictions = new(true)
{
    LogRestRequest = true,
    LogEvent = true,
    LogEventParameters = true,
    LogEventRaised = true,
};

// Create Rainbow SDK objects
var RbApplication = new Application(iniFolderFullPathName: logFolderPath, loggerPrefix: logPrefix, restrictions: restrictions);
var RbContacts = RbApplication.GetContacts();
var RbCustomerCare = RbApplication.GetCustomerCare();

Dictionary<String, String> logRequestsStorage = []; // Key: LogRequestId / Value: ContactId

// Set global configuration info
RbApplication.SetApplicationInfo(credentials.ServerConfig.AppId, credentials.ServerConfig.AppSecret);
RbApplication.SetHostInfo(credentials.ServerConfig.HostName);

// Set event we want to manage
RbCustomerCare.LogRequestAccepted += RbCustomerCare_LogRequestAccepted;
RbCustomerCare.LogRequestRejected += RbCustomerCare_LogRequestRejected;

ConsoleAbstraction.WriteGreen($"Starting login using credentials stored in file [.\\config\\credentials.json]");
var sdkResult = await RbApplication.LoginAsync(credentials.UsersConfig[0].Login, credentials.UsersConfig[0].Password);

if (!sdkResult.Success)
{
    ConsoleAbstraction.WriteRed($"{CR}Cannot loggued - Error:[{sdkResult.Result}]");

    ConsoleAbstraction.WriteWhite($"{CR}Press any key to quit");
    var userInput = ConsoleAbstraction.ReadKey();
    return;
}

var currentContact = RbContacts.GetCurrentContact();

ConsoleAbstraction.WriteWhite($"{CR}Account is now loggued:");
ConsoleAbstraction.WriteWhite($"\t- login:[{currentContact.LoginEmail}]");
ConsoleAbstraction.WriteWhite($"\t- id:[{currentContact.Peer.Id}]");
ConsoleAbstraction.WriteWhite($"\t- resource:[{RbApplication.GetResource()}]");

DisplayInfoMenu();

void DisplayInfoMenu()
{

    ConsoleAbstraction.WriteBlue($"{CR}[ESC] to quit");
    ConsoleAbstraction.WriteBlue($"[L] to ask Log Request");
}

async Task AskLogRequestAsync()
{
    ConsoleAbstraction.WriteBlue($"{CR}Ask log request:");
    ConsoleAbstraction.WriteBlue($"Enter Contact Id:");
    var contactId = ConsoleAbstraction.ReadLine();
    contactId ??= "";

    ConsoleAbstraction.WriteBlue($"Enter resource used by the contact:");
    var resource = ConsoleAbstraction.ReadLine();

    ConsoleAbstraction.WriteBlue($"Enter description to provide for the log request:");
    var description = ConsoleAbstraction.ReadLine();

    ConsoleAbstraction.WriteGreen($"{CR}Asking log request ...");
    var sdkResultLogRequest = await RbCustomerCare.AskLogRequestForUserAsync(Peer.FromContactId(contactId), resource, description);
    if(sdkResultLogRequest.Success)
    {
        var logRequestId = sdkResultLogRequest.Data.Id;
        logRequestsStorage[logRequestId] = contactId;
        ConsoleAbstraction.WriteWhite($"{CR}Log request asked successfully - logRequestId:[{logRequestId}]");
    }
    else
    {
        ConsoleAbstraction.WriteRed($"{CR}Cannot ask log request - Error:[{sdkResultLogRequest.Result}]");
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
    while (ConsoleAbstraction.KeyAvailable)
    {
        var userInput = ConsoleAbstraction.ReadKey();

        switch (userInput?.Key)
        {
            case ConsoleKey.Escape:
                ConsoleAbstraction.WriteYellow($"Asked to end process using [ESC] key");
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
    ConsoleAbstraction.WriteRed($"{CR}A Log request has been rejected by the user - logRequestId:[{logRequestId}]");
}

void RbCustomerCare_LogRequestAccepted(string logRequestId)
{
    ConsoleAbstraction.WriteGreen($"{CR}A Log request has been accepted by the user - logRequestId:[{logRequestId}]");

    Action action = async () =>
    {
        if (logRequestsStorage.TryGetValue(logRequestId, out var contactId))
        {
            ConsoleAbstraction.WriteGreen($"{CR}Asking log request details...");
            var sdkResult = await RbCustomerCare.GetLogRequestAsync(Peer.FromContactId(contactId), logRequestId);
            if(sdkResult.Success)
            {
                var logRequest = sdkResult.Data;
                ConsoleAbstraction.WriteBlue($"{CR}Log request details:{logRequest.ToString(Rainbow.Consts.DetailsLevel.Full)}");
            }
            else
            {
                ConsoleAbstraction.WriteRed($"{CR}Cannot get log request details - Error:[{sdkResult.Result}]");
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

Boolean ReadCredentials(string fileName = "credentials.json", Boolean credentialsCanBeEmpty = false)
{
    var credentialsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}{fileName}";
    if (!File.Exists(credentialsFilePath))
    {
        ConsoleAbstraction.WriteRed($"The file '{credentialsFilePath}' has not been found.");
        return false;
    }

    String jsonConfig = File.ReadAllText(credentialsFilePath);
    var jsonNode = JSON.Parse(jsonConfig);

    var credentials = Credentials.FromJsonNode(jsonNode["credentials"]);
    if (credentials?.IsValid() != true)
    {
        ConsoleAbstraction.WriteRed($"Cannot read 'credentials' object OR invalid/missing data in file:[{fileName}].");
        return false;
    }

    return true;
}
