using Rainbow.WebRTC.Desktop;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotBroadcaster.Model
{
    internal class ConferenceStatus
    {
        public String? ConferenceId { get; set; } = null;             // Current conference Id - to know which conference we are managing - delay between _currentCall.Id and _currentConferenceId in order to manage streams
        
        public Dictionary<int, String> Streams { get; set; } = [];    // Media as key, StreamId as Value

        // Audio / Video / Sharing Stream track currently used
        public AudioStreamTrack? AudioStreamTrack { get; set; } = null;
        public VideoStreamTrack? VideoStreamTrack { get; set; } = null;
        public VideoStreamTrack? SharingStreamTrack { get; set; } = null;

        public void Reset()
        {
            ConferenceId = null;

            Streams = [];

            AudioStreamTrack?.Dispose();
            AudioStreamTrack = null;

            VideoStreamTrack?.Dispose();
            VideoStreamTrack = null;

            SharingStreamTrack?.Dispose();
            SharingStreamTrack = null;
        }

    }
}
