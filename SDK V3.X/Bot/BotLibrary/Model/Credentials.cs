using Rainbow.SimpleJSON;

namespace BotLibrary.Model
{
    public class Credentials
    {
        public ServerConfig ServerConfig { get; set; }

        public UserConfig UserConfig { get; set; }

        public Credentials()
        {
            ServerConfig = new();
            UserConfig = new();
        }

        public static Boolean FromJsonNode(JSONNode jsonNode, out Credentials credentials)
        {
            if (jsonNode is not null)
            {
                credentials = new ();

                if (UserConfig.FromJsonNode(jsonNode["userConfig"], out UserConfig userconfig))
                    credentials.UserConfig = userconfig;
                else
                    return false;

                if (ServerConfig.FromJsonNode(jsonNode["serverConfig"], out ServerConfig serverConfig))
                    credentials.ServerConfig = serverConfig;
                else
                    return false;

                return true;
            }
            else
                credentials = new();

            return false;
        }
    }
}
