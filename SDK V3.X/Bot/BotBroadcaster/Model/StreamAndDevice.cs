using Rainbow.Example.Common;
using Rainbow.Medias;
using Rainbow.WebRTC.Abstractions;
using System;

namespace BotBroadcaster.Model
{
    internal class StreamAndDevice
    {
        public Stream? Stream { get; set;}

        internal Device? Device { get; set; } // The device created using the Stream object

        internal IMediaStreamTrack? AudioTrack { get; set; } // Audio Track created using the device

        internal IMediaStreamTrack? VideoTrack { get; set; } // Video Track created using the device

        internal Boolean UsedInConference{ get; set; } // To know if this track is currently used in the conference

        internal Boolean MustBeRemovedFromConference { get; set; } // To know if this track must be removed from conference

        internal Boolean MustBeDeleted { get; set; } // To know if object must be deleted

        public StreamAndDevice(Stream? stream, Device? device)
        {
            Stream = stream;
            Device = device;

            AudioTrack = null;
            VideoTrack = null;
            UsedInConference = false;
            MustBeRemovedFromConference = false;
            MustBeDeleted = false;
        }
    }
}
