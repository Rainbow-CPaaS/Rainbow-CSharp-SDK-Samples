
using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;
using System.Text;

using Rainbow.Console;
using Util = Rainbow.Console.Util;
using Rainbow.SimpleJSON;
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

Console.OutputEncoding = Encoding.UTF8; // We want to display UTF8 on the console
var canUseEmoji = true;

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
var RbApplication = new Application(loggerPrefix: logPrefix);

var RbContacts = RbApplication.GetContacts();
var RbBubbles = RbApplication.GetBubbles();

// Set global configuration info
RbApplication.SetApplicationInfo(credentials.ServerConfig.AppId, credentials.ServerConfig.AppSecret);
RbApplication.SetHostInfo(credentials.ServerConfig.HostName);

RbContacts.ContactsAdded += RbContacts_ContactsAdded; // Triggered when a Contact is added in the cache

// Start login
Util.WriteWhite("Starting login ...");
var sdkResult = await RbApplication.LoginAsync(credentials.UsersConfig[0].Login, credentials.UsersConfig[0].Password);

if (sdkResult.Success)
{
    // We are connected and SDK is ready
    Util.WriteGreen($"{CR}Connected to Rainbow Server");

    DisplayInputsPossible();
}
else
{
    // We are not connected - Display why:
    Util.WriteRed($"{CR}Connection failed - SdkError:{sdkResult.Result}");
}

Util.WriteBlue($"{CR}Use [ESC] at anytime to quit");

do
{
    await Task.Delay(200);
    await CheckInputKey();
} while (true);


void DisplayInputsPossible()
{
    Util.WriteBlue($"{CR}Use [1, 2 or 3] to get basic (1) to advanced (3) Bubbles details\r\n");

    Util.WriteBlue($"{CR}Use [C] at anytime to contacts information\r\n");
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

            case ConsoleKey.NumPad1:
            case ConsoleKey.D1:
                await DisplayBubbles(true);
                break;
                
            case ConsoleKey.NumPad2:
            case ConsoleKey.D2:
                await DisplayBubbles(true, true);
                break;

            case ConsoleKey.NumPad3:
            case ConsoleKey.D3:
                await DisplayBubbles(true, true, true);
                break;

            case ConsoleKey.C:
                DisplayContacts();
                break;
        }
        DisplayInputsPossible();
    }
}

void DisplayContacts()
{
    DisplayContactsInRoster();
    DisplayContactsNotInRoster();
}

void DisplayContactsNotInRoster()
{
    Util.WriteRed($"{CR}------ START DisplayContactsNotInRoster");

    var contacts = RbContacts.GetAllContacts();
    var contactsInRoster = RbContacts.GetAllContactsInRoster();
    Util.WriteWhite($"Nb Contacts NOT in Roster:[{contacts.Count - contactsInRoster.Count}]");
    foreach (var contact in contacts)
        if(!contact.InRoster)
            Util.WriteBlue($"\t{contact.ToString(DetailsLevel.Small)}");

    Util.WriteRed($"------ END DisplayContactsNotInRoster{CR}");
}

void DisplayContactsInRoster()
{
    // We want to display all contacts in Roster.
    // By default they are provided sorted by display name

    Util.WriteRed($"{CR}------ START DisplayContactsInRoster");

    var contacts = RbContacts.GetAllContactsInRoster();
    Util.WriteWhite($"Nb Contacts in Roster:[{contacts.Count}]");
    foreach (var contact in contacts)
        Util.WriteBlue($"\t{contact.ToString(DetailsLevel.Small)}");

    Util.WriteRed($"------ END DisplayContactsInRoster{CR}");
}

async Task DisplayBubbleInfo(Bubble bubble, Boolean displayBubbleMemberInfo, Boolean displayBubbleMemberInfoAsContact)
{
    String inactiveMsg = canUseEmoji ? "💤💤💤💤💤 " : "[INACTIVE] ";
    String activeMsg = "           ";

    List<String> memberStatus = new();
    List<String> memberPrivilege = new();

    Util.WriteYellow($"{((bubble.IsActive) ? activeMsg : inactiveMsg)}{bubble.ToString(DetailsLevel.Small)}");
    if (displayBubbleMemberInfo && (!displayBubbleMemberInfoAsContact))
    {
        // /!\ Here we ask BubbleMember - if they are not knownn yet as Contact, their Display Name is not available
        // If you want to have if, use "displayBubbleMemberInfoAsContact" and set it to true to see how to retrieve them

        memberStatus.Clear();
        memberStatus.Add(BubbleMemberStatus.Accepted);

        // We want Moderators only and Accepted
        memberPrivilege.Clear();
        memberPrivilege.Add(BubbleMemberPrivilege.Moderator);
        var moderators = RbBubbles.GetMembers(bubble, memberStatus: memberStatus, memberPrivilege: memberPrivilege); ; ;
        Util.WriteGreen($"\t\t{bubble.ToString(DetailsLevel.Small)} - Nb Moderator(s): [{moderators.Count}]");
        foreach (var member in moderators)
            Util.WriteWhite($"\t\t{(String.IsNullOrEmpty(member.Peer.DisplayName) ? member.Peer.Id : member.Peer.DisplayName)}");

        // We want Members only and Accepted 
        memberPrivilege.Clear();
        memberPrivilege.Add(BubbleMemberPrivilege.User);
        var members = RbBubbles.GetMembers(bubble, memberStatus: memberStatus, memberPrivilege: memberPrivilege);
        Util.WriteGreen($"\t\t{bubble.ToString(DetailsLevel.Small)} - Nb Members (s): [{members.Count}]");
        foreach (var member in members)
            Util.WriteWhite($"\t\t{(String.IsNullOrEmpty(member.Peer.DisplayName) ? member.Peer.Id : member.Peer.DisplayName)}");
    }
    else if(displayBubbleMemberInfoAsContact)
    {
        // /!\ Here we ask Members as Contact - So if some members are not yet known, GetMembersAsContactsAsync() will ask the server
        // Once there are known, we not more asks the server

        memberStatus.Clear();
        memberStatus.Add(BubbleMemberStatus.Accepted);

        // We want Moderators only and Accepted
        memberPrivilege.Clear();
        memberPrivilege.Add(BubbleMemberPrivilege.Moderator);
        var sdkResult = await RbBubbles.GetMembersAsContactsAsync(bubble, memberStatus: memberStatus, memberPrivilege: memberPrivilege); ; ;
        if (sdkResult.Success)
        {
            var moderators = sdkResult.Data;
            Util.WriteGreen($"\t\t{bubble.ToString(DetailsLevel.Small)} - Nb Moderator(s): [{moderators.Count}]");
            foreach (var member in moderators)
                Util.WriteWhite($"\t\t{(String.IsNullOrEmpty(member.Peer.DisplayName) ? member.Peer.Id : member.Peer.DisplayName)}");
        }

        // We want Members only and Accepted 
        memberPrivilege.Clear();
        memberPrivilege.Add(BubbleMemberPrivilege.User);
        sdkResult = await RbBubbles.GetMembersAsContactsAsync(bubble, memberStatus: memberStatus, memberPrivilege: memberPrivilege);
        if (sdkResult.Success)
        {
            var members = sdkResult.Data;
            Util.WriteGreen($"\t\t{bubble.ToString(DetailsLevel.Small)} - Nb Members (s): [{members.Count}]");
            foreach (var member in members)
                Util.WriteWhite($"\t\t{(String.IsNullOrEmpty(member.Peer.DisplayName) ? member.Peer.Id : member.Peer.DisplayName)}");
        }
    }
}

async Task DisplayBubbles(Boolean displayBubbleInfo, Boolean displayBubbleMemberInfo = false, Boolean displayBubbleMemberInfoAsContact = false)
{
    String inactiveMsg = canUseEmoji ? "💤💤💤💤💤 " : "[INACTIVE] ";
    String activeMsg = "           ";

    // We want to display Bubbles info
    // By default they are provided sorted by display name

    Util.WriteRed($"{CR}------ START DisplayBubbles");

    List<String> memberStatus = new();
    List<String> memberPrivilege = new();
    List<Bubble> bubbles;

    // Bubbles as Owner only
    memberPrivilege.Clear();
    memberPrivilege.Add(BubbleMemberPrivilege.Owner);
    bubbles = RbBubbles.GetAllBubbles(memberPrivilege: memberPrivilege);
    Util.WriteBlue($"{CR}\tBubble(s) as Owner - Nb:[{bubbles.Count}]");
    if (displayBubbleInfo)
        foreach (var bubble in bubbles)
            await DisplayBubbleInfo(bubble, displayBubbleMemberInfo, displayBubbleMemberInfoAsContact);

    // Bubbles as Moderator only
    memberPrivilege.Clear();
    memberPrivilege.Add(BubbleMemberPrivilege.Moderator);
    bubbles = RbBubbles.GetAllBubbles(memberPrivilege: memberPrivilege);
    Util.WriteBlue($"{CR}\tBubble(s) as Moderator - Nb:[{bubbles.Count}]");
    if (displayBubbleInfo)
        foreach (var bubble in bubbles)
            await DisplayBubbleInfo(bubble, displayBubbleMemberInfo, displayBubbleMemberInfoAsContact);

    // Bubbles as User only
    memberPrivilege.Clear();
    memberPrivilege.Add(BubbleMemberPrivilege.User);
    bubbles = RbBubbles.GetAllBubbles(memberPrivilege: memberPrivilege);
    Util.WriteBlue($"{CR}\tBubble(s) as User - Nb:[{bubbles.Count}]");
    if (displayBubbleInfo)
        foreach (var bubble in bubbles)
            await DisplayBubbleInfo(bubble, displayBubbleMemberInfo, displayBubbleMemberInfoAsContact);


    // Bubbles as Accepted status only - Permits to have all bubbles where current user can communicates
    memberStatus.Clear();
    memberStatus.Add(BubbleMemberStatus.Accepted);
    bubbles = RbBubbles.GetAllBubbles(memberStatus: memberStatus);
    Util.WriteBlue($"{CR}\tBubble(s) as Accepted status - Nb:[{bubbles.Count}]");
    if (displayBubbleInfo)
        foreach (var bubble in bubbles)
            await DisplayBubbleInfo(bubble, displayBubbleMemberInfo, displayBubbleMemberInfoAsContact);

    // Bubbles as Invited status only
    memberStatus.Clear();
    memberStatus.Add(BubbleMemberStatus.Invited);
    bubbles = RbBubbles.GetAllBubbles(memberStatus: memberStatus);
    Util.WriteBlue($"{CR}\tBubble(s) as Invited status - Nb:[{bubbles.Count}]");
    if (displayBubbleInfo)
        foreach (var bubble in bubbles)
            await DisplayBubbleInfo(bubble, displayBubbleMemberInfo, displayBubbleMemberInfoAsContact);

    // Bubbles as Unsubscribed status only
    memberStatus.Clear();
    memberStatus.Add(BubbleMemberStatus.Unsubscribed);
    bubbles = RbBubbles.GetAllBubbles(memberStatus: memberStatus);
    Util.WriteBlue($"\tBubble(s) as Unsubscribed status - Nb:[{bubbles.Count}]");
    if (displayBubbleInfo)
        foreach (var bubble in bubbles)
            await DisplayBubbleInfo(bubble, displayBubbleMemberInfo, displayBubbleMemberInfoAsContact);


    // Bubbles as Owner or Moderator (an Owner is always a Moderator - so here we have same result than asking for Moderator only)
    memberPrivilege.Clear();
    memberPrivilege.Add(BubbleMemberPrivilege.Owner);
    memberPrivilege.Add(BubbleMemberPrivilege.Moderator);
    bubbles = RbBubbles.GetAllBubbles(memberPrivilege: memberPrivilege);
    Util.WriteBlue($"{CR}\tBubble(s) as Owner And Moderator - Nb:[{bubbles.Count}]");
    if (displayBubbleInfo)
        foreach (var bubble in bubbles)
            await DisplayBubbleInfo(bubble, displayBubbleMemberInfo, displayBubbleMemberInfoAsContact);

    // Bubbles as Moderator or User
    memberPrivilege.Clear();
    memberPrivilege.Add(BubbleMemberPrivilege.Moderator);
    memberPrivilege.Add(BubbleMemberPrivilege.User);
    bubbles = RbBubbles.GetAllBubbles(memberPrivilege: memberPrivilege);
    Util.WriteBlue($"{CR}\tBubble(s) as Moderator or User - Nb:[{bubbles.Count}]");
    if (displayBubbleInfo)
        foreach (var bubble in bubbles)
            await DisplayBubbleInfo(bubble, displayBubbleMemberInfo, displayBubbleMemberInfoAsContact);

    Util.WriteRed($"------ END DisplayBubbles{CR}");
}

void RbContacts_ContactsAdded(List<Contact> contacts)
{
    List<String> displayNames = new();
    foreach (var contact in contacts)
        displayNames.Add(contact.ToString(DetailsLevel.Small));

    Util.WriteRed($"Event ContactsAdded triggered - Contact(s):[{String.Join(", ", displayNames)}]");
}

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
