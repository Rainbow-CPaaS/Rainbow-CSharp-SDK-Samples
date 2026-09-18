using Rainbow.Example.Common;
using Rainbow.Model;
using Rainbow.SimpleJSON;

namespace StartJoinConfAndAddParticipant
{
    internal class Config
    {
        public Account? Operator { get; set; }

        public Bubble? Bubble { get; set; }

        public List<String>? PhoneNumbers { get; set; }

        public int MaxPhoneNumberToAddInFirstRow { get; set; }

        public String? AddNewPhoneNumberWhen { get; set; }

        public Boolean IsValid()
        {
            if (Operator?.IsValid() == true)
            {
                if (!String.IsNullOrEmpty(Bubble?.Peer?.DisplayName))
                {
                    if (PhoneNumbers?.Count > 0)
                    {
                        List<String> possibleValues = ["active", "ringing", "progress"];
                        if (possibleValues.Contains(AddNewPhoneNumberWhen ?? ""))
                            return true;
                        else
                            ConsoleAbstraction.WriteRed($"Bad config: addNewPhoneNumberWhen has incorrect value [{AddNewPhoneNumberWhen} - Valids value:[{possibleValues}] ...");
                    }
                    else
                        ConsoleAbstraction.WriteRed("Bad config: No phone number specified ...");
                }
                else
                    ConsoleAbstraction.WriteRed("Bad config: Bubble has no display name ...");
            }
            else
                ConsoleAbstraction.WriteRed("Bad config: Operator is not valid ...");

            return false;
        }


#region FromJSON / ToJSON methods

        /// <summary>
        /// **`static method`** Converts the specified JSON String to its <see cref="Config"/> equivalent.
        /// </summary>
        /// <param name="jsonString"><see cref="String"/>JSON String</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="Config"/> - Config object or Null on error</returns>
        public static Config? FromJson(string jsonString, String? nodeName = null)
            => FromJsonNode(JSON.Parse(jsonString), nodeName);

        /// <summary>
        /// **`static method`** Converts the specified <see cref="JSONNode"/> to its <see cref="Config"/> equivalent.
        /// </summary>
        /// <param name="jsonNode"><see cref="JSONNode"/>JSONNode object</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="Config"/> - Config object or Null on error</returns>
        public static Config? FromJsonNode(JSONNode? jsonNode, String? nodeName = null)
        {
            if ((jsonNode == null) || (!jsonNode.IsObject))
                return null;

            if (!String.IsNullOrWhiteSpace(nodeName))
                return FromJsonNode(jsonNode[nodeName]);

            Config Config = new()
            {
                Operator = Account.FromJsonNode(jsonNode["operator"]),
                Bubble = new () { 
                    Peer = new() { DisplayName = jsonNode["bubble"]?["name"] ?? "Test create-start-join-add_participants" },
                    Topic = jsonNode["bubble"]?["topic"] ?? String.Empty,
                },
                PhoneNumbers = jsonNode["phoneNumbers"],
                MaxPhoneNumberToAddInFirstRow = jsonNode["maxPhoneNumberToAddInFirstRow"],
                AddNewPhoneNumberWhen = jsonNode["addNewPhoneNumberWhen"] ?? "active",
            };

            return Config;
        }

        /// <summary>
        /// Returns a JSON String equivalent of this <see cref="Config"/> object.
        /// </summary>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public String? ToJson(Boolean avoidNull = true, Boolean indent = false)
            => ToJson(this, avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// Returns a JSONNode equivalent of this <see cref="Config"/> object.
        /// </summary>
        /// <returns><see cref="JSONNode"/> - JSONNode object</returns>
        public JSONNode? ToJsonNode()
            => ToJsonNode(this);

        /// <summary>
        /// **`static method`** Returns a JSON String equivalent of this <see cref="Config"/> object.
        /// </summary>
        /// <param name="Config"><see cref="Config"/>Object to serialize in JSON</param>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public static String? ToJson(Config Config, Boolean avoidNull = true, Boolean indent = false)
            => ToJsonNode(Config)?.ToString(avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// **`static method`** Returns a JSONNode equivalent of <see cref="Config"/> object.
        /// </summary>
        /// <param name="config"><see cref="Config"/>Config object</param>
        /// <returns><see cref="JSONNode"/> - JSONNode on success, Null on error</returns>
        public static JSONNode? ToJsonNode(Config? config)
        {
            if (config == null) return null;

            var jsonNode = new JSONObject();
            jsonNode["operator"] = config.Operator?.ToJsonNode() ?? JSONNull.CreateOrGet();
            jsonNode["bubble"] = new JSONObject();
            jsonNode["bubble"]["name"] = config.Bubble?.Peer?.DisplayName;
            jsonNode["bubble"]["topic"] = config.Bubble?.Topic;
            jsonNode["phoneNumbers"] = config.PhoneNumbers;
            jsonNode["maxPhoneNumberToAddInFirstRow"] = config.MaxPhoneNumberToAddInFirstRow;
            jsonNode["addNewPhoneNumberWhen"] = config.AddNewPhoneNumberWhen;

            return jsonNode;
        }

        /// <summary>
        /// Implicit Constructor from <see cref="Config"/> list to <see cref="JSONNode"/>.
        /// </summary>
        /// <param name="objectToJsonNode"><see cref="Config"/>Config</param>
        public static implicit operator JSONNode?(Config objectToJsonNode)
            => ToJsonNode(objectToJsonNode);

        /// <summary>
        /// Implicit Constructor from <see cref="JSONNode"/> to <see cref="Config"/> list.
        /// </summary>s
        /// <param name="jsonNodeToObject"><see cref="JSONNode"/>JSONNode Value</param>
        public static implicit operator Config?(JSONNode jsonNodeToObject)
            => FromJsonNode(jsonNodeToObject);

#endregion FromJSON / ToJSON methods

    }
}
