using Rainbow.Example.Common;
using Rainbow.SimpleJSON;

using Util = Rainbow.Example.Common.Util;

public static class Configuration
{
    public static Boolean NeedsRainbowServerConfiguration = false;  // True to use Rainbow Server Configuration - AppId, AppSecret and HostName
    public static Boolean NeedsRainbowAccounts = false;             // True to use one or several Rainbow Accounts
    public static Boolean UseSplittedView = false;                  // True to use several Rainbow Accounts, False to use only the first one

    public static ExeSettings ExeSettings = new();
    public static Credentials Credentials = new();

    public static Boolean Initialize()
    {
        Tools.InitTerminalGuiApplication();
        return ParseConfigFile();
    }

    private static Boolean ParseConfigFile()
    {
        return ReadExeSettings() && ReadCredentials();
    }

    private static Boolean ReadExeSettings()
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

        if (ExeSettings.FromJsonNode(jsonNode["exeSettings"], out ExeSettings))
        {
            // Set where log files must be stored
            NLogConfigurator.Directory = ExeSettings.LogFolderPath;
        }
        else
        {
            Util.WriteRed($"Cannot read 'exeSettings' object OR invalid/missing data - file:'{exeSettingsFilePath}'.");
            return false;
        }

        return true;
    }

    private static Boolean ReadCredentials()
    {
        var credentialsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}credentials.json";
        if (!File.Exists(credentialsFilePath))
        {
            Util.WriteRed($"The file '{credentialsFilePath}' has not been found.");
            return false;
        }

        String jsonConfig = File.ReadAllText(credentialsFilePath);
        var jsonNode = JSON.Parse(jsonConfig);

        if (!Credentials.FromJsonNode(jsonNode["credentials"], out Credentials))
        {
            Util.WriteRed($"Cannot read 'credentials' object OR invalid/missing data.");
            return false;
        }

        return true;
    }
}

