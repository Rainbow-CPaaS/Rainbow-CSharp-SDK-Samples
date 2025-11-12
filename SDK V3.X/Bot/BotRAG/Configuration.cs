using Rainbow.SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace BotRAG
{
    public class Configuration
    {
        private static Configuration? _instance = null;

        public String Host = "";
        public String Login = "";
        public String Password = "";
        public String Repository = "rainbow";
        
        public int MaxFileSizeInBytes = 52428800;
        public int MaxFilesByBubble = 10;
        public int MaxRecordingsByBubble = 1;
        public int MaxMessagesByBubble = 50;
        public Boolean AvoidVideoFile = true;
        public Boolean AvoidAudioFile = true;
        public Boolean AvoidImageFile = true;
        public Boolean AvoidArchiveFile = true;
        public List<String> ArchiveFiles = new List<String> { "arc", "bz", "bz2", "gz", "jar", "rar", "tar", "zip", "7z" };

        public String Llm = "llama3";
        public String PromptBodyFile = "./config/promptBody.json";
        public int ChunckSize = 1000;

        public List<String> BubbleNamesAllowed = [];


        public static Configuration Instance
        {
            get
            {
                _instance ??= new Configuration();
                return _instance;
            }
        }


        public void Update(JSONNode jsonNode)
        {
            if (jsonNode is null) return;

            Host = jsonNode["host"];
            Login = jsonNode["login"];
            Password = jsonNode["password"];
            Repository = jsonNode["repository"];

            BubbleNamesAllowed = jsonNode["bubbleNamesAllowed"];

            MaxFileSizeInBytes = jsonNode["maxFileSizeInBytes"];

            MaxFilesByBubble = jsonNode["maxFilesByBubble"];
            MaxRecordingsByBubble = jsonNode["maxRecordingsByBubble"];
            MaxMessagesByBubble = jsonNode["maxMessagesByBubble"];

            AvoidVideoFile = jsonNode["avoidVideoFile"];
            AvoidAudioFile = jsonNode["avoidAudioFile"];
            AvoidImageFile = jsonNode["avoidImageFile"];
            AvoidArchiveFile = jsonNode["avoidArchiveFile"];

            ArchiveFiles = jsonNode["archiveFiles"];

            Llm = jsonNode["llm"];
            PromptBodyFile = jsonNode["promptBodyFile"];

            ChunckSize = jsonNode["chunckSize"];
        }


        /*
"host": "api1.nlp.chapsvisiondev.com",
      "login": "rainbow",
      "password": "0vertheRainbow2025!",
      "repository": "rainbow",

      "__comment_maxFileSizeInBytes": "if set to -1 there is no restriction on size for files or recording",
      "maxFileSizeInBytes": 52428800,

      "__comment_bubbleNamesAllowed_part1": "if not specified / null / empty list, all bubbles where the bot is a member is managed. !!! CASE SENSITIVE !!!",
      "__comment_bubbleNamesAllowed_part2": "* can be used as wildcard at the end of a name to allow creation/management of bubble of the fly",
      "bubbleNamesAllowed": [
        "5f4e1c2e1d2b3a0017c8b456",
        "5f4e1c2e1d2b3a0017c8b457"
      ]
         */

    }
}
