using System;
using System.Collections.Generic;
using System.Text;

namespace BotVideoOrchestratorAndRemoteControl
{
    public class ServerConfig
    {
        public String AppId { get; set; }
        
        public String AppSecretKey { get; set; }
        
        public String HostName { get; set; }

        public ServerConfig()
        {
            AppId = "";
            AppSecretKey = "";
            HostName = "";
        }
    }
}
