using Microsoft.Extensions.Logging;
using Rainbow.Example.Common;
using Rainbow.Medias;
using Rainbow.WebRTC.Abstractions;
using Rainbow.WebRTC.Desktop;
using System.Collections.Concurrent;
using System.Drawing;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using Stream = Rainbow.Example.Common.Stream;

namespace Rainbow.Example.CommonSDL2
{
    public class MediaTrackManager
    {
        public delegate void MediaTrackDelegate(String streamId, int media);

        private ILogger? log;
        private String? loggerPrefix = null;

        private ConcurrentDictionary<String, IAudioStreamTrack> audioStreamTracks = [];
        private ConcurrentDictionary<String, IVideoStreamTrack> videoStreamTracks = [];

        private WebRTCFactory _rbWebRTCDesktopFactory;
        private StreamInputManager _streamInputManager;

        public static MediaTrackManager Instance { get { return Nested.instance; } }

        public IAudioStreamTrack? GetAudioStreamTrack(String id)
        {
            IAudioStreamTrack? result = null;
            if (!String.IsNullOrEmpty(id))
                audioStreamTracks.TryGetValue(id, out result);
            return result;
        }

        public IVideoStreamTrack? GetVideoStreamTrack(String id)
        {
            IVideoStreamTrack? result = null;
            if (!String.IsNullOrEmpty(id))
                videoStreamTracks.TryGetValue(id, out result);
            return result;
        }

        private IAudioStreamTrack? CreateAudioStreamTrack(string id)
        {
            var stream = _streamInputManager.GetMediaAudioFromStreamId(id);
            if (stream is null)
                return _rbWebRTCDesktopFactory.CreateAudioTrack(stream);
            return null;
        }

        private IVideoStreamTrack? CreateVideoStreamTrack(string id)
        {
            var stream = _streamInputManager.GetMediaVideoFromStreamId(id);
            if (stream is null)
                return _rbWebRTCDesktopFactory.CreateVideoTrack(stream);
            return null;
        }

        private void RemoveAudioStreamTrack(string id)
        {
            if (audioStreamTracks.TryRemove(id, out var result) && result is not null)
            {
                result.Dispose();
            }
        }

        private void RemoveVideoStreamTrack(string id)
        {
            if (videoStreamTracks.TryRemove(id, out var result) && result is not null)
            {
                result.Dispose();
            }
        }

        public void SetLoggerFactory(ILoggerFactory loggerFactory)
        {
            _rbWebRTCDesktopFactory.SetLoggerFactory(loggerFactory);
        }

        public void SetLoggerPrefix(String loggerPrefix)
        {
            if ( (this.loggerPrefix is null) && (!String.IsNullOrEmpty(loggerPrefix)))
            {
                this.loggerPrefix = loggerPrefix;
                log = Rainbow.LogFactory.CreateLogger<StreamManager>(loggerPrefix);

                _rbWebRTCDesktopFactory.SetLoggerPrefix(loggerPrefix);
                _streamInputManager.SetLoggerPrefix(loggerPrefix);
            }
        }

#region StreamManager events

        private void StreamManager_OnStreamRemoved(string streamId, int media, Boolean stillUsed)
        {
            ConsoleAbstraction.WriteGray($"[ConsoleMediaPlayer] OnStreamRemoved - Stream:[{streamId}] - Media[{Rainbow.Util.MediasToString(media)}] - StillUsed:[{stillUsed}]");

            switch (media)
            {
                case Rainbow.Consts.Media.AUDIO:
                    if (!stillUsed)
                        RemoveAudioStreamTrack(streamId);

                    break;

                case Rainbow.Consts.Media.VIDEO:
                    if (!stillUsed)
                        RemoveVideoStreamTrack(streamId);
                    break;
            }
        }

        private void StreamManager_OnStreamAvailable(string streamId, int media, bool stillUsed)
        {
            ConsoleAbstraction.WriteGray($"[ConsoleMediaPlayer] OnStreamAvailable - Stream:[{streamId}] - Media[{Rainbow.Util.MediasToString(media)}] - StillUsed:[{stillUsed}]");

            switch (media)
            {
                case Rainbow.Consts.Media.AUDIO:
                    // TODO
                    break;


                case Rainbow.Consts.Media.VIDEO:
                    // TODO
                    break;

            }
        }

        private void StreamManager_OnStreamOpened(string streamId, int media, Boolean stillUsed)
        {
            ConsoleAbstraction.WriteGray($"[ConsoleMediaPlayer] OnStreamOpened - Stream:[{streamId}] - Media[{Rainbow.Util.MediasToString(media)}] - StillUsed:[{stillUsed}]");

            switch (media)
            {
                case Rainbow.Consts.Media.AUDIO:
                    var resultAudio = CreateAudioStreamTrack(streamId);
                    if (resultAudio is not null)
                    {
                        //TODO
                    }
                    break;


                case Rainbow.Consts.Media.VIDEO:
                    var resultVideo = CreateVideoStreamTrack(streamId);
                    if (resultVideo is not null)
                    {
                        //TODO
                    }
                    break;

            }
        }

        private void StreamManager_OnStreamDisposing(string streamId)
        {
            ConsoleAbstraction.WriteRed($"[ConsoleMediaPlayer] OnStreamDisposing - Stream:[{streamId}]");
        }

#endregion StreamManager events

        private MediaTrackManager()
        {
            _rbWebRTCDesktopFactory = new();

            _streamInputManager = StreamInputManager.Instance;
            _streamInputManager.OnStreamOpened      += StreamManager_OnStreamOpened;
            _streamInputManager.OnStreamRemoved     += StreamManager_OnStreamRemoved;
            _streamInputManager.OnStreamAvailable   += StreamManager_OnStreamAvailable;
            _streamInputManager.OnStreamDisposing   += StreamManager_OnStreamDisposing;
        }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly MediaTrackManager instance = new();
        }
    }
}
