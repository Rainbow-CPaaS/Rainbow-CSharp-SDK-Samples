using AdaptiveCards.Templating;
using BotAdaptiveCards.Model;
using BotLibrary.Model;
using Rainbow;
using Rainbow.Enums;
using Rainbow.Model;
using Rainbow.SimpleJSON;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Util = Rainbow.Example.Common.Util;

namespace BotAdaptiveCards
{
    public class BotBasicMessages: BotLibrary.BotBase
    {
        // YOU CAN SELECT HERE IF THE SAME AdaptiveCard IS USED FOR ALL THE MCQ or NOT
        static internal readonly Boolean USE_ALWAYS_SAME_ADAPTIVE_CARD = true;

        private readonly int MAX_MCQ_QUESTION = 5;

        private Contacts? _rbContacts = null;
        private InstantMessaging? _rbInstantMessaging = null;

        private BotConfigurationExtended? _currentBotConfigurationExtended = null;
        private MCQInfo _mcQInfo;
        private Dictionary<String, AccountMCQStatus> _accountMCQStatusList; // Key: Jid of Contact

        public BotBasicMessages()
        {
            _mcQInfo = new();

            if (!Init())
                throw new Exception("Cannot Init MCQ questions");

            _accountMCQStatusList = new();
        }

        private Boolean Init()
        {
            String question;
            for (int index = 1; index <= MAX_MCQ_QUESTION; index++)
            {
                String? jsonData = Helper.GetContentOfEmbeddedResource($"mcqQuestion-data-{index.ToString("00")}.json", System.Text.Encoding.UTF8);
                if (jsonData == null)
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
                question = jsonNode["question"];
                List<MCQEntry> entries = new List<MCQEntry>();

                // Get items - to get list of possible answers
                var items = jsonNode["items"];
                if (items == null)
                {
                    Console.WriteLine($"Info about the question [{index}] is not correct - cannot get items... Configuration cannot be done");
                    return false;
                }

                // Loop on each item
                foreach (JSONNode item in items)
                {
                    var choice = item["choice"];
                    var value = item["value"];
                    var isCorrect = item["correct"];

                    if ((choice == null) || (value == null))
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
            return true;
        }

    #region Override some methods of BotLibrary.BotBase to add specific features in this Bot

        public override async Task InstantMessageReceivedAsync(Rainbow.Model.Message message)
        {
            // We don't manage message in Bubble context
            if (message.ToBubble is not null)
                return;

            if (!String.IsNullOrEmpty(message.FromContact?.Peer?.Jid))
            {
                // Check if the sender must use MCQ
                if(_accountMCQStatusList.TryGetValue(message.FromContact.Peer.Jid, out AccountMCQStatus? userMCQStatus))
                {
                    // Is it a response to an adaptive cards ? And if it's the case what is the answer ?
                    (Boolean isAdaptiveCardAnswer, String? userAnswer) = GetUserAnswerFromMCQQuestionAdaptivCardResponse(message);

                    if (isAdaptiveCardAnswer)
                    {
                        //  Store user answer
                        if ((userMCQStatus.CurrentQuestion > 0) && (userMCQStatus.CurrentQuestion <= MAX_MCQ_QUESTION))
                        {
                            userMCQStatus.Answers[userMCQStatus.CurrentQuestion - 1] = userAnswer;
                            Util.WriteDarkYellow($"[{userMCQStatus.Contact.LoginEmail}] answered to question [{userMCQStatus.CurrentQuestion - 1}] with answer :[{userAnswer}]");
                        }

                        // Edit previous Adaptive Card
                        if (userAnswer != null)
                            await EditMCQQuestion(userMCQStatus, userAnswer);

                        // Check if the test is finished
                        if (userMCQStatus.CurrentQuestion >= MAX_MCQ_QUESTION)
                        {
                            // Set as finished and send the result
                            userMCQStatus.TestFinished = true;
                            await SendMCQResultAsync(userMCQStatus);
                        }
                        else
                        {
                            // Increase question
                            userMCQStatus.CurrentQuestion++;

                            // If we use each time a new message with a new Adaptive Card, we send it now
                            if (!USE_ALWAYS_SAME_ADAPTIVE_CARD)
                                await SendMCQQuestionAsync(userMCQStatus);
                        }
                    }
                    else
                    {
                        // We do nothing special since it's not an answer to an adaptive cards.
                    }
                }
            }
        }

        public override async Task ConnectedAsync()
        {
            await UpdateConfigurationAsync(_currentBotConfigurationExtended);
        }

        public override async Task BotConfigurationUpdatedAsync(BotConfigurationUpdate botConfigurationUpdate)
        {
            // Ensure to have an object not null
            if (botConfigurationUpdate is null)
                return;

            _rbContacts ??= Application.GetContacts();
            _rbInstantMessaging ??= Application.GetInstantMessaging();

            // BotConfigurationExtended object has been created to store data structure specific for this bot
            // We try to parse JSON Node to fill this data structure and if it's correct we update the configuration
            if (BotConfigurationExtended.FromJsonNode(botConfigurationUpdate.JSONNodeBotConfiguration, out BotConfigurationExtended botConfigurationExtended))
            {
                await UpdateConfigurationAsync(botConfigurationExtended);
            }

            await Task.CompletedTask;
        }

    #endregion Override some methods of BotLibrary.BotBase to add specific features in this Bot

        private async Task UpdateConfigurationAsync(BotConfigurationExtended? botConfigurationExtended)
        {
            if (botConfigurationExtended is null)
                return; 

            _currentBotConfigurationExtended = botConfigurationExtended;

            // We do nothing if we are not yet connected
            if (!Application.IsConnected())
                return;

            // Check accounts which must have the MCQ
            var accounts = botConfigurationExtended.McqAccounts;

            if(accounts?.Count > 0)
            {
                foreach(var account in accounts)
                {
                    var contact = await GetContactAsync(account);
                    if(contact is null)
                    {
                        Util.WriteRed($"Cannot find a Contact using this Account:[{account}]");
                    }
                    else
                    {
                        // Do we have already this account
                        if(!_accountMCQStatusList.ContainsKey(contact.Peer.Jid))
                        {
                            var accountMCQStatus = new AccountMCQStatus(contact, MAX_MCQ_QUESTION);
                            _accountMCQStatusList[contact.Peer.Jid] = accountMCQStatus;

                            // Send the first message
                            SendMCQQuestionAsync(accountMCQStatus);
                        }
                    }
                }
            }
        }

        private async Task<Contact?> GetContactAsync(Account account)
        {
            if (_rbContacts is null)
                return null;

            Contact? result = null;
            if(!String.IsNullOrEmpty(account.Id))
                result = await _rbContacts.GetContactByIdInCacheFirstAsync(account.Id);

            if (result is not null)
                return result;

            if (!String.IsNullOrEmpty(account.Jid))
                result = await _rbContacts.GetContactByJidInCacheFirstAsync(account.Jid);

            if (result is not null)
                return result;

            if (!String.IsNullOrEmpty(account.Login))
                result = _rbContacts.GetAllContacts()?.Find(contact => contact?.LoginEmail?.Equals(account.Login, StringComparison.InvariantCultureIgnoreCase) == true);

            return result;
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
                    if (alternativeContent.Type == "rainbow/json")
                    {
                        isAdaptiveCardAnswer = true;
                        var content = alternativeContent.Content;

                        var jsonNode = JSON.Parse(content);
                        var questionId = jsonNode["questionId"];

                        // For the first question, we set a default answer
                        if (questionId == "00")
                            answer = " ";
                        else
                            answer = jsonNode["MCQSelection"];
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

            if ((questionIndex >= 0) && (questionIndex <= MAX_MCQ_QUESTION))
            {
                String? jsonTemplate = Helper.GetContentOfEmbeddedResource("mcqQuestion-template.json", System.Text.Encoding.UTF8);
                String? jsonData = Helper.GetContentOfEmbeddedResource($"mcqQuestion-data-{questionIndex.ToString("00")}.json", System.Text.Encoding.UTF8);

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
                    message = jsonNode["title"];

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
        /// To send to the specified user the next MCQ question
        /// </summary>
        /// <param name="accountMCQStatus"></param>
        /// <returns></returns>
        private async Task<Boolean> SendMCQQuestionAsync(AccountMCQStatus accountMCQStatus)
        {
            if (_rbInstantMessaging is null)
                return false;

            if (!accountMCQStatus.TestFinished)
            {
                (String? message, List<MessageAlternativeContent>? alternativeContent) = CreateMCQQuestionAdaptiveCard(accountMCQStatus.CurrentQuestion);
                if ((message != null) && (alternativeContent != null))
                {
                    Boolean result = false;
                    ManualResetEvent pause = new ManualResetEvent(false);
                    var sdkResult = await _rbInstantMessaging.SendMessageWithAlternativeContentsAsync(accountMCQStatus.Contact, message, alternativeContent, MessageUrgencyType.Std, null);
                    if (sdkResult.Success)
                    {
                        result = true;
                        accountMCQStatus.LastAdaptativeCardMessage = sdkResult.Data;
                    }
                    else
                    {
                        result = false;
                        accountMCQStatus.LastAdaptativeCardMessage = null;
                    }

                    return result;
                }
                else
                {
                    Util.WriteRed($"We have a problem to create Adaptive Card for question [{accountMCQStatus.CurrentQuestion}]");
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
        /// <param name="accountMCQStatus"></param>
        /// <param name="userAnswer"></param>
        /// <returns></returns>
        private async Task<Boolean> EditMCQQuestion(AccountMCQStatus accountMCQStatus, String userAnswer)
        {
            if (_rbInstantMessaging is null)
                return false;

            if (!accountMCQStatus.TestFinished)
            {
                int questionIndex;
                String? userHasAnswered;

                if (USE_ALWAYS_SAME_ADAPTIVE_CARD)
                {
                    questionIndex = accountMCQStatus.CurrentQuestion + 1;
                    userHasAnswered = null;
                }
                else
                {
                    questionIndex = accountMCQStatus.CurrentQuestion;
                    userHasAnswered = userAnswer;
                }

                (String? message, List<MessageAlternativeContent>? alternativeContent) = CreateMCQQuestionAdaptiveCard(questionIndex, userHasAnswered);
                if ((message != null) && (alternativeContent != null))
                {
                    if (accountMCQStatus.LastAdaptativeCardMessage != null)
                    {
                        var sdkResult = await _rbInstantMessaging.EditMessageAsync(accountMCQStatus.LastAdaptativeCardMessage, message, alternativeContent);

                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"We don't have a valid Message from the previoude Adaptative Card send.");
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
        /// To send to the specified user the result of his MCQ test
        /// </summary>
        /// <param name="accountMCQStatus"></param>
        /// <returns></returns>
        private async Task<Boolean> SendMCQResultAsync(AccountMCQStatus accountMCQStatus)
        {
            // Create content on the Adaptive Card
            String adaptiveCardResult = "";
            int correctAnswers = 0;

            for (int i = 0; i < MAX_MCQ_QUESTION; i++)
            {
                var question = _mcQInfo.Questions[i].Title;

                adaptiveCardResult += MCQ.MCQ_RESULT_QUESTION.Replace("{question}", $"Question {i + 1}: {_mcQInfo.Questions[i].Title}");
                adaptiveCardResult += MCQ.MCQ_RESULT_ANSWER.Replace("{answer}", $"Answer expected: {_mcQInfo.Questions[i].CorrectChoice()}");

                var userAnwer = accountMCQStatus.Answers[i];
                if (userAnwer != null)
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

            Console.WriteLine($"[{accountMCQStatus.Contact.LoginEmail}] has finished his test - Result: {correctAnswers} / {MAX_MCQ_QUESTION}");

            var message = "MCQ Test - Result";

            // Create an Message Alternative Content
            MessageAlternativeContent messageAlternativeContent = new MessageAlternativeContent();
            messageAlternativeContent.Type = "form/json";
            messageAlternativeContent.Content = adaptiveCardResult;

            var alternativeContent = new List<MessageAlternativeContent> { messageAlternativeContent };

            SdkResult<Message> sdkResult;

            if (USE_ALWAYS_SAME_ADAPTIVE_CARD)
            {
                sdkResult = await _rbInstantMessaging.EditMessageAsync(accountMCQStatus.LastAdaptativeCardMessage, message, alternativeContent);
            }
            else
            {
                sdkResult = await _rbInstantMessaging.SendMessageWithAlternativeContentsAsync(accountMCQStatus.Contact, message, alternativeContent, MessageUrgencyType.Std, null);
            }

            return sdkResult.Success;
        }
    }
}
