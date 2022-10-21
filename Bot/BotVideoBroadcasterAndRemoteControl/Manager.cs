using System;
using System.Collections.Generic;
using System.Text;

namespace BotVideoOrchestratorAndRemoteControl
{
    public class Manager
    {
        public String Email { get; set; }
        public String? Id { get; set; }
        public String? Jid { get; set; }

        public Manager(string email, string? id = null, string? jid = null)
        {
            Email = email;
            Id = id;
            Jid = jid;
        }
    }
}
