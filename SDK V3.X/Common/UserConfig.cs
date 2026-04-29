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

        public Boolean IsValid(Boolean needCredentials)
        {
            if (String.IsNullOrEmpty(Prefix))
                return false;

            if (needCredentials)
            {
                if ((!String.IsNullOrEmpty(ApiKey))
                    || ((!String.IsNullOrEmpty(Login)) && (!String.IsNullOrEmpty(Password))))
                    return true;
            }
            else
                return true;
            return false;
        }

#region FromJSON / ToJSON methods

        /// <summary>
        /// **`static method`** Converts the specified JSON String to its <see cref="UserConfig"/> equivalent.
        /// </summary>
        /// <param name="jsonString"><see cref="String"/>JSON String</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="UserConfig"/> - UserConfig object or Null on error</returns>
        public static UserConfig? FromJson(string jsonString, String? nodeName = null)
            => FromJsonNode(JSON.Parse(jsonString), nodeName);

        /// <summary>
        /// **`static method`** Converts the specified <see cref="JSONNode"/> to its <see cref="UserConfig"/> equivalent.
        /// </summary>
        /// <param name="jsonNode"><see cref="JSONNode"/>JSONNode object</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="UserConfig"/> - UserConfig object or Null on error</returns>
        public static UserConfig? FromJsonNode(JSONNode jsonNode, String? nodeName = null)
        {
            if ((jsonNode == null) || (!jsonNode.IsObject))
                return null;

            if (!String.IsNullOrWhiteSpace(nodeName))
                return FromJsonNode(jsonNode[nodeName]);

            UserConfig UserConfig = new()
            {
                IniFolderPath = jsonNode["iniFolderPath"],
                Prefix = jsonNode["prefix"],
                Login = jsonNode["login"],
                Password = jsonNode["password"],
                ApiKey = jsonNode["apiKey"],
                AutoLogin = jsonNode["autoLogin"]
            };

            return UserConfig;
        }

        /// <summary>
        /// Returns a JSON String equivalent of this <see cref="UserConfig"/> object.
        /// </summary>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public String? ToJson(Boolean avoidNull = true, Boolean indent = false)
            => ToJson(this, avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// Returns a JSONNode equivalent of this <see cref="UserConfig"/> object.
        /// </summary>
        /// <returns><see cref="JSONNode"/> - JSONNode object</returns>
        public JSONNode? ToJsonNode()
            => ToJsonNode(this);

        /// <summary>
        /// **`static method`** Returns a JSON String equivalent of this <see cref="UserConfig"/> object.
        /// </summary>
        /// <param name="UserConfig"><see cref="UserConfig"/>Object to serialize in JSON</param>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public static String? ToJson(UserConfig UserConfig, Boolean avoidNull = true, Boolean indent = false)
            => ToJsonNode(UserConfig)?.ToString(avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// **`static method`** Returns a JSONNode equivalent of <see cref="UserConfig"/> object.
        /// </summary>
        /// <param name="UserConfig"><see cref="UserConfig"/>UserConfig object</param>
        /// <returns><see cref="JSONNode"/> - JSONNode on success, Null on error</returns>
        public static JSONNode? ToJsonNode(UserConfig UserConfig)
        {
            if (UserConfig == null) return null;

            var jsonNode = new JSONObject();
            jsonNode["iniFolderPath"] = UserConfig.IniFolderPath;
            jsonNode["prefix"] = UserConfig.Prefix;
            jsonNode["login"] = UserConfig.Login;
            jsonNode["password"] = UserConfig.Password;
            jsonNode["apiKey"] = UserConfig.ApiKey;
            jsonNode["autoLogin"] = UserConfig.AutoLogin;
            return jsonNode;
        }

        /// <summary>
        /// Implicit Constructor from <see cref="UserConfig"/> list to <see cref="JSONNode"/>.
        /// </summary>
        /// <param name="objectToJsonNode"><see cref="UserConfig"/>UserConfig</param>
        public static implicit operator JSONNode?(UserConfig objectToJsonNode)
            => ToJsonNode(objectToJsonNode);

        /// <summary>
        /// Implicit Constructor from <see cref="JSONNode"/> to <see cref="UserConfig"/> list.
        /// </summary>s
        /// <param name="jsonNodeToObject"><see cref="JSONNode"/>JSONNode Value</param>
        public static implicit operator UserConfig?(JSONNode jsonNodeToObject)
            => FromJsonNode(jsonNodeToObject);

#endregion FromJSON / ToJSON methods

    }
}
