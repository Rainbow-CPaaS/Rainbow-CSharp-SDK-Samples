using System;

using MvvmHelpers;

using Xamarin.Forms;

namespace InstantMessaging.Model
{
    public class Message : ObservableObject
    {
        String id;
        String peerDisplayName;
        String peerJid;
        String peerId;
        String body;
        String backgroundColor;
        String peerDisplayNameIsVisible;
        ImageSource avatarSource;
        DateTime messageDateTime;
        String  messageDateDisplay;

        public string Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        public string PeerDisplayName
        {
            get { return peerDisplayName; }
            set { SetProperty(ref peerDisplayName, value); }
        }

        public string PeerDisplayNameIsVisible
        {
            get { return peerDisplayNameIsVisible; }
            set { SetProperty(ref peerDisplayNameIsVisible, value); }
        }

        public string BackgroundColor
        {
            get { return backgroundColor; }
            set { SetProperty(ref backgroundColor, value); }
        }

        public string PeerId
        {
            get { return peerId; }
            set { SetProperty(ref peerId, value); }
        }

        public string PeerJid
        {
            get { return peerJid; }
            set { SetProperty(ref peerJid, value); }
        }

        public string Body
        {
            get { return body; }
            set { SetProperty(ref body, value); }
        }

        public ImageSource AvatarSource
        {
            get { return avatarSource; }
            set { SetProperty(ref avatarSource, value); }
        }

        public DateTime MessageDateTime
        {
            get { return messageDateTime; }
            set { SetProperty(ref messageDateTime, value); }
        }

        public String MessageDateDisplay
        {
            get { return messageDateDisplay; }
            set { SetProperty(ref messageDateDisplay, value); }
        }
    }
}
