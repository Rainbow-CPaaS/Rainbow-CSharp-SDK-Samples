
using Rainbow;
using Rainbow.Consts;
using Rainbow.Console;
using Rainbow.SimpleJSON;
using Util = Rainbow.Console.Util;
using Rainbow.Model;

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
var RbApplication = new Rainbow.Application(iniFolderFullPathName: logFolderPath, iniFileName: logPrefix +".ini", loggerPrefix: logPrefix);
var RbAutoReconnection = RbApplication.GetAutoReconnection();
var RbContacts = RbApplication.GetContacts();
var RbAdministration = RbApplication.GetAdministration();

RbApplication.Restrictions.LogRestRequest = true;

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

// Start login
Util.WriteWhite($"{CR}Starting login ...");

// Contact / Company managed as Admin
Contact? userManaged = null;
Company? companyManaged = null;

var _ = RbApplication.LoginAsync(credentials.UsersConfig[0].Login, credentials.UsersConfig[0].Password);
do
{
    await Task.Delay(200);
    await CheckInputKey();
} while (true);

void MenuDisplayInfo()
{
    Util.WriteYellow("");
    Util.WriteYellow("[ESC] at anytime to quit");
    Util.WriteYellow("[I] (Info) Display this info");

    Util.WriteYellow("");
    Util.WriteYellow("[A] Use ApiKey as [A]dmin");
    Util.WriteYellow("[U] Use ApiKey as [U]ser (default)");

    Util.WriteYellow("");
    Util.WriteYellow("[L] [L]ist ApiKey");
    Util.WriteYellow("[C] [C]reate ApiKey");
}

async Task CheckInputKey()
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

            case ConsoleKey.A: // Use ApiKey as [A]dmin
                await MenuAdminApiKeyAsync();
                MenuDisplayInfo();
                return;

            case ConsoleKey.U: // Use ApiKey as [U]ser
                await MenuUserApiKeyAsync();
                MenuDisplayInfo();
                return;

            case ConsoleKey.L:
                await MenuListApiKeyAsync();
                MenuDisplayInfo();
                return;

            case ConsoleKey.C:
                await MenuCreateOrUpdateApiKeyAsync();
                MenuDisplayInfo();
                return;
        }
    }
}

async Task MenuListApiKeyAsync()
{
    Boolean canContinue = true;
    int offset = 0;
    int limit = 100;
    List<ApiKey> apiKeysList = [];

    ApiKey? apiKeyManaged = null;

    Util.WriteDarkYellow($"{CR}Asking list of ApiKeys{((userManaged is null) ? "" : $" for Contact:[{userManaged.ToString(DetailsLevel.Small)}]")}:");

    while (canContinue)
    {
        SdkResult<ApiKeyListData> sdkResult;

        if (userManaged is null)
            sdkResult = await RbApplication.GetApiKeysAsync(offset, limit);
        else
            sdkResult = await RbAdministration.GetApiKeysAsync(userManaged.Peer.Id, offset, limit);

        if (sdkResult.Success)
        {
            var listFound = sdkResult.Data.Data;
            apiKeysList.AddRange(listFound);
            canContinue = (listFound.Count == limit);
            offset += limit;
        }
        else
        {
            Util.WriteRed($"GetApiKeysAsync - Error:[{sdkResult.Result}]");
            return;
        }
    }

    if (apiKeysList.Count == 0)
    {
        Util.WriteGreen($"None ApiKeys found ...");
        return;
    }

    Util.WriteYellow($"{CR}List of ApiKeys - Total[{apiKeysList.Count}]):");
    foreach (var apiKey in apiKeysList)
    {
        Util.WriteYellow($"\t- {Rainbow.Util.LogOnOneLine(apiKey.ToString(DetailsLevel.Medium))}");
    }

    canContinue = true;
    while (canContinue)
    {
        Util.WriteYellow($"{CR}Enter Id to select an ApiKey - empty string to cancel");

        var str = Console.ReadLine();
        if (!String.IsNullOrEmpty(str))
        {
            var result = apiKeysList.Find(apiKey => apiKey.Id.Equals(str, StringComparison.InvariantCultureIgnoreCase));

            if (result is null)
                Util.WriteYellow($"No ApiKey found ...");
            else
            {
                apiKeyManaged = result;
                Util.WriteGreen($"ApiKey found and now managed: [{apiKeyManaged.ToString(DetailsLevel.Full)}]");

                Util.WriteYellow($"{CR}Do you want to ");
                Util.WriteRed($"\t [D]elete this ApiKey (can not be undone)");
                Util.WriteYellow($"\t [U]pdate it");
                var userInput = Console.ReadKey(true);
                if (userInput.Key == ConsoleKey.D)
                {
                    Util.WriteDarkYellow($"{CR}Deleting ApiKey ...");

                    SdkResult<Boolean> sdkResultBoolean;
                    if (userManaged is null)
                        sdkResultBoolean = await RbApplication.DeleteApiKeyAsync(apiKeyManaged);
                    else
                        sdkResultBoolean = await RbAdministration.DeleteApiKeyAsync(apiKeyManaged);

                    if (sdkResultBoolean.Success)
                        Util.WriteGreen($"ApiKey has been deleted");
                    else
                        Util.WriteRed($"DeleteApiKeyAsync - Error:[{sdkResultBoolean.Result}]");
                }
                else if (userInput.Key == ConsoleKey.U)
                    await MenuCreateOrUpdateApiKeyAsync(apiKeyManaged);
                else
                    Util.WriteYellow($"{CR}Bad key used ...");
            }
        }
        else
        {
            Util.WriteYellow($"Cancel search ...");
            canContinue = false;
        }
    }
}

async Task MenuCreateOrUpdateApiKeyAsync(ApiKey? apiKey = null)
{
    if(apiKey is null)
        Util.WriteGreen($"{CR}ApiKey can be created only for the current user:[{RbContacts.GetCurrentContact().ToString(DetailsLevel.Small)}]");
    else
    {
        var sdkResult = await RbApplication.GetApiKeyAsync(apiKey.Id);
        if (sdkResult.Success)
            apiKey = sdkResult.Data;
        else
        {
            Util.WriteRed($"{CR}GetApiKeyAsync - Error[{sdkResult.Result}]");
            return;
        }
    }

    Boolean isActive = true;

    Util.WriteYellow($"{CR}Enter ApiKey description - empty string to cancel:");
    var description = Console.ReadLine();
    if (!String.IsNullOrEmpty(description))
    {
        Util.WriteYellow($"{CR}Enter nb days before ApiKey expiration - empty string to have no expiration");
        var str = Console.ReadLine();
        int nb = 0;
        if (!String.IsNullOrEmpty(str))
        {
            string justNumbers = new String(str.Where(Char.IsDigit).ToArray());
            nb = int.Parse(justNumbers);
            Util.WriteDarkYellow($"Nb days specified:[{nb}]");
        }

        if (apiKey is not null)
        {
            Util.WriteYellow($"{CR}Do you want to set Active this ApiKey ? [Y]");
            var consoleKey = Console.ReadKey(true);
            isActive = consoleKey.Key == ConsoleKey.Y;
            Util.WriteDarkYellow($"Set isActive:[{isActive}]");
        }

        if (apiKey is null)
        {
            Util.WriteDarkYellow($"{CR}Creating ApiKey ...");
            ApiKey newApiKey = new()
            {
                Description = description,
                ExpirationDate = (nb == 0) ? null : DateTime.UtcNow.AddDays(nb)
            };

            var sdkResult = await RbApplication.CreateApiKeyAsync(newApiKey);
            if (sdkResult.Success)
            {
                var apiKeyCreated = sdkResult.Data;
                Util.WriteYellow($"{CR}ApiKey created:[{apiKeyCreated.ToString(DetailsLevel.Full)}");
            }
            else
                Util.WriteRed($"{CR}CreateApiKeyAsync - Error:[{sdkResult.Result}");
        }
        else
        {
            Util.WriteDarkYellow($"{CR}Updatign ApiKey ...");
            ApiKey newApiKey = new()
            {
                Id = apiKey.Id,
                Description = description,
                ExpirationDate = (nb == 0) ? null : DateTime.UtcNow.AddDays(nb),
                IsActive = isActive
            };

            var sdkResult = await RbApplication.UpdateApiKeyAsync(newApiKey);
            if (sdkResult.Success)
            {
                var apiKeyCreated = sdkResult.Data;
                Util.WriteYellow($"{CR}ApiKey updated:[{apiKeyCreated.ToString(DetailsLevel.Full)}");
            }
            else
                Util.WriteRed($"{CR}UpdateApiKeyAsync - Error:[{sdkResult.Result}");
        }
    }
    else
    {
        Util.WriteYellow($"Cancel ApiKey creation ...");
    }
}

async Task MenuUserApiKeyAsync()
{
    userManaged = null;
    Util.WriteGreen($"{CR}You are now managing ApiKey as USER");

    await Task.CompletedTask;
}

async Task MenuAdminApiKeyAsync()
{
    companyManaged = RbContacts.GetCompany();
    Util.WriteGreen($"To simplify, we use the company of the current user - Company:[{companyManaged.Name}]");
        
    await MenuListUsersAndSelectOneAsync();

    if (userManaged is null)
    {
        Util.WriteRed($"{CR}No user found/selected or you don't have admin right");
        await MenuUserApiKeyAsync();
    }
    else
    {
        Util.WriteGreen($"{CR}You are now managing ApiKey as ADMIN for Contact:[{userManaged.ToString(DetailsLevel.Small)}]");
    }
}

async Task MenuListUsersAndSelectOneAsync()
{
    Boolean canContinue = true;
    int offset = 0;
    int limit = 100;
    List<Contact> usersList = [];

    Util.WriteDarkYellow($"{CR}Asking list of users for Company [{companyManaged.Name}] ...");

    while (canContinue)
    {
        var sdkResult = await RbAdministration.GetUsersAsync(true, offset, limit, filterCompanyId: companyManaged.Id, null);
        if (sdkResult.Success)
        {
            var listFound = sdkResult.Data.Data;
            usersList.AddRange(listFound);
            canContinue = (listFound.Count == limit);
            offset += limit;
        }
        else
        {
            Util.WriteRed($"GetUsersAsync - Error:[{sdkResult.Result}]");
            return;
        }
    }

    if (usersList.Count == 0)
    {
        Util.WriteGreen($"None user ...");
        return;
    }

    Util.WriteYellow($"{CR}List of Users - Total[{usersList.Count}]):");
    foreach (var contact in usersList)
    {
        Util.WriteYellow($"\t- {Rainbow.Util.LogOnOneLine(contact.ToString(DetailsLevel.Medium))}");
    }

    canContinue = true;
    while (canContinue)
    {
        Util.WriteYellow($"{CR}Enter Id to select a user - empty string to cancel");

        var str = Console.ReadLine();
        if (!String.IsNullOrEmpty(str))
        {
            var result = usersList.Find(contact => contact.Peer.Id.Equals(str, StringComparison.InvariantCultureIgnoreCase));

            if (result is null)
                Util.WriteYellow($"No user found ...");
            else
            {
                canContinue = false;
                userManaged = result;
            }
        }
        else
        {
            Util.WriteYellow($"Cancel user search ...");
            canContinue = false;
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
        Util.WriteYellow($"{CR}We quit the process since AutoReconnection is stopped and we are disconnected");
        System.Environment.Exit(0);
    }

    if (connectionState.Status == ConnectionStatus.Connected)
        MenuDisplayInfo();

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

    Util.WriteWhite($"{CR}We quit since the AutoReconnection has been Cancelled");
    System.Environment.Exit(0);
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

