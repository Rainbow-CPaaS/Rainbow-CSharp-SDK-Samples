﻿using Rainbow.Model;

namespace Login_Simple
{
    internal class SIPDeviceStatus
    {
        public Peer Peer { get; set; }

        public String DeviceId { get; set; }

        public String DeviceShortNumber { get; set; }

        public Boolean Registered { get; set; }
    }
}