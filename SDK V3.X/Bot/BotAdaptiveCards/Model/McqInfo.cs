using System;
using System.Collections.Generic;
using System.Text;

namespace BotAdaptiveCards
{

    static public class MCQ
    {
        static public String MCQ_RESULT_HEADER =
        @"{
        ""$schema"": ""http://adaptivecards.io/schemas/adaptive-card.json"",
        ""type"": ""AdaptiveCard"",
        ""version"": ""1.3"",
        ""body"": [
          {
            ""type"": ""TextBlock"",
            ""size"": ""large"",
            ""weight"": ""bolder"",
            ""text"": ""{result}"",
            ""horizontalAlignment"": ""center"",
            ""wrap"": true
          }";

        static public String MCQ_RESULT_FOOTER =
        @"]}";

        static public String MCQ_RESULT_QUESTION =
        @",{
            ""type"": ""TextBlock"",
            ""size"": ""medium"",
            ""weight"": ""bolder"",
            ""text"": ""{question}"",
            ""horizontalAlignment"": ""left"",
            ""wrap"": true
        }";

        static public String MCQ_RESULT_ANSWER =
        @",{
            ""type"": ""TextBlock"",
            ""size"": ""medium"",
            ""weight"": ""default"",
            ""text"": ""{answer}"",
            ""horizontalAlignment"": ""left"",
            ""wrap"": true,
            ""color"": ""accent""
        }";

        static public String MCQ_RESULT_CORRECT_ANSWER =
        @",{
            ""type"": ""TextBlock"",
            ""size"": ""medium"",
            ""weight"": ""Default"",
            ""text"": ""You have correclty answered."",
            ""horizontalAlignment"": ""left"",
            ""wrap"": true,
            ""color"": ""good""
        }";

    }

    public class MCQInfo
    {
        public List<MCQQuestion> Questions { get; set; }

        public MCQInfo()
        {
            Questions = new List<MCQQuestion>();
        }
    }

    public class MCQQuestion
    {
        public String Title { get; set; }

        public List<MCQEntry> Entries { get; set; }

        public String? CorrectChoice()
        {
            foreach (MCQEntry entry in Entries)
            {
                if (entry.IsCorrect)
                    return entry.Choice;
            }
            return null;
        }

        public Boolean IsCorrectValue(String value)
        {
            foreach (MCQEntry entry in Entries)
            {
                if (entry.IsCorrect && entry.Value.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        public String GetChoice(String value)
        {
            foreach (MCQEntry entry in Entries)
            {
                if (entry.Value.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                    return entry.Choice;
            }
            return "";
        }

        public MCQQuestion()
        {
            Title = "";
            Entries = new List<MCQEntry>();
        }
    }

    public class MCQEntry
    {
        public String Choice { get; set; }

        public String Value { get; set; }

        public Boolean IsCorrect { get; set; }

        public MCQEntry()
        {
            Choice = Value = "";
            IsCorrect = false;
        }
    }
}
