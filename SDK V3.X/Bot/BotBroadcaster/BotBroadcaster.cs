using BotBroadcaster.Model;
using BotLibrary.Model;
using Rainbow;
using Rainbow.Consts;
using Rainbow.Example.Common;
using Rainbow.Example.CommonSDL2;
using Rainbow.Model;
using Rainbow.WebRTC;
using Rainbow.WebRTC.Abstractions;
using Rainbow.WebRTC.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotBroadcaster
{
    public class BotBroadcaster: BotLibrary.BotBase
    {
        Conferences? _rbConferences = null;
        Contacts? _rbContacts = null;
        Bubbles? _rbBubbles = null;

        WebRTCCommunications? _rbWebRTCCommunications = null;
        WebRTCFactory? _rbWebRTCDesktopFactory = null;

        StreamManager? _streamManager = null;

        AudioStreamTrack? _emptyAudioTrack = null;

        // ------------------------------

        BotConfigurationExtended? _configuration = null;
        Boolean _configurationUpdated = false;
        
        Call? _currentCall = null;                          // Current call - updated through event WebRTCCommunications.CallUpdated

        Task _taskCheckConferenceAndMedias = Task.CompletedTask;
        CancelableDelay? _cancelableDelayToCheckConferenceAndMedias = null;
        Task _taskAddOrRemoveMediaForConference = Task.CompletedTask;
        Boolean _conferencesUpdated = false;
        readonly ConferenceStatus _conferenceStatus = new();// Store conference status: media to add/remove, etc ...
        readonly ConcurrentList<String> _conferences = [];

        Task _taskCheckP2PAndMedias = Task.CompletedTask;
        CancelableDelay? _cancelableDelayToCheckP2PAndMedias = null;
        Task _taskAddOrRemoveMediaForP2P = Task.CompletedTask;
        Boolean _p2pUpdated = false;
        readonly P2PStatus _p2pStatus = new();              // Store P2P call status: media to add/remove, etc ...

        private void CreateWebRTCEnvironment()
        {
            if (_rbWebRTCDesktopFactory is null)
            {
                _rbWebRTCDesktopFactory = new();
                _rbWebRTCCommunications = WebRTCCommunications.GetOrCreateInstance(Application, _rbWebRTCDesktopFactory);

                _emptyAudioTrack = (AudioStreamTrack)_rbWebRTCDesktopFactory.CreateEmptyAudioTrack();
                _rbContacts = Application.GetContacts();
                _rbConferences = Application.GetConferences();
                _rbBubbles = Application.GetBubbles();

                RegisterToWebRTCEvents();
            }
        }

        private void CreateStreamManagerEnvironment()
        {
            //TODO - StreamManager must not dispose Streams - We must do it (to release associated MediaTrack)
            _streamManager = new(true, Application.LoggerPrefix); 

            _streamManager.OnStreamOpened += StreamManager_OnStreamOpened;
            _streamManager.OnStreamRemoved += StreamManager_OnStreamRemoved;
            _streamManager.OnStreamDisposing += StreamManager_OnStreamDisposing;
        }

        private void RegisterToWebRTCEvents()
        {
            _rbWebRTCCommunications?.CallUpdated += RbWebRTCCommunications_CallUpdated;

            _rbConferences?.ConferenceUpdated += RbConferences_ConferenceUpdated;
            _rbConferences?.ConferenceRemoved += RbConferences_ConferenceRemoved;
        }

#region CONFERENCE MANAGEMENT - Media + Status

        private void PostPoneCancelableDelayToCheckConferencesAndMedias()
        {
            if (_cancelableDelayToCheckConferenceAndMedias is null)
                _cancelableDelayToCheckConferenceAndMedias = CancelableDelay.StartAfter(500, CheckConferencesAndMedias);
            else
                _cancelableDelayToCheckConferenceAndMedias.PostPone();
        }

        private void CheckConferencesAndMedias()
        {
            _conferencesUpdated = true;
            StartTaskCheckConferencesAndMedias();
        }

        private void StartTaskCheckConferencesAndMedias()
        {
            if (!_taskCheckConferenceAndMedias.IsCompleted)
            {
                ConsoleAbstraction.WriteYellow($"[{BotName}] Task to check conferences and medias is already running => we will not start another one to avoid any conflict - New configuration will be taken into account at the end of current task", logger: log);
                return;
            }
            _taskCheckConferenceAndMedias = Task.Run(TaskCheckConferencesAndMediasAsync);
        }

        private async Task TaskCheckConferencesAndMediasAsync()
        {
            if ((_rbWebRTCDesktopFactory is null)
                || (_rbWebRTCCommunications is null)
                || (_streamManager is null)) return;

            // Do we have a configuration update or a conference update to process
            if ((!_conferencesUpdated) && (!_configurationUpdated)) return;

            // We must indicates that we have taken into account the current configuration / conferences status to avoid to start again this task at the end of current one because of the same configuration update
            _conferencesUpdated = false; 
            _configurationUpdated = false;

            // Ensure to set id, jid and name for conference if it's not the case
            //UpdateConferenceSettings(_configuration);

            // Create a copy of configuration information to work with - we must not work with _configuration object
            var conferencesInConfig = (_configuration?.Conferences is null) ? [] : new List<Model.Conference>(_configuration.Conferences);

            if (String.IsNullOrEmpty(_conferenceStatus.ConferenceId)) // We are NOT currently managing a conference
            {
                ConsoleAbstraction.WriteWhite($"[{BotName}] Not currently managing a conference", logger: log);

                if (_currentCall?.IsConference == true) // A conf is in progress ... This case should not happened
                {
                    // Something to do here ?
                    ConsoleAbstraction.WriteWhite($"[{BotName}] A current call is in progress ... Strange", logger: log);
                }
                else if (_conferences.Count > 0)
                {
                    // Take first conference Id in configuration which is active
                    foreach (var conferenceInConfig in conferencesInConfig)
                    {
                        if (_conferences.Contains(conferenceInConfig.Id))
                        {
                            ConsoleAbstraction.WriteWhite($"[{BotName}] We will join this conference:[{conferenceInConfig.Id}]", logger: log);

                            _conferenceStatus.ConferenceId = conferenceInConfig.Id;

                            // Here we must set Streams as CONNECTED which are used as Audio / Video / Sharing (take into account composition)
                            // The goal is too avoid to close a media if we switch it for Video to Sharing for example

                            // Store media to use for this conference
                            _conferenceStatus.Streams.Clear();

                            if (!String.IsNullOrEmpty(conferenceInConfig.AudioStreamId))
                            {
                                ConsoleAbstraction.WriteWhite($"[{BotName}] AUDIO to use - Stream:[{conferenceInConfig.AudioStreamId}]", logger: log);
                                _conferenceStatus.Streams[Rainbow.Consts.Media.AUDIO] = conferenceInConfig.AudioStreamId;
                            }

                            if (!String.IsNullOrEmpty(conferenceInConfig.VideoStreamId))
                            {
                                ConsoleAbstraction.WriteWhite($"[{BotName}] VIDEO to use - Stream:[{conferenceInConfig.VideoStreamId}]", logger: log);
                                _conferenceStatus.Streams[Rainbow.Consts.Media.VIDEO] = conferenceInConfig.VideoStreamId;
                            }
                            if (!String.IsNullOrEmpty(conferenceInConfig.SharingStreamId))
                            {
                                ConsoleAbstraction.WriteWhite($"[{BotName}] SHARING to use - Stream:[{conferenceInConfig.SharingStreamId}]", logger: log);
                                _conferenceStatus.Streams[Rainbow.Consts.Media.SHARING] = conferenceInConfig.SharingStreamId;
                            }
                            break;
                        }
                    }
                }

                if (!String.IsNullOrEmpty(_conferenceStatus.ConferenceId))
                {
                    // Create Audio Stream track - check if already available in StreamManager
                    if (_conferenceStatus.Streams.TryGetValue(Rainbow.Consts.Media.AUDIO, out var audioStreamId) && audioStreamId != null)
                    {
                        var audioMedia = _streamManager.GetMediaAudioFromStreamId(audioStreamId);
                        if (audioMedia is not null)
                        {
                            ConsoleAbstraction.WriteWhite($"[{BotName}] Create AUDIO TRACK - Stream:[{audioStreamId}]", logger: log);
                            _conferenceStatus.AudioStreamTrack = (AudioStreamTrack?)_rbWebRTCDesktopFactory.CreateAudioTrack(audioMedia);
                        }
                    }

                    // Create empty audio track if necessary
                    if (_conferenceStatus.AudioStreamTrack is null)
                    {
                        ConsoleAbstraction.WriteWhite($"[{BotName}] Create empty AUDIO TRACK", logger: log);
                        _conferenceStatus.AudioStreamTrack = _emptyAudioTrack;
                    }

                    // Join the conference
                    var sdkResult = await _rbWebRTCCommunications.JoinConferenceAsync(_conferenceStatus.ConferenceId, _conferenceStatus.AudioStreamTrack);
                    if (!sdkResult.Success)
                    {
                        ConsoleAbstraction.WriteRed($"[{BotName}] Conference:[{_conferenceStatus.ConferenceId}] - Cannot join:[{sdkResult.Result}]", logger: log);

                        // TODO :  Retry later ... Use a counter to avoid unlimited tentative 
                        PostPoneCancelableDelayToCheckConferencesAndMedias();
                    }
                    else
                    {
                        // TODO: If we use a counter (to avoid unlimited tentative to join conf.) - reset it now

                        ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Conference:[{_conferenceStatus.ConferenceId}] - Joined done with success", logger: log);

                        // Inform streamManager of the new config
                        ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Ask StreamManager to use config file only - no stream to use", logger: log);

                        _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _conferenceStatus.Streams);
                    }


                    // We need to:
                    // - start to join a conference (with empty audio track)
                    // - wait until we have really joined (i.e. we receive the update of the call with status connected and the correct conference Id)
                    // - ask StreamManager to update streams according configuration
                    // - wait each stream used in this conference to be added
                }
                else
                {
                    // We are not currently managing no conference and based on config there is not conf.to join.
                }
            }
            else // We are currently managing streams for a conference
            {
                ConsoleAbstraction.WriteWhite($"[{BotName}] - Currently managing a conference:[{_conferenceStatus.ConferenceId}]", logger: log);

                if (_currentCall is null) // The conference has been closed but we don't stopped related streams yet
                {
                    ConsoleAbstraction.WriteWhite($"[{BotName}] - Current call is null", logger: log);

                    // Hang up the conference
                    await _rbWebRTCCommunications.HangUpCallAsync(_conferenceStatus.ConferenceId);

                    ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Conference:[{_conferenceStatus.ConferenceId}] - HangUp has been done", logger: log);
                    _conferenceStatus.Reset();

                    // Inform streamManager of the new config
                    ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Ask StreamManager to use config file only - no stream to use", logger: log);

                    _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _conferenceStatus.Streams);

                    // Check a little later the config to ensure we must no more be in the conf.
                    PostPoneCancelableDelayToCheckConferencesAndMedias();

                    // We need to:
                    //  - ask streamManager to update streams according configuration
                    //  - wait each stream used in this conferenc to by removed
                    //  - close current Tracksss
                }
                else if (_currentCall.IsConference)
                {
                    if (_conferenceStatus.ConferenceId.Equals(_currentCall?.Id, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ConsoleAbstraction.WriteWhite($"[{BotName}] - Currently managing a conference:[{_conferenceStatus.ConferenceId}]", logger: log);

                        // Check if this conference is still in the config
                        var conferenceInConfig = _configuration?.Conferences?.FirstOrDefault(c => c.Id == _currentCall?.Id);

                        if (conferenceInConfig is null)
                        {
                            ConsoleAbstraction.WriteWhite($"[{BotName}] - We have to hangup from the conference:[{_conferenceStatus.ConferenceId}]", logger: log);
                            // We must hangup ... How to do it simply ?
                            RbWebRTCCommunications_CallUpdated(null);
                        }
                        else
                        {
                            // we must update streams 
                            // Store media to use for this conference
                            _conferenceStatus.Streams.Clear();
                            if (!String.IsNullOrEmpty(conferenceInConfig.AudioStreamId))
                                _conferenceStatus.Streams[Rainbow.Consts.Media.AUDIO] = conferenceInConfig.AudioStreamId;
                            if (!String.IsNullOrEmpty(conferenceInConfig.VideoStreamId))
                                _conferenceStatus.Streams[Rainbow.Consts.Media.VIDEO] = conferenceInConfig.VideoStreamId;
                            if (!String.IsNullOrEmpty(conferenceInConfig.SharingStreamId))
                                _conferenceStatus.Streams[Rainbow.Consts.Media.SHARING] = conferenceInConfig.SharingStreamId;

                            ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Conference:[{_conferenceStatus.ConferenceId}] - Status:[{_currentCall?.CallStatus}] - LocalMedias:[{Rainbow.Util.MediasToString(_currentCall?.LocalMedias ?? 0)}]", logger: log);


                            if (_currentCall?.IsActive() == true)
                            {
                                StartTaskAddOrRemoveMediaForConference();
                            }
                            else if (_currentCall?.CallStatus == Rainbow.Enums.CallStatus.UNKNOWN)
                            {
                                _conferencesUpdated = true;
                                StartTaskCheckConferencesAndMedias();
                            }
                        }
                    }
                    else
                    {
                        ConsoleAbstraction.WriteRed($"[{BotName}] - Currently managing a conference:[{_conferenceStatus.ConferenceId}] but not the good one ... Current Call:[{_currentCall?.Id}]", logger: log);

                        // Is this case possible ?
                        // We are managing a conference but not the good one => We need to change conference and related streams
                    }
                }
            }
        }

        private void StartTaskAddOrRemoveMediaForConference()
        {
            if (!_taskAddOrRemoveMediaForConference.IsCompleted)
            {
                ConsoleAbstraction.WriteYellow($"[{BotName}] Task to Add/remove media is already running => we will not start another one to avoid any conflict", logger: log);
                return;
            }
            _taskAddOrRemoveMediaForConference = Task.Run(TaskAddOrRemoveMediaForConferenceAsync);
        }

        private async Task TaskAddOrRemoveMediaForConferenceAsync()
        {
            if ((_rbWebRTCDesktopFactory is null) 
                || (_rbWebRTCCommunications is null)
                || (_streamManager is null)
                ) return;

            String? streamId;
            Rainbow.Medias.IMediaVideo? mediaVideo;
            Rainbow.Medias.IMediaVideo? mediaSharing;
            SdkResult<Boolean> sdkResult;

            if ((!String.IsNullOrEmpty(_conferenceStatus.ConferenceId))
                && (_currentCall?.IsActive() == true)
                && (_currentCall?.IsConference == true)
                && (_conferenceStatus.ConferenceId.Equals(_currentCall?.Id, StringComparison.InvariantCultureIgnoreCase)))
            {
                // We manage Audio first then Video and finally Sharing
                // --------------------------------
                // --- START: CHECK AUDIO
                if (_conferenceStatus.AudioStreamTrack?.IsEmptyTrack == true)
                {
                    ConsoleAbstraction.WriteYellow($"[{BotName}] AUDIO Track is an empty track", logger: log);

                    if (_conferenceStatus.Streams.TryGetValue(Rainbow.Consts.Media.AUDIO, out streamId)
                        && (streamId is not null))
                    {
                        var mediaAudio = _streamManager.GetMediaAudioFromStreamId(streamId);
                        if (mediaAudio is null) // It means we didn't ask StreamManager to manage it
                        {
                            _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _conferenceStatus.Streams);

                            PostPoneCancelableDelayToCheckConferencesAndMedias();
                            return;
                        }
                        else
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] AUDIO Track is available using StreamManager - Stream:[{streamId}]", logger: log);

                            //var previousTrack = _conferenceStatus.AudioStreamTrack;

                            var audioTrack = _rbWebRTCDesktopFactory.CreateAudioTrack(mediaAudio);
                            if (audioTrack is not null)
                            {
                                sdkResult = await _rbWebRTCCommunications.ChangeAudioAsync(_currentCall?.Id ?? "", audioTrack);
                                if (sdkResult.Success)
                                {
                                    ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Audio has been changed (from empty track) - Stream:[{streamId}]", logger: log);
                                    _conferenceStatus.AudioStreamTrack = (AudioStreamTrack)audioTrack;
                                    //previousTrack?.Dispose();
                                }
                                else
                                {
                                    // Cannot change audio ....
                                    ConsoleAbstraction.WriteRed($"[{BotName}] Cannot change Audio Track - Stream:[{streamId}] - Error: [{sdkResult.Result}]", logger: log);
                                }
                            }
                            else
                            {
                                // Cannot create audio track ....
                                ConsoleAbstraction.WriteRed($"[{BotName}] Cannot create Audio Track - Stream:[{streamId}]", logger: log);
                            }

                            PostPoneCancelableDelayToCheckConferencesAndMedias();
                            return;
                        }
                    }
                    else
                    {
                        ConsoleAbstraction.WriteYellow($"[{BotName}] No AUDIO Track to use in this conference", logger: log);
                    }
                }
                else
                {
                    if (_conferenceStatus.Streams.TryGetValue(Rainbow.Consts.Media.AUDIO, out streamId) && !String.IsNullOrEmpty(streamId))
                    {
                        ConsoleAbstraction.WriteYellow($"[{BotName}] AUDIO Track is not an empty track", logger: log);

                        // Audio track must be changed
                        if (streamId != _conferenceStatus.AudioStreamTrack?.Id)
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] AUDIO Track must be updated", logger: log);

                            var mediaAudio = _streamManager.GetMediaAudioFromStreamId(streamId);
                            if (mediaAudio is null) // It means we didn't ask StreamManager to manage it
                            {
                                ConsoleAbstraction.WriteYellow($"[{BotName}] AUDIO Media [{streamId}] is not known from StreamManager", logger: log);

                                _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _conferenceStatus.Streams);

                                PostPoneCancelableDelayToCheckConferencesAndMedias();
                                return;
                            }
                            else
                            {
                                ConsoleAbstraction.WriteYellow($"[{BotName}] AUDIO Media [{streamId}] is known from StreamManager", logger: log);

                                //var previousTrack = _conferenceStatus.AudioStreamTrack;

                                var audioTrack = _rbWebRTCDesktopFactory.CreateAudioTrack(mediaAudio);
                                if (audioTrack is not null)
                                {
                                    sdkResult = await _rbWebRTCCommunications.ChangeAudioAsync(_currentCall?.Id ?? "", audioTrack);
                                    if (sdkResult.Success)
                                    {
                                        ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Audio has been changed (from a previous track) - Stream:[{streamId}]", logger: log);
                                        _conferenceStatus.AudioStreamTrack = (AudioStreamTrack)audioTrack;
                                        //previousTrack?.Dispose();
                                    }
                                    else
                                    {
                                        // Cannot change audio ....
                                        ConsoleAbstraction.WriteRed($"[{BotName}] Cannot change audio track - Stream:[{streamId}] - Error:[{sdkResult.Result}]", logger: log);
                                    }
                                }
                                else
                                {
                                    // Cannot create audio track ....
                                    ConsoleAbstraction.WriteRed($"[{BotName}] Cannot create audio track - Stream:[{streamId}]", logger: log);
                                }

                                PostPoneCancelableDelayToCheckConferencesAndMedias();
                                return;
                            }
                        }
                        else
                        {
                            // Same audio track to use - nothing to do
                        }
                    }
                    else
                    {
                        ConsoleAbstraction.WriteYellow($"[{BotName}] AUDIO Track must be an empty track", logger: log);

                        // We no more use audio
                        //var previousTrack = _conferenceStatus.AudioStreamTrack;

                        var audioTrack = _emptyAudioTrack;
                        if (audioTrack is not null)
                        {
                            sdkResult = await _rbWebRTCCommunications.ChangeAudioAsync(_currentCall?.Id ?? "", audioTrack);
                            if (sdkResult.Success)
                            {
                                ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Audio has been changed (use empty track)", logger: log);
                                _conferenceStatus.AudioStreamTrack = (AudioStreamTrack)audioTrack;
                                //previousTrack?.Dispose();
                            }
                            else
                            {
                                // Cannot change audio ....
                                ConsoleAbstraction.WriteRed($"[{BotName}] Cannot change audio track  (using empty one) - Error:[{sdkResult.Result}]", logger: log);
                            }
                        }
                        else
                        {
                            // Cannot create audio track ....
                            ConsoleAbstraction.WriteRed($"[{BotName}] Cannot create empty audio track ", logger: log);
                        }

                        _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _conferenceStatus.Streams);

                        PostPoneCancelableDelayToCheckConferencesAndMedias();
                        return;
                    }
                }
                // --- END: CHECK AUDIO
                // --------------------------------

                // --------------------------------
                // --- START: CHECK VIDEO
                String videoAction = "none"; // "add", "remove" "update"

                _conferenceStatus.Streams.TryGetValue(Rainbow.Consts.Media.VIDEO, out streamId);

                if (String.IsNullOrEmpty(streamId))
                {
                    if (Rainbow.Util.MediasWithVideo(_currentCall?.LocalMedias ?? 0))
                        videoAction = "remove";
                }
                else
                {
                    if (streamId != _conferenceStatus.VideoStreamTrack?.Id)
                    {
                        if (Rainbow.Util.MediasWithVideo(_currentCall?.LocalMedias ?? 0))
                        {
                            videoAction = "update";
                        }
                        else
                        {
                            videoAction = "add";
                            // Clear bad status
                            _conferenceStatus.VideoStreamTrack = null;
                        }
                    }
                    else
                    {
                        if (!Rainbow.Util.MediasWithVideo(_currentCall?.LocalMedias ?? 0))
                        {
                            videoAction = "add";
                            // Clear bad status
                            _conferenceStatus.VideoStreamTrack = null;
                        }
                    }
                }
                ConsoleAbstraction.WriteYellow($"[{BotName}] VIDEO Track action:[{videoAction}] - Stream:[{streamId}]", logger: log);

                switch (videoAction)
                {
                    case "add":
                        if(streamId is null)
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] VIDEO Track must be added but streamId is null", logger: log);
                            return;
                        }

                        if (_currentCall?.IsActive() != true)
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] VIDEO Track must be added - Stream:[{streamId}] but call is not active - we do it later", logger: log);
                        }
                        else
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] VIDEO Track must be added - Stream:[{streamId}]", logger: log);
                            mediaVideo = _streamManager.GetMediaVideoFromStreamId(streamId);
                            if (mediaVideo is null) // It means we didn't ask StreamManager to manage it
                            {
                                ConsoleAbstraction.WriteYellow($"[{BotName}] VIDEO Media [{streamId}] is not known from StreamManager", logger: log);
                                _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _conferenceStatus.Streams);
                            }
                            else
                            {
                                ConsoleAbstraction.WriteYellow($"[{BotName}] VIDEO Media is known from StreamManager - Stream:[{streamId}]", logger: log);
                                var videoTrack = _rbWebRTCDesktopFactory.CreateVideoTrack(mediaVideo);
                                if (videoTrack is not null)
                                {
                                    _conferenceStatus.VideoStreamTrack = (VideoStreamTrack)videoTrack;
                                    sdkResult = await _rbWebRTCCommunications.AddVideoAsync(_currentCall?.Id ?? "", videoTrack);
                                    if (sdkResult.Success)
                                    {
                                        ConsoleAbstraction.WriteDarkYellow($"[{BotName}] VIDEO has been added - Stream:[{streamId}]", logger: log);
                                    }
                                    else
                                    {
                                        _conferenceStatus.VideoStreamTrack = null;
                                        // Cannot change VIDEO ....
                                        ConsoleAbstraction.WriteRed($"[{BotName}] Cannot add VIDEO track - Stream:[{streamId}] - Error:[{sdkResult.Result}]", logger: log);
                                    }
                                }
                                else
                                {
                                    // Cannot create VIDEO track ....
                                    ConsoleAbstraction.WriteRed($"[{BotName}] Cannot create VIDEO track - Stream:[{streamId}]", logger: log);
                                }
                            }
                        }

                        PostPoneCancelableDelayToCheckConferencesAndMedias();
                        return;

                    case "update":
                        if (streamId is null)
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] VIDEO Track must be added but streamId is null", logger: log);
                            return;
                        }

                        ConsoleAbstraction.WriteYellow($"[{BotName}] VIDEO Track must be updated - Stream:[{streamId}]", logger: log);

                        mediaVideo = _streamManager.GetMediaVideoFromStreamId(streamId);
                        if (mediaVideo is null) // It means we didn't ask StreamManager to manage it
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] VIDEO Media [{streamId}] is not known from StreamManager", logger: log);
                            _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _conferenceStatus.Streams);
                        }
                        else
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] VIDEO Media is known from StreamManager - Stream:[{streamId}]", logger: log);
                            var videoTrack = _rbWebRTCDesktopFactory.CreateVideoTrack(mediaVideo);
                            if (videoTrack is not null)
                            {
                                _conferenceStatus.VideoStreamTrack = (VideoStreamTrack)videoTrack;
                                sdkResult = await _rbWebRTCCommunications.ChangeVideoAsync(_currentCall?.Id ?? "", videoTrack);
                                if (sdkResult.Success)
                                {
                                    ConsoleAbstraction.WriteDarkYellow($"[{BotName}] VIDEO has been changed - Stream:[{streamId}]", logger: log);
                                }
                                else
                                {
                                    _conferenceStatus.VideoStreamTrack = null;
                                    // Cannot change VIDEO ....
                                    ConsoleAbstraction.WriteRed($"[{BotName}] Cannot changed VIDEO track - Stream:[{streamId}] - Error:[{sdkResult.Result}]", logger: log);
                                }
                            }
                            else
                            {
                                // Cannot create VIDEO track ....
                                ConsoleAbstraction.WriteRed($"[{BotName}] Cannot create VIDEO track - Stream:[{streamId}]", logger: log);
                            }
                        }
                        PostPoneCancelableDelayToCheckConferencesAndMedias();
                        return;

                    case "remove":
                        // We no more use VIDEO
                        ConsoleAbstraction.WriteYellow($"[{BotName}] VIDEO Track must be removed", logger: log);

                        //var previousTrack = _conferenceStatus.VideoStreamTrack;
                        sdkResult = await _rbWebRTCCommunications.RemoveVideoAsync(_currentCall?.Id ?? "");
                        if (sdkResult.Success)
                        {
                            ConsoleAbstraction.WriteDarkYellow($"[{BotName}] VIDEO has been removed", logger: log);
                            _conferenceStatus.VideoStreamTrack = null;
                            //previousTrack?.Dispose();
                        }
                        else
                        {
                            // Cannot change VIDEO ....
                            ConsoleAbstraction.WriteRed($"[{BotName}] Cannot remove VIDEO track - Error:[{sdkResult.Result}]", logger: log);
                        }

                        _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _conferenceStatus.Streams);
                        PostPoneCancelableDelayToCheckConferencesAndMedias();
                        return;
                }
                // --- END: CHECK VIDEO
                // --------------------------------


                // --------------------------------
                // --- START: CHECK SHARING
                String sharingAction = "none"; // "add", "remove" "update"

                _conferenceStatus.Streams.TryGetValue(Rainbow.Consts.Media.SHARING, out streamId);
                
                if (String.IsNullOrEmpty(streamId))
                {
                    if (Rainbow.Util.MediasWithSharing(_currentCall?.LocalMedias ?? 0))
                        sharingAction = "remove";
                }
                else
                {
                    if (streamId != _conferenceStatus.SharingStreamTrack?.Id)
                    {
                        if (Rainbow.Util.MediasWithSharing(_currentCall?.LocalMedias ?? 0))
                        {
                            sharingAction = "update";
                            //sharingAction = "remove"; // We remove then we will add
                        }
                        else
                        {
                            sharingAction = "add";
                            // Clear bad status
                            //_conferenceStatus.SharingStreamTrack?.Dispose();
                            _conferenceStatus.SharingStreamTrack = null;
                        }
                    }
                    else
                    {
                        if (!Rainbow.Util.MediasWithSharing(_currentCall?.LocalMedias ?? 0))
                        {
                            sharingAction = "add";
                            // Clear bad status
                            //_conferenceStatus.SharingStreamTrack?.Dispose();
                            _conferenceStatus.SharingStreamTrack = null;
                        }
                    }
                }
                ConsoleAbstraction.WriteYellow($"[{BotName}] SHARING Track action:[{sharingAction}] - Stream:[{streamId}]", logger: log);
                switch (sharingAction)
                {
                    case "add":
                        if (streamId is null)
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] SHARING Track must be added but streamId is null", logger: log);
                            return;
                        }

                        if (_currentCall?.IsActive() != true)
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] SHARING Track must be added - Stream:[{streamId}] but call is not active - we do it later", logger: log);
                        }
                        else
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] SHARING Track must be added - Stream:[{streamId}]", logger: log);
                            mediaSharing = _streamManager.GetMediaVideoFromStreamId(streamId);
                            if (mediaSharing is null) // It means we didn't ask StreamManager to manage it
                            {
                                ConsoleAbstraction.WriteYellow($"[{BotName}] SHARING Media [{streamId}] is not known from StreamManager", logger: log);
                                _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _conferenceStatus.Streams);
                            }
                            else
                            {
                                ConsoleAbstraction.WriteYellow($"[{BotName}] SHARING Media is known from StreamManager - Stream:[{streamId}]", logger: log);
                                var sharingTrack = _rbWebRTCDesktopFactory.CreateVideoTrack(mediaSharing);
                                if (sharingTrack is not null)
                                {
                                    _conferenceStatus.SharingStreamTrack = (VideoStreamTrack)sharingTrack;
                                    sdkResult = await _rbWebRTCCommunications.AddSharingAsync(_currentCall?.Id ?? "", sharingTrack);
                                    if (sdkResult.Success)
                                    {
                                        ConsoleAbstraction.WriteDarkYellow($"[{BotName}] SHARING has been added - Stream:[{streamId}]", logger: log);
                                    }
                                    else
                                    {
                                        _conferenceStatus.SharingStreamTrack = null;
                                        // Cannot change SHARING ....
                                        ConsoleAbstraction.WriteRed($"[{BotName}] Cannot add SHARING track - Stream:[{streamId}] - Error:[{sdkResult.Result}]", logger: log);
                                    }
                                }
                                else
                                {
                                    // Cannot create SHARING track ....
                                    ConsoleAbstraction.WriteRed($"[{BotName}] Cannot create SHARING track - Stream:[{streamId}]", logger: log);
                                }
                            }
                        }

                        PostPoneCancelableDelayToCheckConferencesAndMedias();
                        return;

                    case "update":
                        if (streamId is null)
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] SHARING Track must be added but streamId is null", logger: log);
                            return;
                        }

                        ConsoleAbstraction.WriteYellow($"[{BotName}] SHARING Track must be updated - Stream:[{streamId}]", logger: log);

                        mediaSharing = _streamManager.GetMediaVideoFromStreamId(streamId);
                        if (mediaSharing is null) // It means we didn't ask StreamManager to manage it
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] SHARING Media [{streamId}] is not known from StreamManager", logger: log);
                            _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _conferenceStatus.Streams);
                        }
                        else
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] SHARING Media is known from StreamManager - Stream:[{streamId}]", logger: log);
                            var sharingTrack = _rbWebRTCDesktopFactory.CreateVideoTrack(mediaSharing);
                            if (sharingTrack is not null)
                            {
                                _conferenceStatus.SharingStreamTrack = (VideoStreamTrack)sharingTrack;
                                sdkResult = await _rbWebRTCCommunications.ChangeSharingAsync(_currentCall?.Id ?? "", sharingTrack);
                                if (sdkResult.Success)
                                {
                                    ConsoleAbstraction.WriteDarkYellow($"[{BotName}] SHARING has been changed - Stream:[{streamId}]", logger: log);
                                }
                                else
                                {
                                    _conferenceStatus.SharingStreamTrack = null;
                                    // Cannot change SHARING ....
                                    ConsoleAbstraction.WriteRed($"[{BotName}] Cannot changed SHARING track - Stream:[{streamId}] - Error:[{sdkResult.Result}]", logger: log);
                                }
                            }
                            else
                            {
                                // Cannot create SHARING track ....
                                ConsoleAbstraction.WriteRed($"[{BotName}] Cannot create SHARING track - Stream:[{streamId}]", logger: log);
                            }
                        }
                        PostPoneCancelableDelayToCheckConferencesAndMedias();
                        return;

                    case "remove":
                        // We no more use SHARING
                        ConsoleAbstraction.WriteYellow($"[{BotName}] SHARING Track must be removed", logger: log);

                        //var previousTrack = _conferenceStatus.SharingStreamTrack;
                        sdkResult = await _rbWebRTCCommunications.RemoveSharingAsync(_currentCall?.Id ?? "");
                        if (sdkResult.Success)
                        {
                            ConsoleAbstraction.WriteDarkYellow($"[{BotName}] SHARING has been removed", logger: log);
                            _conferenceStatus.SharingStreamTrack = null;
                            //previousTrack?.Dispose();
                        }
                        else
                        {
                            // Cannot change sharing ....
                            ConsoleAbstraction.WriteRed($"[{BotName}] Cannot remove SHARING track - Error:[{sdkResult.Result}]", logger: log);
                        }

                        _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _conferenceStatus.Streams);

                        PostPoneCancelableDelayToCheckConferencesAndMedias();
                        return;
                }
                // --- END: CHECK SHARING
                // --------------------------------


                //PostPoneCancelableDelayToCheckConfigAndConference();
            }
        }

#endregion CONFERENCE MANAGEMENT - Media + Status

#region P2P MANAGEMENT - Media + Status

        private void PostPoneCancelableDelayToCheckP2PAndMedias()
        {
            if (_cancelableDelayToCheckP2PAndMedias is null)
                _cancelableDelayToCheckP2PAndMedias = CancelableDelay.StartAfter(500, CheckP2PAndMedias);
            else
                _cancelableDelayToCheckP2PAndMedias.PostPone();
        }

        private void CheckP2PAndMedias()
        {
            _p2pUpdated = true;
            StartTaskCheckP2PAndMedias();
        }

        private void StartTaskCheckP2PAndMedias()
        {
            if (!_taskCheckP2PAndMedias.IsCompleted)
            {
                ConsoleAbstraction.WriteYellow($"[{BotName}] Task to check conferences and medias is already running => we will not start another one to avoid any conflict - New configuration will be taken into account at the end of current task", logger: log);
                return;
            }
            _taskCheckP2PAndMedias = Task.Run(TaskCheckP2PAndMedias);
        }

        private async Task TaskCheckP2PAndMedias()
        {
            if ((_rbWebRTCDesktopFactory is null)
                || (_rbWebRTCCommunications is null)
                || (_streamManager is null)) return;

            // Do we have a configuration update or a p2p update to process
            if ((!_p2pUpdated) && (!_configurationUpdated)) return;

            // We must indicates that we have taken into account the current configuration / p2p status to avoid to start again this task at the end of current one because of the same configuration update
            _p2pUpdated = false;
            _configurationUpdated = false;

            if (String.IsNullOrEmpty(_p2pStatus.CallId)) // We are NOT currently managing a P2P call
            {
                ConsoleAbstraction.WriteWhite($"[{BotName}] Not currently managing a P2P call", logger: log);

                if (_currentCall?.IsConference == false) // A P2P call is in progress ... 
                {
                    if (_currentCall?.IsRinging() == true)
                    {
                        // Check if we must accept or reject this P2P call based on configuration
                        Boolean rejectCall = true;
                        if (_configuration?.P2P?.IsValid == true)
                        {
                            switch (_configuration.P2P.AllowedFor)
                            {
                                case "all":
                                    rejectCall = false;
                                    break;

                                case "none":
                                    rejectCall = true;
                                    break;

                                case "administrator":
                                    var found = _configuration.Administrators?.FirstOrDefault(admin => admin.Id == _currentCall.Peer?.Id || admin.Jid == _currentCall.Peer?.Jid);
                                    rejectCall = found is null;
                                    break;

                                default:
                                    rejectCall = !(_configuration.P2P.Contact?.Peer.Id == _currentCall.Peer?.Id);
                                    break;
                            }
                        }

                        if (rejectCall)
                        {
                            ConsoleAbstraction.WriteWhite($"[{BotName}] P2P call rejected - CallId:[{_currentCall.Id}]", logger: log);
                            var sdkResult = await _rbWebRTCCommunications.RejectCallAsync(_currentCall.Id);
                            if (!sdkResult.Success)
                                ConsoleAbstraction.WriteRed($"[{BotName}] P2P call cannot be rejected:[{sdkResult.Result}]", logger: log);
                            _currentCall = null;
                        }
                        else
                        {
                            // Set tracks used to make P2P call
                            Dictionary<int, IMediaStreamTrack?>? mediaStreamTracks = new()
                            {
                                {Media.AUDIO, _emptyAudioTrack }
                            };
                            _p2pStatus.AudioStreamTrack = _emptyAudioTrack;
                            ConsoleAbstraction.WriteWhite($"[{BotName}] P2P call answered - CallId:[{_currentCall.Id}]", logger: log);
                            var sdkResult = await _rbWebRTCCommunications.AnswerCallAsync(_currentCall.Id, mediaStreamTracks);
                            if (!sdkResult.Success)
                                ConsoleAbstraction.WriteRed($"[{BotName}] P2P call cannot be answered:[{sdkResult.Result}]", logger: log);
                            else
                                _p2pStatus.CallId = _currentCall.Id;
                        }
                    }
                }
            }
            else // We are currently managing streams for a p2p call
            {
                ConsoleAbstraction.WriteWhite($"[{BotName}] - Currently managing a P2P call:[{_p2pStatus.CallId}]", logger: log);

                if (_currentCall is null) // The P2P call has been closed but we don't stopped related streams yet
                {
                    ConsoleAbstraction.WriteWhite($"[{BotName}] - Current call is null", logger: log);

                    // Hang up the P2P call
                    await _rbWebRTCCommunications.HangUpCallAsync(_p2pStatus.CallId);

                    ConsoleAbstraction.WriteDarkYellow($"[{BotName}] P2P call:[{_p2pStatus.CallId}] - HangUp has been done", logger: log);
                    _p2pStatus.Reset();

                    // Inform streamManager of the new config
                    ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Ask StreamManager to use config file only - no stream to use", logger: log);

                    _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _p2pStatus.Streams);

                    // Check a little later the config to ensure we must no more be in the p2p call.
                    PostPoneCancelableDelayToCheckP2PAndMedias();
                }
                else if (!_currentCall.IsConference)
                {

                    if(_currentCall.Id == _p2pStatus.CallId)
                    {
                        ConsoleAbstraction.WriteWhite($"[{BotName}] - Currently managing a P2P call:[{_p2pStatus.CallId}]", logger: log);

                        // TODO - Check if the call is still allowed according the conf
                        if(!true) // => Config is no more ok
                        {

                        }
                        else
                        {
                            // we must update streams 
                            // Store media to use for this conference
                            _p2pStatus.Streams.Clear();
                            if (_configuration is not null)
                            {
                                if (!String.IsNullOrEmpty(_configuration.P2P.AudioStreamId))
                                    _conferenceStatus.Streams[Rainbow.Consts.Media.AUDIO] = _configuration.P2P.AudioStreamId;
                                if (!String.IsNullOrEmpty(_configuration.P2P.VideoStreamId))
                                    _conferenceStatus.Streams[Rainbow.Consts.Media.VIDEO] = _configuration.P2P.VideoStreamId;
                                if (!String.IsNullOrEmpty(_configuration.P2P.SharingStreamId))
                                    _conferenceStatus.Streams[Rainbow.Consts.Media.SHARING] = _configuration.P2P.SharingStreamId;
                            }
                            ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Conference:[{_p2pStatus.CallId}] - Status:[{_currentCall?.CallStatus}] - LocalMedias:[{Rainbow.Util.MediasToString(_currentCall?.LocalMedias ?? 0)}]", logger: log);

                            if (_currentCall?.IsActive() == true)
                            {
                                StartTaskAddOrRemoveMediaForP2P();
                            }
                            else if (_currentCall?.CallStatus == Rainbow.Enums.CallStatus.UNKNOWN)
                            {
                                _p2pUpdated = true;
                                StartTaskCheckP2PAndMedias();
                            }
                        }
                    }
                    else
                    {
                        ConsoleAbstraction.WriteRed($"[{BotName}] - Currently managing a P2P call:[{_p2pStatus.CallId}] but not the good one ... Current Call:[{_currentCall?.Id}]", logger: log);
                    }
                }
            }
        }

        private void StartTaskAddOrRemoveMediaForP2P()
        {
            if (!_taskAddOrRemoveMediaForP2P.IsCompleted)
            {
                ConsoleAbstraction.WriteYellow($"[{BotName}] Task to Add/remove media is already running => we will not start another one to avoid any conflict", logger: log);
                return;
            }
            _taskAddOrRemoveMediaForP2P = Task.Run(TaskAddOrRemoveMediaForP2P);
        }

        private async Task TaskAddOrRemoveMediaForP2P()
        {
            if ((_rbWebRTCDesktopFactory is null) 
                || (_rbWebRTCCommunications is null)
                || (_streamManager is null)
                ) return;
            // TODO

        }

#endregion P2P MANAGEMENT - Media + Status


        private void UpdatePresence()
        {
            if (_rbContacts is null) return;

            // Manage presence according medias used
            if ( (_currentCall is null) || (!_currentCall.IsInProgress()))
            {
                _rbContacts.SetPresenceLevelAsync(_rbContacts.CreatePresence(PresenceLevel.Online));
                _currentCall = null;
            }
            else
            {
                if (_currentCall?.IsRinging() == false)
                {
                    // The call is NOT IN RINGING STATE  => We update presence according media
                    _rbContacts?.SetBusyPresenceAccordingMediasAsync(_currentCall.LocalMedias).StartAndForget();
                }
            }
        }

        private Bubble? GetBubble(string id, string jid, string name)
        {
            if (_rbBubbles is null) return null;
            
            Bubble? result = _rbBubbles.GetBubbleById(id);
            result ??= _rbBubbles.GetBubbleByJid(jid);
            result ??= _rbBubbles.GetAllBubbles().Find(b => b.Peer.DisplayName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return result;
        }

        private async Task<Contact?> GetContactAsync(Account account)
        {
            if (_rbContacts is null)
                return null;

            Contact? result = null;
            if (!String.IsNullOrEmpty(account.Id))
                result = await _rbContacts.GetContactByIdInCacheFirstAsync(account.Id);

            if (result is not null)
                return result;

            if (!String.IsNullOrEmpty(account.Jid))
                result = await _rbContacts.GetContactByJidInCacheFirstAsync(account.Jid);

            if (result is not null)
                return result;

            result = GetContactByEmail(account.Login);

            return result;
        }

        private Contact? GetContactByEmail(String email)
        {
            if (_rbContacts is null)
                return null;

            Contact? result = null;
            if (!String.IsNullOrEmpty(email))
                result = _rbContacts.GetAllContacts()?.Find(contact => contact?.LoginEmail?.Equals(email, StringComparison.InvariantCultureIgnoreCase) == true);

            return result;
        }

        static private Account? GetAccount(Contact? contact)
        {
            if (contact is null) return null;

            return new Account()
            {
                Id = contact.Peer.Id,
                Jid = contact.Peer.Jid,
                Login = contact.LoginEmail,
                FirstName = contact.FirstName,
                LastName = contact.LastName
            };
        }

        private static List<Account> GetAccounts(List<Contact>? contacts)
        {
            var result = new List<Account>();

            if (contacts is null)
                return result;

            foreach (var contact in contacts)
            {
                var account = GetAccount(contact);
                if (account is not null)
                    result.Add(account);
            }

            return result;
        }

        private async Task UpdateConfigurationWithCorrectDataAsync()
        {
            if(Application.IsConnected())
            {
                //use a temporary 
                var _tempConfiguration = _configuration;

                await UpdateAdministratorsConfigurationAsync(_tempConfiguration);

                UpdateConferenceConfiguration(_tempConfiguration);

                UpdateP2PConfiguration(_tempConfiguration);

                _configuration = _tempConfiguration;
            }
        }

        private async Task UpdateAdministratorsConfigurationAsync(BotConfigurationExtended? botConfigurationExtended)
        {
            // Check Administrators
            if (botConfigurationExtended?.Administrators?.Count > 0)
            {
                List<Contact> contacts = [];
                foreach (var admin in botConfigurationExtended.Administrators)
                {
                    var contact = await GetContactAsync(admin);
                    if (contact is not null)
                        contacts.Add(contact);
                }

                botConfigurationExtended.Administrators = GetAccounts(contacts);
            }
        }

        private void UpdateConferenceConfiguration(BotConfigurationExtended? botConfigurationExtended)
        {
            if (botConfigurationExtended?.Conferences?.Count > 0)
            {
                foreach (var conference in botConfigurationExtended.Conferences)
                {
                    Bubble? bubble = GetBubble(conference.Id, conference.Jid, conference.Name);
                    if (bubble is null)
                        continue;

                    // If a bubble has been found, about the settings
                    conference.Id = bubble.Peer.Id;
                    conference.Jid = bubble.Peer.Jid;
                    conference.Name = bubble.Peer.DisplayName;
                }
            }
        }

        private void UpdateP2PConfiguration(BotConfigurationExtended? botConfigurationExtended)
        {
            if ( botConfigurationExtended?.P2P?.AllowedFor is not null && !P2P.PossibleValues.Contains(botConfigurationExtended.P2P.AllowedFor))
                botConfigurationExtended.P2P.Contact = GetContactByEmail(botConfigurationExtended.P2P.AllowedFor);
        }

#region Events triggered by StreamManager

        private void StreamManager_OnStreamRemoved(string streamId, int media, Boolean stillUsed)
        {
            // The MediaInput specified must be removed from current conference
            ConsoleAbstraction.WriteDarkYellow($"[{BotName}] OnStreamRemoved - Media:[{Rainbow.Util.MediasToString(media)}] - Stream:[{streamId}] - StillUsed:[{stillUsed}]", logger: log);

            if (_currentCall?.IsActive() == true)
            {
                if (_currentCall.IsConference == true)
                    StartTaskAddOrRemoveMediaForConference();
                else
                    StartTaskAddOrRemoveMediaForP2P();
            }
        }

        private void StreamManager_OnStreamOpened(string streamId, int media, Boolean stillUsed)
        {
            // The MediaInput specified must be added from current conference
            ConsoleAbstraction.WriteDarkYellow($"[{BotName}] OnStreamOpened - Media:[{Rainbow.Util.MediasToString(media)}] - Stream:[{streamId}] - StillUsed:[{stillUsed}]", logger: log);

            if (_currentCall?.IsActive() == true)
            {
                if (_currentCall.IsConference == true)
                    StartTaskAddOrRemoveMediaForConference();
                else
                    StartTaskAddOrRemoveMediaForP2P();
            }
        }

        private void StreamManager_OnStreamDisposing(string streamId)
        {
            // Conference
            if (_conferenceStatus.AudioStreamTrack?.Id == streamId)
            {
                ConsoleAbstraction.WriteWhite($"[{BotName}] OnStreamDisposing (Conference - Audio context) - Stream:[{streamId}]", logger: log);
                _conferenceStatus.AudioStreamTrack?.Dispose();
            }

            if (_conferenceStatus.VideoStreamTrack?.Id == streamId)
            {
                ConsoleAbstraction.WriteWhite($"[{BotName}] OnStreamDisposing (Conference - Video context) - Stream:[{streamId}]", logger: log);
                _conferenceStatus.VideoStreamTrack?.Dispose();
            }

            if (_conferenceStatus.SharingStreamTrack?.Id == streamId)
            {
                ConsoleAbstraction.WriteWhite($"[{BotName}] OnStreamDisposing (Conference - Sharing context) - Stream:[{streamId}]", logger: log);
                _conferenceStatus.SharingStreamTrack?.Dispose();
            }

            // P2P
            if (_p2pStatus.AudioStreamTrack?.Id == streamId)
            {
                ConsoleAbstraction.WriteWhite($"[{BotName}] OnStreamDisposing (P2P - Audio context) - Stream:[{streamId}]", logger: log);
                _p2pStatus.AudioStreamTrack?.Dispose();
            }

            if (_p2pStatus.VideoStreamTrack?.Id == streamId)
            {
                ConsoleAbstraction.WriteWhite($"[{BotName}] OnStreamDisposing (P2P - Video context) - Stream:[{streamId}]", logger: log);
                _p2pStatus.VideoStreamTrack?.Dispose();
            }

            if (_p2pStatus.SharingStreamTrack?.Id == streamId)
            {
                ConsoleAbstraction.WriteWhite($"[{BotName}] OnStreamDisposing (P2P - Sharing context) - Stream:[{streamId}]", logger: log);
                _p2pStatus.SharingStreamTrack?.Dispose();
            }
        }

#endregion Events triggered by StreamManager


#region Events triggered by Rainbow SDK

        private async void RbConferences_ConferenceRemoved(Rainbow.Model.Conference conference)
            => RbConferences_ConferenceUpdated(conference);

        private async void RbConferences_ConferenceUpdated(Rainbow.Model.Conference conference)
        {
            if (conference is null) return;

            if (conference.Active)
            {
                if (!_conferences.Contains(conference.Peer.Id))
                {
                    _conferences.Add(conference.Peer.Id);
                    ConsoleAbstraction.WriteBlue($"[{BotName}] [ConferenceUpdated] A conference is active - Id:[{conference.Peer.Id}]", logger: log);

                    if ((_currentCall is null) || (_currentCall.IsConference == true))
                    {
                        _conferencesUpdated = true;
                        StartTaskCheckConferencesAndMedias();
                    }
                }
            }
            else
            {
                if (_conferences.Remove(conference.Peer.Id))
                {
                    ConsoleAbstraction.WriteBlue($"[{BotName}] [ConferenceUpdated] A conference is NO MORE active - Id:[{conference.Peer.Id}]", logger: log);

                    if (_currentCall?.IsConference == true)
                    {
                        _conferencesUpdated = true;
                        StartTaskCheckConferencesAndMedias();
                    }
                }
            }
        }

        private async void RbWebRTCCommunications_CallUpdated(Call? call)
        {
            // /!\ This method is used with call == null when there is no more conference / p2p call to manage ...
            if ( (call is null) && (_currentCall is not null))
            {
                if (_currentCall.IsConference)
                {
                    _currentCall = null;
                    PostPoneCancelableDelayToCheckConferencesAndMedias();
                }
                else
                {
                    _currentCall = null;
                    PostPoneCancelableDelayToCheckP2PAndMedias();
                }

                UpdatePresence();
                return;
            }

            if (String.IsNullOrEmpty(call?.Id))
                return;

            if (_currentCall is null)
            {
                _currentCall = call;

                // Manage Conference
                if (_currentCall.IsConference)
                {
                    _conferencesUpdated = true;
                    StartTaskCheckConferencesAndMedias();
                }
                // Manage P2P
                else
                {
                    _p2pUpdated = true;
                    StartTaskCheckP2PAndMedias();
                }
            }
            else if (_currentCall?.Id == call.Id)
            {
                if (call.Equals(_currentCall, true))
                {
                    ConsoleAbstraction.WriteRed($"[{BotName}] Same call update received, no changes - checking participants.", logger: log);
                    return;
                }
                else if (call.Equals(_currentCall, false))
                {
                    ConsoleAbstraction.WriteRed($"[{BotName}] Same call update received, no changes - without checking participants.", logger: log);
                    return;
                }

                // Store new call information
                _currentCall = call;
                if (_currentCall.IsConference)
                {
                    _conferencesUpdated = true;
                    StartTaskCheckConferencesAndMedias();
                }
                else
                {
                    _p2pUpdated = true;
                    StartTaskCheckP2PAndMedias();
                }
            }
            else
            {
                // There is a second call in progress - we don't manage this case
                return;
            }

            UpdatePresence();

            ConsoleAbstraction.WriteBlue($"[{BotName}] [CallUpdated] {call.ToString(Rainbow.Consts.DetailsLevel.Medium)}", logger: log);
        }

#endregion Events triggered by Rainbow SDK


#region OVERRIDE METHODS OF BotBase

        public override Restrictions GetRestrictions()
        {
            var restrictions = base.GetRestrictions();

            // We need to use Conferences and WebRTC in this Bot
            restrictions.UseConferences = true;
            restrictions.UseWebRTC = true;

            return restrictions;
        }

        public override async Task ConnectedAsync()
        {
            // We must ensure the configuration is well set => set id/jid for example based on login email
            await UpdateConfigurationWithCorrectDataAsync();
        }

        public override async Task StoppedAsync(SdkError? sdkError)
        {
            // TODO - we need to hangup / close all media used
            await Task.CompletedTask;
        }

        public override async Task BotConfigurationUpdatedAsync(BotConfigurationUpdate botConfigurationUpdate)
        {
            // Ensure to have an object not null
            if (botConfigurationUpdate is null)                
                return;

            // Create WebRTC environment if necessary (only on startup so when configuration from file is set)
            if (botConfigurationUpdate.Context == "configFile")
            {
                CreateWebRTCEnvironment();
                CreateStreamManagerEnvironment();
            }

            // BotConfigurationExtended object has been created to store data structure specific for this bot
            // We try to parse JSON Node to fill this data structure and if it's correct we update the broadcast configuration
            var botConfigurationExtended = BotConfigurationExtended.FromJsonNode(botConfigurationUpdate.JSONNodeBotConfiguration);
            if (botConfigurationExtended is not null)
            {
                _configuration = botConfigurationExtended;

                await UpdateConfigurationWithCorrectDataAsync();

                await UpdateFirstAndLastName(_configuration.FirstName, _configuration.LastName);

                // On start, we inform StreamManager - perhaps some streams must be connected as soon as possible
                if (botConfigurationUpdate.Context == "configFile")
                    _streamManager?.SetNewConfiguration(botConfigurationExtended.Streams?.Values?.ToList(), null);

                _configurationUpdated = true;
                StartTaskCheckConferencesAndMedias();
                // TODO - check P2P and Medias
            }
        }

    #region Invitations - bubble or user (we do nothing special here)

        public override async Task BubbleInvitationReceivedAsync(Rainbow.Model.BubbleInvitation bubbleInvitation)
        {
            // Nothing to do here
            await Task.CompletedTask;
        }

        public override async Task UserInvitationReceivedAsync(Rainbow.Model.Invitation invitation)
        {
            // Nothing to do here
            await Task.CompletedTask;
        }

    #endregion Invitations - bubble or user

    #region Messages - AckMessage, ApplicationMessage, InstantMessage, InternalMessage  (we do nothing special here)

        public override async Task AckMessageReceivedAsync(Rainbow.Model.AckMessage ackMessage)
        {
            // Nothing to do here
            await Task.CompletedTask;
        }

        public override async Task ApplicationMessageReceivedAsync(Rainbow.Model.ApplicationMessage applicationMessage)
        {
            // Nothing to do here
            await Task.CompletedTask;
        }

        public override async Task InstantMessageReceivedAsync(Rainbow.Model.Message message)
        {
            // Nothing to do here
            await Task.CompletedTask;
        }

        public override async Task InternalMessageReceivedAsync(InternalMessage internalMessage)
        {
            // Nothing to do here
            await Task.CompletedTask;
        }

    #endregion Messages - AckMessage, ApplicationMessage, InstantMessage, InternalMessage

#endregion OVERRIDE METHODS OF BotBase

    }
}