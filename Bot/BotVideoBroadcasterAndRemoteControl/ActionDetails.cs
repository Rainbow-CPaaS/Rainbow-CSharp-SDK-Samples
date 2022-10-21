using System;
using System.Collections.Generic;
using System.Text;

namespace BotVideoOrchestratorAndRemoteControl
{
    public class ActionDetails
    {
        public Dictionary<String, String> Info { get; set; }

        public Action? NextAction { get; set; }
        
        public ActionDetails()
        {
            Info = new Dictionary<string, string>();
            NextAction = null;
        }
    }
}
