using Rainbow.SimpleJSON;

namespace Rainbow.Console
{
    public class UserConfig
    {
        public String? IniFolderPath { get; set; }

        public string Prefix { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public Boolean AutoLogin { get; set; }

        public UserConfig() 
        {
            IniFolderPath = null;
            Login = "";
            Password = "";
            Prefix = "";
            AutoLogin = false;
        }

        public static Boolean FromJsonNode(JSONNode jsonNode, out UserConfig userConfig)
        {
            Boolean result = false;
            if (jsonNode is not null)
            {
                userConfig = new()
                {
                    IniFolderPath = jsonNode["iniFolderPath"],
                    Prefix = jsonNode["prefix"],
                    Login = jsonNode["login"],
                    Password = jsonNode["password"],
                    AutoLogin = jsonNode["autoLogin"]
                };

                // Check validity
                if (String.IsNullOrEmpty(userConfig.Prefix)
                    || String.IsNullOrEmpty(userConfig.Login)
                    || String.IsNullOrEmpty(userConfig.Password))
                    result = false;
                else
                    result = true;
            }
            else
                userConfig = new();

            return result;
        }
    }
}
