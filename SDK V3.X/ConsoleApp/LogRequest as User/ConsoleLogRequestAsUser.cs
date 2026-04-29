
using Rainbow;
using System.Text;

using Rainbow.Example.Common;

using Rainbow.SimpleJSON;

// --------------------------------------------------

ExeSettings? exeSettings = null;
Credentials? credentials = null;

if ((!ReadExeSettings()) || (exeSettings is null))
    return;

if ((!ReadCredentials(credentialsCanBeEmpty: true)) || (credentials is null))
    return;

// --------------------------------------------------

String CR = Rainbow.Util.CR; // Get carriage return;

// In "exeSettings.json" using "logFolderPath" property, we defined a folder where the logs must be stored
String logFolderPath = exeSettings.LogFolderPath;

// In "credentials.json" using "userConfig" object, we defined a prefix used as logger prefix (this prefix permits to have logs stored in specific file for this "userConfig")
String? logPrefix = credentials.UsersConfig[0]?.Prefix;
logPrefix ??= "";

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
var RbApplication = new Application(iniFolderFullPathName: logFolderPath, loggerPrefix: logPrefix, restrictions: restrictions);
var RbAutoReconnexion = RbApplication.GetAutoReconnection();
var RbAdministration = RbApplication.GetAdministration();
var RbContacts = RbApplication.GetContacts();
var RbCustomerCare = RbApplication.GetCustomerCare();

// Set global configuration info
RbApplication.SetApplicationInfo(credentials.ServerConfig.AppId, credentials.ServerConfig.AppSecret);
RbApplication.SetHostInfo(credentials.ServerConfig.HostName);

// Set event we want to manage
RbCustomerCare.LogRequestAsked += RbCustomerCare_LogRequestAsked;

// Check credentials set in file 'credentials.json'
String loginStored = "";
if ( (credentials.UsersConfig.Count > 0) 
    && (!String.IsNullOrEmpty(credentials.UsersConfig[0].Login))
    && (!String.IsNullOrEmpty(credentials.UsersConfig[0].Password)) )
{
    loginStored = credentials.UsersConfig[0].Login;
    ConsoleAbstraction.WriteBlue($"{CR}Credentials specified in file [./config/credentials.json]: {loginStored}");
}
else
    ConsoleAbstraction.WriteRed($"{CR}No valid credentials specific in file [./config/credentials.json]");


selectLoginMethod:
ConsoleAbstraction.WriteBlue($"Which account do you want to use to connect to Rainbow Server ?");
ConsoleAbstraction.WriteBlue($"\t - [F] to use account (if any) specified in File [./config/credentials.json]");
ConsoleAbstraction.WriteBlue($"\t - [A] to use a TV Activation code");
ConsoleAbstraction.WriteBlue($"\t - [P] to use previous credentials used in a previous connection");

ConsoleAbstraction.WriteWhite($"{CR}Select:");

SdkResult<Boolean> sdkResultBoolean;

var userInput = ConsoleAbstraction.ReadKey();
switch (userInput?.Key)
{
    case ConsoleKey.F: // Use File credentials.json to get login / pwd
        RbAutoReconnexion.UsePreviousLoginPwd = false;

        ConsoleAbstraction.WriteGreen($"{CR}Starting login using credentials stored in file [./config/credentials.json] (if any)");
        sdkResultBoolean = await RbApplication.LoginAsync(credentials.UsersConfig[0].Login, credentials.UsersConfig[0].Password);
        if(!sdkResultBoolean.Success)
        {
            ConsoleAbstraction.WriteRed($"{CR}Cannot get login - Error:[{sdkResultBoolean.Result}]");
            goto selectLoginMethod;
        }
        break;

    case ConsoleKey.A: // Ask TV ACtivation code
        RbAutoReconnexion.UsePreviousLoginPwd = false;

        ConsoleAbstraction.WriteBlue($"{CR}Please enter a TV Activation code:");
        var userInputs = ConsoleAbstraction.ReadLine();

        ConsoleAbstraction.WriteGreen($"{CR}Asking environment information about this activation code:[{userInputs}]");
        var sdkResultEnvironment = await RbAdministration.GetTvEnvironmentAsync(userInputs);
        if (sdkResultEnvironment.Success)
        {
            var environment = sdkResultEnvironment.Data;

            if (!environment.EnvironmentName.Equals(credentials.ServerConfig.HostName, StringComparison.InvariantCultureIgnoreCase))
            {
                ConsoleAbstraction.WriteRed($"{CR}This activation code must be used using this environment:[{environment.EnvironmentName}] but this example (as defined in \"credentials.json\") is using [{credentials.ServerConfig.HostName}]. Update in consequence the file ... Exiting...");
                return;
            }

            var sdkResultTvUser = await RbAdministration.ActivateTvAsync(userInputs);
            if (sdkResultTvUser.Success)
            {
                var TvUser = sdkResultTvUser.Data;
                ConsoleAbstraction.WriteWhite($"{CR}TV Account has been created.");

                ConsoleAbstraction.WriteGreen($"{CR}Trying to log on server using this TV Account [{TvUser.LoginEmail}] ...");
                sdkResultBoolean = await RbApplication.LoginAsync(TvUser.LoginEmail, TvUser.Password);

                if (!sdkResultBoolean.Success)
                {
                    ConsoleAbstraction.WriteRed($"{CR}Cannot used this account to log on server - Error:[{sdkResultBoolean.Result}]");
                    goto selectLoginMethod;
                }
            }
            else
            {
                ConsoleAbstraction.WriteRed($"{CR}Cannot create a TV Account- Error:[{sdkResultTvUser.Result}]");
                goto selectLoginMethod;
            }
        }
        else
        {
            ConsoleAbstraction.WriteRed($"{CR}Cannot get environment information - Error:[{sdkResultEnvironment.Result}]");
            goto selectLoginMethod;
        }
        break;

    case ConsoleKey.P: // Use previous login/pwd

        RbAutoReconnexion.UsePreviousLoginPwd = true;

        ConsoleAbstraction.WriteGreen($"Starting login using previous credentials stored in file [{RbApplication.GetIniFullPath()}] (if any)");
        sdkResultBoolean = await RbApplication.LoginUsingPreviousCredentialsAsync();
        if (!sdkResultBoolean.Success)
        {
            ConsoleAbstraction.WriteRed($"{CR}Cannot used previous credentials - Error:[{sdkResultBoolean.Result}]");
            goto selectLoginMethod;
        }
        break;
}
var currentContact = RbContacts.GetCurrentContact();

ConsoleAbstraction.WriteWhite($"{CR}Account is now loggued:");
ConsoleAbstraction.WriteWhite($"\t- login:[{currentContact.LoginEmail}]");
ConsoleAbstraction.WriteWhite($"\t- id:[{currentContact.Peer.Id}]");
ConsoleAbstraction.WriteWhite($"\t- resource:[{RbApplication.GetResource()}]");

ConsoleAbstraction.WriteBlue($"{CR}Use [ESC] at anytime to quit");
ConsoleAbstraction.WriteBlue($"Waiting a log request from an administrator");

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
                Environment.Exit(0);
                return;
        }
    }
}

#region EVENTS TRIGGERED BY THE SDK

void RbCustomerCare_LogRequestAsked(string logId)
{
    ConsoleAbstraction.WriteBlue($"{CR}Log request has been asked by an administrator - logId:[{logId}]");

    Action action = async () =>
    {
        // Acknowledge the request
        ConsoleAbstraction.WriteGreen($"{CR}Acknowledging the log request ...");
        var sdkResultLogRequest = await RbCustomerCare.AcknowledgeLogRequestAskedAsync(logId);
        if (sdkResultLogRequest.Success)
        {
            var logRequest = sdkResultLogRequest.Data;

            ConsoleAbstraction.WriteGreen($"{CR}Uploading a file associated to this log request ...");
            var sdkResultFileDescriptor = await RbCustomerCare.UploadFileForLogRequestAskedAsync(RbApplication.GetIniFullPath());
            if (!sdkResultFileDescriptor.Success)
                ConsoleAbstraction.WriteRed($"{CR}Cannot upload file - Error:[{sdkResultFileDescriptor.Result}]");
            else
            {
                var fileDescriptor = sdkResultFileDescriptor.Data;
                ConsoleAbstraction.WriteWhite($"{CR}File Descriptor Id created :[{fileDescriptor.Id}]");
                logRequest.Attachments ??= [];
                logRequest.Attachments.Add(fileDescriptor.Id);
            }


            // Store some specific
            try
            {
                // Here we just store OS, we a lot more can be stored - check LogRequest objcet documentation
                logRequest.DeviceDetails ??= new();
                logRequest.DeviceDetails.Os ??= new();
                logRequest.DeviceDetails.Os.Name = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
            }
            catch
            {
            }
            
            ConsoleAbstraction.WriteGreen($"{CR}Completing log request ...");
            var sdkResultBoolean = await RbCustomerCare.CompleteLogRequestAskedAsync(logRequest);
            if (!sdkResultBoolean.Success)
                ConsoleAbstraction.WriteRed($"{CR}Cannot complete log request - Error:[{sdkResultFileDescriptor.Result}]");
            else
                ConsoleAbstraction.WriteBlue($"{CR}Log request has been completed");
        }
        else
        {
            ConsoleAbstraction.WriteRed($"{CR}Cannot acknowledge the log request - Error:[{sdkResultLogRequest.Result}]");
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
