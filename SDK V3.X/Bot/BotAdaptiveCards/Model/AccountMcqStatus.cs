using System;
using System.Collections.Generic;
using System.Text;
using Rainbow.Model;

namespace BotAdaptiveCards
{
    /// <summary>
    /// To store the status of one user about the MCQ
    /// </summary>
    public class AccountMCQStatus
    {
        /// <summary>
        /// Rainbow Contact associated to this user - the LoginEmail will be used to find it.
        /// </summary>
        public Contact Contact { get; set; }

        /// <summary>
        /// Index of the question that this user must answer
        /// </summary>
        public int CurrentQuestion { get; set; }

        /// <summary>
        /// To store the last Message which contains the Adaptive Card sent to this user and waiting for an answer.
        /// </summary>
        public Message? LastAdaptativeCardMessage { get; set; }

        /// <summary>
        /// To store the list of answers
        /// </summary>
        public String?[] Answers { get; internal set; }

        /// <summary>
        /// To know if the test is finished or not 
        /// </summary>
        public Boolean TestFinished { get; set; }

        public AccountMCQStatus(Contact contact, int maxQuestions)
        {
            Contact = contact;

            CurrentQuestion = 0;

            LastAdaptativeCardMessage = null;

            Answers = new String?[maxQuestions];

            TestFinished = false;
        }
    }
}
