using Common.Logging;
using Microsoft.Extensions.Logging;
using Rainbow.Example.CommonSDL2;
using Rainbow.WebRTC.Abstractions;
using Rainbow.WebRTC.Desktop;
using System.Collections.Concurrent;

namespace Rainbow.Example.CommonWebRTC
{
    public class MediaTrackManager
    {
        public delegate void MediaTrackDelegate(String streamId, int media);

        private ILogger? _log;
        private String? _loggerPrefix = null;

        private Object _lockCreateMediaTrack = new(); // To ensure to create MediaTrack one by one

        private readonly AudioStreamTrack _emptyAudioTrack; 

        private ConcurrentDictionary<String, IAudioStreamTrack> _audioStreamTracks = [];
        private ConcurrentDictionary<String, IVideoStreamTrack> _videoStreamTracks = [];

        private WebRTCFactory _rbWebRTCDesktopFactory;
        private StreamInputManager _streamInputManager;

#region Public API

        public static MediaTrackManager Instance { get { return Nested.instance; } }

        public AudioStreamTrack EmptyAudioTrack => _emptyAudioTrack;

        public WebRTCFactory WebRTCFactory => _rbWebRTCDesktopFactory;

        public IAudioStreamTrack? GetAudioStreamTrack(String id)
        {
            IAudioStreamTrack? result = null;
            if (!String.IsNullOrEmpty(id))
                _audioStreamTracks.TryGetValue(id, out result);
            return result;
        }

        public IVideoStreamTrack? GetVideoStreamTrack(String id)
        {
            IVideoStreamTrack? result = null;
            if (!String.IsNullOrEmpty(id))
                _videoStreamTracks.TryGetValue(id, out result);
            return result;
        }

        public void SetLoggerFactory(ILoggerFactory loggerFactory)
        {
            _rbWebRTCDesktopFactory.SetLoggerFactory(loggerFactory);
        }

        public void SetLoggerPrefix(String loggerPrefix)
        {
            if ( (this._loggerPrefix is null) && (!String.IsNullOrEmpty(loggerPrefix)))
            {
                this._loggerPrefix = loggerPrefix;
                _log = Rainbow.LogFactory.CreateLogger<StreamInputManager>(loggerPrefix); // We use same logguer thant StreamInputManager

                _rbWebRTCDesktopFactory.SetLoggerPrefix(loggerPrefix);
                _streamInputManager.SetLoggerPrefix(loggerPrefix);
            }
        }

#endregion Public API

#region Private API

        private IAudioStreamTrack? CreateAudioStreamTrack(string id)
        {
            lock (_lockCreateMediaTrack)
            {
                if (String.IsNullOrEmpty(id)) return null;

                var result = GetAudioStreamTrack(id);
                if (result is not null) return result;

                var stream = _streamInputManager.GetMediaAudioFromStreamId(id);
                if (stream is not null)
                {
                    try
                    {
                        result = _rbWebRTCDesktopFactory.CreateAudioTrack(stream);
                        if (result is not null)
                            _audioStreamTracks[id] = result;
                        else
                            ConsoleAbstraction.WriteRed($"[MediaTrackManager] Cannot create Audio Track using Id:[{id}]", logger: _log);
                    }
                    catch (Exception ex)
                    {
                        ConsoleAbstraction.WriteRed($"[MediaTrackManager] Cannot create Audio Track using Id:[{id}] - Exception:[{ex}]", logger: _log);
                    }
                }
                else
                    ConsoleAbstraction.WriteRed($"[MediaTrackManager] Cannot get Audio Stream using Id:[{id}]", logger: _log);
                return result;
            }
        }

        private IVideoStreamTrack? CreateVideoStreamTrack(string id)
        {
            lock (_lockCreateMediaTrack)
            {
                if (String.IsNullOrEmpty(id)) return null;

                var result = GetVideoStreamTrack(id);
                if (result is not null) return result;

                var stream = _streamInputManager.GetMediaVideoFromStreamId(id);
                if (stream is not null)
                {
                    try
                    { 
                        result = _rbWebRTCDesktopFactory.CreateVideoTrack(stream);
                        if (result is not null)
                            _videoStreamTracks[id] = result;
                        else
                            ConsoleAbstraction.WriteRed($"[MediaTrackManager] Cannot create Video Tack using Id:[{id}]", logger: _log);
                    }
                    catch (Exception ex)
                    {
                        ConsoleAbstraction.WriteRed($"[MediaTrackManager] Cannot create VideoTrack using Id:[{id}] - Exception:[{ex}]", logger: _log);
                    }
                }
                else
                    ConsoleAbstraction.WriteRed($"[MediaTrackManager] Cannot get Video Stream using Id:[{id}]", logger: _log);
                return result;
            }
        }

        private void RemoveAudioStreamTrack(string id)
        {
            if (_audioStreamTracks.TryRemove(id, out var result) && result is not null)
            {
                result.Dispose();
            }
        }

        private void RemoveVideoStreamTrack(string id)
        {
            if (_videoStreamTracks.TryRemove(id, out var result) && result is not null)
            {
                result.Dispose();
            }
        }

        private MediaTrackManager()
        {
            _rbWebRTCDesktopFactory = new();
            _emptyAudioTrack = (AudioStreamTrack)_rbWebRTCDesktopFactory.CreateEmptyAudioTrack();

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

#endregion Private API

#region StreamManager events

        private void StreamManager_OnStreamRemoved(string streamId, int media, Boolean stillUsed)
        {
            ConsoleAbstraction.WriteGray($"[MediaTrackManage] OnStreamRemoved - Stream:[{streamId}] - Media[{Rainbow.Util.MediasToString(media)}] - StillUsed:[{stillUsed}]", logger: _log);

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
            ConsoleAbstraction.WriteGray($"[MediaTrackManage] OnStreamAvailable - Stream:[{streamId}] - Media[{Rainbow.Util.MediasToString(media)}] - StillUsed:[{stillUsed}]", logger: _log);

            switch (media)
            {
                case Rainbow.Consts.Media.AUDIO:
                    CreateAudioStreamTrack(streamId); // Will do nothing if audio track already exist
                    break;


                case Rainbow.Consts.Media.VIDEO:
                    CreateVideoStreamTrack(streamId); // Will do nothing if audio track already exist
                    break;

            }
        }

        private void StreamManager_OnStreamOpened(string streamId, int media, Boolean stillUsed)
        {
            ConsoleAbstraction.WriteGray($"[MediaTrackManage] OnStreamOpened - Stream:[{streamId}] - Media[{Rainbow.Util.MediasToString(media)}] - StillUsed:[{stillUsed}]", logger: _log);

            switch (media)
            {
                case Rainbow.Consts.Media.AUDIO:
                    CreateAudioStreamTrack(streamId);
                    break;


                case Rainbow.Consts.Media.VIDEO:
                    CreateVideoStreamTrack(streamId);
                    break;

            }
        }

        private void StreamManager_OnStreamDisposing(string streamId)
        {
            RemoveAudioStreamTrack(streamId);
            RemoveVideoStreamTrack(streamId);
        }

#endregion StreamManager events


    }
}
