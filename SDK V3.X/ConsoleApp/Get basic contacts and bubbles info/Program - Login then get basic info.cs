
using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;
using System.Text;

// --------------------------------------------------

//  /!\ Define you Rainbow settings below:

String appId = ""; // To set according your settings
String appSecretKey = ""; // To set according your settings
String hostName = ""; // To set according your settings

String login = ""; // To set according your settings
String password = ""; // To set according your settings

// --------------------------------------------------

Console.OutputEncoding = Encoding.UTF8; // We want to display UTF8 on the console
Boolean canUseEmoji = true;             // Set to false if Emoji cannot be displayed in your console

Object consoleLockObject = new(); // To lock until the current console display is performed
String CR = Rainbow.Util.CR; // Get carriage return;

// Check if information are set
if (String.IsNullOrEmpty(appId)
    || String.IsNullOrEmpty(appSecretKey)
    || String.IsNullOrEmpty(hostName)
    || String.IsNullOrEmpty(login)
    || String.IsNullOrEmpty(password))
{
    WriteRed("Ensure to set appId / appSecretKey / hostName and login / password");
    System.Environment.Exit(0);
}

Rainbow.Util.SetLogAnonymously(false);

// Add default logger
NLogConfigurator.AddLogger();

// Create Rainbow SDK objects
var RbApplication = new Application();

var RbContacts = RbApplication.GetContacts();
var RbBubbles = RbApplication.GetBubbles();



// Set global configuration info
RbApplication.SetApplicationInfo(appId, appSecretKey);
RbApplication.SetHostInfo(hostName);

RbContacts.ContactsAdded += RbContacts_ContactsAdded; // Triggered when a Contact is added in the cache

// Start login
WriteWhite("Starting login ...");
var sdkResult = await RbApplication.LoginAsync(login, password);

if (sdkResult.Success)
{
    // We are connected and SDK is ready
    WriteGreen($"{CR}Connected to Rainbow Server");

    DisplayInputsPossible();
}
else
{
    // We are not connected - Display why:
    WriteRed($"{CR}Connection failed - SdkError:{sdkResult.Result}");
}

WriteBlue($"{CR}Use [ESC] at anytime to quit");

do
{
    await Task.Delay(200);
    await CheckInputKey();
} while (true);


void DisplayInputsPossible()
{
    WriteBlue($"{CR}Use [1, 2 or 3] to get basic (1) to advanced (3) Bubbles details\r\n");

    WriteBlue($"{CR}Use [C] at anytime to contacts information\r\n");
}

async Task CheckInputKey()
{
    while (Console.KeyAvailable)
    {
        var userInput = Console.ReadKey(true);

        switch (userInput.Key)
        {
            case ConsoleKey.Escape:
                WriteYellow($"Asked to end process using [ESC] key");
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
    WriteRed($"{CR}------ START DisplayContactsNotInRoster");

    var contacts = RbContacts.GetAllContacts();
    var contactsInRoster = RbContacts.GetAllContactsInRoster();
    WriteWhite($"Nb Contacts NOT in Roster:[{contacts.Count - contactsInRoster.Count}]");
    foreach (var contact in contacts)
        if(!contact.InRoster)
            WriteBlue($"\t{contact.ToString(DetailsLevel.Small)}");

    WriteRed($"------ END DisplayContactsNotInRoster{CR}");
}

void DisplayContactsInRoster()
{
    // We want to display all contacts in Roster.
    // By default they are provided sorted by display name

    WriteRed($"{CR}------ START DisplayContactsInRoster");

    var contacts = RbContacts.GetAllContactsInRoster();
    WriteWhite($"Nb Contacts in Roster:[{contacts.Count}]");
    foreach (var contact in contacts)
        WriteBlue($"\t{contact.ToString(DetailsLevel.Small)}");

    WriteRed($"------ END DisplayContactsInRoster{CR}");
}

async Task DisplayBubbleInfo(Bubble bubble, Boolean displayBubbleMemberInfo, Boolean displayBubbleMemberInfoAsContact)
{
    String inactiveMsg = canUseEmoji ? "💤💤💤💤💤 " : "[INACTIVE] ";
    String activeMsg = "           ";

    List<String> memberStatus = new();
    List<String> memberPrivilege = new();

    WriteYellow($"{((bubble.IsActive) ? activeMsg : inactiveMsg)}{bubble.ToString(DetailsLevel.Small)}");
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
        WriteGreen($"\t\t{bubble.ToString(DetailsLevel.Small)} - Nb Moderator(s): [{moderators.Count}]");
        foreach (var member in moderators)
            WriteWhite($"\t\t{(String.IsNullOrEmpty(member.Peer.DisplayName) ? member.Peer.Id : member.Peer.DisplayName)}");

        // We want Members only and Accepted 
        memberPrivilege.Clear();
        memberPrivilege.Add(BubbleMemberPrivilege.User);
        var members = RbBubbles.GetMembers(bubble, memberStatus: memberStatus, memberPrivilege: memberPrivilege);
        WriteGreen($"\t\t{bubble.ToString(DetailsLevel.Small)} - Nb Members (s): [{members.Count}]");
        foreach (var member in members)
            WriteWhite($"\t\t{(String.IsNullOrEmpty(member.Peer.DisplayName) ? member.Peer.Id : member.Peer.DisplayName)}");
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
            WriteGreen($"\t\t{bubble.ToString(DetailsLevel.Small)} - Nb Moderator(s): [{moderators.Count}]");
            foreach (var member in moderators)
                WriteWhite($"\t\t{(String.IsNullOrEmpty(member.Peer.DisplayName) ? member.Peer.Id : member.Peer.DisplayName)}");
        }

        // We want Members only and Accepted 
        memberPrivilege.Clear();
        memberPrivilege.Add(BubbleMemberPrivilege.User);
        sdkResult = await RbBubbles.GetMembersAsContactsAsync(bubble, memberStatus: memberStatus, memberPrivilege: memberPrivilege);
        if (sdkResult.Success)
        {
            var members = sdkResult.Data;
            WriteGreen($"\t\t{bubble.ToString(DetailsLevel.Small)} - Nb Members (s): [{members.Count}]");
            foreach (var member in members)
                WriteWhite($"\t\t{(String.IsNullOrEmpty(member.Peer.DisplayName) ? member.Peer.Id : member.Peer.DisplayName)}");
        }
    }
}

async Task DisplayBubbles(Boolean displayBubbleInfo, Boolean displayBubbleMemberInfo = false, Boolean displayBubbleMemberInfoAsContact = false)
{
    String inactiveMsg = canUseEmoji ? "💤💤💤💤💤 " : "[INACTIVE] ";
    String activeMsg = "           ";

    // We want to display Bubbles info
    // By default they are provided sorted by display name

    WriteRed($"{CR}------ START DisplayBubbles");

    List<String> memberStatus = new();
    List<String> memberPrivilege = new();
    List<Bubble> bubbles;

    // Bubbles as Owner only
    memberPrivilege.Clear();
    memberPrivilege.Add(BubbleMemberPrivilege.Owner);
    bubbles = RbBubbles.GetAllBubbles(memberPrivilege: memberPrivilege);
    WriteBlue($"Bubble(s) as Owner - Nb:[{bubbles.Count}]");
    if (displayBubbleInfo)
        foreach (var bubble in bubbles)
            await DisplayBubbleInfo(bubble, displayBubbleMemberInfo, displayBubbleMemberInfoAsContact);

    // Bubbles as Moderator only
    memberPrivilege.Clear();
    memberPrivilege.Add(BubbleMemberPrivilege.Moderator);
    bubbles = RbBubbles.GetAllBubbles(memberPrivilege: memberPrivilege);
    WriteBlue($"\tBubble(s) as Moderator - Nb:[{bubbles.Count}]");
    if (displayBubbleInfo)
        foreach (var bubble in bubbles)
            await DisplayBubbleInfo(bubble, displayBubbleMemberInfo, displayBubbleMemberInfoAsContact);

    // Bubbles as User only
    memberPrivilege.Clear();
    memberPrivilege.Add(BubbleMemberPrivilege.User);
    bubbles = RbBubbles.GetAllBubbles(memberPrivilege: memberPrivilege);
    WriteBlue($"\tBubble(s) as User - Nb:[{bubbles.Count}]");
    if (displayBubbleInfo)
        foreach (var bubble in bubbles)
            await DisplayBubbleInfo(bubble, displayBubbleMemberInfo, displayBubbleMemberInfoAsContact);


    // Bubbles as Accepted status only - Permits to have all bubbles where current user can communicates
    memberStatus.Clear();
    memberStatus.Add(BubbleMemberStatus.Accepted);
    bubbles = RbBubbles.GetAllBubbles(memberStatus: memberStatus);
    WriteBlue($"\tBubble(s) as Accepted status - Nb:[{bubbles.Count}]");
    if (displayBubbleInfo)
        foreach (var bubble in bubbles)
            await DisplayBubbleInfo(bubble, displayBubbleMemberInfo, displayBubbleMemberInfoAsContact);

    // Bubbles as Invited status only
    memberStatus.Clear();
    memberStatus.Add(BubbleMemberStatus.Invited);
    bubbles = RbBubbles.GetAllBubbles(memberStatus: memberStatus);
    WriteBlue($"\tBubble(s) as Invited status - Nb:[{bubbles.Count}]");
    if (displayBubbleInfo)
        foreach (var bubble in bubbles)
            await DisplayBubbleInfo(bubble, displayBubbleMemberInfo, displayBubbleMemberInfoAsContact);

    // Bubbles as Unsubscribed status only
    memberStatus.Clear();
    memberStatus.Add(BubbleMemberStatus.Unsubscribed);
    bubbles = RbBubbles.GetAllBubbles(memberStatus: memberStatus);
    WriteBlue($"\tBubble(s) as Unsubscribed status - Nb:[{bubbles.Count}]");
    if (displayBubbleInfo)
        foreach (var bubble in bubbles)
            await DisplayBubbleInfo(bubble, displayBubbleMemberInfo, displayBubbleMemberInfoAsContact);


    // Bubbles as Owner or Moderator (an Owner is always a Moderator - so here we have same result than asking for Moderator only)
    memberPrivilege.Clear();
    memberPrivilege.Add(BubbleMemberPrivilege.Owner);
    memberPrivilege.Add(BubbleMemberPrivilege.Moderator);
    bubbles = RbBubbles.GetAllBubbles(memberPrivilege: memberPrivilege);
    WriteBlue($"\tBubble(s) as Owner And Moderator - Nb:[{bubbles.Count}]");
    if (displayBubbleInfo)
        foreach (var bubble in bubbles)
            await DisplayBubbleInfo(bubble, displayBubbleMemberInfo, displayBubbleMemberInfoAsContact);

    // Bubbles as Moderator or User
    memberPrivilege.Clear();
    memberPrivilege.Add(BubbleMemberPrivilege.Moderator);
    memberPrivilege.Add(BubbleMemberPrivilege.User);
    bubbles = RbBubbles.GetAllBubbles(memberPrivilege: memberPrivilege);
    WriteBlue($"\tBubble(s) as Moderator or User - Nb:[{bubbles.Count}]");
    if (displayBubbleInfo)
        foreach (var bubble in bubbles)
            await DisplayBubbleInfo(bubble, displayBubbleMemberInfo, displayBubbleMemberInfoAsContact);

    WriteRed($"------ END DisplayBubbles{CR}");
}

void RbContacts_ContactsAdded(List<Contact> contacts)
{
    List<String> displayNames = new();
    foreach (var contact in contacts)
        displayNames.Add(contact.ToString(DetailsLevel.Small));

    WriteRed($"Event ContactsAdded triggered - Contact(s):[{String.Join(", ", displayNames)}]");
}

#region UTILITY METHODS

void WriteYellow(String message)
{
    WriteToConsole(message, ConsoleColor.Yellow);
}

void WriteWhite(String message)
{
    WriteToConsole(message, ConsoleColor.White);
}

void WriteRed(String message)
{
    WriteToConsole(message, ConsoleColor.Red);
}

void WriteGreen(String message)
{
    WriteToConsole(message, ConsoleColor.Green);
}

void WriteBlue(String message)
{
    WriteToConsole(message, ConsoleColor.Blue);
}

void WriteToConsole(String message, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null)
{
    if (String.IsNullOrEmpty(message))
        return;

    lock (consoleLockObject)
    {
        if (foregroundColor != null)
            Console.ForegroundColor = foregroundColor.Value;

        if (backgroundColor != null)
            Console.BackgroundColor = backgroundColor.Value;

        Console.WriteLine(message);
        Console.ResetColor();
    }
}
#endregion UTILITY METHODS
