using BotLibrary.Model;
using Rainbow.Model;
using Rainbow.SimpleJSON;
using System;
using System.Collections.Generic;

namespace BotBroadcaster.Model
{
    public class P2P : StreamConfig
    {
        /// <summary>
        /// To store list of possible values for <see cref="AllowedFor"/> property.
        /// </summary>
        public static readonly List<String> PossibleValues = ["administrator", "all", "none"];

        /// <summary>
        /// To know which P2P are allowed for this bot - see <see cref="PossibleValues"/> for list of possible values. You can also specified a login email of a contact.
        /// </summary>
        public string AllowedFor { get; set; } = "none";

        internal Contact? Contact { get; set; } = null;

        /// <summary>
        /// True if the content of this object is valid (mainly checking if <see cref="AllowedFor"/> is valid or not null/empty), false otherwise.
        /// </summary>
        public Boolean IsValid
            => PossibleValues.Contains(AllowedFor) || !String.IsNullOrEmpty(AllowedFor);

#region FromJSON / ToJSON methods

        /// <summary>
        /// **`static method`** Converts the specified JSON String to its <see cref="P2P"/> equivalent.
        /// </summary>
        /// <param name="jsonString"><see cref="String"/>JSON String</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="P2P"/> - P2P object or Null on error</returns>
        public static P2P? FromJson(string jsonString, String? nodeName = null)
            => FromJsonNode(JSON.Parse(jsonString), nodeName);

        /// <summary>
        /// **`static method`** Converts the specified <see cref="JSONNode"/> to its <see cref="P2P"/> equivalent.
        /// </summary>
        /// <param name="jsonNode"><see cref="JSONNode"/>JSONNode object</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="P2P"/> - P2P object or Null on error</returns>
        public static P2P? FromJsonNode(JSONNode? jsonNode, String? nodeName = null)
        {
            if ((jsonNode == null) || (!jsonNode.IsObject))
                return null;

            if (!String.IsNullOrWhiteSpace(nodeName))
                return FromJsonNode(jsonNode[nodeName]);

            P2P P2P = new()
            {
                AllowedFor = (String?)jsonNode["allowedFor"] ?? "none",
                AudioStreamId = (String?)jsonNode["audioStreamId"] ?? "",
                VideoStreamId = (String?)jsonNode["videoStreamId"] ?? "",
                SharingStreamId = (String?)jsonNode["sharingStreamId"] ?? ""
            };

            return P2P;
        }

        /// <summary>
        /// Returns a JSON String equivalent of this <see cref="P2P"/> object.
        /// </summary>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public String? ToJson(Boolean avoidNull = true, Boolean indent = false)
            => ToJson(this, avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// Returns a JSONNode equivalent of this <see cref="P2P"/> object.
        /// </summary>
        /// <returns><see cref="JSONNode"/> - JSONNode object</returns>
        public JSONNode? ToJsonNode()
            => ToJsonNode(this);

        /// <summary>
        /// **`static method`** Returns a JSON String equivalent of this <see cref="P2P"/> object.
        /// </summary>
        /// <param name="P2P"><see cref="P2P"/>Object to serialize in JSON</param>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public static String? ToJson(P2P? P2P, Boolean avoidNull = true, Boolean indent = false)
            => ToJsonNode(P2P)?.ToString(avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// **`static method`** Returns a JSONNode equivalent of <see cref="P2P"/> object.
        /// </summary>
        /// <param name="P2P"><see cref="P2P"/>P2P object</param>
        /// <returns><see cref="JSONNode"/> - JSONNode on success, Null on error</returns>
        public static JSONNode ToJsonNode(P2P? P2P)
        {
            if (P2P == null) return JSONNull.CreateOrGet();

            var jsonNode = new JSONObject();
            jsonNode["allowedFor"] = P2P.AllowedFor;
            jsonNode["audioStreamId"] = (P2P.AudioStreamId == "-") ? "" : P2P.AudioStreamId;
            jsonNode["videoStreamId"] = (P2P.VideoStreamId == "-") ? "" : P2P.VideoStreamId;
            jsonNode["sharingStreamId"] = (P2P.SharingStreamId == "-") ? "" : P2P.SharingStreamId;

            return jsonNode;
        }

        /// <summary>
        /// Implicit Constructor from <see cref="P2P"/> list to <see cref="JSONNode"/>.
        /// </summary>
        /// <param name="objectToJsonNode"><see cref="P2P"/>P2P</param>
        public static implicit operator JSONNode?(P2P? objectToJsonNode)
            => ToJsonNode(objectToJsonNode);

        /// <summary>
        /// Implicit Constructor from <see cref="JSONNode"/> to <see cref="P2P"/> list.
        /// </summary>s
        /// <param name="jsonNodeToObject"><see cref="JSONNode"/>JSONNode Value</param>
        public static implicit operator P2P?(JSONNode? jsonNodeToObject)
            => FromJsonNode(jsonNodeToObject);

#endregion FromJSON / ToJSON methods
    }
}
