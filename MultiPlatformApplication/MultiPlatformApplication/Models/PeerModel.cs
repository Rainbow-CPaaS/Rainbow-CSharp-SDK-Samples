using MultiPlatformApplication.Helpers;
using Rainbow.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiPlatformApplication.Models
{
    public class PeerModel
    {
        public String Id { get; set; }
        
        public String Jid { get; set; }

        public String DisplayName { get; set; }

        public String Type { get; set; } // "user" , "room", "bot"
        
        public Boolean DisplayPresence { get; set; }

        public PeerModel()
        {
        }

        public PeerModel(Contact contact)
        {
            Id = contact.Id;
            Jid = contact.Jid_im;
            DisplayName = Rainbow.Util.GetContactDisplayName(contact);
            Type = Rainbow.Model.Conversation.ConversationType.User;
        }
    }
}
