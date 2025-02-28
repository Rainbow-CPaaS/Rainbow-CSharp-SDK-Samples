using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;

using Rainbow.Console;
using Util = Rainbow.Console.Util;
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

Util.WriteRed($"Account used: [{credentials.UsersConfig[0].Login}]");

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

// Create Rainbow SDK objects
var RbApplication = new Application(loggerPrefix: logPrefix);
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

Util.WriteGreen($"{CR}Use [ESC] at anytime to quit");

Util.WriteGreen($"{CR}Use [Q] at anytime once loggued to quit using Logout");

// Start login
Util.WriteWhite($"{CR}Starting login ...");
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
    Util.WriteGreen($"{CR}Use [C] to search a Contact and retrieve messages exchanged with him in Peer to Peer");

    Util.WriteGreen($"{CR}Use [B] to search a Bubble and retrieve message exchanged in it");
}

Contact? SearchForContactsInRoster()
{
    Contact? result = null;
    Util.WriteYellow($"{CR}Enter a text to search a contact from you Roster: ");
    var str = Console.ReadLine();
    if (str != null)
    {
        var contactsList = RbContacts.GetAllContactsInRoster();
        result = contactsList.Find(contact => contact.Peer.DisplayName.Contains(str, StringComparison.InvariantCultureIgnoreCase) && contact.Peer.Id != RbContacts.GetCurrentContact().Peer.Id);
    }

    if(result == null)
        Util.WriteRed($"{CR}No contact found with [{str}]");
    else
        Util.WriteWhite($"Contact found:[{result.ToString(DetailsLevel.Small)}]");

    return result;
}

Bubble? SearchForBubbles()
{
    Bubble? result = null;
    Util.WriteYellow($"{CR}Enter a text to search a Bubble:");
    var str = Console.ReadLine();
    if (str != null)
    {
        List<String> memberStatus = new() { BubbleMemberStatus.Accepted };
        var bubblesList = RbBubbles.GetAllBubbles(memberStatus: memberStatus);
        result = bubblesList.Find(contact => contact.Peer.DisplayName.Contains(str, StringComparison.InvariantCultureIgnoreCase));
    }

    if (result == null)
        Util.WriteRed($"{CR}No bubble found with [{str}]");
    else
        Util.WriteWhite($"bubble found:[{result.ToString(DetailsLevel.Small)}]");

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
            int nbMessagesByRow = 100;

            SdkResult<List<Rainbow.Model.Message>> mamSdkResult;
            Boolean canContinue = true;
            Boolean error = false;
            int messagesCount = 0;

            Util.WriteBlue($"Start to fetch all messages from Peer:[{RbApplication.GetPeerDisplayName(peer)}]");

            DateTime start = DateTime.Now;
            DateTime startMsgRow = start;
            DateTime endMsgRow;
            TimeSpan elapsed;
            do
            {
                startMsgRow = DateTime.Now;
                mamSdkResult = await RbInstantMessaging.GetOlderMessagesAsync(peer, nbMessagesByRow);
                if (mamSdkResult.Success)
                {
                    endMsgRow = DateTime.Now;
                    elapsed = endMsgRow - startMsgRow;
                    if (mamSdkResult.Data.Count > 0)
                    {
                        messagesCount += mamSdkResult.Data.Count;
                        Util.WriteYellow($"Retrieved: {mamSdkResult.Data.Count} - Time elapsed:[{elapsed.TotalSeconds} sec] - Count:[{messagesCount}]");
                    }
                    else
                        canContinue = false;
                }
                else
                {
                    Util.WriteRed($"SdkError:{mamSdkResult.Result}");
                    canContinue = false;
                    error = true;
                }
            }
            while (canContinue);

            if (!error)
            {
                // Here messages object contains all messages exchanged with the Peer
                List<Rainbow.Model.Message> messages = RbInstantMessaging.GetOlderMessages(peer);
                elapsed = DateTime.Now - start;

                if (messagesCount != 0)
                    Util.WriteBlue($"Nb Messages:[{messagesCount}] - Time elapsed:[{elapsed.TotalSeconds} sec]");
                else
                    Util.WriteBlue($"We have already get all messages from this Peer - Nb Messages:[{messages.Count()}] - Time elapsed:[{elapsed.TotalSeconds} sec]");

                if (saveAllMessages)
                {
                    var fileNameAllMessages = Path.Combine(Directory.GetCurrentDirectory(), $"Messages-ALL-{messagesCount}.txt");

                    Util.WriteDarkYellow($"Saving all messages in the file:[{Path.GetFullPath(fileNameAllMessages)}] ...");

                    StringBuilder stringBuilder = new StringBuilder();

                    // Delete previous one if any
                    File.Delete(fileNameAllMessages);
                    for (int i = 0; i < messagesCount; i++)
                    {
                        var message = messages[i];
                        AppendMessage(ref stringBuilder, message);

                        // Store in file every 500 messages
                        if ((i != 0) && (i % 500 == 0))
                        {
                            File.AppendAllText(fileNameAllMessages, stringBuilder.ToString(), Encoding.UTF8);
                            stringBuilder.Clear();
                            Util.WriteBlue($"500 Messages stored in file");
                        }
                    }
                    File.AppendAllText(fileNameAllMessages, stringBuilder.ToString(), Encoding.UTF8);
                    Util.WriteBlue($"All messages have been stored");
                }
                else
                {
                    // Log first and last messages
                    int nbMessagesScope = 100;

                    if (messages.Count > nbMessagesScope)
                    {
                        var fileNameFirstMessages = Path.Combine(Directory.GetCurrentDirectory(), $"Messages-First{nbMessagesScope}.txt");
                        StringBuilder stringBuilder = new();

                        // Delete previous one if any
                        File.Delete(fileNameFirstMessages);
                        for (int i = 0; i < nbMessagesScope; i++)
                        {
                            var message = messages[i];
                            AppendMessage(ref stringBuilder, message);
                        }
                        File.AppendAllText(fileNameFirstMessages, stringBuilder.ToString(), Encoding.UTF8);

                        stringBuilder.Clear();
                        var fileNameLastMessages = Path.Combine(Directory.GetCurrentDirectory(), $"Messages-Last{nbMessagesScope}.txt");
                        // Delete previous one if any
                        File.Delete(fileNameLastMessages);
                        for (int i = 0; i < nbMessagesScope; i++)
                        {
                            var message = messages[messagesCount - nbMessagesScope + i];
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

void RbContacts_ContactsAdded(IEnumerable<Rainbow.Model.Contact> contacts)
{
    List<String> displayNames = new();
    foreach (var contact in contacts)
    {
        displayNames.Add(contact.ToString(DetailsLevel.Small));
    }

    Util.WriteBlue($"{Rainbow.Util.CR}Event ContactsAdded triggered - Nb:[{displayNames.Count}] - Contact(s):[{String.Join(", ", displayNames)}]{Rainbow.Util.CR}");
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