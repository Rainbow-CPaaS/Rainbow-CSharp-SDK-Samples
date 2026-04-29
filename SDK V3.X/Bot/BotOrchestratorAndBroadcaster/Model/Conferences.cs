using Rainbow.SimpleJSON;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotOrchestratorAndBroadcaster.Model
{
    public class Conferences
    {
        public Boolean UseAll { get; set; } = false;

        // Not necessary: we use all bubbles where the orchestrator is a moderator
        //public List<String> List { get; set; } = [];

        public List<String> Selected { get; set; } = [];

        public Boolean RemoveAsMemberBroadcaster { get; set; } = false;

        public List<String> BroadcastersJoiningConf { get; set; } = [];


#region FromJSON / ToJSON methods


        /// <summary>
        /// **`static method`** Converts the specified JSON String to its <see cref="Conferences"/> equivalent.
        /// </summary>
        /// <param name="jsonString"><see cref="String"/>JSON String</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="Conferences"/> - Conferences object or Null on error</returns>
        public static Conferences? FromJson(string jsonString, String? nodeName = null)
            => FromJsonNode(JSON.Parse(jsonString), nodeName);

        /// <summary>
        /// **`static method`** Converts the specified <see cref="JSONNode"/> to its <see cref="Conferences"/> equivalent.
        /// </summary>
        /// <param name="jsonNode"><see cref="JSONNode"/>JSONNode object</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="Conferences"/> - Conferences object or Null on error</returns>
        public static Conferences? FromJsonNode(JSONNode jsonNode, String? nodeName = null)
        {
            if ((jsonNode == null) || (!jsonNode.IsObject))
                return null;

            if (!String.IsNullOrWhiteSpace(nodeName))
                return FromJsonNode(jsonNode[nodeName]);

            Conferences Conferences = new()
            {
                UseAll = jsonNode["useAll"],
                //List = jsonNode["list"],
                Selected = jsonNode["selected"],
                RemoveAsMemberBroadcaster = jsonNode["removeAsMemberBroadcaster"],
                BroadcastersJoiningConf = jsonNode["broadcastersJoiningConf"],
            };
            return Conferences;
        }

        /// <summary>
        /// Returns a JSON String equivalent of this <see cref="Conferences"/> object.
        /// </summary>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public String? ToJson(Boolean avoidNull = true, Boolean indent = false)
            => ToJson(this, avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// Returns a JSONNode equivalent of this <see cref="Conferences"/> object.
        /// </summary>
        /// <returns><see cref="JSONNode"/> - JSONNode object</returns>
        public JSONNode? ToJsonNode()
            => ToJsonNode(this);

        /// <summary>
        /// **`static method`** Returns a JSON String equivalent of this <see cref="Conferences"/> object.
        /// </summary>
        /// <param name="Conferences"><see cref="Conferences"/>Object to serialize in JSON</param>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public static String? ToJson(Conferences Conferences, Boolean avoidNull = true, Boolean indent = false)
            => ToJsonNode(Conferences)?.ToString(avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// **`static method`** Returns a JSONNode equivalent of <see cref="Conferences"/> object.
        /// </summary>
        /// <param name="Conferences"><see cref="Conferences"/>Conferences object</param>
        /// <returns><see cref="JSONNode"/> - JSONNode on success, Null on error</returns>
        public static JSONNode? ToJsonNode(Conferences Conferences)
        {
            if (Conferences == null) return null;

            var jsonNode = new JSONObject();
            jsonNode["useAll"] = Conferences.UseAll;
            //jsonNode["list"] = Conferences.List;
            jsonNode["selected"] = Conferences.Selected;
            jsonNode["removeAsMemberBroadcaster"] = Conferences.RemoveAsMemberBroadcaster;
            jsonNode["broadcastersJoiningConf"] = Conferences.BroadcastersJoiningConf;
            return jsonNode;
        }

        /// <summary>
        /// Implicit Constructor from <see cref="Conferences"/> list to <see cref="JSONNode"/>.
        /// </summary>
        /// <param name="objectToJsonNode"><see cref="Conferences"/>Conferences</param>
        public static implicit operator JSONNode?(Conferences objectToJsonNode)
            => ToJsonNode(objectToJsonNode);

        /// <summary>
        /// Implicit Constructor from <see cref="JSONNode"/> to <see cref="Conferences"/> list.
        /// </summary>s
        /// <param name="jsonNodeToObject"><see cref="JSONNode"/>JSONNode Value</param>
        public static implicit operator Conferences?(JSONNode jsonNodeToObject)
            => FromJsonNode(jsonNodeToObject);

#endregion FromJSON / ToJSON methods


    }
}
