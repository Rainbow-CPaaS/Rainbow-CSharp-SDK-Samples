using Rainbow.SimpleJSON;

namespace BotLibrary.Model
{
    public class BotCredentials
    {
        public ServerConfig ServerConfig { get; set; }

        public UserConfig UserConfig { get; set; }

        public BotCredentials()
        {
            ServerConfig = new();
            UserConfig = new();
        }

        public static Boolean FromJsonNode(JSONNode jsonNode, out BotCredentials botCredentials)
        {
            if (jsonNode is not null)
            {
                botCredentials = new ();

                if (UserConfig.FromJsonNode(jsonNode["userConfig"], out UserConfig userconfig))
                    botCredentials.UserConfig = userconfig;
                else
                    return false;

                if (ServerConfig.FromJsonNode(jsonNode["serverConfig"], out ServerConfig serverConfig))
                    botCredentials.ServerConfig = serverConfig;
                else
                    return false;

                return true;
            }
            else
                botCredentials = new();

            return false;
        }
    }
}
