using System;
using System.Collections.Generic;

namespace BotVideoOrchestratorAndBroadcaster
{
    public class Video
    {
        public int Index;
        public String Uri { get; set; }

        public String Filter { get; set; }

        public Dictionary<String, String> Settings { get; set; }

        public Video(int index)
        {
            Index = index;
            Uri = "";
            Settings = new Dictionary<String, String>();
            Filter = "";
        }
    }
}
