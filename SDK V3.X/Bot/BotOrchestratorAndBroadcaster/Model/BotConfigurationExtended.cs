using BotLibrary.Model;
using Rainbow.Example.Common;
using Rainbow.SimpleJSON;
using System;
using System.Collections.Generic;

namespace BotOrchestratorAndBroadcaster.Model
{
    public class BotConfigurationExtended : BotLibrary.Model.BotConfiguration
    {
        public String Labels { get; set; } = "labels_EN.json";

        public List<AccountAndStreamUsage> Broadcasters { get; set; } = [];

        public Conferences Conferences { get; set; } = new();

        public List<Stream> Streams { get; set; } = [];

        public BotConfigurationExtended() : base()
        {
        }

#region FromJSON / ToJSON methods


        /// <summary>
        /// **`static method`** Converts the specified JSON String to its <see cref="BotConfigurationExtended"/> equivalent.
        /// </summary>
        /// <param name="jsonString"><see cref="String"/>JSON String</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="BotConfigurationExtended"/> - BotConfigurationExtended object or Null on error</returns>
        public static new BotConfigurationExtended? FromJson(string jsonString, String? nodeName = null)
            => FromJsonNode(JSON.Parse(jsonString), nodeName);

        /// <summary>
        /// **`static method`** Converts the specified <see cref="JSONNode"/> to its <see cref="BotConfigurationExtended"/> equivalent.
        /// </summary>
        /// <param name="jsonNode"><see cref="JSONNode"/>JSONNode object</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="BotConfigurationExtended"/> - BotConfigurationExtended object or Null on error</returns>
        public static new BotConfigurationExtended? FromJsonNode(JSONNode jsonNode, String? nodeName = null)
        {
            if ((jsonNode == null) || (!jsonNode.IsObject))
                return null;

            if (!String.IsNullOrWhiteSpace(nodeName))
                return FromJsonNode(jsonNode[nodeName]);

            var botConfiguration = BotConfiguration.FromJsonNode(jsonNode);
            if (botConfiguration is null) return null;

            BotConfigurationExtended botConfigurationExtended = new()
            {
                // Get data from BotConfiguration part
                Administrators = botConfiguration.Administrators,
                GuestsAccepted = botConfiguration.GuestsAccepted,
                InstantMessageAutoAccept = botConfiguration.InstantMessageAutoAccept,
                PrivateMessageAutoAccept = botConfiguration.PrivateMessageAutoAccept,
                AckMessageAutoAccept = botConfiguration.AckMessageAutoAccept,
                ApplicationMessageAutoAccept = botConfiguration.ApplicationMessageAutoAccept,
                BubbleInvitationAutoAccept = botConfiguration.BubbleInvitationAutoAccept,
                UserInvitationAutoAccept = botConfiguration.UserInvitationAutoAccept,
                FirstName = botConfiguration.FirstName,
                LastName = botConfiguration.LastName,
                Bot = botConfiguration.Bot,

                Labels = jsonNode["labels"],
                Broadcasters = JSON.ToList<AccountAndStreamUsage>(jsonNode["broadcasters"], AccountAndStreamUsage.FromJsonNode),
                Conferences = Conferences.FromJsonNode(jsonNode["conferences"]) ?? new Conferences(),
                Streams = JSON.ToList<Stream>(jsonNode["streams"], Stream.FromJsonNode),
            };

            if(String.IsNullOrWhiteSpace(botConfigurationExtended.Labels))
                botConfigurationExtended.Labels = "labels_EN.json"; // default value

            return botConfigurationExtended;
        }

        /// <summary>
        /// Returns a JSON String equivalent of this <see cref="BotConfigurationExtended"/> object.
        /// </summary>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public new String? ToJson(Boolean avoidNull = true, Boolean indent = false)
            => ToJson(this, avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// Returns a JSONNode equivalent of this <see cref="BotConfigurationExtended"/> object.
        /// </summary>
        /// <returns><see cref="JSONNode"/> - JSONNode object</returns>
        public new JSONNode? ToJsonNode()
            => ToJsonNode(this);

        /// <summary>
        /// **`static method`** Returns a JSON String equivalent of this <see cref="BotConfigurationExtended"/> object.
        /// </summary>
        /// <param name="BotConfigurationExtended"><see cref="BotConfigurationExtended"/>Object to serialize in JSON</param>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public static String? ToJson(BotConfigurationExtended BotConfigurationExtended, Boolean avoidNull = true, Boolean indent = false)
            => ToJsonNode(BotConfigurationExtended)?.ToString(avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// **`static method`** Returns a JSONNode equivalent of <see cref="BotConfigurationExtended"/> object.
        /// </summary>
        /// <param name="botConfigurationExtended"><see cref="BotConfigurationExtended"/>BotConfigurationExtended object</param>
        /// <returns><see cref="JSONNode"/> - JSONNode on success, Null on error</returns>
        public static JSONNode? ToJsonNode(BotConfigurationExtended botConfigurationExtended)
        {
            if (botConfigurationExtended == null) return null;

            var jsonNode = BotConfiguration.ToJsonNode(botConfigurationExtended);
            if(jsonNode is null) return null;

            jsonNode["labels"] = botConfigurationExtended.Labels;
            jsonNode["broadcasters"] = JSON.ToJSONArray<AccountAndStreamUsage>(botConfigurationExtended.Broadcasters, AccountAndStreamUsage.ToJsonNode);
            jsonNode["conferences"] = JSON.ToJsonNode(botConfigurationExtended.Conferences);
            jsonNode["streams"] = JSON.ToJSONArray<Stream>(botConfigurationExtended.Streams, Stream.ToJsonNode);
            return jsonNode;
        }

        /// <summary>
        /// Implicit Constructor from <see cref="BotConfigurationExtended"/> list to <see cref="JSONNode"/>.
        /// </summary>
        /// <param name="objectToJsonNode"><see cref="BotConfigurationExtended"/>BotConfigurationExtended</param>
        public static implicit operator JSONNode?(BotConfigurationExtended objectToJsonNode)
            => ToJsonNode(objectToJsonNode);

        /// <summary>
        /// Implicit Constructor from <see cref="JSONNode"/> to <see cref="BotConfigurationExtended"/> list.
        /// </summary>s
        /// <param name="jsonNodeToObject"><see cref="JSONNode"/>JSONNode Value</param>
        public static implicit operator BotConfigurationExtended?(JSONNode jsonNodeToObject)
            => FromJsonNode(jsonNodeToObject);

#endregion FromJSON / ToJSON methods

    }
}
