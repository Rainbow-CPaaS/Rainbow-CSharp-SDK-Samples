using System;
using System.Collections.Generic;
using System.Text;

namespace BotVideoOrchestratorAndRemoteControl
{
    public class Broadcaster
    {
        public ServerConfig? ServerConfig { get; set; }

        public String? Name { get; set; }

        public String? Login { get; set; }

        public String? Pwd { get; set; }

        public String? VideoURI { get; set; }

        public String? SharingURI { get; set; }

        public Boolean? AutoJoinConference { get; set; }

        public String? OnlyJoinBubbleJid { get; set; }

        public List<Manager>? Managers { get; set; }

        public List<Action>? CommandsFromIM { get; set; }

        public List<Action>? CommandsFromAC { get; set; }
    }
}
