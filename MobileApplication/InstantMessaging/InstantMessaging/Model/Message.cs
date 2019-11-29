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

        String replyId;
        String replyPeerDisplayName;
        String replyPeerId;
        String replyPeerJid;
        String replyPartIsVisible;
        String replyBackgroundColor;
        String replyBody;

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

        public string PeerDisplayNameIsVisible // Used to avoid to display name in one-to-one conversation
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

#region PROPERTIES FOR REPLY DISPLAY

        public string ReplyId
        {
            get { return replyId; }
            set { SetProperty(ref replyId, value); }
        }

        public string ReplyPeerDisplayName
        {
            get { return replyPeerDisplayName; }
            set { SetProperty(ref replyPeerDisplayName, value); }
        }

        public string ReplyPeerId
        {
            get { return replyPeerId; }
            set { SetProperty(ref replyPeerId, value); }
        }
        public string ReplyPeerJid
        {
            get { return replyPeerJid; }
            set { SetProperty(ref replyPeerJid, value); }
        }
        public string ReplyPartIsVisible
        {
            get { return replyPartIsVisible; }
            set { SetProperty(ref replyPartIsVisible, value); }
        }

        public string ReplyBackgroundColor
        {
            get { return replyBackgroundColor; }
            set { SetProperty(ref replyBackgroundColor, value); }
        }



        public string ReplyBody
        {
            get { return replyBody; }
            set { SetProperty(ref replyBody, value); }
        }

#endregion PROPERTIES FOR REPLY DISPLAY

    }
}
