using BotBroadcaster.Model;
using BotLibrary.Model;
using Rainbow;
using Rainbow.Example.Common;
using Rainbow.Example.CommonSDL2;
using Rainbow.Model;
using Rainbow.WebRTC;
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

        readonly StreamManager _streamManager = new(true); // StreamManager must not dispose Streams - We must do it (to release associated MediaTrack)

        // ------------------------------

        BotConfigurationExtended? _configuration = null;
        Boolean _configurationUpdated = false;

        readonly ConcurrentList<String> _conferences = [];
        Boolean _conferencesUpdated = false;

        Call? _currentCall = null;                          // Current call - updated through event WebRTCCommunications.CallUpdated
        readonly ConferenceStatus _conferenceStatus = new();// Store conference status: media to add/remove, etc ...

        Task _taskCheckConferenceAndMedia = Task.CompletedTask;
        Task _taskAddOrRemoveMedia = Task.CompletedTask;

        CancelableDelay? _cancelableDelayToCheckConfigAndConference = null;

        private void CheckConfigAndConference()
        {
            _conferencesUpdated = true;
            StartTaskCheckConferencesAndMedias();
        }

        private void PostPoneCancelableDelayToCheckConfigAndConference()
        {
            if (_cancelableDelayToCheckConfigAndConference is null)
                _cancelableDelayToCheckConfigAndConference = CancelableDelay.StartAfter(500, CheckConfigAndConference);
            else
                _cancelableDelayToCheckConfigAndConference.PostPone();
        }

        private void CreateWebRTCEnvironment()
        {
            if (_rbWebRTCDesktopFactory is null)
            {
                _rbWebRTCDesktopFactory = new();
                _rbWebRTCCommunications = WebRTCCommunications.GetOrCreateInstance(Application, _rbWebRTCDesktopFactory);

                _rbContacts = Application.GetContacts();
                _rbConferences = Application.GetConferences();
                _rbBubbles = Application.GetBubbles();

                RegisterToWebRTCEvents();
            }
        }

        private void CreateStreamManagerEnvironment()
        {
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

        private void StartTaskAddOrRemoveMedia()
        {
            if (!_taskAddOrRemoveMedia.IsCompleted)
            {
                ConsoleAbstraction.WriteYellow($"[{BotName}] Task to Add/remove media is already running => we will not start another one to avoid any conflict");
                return;
            }
            _taskAddOrRemoveMedia = Task.Run(TaskAddOrRemoveMediaAsync);
        }

        private async Task TaskAddOrRemoveMediaAsync()
        {
            if ((_rbWebRTCDesktopFactory is null) || (_rbWebRTCCommunications is null)) return;

            String? streamId;
            Rainbow.Medias.IMediaVideo? mediaVideo;
            Rainbow.Medias.IMediaVideo? mediaSharing;
            SdkResult<Boolean> sdkResult;

            if ((!String.IsNullOrEmpty(_conferenceStatus.ConferenceId))
                && (_currentCall?.IsActive() == true)
                && (_conferenceStatus.ConferenceId.Equals(_currentCall?.Id, StringComparison.InvariantCultureIgnoreCase)))
            {
                // We manage Audio first then Video and finally Sharing

                Dictionary<int, String> streamsToUse = [];

                // --------------------------------
                // --- START: CHECK AUDIO
                if (_conferenceStatus.AudioStreamTrack?.IsEmptyTrack == true)
                {
                    ConsoleAbstraction.WriteYellow($"[{BotName}] AUDIO Track is an empty track");

                    if (_conferenceStatus.Streams.TryGetValue(Rainbow.Consts.Media.AUDIO, out streamId)
                        && (streamId is not null))
                    {
                        var mediaAudio = _streamManager.GetMediaAudioFromStreamId(streamId);
                        if (mediaAudio is null) // It means we didn't ask StreamManager to manage it
                        {
                            _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _conferenceStatus.Streams);

                            PostPoneCancelableDelayToCheckConfigAndConference();
                            return;
                        }
                        else
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] AUDIO Track is available using StreamManager - Stream:[{streamId}]");

                            //var previousTrack = _conferenceStatus.AudioStreamTrack;

                            var audioTrack = _rbWebRTCDesktopFactory.CreateAudioTrack(mediaAudio);
                            if (audioTrack is not null)
                            {
                                sdkResult = await _rbWebRTCCommunications.ChangeAudioAsync(_currentCall?.Id ?? "", audioTrack);
                                if (sdkResult.Success)
                                {
                                    ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Audio has been changed (from empty track) - Stream:[{streamId}]");
                                    _conferenceStatus.AudioStreamTrack = (AudioStreamTrack)audioTrack;
                                    //previousTrack?.Dispose();
                                }
                                else
                                {
                                    // Cannot change audio ....
                                    ConsoleAbstraction.WriteRed($"[{BotName}] Cannot change Audio Track - Stream:[{streamId}] - Error: [{sdkResult.Result}]");
                                }
                            }
                            else
                            {
                                // Cannot create audio track ....
                                ConsoleAbstraction.WriteRed($"[{BotName}] Cannot create Audio Track - Stream:[{streamId}]");
                            }

                            PostPoneCancelableDelayToCheckConfigAndConference();
                            return;
                        }
                    }
                    else
                    {
                        ConsoleAbstraction.WriteYellow($"[{BotName}] No AUDIO Track to use in this conference");
                    }
                }
                else
                {
                    if (_conferenceStatus.Streams.TryGetValue(Rainbow.Consts.Media.AUDIO, out streamId) && !String.IsNullOrEmpty(streamId))
                    {
                        ConsoleAbstraction.WriteYellow($"[{BotName}] AUDIO Track is not an empty track");

                        // Audio track must be changed
                        if (streamId != _conferenceStatus.AudioStreamTrack?.Id)
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] AUDIO Track must be updated");

                            var mediaAudio = _streamManager.GetMediaAudioFromStreamId(streamId);
                            if (mediaAudio is null) // It means we didn't ask StreamManager to manage it
                            {
                                ConsoleAbstraction.WriteYellow($"[{BotName}] AUDIO Media [{streamId}] is not known from StreamManager");

                                _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _conferenceStatus.Streams);

                                PostPoneCancelableDelayToCheckConfigAndConference();
                                return;
                            }
                            else
                            {
                                ConsoleAbstraction.WriteYellow($"[{BotName}] AUDIO Media [{streamId}] is known from StreamManager");

                                //var previousTrack = _conferenceStatus.AudioStreamTrack;

                                var audioTrack = _rbWebRTCDesktopFactory.CreateAudioTrack(mediaAudio);
                                if (audioTrack is not null)
                                {
                                    sdkResult = await _rbWebRTCCommunications.ChangeAudioAsync(_currentCall?.Id ?? "", audioTrack);
                                    if (sdkResult.Success)
                                    {
                                        ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Audio has been changed (from a previous track) - Stream:[{streamId}]");
                                        _conferenceStatus.AudioStreamTrack = (AudioStreamTrack)audioTrack;
                                        //previousTrack?.Dispose();
                                    }
                                    else
                                    {
                                        // Cannot change audio ....
                                        ConsoleAbstraction.WriteRed($"[{BotName}] Cannot change audio track - Stream:[{streamId}] - Error:[{sdkResult.Result}]");
                                    }
                                }
                                else
                                {
                                    // Cannot create audio track ....
                                    ConsoleAbstraction.WriteRed($"[{BotName}] Cannot create audio track - Stream:[{streamId}]");
                                }

                                PostPoneCancelableDelayToCheckConfigAndConference();
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
                        ConsoleAbstraction.WriteYellow($"[{BotName}] AUDIO Track must be an empty track");

                        // We no more use audio
                        //var previousTrack = _conferenceStatus.AudioStreamTrack;

                        var audioTrack = _rbWebRTCDesktopFactory.CreateEmptyAudioTrack();
                        if (audioTrack is not null)
                        {
                            sdkResult = await _rbWebRTCCommunications.ChangeAudioAsync(_currentCall?.Id ?? "", audioTrack);
                            if (sdkResult.Success)
                            {
                                ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Audio has been changed (use empty track)");
                                _conferenceStatus.AudioStreamTrack = (AudioStreamTrack)audioTrack;
                                //previousTrack?.Dispose();
                            }
                            else
                            {
                                // Cannot change audio ....
                                ConsoleAbstraction.WriteRed($"[{BotName}] Cannot change audio track  (using empty one) - Error:[{sdkResult.Result}]");
                            }
                        }
                        else
                        {
                            // Cannot create audio track ....
                            ConsoleAbstraction.WriteRed($"[{BotName}] Cannot create empty audio track ");
                        }

                        _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _conferenceStatus.Streams);

                        PostPoneCancelableDelayToCheckConfigAndConference();
                        return;
                    }
                }
                // --- END: CHECK AUDIO
                // --------------------------------

                // --------------------------------
                // --- START: CHECK VIDEO
                String videoAction = "none"; // "add", "remove" "update"

                streamId = null;
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
                            //videoAction = "remove"; // We remove then we will add
                        }
                        else
                        {
                            videoAction = "add";
                            // Clear bad status
                            //_conferenceStatus.VideoStreamTrack?.Dispose();
                            _conferenceStatus.VideoStreamTrack = null;
                        }
                    }
                    else
                    {
                        if (!Rainbow.Util.MediasWithVideo(_currentCall?.LocalMedias ?? 0))
                        {
                            videoAction = "add";
                            // Clear bad status
                            //_conferenceStatus.VideoStreamTrack?.Dispose();
                            _conferenceStatus.VideoStreamTrack = null;
                        }
                    }
                }
                ConsoleAbstraction.WriteYellow($"[{BotName}] VIDEO Track action:[{videoAction}] - Stream:[{streamId}]");

                switch (videoAction)
                {
                    case "add":
                        if (_currentCall?.IsActive() != true)
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] VIDEO Track must be added - Stream:[{streamId}] but call is not active - we do it later");
                        }
                        else
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] VIDEO Track must be added - Stream:[{streamId}]");
                            mediaVideo = _streamManager.GetMediaVideoFromStreamId(streamId);
                            if (mediaVideo is null) // It means we didn't ask StreamManager to manage it
                            {
                                ConsoleAbstraction.WriteYellow($"[{BotName}] VIDEO Media [{streamId}] is not known from StreamManager");
                                _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _conferenceStatus.Streams);
                            }
                            else
                            {
                                ConsoleAbstraction.WriteYellow($"[{BotName}] VIDEO Media is known from StreamManager - Stream:[{streamId}]");
                                var videoTrack = _rbWebRTCDesktopFactory.CreateVideoTrack(mediaVideo);
                                if (videoTrack is not null)
                                {
                                    _conferenceStatus.VideoStreamTrack = (VideoStreamTrack)videoTrack;
                                    sdkResult = await _rbWebRTCCommunications.AddVideoAsync(_currentCall?.Id ?? "", videoTrack);
                                    if (sdkResult.Success)
                                    {
                                        ConsoleAbstraction.WriteDarkYellow($"[{BotName}] VIDEO has been added - Stream:[{streamId}]");
                                    }
                                    else
                                    {
                                        _conferenceStatus.VideoStreamTrack = null;
                                        // Cannot change VIDEO ....
                                        ConsoleAbstraction.WriteRed($"[{BotName}] Cannot add VIDEO track - Stream:[{streamId}] - Error:[{sdkResult.Result}]");
                                    }
                                }
                                else
                                {
                                    // Cannot create VIDEO track ....
                                    ConsoleAbstraction.WriteRed($"[{BotName}] Cannot create VIDEO track - Stream:[{streamId}]");
                                }
                            }
                        }

                        PostPoneCancelableDelayToCheckConfigAndConference();
                        return;

                    case "update":
                        ConsoleAbstraction.WriteYellow($"[{BotName}] VIDEO Track must be updated - Stream:[{streamId}]");

                        mediaVideo = _streamManager.GetMediaVideoFromStreamId(streamId);
                        if (mediaVideo is null) // It means we didn't ask StreamManager to manage it
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] VIDEO Media [{streamId}] is not known from StreamManager");
                            _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _conferenceStatus.Streams);
                        }
                        else
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] VIDEO Media is known from StreamManager - Stream:[{streamId}]");
                            var videoTrack = _rbWebRTCDesktopFactory.CreateVideoTrack(mediaVideo);
                            if (videoTrack is not null)
                            {
                                _conferenceStatus.VideoStreamTrack = (VideoStreamTrack)videoTrack;
                                sdkResult = await _rbWebRTCCommunications.ChangeVideoAsync(_currentCall?.Id ?? "", videoTrack);
                                if (sdkResult.Success)
                                {
                                    ConsoleAbstraction.WriteDarkYellow($"[{BotName}] VIDEO has been changed - Stream:[{streamId}]");
                                }
                                else
                                {
                                    _conferenceStatus.VideoStreamTrack = null;
                                    // Cannot change VIDEO ....
                                    ConsoleAbstraction.WriteRed($"[{BotName}] Cannot changed VIDEO track - Stream:[{streamId}] - Error:[{sdkResult.Result}]");
                                }
                            }
                            else
                            {
                                // Cannot create VIDEO track ....
                                ConsoleAbstraction.WriteRed($"[{BotName}] Cannot create VIDEO track - Stream:[{streamId}]");
                            }
                        }
                        PostPoneCancelableDelayToCheckConfigAndConference();
                        return;

                    case "remove":
                        // We no more use VIDEO
                        ConsoleAbstraction.WriteYellow($"[{BotName}] VIDEO Track must be removed");

                        //var previousTrack = _conferenceStatus.VideoStreamTrack;
                        sdkResult = await _rbWebRTCCommunications.RemoveVideoAsync(_currentCall?.Id ?? "");
                        if (sdkResult.Success)
                        {
                            ConsoleAbstraction.WriteDarkYellow($"[{BotName}] VIDEO has been removed");
                            _conferenceStatus.VideoStreamTrack = null;
                            //previousTrack?.Dispose();
                        }
                        else
                        {
                            // Cannot change VIDEO ....
                            ConsoleAbstraction.WriteRed($"[{BotName}] Cannot remove VIDEO track - Error:[{sdkResult.Result}]");
                        }

                        _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _conferenceStatus.Streams);
                        PostPoneCancelableDelayToCheckConfigAndConference();
                        return;
                }
                // --- END: CHECK VIDEO
                // --------------------------------


                // --------------------------------
                // --- START: CHECK SHARING
                String sharingAction = "none"; // "add", "remove" "update"

                streamId = null;
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
                ConsoleAbstraction.WriteYellow($"[{BotName}] SHARING Track action:[{sharingAction}] - Stream:[{streamId}]");
                switch (sharingAction)
                {
                    case "add":

                        if (_currentCall?.IsActive() != true)
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] SHARING Track must be added - Stream:[{streamId}] but call is not active - we do it later");
                        }
                        else
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] SHARING Track must be added - Stream:[{streamId}]");
                            mediaSharing = _streamManager.GetMediaVideoFromStreamId(streamId);
                            if (mediaSharing is null) // It means we didn't ask StreamManager to manage it
                            {
                                ConsoleAbstraction.WriteYellow($"[{BotName}] SHARING Media [{streamId}] is not known from StreamManager");
                                _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _conferenceStatus.Streams);
                            }
                            else
                            {
                                ConsoleAbstraction.WriteYellow($"[{BotName}] SHARING Media is known from StreamManager - Stream:[{streamId}]");
                                var sharingTrack = _rbWebRTCDesktopFactory.CreateVideoTrack(mediaSharing);
                                if (sharingTrack is not null)
                                {
                                    _conferenceStatus.SharingStreamTrack = (VideoStreamTrack)sharingTrack;
                                    sdkResult = await _rbWebRTCCommunications.AddSharingAsync(_currentCall?.Id ?? "", sharingTrack);
                                    if (sdkResult.Success)
                                    {
                                        ConsoleAbstraction.WriteDarkYellow($"[{BotName}] SHARING has been added - Stream:[{streamId}]");
                                    }
                                    else
                                    {
                                        _conferenceStatus.SharingStreamTrack = null;
                                        // Cannot change SHARING ....
                                        ConsoleAbstraction.WriteRed($"[{BotName}] Cannot add SHARING track - Stream:[{streamId}] - Error:[{sdkResult.Result}]");
                                    }
                                }
                                else
                                {
                                    // Cannot create SHARING track ....
                                    ConsoleAbstraction.WriteRed($"[{BotName}] Cannot create SHARING track - Stream:[{streamId}]");
                                }
                            }
                        }

                        PostPoneCancelableDelayToCheckConfigAndConference();
                        return;

                    case "update":
                        ConsoleAbstraction.WriteYellow($"[{BotName}] SHARING Track must be updated - Stream:[{streamId}]");

                        mediaSharing = _streamManager.GetMediaVideoFromStreamId(streamId);
                        if (mediaSharing is null) // It means we didn't ask StreamManager to manage it
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] SHARING Media [{streamId}] is not known from StreamManager");
                            _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _conferenceStatus.Streams);
                        }
                        else
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] SHARING Media is known from StreamManager - Stream:[{streamId}]");
                            var sharingTrack = _rbWebRTCDesktopFactory.CreateVideoTrack(mediaSharing);
                            if (sharingTrack is not null)
                            {
                                _conferenceStatus.SharingStreamTrack = (VideoStreamTrack)sharingTrack;
                                sdkResult = await _rbWebRTCCommunications.ChangeSharingAsync(_currentCall?.Id ?? "", sharingTrack);
                                if (sdkResult.Success)
                                {
                                    ConsoleAbstraction.WriteDarkYellow($"[{BotName}] SHARING has been changed - Stream:[{streamId}]");
                                }
                                else
                                {
                                    _conferenceStatus.SharingStreamTrack = null;
                                    // Cannot change SHARING ....
                                    ConsoleAbstraction.WriteRed($"[{BotName}] Cannot changed SHARING track - Stream:[{streamId}] - Error:[{sdkResult.Result}]");
                                }
                            }
                            else
                            {
                                // Cannot create SHARING track ....
                                ConsoleAbstraction.WriteRed($"[{BotName}] Cannot create SHARING track - Stream:[{streamId}]");
                            }
                        }
                        PostPoneCancelableDelayToCheckConfigAndConference();
                        return;

                    case "remove":
                        // We no more use SHARING
                        ConsoleAbstraction.WriteYellow($"[{BotName}] SHARING Track must be removed");

                        //var previousTrack = _conferenceStatus.SharingStreamTrack;
                        sdkResult = await _rbWebRTCCommunications.RemoveSharingAsync(_currentCall?.Id ?? "");
                        if (sdkResult.Success)
                        {
                            ConsoleAbstraction.WriteDarkYellow($"[{BotName}] SHARING has been removed");
                            _conferenceStatus.SharingStreamTrack = null;
                            //previousTrack?.Dispose();
                        }
                        else
                        {
                            // Cannot change sharing ....
                            ConsoleAbstraction.WriteRed($"[{BotName}] Cannot remove SHARING track - Error:[{sdkResult.Result}]");
                        }

                        _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _conferenceStatus.Streams);

                        PostPoneCancelableDelayToCheckConfigAndConference();
                        return;
                }
                // --- END: CHECK SHARING
                // --------------------------------


                //PostPoneCancelableDelayToCheckConfigAndConference();
            }
        }

        private void StartTaskCheckConferencesAndMedias()
        {
            if (!_taskCheckConferenceAndMedia.IsCompleted)
            {
                ConsoleAbstraction.WriteYellow($"[{BotName}] Task to check conferences and medias is already running => we will not start another one to avoid any conflict - New configuration will be taken into account at the end of current task");
                return;
            }
            _taskCheckConferenceAndMedia = Task.Run(TaskCheckConferencesAndMediasAsync);
        }

        private async Task TaskCheckConferencesAndMediasAsync()
        {
            // Do we have a configuration update or a conference update to process
            if ( (!_conferencesUpdated) && (!_configurationUpdated)) return;

            if ( (_rbWebRTCDesktopFactory is null) || (_rbWebRTCCommunications is null)) return;

            // We must indicates that we have taken into account the current configuration / conferences status to avoid to start again this task at the end of current one because of the same configuration update
            _configurationUpdated = false;
            _conferencesUpdated = false;

            // Ensure to set id, jid and name for conference if it's not the case
            UpdateConferenceSettings(_configuration);

            // Create a copy of configuration information to work with - we must not work with _configuration object
            //var streams = (_configuration?.Streams is null) ? [] : new Dictionary<String, Stream>(_configuration.Streams);
            var conferencesInConfig = (_configuration?.Conferences is null) ? [] : new List<Model.Conference>(_configuration.Conferences);

            if (String.IsNullOrEmpty(_conferenceStatus.ConferenceId)) // We are NOT currently managing a conference
            {
                ConsoleAbstraction.WriteWhite($"[{BotName}] Not currently managing a conference");

                if (_currentCall is not null) // A call is in progress ... This case should not happened
                {
                    // Something to do here ?
                    ConsoleAbstraction.WriteWhite($"[{BotName}] A current call is in progress ... Strange");
                }
                else if (_conferences.Count > 0)
                {
                    // Take first conference Id in configuration which is active
                    foreach (var conferenceInConfig in conferencesInConfig)
                    {
                        if (_conferences.Contains(conferenceInConfig.Id))
                        {
                            ConsoleAbstraction.WriteWhite($"[{BotName}] We will join this conference:[{conferenceInConfig.Id}]");

                            _conferenceStatus.ConferenceId = conferenceInConfig.Id;

                            // Here we must set Streams as CONNECTED which are used as Audio / Video / Sharing (take into account composition)
                            // The goal is too avoid to close a media if we switch it for Video to Sharing for example

                            // Store media to use for this conference
                            _conferenceStatus.Streams.Clear();

                            if (!String.IsNullOrEmpty(conferenceInConfig.AudioStreamId))
                            {
                                ConsoleAbstraction.WriteWhite($"[{BotName}] AUDIO to use - Stream:[{conferenceInConfig.AudioStreamId}]");
                                _conferenceStatus.Streams[Rainbow.Consts.Media.AUDIO] = conferenceInConfig.AudioStreamId;
                            }

                            if (!String.IsNullOrEmpty(conferenceInConfig.VideoStreamId))
                            {
                                ConsoleAbstraction.WriteWhite($"[{BotName}] VIDEO to use - Stream:[{conferenceInConfig.VideoStreamId}]");
                                _conferenceStatus.Streams[Rainbow.Consts.Media.VIDEO] = conferenceInConfig.VideoStreamId;
                            }
                            if (!String.IsNullOrEmpty(conferenceInConfig.SharingStreamId))
                            {
                                ConsoleAbstraction.WriteWhite($"[{BotName}] SHARING to use - Stream:[{conferenceInConfig.SharingStreamId}]");
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
                            ConsoleAbstraction.WriteWhite($"[{BotName}] Create AUDIO TRACK - Stream:[{audioStreamId}]");
                            _conferenceStatus.AudioStreamTrack = (AudioStreamTrack?)_rbWebRTCDesktopFactory.CreateAudioTrack(audioMedia);
                        }
                    }

                    // Create empty audio track if necessary
                    if (_conferenceStatus.AudioStreamTrack is null)
                    {
                        ConsoleAbstraction.WriteWhite($"[{BotName}] Create empty AUDIO TRACK");
                        _conferenceStatus.AudioStreamTrack = (AudioStreamTrack)_rbWebRTCDesktopFactory.CreateEmptyAudioTrack();
                    }

                    // Join the conference
                    var sdkResult = await _rbWebRTCCommunications.JoinConferenceAsync(_conferenceStatus.ConferenceId, _conferenceStatus.AudioStreamTrack);
                    if (!sdkResult.Success)
                    {
                        ConsoleAbstraction.WriteRed($"[{BotName}] Conference:[{_conferenceStatus.ConferenceId}] - Cannot join:[{sdkResult.Result}]");

                        // TODO :  Retry later ... Use a counter to avoid unlimited tentative 
                        PostPoneCancelableDelayToCheckConfigAndConference();
                    }
                    else
                    {
                        // TODO: If we use a counter (to avoid unlimited tentative to join conf.) - reset it now

                        ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Conference:[{_conferenceStatus.ConferenceId}] - Joined done with success");

                        // Inform streamManager of the new config
                        ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Ask StreamManager to use config file only - no stream to use");

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
                ConsoleAbstraction.WriteWhite($"[{BotName}] - Currently managing a conference:[{_conferenceStatus.ConferenceId}]");

                if (_currentCall is null) // The conference has been closed but we don't stopped related streams yet
                {
                    ConsoleAbstraction.WriteWhite($"[{BotName}] - Current call is null");

                    // Hang up the conference
                    await _rbWebRTCCommunications.HangUpCallAsync(_conferenceStatus.ConferenceId);

                    ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Conference:[{_conferenceStatus.ConferenceId}] - HangUp has been done");
                    _conferenceStatus.Reset();

                    // Inform streamManager of the new config
                    ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Ask StreamManager to use config file only - no stream to use");

                    _streamManager.SetNewConfiguration(_configuration?.Streams.Values.ToList(), _conferenceStatus.Streams);

                    // Check a little later the config to ensure we must no more be in the conf.
                    PostPoneCancelableDelayToCheckConfigAndConference();

                    // We need to:
                    //  - ask streamManager to update streams according configuration
                    //  - wait each stream used in this conferenc to by removed
                    //  - close current Tracksss
                }
                else
                {
                    if(_conferenceStatus.ConferenceId.Equals(_currentCall?.Id, StringComparison.InvariantCultureIgnoreCase))
                    {

                        ConsoleAbstraction.WriteWhite($"[{BotName}] - Currently managing a conference:[{_conferenceStatus.ConferenceId}]");

                        // Check if this conference is still in the config
                        var conferenceInConfig = _configuration?.Conferences?.FirstOrDefault(c => c.Id == _currentCall?.Id);
                        
                        if(conferenceInConfig is null)
                        {
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

                            ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Conference:[{_conferenceStatus.ConferenceId}] - Status:[{_currentCall?.CallStatus}] - LocalMedias:[{Rainbow.Util.MediasToString(_currentCall?.LocalMedias ?? 0)}]");

                            if (_currentCall?.IsActive() == true)
                                StartTaskAddOrRemoveMedia();
                            else if(_currentCall?.CallStatus == Rainbow.Enums.CallStatus.UNKNOWN)
                            {
                                _conferencesUpdated = true;
                                StartTaskCheckConferencesAndMedias();
                            }
                        }
                    }
                    else
                    {
                        ConsoleAbstraction.WriteRed($"[{BotName}] - Currently managing a conference:[{_conferenceStatus.ConferenceId}] but not the good one ... Current Call:[{_currentCall?.Id}]");

                        // Is this case possible ?
                        // We are managing a conference but not the good one => We need to change conference and related streams
                    }
                }
            }
        }

        private List<Stream> GetAsConnectedStreamsUsed(Dictionary<String, Stream>? streamsById, List<String>? streamsToUse)
        {
            List<Stream> result = [];

            if ( (streamsById is null) || (streamsById.Count == 0) 
                || (streamsToUse is null) || (streamsToUse.Count == 0)) return result;

            var streamsById_Copy = new Dictionary<String, Stream>(streamsById);
            var streamsToUse_Copy = new List<String>(streamsToUse);

            foreach (var streamId in streamsToUse_Copy)
            {
                if(String.IsNullOrEmpty(streamId)) continue;

                if (streamsById_Copy.TryGetValue(streamId, out var stream) && (stream is not null))
                {
                    stream.Connected = true;

                    if(stream.VideoComposition?.Count > 0)
                    {
                        foreach(var sid in stream.VideoComposition)
                        {
                            if (streamsById_Copy.TryGetValue(streamId, out stream) && (stream is not null))
                                stream.Connected = true;
                        }
                    }
                }
                else
                {
                    ConsoleAbstraction.WriteRed($"[{BotName}] Stream[{streamId}] is not know in the dictionary of Streams ...");
                    streamsToUse.Remove(streamId);
                }
            }

            return streamsById_Copy.Values.ToList();
        }

        private void UpdateConferenceSettings(BotConfigurationExtended? botConfigurationExtended)
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

        private Bubble? GetBubble(string id, string jid, string name)
        {
            if (_rbBubbles is null) return null;
            
            Bubble? result = _rbBubbles.GetBubbleById(id);
            result ??= _rbBubbles.GetBubbleByJid(jid);
            result ??= _rbBubbles.GetAllBubbles().Find(b => b.Peer.DisplayName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return result;
        }

#region Events triggered by StreamManager
        
        private void StreamManager_OnStreamRemoved(string streamId, int media, Boolean stillUsed)
        {
            // The MediaInput specified must be removed from current conference
            ConsoleAbstraction.WriteDarkYellow($"[{BotName}] OnStreamRemoved - Media:[{Rainbow.Util.MediasToString(media)}] - Stream:[{streamId}] - StillUsed:[{stillUsed}]");

            StartTaskAddOrRemoveMedia();
        }

        private void StreamManager_OnStreamOpened(string streamId, int media, Boolean stillUsed)
        {
            // The MediaInput specified must be added from current conference
            ConsoleAbstraction.WriteDarkYellow($"[{BotName}] OnStreamOpened - Media:[{Rainbow.Util.MediasToString(media)}] - Stream:[{streamId}] - StillUsed:[{stillUsed}]");

            StartTaskAddOrRemoveMedia();
        }

        private void StreamManager_OnStreamDisposing(string streamId)
        {
            if (_conferenceStatus.AudioStreamTrack?.Id == streamId)
            {
                ConsoleAbstraction.WriteWhite($"[{BotName}] OnStreamDisposing (Audio context) - Stream:[{streamId}]");
                _conferenceStatus.AudioStreamTrack?.Dispose();
            }

            if (_conferenceStatus.VideoStreamTrack?.Id == streamId)
            {
                ConsoleAbstraction.WriteWhite($"[{BotName}] OnStreamDisposing (Video context) - Stream:[{streamId}]");
                _conferenceStatus.VideoStreamTrack?.Dispose();
            }

            if (_conferenceStatus.SharingStreamTrack?.Id == streamId)
            {
                ConsoleAbstraction.WriteWhite($"[{BotName}] OnStreamDisposing (Sharing context) - Stream:[{streamId}]");
                _conferenceStatus.SharingStreamTrack?.Dispose();
            }
        }

#endregion Events triggered by StreamManager


#region Events triggered by Rainbow SDK

        private async void RbConferences_ConferenceRemoved(Rainbow.Model.Conference conference)
        {
            RbConferences_ConferenceUpdated(conference);
        }

        private async void RbConferences_ConferenceUpdated(Rainbow.Model.Conference conference)
        {
            if (conference.Active)
            {
                if (!_conferences.Contains(conference.Peer.Id))
                {
                    _conferences.Add(conference.Peer.Id);
                    ConsoleAbstraction.WriteBlue($"[{BotName}] [ConferenceUpdated] A conference is active - Id:[{conference.Peer.Id}]");

                    _conferencesUpdated = true;
                    StartTaskCheckConferencesAndMedias();
                }
            }
            else
            {

                if (_conferences.Remove(conference.Peer.Id))
                {
                    ConsoleAbstraction.WriteBlue($"[{BotName}] [ConferenceUpdated] A conference is NO MORE active - Id:[{conference.Peer.Id}]");

                    _conferencesUpdated = true;
                    StartTaskCheckConferencesAndMedias();
                }
            }
        }

        private async void RbWebRTCCommunications_CallUpdated(Call? call)
        {
            if ((call is null) && (_currentCall is not null))
            {
                _currentCall = call;

                _conferencesUpdated = true;
                StartTaskCheckConferencesAndMedias();
                return;
            }
            else
            {
                if ((call is null) || String.IsNullOrEmpty(call.Id))
                    return;
            } 

            if (_currentCall is null)
            {
                _currentCall = call;

                _conferencesUpdated = true;
                StartTaskCheckConferencesAndMedias();
            }
            else if (_currentCall?.Id == call.Id)
            {
                if (call.Equals(_currentCall, true))
                {
                    ConsoleAbstraction.WriteRed($"[{BotName}] Same call update received, no changes - checking participants.");
                    return;
                }
                else if (call.Equals(_currentCall, false))
                {
                    ConsoleAbstraction.WriteRed($"[{BotName}] Same call update received, no changes - without checking participants.");
                    return;
                }

                // Store new call information
                _currentCall = call;
                _conferencesUpdated = true;
                StartTaskCheckConferencesAndMedias();
            }
            else
                return;

            if (! (_currentCall?.IsInProgress() == true))
            {
                // The call is NO MORE in Progress => We Rollback presence if any
                var _1 = _rbContacts?.RollbackPresenceSavedAsync();
                _currentCall = null;
            }
            else
            {
                // Add / Update this call
                if (!(_currentCall?.IsRinging() == true))
                {
                    // The call is NOT IN RINGING STATE  => We update presence according media
                    var _1 = _rbContacts?.SetBusyPresenceAccordingMediasAsync(call.LocalMedias);
                }
            }

            ConsoleAbstraction.WriteBlue($"[{BotName}] [CallUpdated] {call.ToString(Rainbow.Consts.DetailsLevel.Medium)}");
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
            // Nothing to do here
            await Task.CompletedTask;
        }

        public override async Task StoppedAsync(SdkError sdkError)
        {
            // TODO - we need to hangup / close all media used
            await Task.CompletedTask;
        }

        public override async Task BotConfigurationUpdatedAsync(BotConfigurationUpdate botConfigurationUpdate)
        {
            // Ensure to have an object not null
            if (botConfigurationUpdate is null)
                return;

            // Create WebRTC environment if necessary
            if (botConfigurationUpdate.Context == "configFile")
            {
                CreateWebRTCEnvironment();
                CreateStreamManagerEnvironment();
            }

            // BotConfigurationExtended object has been created to store data structure specific for this bot
            // We try to parse JSON Node to fill this data structure and if it's correct we update the broadcast configuration
            if (BotConfigurationExtended.FromJsonNode(botConfigurationUpdate.JSONNodeBotConfiguration, out BotConfigurationExtended botConfigurationExtended))
            {
                _configuration = botConfigurationExtended;

                // On start / reload, we inform StreamManager - perhaps some streams must be connected as soon as possible
                if (botConfigurationUpdate.Context == "configFile")
                    _streamManager.SetNewConfiguration(botConfigurationExtended.Streams?.Values?.ToList(), null);

                _configurationUpdated = true;
                StartTaskCheckConferencesAndMedias();
            }
        }

    #region Invitations - bubble or user
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

    #region Messages - AckMessage, ApplicationMessage, InstantMessage, InternalMessage
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