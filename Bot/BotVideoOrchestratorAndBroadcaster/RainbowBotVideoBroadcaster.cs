using System;
using Stateless;
using Stateless.Graph;
using Rainbow;
using Rainbow.Model;
using Rainbow.Events;
using System.Collections.Concurrent;
using Rainbow.WebRTC;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Rainbow.Medias;

namespace BotVideoOrchestratorAndBroadcaster
{
    public class RainbowBotVideoBroadcaster
    {

        public static readonly String INVALID_BUBBLE_ID = "INVALID_BUBBLE_ID";

        /// <summary>
        /// State list used by this bot
        /// </summary>
        public enum State {
            Created,

            NotConnected,
            Connecting,
            ConnectionFailed,
            Authenticated,

            Initialized,
            Connected,

            AutoReconnection,

            BubbleInvitationReceived,

            JoinConference,
            QuitConference,

            MessageFromPeer,
            StopMessageReceived,

            AddVideoStream,
            AddSharingStream
        }

        /// <summary>
        /// Trigger list used by this bot
        /// </summary>
        public enum Trigger {
            Configure,

            StartLogin,
            Disconnect,
            ServerNotReachable,
            IncorrectCredentials,
            AuthenticationSucceeded,

            InitializationPerformed,
            Connect,

            TooManyAttempts,

            BubbleInvitationReceived,
            BubbleInvitationManaged,

            ConferenceAvailable,
            ConferenceQuit,
            ActionDone,

            StopMessage,
            MessageReceivedFromPeer,
            MessageManaged,

            VideoStreamAvailable,
            SharingStreamAvailable,
        }

        private readonly StateMachine<State, Trigger> _machine;

        // The TriggerWithParameters object is used when a trigger requires a payload.
        private StateMachine<State, Trigger>.TriggerWithParameters<String>? _bubbleInvitationReceivedTrigger;
        private StateMachine<State, Trigger>.TriggerWithParameters<String>? _conferenceAvailableTrigger;
        private StateMachine<State, Trigger>.TriggerWithParameters<MessageEventArgs>? _messageReceivedFromPeerTrigger;

        // Concurrent queue of messages received
        private readonly ConcurrentQueue<MessageEventArgs> _messageQueue;

        // Concurrent queue of invitations received to be  member of a bubble
        private readonly ConcurrentQueue<String> _bubbleInvitationQueue;
        private readonly List<String> _bubbleInvitationInProgress = new List<String>();

        // Several internal variables
        private Contact? _currentContact;
        private Trigger _lastTrigger;

        private readonly List<String> _unknownUser = new List<String>();

        private String _botName = "";
        private String _accountName = "";
        private String? _botLogin;
        private String? _botPwd;

        List<BotManager>? _botManagers;
        private String _stopMessage = "";

        private MediaInput? _videoStream = null;
        private String? _videoStreamUri = null;

        private MediaInput? _sharingStream = null;
        private String? _sharingStreamUri = null;

        private String? _monitoredBubbleId = null;
        private String? _currentConferenceId = null;
        private Boolean _isParticipantInConference = true;
        private Call? _currentCall = null;

        private Boolean _needToAddVideoStreamInConference = false;
        private Boolean _addingVideoStreamInConference = false;

        private Boolean _needToAddSharingStreamInConference = false;
        private Boolean _addingSharingStreamInConference = false;

        private readonly List<String> _conferencesInProgress = new List<string>();

        // Define all Rainbow objects we use
        private Rainbow.Application RbApplication;
        private Rainbow.AutoReconnection RbAutoReconnection;
        private Rainbow.Bubbles RbBubbles;
        private Rainbow.Contacts RbContacts;
        private Rainbow.Conferences RbConferences;
        private Rainbow.InstantMessaging RbInstantMessaging;
        private Rainbow.Invitations RbInvitations;

        private Rainbow.WebRTC.WebRTCCommunications RbWebRTCCommunications;

#region PRIVATE API

        /// <summary>
        /// To create / configure all the state machine
        /// </summary>
        private void ConfigureStateMachine()
        {
            // Create Trigger(s) using parameters
            _conferenceAvailableTrigger = _machine.SetTriggerParameters<String>(Trigger.ConferenceAvailable);
            _bubbleInvitationReceivedTrigger = _machine.SetTriggerParameters<String>(Trigger.BubbleInvitationReceived);
            _messageReceivedFromPeerTrigger = _machine.SetTriggerParameters<MessageEventArgs>(Trigger.MessageReceivedFromPeer);

            // Configure the Configured state
            _machine.Configure(State.Created)
                .Permit(Trigger.Configure, State.NotConnected);

            // Configure the Disconnected state
            _machine.Configure(State.NotConnected)
                .Permit(Trigger.StartLogin, State.Connecting);

            // Configure the Connecting state
            _machine.Configure(State.Connecting)
                .Permit(Trigger.Disconnect, State.ConnectionFailed)
                .Permit(Trigger.AuthenticationSucceeded, State.Authenticated);

            // Configure the Connecting state
            _machine.Configure(State.ConnectionFailed)
                .Permit(Trigger.ServerNotReachable, State.NotConnected)
                .Permit(Trigger.IncorrectCredentials, State.Created);

            // Configure the Connecting state
            _machine.Configure(State.Authenticated)
                .Permit(Trigger.InitializationPerformed, State.Initialized)
                .Permit(Trigger.Disconnect, State.AutoReconnection);

            // Configure the Connected state
            _machine.Configure(State.Initialized)
                .Permit(Trigger.Connect, State.Connected)
                .Permit(Trigger.Disconnect, State.AutoReconnection);

            // Configure the Connected state
            _machine.Configure(State.Connected)
                .OnEntry(CheckConnectionConferenceAndMessages)

                .Permit(Trigger.Disconnect, State.AutoReconnection)

                .PermitIf(_conferenceAvailableTrigger, State.JoinConference)
                .PermitIf(Trigger.ConferenceQuit, State.QuitConference)

                .Permit(Trigger.VideoStreamAvailable, State.AddVideoStream)
                .Permit(Trigger.SharingStreamAvailable, State.AddSharingStream)

                .PermitIf(_messageReceivedFromPeerTrigger, State.MessageFromPeer)
                .PermitIf(_bubbleInvitationReceivedTrigger, State.BubbleInvitationReceived)

                .Permit(Trigger.StopMessage, State.StopMessageReceived);

            // Configure the AddVideoStream state
            _machine.Configure(State.AddVideoStream)
                .OnEntry(AddVideoStream)
                .Permit(Trigger.ActionDone, State.Connected);

            // Configure the AddSharingStream state
            _machine.Configure(State.AddSharingStream)
                .OnEntry(AddSharingStream)
                .Permit(Trigger.ActionDone, State.Connected);

            // Configure the JoinConference state
            _machine.Configure(State.JoinConference)
                .OnEntryFrom(_conferenceAvailableTrigger, JoinConference)
                .Permit(Trigger.ActionDone, State.Connected);

            // Configure the QuitConference state
            _machine.Configure(State.QuitConference)
                .OnEntry(QuitConference)
                .Permit(Trigger.ActionDone, State.Connected);

            // Configure the AutoReconnection state
            _machine.Configure(State.AutoReconnection)
                .PermitReentry(Trigger.Disconnect)
                .Permit(Trigger.TooManyAttempts, State.NotConnected)
                .Permit(Trigger.IncorrectCredentials, State.Created)
                .Permit(Trigger.InitializationPerformed, State.Initialized)
                .Permit(Trigger.AuthenticationSucceeded, State.Authenticated);

            // Configure the InvitationReceived state
            _machine.Configure(State.BubbleInvitationReceived)
                .OnEntryFrom(_bubbleInvitationReceivedTrigger, AnswerToBubbleInvitation)
                .Permit(Trigger.BubbleInvitationManaged, State.Connected);

            // Configure the MessageFromPeer state
            _machine.Configure(State.MessageFromPeer)
                .OnEntryFrom(_messageReceivedFromPeerTrigger, AnswerToPeerMessage)
                .Permit(Trigger.MessageManaged, State.Connected);

            // Configure the StopMessageReceived state
            _machine.Configure(State.StopMessageReceived)
                .OnEntry(StopBot)
                .Permit(Trigger.Disconnect, State.NotConnected);

            _machine.OnUnhandledTrigger((state, trigger) => Util.WriteWarningToConsole($"[{_botName}] OnUnhandledTrigger - State: {state} with Trigger: {trigger}"));

            _machine.OnTransitionCompleted(transition => {
                // Store the trigger used
                _lastTrigger = transition.Trigger;

                // Log info about transition
                Util.WriteInfoToConsole($"[{_botName}] OnTransitionCompleted - State: {transition.Source} -> {transition.Destination} with Trigger: {transition.Trigger}({string.Join(", ", transition.Parameters)})");
            });
        }

        private void ResetValues()
        {
            _currentConferenceId = null;
            _currentCall = null;
            _isParticipantInConference = false;

            _needToAddVideoStreamInConference = false;
            _addingVideoStreamInConference = false;

            _needToAddSharingStreamInConference = false;
            _addingSharingStreamInConference = false;
        }

        private Boolean? IsMessageFromMasterBot(Message message)
        {
            // Get jid of the user who has sent this message
            String fromJid = message.FromJid;

            // Get login email of the user who has sent this message (if any)
            String? fromEmail = null;
            Contact contact = RbContacts.GetContactFromContactJid(message.FromJid);
            if (contact != null)
                fromEmail = contact.LoginEmail;

            // Check if we have a bot manager wiht this info
            var botManager = _botManagers.Where(b => (b.Email?.Equals(fromEmail) == true) || (b.Jid?.Equals(fromJid) == true)).FirstOrDefault();
            if (botManager != null)
                return true;

            // We try to get more info about this contact
            if (contact == null)
            {
                if (!_unknownUser.Contains(message.FromJid))
                {
                    _unknownUser.Add(message.FromJid);
                    Util.WriteDebugToConsole($"[{_botName}] Unknow user - asking more info for  [{message.FromJid}]");
                    RbContacts.GetContactFromContactJidFromServer(message.FromJid, callback =>
                    {
                        if (!callback.Result.Success)
                        {
                            Util.WriteErrorToConsole($"[{_botName}] Cannot get info for user [{message.FromJid}] ...");
                            _unknownUser.Remove(message.FromJid);
                        }
                    });
                }
                return null;
            }

            // We know this contact
            return false;
        }

        /// <summary>
        /// To check if specified message is the "stop message"
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private Boolean IsStopMessage(Message message)
        {
            return (message?.Content.Equals(_stopMessage, StringComparison.InvariantCultureIgnoreCase) == true);
        }

        /// <summary>
        /// To retrieve Alternate Content from the specified message with type set to rainbow/json
        /// </summary>
        /// <param name="message"><see cref="Rainbow.Model.Message"/>Message received</param>
        /// <returns></returns>
        private (Boolean isWithAlternateContent, String? alternateContent) GetAlternateContent(Message message, String type)
        {
            Boolean isWithAlternateContent = false;
            String? alternateContent = null;
            if (message.AlternativeContent != null)
            {
                foreach (MessageAlternativeContent alternativeContent in message.AlternativeContent)
                {
                    if (alternativeContent.Type.Equals(type, StringComparison.InvariantCultureIgnoreCase))
                    {
                        isWithAlternateContent = true;
                        alternateContent = alternativeContent.Content;
                        break;
                    }

                }
            }
            return (isWithAlternateContent, alternateContent);
        }

        /// <summary>
        /// To answer to the specified message coming directly for another user
        /// </summary>
        private void AnswerToPeerMessage(MessageEventArgs? messageEvent)
        {
            if (messageEvent?.Message != null)
            {
                (Boolean isWithAlternateContent, String? alternateContent) = GetAlternateContent(messageEvent.Message, "application/json");
                if (isWithAlternateContent)
                {
                    var dicoData = JsonConvert.DeserializeObject<Dictionary<String, Object>>(alternateContent);

                    if(dicoData != null)
                    {
                        Boolean joinConference = (Boolean)dicoData["joinConference"];
                        String conferenceId = (String)dicoData["conferenceId"];
                        String useName = (String)dicoData["useName"];
                        _videoStreamUri = (String)dicoData["videoUri"];
                        _sharingStreamUri = (String)dicoData["sharingUri"];

                        if (joinConference && (!String.IsNullOrEmpty(conferenceId)))
                        {
                            if (conferenceId != _monitoredBubbleId)
                            {
                                Util.WriteDebugToConsole($"[{_botName}] We have to JOIN the conference [{conferenceId}]");
                                _monitoredBubbleId = conferenceId;
                            }
                            UpdateVideoStream(_videoStreamUri);
                            UpdateSharingStream(_sharingStreamUri);
                        }
                        else
                        {
                            if (_monitoredBubbleId != INVALID_BUBBLE_ID)
                            {
                                Util.WriteDebugToConsole($"[{_botName}] We have to QUIT the conference [{_monitoredBubbleId}]");
                                _monitoredBubbleId = INVALID_BUBBLE_ID;
                            }
                        }
                    }
                    // TODO
                    //Util.WriteDebugToConsole($"[{_botName}] Message received from a Bot Manager - Content:[{alternateContent}]");
                }
                else
                {
                    Util.WriteWarningToConsole($"[{_botName}] Message received from a Bot Manager but it's not managed");
                }
            }
            FireTrigger(Trigger.MessageManaged);
        }

        /// <summary>
        /// Check if conference are in progress,
        /// Dequeue ack messages received (if any)
        /// </summary>
        private void CheckConnectionConferenceAndMessages()
        {
            // Check first if are still connected
            if (!RbApplication.IsConnected())
            {
                FireTrigger(Trigger.Disconnect);
                return;
            }

            // Dequeue bubble's invitation
            if (_bubbleInvitationQueue.TryDequeue(out String? bubbleId))
            {
                if (bubbleId != null)
                {
                    _machine.Fire(_bubbleInvitationReceivedTrigger, bubbleId);
                    return;
                }
            }

            // Check if have to be still in a conference
            if( (!String.IsNullOrEmpty(_currentConferenceId)) && (_currentConferenceId != _monitoredBubbleId) )
            {
                _machine.Fire(Trigger.ConferenceQuit);
                return;
            }

            // Check if we are not in a conference and if a conference is in progress
            if (String.IsNullOrEmpty(_currentConferenceId) && (_conferencesInProgress.Count > 0))
            {
                foreach (String confId in _conferencesInProgress)
                {
                    // Check if can use this conference
                    if (String.IsNullOrEmpty(_monitoredBubbleId) || _monitoredBubbleId.Equals(confId))
                    {
                        Bubble bubble = RbBubbles.GetBubbleByIdFromCache(confId);
                        if (bubble != null)
                        {
                            // Check if we are an accepted member of this bubble
                            if (RbBubbles.IsAccepted(bubble) == true)
                            { 
                                if (_bubbleInvitationInProgress.Contains(confId))
                                    _bubbleInvitationInProgress.Remove(confId);

                                _machine.Fire(_conferenceAvailableTrigger, confId);
                                return;
                            }
                        }
                        else
                        {
                            // Ask more info about this bubble
                            RbBubbles.GetBubbleById(confId);
                        }
                    }
                }
            }

            // If we are a participant in a conference, check if video and/or sharing stream must be added
            CheckIfVideoAndSharingMustBeAdded();
            if (!String.IsNullOrEmpty(_currentConferenceId))
            {
                if (_needToAddVideoStreamInConference && !_addingVideoStreamInConference)
                {
                    _machine.Fire(Trigger.VideoStreamAvailable);
                    return;
                }

                if (_needToAddSharingStreamInConference && !_addingSharingStreamInConference)
                {
                    // We check if sharing is not already used
                    if (IsSharingNotUsedByPeer())
                    {
                        _machine.Fire(Trigger.SharingStreamAvailable);
                        return;
                    }
                }
            }

            // Dequeue messages
            if (_messageQueue.Count > 0)
            {
                if (_messageQueue.TryDequeue(out MessageEventArgs? messageEvent))
                {
                    if (messageEvent != null)
                    {

                        Boolean? messageFromMasterBot = IsMessageFromMasterBot(messageEvent.Message);
                        if (messageFromMasterBot == true)
                        {
                            // Is-it the stop message from master bot ?
                            if (IsStopMessage(messageEvent.Message))
                            {
                                _machine.Fire(Trigger.StopMessage);
                                return;
                            }
                            else 
                            {
                                _machine.Fire(_messageReceivedFromPeerTrigger, messageEvent);
                                return;
                            }
                        }
                        else if (messageFromMasterBot == null)
                        {
                            // Since we don't know if it's the message is coming from master bot we enqueue the message
                            _messageQueue.Enqueue(messageEvent);
                        }
                    }
                }
            }

            CancelableDelay.StartAfter(10, CheckConnectionConferenceAndMessages);
        }

        /// <summary>
        /// To stop  the bot 
        /// </summary>
        private void StopBot()
        {
            HangUp();
            CancelableDelay.StartAfter(500, () => RbApplication.Logout());
        }

        /// <summary>
        /// To answer to the specified invitations to be a member of a bubble
        /// </summary>
        private void AnswerToBubbleInvitation(String? bubbleId)
        {
            if (bubbleId != null)
            {
                if(!_bubbleInvitationInProgress.Contains(bubbleId))
                    _bubbleInvitationInProgress.Add(bubbleId);

                RbBubbles.AcceptInvitation(bubbleId, callback =>
                {
                    if(!callback.Result.Success)
                    {
                        _bubbleInvitationInProgress.Remove(bubbleId);
                    }
                });
            }

            FireTrigger(Trigger.BubbleInvitationManaged);
        }

        /// <summary>
        /// To specify restrictions to use in the Rainbow SDK - for example we want to use the AutoReconnection service and don' want to store message
        /// </summary>
        private void SetRainbowRestrictions()
        {
            // We want to let MCQ message visible to the end user
            RbApplication.Restrictions.MessageStorageMode = Restrictions.SDKMessageStorageMode.Store;

            // Since we are using a Bot we want to send automatically a Read Receipt when a message has been received
            RbApplication.Restrictions.SendReadReceipt = true;

            // Since we are using a bot, we don't want to allow messages to be sent to oneself.
            RbApplication.Restrictions.SendMessageToConnectedUser = false;

            // We want to use always the same resource id when we connect to the event server
            RbApplication.Restrictions.UseSameResourceId = true;

            // We want to auto reconnect in case of network trouble
            RbApplication.Restrictions.AutoReconnection = true;

            // We use XMPP for event mode
            RbApplication.Restrictions.EventMode = Restrictions.SDKEventMode.XMPP;

            // We want to use conference features via API V2
            RbApplication.Restrictions.UseConferences = true;

            // We want to use WebRTC
            RbApplication.Restrictions.UseWebRTC = true;
        }

        /// <summary>
        /// To create all necessary objects from the Rainbow SDK C#
        /// </summary>
        private Boolean CreateRainbowObjects()
        {
            RbAutoReconnection = RbApplication.GetAutoReconnection();
            RbAutoReconnection.MaxNbAttempts = 5; // For tests purpose we use here a low value

            RbBubbles = RbApplication.GetBubbles();
            RbContacts = RbApplication.GetContacts();
            RbConferences = RbApplication.GetConferences();
            RbInstantMessaging = RbApplication.GetInstantMessaging();
            RbInvitations = RbApplication.GetInvitations();

            try
            {
                RbWebRTCCommunications = Rainbow.WebRTC.WebRTCCommunications.CreateInstance(RbApplication, RainbowApplicationInfo.ffmpegLibFolderPath);
            }
            catch (Exception ex)
            {
                Util.WriteErrorToConsole($"[{_botName}] Initialization failed ... \r\nException:[{Rainbow.Util.SerializeException(ex)}] \r\nPossible reason: SDL2 library is not in the same folder than the executable");
                return false;
            }

            SubscribeToRainbowEvents();
            return true;
        }

        /// <summary>
        /// To subscribe to all necessary events from the Rainbow SDK C#
        /// </summary>
        private void UnsubscribeToRainbowEvents()
        {
            if (RbApplication == null)
                return;

            RbApplication.AuthenticationSucceeded -= RbApplication_AuthenticationSucceeded;
            RbApplication.ConnectionStateChanged -= RbApplication_ConnectionStateChanged;
            RbApplication.InitializationPerformed -= RbApplication_InitializationPerformed;

            RbAutoReconnection.MaxNbAttemptsReached -= RbAutoReconnection_MaxNbAttemptsReached;
            RbAutoReconnection.Cancelled -= RbAutoReconnection_Cancelled;

            RbConferences.ConferenceRemoved -= RbConferences_ConferenceRemoved;
            RbConferences.ConferenceUpdated -= RbConferences_ConferenceUpdated;
            RbConferences.ConferenceParticipantsUpdated -= RbConferences_ConferenceParticipantsUpdated;

            RbInstantMessaging.MessageReceived -= RbInstantMessaging_MessageReceived;

            RbWebRTCCommunications.CallUpdated -= RbWebRTCCommunications_CallUpdated;
            RbWebRTCCommunications.OnLocalVideoError -= RbWebRTCCommunications_OnLocalVideoError;
        }

        /// <summary>
        /// To unsubscribe to all necessary events previously subscribe
        /// </summary>
        private void SubscribeToRainbowEvents()
        {
            RbApplication.AuthenticationSucceeded += RbApplication_AuthenticationSucceeded;
            RbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;
            RbApplication.InitializationPerformed += RbApplication_InitializationPerformed;

            RbAutoReconnection.MaxNbAttemptsReached += RbAutoReconnection_MaxNbAttemptsReached;
            RbAutoReconnection.Cancelled += RbAutoReconnection_Cancelled;

            RbInstantMessaging.MessageReceived += RbInstantMessaging_MessageReceived;

            RbBubbles.BubbleInvitationReceived += RbBubbles_BubbleInvitationReceived;

            RbConferences.ConferenceRemoved += RbConferences_ConferenceRemoved;
            RbConferences.ConferenceUpdated += RbConferences_ConferenceUpdated;
            RbConferences.ConferenceParticipantsUpdated += RbConferences_ConferenceParticipantsUpdated;

            RbWebRTCCommunications.CallUpdated += RbWebRTCCommunications_CallUpdated;
            RbWebRTCCommunications.OnLocalVideoError += RbWebRTCCommunications_OnLocalVideoError;
        }

        /// <summary>
        /// Facilitator to fire/raise a trigger on the state machine
        /// </summary>
        private Boolean FireTrigger(Trigger trigger)
        {
            try
            {
                _machine.Fire(trigger);
            }
            catch
            {
                return false;
            }
            return true;
        }

    #region METHODS ABOUT CONFERENCE and STREAM

        private void CheckIfVideoAndSharingMustBeAdded()
        {
            if((_currentCall != null) && (_currentCall.CallStatus == Call.Status.ACTIVE) )
            {
                if( (!_addingVideoStreamInConference) && (!Rainbow.Util.MediasWithVideo(_currentCall.LocalMedias)) )
                    _needToAddVideoStreamInConference = true;

                if ((!_addingSharingStreamInConference) && (!Rainbow.Util.MediasWithSharing(_currentCall.LocalMedias)) )
                    _needToAddSharingStreamInConference = true;
            }
        }

        private void JoinConference(String confId)
        {
            _currentConferenceId = confId;

            RbWebRTCCommunications.JoinConference(confId, null, false, 0, false, callback =>
            {
                if (!callback.Result.Success)
                {
                    _currentConferenceId = null;
                    Util.WriteErrorToConsole($"[{_botName}] Cannot Join Conference - BubbleID:[{confId}] - Error:[{Rainbow.Util.SerializeSdkError(callback.Result)}");
                }
            });
            
            FireTrigger(Trigger.ActionDone);
        }

        private void HangUp()
        {
            if (_currentCall != null)
            {
                String callId = _currentCall.Id;
                if (_currentCall.IsInProgress())
                    RbWebRTCCommunications.HangUpCall(callId);

                _currentCall = null;
            }
        }

        private void QuitConference()
        {
            SetNewVideoOrSharingStream(ref _videoStream, null);
            SetNewVideoOrSharingStream(ref _sharingStream, null);

            HangUp();
            ResetValues();
            FireTrigger(Trigger.ActionDone);
        }

        public void AddVideoStream()
        {
            _needToAddVideoStreamInConference = false;
            _addingVideoStreamInConference = true;

            if (!String.IsNullOrEmpty(_videoStreamUri))
            {
                Rainbow.CancelableDelay.StartAfter(500, () =>
                {
                    UpdateVideoStream(_videoStreamUri);
                    _addingVideoStreamInConference = false;

                });
            }
            FireTrigger(Trigger.ActionDone);
        }

        public void AddSharingStream()
        {
            _needToAddSharingStreamInConference = false;
            _addingSharingStreamInConference = true;

            if (!String.IsNullOrEmpty(_sharingStreamUri))
            {
                Rainbow.CancelableDelay.StartAfter(500, () =>
                {
                    UpdateSharingStream(_sharingStreamUri);
                    _addingSharingStreamInConference = false;

                });
            }
            FireTrigger(Trigger.ActionDone);
        }

        private void SetNewVideoOrSharingStream(ref MediaInput? internalStream, MediaInput? newSharingStream)
        {
            if (internalStream == null)
                internalStream = newSharingStream;

            if (newSharingStream != null)
            {
                if (newSharingStream.Path != internalStream.Path)
                {
                    // Dispose previous stream
                    internalStream.Dispose();

                    // Store new one
                    internalStream = newSharingStream;
                }
            }
            else if(internalStream != null)
            {
                // Dispose previous stream
                internalStream.Dispose();

                internalStream = null;
            }
        }

        private void SetNewSharingStream(MediaInput? newSharingStream)
        {
            SetNewVideoOrSharingStream(ref _sharingStream, newSharingStream);
        }

        private void SetNewVideoStream(MediaInput ? newVideoStream)
        {
            SetNewVideoOrSharingStream(ref _videoStream, newVideoStream);
        }

        public void UpdateVideoStream(String? uri)
        {
            if (String.IsNullOrEmpty(uri))
            {
                Util.WriteInfoToConsole($"[{_botName}] URI for Video Stream is null/empty");
                return;
            }

            // Are we currently in a conference ?
            if ((_currentCall != null) && (_currentCall.CallStatus == Call.Status.ACTIVE))
            {
                MediaInput newStreamToUse;

                if (_videoStream?.Path == uri)
                {
                    newStreamToUse = _videoStream;
                }
                else
                {
                    var uriDevice = new InputStreamDevice("videoStream", "videoStream", uri, true, false, true);
                    newStreamToUse = new MediaInput(uriDevice);
                    if (!newStreamToUse.Init(true))
                    {
                        Util.WriteErrorToConsole($"[{_botName}] Canno init Video Stream using this URI:{uri}].");
                        return;
                    }
                }

                Util.WriteWarningToConsole($"[{_botName}] For Video Stream trying to use this URI:{uri}].");

                Boolean videoHasNotBeenSet = false;

                if (Rainbow.Util.MediasWithVideo(_currentCall.LocalMedias))
                {
                    if (newStreamToUse.Path == _videoStream?.Path)
                    {
                        Util.WriteWarningToConsole($"[{_botName}] For Video, We alreayd use this URI:[{uri}]");
                        return;
                    }

                    if (!RbWebRTCCommunications.ChangeVideo(_currentConferenceId, newStreamToUse))
                    {
                        videoHasNotBeenSet = true;
                        Util.WriteErrorToConsole($"[{_botName}] Cannot UPDATE Video stream to new URI:[{uri}]");

                        RbWebRTCCommunications.RemoveVideo(_currentConferenceId);
                    }
                    else
                    {
                        Util.WriteWarningToConsole($"[{_botName}] Video stream using URI:[{uri}]");
                    }
                }
                else
                {
                    if (!RbWebRTCCommunications.AddVideo(_currentConferenceId, newStreamToUse))
                    {
                        videoHasNotBeenSet = true;
                        Util.WriteErrorToConsole($"[{_botName}] Cannot SET Video stream to new URI:[{uri}]");
                    }
                    else
                    {
                        Util.WriteWarningToConsole($"[{_botName}] Video stream using URI:[{uri}]");
                    }
                }

                if (!videoHasNotBeenSet)
                    SetNewVideoStream(newStreamToUse);
            }
        }

        public void UpdateSharingStream(String? uri)
        {
            if (String.IsNullOrEmpty(uri))
            {
                Util.WriteDebugToConsole($"[{_botName}] URI for Sharing Stream is null/empty");
                return;
            }

            // Are we currently in a conference ?
            if ((_currentConferenceId != null) && (_currentCall != null))
            {
                MediaInput newStreamToUse;

                if (_sharingStream?.Path == uri)
                {
                    newStreamToUse = _sharingStream;
                }
                else
                {
                    var uriDevice = new InputStreamDevice("sharingStream", "sharingStream", uri, true, false, true);
                    newStreamToUse = new MediaInput(uriDevice);
                    if (!newStreamToUse.Init(true))
                    {
                        Util.WriteErrorToConsole($"[{_botName}] Canno init Sharing Stream using this URI:|{uri}].");
                        return;
                    }
                }

                Util.WriteWarningToConsole($"[{_botName}] For Sharing Stream trying to use this URI:{uri}].");

                Boolean sharingHasNotBeenSet = false;
            

                if (Rainbow.Util.MediasWithSharing(_currentCall.LocalMedias))
                {
                    if (newStreamToUse.Path == _sharingStream?.Path)
                    {
                        Util.WriteWarningToConsole($"[{_botName}] For Sharing, We alreayd use this URI:[{uri}]");
                        return;
                    }

                    if (!RbWebRTCCommunications.ChangeSharing(_currentConferenceId, newStreamToUse))
                    {
                        sharingHasNotBeenSet = true;
                        Util.WriteErrorToConsole($"[{_botName}] Cannot UPDATE Sharing stream to new URI:[{uri}]");

                        RbWebRTCCommunications.RemoveSharing(_currentConferenceId);
                    }
                    else
                    {
                        Util.WriteWarningToConsole($"[{_botName}] Sharing stream using URI:[{uri}]");
                    }
                }
                else
                {
                    if (!RbWebRTCCommunications.AddSharing(_currentConferenceId, newStreamToUse))
                    {
                        sharingHasNotBeenSet = true;
                        Util.WriteErrorToConsole($"[{_botName}] Cannot SET Sharing stream to new URI:[{uri}]");
                    }
                    else
                    {
                        Util.WriteWarningToConsole($"[{_botName}] Sharing stream using URI:[{uri}]");
                    }
                }

                if (!sharingHasNotBeenSet)
                    SetNewSharingStream(newStreamToUse);
            }
        }
        
        private Boolean IsSharingNotUsedByPeer()
        {
            return (_currentCall != null) && !Rainbow.Util.MediasWithSharing(_currentCall.RemoteMedias);
        }

    #endregion METHODS ABOUT CONFERENCE and STREAM

    #region EVENTS RAISED FROM RAINBOW OBJECTS

        /// <summary>
        /// Raised when Authentication has succeeded on Rainbow server
        /// </summary>
        private void RbApplication_AuthenticationSucceeded(object? sender, EventArgs e)
        {
            FireTrigger(Trigger.AuthenticationSucceeded);
        }

        /// <summary>
        /// RAised when connection status with Rainbow server has chnaged
        /// </summary>
        private void RbApplication_ConnectionStateChanged(object? sender, ConnectionStateEventArgs e)
        {

            switch (e.State)
            {
                case ConnectionState.Connected:
                    if (RbApplication.IsInitialized())
                        FireTrigger(Trigger.Connect);
                    break;

                case ConnectionState.Disconnected:
                    FireTrigger(Trigger.Disconnect);
                    break;

                case ConnectionState.Connecting:
                    //FireTrigger(Trigger.Connecting);
                    break;
            }

        }

        /// <summary>
        /// Raised when initialization process has been performed (after authentication has succeeded and just before the status connected is raised)
        /// </summary>
        private void RbApplication_InitializationPerformed(object? sender, EventArgs e)
        {
            // Store info about the current contact
            _currentContact = RbContacts.GetCurrentContact();

            // Update the name using _botName
            UpdateName();

            // Get list of bubbles - it's mandatory to be informed when a conference is started in one of this bubble
            RbBubbles.GetAllBubbles(callback =>
            {
                if(callback.Result.Success)
                {
                    // We need to accept any invitation available from bubbles
                    var bubbles = callback.Data;
                    foreach(var bubble in bubbles)
                    {
                        if(RbBubbles.IsInvited(bubble) == true)
                        {
                            if (!_bubbleInvitationInProgress.Contains(bubble.Id))
                                _bubbleInvitationQueue.Enqueue(bubble.Id);
                        }
                    }
                }
            });


            // Check for each bot manager if it's known. If it's not the case, we send an invitation
            if (_botManagers != null)
            { 
                foreach(var bot in _botManagers)
                {
                    Contact? contact = null;
                    if (!String.IsNullOrEmpty(bot.Id))
                    {
                        contact = RbContacts.GetContactFromContactId(bot.Id);
                        if (contact == null)
                        {
                            RbInvitations.SendInvitationByContactId(bot.Id);
                            Util.WriteWarningToConsole($"[{_botName}] The bot [{bot.Email} - {bot.Id}] is UNKNOWN but we just send him an invitation.");
                        }
                    }
                    else
                    {
                        var contacts = RbContacts.GetAllContactsFromCache();
                        contact = contacts.Where(c => ((bot.Jid != null) && c.Jid_im.Equals(bot.Jid, StringComparison.InvariantCultureIgnoreCase))
                                                        || ((bot.Email != null) && c.LoginEmail.Equals(bot.Email, StringComparison.InvariantCultureIgnoreCase))).FirstOrDefault();
                        if(contact == null)
                            Util.WriteErrorToConsole($"[{_botName}] The bot [{bot.Email}] is UNKNOWN and no invitation can be send to him. We cannot received command for him !");
                    }
                }
            }

            FireTrigger(Trigger.InitializationPerformed);
        }

        /// <summary>
        /// Raised when the auto reconnection service reach the max number of attempts to successfully connect to the Rainbow server
        /// </summary>
        private void RbAutoReconnection_MaxNbAttemptsReached(object? sender, EventArgs e)
        {
            FireTrigger(Trigger.TooManyAttempts);
        }

        /// <summary>
        /// Raised when the auto reconnection service has beend cancelled
        /// </summary>
        private void RbAutoReconnection_Cancelled(object? sender, EventArgs e)
        {
            if (RbAutoReconnection.CurrentNbAttempts < RbAutoReconnection.MaxNbAttempts)
            {
                if (_machine.IsInState(State.AutoReconnection))
                    FireTrigger(Trigger.IncorrectCredentials);
            }
        }

        /// <summary>
        /// Raised when a invitation to be a member of a bubble has been received
        /// </summary>
        private void RbBubbles_BubbleInvitationReceived(object? sender, BubbleInvitationEventArgs bubbleInvitationEvent)
        {
            // Queue bubble invitation
            if(!_bubbleInvitationInProgress.Contains(bubbleInvitationEvent.BubbleId))
                _bubbleInvitationQueue.Enqueue(bubbleInvitationEvent.BubbleId);
        }

        /// <summary>
        /// Event fired when a conference is removed/finished
        /// </summary>
        private void RbConferences_ConferenceRemoved(object? sender, IdEventArgs e)
        {
            if (e.Id == _currentConferenceId)
                ResetValues();

            // Remove info about this conference
            _conferencesInProgress.Remove(e.Id);
        }

        /// <summary>
        /// Event fired when a conference informationn is updated.
        /// </summary>
        private void RbConferences_ConferenceUpdated(object? sender, ConferenceEventArgs e)
        {
            if (e.Conference == null)
                return;

            if(e.Conference.Active)
            {
                // Store info about this conference
                if (!_conferencesInProgress.Contains(e.Conference.Id))
                    _conferencesInProgress.Add(e.Conference.Id);
            }
        }

        /// <summary>
        /// Event fired when at least one participant has been added, removed, updated
        /// </summary>
        private void RbConferences_ConferenceParticipantsUpdated(object? sender, Rainbow.Events.ConferenceParticipantsEventArgs e)
        {
            if (_currentContact == null)
                return;

            _isParticipantInConference = (e.Participants?.Count > 0) && e.Participants.ContainsKey(_currentContact.Id);
        }

        /// <summary>
        /// Event raised when the current user received a message from a conversation
        /// </summary>
        private void RbInstantMessaging_MessageReceived(object? sender, MessageEventArgs messageEvent)
        {
            // We don't want to manage message coming from ourself
            if (messageEvent.ContactJid?.Equals(_currentContact?.Jid_im, StringComparison.InvariantCultureIgnoreCase) == false)
            {
                // Queue message
                _messageQueue.Enqueue(messageEvent);
            }
        }

        /// <summary>
        /// Event fired when a webRTC call has been updated
        /// </summary>
        private void RbWebRTCCommunications_CallUpdated(object? sender, CallEventArgs e)
        {
            if (e.Call.Id == _currentConferenceId)
            {
                _currentCall = e.Call;

                if (e.Call.IsInProgress())
                {
                    if (!e.Call.IsRinging())
                    {
                        RbContacts?.SetBusyPresenceAccordingMedias(e.Call.LocalMedias);
                    }
                }
                else
                {
                    RbContacts.SetPresenceLevel(new Presence("online"));
                }
            }
        }

        /// <summary>
        /// Event raised when the Sharing input stream of the current user is not possible / accessible
        /// </summary>
        private void RbWebRTCCommunications_OnLocalVideoError(string callId, string userId, string mediaId, string message)
        {
            if (mediaId == "videoStream")
            {
                Util.WriteErrorToConsole($"[{_botName}] Failed to stream video using URI:[{_videoStreamUri}] - Retrying ...");

                if (_currentCall != null)
                {
                    // We remove the video stream in the WebRTC Communication.
                    RbWebRTCCommunications.RemoveVideo(_currentCall.Id);

                    // /!\ An automatic attempt to stream it again will be performed in CheckConnectionConferenceAndMessages method
                }
            }
            else
            {
                Util.WriteErrorToConsole($"[{_botName}] Failed to stream Sharing using URI:[{_sharingStreamUri}] - Retrying ...");

                if (_currentCall != null)
                {
                    // We remove the sharing stream in the WebRTC Communication.
                    RbWebRTCCommunications.RemoveSharing(_currentCall.Id);

                    // /!\ An automatic attempt to stream it again will be performed in CheckConnectionConferenceAndMessages method
                }
            }
        }

        
    #endregion EVENTS RAISED FROM RAINBOW OBJECTS

#endregion PRIVATE API

#region PUBLIC API

        public Contact ? GetCurrentContact()
        {
            return _currentContact;
        }

        public String BotName
        {
            get { return _botName; }
        }

        /// <summary>
        /// Constructor for the RainbowBotBase class
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public RainbowBotVideoBroadcaster()
        {
            // Instantiate a new state machine in the Created state
            _machine = new StateMachine<State, Trigger>(State.Created);

            ConfigureStateMachine();

            _messageQueue = new ConcurrentQueue<MessageEventArgs>();
            _bubbleInvitationQueue = new ConcurrentQueue<String>();
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        ~RainbowBotVideoBroadcaster()
        {
            UnsubscribeToRainbowEvents();
        }

        public void UpdateName(String? name = null)
        {
            if ((name != null) && (name == _accountName))
                return;

            if (name != null)
                _accountName = name;

            if (RbApplication == null)
                return;

            if (!RbApplication.IsConnected())
                return;

            var contact = RbContacts.GetCurrentContact();
            if(contact != null)
            {
                

                contact.FirstName = _accountName;
                contact.LastName = "_";
                RbContacts.UpdateCurrentContact(contact, callback =>
                {
                    if(callback.Result.Success)
                    {

                    }
                });
            }
        }

        /// <summary>
        /// Get Dot Graph of this Bot
        /// </summary>
        /// <returns>The Dot grpah as string</returns>
        public string ToDotGraph()
        {
            return UmlDotGraph.Format(_machine.GetInfo());
        }

        /// <summary>
        /// Get the current state of the Bot
        /// </summary>
        /// <returns><see cref="State"/></returns>
        public State GetState()
        {
            return _machine.State;
        }

        /// <summary>
        /// Determine if the Bot is in the supplied state
        /// </summary>
        /// <param name="state"><see cref="State"/></param>
        /// <returns>True the is the current is equel to, or a substate of, the supplied state</returns>
        public Boolean IsInState(State state)
        {
            return _machine.IsInState(state);
        }

        /// <summary>
        /// Configure Rainbow Bot
        /// </summary>
        /// <param name="appId">APP_ID</param>
        /// <param name="appSecretKey">APP_SECRET_KEY</param>
        /// <param name="hostname">Host name</param>
        /// <param name="login">Rainbow login</param>
        /// <param name="pwd">Rainbow password</param>
        /// <param name="iniFolderFullPathName">[optional]Full Folder path to ini file</param>
        /// <param name="iniFileName">[optional]ini file name</param>
        /// <returns>True if configuration has been done</returns>
        public Boolean Configure(String appId, String appSecretKey, String hostname, String login, String pwd, String botName, List<BotManager>? botManagers, String stopMessage, String? monitoredBubbleId, String? videoStreamUri, String? sharingStreamUri, String? iniFolderFullPathName = null, String? iniFileName = null)
        {
            // Check that the trigger can be used
            Trigger triggerToUse = Trigger.Configure;
            if (!_machine.CanFire(triggerToUse))
                return false;

            // Create Rainbow Application (root object of th SDK)
            RbApplication = new Rainbow.Application(iniFolderFullPathName, iniFileName, botName + "_");

            // Set APP_ID, APP_SECRET_KET and HOSTNAME
            RbApplication.SetApplicationInfo(appId, appSecretKey);
            RbApplication.SetHostInfo(hostname);

            RbApplication.SetTimeout(10000);

            // Set restrictions
            SetRainbowRestrictions();

            // Create others Rainbow SDK Objects
            if (!CreateRainbowObjects())
                return false;

            // Store the name
            _botName = botName;

            // Store login/pwd
            _botLogin = login;
            _botPwd = pwd;

            // Store bot managers
            _botManagers = botManagers;

            _stopMessage = stopMessage;

            // Store Video / Sharing Stream Uri
            _videoStreamUri = videoStreamUri;
            _sharingStreamUri = sharingStreamUri;
            UpdateVideoStream(videoStreamUri);
            UpdateSharingStream(sharingStreamUri);

            // Store Bubble ID monitored 
            _monitoredBubbleId = monitoredBubbleId;

            // Fire the trigger
            return FireTrigger(triggerToUse);
        }

        /// <summary>
        /// Start login process
        /// </summary>
        /// <returns>True if the login process has started</returns>
        public Boolean StartLogin()
        {
            ResetValues();

            // Check that the trigger can be used
            Trigger triggerToUse = Trigger.StartLogin;
            if (!_machine.CanFire(triggerToUse))
                return false;


            if (FireTrigger(triggerToUse))
            {
                RbApplication.Login(_botLogin, _botPwd, callbackLogin =>
                {
                    if (!callbackLogin.Result.Success)
                    {
                        // Need to check if it's a network pb OR invalid credentials
                        if (callbackLogin.Result.HttpStatusCode == 0)
                        {
                            FireTrigger(Trigger.ServerNotReachable);
                        }
                        else
                        {
                            FireTrigger(Trigger.IncorrectCredentials);
                        }
                    }
                });
                return true;
            }
            else
                return false;

        }

        /// <summary>
        /// Get the current state and the trigger used to reach it
        /// </summary>
        /// <returns>(<see cref="State"/>, <see cref="Trigger"/>)</returns>
        public (State, Trigger) GetStateAndTrigger()
        {
            return (_machine.State, _lastTrigger);
        }

#endregion PUBLIC API

    }
}
