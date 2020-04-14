using System;
using System.Collections.Generic;
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
        BitmapImage m_avatarSource;

        String m_backgroundColor;           // Calculated from m_name
        Visibility m_topicIsVisible;        // Calculated from m_topic
        Visibility m_mastMessageIsVisible;  // Calculated from m_lastMessage
        Visibility m_presenceIsVisible;     // Calculated from m_presenceSource
        BitmapImage m_presenceImageSource;  // Calculated from m_presenceSource
        Visibility m_nbMsgUnreadIsVisible;  // Calculated from m_presenceSource
        String m_messageTimeDisplay;        // Calculated from m_lastMessageDateTime

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
            set 
            { 
                SetProperty(ref m_name, value);
                BackgroundColor = AvatarPool.GetColorFromDisplayName(Name);
            }
        }

        public string BackgroundColor
        {
            get { return m_backgroundColor; }
            set { SetProperty(ref m_backgroundColor, value); }
        }

        public string Topic
        {
            get { return m_topic; }
            set
            {
                SetProperty(ref m_topic, value);
                if (String.IsNullOrEmpty(Topic))
                    TopicIsVisible = Visibility.Collapsed;
                else
                    TopicIsVisible = Visibility.Visible;
            }
        }

        public Visibility TopicIsVisible
        {
            get { return m_topicIsVisible; }
            set { SetProperty(ref m_topicIsVisible, value); }
        }

        public string PresenceSource
        {
            get { return m_presenceSource; }
            set { 
                    SetProperty(ref m_presenceSource, value);
                    if (String.IsNullOrEmpty(PresenceSource))
                    {
                        PresenceIsVisible = Visibility.Collapsed;
                        PresenceImageSource = null;
                    }
                    else
                    {
                        PresenceIsVisible = Visibility.Visible;
                        PresenceImageSource = Helper.GetBitmapImageFromResource(PresenceSource);
                    }
                }
        }

        public Visibility PresenceIsVisible
        {
            get { return m_presenceIsVisible; }
            set { SetProperty(ref m_presenceIsVisible, value); }
        }

        public BitmapImage PresenceImageSource
        {
            get { return m_presenceImageSource; }
            set { SetProperty(ref m_presenceImageSource, value); }
        }

        public Int64 NbMsgUnread
        {
            get { return m_nbMsgUnread; }
            set 
            { 
                SetProperty(ref m_nbMsgUnread, value);
                if (NbMsgUnread > 0)
                    NbMsgUnreadIsVisible = Visibility.Visible;
                else
                    NbMsgUnreadIsVisible = Visibility.Collapsed;
            }
        }

        public Visibility NbMsgUnreadIsVisible
        {
            get { return m_nbMsgUnreadIsVisible; }
            set { SetProperty(ref m_nbMsgUnreadIsVisible, value); }
        }

        public String LastMessage
        {
            get { return m_lastMessage; }
            set 
            { 
                SetProperty(ref m_lastMessage, value);
                if (String.IsNullOrEmpty(LastMessage))
                    LastMessageIsVisible = Visibility.Collapsed;
                else
                    LastMessageIsVisible = Visibility.Visible;
            }
        }

        public Visibility LastMessageIsVisible
        {
            get { return m_mastMessageIsVisible; }
            set { SetProperty(ref m_mastMessageIsVisible, value); }
        }

        public DateTime LastMessageDateTime
        {
            get { return m_lastMessageDateTime; }
            set 
            { 
                SetProperty(ref m_lastMessageDateTime, value);
                LastMessageTimeDisplay = Helper.HumanizeDateTime(LastMessageDateTime);
            }
        }

        public String LastMessageTimeDisplay
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

