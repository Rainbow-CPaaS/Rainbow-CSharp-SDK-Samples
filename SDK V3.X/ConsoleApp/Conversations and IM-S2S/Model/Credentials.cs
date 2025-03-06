using Rainbow.SimpleJSON;

namespace Rainbow.Console
{
    public class Credentials
    {
        public ServerConfig ServerConfig { get; set; }

        public List<UserConfig> UsersConfig { get; set; }

        public Credentials()
        {
            ServerConfig = new();
            UsersConfig = new();
        }

        public static Boolean FromJsonNode(JSONNode jsonNode, out Credentials credentials)
        {
            if (jsonNode is not null)
            {
                credentials = new ();

                if (jsonNode["usersConfig"]?.IsArray == true)
                {
                    foreach(JSONNode node in jsonNode["usersConfig"])
                    {
                        if (UserConfig.FromJsonNode(node, out UserConfig userconfig))
                            credentials.UsersConfig.Add(userconfig);
                    }
                }
                if(credentials.UsersConfig.Count == 0)
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
