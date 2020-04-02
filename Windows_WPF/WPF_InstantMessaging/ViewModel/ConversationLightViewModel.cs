using System;
using System.Windows;
using System.Windows.Media.Imaging;

using InstantMessaging.Helpers;
using InstantMessaging.Pool;

namespace InstantMessaging.ViewModel
{
    public class ConversationLightViewModel : ObservableObject
    {
        String m_id;
        String m_jid;
        String m_peerId;
        String m_type;
        String m_name;
        String m_topic;
        String m_lastMessage;
        String m_presenceSource;
        Int64 m_nbMsgUnread;
        DateTime m_lastMessageDateTime;
        String m_messageTimeDisplay;
        BitmapImage m_avatarSource;

        public string Id
        {
            get { return m_id; }
            set { SetProperty(ref m_id, value); }
        }

        public string Jid
        {
            get { return m_jid; }
            set { SetProperty(ref m_jid, value); }
        }

        public string PeerId
        {
            get { return m_peerId; }
            set { SetProperty(ref m_peerId, value); }
        }

        public string Type
        {
            get { return m_type; }
            set { SetProperty(ref m_type, value); }
        }

        public string Name
        {
            get { return m_name; }
            set { SetProperty(ref m_name, value); }
        }

        public String BackgroundColor => AvatarPool.GetColorFromDisplayName(Name);

        public string Topic
        {
            get { return m_topic; }
            set { SetProperty(ref m_topic, value); }
        }

        public Visibility TopicIsVisible => String.IsNullOrEmpty(Topic) ? Visibility.Collapsed : Visibility.Visible;

        public string PresenceSource
        {
            get { return m_presenceSource; }
            set { SetProperty(ref m_presenceSource, value); }
        }
        public Visibility PresenceIsVisible => String.IsNullOrEmpty(PresenceSource) ? Visibility.Collapsed : Visibility.Visible;
        public BitmapImage PresenceImageSource => String.IsNullOrEmpty(PresenceSource) ? null : Helper.GetBitmapImageFromResource(PresenceSource);

        public Int64 NbMsgUnread
        {
            get { return m_nbMsgUnread; }
            set { SetProperty(ref m_nbMsgUnread, value); }
        }

        public Visibility NbMsgUnreadIsVisible => (NbMsgUnread > 0) ? Visibility.Visible : Visibility.Collapsed;

        public String LastMessage
        {
            get { return m_lastMessage; }
            set { SetProperty(ref m_lastMessage, value); }
        }

        public Visibility LastMessageIsVisible => String.IsNullOrEmpty(LastMessage) ? Visibility.Collapsed : Visibility.Visible;

        public DateTime LastMessageDateTime
        {
            get { return m_lastMessageDateTime; }
            set { SetProperty(ref m_lastMessageDateTime, value); }
        }

        public String MessageTimeDisplay
        {
            get { return m_messageTimeDisplay; }
            set { SetProperty(ref m_messageTimeDisplay, value); }
        }

        public BitmapImage AvatarImageSource
        {
            get { return m_avatarSource; }
            set { SetProperty(ref m_avatarSource, value); }
        }
    }
}

