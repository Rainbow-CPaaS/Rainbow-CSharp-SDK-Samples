using Rainbow.SimpleJSON;

namespace Rainbow.Example.Common
{
    public class UserConfig
    {
        public String? IniFolderPath { get; set; }

        public string Prefix { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public string ApiKey { get; set; }

        public Boolean AutoLogin { get; set; }

        public UserConfig()
        {
            IniFolderPath = null;
            Login = "";
            Password = "";
            Prefix = "";
            ApiKey = "";
            AutoLogin = false;
        }

        public static Boolean FromJsonNode(JSONNode jsonNode, Boolean needCredentials, out UserConfig userConfig)
        {
            if (jsonNode is not null)
            {
                userConfig = new()
                {
                    IniFolderPath = jsonNode["iniFolderPath"],
                    Prefix = jsonNode["prefix"],
                    Login = jsonNode["login"],
                    Password = jsonNode["password"],
                    ApiKey = jsonNode["apiKey"],
                    AutoLogin = jsonNode["autoLogin"],
                };

                // Check validity
                if (String.IsNullOrEmpty(userConfig.Prefix))
                    return false;

                if (needCredentials)
                {
                    if ((!String.IsNullOrEmpty(userConfig.ApiKey))
                        || ((!String.IsNullOrEmpty(userConfig.Login)) && (!String.IsNullOrEmpty(userConfig.Password))))
                        return true;
                }
                else
                    return true;
            }
            else
                userConfig = new();

            return false;
        }
    }
}
