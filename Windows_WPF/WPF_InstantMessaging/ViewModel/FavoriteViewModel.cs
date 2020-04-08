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
        String m_name;
        String m_presenceSource;
        Int64 m_nbMsgUnread;
        BitmapImage m_avatarSource;
        Int64 m_position;

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

        public Int64 Position
        {
            get { return m_position; }
            set { SetProperty(ref m_position, value); }
        }

        private class SortAscendingHelper : IComparer
        {
            int IComparer.Compare(object a, object b)
            {
                FavoriteViewModel o1 = (FavoriteViewModel)a;
                FavoriteViewModel o2 = (FavoriteViewModel)b;

                if (o1.Position > o2.Position)
                    return 1;

                if (o1.Position < o2.Position)
                    return -1;

                else
                    return 0;
            }
        }

        public static IComparer SortAscending()
        {
            return (IComparer)new SortAscendingHelper();
        }

        public class FavoriteViewModelComparer : IComparer<FavoriteViewModel>
        {
            // Compares by Height, Length, and Width.
            public int Compare(FavoriteViewModel o1, FavoriteViewModel o2)
            {
                if (o1.Position > o2.Position)
                    return 1;

                if (o1.Position < o2.Position)
                    return -1;

                else
                    return 0;
            }
        }
    }
}
