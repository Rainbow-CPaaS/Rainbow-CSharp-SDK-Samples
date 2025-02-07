using BotLibrary.Model;
using Microsoft.Extensions.Logging;
using Rainbow;
using Rainbow.Consts;
using Rainbow.Enums;
using Rainbow.Model;
using Rainbow.SimpleJSON;
using Stateless;
using Stateless.Graph;
using System.Collections.Concurrent;
using System.Text;

namespace BotLibrary
{
    /// <summary>
    /// State list used by this bot
    /// </summary>
    public enum State
    {
        Created,

        NotConnected,
        Connecting,
        Authenticated,
        Connected,

        Stopped,

        CheckingDataAvailability,

        ManageBubbleInvitationReceived,
        ManageUserInvitationReceived,

        ManageAckMessageReceived,
        ManageApplicationMessageReceived,
        ManageInstantMessageReceived,
        ManageInternalMessageReceived,
    }

    /// <summary>
    /// Trigger list used by this bot
    /// </summary>
    public enum Trigger
    {
        Configure,

        StartLogin,
        Disconnect,
        Connect,
        AuthenticationSucceeded,

        UserInvitation,
        BubbleInvitation,
        AckMessage,
        InstantMessage,
        ApplicationMessage,
        InternalMessage,

        NextStep,

        Stop
    }

    public class BotBase
    {
        internal ILogger log;

        private const String BOT_CONFIGURATION = "botConfiguration";

        private readonly StateMachine<State, Trigger> _machine;

        // Concurrent queue of data received:
        //  - AckMessages
        //  - ApplicationMessages
        //  - IMMessages
        //  - Bubble invitations
        //  - User invitations
        private readonly ConcurrentQueue<BubbleInvitation> _queueBubbleInvitationsReceived;
        private readonly ConcurrentQueue<Invitation> _queueUserInvitationsReceived;

        private readonly ConcurrentQueue<Message> _queueInstantMessagesReceived;
        private readonly ConcurrentQueue<AckMessage> _queueAckMessagesReceived;
        private readonly ConcurrentQueue<ApplicationMessage> _queueApplicationMessagesReceived;
        
        private readonly ConcurrentQueue<InternalMessage> _queueInternalMessagesReceived;

        private Credentials _credentials;
        private BotConfiguration _botConfiguration;
        private JSONNode _jsonNodeBotConfiguration;

        // Several internal variables
        private Contact? _currentContact;
        private Trigger _lastTrigger;
        private SdkError? _botCancelledSdkError = null;

        // Define all Rainbow objects we use
        private Rainbow.Application _rbApplication;
        private Rainbow.AutoReconnection _rbAutoReconnection;
        private Rainbow.Bubbles _rbBubbles;
        private Rainbow.Contacts _rbContacts;
        private Rainbow.InstantMessaging _rbInstantMessaging;
        private Rainbow.Invitations _rbInvitations;
        private Rainbow.FileStorage _rbFileStorage;

#region PRIVATE API

        /// <summary>
        /// To create / configure all the state machine
        /// </summary>
        private void ConfigureStateMachine()
        {
            // Configure the Configured state
            _machine.Configure(State.Created)
                .Permit(Trigger.Configure, State.NotConnected);

            // Configure the Stopped state
            _machine.Configure(State.Stopped)
                .OnEntryAsync(OnEntryStoppedAsync)
                .Permit(Trigger.StartLogin, State.Connecting);

            // Configure the Disconnected state
            _machine.Configure(State.NotConnected)
                .Permit(Trigger.StartLogin, State.Connecting)
                .Permit(Trigger.Stop, State.Stopped);

            // Configure the Connecting state
            _machine.Configure(State.Connecting)
                .Permit(Trigger.Disconnect, State.NotConnected)
                .Permit(Trigger.Connect, State.Connected)
                .Permit(Trigger.AuthenticationSucceeded, State.Authenticated);

            // Configure the Connecting state
            _machine.Configure(State.Authenticated)
                .Permit(Trigger.Connect, State.Connected)
                .Permit(Trigger.Disconnect, State.NotConnected);

            // Configure the Connected state
            _machine.Configure(State.Connected)
                .OnEntryAsync(OnEntryConnectedAsync)
                .Permit(Trigger.NextStep, State.CheckingDataAvailability)
                .Permit(Trigger.Disconnect, State.NotConnected);

            // Configure the ManageBubbleInvitations state
            _machine.Configure(State.CheckingDataAvailability)
                .OnEntryAsync(OnEntryCheckDataAvailabilityAsync)
                .SubstateOf(State.Connected)
                .PermitReentry(Trigger.NextStep)
                .Permit(Trigger.BubbleInvitation, State.ManageBubbleInvitationReceived)
                .Permit(Trigger.UserInvitation, State.ManageUserInvitationReceived)
                .Permit(Trigger.AckMessage, State.ManageAckMessageReceived)
                .Permit(Trigger.ApplicationMessage, State.ManageApplicationMessageReceived)
                .Permit(Trigger.InternalMessage, State.ManageInternalMessageReceived)
                .Permit(Trigger.InstantMessage, State.ManageInstantMessageReceived);
            
            // Configure the ManageBubbleInvitations state
            _machine.Configure(State.ManageBubbleInvitationReceived)
                .OnEntryAsync(OnEntryManageBubbleInvitationReceivedAsync)
                .SubstateOf(State.Connected)
                .Permit(Trigger.NextStep, State.CheckingDataAvailability);

            // Configure the ManageUserInvitations state
            _machine.Configure(State.ManageUserInvitationReceived)
                .OnEntryAsync(OnEntryManageUserInvitationReceivedAsync)
                .SubstateOf(State.Connected)
                .Permit(Trigger.NextStep, State.CheckingDataAvailability);
            
            // Configure the SubStateManageMessages state
            _machine.Configure(State.ManageAckMessageReceived)
                .OnEntryAsync(OnEntryManageAckMessageReceivedAsync)
                .SubstateOf(State.Connected)
                .Permit(Trigger.NextStep, State.CheckingDataAvailability);

            // Configure the SubStateManageMessages state
            _machine.Configure(State.ManageApplicationMessageReceived)
                .OnEntryAsync(OnEntryManageApplicationMessageReceivedAsync)
                .SubstateOf(State.Connected)
                .Permit(Trigger.NextStep, State.CheckingDataAvailability);

            // Configure the SubStateManageMessages state
            _machine.Configure(State.ManageInternalMessageReceived)
                .OnEntryAsync(OnEntryManageInternalMessageReceivedAsync)
                .SubstateOf(State.Connected)
                .Permit(Trigger.NextStep, State.CheckingDataAvailability);

            // Configure the SubStateManageMessages state
            _machine.Configure(State.ManageInstantMessageReceived)
                .OnEntryAsync(OnEntryManageInstantMessageReceivedAsync)
                .SubstateOf(State.Connected)
                .Permit(Trigger.NextStep, State.CheckingDataAvailability);

            _machine.OnUnhandledTrigger((state, trigger) => Util.WriteWarningToConsole($"[{BotName}] OnUnhandledTrigger - State: {state} with Trigger: {trigger}"));

            _machine.OnTransitionCompleted(transition => {
                // Store the trigger used
                _lastTrigger = transition.Trigger;

                // Log info about transition
                var parameters = string.Join(", ", transition.Parameters);
                Util.WriteInfoToConsole($"[{DateTime.Now:HH:mm:ss.fff}][{_credentials.UserConfig.Prefix}] State [{transition.Destination}] from [{transition.Source}] with Trigger: [{transition.Trigger}]{(String.IsNullOrEmpty(parameters) ? "" : " Parameter(s):[" + parameters + "]")}");
            });
        }

        private async Task<Boolean> IsValidJSONBotConfiguration(string json, string context, Object? contextData)
        {
            var jsonNode = JSON.Parse(json);
            var jsonNodeBotConfiguration = jsonNode[BOT_CONFIGURATION];
            if (jsonNodeBotConfiguration?.IsObject == true)
            {
                if (BotConfiguration.FromJsonNode(jsonNodeBotConfiguration, out var botConfiguration))
                {
                    // Check if the bot specified is correct
                    if ( (botConfiguration.Bot is not null) && (_currentContact is not null) && (_currentContact.Peer is not null))
                    {
                        if ((!String.IsNullOrEmpty(botConfiguration.Bot.Id))
                            && (!botConfiguration.Bot.Id.Equals(_currentContact.Peer.Id, StringComparison.InvariantCultureIgnoreCase)) )
                            return false;

                        if ((!String.IsNullOrEmpty(botConfiguration.Bot.Jid))
                            && (!botConfiguration.Bot.Jid.Equals(_currentContact.Peer.Jid, StringComparison.InvariantCultureIgnoreCase)))
                            return false;

                        if ((!String.IsNullOrEmpty(botConfiguration.Bot.Login))
                            && (!botConfiguration.Bot.Login.Equals(_currentContact.LoginEmail, StringComparison.InvariantCultureIgnoreCase)))
                            return false;
                    }

                    // Store the new configuration
                    _jsonNodeBotConfiguration = jsonNodeBotConfiguration;

                    // Need to avoid to loose info about optional values
                    if (botConfiguration.Administrators is null)
                        botConfiguration.Administrators = _botConfiguration.Administrators;

                    botConfiguration.GuestsAccepted = _botConfiguration.GuestsAccepted;
                    botConfiguration.UserInvitationAutoAccept = _botConfiguration.UserInvitationAutoAccept;
                    botConfiguration.BubbleInvitationAutoAccept = _botConfiguration.BubbleInvitationAutoAccept;

                    // Store new config
                    _botConfiguration = botConfiguration;

                    var botConfigurationUpdate = new BotConfigurationUpdate(jsonNodeBotConfiguration, context, contextData);
                    await BotConfigurationUpdatedAsync(botConfigurationUpdate);

                    var trigger = Trigger.NextStep;
                    if (_machine.CanFire(trigger))
                        FireTrigger(trigger);

                    return true;
                }
            }
            return false;
        }

    #region OnEntry methods
        private async Task OnEntryManageInternalMessageReceivedAsync()
        {
            if (_queueInternalMessagesReceived.TryDequeue(out InternalMessage? internalMessage))
            {
                if (internalMessage is not null)
                {
                    try
                    {
                        if ( (internalMessage.Type == BOT_CONFIGURATION) 
                            && internalMessage.Data is String json)
                        {
                            var result = await IsValidJSONBotConfiguration(json, "internalMessage", internalMessage);
                            if (result)
                                return;
                        }

                        await InternalMessageReceivedAsync(internalMessage);
                    }
                    catch (Exception ex)
                    {
                        log.LogWarning("[OnEntryManageInternalMessageReceivedAsync] Exception occured when calling [InternalMessageReceivedAsync] - Exception:[{Exception}]", ex);
                    }
                }
            }
            var trigger = Trigger.NextStep;
            if (_machine.CanFire(trigger))
                FireTrigger(trigger);
        }

        private async Task OnEntryManageInstantMessageReceivedAsync()
        {
            if (_queueInstantMessagesReceived.TryDequeue(out Message? message))
            {
                if (message is not null)
                {
                    try
                    {
                        // Two ways to receive a BotConfiguration:
                        //  - alternative content as JSON
                        //  - file attachment with ".json" extension

                        // Check AlternatContent
                        if(message.AlternativeContent?.Count > 0)
                        {
                            foreach(var alternativeContent in message.AlternativeContent)
                            {
                                if (alternativeContent.Type == HttpRequestDescriptor.MIME_TYPE_JSON)
                                {
                                    var result = await IsValidJSONBotConfiguration(alternativeContent.Content, "message", message);
                                    if (result)
                                        return;
                                }
                            }
                        }

                        // Check File Attachment
                        if (message.FileAttachment is not null)
                        {
                            // Need to get file descriptor
                            var fileDescriptor = await DownloadFileDescriptorAsync(message.FileAttachment.Id);

                            if (fileDescriptor?.FileName.EndsWith(".json") == true)
                            {
                                // Download the file as String / JSON file
                                var json = await DownloadJsonFileAsync(fileDescriptor);
                                if (!String.IsNullOrEmpty(json))
                                {
                                    var result = await IsValidJSONBotConfiguration(json, "message", message);
                                    if (result)
                                        return;
                                }
                            }
                        }

                        await InstantMessageReceivedAsync(message);
                    }
                    catch (Exception ex)
                    {
                        log.LogWarning("[OnEntryManageInstantMessageReceivedAsync] Exception occured when calling [InstantMessageReceivedAsync] - Exception:[{Exception}]", ex);
                    }
                }
            }
            var trigger = Trigger.NextStep;
            if (_machine.CanFire(trigger))
                FireTrigger(trigger);
        }

        private async Task OnEntryManageApplicationMessageReceivedAsync()
        {
            if (_queueApplicationMessagesReceived.TryDequeue(out ApplicationMessage? applicationMessage))
            {
                if (applicationMessage is not null)
                {
                    try
                    {
                        if(applicationMessage.XmlElements?.Count > 0)
                        {
                            var xmlElement = applicationMessage.XmlElements[0];
                            if (xmlElement.Name == BOT_CONFIGURATION)
                            {
                                var json = xmlElement.InnerText;
                                var result = await IsValidJSONBotConfiguration(json, "applicationMessage", applicationMessage);
                                if (result)
                                    return;
                            }
                        }

                        await ApplicationMessageReceivedAsync(applicationMessage);
                    }
                    catch (Exception ex)
                    {
                        log.LogWarning("[OnEntryManageApplicationMessageReceivedAsync] Exception occured when calling [ApplicationMessageReceivedAsync] - Exception:[{Exception}]", ex);
                    }
                }
            }
            var trigger = Trigger.NextStep;
            if (_machine.CanFire(trigger))
                FireTrigger(trigger);
        }

        private async Task OnEntryManageAckMessageReceivedAsync()
        {
            if (_queueAckMessagesReceived.TryDequeue(out AckMessage? ackMessage))
            {
                if (ackMessage is not null)
                {
                    try
                    {
                        if (ackMessage.Action == BOT_CONFIGURATION && ackMessage.MimeType == HttpRequestDescriptor.MIME_TYPE_JSON)
                        {
                            var json = ackMessage.Content;
                            var result = await IsValidJSONBotConfiguration(json, "ackMessage", ackMessage);
                            if (result)
                                return;
                        }

                        await AckMessageReceivedAsync(ackMessage);
                    }
                    catch (Exception ex)
                    {
                        log.LogWarning("[OnEntryManageAckMessageReceivedAsync] Exception occured when calling [AckMessageReceivedAsync] - Exception:[{Exception}]", ex);
                    }
                }
            }
            var trigger = Trigger.NextStep;
            if (_machine.CanFire(trigger))
                FireTrigger(trigger);
        }

        private async Task OnEntryManageBubbleInvitationReceivedAsync()
        {
            if (_queueBubbleInvitationsReceived.TryDequeue(out BubbleInvitation? bubbleInvitation))
            {
                if (bubbleInvitation is not null)
                {
                    try
                    {
                        await BubbleInvitationReceivedAsync(bubbleInvitation);
                    }
                    catch (Exception ex)
                    {
                        log.LogWarning("[OnEntryManageBubbleInvitationReceivedAsync] Exception occured when calling [BubbleInvitationReceivedAsync] - Exception:[{Exception}]", ex);
                    }
                }
            }
            var trigger = Trigger.NextStep;
            if (_machine.CanFire(trigger))
                FireTrigger(trigger);
        }

        private async Task OnEntryManageUserInvitationReceivedAsync()
        {
            if (_queueUserInvitationsReceived.TryDequeue(out Invitation? invitation))
            {
                if (invitation is not null)
                {
                    try
                    {
                        await UserInvitationReceivedAsync(invitation);
                    }
                    catch (Exception ex)
                    {
                        log.LogWarning("[OnEntryManageUserInvitationReceivedAsync] Exception occured when calling [UserInvitationReceivedAsync] - Exception:[{Exception}]", ex);
                    }
                }
            }
            var trigger = Trigger.NextStep;
            if (_machine.CanFire(trigger))
                FireTrigger(trigger);
        }

        private async Task OnEntryCheckDataAvailabilityAsync()
        {
            Trigger? trigger = null;

            // BubbleInvi
            if (!_queueBubbleInvitationsReceived.IsEmpty)
                trigger = (Trigger.BubbleInvitation);

            else if(!_queueUserInvitationsReceived.IsEmpty)
                trigger = (Trigger.UserInvitation);

            else if (!_queueAckMessagesReceived.IsEmpty)
                trigger = (Trigger.AckMessage);

            else if (!_queueApplicationMessagesReceived.IsEmpty)
                trigger = (Trigger.ApplicationMessage);

            else if (!_queueInternalMessagesReceived.IsEmpty)
                trigger = (Trigger.InternalMessage);

            else if (!_queueInstantMessagesReceived.IsEmpty)
                trigger = (Trigger.InstantMessage);

            await Task.CompletedTask;

            if ( (trigger != null) && _machine.CanFire(trigger.Value))
                FireTrigger(trigger.Value);
        }

        private async Task OnEntryConnectedAsync()
        {
            // We don't need to get penging Bubble invitations - done by the SDK

            // We need to get pending user invitations - to auto-accept them or store them
            var sdkResult = await _rbInvitations.GetReceivedInvitationsAsync(InvitationStatus.Pending);
            if(sdkResult.Success)
            {
                var list = sdkResult.Data;
                while(list?.Count > 0)
                {
                    // Get first inviation and remove it form the list
                    var invitation = list[0];
                    list.RemoveAt(0);

                    if (_botConfiguration.UserInvitationAutoAccept == true)
                    {
                        await _rbInvitations.AcceptReceivedPendingInvitationAsync(invitation);
                    }
                    else
                    {
                        _queueUserInvitationsReceived.Enqueue(invitation);
                        var trigger = Trigger.NextStep;
                        if (_machine.CanFire(trigger))
                            FireTrigger(trigger);
                    }
                }
            }

            try
            {
                await ConnectedAsync();
            }
            catch (Exception ex)
            {
                log.LogWarning("[OnEntryConnectedAsync] Exception occured when calling [ConnectedAsync] - Exception:[{Exception}]", ex);
            }
            FireTrigger(Trigger.NextStep);
        }

        private async Task OnEntryStoppedAsync()
        {
            try
            {
                await StoppedAsync(_botCancelledSdkError);
            }
            catch (Exception ex)
            {
                log.LogWarning("[OnEntryStoppedAsync] Exception occured when calling [StoppedAsync] - Exception:[{Exception}]", ex);
            }
            // Last State - Nothing to trigger
        }

    #endregion OnEntry methods

        private void ResetValues()
        {
            _queueBubbleInvitationsReceived.Clear();
            _queueUserInvitationsReceived.Clear();

            _queueAckMessagesReceived.Clear();
            _queueApplicationMessagesReceived.Clear();
            _queueInternalMessagesReceived.Clear();
            _queueInstantMessagesReceived.Clear();            
        }

        /// <summary>
        /// To specify restrictions to use in the Rainbow SDK - for example we want to use the AutoReconnection service and don' want to store message
        /// </summary>
        private void SetRainbowRestrictions()
        {
            //TODO - need to be defined by a configuration file

            // We want to let MCQ message visible to the end user
            _rbApplication.Restrictions.MessageStorageMode = SdkMessageStorageMode.Store;

            // Since we are using a Bot we want to send automatically a Read Receipt when a message has been received
            _rbApplication.Restrictions.SendReadReceipt = true;

            // Since we are using a bot, we don't want to allow messages to be sent to oneself.
            _rbApplication.Restrictions.SendMessageToConnectedUser = false;

            // We want to use always the same resource id when we connect to the event server
            _rbApplication.Restrictions.UseSameResourceId = true;

            // We use XMPP for event mode
            _rbApplication.Restrictions.EventMode = SdkEventMode.XMPP;

            // We want to use conference features
            _rbApplication.Restrictions.UseConferences = true;

            // We want to use WebRTC
            _rbApplication.Restrictions.UseWebRTC = true;


            _rbApplication.Restrictions.LogRestRequest = true;
            
            _rbApplication.Restrictions.UseBubbles = true;

            Rainbow.Util.SetLogAnonymously(false);
        }

        /// <summary>
        /// To create all necessary objects from the Rainbow SDK C#
        /// </summary>
        private Boolean CreateRainbowObjects()
        {
            _rbAutoReconnection = _rbApplication.GetAutoReconnection();
            _rbAutoReconnection.MaxNbAttempts = 5; // For tests purpose we use here a low value

            _rbBubbles = _rbApplication.GetBubbles();
            _rbContacts = _rbApplication.GetContacts();
            _rbInstantMessaging = _rbApplication.GetInstantMessaging();
            _rbInvitations = _rbApplication.GetInvitations();
            _rbFileStorage = _rbApplication.GetFileStorage();

            return true;
        }

        /// <summary>
        /// To subscribe to all necessary events from the Rainbow SDK C#
        /// </summary>
        private void UnsubscribeToRainbowEvents()
        {
            if (_rbApplication == null)
                return;

            _rbApplication.AuthenticationSucceeded -= RbApplication_AuthenticationSucceeded;
            _rbApplication.ConnectionStateChanged -= RbApplication_ConnectionStateChanged;

            _rbAutoReconnection.MaxNbAttemptsReached -= RbAutoReconnection_MaxNbAttemptsReached;
            _rbAutoReconnection.Cancelled -= RbAutoReconnection_Cancelled;

            _rbInstantMessaging.AckMessageReceived -= RbInstantMessaging_AckMessageReceived;
            _rbInstantMessaging.ApplicationMessageReceived -= RbInstantMessaging_ApplicationMessageReceived;
            _rbInstantMessaging.MessageReceived -= RbInstantMessaging_MessageReceived;
            

            _rbBubbles.BubbleInvitationReceived -= RbBubbles_BubbleInvitationReceived;
            _rbInvitations.InvitationReceived -= RbInvitations_InvitationReceived;
        }

        /// <summary>
        /// To unsubscribe to all necessary events previously subscribe
        /// </summary>
        private void SubscribeToRainbowEvents()
        {
            _rbApplication.AuthenticationSucceeded += RbApplication_AuthenticationSucceeded;
            _rbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;

            _rbAutoReconnection.MaxNbAttemptsReached += RbAutoReconnection_MaxNbAttemptsReached;
            _rbAutoReconnection.Cancelled += RbAutoReconnection_Cancelled;

            _rbInstantMessaging.AckMessageReceived += RbInstantMessaging_AckMessageReceived;
            _rbInstantMessaging.ApplicationMessageReceived += RbInstantMessaging_ApplicationMessageReceived;
            _rbInstantMessaging.MessageReceived += RbInstantMessaging_MessageReceived;

            _rbBubbles.BubbleInvitationReceived += RbBubbles_BubbleInvitationReceived;
            _rbInvitations.InvitationReceived += RbInvitations_InvitationReceived;
        }

        /// <summary>
        /// Facilitator to fire/raise a trigger on the state machine
        /// </summary>
        private Boolean FireTrigger(Trigger trigger)
        {
            try
            {
                _machine.FireAsync(trigger);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private async Task<FileDescriptor ?> DownloadFileDescriptorAsync(String fileDescriptorId)
        {
            int count = 0;
            var canContinue = true;
            while (canContinue)
            {
                var sdkResult = await _rbFileStorage.GetFileDescriptorAsync(fileDescriptorId);
                //Do we have a correct answer
                if (sdkResult.Success)
                    return sdkResult.Data;

                if (sdkResult.Result.IncorrectUseError != null)
                    return null; // The server informs us that the download cannot be performed.

                await Task.Delay(200);
                count++;
            }
            return null;
        }

        private async Task<String?> DownloadJsonFileAsync(FileDescriptor fileDescriptor)
        {
            // TODO - check if we need to wait the end of the upload

            //int count = 0;
            //var canContinue = true;
            //while (canContinue)
            //{
                MemoryStream memoryStream = new ();
                var sdkResult = await _rbFileStorage.DownloadFileAsync(fileDescriptor, memoryStream);

                //Do we have a correct answer
                if (sdkResult.Success)
                    return Encoding.UTF8.GetString(memoryStream.ToArray());

                return null; // The server informs us that the download cannot be performed.

            //    await Task.Delay(200);
            //    count++;
            //}
            //return null;
        }

    #region EVENTS RAISED FROM RAINBOW OBJECTS

        /// <summary>
        /// Raised when Authentication has succeeded on Rainbow server
        /// </summary>
        private void RbApplication_AuthenticationSucceeded()
        {
            FireTrigger(Trigger.AuthenticationSucceeded);
        }

        /// <summary>
        /// RAised when connection status with Rainbow server has chnaged
        /// </summary>
        private void RbApplication_ConnectionStateChanged(ConnectionState connectionState)
        {
            switch (connectionState.Status)
            {
                case ConnectionStatus.Connected:
                    _currentContact = _rbContacts.GetCurrentContact();
                    FireTrigger(Trigger.Connect);
                    break;

                case ConnectionStatus.Disconnected:
                    if(_machine.CanFire(Trigger.Disconnect))
                        FireTrigger(Trigger.Disconnect);
                    break;

                case ConnectionStatus.Connecting:
                    if (_machine.CanFire(Trigger.StartLogin))
                        FireTrigger(Trigger.StartLogin);
                    break;
            }
        }

        /// <summary>
        /// Raised when the auto reconnection service reach the max number of attempts to successfully connect to the Rainbow server
        /// </summary>
        private void RbAutoReconnection_MaxNbAttemptsReached()
        {
            //FireTrigger(Trigger.TooManyAttempts);
        }

        /// <summary>
        /// Raised when the auto reconnection service has beend cancelled
        /// </summary>
        private void RbAutoReconnection_Cancelled(SdkError sdkError)
        {
            _botCancelledSdkError = sdkError;

            Trigger trigger = Trigger.Disconnect;
            while (_machine.CanFire(trigger))
                FireTrigger(trigger);

            FireTrigger(Trigger.Stop);
        }

        /// <summary>
        /// Raised when a invitation to be a member of a bubble has been received
        /// </summary>
        private void RbBubbles_BubbleInvitationReceived(BubbleInvitation bubbleInvitation)
        {
            // Doc we accept all Bubble invitations
            if (_botConfiguration.BubbleInvitationAutoAccept == true)
            {
                _rbBubbles?.AcceptInvitationAsync(bubbleInvitation.Bubble);
                return;
            }

            // Queue bubble invitation
            _queueBubbleInvitationsReceived.Enqueue(bubbleInvitation);
            var trigger = Trigger.NextStep;
            if (_machine.CanFire(trigger))
                FireTrigger(trigger);
        }

        /// <summary>
        /// Raised when an user invitation has been received
        /// </summary>
        private void RbInvitations_InvitationReceived(Invitation invitation)
        {
            // Doc we accept all User invitations ?
            if (_botConfiguration.UserInvitationAutoAccept == true)
            {
                _rbInvitations?.AcceptReceivedPendingInvitationAsync(invitation);
                return;
            }

            // Queue user invitation
            _queueUserInvitationsReceived.Enqueue(invitation);
            var trigger = Trigger.NextStep;
            if (_machine.CanFire(trigger))
                FireTrigger(trigger);
        }

        /// <summary>
        /// Event raised when the current user received an AckMessage
        /// </summary>
        private async void RbInstantMessaging_AckMessageReceived(AckMessage ackMessage)
        {
            // We don't want to manage AckMessage coming from ourself
            if (ackMessage.FromJid.Equals(_currentContact?.Peer?.Jid, StringComparison.InvariantCultureIgnoreCase) == true)
                return;

            var isAdmin = await IsAdministrator(ackMessage.FromJid);
            if(isAdmin)
            {
                // Queue Ack Message
                _queueAckMessagesReceived.Enqueue(ackMessage);
                var trigger = Trigger.NextStep;
                if (_machine.CanFire(trigger))
                    FireTrigger(trigger);
            }
        }

        /// <summary>
        /// Event raised when the current user received an ApplicationMessage
        /// </summary>
        private async void RbInstantMessaging_ApplicationMessageReceived(ApplicationMessage applicationMessage)
        {
            // We don't want to manage AckMessage coming from ourself
            if (applicationMessage.FromJid.Equals(_currentContact?.Peer?.Jid, StringComparison.InvariantCultureIgnoreCase) == true)
                return;

            var isAdmin = await IsAdministrator(applicationMessage.FromJid);
            if (isAdmin)
            {
                // Queue ApplicationMessage
                _queueApplicationMessagesReceived.Enqueue(applicationMessage);
                var trigger = Trigger.NextStep;
                if (_machine.CanFire(trigger))
                    FireTrigger(trigger);
            }
        }

        /// <summary>
        /// Event raised when the current user received a Instant Message
        /// </summary>
        private async void RbInstantMessaging_MessageReceived(Message message, Boolean _carbonCopy)
        {
            // We don't want to manage message coming from ourself
            if (message.FromContact?.Peer?.Jid?.Equals(_currentContact?.Peer.Jid, StringComparison.InvariantCultureIgnoreCase) == true)
                return;

            var isAdmin = await IsAdministrator(message.FromContact?.Peer?.Jid);
            if (isAdmin)
            {
                // Queue Instant Message
                _queueInstantMessagesReceived.Enqueue(message);
                var trigger = Trigger.NextStep;
                if (_machine.CanFire(trigger))
                    FireTrigger(trigger);
            }
        }

    #endregion EVENTS RAISED FROM RAINBOW OBJECTS

#endregion PRIVATE API

#region PUBLIC API

        /// <summary>
        /// Constructor for the RainbowBotBase class
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public BotBase()
        {
            // Instantiate a new state machine in the Created state
            _machine = new StateMachine<State, Trigger>(State.Created);

            ConfigureStateMachine();

            _queueBubbleInvitationsReceived = new ();
            _queueUserInvitationsReceived = new ();

            _queueInstantMessagesReceived = new();
            _queueApplicationMessagesReceived = new();
            _queueAckMessagesReceived = new();
            _queueInternalMessagesReceived = new();
        }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        ~BotBase()
        {
            UnsubscribeToRainbowEvents();
        }

        /// <summary>
        /// To provide access to the Rainbow.Application object used by the bot. Permit to use all SDK C# features from it
        /// </summary>
        public Rainbow.Application Application
        {
            get => _rbApplication;
        }

        /// <summary>
        /// To get the name of the Bot (it's the prefix defined in file credentials.json file)
        /// </summary>
        public String BotName
        {
            get { return _credentials.UserConfig.Prefix; }
        }

        /// <summary>
        /// To configure the bot - must be called before to use <see cref="Login"/>
        /// </summary>
        /// <param name="jsonNodeCredentials"><see cref="Credentials"/></param>
        /// <param name="jsonNodeBotConfiguration"><see cref="BotConfiguration"/></param>
        /// <returns></returns>
        public async Task<Boolean> Configure(JSONNode jsonNodeCredentials, JSONNode jsonNodeBotConfiguration)
        {
            // Check that the trigger can be used
            Trigger triggerToUse = Trigger.Configure;
            if (!_machine.CanFire(triggerToUse))
                return false;

            _jsonNodeBotConfiguration = jsonNodeBotConfiguration;

            if (!Credentials.FromJsonNode(jsonNodeCredentials, out _credentials))
            {
                Util.WriteErrorToConsole($"Cannot read 'credentials' object OR invalid/missing data.");
                return false;
            }

            if (!BotConfiguration.FromJsonNode(_jsonNodeBotConfiguration, out _botConfiguration))
            {
                Util.WriteErrorToConsole($"Cannot read 'botConfiguration' object OR invalid/missing data.");
                return false;
            }

            // At least one administrator must be set or guests accepted
            if (!((_botConfiguration.GuestsAccepted == true) || (_botConfiguration.Administrators?.Count > 0)))
            {
                Util.WriteErrorToConsole($"At least one administrator must be set or guests accepted");
                return false;
            }

            var prefix = _credentials.UserConfig.Prefix;
            var loggerPrefix = prefix + "_";

            // We want to log specifically using a prefix for this bot
            if (!NLogConfigurator.AddLogger(loggerPrefix))
                return false;

            log = Rainbow.LogFactory.CreateLogger<Application>(loggerPrefix);

            // Create Rainbow Application (root object of the SDK)
            _rbApplication = new Rainbow.Application(_credentials.UserConfig.IniFolderPath, prefix + ".ini", loggerPrefix);

            // Set APP_ID, APP_SECRET_KET and HOSTNAME
            _rbApplication.SetApplicationInfo(_credentials.ServerConfig.AppId, _credentials.ServerConfig.AppSecret);
            _rbApplication.SetHostInfo(_credentials.ServerConfig.HostName);

            _rbApplication.SetTimeout(10000);

            // Set restrictions
            SetRainbowRestrictions();

            // Create others Rainbow SDK Objects
            if (!CreateRainbowObjects())
                return false;

            await BotConfigurationUpdatedAsync(new BotConfigurationUpdate(_jsonNodeBotConfiguration, "configFile", null));

            // Fire the trigger
            return FireTrigger(triggerToUse);
        }

        /// <summary>
        /// Once configured, to start login process
        /// </summary>
        /// <returns>True if the login process has started</returns>
        public Boolean Login()
        {
            // Check that the trigger can be used
            Trigger triggerToUse = Trigger.StartLogin;

            if (!_machine.CanFire(triggerToUse))
                return false;

            ResetValues();

            if (FireTrigger(triggerToUse))
            {
                UnsubscribeToRainbowEvents();
                SubscribeToRainbowEvents();

                var _ = _rbApplication.LoginAsync(_credentials.UserConfig.Login, _credentials.UserConfig.Password);
                return true;
            }
            return false;
        }

        /// <summary>
        /// To start logout process - <see cref="StoppedAsync"/> will be called once done
        /// </summary>
        public void Logout()
        {
            var _ = _rbApplication.LogoutAsync();
        }

        /// <summary>
        /// To add an <see cref="InternalMessage"/> to the queue.
        /// It will be dequeue using <see cref="InternalMessageReceivedAsync(InternalMessage)"/>
        /// It permits by code to send a BotConfiguration Update or any other data
        /// </summary>
        /// <param name="internalMessage"><see cref="InternalMessage"/> An InternalMessage object</param>
        public void AddInternalMessage(InternalMessage internalMessage)
        {
            if (internalMessage != null)
            {
                _queueInternalMessagesReceived.Enqueue(internalMessage);
                var trigger = Trigger.NextStep;
                if (_machine.CanFire(trigger))
                    FireTrigger(trigger);
            }
        }

        /// <summary>
        /// To know if Contact specified is an administrator of this bot
        /// </summary>
        /// <param name="contact"><see cref="Contact"/>Contact object</param>
        /// <returns><see cref="Boolean"/> - True if contact specified is an administrator of this bot</returns>
        public Boolean IsAdministrator(Contact? contact)
        {
            if ( (contact is null) || (contact.Peer is null))
                return false;

            // Check for Guest 
            if(contact.GuestMode && (_botConfiguration.GuestsAccepted == true))
                return true;

            // Check in Administrators list
            var account = _botConfiguration.Administrators?.FirstOrDefault(account => 
                (account.Jid?.Equals(contact.Peer.Jid, StringComparison.InvariantCultureIgnoreCase) == true)
                || (account.Id?.Equals(contact.Peer.Id, StringComparison.InvariantCultureIgnoreCase) == true)
                || (account.Login?.Equals(contact.LoginEmail, StringComparison.InvariantCultureIgnoreCase) == true)
                );

            return (account is not null);
        }

        /// <summary>
        /// To know if Jid specified is administrator of this bot. if this jid is not in the cache, ask the server more info
        /// </summary>
        /// <param name="jid"><see cref="String"/>Jid</param>
        /// <returns><see cref="Boolean"/> - True if Jid specified is an administrator of this bot</returns>
        public async Task<Boolean> IsAdministrator(String? jid)
        {
            if (String.IsNullOrEmpty(jid))
                return false;

            if(_botConfiguration.Administrators?.Count > 0)
            {
                var account = _botConfiguration.Administrators.FirstOrDefault(account => account.Jid?.Equals(jid, StringComparison.InvariantCultureIgnoreCase) == true);
                if (account is not null)
                    return true;

                var contact = await _rbContacts.GetContactByJidInCacheFirstAsync(jid);
                return IsAdministrator(contact);
            }
            return false;
        }

        /// <summary>
        /// To know if the bot is stopped and why
        /// </summary>
        public (Boolean isStopped, SdkError? sdkError) IsStopped()
        {
            if (_machine.State == State.Stopped)
                return (true, _botCancelledSdkError);
            return (false, null);
        }

        /// <summary>
        /// Get Dot Graph of this Bot
        /// </summary>
        /// <returns>The dot graph as string</returns>
        public String ToDotGraph()
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
        /// Get the current state and the trigger used to reach it
        /// </summary>
        /// <returns>(<see cref="State"/>, <see cref="Trigger"/>)</returns>
        public (State, Trigger) GetStateAndTrigger()
        {
            return (_machine.State, _lastTrigger);
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
        /// To check the bot status
        /// </summary>
        /// <returns></returns>
        public (BotLibrary.State state, Boolean canContinue, String message) CheckBotStatus()
        {
            Boolean canContinue = true;
            String message = "";

            // Get the curent state of the bot and the trigger used to reach it
            (BotLibrary.State state, var _) = GetStateAndTrigger();

            // Two states are important here:
            //  - NotConnected: the bot is not connected (or no more connected), we need to add logic according uour needs
            //  - Created: the bot is not connected because credentials are not correct
            // All other states are managed in the bot logic itself

            // Check if bot is in NotConnected state
            if ( (state == BotLibrary.State.Stopped) && (!_rbAutoReconnection.IsStarted) )
            {
                canContinue = false;

                if(_botCancelledSdkError?.Type == SdkErrorType.NoError)
                    message = $"[{_credentials.UserConfig.Prefix}] Bot has been stopped ...";

                else if (_botCancelledSdkError?.IncorrectUseError?.ErrorDetailsCode == (int)SdkInternalErrorEnum.LOGIN_PROCESS_INVALID_CREDENTIALS)
                    message = $"[{_credentials.UserConfig.Prefix}] Bot is not connected because the credentials are not correct ...";

                else if (_botCancelledSdkError?.IncorrectUseError?.ErrorDetailsCode == (int)SdkInternalErrorEnum.LOGIN_PROCESS_MAX_ATTEMPTS_REACHED)
                    message = $"[{_credentials.UserConfig.Prefix}] Bot was connected but after several attempts it can't reach the server anymore...";

                else 
                    message = $"[{_credentials.UserConfig.Prefix}] Bot has stopped ... SdkError:[{_botCancelledSdkError}]";
            }

            return (state, canContinue, message);
        }

    #region virtual methods

        // Called when the Bot is connected to Rainbow server (called also after reconnection)
        public virtual async Task ConnectedAsync()
        {
            await Task.CompletedTask;
            return;
        }
        
        // Called when the Bot has been stopped (after too many auto-reconnection attempts or after a logout)
        public virtual async Task StoppedAsync(SdkError sdkerror)
        {
            await Task.CompletedTask;
        }

        // Called when a Bubble Invitation is received if bubbleInvitationAutoAccept setting is not set to true
        public virtual async Task BubbleInvitationReceivedAsync(BubbleInvitation bubbleInvitation)
        {
            await Task.CompletedTask;
        }

        // Called when a Bubble Invitation is received if userInvitationAutoAccept setting is not set to true
        public virtual async Task UserInvitationReceivedAsync(Invitation invitation)
        {
            await Task.CompletedTask;
        }

        // Called when a BotConfiguration update has been received
        public virtual async Task BotConfigurationUpdatedAsync(BotConfigurationUpdate botConfigurationUpdate)
        {
            await Task.CompletedTask;
        }

        // Called when an AckMessage is received. If it contains a BotConfiguration update, BotConfigurationUpdatedAsync method is called instead
        public virtual async Task AckMessageReceivedAsync(AckMessage ackMessage)
        {
            await _rbInstantMessaging.AnswerToAckMessageAsync(ackMessage, MessageType.Result);
        }

        // Called when an ApplicationMessage is received. If it contains a BotConfiguration update, BotConfigurationUpdatedAsync method is called instead
        public virtual async Task ApplicationMessageReceivedAsync(ApplicationMessage applicationMessage)
        {
            await Task.CompletedTask;
        }

        // Called when an InstantMessage is received. If it contains a BotConfiguration update, BotConfigurationUpdatedAsync method is called instead
        public virtual async Task InstantMessageReceivedAsync(Message message)
        {
            await Task.CompletedTask;
        }

        // Called when an InternalMessage is received. If it contains a BotConfiguration update, BotConfigurationUpdatedAsync method is called instead
        public virtual async Task InternalMessageReceivedAsync(InternalMessage internalMessage)
        {
            await Task.CompletedTask;
        }
    #endregion virtual methods

#endregion PUBLIC API

    }
}
