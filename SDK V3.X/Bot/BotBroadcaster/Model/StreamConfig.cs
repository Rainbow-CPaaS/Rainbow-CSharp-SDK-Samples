using System;
using System.Collections.Generic;
using System.Text;

namespace BotBroadcaster.Model
{
    public class StreamConfig
    {
        /// <summary>
        /// Audio Stream Id
        /// </summary>
        public string AudioStreamId { get; set; } = "";

        /// <summary>
        /// Video Stream Id
        /// </summary>
        public string VideoStreamId { get; set; } = "";

        /// <summary>
        /// Sharing Stream Id
        /// </summary>
        public string SharingStreamId { get; set; } = "";
    }
}
