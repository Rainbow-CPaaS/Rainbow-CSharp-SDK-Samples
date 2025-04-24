
using Rainbow;
using Rainbow.Consts;

using Rainbow.Example.Common;
using Util = Rainbow.Example.Common.Util;
using Rainbow.SimpleJSON;
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

Task RbTask = Task.CompletedTask;

// Create Rainbow SDK objects
var RbApplication = new Application(iniFolderFullPathName: logFolderPath, iniFileName: logPrefix +".ini", loggerPrefix: logPrefix);
var RbAutoReconnection = RbApplication.GetAutoReconnection();
var RbBubbles = RbApplication.GetBubbles();
var RbContacts = RbApplication.GetContacts();

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

RbApplication.Restrictions.LogRestRequest = true;

await MenuDisplayInfoAsync();

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
                await MenuEscapeAsync();
                return;

            case ConsoleKey.I: // Info
                await MenuDisplayInfoAsync();
                return;

            case ConsoleKey.C: // Search Contact 
                await MenuSearchContactAsync();
                return;
        }
    }
}

async Task MenuDisplayInfoAsync()
{
    Util.WriteYellow("");
    Util.WriteYellow("[ESC] at anytime to quit");
    Util.WriteYellow("[I] (Info) Display this info");

    Util.WriteYellow("");
    Util.WriteYellow("[C] (Contact) To search for a contact");

    await Task.CompletedTask;
}

async Task MenuSearchContactAsync()
{
    if (!RbTask.IsCompleted)
    {
        Util.WriteRed("Task is already in progress");
        return;
    }

    Action action = async () =>
    {
        if (!RbApplication.IsConnected())
        {
            Util.WriteRed($"You are not connected to Rainbow server ...");
            return;
        }

        var comparers = RbApplication.GetComparers();
        var company = RbContacts.GetCompany();

        var canContinue = true;

        while (canContinue)
        {
            Util.WriteYellow($"{Rainbow.Util.CR}Select a the search mode:");
            Util.WriteYellow($"\t[0] By display name");
            Util.WriteYellow($"\t[1] By tag");
            Util.WriteYellow($"\t[2] By phone number (exact match)");
            Util.WriteYellow($"\t[3] By email");
            Util.WriteYellow($"{CR}\t[C] to cancel");
            var userInput = Console.ReadKey();

            if (userInput.Key == ConsoleKey.C)
            {
                canContinue = false;
                Util.WriteDarkYellow("Cancel search ...");
            }
            else if (char.IsDigit(userInput.KeyChar))
            {
                var selection = int.Parse(userInput.KeyChar.ToString());
                if ((selection >= 0) && (selection < 4))
                {
                    String mode;
                    switch (selection)
                    {
                        default:
                        case 0:
                            mode = "Display Name";
                            break;
                        case 1:
                            mode = "Tag";
                            break;
                        case 2:
                            mode = "Phone number";
                            break;
                        case 3:
                            mode = "Email";
                            break;
                    }

                    while (canContinue)
                    {
                        Util.WriteYellow($"{Rainbow.Util.CR}Enter a text to search a Contact by {mode}: (empty text to cancel)");
                        var str = Console.ReadLine();
                        if (String.IsNullOrEmpty(str))
                        {
                            canContinue = false;
                            Util.WriteDarkYellow($"Cancel search by {mode}...");
                        }
                        else
                        {
                            Util.WriteGreen($"Searching for [{str}] by {mode} ...");
                            SdkResult<SearchContactsResult> sdkResult;

                            switch (selection)
                            {
                                default:
                                case 0:
                                    sdkResult = await RbContacts.SearchContactsByDisplayNameAsync(str, 20, withPresence: true);
                                    break;
                                case 1:
                                    sdkResult = await RbContacts.SearchContactsByTagAsync(str, 20, withPresence: true);
                                    break;
                                case 2:
                                    sdkResult = await RbContacts.SearchContactsByPhoneNumberAsync(str);
                                    break;
                                case 3:
                                    sdkResult = await RbContacts.SearchContactsByEmailsAsync([str]);
                                    break;
                            }

                            if (sdkResult.Success)
                            {
                                int nb = 0;
                                var rainbowContacts = sdkResult.Data.ContactsList;
                                var directoryContacts = sdkResult.Data.DirectoryContactsList;
                                var phoneBookContacts = sdkResult.Data.PhonebooksList;
                                var office365Contacts = sdkResult.Data.O365AdContactsList;

                                Util.WriteWhite($"Searching Result:");

                                // Rainbow Contacts:
                                nb = (rainbowContacts is null) ? 0 : rainbowContacts.Count;
                                Util.WriteToConsole($"{Util.WHITE}Rainbow contacts - found: {Util.BLUE}{nb}");
                                if (rainbowContacts is not null)
                                {
                                    foreach (var rbContact in rainbowContacts)
                                    {
                                        var aboutCompany = "";
                                        if (rbContact.CompanyId == company.Id)
                                            aboutCompany = $" {Util.BLUE}[In your company]";
                                        else
                                            aboutCompany = $" {Util.GREEN}[Not in you company]";

                                        var aboutPresence = "";
                                        if (!String.IsNullOrEmpty(rbContact.Presence))
                                            aboutPresence = $"{Util.WHITE} - Presence:[{rbContact.Presence}]";

                                        Util.WriteToConsole($"\t{Util.WHITE}{rbContact.ToString(DetailsLevel.Small)}{aboutCompany}{aboutPresence}");
                                    }
                                }


                                // Directory Contacts:
                                nb = (directoryContacts is null) ? 0 : directoryContacts.Count;
                                Util.WriteToConsole($"{Util.WHITE}Directory contacts - found: {Util.BLUE}{nb}");
                                if (directoryContacts is not null)
                                {
                                    foreach (var directoryContact in directoryContacts)
                                    {
                                        var aboutCompany = "";
                                        if (directoryContact.CompanyId == company.Id)
                                            aboutCompany = $" {Util.BLUE}[In your company]";
                                        else
                                            aboutCompany = $" {Util.GREEN}[Not in you company]";

                                        var displayName = comparers.GetDisplayName(directoryContact.FirstName, directoryContact.LastName);

                                        Util.WriteToConsole($"\t{Util.WHITE}{displayName}{aboutCompany}");
                                    }
                                }

                                // Phonebooks Contacts:
                                nb = (phoneBookContacts is null) ? 0 : phoneBookContacts.Count;
                                Util.WriteToConsole($"{Util.WHITE}Phonebook contacts - found: {Util.BLUE}{nb}");
                                if (phoneBookContacts is not null)
                                {
                                    foreach (var phonebookContact in phoneBookContacts)
                                    {
                                        var displayName = comparers.GetDisplayName(phonebookContact.FirstName, phonebookContact.LastName);

                                        Util.WriteToConsole($"\t{Util.WHITE}{displayName}");
                                    }
                                }

                                // Office365 Contacts:
                                nb = (office365Contacts is null) ? 0 : office365Contacts.Count;
                                Util.WriteToConsole($"{Util.WHITE}Office365 contacts - found: {Util.BLUE}{nb}");
                                if (office365Contacts is not null)
                                {
                                    foreach (var office365Contact in office365Contacts)
                                    {
                                        var displayName = comparers.GetDisplayName(office365Contact.FirstName, office365Contact.LastName);

                                        Util.WriteToConsole($"\t{Util.WHITE}{displayName}");
                                    }
                                }
                            }
                            else
                            {
                                Util.WriteRed($"Error:[{sdkResult.Result}]");
                            }
                        }
                    }

                    canContinue = true;
                }
                else
                {
                    Util.WriteDarkYellow($"{CR}Bad key used ...");
                }
            }
            else
            {
                Util.WriteDarkYellow($"{CR}Bad key used ...");
            }
        }
    };

    RbTask = Task.Run(action);
    await RbTask;
}

async Task MenuEscapeAsync()
{
    if (!RbTask.IsCompleted)
    {
        Util.WriteRed("Task is already in progress");
        return;
    }

    Action action = async () =>
    {
        Util.WriteGreen($"Asked to end process using [ESC] key");
        if (RbApplication.IsConnected())
        {
            Util.WriteGreen($"Asked to quit - Logout");
            var sdkResultBoolean = await RbApplication.LogoutAsync();
            if (!sdkResultBoolean.Success)
                Util.WriteRed($"Cannot logout - SdkError:[{sdkResultBoolean.Result}]");
            await Task.Delay(500);
        }
        System.Environment.Exit(0);
    };
    RbTask = Task.Run(action);
    await RbTask;
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
        System.Environment.Exit(0);
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

