
using Rainbow;
using Rainbow.Consts;
using Rainbow.Example.Common;
using Rainbow.SimpleJSON;

using Rainbow.Model;

// --------------------------------------------------

ExeSettings? exeSettings = null;
Credentials? credentials = null;

if ((!ReadExeSettings()) || (exeSettings is null))
    return;

if ((!ReadCredentials()) || (credentials is null))
    return;



ConsoleAbstraction.WriteRed($"Account used: [{credentials.UsersConfig[0].Login}]");

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

// Set restrictions
Restrictions restrictions = new(true)
{
    LogRestRequest = true,
    LogEvent = true,
    LogEventParameters = true,
    LogEventRaised = true,
};

// Create Rainbow SDK objects
var RbApplication = new Rainbow.Application(iniFolderFullPathName: logFolderPath, iniFileName: logPrefix +".ini", loggerPrefix: logPrefix, restrictions: restrictions);
var RbAutoReconnection = RbApplication.GetAutoReconnection();
var RbContacts = RbApplication.GetContacts();
var RbAdministration = RbApplication.GetAdministration();


// We want to receive events from SDK
RbApplication.AuthenticationFailed += RbApplication_AuthenticationFailed;       // Triggered when the authentication process will fail
RbApplication.AuthenticationSucceeded += RbApplication_AuthenticationSucceeded; // Triggered when the authentication process will succeed
RbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;   // Triggered when the Connection State will change

RbAutoReconnection.Cancelled += RbAutoReconnection_Cancelled;                       // Triggered when AutoReonnection service is cancelled
RbAutoReconnection.Started += RbAutoReconnection_Started;                           // Triggered when AutoReonnection service is started
RbAutoReconnection.MaxNbAttemptsReached += RbAutoReconnection_MaxNbAttemptsReached; // Triggered when AutoReonnection service reachde the mawimun number of attempts tryig to connect to the server
RbAutoReconnection.TokenExpired += RbAutoReconnection_TokenExpired;                 // Triggered when the Security Token is expired

// Set global configuration info
RbApplication.SetApplicationInfo(credentials.ServerConfig.AppId, credentials.ServerConfig.AppSecret);
RbApplication.SetHostInfo(credentials.ServerConfig.HostName);

// Start login
ConsoleAbstraction.WriteWhite($"{CR}Starting login ...");

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
    ConsoleAbstraction.WriteYellow("");
    ConsoleAbstraction.WriteYellow("[ESC] at anytime to quit");
    ConsoleAbstraction.WriteYellow("[I] (Info) Display this info");

    ConsoleAbstraction.WriteYellow("");
    ConsoleAbstraction.WriteYellow("[A] Use ApiKey as [A]dmin");
    ConsoleAbstraction.WriteYellow("[U] Use ApiKey as [U]ser (default)");

    ConsoleAbstraction.WriteYellow("");
    ConsoleAbstraction.WriteYellow("[L] [L]ist ApiKey");
    ConsoleAbstraction.WriteYellow("[C] [C]reate ApiKey");
}

async Task CheckInputKey()
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
                        ConsoleAbstraction.WriteRed($"LogoutAsync - Error:[{taskSdkResult.Result}]");
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

    ConsoleAbstraction.WriteDarkYellow($"{CR}Asking list of ApiKeys{((userManaged is null) ? "" : $" for Contact:[{userManaged.ToString(DetailsLevel.Small)}]")}:");

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
            ConsoleAbstraction.WriteRed($"GetApiKeysAsync - Error:[{sdkResult.Result}]");
            return;
        }
    }

    if (apiKeysList.Count == 0)
    {
        ConsoleAbstraction.WriteGreen($"None ApiKeys found ...");
        return;
    }

    ConsoleAbstraction.WriteYellow($"{CR}List of ApiKeys - Total[{apiKeysList.Count}]):");
    foreach (var apiKey in apiKeysList)
    {
        ConsoleAbstraction.WriteYellow($"\t- {Rainbow.Util.LogOnOneLine(apiKey.ToString(DetailsLevel.Medium))}");
    }

    canContinue = true;
    while (canContinue)
    {
        ConsoleAbstraction.WriteYellow($"{CR}Enter Id to select an ApiKey - empty string to cancel");

        var str = ConsoleAbstraction.ReadLine();
        if (!String.IsNullOrEmpty(str))
        {
            var result = apiKeysList.Find(apiKey => apiKey.Id.Equals(str, StringComparison.InvariantCultureIgnoreCase));

            if (result is null)
                ConsoleAbstraction.WriteYellow($"No ApiKey found ...");
            else
            {
                apiKeyManaged = result;
                ConsoleAbstraction.WriteGreen($"ApiKey found and now managed: [{apiKeyManaged.ToString(DetailsLevel.Full)}]");

                ConsoleAbstraction.WriteYellow($"{CR}Do you want to ");
                ConsoleAbstraction.WriteRed($"\t [D]elete this ApiKey (can not be undone)");
                ConsoleAbstraction.WriteYellow($"\t [U]pdate it");
                var userInput = ConsoleAbstraction.ReadKey();
                if (userInput?.Key == ConsoleKey.D)
                {
                    ConsoleAbstraction.WriteDarkYellow($"{CR}Deleting ApiKey ...");

                    SdkResult<Boolean> sdkResultBoolean;
                    if (userManaged is null)
                        sdkResultBoolean = await RbApplication.DeleteApiKeyAsync(apiKeyManaged);
                    else
                        sdkResultBoolean = await RbAdministration.DeleteApiKeyAsync(apiKeyManaged);

                    if (sdkResultBoolean.Success)
                        ConsoleAbstraction.WriteGreen($"ApiKey has been deleted");
                    else
                        ConsoleAbstraction.WriteRed($"DeleteApiKeyAsync - Error:[{sdkResultBoolean.Result}]");
                }
                else if (userInput?.Key == ConsoleKey.U)
                    await MenuCreateOrUpdateApiKeyAsync(apiKeyManaged);
                else
                    ConsoleAbstraction.WriteYellow($"{CR}Bad key used ...");
            }
        }
        else
        {
            ConsoleAbstraction.WriteYellow($"Cancel search ...");
            canContinue = false;
        }
    }
}

async Task MenuCreateOrUpdateApiKeyAsync(ApiKey? apiKey = null)
{
    if(apiKey is null)
        ConsoleAbstraction.WriteGreen($"{CR}ApiKey can be created only for the current user:[{RbContacts.GetCurrentContact().ToString(DetailsLevel.Small)}]");
    else
    {
        var sdkResult = await RbApplication.GetApiKeyAsync(apiKey.Id);
        if (sdkResult.Success)
            apiKey = sdkResult.Data;
        else
        {
            ConsoleAbstraction.WriteRed($"{CR}GetApiKeyAsync - Error[{sdkResult.Result}]");
            return;
        }
    }

    Boolean isActive = true;

    ConsoleAbstraction.WriteYellow($"{CR}Enter ApiKey description - empty string to cancel:");
    var description = ConsoleAbstraction.ReadLine();
    if (!String.IsNullOrEmpty(description))
    {
        ConsoleAbstraction.WriteYellow($"{CR}Enter nb days before ApiKey expiration - empty string to have no expiration");
        var str = ConsoleAbstraction.ReadLine();
        int nb = 0;
        if (!String.IsNullOrEmpty(str))
        {
            string justNumbers = new String(str.Where(Char.IsDigit).ToArray());
            nb = int.Parse(justNumbers);
            ConsoleAbstraction.WriteDarkYellow($"Nb days specified:[{nb}]");
        }

        if (apiKey is not null)
        {
            ConsoleAbstraction.WriteYellow($"{CR}Do you want to set Active this ApiKey ? [Y]");
            var consoleKey = ConsoleAbstraction.ReadKey();
            isActive = consoleKey?.Key == ConsoleKey.Y;
            ConsoleAbstraction.WriteDarkYellow($"Set isActive:[{isActive}]");
        }

        if (apiKey is null)
        {
            ConsoleAbstraction.WriteDarkYellow($"{CR}Creating ApiKey ...");
            ApiKey newApiKey = new()
            {
                Description = description,
                ExpirationDate = (nb == 0) ? null : DateTime.UtcNow.AddDays(nb)
            };

            var sdkResult = await RbApplication.CreateApiKeyAsync(newApiKey);
            if (sdkResult.Success)
            {
                var apiKeyCreated = sdkResult.Data;
                ConsoleAbstraction.WriteYellow($"{CR}ApiKey created:[{apiKeyCreated.ToString(DetailsLevel.Full)}");
            }
            else
                ConsoleAbstraction.WriteRed($"{CR}CreateApiKeyAsync - Error:[{sdkResult.Result}");
        }
        else
        {
            ConsoleAbstraction.WriteDarkYellow($"{CR}Updatign ApiKey ...");
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
                ConsoleAbstraction.WriteYellow($"{CR}ApiKey updated:[{apiKeyCreated.ToString(DetailsLevel.Full)}");
            }
            else
                ConsoleAbstraction.WriteRed($"{CR}UpdateApiKeyAsync - Error:[{sdkResult.Result}");
        }
    }
    else
    {
        ConsoleAbstraction.WriteYellow($"Cancel ApiKey creation ...");
    }
}

async Task MenuUserApiKeyAsync()
{
    userManaged = null;
    ConsoleAbstraction.WriteGreen($"{CR}You are now managing ApiKey as USER");

    await Task.CompletedTask;
}

async Task MenuAdminApiKeyAsync()
{
    companyManaged = RbContacts.GetCompany();
    ConsoleAbstraction.WriteGreen($"To simplify, we use the company of the current user - Company:[{companyManaged.Name}]");
        
    await MenuListUsersAndSelectOneAsync();

    if (userManaged is null)
    {
        ConsoleAbstraction.WriteRed($"{CR}No user found/selected or you don't have admin right");
        await MenuUserApiKeyAsync();
    }
    else
    {
        ConsoleAbstraction.WriteGreen($"{CR}You are now managing ApiKey as ADMIN for Contact:[{userManaged.ToString(DetailsLevel.Small)}]");
    }
}

async Task MenuListUsersAndSelectOneAsync()
{
    Boolean canContinue = true;
    int offset = 0;
    int limit = 100;
    List<Contact> usersList = [];

    ConsoleAbstraction.WriteDarkYellow($"{CR}Asking list of users for Company [{companyManaged.Name}] ...");

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
            ConsoleAbstraction.WriteRed($"GetUsersAsync - Error:[{sdkResult.Result}]");
            return;
        }
    }

    if (usersList.Count == 0)
    {
        ConsoleAbstraction.WriteGreen($"None user ...");
        return;
    }

    ConsoleAbstraction.WriteYellow($"{CR}List of Users - Total[{usersList.Count}]):");
    foreach (var contact in usersList)
    {
        ConsoleAbstraction.WriteYellow($"\t- {Rainbow.Util.LogOnOneLine(contact.ToString(DetailsLevel.Medium))}");
    }

    canContinue = true;
    while (canContinue)
    {
        ConsoleAbstraction.WriteYellow($"{CR}Enter Id to select a user - empty string to cancel");

        var str = ConsoleAbstraction.ReadLine();
        if (!String.IsNullOrEmpty(str))
        {
            var result = usersList.Find(contact => contact.Peer.Id.Equals(str, StringComparison.InvariantCultureIgnoreCase));

            if (result is null)
                ConsoleAbstraction.WriteYellow($"No user found ...");
            else
            {
                canContinue = false;
                userManaged = result;
            }
        }
        else
        {
            ConsoleAbstraction.WriteYellow($"Cancel user search ...");
            canContinue = false;
        }
    }
}

#region Events received from the SDK
void RbApplication_ConnectionStateChanged(Rainbow.Model.ConnectionState connectionState)
{
    // Display the CurrentNbAttempts
    if (connectionState.Status == ConnectionStatus.Connecting)
        ConsoleAbstraction.WriteYellow($"{CR}AutoReconnection.CurrentNbAttempts: [{RbAutoReconnection.CurrentNbAttempts}]");

    // We log connection state in the console - use differnte color according the status
    // We log connection state in the console - use differnte color according the status
    String color;
    switch (connectionState.Status)
    {
        case ConnectionStatus.Connected:
            color = ConsoleAbstraction.BLUE;
            break;

        case ConnectionStatus.Disconnected:
            color = ConsoleAbstraction.RED;
            break;

        case ConnectionStatus.Connecting:
        default:
            color = ConsoleAbstraction.GREEN;
            break;

    }
    ConsoleAbstraction.WriteLine($"{CR}{color}Event Application.ConnectionStateChanged triggered - Connection Status: [{connectionState.Status}]");

    // If we are disconnected and the AutoReconnection is stopped, nothing more wille happpen
    // So we quit the process
    if ((connectionState.Status == ConnectionStatus.Disconnected) && (!RbAutoReconnection.IsStarted))
    {
        ConsoleAbstraction.WriteYellow($"{CR}We quit the process since AutoReconnection is stopped and we are disconnected");
        System.Environment.Exit(0);
    }

    if (connectionState.Status == ConnectionStatus.Connected)
        MenuDisplayInfo();

}

void RbApplication_AuthenticationSucceeded()
{
    // Authentication Succeeded- we display in the console the info
    ConsoleAbstraction.WriteBlue($"{CR}Event Application.AuthenticationSucceeded triggered");
}

void RbApplication_AuthenticationFailed(SdkError sdkError)
{
    // Authentication failed - we display in the console the reason
    ConsoleAbstraction.WriteRed($"{CR}Event Application.AuthenticationFailed triggered - SdkError:{sdkError}");
}

void RbAutoReconnection_TokenExpired()
{
    ConsoleAbstraction.WriteRed($"{CR}Event AutoReconnection.TokenExpired triggered");
}

void RbAutoReconnection_MaxNbAttemptsReached()
{
    ConsoleAbstraction.WriteRed($"{CR}Event AutoReconnection.MaxNbAttemptsReached triggered");
}

void RbAutoReconnection_Started()
{
    ConsoleAbstraction.WriteBlue($"{CR}Event AutoReconnection.Started triggered");
}

void RbAutoReconnection_Cancelled(SdkError sdkError)
{
    if (sdkError.Type == Rainbow.Enums.SdkErrorType.NoError)
    {
        // The service has been cancelled/stopped voluntarily
        ConsoleAbstraction.WriteYellow($"{CR}Event AutoReconnection.Cancelled triggered - Done using the SDK voluntarily");
    }
    else 
    {
        // The service has been cancelled/stopped involuntarily - display the reason
        ConsoleAbstraction.WriteBlue($"{CR}Event AutoReconnection.Cancelled triggered - SdkError(Exception]:[{sdkError}]");
    }

    ConsoleAbstraction.WriteWhite($"{CR}We quit since the AutoReconnection has been Cancelled");
    System.Environment.Exit(0);
}

#endregion Events received from the SDK

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

Boolean ReadCredentials(string fileName = "credentials.json")
{
    var credentialsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}{fileName}";
    if (!File.Exists(credentialsFilePath))
    {
        ConsoleAbstraction.WriteRed($"The file '{credentialsFilePath}' has not been found.");
        return false;
    }

    String jsonConfig = File.ReadAllText(credentialsFilePath);
    var jsonNode = JSON.Parse(jsonConfig);

    if (!Credentials.FromJsonNode(jsonNode["credentials"], out credentials))
    {
        ConsoleAbstraction.WriteRed($"Cannot read 'credentials' object OR invalid/missing data in file:[{fileName}].");
        return false;
    }

    return true;
}

