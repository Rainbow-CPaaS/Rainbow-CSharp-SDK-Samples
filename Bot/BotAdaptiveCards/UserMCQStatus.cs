using System;
using System.Collections.Generic;
using System.Text;
using Rainbow.Model;

namespace BotAdaptiveCards
{
    /// <summary>
    /// To store the status of one user about the MCQ
    /// </summary>
    public class UserMCQStatus
    {
        /// <summary>
        /// Login Email - the only info we know about this user
        /// </summary>
        public String LoginEmail { get; set; }

        /// <summary>
        /// Rainbow Contact associated to this user - the LoginEmail will be used to find it.
        /// </summary>
        public Contact? Contact { get; set; }

        /// <summary>
        /// Converstion ID with this user
        /// </summary>
        public String? ConversationId { get; set; }

        /// <summary>
        /// Index of the question that this user must answer
        /// </summary>
        public int CurrentQuestion { get; set; }

        /// <summary>
        /// To store the last message Id which contains the Adaptive Card sent to this user and waiting for an answer.
        /// </summary>
        public String? LastAdaptativeCardMessageID { get; set; } 

        /// <summary>
        /// To store the list of answers
        /// </summary>
        public String?[] Answers { get; internal set; }

        // To know if the test is finished or not
        public Boolean TestFinished { get; set; }

        public UserMCQStatus(String loginEmail, int maxQuestions)
        {
            LoginEmail = loginEmail;

            Contact = null;

            ConversationId = null;

            CurrentQuestion = 0;

            LastAdaptativeCardMessageID = null;

            Answers = new String?[maxQuestions];
        }
    }
}
