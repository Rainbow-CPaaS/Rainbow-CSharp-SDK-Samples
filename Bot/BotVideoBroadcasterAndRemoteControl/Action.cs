using System;
using System.Collections.Generic;
using System.Text;
using static Rainbow.Model.Contact;

namespace BotVideoOrchestratorAndRemoteControl
{
    public class Action
    {
        public String Trigger { get; set; }

        public String Type { get; set; }

        public ActionDetails Details { get; set; }

        public Action()
        {
            Trigger = Type = "";
            Details = new ActionDetails();
        }
    }
}
