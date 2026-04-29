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

        public Boolean IsValid()
            => !String.IsNullOrEmpty(AppId) && !String.IsNullOrEmpty(AppSecret) && !String.IsNullOrEmpty(HostName);

#region FromJSON / ToJSON methods


        /// <summary>
        /// **`static method`** Converts the specified JSON String to its <see cref="ServerConfig"/> equivalent.
        /// </summary>
        /// <param name="jsonString"><see cref="String"/>JSON String</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="ServerConfig"/> - ServerConfig object or Null on error</returns>
        public static ServerConfig? FromJson(string jsonString, String? nodeName = null)
            => FromJsonNode(JSON.Parse(jsonString), nodeName);

        /// <summary>
        /// **`static method`** Converts the specified <see cref="JSONNode"/> to its <see cref="ServerConfig"/> equivalent.
        /// </summary>
        /// <param name="jsonNode"><see cref="JSONNode"/>JSONNode object</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="ServerConfig"/> - ServerConfig object or Null on error</returns>
        public static ServerConfig? FromJsonNode(JSONNode jsonNode, String? nodeName = null)
        {
            if ((jsonNode == null) || (!jsonNode.IsObject))
                return null;

            if (!String.IsNullOrWhiteSpace(nodeName))
                return FromJsonNode(jsonNode[nodeName]);

            ServerConfig ServerConfig = new()
            {
                AppId = jsonNode["appId"],
                AppSecret = jsonNode["appSecret"],
                HostName = jsonNode["hostname"],
                OAuthPorts = jsonNode["oauthPorts"],
                SSORedirectUrl = jsonNode["ssoRedirectUrl"]
            };

            return ServerConfig;
        }

        /// <summary>
        /// Returns a JSON String equivalent of this <see cref="ServerConfig"/> object.
        /// </summary>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public String? ToJson(Boolean avoidNull = true, Boolean indent = false)
            => ToJson(this, avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// Returns a JSONNode equivalent of this <see cref="ServerConfig"/> object.
        /// </summary>
        /// <returns><see cref="JSONNode"/> - JSONNode object</returns>
        public JSONNode? ToJsonNode()
            => ToJsonNode(this);

        /// <summary>
        /// **`static method`** Returns a JSON String equivalent of this <see cref="ServerConfig"/> object.
        /// </summary>
        /// <param name="ServerConfig"><see cref="ServerConfig"/>Object to serialize in JSON</param>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public static String? ToJson(ServerConfig ServerConfig, Boolean avoidNull = true, Boolean indent = false)
            => ToJsonNode(ServerConfig)?.ToString(avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// **`static method`** Returns a JSONNode equivalent of <see cref="ServerConfig"/> object.
        /// </summary>
        /// <param name="ServerConfig"><see cref="ServerConfig"/>ServerConfig object</param>
        /// <returns><see cref="JSONNode"/> - JSONNode on success, Null on error</returns>
        public static JSONNode? ToJsonNode(ServerConfig ServerConfig)
        {
            if (ServerConfig == null) return null;

            var jsonNode = new JSONObject();
            jsonNode["appId"] = ServerConfig.AppId;
            jsonNode["appSecret"] = ServerConfig.AppSecret;
            jsonNode["hostname"] = ServerConfig.HostName;
            jsonNode["oauthPorts"] = ServerConfig.OAuthPorts;
            jsonNode["ssoRedirectUrl"] = ServerConfig.SSORedirectUrl;

            return jsonNode;
        }

        /// <summary>
        /// Implicit Constructor from <see cref="ServerConfig"/> list to <see cref="JSONNode"/>.
        /// </summary>
        /// <param name="objectToJsonNode"><see cref="ServerConfig"/>ServerConfig</param>
        public static implicit operator JSONNode?(ServerConfig objectToJsonNode)
            => ToJsonNode(objectToJsonNode);

        /// <summary>
        /// Implicit Constructor from <see cref="JSONNode"/> to <see cref="ServerConfig"/> list.
        /// </summary>s
        /// <param name="jsonNodeToObject"><see cref="JSONNode"/>JSONNode Value</param>
        public static implicit operator ServerConfig?(JSONNode jsonNodeToObject)
            => FromJsonNode(jsonNodeToObject);

#endregion FromJSON / ToJSON methods

    }
}
