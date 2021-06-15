using System;
using System.IO;
using System.Collections.ObjectModel;

using Xamarin.Forms;

using Rainbow;

using NLog;
using MultiPlatformApplication.Helpers;

namespace MultiPlatformApplication.Models
{
    public class ConversationModel : ObservableObject
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(ConversationModel));

        String id;
        String jid;
        String peerId;
        String type;
        String name;
        String topic;
        String lastMessage;
        String presenceSource;
        Boolean presenceIsVisible;
        Int64 nbMsgUnread;
        DateTime lastMessageDateTime;
        String messageTimeDisplay;
        String avatarFilePath;
        ImageSource presenceImageSource;

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

        public String BackgroundColor => SdkWrapper.GetColorFromDisplayName(Name);

        public string Topic
        {
            get { return topic; }
            set { SetProperty(ref topic, value); }
        }

        public String PresenceSource
        {
            get { return presenceSource; }
            set {
                if (presenceSource != value)
                {
                    if (String.IsNullOrEmpty(value))
                    {
                        PresenceImageSource = null;
                        PresenceIsVisible = false;
                    }
                    else
                    {
                        PresenceImageSource = ImageSource.FromResource(value, typeof(Helpers.Helper).Assembly);
                        PresenceIsVisible = true;
                    }
                    SetProperty(ref presenceSource, value);
                }
                
            }
        }
        
        public Boolean PresenceIsVisible
        {
            get { return presenceIsVisible; }
            set { SetProperty(ref presenceIsVisible, value); }
        }

        public ImageSource PresenceImageSource
        {
            get { return presenceImageSource; }
            set { SetProperty(ref presenceImageSource, value); }
        }

        public Int64 NbMsgUnread
        {
            get { return nbMsgUnread; }
            set { SetProperty(ref nbMsgUnread, value); }
        }

        public String LastMessage
        {
            get { return lastMessage; }
            set { SetProperty(ref lastMessage, value); }
        }

        public DateTime LastMessageDateTime
        {
            get { return lastMessageDateTime; }
            set {
                SetProperty(ref lastMessageDateTime, value);
                MessageTimeDisplay = Helpers.Helper.HumanizeDateTime(LastMessageDateTime);
            }
        }

        public String MessageTimeDisplay
        {
            get { return messageTimeDisplay; }
            set { SetProperty(ref messageTimeDisplay, value); }
        }

        public String AvatarFilePath
        {
            get { return avatarFilePath; }
            set { SetProperty(ref avatarFilePath, value); }
        }

        public ConversationModel()
        {
            Id = "";
            Jid = "";
            PeerId = "";
            Type = "";
            Name = "";
            Topic = "";
            PresenceSource = "";

            NbMsgUnread = 0;

            LastMessage = "";
            LastMessageDateTime = DateTime.Now;

            AvatarFilePath = "";
        }
            
    }
}

