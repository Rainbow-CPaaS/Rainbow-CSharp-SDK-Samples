using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

using InstantMessaging.Helpers;
using InstantMessaging.Pool;

namespace InstantMessaging.ViewModel
{
    public class FavoriteViewModel : ObservableObject
    {
        String m_id;
        String m_peerId;
        String m_jid;
        Int64 m_position;
        String m_name;
        String m_presenceSource;
        Int64 m_nbMsgUnread;
        BitmapImage m_avatarSource;
        Boolean m_isVisible;
        
        Visibility m_presenceIsVisible;     // Calculated from m_presenceSource
        BitmapImage m_presenceImageSource;  // Calculated from m_presenceSource
        Visibility m_nbMsgUnreadIsVisible;  // Calculated from m_nbMsgUnread
        Visibility m_UIIsVisible;           // Calculated from isVisible

        public string Id
        {
            get { return m_id; }
            set { SetProperty(ref m_id, value); }
        }

        public string PeerId
        {
            get { return m_peerId; }
            set { SetProperty(ref m_peerId, value); }
        }

        public string Jid
        {
            get { return m_jid; }
            set { SetProperty(ref m_jid, value); }
        }

        public string Name
        {
            get { return m_name; }
            set { SetProperty(ref m_name, value); }
        }

        public string PresenceSource
        {
            get { return m_presenceSource; }
            set
            { 
                SetProperty(ref m_presenceSource, value); 
                if(String.IsNullOrEmpty(PresenceSource))
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

        public Boolean IsVisible
        {
            get { return m_isVisible; }
            set 
            { 
                SetProperty(ref m_isVisible, value);
                if (m_isVisible)
                    UIIsVisible = Visibility.Visible;
                else
                    UIIsVisible = Visibility.Collapsed;
            }
        }

        public Visibility UIIsVisible
        {
            get { return m_UIIsVisible; }
            set { SetProperty(ref m_UIIsVisible, value); }
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

        public BitmapImage AvatarImageSource
        {
            get { return m_avatarSource; }
            set { SetProperty(ref m_avatarSource, value); }
        }

        public Int64 Position
        {
            get { return m_position; }
            set { SetProperty(ref m_position, value); }
        }
    }
}
