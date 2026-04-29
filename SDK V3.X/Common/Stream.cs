using Rainbow.SimpleJSON;

namespace Rainbow.Example.Common
{
    public class Stream
    {
        public static readonly List<String> mediaPossible = ["audio", "video", "audio+video", "composition"];
        public static readonly List<String> uriTypePossible = ["screen", "webcam", "microphone", "other"];

        /// <summary>
        /// Unique id for this stream
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Type of media used in this stream : 'audio', 'video', 'audio+video' or 'composition'",
        /// </summary>
        public string Media { get; set; }

        /// <summary>
        /// (optional) if it's a video composition, list of streams id used to create it. Their order is important and related to the VideoFilter property
        /// </summary>
        public List<string>? VideoComposition { get; set; }

        /// <summary>
        /// (optional) if it's not a composition, uri (local or distant) to use
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// (optional) if it's not a composition, define the type of the uri: "screen", "webcam", "microphone" or "other" (default value)
        /// According it's value, Media is set. 
        /// For example:
        /// - if "screen" is used, Media will be set to "video"
        /// - if "microphone" is used, Media will be set to "audio"
        /// </summary>
        public string UriType { get; set; }

        /// <summary>
        /// (optional) if it's not a composition, settings used to connect to the uri (can be null/empty) => ffmpeg parameters",
        /// </summary>
        public Dictionary<string, string>? UriSettings { get; set; }

        /// <summary>
        /// True if the bot needs to stay connected on the Uri even if the media is not used in the conference
        /// </summary>
        public Boolean Connected { get; set; }

        /// <summary>
        /// If it's a video composition, video filter used to create it. 
        /// If it's not, video filter (optional) used on the stream before to broadcast it to the conference
        /// </summary>
        public string? VideoFilter { get; set; }

        /// <summary>
        /// Based on JSON description, used to create VideoFilter
        /// </summary>
        public JSONNode? VideoFilterJsonNode { get; set; }

        // Video Filter used in the conference - not set directly in JSON
        public string? VideoFilterInConference { get; set; }

        /// <summary>
        /// Can help in very rare case - false by default
        /// </summary>
        public bool? ForceLiveStream { get; set; }

        public Stream()
        {
            Id = "";
            Media = "";
            Uri = "";
            UriType = "other";
            UriSettings = null;
            VideoComposition = null;
            Connected = false;
            VideoFilter = null;
            VideoFilterJsonNode = null;
            ForceLiveStream = null;
        }

        public static Stream FromStream(Stream other)
        {
            Stream result = new()
            {
                Id = other.Id,
                Media = other.Media,
                Uri = other.Uri,
                UriType = other.UriType,
                UriSettings = other.UriSettings,
                VideoComposition = other.VideoComposition,
                Connected = other.Connected,
                VideoFilter = other.VideoFilter,
                VideoFilterJsonNode = other.VideoFilterJsonNode,
                ForceLiveStream = other.ForceLiveStream,
            };
            return result;
        }

        public static Boolean FromJsonNode(JSONNode jsonNode, out Stream stream)
        {
            if (jsonNode is not null)
            {
                stream = new()
                {
                    Id = jsonNode["id"],
                    Media = jsonNode["media"],
                    VideoComposition = jsonNode["videoComposition"],
                    Uri = jsonNode["uri"],
                    UriType = jsonNode["uriType"],
                    UriSettings = jsonNode["uriSettings"],
                    Connected = jsonNode["connected"],
                    //VideoFilter = jsonNode["videoFilter"],
                    ForceLiveStream = jsonNode["forceLiveStream"],
                };

                if (jsonNode["videoFilter"]?.IsObject == true)
                {
                    stream.VideoFilterJsonNode = jsonNode["videoFilter"];
                }
                else
                    stream.VideoFilter = jsonNode["videoFilter"];

                // Check validity
                if (String.IsNullOrEmpty(stream.Id))
                    return false;

                // Check UriType
                if (String.IsNullOrEmpty(stream.UriType))
                    stream.UriType = "other";

                if (!uriTypePossible.Contains(stream.UriType))
                    return false;

                switch (stream.UriType)
                {
                    case "screen":
                    case "webcam":
                        stream.Media = "video";
                        break;

                    case "microphone":
                        stream.Media = "audio";
                        break;
                }

                if (!mediaPossible.Contains(stream.Media))
                    return false;


                if (stream.Media.Equals("composition", StringComparison.InvariantCultureIgnoreCase))
                {
                    if ((stream.VideoComposition is null) || (stream.VideoComposition.Count == 0))
                        return false;
                }
                else
                {
                    if (String.IsNullOrEmpty(stream.Uri))
                        return false;
                }

                return true;
            }
            else
                stream = new();

            return false;
        }

        public Boolean IsSame(Stream? other)
        {
            if ((other is null)
                || (other.Id != Id)
                || (other.Media != Media)
                || (other.Uri != Uri)
                || (other.VideoFilter != VideoFilter)
                || (other.VideoFilterJsonNode?.ToString() != VideoFilterJsonNode?.ToString())
                || (other.ForceLiveStream != ForceLiveStream)
                || ((VideoComposition is not null) && (other.VideoComposition is not null) && (!VideoComposition.SequenceEqual(other.VideoComposition)))
                )
                return false;

            return true;
        }

        public override string ToString()
        {
            string result = $"Id:[{Id}] - Media:[{Media}]";

            if (VideoComposition is not null)
                result += $" - VideoComposition:[{String.Join(", ", VideoComposition)}]";
            else
            {
                var uriType = UriType.Equals("other") ? "" : $" - UriType:[{UriType}]" ;
                var uriSettings = UriSettings is null ? "" : $" - UriSettings:[{((UriSettings is null) ? "None" : String.Join(", ", UriSettings))}]";
                var forceLiveStream = ForceLiveStream is null ? "" : $" - ForceLiveStream:[{ForceLiveStream.Value}]";

                result += $" - Uri:[{Uri}]{uriType}{uriSettings}{forceLiveStream}";
            }
            return result;
        }

        public Boolean IsValid()
        {
            if (String.IsNullOrEmpty(Id))
                return false;

            if (!uriTypePossible.Contains(UriType))
                return false;

            if (!mediaPossible.Contains(Media))
                return false;

            if (Media.Equals("composition", StringComparison.InvariantCultureIgnoreCase))
            {
                if ((VideoComposition is null) || (VideoComposition.Count == 0))
                    return false;
            }
            else
            {
                if (String.IsNullOrEmpty(Uri))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check one by one if stream is valid. 
        /// 
        /// Check if composition can be well created (i.e. they reference streams known)
        /// 
        /// Filters are not checked here.
        /// </summary>
        /// <param name="listStreams">List of streams to check</param>
        /// <returns>list of valid streams </returns>
        static public List<Stream> GetValidStreams(List<Stream> listStreams)
        {
            List<Stream> result = new();
            if (listStreams is not null)
            {
                List<Stream> compositionsList = [];

                // Loop to check stream validity - Store a specific list composition to check them after
                foreach (var stream in listStreams)
                {
                    if (stream?.IsValid() == true)
                    {
                        // Store composition a part
                        if (stream.Media.Equals("composition"))
                            compositionsList.Add(stream);
                        else
                            result.Add(stream);
                    }
                }

                // Now check if composition are really useable (i.e. they reference media known)
                foreach (var stream in compositionsList)
                {
                    if (stream.VideoComposition is null)
                        continue;

                    var streamsFound = result.Where(x => stream.VideoComposition.Contains(x.Id) == true).ToList();
                    if (streamsFound.Count == stream.VideoComposition.Count)
                        result.Add(stream);
                }
            }
            return result;
        }


#region FromJSON / ToJSON methods


        /// <summary>
        /// **`static method`** Converts the specified JSON String to its <see cref="Stream"/> equivalent.
        /// </summary>
        /// <param name="jsonString"><see cref="String"/>JSON String</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="Stream"/> - Stream object or Null on error</returns>
        public static Stream? FromJson(string jsonString, String? nodeName = null)
            => FromJsonNode(JSON.Parse(jsonString), nodeName);

        /// <summary>
        /// **`static method`** Converts the specified <see cref="JSONNode"/> to its <see cref="Stream"/> equivalent.
        /// </summary>
        /// <param name="jsonNode"><see cref="JSONNode"/>JSONNode object</param>
        /// <param name="nodeName"><see cref="String"/>**`Optional - default value: null`** <br/>Node name to use to start parsing</param>
        /// <returns><see cref="Stream"/> - Stream object or Null on error</returns>
        public static Stream? FromJsonNode(JSONNode jsonNode, String? nodeName = null)
        {
            if ((jsonNode == null) || (!jsonNode.IsObject))
                return null;

            if (!String.IsNullOrWhiteSpace(nodeName))
                return FromJsonNode(jsonNode[nodeName]);

            Stream stream = new()
            {
                Id = jsonNode["id"],
                Media = jsonNode["media"],
                VideoComposition = jsonNode["videoComposition"],
                Uri = jsonNode["uri"],
                UriType = jsonNode["uriType"],
                UriSettings = jsonNode["uriSettings"],
                Connected = jsonNode["connected"],
                ForceLiveStream = jsonNode["forceLiveStream"],
            };

            // Set VideoFilterJsonNode / VideoFilter properties
            if (jsonNode["videoFilter"]?.IsObject == true)
                stream.VideoFilterJsonNode = jsonNode["videoFilter"];
            else
                stream.VideoFilter = jsonNode["videoFilter"];

            // Check UriType
            if (String.IsNullOrEmpty(stream.UriType))
                stream.UriType = "other";

            // Set media according UriType
            switch (stream.UriType)
            {
                case "screen":
                case "webcam":
                    stream.Media = "video";
                    break;

                case "microphone":
                    stream.Media = "audio";
                    break;
            }

            return stream;
        }

        /// <summary>
        /// Returns a JSON String equivalent of this <see cref="Stream"/> object.
        /// </summary>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public String? ToJson(Boolean avoidNull = true, Boolean indent = false)
            => ToJson(this, avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// Returns a JSONNode equivalent of this <see cref="Stream"/> object.
        /// </summary>
        /// <returns><see cref="JSONNode"/> - JSONNode object</returns>
        public JSONNode? ToJsonNode()
            => ToJsonNode(this);

        /// <summary>
        /// **`static method`** Returns a JSON String equivalent of this <see cref="Stream"/> object.
        /// </summary>
        /// <param name="Stream"><see cref="Stream"/>Object to serialize in JSON</param>
        /// <param name="avoidNull"><see cref="Boolean"/>**`Optional - default value: true`** <br/>True to avoid null values</param>
        /// <param name="indent"><see cref="Boolean"/>**`Optional - default value: false`** <br/>True to indent</param>
        /// <returns><see cref="String"/> - String on success, Null on error</returns>
        public static String? ToJson(Stream Stream, Boolean avoidNull = true, Boolean indent = false)
            => ToJsonNode(Stream)?.ToString(avoidNull: avoidNull, indent: indent);

        /// <summary>
        /// **`static method`** Returns a JSONNode equivalent of <see cref="Stream"/> object.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/>Stream object</param>
        /// <returns><see cref="JSONNode"/> - JSONNode on success, Null on error</returns>
        public static JSONNode? ToJsonNode(Stream stream)
        {
            if (stream == null) return null;

            var jsonNode = new JSONObject();
            jsonNode["id"] = stream.Id;
            jsonNode["media"] = stream.Media;
            jsonNode["videoComposition"] = stream.VideoComposition;
            jsonNode["uri"] = stream.Uri;
            if (stream.UriType == "other")
                jsonNode["uriType"] = null;
            else
                jsonNode["uriType"] = stream.UriType;
            jsonNode["uriSettings"] = stream.UriSettings;
            jsonNode["connected"] = stream.Connected;
            jsonNode["forceLiveStream"] = stream.ForceLiveStream;

            // Set videoFilter
            jsonNode["videoFilter"] = stream.VideoFilterJsonNode ?? stream.VideoFilter;
            //if (stream.VideoFilterJsonNode is not null)
            //    jsonNode["videoFilter"] = stream.VideoFilterJsonNode;
            //else
            //    jsonNode["videoFilter"] = stream.VideoFilter;

            return jsonNode;
        }

        /// <summary>
        /// Implicit Constructor from <see cref="Stream"/> list to <see cref="JSONNode"/>.
        /// </summary>
        /// <param name="objectToJsonNode"><see cref="Stream"/>Stream</param>
        public static implicit operator JSONNode?(Stream objectToJsonNode)
            => ToJsonNode(objectToJsonNode);

        /// <summary>
        /// Implicit Constructor from <see cref="JSONNode"/> to <see cref="Stream"/> list.
        /// </summary>s
        /// <param name="jsonNodeToObject"><see cref="JSONNode"/>JSONNode Value</param>
        public static implicit operator Stream?(JSONNode jsonNodeToObject)
            => FromJsonNode(jsonNodeToObject);

#endregion FromJSON / ToJSON methods


    }
}
