using AdaptiveCards;
using Rainbow;
using Rainbow.Consts;
using Rainbow.Example.Common;
using Rainbow.Model;
using Rainbow.SimpleJSON;

// --------------------------------------------------

ExeSettings? exeSettings = null;
Credentials? credentials = null;
Configuration? configuration = null;

if ((!ReadExeSettings()) || (exeSettings is null))
    return;

if ((!ReadCredentials()) || (credentials is null))
    return;

if ((!ReadConfiguration()) || (configuration is null))
    return;
ConsoleAbstraction.WriteGreen($"Configuration is correct");

ConsoleAbstraction.WriteRed($"Rainbow Account used: [{credentials.UsersConfig[0].Login}]");

// --------------------------------------------------

// Define some objects
Rainbow.Model.Message? messageAC = null; // message sent as AC (Adaptive Card)
Boolean needToWaitAnswer = false;
CancelableDelay? cancelableDelay = null;

// --------------------------------------------------

String CR = Rainbow.Util.CR; // Get carriage return;

// In "exeSettings.json" using "logFolderPath" property, we defined a folder where the logs must be stored
String logFolderPath = exeSettings.LogFolderPath;

// In "credentials.json" using "userConfig" object, we defined a prefix used as logger prefix (this prefix permits to have logs stored in specific file for this "userConfig")
String? logPrefix = credentials.UsersConfig[0].Prefix;

Rainbow.Util.SetLogAnonymously(false);

Restrictions restrictions = new(false)
{
    UseBubbles = true,
    UseInstantMessaging = true,

    StoreMessages = true,
    AcceptBubbleInvitation = true,
    AcceptUserInvitation = true,

    LogRestRequest = true,
};

// Create Rainbow SDK objects
var RbApplication = new Application(iniFolderFullPathName: logFolderPath, iniFileName: logPrefix + ".ini", loggerPrefix: logPrefix, restrictions: restrictions);
var RbAutoReconnection = RbApplication.GetAutoReconnection();
var RbContacts = RbApplication.GetContacts();
var RbBubbles = RbApplication.GetBubbles();
var RbInstantMessaging = RbApplication.GetInstantMessaging();
var RbConversations = RbApplication.GetConversations();

// We want to receive events from SDK
RbApplication.AuthenticationFailed += RbApplication_AuthenticationFailed;       // Triggered when the authentication process will fail
RbApplication.AuthenticationSucceeded += RbApplication_AuthenticationSucceeded; // Triggered when the authentication process will succeed
RbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;   // Triggered when the Connection State will change

RbInstantMessaging.MessageReceived += RbInstantMessaging_MessageReceived;

// Set global configuration info
RbApplication.SetApplicationInfo(credentials.ServerConfig.AppId, credentials.ServerConfig.AppSecret);
RbApplication.SetHostInfo(credentials.ServerConfig.HostName);

ConsoleAbstraction.WriteGreen($"{CR}Use [ESC] at anytime to quit");
ConsoleAbstraction.WriteGreen($"{CR}Use [Q] at anytime once loggued to quit using Logout");
ConsoleAbstraction.WriteGreen($"{CR}Use [L] at anytime to reload confiuration.json and restart at the beginning");

// Start login
ConsoleAbstraction.WriteWhite($"{CR}Starting login ...");
var taskSdkResult = RbApplication.LoginAsync(credentials.UsersConfig[0].Login, credentials.UsersConfig[0].Password); // Here we don't wait the Login process before to continue

do
{
    if (taskSdkResult?.Result?.Success == true)
    {
        taskSdkResult = null;
        DisplayInputsInfo();
    }

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
                        // Nothing to do here - on success
                    }
                    else
                        ConsoleAbstraction.WriteRed($"LogoutAsync - Error:[{taskSdkResult.Result}]");
                }
                return;
        }
    }
}

void DisplayInputsInfo()
{
    ConsoleAbstraction.WriteGreen($"{CR}Use [I] to display this [I]nfo menu");

    ConsoleAbstraction.WriteGreen($"{CR}Use [C] to list [C]onversations");

    ConsoleAbstraction.WriteGreen($"{CR}Use [D] to [D]elete first Conversation from list");

    ConsoleAbstraction.WriteGreen($"{CR}Use [O] to [O]pen a Conversation previously deleted");

    ConsoleAbstraction.WriteGreen($"{CR}Use [S] to [S]end an IM to a Contact or a Bubble");

    ConsoleAbstraction.WriteGreen($"{CR}Use [M] to get older [M]essages exchanged with a Contact or a Bubble");
}

#region Events received from the SDK
async void RbApplication_ConnectionStateChanged(Rainbow.Model.ConnectionState connectionState)
{
    // Display the CurrentNbAttempts
    if (connectionState.Status == ConnectionStatus.Connecting)
        ConsoleAbstraction.WriteYellow($"{CR}AutoReconnection.CurrentNbAttempts: [{RbAutoReconnection.CurrentNbAttempts}]");

    // We log connection state in the console - use different color according the status
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

    if (connectionState.Status == ConnectionStatus.Connected)
    {
        // Get Contact / Bubble basd on the configuration
        configuration.Contact = await GetContactAsync(configuration.ContactConfiguerd);
        configuration.Bubble = await GetBubbleAsync(configuration.BubbleConfiguerd);

        if ( (configuration.Contact is null) && (configuration.Bubble is null))
        {
            ConsoleAbstraction.WriteRed($"Both Contact and Bubble information provided in \"configuration.json\" don't permit to get a real contact / bubble form server => Application is stopped");
            await RbApplication.LogoutAsync();
        }

        cancelableDelay = CancelableDelay.StartAfter(configuration.DelayBetweenAC, SendOrEditACAsync);
    }


    // If we are disconnected and the AutoReconnection is stopped, nothing more will happpen
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

void RbInstantMessaging_MessageReceived(Rainbow.Model.Message message, bool carbonCopy)
{
    // Check if message reeceived is sent by the contact configured
    if (message?.FromContact?.Peer?.Id != configuration.Contact.Peer.Id)
        return;


    // Check if message is sent in bubble configured
    Boolean bubbleContext = ((message.ToBubble is not null) && (configuration.Bubble is not null)
        && (message.ToBubble.Peer.Id == configuration.Bubble.Peer.Id));

    (Boolean isAdaptiveCardAnswer, JSONNode? answerNode) = ParseAsAdaptivCardResponse(message);

    if (isAdaptiveCardAnswer is true && answerNode is not null)
    {
        needToWaitAnswer = false;
        cancelableDelay?.PostPone();

        // Since it's an example her we do nothing abaout the content of the answer but you could have acces to the data
        // See how method ACHelper.CreateCallBackItem is used and defined
        String? btnId = answerNode["id"]; // here btnId == "reachAgain"
    }
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
    if (jsonNode is null)
    {
        ConsoleAbstraction.WriteRed($"Cannot parse 'credentials' is JSON - check file:[{fileName}].");
        return false;
    }

    credentials = Credentials.FromJsonNode(jsonNode["credentials"]);
    if (credentials?.IsValid() != true)
    {
        ConsoleAbstraction.WriteRed($"Cannot read 'credentials' object OR invalid/missing data in file:[{fileName}].");
        return false;
    }

    return true;
}

Boolean ReadConfiguration(string fileName = "configuration.json")
{
    var configContentJsonNode = (JSONNode?) GetFileContent("Reading configuration", fileName, true);
    if (configContentJsonNode is null) return false;
   
    configuration ??= new();
    configuration.Reset();

    // Get labesl path and check file as JSON
    configuration.Labels = (JSONNode?) GetFileContent("Reading configuration - reading \"labels\" node", configContentJsonNode["labels"], true);
    if (configuration.Labels is null) return false;

    configuration.DelayBetweenAC = configContentJsonNode["delayBetweenAC"];
    if (configuration.DelayBetweenAC == 0)
        configuration.DelayBetweenAC = 800;

    // Bubble info could not be filled
    configuration.BubbleConfiguerd = Account.FromJsonNode(configContentJsonNode["bubble"]);

    configuration.ContactConfiguerd = Account.FromJsonNode(configContentJsonNode["contact"]);
    if (configuration.ContactConfiguerd is null || (!configuration.ContactConfiguerd.IsValid()))
    {
        ConsoleAbstraction.WriteRed($"Reading configuration - reading \"contact\" node - missing data: id, jid or login");
        return false;
    }

    configuration.Trains = JSON.ToList(configContentJsonNode["trains"], Train.FromJsonNode);
    if (configuration.Trains is null || (configuration.Trains.Count == 0) )
    {
        ConsoleAbstraction.WriteRed($"Reading configuration - reading \"trains\" node - missing information");
        return false;
    }

    var listSteps = configContentJsonNode["steps"];
    if (listSteps is null || (!listSteps.IsArray))
    {
        ConsoleAbstraction.WriteRed($"Reading configuration - reading \"steps\" node - missing information (not an array)");
        return false;
    }

    // Read steps
    configuration.Steps = [];
    foreach (JSONNode jsonNode in listSteps)
    {
        List<String>? subSteps = jsonNode["list"];
        if (Predicates.IsNullOrEmpty(subSteps))
        {
            ConsoleAbstraction.WriteRed($"Reading configuration - reading \"steps\" node - reading \"steps\" node - missing value");
            return false;
        }

        List<List<TrainStatus>> trainStatusList = new();
        if (subSteps?.Count > 0)
        {
            foreach (var subStep in subSteps)
            {
                // Check if file exists
                var jsonNodeTrainStatus = (JSONNode?)GetFileContent($"Reading configuration - reading \"steps\" node - parsing file [{subStep}]", subStep, true);
                if (jsonNodeTrainStatus is null)
                    return false;

                List<TrainStatus>? trainStatus = JSON.ToList(jsonNodeTrainStatus["status"], TrainStatus.FromJsonNode);
                if (Predicates.IsNullOrEmpty(trainStatus))
                {
                    ConsoleAbstraction.WriteRed($"Reading configuration - reading \"steps\" node - parsing file [{subStep}] - no train status found");
                    return false;
                }

                foreach(var train in trainStatus)
                {
                    // Check Id
                    var result = configuration.Trains.Where(t => t.Id == train.Id).FirstOrDefault();
                    if (result is null)
                    {
                        ConsoleAbstraction.WriteRed($"Reading configuration - reading \"steps\" node - parsing file [{subStep}] - no train defined with this id:[{train.Id}]");
                        return false;
                    }

                    // Check status
                    if (!configuration.Labels.HasKey(train.Status ?? ""))
                    {
                        ConsoleAbstraction.WriteRed($"Reading configuration - reading \"steps\" node - parsing file [{subStep}] - no label found using :[{train.Status}]");
                        return false;
                    }
                }

                trainStatusList.Add(trainStatus);
            }
        }
        if (!Predicates.IsNullOrEmpty(trainStatusList))
            configuration.Steps.Add(trainStatusList);
    }

    if (configuration.Steps.Count == 0)
    {
        ConsoleAbstraction.WriteRed($"Reading configuration - reading \"steps\" node - missing information (sub steps not defined correctly)");
        return false;
    }

    return true;
}

Object? GetFileContent(String context, String? fileName, Boolean asJson)
{
    if(String.IsNullOrEmpty(fileName))
    {
        ConsoleAbstraction.WriteRed($"{context} - File name provided is null/empty ...");
        return null;
    }
    var filePath = GetFilePath(fileName);
    if (!File.Exists(filePath))
    {
        ConsoleAbstraction.WriteRed($"{context} - Using [{fileName}] no file exists");
        return null;
    }

    String content = File.ReadAllText(filePath);
    if (asJson)
    {
        var jsonNode = JSON.Parse(content);
        if (jsonNode is null)
        {
            ConsoleAbstraction.WriteRed($"{context} - Using [{fileName}] cannot parse as JSON object");
            return null;
        }
        return jsonNode;
    }
    return content;
}

String GetFilePath(String fileName) => $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}{fileName}";

String GetLogFilePath(String fileName) => $".{Path.DirectorySeparatorChar}logs{Path.DirectorySeparatorChar}{fileName}";

void IncrementStep()
{
    // Increase subStep
    configuration.CurrentSubStep++;

    if (configuration.CurrentStep >= configuration.Steps.Count)
        configuration.CurrentStep = 0;

    if (configuration.CurrentSubStep >= configuration.Steps[configuration.CurrentStep].Count)
    {
        configuration.CurrentSubStep = 0;

        configuration.CurrentStep++;
        if (configuration.CurrentStep >= configuration.Steps.Count)
            configuration.CurrentStep = 0;
    }
}

(String? message, List<MessageAlternativeContent>? alternativeContent) CreateAdaptiveCard()
{
    String? message = null;
    List<MessageAlternativeContent>? alternativeContent = null;

    try
    {
        // Get train status
        var trainStatus = configuration.Steps[configuration.CurrentStep][configuration.CurrentSubStep];


        String? resultAC = null;
        var jsonNodeRoot = ACHelper.CreateRoot();
        if (jsonNodeRoot is not null)
        {
            var jsonNodeTitle = ACHelper.CreateTitle(configuration.Labels["title"]);
            ACHelper.AddItemToRoot(jsonNodeRoot, jsonNodeTitle);

            var trainNotActive = trainStatus.Where(t => t.Status != "callActive").FirstOrDefault();
            if (trainNotActive is not null)
            {
                var jsonNodeSubTitle = ACHelper.CreateSubTitle(configuration.Labels["statusWarning"], "Attention");
                ACHelper.AddItemToRoot(jsonNodeRoot, jsonNodeSubTitle);
            }

            var jsonNodeColumnSet = ACHelper.CreateColumnSet();
            if (jsonNodeColumnSet is not null)
            {
                // /!\ Here we assume trains are always in correct order ....

                // Create column with train name
                List<(String text, String color)> data = [];
                foreach (var train in configuration.Trains)
                {
                    data.Add((train.Name, ""));
                }
                ACHelper.AddColumnToColumnSet(jsonNodeColumnSet, ACHelper.CreateColumn(configuration.Labels["train"], data));


                // Create column with train phone number
                data.Clear();
                foreach (var train in configuration.Trains)
                {
                    data.Add((train.PhoneNumber, ""));
                }
                ACHelper.AddColumnToColumnSet(jsonNodeColumnSet, ACHelper.CreateColumn(configuration.Labels["phoneNumber"], data));

                // Create column with train status
                data.Clear();
                foreach (var train in trainStatus)
                {
                    String? name = configuration.Labels[train.Status];
                    String? color = configuration.Labels["colors"][train.Status];
                    data.Add((name, color));
                }
                ACHelper.AddColumnToColumnSet(jsonNodeColumnSet, ACHelper.CreateColumn("Status", data));

                // Now add ColumnSet to root
                ACHelper.AddItemToRoot(jsonNodeRoot, jsonNodeColumnSet);
            }

            // Do we have a taing which has hang up ?
            var trainHangUp = trainStatus.Where(t => t.Status == "callHangUp").FirstOrDefault();
            if (trainHangUp is not null)
            {
                // Here we create a button

                var jsonNodeCallBackItem = ACHelper.CreateCallBackItem(configuration.Labels["callBackTitle"], configuration.Labels["callBack"], "reachAgain");
                ACHelper.AddItemToRoot(jsonNodeRoot, jsonNodeCallBackItem);
            }

            if (trainNotActive is null)
            {
                var jsonNodeSubTitle = ACHelper.CreateSubTitle(configuration.Labels["statusOk"], "Accent");
                ACHelper.AddItemToRoot(jsonNodeRoot, jsonNodeSubTitle);
            }

            // For debug save AC in file indented
            //var filePath = GetLogFilePath($"AC-{configuration.CurrentStep:00}-{configuration.CurrentSubStep:00}.json");
            //File.WriteAllText(filePath, jsonNodeRoot.ToString(indent: true), System.Text.Encoding.UTF8);

            // Get JSON
            resultAC = jsonNodeRoot.ToString(indent: false);

            // Create a Message Alternative Content
            MessageAlternativeContent messageAlternativeContent = new()
            {
                Type = "form/json",
                Content = resultAC
            };
            alternativeContent = [messageAlternativeContent];

            message = configuration.Labels["title"];
        }
    }
    catch(Exception ex)
    {
        ConsoleAbstraction.WriteRed($"[CreateAdaptiveCard] Exception:[{ex}]");
    }

    return (message, alternativeContent);

}

async Task<Contact?> GetContactAsync(Account? account)
{
    if ((RbContacts is null) || (account is null))
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

    if (!String.IsNullOrEmpty(account.Login))
        result = RbContacts.GetAllContacts()?.Find(contact => contact?.LoginEmail?.Equals(account.Login, StringComparison.InvariantCultureIgnoreCase) == true);

    return result;
}

async Task<Bubble?> GetBubbleAsync(Account? accountasBubble)
{
    if ((RbBubbles is null) || (accountasBubble is null))
        return null;

    Bubble? result = null;
    result ??= await RbBubbles.GetBubbleByIdInCacheFirstAsync(accountasBubble.Id);
    result ??= await RbBubbles.GetBubbleByJidInCacheFirstAsync(accountasBubble.Jid );
    if (result is null && !String.IsNullOrEmpty(accountasBubble.Name))
        result = RbBubbles.GetAllBubbles()?.Find(bubble => bubble?.Peer?.DisplayName.Equals(accountasBubble.Name, StringComparison.InvariantCultureIgnoreCase) == true);

    return result;
}

(Boolean isAdaptiveCardAnswer, JSONNode? answerNode) ParseAsAdaptivCardResponse(Rainbow.Model.Message message)
{
    Boolean isAdaptiveCardAnswer = false;
    JSONNode? answerNode = null;
    if (message.AlternativeContent != null)
    {
        foreach (MessageAlternativeContent alternativeContent in message.AlternativeContent)
        {
            if (alternativeContent.Type == "rainbow/json")
            {
                isAdaptiveCardAnswer = true;
                var content = alternativeContent.Content;

                answerNode = JSON.Parse(content);
                break;
            }
        }
    }
    return (isAdaptiveCardAnswer, answerNode);
}

async Task<SdkResult<Boolean>> SendOrEditACAsync()
{
    if (needToWaitAnswer) return new SdkResult<Boolean>(false);

    if (!RbApplication.IsConnected()) return new SdkResult<Boolean>(false);

    (String? message, List<MessageAlternativeContent>? alternativeContent) = CreateAdaptiveCard();
    if(Predicates.IsNullOrEmpty(alternativeContent))
        return new SdkResult<Boolean>(SdkInternalError.FromInternalError(Rainbow.Enums.SdkInternalErrorEnum.OPERATION_FAILED, ["Cannot create alternative content", nameof(SendOrEditACAsync)]));

    Peer? peerToSendAC;
    peerToSendAC = configuration.Bubble;
    peerToSendAC ??= configuration.Contact;

    SdkResult<Message> sdkResult;
    if (messageAC is null)
        sdkResult = await RbInstantMessaging.SendMessageWithAlternativeContentsAsync(peerToSendAC, message, alternativeContent);
    else
        sdkResult = await RbInstantMessaging.EditMessageAsync(messageAC, message, alternativeContent);

    if (sdkResult.Success)
    {
        // Store message sent with AC
        messageAC = sdkResult.Data;

        var currentStep = configuration.CurrentStep;
        IncrementStep();
        needToWaitAnswer = (currentStep != configuration.CurrentStep);


        // Check if we reach the end of the steps

        if ( (configuration.CurrentStep == 0) && (configuration.CurrentSubStep == 0) )
        {
            ConsoleAbstraction.WriteGreen("We reach the end of steps. We log out ...");
            RbApplication.LogoutAsync().StartAndForget();
        }
        else
            cancelableDelay?.PostPone();
        return new SdkResult<Boolean>(true);
    }
    else
        return new SdkResult<Boolean>(sdkResult.Result);

}