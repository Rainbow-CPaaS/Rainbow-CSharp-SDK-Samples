using MultiPlatformApplication.Helpers;
using Rainbow.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiPlatformApplication.Models
{
    public class MessageElementModel: ObservableObject
    {
        public String Id { get; set; }

        public String ConversationType { get; set; } // Rainbow.Model.Conversation.ConversationType.Room | User | Bot | Channel

        public String ConversationId { get; set; } // Needed to manage attachment

        public DateTime Date { get; set; } // Necessary ?

        public String DateDisplayed { get; set; }

        public PeerModel Peer { get; set; }

        public MessageReplyModel Reply { get; set; }

        public Boolean IsForwarded { get; set; }

        public String ReplaceId { get; set; }

        public String Receipt { get; set; }

        public CallLogAttachment CallLogAttachment { get; set; }

        public MessageContentModel Content { get; set; }

        public MessageElementModel()
        {
            Peer = new PeerModel();
            Content = new MessageContentModel();
        }

    }

    public class MessageReplyModel
    {
        public String Id { get; set; }

        public String Stamp { get; set; }

        public PeerModel Peer { get; set; }

        public MessageContentModel Content { get; set; }

        public MessageReplyModel()
        {
            Peer = new PeerModel();
            Content = new MessageContentModel();
        }
    }

    public class MessageContentModel
    {
        public UrgencyType Urgency { get; set; } //  Std, Low, Middle, High

        public String Type { get; set; } // "message", "deletedMessage",  "image",  "displayName", "date", "event"

        public String Body { get; set; }

        public Boolean WithAvatar { get; set; }

        public MessageAttachmentModel Attachment { get; set; }

    }

    public class MessageAttachmentModel
    {
        public String Id { get; set; }

        public String Name { get; set; }

        public String Size { get; set; }
    }
}
