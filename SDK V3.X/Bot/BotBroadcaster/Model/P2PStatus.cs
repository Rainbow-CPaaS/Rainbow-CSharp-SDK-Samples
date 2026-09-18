using Org.BouncyCastle.Asn1.Ocsp;
using Rainbow;
using Rainbow.Model;
using Rainbow.SimpleJSON;
using Rainbow.WebRTC.Desktop;
using System;
using System.Collections.Generic;

namespace BotBroadcaster.Model
{
    internal class P2PStatus
    {
        public String? CallId { get; set; } = null;

        public Contact? Remote { get; set; } = null; // Remote as <see cref="Contact"/> in the P2P call

        public Dictionary<int, String> Streams { get; set; } = []; // Media as key, StreamId as Value

        // Audio / Video / Sharing Stream track currently used
        public AudioStreamTrack? AudioStreamTrack { get; set; } = null;
        public VideoStreamTrack? VideoStreamTrack { get; set; } = null;
        public VideoStreamTrack? SharingStreamTrack { get; set; } = null;

        public void Reset()
        {
            CallId = null;

            Remote = null;

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
