using Rainbow.SimpleJSON;

namespace Rainbow.Example.Common
{
    public class Credentials
    {
        public ServerConfig ServerConfig { get; set; }

        public List<UserConfig> UsersConfig { get; set; }

        public Credentials()
        {
            ServerConfig = new();
            UsersConfig = [];
        }

        public static Boolean FromJsonNode(JSONNode jsonNode, out Credentials credentials)
        {
            if (jsonNode is not null)
            {
                credentials = new();

                if (ServerConfig.FromJsonNode(jsonNode["serverConfig"], out ServerConfig serverConfig))
                    credentials.ServerConfig = serverConfig;
                else
                    return false;

                Boolean needCredentials = !(credentials.ServerConfig.OAuthPorts?.Count > 0);

                if (jsonNode["usersConfig"]?.IsArray == true)
                {
                    foreach (JSONNode node in jsonNode["usersConfig"])
                    {
                        if (UserConfig.FromJsonNode(node, needCredentials, out UserConfig userconfig))
                            credentials.UsersConfig.Add(userconfig);
                    }
                }
                if (credentials.UsersConfig.Count == 0)
                    return false;



                return true;
            }
            else
                credentials = new();

            return false;
        }
    }
}
