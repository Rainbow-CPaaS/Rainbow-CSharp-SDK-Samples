using AdaptiveCards.Templating;
using Rainbow;
using Rainbow.SimpleJSON;
using Rainbow.Model;
using Rainbow.Events;
using Stateless;
using Stateless.Graph;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace BotAdaptiveCards
{
    public class RainbowBotConnect
    {
        private readonly int MAX_MCQ_QUESTION = 5;

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

            InitializeUserMCQ,

            AutoReconnection,

            StopMessageReceived,

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

            MCQToInitialize,
            MCQInitialized,

            AllMCQTestPassed,

            DequeueMessages,

            MessageReceivedFromPeer,
            StopMessage,

            MessageManaged,
        }

        private readonly StateMachine<State, Trigger> _machine;

        // The TriggerWithParameters object is used when a trigger requires a payload.
        private StateMachine<State, Trigger>.TriggerWithParameters<MessageEventArgs>? _messageReceivedFromPeerTrigger;
        private StateMachine<State, Trigger>.TriggerWithParameters<UserMCQStatus>? _MCQToInitializeTrigger;

        // Concurrent queue of messages received
        private readonly ConcurrentQueue<MessageEventArgs> _messageQueue;

        private readonly List<UserMCQStatus> _userMCQStatusList;
        private readonly MCQInfo _mcQInfo;

        // Several internal variables
        private Trigger _lastTrigger;
        private String? _botLogin;
        private String? _botPwd;
        private String? _masterBotEmail;
        private Contact _currentContact;

        // Define all Rainbow objects we use
        private Rainbow.Application RbApplication;
        private Rainbow.AutoReconnection RbAutoReconnection;
        private Rainbow.Contacts RbContacts;
        private Rainbow.Conversations RbConversations;
        private Rainbow.InstantMessaging RbInstantMessaging;

#region PRIVATE API

        /// <summary>
        /// To create / configure all the state machine
        /// </summary>
        private void ConfigureStateMachine()
        {
            // Create Trigger(s) using parameters
            _messageReceivedFromPeerTrigger = _machine.SetTriggerParameters<MessageEventArgs>(Trigger.MessageReceivedFromPeer);
            _MCQToInitializeTrigger = _machine.SetTriggerParameters<UserMCQStatus>(Trigger.MCQToInitialize);

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

            // Configure the Initialized state
            _machine.Configure(State.Connected)
                .Permit(Trigger.InitializationPerformed, State.Initialized)
                .Permit(Trigger.Disconnect, State.AutoReconnection);

            // Configure the Connected state
            _machine.Configure(State.Initialized)
                .OnEntry(CheckConnectedState)

                .PermitReentry(Trigger.DequeueMessages)

                .Permit(Trigger.Disconnect, State.AutoReconnection)

                .Permit(Trigger.StopMessage, State.StopMessageReceived)
                .PermitIf(_messageReceivedFromPeerTrigger, State.MessageFromPeer)

                .PermitIf(_MCQToInitializeTrigger, State.InitializeUserMCQ)
                
                .Permit(Trigger.AllMCQTestPassed, State.Created);

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

            // Configure the MessageFromPeer state
            _machine.Configure(State.MessageFromPeer)
                .OnEntryFrom(_messageReceivedFromPeerTrigger, AnswerToPeerMessage)
                .Permit(Trigger.MessageManaged, State.Initialized);

            // Configure the InitializeUserMCQ state
            _machine.Configure(State.InitializeUserMCQ)
                .OnEntryFrom(_MCQToInitializeTrigger, InitializeUserMCQ)
                .Permit(Trigger.MCQInitialized, State.Initialized);

            _machine.OnUnhandledTrigger( (state, trigger) => Console.WriteLine($"OnUnhandledTrigger - State: {state} with Trigger: {trigger}"));
            
            _machine.OnTransitionCompleted(transition => {
                // Store the trigger used
                _lastTrigger = transition.Trigger;

                // Log info about transition
                Console.WriteLine($"OnTransitionCompleted- State: {transition.Source} -> {transition.Destination} with Trigger: {transition.Trigger}({string.Join(", ", transition.Parameters)})");
            });
        }

        /// <summary>
        /// To check if specified message is the "stop message" sent by master bot"
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private Boolean IsStopMessageFromMasterBot(Message message)
        {
            // Get login email of the user who has sent this message
            Contact contact = RbContacts.GetContactFromContactJid(message.FromJid);
            if (contact != null)
            {
                String contactLoginEmail = contact.LoginEmail;

                // Check if it's the master bot who sent this message
                if ((contactLoginEmail != null) && contactLoginEmail.Equals(_masterBotEmail, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Check if it's the "stop message"
                    return (message.Content.Equals(RainbowApplicationInfo.STOP_MESSAGE, StringComparison.InvariantCultureIgnoreCase));
                }
            }
            else
                RbContacts.GetContactFromContactJidFromServer(message.FromJid); // The goal here is to know finally who is this Jid ?

            return false;
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
        private void CheckMCQforUsersAndDequeuedMessages()
        {
            // Check if at least one user has beend initialzed to start MCQ
            UserMCQStatus? userMCQ = _userMCQStatusList.Find(userMCQ => (userMCQ.Contact == null) && (!userMCQ.TestFinished) );
            if (userMCQ != null)
            {
                _machine.Fire(_MCQToInitializeTrigger, userMCQ);
                return;
            }

            // Check if all users have finisehd the test
            userMCQ = _userMCQStatusList.Find(userMCQ => (!userMCQ.TestFinished));
            if(userMCQ == null)
            {
                _machine.Fire(Trigger.AllMCQTestPassed);
                return;
            }

            // Dequeue messages
            if (_messageQueue.Count > 0)
            {
                if (_messageQueue.TryDequeue(out MessageEventArgs? messageEvent))
                {
                    if (messageEvent?.Message != null)
                    {
                        // Is-it the stop message from master bot ?
                        if (IsStopMessageFromMasterBot(messageEvent.Message))
                        {
                            _machine.Fire(Trigger.StopMessage);
                            return;
                        }

                        // Is it a message from a Peer  and not from bubble  ?
                        if (IsMessageNotFromBubble(messageEvent.Message))
                        {
                            _machine.Fire(_messageReceivedFromPeerTrigger, messageEvent);
                            return;
                        }
                    }
                }
            }

            CancelableDelay.StartAfter(10, CheckMCQforUsersAndDequeuedMessages);
        }

        /// <summary>
        /// To logout the bot form Rainbow server
        /// </summary>
        private void LogoutBot()
        {
            RbApplication.Logout();
        }

        /// <summary>
        /// To retrieve user answer from the specified message
        /// 
        /// </summary>
        /// <param name="content"><see cref="Rainbow.Model.Message"/>Message received</param>
        /// <returns></returns>
        private (Boolean isAdaptiveCardAnswer, String? userAnswer) GetUserAnswerFromMCQQuestionAdaptivCardResponse(Message message)
        {
            Boolean isAdaptiveCardAnswer = false;
            String? answer = null;
            if (message.AlternativeContent != null)
            {
                foreach (MessageAlternativeContent alternativeContent in message.AlternativeContent)
                {
                    if(alternativeContent.Type == "rainbow/json")
                    {
                        isAdaptiveCardAnswer = true;
                        var content = alternativeContent.Content;

                        var jsonNode = JSON.Parse(content);
                        var questionId = UtilJson.AsString(jsonNode, "questionId");

                        // For the first question, we set a default answer
                        if (questionId == "00")
                            answer = " ";
                        else
                            answer = UtilJson.AsString(jsonNode, "MCQSelection");
                        break;
                    }

                }
            }
            return (isAdaptiveCardAnswer, answer);
        }

        /// <summary>
        /// To create all elements necessary to send (or edit) the Adaptive Card using the specified question index
        /// </summary>
        /// <param name="questionIndex"></param>
        /// <param name="userAnswer"></param>
        /// <returns></returns>
        private (String? message, List<MessageAlternativeContent>? alternativeContent) CreateMCQQuestionAdaptiveCard(int questionIndex, String? userAnswer = null)
        {
            String? message = null;
            List<MessageAlternativeContent>? alternativeContent = null;

            if ( (questionIndex >= 0) && (questionIndex <= MAX_MCQ_QUESTION))
            {
                String? jsonTemplate = Util.GetContentOfEmbeddedResource("mcqQuestion-template.json", System.Text.Encoding.UTF8);
                String? jsonData = Util.GetContentOfEmbeddedResource($"mcqQuestion-data-{questionIndex.ToString("00")}.json", System.Text.Encoding.UTF8);

                if ((jsonData != null) && (jsonTemplate != null))
                {
                    // Nothing is required for the first adaptive card
                    if (questionIndex == 0)
                        jsonTemplate = jsonTemplate.Replace("\"isRequired\": true", "\"isRequired\": false");

                    // Do we need to change the content since we have the user answer ?
                    if (!String.IsNullOrEmpty(userAnswer))
                    {
                        jsonTemplate = jsonTemplate.Replace("\"isVisible\": false", "\"isVisible\": true"); // To display a Textblock previously not visible
                        jsonTemplate = jsonTemplate.Replace("\"actions\"", "\"noActions\""); // To avoid to display action
                        jsonTemplate = jsonTemplate.Replace("\"MCQSelection\"", $"\"MCQSelection{questionIndex.ToString("00")}\""); // To avoid to have several time the same ID

                        jsonData = jsonData.Replace("\"userAnswer\": \"\"", $"\"userAnswer\": \"{userAnswer}\"");
                    }

                    // Get title of the question
                    var jsonNode = JSON.Parse(jsonData);
                    message = UtilJson.AsString(jsonNode, "title");

                    if (message == null)
                        message = " "; // => Due to a bug in WebClient must not be empty/null

                    // Create a Template instance from the template payload
                    AdaptiveCardTemplate template = new AdaptiveCardTemplate(jsonTemplate);

                    // "Expand" the template - this generates the final Adaptive Card payload
                    string cardJson = template.Expand(jsonData);

                    // Create an Message Alternative Content
                    MessageAlternativeContent messageAlternativeContent = new MessageAlternativeContent();
                    messageAlternativeContent.Type = "form/json";
                    messageAlternativeContent.Content = cardJson;

                    alternativeContent = new List<MessageAlternativeContent> { messageAlternativeContent };
                }
            }

            return (message, alternativeContent);
        }

        /// <summary>
        /// To send to the specified user the result of his MCQ test
        /// </summary>
        /// <param name="userMCQStatus"></param>
        /// <returns></returns>
        private Boolean SendMCQResult(UserMCQStatus userMCQStatus)
        {
            // Create content on the Adaptive Card
            String adaptiveCardResult = "";
            int correctAnswers = 0;

            for(int i = 0; i < MAX_MCQ_QUESTION; i++)
            {
                var question = _mcQInfo.Questions[i].Title;
                
                adaptiveCardResult += MCQ.MCQ_RESULT_QUESTION.Replace("{question}", $"Question {i+1}: {_mcQInfo.Questions[i].Title}");
                adaptiveCardResult += MCQ.MCQ_RESULT_ANSWER.Replace("{answer}", $"Answer expected: {_mcQInfo.Questions[i].CorrectChoice()}");

                var userAnwer = userMCQStatus.Answers[i];
                if(userAnwer != null)
                {
                    if (_mcQInfo.Questions[i].IsCorrectValue(userAnwer))
                    {
                        adaptiveCardResult += MCQ.MCQ_RESULT_CORRECT_ANSWER;
                        correctAnswers++;
                    }
                    else
                    {
                        String answer = MCQ.MCQ_RESULT_ANSWER.Replace("{answer}", $"Your answer: {_mcQInfo.Questions[i].GetChoice(userAnwer)}");
                        answer = answer.Replace("accent", "attention");
                        adaptiveCardResult += answer;
                    }
                }
                else
                    adaptiveCardResult += MCQ.MCQ_RESULT_ANSWER.Replace("{answer}", $"You didn't answer ...");
            }

            adaptiveCardResult += MCQ.MCQ_RESULT_FOOTER;

            adaptiveCardResult = MCQ.MCQ_RESULT_HEADER.Replace("{result}", $"MCQ Test - Result: {correctAnswers} / {MAX_MCQ_QUESTION}") + adaptiveCardResult;

            Console.WriteLine($"[{userMCQStatus.LoginEmail}] has finished his test - Result: {correctAnswers} / {MAX_MCQ_QUESTION}");

            // Get conversation Id
            var conversationId = userMCQStatus.ConversationId;

            var message = "MCQ Test - Result";

            // Create an Message Alternative Content
            MessageAlternativeContent messageAlternativeContent = new MessageAlternativeContent();
            messageAlternativeContent.Type = "form/json";
            messageAlternativeContent.Content = adaptiveCardResult;

            var alternativeContent = new List<MessageAlternativeContent> { messageAlternativeContent };

            ManualResetEvent pause = new ManualResetEvent(false);

            if (RainbowApplicationInfo.USE_ALWAYS_SAME_ADAPTIVE_CARD)
            {
                RbInstantMessaging.EditMessage(conversationId, userMCQStatus.LastAdaptativeCardMessageID, message, alternativeContent, callback =>
                {
                    pause.Set();
                }); ;
            }
            else
            {
                RbInstantMessaging.SendAlternativeContentsToConversationId(conversationId, message, alternativeContent, UrgencyType.Std, null, callback =>
                {
                    pause.Set();
                });
            }
            pause.WaitOne();

            return true;
        }

        /// <summary>
        /// To send to the specified user the next MCQ question
        /// </summary>
        /// <param name="userMCQStatus"></param>
        /// <returns></returns>
        private Boolean SendMCQQuestion(UserMCQStatus userMCQStatus)
        {
            if(!userMCQStatus.TestFinished)
            {
                String? conversationId = userMCQStatus.ConversationId;
                if (conversationId == null)
                {
                    var conversation = RbConversations.GetOrCreateConversationFromUserId(userMCQStatus.Contact?.Id);
                    conversationId = userMCQStatus.ConversationId = conversation?.Id;
                }

                if(conversationId == null)
                {
                    Console.WriteLine($"Cannot get/create conversation for [{userMCQStatus.LoginEmail}]");
                    return false;
                }

                (String? message, List<MessageAlternativeContent>? alternativeContent) = CreateMCQQuestionAdaptiveCard(userMCQStatus.CurrentQuestion);
                if ((message != null) && (alternativeContent != null))
                {
                    Boolean result = false;
                    ManualResetEvent pause = new ManualResetEvent(false);
                    RbInstantMessaging.SendAlternativeContentsToConversationId(conversationId, message, alternativeContent, UrgencyType.Std, null, callback =>
                    {
                        if (callback.Result.Success)
                        {
                            result = true;
                            userMCQStatus.LastAdaptativeCardMessageID = callback.Data.Id;
                        }
                        else
                        {
                            result = false;
                            userMCQStatus.LastAdaptativeCardMessageID = null;
                        }
                        pause.Set();
                    });
                    pause.WaitOne();

                        
                    return result;
                }
                else
                {
                    Console.WriteLine($"We have a problem to create Adaptive Card for question [{userMCQStatus.CurrentQuestion}]");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// To edit (update) previous MCQ question set (to avoid the user to answer to it again)
        /// NOTE: 
        ///     - Today ther is a bug on RB WebClient which doesn't support the edition of message with alternate content.
        ///     - So the message edition is well performed but not visible
        /// </summary>
        /// <param name="userMCQStatus"></param>
        /// <param name="userAnswer"></param>
        /// <returns></returns>
        private Boolean EditMCQQuestion(UserMCQStatus userMCQStatus, String userAnswer)
        {
            if (!userMCQStatus.TestFinished)
            {
                String? conversationId = userMCQStatus.ConversationId;
                if (conversationId == null)
                {
                    var conversation = RbConversations.GetOrCreateConversationFromUserId(userMCQStatus.Contact?.Id);
                    conversationId = userMCQStatus.ConversationId = conversation?.Id;
                }

                if (conversationId == null)
                {
                    Console.WriteLine($"Cannot get/create conversation for [{userMCQStatus.LoginEmail}]");
                    return false;
                }

                int questionIndex;
                String? userHasAnswered;

                if (RainbowApplicationInfo.USE_ALWAYS_SAME_ADAPTIVE_CARD)
                {
                    questionIndex = userMCQStatus.CurrentQuestion + 1;
                    userHasAnswered = null;
                }
                else
                {
                    questionIndex = userMCQStatus.CurrentQuestion;
                    userHasAnswered = userAnswer;
                }
                    

                (String? message, List<MessageAlternativeContent>? alternativeContent) = CreateMCQQuestionAdaptiveCard(questionIndex, userHasAnswered);
                if ((message != null) && (alternativeContent != null))
                {
                    if (userMCQStatus.LastAdaptativeCardMessageID != null)
                    {
                        Boolean result = false;
                        ManualResetEvent pause = new ManualResetEvent(false);
                        pause.Reset();

                        RbInstantMessaging.EditMessage(conversationId, userMCQStatus.LastAdaptativeCardMessageID, message, alternativeContent, callback =>
                        {
                            result = callback.Result.Success;
                            pause.Set();
                        });

                        return result;
                    }
                    else
                    {
                        Console.WriteLine($"We don't have a valid Message Id from the previoude Adaptative Card send.");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine($"We have a problem to create Adaptive Card for question [{questionIndex}]");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// To get valid UserMCQStatus object using the specified jid
        /// </summary>
        /// <param name="userJid"></param>
        /// <returns></returns>
        private UserMCQStatus? GetUserMCQStatusByUserJid(String userJid)
        {
            return _userMCQStatusList.Find(userMCQStatus => userMCQStatus?.Contact?.Jid_im == userJid);
        }

        /// <summary>
        /// To answer to the specified message coming directly for another user
        /// </summary>
        private void AnswerToPeerMessage(MessageEventArgs? messageEvent)
        {
            if (messageEvent?.Message != null)
            {
                // Is it a response to an adaptive cards ? And if it's the case what is the answer ?
                (Boolean isAdaptiveCardAnswer, String? userAnswer) = GetUserAnswerFromMCQQuestionAdaptivCardResponse(messageEvent.Message);

                if (isAdaptiveCardAnswer)
                {
                    var userMCQStatus = GetUserMCQStatusByUserJid(messageEvent.ContactJid);
                    if (userMCQStatus != null)
                    {
                        //  Store user answer
                        if ((userMCQStatus.CurrentQuestion > 0) && (userMCQStatus.CurrentQuestion <= MAX_MCQ_QUESTION))
                        {
                            userMCQStatus.Answers[userMCQStatus.CurrentQuestion - 1] = userAnswer;
                            Console.WriteLine($"[{userMCQStatus.LoginEmail}] answered to question [{userMCQStatus.CurrentQuestion - 1}] with answer :[{userAnswer}]");
                        }

                        // Check if the test is finished
                        if (userMCQStatus.CurrentQuestion >= MAX_MCQ_QUESTION)
                        {
                            // Set as finished and send the result
                            userMCQStatus.TestFinished = true;
                            SendMCQResult(userMCQStatus);
                        }
                        else
                        {
                            // Edit previous Adaptive Card
                            if (userAnswer != null)
                                EditMCQQuestion(userMCQStatus, userAnswer);

                            // Increase question
                            userMCQStatus.CurrentQuestion++;

                            // If we use each time a new message with a new Adaptive Card, we send it now
                            if (!RainbowApplicationInfo.USE_ALWAYS_SAME_ADAPTIVE_CARD)
                                SendMCQQuestion(userMCQStatus);
                        }
                    }
                }
                else
                {
                    // We do nothing special since it's not an answer to an adaptive cards.
                }
            }
            FireTrigger(Trigger.MessageManaged);
        }

        /// <summary>
        /// To inilizae MCQ test for the specified user
        /// </summary>
        /// <param name="userMCQStatus"></param>
        private void InitializeUserMCQ(UserMCQStatus userMCQStatus)
        {
            var contacts = RbContacts.GetAllContactsFromCache();
            var contact = contacts.Find(contact => contact.LoginEmail == userMCQStatus.LoginEmail);

            if(contact == null)
            {
                userMCQStatus.TestFinished = true; // We don't know a contact with this login.
                Console.WriteLine($"No contact found with this login [{userMCQStatus.LoginEmail}]. MCQ Test is finished for him.");
            }
            else
            {
                userMCQStatus.Contact = contact;
                SendMCQQuestion(userMCQStatus);
            }

            FireTrigger(Trigger.MCQInitialized);
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
            CheckMCQforUsersAndDequeuedMessages();
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

            // We want to use conference features via API V2
            RbApplication.Restrictions.UseConferences = true;
        }

        /// <summary>
        /// To create all necessary objects from the Rainbow SDK C#
        /// </summary>
        private void CreateRainbowObjects()
        {
            RbAutoReconnection = RbApplication.GetAutoReconnection();
            RbAutoReconnection.MaxNbAttempts = 5; // For tests purpose wu use here a low value

            RbContacts = RbApplication.GetContacts();
            RbConversations = RbApplication.GetConversations();
            RbInstantMessaging = RbApplication.GetInstantMessaging();

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

            RbInstantMessaging.MessageReceived -= RbInstantMessaging_MessageReceived;
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

            switch(e.ConnectionState.State)
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

    #endregion EVENTS RAISED FROM RAINBOW OBJECTS

#endregion PRIVATE API

#region PUBLIC API

        /// <summary>
        /// Constructor for the RainbowBotBase class
        /// </summary>
        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public RainbowBotConnect()
        {
            // Instantiate a new state machine in the Created state
            _machine = new StateMachine<State, Trigger>(State.Created);

            ConfigureStateMachine();

            _messageQueue = new ConcurrentQueue<MessageEventArgs>();

            _userMCQStatusList = new List<UserMCQStatus>();

            _mcQInfo = new MCQInfo();
        }
        #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        
        ~RainbowBotConnect()
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
        public Boolean Configure(String appId, String appSecretKey, String hostname, String login, String pwd, String masterBotEmail, List<String> usersForMCQ, String? iniFolderFullPathName = null, String? iniFileName = null)
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
            SetRainbowRestrictions();

            // Create others Rainbow SDK Objects
            CreateRainbowObjects();

            // Store login/pwd
            _botLogin = login;
            _botPwd = pwd;

            // Store master bot email
            _masterBotEmail = masterBotEmail;

            // Create list of users who take the MCQ test
            if(usersForMCQ?.Count >0)
            {
                foreach(var user in usersForMCQ)
                {
                    var mcqStatus = new UserMCQStatus(user, MAX_MCQ_QUESTION);
                    _userMCQStatusList.Add(mcqStatus);
                }
            }
            else
            {
                return false;
            }

            // Create MCQ Info (and check if it's correct)
            String question;
            for(int index = 1; index <= MAX_MCQ_QUESTION; index++)
            {
                String? jsonData = Util.GetContentOfEmbeddedResource($"mcqQuestion-data-{index.ToString("00")}.json", System.Text.Encoding.UTF8);
                if(jsonData == null)
                {
                    Console.WriteLine($"Cannot get info about the question [{index}] ... Configuration cannot be done");
                    return false;
                }

                // Get Json
                var jsonNode = JSON.Parse(jsonData);
                if (jsonNode?.IsObject != true)
                {
                    Console.WriteLine($"Info about the question [{index}] is not a valid JSON... Configuration cannot be done");
                    return false;
                }

                // Get the question
                question = UtilJson.AsString(jsonNode, "question");
                List<MCQEntry> entries = new List<MCQEntry>();

                // Get items - to get list of possible answers
                var items = jsonNode["items"];
                if (items == null)
                {
                    Console.WriteLine($"Info about the question [{index}] is not correct - cannot get items... Configuration cannot be done");
                    return false;
                }

                // Loop on each item
                foreach (var item in items)
                {
                    var choice = UtilJson.AsString(item, "choice");
                    var value = UtilJson.AsString(item, "value");
                    var isCorrect = UtilJson.AsBoolean(item, "correct");

                    if ( (choice == null) || (value == null) )
                    {
                        Console.WriteLine($"Info about the question [{index}] is not correct - cannot get item info ... Configuration cannot be done");
                        return false;
                    }
                    var entry = new MCQEntry()
                    {
                        Choice = choice,
                        Value = value,
                        IsCorrect = isCorrect,
                    };
                    entries.Add(entry);
                }

                MCQQuestion mcqQuestion = new MCQQuestion()
                {
                    Entries = entries,
                    Title = question
                };

                _mcQInfo.Questions.Add(mcqQuestion);
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
