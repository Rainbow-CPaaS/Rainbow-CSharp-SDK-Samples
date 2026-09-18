using Rainbow;
using Rainbow.Consts;
using Rainbow.Example.Common;
using Rainbow.Model;
using Rainbow.SimpleJSON;
using StartJoinConfAndAddParticipant;
using System.Text;

// --------------------------------------------------

ExeSettings? exeSettings = null;
Credentials? credentials = null;
Config? config = null;

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

Task RbTask = Task.CompletedTask;

ConferenceStatus _conferenceStatus = new();

// Set restrictions
Restrictions restrictions = new(true) // Here we choose to use all services - according your need it's better to restrict to the minimum
{
    LogRestRequest = true
};

// Create Rainbow SDK objects
var RbApplication = new Application(iniFolderFullPathName: logFolderPath, iniFileName: logPrefix +".ini", loggerPrefix: logPrefix, restrictions: restrictions);
var RbAutoReconnection = RbApplication.GetAutoReconnection();
var RbBubbles = RbApplication.GetBubbles();
var RbContacts = RbApplication.GetContacts();
var RbConferences = RbApplication.GetConferences();

// We want to receive events from SDK
RbApplication.AuthenticationFailed += RbApplication_AuthenticationFailed;           // Triggered when the authentication process will fail
RbApplication.AuthenticationSucceeded += RbApplication_AuthenticationSucceeded;     // Triggered when the authentication process will succeed
RbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;       // Triggered when the Connection State will change

RbAutoReconnection.Cancelled += RbAutoReconnection_Cancelled;                       // Triggered when AutoReonnection service is cancelled
RbAutoReconnection.Started += RbAutoReconnection_Started;                           // Triggered when AutoReonnection service is started
RbAutoReconnection.MaxNbAttemptsReached += RbAutoReconnection_MaxNbAttemptsReached; // Triggered when AutoReonnection service reachde the mawimun number of attempts tryig to connect to the server
RbAutoReconnection.TokenExpired += RbAutoReconnection_TokenExpired;                 // Triggered when the Security Token is expired

RbBubbles.BubbleAffiliationsPerformed += RbBubbles_BubbleAffiliationsPerformed;     // Triggered when all bubbles have been affiliated

RbConferences.ConferenceUpdated += RbConferences_ConferenceUpdated;
RbConferences.ConferenceParticipantsUpdated += RbConferences_ConferenceParticipantsUpdated;
RbConferences.ConferenceRemoved += RbConferences_ConferenceRemoved;

// Set global configuration info
RbApplication.SetApplicationInfo(credentials.ServerConfig.AppId, credentials.ServerConfig.AppSecret);
RbApplication.SetHostInfo(credentials.ServerConfig.HostName);

await MenuDisplayInfoAsync();

// Start login
ConsoleAbstraction.WriteWhite($"{CR}Starting login ...");

var _ = RbApplication.LoginAsync(credentials.UsersConfig[0].Login, credentials.UsersConfig[0].Password);

do
{
    await Task.Delay(200);
    await CheckInputKey();
} while (true);

async Task CheckInputKey()
{
    while (ConsoleAbstraction.KeyAvailable)
    {
        var userInput = ConsoleAbstraction.ReadKey();

        switch (userInput?.Key)
        {
            case ConsoleKey.Escape:
                await MenuEscapeAsync();
                return;

            case ConsoleKey.I: // Info
                await MenuDisplayInfoAsync();
                return;

            case ConsoleKey.L: // Load (Reload) Config file
                await MenuLoadConfigAsync();
                return;

            case ConsoleKey.C: //(Conference) To create / start / join conference
                await MenuCreateStartAndJoinConferenceAsync();
                return;

            case ConsoleKey.P: // (Phone Numbers) To start to add phone numbers
                await MenuAddPhoneNumbersAsync();
                return;
        }
    }
}

async Task MenuDisplayInfoAsync()
{
    ConsoleAbstraction.WriteYellow("");
    ConsoleAbstraction.WriteYellow("[ESC] at anytime to quit");
    ConsoleAbstraction.WriteYellow("[I] (Info) Display this info");

    ConsoleAbstraction.WriteYellow("");
    ConsoleAbstraction.WriteYellow("[R] (Reload) To reload config");

    ConsoleAbstraction.WriteYellow("");
    ConsoleAbstraction.WriteYellow("[C] (Conference) To create / start / join conference");

    ConsoleAbstraction.WriteYellow("");
    ConsoleAbstraction.WriteYellow("[P] (Conference) (Phone Numbers) To start to add phone numbers");
    await Task.CompletedTask;
}

async Task MenuLoadConfigAsync()
{
    if (!RbTask.IsCompleted)
    {
        ConsoleAbstraction.WriteRed("Load Config - Task is already in progress. Try later.");
        return;
    }

    if (!RbBubbles.BubbleAffiliationsIsPerformed)
    {
        ConsoleAbstraction.WriteRed($"You are not connectecd or bubble affiliatsionts it not yet over. Try later.");
        return;
    }

    if (_conferenceStatus.ConferenceInProgress)
    {
        ConsoleAbstraction.WriteRed($"The process to create / start / join conference and add participants is in progress ...");
        return;
    }

    ConsoleAbstraction.WriteGreen($"Loading config ...");

    //TODO - check is a process (start-join-add_participant) is already in progress - if yes, we cannot load config

    if (ReadConfig())
    {
        ConsoleAbstraction.WriteBlue($"Config well loaded ...");
    }
    else
    {
        ConsoleAbstraction.WriteRed($"Cannot load config ...");
    }
}

async Task MenuCreateStartAndJoinConferenceAsync()
{
    if (!RbTask.IsCompleted)
    {
        ConsoleAbstraction.WriteRed("CreateStartAndJoinConference - Task is already in progress. Try later.");
        return;
    }

    if (!RbBubbles.BubbleAffiliationsIsPerformed)
    {
        ConsoleAbstraction.WriteRed($"You are nopt connectecd or bubble affiliatsionts it not yet over. Try later.");
        return;
    }

    if (_conferenceStatus.ConferenceInProgress)
    {
        ConsoleAbstraction.WriteRed($"The process to create / start / join conference and add participants is in progress ...");
        return;
    }

    ConsoleAbstraction.WriteGreen($"Create / Start / Join Conference  ...");

    if (config?.Bubble is null)
    {
        ConsoleAbstraction.WriteRed($"Bubble in config object is null...");
        return;
    }

    // Get Operator as Contact
    if (_conferenceStatus.Operator is null)
    {
        var contact = await GetContactAsync(config.Operator);
        if (contact is null)
        {
            ConsoleAbstraction.WriteRed($"Cannot get Contact object using Operator config info ...");
            return;
        }
        _conferenceStatus.Operator = contact; // Store info
    }

    // Create Bubble
    var sdkResultBubble = await RbBubbles.CreateBubbleAsync(GetBubbleName(), config.Bubble.Topic, BubbleVisibility.AsPrivate);
    if (!sdkResultBubble.Success)
    {
        ConsoleAbstraction.WriteRed($"Cannot create bubble - [{sdkResultBubble.Result}]");
        return;
    }
    _conferenceStatus.Bubble = sdkResultBubble.Data; // Store info

    // Add Operator as Moderator in this bubble
    var sdkResultBoolean = await RbBubbles.AddMemberAsync(_conferenceStatus.Bubble, _conferenceStatus.Operator, BubbleMemberPrivilege.Moderator, true);
    if (!sdkResultBoolean.Success)
    {
        ConsoleAbstraction.WriteRed($"Cannot add operator as Moderator - [{sdkResultBoolean.Result}]");

        // Delete the bubble previously created
        RbBubbles.DeleteBubbleAsync(_conferenceStatus.Bubble).StartAndForget();
        return;
    }

    // Start conference
    sdkResultBoolean = await RbConferences.StartConferenceAsync(_conferenceStatus.Bubble);
    if (!sdkResultBoolean.Success)
    {
        ConsoleAbstraction.WriteRed($"Cannot start conference - [{sdkResultBoolean.Result}]");

        // Delete the bubble previously created
        RbBubbles.DeleteBubbleAsync(_conferenceStatus.Bubble).StartAndForget();
        return;
    }

    // Join conference
    var sdkResultString = await RbConferences.JoinConferenceWithoutMediaAsync(_conferenceStatus.Bubble);
    if (!sdkResultString.Success)
    {
        ConsoleAbstraction.WriteRed($"Cannot join conference - [{sdkResultBoolean.Result}]");

        // Stop conference then Delete the bubble previously created
        RbConferences.StopConferenceAsync(_conferenceStatus.Bubble)
            .ContinueWith( async (_sdkResultBool) => { await RbBubbles.DeleteBubbleAsync(_conferenceStatus.Bubble); })
            .StartAndForget();
        return;
    }
    _conferenceStatus.ConferenceInProgress = true;

    // Now we use event ConferenceUpdated from Conferences service to follow participants in the conference
}

async Task MenuAddPhoneNumbersAsync()
{
    if (!RbTask.IsCompleted)
    {
        ConsoleAbstraction.WriteRed("AddPhoneNumbers - Task is already in progress. Try later.");
        return;
    }

    if (!RbBubbles.BubbleAffiliationsIsPerformed)
    {
        ConsoleAbstraction.WriteRed($"You are nopt connectecd or bubble affiliatsionts it not yet over. Try later.");
        return;
    }

    if (!_conferenceStatus.ConferenceInProgress)
    {
        ConsoleAbstraction.WriteRed($"The process to create / start / join conference and add participants is NOT in progress ...");
        return;
    }

    if (_conferenceStatus.PhoneNumbersToHaveInConf.Count > 0)
    {
        ConsoleAbstraction.WriteRed($"The process to add phone numbers is already in progress ...");
        return;
    }

    if(config is null)
    {
        ConsoleAbstraction.WriteRed($"Config object is null ...");
        return;
    }

    var max = config.MaxPhoneNumberToAddInFirstRow;
    List<String> phoneNumbers = (config.PhoneNumbers?.Count > 0) ? [ ..config.PhoneNumbers] : [];
    _conferenceStatus.PhoneNumbersToHaveInConf = phoneNumbers ;

    // Take first only max phone numbers
    phoneNumbers = [ ..phoneNumbers.Take(max)];
    max = phoneNumbers.Count;
    _conferenceStatus.PhoneNumbersInProgress = phoneNumbers;

    ConsoleAbstraction.WriteGreen($"Asking to add first [{max}] phone numbers [{String.Join(", ", phoneNumbers)}] to the conf ...");

    var sdkResult = await RbConferences.AddParticipantsAsync(_conferenceStatus.Bubble, phoneNumbers: phoneNumbers);
    if (sdkResult.Success)
    {
        ConsoleAbstraction.WriteGreen($"Server has taken into account the action to add in conf first [{max}] phone numbers.");
    }
    else
    {
        ConsoleAbstraction.WriteRed($"Phone numbers cannot be added to the conf - [{sdkResult.Result}]");

        _conferenceStatus.PhoneNumbersInProgress.Clear();
        _conferenceStatus.PhoneNumbersToHaveInConf.Clear();
    }

    return;
}

async Task MenuEscapeAsync()
{
    if (!RbTask.IsCompleted)
    {
        ConsoleAbstraction.WriteRed("Task is already in progress - try later");
        return;
    }

    Action action = async () =>
    {
        ConsoleAbstraction.WriteGreen($"Asked to end process using [ESC] key");
        if (RbApplication.IsConnected())
        {
            ConsoleAbstraction.WriteGreen($"Asked to quit - Logout");
            var sdkResultBoolean = await RbApplication.LogoutAsync();
            if (!sdkResultBoolean.Success)
                ConsoleAbstraction.WriteRed($"Cannot logout - SdkError:[{sdkResultBoolean.Result}]");
            await Task.Delay(500);
        }
        System.Environment.Exit(0);
    };
    RbTask = Task.Run(action);
    await RbTask;
}

async Task<Contact?> GetContactAsync(Account? account)
{
    if ( (RbContacts is null) || (account is null) )
        return null;

    Contact? result = null;
    if (!String.IsNullOrEmpty(account.Id))
        result = await RbContacts.GetContactByIdInCacheFirstAsync(account.Id);

    if (result is not null)
        return result;

    if (!String.IsNullOrEmpty(account.Jid))
        result = await RbContacts.GetContactByJidInCacheFirstAsync(account.Jid);

    if (result is not null)
        return result;

    result = GetContactByEmail(account.Login);

    return result;
}

void UpdateBubbleName()
{
    if(_conferenceStatus.Bubble is not null)
    {
        var name = GetBubbleName();
        RbBubbles.UpdateBubbleAsync(_conferenceStatus.Bubble, name, config?.Bubble?.Topic ?? "", BubbleVisibility.AsPrivate).StartAndForget();
    }
}

Contact? GetContactByEmail(String email)
{
    if (RbContacts is null)
        return null;

    Contact? result = null;
    if (!String.IsNullOrEmpty(email))
        result = RbContacts.GetAllContacts()?.Find(contact => contact?.LoginEmail?.Equals(email, StringComparison.InvariantCultureIgnoreCase) == true);

    return result;
}

String GetBubbleName()
{
    String name = config?.Bubble?.Peer.DisplayName ?? "";
    if (name == "") return "";

    int nb = 0;
    int max = config?.PhoneNumbers?.Count ?? 0;
    if (max == 0) return name;

    if (_conferenceStatus.ConferenceInProgress)
        nb = _conferenceStatus.PhoneNumbersAlreadyInConference.Count;
 
    if(nb == max)
        return $"[READY] {name}";
    return $"[{nb}/{max}] {name}";
}

#region Events received from the SDK
void RbApplication_ConnectionStateChanged(Rainbow.Model.ConnectionState connectionState)
{
    // Display the CurrentNbAttempts
    if (connectionState.Status == ConnectionStatus.Connecting)
        ConsoleAbstraction.WriteYellow($"{CR}AutoReconnection.CurrentNbAttempts: [{RbAutoReconnection.CurrentNbAttempts}]");

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

void RbBubbles_BubbleAffiliationsPerformed()
{
    // Bubble Affiliations Performed - we display in the console the info
    ConsoleAbstraction.WriteBlue($"{CR}Event Bubbles_BubbleAffiliationsPerformed triggered");

    // Load config
    MenuLoadConfigAsync().StartAndForget();
}

void RbConferences_ConferenceUpdated(Conference conference)
{
}

void RbConferences_ConferenceRemoved(Conference conference)
{
    if ((_conferenceStatus is null) || (_conferenceStatus.Bubble is null) || (_conferenceStatus.Bubble.Peer is null))
        return;

    if (conference.Peer.Id == _conferenceStatus.Bubble.Peer.Id)
    {
        if (_conferenceStatus.ConferenceDelegatedToOperator)
        {
            ConsoleAbstraction.WriteGreen($"Conference has been stopped by the Operator");

            // Delete the bubble previously created
            RbBubbles.DeleteBubbleAsync(_conferenceStatus.Bubble)
                .ContinueWith(async (task) =>
                {
                    _conferenceStatus.Reset();
                    var sdkResult = await task;
                    if (sdkResult.Success)
                    {
                        ConsoleAbstraction.WriteGreen($"Bubble has been deleted");
                    }
                    else
                        ConsoleAbstraction.WriteRed($"Bubble has not been deleted - [{sdkResult.Result}]");
                })
                .StartAndForget();
        }
    }
}

void RbConferences_ConferenceParticipantsUpdated(Conference conference, Dictionary<string, ConferenceParticipant> participantsById)
{
    if ((_conferenceStatus is null) || (_conferenceStatus.Bubble is null) || (_conferenceStatus.Bubble.Peer is null) || (config is null) )
        return;

    ConferenceParticipant? participant = null;

    // Check if the conference we have started / joined
    if (conference.Peer.Id == _conferenceStatus.Bubble.Peer.Id)
    {
        // Check Operator - We must delegate to him the conf
        if (participantsById.TryGetValue(_conferenceStatus.Operator?.Peer?.Id ?? "", out participant) && participant is not null)
        {
            if(participant.Status.Equals("active", StringComparison.InvariantCultureIgnoreCase))
            {
                // Delegate conference to the operator (if not already done)
                if (!_conferenceStatus.ConferenceDelegatedToOperator)
                {
                    _conferenceStatus.ConferenceDelegatedToOperator = true;
                    RbConferences.DelegateConferenceAsync(_conferenceStatus.Bubble, _conferenceStatus.Operator?.Peer?.Id ?? "")
                        .ContinueWith(async (task) =>
                        {
                            var sdkResult = await task;
                            if (sdkResult.Success)
                            {
                                ConsoleAbstraction.WriteGreen($"Conference has been delegated to the Operator");
                                ConsoleAbstraction.WriteYellow($"{Util.CR}\t\tℹ️ℹ️ You can now add participants using their phone numbers ℹ️ℹ️");
                            }
                            else
                                ConsoleAbstraction.WriteRed($"Conference cannot been delegated to the Operator - [{sdkResult.Result}]");
                            _conferenceStatus.ConferenceDelegatedToOperator = sdkResult.Success;
                        });
                }
            }
        }

        if (_conferenceStatus.PhoneNumbersToHaveInConf.Count > 0)
        { 
            var participants = participantsById.Values;
            var participantsIdWithPhoneNumbers = participants.Where(p => p?.Peer?.Type?.Equals(EntityType.PhoneNumber) == true).Select(p => p.Id);
                                          
            foreach(var id in participantsIdWithPhoneNumbers)
            {
                if(participantsById.TryGetValue(id , out participant) && participant is not null)
                {
                    var phoneNumber = participant.Peer.PhoneNumber;

                    if (participant.Status.Equals(config.AddNewPhoneNumberWhen ?? "", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!_conferenceStatus.PhoneNumbersAlreadyInConference.Contains(phoneNumber))
                        {
                            _conferenceStatus.PhoneNumbersAlreadyInConference.Add(phoneNumber);
                            _conferenceStatus.PhoneNumbersInProgress.Remove(phoneNumber);

                            ConsoleAbstraction.WriteGreen($"Phone number [{phoneNumber}] is now considered to be managed (call is in progress / finished with it).");

                            UpdateBubbleName();
                        }
                    }

                    // Perhaps a remote didn't answer and the server hangup the call. In this case we call again this phone number (this case must be enhanced in real scenario to avoid to call again and again)
                    else if (participant.Status.Equals("hangup", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _conferenceStatus.PhoneNumbersAlreadyInConference.Remove(phoneNumber);
                        _conferenceStatus.PhoneNumbersInProgress.Remove(phoneNumber);

                        ConsoleAbstraction.WriteYellow($"Phone number [{phoneNumber}] is now in status \"hangup\" - we will try to call it again");
                    }
                }
            }

            // Check if it's necessary to add another phone number
            CallNextPhoneNumber();
        }

        // List all Participants with their status
        StringBuilder sb = new();
        sb.Append($"Participants list:");
        foreach (var confParticipant in participantsById.Values)
        {
            sb.Append($"{CR}{Util.LogOnOneLine(confParticipant.ToString(false, DetailsLevel.Full))}");
        }
        sb.Append($"{CR}{CR}{CR}");
        ConsoleAbstraction.WriteDarkYellow(sb.ToString());
    }
}

void CallNextPhoneNumber()
{
    if (_conferenceStatus.PhoneNumbersInProgress.Count < config.MaxPhoneNumberToAddInFirstRow)
    {
        // Check if it's necessary to add another phone number
        var nextPhoneNumber = GetNextPhoneNumberToAdd();
        if (nextPhoneNumber is not null)
        {
            _conferenceStatus.PhoneNumbersInProgress.Add(nextPhoneNumber);

            ConsoleAbstraction.WriteGreen($"Adding Phone number [{nextPhoneNumber}] to the conf ...");

            RbConferences.AddParticipantByPhoneNumberAsync(_conferenceStatus.Bubble, nextPhoneNumber)
                .ContinueWith(async task =>
                {
                    var sdkResult = await task;
                    if (sdkResult.Success)
                    {
                        ConsoleAbstraction.WriteGreen($"Server has taken into account the action to add in conf [{nextPhoneNumber}].");
                    }
                    else
                    {
                        ConsoleAbstraction.WriteRed($"Phone number cannot be added to the conf - [{sdkResult.Result}]");
                        _conferenceStatus.PhoneNumbersInProgress.Remove(nextPhoneNumber);
                    }
                });
        }
        else
        {
            ConsoleAbstraction.WriteYellow("No more phone numbers to add in conf");
        }
    }
    else
        ConsoleAbstraction.WriteYellow("Wait to call another phone number - max contraint reached");
}

String? GetNextPhoneNumberToAdd()
{
    if(_conferenceStatus.PhoneNumbersToHaveInConf.Count > _conferenceStatus.PhoneNumbersAlreadyInConference.Count)
    {
        foreach(var phoneNumber in _conferenceStatus.PhoneNumbersToHaveInConf)
        {
            if ( (!_conferenceStatus.PhoneNumbersInProgress.Contains(phoneNumber)) &&
                (!_conferenceStatus.PhoneNumbersAlreadyInConference.Contains(phoneNumber)) )
            {
                return phoneNumber;
            }
        }
    }

    return null;
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
        LogConfigurator.Configure(exeSettings.LogFolderPath);
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
    credentials = Credentials.FromJsonNode(jsonNode?["credentials"]);
    if (credentials?.IsValid() != true)
    {
        ConsoleAbstraction.WriteRed($"Cannot read 'credentials' object OR invalid/missing data in file:[{fileName}].");
        return false;
    }

    return true;
}

Boolean ReadConfig()
{
    var configFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}config.json";
    if (!File.Exists(configFilePath))
    {
        ConsoleAbstraction.WriteRed($"The file '{configFilePath}' has not been found.");
        return false;
    }

    String jsonConfig = File.ReadAllText(configFilePath);
    var jsonNode = JSON.Parse(jsonConfig);

    config = Config.FromJsonNode(jsonNode);
    if (config?.IsValid() != true)
    {
        ConsoleAbstraction.WriteRed($"Cannot read 'config' object OR invalid/missing data in file:[{configFilePath}].");
        return false;
    }

    return true;
}

