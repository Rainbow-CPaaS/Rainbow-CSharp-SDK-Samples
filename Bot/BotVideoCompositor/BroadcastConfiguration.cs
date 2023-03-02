using Newtonsoft.Json.Linq;
using Rainbow.Medias;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace BotVideoCompositor
{
    internal class BroadcastConfiguration
    {
        public Boolean StartBroadcast;

        public Dictionary<String, string> LastACMessageIdByConversationId { get; set; } // Id of the last IM message sent about BroabcastConfiguration by ConversationId

        public Dictionary<String, MediaInput> MediaInputCollections;

        public MediaFiltered? MediaFiltered = null;

        public List<String> StreamsSelected; // List of Streams (by id) delimited by comma
        
        public String Mode; //None, OneStream, Overlay or Mosaic
        
        public String Size; // "1920x1080" for example - Used in OneStream OR Overlay mode

        public String VignetteSize; // "1920x1080" for example - Used in Mosaic OR Overlay mode

        public int Fps; // 25 for example

        public String Layout; // For Overlay: TL, TR, BR, BL.   For Mosaic: G, H, V, H-M+2v, V-M+2v, W 

        public Boolean ConfigError;

        public Boolean StayConnectedToStreams;

        public BroadcastConfiguration()
        {
            StartBroadcast = false;
            StayConnectedToStreams = true;

            LastACMessageIdByConversationId = new Dictionary<string, string>();
            StreamsSelected = new List<string>();
            MediaInputCollections = new Dictionary<string, MediaInput>();

            Mode = "None";
            Size = "1920x1080";
            VignetteSize = "800x640";
            Fps = 5;
            Layout = "TR";
        }

        public void Init()
        {
            Mode = "None";

            ConfigError = false;

            if (RainbowApplicationInfo.outputs?.Count > 0)
                Size = RainbowApplicationInfo.outputs.First().Key;
            else
                Size = "1920x1080";

            if (RainbowApplicationInfo.vignettes?.Count > 0)
                VignetteSize = RainbowApplicationInfo.vignettes.First().Key;
            else
                VignetteSize = "800x640";

            if (RainbowApplicationInfo.fps?.Count > 0)
                Fps = RainbowApplicationInfo.fps[0];
            else
                Fps = 5;

            if (RainbowApplicationInfo.overlayDisplay?.Count > 0)
                Layout = RainbowApplicationInfo.overlayDisplay.First().Key;
            else
                Layout = "TR";

        }

    }
}
