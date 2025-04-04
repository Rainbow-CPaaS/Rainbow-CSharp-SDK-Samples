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
        return ParseConfigFile();
    }

    private static Boolean ParseConfigFile()
    {
        return ReadExeSettings() && ReadCredentials();
        //return ReadExeSettings() && ReadCredentials() && ReadStreamsSettings();
/*
        String configFilePath = GetConfigFilePath();

        if (!File.Exists(configFilePath))
        {
            _errorMessage += $"This file doesn't exist:\r\n\t{configFilePath}";
            return false;
        }

        try
        {
            String jsonConfig = File.ReadAllText(configFilePath);
            var json = JSON.Parse(jsonConfig);

            if (json == null)
            {
                _errorMessage += $"Cannot get JSON data from file:\r\n\t{configFilePath}";
                return false;
            }


            // HostName, AppId, AppSecret
            if (NeedsRainbowServerConfiguration)
            {
                if (json["serverConfig"] != null)
                {
                    RainbowServerConfiguration = new();
                    var jobject = json["serverConfig"];

                    RainbowServerConfiguration.AppId = jobject["appId"];
                    CheckDefaultValue(RainbowServerConfiguration.AppId, "appId");

                    RainbowServerConfiguration.AppSecret = jobject["appSecret"];
                    CheckDefaultValue(RainbowServerConfiguration.AppSecret, "appSecret");

                    RainbowServerConfiguration.HostName = jobject["hostname"];
                    CheckDefaultValue(RainbowServerConfiguration.HostName, "hostname");
                }
                else
                    _errorMessage += $"\r\nCannot read 'serverConfig' property in config.json";
            }

            // S2SCallbackURL
            S2SCallbackURL = json["s2sCallbackURL"];

            // Accounts
            if (NeedsRainbowAccounts)
            {
                if (json["accounts"].IsArray == true)
                {
                    int index = 1;
                    foreach (JSONNode jobject in json["accounts"])
                    {
                        RainbowAccount rainbowAccount = new();

                        if (jobject["botName"] != null)
                            rainbowAccount.BotName = jobject["botName"];
                        else
                            _errorMessage += $"\r\nCannot read 'botName' in 'accounts' for index:[{index}]";

                        if (jobject["login"] != null)
                            rainbowAccount.Login = jobject["login"];
                        else
                            _errorMessage += $"\r\nCannot read 'login' in 'accounts' for index:[{index}]";

                        if (jobject["password"] != null)
                            rainbowAccount.Password = jobject["password"];
                        else
                            _errorMessage += $"\r\nCannot read 'password' in 'accounts' for index:[{index}]";

                        if (jobject["autoLogin"] != null)
                            rainbowAccount.AutoLogin = jobject["autoLogin"];
                        else
                            _errorMessage += $"\r\nCannot read 'autoLogin' in 'accounts' for index:[{index}]";

                        rainbowAccount.UseS2S = jobject["useS2S"];
                        

                        RainbowAccounts.Add(rainbowAccount);
                    }
                }
                else
                    _errorMessage += $"\r\nCannot read 'accounts' property in config.json";
            }
        }
        catch (Exception exc)
        {
            _errorMessage += $"\r\nException:[{Rainbow.Util.SerializeException(exc)}]";
        }

        if (String.IsNullOrEmpty(_errorMessage))
            return true;

        _errorMessage = $"Pb occured while reading/parsing this file:\r\n\t{configFilePath}" + _errorMessage;
        return false;
*/
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

