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

        public static Boolean FromJsonNode(JSONNode jsonNode, out Conference? conference)
        {
            if (jsonNode is not null)
            {
                conference = new()
                {
                    Id = jsonNode["id"],
                    Jid = jsonNode["jid"],
                    Name = jsonNode["name"],
                    AudioStreamId = jsonNode["audioStreamId"],
                    VideoStreamId = jsonNode["videoStreamId"],
                    SharingStreamId = jsonNode["sharingStreamId"],
                };

                // Check validity
                if (String.IsNullOrEmpty(conference.Id)
                    && String.IsNullOrEmpty(conference.Jid)
                    && String.IsNullOrEmpty(conference.Name))
                    return false;

                return true;
            }
            else
                conference = null;

            return false;
        }
    }
}
