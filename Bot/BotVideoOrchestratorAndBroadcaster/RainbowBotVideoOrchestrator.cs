using AdaptiveCards.Templating;
using Rainbow;
using Rainbow.Model;
using Rainbow.Events;
using Rainbow.SimpleJSON;
using Stateless;
using Stateless.Graph;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


namespace BotVideoOrchestratorAndBroadcaster
{
    public class RainbowBotVideoOrchestrator
    {
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
            UserInvitationReceived,

            JoinConference,

            StopMessageReceived,
            HelpMessageReceived,

            MessageFromPeer,
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

            UserInvitationReceived,
            UserInvitationManaged,

            ConferenceAvailable,
            ActionDone,

            StopMessage,
            HelpMessage,
            MessageReceivedFromPeer,

            MessageManaged,
        }

        private readonly StateMachine<State, Trigger> _machine;

        // The TriggerWithParameters object is used when a trigger requires a payload.
        private StateMachine<State, Trigger>.TriggerWithParameters<String>? _bubbleInvitationReceivedTrigger;
        private StateMachine<State, Trigger>.TriggerWithParameters<String>? _userInvitationReceivedTrigger;

        private StateMachine<State, Trigger>.TriggerWithParameters<String>? _conferenceAvailableTrigger;
        private StateMachine<State, Trigger>.TriggerWithParameters<MessageEventArgs>? _messageReceivedFromPeerTrigger;
        private StateMachine<State, Trigger>.TriggerWithParameters<MessageEventArgs>? _helpMessageReceivedTrigger;

        // Concurrent queue of messages received
        private readonly ConcurrentQueue<MessageEventArgs> _messageQueue;

        // Concurrent queue of invitations received to be member of a bubble
        private readonly ConcurrentQueue<String> _bubbleInvitationQueue;
        private readonly List<String> _bubbleInvitationInProgress = new List<String>();
        // Concurrent queue of invitations received by user (to be on the roster)
        private readonly ConcurrentQueue<String> _userInvitationQueue;
        private readonly List<String> userInvitationInProgress = new List<String>();

        private readonly List<String> _unknownUser = new List<String>();

        // Several internal variables
        private Trigger _lastTrigger;
        private String _botName = "";
        private String? _botLogin;
        private String? _botPwd;
        List<BotManager>? _botManagers;
        private Contact _currentContact;

        private String? _monitoredBubbleId = null;
        private String? _currentConferenceId = null;
        private Boolean _isParticipantInConference = true;
        private Call? _currentCall = null;

        private readonly List<String> _conferencesInProgress = new List<string>();

        private readonly Dictionary<String, String> _adaptiveCardMessageIdByConversationId = new Dictionary<String, String>(); // Conversation Id / Message Id

        private List<BotVideoBroadcasterInfo> botVideoBroadcasterInfos;
        private List<VideoInfo> videoInfos;

        // Define all Rainbow objects we use
        private Rainbow.Application RbApplication;
        private Rainbow.AutoReconnection RbAutoReconnection;
        private Rainbow.Bubbles RbBubbles;
        private Rainbow.Conferences RbConferences;
        private Rainbow.Contacts RbContacts;
        private Rainbow.Conversations RbConversations;
        private Rainbow.InstantMessaging RbInstantMessaging;
        private Rainbow.Invitations RbInvitations;

        private Rainbow.WebRTC.Desktop.WebRTCFactory RbWebRTCDesktopFactory;
        private Rainbow.WebRTC.WebRTCCommunications RbWebRTCCommunications;

        private int _messageCount = 0;

#region PRIVATE API

        /// <summary>
        /// To create / configure all the state machine
        /// </summary>
        private void ConfigureStateMachine()
        {
            // Create Trigger(s) using parameters
            _bubbleInvitationReceivedTrigger = _machine.SetTriggerParameters<String>(Trigger.BubbleInvitationReceived);
            _userInvitationReceivedTrigger = _machine.SetTriggerParameters<String>(Trigger.UserInvitationReceived);

            _messageReceivedFromPeerTrigger = _machine.SetTriggerParameters<MessageEventArgs>(Trigger.MessageReceivedFromPeer);
            _helpMessageReceivedTrigger = _machine.SetTriggerParameters<MessageEventArgs>(Trigger.HelpMessage);
            _conferenceAvailableTrigger = _machine.SetTriggerParameters<String>(Trigger.ConferenceAvailable);

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
                .Permit(Trigger.Connect, State.Connected)
                .Permit(Trigger.Disconnect, State.AutoReconnection);

            // Configure the Connected state
            _machine.Configure(State.Connected)
                .Permit(Trigger.InitializationPerformed, State.Initialized)
                .Permit(Trigger.Disconnect, State.AutoReconnection);

            // Configure the Connected state
            _machine.Configure(State.Initialized)
                .OnEntry(CheckConnectedStateAndDequeudMessages)

                .Permit(Trigger.Disconnect, State.AutoReconnection)

                .PermitIf(_bubbleInvitationReceivedTrigger, State.BubbleInvitationReceived)
                .PermitIf(_userInvitationReceivedTrigger, State.UserInvitationReceived)

                .PermitIf(Trigger.ConferenceAvailable, State.JoinConference)

                .Permit(Trigger.StopMessage, State.StopMessageReceived)
                .PermitIf(_helpMessageReceivedTrigger, State.HelpMessageReceived)
                .PermitIf(_messageReceivedFromPeerTrigger, State.MessageFromPeer);

            // Configure the JoinConference state
            _machine.Configure(State.JoinConference)
                .OnEntry(JoinConference)
                .Permit(Trigger.ActionDone, State.Initialized);

            // Configure the AutoReconnection state
            _machine.Configure(State.AutoReconnection)
                .PermitReentry(Trigger.Disconnect)
                .Permit(Trigger.TooManyAttempts, State.NotConnected)
                .Permit(Trigger.IncorrectCredentials, State.Created)
                .Permit(Trigger.InitializationPerformed, State.Initialized)
                .Permit(Trigger.AuthenticationSucceeded, State.Authenticated);

            // Configure the BubbleInvitationReceived state
            _machine.Configure(State.BubbleInvitationReceived)
                .OnEntryFrom(_bubbleInvitationReceivedTrigger, AnswerToBubbleInvitation)
                .Permit(Trigger.BubbleInvitationManaged, State.Initialized);

            // Configure the InvitationReceived state
            _machine.Configure(State.UserInvitationReceived)
                .OnEntryFrom(_userInvitationReceivedTrigger, AnswerToUserInvitation)
                .Permit(Trigger.UserInvitationManaged, State.Initialized);

            // Configure the StopMessageReceived state
            _machine.Configure(State.StopMessageReceived)
                .OnEntry(LogoutBot)
                .Permit(Trigger.Disconnect, State.NotConnected);

            // Configure the HelpMessageReceived state
            _machine.Configure(State.HelpMessageReceived)
                .OnEntryFrom(_helpMessageReceivedTrigger, AnswerToMenuMessage)
                .Permit(Trigger.MessageManaged, State.Initialized);

            // Configure the MessageFromPeer state
            _machine.Configure(State.MessageFromPeer)
                .OnEntryFrom(_messageReceivedFromPeerTrigger, AnswerToPeerMessage)
                .Permit(Trigger.MessageManaged, State.Initialized);

            _machine.OnUnhandledTrigger((state, trigger) => Util.WriteWarningToConsole($"[{_botName}] OnUnhandledTrigger - State: {state} with Trigger: {trigger}"));

            _machine.OnTransitionCompleted(transition => {
                // Store the trigger used
                _lastTrigger = transition.Trigger;

                // Log info about transition
                Util.WriteInfoToConsole($"[{_botName}] OnTransitionCompleted- State: {transition.Source} -> {transition.Destination} with Trigger: {transition.Trigger}({string.Join(", ", transition.Parameters)})");
            });
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
            return (message?.Content.Equals(RainbowApplicationInfo.commandStop, StringComparison.InvariantCultureIgnoreCase) == true);
        }

        /// <summary>
        /// To check if specified message is the "help message"
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private Boolean IsHelpMessage(Message message)
        {
            return (message?.Content.Equals(RainbowApplicationInfo.commandMenu, StringComparison.InvariantCultureIgnoreCase) == true);
        }

        /// <summary>
        /// To check if the specified message is coming from bubble or not
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private Boolean IsMessageNotFromBubble(Message message)
        {
            return String.IsNullOrEmpty(message.FromBubbleJid);
        }

        /// <summary>
        /// First check MCQ of users (if necessary).
        /// Then dequeue messages received (if any)
        /// </summary>
        private void DequeuedMessages()
        {
            // Dequeue messages
            if (_messageQueue.Count > 0)
            {
                if (_messageQueue.TryDequeue(out MessageEventArgs? messageEvent))
                {
                    if (messageEvent?.Message != null)
                    {
                        // The message must be sent by a master bot
                        // The message must be sent from the conference in progress (if any)
                        //  OR in P2P

                        Boolean? messageFromMasterBot = IsMessageFromMasterBot(messageEvent.Message);

                        if (messageFromMasterBot == true)
                        {
                            var conversation = RbConversations.GetConversationByIdFromCache(messageEvent.ConversationId);
                            if (conversation != null)
                            {
                                if ((conversation.Type == Conversation.ConversationType.Room) && (conversation.PeerId != _currentConferenceId))
                                {
                                    // We do nothing. We received a message from a bubble but not the one where there is a conference
                                    Util.WriteDebugToConsole($"[{_botName}] Message received from a Bot Manager from a BUBBLE but not the one used for the conference in progress ...");
                                }
                                else
                                {
                                    // Is-it the stop message from master bot ?
                                    if (IsStopMessage(messageEvent.Message))
                                    {
                                        _machine.Fire(Trigger.StopMessage);
                                        return;
                                    }
                                    // Is-it the help message from master bot ?
                                    else if (IsHelpMessage(messageEvent.Message))
                                    {
                                        _machine.Fire(_helpMessageReceivedTrigger, messageEvent);
                                        return;
                                    }
                                    else
                                    {
                                        _machine.Fire(_messageReceivedFromPeerTrigger, messageEvent);
                                        return;
                                    }
                                }
                            }
                            else
                                Util.WriteWarningToConsole($"[{_botName}] Message received from a Bot Manager - Cannot get a conversation from the message recevied - ConversationId: [{messageEvent.ConversationId}]");


                        }
                        else if (messageFromMasterBot == null)
                        {
                            // Since we don't know if it's the message is coming from master bot we enqueue the message
                            _messageQueue.Enqueue(messageEvent);
                        }
                        //else
                        //{
                        //    //// Is it a message from bubble  ?
                        //    //if (!IsMessageNotFromBubble(messageEvent.Message))
                        //    //{
                        //    //    _machine.Fire(_messageReceivedFromPeerTrigger, messageEvent);
                        //    //    return;
                        //    //}
                        //}

                    }
                }
            }
            CancelableDelay.StartAfter(10, CheckConnectedStateAndDequeudMessages);
        }

        /// <summary>
        /// To logout the bot form Rainbow server
        /// </summary>
        private void LogoutBot()
        {
            Util.WriteDebugToConsole($"[{_botName}] Message received from a Bot Manager - STOP message");

            // UPDATE the Adaptive Card in all Conversation previously send

            (String? message, List<MessageAlternativeContent>? alternateContent) = CreateAdaptiveCardForEnd();
            var keys = _adaptiveCardMessageIdByConversationId.Keys.ToList();
            foreach (var key in keys)
            {
                var previousMessageId = _adaptiveCardMessageIdByConversationId[key];
                RbInstantMessaging.EditMessage(key, previousMessageId, message, alternateContent, callback =>
                {
                    if (callback.Result.Success)
                    {
                        Util.WriteDebugToConsole($"[{_botName}] UPDATE previous AdaptiveCard in ConversationId:[{key}] - MessageID:[{previousMessageId}]");
                    }
                    else
                    {
                        Util.WriteErrorToConsole($"[{_botName}] Cannot UPDATE Adaptive Card in ConversationId:[{key}] - MessageID:[{previousMessageId}] - Exception:[{callback.Result}]");
                    }
                });
            }
            _adaptiveCardMessageIdByConversationId.Clear();

            // Send to all bot the STOP message
            int index = 0;
            foreach (var bot in botVideoBroadcasterInfos)
            {
                if (bot.Name != RainbowApplicationInfo.labelNone)
                {
                    index++;
                    var conversation = RbConversations.GetOrCreateConversationFromUserId(bot.Id);
                    if (conversation != null)
                    {
                        RbInstantMessaging.SendMessageToConversationId(conversation.Id, RainbowApplicationInfo.commandStop, null, UrgencyType.Low, null, callback =>
                        {

                        });
                    }

                }
            }

            int delay = 2000 + (index * 500);
            Util.WriteErrorToConsole($"[{_botName}] We will quit after some delay:[{delay}ms]");
            CancelableDelay.StartAfter(delay, () =>
            {
                HangUp();
                RbApplication.Logout();
            });
        }

        /// <summary>
        /// To answer to the specified invitations to be a member of a bubble
        /// </summary>
        private void AnswerToBubbleInvitation(String? bubbleId)
        {
            if (bubbleId != null)
            {
                if (!_bubbleInvitationInProgress.Contains(bubbleId))
                    _bubbleInvitationInProgress.Add(bubbleId);

                RbBubbles.AcceptInvitation(bubbleId, callback =>
                {
                    if (!callback.Result.Success)
                    {
                        _bubbleInvitationInProgress.Remove(bubbleId);
                    }
                });
            }

            FireTrigger(Trigger.BubbleInvitationManaged);
        }

        /// <summary>
        /// To answer to the specified invitations form a user to be in its roster
        /// </summary>
        private void AnswerToUserInvitation(String? invitationId)
        {
            if (invitationId != null)
            {
                if (!userInvitationInProgress.Contains(invitationId))
                    userInvitationInProgress.Add(invitationId);

                RbInvitations.AcceptReceivedPendingInvitation(invitationId, callback =>
                {
                    if (!callback.Result.Success)
                    {
                        userInvitationInProgress.Remove(invitationId);
                    }
                });
            }

            FireTrigger(Trigger.UserInvitationManaged);
        }

        /// <summary>
        /// To retrieve Alternate Content from the specified message sent as response to an Adaptive Card
        /// </summary>
        /// <param name="message"><see cref="Rainbow.Model.Message"/>Message received</param>
        /// <returns></returns>
        private (Boolean isAdaptiveCardAnswer, String? alternateContent) GetAlternateContentFromAdaptiveCardResponse(Message message)
        {
            Boolean isAdaptiveCardAnswer = false;
            String? alternateContent = null;
            if (message.AlternativeContent != null)
            {
                foreach (MessageAlternativeContent alternativeContent in message.AlternativeContent)
                {
                    if (alternativeContent.Type == "rainbow/json")
                    {
                        isAdaptiveCardAnswer = true;
                        alternateContent = alternativeContent.Content;
                        break;
                    }

                }
            }
            return (isAdaptiveCardAnswer, alternateContent);
        }

        /// <summary>
        /// To create the data used to create the Adaptive Card using templating
        /// </summary>
        /// <returns>the data</returns>
        private String CreateAdaptiveCardData()
        {
            String result = "{";

            //result += $"\r\n\"title\": \"{RainbowApplicationInfo.labelTitle} - {_messageCount++}\",";
            result += $"\r\n\"title\": \"{RainbowApplicationInfo.labelTitle}\",";
            result += $"\r\n\"set\": \"{RainbowApplicationInfo.labelSet}\",";
            result += $"\r\n\"stop\": \"{RainbowApplicationInfo.labelStop}\",";
            result += $"\r\n\"setAction\": \"\",";
            result += $"\r\n\"useSharingStream\": \"{RainbowApplicationInfo.labelUseSharingStream}\",";


            // Add botsInfo
            List<string> botInfos = new List<string>();

            foreach (var bot in botVideoBroadcasterInfos)
            {
                String uri = bot.Uri;
                if (uri.StartsWith('"'))
                    uri = bot.Uri.Substring(1);
                if (uri.EndsWith('"'))
                    uri = bot.Uri.Substring(0, bot.Uri.Length-1);

                if (String.IsNullOrEmpty(uri))
                    uri = RainbowApplicationInfo.labelNoVideo;

                String info = "{";
                info += $"\r\n\"name\": \"{bot.Name}\",";
                info += $"\r\n\"uri\": \"{uri.Replace("\\", "\\\\")}\",";
                info += $"\r\n\"checked\": \"{(bot.Selected ? "true" : "false")}\"";
                info += "}";

                botInfos.Add(info);
            }
            result += $"\r\n\"botsInfo\": [{String.Join(",", botInfos)}],";


            // Add videos 
            List<string> videos = new List<string>();
            foreach (var video in videoInfos)
            {
                String info = "{";
                info += $"\r\n\"title\": \"{video.Title.Replace("\\", "\\\\")}\",";
                info += $"\r\n\"uri\": \"{video.Uri.Replace("\\", "\\\\")}\"";
                info += "}";

                videos.Add(info);
            }
            result += $"\r\n\"videos\": [{String.Join(",", videos)}],";


            // Videos for sharing doesn't contain "No Video" (so the first element)
            videos.RemoveAt(0);
            result += $"\r\n\"videosForSharing\": [{String.Join(",", videos)}],";

            // Add sharingSelection
            List<string> sharingSelection = new List<string>();
            String sharingSelected = RainbowApplicationInfo.labelNone;
            String sharingUri = videoInfos[1].Uri;
            foreach (var bot in botVideoBroadcasterInfos)
            {
                String info = "{";
                info += $"\r\n\"name\": \"{bot.Name}\"";
                info += "}";

                sharingSelection.Add(info);
                if (!String.IsNullOrEmpty(bot.SharingUri))
                {
                    sharingSelected = bot.Name;
                    sharingUri = bot.SharingUri;
                }
            }
            result += $"\r\n\"sharingStreamSelected\": \"{sharingUri.Replace("\\", "\\\\")}\",";
            result += $"\r\n\"sharingSelected\": \"{sharingSelected}\",";
            result += $"\r\n\"sharingSelection\": [{String.Join(",", sharingSelection)}]";

            result += "}";
            return result;
        }

        private (String? message, List<MessageAlternativeContent>? alternativeContent) CreateAdaptiveCardForEnd()
        {
            String? message = null;
            List<MessageAlternativeContent>? alternativeContent = null;

            // Create a Template instance from the template payload
            String? jsonTemplate = Util.GetContentOfEmbeddedResource("VideoBroadcastEnd.json", System.Text.Encoding.UTF8);
            String jsonData = "{";
            jsonData += $"\r\n\"title\": \"{RainbowApplicationInfo.labelTitleEnd}\"";
            jsonData += "}";

            // Create template
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(jsonTemplate);

            // "Expand" the template - this generates the final Adaptive Card payload
            string cardJson = template.Expand(jsonData);

            // Create an Message Alternative Content
            MessageAlternativeContent messageAlternativeContent = new MessageAlternativeContent();
            messageAlternativeContent.Type = "form/json";
            messageAlternativeContent.Content = cardJson;

            alternativeContent = new List<MessageAlternativeContent> { messageAlternativeContent };
            // Get title of the question
            var json = JSON.Parse(jsonData);
            message = UtilJson.AsString(json, "title");
            if (String.IsNullOrEmpty(message))
                message = " "; // => Due to a bug in WebClient must not be empty/null

            return (message, alternativeContent);
        }

        /// <summary>
        /// To create all elements necessary to send (or edit) the Adaptive Card using the specified question index
        /// </summary>
        /// <param name="questionIndex"></param>
        /// <param name="userAnswer"></param>
        /// <returns></returns>
        private (String? message, List<MessageAlternativeContent>? alternativeContent) CreateAdaptiveCard()
        {
            String? message = null;
            List<MessageAlternativeContent>? alternativeContent = null;

            // Create a Template instance from the template payload
            String? jsonTemplate = Util.GetContentOfEmbeddedResource("VideoBroadcastSettings.json", System.Text.Encoding.UTF8);

            String? broadCastInfo = Util.GetContentOfEmbeddedResource("VideoBroadcastInfo.json", System.Text.Encoding.UTF8);

            if ((jsonTemplate == null) || (broadCastInfo == null))
                return (null, null);

            List<String> videoInfo = new List<String>();
            var nb = botVideoBroadcasterInfos.Count;
            for (int i = 1; i < nb; i++)
            {
                videoInfo.Add(broadCastInfo.Replace("[[$ID]]", i.ToString()));
            }

            String globalVideoInfo = String.Join(",", videoInfo);
            jsonTemplate = jsonTemplate.Replace("${BOT_VIDEO_BROADCAST_INFO}", globalVideoInfo);

            AdaptiveCardTemplate template = new AdaptiveCardTemplate(jsonTemplate);

            // "Expand" the template - this generates the final Adaptive Card payload
            String? jsonData = CreateAdaptiveCardData();
            string cardJson = template.Expand(jsonData);
            
            // Create an Message Alternative Content
            MessageAlternativeContent messageAlternativeContent = new MessageAlternativeContent();
            messageAlternativeContent.Type = "form/json";
            messageAlternativeContent.Content = cardJson;

            alternativeContent = new List<MessageAlternativeContent> { messageAlternativeContent };
            // Get title of the question
            var json = JSON.Parse(jsonData);
            message = UtilJson.AsString(json, "title");
            if (String.IsNullOrEmpty(message))
                message = " "; // => Due to a bug in WebClient must not be empty/null

            return (message, alternativeContent);
        }

        private void AnswerToMenuMessage(MessageEventArgs? messageEvent)
        {   
            try
            {
                if (messageEvent != null)
                {
                    ManualResetEvent pause = new ManualResetEvent(false);

                    // Check if a conference is in progress
                    if (String.IsNullOrEmpty(_currentConferenceId))
                    {
                        Util.WriteDebugToConsole($"[{_botName}] MENU Message received from a Bot Manager but no conference is in progress ...");
                        RbInstantMessaging.SendMessageToConversationId(messageEvent.ConversationId, RainbowApplicationInfo.labelNoConferenceInProgress, null, UrgencyType.Low, null, callback =>
                        {
                            pause.Set();
                        });

                        pause.WaitOne();
                    }
                    else
                    {
                        (String? message, List<MessageAlternativeContent>? alternativeContent) = CreateAdaptiveCard();
                        if (alternativeContent?.Count > 0)
                        {
                            // Update the Adaptive Card in all Conversation already known
                            var keys = _adaptiveCardMessageIdByConversationId.Keys.ToList();
                            foreach (var key in keys)
                            {
                                var previousMessageId = _adaptiveCardMessageIdByConversationId[key];
                                RbInstantMessaging.EditMessage(key, previousMessageId, message, alternativeContent, callback =>
                                {
                                    if (callback.Result.Success)
                                    {
                                        Util.WriteDebugToConsole($"[{_botName}] MENU Message received from a Bot Manager - UPDATE previous AdaptiveCard in ConversationId:[{key}] - MessageID:[{previousMessageId}] ");
                                        // Store this message Id in order to update it later
                                        //_adaptiveCardMessageIdByConversationId[key] = callback.Data.Id;
                                    }
                                    else
                                    {
                                        Util.WriteErrorToConsole($"[{_botName}] MENU Message received from a Bot Manager - Cannot UPDATE Adaptive Card in ConversationId:[{key}] - Exception:[{callback.Result}]");
                                    }
                                    pause.Set();
                                });
                            }

                            // Check if have already send a AdaptiveCard in this Conversation
                            if (!_adaptiveCardMessageIdByConversationId.ContainsKey(messageEvent.ConversationId))
                            {
                                RbInstantMessaging.SendAlternativeContentsToConversationId(messageEvent.ConversationId, message, alternativeContent, UrgencyType.Std, null, callback =>
                                {
                                    if (callback.Result.Success)
                                    {
                                        // Store this message Id in order to update it later
                                        var messageId = callback.Data.Id;
                                        Util.WriteDebugToConsole($"[{_botName}] MENU Message received from a Bot Manager - send AdaptiveCard in ConversationId:[{messageEvent.ConversationId}] - MessageID:[{messageId}]");
                                        _adaptiveCardMessageIdByConversationId.Add(messageEvent.ConversationId, messageId);
                                    }
                                    else
                                    {
                                        Util.WriteErrorToConsole($"[{_botName}] MENU Message received from a Bot Manager - Cannot SEND Adaptive Card in ConversationId:[{messageEvent.ConversationId}] - Exception:[{callback.Result}]");
                                    }
                                    pause.Set();
                                });
                            }
                            pause.WaitOne();
                        }
                        else
                            Util.WriteWarningToConsole($"[{_botName}] MENU Message received from a Bot Manager - Cannot create Adaptive Card - ConversationId: [{messageEvent.ConversationId}]");
                    }
                }
            
            } catch (Exception exc)
            {
            }

            FireTrigger(Trigger.MessageManaged);
        }

        private String GetVideoTitleUsingUri(String uri)
        {
            string result = "";
            foreach(var video in videoInfos)
            {
                if(video.Uri == uri)
                {
                    result = video.Title;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// To answer to the specified message coming directly for another user
        /// </summary>
        private void AnswerToPeerMessage(MessageEventArgs? messageEvent)
        {
            if (messageEvent?.Message != null)
            {
                // Is it a response to an adaptive cards ? And if it's the case what is the answer ?
                (Boolean isAdaptiveCardAnswer, String? alternateContent) = GetAlternateContentFromAdaptiveCardResponse(messageEvent.Message);
                if (isAdaptiveCardAnswer && (alternateContent != null) ) 
                {
                    Util.WriteDebugToConsole($"[{_botName}] Message received from a Bot Manager using AdaptiveCard");

                    var json = JSON.Parse(alternateContent);
                    if(json != null)
                    {
                        // Check first if we have to stop or not
                        if ( (json["rainbow"] != null) && (json["rainbow"]["id"] != null) )
                        {
                            if(json["rainbow"]["id"].Value == "stop")
                            {
                                FireTrigger(Trigger.MessageManaged);
                                FireTrigger(Trigger.StopMessage);
                            }
                        }


                        RainbowBotVideoBroadcaster.State botState;
                        Boolean botCanContinue;
                        String botMessage;
                        
                        string toggle;
                        string choiceSet;
                        string videoTitle;

                        string sharingChoiceSet = "";
                        string sharingStreamChoiceSet = "";

                        if (json["Sharing_ChoiceSet"] != null)
                            sharingChoiceSet = UtilJson.AsString(json, "Sharing_ChoiceSet");

                        if (json["SharingStream_ChoiceSet"] != null)
                        {
                            sharingStreamChoiceSet = UtilJson.AsString(json, "SharingStream_ChoiceSet");
                            if (sharingStreamChoiceSet.Equals(RainbowApplicationInfo.labelNoVideo))
                                sharingStreamChoiceSet = "";
                        }

                        int index = 0;
                        foreach(var bot in botVideoBroadcasterInfos)
                        {
                            toggle = UtilJson.AsString(json, $"BOT{index}_Toggle");
                            choiceSet = UtilJson.AsString(json, $"BOT{index}_ChoiceSet");
                            if ( (!String.IsNullOrEmpty(toggle)) && (!String.IsNullOrEmpty(choiceSet)) ) 
                            {
                                if (choiceSet.Equals(RainbowApplicationInfo.labelNoVideo))
                                    choiceSet = "";

                                (botState, botCanContinue, botMessage) = bot.CheckBotStatus();

                                if ((botState != RainbowBotVideoBroadcaster.State.Created)
                                        && (botState != RainbowBotVideoBroadcaster.State.NotConnected))
                                {

                                    videoTitle = GetVideoTitleUsingUri(choiceSet);
                                    if (String.IsNullOrEmpty(videoTitle))
                                        videoTitle = bot.Name;

                                    var confId = _currentConferenceId;
                                    if (confId == null)
                                        confId = "";
                                    var joinConference = (toggle == "true");


                                    // Update Bot info:
                                    bot.Selected = joinConference;
                                    bot.Uri = choiceSet;
                                    bot.SharingUri = (sharingChoiceSet == bot.Name) ? sharingStreamChoiceSet : "";

                                    Dictionary <String, Object> dataToSend = new Dictionary<string, Object>();
                                    dataToSend.Add("joinConference", joinConference);
                                    dataToSend.Add("conferenceId", confId);
                                    dataToSend.Add("useName", videoTitle);
                                    dataToSend.Add("videoUri", bot.Uri);
                                    dataToSend.Add("sharingUri", bot.SharingUri);
                                    
                                    String dataToSendJsonString = UtilJson.JsonStringFromDictionaryOfStringAndObject(dataToSend);
                                    //Console.WriteLine($"[{_botName}] Message to send to [{bot.Name}]: [{dataToSendJsonString}]");

                                    var conversation = RbConversations.GetOrCreateConversationFromUserId(bot.Id);
                                    if (conversation != null)
                                    {
                                        MessageAlternativeContent messageAlternativeContent = new MessageAlternativeContent();
                                        messageAlternativeContent.Content = dataToSendJsonString;
                                        messageAlternativeContent.Type = "application/json";
                                        RbInstantMessaging.SendAlternativeContentsToConversationId(conversation.Id, null, new List<MessageAlternativeContent>() { messageAlternativeContent }, UrgencyType.Std, null, callbackMessage: callback =>
                                        {
                                            if (!callback.Result.Success)
                                            {
                                                Util.WriteErrorToConsole($"[{_botName}] Message cannot be send to [{bot.Name}] ....");
                                            }
                                        });
                                    }
                                    else
                                    {
                                        Util.WriteErrorToConsole($"[{_botName}] Message cannot be send to [{bot.Name}] because we cannot have a conversation Id....");
                                    }
                                }
                            }
                            index++;
                        }


                        // Update the Adaptive Card in all Conversation already known
                        (String? message, List<MessageAlternativeContent>? alternativeContent) = CreateAdaptiveCard();
                        if (alternativeContent?.Count > 0)
                        {
                            var keys = _adaptiveCardMessageIdByConversationId.Keys.ToList();
                            foreach (var key in keys)
                            {
                                var previousMessageId = _adaptiveCardMessageIdByConversationId[key];
                                RbInstantMessaging.EditMessage(key, previousMessageId, message, alternativeContent, callback =>
                                {
                                    if (callback.Result.Success)
                                    {
                                        Util.WriteDebugToConsole($"[{_botName}] MENU Message received from a Bot Manager - UPDATE previous AdaptiveCard in ConversationId:[{key}] - MessageID:[{previousMessageId}] ");
                                        // Store this message Id in order to update it later
                                        //_adaptiveCardMessageIdByConversationId[key] = callback.Data.Id;
                                    }
                                    else
                                    {
                                        Util.WriteErrorToConsole($"[{_botName}] MENU Message received from a Bot Manager - Cannot UPDATE Adaptive Card in ConversationId:[{key}] - Exception:[{callback.Result}]");
                                    }
                                });
                            }
                        }
                    }
                    else
                    {
                        // We have not received valid JSON Data ...
                        Util.WriteErrorToConsole($"[{_botName}] Message received doesn't contain valid JSON. We can't parse it");
                    }
                }
                else
                {
                    // We do nothing special since it's not an answer to an adaptive cards.
                    //Util.WriteDebugToConsole($"[{_botName}] Message received from a Bot Manager but can not use it.");
                }
            }
            FireTrigger(Trigger.MessageManaged);
        }

        /// <summary>
        /// On entry of the Connected State, we check the connection status with the server and if it's ok we dequeue invitations/messages
        /// </summary>
        private void CheckConnectedStateAndDequeudMessages()
        {
            // Check first if are still connected
            if (!RbApplication.IsConnected())
                FireTrigger(Trigger.Disconnect);

            // Check if a Bot has not been started / Created
            foreach (var bot in botVideoBroadcasterInfos)
            {
                RainbowBotVideoBroadcaster.State botState;
                Boolean botCanContinue;
                String botMessage;

                if (bot.Name != RainbowApplicationInfo.labelNone)
                {
                    (botState, botCanContinue, botMessage) = bot.CheckBotStatus();
                    // If a bot has just been created, we configure it and start login
                    if (botState == RainbowBotVideoBroadcaster.State.Created)
                    {
                        bot.Configure(RainbowApplicationInfo.commandStop, new BotManager(_currentContact.LoginEmail, _currentContact.Id, _currentContact.Jid_im));
                        break;
                    }
                    // If this bot is not already connected, we don't try to check another one
                    else if (botState != RainbowBotVideoBroadcaster.State.Connected)
                    {
                        break;
                    }
                }
            }

            // TO DO - we need to check taht all bot are running and ready before to continue here



            // Dequeue user's invitation
            if (_userInvitationQueue.TryDequeue(out String? invitationId))
            {
                if (invitationId != null)
                {
                    _machine.Fire(_userInvitationReceivedTrigger, invitationId);
                    return;
                }
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

            

            // Check if we are not in a conference and if a conference is in progress
            if (String.IsNullOrEmpty(_currentConferenceId) && (_conferencesInProgress.Count > 0) && (RbBubbles != null))
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

                                // Store the conference id
                                _currentConferenceId = bubble.Id;

                                Util.WriteDebugToConsole($"[{_botName}] This conference has been started - Bubble:[{bubble.Name}] - BubbleId:{bubble.Id}]. We will use it to broadcast videos");

                                // We check if the orchestrator is a moderator of this bubble
                                if(RbBubbles.IsModerator(bubble) == true)
                                {
                                    foreach(var bot in botVideoBroadcasterInfos)
                                    {
                                        if( (!String.IsNullOrEmpty(bot.Id)) && (!bot.Login.Equals(RainbowApplicationInfo.labelNone, StringComparison.InvariantCultureIgnoreCase)) )
                                        {
                                            if(RbBubbles.IsAccepted(bubble, bot.Id) != true)
                                            {
                                                Util.WriteWarningToConsole($"[{_botName}] The bot [{bot.Name}] is not a member of this bubble [{bubble.Name}]. We send an invitation to it.");
                                                RbBubbles.AddContactById(bubble.Id, bot.Id, Bubble.MemberPrivilege.User, callback: callback =>
                                                {  
                                                    if(!callback.Result.Success)
                                                    {
                                                        Util.WriteErrorToConsole($"[{_botName}] The bot [{bot.Name}] is not a member of this bubble AND we cannot add it: Exception[{callback.Result}]");
                                                    }
                                                }, true);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Util.WriteWarningToConsole($"[{_botName}] This bot is not a moderator of this conference - Bubble:[{bubble.Name}] - BubbleId:{bubble.Id}]. We cannot invite bot video broadcaster to this bubble if necessary.");
                                }


                                // Join conf only if asked
                                if (RainbowApplicationInfo.botVideoOrchestratorAutoJoinConference)
                                {
                                    _machine.Fire(_conferenceAvailableTrigger, confId);
                                    return; 
                                }
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

            // Now dequeue invitations, bubble's invitation or messages
            DequeuedMessages();
        }

        private void JoinConference()
        {
            if (_currentConferenceId != null)
            {
                RbWebRTCCommunications.JoinConference(_currentConferenceId, null, callback =>
                {
                    if (!callback.Result.Success)
                    {
                        _currentConferenceId = null;
                        Util.WriteErrorToConsole($"[{_botName}] Cannot Join Conference - BubbleID:[{_currentConferenceId}] - Error:[{callback.Result}");
                    }
                });
            }
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

            // We use XMPP for event mode
            RbApplication.Restrictions.EventMode = Restrictions.SDKEventMode.XMPP;

            // We want to use conference features
            RbApplication.Restrictions.UseConferences = true;

            // We want to use WebRTC
            RbApplication.Restrictions.UseWebRTC = true;
        }

        private void ResetValues()
        {
            _currentConferenceId = null;
            _currentCall = null;
            _isParticipantInConference = false;
        }

        /// <summary>
        /// To create all necessary objects from the Rainbow SDK C#
        /// </summary>
        private Boolean CreateRainbowObjects()
        {
            RbAutoReconnection = RbApplication.GetAutoReconnection();
            RbAutoReconnection.MaxNbAttempts = 5; // For tests purpose wu use here a low value

            RbBubbles = RbApplication.GetBubbles();
            RbConferences = RbApplication.GetConferences();
            RbContacts = RbApplication.GetContacts();
            RbConversations = RbApplication.GetConversations();
            RbInstantMessaging = RbApplication.GetInstantMessaging();
            RbInvitations = RbApplication.GetInvitations();

            try
            {
                RbWebRTCDesktopFactory = new Rainbow.WebRTC.Desktop.WebRTCFactory();
                RbWebRTCCommunications = Rainbow.WebRTC.WebRTCCommunications.GetOrCreateInstance(RbApplication, RbWebRTCDesktopFactory);
            }
            catch (Exception ex)
            {
                Util.WriteErrorToConsole($"[{_botName}] Initialization failed ... \r\nException:[{Rainbow.Util.SerializeException(ex)}] \r\nPossible reason: you have not SDL2 libraries in the executable folder path");
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

            RbInstantMessaging.MessageReceived -= RbInstantMessaging_MessageReceived;

            RbInvitations.InvitationReceived -= RbInvitations_InvitationReceived;

            RbBubbles.BubbleInvitationReceived -= RbBubbles_BubbleInvitationReceived;

            RbConferences.ConferenceRemoved -= RbConferences_ConferenceRemoved;
            RbConferences.ConferenceUpdated -= RbConferences_ConferenceUpdated;
            RbConferences.ConferenceParticipantsUpdated -= RbConferences_ConferenceParticipantsUpdated;

            RbWebRTCCommunications.CallUpdated -= RbWebRTCCommunications_CallUpdated;            
        }

        /// <summary>
        /// To unsubscribe to all necessary events previously subscribe
        /// </summary>
        private void SubscribeToRainbowEvents()
        {
            if (RbApplication == null)
                return;

            RbApplication.AuthenticationSucceeded += RbApplication_AuthenticationSucceeded;
            RbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;
            RbApplication.InitializationPerformed += RbApplication_InitializationPerformed;

            RbAutoReconnection.MaxNbAttemptsReached += RbAutoReconnection_MaxNbAttemptsReached;
            RbAutoReconnection.Cancelled += RbAutoReconnection_Cancelled;

            RbInstantMessaging.MessageReceived += RbInstantMessaging_MessageReceived;

            RbInvitations.InvitationReceived += RbInvitations_InvitationReceived;

            RbBubbles.BubbleInvitationReceived += RbBubbles_BubbleInvitationReceived;

            RbConferences.ConferenceRemoved += RbConferences_ConferenceRemoved;
            RbConferences.ConferenceUpdated += RbConferences_ConferenceUpdated;
            RbConferences.ConferenceParticipantsUpdated += RbConferences_ConferenceParticipantsUpdated;

            RbWebRTCCommunications.CallUpdated += RbWebRTCCommunications_CallUpdated;
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

            switch(e.State)
            {
                case ConnectionState.Connected:
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

            RbBubbles.GetAllBubbles(callback =>
            {
                if (callback.Result.Success)
                {
                    // We need to accept any invitation available from bubbles
                    var bubbles = callback.Data;
                    foreach (var bubble in bubbles)
                    {
                        if (RbBubbles.IsInvited(bubble) == true)
                        {
                            if (!_bubbleInvitationInProgress.Contains(bubble.Id))
                                _bubbleInvitationQueue.Enqueue(bubble.Id);
                        }
                    }
                }
            });


            RbInvitations.GetReceivedInvitations(status: Invitation.InvitationStatus.Pending, callback: callback =>
            {
                if (callback.Result.Success)
                {
                    var list = callback.Data;
                    if (list?.Count > 0)
                    {
                        foreach (var invitation in list)
                        {
                            // Queue user invitation
                            if (!_userInvitationQueue.Contains(invitation.Id))
                                _userInvitationQueue.Enqueue(invitation.Id);
                        }
                        
                    }
                }
            });


            //// We need to get contact id of each bot !
            //var contacts = RbContacts.GetAllContactsFromCache();
            //Boolean canContinue = true;
            //foreach(var bot in botVideoBroadcasterInfos)
            //{
            //    if(bot.Login != RainbowApplicationInfo.labelNone)
            //    {
            //        var contact = contacts.Where(c => c.LoginEmail.Equals(bot.Login, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            //        if (contact != null)
            //            bot.Id = contact.Id;
            //        else
            //        {
            //            Util.WriteErrorToConsole($"[{_botName} The [{bot.Name}] is UNKNOWN so we cannot have its id ")
            //            // TODO - Need to log and quit
            //        }
            //    }
            //}

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
            if(RbAutoReconnection.CurrentNbAttempts < RbAutoReconnection.MaxNbAttempts)
            {
                if(_machine.IsInState(State.AutoReconnection))
                    FireTrigger(Trigger.IncorrectCredentials);
            }
        }

        /// <summary>
        /// Raised when a IM message from a bubble or direclty from a user has been received
        /// </summary>
        private void RbInstantMessaging_MessageReceived(object? sender, MessageEventArgs messageEvent)
        {
            // We don't want to manage message coming from ourself
            if (messageEvent.ContactJid?.Equals(_currentContact.Jid_im, StringComparison.InvariantCultureIgnoreCase) == false)
            {
                // Queue message
                _messageQueue.Enqueue(messageEvent);

                //// If we are in state Connected, we ask to dequeue messages
                //if (_machine.IsInState(State.Connected))
                //    _machine.Fire(Trigger.DequeueMessages);
            }
        }

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
                    RbContacts.SetPresenceLevel(RbContacts.CreatePresence(true, "online", ""));
                }
            }
        }

        private void RbConferences_ConferenceParticipantsUpdated(object? sender, ConferenceParticipantsEventArgs e)
        {
            if (_currentContact == null)
                return;

            _isParticipantInConference = (e.Participants?.Count > 0) && e.Participants.ContainsKey(_currentContact.Id);
        }

        private void RbConferences_ConferenceUpdated(object? sender, ConferenceEventArgs e)
        {
            if (e.Conference == null)
                return;

            if (e.Conference.Active)
            {
                // Store info about this conference
                if (!_conferencesInProgress.Contains(e.Conference.Id))
                    _conferencesInProgress.Add(e.Conference.Id);
            }
        }

        private void RbConferences_ConferenceRemoved(object? sender, IdEventArgs e)
        {
            if (e.Id == _currentConferenceId)
            {
                Util.WriteDebugToConsole($"[{_botName}] This conference has been stopped: [{_currentConferenceId}] - We no more use it to broadcast videos");
                // TODO - inform all bots
                ResetValues();
            }

            // Remove info about this conference
            _conferencesInProgress.Remove(e.Id);
        }

        private void RbBubbles_BubbleInvitationReceived(object? sender, BubbleInvitationEventArgs bubbleInvitationEvent)
        {
            // Queue bubble invitation
            if (!_bubbleInvitationInProgress.Contains(bubbleInvitationEvent.BubbleId))
                _bubbleInvitationQueue.Enqueue(bubbleInvitationEvent.BubbleId);
        }

        private void RbInvitations_InvitationReceived(object? sender, InvitationEventArgs e)
        {
            // Queue user invitation
            if (!_userInvitationQueue.Contains(e.InvitationId))
                _userInvitationQueue.Enqueue(e.InvitationId);   
        }


    #endregion EVENTS RAISED FROM RAINBOW OBJECTS

#endregion PRIVATE API

#region PUBLIC API

        /// <summary>
        /// Constructor for the RainbowBotBase class
        /// </summary>
        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public RainbowBotVideoOrchestrator()
        {
            // Instantiate a new state machine in the Created state
            _machine = new StateMachine<State, Trigger>(State.Created);

            ConfigureStateMachine();

            _messageQueue = new ConcurrentQueue<MessageEventArgs>();
            _bubbleInvitationQueue = new ConcurrentQueue<String>();
            _userInvitationQueue = new ConcurrentQueue<String>();
        }
        #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        
        ~RainbowBotVideoOrchestrator()
        {
            UnsubscribeToRainbowEvents();
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
        public Boolean Configure(String appId, String appSecretKey, String hostname, String login, String pwd, String botName, List<BotManager>? botManagers, String? iniFolderFullPathName = null, String? iniFileName = null)
        {
            // Check that the trigger can be used
            Trigger triggerToUse = Trigger.Configure;
            if (!_machine.CanFire(triggerToUse))
                return false;

            // Create Rainbow Application (root object of th SDK)
            RbApplication = new Rainbow.Application(iniFolderFullPathName, iniFileName);

            // Set APP_ID, APP_SECRET_KET and HOSTNAME
            RbApplication.SetApplicationInfo(appId, appSecretKey);
            RbApplication.SetHostInfo(hostname);

            RbApplication.SetTimeout(10000);

            // Set restrictions
            SetRainbowRestrictions();

            // Create others Rainbow SDK Objects
            if (!CreateRainbowObjects())
                return false;

            // Store login/pwd
            _botLogin = login;
            _botPwd = pwd;
            _botName = botName;

            // Store list of mangers for this bot
            _botManagers = botManagers;

            // Create list of VideoInfo
            if (RainbowApplicationInfo.videosUri == null)
                return false;
            videoInfos = new List<VideoInfo>();
            videoInfos.Add(new VideoInfo(RainbowApplicationInfo.labelNoVideo));
            int index = 0;
            foreach(var uri in RainbowApplicationInfo.videosUri)
            {
                var videoInfo = new VideoInfo(uri);
                if(RainbowApplicationInfo.labelVideosUriName?.Count > index)
                    videoInfo.Title = RainbowApplicationInfo.labelVideosUriName[index];
                videoInfos.Add(videoInfo);
                index++;
            }

            // Create list of BotVideoBroadcasterInfo
            if (RainbowApplicationInfo.botsVideoBroadcaster == null)
                return false;
            botVideoBroadcasterInfos = new List<BotVideoBroadcasterInfo>();
            var botVideoBroadcasterNone = new BotVideoBroadcasterInfo(RainbowApplicationInfo.labelNone, RainbowApplicationInfo.labelNone);
            botVideoBroadcasterNone.Name = botVideoBroadcasterNone.Uri = RainbowApplicationInfo.labelNone;
            botVideoBroadcasterInfos.Add(botVideoBroadcasterNone);
            index = 0;
            foreach (var account in RainbowApplicationInfo.botsVideoBroadcaster)
            {
                var botVideoBroadcasterInfo = new BotVideoBroadcasterInfo(account.Login, account.Password);
                
                // Set Bot name if specified
                if (RainbowApplicationInfo.labelBotsVideoBroadcasterName?.Count > index)
                    botVideoBroadcasterInfo.Name = RainbowApplicationInfo.labelBotsVideoBroadcasterName[index];
                else
                    botVideoBroadcasterInfo.Name = $"CCTV {index+1}";

                // Set Bot default URI
                if (RainbowApplicationInfo.videosUri?.Count > 0)
                {
                    if (RainbowApplicationInfo.videosUri.Count > index)
                        botVideoBroadcasterInfo.Uri = RainbowApplicationInfo.videosUri[index];
                    else
                        botVideoBroadcasterInfo.Uri = RainbowApplicationInfo.videosUri[index % RainbowApplicationInfo.videosUri.Count];
                }

                //if (index == 0)
                //    botVideoBroadcasterInfo.SharingSelected = true;

                botVideoBroadcasterInfos.Add(botVideoBroadcasterInfo);
                index++;
                if ((RainbowApplicationInfo.nbMaxVideoBroadcaster > 0) && (index == RainbowApplicationInfo.nbMaxVideoBroadcaster))
                    break;
            }

            // Fire the trigger
            return FireTrigger(triggerToUse);
        }

        /// <summary>
        /// Start login process
        /// </summary>
        /// <returns>True if the login process has started</returns>
        public Boolean StartLogin()
        {
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
