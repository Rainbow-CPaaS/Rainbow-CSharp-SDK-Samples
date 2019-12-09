using System;

using Xamarin.Forms;

using Rainbow.Helpers;

namespace InstantMessaging.Model
{
    public class Message : ObservableObject
    {
        String id;
        String peerDisplayName;
        String peerJid;
        String peerId;
        String body;
        FontAttributes bodyFontAttributes;
        Color bodyColor;
        Color backgroundColor;
        String peerDisplayNameIsVisible;
        ImageSource avatarSource;
        DateTime messageDateTime;
        String  messageDateDisplay;

        String isEventMessage;
        String eventMessageBodyPart1;
        String eventMessageBodyPart2;
        Color eventMessageBodyPart2Color;

        String callOtherJid;
        String callOriginator;
        int callDuration;
        String callType;
        String callState;

        String replyId;
        String replyPeerDisplayName;
        String replyPeerId;
        String replyPeerJid;
        String replyPartIsVisible;
        Color replyBackgroundColor;
        String replyBody;

        String editedIsVisible;

        String receiptIsVisible;
        String receiptType;
        ImageSource receiptSource;

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

        public Color BackgroundColor
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

        public FontAttributes BodyFontAttributes
        {
            get { return bodyFontAttributes; }
            set { SetProperty(ref bodyFontAttributes, value); }
        }

        public Color BodyColor
        {
            get { return bodyColor; }
            set { SetProperty(ref bodyColor, value); }
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

#region PROPERTIES FOR EVENT

        public String IsEventMessage
        {
            get { return isEventMessage; }
            set { SetProperty(ref isEventMessage, value); }
        }

        public String EventMessageBodyPart1
        {
            get { return eventMessageBodyPart1; }
            set { SetProperty(ref eventMessageBodyPart1, value); }
        }

        public String EventMessageBodyPart2
        {
            get { return eventMessageBodyPart2; }
            set { SetProperty(ref eventMessageBodyPart2, value); }
        }

        public Color EventMessageBodyPart2Color
        {
            get { return eventMessageBodyPart2Color; }
            set { SetProperty(ref eventMessageBodyPart2Color, value); }
        }

        public String EventMessageBodyPart2IsVisible => String.IsNullOrEmpty(EventMessageBodyPart2) ? "False": "True";

#endregion PROPERTIES FOR EVENT

#region PROPERTIES FOR CALL LOG

        public String CallOtherJid
        {
            get { return callOtherJid; }
            set { SetProperty(ref callOtherJid, value); }
        }
        public String CallOriginator
        {
            get { return callOriginator; }
            set { SetProperty(ref callOriginator, value); }
        }
        public int CallDuration
        {
            get { return callDuration; }
            set { SetProperty(ref callDuration, value); }
        }

        public String CallType
        {
            get { return callType; }
            set { SetProperty(ref callType, value); }
        }

        public String CallState
        {
            get { return callState; }
            set { SetProperty(ref callState, value); }
        }

#endregion PROPERTIES FOR CALL LOG

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

        public Color ReplyBackgroundColor
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

#region PROPERTIES FOR RECEIPT DISPLAY

        public String ReceiptType
        {
            get { return receiptType; }
            set { SetProperty(ref receiptType, value); }
        }

        public ImageSource ReceiptSource
        {
            get { return receiptSource; }
            set { SetProperty(ref receiptSource, value); }
        }

#endregion PROPERTIES FOR RECEIPT DISPLAY

        
        public String EditedIsVisible
        {
            get { return editedIsVisible; }
            set { SetProperty(ref editedIsVisible, value); }
        }

    }
}
