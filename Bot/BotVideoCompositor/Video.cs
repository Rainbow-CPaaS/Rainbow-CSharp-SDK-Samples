using System;
using System.Collections.Generic;

namespace BotVideoCompositor
{
    public class Video
    {
        public String Uri { get; set; }

        public Dictionary<String, String> Settings { get; set; }

        public Video() 
        {
            Uri = "";
            Settings = new Dictionary<String, String>();
        }
    }
}
