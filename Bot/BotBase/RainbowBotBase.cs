using System;
using System.Collections.Generic;
using System.Text;
using Stateless;
using Stateless.Graph;
using Rainbow;
using Rainbow.Model;
using Rainbow.Events;
using System.Collections.Concurrent;
using System.Threading;

namespace RainbowBotBase
{
    public class RainbowBotBase
    {
        private readonly String STOP_MESSAGE = "Bot please stop";
        private readonly String AUTOMATIC_ANSWER_P2P = "Automatic answer ! I'm a Bot using SDK C# :). Thanks for your message ! [P2P]";
        private readonly String AUTOMATIC_ANSWER_BUBBLE = "Automatic answer ! I'm a Bot using SDK C# :). Thanks for your message ! [Bubble]";
        private readonly int MESSAGE_READ_BY_SECOND = 10; // nb messages READ by seconds. We will wait the necessary delay to read the next one. No message are lost. They are just read step by step avoiding a flood

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

            StopMessageReceived,

            MessageFromBubble,
            MessageFromPeer,

            BubbleInvitationReceived,
            InvitationReceived,
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

            DequeueInvitationsAndMessages,

            MessageReceivedFromBubble,
            MessageReceivedFromPeer,
            StopMessage,

            MessageManaged,

            BubbleInvitationReceived,
            BubbleInvitationManaged,

            InvitationReceived,
            InvitationManaged,
        }

        private readonly StateMachine<State, Trigger> _machine;

        // The TriggerWithParameters object is used when a trigger requires a payload.
        private StateMachine<State, Trigger>.TriggerWithParameters<MessageEventArgs>? _messageReceivedFromBubbleTrigger;
        private StateMachine<State, Trigger>.TriggerWithParameters<MessageEventArgs>? _messageReceivedFromPeerTrigger;
        private StateMachine<State, Trigger>.TriggerWithParameters<BubbleInvitationEventArgs>? _bubbleInvitationReceivedTrigger;
        private StateMachine<State, Trigger>.TriggerWithParameters<InvitationEventArgs>? _invitationReceivedTrigger;


        // Concurrent queue of messages received
        private readonly ConcurrentQueue<MessageEventArgs> _messageQueue;

        // Concurrent queue of invitations received to be  member of a bubble
        private readonly ConcurrentQueue<BubbleInvitationEventArgs> _bubbleInvitationQueue;

        // Concurrent queue of invitations received to sare its presence with other users
        private readonly ConcurrentQueue<InvitationEventArgs> _invitationQueue;

        // list of messages' datetime. use to avoid the flod
        private readonly List<DateTime> _messageDateList;

        // Several internal variables
        private Trigger _lastTrigger;
        private String? _login;
        private String? _pwd;
        private String? _masterBotJid = null;
        private String? _masterBotEmail;
        private Contact _currentContact;

        // Define all Rainbow objects we use
        private Rainbow.Application RbApplication;
        private Rainbow.AutoReconnection RbAutoReconnection;
        private Rainbow.Bubbles RbBubbles;
        private Rainbow.Contacts RbContacts;
        private Rainbow.Invitations RbInvitations;
        private Rainbow.InstantMessaging RbInstantMessaging;

#region PRIVATE API

        /// <summary>
        /// To create / configure all the state machine
        /// </summary>
        private void ConfigureStateMachine()
        {
            // Create Trigger(s) using parameters
            _messageReceivedFromPeerTrigger = _machine.SetTriggerParameters<MessageEventArgs>(Trigger.MessageReceivedFromPeer);
            _messageReceivedFromBubbleTrigger = _machine.SetTriggerParameters<MessageEventArgs>(Trigger.MessageReceivedFromBubble);
            _bubbleInvitationReceivedTrigger = _machine.SetTriggerParameters<BubbleInvitationEventArgs>(Trigger.BubbleInvitationReceived);
            _invitationReceivedTrigger = _machine.SetTriggerParameters<InvitationEventArgs>(Trigger.InvitationReceived);

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
                .OnEntry(CheckConnectedState)

                .PermitReentry(Trigger.DequeueInvitationsAndMessages)

                .Permit(Trigger.Disconnect, State.AutoReconnection)

                .Permit(Trigger.StopMessage, State.StopMessageReceived)
                .PermitIf(_messageReceivedFromBubbleTrigger, State.MessageFromBubble)
                .PermitIf(_messageReceivedFromPeerTrigger, State.MessageFromPeer)

                .PermitIf(_bubbleInvitationReceivedTrigger, State.BubbleInvitationReceived )
                .PermitIf(_invitationReceivedTrigger, State.InvitationReceived);

            // Configure the AutoReconnection state
            _machine.Configure(State.AutoReconnection)
                .PermitReentry(Trigger.Disconnect)
                .Permit(Trigger.TooManyAttempts, State.NotConnected)
                .Permit(Trigger.IncorrectCredentials, State.Created)
                .Permit(Trigger.AuthenticationSucceeded, State.Authenticated);

            // Configure the LogOut state
            _machine.Configure(State.StopMessageReceived)
                .OnEntry(LogoutBot)
                .Permit(Trigger.Disconnect, State.NotConnected);

            // Configure the MessageFromBubble state
            _machine.Configure(State.MessageFromBubble)
                .OnEntryFrom(_messageReceivedFromBubbleTrigger, AnswerToBubbleMessage)
                .Permit(Trigger.MessageManaged, State.Connected);

            // Configure the MessageFromPeer state
            _machine.Configure(State.MessageFromPeer)
                .OnEntryFrom(_messageReceivedFromPeerTrigger, AnswerToPeerMessage)
                .Permit(Trigger.MessageManaged, State.Connected);

            // Configure the InvitationReceived state
            _machine.Configure(State.BubbleInvitationReceived)
                .OnEntryFrom(_bubbleInvitationReceivedTrigger, AnswerToBubbleInvitation)
                .Permit(Trigger.BubbleInvitationManaged, State.Connected);

            // Configure the InvitationReceived state
            _machine.Configure(State.InvitationReceived)
                .OnEntryFrom(_invitationReceivedTrigger, AnswerToInvitation)
                .Permit(Trigger.InvitationManaged, State.Connected);

            _machine.OnUnhandledTrigger( (state, trigger) => Console.WriteLine($"OnUnhandledTrigger - State: {state} with Trigger: {trigger}"));
            
            _machine.OnTransitionCompleted(transition => {
                // Store the trigger used
                _lastTrigger = transition.Trigger;

                // Log info about transition
                Console.WriteLine($"OnTransitionCompleted- State: {transition.Source} -> {transition.Destination} with Trigger: {transition.Trigger}({string.Join(", ", transition.Parameters)})");
            });
        }

        /// <summary>
        /// To dequeue in this oreder: 
        ///     - invitations to share presence
        ///     - invitations to be a member of a bubble
        ///     - message received (we avoid a flod here using MESSAGE_READ_BY_SECOND variable)
        /// </summary>
        private void DequeueInvitationsAndMessages()
        {
            // We have to dequeue invitations then bubble's invitation and finally messages


            // Dequeue invitation
            if (_invitationQueue.TryDequeue(out InvitationEventArgs? invitation))
            {
                if (invitation != null)
                {
                    _machine.Fire(_invitationReceivedTrigger, invitation);
                    return;
                }
            }

            // Dequeue bubble's invitation
            if (_bubbleInvitationQueue.TryDequeue(out BubbleInvitationEventArgs? bubbleInvitation))
            {
                if (bubbleInvitation != null)
                {
                    _machine.Fire(_bubbleInvitationReceivedTrigger, bubbleInvitation);
                    return;
                }
            }

            // Dequeue messages
            if (_messageQueue.Count > 0)
            {
                //  /!\ Here we add a protection to avoid to manage a flood of messages using a restriction: a nb of messages to read by sedonc (using MESSAGE_READ_BY_SECOND)
                Boolean canReadMessage = false;
                if (_messageDateList.Count < MESSAGE_READ_BY_SECOND)
                {
                    canReadMessage = true;
                    _messageDateList.Add(DateTime.Now);
                }
                else
                {
                    TimeSpan span = DateTime.Now - _messageDateList[0];
                    if (span.TotalMilliseconds >= 1000)
                    {
                        canReadMessage = true;
                        _messageDateList.RemoveAt(0);
                        _messageDateList.Add(DateTime.Now);
                    }
                }

                if (canReadMessage)
                {
                    if (_messageQueue.TryDequeue(out MessageEventArgs? messageEvent))
                    {
                        if (messageEvent != null)
                        {
                            // Get Jif of the master bot
                            if (_masterBotJid == null)
                            {
                                Contact contact = RbContacts.GetContactFromContactJid(messageEvent.ContactJid);
                                if (contact != null)
                                    _masterBotJid = contact.LoginEmail;
                                else
                                    RbContacts.GetContactFromContactJidFromServer(messageEvent.ContactJid);
                            }

                            // Check if it's the master bot who sent this message
                            if ((_masterBotJid != null) && _masterBotJid.Equals(_masterBotEmail, StringComparison.InvariantCultureIgnoreCase))
                            {
                                // Check if it's the "stop message"
                                if (messageEvent.Message.Content.Equals(STOP_MESSAGE, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _machine.Fire(Trigger.StopMessage);
                                    return;
                                }
                            }

                            // Is it a message from a Bubble or from a Peer ?
                            if (String.IsNullOrEmpty(messageEvent.Message.FromBubbleJid))
                                _machine.Fire(_messageReceivedFromPeerTrigger, messageEvent);
                            else
                                _machine.Fire(_messageReceivedFromBubbleTrigger, messageEvent);
                            return;
                        }
                        
                    }
                }
                else
                {
                    CancelableDelay.StartAfter(10, DequeueInvitationsAndMessages);
                }
            }
        }

        /// <summary>
        /// To logout the bot form Rainbow server
        /// </summary>
        private void LogoutBot()
        {
            RbApplication.Logout();
        }

        /// <summary>
        /// To answer to the specified message coming from a bubble
        /// </summary>
        private void AnswerToBubbleMessage(MessageEventArgs? messageEvent)
        {
            if (messageEvent != null)
            {
                String conversationId = messageEvent.ConversationId;

                RbInstantMessaging.SendMessageToConversationId(conversationId, AUTOMATIC_ANSWER_BUBBLE, null);
            }
            FireTrigger(Trigger.MessageManaged);
        }

        /// <summary>
        /// To answer to the specified message coming directly for another user
        /// </summary>
        private void AnswerToPeerMessage(MessageEventArgs? messageEvent)
        {
            if (messageEvent != null)
            {
                String conversationId = messageEvent.ConversationId;

                RbInstantMessaging.SendMessageToConversationId(conversationId, AUTOMATIC_ANSWER_P2P, null);
            }
            FireTrigger(Trigger.MessageManaged);
        }

        /// <summary>
        /// To answer to the specified invitations to be a member of a bubble
        /// </summary>
        private void AnswerToBubbleInvitation(BubbleInvitationEventArgs? bubbleInvitation)
        {
            if(bubbleInvitation != null)
            {
                RbBubbles.AcceptInvitation(bubbleInvitation.BubbleId);
            }

            FireTrigger(Trigger.BubbleInvitationManaged);
        }

        /// <summary>
        /// To answer to the specified invitations to share its presence with another user
        /// </summary>
        private void AnswerToInvitation(InvitationEventArgs? invitation)
        {
            if (invitation != null)
            {
                RbInvitations.AcceptReceivedPendingInvitation(invitation.InvitationId);
            }

            FireTrigger(Trigger.InvitationManaged);
        }

        /// <summary>
        /// On entry of the Connected State, we check the connection status with the server and if it's ok we dequeue invitations/messages
        /// </summary>
        private void CheckConnectedState()
        {
            // Check first if are stille connected
            if (!RbApplication.IsConnected())
                FireTrigger(Trigger.Disconnect);

            // Now dequeue invitations, bubble's invitation or messages
            DequeueInvitationsAndMessages();
        }

        /// <summary>
        /// To specify restrictions to use in the Rainbow SDK - for example we want to use the AutoReconnection service and don' want to store message
        /// </summary>
        private void SetRainbowRestictions()
        {
            // Since we are using a Bot, no storage is necessary for message sent by it.
            RbApplication.Restrictions.MessageStorageMode = Restrictions.SDKMessageStorageMode.NoStore;

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
            RbApplication.Restrictions.UseAPIConferenceV2 = true;
        }

        /// <summary>
        /// To create all necessary objects from the Rainbow SDK C#
        /// </summary>
        private void CreateRainbowObjects()
        {
            RbAutoReconnection = RbApplication.GetAutoReconnection();
            RbAutoReconnection.MaxNbAttempts = 5; // For tests purpose wu use here a low value

            RbBubbles = RbApplication.GetBubbles();
            RbContacts = RbApplication.GetContacts();
            RbInstantMessaging = RbApplication.GetInstantMessaging();
            RbInvitations = RbApplication.GetInvitations();

            SubscribeToRainbowEvents();
        }

        /// <summary>
        /// To subscribe to all necessary events from the Rainbow SDK C#
        /// </summary>
        private void UnsubscribeToRainbowEvents()
        {
            RbApplication.AuthenticationSucceeded -= RbApplication_AuthenticationSucceeded;
            RbApplication.ConnectionStateChanged -= RbApplication_ConnectionStateChanged;
            RbApplication.InitializationPerformed -= RbApplication_InitializationPerformed;

            RbAutoReconnection.MaxNbAttemptsReached -= RbAutoReconnection_MaxNbAttemptsReached;
            RbAutoReconnection.Cancelled -= RbAutoReconnection_Cancelled;

            RbBubbles.BubbleInvitationReceived -= RbBubbles_BubbleInvitationReceived;

            RbInstantMessaging.MessageReceived -= RbInstantMessaging_MessageReceived;

            RbInvitations.InvitationReceived -= RbInvitations_InvitationReceived;
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

            RbBubbles.BubbleInvitationReceived += RbBubbles_BubbleInvitationReceived;

            RbInstantMessaging.MessageReceived += RbInstantMessaging_MessageReceived;

            RbInvitations.InvitationReceived += RbInvitations_InvitationReceived;
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
                    if(RbApplication.IsInitialized())
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

            // We ask server all bubbles information. For each of them, the SDK will automatically "register" to them. It's mandatory to receive message send to a bubble.
            RbBubbles.GetAllBubbles();

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
        /// Raised when a invitation to share presence with another user has been received
        /// </summary>
        private void RbInvitations_InvitationReceived(object? sender, InvitationEventArgs invitationEvent)
        {
            // Queue invitation
            _invitationQueue.Enqueue(invitationEvent);

            // If we are in state Connected, we ask to dequeue invitations and messages
            if (_machine.IsInState(State.Connected))
                _machine.Fire(Trigger.DequeueInvitationsAndMessages);
        }

        /// <summary>
        /// Raised when a invitation to be a member of a bubble has been received
        /// </summary>
        private void RbBubbles_BubbleInvitationReceived(object? sender, BubbleInvitationEventArgs bubbleInvitationEvent)
        {
            // Queue bubble invitation
            _bubbleInvitationQueue.Enqueue(bubbleInvitationEvent);

            // If we are in state Connected, we ask to dequeue invitations and messages
            if (_machine.IsInState(State.Connected))
                _machine.Fire(Trigger.DequeueInvitationsAndMessages);
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

                // If we are in state Connected, we ask to dequeue invitations and messages
                if (_machine.IsInState(State.Connected))
                    _machine.Fire(Trigger.DequeueInvitationsAndMessages);
            }
        }

    #endregion EVENTS RAISED FROM RAINBOW OBJECTS

#endregion PRIVATE API

#region PUBLIC API

        /// <summary>
        /// Constructor for the RainbowBotBase class
        /// </summary>
        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public RainbowBotBase()
        {
            // Instantiate a new state machine in the Created state
            _machine = new StateMachine<State, Trigger>(State.Created);

            ConfigureStateMachine();

            _messageQueue = new ConcurrentQueue<MessageEventArgs>();
            _bubbleInvitationQueue = new ConcurrentQueue<BubbleInvitationEventArgs>();
            _invitationQueue = new ConcurrentQueue<InvitationEventArgs>();

            _messageDateList = new List<DateTime>();
        }
        #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        
        ~RainbowBotBase()
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
        public Boolean Configure(String appId, String appSecretKey, String hostname, String login, String pwd, String masterBotEmail, String? iniFolderFullPathName = null, String? iniFileName = null)
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

            // Set restrictions
            SetRainbowRestictions();

            // Create others Rainbow SDK Objects
            CreateRainbowObjects();

            // Store login/pwd
            _login = login;
            _pwd = pwd;

            // Store master bot email
            _masterBotEmail = masterBotEmail;

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
                RbApplication.Login(_login, _pwd, callbackLogin =>
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
