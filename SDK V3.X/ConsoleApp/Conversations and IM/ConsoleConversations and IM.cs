using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;


using Rainbow.SimpleJSON;
using System.Text;
using Rainbow.Enums;
using Rainbow.Example.Common;

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

Conversation? lastConversationDeleted = null;

Restrictions restrictions = new(true)
{
    StoreMessages = true,

    LogRestRequest = true,
    LogEvent = true,
    LogEventParameters = true,
};

// Create Rainbow SDK objects
var RbApplication = new Application(iniFolderFullPathName: logFolderPath, iniFileName: logPrefix + ".ini", loggerPrefix: logPrefix, restrictions: restrictions);
var RbAutoReconnection = RbApplication.GetAutoReconnection();
var RbBubbles = RbApplication.GetBubbles();
var RbContacts = RbApplication.GetContacts();
var RbInstantMessaging = RbApplication.GetInstantMessaging();
var RbConversations = RbApplication.GetConversations();

// We want to receive events from SDK
RbApplication.AuthenticationFailed += RbApplication_AuthenticationFailed;       // Triggered when the authentication process will fail
RbApplication.AuthenticationSucceeded += RbApplication_AuthenticationSucceeded; // Triggered when the authentication process will succeed
RbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;   // Triggered when the Connection State will change

RbAutoReconnection.Cancelled += RbAutoReconnection_Cancelled;                       // Triggered when AutoReonnection service is cancelled
RbAutoReconnection.Started += RbAutoReconnection_Started;                           // Triggered when AutoReonnection service is started
RbAutoReconnection.MaxNbAttemptsReached += RbAutoReconnection_MaxNbAttemptsReached; // Triggered when AutoReonnection service reachde the mawimun number of attempts tryig to connect to the server
RbAutoReconnection.TokenExpired += RbAutoReconnection_TokenExpired;                 // Triggered when the Security Token is expired

RbContacts.ContactsAdded += RbContacts_ContactsAdded;

RbInstantMessaging.MessageReceived += RbInstantMessaging_MessageReceived;
RbInstantMessaging.ReceiptReceived += RbInstantMessaging_ReceiptReceived;
RbInstantMessaging.UserTypingChanged += RbInstantMessaging_UserTypingChanged;

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
                ListConversations();
                break;

            case ConsoleKey.D:
                var _del = DeleteFirstConversationAsync();
                break;

            case ConsoleKey.O:
                var _open = OpenConversationDeletedAsync();
                break;

            case ConsoleKey.S:
                await SendIMAsync();
                break;

            case ConsoleKey.M:
                await GetOlderMessagesAsync();
                break;

            case ConsoleKey.I:
                DisplayInputsInfo();
                break;
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

async Task<Boolean> OpenConversationDeletedAsync()
{
    if(lastConversationDeleted == null)
    {
        ConsoleAbstraction.WriteRed($"{CR}No conversation previously deleted");
        return false;
    }
    else
    {
        var sdkResult = await RbConversations.GetOrCreateConversationFromPeerAsync(lastConversationDeleted);
        if (sdkResult.Success)
        {
            var conversation = sdkResult.Data;
            ConsoleAbstraction.WriteDarkYellow($"{CR}Conversation opened:[{conversation.ToString(DetailsLevel.Medium)}]");
            lastConversationDeleted = null;
            return true;
        }
        else
        {
            ConsoleAbstraction.WriteRed($"{CR}OpenConversationDeletedAsync() - Error occured:{sdkResult.Result}");
            return false;
        }
    }
}

async Task<Boolean> DeleteFirstConversationAsync()
{
    var list = RbConversations.GetAllConversations();
    if (list?.Count > 0)
    {
        var conversation = list.First();
        var sdkResult = await RbConversations.RemoveFromConversationsAsync(conversation);
        if (sdkResult.Success)
        {
            lastConversationDeleted = sdkResult.Data;
            ConsoleAbstraction.WriteDarkYellow($"{CR}Conversation deleted:[{lastConversationDeleted.ToString(DetailsLevel.Medium)}]");
            return true;
        }

        ConsoleAbstraction.WriteRed($"{CR}DeleteFirstConversationAsync() - Error occured:{sdkResult.Result}");
        return false;
    }
    else
    {
        ConsoleAbstraction.WriteDarkYellow($"{CR}No conversations already exists");
        return false;
    }
}

void ListConversations()
{
    var list = RbConversations.GetAllConversations();
    if(list?.Count > 0)
    {
        ConsoleAbstraction.WriteDarkYellow($"{CR}Nb conversations:[{list.Count}]");
        var index = 1;
        foreach (var conversation in list)
        {
            ConsoleAbstraction.WriteDarkYellow($"{CR}Conversation {index++:00}:{conversation.ToString(DetailsLevel.Medium)}]");
        }
    }
    else
        ConsoleAbstraction.WriteDarkYellow($"{CR}No conversations already exists");
}

async Task GetOlderMessagesAsync()
{
    Boolean canContinue = true;
    Boolean displayInfo = true;
    while (canContinue)
    {
        if (displayInfo)
        {
            displayInfo = false;
            ConsoleAbstraction.WriteGreen($"{CR}Get last message from:");
            ConsoleAbstraction.WriteGreen($"\t - [U] User");
            ConsoleAbstraction.WriteGreen($"\t - [B] Bubble");
            ConsoleAbstraction.WriteGreen($"\t - [C] to cancel");
        }

        while (ConsoleAbstraction.KeyAvailable)
        {
            var userInput = ConsoleAbstraction.ReadKey();
            switch (userInput?.Key)
            {
                case ConsoleKey.U:
                    var contact = SearchForUserInRoster();
                    if (contact is not null)
                        await GetOlderMessagesWithPeerAsync(contact);
                    displayInfo = true;
                    break;

                case ConsoleKey.B:
                    var bubble = SearchForBubbles();
                    if (bubble is not null)
                        await GetOlderMessagesWithPeerAsync(bubble);
                    displayInfo = true;
                    break;

                case ConsoleKey.C:
                    canContinue = false;
                    break;

                default:
                    ConsoleAbstraction.WriteYellow($"Invalid key ...");
                    displayInfo = true;
                    break;
            }
        }
    }

    DisplayInputsInfo();
}

async Task GetOlderMessagesWithPeerAsync(Peer peer)
{
    // /!\ According value of Restrictions.StoreMessage, if you want to retrieve step by step older messages by peer you have
    // - to store the last message exahnge with it
    // - and use it when calling GetOlderMessagesAsync(...) method
    // For a full example check the dedicate example to fetch all messages from a peer

    var sdkResultMessages = await RbInstantMessaging.GetOlderMessagesAsync(peer, 5);
    if(sdkResultMessages.Success)
    {
        var messages = sdkResultMessages.Data;

        if (messages.Count > 0)
        {
            StringBuilder stringBuilder = new();
            int index = 0;
            foreach (var message in messages)
            {
                stringBuilder.Append($"{CR}Message [{index++:00}]:");
                AppendMessage(ref stringBuilder, message, $"{CR}\t");
            }

            ConsoleAbstraction.WriteDarkYellow($"{CR}Older messages with Peer:[{peer.ToString(DetailsLevel.Small)}]:");
            ConsoleAbstraction.WriteDarkYellow($"{CR}{stringBuilder}");
        }
        else
            ConsoleAbstraction.WriteDarkYellow($"{CR}No more older messages to retrieve with Peer:[{peer.ToString(DetailsLevel.Small)}]:");
    }
    else
    {
        ConsoleAbstraction.WriteBlue($"{CR}Cannot get older messages with [{peer.ToString(DetailsLevel.Small)}]");
    }
}

async Task SendIMAsync()
{
    Boolean canContinue = true;
    Boolean displayInfo = true;
    while (canContinue)
    {
        if (displayInfo)
        {
            displayInfo = false;
            ConsoleAbstraction.WriteGreen($"{CR}Send an IM To:");
            ConsoleAbstraction.WriteGreen($"\t - [U] User");
            ConsoleAbstraction.WriteGreen($"\t - [B] Bubble");
            ConsoleAbstraction.WriteGreen($"\t - [C] to cancel");
        }

        while (ConsoleAbstraction.KeyAvailable && !displayInfo)
        {
            var userInput = ConsoleAbstraction.ReadKey();
            switch (userInput?.Key)
            {
                case ConsoleKey.U:
                    var contact = SearchForUserInRoster();
                    if (contact is not null)
                        await SendImToPeerAsync(contact);
                    displayInfo = true;
                    break;

                case ConsoleKey.B:
                    var bubble = SearchForBubbles();
                    if (bubble is not null)
                        await SendImToPeerAsync(bubble);
                    displayInfo = true;
                    break;

                case ConsoleKey.C:
                    canContinue = false;
                    break;

                default:
                    ConsoleAbstraction.WriteYellow($"Invalid key ...");
                    displayInfo = true;
                    break;
            }
        }
    }

    DisplayInputsInfo();
}

async Task SendImToPeerAsync(Peer peer)
{
    ConsoleAbstraction.WriteYellow($"{CR}Enter a text to send it to [{peer.ToString(DetailsLevel.Small)}]: ");
    var str = ConsoleAbstraction.ReadLine();
    if (str != null)
    {
        var sdkResult = await RbInstantMessaging.SendMessageAsync(peer, str);
        if(sdkResult.Success)
            ConsoleAbstraction.WriteBlue($"{CR}IM sent to [{peer.ToString(DetailsLevel.Small)}]");
        else
            ConsoleAbstraction.WriteRed($"{CR}Error sending IM: [{sdkResult.Result}]");
    }
    else
        ConsoleAbstraction.WriteRed($"No text specified - No IM sent");
}

Contact? SearchForUserInRoster()
{
    Contact? result = null;
    ConsoleAbstraction.WriteYellow($"{CR}Enter a text to search a user from you Roster: ");
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

void AppendMessage(ref StringBuilder stringBuilder, Rainbow.Model.Message message, String separator = " - ")
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
    stringBuilder.Append($"{separator}Date:[{message.Date.ToString(Rainbow.SimpleJSON.JSON.DATE_TIME_FORMAT_24)}");
    stringBuilder.Append($"]{separator}Id:[");
    stringBuilder.Append(String.Format("{0,-50}", message.Id));

    stringBuilder.Append($"]{separator}FromJid:[");
    stringBuilder.Append(String.Format("{0,-50}", message.FromContact?.Peer.Jid));

    stringBuilder.Append($"]{separator}ToJid:[");
    if (message.ToBubble != null)
        stringBuilder.Append(String.Format("{0,-50}", message.ToBubble.Peer.Jid));
    else
        stringBuilder.Append(String.Format("{0,-50}", message.ToContact?.Peer.Jid));

    stringBuilder.Append($"]{separator}Urgency:[");
    stringBuilder.Append(String.Format("{0,-6}", message.Urgency.ToFriendlyString()));

    stringBuilder.Append($"]{separator}AttachmentId:[");
    stringBuilder.Append(String.Format("{0,-50}", (message.FileAttachment is null) ? "" : message.FileAttachment.Id));

    stringBuilder.Append($"]{separator}Content:[");
    stringBuilder.Append(String.Format("{0,-50}", content));

    stringBuilder.Append($"]{separator}Deleted:[");
    stringBuilder.Append(message.Deleted ? "X" : " ");

    stringBuilder.Append($"]{separator}Modified:[");
    stringBuilder.Append(message.Modified ? "X" : " ");

    stringBuilder.Append($"]{separator}Forwarded:[");
    stringBuilder.Append(message.IsForwarded ? "X" : " ");

    stringBuilder.Append($"]{separator}Event:[");
    stringBuilder.Append(bubbleEvent);
    stringBuilder.Append("]\r\n");
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

void RbInstantMessaging_MessageReceived(Message message, bool carbonCopy)
{
    if(message.ToBubble is null)
    {
        // Message has NOT be sent to a Bubble
        ConsoleAbstraction.WriteBlue($"{CR}Message received: sent by [{message.FromContact.ToString(DetailsLevel.Small)}] to you:");
    }
    else
    {
        // Message has been sent to a Bubble
        ConsoleAbstraction.WriteBlue($"{CR}Message received: sent by [{message.FromContact.ToString(DetailsLevel.Small)}] in Bubble [{message.ToBubble.ToString(DetailsLevel.Small)}]:");
    }
    StringBuilder stringBuilder = new();
    AppendMessage(ref stringBuilder, message, $"{CR}\t");
    ConsoleAbstraction.WriteDarkYellow($"{CR}{stringBuilder}");
}

void RbInstantMessaging_ReceiptReceived(Peer peerContext, Message message, MessageReceiptType receiptType)
{
    string messageId = message.Id; // ID of the message
    var receipt = receiptType; // receipt Type
}

void RbInstantMessaging_UserTypingChanged(Peer peerContext, Contact contact, bool isTyping)
{
    if(peerContext.Type == EntityType.User)
    {
        // A P2P context
        ConsoleAbstraction.WriteBlue($"{CR} Contact [{contact.ToString(DetailsLevel.Small)}] - isTyping:[{isTyping}]");
    }
    else
    {
        // A Bubble context
        ConsoleAbstraction.WriteBlue($"{CR} Contact [{contact.ToString(DetailsLevel.Small)}] - isTyping:[{isTyping}] in Bubble:[{peerContext.ToString(DetailsLevel.Small)}]");
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