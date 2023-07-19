using NLog.Config;
using Rainbow;
using Rainbow.SimpleJSON;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SDK.UIForm.WebRTC
{
    public sealed class Configuration
    {
        private static Configuration? _instance;
        
        private Boolean _initialized = false;

        private const String TO_DEFINE = "TO DEFINE";

        private String? _ffmpegLibPath;
        private String? _nlogConfigFilePath;
        private String? _mediasConfigFilePath;

        private String? _hostname;
        private String? _appId;
        private String? _appSecret;

        private String? _userLogin;
        private String? _userPassword;
        private Boolean _autoLogin = false;

        public static Configuration Instance
        {
            get {
                if(_instance == null)
                    _instance = new Configuration();
                return _instance;
            }
        }

        public Boolean Initialized{ get => _initialized; }

        public String? FFmpegLibPath { get => _ffmpegLibPath; }
        public String? NLogConfigFilePath { get => _nlogConfigFilePath; }
        public String? MediasConfigFilePath { get => _mediasConfigFilePath; }
        public String? HostName { get => _hostname; }
        public String? AppId { get => _appId; }
        public String? AppSecret { get => _appSecret; }
        public String? UserLogin { get => _userLogin; }
        public String? UserPassword { get => _userPassword; }
        public Boolean AutoLogin { get => _autoLogin; }

        private Configuration()
        {
            if (ParseConfigFile() && CheckRainbowInfo() && CheckAccount() && CheckSDL2Libraries() && CheckFFmpegLibraries())
            {
                _initialized = true;
                if (!InitLogsWithNLog())
                    MessageBox.Show("Cannot initialize logs using NLog configuration file ...");
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

        private Boolean CheckRainbowInfo()
        {
            if (IsValueCorrect(AppId) && IsValueCorrect(AppSecret) && IsValueCorrect(HostName))
                return true;

            String configFilePath = GetConfigFilePath();
            MessageBox.Show($"In this file:\r\n\t{configFilePath}\r\n\r\n In object 'serverConfig': 'appId', 'appSecret' and/or 'hostName' properties have not been correctly defined ...");
            return false;
        }

        private Boolean CheckAccount()
        {
            if (IsValueCorrect(UserLogin, false) && IsValueCorrect(UserPassword, false))
                return true;

            String configFilePath = GetConfigFilePath();
            MessageBox.Show($"In this file:\r\n\t{configFilePath}\r\n\r\n In object 'userConfig': 'login' and/or 'password' have not been correctly defined ...");
            return false;
        }

        private Boolean CheckSDL2Libraries()
        {
            // TODO - enhance SDL2 libraries check on other platforms 
            var filePath = $".{Path.DirectorySeparatorChar}SDL2.dll";
            if(File.Exists(filePath))
                return true;

            MessageBox.Show($"SDL2.dll file has not been found here:\r\n\t{Path.GetFullPath(filePath)}");
            return false;
        }

        private Boolean CheckFFmpegLibraries()
        {
            var configFilePath = GetConfigFilePath();

            // TODO - enhance SDL2 libraries check on other platforms 
            var folderPath = FFmpegLibPath;
            if(String.IsNullOrEmpty(folderPath))
            {
                MessageBox.Show($"Invalid 'ffmpegLibFolderPath' property set in file\r\n\t{configFilePath}");
                return false;
            }

            folderPath = Path.GetFullPath(folderPath);
            if (Directory.Exists(folderPath))
            {
                // TODO - enhance FFmpeg libraries check on other platforms 
                var files = new List<String>() { "avcodec-59.dll", "avdevice-59.dll", "avfilter-8.dll", "avformat-59.dll", "avutil-57.dll", "postproc-56.dll", "swresample-4.dll", "swscale-6.dll"};
                foreach (var file in files)
                {
                    if(!File.Exists(Path.Combine(folderPath, file)))
                    {
                        MessageBox.Show($"Invalid 'ffmpegLibFolderPath' property set in file\r\n\t{configFilePath}.\r\nOne or more files are missing in this folder:\r\n\t{folderPath}.\r\nNecessary files:\r\n\t{String.Join("\r\n\t", files)}");
                        return false;
                    }
                }

                return true;
            }

            MessageBox.Show($"Invalid 'ffmpegLibFolderPath' property set in file\r\n\t{configFilePath}.\r\nthis folder doesn't exist:\r\n\t{folderPath}");
            return false;
        }

        private String GetConfigFilePath()
        {
            return Path.GetFullPath($".{Path.DirectorySeparatorChar}Resources{Path.DirectorySeparatorChar}config.json");
        }

        private Boolean ParseConfigFile()
        {
            String message = "";
            String configFilePath = GetConfigFilePath();

            if (!File.Exists(configFilePath))
            {
                MessageBox.Show($"This file doesn't exist:\r\n\t{configFilePath}");
                return false;
            }

            try
            {
                String jsonConfig = File.ReadAllText(configFilePath);
                var json = JSON.Parse(jsonConfig);

                if (json == null)
                {
                    MessageBox.Show($"Cannot get JSON data from file:\r\n\t{configFilePath}");
                    return false;
                }

                // FFmpeg
                if (json["ffmpegLibFolderPath"] != null)
                    _ffmpegLibPath = UtilJson.AsString(json, "ffmpegLibFolderPath");
                else
                    message += $"\r\nCannot read 'ffmpegLibFolderPath' property";

                // NLog
                if (json["nlogConfigFilePath"] != null)
                    _nlogConfigFilePath = UtilJson.AsString(json, "nlogConfigFilePath");
                else
                    message += $"\r\nCannot read 'nlogConfigFilePath' property";

                // Medias
                if (json["mediasConfigFilePath"] != null)
                    _mediasConfigFilePath = UtilJson.AsString(json, "mediasConfigFilePath");
                else
                    message += $"\r\nCannot read 'mediasConfigFilePath' property";

                // HostName, AppId, AppSecret
                if (json["serverConfig"] != null)
                {
                    var jobject = json["serverConfig"];

                    if (jobject["appId"] != null)
                        _appId = UtilJson.AsString(jobject, "appId");
                    else
                        message += $"\r\nCannot read 'appId' property in 'serverConfig' object";

                    if (jobject["appSecret"] != null)
                        _appSecret = UtilJson.AsString(jobject, "appSecret");
                    else
                        message += $"\r\nCannot read 'apappSecretpId' property in 'serverConfig' object";

                    if (jobject["hostname"] != null)
                        _hostname = UtilJson.AsString(jobject, "hostname");
                    else
                        message += $"\r\nCannot read 'hostname' property in 'serverConfig' object";
                }
                else
                    message += $"\r\nCannot read 'serverConfig' object";

                // UserLogin, UserPassword
                if (json["userConfig"] != null)
                {
                    var jobject = json["userConfig"];

                    if (jobject["login"] != null)
                        _userLogin = UtilJson.AsString(jobject, "login");
                    else
                        message += $"\r\nCannot read 'appId' property in 'userConfig' object";

                    if (jobject["password"] != null)
                        _userPassword = UtilJson.AsString(jobject, "password");
                    else
                        message += $"\r\nCannot read 'password' property in 'userConfig' object";

                    if (jobject["autoLogin"] != null)
                        _autoLogin = UtilJson.AsBoolean(jobject, "autoLogin");
                    else
                        message += $"\r\nCannot read 'autoLogin' property in 'userConfig' object";
                }
                else
                    message += $"\r\nCannot read 'userConfig' object";

            }
            catch (Exception exc)
            {
                message += $"\r\nException:[{Rainbow.Util.SerializeException(exc)}]";

            }

            if (String.IsNullOrEmpty(message))
                return true;

            message = $"Pb occured while reading/parsing this file:\r\n\t{configFilePath}" + message;
            MessageBox.Show(message);
            return false;
        }

        private Boolean InitLogsWithNLog() // To inialize log using NLog XML configuration file wiht SDK 2.6.0 and more
        {
            String logConfigFilePath = _nlogConfigFilePath; // File path to log configuration (or you could also use an embedded resource)

            try
            {
                if (File.Exists(logConfigFilePath))
                {
                    // Get content of the log file configuration
                    String logConfigContent = File.ReadAllText(logConfigFilePath, System.Text.Encoding.UTF8);

                    // Create NLog configuration using XML file content
                    XmlLoggingConfiguration config = XmlLoggingConfiguration.CreateFromXmlString(logConfigContent);
                    if (config.InitializeSucceeded == true)
                    {
                        // Set NLog configuration
                        NLog.LogManager.Configuration = config;

                        // Create Logger factory
                        var factory = new NLog.Extensions.Logging.NLogLoggerFactory();

                        // Set Logger factory to Rainbow SDK
                        Rainbow.LogFactory.Set(factory);

                        return true;
                    }
                }
            }
            catch { }

            return false;
        }
    }
}
