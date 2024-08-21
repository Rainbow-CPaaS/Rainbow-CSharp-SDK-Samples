using Rainbow;
using Rainbow.SimpleJSON;

public static class Configuration
{
    public static Boolean NeedsRainbowServerConfiguration = false;  // True to use Rainbow Server Configuration - AppId, AppSecret and HostName
    public static Boolean NeedsRainbowAccounts = false;             // True to use one or several Rainbow Accounts
    public static Boolean UseSplittedView = false;                  // True to use several Rainbow Accounts, False to use only the first one

    public static RainbowServerConfiguration RainbowServerConfiguration = new();
    public static List<RainbowAccount> RainbowAccounts = [];

    private static readonly String DEFAULT_VALUE = "TO DEFINE";
    private static String _errorMessage = "";

    public static String GetErrorMessage()
    {
        return _errorMessage;
    }

    public static Boolean Initialize()
    {
        return ParseConfigFile();
    }

    private static String GetConfigFilePath()
    {
        return Path.GetFullPath($".{Path.DirectorySeparatorChar}Resources{Path.DirectorySeparatorChar}config.json");
    }

    private static void CheckDefaultValue(String val, String propertyName)
    {
        if (String.IsNullOrEmpty(val))
            _errorMessage += $"\r\nProperty '{propertyName}' is not defined or has an empty value";
        else if (val.Equals(DEFAULT_VALUE, StringComparison.InvariantCultureIgnoreCase))
            _errorMessage += $"\r\nProperty '{propertyName}' has a default value - set a correct one";
    }

    private static Boolean ParseConfigFile()
    {
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

                    RainbowServerConfiguration.AppId = UtilJson.AsString(jobject, "appId");
                    CheckDefaultValue(RainbowServerConfiguration.AppId, "appId");

                    RainbowServerConfiguration.AppSecret = UtilJson.AsString(jobject, "appSecret");
                    CheckDefaultValue(RainbowServerConfiguration.AppSecret, "appSecret");

                    RainbowServerConfiguration.HostName = UtilJson.AsString(jobject, "hostname");
                    CheckDefaultValue(RainbowServerConfiguration.HostName, "hostname");
                }
                else
                    _errorMessage += $"\r\nCannot read 'serverConfig' property in config.json";
            }

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
                            rainbowAccount.BotName = UtilJson.AsString(jobject, "botName");
                        else
                            _errorMessage += $"\r\nCannot read 'botName' in 'accounts' for index:[{index}]";

                        if (jobject["login"] != null)
                            rainbowAccount.Login = UtilJson.AsString(jobject, "login");
                        else
                            _errorMessage += $"\r\nCannot read 'login' in 'accounts' for index:[{index}]";

                        if (jobject["password"] != null)
                            rainbowAccount.Password = UtilJson.AsString(jobject, "password");
                        else
                            _errorMessage += $"\r\nCannot read 'password' in 'accounts' for index:[{index}]";

                        if (jobject["autoLogin"] != null)
                            rainbowAccount.AutoLogin = UtilJson.AsBoolean(jobject, "autoLogin");
                        else
                            _errorMessage += $"\r\nCannot read 'autoLogin' in 'accounts' for index:[{index}]";

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
    }
}

public class RainbowServerConfiguration
{
    public String AppId = "";
    public String AppSecret = "";
    public String HostName = "";
}

public class RainbowAccount
{
    public String BotName = "";
    public String Login = "";
    public String Password = "";
    public Boolean AutoLogin = false;
}
