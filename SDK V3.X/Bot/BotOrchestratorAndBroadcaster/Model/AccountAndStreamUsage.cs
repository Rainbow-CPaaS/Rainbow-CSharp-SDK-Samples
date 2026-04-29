using BotLibrary.Model;
using Rainbow.SimpleJSON;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotOrchestratorAndBroadcaster.Model
{
    public class AccountAndStreamUsage: Account
    {
        public String AudioStreamId { get; set; } = String.Empty;

        public String VideoStreamId { get; set; } = String.Empty;

        public String SharingStreamId { get; set; } = String.Empty;

        public String InConf{ get; set; } = "false";


#region FromJSON / ToJSON methods


        /// <summary>
        /// **`static method`** Converts the specified JSON String to its <see cref="AccountAndStreamUsage"/> equivalent.
        /// </summary>
        /// <param name="jsonString"><see cref="String"/>JSON String</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="AccountAndStreamUsage"/> - AccountAndStreamUsage object or Null on error</returns>
        public static new AccountAndStreamUsage? FromJson(string jsonString, String? nodeName = null)
            => FromJsonNode(JSON.Parse(jsonString), nodeName);

        /// <summary>
        /// **`static method`** Converts the specified <see cref="JSONNode"/> to its <see cref="AccountAndStreamUsage"/> equivalent.
        /// </summary>
        /// <param name="jsonNode"><see cref="JSONNode"/>JSONNode object</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="AccountAndStreamUsage"/> - AccountAndStreamUsage object or Null on error</returns>
        public static new AccountAndStreamUsage? FromJsonNode(JSONNode jsonNode, String? nodeName = null)
        {
            if ((jsonNode == null) || (!jsonNode.IsObject))
                return null;

            if (!String.IsNullOrWhiteSpace(nodeName))
                return FromJsonNode(jsonNode[nodeName]);


            var account = Account.FromJsonNode(jsonNode);
            if (account == null) return null;

            AccountAndStreamUsage accountAndStreamUsage = new()
            {
                Id = account.Id,
                Jid = account.Jid,
                Login = account.Login,
                Password = account.Password,
                FirstName = account.FirstName,
                LastName = account.LastName,

                AudioStreamId = jsonNode["audioStreamId"],
                VideoStreamId = jsonNode["videoStreamId"],
                SharingStreamId = jsonNode["sharingStreamId"],
                InConf = jsonNode["inConf"],
            };
            return accountAndStreamUsage;
        }

        /// <summary>
        /// Returns a JSON String equivalent of this <see cref="AccountAndStreamUsage"/> object.
        /// </summary>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public new String? ToJson(Boolean avoidNull = true, Boolean indent = false)
            => ToJson(this, avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// Returns a JSONNode equivalent of this <see cref="AccountAndStreamUsage"/> object.
        /// </summary>
        /// <returns><see cref="JSONNode"/> - JSONNode object</returns>
        public new JSONNode? ToJsonNode()
            => ToJsonNode(this);

        /// <summary>
        /// **`static method`** Returns a JSON String equivalent of this <see cref="AccountAndStreamUsage"/> object.
        /// </summary>
        /// <param name="AccountAndStreamUsage"><see cref="AccountAndStreamUsage"/>Object to serialize in JSON</param>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public static String? ToJson(AccountAndStreamUsage AccountAndStreamUsage, Boolean avoidNull = true, Boolean indent = false)
            => ToJsonNode(AccountAndStreamUsage)?.ToString(avoidNull: avoidNull, indent: indent);

        /// <summary>   
        /// **`static method`** Returns a JSONNode equivalent of <see cref="AccountAndStreamUsage"/> object.
        /// </summary>
        /// <param name="accountAndStreamUsage"><see cref="AccountAndStreamUsage"/>AccountAndStreamUsage object</param>
        /// <returns><see cref="JSONNode"/> - JSONNode on success, Null on error</returns>
        public static JSONNode? ToJsonNode(AccountAndStreamUsage accountAndStreamUsage)
        {
            if (accountAndStreamUsage == null) return null;

            var jsonNode = Account.ToJsonNode(accountAndStreamUsage);
            if(jsonNode is null) return null;

            jsonNode["audioStreamId"] = (accountAndStreamUsage.AudioStreamId == "-") ? "" : accountAndStreamUsage.AudioStreamId;
            jsonNode["videoStreamId"] = (accountAndStreamUsage.VideoStreamId == "-") ? "" : accountAndStreamUsage.VideoStreamId;
            jsonNode["sharingStreamId"] = (accountAndStreamUsage.SharingStreamId == "-") ? "" : accountAndStreamUsage.SharingStreamId;
            jsonNode["inConf"] = accountAndStreamUsage.InConf;
            return jsonNode;
        }

        /// <summary>
        /// Implicit Constructor from <see cref="AccountAndStreamUsage"/> list to <see cref="JSONNode"/>.
        /// </summary>
        /// <param name="objectToJsonNode"><see cref="AccountAndStreamUsage"/>AccountAndStreamUsage</param>
        public static implicit operator JSONNode?(AccountAndStreamUsage objectToJsonNode)
            => ToJsonNode(objectToJsonNode);

        /// <summary>
        /// Implicit Constructor from <see cref="JSONNode"/> to <see cref="AccountAndStreamUsage"/> list.
        /// </summary>s
        /// <param name="jsonNodeToObject"><see cref="JSONNode"/>JSONNode Value</param>
        public static implicit operator AccountAndStreamUsage?(JSONNode jsonNodeToObject)
            => FromJsonNode(jsonNodeToObject);

#endregion FromJSON / ToJSON methods

    }
}
