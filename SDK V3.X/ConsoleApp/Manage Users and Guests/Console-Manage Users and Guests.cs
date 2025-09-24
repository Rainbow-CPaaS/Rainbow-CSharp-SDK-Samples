
using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;
using System.Text;

using Rainbow.Example.Common;
using Util = Rainbow.Example.Common.Util;
using Rainbow.SimpleJSON;


ExeSettings? exeSettings = null;
Credentials? credentials = null;

if ((!ReadExeSettings()) || (exeSettings is null))
    return;

credentials = ReadCredentials("credentials.json");
if (credentials is null)
    return;

// --------------------------------------------------

Object consoleLockObject = new(); // To lock until the current console display is performed
var CR = Rainbow.Util.CR;

Console.OutputEncoding = Encoding.UTF8; // We want to display UTF8 on the console

// Set folder / directory path
NLogConfigurator.Directory = exeSettings.LogFolderPath;
var logFullPath = Path.GetFullPath(exeSettings.LogFolderPath);
Util.WriteBlue($"Logs files will be stored in folder:[{logFullPath}]");

Rainbow.Util.SetLogAnonymously(false);

// ------------------------------------------------

// Log with first account
NLogConfigurator.AddLogger(credentials.UsersConfig[0].Prefix);

// Create Rainbow SDK objects
var RbApplication = new Application(iniFolderFullPathName: exeSettings.LogFolderPath, iniFileName: credentials.UsersConfig[0].Prefix + ".ini", loggerPrefix: credentials.UsersConfig[0].Prefix);
var RbContacts = RbApplication.GetContacts();
var RbBubbles = RbApplication.GetBubbles();
var RbAdministration = RbApplication.GetAdministration();

// Set global configuration info
RbApplication.SetApplicationInfo(credentials.ServerConfig.AppId, credentials.ServerConfig.AppSecret);
RbApplication.SetHostInfo(credentials.ServerConfig.HostName);

RbApplication.Restrictions.LogRestRequest = true;

Util.WriteRed($"{CR}Account used: [{credentials.UsersConfig[0].Login}]");
Util.WriteDarkYellow($"Logging in progress ...");
var sdkResult = await RbApplication.LoginAsync(credentials.UsersConfig[0].Login, credentials.UsersConfig[0].Password);

if(!sdkResult.Success)
{
    Util.WriteRed($"Cannot login account:[{sdkResult.Result}] - Error:[{sdkResult.Result}]");
    return;
}
else
{
    // Check roles of this account
    var roles = RbApplication.Roles;

    Util.WriteGreen($"{CR}Roles of this account:[{String.Join(", ", roles)}]");
    var adminRoles = roles.Where(s => s.Contains("admin")).ToList();
    if (adminRoles.Count == 0)
    {
        Util.WriteRed($"Without admin right");
        Util.WriteRed($"\tyou can create User using Company Link (but you need to have a valid one first. They can be created only with admin right)");
        Util.WriteRed($"\tyou can create GuestMode using Bubble Link");
    }
}

Organisation? organisationManaged = null;
Boolean noneOrganization = false;
Company? companyManaged = null;
Contact? userManaged = null;
Bubble? bubbleManaged = null;

// ------------------------------------------------

MenuDisplayInfo();

do
{
    await CheckInputKey();
    await Task.Delay(200);
} while (true);

async Task CheckInputKey()
{
    while (Console.KeyAvailable)
    {
        var userInput = Console.ReadKey(true);

        switch (userInput.Key)
        {
            case ConsoleKey.O:
                await MenuOrganisationsAsync();
                MenuDisplayInfo();
                return;

            case ConsoleKey.C:
                await MenuCompagniesAsync();
                MenuDisplayInfo();
                return;

            case ConsoleKey.L:
                await MenuCompanyLinkAsync();
                MenuDisplayInfo();
                return;

            case ConsoleKey.B:
                await MenuBubbleLinkAsync();
                MenuDisplayInfo();
                return;

            case ConsoleKey.U:
                await MenuUserAsync();
                MenuDisplayInfo();
                return;

            case ConsoleKey.I:
                MenuDisplayInfo();
                return;

            case ConsoleKey.Escape:
                Util.WriteYellow($"Asked to end process using [ESC] key");
                System.Environment.Exit(0);
                return;
        }
    }
}

async Task MenuOrganisationsAsync()
{
    int offset = 0;
    int limit = 100;
    Boolean canContinue = true;
    List<Organisation> organisationsList = new ();

    Util.WriteDarkYellow($"{CR}Asking server list of organisations ...");
    while (canContinue)
    {
        var sdkResult = await RbAdministration.GetOrganisationsAsync(DetailsLevel.Medium, offset: offset, limit: limit);
        if (sdkResult.Success)
        {
            var listFound = sdkResult.Data.Data;
            organisationsList.AddRange(listFound);
            canContinue = (listFound.Count == limit);
            offset += limit;
        }
        else
        {
            Util.WriteRed($"GetOrganisationsAsync - Error:[{sdkResult.Result}]");
            return;
        }
    }

    if(organisationsList.Count == 0)
    {
        Util.WriteYellow($"This account cannot manage organization.");
        organisationManaged = null;
        noneOrganization = true;
        return;
    }

    noneOrganization = false;
    Util.WriteYellow($"The account can manage [{organisationsList.Count}] organization(s)");
    if(organisationsList.Count == 1)
    {
        organisationManaged = organisationsList[0];
        Util.WriteYellow($"Organization managed: [{organisationManaged.Name}]");
        return;
    }

    if(organisationManaged is not null)
        Util.WriteYellow($"{CR}Current Organization managed: [{organisationManaged.Name}]");

    canContinue = true;
    while (canContinue)
    {
        Util.WriteYellow($"{CR}Enter a text to search an organization (starts with) - empty string to cancel");
        var str = Console.ReadLine();
        if (!String.IsNullOrEmpty(str))
        {
            var result = organisationsList.Find(organisation => organisation.Name.StartsWith(str, StringComparison.InvariantCultureIgnoreCase));
            if (result is null)
                Util.WriteYellow($"No organization found ...");
            else
            {
                if(result.Id != organisationManaged?.Id)
                {
                    if (companyManaged is not null)
                    {
                        Util.WriteYellow($"Previous company managed is no more used since the managed Organization has changed");
                        companyManaged = null;
                    }
                }
                organisationManaged = result;
                Util.WriteYellow($"Organization found and now managed: [{organisationManaged.Name}]");
                return;
            }
        }
        else
        {
            Util.WriteYellow($"Cancel organization search ...");
            canContinue = false;
        }
    }
}

async Task MenuCompagniesAsync()
{
    if(organisationManaged is null && !noneOrganization)
    {
        await MenuOrganisationsAsync();
        if(organisationManaged is null && !noneOrganization)
        {
            Util.WriteRed($"{CR}No Organization managed yet ...");
            return;
        }
    }

    int offset = 0;
    int limit = 100;
    Boolean canContinue = true;
    List<Company> companiesList = new ();

    if(organisationManaged is null)
        Util.WriteDarkYellow($"{CR}Asking server list of companies ...");
    else
        Util.WriteDarkYellow($"{CR}Asking server list of companies for Organization [{organisationManaged.Name}] ...");
    while (canContinue)
    {
        String? orgId = (organisationManaged is null) ? null : organisationManaged.Id;
        var sdkResult = await RbAdministration.GetCompaniesAsync(DetailsLevel.Medium, offset, limit, orgId);
        if (sdkResult.Success)
        {
            var listFound = sdkResult.Data.Data;
            companiesList.AddRange(listFound);
            canContinue = (listFound.Count == limit);
            offset += limit;
        }
        else
        {
            Util.WriteRed($"GetCompaniesAsync - Error:[{sdkResult.Result}]");
            return;
        }
    }

    Util.WriteYellow($"The account can manage [{companiesList.Count}] companies");
    if (companiesList.Count == 1)
    {
        companyManaged = companiesList[0];
        Util.WriteYellow($"Company managed: [{companyManaged.Name}]");
        return;
    }

    if (companyManaged is not null)
        Util.WriteYellow($"{CR}Current Company managed: [{companyManaged.Name}]");

    canContinue = true;
    while (canContinue)
    {
        Util.WriteYellow($"{CR}Enter a text to search a company (starts with) - empty string to cancel");
        var str = Console.ReadLine();
        if (!String.IsNullOrEmpty(str))
        {
            var result = companiesList.Find(company => company.Name.StartsWith(str, StringComparison.InvariantCultureIgnoreCase));
            if (result is null)
                Util.WriteYellow($"No company found ...");
            else
            {
                companyManaged = result;
                Util.WriteYellow($"Company found and now managed: [{companyManaged.Name}]");
                return;
            }
        }
        else
        {
            Util.WriteYellow($"Cancel company search ...");
            canContinue = false;
        }
    }


}

async Task MenuCompanyLinkAsync()
{
    if (companyManaged is null)
    {
        Util.WriteYellow($"{CR}No Company managed yet ...");
        await MenuCompagniesAsync();
        if (companyManaged is null)
            companyManaged = RbContacts.GetCompany();
    }

    int offset = 0;
    int limit = 100;
    Boolean canContinue = true;
    List<JoinCompanyLink> joinCompanyLinkList = new();

    Boolean isAdmin = true;

    Util.WriteDarkYellow($"{CR}Asking server list of join company link for Company [{companyManaged.Name}] ...");
    while (canContinue)
    {
        var sdkResult = await RbAdministration.GetJoinCompanyLinksAsync(DetailsLevel.Medium, offset, limit, companyManaged.Id);
        if (sdkResult.Success)
        {
            var listFound = sdkResult.Data.Data;
            joinCompanyLinkList.AddRange(listFound);
            canContinue = (listFound.Count == limit);
            offset += limit;
        }
        else
        {
            isAdmin = false;
            Util.WriteRed($"GetJoinCompanyLinksAsync - Error:[{sdkResult.Result}]");

            Util.WriteGreen($"{CR}You can still create a user if you have already a valid Company Link ");
            canContinue = false;
        }
    }
 
    if (joinCompanyLinkList.Count > 0)
    {
        Util.WriteYellow($"{CR}List of Join Company Link - Total:[{joinCompanyLinkList.Count}]:");
        foreach(var jcl in joinCompanyLinkList)
            Util.WriteDarkYellow($"\t{Rainbow.Util.LogOnOneLine(jcl.ToString(DetailsLevel.Medium))}");
    }
    else
    {
        if(isAdmin)
            Util.WriteDarkYellow($"{CR}No Join Company Link.");
    }

    Boolean listLinks = false;

    if (isAdmin)
        Util.WriteYellow($"\t [A] Add a Join Company Link");
    if (joinCompanyLinkList.Count > 0)
        Util.WriteYellow($"\t [D] Delete a Join Company Link");
    Util.WriteYellow($"\t [U] Create [U]ser with Join Company Link");
    if (isAdmin)
        Util.WriteYellow($"\t [L] List Join Company Link");
    Util.WriteYellow($"\t [C] Cancel");

    String str;
    String id;

    canContinue = true;
    do
    {
        while (Console.KeyAvailable)
        {
            var userInput = Console.ReadKey(true);

            switch (userInput.Key)
            {
                case ConsoleKey.A:
                    if (isAdmin)
                    {
                        Util.WriteDarkYellow($"{CR}Asking server to create a join company link for Company [{companyManaged.Name}] ...");
                        var createSdkResult = await RbAdministration.CreateJoinCompanyLinkAsync(companyManaged.Id, 20);
                        if (createSdkResult.Success)
                            Util.WriteYellow($"JoinCompanyLink created: [{createSdkResult.Data.ToString(DetailsLevel.Medium)}]");
                        else
                            Util.WriteRed($"CreateJoinCompanyLinkAsync - Error:[{createSdkResult.Result}]");

                        listLinks = true;
                    }
                    break;

                case ConsoleKey.U:
                    Util.WriteYellow($"{CR}Enter an Id of a join company link or empty string to use it to create an user");
                    str = Console.ReadLine();
                    id = "";
                    if (String.IsNullOrEmpty(str))
                        id = joinCompanyLinkList[0].Id;
                    else
                        id = str;

                    String loginEmail = Rainbow.Util.GetGUID().Substring(0, 16) + "@sdk.drop.me";
                    String pwd = Rainbow.Util.GetGUID().ToUpper() + "!a";
                    String firstName = Rainbow.Util.GetGUID().Substring(0, 5);
                    String lastName ="USER company link";

                    Util.WriteDarkYellow($"Creating user with Join Company Link ...");
                    var sdkResultContact = await RbAdministration.CreateUserWithCompanyLinkAsync(id, loginEmail, pwd, firstName, lastName, null, true);
                    if (sdkResultContact.Success)
                    {
                        var contactCreated = sdkResultContact.Data;
                        Util.WriteBlue($"Contact created - Login:[{loginEmail}] - Pwd:[{pwd}] - {Rainbow.Util.LogOnOneLine(contactCreated.ToString(DetailsLevel.Medium))}");
                    }
                    else
                        Util.WriteRed($"CreateUserWithCompanyLinkAsync - Error:[{sdkResultContact.Result}]");

                    listLinks = true;
                    break;

                case ConsoleKey.D:
                    if (joinCompanyLinkList.Count > 0)
                    {
                        Util.WriteYellow($"{CR}Enter an Id of a join company link or empty string to delete the first one");
                        str = Console.ReadLine();
                        id = "";
                        if (String.IsNullOrEmpty(str))
                            id = joinCompanyLinkList[0].Id;
                        else
                            id = str;
                        Util.WriteDarkYellow($"Deleting Join Company Link ...");
                        var deleteSdkResult = await RbAdministration.DeleteJoinCompanyLinkAsync(companyManaged.Id, id);
                        if (deleteSdkResult.Success)
                            Util.WriteYellow($"JoinCompanyLink deleted");
                        else
                            Util.WriteRed($"DeleteJoinCompanyLinkAsync - Error:[{deleteSdkResult.Result}]");

                        listLinks = true;
                    }
                    break;

                case ConsoleKey.L:
                    if (isAdmin)
                        listLinks = true;
                    break;

                case ConsoleKey.C:
                    Util.WriteYellow($"Cancelled ...");
                    MenuDisplayInfo();
                    return;
            }
        }
        if (listLinks)
            break;
        await Task.Delay(200);
    } while (canContinue);

    if(listLinks)
        await MenuCompanyLinkAsync();
}

async Task MenuBubbleLinkAsync()
{
    // We want list of bubbles as Owner or Moderator (to create / update bubble link)
    List<String> bubbleMemberPrivilegeList = [ BubbleMemberPrivilege.Owner, BubbleMemberPrivilege.Moderator];

    var bubblesList = RbBubbles.GetAllBubbles(bubbleMemberPrivilegeList: bubbleMemberPrivilegeList);
    if(bubblesList.Count == 0)
    {
        Util.WriteDarkYellow("This account has no bubble as Owner or Moderator. Creating one ...");
        var sdkResultBubble = await RbBubbles.CreateBubbleAsync("Test from SDK C#", "", BubbleVisibility.AsPublic);
        if(sdkResultBubble.Success)
        {
            var bubble = sdkResultBubble.Data;
            Util.WriteGreen($"Bubble created:[{Rainbow.Util.LogOnOneLine(bubble.ToString(DetailsLevel.Medium))}]");
        }
        else
        {
            Util.WriteRed($"CreateBubbleAsync - Error:[{sdkResultBubble.Result}]");
            return;
        }
    }

    bubblesList = RbBubbles.GetAllBubbles(bubbleMemberPrivilegeList: bubbleMemberPrivilegeList);
    if (bubblesList.Count == 0)
    {
        Util.WriteRed("This account has no bubble as Owner or Moderator ...");
        return;
    }
    if (bubblesList.Count == 1)
    {
        bubbleManaged = bubblesList[0];
    }
    else
    {
        Util.WriteYellow($"List of Bubbles (as Owner or Moderator) - Total[{bubblesList.Count}]");
        foreach (var bubble in bubblesList)
        {
            Util.WriteYellow($"\tId:[{bubble.Peer.Id}] - Name:[{bubble.Peer.DisplayName}]");
        }

        Util.WriteYellow($"{CR}Enter an Id of bubble or empty string to use the first one");
        var str = Console.ReadLine();
        if (String.IsNullOrEmpty(str))
            str = bubblesList[0].Peer.Id;
        bubbleManaged = bubblesList.FirstOrDefault(b => b.Peer.Id == str);
        if (bubbleManaged is null)
        {
            Util.WriteRed($"No bubble found with this id:[{str}]");
            return;
        }
    }
    Util.WriteYellow($"Checking if this bubble [{bubbleManaged.Peer.DisplayName}] has already a 'bubble link' ...");

    var sdkResultString = await RbBubbles.GetBubbleLinkAsync(bubbleManaged);
    if(sdkResultString.Success)
    {
        var link = sdkResultString.Data;
        if(link is null)
        {
            Util.WriteDarkYellow($"No bubble link yet. Creating one ...");
            sdkResultString = await RbBubbles.CreateBubbleLinkAsync(bubbleManaged);
            if (sdkResultString.Success)
            {
                link = sdkResultString.Data;
                Util.WriteGreen($"Bubble link created.");
            }
            else
            {
                Util.WriteRed($"CreateBubbleLinkAsync - Error:[{sdkResultString.Result}]");
                return;
            }
        }

        Util.WriteGreen($"Bubble [{bubbleManaged.Peer.DisplayName}] has a bubble link:[{link}]");

        Util.WriteDarkYellow($"Creating a User (a GuestMode) using the bubble Link ...");
        String loginEmail = Rainbow.Util.GetGUID().Substring(0, 16) + "@sdk.drop.me";
        String pwd = Rainbow.Util.GetGUID().ToUpper() + "!a";
        String firstName = Rainbow.Util.GetGUID().Substring(0, 5);
        String lastName = "USER bubble link";
                
        var sdkResultContact = await RbAdministration.CreateUserWithBubbleLinkAsync(link, loginEmail, pwd, firstName, lastName, null, null, true);
        if (sdkResultContact.Success)
        {
            var contactCreated = sdkResultContact.Data;
            Util.WriteBlue($"Contact created - Login:[{loginEmail}] - Pwd:[{pwd}] - {Rainbow.Util.LogOnOneLine(contactCreated.ToString(DetailsLevel.Medium))}");
        }
        else
            Util.WriteRed($"CreateUserWithBubbleLinkAsync - Error:[{sdkResultContact.Result}]");
    }
    else
    {
        Util.WriteRed($"GetBubbleLinkAsync - Error:[{sdkResultString.Result}]");
        return;
    }
}

async Task MenuUserAsync()
{
    if (companyManaged is null)
    {
        Util.WriteYellow($"{CR}No Company managed yet ...");
        await MenuCompagniesAsync();
        if (companyManaged is null)
        {
            Util.WriteRed($"{CR}No Company managed yet ...");
            return;
        }
    }

    Util.WriteYellow($"{CR}Do you want to [A]dd, [L]ist/delete users in company:[{companyManaged.Name}] or [C]ancel ?");

    Boolean canContinue = true;
    while (canContinue)
    {
        if (Console.KeyAvailable)
        {
            var userInput = Console.ReadKey(true);
            switch (userInput.Key)
            {
                case ConsoleKey.A:
                    canContinue = false;
                    await MenuCreateUserAsync();
                    break;

                case ConsoleKey.L:
                    await MenuListUserAsync();
                    canContinue = false;
                    break;

                case ConsoleKey.C:
                    canContinue = false;
                    Util.WriteDarkYellow("Cancelled ...");
                    break;
            }
        }
        await Task.Delay(200);
    }
}

async Task MenuCreateUserAsync()
{
    if (companyManaged is null)
    {
        Util.WriteYellow($"{CR}No Company managed yet ...");
        await MenuCompagniesAsync();
        if (companyManaged is null)
        {
            Util.WriteRed($"{CR}No Company managed yet ...");
            return;
        }
    }

    String firstName = Rainbow.Util.GetGUID().Substring(0, 5);
    String lastName ="USER";

    Util.WriteDarkYellow("Creating user ...");
    String loginEmail = Rainbow.Util.GetGUID().Substring(0, 16) + "@sdk.drop.me";
    String pwd = Rainbow.Util.GetGUID().ToUpper() + "!a";

    var sdkResultContact = await RbAdministration.CreateUserAsync(loginEmail, pwd,  firstName, lastName, null, companyManaged.Id, true, false);
    if (sdkResultContact.Success)
    {
        var contact = sdkResultContact.Data;
        Util.WriteGreen($"User created - Login:[{loginEmail}] - Pwd:[{pwd}] - {Rainbow.Util.LogOnOneLine(contact.ToString(DetailsLevel.Medium))}");
    }
    else
        Util.WriteRed($"CreateUserAsync - Error:[{sdkResultContact.Result}]");
}

async Task MenuListUserAsync()
{
    Boolean canContinue = true;
    int offset = 0;
    int limit = 100;
    List<Contact> usersList = [];

    List<String>? roles = null;

    Util.WriteDarkYellow($"{CR}Asking list of users for Company [{companyManaged.Name}] ...");

    while (canContinue)
    {
        var sdkResult = await RbAdministration.GetUsersAsync(true, offset, limit, filterCompanyId: companyManaged.Id, roles: roles);
        if (sdkResult.Success)
        {
            var listFound = sdkResult.Data.Data;
            usersList.AddRange(listFound);
            canContinue = (listFound.Count == limit);
            offset += limit;
        }
        else
        {
            Util.WriteRed($"GetUsersAsync - Error:[{sdkResult.Result}]");
            return;
        }
    }

    if (usersList.Count == 0)
    {
        Util.WriteGreen($"None user ...");
        return;
    }

    Util.WriteYellow($"{CR}List of Users - Total[{usersList.Count}]):");
    foreach (var contact in usersList)
    {
        Util.WriteYellow($"\t- {Rainbow.Util.LogOnOneLine(contact.ToString(DetailsLevel.Medium))}");
    }

    canContinue = true;
    while (canContinue)
    {
        Util.WriteYellow($"{CR}Enter Id to select a user - empty string to cancel");

        var str = Console.ReadLine();
        if (!String.IsNullOrEmpty(str))
        {
            var result = usersList.Find(contact => contact.Peer.Id.Equals(str, StringComparison.InvariantCultureIgnoreCase));

            if (result is null)
                Util.WriteYellow($"None user ...");
            else
            {
                userManaged = result;
                Util.WriteYellow($"User found and now managed: [{userManaged.ToString(DetailsLevel.Medium)}]");

                Util.WriteRed($"Do you want to delete it ? (can not be undone !) [Y]");
                string userId = userManaged.Peer.Id;
                while (true)
                {
                    if (Console.KeyAvailable)
                    {
                        var userInput = Console.ReadKey(true);
                        switch (userInput.Key)
                        {
                            case ConsoleKey.Y:
                                Util.WriteDarkYellow($"Delete in progress ...");
                                var sdkResult = await RbAdministration.DeleteUserAsync(userId);
                                if (sdkResult.Success)
                                    Util.WriteGreen($"user has been deleted");
                                else
                                    Util.WriteRed($"DeleteUserAsync - Error:[{sdkResult.Result}]");
                                return;

                            default:
                                Util.WriteGreen($"user has been deleted");
                                return;
                        }
                    }
                    await Task.Delay(200);
                }
            }
        }
        else
        {
            Util.WriteYellow($"Cancel user search ...");
            canContinue = false;
        }
    }
}

void MenuDisplayInfo()
{
    Util.WriteYellow("");
    Util.WriteYellow("[ESC] to quit");
    Util.WriteYellow("[I] Display this [I]nfo");

    Util.WriteYellow("");
    Util.WriteYellow("[O] Select working [O]rganisation");
    Util.WriteYellow("[C] Select working [C]ompany");

    Util.WriteYellow("");
    Util.WriteYellow("[L] Manage Company [L]ink");
    Util.WriteYellow("[B] Manage [B]ubble link");

    Util.WriteYellow("");
    Util.WriteYellow("[U] Manage [U]ser");
    Util.WriteYellow("[G] Manage [G]uest User");
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

Credentials? ReadCredentials(string fileName = "credentials.json")
{
    var credentialsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}{fileName}";
    if (!File.Exists(credentialsFilePath))
    {
        Util.WriteRed($"The file '{credentialsFilePath}' has not been found.");
        return null;
    }

    String jsonConfig = File.ReadAllText(credentialsFilePath);
    var jsonNode = JSON.Parse(jsonConfig);

    if (!Credentials.FromJsonNode(jsonNode["credentials"], out Credentials credentials))
    {
        Util.WriteRed($"Cannot read 'credentials' object OR invalid/missing data in file:[{fileName}].");
        return null;
    }

    return credentials;
}
