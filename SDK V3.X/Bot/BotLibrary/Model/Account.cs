using Rainbow.SimpleJSON;

namespace BotLibrary.Model
{
    public class Account
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Jid
        /// </summary>
        public string Jid { get; set; }

        /// <summary>
        /// Login
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// NickName
        /// </summary>
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Account()
        {
            Id = "";
            Jid = "";
            Login = "";
            Password = "";
            FirstName = "";
            LastName = "";
        }

        public override string ToString()
        {
            String result = "";
            if (!String.IsNullOrEmpty(Id))
                result += $"Id:[{Id}] ";

            if (!String.IsNullOrEmpty(Jid))
                result += $"Jid:[{Jid}] ";

            if (!String.IsNullOrEmpty(Login))
                result += $"Login:[{Login}] ";

            if (!String.IsNullOrEmpty(FirstName))
                result += $"FirstName:[{FirstName}] ";

            if (!String.IsNullOrEmpty(LastName))
                result += $"LastName:[{LastName}] ";

            return result.Trim();
        }

        public Boolean IsValid()
        {
            if (String.IsNullOrEmpty(Id) && String.IsNullOrEmpty(Jid) && String.IsNullOrEmpty(Login))
                return false;
            return true;
        }

#region FromJSON / ToJSON methods


        /// <summary>
        /// **`static method`** Converts the specified JSON String to its <see cref="Account"/> equivalent.
        /// </summary>
        /// <param name="jsonString"><see cref="String"/>JSON String</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="Account"/> - Account object or Null on error</returns>
        public static Account? FromJson(string jsonString, String? nodeName = null)
            => FromJsonNode(JSON.Parse(jsonString), nodeName);

        /// <summary>
        /// **`static method`** Converts the specified <see cref="JSONNode"/> to its <see cref="Account"/> equivalent.
        /// </summary>
        /// <param name="jsonNode"><see cref="JSONNode"/>JSONNode object</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="Account"/> - Account object or Null on error</returns>
        public static Account? FromJsonNode(JSONNode jsonNode, String? nodeName = null)
        {
            if ((jsonNode == null) || (!jsonNode.IsObject))
                return null;

            if (!String.IsNullOrWhiteSpace(nodeName))
                return FromJsonNode(jsonNode[nodeName]);

            Account Account = new()
            {
                Id = jsonNode["id"],
                Jid = jsonNode["jid"],
                Login = jsonNode["login"],
                Password = jsonNode["password"],
                FirstName = jsonNode["firstName"],
                LastName = jsonNode["lastName"]
            };
            return Account;
        }

        /// <summary>
        /// Returns a JSON String equivalent of this <see cref="Account"/> object.
        /// </summary>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public String? ToJson(Boolean avoidNull = true, Boolean indent = false)
            => ToJson(this, avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// Returns a JSONNode equivalent of this <see cref="Account"/> object.
        /// </summary>
        /// <returns><see cref="JSONNode"/> - JSONNode object</returns>
        public JSONNode? ToJsonNode()
            => ToJsonNode(this);

        /// <summary>
        /// **`static method`** Returns a JSON String equivalent of this <see cref="Account"/> object.
        /// </summary>
        /// <param name="Account"><see cref="Account"/>Object to serialize in JSON</param>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public static String? ToJson(Account Account, Boolean avoidNull = true, Boolean indent = false)
            => ToJsonNode(Account)?.ToString(avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// **`static method`** Returns a JSONNode equivalent of <see cref="Account"/> object.
        /// </summary>
        /// <param name="Account"><see cref="Account"/>Account object</param>
        /// <returns><see cref="JSONNode"/> - JSONNode on success, Null on error</returns>
        public static JSONNode? ToJsonNode(Account Account)
        {
            if (Account == null) return null;

            var jsonNode = new JSONObject();
            jsonNode["id"] = Account.Id;
            jsonNode["jid"] = Account.Jid;
            jsonNode["login"] = Account.Login;
            jsonNode["password"] = Account.Password;
            jsonNode["firstName"] = Account.FirstName;
            jsonNode["lastName"] = Account.LastName;
            return jsonNode;
        }

        /// <summary>
        /// Implicit Constructor from <see cref="Account"/> list to <see cref="JSONNode"/>.
        /// </summary>
        /// <param name="objectToJsonNode"><see cref="Account"/>Account</param>
        public static implicit operator JSONNode?(Account objectToJsonNode)
            => ToJsonNode(objectToJsonNode);

        /// <summary>
        /// Implicit Constructor from <see cref="JSONNode"/> to <see cref="Account"/> list.
        /// </summary>s
        /// <param name="jsonNodeToObject"><see cref="JSONNode"/>JSONNode Value</param>
        public static implicit operator Account?(JSONNode jsonNodeToObject)
            => FromJsonNode(jsonNodeToObject);

#endregion FromJSON / ToJSON methods


    }
}
