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

        readonly StreamManager _streamManager = new();

        // ------------------------------

        BotConfigurationExtended? _configuration = null;
        Boolean _configurationUpdated = false;

        readonly ConcurrentList<String> _conferences = [];
        Boolean _conferencesUpdated = false;

        Call? _currentCall = null;                  // Current call - updated through event WebRTCCommunications.CallUpdated
        readonly ConferenceStatus _conferenceStatus = new(); // Store conference status: media to add/added, etc ...

        Task _taskCheckConferenceAndMedia = Task.CompletedTask;
        Task _taskAddOrRemoveMedia = Task.CompletedTask;

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
            _streamManager.OnStreamClosed += StreamManager_OnStreamClosed;

            _streamManager.OnAudioError += StreamManager_OnAudioError;
            _streamManager.OnVideoError += StreamManager_OnVideoError;
            _streamManager.OnSharingError += StreamManager_OnSharingError;
        }

        private void RegisterToWebRTCEvents()
        {
            _rbWebRTCCommunications?.CallUpdated += RbWebRTCCommunications_CallUpdated;

            _rbConferences?.ConferenceUpdated += RbConferences_ConferenceUpdated;
            _rbConferences?.ConferenceRemoved += RbConferences_ConferenceRemoved;
        }

        private void StartTaskAddOrRemoveMedia()
        {
            if (_taskAddOrRemoveMedia.Status == TaskStatus.Running)
            {
                ConsoleAbstraction.WriteYellow($"[{BotName}] Task to Add/remove media is already running => we will not start another one to avoid any conflict");
                return;
            }
            _taskAddOrRemoveMedia = Task.Run(TaskAddOrRemoveMediaAsync);
        }

        private async Task TaskAddOrRemoveMediaAsync()
        {
            if ((_rbWebRTCDesktopFactory is null) || (_rbWebRTCCommunications is null)) return;

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

                    if (_conferenceStatus.Streams.TryGetValue(Rainbow.Consts.Media.AUDIO, out var streamId)
                        && (streamId is not null))
                    {
                        var mediaAudio = _streamManager.GetMediaAudioFromStreamId(streamId);
                        if (mediaAudio is null) // It means we didntt ask StreamManager to manage it
                        {
                            ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Ask StreamManager to manage AUDIO - Stream:[{streamId}]");
                            _streamManager.UpdateOneStreamToUse(Rainbow.Consts.Media.AUDIO,  streamId);
                            return;
                        }
                        else
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] AUDIO Track is available using StreamManager - Stream:[{streamId}]");

                            var previousTrack = _conferenceStatus.AudioStreamTrack;

                            var audioTrack = _rbWebRTCDesktopFactory.CreateAudioTrack(mediaAudio);
                            if (audioTrack is not null)
                            {
                                var sdkResult = await _rbWebRTCCommunications.ChangeAudioAsync(_currentCall.Id, audioTrack);
                                if (sdkResult.Success)
                                {
                                    ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Audio has been changed (from empty track) - Stream:[{streamId}]");
                                    _conferenceStatus.AudioStreamTrack = (AudioStreamTrack)audioTrack;
                                    previousTrack?.Dispose();
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
                    if (_conferenceStatus.Streams.TryGetValue(Rainbow.Consts.Media.AUDIO, out var streamId))
                    {
                        ConsoleAbstraction.WriteYellow($"[{BotName}] AUDIO Track is not an empty track");

                        // Audio track must be changed
                        if(streamId != _conferenceStatus.AudioStreamTrack?.Id)
                        {
                            ConsoleAbstraction.WriteYellow($"[{BotName}] AUDIO Track must be updated");

                            var mediaAudio = _streamManager.GetMediaAudioFromStreamId(streamId);
                            if (mediaAudio is null) // It means we didn't ask StreamManager to manage it
                            {
                                ConsoleAbstraction.WriteYellow($"[{BotName}] AUDIO Media [{streamId}] is not known from StreamManager");

                                ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Ask StreamManager to manage AUDIO Stream:[{streamId}]");
                                _streamManager.UpdateOneStreamToUse(Rainbow.Consts.Media.AUDIO, streamId);
                                return;
                            }
                            else
                            {
                                ConsoleAbstraction.WriteYellow($"[{BotName}] AUDIO Media [{streamId}] is known from StreamManager");

                                var previousTrack = _conferenceStatus.AudioStreamTrack;

                                var audioTrack = _rbWebRTCDesktopFactory.CreateAudioTrack(mediaAudio);
                                if (audioTrack is not null)
                                {
                                    var sdkResult = await _rbWebRTCCommunications.ChangeAudioAsync(_currentCall.Id, audioTrack);
                                    if (sdkResult.Success)
                                    {
                                        ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Audio has been changed (from a previous track) - Stream:[{streamId}]");
                                        _conferenceStatus.AudioStreamTrack = (AudioStreamTrack)audioTrack;
                                        previousTrack?.Dispose();
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
                        var previousTrack = _conferenceStatus.AudioStreamTrack;

                        var audioTrack = _rbWebRTCDesktopFactory.CreateEmptyAudioTrack();
                        if (audioTrack is not null)
                        {
                            var sdkResult = await _rbWebRTCCommunications.ChangeAudioAsync(_currentCall.Id, audioTrack);
                            if (sdkResult.Success)
                            {
                                ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Audio has been changed (use empty track)");
                                _conferenceStatus.AudioStreamTrack = (AudioStreamTrack)audioTrack;
                                previousTrack?.Dispose();
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

                        ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Ask StreamManager to no more manage AUDIO Stream");
                        _streamManager.UpdateOneStreamToUse(Rainbow.Consts.Media.AUDIO, null);
                        return;
                    }
                }
                // --- END: CHECK AUDIO
                // --------------------------------
            }
        }

        private void StartTaskCheckConferencesAndMedias()
        {
            if (_taskCheckConferenceAndMedia.Status == TaskStatus.Running)
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
            var streams = (_configuration?.Streams is null) ? [] : new Dictionary<String, Stream>(_configuration.Streams);
            var conferencesInConfig = (_configuration?.Conferences is null) ? [] : new List<Model.Conference>(_configuration.Conferences);

            if (String.IsNullOrEmpty(_conferenceStatus.ConferenceId)) // We are NOT currently managing a conference
            {
                if (_currentCall is not null) // A call is in progress ... This case should not happened
                {
                    // Something to do here ?
                }
                else if (_conferences.Count > 0)
                {
                    // Take first conference Id in configuration which is active
                    foreach (var conferenceInConfig in conferencesInConfig)
                    {
                        if (_conferences.Contains(conferenceInConfig.Id))
                        {
                            _conferenceStatus.ConferenceId = conferenceInConfig.Id;

                            // Store media to use for this conference
                            _conferenceStatus.Streams.Clear();
                            if (!String.IsNullOrEmpty(conferenceInConfig.AudioStreamId))
                                _conferenceStatus.Streams[Rainbow.Consts.Media.AUDIO] = conferenceInConfig.AudioStreamId;
                            if (!String.IsNullOrEmpty(conferenceInConfig.VideoStreamId))
                                _conferenceStatus.Streams[Rainbow.Consts.Media.VIDEO] = conferenceInConfig.VideoStreamId;
                            if (!String.IsNullOrEmpty(conferenceInConfig.SharingStreamId))
                                _conferenceStatus.Streams[Rainbow.Consts.Media.SHARING] = conferenceInConfig.SharingStreamId;
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
                            _conferenceStatus.AudioStreamTrack = (AudioStreamTrack?)_rbWebRTCDesktopFactory.CreateAudioTrack(audioMedia);
                    }

                    // Create empty audio track if necessary
                    _conferenceStatus.AudioStreamTrack ??= (AudioStreamTrack)_rbWebRTCDesktopFactory.CreateEmptyAudioTrack();

                    // Join the conference
                    var sdkResult = await _rbWebRTCCommunications.JoinConferenceAsync(_conferenceStatus.ConferenceId, _conferenceStatus.AudioStreamTrack);
                    if (!sdkResult.Success)
                    {
                        // Retry later ... Use a counter to avoid unlimited tentative 

                        _conferenceStatus.ConferenceId = null;
                        CancelableDelay.StartAfter(500, TaskCheckConferencesAndMediasAsync);

                        ConsoleAbstraction.WriteRed($"[{BotName}] Conference:[{_conferenceStatus.ConferenceId}] - Cannot join:[{sdkResult.Result}]");
                    }
                    else
                    {
                        // If we use a counter (to avoid unlimited tentative to join conf.) - reset it now


                        ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Conference:[{_conferenceStatus.ConferenceId}] - Joined done with success");

                        // Inform streamManager of the new config
                        ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Ask StreamManager to use config file only - no stream to use");
                        _streamManager.SetNewConfiguration(streams.Values.ToList(), null);
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
                if(_currentCall is null) // The conference has been closed but we don't stopped related streams yet
                {
                    // Hang up the conference
                    await _rbWebRTCCommunications.HangUpCallAsync(_conferenceStatus.ConferenceId);

                    ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Conference:[{_conferenceStatus.ConferenceId}] - HangUp has been done");
                    _conferenceStatus.Reset();

                    // Inform streamManager of the new config
                    ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Ask StreamManager to use config file only - no stream to use");
                    _streamManager.SetNewConfiguration(streams.Values.ToList(), null);


                    // Check a little later the config to ensure we must no more be in the conf.
                    CancelableDelay.StartAfter(500, () =>
                    {
                        _conferencesUpdated = true;
                        StartTaskCheckConferencesAndMedias();
                    });


                    // We need to:
                    //  - ask streamManager to update streams according configuration
                    //  - wait each stream used in this conferenc to by removed
                    //  - close current Tracksss
                }
                else
                {
                    if(_conferenceStatus.ConferenceId.Equals(_currentCall.Id, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Check if this conference is still in the config
                        var conferenceInConfig = _configuration?.Conferences?.FirstOrDefault(c => c.Id == _currentCall.Id);
                        
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

                            ConsoleAbstraction.WriteDarkYellow($"[{BotName}] Conference:[{_conferenceStatus.ConferenceId}] - Status:[{_currentCall.CallStatus}] - LocalMedias:[{Rainbow.Util.MediasToString(_currentCall.LocalMedias)}]");

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
                        // Is this case possible ?
                        // We are managing a conference but not the good one => We need to change conference and related streams
                    }
                }
            }
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
        
        private void StreamManager_OnStreamClosed(string streamId, int media)
        {
            // The MediaInput specified must be removed from current conference
            ConsoleAbstraction.WriteDarkYellow($"[{BotName}] OnStreamClosed - Media:[{Rainbow.Util.MediasToString(media)}] - Stream:[{streamId}]");

            StartTaskAddOrRemoveMedia();
        }

        private void StreamManager_OnStreamOpened(string streamId, int media)
        {
            // The MediaInput specified must be added from current conference
            ConsoleAbstraction.WriteDarkYellow($"[{BotName}] OnStreamOpened - Media:[{Rainbow.Util.MediasToString(media)}] - Stream:[{streamId}]");

            StartTaskAddOrRemoveMedia();
        }
        
        private void StreamManager_OnSharingError(string mediaId, string message)
        {
            // The MediaInput used for Sharing has stoppped ...
        }

        private void StreamManager_OnVideoError(string mediaId, string message)
        {
            // The MediaInput used for Video has stoppped ...
        }

        private void StreamManager_OnAudioError(string mediaId, string message)
        {
            // The MediaInput used for Audio has stoppped ...
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
            else if (_currentCall.Id == call.Id)
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

            if (!_currentCall.IsInProgress())
            {
                // The call is NO MORE in Progress => We Rollback presence if any
                var _1 = _rbContacts?.RollbackPresenceSavedAsync();
                _currentCall = null;
            }
            else
            {
                // Add / Update this call
                if (!_currentCall.IsRinging())
                {
                    // The call is NOT IN RINGING STATE  => We update presence according media
                    var _1 = _rbContacts?.SetBusyPresenceAccordingMediasAsync(call.LocalMedias);

                    if (_currentCall.IsActive())
                    {
                        if (Rainbow.Util.MediasWithAudio(_currentCall.LocalMedias))
                        {
                        }
                        else if (Rainbow.Util.MediasWithVideo(_currentCall.LocalMedias))
                        {
                        }
                        else if (Rainbow.Util.MediasWithSharing(_currentCall.LocalMedias))
                        {
                        }
                        // TODO - needs to use _expectingAudioTrack / _expectingVideoTrack / _expectingSharingTrack to check if we are waiting for a media update and if the current update is corresponding to what we are waiting for
                    }
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
            // TODO - we need to close all media used
            // Here we do nothing special
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
    }
    #endregion Messages - AckMessage, ApplicationMessage, InstantMessage, InternalMessage

#endregion OVERRIDE METHODS OF BotBase
}