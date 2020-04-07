using System;
using System.Windows;
using System.Windows.Media.Imaging;

using InstantMessaging.Helpers;
using InstantMessaging.Pool;

namespace InstantMessaging.ViewModel
{
    public class FavoriteViewModel : ObservableObject
    {
        String m_id;
        String m_name;
        String m_presenceSource;
        Int64 m_nbMsgUnread;
        BitmapImage m_avatarSource;

        public string Id
        {
            get { return m_id; }
            set { SetProperty(ref m_id, value); }
        }

        public string Name
        {
            get { return m_name; }
            set { SetProperty(ref m_name, value); }
        }

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

        public BitmapImage AvatarImageSource
        {
            get { return m_avatarSource; }
            set { SetProperty(ref m_avatarSource, value); }
        }
    }
}
