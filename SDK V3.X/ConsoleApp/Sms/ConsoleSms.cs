
using Rainbow;
using System.Text;

using Rainbow.Example.Common;
using Util = Rainbow.Example.Common.Util;
using Rainbow.SimpleJSON;
using Rainbow.Consts;

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
var RbSms = RbApplication.GetSms();

// Set global configuration info
RbApplication.SetApplicationInfo(credentials.ServerConfig.AppId, credentials.ServerConfig.AppSecret);
RbApplication.SetHostInfo(credentials.ServerConfig.HostName);


// Start login
Util.WriteWhite("Starting login ...");
var sdkResult = await RbApplication.LoginAsync(credentials.UsersConfig[0].Login, credentials.UsersConfig[0].Password);

if (sdkResult.Success)
{
    // We are connected and SDK is ready
    Util.WriteGreen($"{CR}Connected to Rainbow Server");
}
else
{
    // We are not connected - Display why:
    Util.WriteRed($"{CR}Connection failed - SdkError:{sdkResult.Result}");
}

Util.WriteBlue($"{CR}Use [ESC] at anytime to quit");
Util.WriteBlue($"{CR}Use [S] to send SMS");

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
                Util.WriteYellow($"Asked to end process using [ESC] key");
                Environment.Exit(0);
                return;

            case ConsoleKey.S:
                await SendSmdAsync();
                return;
        }
    }
}

async Task SendSmdAsync()
{
    Util.WriteWhite("Enter Phone number: (E.123 international notation format - like \"+33 01 23 45 67 89\" (with \"+33\" for telephone country codes for France, \"+49\" for Germany etc ...)");
    var phoneNumber = Console.ReadLine();

    Util.WriteWhite("Enter Region: (two uppercase letters as defined in ISO 3166-1 alpha-2 - like \"FR\" for France or \"DE\" for Germany");
    var region = Console.ReadLine();

    Util.WriteWhite("Enter Message:");
    var message = Console.ReadLine();

    Util.WriteWhite("With confirmation ? Y/N:");
    var confirmation = Console.ReadKey(true).Key == ConsoleKey.Y;

    SdkResult<String> sdkResultString;
    if (confirmation)
    {
        Util.WriteGreen($"Trying to send SMS with confirmation ...");
        sdkResultString = await RbSms.SendSmsAndWaitSentConfirmationStatusAsync(phoneNumber, region, message);
    }
    else
    {
        Util.WriteGreen($"Trying to send SMS without confirmation ...");
        sdkResultString = await RbSms.SendSmsAsync(phoneNumber, region, message);
    }

    if (sdkResultString.Success)
        Util.WriteBlue($"SMS sent status:[{sdkResultString.Data}]");
    else
        Util.WriteRed($"Cannot send SMS - Error[{sdkResultString.Result}]");
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
