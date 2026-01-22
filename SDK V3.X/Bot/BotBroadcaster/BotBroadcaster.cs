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
using System.Threading;
using System.Threading.Tasks;
using Util = Rainbow.Example.Common.Util;

namespace BotBroadcaster
{
    public class BotBroadcaster: BotLibrary.BotBase
    {
        Task RbTask = Task.CompletedTask;

        Conferences? _rbConferences = null;
        Contacts? _rbContacts = null;
        Bubbles? _rbBubbles = null;

        WebRTCCommunications? _rbWebRTCCommunications = null;
        WebRTCFactory? _rbWebRTCDesktopFactory = null;

        Object _lockBotConfiguration = new();
        
        BotConfigurationUpdate? _currentBotConfigurationUpdate = null;
        BotConfigurationExtended? _currentBotConfigurationExtended = null;
        BotConfigurationUpdate? _nextBotConfigurationUpdate = null;

        Timer? _botConfigurationTimer = null;

        List<String> conferencesInProgress = [];

        Dictionary<string, Call> currentCallsList = new();

        StreamManager _streamManager = new();

        // Method used to update the brodcast configuration
        private void UpdateBroadcastConfiguration()
        {
            // Create timer (if necessary)
            _botConfigurationTimer ??= new Timer(
                                _state => CheckBroadcastConfiguration(), null,
                                Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            lock (_lockBotConfiguration)
            {
                if (_nextBotConfigurationUpdate is not null)
                {
                    _currentBotConfigurationUpdate = _nextBotConfigurationUpdate;
                    _nextBotConfigurationUpdate = null;

                    if (_currentBotConfigurationUpdate is not null)
                    {
                        BotConfigurationExtended.FromJsonNode(_currentBotConfigurationUpdate.JSONNodeBotConfiguration, out _currentBotConfigurationExtended);

                        List<Stream> compositionsList = [];
                        _streamManager.streamsList = [];
                        foreach (var stream in _currentBotConfigurationExtended.Streams.Values)
                        {
                            if (stream.Media.Equals("composition"))
                                compositionsList.Add(stream);
                            else
                                _streamManager.streamsList.Add(stream);
                        }

                        // Check if media with composition are really useable (i.e. they reference media known)
                        foreach (var stream in compositionsList)
                        {
                            if (stream.VideoComposition is null)
                                continue;

                            var streamsFound = _streamManager.streamsList.Where(x => stream.VideoComposition.Contains(x.Id) == true).ToList();
                            if (streamsFound.Count == stream.VideoComposition.Count)
                                _streamManager.streamsList.Add(stream);
                        }
                    }
                }
                else
                {
                    // Current configuration has not been taken into account yet.
                    // We don't continue;
                }
            }

            // Do we have a configuration to set
            if (_currentBotConfigurationUpdate is null)
                return;

            if(_currentBotConfigurationUpdate.Context == "configFile")
                CreateWebRTCEnvironment();


            // CheckBroadcastConfiguration 
            _botConfigurationTimer?.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
        }

        private Boolean IsNewConfigAvailable()
        {
            return _nextBotConfigurationUpdate is not null;
        }

        public Boolean broadcastConfigurationMustBeCheckedAgain = false;

        private void CheckBroadcastConfiguration()
        {
            Boolean result = true;

            if (!RbTask.IsCompleted)
            {
                broadcastConfigurationMustBeCheckedAgain = (currentCallsList.Count == 0);
                return;
            }
            

            Action action = async () =>
            {
                if (IsNewConfigAvailable())
                {
                    UpdateBroadcastConfiguration();
                    return;
                }

                // Ensure to set id / jid for conference if it's not the case
                UpdateConferenceSettings();

                if (currentCallsList.Count > 0)
                {
                    foreach(var call in currentCallsList.Values.ToList())
                    {
                        // STEPS:
                        // 1 - Quit current conference if necessary
                        // 2 - Join conference if necessary
                        // 3 - Connect to necessary medias (connected = true) or used in conference
                        // 4 - Add/Update medias in this order: audio, video and sharing (if possible)
                        // NOTE: Between each steps, check if a new config is available

                        if ((call.CallStatus == Rainbow.Enums.CallStatus.CONNECTING)
                            || (call.CallStatus == Rainbow.Enums.CallStatus.UNKNOWN))
                            continue;

                        result = await QuitConferenceIfNecessaryAsync(call);

                        if (IsNewConfigAvailable())
                            continue;

                        if (result)
                            result = await JoinConferenceIfNecessaryAsync();

                        if (IsNewConfigAvailable())
                            continue;

                        // We have to manage medias in another task to avoid blocking CallUpdated event
                        var _ = ConnectOrDisconnectRemoteMediasAsync(call);
                    }
                }
                else
                {
                    await JoinConferenceIfNecessaryAsync();
                    await ConnectOrDisconnectRemoteMediasAsync(null);
                }
            };

            RbTask = Task.Run(action).ContinueWith(task =>
            {
                if (IsNewConfigAvailable())
                    UpdateBroadcastConfiguration();

                if (result && !broadcastConfigurationMustBeCheckedAgain)
                    _botConfigurationTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                else
                {
                    broadcastConfigurationMustBeCheckedAgain = false;
                    _botConfigurationTimer?.Change(TimeSpan.FromMilliseconds(200), Timeout.InfiniteTimeSpan);
                }
            });
        }

        private Bubble? GetBubble(string id, string jid, string name)
        {
            if (_rbBubbles is null)
                return null;

            Bubble? result = null;

            result = _rbBubbles.GetBubbleById(id);
            result ??= _rbBubbles.GetBubbleByJid(jid);
            result ??= _rbBubbles.GetAllBubbles().Find(b => b.Peer.DisplayName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return result;
        }

        private void UpdateConferenceSettings()
        {
            if (_currentBotConfigurationExtended?.Conferences?.Count > 0)
            {
                foreach (var conference in _currentBotConfigurationExtended.Conferences)
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

        private async Task<Boolean> QuitConferenceIfNecessaryAsync(Call call)
        {
            if (_rbWebRTCCommunications is null) return false;

            if (call is not null)
            {
                Boolean needToQuit = true;
                if (_currentBotConfigurationExtended?.Conferences?.Count > 0)
                {
                    foreach(var conference in _currentBotConfigurationExtended.Conferences)
                    {
                        if(conference.Id == call.Id)
                        {
                            needToQuit = false;
                            break;
                        }
                    }
                }

                if (needToQuit)
                {
                    var sdkResult = await _rbWebRTCCommunications.HangUpCallAsync(call.Id);
                    return (sdkResult.Success);
                }
            }
            return true;
        }

        private async Task<Boolean> JoinConferenceIfNecessaryAsync()
        {
            if ( (_rbWebRTCCommunications is null) || (_rbWebRTCDesktopFactory is null) ) return false;

            if (_currentBotConfigurationExtended?.Conferences?.Count > 0)
            {
                foreach(var conference in _currentBotConfigurationExtended.Conferences)
                {
                    if(conference?.Id is not null)
                    {
                        if (conferencesInProgress.Contains(conference.Id) && !currentCallsList.ContainsKey(conference.Id))
                        {
                            // TODO - manage presence
                            _rbContacts?.SavePresenceForRollback();

                            // Always join with Audio Empty Track
                            var emptyAudioTrack = _rbWebRTCDesktopFactory.CreateEmptyAudioTrack();
                            var sdkResult = await _rbWebRTCCommunications.JoinConferenceAsync(conference.Id, emptyAudioTrack);
                            return (sdkResult.Success);
                        }
                    }
                }
            }
            return true;
        }

        private async Task<Boolean> ConnectOrDisconnectRemoteMediasAsync(Call? call)
        {
            if ((_rbWebRTCCommunications is null) || (_rbWebRTCDesktopFactory is null)) return false;

            // From configuration get streams to connect
            List<Stream>? allStreamsToConnect = null;
            Stream? audioStream = null;
            Stream? videoStream = null;
            Stream? sharingStream = null;

            if (_streamManager.streamsList is not null)
            { 
                allStreamsToConnect = _streamManager.streamsList.Where(s => s.Connected).ToList();
                
                if (call is not null)
                {
                    var conference = _currentBotConfigurationExtended?.Conferences?.FirstOrDefault(c => c.Id == call.Id);
                    if (conference is not null)
                    {
                        audioStream = _currentBotConfigurationExtended?.Streams.Values.FirstOrDefault(s => s.Id == conference.AudioStreamId);
                        videoStream = _currentBotConfigurationExtended?.Streams.Values.FirstOrDefault(s => s.Id == conference.VideoStreamId);
                        sharingStream = _currentBotConfigurationExtended?.Streams.Values.FirstOrDefault(s => s.Id == conference.SharingStreamId);

                        Util.WriteBlue($"[{BotName}] Conference Id:[{conference.Id}] - AudioStreamId:[{audioStream?.Id}] - VideoStreamId:[{videoStream?.Id}] - SharingStreamId:[{sharingStream?.Id}]");
                    }
                }
            }

            await _streamManager.UseStreamsAsync(audioStream, videoStream, sharingStream, (allStreamsToConnect is null) ? [] : allStreamsToConnect.ToList());

            if (call?.IsActive() == true)
            {
                SdkResult<Boolean> sdkResultBoolean;
                var tracksUsed = _rbWebRTCCommunications.GetTracks(call.Id, Rainbow.Consts.Media.AUDIO + Rainbow.Consts.Media.VIDEO + Rainbow.Consts.Media.SHARING, true);

                var audioTrackUsed = tracksUsed?.FirstOrDefault(t => t.Media == Rainbow.Consts.Media.AUDIO);
                var videoTrackUsed = tracksUsed?.FirstOrDefault(t => t.Media == Rainbow.Consts.Media.VIDEO);
                var sharingTrackUsed = tracksUsed?.FirstOrDefault(t => t.Media == Rainbow.Consts.Media.SHARING);

                // Manage Audio
                if (_streamManager.mediaInputAudio is null)
                {
                    if (Rainbow.Util.MediasWithAudio(call.LocalMedias))
                    {
                        // TODO - Remove audio ... => Or at least use "empty track" 
                    }
                    // TODO - Close AudioStreamTrack
                }
                else
                {
                    if (Rainbow.Util.MediasWithAudio(call.LocalMedias))
                    {
                        // Check if we need to update AudioStreamTrack
                    }
                    else
                    {
                        // Need to add AudioStreamTrack
                    }
                }


                // Manage Video
                if (_streamManager.mediaInputVideo is null)
                {
                    if (Rainbow.Util.MediasWithVideo(call.LocalMedias))
                    {
                        // We need to remove Video
                        Util.WriteGreen($"[{BotName}] Remove Video in conf - no stream to use");
                        sdkResultBoolean = await _rbWebRTCCommunications.RemoveVideoAsync(call.Id);
                    }
                    // TODO - Close VideoStreamTrack
                }
                else
                {
                    if (Rainbow.Util.MediasWithVideo(call.LocalMedias))
                    {

                        // Check if we need to update VideoStreamTrack
                        if (videoTrackUsed?.MediaStreamTrack?.Id != _streamManager.mediaInputVideo.Id)
                        {
                            Util.WriteGreen($"[{BotName}] Change Video in conf - Id:[{_streamManager.mediaInputVideo.Id}]");
                            var videoTrack = _rbWebRTCDesktopFactory.CreateVideoTrack(_streamManager.mediaInputVideo);

                            sdkResultBoolean = await _rbWebRTCCommunications.ChangeVideoAsync(call.Id, videoTrack);
                            if (sdkResultBoolean.Success)
                            {
                                // DONE WELL
                            }
                            else
                            {
                                // 
                            }
                        }
                        else
                        {
                            // Same video SPECIFIED
                        }
                    }
                    else
                    {
                        // Need to add VideoStreamTrack
                        var videoTrack = _rbWebRTCDesktopFactory.CreateVideoTrack(_streamManager.mediaInputVideo);
                        sdkResultBoolean = await _rbWebRTCCommunications.AddVideoAsync(call.Id, videoTrack);
                        if (sdkResultBoolean.Success)
                        {
                            // DONE WELL
                        }
                        else
                        {
                            // 
                        }
                    }
                }

                // Manage Sharing
                if (_streamManager.mediaInputSharing is null)
                {
                    if (Rainbow.Util.MediasWithSharing(call.LocalMedias))
                    {
                        // We need to remove Video
                        Util.WriteGreen($"[{BotName}] Remove Sharing in conf - no stream to use");
                        sdkResultBoolean = await _rbWebRTCCommunications.RemoveSharingAsync(call.Id);
                    }
                    // TODO - Close VideoStreamTrack
                }
                else
                {
                    if (Rainbow.Util.MediasWithSharing(call.LocalMedias))
                    {

                        // Check if we need to update VideoStreamTrack
                        if (sharingTrackUsed?.MediaStreamTrack?.Id != _streamManager.mediaInputSharing.Id)
                        {
                            Util.WriteGreen($"[{BotName}] Change Video in conf - Id:[{_streamManager.mediaInputSharing.Id}]");
                            var sharingTrack = _rbWebRTCDesktopFactory.CreateVideoTrack(_streamManager.mediaInputSharing);

                            sdkResultBoolean = await _rbWebRTCCommunications.ChangeVideoAsync(call.Id, sharingTrack);
                            if (sdkResultBoolean.Success)
                            {
                                // DONE WELL
                            }
                            else
                            {
                                // 
                            }
                        }
                        else
                        {
                            // Same video SPECIFIED
                        }
                    }
                    else
                    {
                        // Need to add VideoStreamTrack
                        var videoTrack = _rbWebRTCDesktopFactory.CreateVideoTrack(_streamManager.mediaInputSharing);
                        sdkResultBoolean = await _rbWebRTCCommunications.AddSharingAsync(call.Id, videoTrack);
                        if (sdkResultBoolean.Success)
                        {
                            // DONE WELL
                        }
                        else
                        {
                            // 
                        }
                    }
                }

            }

            return true;
        }

        private void CreateWebRTCEnvironment()
        {
            if (_rbWebRTCDesktopFactory is null)
            {
                UnregisterToWebRTCEvents();

                _rbWebRTCDesktopFactory = new();
                _rbWebRTCCommunications = WebRTCCommunications.GetOrCreateInstance(Application, _rbWebRTCDesktopFactory);

                _rbContacts = Application.GetContacts();
                _rbConferences = Application.GetConferences();
                _rbBubbles = Application.GetBubbles();

                RegisterToWebRTCEvents();
            }
        }

        private void RegisterToWebRTCEvents()
        {
            if (_rbWebRTCCommunications is not null)
                _rbWebRTCCommunications.CallUpdated += RbWebRTCCommunications_CallUpdated;

            if (_rbConferences is not null)
            {
                _rbConferences.ConferenceUpdated += RbConferences_ConferenceUpdated;
                _rbConferences.ConferenceRemoved += RbConferences_ConferenceRemoved;
            }
        }

        private void UnregisterToWebRTCEvents()
        {
            if (_rbWebRTCCommunications is not null)
                _rbWebRTCCommunications.CallUpdated -= RbWebRTCCommunications_CallUpdated;

            if (_rbConferences is not null)
            {
                _rbConferences.ConferenceUpdated -= RbConferences_ConferenceUpdated;
                _rbConferences.ConferenceRemoved -= RbConferences_ConferenceRemoved;
            }
        }

#region Events triggered by Rainbow SDK

        private void RbConferences_ConferenceRemoved(Rainbow.Model.Conference conference)
        {
            RbConferences_ConferenceUpdated(conference);
        }

        private void RbConferences_ConferenceUpdated(Rainbow.Model.Conference conference)
        {
            lock (conferencesInProgress)
            {
                if (conference.Active)
                {
                    if (!conferencesInProgress.Contains(conference.Peer.Id))
                    {
                        conferencesInProgress.Add(conference.Peer.Id);
                        Util.WriteBlue($"[{BotName}] [ConferenceUpdated] A conference is active - Id:[{conference.Peer.Id}]");
                    }
                }
                else
                {
                    if (conferencesInProgress.Contains(conference.Peer.Id))
                    {
                        conferencesInProgress.Remove(conference.Peer.Id);
                        Util.WriteBlue($"[{BotName}] [ConferenceUpdated] A conference is NO MORE active - Id:[{conference.Peer.Id}]");
                    }
                }
                CheckBroadcastConfiguration();
            }
        }

        private void RbWebRTCCommunications_CallUpdated(Call call)
        {
            if ( (call == null) || (call.Id is null) )
                return;

            if(currentCallsList.TryGetValue(call.Id, out var previousCall))
            {
                if (previousCall.Equals(call, true))
                { 
                    Util.WriteRed("Same call update received, no changes - checking participants.");
                    return;
                }
                else if (previousCall.Equals(call, false))
                {
                    Util.WriteRed("Same call update received, no changes - without checking participants.");
                    return;
                }
            }


            if (!call.IsInProgress())
            {
                currentCallsList.Remove(call.Id);

                // The call is NO MORE in Progress => We Rollback presence if any
                var _1 = _rbContacts?.RollbackPresenceSavedAsync();

            }
            else
            {
                // Add / Update this call
                currentCallsList[call.Id] = call;
                if (!call.IsRinging())
                {
                    // The call is NOT IN RINGING STATE  => We update presence according media
                    var _1 = _rbContacts?.SetBusyPresenceAccordingMediasAsync(call.LocalMedias);
                }
            }
                
            Util.WriteBlue($"[{BotName}] [CallUpdated] {call.ToString(Rainbow.Consts.DetailsLevel.Medium)}");
            CheckBroadcastConfiguration();
        }

#endregion Events triggered by Rainbow SDK


#region OVERRIDE METHODS OF BotBase
        public override async Task ConnectedAsync()
        {
            this.Application.Restrictions.JoinMultipleConferences = true;

            // Nothing to do here
            await Task.CompletedTask;
        }

        public override async Task StoppedAsync(SdkError sdkError)
        {
            // Here we do nothing special
            await Task.CompletedTask;
        }

        public override async Task BotConfigurationUpdatedAsync(BotConfigurationUpdate botConfigurationUpdate)
        {
            // Ensure to have an object not null
            if (botConfigurationUpdate is null)
                return;

            // BotConfigurationExtended object has been created to store data structure specific for this bot
            // We try to parse JSON Node to fill this data structure and if it's correct we update the broadcast configuration
            if (BotConfigurationExtended.FromJsonNode(botConfigurationUpdate.JSONNodeBotConfiguration, out BotConfigurationExtended botConfigurationExtended))
            {
                lock (_lockBotConfiguration)
                    _nextBotConfigurationUpdate = botConfigurationUpdate;

                UpdateBroadcastConfiguration();
            }

            await Task.CompletedTask;
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