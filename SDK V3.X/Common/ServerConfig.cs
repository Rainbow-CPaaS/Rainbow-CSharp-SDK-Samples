using Rainbow.SimpleJSON;

namespace Rainbow.Example.Common
{
    public class ServerConfig
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string HostName { get; set; }

        public List<int>? OAuthPorts { get; set; }

        public String? SSORedirectUrl { get; set; }

        public ServerConfig()
        {
            AppId = "";
            AppSecret = "";
            HostName = "";
            OAuthPorts = null;
            SSORedirectUrl = null;
        }

        public static Boolean FromJsonNode(JSONNode jsonNode, out ServerConfig serverConfig)
        {
            Boolean result = false;
            if (jsonNode is not null)
            {
                serverConfig = new()
                {
                    AppId = jsonNode["appId"],
                    AppSecret = jsonNode["appSecret"],
                    HostName = jsonNode["hostname"],
                    OAuthPorts = jsonNode["oauthPorts"],
                    SSORedirectUrl = jsonNode["ssoRedirectUrl"]
                };

                // Check validity
                if (String.IsNullOrEmpty(serverConfig.AppId)
                    || String.IsNullOrEmpty(serverConfig.AppSecret)
                    || String.IsNullOrEmpty(serverConfig.HostName))
                    result = false;
                else
                    result = true;
            }
            else
                serverConfig = new();

            return result;
        }
    }
}
