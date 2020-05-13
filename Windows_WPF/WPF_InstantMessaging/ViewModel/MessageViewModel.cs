using System;
using System.Windows;
using System.Windows.Media.Imaging;
using InstantMessaging.Helpers;
using Brush = System.Windows.Media.Brush;

namespace InstantMessaging.ViewModel
{
    class MessageViewModel : ObservableObject
    {
        String id;
        String peerDisplayName;
        String peerJid;
        String peerId;

        Visibility bodyIsVisible;
        String body;
        FontStyle bodyFontStyle;
        Brush bodyColor;

        Brush backgroundColor;
        Visibility peerDisplayNameIsVisible;
        BitmapImage avatarImageSource;
        DateTime messageDateTime;
        String messageDateDisplay;

        Boolean isEventMessage;
        String eventName;
        String eventMessageBodyPart1;
        String eventMessageBodyPart2;
        Brush eventMessageBodyPart2Color;
        Visibility eventMessageBodyPart2IsVisible;

        String callOtherJid;
        String callOriginator;
        int callDuration;
        String callType;
        String callState;

        String replyId;
        String replyPeerId;
        String replyPeerJid;
        String replyPeerDisplayName;
        Visibility replyPartIsVisible;
        Brush replyBackgroundColor;
        String replyBody;

        Visibility editedIsVisible;

        String receiptType;
        BitmapImage receiptImageSource;

        Visibility fileAttachmentIsVisible;
        BitmapImage fileAttachmentImageSource;
        int fileAttachmentImageWidth;
        int fileAttachmentImageHeight;
        Visibility fileInfoIsVisible;
        String fileId;
        String fileName;
        String fileSize;

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

        public Visibility PeerDisplayNameIsVisible // Used to avoid to display name in one-to-one conversation
        {
            get { return peerDisplayNameIsVisible; }
            set { SetProperty(ref peerDisplayNameIsVisible, value); }
        }

        public Brush BackgroundColor
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

        public Visibility BodyIsVisible
        {
            get { return bodyIsVisible; }
            set { SetProperty(ref bodyIsVisible, value); }
        }

        public FontStyle BodyFontStyle
        {
            get { return bodyFontStyle; }
            set { SetProperty(ref bodyFontStyle, value); }
        }

        public Brush BodyColor
        {
            get { return bodyColor; }
            set { SetProperty(ref bodyColor, value); }
        }

        public BitmapImage AvatarImageSource
        {
            get { return avatarImageSource; }
            set { SetProperty(ref avatarImageSource, value); }
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

        public Boolean IsEventMessage
        {
            get { return isEventMessage; }
            set { SetProperty(ref isEventMessage, value); }
        }

        public String EventName
        {
            get { return eventName; }
            set { SetProperty(ref eventName, value); }
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

        public Brush EventMessageBodyPart2Color
        {
            get { return eventMessageBodyPart2Color; }
            set { SetProperty(ref eventMessageBodyPart2Color, value); }
        }

        public Visibility EventMessageBodyPart2IsVisible
        {
            get { return eventMessageBodyPart2IsVisible; }
            set { SetProperty(ref eventMessageBodyPart2IsVisible, value); }
        }

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
        public Visibility ReplyPartIsVisible
        {
            get { return replyPartIsVisible; }
            set { SetProperty(ref replyPartIsVisible, value); }
        }

        public Brush ReplyBackgroundColor
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

        public BitmapImage ReceiptImageSource
        {
            get { return receiptImageSource; }
            set { SetProperty(ref receiptImageSource, value); }
        }

#endregion PROPERTIES FOR RECEIPT DISPLAY

        public Visibility EditedIsVisible
        {
            get { return editedIsVisible; }
            set { SetProperty(ref editedIsVisible, value); }
        }

#region PROPERTIES FOR FILE ATTACHMENT

        public Visibility FileAttachmentIsVisible
        {
            get { return fileAttachmentIsVisible; }
            set { SetProperty(ref fileAttachmentIsVisible, value); }
        }

        public int FileAttachmentImageWidth
        {
            get { return fileAttachmentImageWidth; }
            set { SetProperty(ref fileAttachmentImageWidth, value); }
        }

        public int FileAttachmentImageHeight
        {
            get { return fileAttachmentImageHeight; }
            set { SetProperty(ref fileAttachmentImageHeight, value); }
        }

        public Visibility FileInfoIsVisible
        {
            get { return fileInfoIsVisible; }
            set { SetProperty(ref fileInfoIsVisible, value); }
        }

        public String FileId
        {
            get { return fileId; }
            set { SetProperty(ref fileId, value); }
        }

        public String FileName
        {
            get { return fileName; }
            set { SetProperty(ref fileName, value); }
        }

        public String FileSize
        {
            get { return fileSize; }
            set { SetProperty(ref fileSize, value); }
        }

        public BitmapImage FileAttachmentImageSource
        {
            get { return fileAttachmentImageSource; }
            set { SetProperty(ref fileAttachmentImageSource, value); }
        }

#endregion PROPERTIES FOR FILE ATTACHMENT

    }
}
