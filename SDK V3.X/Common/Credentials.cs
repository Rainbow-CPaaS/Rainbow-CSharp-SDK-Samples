using Rainbow.SimpleJSON;
using System.Net;

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

        public Boolean IsValid(Boolean credentialsCanBeEmpty = false)
        {
            if (ServerConfig?.IsValid() != true)
                return false;

            Boolean needCredentials;
            if (credentialsCanBeEmpty)
                needCredentials = false;
            else
                needCredentials = !( (ServerConfig.OAuthPorts?.Count > 0) || !String.IsNullOrEmpty(ServerConfig.SSORedirectUrl) );
            if (needCredentials && (UsersConfig == null || UsersConfig.Count == 0))
                return false;

            var usersConfigList = UsersConfig?.ToList() ?? [];
            UsersConfig = [];
            foreach (var userConfig in usersConfigList)
            {
                if (userConfig?.IsValid(needCredentials) == true)
                    UsersConfig.Add(userConfig);
            }
            if (UsersConfig.Count == 0)
                return false;

            return true;
        }

#region FromJSON / ToJSON methods

        /// <summary>
        /// **`static method`** Converts the specified JSON String to its <see cref="Credentials"/> equivalent.
        /// </summary>
        /// <param name="jsonString"><see cref="String"/>JSON String</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="Credentials"/> - Credentials object or Null on error</returns>
        public static Credentials? FromJson(string jsonString, String? nodeName = null)
            => FromJsonNode(JSON.Parse(jsonString), nodeName);

        /// <summary>
        /// **`static method`** Converts the specified <see cref="JSONNode"/> to its <see cref="Credentials"/> equivalent.
        /// </summary>
        /// <param name="jsonNode"><see cref="JSONNode"/>JSONNode object</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="Credentials"/> - Credentials object or Null on error</returns>
        public static Credentials? FromJsonNode(JSONNode jsonNode, String? nodeName = null)
        {
            if ((jsonNode == null) || (!jsonNode.IsObject))
                return null;

            if (!String.IsNullOrWhiteSpace(nodeName))
                return FromJsonNode(jsonNode[nodeName]);

            Credentials Credentials = new()
            {
                ServerConfig = ServerConfig.FromJsonNode(jsonNode["serverConfig"]) ?? new(),
                UsersConfig = JSON.ToList<UserConfig>(jsonNode["usersConfig"], UserConfig.FromJsonNode)
            };

            return Credentials;
        }

        /// <summary>
        /// Returns a JSON String equivalent of this <see cref="Credentials"/> object.
        /// </summary>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public String? ToJson(Boolean avoidNull = true, Boolean indent = false)
            => ToJson(this, avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// Returns a JSONNode equivalent of this <see cref="Credentials"/> object.
        /// </summary>
        /// <returns><see cref="JSONNode"/> - JSONNode object</returns>
        public JSONNode? ToJsonNode()
            => ToJsonNode(this);

        /// <summary>
        /// **`static method`** Returns a JSON String equivalent of this <see cref="Credentials"/> object.
        /// </summary>
        /// <param name="Credentials"><see cref="Credentials"/>Object to serialize in JSON</param>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public static String? ToJson(Credentials Credentials, Boolean avoidNull = true, Boolean indent = false)
            => ToJsonNode(Credentials)?.ToString(avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// **`static method`** Returns a JSONNode equivalent of <see cref="Credentials"/> object.
        /// </summary>
        /// <param name="credentials"><see cref="Credentials"/>Credentials object</param>
        /// <returns><see cref="JSONNode"/> - JSONNode on success, Null on error</returns>
        public static JSONNode? ToJsonNode(Credentials? credentials)
        {
            if (credentials == null) return null;

            var jsonNode = new JSONObject();
            jsonNode["serverConfig"] = credentials.ServerConfig?.ToJsonNode() ?? new JSONObject();
            jsonNode["usersConfig"] = JSON.ToJSONArray(credentials.UsersConfig, UserConfig.ToJsonNode);
            return jsonNode;
        }

        /// <summary>
        /// Implicit Constructor from <see cref="Credentials"/> list to <see cref="JSONNode"/>.
        /// </summary>
        /// <param name="objectToJsonNode"><see cref="Credentials"/>Credentials</param>
        public static implicit operator JSONNode?(Credentials objectToJsonNode)
            => ToJsonNode(objectToJsonNode);

        /// <summary>
        /// Implicit Constructor from <see cref="JSONNode"/> to <see cref="Credentials"/> list.
        /// </summary>s
        /// <param name="jsonNodeToObject"><see cref="JSONNode"/>JSONNode Value</param>
        public static implicit operator Credentials?(JSONNode jsonNodeToObject)
            => FromJsonNode(jsonNodeToObject);

#endregion FromJSON / ToJSON methods

    }
}
