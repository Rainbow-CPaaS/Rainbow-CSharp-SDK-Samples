using Rainbow.Example.Common;
using Rainbow.Model;
using Rainbow.SimpleJSON;

namespace AdaptiveCards
{
    public class Configuration
    {
        public JSONNode? Labels { get; set; } // Parsed from configuration.json

        public Account? ContactConfiguerd { get; set; } // Parsed from configuration.json
        public Contact? Contact { get; set; } // Retrieved from server using ContactConfiguerd

        public Account? BubbleConfiguerd { get; set; } // Parsed from configuration.json
        public Bubble? Bubble { get; set; } // Retrieved from server using BubbleConfiguerd

        public List<Train>? Trains { get; set; } // Parsed from configuration.json

        public List<List<List<TrainStatus>>>? Steps { get; set; } // Parsed from configuration.json

        public int DelayBetweenAC { get; set; } // Parsed from configuration.json

        public String? currentMessageIdWithAC = null;
        public int CurrentStep { get; set; } = 0;
        public int CurrentSubStep { get; set; } = 0;

        public void Reset()
        {
            CurrentStep = CurrentSubStep = 0;
        }
    }


    public class TrainStatus
    {
        public String? Id { get; set; }
        public String? Status { get; set; }

#region FromJSON / ToJSON methods

        /// <summary>
        /// **`static method`** Converts the specified JSON String to its <see cref="TrainStatus"/> equivalent.
        /// </summary>
        /// <param name="jsonString"><see cref="String"/>JSON String</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="TrainStatus"/> - TrainStatus object or Null on error</returns>
        public static TrainStatus? FromJson(string jsonString, String? nodeName = null)
            => FromJsonNode(JSON.Parse(jsonString), nodeName);

        /// <summary>
        /// **`static method`** Converts the specified <see cref="JSONNode"/> to its <see cref="TrainStatus"/> equivalent.
        /// </summary>
        /// <param name="jsonNode"><see cref="JSONNode"/>JSONNode object</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="TrainStatus"/> - TrainStatus object or Null on error</returns>
        public static TrainStatus? FromJsonNode(JSONNode? jsonNode, String? nodeName = null)
        {
            if ((jsonNode == null) || (!jsonNode.IsObject))
                return null;

            if (!String.IsNullOrWhiteSpace(nodeName))
                return FromJsonNode(jsonNode[nodeName]);

            TrainStatus TrainStatus = new()
            {
                Id = jsonNode["id"],
                Status = jsonNode["status"]
            };

            return TrainStatus;
        }

        /// <summary>
        /// Returns a JSON String equivalent of this <see cref="TrainStatus"/> object.
        /// </summary>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public String? ToJson(Boolean avoidNull = true, Boolean indent = false)
            => ToJson(this, avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// Returns a JSONNode equivalent of this <see cref="TrainStatus"/> object.
        /// </summary>
        /// <returns><see cref="JSONNode"/> - JSONNode object</returns>
        public JSONNode? ToJsonNode()
            => ToJsonNode(this);

        /// <summary>
        /// **`static method`** Returns a JSON String equivalent of this <see cref="TrainStatus"/> object.
        /// </summary>
        /// <param name="TrainStatus"><see cref="TrainStatus"/>Object to serialize in JSON</param>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public static String? ToJson(TrainStatus TrainStatus, Boolean avoidNull = true, Boolean indent = false)
            => ToJsonNode(TrainStatus)?.ToString(avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// **`static method`** Returns a JSONNode equivalent of <see cref="TrainStatus"/> object.
        /// </summary>
        /// <param name="TrainStatus"><see cref="TrainStatus"/>TrainStatus object</param>
        /// <returns><see cref="JSONNode"/> - JSONNode on success, Null on error</returns>
        public static JSONNode? ToJsonNode(TrainStatus TrainStatus)
        {
            if (TrainStatus == null) return null;

            var jsonNode = new JSONObject();
            jsonNode["id"] = TrainStatus.Id;
            jsonNode["status"] = TrainStatus.Status;
            return jsonNode;
        }

        /// <summary>
        /// Implicit Constructor from <see cref="TrainStatus"/> list to <see cref="JSONNode"/>.
        /// </summary>
        /// <param name="objectToJsonNode"><see cref="TrainStatus"/>TrainStatus</param>
        public static implicit operator JSONNode?(TrainStatus objectToJsonNode)
            => ToJsonNode(objectToJsonNode);

        /// <summary>
        /// Implicit Constructor from <see cref="JSONNode"/> to <see cref="TrainStatus"/> list.
        /// </summary>s
        /// <param name="jsonNodeToObject"><see cref="JSONNode"/>JSONNode Value</param>
        public static implicit operator TrainStatus?(JSONNode jsonNodeToObject)
            => FromJsonNode(jsonNodeToObject);

#endregion FromJSON / ToJSON methods
    }


    public class Train
    {
        public String? Id { get; set; }
        public String? Name { get; set; }
        public String? PhoneNumber { get; set; }

#region FromJSON / ToJSON methods

        /// <summary>
        /// **`static method`** Converts the specified JSON String to its <see cref="Train"/> equivalent.
        /// </summary>
        /// <param name="jsonString"><see cref="String"/>JSON String</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="Train"/> - Train object or Null on error</returns>
        public static Train? FromJson(string jsonString, String? nodeName = null)
            => FromJsonNode(JSON.Parse(jsonString), nodeName);

        /// <summary>
        /// **`static method`** Converts the specified <see cref="JSONNode"/> to its <see cref="Train"/> equivalent.
        /// </summary>
        /// <param name="jsonNode"><see cref="JSONNode"/>JSONNode object</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="Train"/> - Train object or Null on error</returns>
        public static Train? FromJsonNode(JSONNode? jsonNode, String? nodeName = null)
        {
            if ((jsonNode == null) || (!jsonNode.IsObject))
                return null;

            if (!String.IsNullOrWhiteSpace(nodeName))
                return FromJsonNode(jsonNode[nodeName]);

            Train Train = new()
            {
                Id = jsonNode["id"],
                Name = jsonNode["name"],
                PhoneNumber = jsonNode["phoneNumber"]
            };

            return Train;
        }

        /// <summary>
        /// Returns a JSON String equivalent of this <see cref="Train"/> object.
        /// </summary>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public String? ToJson(Boolean avoidNull = true, Boolean indent = false)
            => ToJson(this, avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// Returns a JSONNode equivalent of this <see cref="Train"/> object.
        /// </summary>
        /// <returns><see cref="JSONNode"/> - JSONNode object</returns>
        public JSONNode? ToJsonNode()
            => ToJsonNode(this);

        /// <summary>
        /// **`static method`** Returns a JSON String equivalent of this <see cref="Train"/> object.
        /// </summary>
        /// <param name="Train"><see cref="Train"/>Object to serialize in JSON</param>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public static String? ToJson(Train Train, Boolean avoidNull = true, Boolean indent = false)
            => ToJsonNode(Train)?.ToString(avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// **`static method`** Returns a JSONNode equivalent of <see cref="Train"/> object.
        /// </summary>
        /// <param name="Train"><see cref="Train"/>Train object</param>
        /// <returns><see cref="JSONNode"/> - JSONNode on success, Null on error</returns>
        public static JSONNode? ToJsonNode(Train Train)
        {
            if (Train == null) return null;

            var jsonNode = new JSONObject();
            jsonNode["id"] = Train.Id;
            jsonNode["name"] = Train.Name;
            jsonNode["phoneNumber"] = Train.PhoneNumber;
            return jsonNode;
        }

        /// <summary>
        /// Implicit Constructor from <see cref="Train"/> list to <see cref="JSONNode"/>.
        /// </summary>
        /// <param name="objectToJsonNode"><see cref="Train"/>Train</param>
        public static implicit operator JSONNode?(Train objectToJsonNode)
            => ToJsonNode(objectToJsonNode);

        /// <summary>
        /// Implicit Constructor from <see cref="JSONNode"/> to <see cref="Train"/> list.
        /// </summary>s
        /// <param name="jsonNodeToObject"><see cref="JSONNode"/>JSONNode Value</param>
        public static implicit operator Train?(JSONNode jsonNodeToObject)
            => FromJsonNode(jsonNodeToObject);

#endregion FromJSON / ToJSON methods
    }
}
