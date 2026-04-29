using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;

using Rainbow.Example.Common;

using Rainbow.SimpleJSON;
using System.Text;
using Rainbow.Enums;

// --------------------------------------------------

ExeSettings? exeSettings = null;
Credentials? credentials = null;

if ((!ReadExeSettings()) || (exeSettings is null))
    return;

if ((!ReadCredentials()) || (credentials is null))
    return;

ConsoleAbstraction.WriteRed($"Account used: [{credentials.UsersConfig[0].Login}]");

// --------------------------------------------------

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

Boolean saveAllMessages = true;

Restrictions restrictions = new(true)
{
    StoreMessages = false, // Set to true or false to test use of cache in SDK

    LogRestRequest = true,
    LogEvent = true
};

// Create Rainbow SDK objects
var RbApplication = new Application(iniFolderFullPathName: logFolderPath, iniFileName: logPrefix+".ini", loggerPrefix: logPrefix, restrictions: restrictions);
var RbAutoReconnection = RbApplication.GetAutoReconnection();
var RbBubbles = RbApplication.GetBubbles();
var RbContacts = RbApplication.GetContacts();
var RbInstantMessaging = RbApplication.GetInstantMessaging();

// We want to receive events from SDK
RbApplication.AuthenticationFailed += RbApplication_AuthenticationFailed;       // Triggered when the authentication process will fail
RbApplication.AuthenticationSucceeded += RbApplication_AuthenticationSucceeded; // Triggered when the authentication process will succeed
RbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;   // Triggered when the Connection State will change

RbAutoReconnection.Cancelled += RbAutoReconnection_Cancelled;                       // Triggered when AutoReonnection service is cancelled
RbAutoReconnection.Started += RbAutoReconnection_Started;                           // Triggered when AutoReonnection service is started
RbAutoReconnection.MaxNbAttemptsReached += RbAutoReconnection_MaxNbAttemptsReached; // Triggered when AutoReonnection service reachde the mawimun number of attempts tryig to connect to the server
RbAutoReconnection.TokenExpired += RbAutoReconnection_TokenExpired;                 // Triggered when the Security Token is expired

RbContacts.ContactsAdded += RbContacts_ContactsAdded;

// Set global configuration info
RbApplication.SetApplicationInfo(credentials.ServerConfig.AppId, credentials.ServerConfig.AppSecret);
RbApplication.SetHostInfo(credentials.ServerConfig.HostName);

// MaxNbAttempts: By default it's 50. We reduce here the number so it will take less time to reach the limit.
// The goal here is to understand how events are triggered by the AutoReconnection service
RbAutoReconnection.MaxNbAttempts = 10; 

ConsoleAbstraction.WriteGreen($"{CR}Use [ESC] at anytime to quit");

ConsoleAbstraction.WriteGreen($"{CR}Use [Q] at anytime once loggued to quit using Logout");

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
                        // Nothing to do here - on success, event Cancelled from AutoReconnexion is triggered
                    }
                    else
                        ConsoleAbstraction.WriteRed($"LogoutAsync - Error:[{taskSdkResult.Result}]");
                }
                return;

            case ConsoleKey.C:
                var contact = SearchForContactsInRoster();
                if (contact != null)
                    FetchAllMessagesFromPeer(contact);
                else
                    DisplayInputsInfo();
                break;

            case ConsoleKey.B:
                var bubble = SearchForBubbles();
                if (bubble != null)
                    FetchAllMessagesFromPeer(bubble);
                else
                    DisplayInputsInfo();
                break;

        }
    }
}

void DisplayInputsInfo()
{
    ConsoleAbstraction.WriteGreen($"{CR}Use [C] to search a Contact and retrieve messages exchanged with him in Peer to Peer");

    ConsoleAbstraction.WriteGreen($"{CR}Use [B] to search a Bubble and retrieve message exchanged in it");
}

Contact? SearchForContactsInRoster()
{
    Contact? result = null;
    ConsoleAbstraction.WriteYellow($"{CR}Enter a text to search a contact from you Roster: ");
    var str = ConsoleAbstraction.ReadLine();
    if (str != null)
    {
        var contactsList = RbContacts.GetAllContactsInRoster();
        result = contactsList.Find(contact => contact.Peer.DisplayName.Contains(str, StringComparison.InvariantCultureIgnoreCase) && contact.Peer.Id != RbContacts.GetCurrentContact().Peer.Id);
    }

    if(result == null)
        ConsoleAbstraction.WriteRed($"{CR}No contact found with [{str}]");
    else
        ConsoleAbstraction.WriteWhite($"Contact found:[{result.ToString(DetailsLevel.Small)}]");

    return result;
}

Bubble? SearchForBubbles()
{
    Bubble? result = null;
    ConsoleAbstraction.WriteYellow($"{CR}Enter a text to search a Bubble:");
    var str = ConsoleAbstraction.ReadLine();
    if (str != null)
    {
        List<String> memberStatus = new() { BubbleMemberStatus.Accepted };
        var bubblesList = RbBubbles.GetAllBubbles(bubbleMemberStatusList: memberStatus);
        result = bubblesList.Find(contact => contact.Peer.DisplayName.Contains(str, StringComparison.InvariantCultureIgnoreCase));
    }

    if (result == null)
        ConsoleAbstraction.WriteRed($"{CR}No bubble found with [{str}]");
    else
        ConsoleAbstraction.WriteWhite($"bubble found:[{result.ToString(DetailsLevel.Small)}]");

    return result;
}

void FetchAllMessagesFromPeer(Peer peer)
{
    if (peer == null)
        return;

    Task.Run(async () =>
    {
        try
        {
            int nbMessagesByRow = 250;

            SdkResult<List<Rainbow.Model.Message>> mamSdkResult;
            Boolean canContinue = true;
            Boolean error = false;
            int messagesCount = 0;

            ConsoleAbstraction.WriteBlue($"Start to fetch all messages from Peer:[{RbApplication.GetPeerDisplayName(peer)}]");

            DateTime start = DateTime.Now;
            DateTime startMsgRow = start;
            DateTime endMsgRow;
            TimeSpan elapsed;

            Dictionary<String, Rainbow.Model.Message> messages = [];
            Rainbow.Model.Message? lastMessage = null;
            do
            {
                startMsgRow = DateTime.Now;
                mamSdkResult = await RbInstantMessaging.GetOlderMessagesAsync(peer, nbMessagesByRow, lastMessage);
                if (mamSdkResult.Success)
                {
                    if (mamSdkResult.Data.Count > 0)
                    {
                        var dicToAdd = mamSdkResult.Data.ToDictionary(m => m.Id, m => m);
                        if (messages.Count == 0)
                            messages = dicToAdd;
                        else
                            // We add only message with new keys
                            // If they have same keys, it means it's message which has been updated. So older messages must be removed.
                            AddRangeNewOnly(messages, dicToAdd);

                        lastMessage = messages.Values.Last();

                        //ConsoleAbstraction.WriteYellow($"LastMessage - Id:[{lastMessage.Id}] - DateKind:[{lastMessage.Date.Kind}] - Date:[{lastMessage.Date:o}] - Epoch:[{Rainbow.Util.DateTimeToUnixTimeStamp(lastMessage.Date, false) * 1000}]");

                        endMsgRow = DateTime.Now;
                        elapsed = endMsgRow - startMsgRow;
                        messagesCount += mamSdkResult.Data.Count;
                        ConsoleAbstraction.WriteYellow($"Retrieved: {mamSdkResult.Data.Count} - Time elapsed:[{elapsed.TotalSeconds} sec] - Count:[{messagesCount}] - CountStored:[{messages.Count}]");// - lastStanzaId:[{lastStanzaId}]");
                    }
                    else
                        canContinue = false;
                }
                else
                {
                    ConsoleAbstraction.WriteRed($"SdkError:{mamSdkResult.Result}");
                    canContinue = false;
                    error = true;
                }
            }
            while (canContinue);

            if (!error)
            {
                // Here messages object contains all messages exchanged with the Peer
                elapsed = DateTime.Now - start;

                if (messagesCount != 0)
                    ConsoleAbstraction.WriteBlue($"Nb Messages:[{messagesCount}] - Time elapsed:[{elapsed.TotalSeconds} sec]");
                else
                    ConsoleAbstraction.WriteBlue($"We have already get all messages from this Peer - Nb Messages:[{messages.Count()}] - Time elapsed:[{elapsed.TotalSeconds} sec]");

                if (saveAllMessages)
                {
                    var fileNameAllMessages = Path.Combine(Directory.GetCurrentDirectory(), $"Messages-ALL-{peer.DisplayName}-{messages.Count}{(RbApplication.Restrictions().StoreMessages ? "-usingCache" : "-noCache")}.txt");

                    ConsoleAbstraction.WriteDarkYellow($"Saving all messages in the file:[{Path.GetFullPath(fileNameAllMessages)}] ...");

                    StringBuilder stringBuilder = new StringBuilder();

                    // Delete previous one if any
                    File.Delete(fileNameAllMessages);
                    int i = 0;
                    foreach (var message in messages.Values.ToList())
                    {
                        AppendMessage(ref stringBuilder, message);

                        i++;
                        // Store in file every 500 messages
                        if ((i != 0) && (i % 500 == 0))
                        {
                            File.AppendAllText(fileNameAllMessages, stringBuilder.ToString(), Encoding.UTF8);
                            stringBuilder.Clear();
                            ConsoleAbstraction.WriteBlue($"500 Messages stored in file");
                        }
                    }
                    File.AppendAllText(fileNameAllMessages, stringBuilder.ToString(), Encoding.UTF8);
                    ConsoleAbstraction.WriteBlue($"All messages have been stored");
                }
                else
                {
                    // Log first and last messages
                    int nbMessagesScope = 100;

                    if (messages.Count > nbMessagesScope)
                    {
                        var fileNameFirstMessages = Path.Combine(Directory.GetCurrentDirectory(), $"Messages-First-{peer.DisplayName}-{nbMessagesScope}{(RbApplication.Restrictions().StoreMessages ? "-usingCache" : "-noCache")}.txt");
                        StringBuilder stringBuilder = new();

                        // Delete previous one if any
                        File.Delete(fileNameFirstMessages);
                        foreach (var message in messages.Values.Take(nbMessagesScope))
                        {
                            AppendMessage(ref stringBuilder, message);
                        }
                        File.AppendAllText(fileNameFirstMessages, stringBuilder.ToString(), Encoding.UTF8);

                        stringBuilder.Clear();
                        var fileNameLastMessages = Path.Combine(Directory.GetCurrentDirectory(), $"Messages-Last-{peer.DisplayName}-{nbMessagesScope}{(RbApplication.Restrictions().StoreMessages ? "-usingCache" : "-noCache")}.txt");
                        // Delete previous one if any
                        File.Delete(fileNameLastMessages);
                        foreach (var message in messages.Values.TakeLast(nbMessagesScope))
                        {
                            AppendMessage(ref stringBuilder, message);
                        }
                        File.AppendAllText(fileNameLastMessages, stringBuilder.ToString(), Encoding.UTF8);
                    }
                }

            }
        }
        catch (Exception e)
        {

        }

        DisplayInputsInfo();

    });
}
void AddRangeNewOnly<TKey, TValue>(IDictionary<TKey, TValue> dic, IDictionary<TKey, TValue> dicToAdd)
{
    foreach (var kvp in dicToAdd)
    {
        if (!dic.ContainsKey(kvp.Key))
            dic.Add(kvp.Key, kvp.Value);
    }
}

void AppendMessage(ref StringBuilder stringBuilder, Rainbow.Model.Message message)
{
    if (message == null)
        return;

    String bubbleEvent;
    if (message.BubbleEvent == "conferenceStarted")
        bubbleEvent = "conferenceAdd";
    else if (message.BubbleEvent == "conferenceEnded")
        bubbleEvent = "conferenceRemove";
    else
        bubbleEvent = message.BubbleEvent;

    // DON'T STORE bubbleEvent (used to compare S2S vs XMPP)
    bubbleEvent = "";

    var content = message.Content;
    if (content != null)
    {
        content = content.Replace("\r", "").Replace("\n", "");
        if (content?.Length > 50)
            content = content.Substring(0, 50);
    }
    stringBuilder.Append(message.Date.ToString(Rainbow.SimpleJSON.JSON.DATE_TIME_FORMAT_24));
    stringBuilder.Append(" - Id:[");
    stringBuilder.Append(String.Format("{0,-50}", message.Id));

    stringBuilder.Append("] - FromJid:[");
    stringBuilder.Append(String.Format("{0,-50}", message.FromContact?.Peer.Jid));

    stringBuilder.Append("] - ToJid:[");
    if (message.ToBubble != null)
        stringBuilder.Append(String.Format("{0,-50}", message.ToBubble.Peer.Jid));
    else
        stringBuilder.Append(String.Format("{0,-50}", message.ToContact?.Peer.Jid));

    stringBuilder.Append("] - Urgency:[");
    stringBuilder.Append(String.Format("{0,-6}", message.Urgency.ToFriendlyString()));

    stringBuilder.Append("] - AttachmentId:[");
    stringBuilder.Append(String.Format("{0,-50}", (message.FileAttachment is null) ? "" : message.FileAttachment.Id));

    stringBuilder.Append("] - Content:[");
    stringBuilder.Append(String.Format("{0,-50}", content));

    stringBuilder.Append("] - Deleted:[");
    stringBuilder.Append(message.Deleted ? "X" : " ");

    stringBuilder.Append("] - Modified:[");
    stringBuilder.Append(message.Modified ? "X" : " ");

    stringBuilder.Append("] - Forwarded:[");
    stringBuilder.Append(message.IsForwarded ? "X" : " ");

    stringBuilder.Append("] - Event:[");
    stringBuilder.Append(bubbleEvent);
    stringBuilder.Append("]\r\n");
}

String UrgencyToString(MessageUrgencyType type)
{
    if (type == MessageUrgencyType.Std)
        return "";
    return type.ToString();
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

void RbContacts_ContactsAdded(IEnumerable<Rainbow.Model.Contact> contacts)
{
    List<String> displayNames = new();
    foreach (var contact in contacts)
    {
        displayNames.Add(contact.ToString(DetailsLevel.Small));
    }

    ConsoleAbstraction.WriteBlue($"{Rainbow.Util.CR}Event ContactsAdded triggered - Nb:[{displayNames.Count}] - Contact(s):[{String.Join(", ", displayNames)}]{Rainbow.Util.CR}");
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

    var credentials = Credentials.FromJsonNode(jsonNode["credentials"]);
    if (credentials?.IsValid() != true)
    {
        ConsoleAbstraction.WriteRed($"Cannot read 'credentials' object OR invalid/missing data in file:[{fileName}].");
        return false;
    }

    return true;
}