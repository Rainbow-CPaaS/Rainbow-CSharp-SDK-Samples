using Rainbow.SimpleJSON;

namespace BotLibrary.Model
{
    public class BotConfiguration
    {
        /// <summary>
        /// Administrators
        /// </summary>
        public List<Account>? Administrators { get; set; }

        public Boolean GuestsAccepted { get; set; }

        public Boolean InstantMessageAutoAccept { get; set; }

        public Boolean PrivateMessageAutoAccept { get; set; }

        public Boolean AckMessageAutoAccept { get; set; }

        public Boolean ApplicationMessageAutoAccept { get; set; }

        public Boolean BubbleInvitationAutoAccept { get; set; }

        public Boolean UserInvitationAutoAccept { get; set; }

        public String? FirstName { get; set; }

        public String LastName { get; set; }

        public Account? Bot { get; set; }

        public BotConfiguration()
        {
            Administrators = null;
            GuestsAccepted = false;

            InstantMessageAutoAccept = false;
            PrivateMessageAutoAccept = false;
            AckMessageAutoAccept = false;
            ApplicationMessageAutoAccept = false;

            BubbleInvitationAutoAccept = true;
            UserInvitationAutoAccept = true;

            FirstName = null;
            LastName = null;
            Bot = null;
        }

#region FromJSON / ToJSON methods


        /// <summary>
        /// **`static method`** Converts the specified JSON String to its <see cref="BotConfiguration"/> equivalent.
        /// </summary>
        /// <param name="jsonString"><see cref="String"/>JSON String</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="BotConfiguration"/> - BotConfiguration object or Null on error</returns>
        public static BotConfiguration? FromJson(string jsonString, String? nodeName = null)
            => FromJsonNode(JSON.Parse(jsonString), nodeName);

        /// <summary>
        /// **`static method`** Converts the specified <see cref="JSONNode"/> to its <see cref="BotConfiguration"/> equivalent.
        /// </summary>
        /// <param name="jsonNode"><see cref="JSONNode"/>JSONNode object</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="BotConfiguration"/> - BotConfiguration object or Null on error</returns>
        public static BotConfiguration? FromJsonNode(JSONNode jsonNode, String? nodeName = null)
        {
            if ((jsonNode == null) || (!jsonNode.IsObject))
                return null;

            if (!String.IsNullOrWhiteSpace(nodeName))
                return FromJsonNode(jsonNode[nodeName]);

            BotConfiguration botConfiguration = new()
            {
                Administrators = [],
                GuestsAccepted = jsonNode["administrators"]?["guestsAccepted"],
                InstantMessageAutoAccept = jsonNode["instantMessageAutoAccept"],
                PrivateMessageAutoAccept = jsonNode["privateMessageAutoAccept"],
                AckMessageAutoAccept = jsonNode["ackMessageAutoAccept"],
                ApplicationMessageAutoAccept = jsonNode["applicationMessageAutoAccept"],
                UserInvitationAutoAccept = jsonNode["userInvitationAutoAccept"],
                BubbleInvitationAutoAccept = jsonNode["bubbleInvitationAutoAccept"],
                FirstName = jsonNode["firstName"],
                LastName = jsonNode["lastName"],
                Bot = Account.FromJsonNode(jsonNode["bot"])
            };

            // Add only valid administrators
            var administrators = JSON.ToList<Account>(jsonNode["administrators"]?["rainbowAccounts"], Account.FromJsonNode) ?? [];
            foreach (var admin in administrators)
            {
                if (admin?.IsValid() == true)
                    botConfiguration.Administrators.Add(admin);
            }
            return botConfiguration;
        }

        /// <summary>
        /// Returns a JSON String equivalent of this <see cref="BotConfiguration"/> object.
        /// </summary>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public String? ToJson(Boolean avoidNull = true, Boolean indent = false)
            => ToJson(this, avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// Returns a JSONNode equivalent of this <see cref="BotConfiguration"/> object.
        /// </summary>
        /// <returns><see cref="JSONNode"/> - JSONNode object</returns>
        public JSONNode? ToJsonNode()
            => ToJsonNode(this);

        /// <summary>
        /// **`static method`** Returns a JSON String equivalent of this <see cref="BotConfiguration"/> object.
        /// </summary>
        /// <param name="BotConfiguration"><see cref="BotConfiguration"/>Object to serialize in JSON</param>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public static String? ToJson(BotConfiguration BotConfiguration, Boolean avoidNull = true, Boolean indent = false)
            => ToJsonNode(BotConfiguration)?.ToString(avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// **`static method`** Returns a JSONNode equivalent of <see cref="BotConfiguration"/> object.
        /// </summary>
        /// <param name="botConfiguration"><see cref="BotConfiguration"/>BotConfiguration object</param>
        /// <returns><see cref="JSONNode"/> - JSONNode on success, Null on error</returns>
        public static JSONNode? ToJsonNode(BotConfiguration botConfiguration)
        {
            if (botConfiguration == null) return null;

            var jsonNode = new JSONObject();
            jsonNode["administrators"] = new JSONObject();
            jsonNode["administrators"]["rainbowAccounts"] = JSON.ToJSONArray(botConfiguration.Administrators, Account.ToJsonNode);
            jsonNode["administrators"]["guestsAccepted"] = botConfiguration.GuestsAccepted;
            jsonNode["instantMessageAutoAccept"] = botConfiguration.InstantMessageAutoAccept;
            jsonNode["privateMessageAutoAccept"] = botConfiguration.PrivateMessageAutoAccept;
            jsonNode["ackMessageAutoAccept"] = botConfiguration.AckMessageAutoAccept;
            jsonNode["applicationMessageAutoAccept"] = botConfiguration.ApplicationMessageAutoAccept;
            jsonNode["userInvitationAutoAccept"] = botConfiguration.UserInvitationAutoAccept;
            jsonNode["bubbleInvitationAutoAccept"] = botConfiguration.BubbleInvitationAutoAccept;
            jsonNode["firstName"] = botConfiguration.FirstName;
            jsonNode["lastName"] = botConfiguration.LastName;
            if (botConfiguration.Bot is not null)
                jsonNode["bot"] = Account.ToJsonNode(botConfiguration.Bot);
            return jsonNode;
        }

        /// <summary>
        /// Implicit Constructor from <see cref="BotConfiguration"/> list to <see cref="JSONNode"/>.
        /// </summary>
        /// <param name="objectToJsonNode"><see cref="BotConfiguration"/>BotConfiguration</param>
        public static implicit operator JSONNode?(BotConfiguration objectToJsonNode)
            => ToJsonNode(objectToJsonNode);

        /// <summary>
        /// Implicit Constructor from <see cref="JSONNode"/> to <see cref="BotConfiguration"/> list.
        /// </summary>s
        /// <param name="jsonNodeToObject"><see cref="JSONNode"/>JSONNode Value</param>
        public static implicit operator BotConfiguration?(JSONNode jsonNodeToObject)
            => FromJsonNode(jsonNodeToObject);

#endregion FromJSON / ToJSON methods

    }
}
