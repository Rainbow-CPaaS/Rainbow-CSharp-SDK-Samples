using System;

using Xamarin.Forms;

using System.IO;
using MultiPlatformApplication.Helpers;

namespace MultiPlatformApplication.Models
{
    public class MessageModel : ObservableObject
    {
        String id;
        String peerDisplayName;
        String peerJid;
        String peerId;

        Boolean isBubbleContext;

        String body;
        FontAttributes bodyFontAttributes;
        Color bodyColor;

        Color backgroundColor;
        
        String avatarFilePath;
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
        Color replyBackgroundColor;
        String replyBody;

        String replaceId;

        String receiptType;
        String receiptSource;

        ImageSource fileAttachmentImageSource;
        int fileAttachmentSourceWidth;
        int fileAttachmentSourceHeight;
        String fileDefaultInfoIsVisible;
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

        public Boolean IsBubbleContext 
        {
            get { return isBubbleContext; }
            set { SetProperty(ref isBubbleContext, value); }
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

        public String AvatarFilePath
        {
            get { return avatarFilePath; }
            set { SetProperty(ref avatarFilePath, value); }
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

        public String ReceiptSource 
        {
            get { return receiptSource; }
            set { SetProperty(ref receiptSource, value); }
        }

#endregion PROPERTIES FOR RECEIPT DISPLAY

        public String ReplaceId
        {
            get { return replaceId; }
            set { SetProperty(ref replaceId, value); }
        }

#region PROPERTIES FOR FILE ATTACHMENT

        public int FileAttachmentSourceWidth
        {
            get { return fileAttachmentSourceWidth; }
            set { SetProperty(ref fileAttachmentSourceWidth, value); }
        }

        public int FileAttachmentSourceHeight
        {
            get { return fileAttachmentSourceHeight; }
            set { SetProperty(ref fileAttachmentSourceHeight, value); }
        }

        public String FileDefaultInfoIsVisible
        {
            get { return fileDefaultInfoIsVisible; }
            set { SetProperty(ref fileDefaultInfoIsVisible, value); }
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

        //public String FileDefaultAttachmentSource => Helpers.Helper.GetFileAttachmentSourceFromFileName(FileName);

        public ImageSource FileAttachmentImageSource
        {
            get { return fileAttachmentImageSource; }
            set { SetProperty(ref fileAttachmentImageSource, value); }
        }


#endregion PROPERTIES FOR FILE ATTACHMENT

        public MessageModel()
        {
            Id = "";
            PeerDisplayName = "";
            IsBubbleContext = false;
            BackgroundColor = Color.FromHex("#FFFFFF");
            PeerId = "";
            PeerJid = "";
            Body = "";
            BodyFontAttributes = FontAttributes.None;
            BodyColor = Color.FromHex("#000000");
            MessageDateTime = DateTime.Now;
            MessageDateDisplay = "";
            IsEventMessage = "True";
            EventMessageBodyPart1 = "";
            EventMessageBodyPart2 = "";
            EventMessageBodyPart2Color = Color.FromHex("#000000");
            CallOtherJid = "";
            CallOriginator = "";
            CallDuration = 0;
            CallType = "";
            CallState = "";

            ReplyId = "";
            ReplyPeerDisplayName = "";
            ReplyPeerId = "";
            ReplyPeerJid = "";
            ReplyBackgroundColor = Color.FromHex("#000000");
            ReplyBody = "";

            ReceiptType = "";
            ReceiptSource = "";

            ReplaceId = "";

            FileAttachmentSourceWidth = 0;
            FileAttachmentSourceHeight = 0;
            FileDefaultInfoIsVisible = "False";
            FileId = "";
            FileName = "";
            FileSize = "";
            FileAttachmentImageSource = null;
        }

        ~MessageModel()
        {
            FileAttachmentImageSource = null;
        }
    }
}
