using Rainbow.SimpleJSON;

public sealed class Configuration
{
    private static Boolean CHECK_APPID = true;
    private static Boolean CHECK_ACCOUNT = true;
    private static Boolean CHECK_VIDEOS = false;

    private static Configuration? _instance;
        
    private Boolean _initialized = false;
    private String _errorMessage = "";

    private const String TO_DEFINE = "TO DEFINE";

    private String _logsFolderPath;

    private String _hostname;
    private String _appId;
    private String _appSecret;

    private RainbowAccount _rbAdmin;
    private List<RainbowAccount> _rbAccounts;

    public static Configuration Instance
    {
        get {
            if(_instance == null)
                _instance = new Configuration();
            return _instance;
        }
    }

    public Boolean Initialized{ get => _initialized; }
    public String ErrorMessage { get => _errorMessage; }

    public String LogsFolderPath { get => _logsFolderPath; }

    public String HostName { get => _hostname; }
    public String AppId { get => _appId; }
    public String AppSecret { get => _appSecret; }

    public RainbowAccount RbAdmin { get => _rbAdmin; }

    public List<RainbowAccount> RbAccounts { get => _rbAccounts; }

    private Configuration()
    {
        if (ParseConfigFile() && CheckApplicationInfo() && CheckRainbowInfo() && CheckAccounts())
        {
            if (!InitLogsWithNLog())
                _errorMessage = "Cannot initialize logs using NLog configuration file\r\n" + _errorMessage;
            else
                _initialized = true;
        }
    }

    private Boolean IsValueCorrect(String? value, Boolean notAllowEmpty = true)
    {
        if(value == null)
            return false;

        if (String.IsNullOrEmpty(value) && notAllowEmpty)
            return false;

        if (value.Equals(TO_DEFINE, StringComparison.InvariantCultureIgnoreCase))
            return false;

        return true;
    }

    private Boolean CheckApplicationInfo()
    {
        return true;
    }

    private Boolean CheckRainbowInfo()
    {
        if (!CHECK_APPID)
            return true;

        if (IsValueCorrect(AppId) && IsValueCorrect(AppSecret) && IsValueCorrect(HostName))
            return true;

        String configFilePath = GetConfigFilePath();
        _errorMessage += $"In this file:\r\n\t{configFilePath}\r\n\r\n In object 'serverConfig': 'appId', 'appSecret' and/or 'hostName' properties have not been correctly defined ...";
        return false;
    }

    private Boolean CheckAccounts()
    {
        if (!CHECK_ACCOUNT)
            return true;

        String configFilePath = GetConfigFilePath();

        if ( (_rbAccounts == null) || _rbAccounts.Count == 0)
        {
            _errorMessage += $"In this file:\r\n\t{configFilePath}\r\n\r\n In object 'usersConfig': Rainbow accounts have not been correctly defined ...";
            return false;
        }

        // Check unicity or Prefix and Login
        Boolean result = true;

        var nbDistinctLogin = _rbAccounts.Select(a => a.Login).Distinct().Count();
        if (_rbAccounts.Count != nbDistinctLogin)
        {
            _errorMessage += $"In this file:\r\n\t{configFilePath}\r\n\r\n In object 'usersConfig': same LOGIN are set - found [{nbDistinctLogin}] distinct LOGIN instead of [{_rbAccounts.Count}]";
            result = false;
        }

        var nbDistinctPrefix = _rbAccounts.Select(a => a.Prefix).Distinct().Count();
        if (_rbAccounts.Count != nbDistinctPrefix)
        {
            _errorMessage += $"In this file:\r\n\t{configFilePath}\r\n\r\n In object 'usersConfig': same PREFIX are set - found [{nbDistinctPrefix}] distinct PREFIX instead of [{_rbAccounts.Count}]";
            result = false;
        }
        return result;
    }

    private String GetConfigFilePath()
    {
        return Path.GetFullPath($".{Path.DirectorySeparatorChar}Resources{Path.DirectorySeparatorChar}config.json");
    }

    private Boolean ParseConfigFile()
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

            // LogsFolderPath
            if (json["applicationConfig"] != null)
            {
                var jobject = json["applicationConfig"];

                if (jobject["logsFolderPath"] != null)
                    _logsFolderPath = jobject["logsFolderPath"];
                else
                    _errorMessage += $"\r\nCannot read 'logsFolderPath' property in 'applicationConfig' object";
            }

            // HostName, AppId, AppSecret
            if (json["serverConfig"] != null)
            {
                var jobject = json["serverConfig"];

                if (jobject["appId"] != null)
                    _appId = jobject["appId"];
                else
                    _errorMessage += $"\r\nCannot read 'appId' property in 'serverConfig' object";

                if (jobject["appSecret"] != null)
                    _appSecret = jobject["appSecret"];
                else
                    _errorMessage += $"\r\nCannot read 'apappSecretpId' property in 'serverConfig' object";

                if (jobject["hostname"] != null)
                    _hostname = jobject["hostname"];
                else
                    _errorMessage += $"\r\nCannot read 'hostname' property in 'serverConfig' object";
            }
            else if(CHECK_APPID)
                _errorMessage += $"\r\nCannot read 'serverConfig' object";

            if (json["adminConfig"]?.IsObject == true)
            {
                var adminConfig = json["adminConfig"];
                _rbAdmin = new RainbowAccount()
                {
                    Prefix = adminConfig["prefix"],
                    Login = adminConfig["login"],
                    Password = adminConfig["password"],
                    AutoLogin = adminConfig["autoLogin"],
                };

                if (!(IsValueCorrect(_rbAdmin.Prefix) && IsValueCorrect(_rbAdmin.Login) && IsValueCorrect(_rbAdmin.Password)))
                    _errorMessage += $"\r\nCannot read 'prefix', 'login' or 'password' in 'adminConfig' object";
            }
            else if (CHECK_ACCOUNT)
                _errorMessage += $"\r\nCannot read 'adminConfig' object";

            // UserLogin, UserPassword
            if (json["usersConfig"]?.IsArray == true)
            {
                _rbAccounts = new();
                int index = 0;
                foreach (JSONNode userconfig in json["usersConfig"])
                {
                    var account = new RainbowAccount()
                    {
                        Prefix = userconfig["prefix"],
                        Login = userconfig["login"],
                        Password = userconfig["password"],
                        AutoLogin = userconfig["autoLogin"],
                    };

                    if (!(IsValueCorrect(account.Prefix) && IsValueCorrect(account.Login) && IsValueCorrect(account.Password)))
                        _errorMessage += $"\r\nCannot read 'prefix', 'login' or 'password' in 'usersConfig' object at index [{index}]";
                    else
                        _rbAccounts.Add(account);
                    index++;
                }
            }
            else if (CHECK_ACCOUNT)
                _errorMessage += $"\r\nCannot read 'usersConfig' object as array";
            
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

    private Boolean InitLogsWithNLog()
    {
        _errorMessage = "";

        // Set folder / directory path
        NLogConfigurator.Directory = _logsFolderPath;

        // Add loggers using prefix set on std accounts
        var prefixArray = _rbAccounts.Select(a => a.Prefix).Distinct().ToArray();
        if(!NLogConfigurator.AddLogger(prefixArray))
            _errorMessage += $"\r\n\tCannot set logger for prefix:[{prefixArray}]";

        // Add logger using prefix set on Admin account
        var prefix = _rbAdmin.Prefix;
        if(prefixArray.Contains(prefix))
            _errorMessage += $"\r\n\tPrefix set for Admin account is already used - prefix:[{prefix}]";
        else if (!NLogConfigurator.AddLogger(prefix))
            _errorMessage += $"\r\n\tCannot set logger for prefix:[{prefix}]";

        return _errorMessage == "";
    }
}

public class RainbowAccount
{
    public String Prefix { get; set; }

    public String Login { get; set; }

    public String Password { get; set; }

    public Boolean AutoLogin { get; set; }
}
