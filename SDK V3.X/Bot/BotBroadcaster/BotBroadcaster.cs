using BotBroadcaster.Model;
using BotLibrary.Model;
using Microsoft.Extensions.Logging;
using Rainbow;
using Rainbow.Consts;
using Rainbow.Enums;
using Rainbow.Example.Common;
using Rainbow.Example.CommonSDL2;
using Rainbow.Example.CommonWebRTC;
using Rainbow.Model;
using Rainbow.WebRTC;
using Rainbow.WebRTC.Desktop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotBroadcaster
{
    public class BotBroadcaster: BotLibrary.BotBase
    {
        Conferences? _rbConferences = null;
        Contacts? _rbContacts = null;
        Bubbles? _rbBubbles = null;

        // ------------------------------

        WebRTCCommunications? _rbWebRTCCommunications = null;
        MediaTrackManager? _mediaTrackManager = null;

        // ------------------------------

        public AudioStreamTrack? _audioStreamTrack = null;
        public VideoStreamTrack? _videoStreamTrack = null;
        public VideoStreamTrack? _sharingStreamTrack  = null;

        // ------------------------------
        int _checkConferenceRunning = 0; // 0 = loop idle, 1 = loop running.Only accessed through Interlocked

        BotConfigurationExtended? _currentConfiguration = null;
        BotConfigurationExtended? _nextConfiguration = null;

        BotConfigurationExtended? _lastConfiguration = null; // Store the last configuration set / received - must never be set to null once set

        // ------------------------------

        Call? _currentCall = null; // Current call - updated through event WebRTCCommunications.CallUpdated

        Boolean _usedAlone;
        Object _lockSetListStreamsToUse = new();

        // ------------------------------

        public BotBroadcaster(Boolean usedAlone)
        {
            _usedAlone = usedAlone;
        }

        public void SetWebRTCEnvironment(MediaTrackManager mediaTrackManager)
        {
            _mediaTrackManager = mediaTrackManager;

            _rbWebRTCCommunications = WebRTCCommunications.GetOrCreateInstance(Application, _mediaTrackManager.WebRTCFactory);

            _rbContacts = Application.GetContacts();
            _rbConferences = Application.GetConferences();
            _rbBubbles = Application.GetBubbles();

            RegisterToWebRTCEvents();
        }

        private void RegisterToWebRTCEvents()
        {
            _rbWebRTCCommunications?.CallUpdated += RbWebRTCCommunications_CallUpdated;

            _rbConferences?.ConferenceUpdated += RbConferences_ConferenceUpdated;
            _rbConferences?.ConferenceRemoved += RbConferences_ConferenceRemoved;
        }

        private void SetListStreamsToUse()
        {
            lock (_lockSetListStreamsToUse)
            {

                // If use alone, we have to inform StreamInputManager of streams list and which are used or not
                if (!_usedAlone) return;

                var streamInputManager = StreamInputManager.Instance;

                var configuration = _currentConfiguration ?? _lastConfiguration;
                if (configuration is null)
                {
                    streamInputManager.SetNewConfiguration(null, null);
                    return;
                }

                List<StreamUsage> streamsToUse = [];
                List<Rainbow.Example.Common.Stream> streams = configuration.Streams?.Values.ToList() ?? [];

                // Store list of streams with connected status
                var streamsConnected = streams.Where(s => s.Connected == true).ToList();
                if (streamsConnected?.Count > 0)
                {
                    foreach (var stream in streamsConnected)
                    {
                        streamsToUse.Add(new() { Id = stream.Id, Media = StreamUsage.MediaFromString(stream.Media) });
                    }
                }

                // Store list of streams used for the current conference
                if ((_currentCall?.IsConference == true) && (_currentCall?.IsInProgress() == true))
                {
                    var callId = _currentCall.Id;

                    var conference = configuration.Conferences?.Where(c => c?.Id == callId).FirstOrDefault();
                    if (conference is not null)
                    {
                        if (!String.IsNullOrEmpty(conference.AudioStreamId))
                            streamsToUse.Add(new() { Id = conference.AudioStreamId, Media = Rainbow.Consts.Media.AUDIO });
                        if (!String.IsNullOrEmpty(conference.VideoStreamId))
                            streamsToUse.Add(new() { Id = conference.VideoStreamId, Media = Rainbow.Consts.Media.VIDEO });
                        if (!String.IsNullOrEmpty(conference.SharingStreamId))
                            streamsToUse.Add(new() { Id = conference.SharingStreamId, Media = Rainbow.Consts.Media.VIDEO }); // Here we indicates VIDEO and not SHARING !
                    }
                }

                // Inform streamInputManager
                streamInputManager.SetNewConfiguration(streams, streamsToUse);
            }
        }

#region CONFERENCE MANAGEMENT - Media + Status
        
        /// <summary>
        /// Central point to use to update conference (join / hang up, add / remove media)
        /// </summary>
        internal void StartCheckConference()
        {
            if (!Application.IsConnected())
                return;

            // Atomically "claim" the loop. If it's already claimed, the running loop will take
            // care of the bubbles in the pool (equivalent of the former "IsBusy" test).
            if (Interlocked.CompareExchange(ref _checkConferenceRunning, 1, 0) != 0)
                return;

            try
            {
                // Task.Run: never block the caller (it can be an event handler / UI thread)
                // and never run the first iteration synchronously.
                _ = Task.Run(() => TaskCheckConferenceAsync());
            }
            catch (Exception ex)
            {
                // Could not start the loop: release the flag, otherwise it would never run again
                Interlocked.Exchange(ref _checkConferenceRunning, 0);
                log.LogWarning(ex, "[StartTaskCheckConference] Cannot start TaskCheckConference");
            }
        }

        private async Task TaskCheckConferenceAsync()
        {
            try
            {
                // Use the last configuration available if current one is null
                if (_currentConfiguration is null)
                {
                    _currentConfiguration = _nextConfiguration;
                    _nextConfiguration = null;
                }

                await TaskCheckConferenceDoWorkAsync();
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "[TaskCheckConference] Error occurred during the join step");
            }
            finally
            {
                // Release the loop BEFORE executing the "completed" part:
                // so a call to StartTaskCheckConference() from there (or from an event handler) could start a new run.
                Interlocked.Exchange(ref _checkConferenceRunning, 0);
            }

            try
            {
                if ( _nextConfiguration is not null)
                {
                    _currentConfiguration = _nextConfiguration;
                    _nextConfiguration = null;
                }

                if(_currentConfiguration is not null)
                    StartCheckConference(); // Start a new iteration (not recursive: it's scheduled on the thread pool)

            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "[BubbleJoinPoolWork] Error occurred after the join step");
            }
        }

        private async Task TaskCheckConferenceDoWorkAsync()
        {
            // Sanity check
            if ( (_rbWebRTCCommunications is null) 
                || (_mediaTrackManager is null)
                || (_rbConferences is null) 
                )
                return;

            Boolean mediaTrackNotYetAvailable = false;

            // Each time we perform an action, we must quit this method in order to use the last configuration available (if any)
            // If no action has been performed, we must set _currentConfiguration to null to avoid to loop

            // STEPS:
            // 1) if we are in a conf, check if it's the correct one
            // 2) if we are not in a conf, check if we must join one
            // 3) Check Media in the current conference (if any)

            // 1) if we are in a conf, check if it's the correct one
            ConsoleAbstraction.WriteDarkYellow(" 1) if we are in a conf, check if it's the correct one");
            if ( (_currentCall?.IsConference == true) && _currentCall.IsInProgress())
            {
                string callId = _currentCall.Id;

                // We must quit the conf if the current configuration is null or has no reference about it
                var conference = _currentConfiguration?.Conferences?.Where(c => c?.Id == callId).FirstOrDefault();
                if (conference is null)
                {
                    _audioStreamTrack = null;
                    _videoStreamTrack = null;
                    _sharingStreamTrack = null;

                    var sdkResult = await _rbWebRTCCommunications.HangUpCallAsync(_currentCall.Id);
                    if (!sdkResult.Success)
                        ConsoleAbstraction.WriteRed($"[HangUpCallAsync] - Result:[{sdkResult.Result}]");
                    else 
                        ConsoleAbstraction.WriteDarkYellow(" => HangUpCallAsync");
                    return;
                }
            }

            // 2) if we are not in a conf, check if we must join one
            ConsoleAbstraction.WriteDarkYellow(" 2) if we are not in a conf, check if we must join one");
            if ( ((_currentCall is null) || !_currentCall.IsInProgress())
                && (_currentConfiguration is not null) )
            {
                // Get conferences in progress
                var confInProgress = _rbConferences.GetAllConferences()?.Select(c => c.Peer.Id).ToList();
                if (confInProgress?.Count > 0)
                {
                    // We must just the conf if the current configuration has a reference about it
                    var conference = _currentConfiguration.Conferences.Where(c => !String.IsNullOrEmpty(c?.Id)).FirstOrDefault();
                    if (conference is not null)
                    {
                        _audioStreamTrack = (AudioStreamTrack?) _mediaTrackManager.GetAudioStreamTrack(conference.AudioStreamId);
                        _audioStreamTrack ??= _mediaTrackManager.EmptyAudioTrack;

                        var sdkResult = await _rbWebRTCCommunications.JoinConferenceAsync(conference.Id, _audioStreamTrack);
                        if (!sdkResult.Success)
                            ConsoleAbstraction.WriteRed($"[JoinConferenceAsync] - Result:[{sdkResult.Result}]");
                        else
                            ConsoleAbstraction.WriteDarkYellow(" => JoinConferenceAsync");
                        return;
                    }
                }
            }

            // 3) Check Media in the current conference (if any)
            ConsoleAbstraction.WriteDarkYellow(" 3) Check Media in the current conference (if any)");
            if ( (_currentCall?.IsInProgress() == true) && _currentCall.IsActive())
            {
                string callId = _currentCall.Id;

                var conference = _currentConfiguration?.Conferences?.Where(c => c?.Id == callId).FirstOrDefault();
                if (conference is null) // We should not be in this case
                {
                    _audioStreamTrack = null;
                    _videoStreamTrack = null;
                    _sharingStreamTrack = null;

                    var sdkResult = await _rbWebRTCCommunications.HangUpCallAsync(_currentCall.Id);
                    if (!sdkResult.Success)
                        ConsoleAbstraction.WriteRed($"[HangUpCallAsync] - Result:[{sdkResult.Result}]");
                    else
                        ConsoleAbstraction.WriteDarkYellow(" => HangUpCallAsync");
                    return;
                }
                else
                {

                    // ------------------------------------------------------------------------------------------------------------------------
                    // Check Audio
                    if (!String.IsNullOrEmpty(conference.AudioStreamId))
                    {
                        if (_audioStreamTrack is null || _audioStreamTrack.IsEmptyTrack || _audioStreamTrack.Id != conference.AudioStreamId)
                        {
                            var track = (AudioStreamTrack?)_mediaTrackManager.GetAudioStreamTrack(conference.AudioStreamId);
                            if (track is not null)
                            {
                                var sdkResult = await _rbWebRTCCommunications.ChangeAudioAsync(callId, track);
                                if (sdkResult.Success)
                                {
                                    _audioStreamTrack = track;
                                    ConsoleAbstraction.WriteDarkYellow(" => ChangeAudioAsync");
                                }
                                else
                                    ConsoleAbstraction.WriteRed($"[ChangeAudioAsync] - Result:[{sdkResult.Result}]");
                                return;
                            }
                            else
                                mediaTrackNotYetAvailable = true;
                        }
                    }
                    else
                    {
                        if (_audioStreamTrack is null || !_audioStreamTrack.IsEmptyTrack)
                        {
                            var track = _mediaTrackManager.EmptyAudioTrack;
                            if (track is not null)
                            {
                                var sdkResult = await _rbWebRTCCommunications.ChangeAudioAsync(callId, track);
                                if (sdkResult.Success)
                                {
                                    _audioStreamTrack = track;
                                    ConsoleAbstraction.WriteDarkYellow(" => ChangeAudioAsync");
                                }
                                else
                                    ConsoleAbstraction.WriteRed($"[ChangeAudioAsync] - Result:[{sdkResult.Result}]");
                                return;
                            }
                        }
                    }

                    // ------------------------------------------------------------------------------------------------------------------------
                    // Check Video
                    if (String.IsNullOrEmpty(conference.VideoStreamId))
                    {
                        if (Rainbow.Util.MediasWithVideo(_currentCall.LocalMedias))
                        {
                            // We must remove local video
                            var sdkResult = await _rbWebRTCCommunications.RemoveVideoAsync(callId);
                            if (!sdkResult.Success)
                                ConsoleAbstraction.WriteRed($"[RemoveVideoAsync] - Result:[{sdkResult.Result}]");
                            else
                            {
                                _videoStreamTrack = null;
                                ConsoleAbstraction.WriteDarkYellow(" => RemoveVideoAsync");
                            }
                            return;
                        }
                    }
                    // A video must be streamed in the conference
                    else
                    {
                        // Is current Video is the correct one ?
                        if (Rainbow.Util.MediasWithVideo(_currentCall.LocalMedias))
                        {
                            if (_videoStreamTrack is null || _videoStreamTrack.Id != conference.VideoStreamId)
                            {
                                var track = (VideoStreamTrack?)_mediaTrackManager.GetVideoStreamTrack(conference.VideoStreamId);
                                if (track is not null)
                                {
                                    // We must update local video
                                    var sdkResult = await _rbWebRTCCommunications.ChangeVideoAsync(callId, track);
                                    if (sdkResult.Success)
                                    {
                                        _videoStreamTrack = track;
                                        ConsoleAbstraction.WriteDarkYellow(" => ChangeVideoAsync");
                                    }
                                    else
                                        ConsoleAbstraction.WriteRed($"[ChangeVideoAsync] - Result:[{sdkResult.Result}]");
                                    return;
                                }
                                else
                                    mediaTrackNotYetAvailable = true;
                            }
                        }
                        else
                        {
                            // There is no video yet - we must must add it
                            var track = (VideoStreamTrack?)_mediaTrackManager.GetVideoStreamTrack(conference.VideoStreamId);
                            if (track is not null)
                            {
                                // We must update local video
                                var sdkResult = await _rbWebRTCCommunications.AddVideoAsync(callId, track);
                                if (sdkResult.Success)
                                {
                                    _videoStreamTrack = track;
                                    ConsoleAbstraction.WriteDarkYellow(" => ChangeVideoAsync");
                                }
                                else
                                {
                                    _videoStreamTrack = null;
                                    ConsoleAbstraction.WriteRed($"[AddVideoAsync] - Result:[{sdkResult.Result}]");
                                }
                                return;
                            }
                            else
                                mediaTrackNotYetAvailable = true;
                        }
                    }

                    // ------------------------------------------------------------------------------------------------------------------------
                    // Check Sharing
                    if (String.IsNullOrEmpty(conference.SharingStreamId))
                    {
                        if (Rainbow.Util.MediasWithSharing(_currentCall.LocalMedias))
                        {
                            // We must remove local sharing
                            var sdkResult = await _rbWebRTCCommunications.RemoveSharingAsync(callId);
                            if (!sdkResult.Success)
                                ConsoleAbstraction.WriteRed($"[RemoveSharingAsync] - Result:[{sdkResult.Result}]");
                            else
                                ConsoleAbstraction.WriteDarkYellow(" => RemoveSharingAsync");
                            _sharingStreamTrack = null;
                            return;
                        }
                    }
                    // A sharing must be streamed in the conference
                    else
                    {
                        // Is current sharing is the correct one ?
                        if (Rainbow.Util.MediasWithSharing(_currentCall.LocalMedias))
                        {
                            if (_sharingStreamTrack is null || _sharingStreamTrack.Id != conference.SharingStreamId)
                            {
                                var track = (VideoStreamTrack?)_mediaTrackManager.GetVideoStreamTrack(conference.SharingStreamId);
                                if (track is not null)
                                {
                                    // We must update local sharing
                                    var sdkResult = await _rbWebRTCCommunications.ChangeSharingAsync(callId, track);
                                    if (sdkResult.Success)
                                    {
                                        _sharingStreamTrack = track;
                                        ConsoleAbstraction.WriteDarkYellow(" => ChangeSharingAsync");
                                    }
                                    else
                                        ConsoleAbstraction.WriteRed($"[ChangeSharingAsync] - Result:[{sdkResult.Result}]");
                                    return;
                                }
                                else
                                    mediaTrackNotYetAvailable = true;
                            }
                        }
                        else
                        {
                            // There is no sharing yet - we must must add it
                            var track = (VideoStreamTrack?)_mediaTrackManager.GetVideoStreamTrack(conference.SharingStreamId);
                            if (track is not null)
                            {
                                // We must update local sharing
                                var sdkResult = await _rbWebRTCCommunications.AddSharingAsync(callId, track);
                                if (sdkResult.Success)
                                {
                                    _sharingStreamTrack = track;
                                    ConsoleAbstraction.WriteDarkYellow(" => AddSharingAsync");
                                }
                                else
                                {
                                    ConsoleAbstraction.WriteRed($"[AddSharingAsync] - Result:[{sdkResult.Result}]");
                                    _sharingStreamTrack = null;
                                }
                                return;
                            }
                            else
                                mediaTrackNotYetAvailable = true;
                        }
                    }

                }
            }


            if ((_currentCall?.IsInProgress() == true) && !_currentCall.IsActive()
                || mediaTrackNotYetAvailable )
            {
                if (mediaTrackNotYetAvailable)
                {
                    ConsoleAbstraction.WriteDarkYellow("/!\\ A Media Track is not yet available /!\\ ");
                    await Task.Delay(1000);
                }
                else
                    await Task.Delay(500);
                return;
            }

            // We check conf status (need to be in / out of the conf ?)
            // Then we check medies (audio, video, sharing)
            // All is correct => _currentConfiguration can be set to null
            _currentConfiguration = null;
            ConsoleAbstraction.WriteDarkYellow("/!\\ Current configuration has been totally SET /!\\ ");
        }

#endregion CONFERENCE MANAGEMENT - Media + Status

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
                result ??= await _rbContacts.GetContactByIdInCacheFirstAsync(account.Id);

            if (!String.IsNullOrEmpty(account.Jid))
                result ??= await _rbContacts.GetContactByJidInCacheFirstAsync(account.Jid);

            result ??= GetContactByEmail(account.Login);

            return result;
        }

        private Contact? GetContactByEmail(String? email)
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

        private async Task UpdateConfigurationWithCorrectDataAsync(BotConfigurationExtended? configuration)
        {
            if(Application.IsConnected() && configuration is not null)
            {
                await UpdateAdministratorsConfigurationAsync(configuration);

                UpdateConferenceConfiguration(configuration);
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

#region Events triggered by Rainbow SDK

        private async void RbConferences_ConferenceRemoved(Rainbow.Model.Conference conference)
            => RbConferences_ConferenceUpdated(conference);

        private async void RbConferences_ConferenceUpdated(Rainbow.Model.Conference conference)
        {
            await UpdateConfigurationWithCorrectDataAsync(_nextConfiguration);
            await UpdateConfigurationWithCorrectDataAsync(_currentConfiguration);

            _nextConfiguration ??= _lastConfiguration;
            StartCheckConference();
        }

        private async void RbWebRTCCommunications_CallUpdated(Call? call)
        {
            if (call is null) return;
            if ((_currentCall is null) || (_currentCall.Id == call.Id))
            {
                ConsoleAbstraction.WriteBlue($"[{BotName}] [CallUpdated] {call.ToString(Rainbow.Consts.DetailsLevel.Medium)}", logger: log);

                if (call.CallStatus == CallStatus.UNKNOWN)
                    call = null;

                _currentCall = call;
                UpdatePresence();

                SetListStreamsToUse();

                _nextConfiguration ??= _lastConfiguration;
                StartCheckConference();
            }
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
            await UpdateConfigurationWithCorrectDataAsync(_currentConfiguration);
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

            // BotConfigurationExtended object has been created to store data structure specific for this bot
            // We try to parse JSON Node to fill this data structure and if it's correct we update the broadcast configuration
            var botConfigurationExtended = BotConfigurationExtended.FromJsonNode(botConfigurationUpdate.JSONNodeBotConfiguration);
            if (botConfigurationExtended is not null)
            {
                // Store the configuration
                await UpdateConfigurationWithCorrectDataAsync(botConfigurationExtended);
                _lastConfiguration = botConfigurationExtended;

                // It will be the next one to be used
                _nextConfiguration = botConfigurationExtended;

                SetListStreamsToUse();
                StartCheckConference();
            }
        }

    #region Invitations - bubble or user (we do nothing special here)

        public override async Task BubbleInvitationReceivedAsync(Rainbow.Model.BubbleInvitation bubbleInvitation) => await Task.CompletedTask; // Nothing to do here

        public override async Task UserInvitationReceivedAsync(Rainbow.Model.Invitation invitation) => await Task.CompletedTask; // Nothing to do here

    #endregion Invitations - bubble or user

    #region Messages - AckMessage, ApplicationMessage, InstantMessage, InternalMessage  (we do nothing special here)

        public override async Task AckMessageReceivedAsync(Rainbow.Model.AckMessage ackMessage) => await Task.CompletedTask; // Nothing to do here

        public override async Task ApplicationMessageReceivedAsync(Rainbow.Model.ApplicationMessage applicationMessage) => await Task.CompletedTask; // Nothing to do here

        public override async Task InstantMessageReceivedAsync(Rainbow.Model.Message message) => await Task.CompletedTask; // Nothing to do here

        public override async Task InternalMessageReceivedAsync(InternalMessage internalMessage) => await Task.CompletedTask; // Nothing to do here

    #endregion Messages - AckMessage, ApplicationMessage, InstantMessage, InternalMessage

#endregion OVERRIDE METHODS OF BotBase

    }
}