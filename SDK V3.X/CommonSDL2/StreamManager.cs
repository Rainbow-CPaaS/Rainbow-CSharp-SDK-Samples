using Rainbow.Example.Common;
using Rainbow.Medias;
using System.Collections.Concurrent;
using System.Drawing;
using Stream = Rainbow.Example.Common.Stream;

namespace Rainbow.Example.CommonSDL2
{
    public class StreamManager
    {
        public delegate void StreamDelegate(String streamId, int media, Boolean stillUsed);

        // --- To store list of Streams as current configuration and used in current conference
        private ConcurrentDictionary<String, Stream> _streamsList = [];  // Stream Id as key
        private Dictionary<int, String> _streamsIdToUse = []; // Media (audio/video/sharing) as key and Stream Id as value
        private Boolean _newConfigurationReceived;

        private Object lockNewConfiguration = new ();
        private Object lockStartToOpenOrCloseStreams = new ();

        private List<String> _streamsToOpen = [];
        private List<String> _streamsToClose = [];

        private Task TaskOpenOrCloseStreams = Task.CompletedTask;

        private Boolean _disposeStreams = false;

        // --- IMedia used / created (i.e. we are connected to them)
        private readonly ConcurrentDictionary<String, IMediaAudio> _mediasForAudio = []; // Stream Id as key
        private readonly ConcurrentDictionary<String, IMediaVideo> _mediasForVideo = []; // Stream Id as key

        public String? _streamIdUsedForAudio;      // To know which stream is used for Audio currently used/played
        public String? _streamIdUsedForVideo;      // To know which stream is used for Video currently used/played
        public String? _streamIdUsedForSharing;    // To know which stream is used for Sharing currently used/played

        public event StreamDelegate? OnStreamOpened;
        public event StreamDelegate? OnStreamRemoved; // The stream id specified for the media specified is no more used.
        public event Rainbow.Delegates.IdDelegate? OnStreamDisposing; // The stream will be disposing (according _disposeStreams value)

        public StreamManager(Boolean disposeStreams)
        {
            _disposeStreams = disposeStreams;
        }

        public int GetMediaUsedFromStreamId(String streamID)
        {
            if (streamID == _streamIdUsedForAudio) return Rainbow.Consts.Media.AUDIO;
            if (streamID == _streamIdUsedForVideo) return Rainbow.Consts.Media.VIDEO;
            if (streamID == _streamIdUsedForSharing) return Rainbow.Consts.Media.SHARING;
            return Rainbow.Consts.Media.NONE;
        }

        public IMediaAudio? GetMediaAudioFromStreamId(String streamID)
        {
            _mediasForAudio.TryGetValue(streamID, out IMediaAudio? mediaAudio);
            return mediaAudio;
        }

        public IMediaVideo? GetMediaVideoFromStreamId(String streamID)
        {
            _mediasForVideo.TryGetValue(streamID, out IMediaVideo? mediaVideo);
            return mediaVideo;
        }

        public void SetNewConfiguration(List<Stream>? streams, Dictionary<int, String>? streamsToUse)
        {
            lock (lockNewConfiguration)
            {
                ConsoleAbstraction.WriteBlue($"{Rainbow.Util.CR}[StreamManager] List of streams correctly set in config file: (doesn't mean they can be really used)");
                if (streams is null)
                    ConsoleAbstraction.WriteBlue($"\tNo stream");
                else
                {
                    int index = 0;
                    foreach (var stream in streams)
                        ConsoleAbstraction.WriteBlue($"\t{++index:00} - {stream} - Connected:[{stream.Connected}]");
                }

                List<String> streamsToBeOpened = [];

                // Reset current information about streams to open and close (we will fill them again with new configuration)
                _streamsToOpen.Clear();
                _streamsToClose.Clear();

                
                streams ??= [];
                streamsToUse ??= [];

                // Check Streams which must stay opened
                streamsToBeOpened = GetStreamsToOpen(streams) ?? [];
                var currentStreamsOpened = GetCurrentStreamsOpened();
                if (!CheckIfListsHaveSameContent(streamsToBeOpened, currentStreamsOpened))
                    _newConfigurationReceived = true;

                // Check AUDIO media set as used/played
                {
                    var audioStreamId = _streamsIdToUse.FirstOrDefault(x => x.Key == Consts.Media.AUDIO).Value;
                    var newAudioStreamId = streamsToUse.FirstOrDefault(x => x.Key == Consts.Media.AUDIO).Value;
                    if (!string.Equals(audioStreamId, newAudioStreamId, StringComparison.OrdinalIgnoreCase))
                    {
                        _newConfigurationReceived = true;
                        
                        // Add new one to the list of stream to open
                        if (!streamsToBeOpened.Contains(newAudioStreamId))
                            streamsToBeOpened.Add(newAudioStreamId);
                        
                        // Close the previous one (if any)
                        CheckIfStreamUsedAndTriggerCloseEvent(audioStreamId, Rainbow.Consts.Media.AUDIO);
                    }
                }

                // Check VIDEO media set as used/played
                {
                    var videoStreamId = _streamsIdToUse.FirstOrDefault(x => x.Key == Consts.Media.VIDEO).Value;
                    var newVideoStreamId = streamsToUse.FirstOrDefault(x => x.Key == Consts.Media.VIDEO).Value;
                    if (!string.Equals(videoStreamId, newVideoStreamId, StringComparison.OrdinalIgnoreCase))
                    {
                        _newConfigurationReceived = true;

                        // Add new one to the list of stream to open
                        if (!streamsToBeOpened.Contains(newVideoStreamId))
                            streamsToBeOpened.Add(newVideoStreamId);

                        // Close the previous one (if any)
                        CheckIfStreamUsedAndTriggerCloseEvent(videoStreamId, Rainbow.Consts.Media.VIDEO);
                    }
                }

                // Check SHARING media set as used/played
                {
                    var sharingStreamId = _streamsIdToUse.FirstOrDefault(x => x.Key == Consts.Media.SHARING).Value;
                    var newSharingStreamId = streamsToUse.FirstOrDefault(x => x.Key == Consts.Media.SHARING).Value;
                    if (!string.Equals(sharingStreamId, newSharingStreamId, StringComparison.OrdinalIgnoreCase))
                    {
                        _newConfigurationReceived = true;

                        /// Add new one to the list of stream to open
                        if (!streamsToBeOpened.Contains(newSharingStreamId))
                        streamsToBeOpened.Add(newSharingStreamId);

                        // Close the previous one (if any)
                        CheckIfStreamUsedAndTriggerCloseEvent(sharingStreamId, Rainbow.Consts.Media.SHARING);
                    }
                }

                // Store new configuration of streams
                _streamsList = new ConcurrentDictionary<string, Stream>(streams.ToDictionary(s => s.Id, s => s));

                /// Store new configuration of streams to use/play
                _streamsIdToUse = streamsToUse;

                var newStreamsToOpen = GetStreamsToOpen(streamsToUse.Values.ToList(), _streamsList);
                // Update list of streams to be opened - Merge two lists
                streamsToBeOpened = (streamsToBeOpened ?? []).Union(newStreamsToOpen).ToList();

                List<string> listStreamsAlreadyOpened = []; // Used only to log info
                // First get list of stream to open
                foreach (var streamId in streamsToBeOpened)
                {
                    if (String.IsNullOrEmpty(streamId)) continue;

                    // if not already used as audio or video stream add it to the list of stream to open
                    if ((!_mediasForAudio.ContainsKey(streamId)) && (!_mediasForVideo.ContainsKey(streamId)))
                    {
                        if (!_streamsToOpen.Contains(streamId))
                            _streamsToOpen.Add(streamId);
                    }
                    else
                    {
                        if ( ! ((_mediasForAudio.ContainsKey(streamId) || _mediasForVideo.ContainsKey(streamId))) )
                            if (!listStreamsAlreadyOpened.Contains(streamId))
                                listStreamsAlreadyOpened.Add(streamId);
                    }
                }

                // Now check audio media to close
                foreach (var streamId in _mediasForAudio.Keys)
                {
                    if (String.IsNullOrEmpty(streamId)) continue;

                    if ( (!streamsToBeOpened.Contains(streamId)) && !_streamsToClose.Contains(streamId))
                        _streamsToClose.Add(streamId);
                }

                // Now check video media to close
                foreach (var streamId in _mediasForVideo.Keys)
                {
                    if (String.IsNullOrEmpty(streamId)) continue;

                    if ( (!streamsToBeOpened.Contains(streamId)) && !_streamsToClose.Contains(streamId))
                        _streamsToClose.Add(streamId);
                }

                _newConfigurationReceived = _newConfigurationReceived || (_streamsToClose.Count > 0) || (_streamsToOpen.Count > 0);
                if (_newConfigurationReceived)
                {
                    ConsoleAbstraction.WriteGreen("[StreamManager] New Configuration Set:");
                    ConsoleAbstraction.WriteGreen($"\tStream(s) already OPENED: [{String.Join(", ", listStreamsAlreadyOpened)}]");
                    ConsoleAbstraction.WriteGreen($"\tStream(s) which need to be OPENED: [{String.Join(", ", _streamsToOpen)}]");
                    ConsoleAbstraction.WriteGreen($"\tStream(s) which need to be CLOSED: [{String.Join(", ", _streamsToClose)}]");
                    var audioStreamId = _streamsIdToUse.FirstOrDefault(x => x.Key == Consts.Media.AUDIO).Value;
                    var videoStreamId = _streamsIdToUse.FirstOrDefault(x => x.Key == Consts.Media.VIDEO).Value;
                    var sharingStreamId = _streamsIdToUse.FirstOrDefault(x => x.Key == Consts.Media.SHARING).Value;
                    ConsoleAbstraction.WriteGreen($"\tStream which must be used for AUDIO:   [{((audioStreamId is not null) ? audioStreamId : "NONE")}]");
                    ConsoleAbstraction.WriteGreen($"\tStream which must be used for VIDEO:   [{((videoStreamId is not null) ? videoStreamId : "NONE")}]");
                    ConsoleAbstraction.WriteGreen($"\tStream which must be used for SHARING: [{((sharingStreamId is not null) ? sharingStreamId : "NONE")}]");
                }

                ConsoleAbstraction.WriteGreen($"\tMediasForAudio: [{String.Join(", ", _mediasForAudio.Keys.ToList())}]");
                ConsoleAbstraction.WriteGreen($"\tMediasForVideo: [{String.Join(", ", _mediasForVideo.Keys.ToList())}]");

                StartToOpenOrCloseStreams();
            }
        }

        /// <summary>
        /// To update / remove ONLY one media stream. Strema
        /// </summary>
        /// <param name="media"></param>
        /// <param name="streamId"></param>
        public void UpdateOneStreamToUse(int media, String? streamId)
        {
            Dictionary<int, String>? streamsToUse = [];
            lock (lockNewConfiguration)
            {
                if (!String.IsNullOrEmpty(_streamIdUsedForAudio))
                    streamsToUse[Rainbow.Consts.Media.AUDIO] = _streamIdUsedForAudio;
                if (!String.IsNullOrEmpty(_streamIdUsedForVideo))
                    streamsToUse[Rainbow.Consts.Media.VIDEO] = _streamIdUsedForVideo;
                if (!String.IsNullOrEmpty(_streamIdUsedForSharing))
                    streamsToUse[Rainbow.Consts.Media.SHARING] = _streamIdUsedForSharing;

                if (String.IsNullOrEmpty(streamId))
                    streamsToUse.Remove(media);
                else
                    streamsToUse[media] = streamId;


                if (streamsToUse.Count == 0)
                    streamsToUse = null;
            }
            SetNewConfiguration(_streamsList.Values.ToList(), streamsToUse);
        }

        internal void StartToOpenOrCloseStreams()
        {
            if (!TaskOpenOrCloseStreams.IsCompleted)
            {
                ConsoleAbstraction.WriteYellow("[StreamManager] Task to open or close streams is already running => we will not start another one to avoid any conflict - New configuration will be taken into account at the end of current task");
                return;
            }
            TaskOpenOrCloseStreams = Task.Run(OpenOrCloseStreams);
        }

        internal void OpenOrCloseStreams()
        {
            // Algorithm:
            // We must first know:
            // - which streams must be opened (audio and/or video, compositions)
            // - which streams must be closed (audio and/or video, compositions)
            // - which streams are used/played (audio, video and/or sharing)
            //
            // 0] Check if Audio/Video/Sharing streams used/played are no more the same to remove them for. 
            //
            // A] Loop on audio and/or video streams to open (not composition). (it includes video which will be used for composition)
            //      A.1] Starts / Open it and update cache
            //      A.2] Check if new config and abort this task if it's the case
            //
            // B] Loop on composition streams to open .
            //      B.1] Starts / Open it and update cache
            //      B.2] Check if new config and abort this task if it's the case
            //
            // C] Loop on composition streams to close
            //      C.1] If used/played trigger event to inform to remove it
            //      C.2] Close it and update cache
            //      C.3] Check if new config and abort this task if it's the case
            //
            // D] Loop on audio and/or video streams to close
            //      D.1] If used/played trigger event to inform to remove it
            //      D.2] Close it and update cache
            //      D.3] Check if new config and abort this task if it's the case
            //
            // E] Inform to add streams used/played
            //      E.1] If used/played trigger event to inform to add audio
            //      E.2] Check if new config and abort this task if it's the case
            //      E.3] If used/played trigger event to inform to add video
            //      E.4] Check if new config and abort this task if it's the case
            //      E.5] If used/played trigger event to inform to add sharing

            lock (lockStartToOpenOrCloseStreams)
            {
                // There is no new configuration - we can quit
                if (!_newConfigurationReceived) return;

                // Set new configuration as false
                _newConfigurationReceived = false;

                // We create a copy of list of stream to open/cloe to avoid any issue if this list is updated while we are opening streams
                var streamsToOpen = _streamsToOpen.ToList();
                var streamsToClose = _streamsToClose.ToList();

                // A] Loop on audio and/or video streams to open (not composition). (it includes video which will be used for composition)
                ConsoleAbstraction.WriteGreen("[StreamManager] A] Loop on audio and/or video streams to open (not composition)");
                foreach (var streamId in streamsToOpen)
                {
                    if (_streamsList is not null && _streamsList.TryGetValue(streamId, out Stream? stream)
                            && stream is not null)
                    {
                        // Don't manage here composition
                        if (stream.VideoComposition is null)
                        {
                            // A.1] Starts / Open it and update cache
                            ConsoleAbstraction.WriteGreen($"[StreamManager] A.1] Starts / Open it and update cache - Stream:[{stream.Id}]");
                            OpenAudioOrVideoStream(stream);
                        }
                        else
                        {
                            // Nothing special here - we don't manage composition for the moment
                        }
                    }
                    else
                    {
                        // Cannot find this stream in list ...
                    }

                    // A.2] Check if new config and abort this task if it's the case
                    if (_newConfigurationReceived)
                    {
                        ConsoleAbstraction.WriteGreen($"[StreamManager] A.2] Check if new config => Yes abort");
                        CancelableDelay.StartAfter(500, StartToOpenOrCloseStreams);
                        return;
                    }
                }

                // B] Loop on composition streams to open .
                ConsoleAbstraction.WriteGreen("[StreamManager] B] Loop on composition streams to open");
                foreach (var streamId in streamsToOpen)
                {
                    if (_streamsList is not null && _streamsList.TryGetValue(streamId, out Stream? stream)
                            && stream is not null)
                    {
                        // Manage here only composition
                        if (stream.VideoComposition is not null)
                        {
                            // B.1] Starts / Open it and update cache
                            ConsoleAbstraction.WriteGreen($"[StreamManager] B.1] Starts / Open it and update cache - Stream:[{stream.Id}]");
                            OpenCompositionStream(stream);
                        }
                        else
                        {
                            // Nothing special here - we manage here only composition
                        }
                    }
                    else
                    {
                        // Cannot find this stream in list ...
                    }

                    // B.2] Check if new config and abort this task if it's the case
                    if (_newConfigurationReceived)
                    {
                        ConsoleAbstraction.WriteGreen($"[StreamManager] B.2] Check if new config => Yes abort");
                        CancelableDelay.StartAfter(500, StartToOpenOrCloseStreams);
                        return;
                    }
                }


                // C] Loop on composition streams to close
                ConsoleAbstraction.WriteGreen("[StreamManager] C] Loop on composition streams to close");
                foreach (var streamId in streamsToClose)
                {
                    if (_streamsList is not null && _streamsList.TryGetValue(streamId, out Stream? stream)
                                            && stream is not null)
                    {
                        // Manage here only composition
                        if (stream.VideoComposition is not null)
                        {
                            // C.1] If used/played trigger event to inform to remove it
                            ConsoleAbstraction.WriteGreen($"[StreamManager] C.1] If used/played trigger event to inform to remove it - Stream:[{stream.Id}]");
                            CheckIfStreamUsedAndTriggerCloseEvent(stream.Id);

                            // C.2] Close it and update cache
                            ConsoleAbstraction.WriteGreen($"[StreamManager] C.2] Close it and update cache - Stream:[{stream.Id}]");
                            CloseStream(stream);
                        }
                    }
                    else
                    {
                        // Cannot find this stream in list ...
                    }

                    // C.3] Check if new config and abort this task if it's the case
                    if (_newConfigurationReceived)
                    {
                        ConsoleAbstraction.WriteGreen($"[StreamManager] C.3] Check if new config => Yes abort");
                        CancelableDelay.StartAfter(500, StartToOpenOrCloseStreams);
                        return;
                    }
                }

                // D] Loop on audio and/or video streams to close
                ConsoleAbstraction.WriteGreen("[StreamManager] D] Loop on audio and/or video streams to close");
                foreach (var streamId in streamsToClose)
                {
                    if (_streamsList is not null && _streamsList.TryGetValue(streamId, out Stream? stream)
                                            && stream is not null)
                    {
                        // Manage here NOT composition
                        if (stream.VideoComposition is null)
                        {
                            // D.1] If used/played trigger event to inform to remove it
                            ConsoleAbstraction.WriteGreen($"[StreamManager] D.1] If used/played trigger event to inform to remove it - Stream:[{stream.Id}]");
                            CheckIfStreamUsedAndTriggerCloseEvent(stream.Id);

                            // D.2] Close it and update cache
                            ConsoleAbstraction.WriteGreen($"[StreamManager] D.2] Close it and update cache - Stream:[{stream.Id}]");
                            CloseStream(stream);
                        }
                    }
                    else
                    {
                        // Cannot find this stream in list ...
                    }

                    // D.3] Check if new config and abort this task if it's the case
                    if (_newConfigurationReceived)
                    {
                        ConsoleAbstraction.WriteGreen($"[StreamManager] D.3] Check if new config => Yes abort");
                        CancelableDelay.StartAfter(500, StartToOpenOrCloseStreams);
                        return;
                    }
                }

                // E] Inform to add streams used/played
                ConsoleAbstraction.WriteGreen("[StreamManager] E] Inform to add streams used/played");
                Dictionary<int, String> streamsToUse = new(_streamsIdToUse);
                foreach (var kvp in streamsToUse)
                {
                    // E.1] If used/played trigger event to inform to add audio
                    if (kvp.Key == Consts.Media.AUDIO)
                    {
                        if (!String.Equals(_streamIdUsedForAudio, kvp.Value, StringComparison.OrdinalIgnoreCase))
                        {
                            if (_mediasForAudio.TryGetValue(kvp.Value, out IMediaAudio? mediaAudio) && (mediaAudio?.IsStarted == true))
                            {
                                try
                                {
                                    ConsoleAbstraction.WriteGreen($"[StreamManager] E.1] If used/played trigger event to inform to add audio - Stream:[{kvp.Value}]");

                                    OnStreamOpened?.Invoke(kvp.Value, kvp.Key, true);
                                }
                                finally
                                {
                                    _streamIdUsedForAudio = kvp.Value;
                                }
                            }
                            else
                            {

                            }
                        }
                        else
                        {

                        }
                    }

                    //  E.2] Check if new config and abort this task if it's the case
                    if (_newConfigurationReceived)
                    {
                        ConsoleAbstraction.WriteGreen($"[StreamManager] E.2] Check if new config => Yes abort");
                        CancelableDelay.StartAfter(500, StartToOpenOrCloseStreams);
                        return;
                    }

                    // E.3] If used/played trigger event to inform to add video
                    if (kvp.Key == Consts.Media.VIDEO)
                    {
                        if (!String.Equals(_streamIdUsedForVideo, kvp.Value, StringComparison.OrdinalIgnoreCase))
                        {
                            if (_mediasForVideo.TryGetValue(kvp.Value, out IMediaVideo? mediaVideo) && (mediaVideo?.IsStarted == true))
                            {
                                try
                                {
                                    ConsoleAbstraction.WriteGreen($"[StreamManager] E.3] If used/played trigger event to inform to add video - Stream:[{kvp.Value}]");

                                    OnStreamOpened?.Invoke(kvp.Value, kvp.Key, true);
                                }
                                finally
                                {
                                    _streamIdUsedForVideo = kvp.Value;
                                }
                            }
                        }
                    }

                    //  E.4] Check if new config and abort this task if it's the case
                    if (_newConfigurationReceived)
                    {
                        ConsoleAbstraction.WriteGreen($"[StreamManager] E.4] Check if new config => Yes abort");
                        CancelableDelay.StartAfter(500, StartToOpenOrCloseStreams);
                        return;
                    }

                    // E.5] If used/played trigger event to inform to add sharing
                    if (kvp.Key == Consts.Media.SHARING)
                    {
                        if (!String.Equals(_streamIdUsedForSharing, kvp.Value, StringComparison.OrdinalIgnoreCase))
                        {
                            if (_mediasForVideo.TryGetValue(kvp.Value, out IMediaVideo? mediaSharing) && (mediaSharing?.IsStarted == true))
                            {
                                try
                                {
                                    ConsoleAbstraction.WriteGreen($"[StreamManager] E.5] If used/played trigger event to inform to add sharing - Stream:[{kvp.Value}]");

                                    OnStreamOpened?.Invoke(kvp.Value, kvp.Key, true);
                                }
                                finally
                                {
                                    _streamIdUsedForSharing = kvp.Value;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CheckIfStreamUsedAndTriggerCloseEvent(String streamId, int media = Rainbow.Consts.Media.NONE)
        {
            if (String.IsNullOrEmpty(streamId)) return;

            Boolean stillUsed;
            if (_streamIdUsedForAudio == streamId && 
                ((media == Rainbow.Consts.Media.AUDIO) || (media == Rainbow.Consts.Media.NONE)))
            {
                try
                {
                    stillUsed = false;

                    ConsoleAbstraction.WriteDarkYellow($"[StreamManager] Stream:[{streamId}] is currently used for AUDIO - Trigger event to inform to remove it ...");
                    if (_mediasForAudio.TryGetValue(_streamIdUsedForAudio, out IMediaAudio? mediaAudio) && mediaAudio is not null)
                    {
                        stillUsed = true;

                        ConsoleAbstraction.WriteDarkYellow($"[StreamManager] Stream:[{streamId}] Remove Media Audio events ...");
                    }

                    if (_mediasForVideo.TryGetValue(_streamIdUsedForAudio, out var mediaVideo) && mediaVideo is not null)
                        stillUsed = true;

                    //Task.Delay(200).Wait();

                    OnStreamRemoved?.Invoke(_streamIdUsedForAudio, Consts.Media.AUDIO, stillUsed);
                }
                finally
                {
                    _streamIdUsedForAudio = null;
                }
            }
            if (_streamIdUsedForVideo == streamId &&
                ((media == Rainbow.Consts.Media.VIDEO) || (media == Rainbow.Consts.Media.NONE)))
            {
                try
                {
                    stillUsed = false;

                    ConsoleAbstraction.WriteDarkYellow($"[StreamManager] Stream:[{streamId}] is currently used for VIDEO - Trigger event to inform to remove it ...");
                    if (_mediasForVideo.TryGetValue(_streamIdUsedForVideo, out IMediaVideo? mediaVideo) && mediaVideo is not null)
                    {
                        stillUsed = true;

                        ConsoleAbstraction.WriteDarkYellow($"[StreamManager] Stream:[{streamId}] Remove Media Video events ...");
                    }

                    if (_mediasForAudio.TryGetValue(streamId, out var mediaAudio) && mediaAudio is not null)
                        stillUsed = true;

                    ///Task.Delay(200).Wait();

                    OnStreamRemoved?.Invoke(streamId, Consts.Media.VIDEO, stillUsed);
                }
                finally
                {
                    _streamIdUsedForVideo = null;
                }
            }
            if (_streamIdUsedForSharing == streamId &&
                ((media == Rainbow.Consts.Media.SHARING) || (media == Rainbow.Consts.Media.NONE)))
            {
                try
                {
                    stillUsed = false;

                    ConsoleAbstraction.WriteDarkYellow($"[StreamManager] Stream:[{streamId}] is currently used for SHARING - Trigger event to inform to remove it ...");
                    if (_mediasForVideo.TryGetValue(_streamIdUsedForSharing, out IMediaVideo? mediaSharing) && mediaSharing is not null)
                    {
                        stillUsed = true;

                        ConsoleAbstraction.WriteDarkYellow($"[StreamManager] Stream:[{streamId}] Remove Media Video (Sharing case) events ...");
                    }

                    if (_mediasForAudio.TryGetValue(streamId, out var mediaAudio) && mediaAudio is not null)
                        stillUsed = true;

                    //Task.Delay(200).Wait();

                    OnStreamRemoved?.Invoke(_streamIdUsedForSharing, Consts.Media.SHARING, stillUsed);
                }
                finally
                {
                    _streamIdUsedForSharing = null;
                }
            }
        }

        private void OpenCompositionStream(Stream stream)
        {
            // Here we are sure to have a stream with a composition (with several videos)

            IMediaVideo? videoInput = GetMediaInputForComposition(stream); // Here the composition is already started
            if(videoInput is not null)
            {
                // Store it as video
                _mediasForVideo[stream.Id] = videoInput;
            }
            else
            {
                // cannot create media input for this composition ...
            }
        }
        
        private void CloseStream(Stream stream)
        {
            if (_mediasForAudio.TryRemove(stream.Id, out IMediaAudio? mediaAudio) && mediaAudio?.IsStarted == true)
            {
                if (!_mediasForVideo.ContainsKey(stream.Id))
                {
                    ConsoleAbstraction.WriteDarkYellow($"[StreamManager] Dispose Media (Audio case) for Stream:[{stream.Id}] ...");

                    OnStreamDisposing?.Invoke(stream.Id);
                    if(_disposeStreams) mediaAudio?.Dispose();
                }
            }

            if (_mediasForVideo.TryRemove(stream.Id, out IMediaVideo? mediaVideo) && mediaVideo?.IsStarted == true)
            {
                if (!_mediasForAudio.ContainsKey(stream.Id))
                {
                    ConsoleAbstraction.WriteDarkYellow($"[StreamManager] Dispose Media (Video case) for Stream:[{stream.Id}] ...");

                    OnStreamDisposing?.Invoke(stream.Id);
                    if (_disposeStreams) mediaVideo?.Dispose();
                }
            }
        }

        private void OpenAudioOrVideoStream(Stream stream)
        {
            // Here we are sure to have a stream with audio and/or video and not a composition (with several videos)

            // Create media inputs for this stream
            (IMediaAudio? audioInput, IMediaVideo? videoInput) = GetMediaInputs(stream);

            // Init Media Input
            if (audioInput is not null)
            {
                // Start audio Input
                if (audioInput.IsStarted ||audioInput.Init(true))
                {
                    ConsoleAbstraction.WriteWhite($"[StreamManager] Stream init/started (Audio context) - Stream:[{stream.Id}]");
                    // Store it as audio
                    _mediasForAudio[stream.Id] = audioInput;

                    // Store it as video (if necessary)
                    if (videoInput is not null)
                    {
                        _mediasForVideo[stream.Id] = videoInput;
                        ConsoleAbstraction.WriteWhite($"[StreamManager] Stream init/started also as Video context - Stream:[{stream.Id}]");
                    }
                }
                else
                {
                    // Cannot Init Audio Input ...
                    ConsoleAbstraction.WriteRed($"[StreamManager] Cannot open AUDIO stream:[{stream.Id}]");
                }
            }
            else if (videoInput is not null)
            {
                // Start Video Input
                if (videoInput.IsStarted || videoInput.Init(true))
                {
                    ConsoleAbstraction.WriteWhite($"[StreamManager] Stream init/started (Video context) - Stream:[{stream.Id}]");

                    // Store it as video
                    _mediasForVideo[stream.Id] = videoInput;
                }
                else
                {
                    // Cannot Init Video Input ...
                    ConsoleAbstraction.WriteRed($"[StreamManager] Cannot open VIDEO stream:[{stream.Id}]");
                }
            }
            else
            {
                // cannot create media input for this stream ...
                ConsoleAbstraction.WriteRed($"[StreamManager] Cannot create media input for this stream:[{stream.Id}]");
            }
        }

        private MediaFiltered? GetMediaInputForComposition(Stream stream)
        {
            if(stream.VideoComposition is null) return null;

            Boolean success = true;
            List<Size> videoSize = [];

            List<IMediaVideo> mediaVideoList = [];

            ConsoleAbstraction.WriteGreen($"[StreamManager] Trying to create composition for Stream [{stream.Id}] ...");
            foreach (var id in stream.VideoComposition)
            {
                _streamsList.TryGetValue(id, out var s);
                if (s is not null)
                {
                    _mediasForVideo.TryGetValue(id, out IMediaVideo? iMediaVideo);
                    {
                        if (iMediaVideo is not null)
                        {
                            var size = new Size(iMediaVideo.Width, iMediaVideo.Height);
                            videoSize.Add(size);

                            mediaVideoList.Add(iMediaVideo);
                        }
                        else
                        {
                            // Cannot start 
                            ConsoleAbstraction.WriteRed($"[StreamManager] For composition, MediaInput cannot be created for Stream:[{id}]");
                            success = false;
                            break;
                        }
                    }
                }
                else
                {
                    ConsoleAbstraction.WriteRed($"[StreamManager] For composition, no Stream:[{id}] defined ...");
                    success = false;
                    break;
                }
            }

            if (success)
            {
                String? filter = null;
                var mediaFiltered = new MediaFiltered(stream.Id, mediaVideoList, null);

                // Check if we have a filter already defined or we need to create it using a "template" ?
                if (stream.VideoFilterJsonNode is not null)
                {
                    // We need to get size of media inputs used.
                    String type = stream.VideoFilterJsonNode["type"];
                    var node = stream.VideoFilterJsonNode["data"];

                    if (node is not null)
                    {
                        Size? size;
                        int fps;

                        switch (type)
                        {
                            case "mosaic":
                                // Get fps
                                fps = node["fps"];
                                if (fps == 0)
                                    fps = 10;

                                // Get size of vignette
                                size = VideoFilter.SizeFromString(node["sizeVignette"]);
                                size ??= videoSize[0];

                                // Get layout
                                String layout = node["layout"];
                                if (String.IsNullOrEmpty(layout))
                                    layout = "G";

                                filter = VideoFilter.FilterToMosaic(
                                    srcVideoSize: videoSize,
                                    vignette: size.Value,
                                    layout: layout,
                                    fps: fps
                                    );

                                break;

                            case "overlay":

                                // Get size
                                size = VideoFilter.SizeFromString(node["size"]);
                                size ??= videoSize[0];

                                // Get fps
                                fps = node["fps"];
                                if (fps == 0)
                                    fps = 10;

                                // Get overlay size
                                var sizeOverlay = VideoFilter.SizeFromString(node["sizeOverlay"]);
                                sizeOverlay ??= new Size((int)(videoSize[0].Width / 4), (int)(videoSize[0].Height / 4));

                                // Get overlay position
                                String positionOverLay = node["positionOverlay"];
                                if (String.IsNullOrEmpty(positionOverLay))
                                    positionOverLay = "TR";

                                filter = VideoFilter.FilterToOverlay(
                                    srcMain: videoSize[0],
                                    srcOverlay: videoSize[1],
                                    dst: size.Value,
                                    dstOverlay: sizeOverlay.Value,
                                    dstOverlayPosition: positionOverLay,
                                    fps: fps
                                    );
                                break;
                        }
                    }
                    if (filter is null)
                        ConsoleAbstraction.WriteRed($"[StreamManager] For composition, using \"template\" cannot create video filter ...");
                    else
                        ConsoleAbstraction.WriteDarkYellow($"[StreamManager] For composition, using \"template\" filter created:\r\n{filter}");
                }
                else
                    filter = stream.VideoFilter;

                if (!String.IsNullOrEmpty(filter))
                {
                    if (mediaFiltered.SetVideoFilter(stream.VideoComposition, filter))
                    {
                        if (mediaFiltered.Init(true))
                        {
                            // Store filter used
                            stream.VideoFilterInConference = filter;
                            return mediaFiltered;
                        }
                        else
                            ConsoleAbstraction.WriteRed($"[StreamManager] Composition, Cannot start - Stream:[{stream.Id}]");
                    }
                    else
                        ConsoleAbstraction.WriteRed($"[StreamManager] Composition, filter is incorrect - Stream:[{stream.Id}]:\r\n{stream.VideoFilter}");
                }
            }
            return null;
        }

        static private (IMediaAudio? audioInput, IMediaVideo? videoInput) GetMediaInputs(Stream? stream)
        {
            IMediaAudio? audioInput = null;
            IMediaVideo? videoInput = null;
            if (stream is null) return (audioInput, videoInput);

            Boolean withAudio = stream.Media.Contains("audio");
            Boolean withVideo = stream.Media.Contains("video");

            var options = stream.UriSettings;

            List<String> settings = [];
            if (options is not null)
            {
                foreach (var key in options.Keys)
                    settings.Add(key + ":" + options[key]);
            }

            switch (stream.UriType)
            {
                case "webcam":
                    var webcamDevice = GetWebcamDevice(stream.Uri);
                    if (webcamDevice is not null)
                    {
                        videoInput = MediaInput.FromWebcamDevice(webcamDevice, false);
                        if (videoInput is null)
                            ConsoleAbstraction.WriteRed($"[StreamManager] Cannot create and/or start MediaInput using this webcamDevice:[{webcamDevice}]");
                    }
                    else
                        ConsoleAbstraction.WriteRed($"[StreamManager] Cannot get a webcamDevice with uri:[{stream.Uri}]");
                    break;

                case "screen":
                    var screenDevice = GetScreenDevice(stream.Uri);
                    if (screenDevice is not null)
                    {
                        videoInput = MediaInput.FromScreenDevice(screenDevice, false);
                        if (videoInput is null)
                            ConsoleAbstraction.WriteRed($"[StreamManager] Cannot create and/or start MediaInput using this screenDevice:[{screenDevice}]");
                    }
                    else
                        ConsoleAbstraction.WriteRed($"[StreamManager] Cannot get a screenDevice with uri:[{stream.Uri}]");
                    break;

                case "microphone":
                    var microphoneDevice = GetMicrophoneDevice(stream.Uri);
                    if (microphoneDevice is not null)
                    {
                        audioInput = new SDL2AudioInput(microphoneDevice);
                        if (audioInput is null)
                            ConsoleAbstraction.WriteRed($"[StreamManager] Cannot create and/or start MediaInput using this screenDevice:[{microphoneDevice}]");
                    }
                    else
                        ConsoleAbstraction.WriteRed($"[StreamManager] Cannot get a screenDevice with uri:[{stream.Uri}]");
                    break;

                default:
                    ConsoleAbstraction.WriteGreen($"[StreamManager] Creating InputStreamDevice: {stream} ...");
                    var inputStreamDevice = new InputStreamDevice(stream.Id, stream.Id, stream.Uri, withVideo: withVideo, withAudio: withAudio, loop: true, options: options);

                    ConsoleAbstraction.WriteGreen($"[StreamManager] Creating MediaInput for [{stream.Id}] ...");
                    var mediaInput = new MediaInput(inputStreamDevice, forceLivestream: stream.ForceLiveStream);
                    if (withVideo)
                        videoInput = mediaInput;
                    if (withAudio)
                        audioInput = mediaInput;
                    break;
            }

            return (audioInput, videoInput);
        }

        static private Boolean CheckIfListsHaveSameContent(List<string> array1, List<string> array2)
        {
            var set1 = new HashSet<string>(array1);
            var set2 = new HashSet<string>(array2);

            return  set1.SetEquals(set2);
        }

        static private List<String> GetStreamsToOpen(List<Stream>? streams)
        {
            List<String> result = [];
            if(streams is null) return result;

            foreach (var stream in streams)
            {
                if (stream.Connected)
                {
                    // Add Id of this stream
                    if (!result.Contains(stream.Id)) result.Add(stream.Id);

                    // If it's a compostion add Id of all streams used for it
                    if (stream.VideoComposition?.Count > 0)
                        foreach(var id in stream.VideoComposition)
                            if(!result.Contains(id)) result.Add(id);
                }
            }
            return result;
        }

        static private List<String> GetStreamsToOpen(List<String>? streamsId, ConcurrentDictionary<String, Stream>? streams)
        {
            List<String> result = [];
            if (streams is null) return result;
            if (streamsId is null) return result;

            foreach (var id in streamsId)
            {
                if(streams.TryGetValue(id, out Stream? stream) && stream is not null)
                {
                    // Add Id of this stream
                    if (!result.Contains(stream.Id)) result.Add(stream.Id);

                    // If it's a compostion add Id of all streams used for it
                    if (stream.VideoComposition?.Count > 0)
                        foreach (var subId in stream.VideoComposition)
                            if (!result.Contains(subId)) result.Add(subId);
                }
            }
            return result;
        }

        private List<String> GetCurrentStreamsOpened()
        {
            var audios = _mediasForAudio?.Keys?.ToList();
            var videos  = _mediasForVideo?.Keys?.ToList();
            if (audios is not null)
            {
                if(videos is not null)
                    return audios.Union(videos).ToList(); ;
                return audios;
            }
            return videos ?? [];
        }
                                                                                                                                                                                 
#region Device 

        static public Device? GetDevice(string name, string type)
        {
            List<Device>? devices = null;

            switch (type)
            {
                case "webcam":
                    devices = Rainbow.Medias.Devices.GetWebcamDevices(false).Select(s => (Device)s).ToList();
                    break;

                case "screen":
                    devices = Rainbow.Medias.Devices.GetScreenDevices(false).Select(s => (Device)s).ToList();
                    break;

                case "microphone":
                    devices = Rainbow.Medias.Devices.GetAudioInputDevices();
                    break;
            }

            return devices?.FirstOrDefault(d => d.Name == name);
        }

        static public ScreenDevice? GetScreenDevice(string name)
        {
            var device = GetDevice(name, "screen");
            return (ScreenDevice?)device;
        }

        static public WebcamDevice? GetWebcamDevice(string name)
        {
            var device = GetDevice(name, "webcam");
            return (WebcamDevice?)device;

        }

        static public Device? GetMicrophoneDevice(string name)
        {
            var device = GetDevice(name, "microphone");
            return device;
        }

#endregion Device 

    }
}
