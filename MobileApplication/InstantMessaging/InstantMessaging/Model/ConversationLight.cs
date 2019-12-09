using System;

using Xamarin.Forms;

using Rainbow.Helpers;

namespace InstantMessaging
{
    public class ConversationLight : ObservableObject
    {
        String id;
        String jid;
        String peerId;
        String type;
        String name;
        String topic;
        String lastMessage;
        String presenceSource;
        Int64 nbMsgUnread;
        String nbMsgUnreadIsVisible;
        DateTime lastMessageDateTime;
        String messageTimeDisplay;
        ImageSource avatarSource;

        public string Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        public string Jid
        {
            get { return jid; }
            set { SetProperty(ref jid, value); }
        }

        public string PeerId
        {
            get { return peerId; }
            set { SetProperty(ref peerId, value); }
        }

        public string Type
        {
            get { return type; }
            set { SetProperty(ref type, value); }
        }

        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        public String BackgroundColor => Rainbow.Helpers.AvatarPool.GetColorFromDisplayName(Name);

        public string Topic
        {
            get { return topic; }
            set { SetProperty(ref topic, value); }
        }

        public String TopicIsVisible => String.IsNullOrEmpty(Topic) ? "False" : "True";

        public string PresenceSource
        {
            get { return presenceSource; }
            set { SetProperty(ref presenceSource, value); }
        }
        public String PresenceIsVisible => String.IsNullOrEmpty(PresenceSource) ? "False" : "True";

        public Int64 NbMsgUnread
        {
            get { return nbMsgUnread; }
            set { SetProperty(ref nbMsgUnread, value); }
        }

        public String NbMsgUnreadIsVisible
        {
            get { return nbMsgUnreadIsVisible; }
            set { SetProperty(ref nbMsgUnreadIsVisible, value); }
        }

        public String LastMessage
        {
            get { return lastMessage; }
            set { SetProperty(ref lastMessage, value); }
        }

        public String LastMessageIsVisible => String.IsNullOrEmpty(LastMessage) ? "False" : "True";

        public DateTime LastMessageDateTime
        {
            get { return lastMessageDateTime; }
            set { SetProperty(ref lastMessageDateTime, value); }
        }

        public String MessageTimeDisplay
        {
            get { return messageTimeDisplay; }
            set { SetProperty(ref messageTimeDisplay, value); }
        }

        public ImageSource AvatarSource
        {
            get { return avatarSource; }
            set { SetProperty(ref avatarSource, value); }
        }
    }
}

