using Rainbow.SimpleJSON;
using System;

namespace BotBroadcaster.Model
{
    public class Conference
    {
        /// <summary>
        /// Conference / Bubble Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Conference / Bubble Jid
        /// </summary>
        public string Jid { get; set; }

        /// <summary>
        /// Conference / Bubble Jid
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Audio Stream Id
        /// </summary>
        public string AudioStreamId { get; set; }

        /// <summary>
        /// Video Stream Id
        /// </summary>
        public string VideoStreamId { get; set; }

        /// <summary>
        /// Sharing Stream Id
        /// </summary>
        public string SharingStreamId { get; set; }

        public Conference() 
        {
            Id = "";
            Jid = "";
            Name = "";
            AudioStreamId   = "";
            VideoStreamId   = "";
            SharingStreamId = "";
        }

        public Boolean IsValid()
            => !String.IsNullOrEmpty(Id) || !String.IsNullOrEmpty(Jid) || !String.IsNullOrEmpty(Name);


#region FromJSON / ToJSON methods


        /// <summary>
        /// **`static method`** Converts the specified JSON String to its <see cref="Conference"/> equivalent.
        /// </summary>
        /// <param name="jsonString"><see cref="String"/>JSON String</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="Conference"/> - Conference object or Null on error</returns>
        public static Conference? FromJson(string jsonString, String? nodeName = null)
            => FromJsonNode(JSON.Parse(jsonString), nodeName);

        /// <summary>
        /// **`static method`** Converts the specified <see cref="JSONNode"/> to its <see cref="Conference"/> equivalent.
        /// </summary>
        /// <param name="jsonNode"><see cref="JSONNode"/>JSONNode object</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="Conference"/> - Conference object or Null on error</returns>
        public static Conference? FromJsonNode(JSONNode jsonNode, String? nodeName = null)
        {
            if ((jsonNode == null) || (!jsonNode.IsObject))
                return null;

            if (!String.IsNullOrWhiteSpace(nodeName))
                return FromJsonNode(jsonNode[nodeName]);

            Conference conference = new()
            {
                Id = jsonNode["id"],
                Jid = jsonNode["jid"],
                Name = jsonNode["name"],
                AudioStreamId = jsonNode["audioStreamId"],
                VideoStreamId = jsonNode["videoStreamId"],
                SharingStreamId = jsonNode["sharingStreamId"]
            };

            return conference;
        }

        /// <summary>
        /// Returns a JSON String equivalent of this <see cref="Conference"/> object.
        /// </summary>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public String? ToJson(Boolean avoidNull = true, Boolean indent = false)
            => ToJson(this, avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// Returns a JSONNode equivalent of this <see cref="Conference"/> object.
        /// </summary>
        /// <returns><see cref="JSONNode"/> - JSONNode object</returns>
        public JSONNode? ToJsonNode()
            => ToJsonNode(this);

        /// <summary>
        /// **`static method`** Returns a JSON String equivalent of this <see cref="Conference"/> object.
        /// </summary>
        /// <param name="Conference"><see cref="Conference"/>Object to serialize in JSON</param>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public static String? ToJson(Conference Conference, Boolean avoidNull = true, Boolean indent = false)
            => ToJsonNode(Conference)?.ToString(avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// **`static method`** Returns a JSONNode equivalent of <see cref="Conference"/> object.
        /// </summary>
        /// <param name="conference"><see cref="Conference"/>Conference object</param>
        /// <returns><see cref="JSONNode"/> - JSONNode on success, Null on error</returns>
        public static JSONNode? ToJsonNode(Conference conference)
        {
            if (conference == null) return null;

            var jsonNode = new JSONObject();
            jsonNode["id"] = conference.Id;
            jsonNode["jid"] = conference.Jid;
            jsonNode["name"] = conference.Name;
            jsonNode["audioStreamId"] = (conference.AudioStreamId == "-") ? "": conference.AudioStreamId;
            jsonNode["videoStreamId"] = (conference.VideoStreamId == "-") ? "" : conference.VideoStreamId;
            jsonNode["sharingStreamId"] = (conference.SharingStreamId == "-") ? "" : conference.SharingStreamId;

            return jsonNode;
        }

        /// <summary>
        /// Implicit Constructor from <see cref="Conference"/> list to <see cref="JSONNode"/>.
        /// </summary>
        /// <param name="objectToJsonNode"><see cref="Conference"/>Conference</param>
        public static implicit operator JSONNode?(Conference objectToJsonNode)
            => ToJsonNode(objectToJsonNode);

        /// <summary>
        /// Implicit Constructor from <see cref="JSONNode"/> to <see cref="Conference"/> list.
        /// </summary>s
        /// <param name="jsonNodeToObject"><see cref="JSONNode"/>JSONNode Value</param>
        public static implicit operator Conference?(JSONNode jsonNodeToObject)
            => FromJsonNode(jsonNodeToObject);

#endregion FromJSON / ToJSON methods
    }
}
