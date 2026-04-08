
using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;
using System.Text;

using Rainbow.Example.Common;

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

// Set folder / directory path
NLogConfigurator.Directory = exeSettings.LogFolderPath;
var logFullPath = Path.GetFullPath(exeSettings.LogFolderPath);
ConsoleAbstraction.WriteBlue($"Logs files will be stored in folder:[{logFullPath}]");

Rainbow.Util.SetLogAnonymously(false);

// ------------------------------------------------

// Log with first account
NLogConfigurator.AddLogger(credentials.UsersConfig[0].Prefix);

// Set restrictions
Restrictions restrictions = new(true)
{
    LogRestRequest = true,
    LogEvent = true,
    LogEventParameters = true,
    LogEventRaised = true,
};

// Create Rainbow SDK objects
var RbApplication = new Application(iniFolderFullPathName: exeSettings.LogFolderPath, iniFileName: credentials.UsersConfig[0].Prefix + ".ini", loggerPrefix: credentials.UsersConfig[0].Prefix, restrictions: restrictions);
var RbContacts = RbApplication.GetContacts();
var RbBubbles = RbApplication.GetBubbles();
var RbAdministration = RbApplication.GetAdministration();

// Set global configuration info
RbApplication.SetApplicationInfo(credentials.ServerConfig.AppId, credentials.ServerConfig.AppSecret);
RbApplication.SetHostInfo(credentials.ServerConfig.HostName);

ConsoleAbstraction.WriteRed($"{CR}Account used: [{credentials.UsersConfig[0].Login}]");
ConsoleAbstraction.WriteDarkYellow($"Logging in progress ...");
var sdkResult = await RbApplication.LoginAsync(credentials.UsersConfig[0].Login, credentials.UsersConfig[0].Password);

if(!sdkResult.Success)
{
    ConsoleAbstraction.WriteRed($"Cannot login account:[{sdkResult.Result}] - Error:[{sdkResult.Result}]");
    return;
}
else
{
    // Check roles of this account
    var roles = RbApplication.Roles;

    ConsoleAbstraction.WriteGreen($"{CR}Roles of this account:[{String.Join(", ", roles)}]");
    var adminRoles = roles.Where(s => s.Contains("admin")).ToList();
    if (adminRoles.Count == 0)
    {
        ConsoleAbstraction.WriteRed($"Without admin right");
        ConsoleAbstraction.WriteRed($"\tyou can create User using Company Link (but you need to have a valid one first. They can be created only with admin right)");
        ConsoleAbstraction.WriteRed($"\tyou can create GuestMode using Bubble Link");
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
    while (ConsoleAbstraction.KeyAvailable)
    {
        var userInput = ConsoleAbstraction.ReadKey();

        switch (userInput?.Key)
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
                ConsoleAbstraction.WriteYellow($"Asked to end process using [ESC] key");
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

    ConsoleAbstraction.WriteDarkYellow($"{CR}Asking server list of organisations ...");
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
            ConsoleAbstraction.WriteRed($"GetOrganisationsAsync - Error:[{sdkResult.Result}]");
            return;
        }
    }

    if(organisationsList.Count == 0)
    {
        ConsoleAbstraction.WriteYellow($"This account cannot manage organization.");
        organisationManaged = null;
        noneOrganization = true;
        return;
    }

    noneOrganization = false;
    ConsoleAbstraction.WriteYellow($"The account can manage [{organisationsList.Count}] organization(s)");
    if(organisationsList.Count == 1)
    {
        organisationManaged = organisationsList[0];
        ConsoleAbstraction.WriteYellow($"Organization managed: [{organisationManaged.Name}]");
        return;
    }

    if(organisationManaged is not null)
        ConsoleAbstraction.WriteYellow($"{CR}Current Organization managed: [{organisationManaged.Name}]");

    canContinue = true;
    while (canContinue)
    {
        ConsoleAbstraction.WriteYellow($"{CR}Enter a text to search an organization (starts with) - empty string to cancel");
        var str = ConsoleAbstraction.ReadLine();
        if (!String.IsNullOrEmpty(str))
        {
            var result = organisationsList.Find(organisation => organisation.Name.StartsWith(str, StringComparison.InvariantCultureIgnoreCase));
            if (result is null)
                ConsoleAbstraction.WriteYellow($"No organization found ...");
            else
            {
                if(result.Id != organisationManaged?.Id)
                {
                    if (companyManaged is not null)
                    {
                        ConsoleAbstraction.WriteYellow($"Previous company managed is no more used since the managed Organization has changed");
                        companyManaged = null;
                    }
                }
                organisationManaged = result;
                ConsoleAbstraction.WriteYellow($"Organization found and now managed: [{organisationManaged.Name}]");
                return;
            }
        }
        else
        {
            ConsoleAbstraction.WriteYellow($"Cancel organization search ...");
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
            ConsoleAbstraction.WriteRed($"{CR}No Organization managed yet ...");
            return;
        }
    }

    int offset = 0;
    int limit = 100;
    Boolean canContinue = true;
    List<Company> companiesList = new ();

    if(organisationManaged is null)
        ConsoleAbstraction.WriteDarkYellow($"{CR}Asking server list of companies ...");
    else
        ConsoleAbstraction.WriteDarkYellow($"{CR}Asking server list of companies for Organization [{organisationManaged.Name}] ...");
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
            ConsoleAbstraction.WriteRed($"GetCompaniesAsync - Error:[{sdkResult.Result}]");
            return;
        }
    }

    ConsoleAbstraction.WriteYellow($"The account can manage [{companiesList.Count}] companies");
    if (companiesList.Count == 1)
    {
        companyManaged = companiesList[0];
        ConsoleAbstraction.WriteYellow($"Company managed: [{companyManaged.Name}]");
        return;
    }

    if (companyManaged is not null)
        ConsoleAbstraction.WriteYellow($"{CR}Current Company managed: [{companyManaged.Name}]");

    canContinue = true;
    while (canContinue)
    {
        ConsoleAbstraction.WriteYellow($"{CR}Enter a text to search a company (starts with) - empty string to cancel");
        var str = ConsoleAbstraction.ReadLine();
        if (!String.IsNullOrEmpty(str))
        {
            var result = companiesList.Find(company => company.Name.StartsWith(str, StringComparison.InvariantCultureIgnoreCase));
            if (result is null)
                ConsoleAbstraction.WriteYellow($"No company found ...");
            else
            {
                companyManaged = result;
                ConsoleAbstraction.WriteYellow($"Company found and now managed: [{companyManaged.Name}]");
                return;
            }
        }
        else
        {
            ConsoleAbstraction.WriteYellow($"Cancel company search ...");
            canContinue = false;
        }
    }


}

async Task MenuCompanyLinkAsync()
{
    if (companyManaged is null)
    {
        ConsoleAbstraction.WriteYellow($"{CR}No Company managed yet ...");
        await MenuCompagniesAsync();
        if (companyManaged is null)
            companyManaged = RbContacts.GetCompany();
    }

    int offset = 0;
    int limit = 100;
    Boolean canContinue = true;
    List<JoinCompanyLink> joinCompanyLinkList = new();

    Boolean isAdmin = true;

    ConsoleAbstraction.WriteDarkYellow($"{CR}Asking server list of join company link for Company [{companyManaged.Name}] ...");
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
            ConsoleAbstraction.WriteRed($"GetJoinCompanyLinksAsync - Error:[{sdkResult.Result}]");

            ConsoleAbstraction.WriteGreen($"{CR}You can still create a user if you have already a valid Company Link ");
            canContinue = false;
        }
    }
 
    if (joinCompanyLinkList.Count > 0)
    {
        ConsoleAbstraction.WriteYellow($"{CR}List of Join Company Link - Total:[{joinCompanyLinkList.Count}]:");
        foreach(var jcl in joinCompanyLinkList)
            ConsoleAbstraction.WriteDarkYellow($"\t{Rainbow.Util.LogOnOneLine(jcl.ToString(DetailsLevel.Medium))}");
    }
    else
    {
        if(isAdmin)
            ConsoleAbstraction.WriteDarkYellow($"{CR}No Join Company Link.");
    }

    Boolean listLinks = false;

    if (isAdmin)
        ConsoleAbstraction.WriteYellow($"\t [A] Add a Join Company Link");
    if (joinCompanyLinkList.Count > 0)
        ConsoleAbstraction.WriteYellow($"\t [D] Delete a Join Company Link");
    ConsoleAbstraction.WriteYellow($"\t [U] Create [U]ser with Join Company Link");
    if (isAdmin)
        ConsoleAbstraction.WriteYellow($"\t [L] List Join Company Link");
    ConsoleAbstraction.WriteYellow($"\t [C] Cancel");

    String str;
    String id;

    canContinue = true;
    do
    {
        while (ConsoleAbstraction.KeyAvailable)
        {
            var userInput = ConsoleAbstraction.ReadKey();

            switch (userInput?.Key)
            {
                case ConsoleKey.A:
                    if (isAdmin)
                    {
                        ConsoleAbstraction.WriteDarkYellow($"{CR}Asking server to create a join company link for Company [{companyManaged.Name}] ...");
                        var createSdkResult = await RbAdministration.CreateJoinCompanyLinkAsync(companyManaged.Id, 20);
                        if (createSdkResult.Success)
                            ConsoleAbstraction.WriteYellow($"JoinCompanyLink created: [{createSdkResult.Data.ToString(DetailsLevel.Medium)}]");
                        else
                            ConsoleAbstraction.WriteRed($"CreateJoinCompanyLinkAsync - Error:[{createSdkResult.Result}]");

                        listLinks = true;
                    }
                    break;

                case ConsoleKey.U:
                    ConsoleAbstraction.WriteYellow($"{CR}Enter an Id of a join company link or empty string to use it to create an user");
                    str = ConsoleAbstraction.ReadLine();
                    id = "";
                    if (String.IsNullOrEmpty(str))
                        id = joinCompanyLinkList[0].Id;
                    else
                        id = str;

                    String loginEmail = Rainbow.Util.GetGUID().Substring(0, 16) + "@sdk.drop.me";
                    String pwd = Rainbow.Util.GetGUID().ToUpper() + "!a";
                    String firstName = Rainbow.Util.GetGUID().Substring(0, 5);
                    String lastName ="USER company link";

                    ConsoleAbstraction.WriteDarkYellow($"Creating user with Join Company Link ...");
                    var sdkResultContact = await RbAdministration.CreateUserWithCompanyLinkAsync(id, loginEmail, pwd, firstName, lastName, null, true);
                    if (sdkResultContact.Success)
                    {
                        var contactCreated = sdkResultContact.Data;
                        ConsoleAbstraction.WriteBlue($"Contact created - Login:[{loginEmail}] - Pwd:[{pwd}] - {Rainbow.Util.LogOnOneLine(contactCreated.ToString(DetailsLevel.Medium))}");
                    }
                    else
                        ConsoleAbstraction.WriteRed($"CreateUserWithCompanyLinkAsync - Error:[{sdkResultContact.Result}]");

                    listLinks = true;
                    break;

                case ConsoleKey.D:
                    if (joinCompanyLinkList.Count > 0)
                    {
                        ConsoleAbstraction.WriteYellow($"{CR}Enter an Id of a join company link or empty string to delete the first one");
                        str = ConsoleAbstraction.ReadLine();
                        id = "";
                        if (String.IsNullOrEmpty(str))
                            id = joinCompanyLinkList[0].Id;
                        else
                            id = str;
                        ConsoleAbstraction.WriteDarkYellow($"Deleting Join Company Link ...");
                        var deleteSdkResult = await RbAdministration.DeleteJoinCompanyLinkAsync(companyManaged.Id, id);
                        if (deleteSdkResult.Success)
                            ConsoleAbstraction.WriteYellow($"JoinCompanyLink deleted");
                        else
                            ConsoleAbstraction.WriteRed($"DeleteJoinCompanyLinkAsync - Error:[{deleteSdkResult.Result}]");

                        listLinks = true;
                    }
                    break;

                case ConsoleKey.L:
                    if (isAdmin)
                        listLinks = true;
                    break;

                case ConsoleKey.C:
                    ConsoleAbstraction.WriteYellow($"Cancelled ...");
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
        ConsoleAbstraction.WriteDarkYellow("This account has no bubble as Owner or Moderator. Creating one ...");
        var sdkResultBubble = await RbBubbles.CreateBubbleAsync("Test from SDK C#", "", BubbleVisibility.AsPublic);
        if(sdkResultBubble.Success)
        {
            var bubble = sdkResultBubble.Data;
            ConsoleAbstraction.WriteGreen($"Bubble created:[{Rainbow.Util.LogOnOneLine(bubble.ToString(DetailsLevel.Medium))}]");
        }
        else
        {
            ConsoleAbstraction.WriteRed($"CreateBubbleAsync - Error:[{sdkResultBubble.Result}]");
            return;
        }
    }

    bubblesList = RbBubbles.GetAllBubbles(bubbleMemberPrivilegeList: bubbleMemberPrivilegeList);
    if (bubblesList.Count == 0)
    {
        ConsoleAbstraction.WriteRed("This account has no bubble as Owner or Moderator ...");
        return;
    }
    if (bubblesList.Count == 1)
    {
        bubbleManaged = bubblesList[0];
    }
    else
    {
        ConsoleAbstraction.WriteYellow($"List of Bubbles (as Owner or Moderator) - Total[{bubblesList.Count}]");
        foreach (var bubble in bubblesList)
        {
            ConsoleAbstraction.WriteYellow($"\tId:[{bubble.Peer.Id}] - Name:[{bubble.Peer.DisplayName}]");
        }

        ConsoleAbstraction.WriteYellow($"{CR}Enter an Id of bubble or empty string to use the first one");
        var str = ConsoleAbstraction.ReadLine();
        if (String.IsNullOrEmpty(str))
            str = bubblesList[0].Peer.Id;
        bubbleManaged = bubblesList.FirstOrDefault(b => b.Peer.Id == str);
        if (bubbleManaged is null)
        {
            ConsoleAbstraction.WriteRed($"No bubble found with this id:[{str}]");
            return;
        }
    }
    ConsoleAbstraction.WriteYellow($"Checking if this bubble [{bubbleManaged.Peer.DisplayName}] has already a 'bubble link' ...");

    var sdkResultString = await RbBubbles.GetBubbleLinkAsync(bubbleManaged);
    if(sdkResultString.Success)
    {
        var link = sdkResultString.Data;
        if(link is null)
        {
            ConsoleAbstraction.WriteDarkYellow($"No bubble link yet. Creating one ...");
            sdkResultString = await RbBubbles.CreateBubbleLinkAsync(bubbleManaged);
            if (sdkResultString.Success)
            {
                link = sdkResultString.Data;
                ConsoleAbstraction.WriteGreen($"Bubble link created.");
            }
            else
            {
                ConsoleAbstraction.WriteRed($"CreateBubbleLinkAsync - Error:[{sdkResultString.Result}]");
                return;
            }
        }

        ConsoleAbstraction.WriteGreen($"Bubble [{bubbleManaged.Peer.DisplayName}] has a bubble link:[{link}]");

        ConsoleAbstraction.WriteDarkYellow($"Creating a User (a GuestMode) using the bubble Link ...");
        String loginEmail = Rainbow.Util.GetGUID().Substring(0, 16) + "@sdk.drop.me";
        String pwd = Rainbow.Util.GetGUID().ToUpper() + "!a";
        String firstName = Rainbow.Util.GetGUID().Substring(0, 5);
        String lastName = "USER bubble link";
                
        var sdkResultContact = await RbAdministration.CreateUserWithBubbleLinkAsync(link, loginEmail, pwd, firstName, lastName, null, null, true);
        if (sdkResultContact.Success)
        {
            var contactCreated = sdkResultContact.Data;
            ConsoleAbstraction.WriteBlue($"Contact created - Login:[{loginEmail}] - Pwd:[{pwd}] - {Rainbow.Util.LogOnOneLine(contactCreated.ToString(DetailsLevel.Medium))}");
        }
        else
            ConsoleAbstraction.WriteRed($"CreateUserWithBubbleLinkAsync - Error:[{sdkResultContact.Result}]");
    }
    else
    {
        ConsoleAbstraction.WriteRed($"GetBubbleLinkAsync - Error:[{sdkResultString.Result}]");
        return;
    }
}

async Task MenuUserAsync()
{
    if (companyManaged is null)
    {
        ConsoleAbstraction.WriteYellow($"{CR}No Company managed yet ...");
        await MenuCompagniesAsync();
        if (companyManaged is null)
        {
            ConsoleAbstraction.WriteRed($"{CR}No Company managed yet ...");
            return;
        }
    }

    ConsoleAbstraction.WriteYellow($"{CR}Do you want to [A]dd, [L]ist/delete users in company:[{companyManaged.Name}] or [C]ancel ?");

    Boolean canContinue = true;
    while (canContinue)
    {
        if (ConsoleAbstraction.KeyAvailable)
        {
            var userInput = ConsoleAbstraction.ReadKey();
            switch (userInput?.Key)
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
                    ConsoleAbstraction.WriteDarkYellow("Cancelled ...");
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
        ConsoleAbstraction.WriteYellow($"{CR}No Company managed yet ...");
        await MenuCompagniesAsync();
        if (companyManaged is null)
        {
            ConsoleAbstraction.WriteRed($"{CR}No Company managed yet ...");
            return;
        }
    }

    String firstName = Rainbow.Util.GetGUID().Substring(0, 5);
    String lastName ="USER";

    ConsoleAbstraction.WriteDarkYellow("Creating user ...");
    String loginEmail = Rainbow.Util.GetGUID().Substring(0, 16) + "@sdk.drop.me";
    String pwd = Rainbow.Util.GetGUID().ToUpper() + "!a";

    var sdkResultContact = await RbAdministration.CreateUserAsync(loginEmail, pwd,  firstName, lastName, null, companyManaged.Id, true, false);
    if (sdkResultContact.Success)
    {
        var contact = sdkResultContact.Data;
        ConsoleAbstraction.WriteGreen($"User created - Login:[{loginEmail}] - Pwd:[{pwd}] - {Rainbow.Util.LogOnOneLine(contact.ToString(DetailsLevel.Medium))}");
    }
    else
        ConsoleAbstraction.WriteRed($"CreateUserAsync - Error:[{sdkResultContact.Result}]");
}

async Task MenuListUserAsync()
{
    Boolean canContinue = true;
    int offset = 0;
    int limit = 100;
    List<Contact> usersList = [];

    List<String>? roles = null;

    ConsoleAbstraction.WriteDarkYellow($"{CR}Asking list of users for Company [{companyManaged.Name}] ...");

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
            ConsoleAbstraction.WriteRed($"GetUsersAsync - Error:[{sdkResult.Result}]");
            return;
        }
    }

    if (usersList.Count == 0)
    {
        ConsoleAbstraction.WriteGreen($"None user ...");
        return;
    }

    ConsoleAbstraction.WriteYellow($"{CR}List of Users - Total[{usersList.Count}]):");
    foreach (var contact in usersList)
    {
        ConsoleAbstraction.WriteYellow($"\t- {Rainbow.Util.LogOnOneLine(contact.ToString(DetailsLevel.Medium))}");
    }

    canContinue = true;
    while (canContinue)
    {
        ConsoleAbstraction.WriteYellow($"{CR}Enter Id to select a user - empty string to cancel");

        var str = ConsoleAbstraction.ReadLine();
        if (!String.IsNullOrEmpty(str))
        {
            var result = usersList.Find(contact => contact.Peer.Id.Equals(str, StringComparison.InvariantCultureIgnoreCase));

            if (result is null)
                ConsoleAbstraction.WriteYellow($"None user ...");
            else
            {
                userManaged = result;
                ConsoleAbstraction.WriteYellow($"User found and now managed: [{userManaged.ToString(DetailsLevel.Medium)}]");

                ConsoleAbstraction.WriteRed($"Do you want to delete it ? (can not be undone !) [Y]");
                string userId = userManaged.Peer.Id;
                while (true)
                {
                    if (ConsoleAbstraction.KeyAvailable)
                    {
                        var userInput = ConsoleAbstraction.ReadKey();
                        switch (userInput?.Key)
                        {
                            case ConsoleKey.Y:
                                ConsoleAbstraction.WriteDarkYellow($"Delete in progress ...");
                                var sdkResult = await RbAdministration.DeleteUserAsync(userId);
                                if (sdkResult.Success)
                                    ConsoleAbstraction.WriteGreen($"user has been deleted");
                                else
                                    ConsoleAbstraction.WriteRed($"DeleteUserAsync - Error:[{sdkResult.Result}]");
                                return;

                            default:
                                ConsoleAbstraction.WriteGreen($"user has been deleted");
                                return;
                        }
                    }
                    await Task.Delay(200);
                }
            }
        }
        else
        {
            ConsoleAbstraction.WriteYellow($"Cancel user search ...");
            canContinue = false;
        }
    }
}

void MenuDisplayInfo()
{
    ConsoleAbstraction.WriteYellow("");
    ConsoleAbstraction.WriteYellow("[ESC] to quit");
    ConsoleAbstraction.WriteYellow("[I] Display this [I]nfo");

    ConsoleAbstraction.WriteYellow("");
    ConsoleAbstraction.WriteYellow("[O] Select working [O]rganisation");
    ConsoleAbstraction.WriteYellow("[C] Select working [C]ompany");

    ConsoleAbstraction.WriteYellow("");
    ConsoleAbstraction.WriteYellow("[L] Manage Company [L]ink");
    ConsoleAbstraction.WriteYellow("[B] Manage [B]ubble link");

    ConsoleAbstraction.WriteYellow("");
    ConsoleAbstraction.WriteYellow("[U] Manage [U]ser");
    ConsoleAbstraction.WriteYellow("[G] Manage [G]uest User");
}

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

Credentials? ReadCredentials(string fileName = "credentials.json")
{
    var credentialsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}{fileName}";
    if (!File.Exists(credentialsFilePath))
    {
        ConsoleAbstraction.WriteRed($"The file '{credentialsFilePath}' has not been found.");
        return null;
    }

    String jsonConfig = File.ReadAllText(credentialsFilePath);
    var jsonNode = JSON.Parse(jsonConfig);

    if (!Credentials.FromJsonNode(jsonNode["credentials"], out Credentials credentials))
    {
        ConsoleAbstraction.WriteRed($"Cannot read 'credentials' object OR invalid/missing data in file:[{fileName}].");
        return null;
    }

    return credentials;
}
