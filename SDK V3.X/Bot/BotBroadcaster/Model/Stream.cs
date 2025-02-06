using Rainbow.SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BotBroadcaster.Model
{
    public class Stream
    {
        public static List<String> mediaPossible = new List<String>() { "audio", "video", "audio+video", "composition" };

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
        /// (optional) if it's not a composition, uri (local or distant) to use
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// (optional) if it's not a composition, settings used to connected to the uri (can be null/empty) => ffmpeg parameters",
        /// </summary>
        public Dictionary<string, string>? UriSettings { get; set; }

        /// <summary>
        /// True if the bot needs to stay connected on the Uri even if the media is not used in the conference
        /// </summary>
        public Boolean Connected { get; set; }

        /// <summary>
        /// (optional) if it's a video composition, list of streams id used to create it. Their order is important and related to the VideoFilter property
        /// </summary>
        public List<string>? VideoComposition { get; set; }

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
            VideoComposition = new();
            Uri = "";
            UriSettings = new();
            VideoFilter = "";
            ForceLiveStream = false;
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
                    UriSettings = jsonNode["uriSettings"],
                    Connected = jsonNode["connected"],
                    VideoFilter = jsonNode["videoFilter"],
                    ForceLiveStream = jsonNode["forceLiveStream"],
                };

                // Check validity
                if (String.IsNullOrEmpty(stream.Id))
                    return false;
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
    }
}
