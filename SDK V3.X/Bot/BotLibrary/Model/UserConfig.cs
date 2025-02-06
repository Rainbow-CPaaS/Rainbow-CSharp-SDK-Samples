using Rainbow.SimpleJSON;

namespace BotLibrary.Model
{
    public class UserConfig
    {
        public String? IniFolderPath { get; set; }

        public string Prefix { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public UserConfig() 
        {
            IniFolderPath = null;
            Login = "";
            Password = "";
            Prefix = "";
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
                    Password = jsonNode["password"]
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
