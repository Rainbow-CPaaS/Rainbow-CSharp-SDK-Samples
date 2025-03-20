using Rainbow.SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rainbow.Console
{
    public class Stream
    {
        public static readonly List<String> mediaPossible = [ "audio", "video", "audio+video", "composition" ];
        public static readonly List<String> uriTypePossible = [ "screen", "webcam", "microphone", "other" ];

        /// <summary>
        /// Unique id for this stream
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Type of media used in this stream : 'audio', 'video', 'audio+video' or 'composition'",
        /// </summary>
        public string Media { get; set; }

        // Media used in the conference - not set directly in JSON
        internal String? MediaInConference { get; set; }

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
        /// Accoring it's value, Media is set. 
        /// For example:
        /// - if "screen" is used, Media will be set to "video"
        /// - if "microphone" is used, Media will be set to "audio"
        /// </summary>
        public string UriType { get; set; }

        /// <summary>
        /// (optional) if it's not a composition, settings used to connected to the uri (can be null/empty) => ffmpeg parameters",
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
                ForceLiveStream= other.ForceLiveStream,

                // Internal property
                MediaInConference = other.MediaInConference,
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
                    UriType = jsonNode["uriType"].As<String>("other"),
                    UriSettings = jsonNode["uriSettings"],
                    Connected = jsonNode["connected"],
                    VideoFilter = jsonNode["videoFilter"],
                    ForceLiveStream = jsonNode["forceLiveStream"],
                };

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

        public static JSONNode? ToJsonNode(Stream stream)
        {
            if (stream == null) return null;

            var jsonNode = new JSONObject();

            jsonNode["id"] = stream.Id;
            jsonNode["media"] = stream.Media;
            jsonNode["uri"] = stream.Uri;
            if(stream.UriType == "other")
                jsonNode["uriType"] = null;
            else
                jsonNode["uriType"] = stream.UriType;
            jsonNode["uriSettings"] = stream.UriSettings;
            jsonNode["videoComposition"] = stream.VideoComposition;
            jsonNode["videoFilter"] = stream.VideoFilter;
            jsonNode["forceLiveStream"] = stream.ForceLiveStream;

            return jsonNode;
        }

        public Boolean IsSame(Stream? other)
        {
            if ( (other is null)
                || (other.Id != Id)
                || (other.Media != Media)
                || (other.Uri != Uri)
                || (other.VideoFilter != VideoFilter)
                || (other.ForceLiveStream != ForceLiveStream)
                || (other.MediaInConference != MediaInConference)
                || ( (VideoComposition is not null) && (other.VideoComposition is not null) && (!VideoComposition.SequenceEqual(other.VideoComposition)))
                )
                return false;

            return true;
        }

        public override string ToString()
        {
            string result = $"Id:[{Id}] - Media:[{Media}] - Uri:[{Uri}] - UriType:[{UriType}] - UriSettings:[{((UriSettings is null) ? "None" :String.Join(", ", UriSettings))}] - ForceLiveStream:[{ForceLiveStream}]";
            return result;
        }
    }
}
