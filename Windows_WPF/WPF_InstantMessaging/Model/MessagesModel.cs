using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

using InstantMessaging.Helpers;
using InstantMessaging.Pool;
using InstantMessaging.ViewModel;


using Rainbow;

using log4net;


namespace InstantMessaging.Model
{
    class MessagesModel
    {
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(LoginModel));
        App CurrentApplication = (App)System.Windows.Application.Current;

        private Bubbles RbBubbles = null;
        private Contacts RbContacts = null;
        private Conversations RbConversations = null;

        private readonly AvatarPool AvatarPool; // To manage avatars

        public RangeObservableCollection<MessageViewModel> MessagesList { get; set; } // Need to be public - Used as Binding from XAML

        public MessagesModel()
        {
            AvatarPool = InstantMessaging.Pool.AvatarPool.Instance;

            MessagesList = new RangeObservableCollection<MessageViewModel>();

            if (CurrentApplication.USE_DUMMY_DATA)
            {
                LoadFakeMessages();
                return;
            }
        }

        public void LoadFakeMessages()
        {
            double nbSecs;
            int mns;
            int sec;

            DateTime utcNow = DateTime.UtcNow;
            MessageViewModel message;

            #region FAKE CONVERSATIONS - TEST 'OTHER USER' DISPLAY

            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , short body
            message = new MessageViewModel();
            message.Id = "1";

            message.PeerId = "peerId";
            message.PeerJid = "peerJid";
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "message body";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.White;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Collapsed;

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = false;

            MessagesList.Add(message);


            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , short body, edited
            message = new MessageViewModel();
            message.Id = "2";

            message.PeerId = "peerId";
            message.PeerJid = "peerJid";
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "message body";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.White;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Visible; // EDITED

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = false;

            MessagesList.Add(message);


            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , short body, reply part
            message = new MessageViewModel();
            message.Id = "3";

            message.PeerId = "peerId";
            message.PeerJid = "peerJid";
            message.PeerDisplayName = "Frederic Reard";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "message body";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.White;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Visible; // EDITED

            message.ReplyPartIsVisible = Visibility.Visible;
            message.ReplyId = "1";
            message.ReplyPeerDisplayName = "Christophe Irles";
            message.ReplyBackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetDarkerColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;
            message.ReplyPeerId = CurrentApplication.CurrentUserId;
            message.ReplyPeerJid = CurrentApplication.CurrentUserJid;
            message.ReplyBody = "reply message body";

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = false;

            MessagesList.Add(message);


            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , short body, File Attachment + File Info
            message = new MessageViewModel();
            message.Id = "4";

            message.PeerId = "peerId";
            message.PeerJid = "peerJid";
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "message body";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.White;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Visible; // EDITED

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Visible;
            message.FileAttachmentImageSource = Helper.GetBitmapImageFromResource("icon_unknown_blue.png");
            message.FileAttachmentImageHeight = 60;
            message.FileAttachmentImageWidth = 60;

            message.FileInfoIsVisible = Visibility.Visible;
            message.FileId = "1";
            message.FileName = "My word document.ppt";
            message.FileSize = Helper.HumanizeFileSize(10248);

            message.IsEventMessage = false;

            MessagesList.Add(message);

            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , short body, File Attachment without File Info
            message = new MessageViewModel();
            message.Id = "5";

            message.PeerId = "peerId";
            message.PeerJid = "peerJid";
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "message body";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.White;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Visible; // EDITED

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Visible;
            message.FileAttachmentImageSource = Helper.GetBitmapImageFromResource("icon_unknown_blue.png");
            message.FileAttachmentImageHeight = 80;
            message.FileAttachmentImageWidth = 80;

            message.FileInfoIsVisible = Visibility.Collapsed;
            message.FileId = "1";

            message.IsEventMessage = false;

            MessagesList.Add(message);


            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , simulate delete message
            message = new MessageViewModel();
            message.Id = "1";

            message.PeerId = "peerId";
            message.PeerJid = "peerJid";
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "This message was deleted";
            message.BodyFontStyle = FontStyles.Italic; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = new BrushConverter().ConvertFromString(AvatarPool.GetDarkerColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Collapsed;

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = false;

            MessagesList.Add(message);

            #endregion FAKE CONVERSATIONS - TEST 'OTHER USER' DISPLAY

            #region FAKE CONVERSATIONS - TEST 'CURRENT USER' DISPLAY

            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , short body
            message = new MessageViewModel();
            message.Id = "1";

            message.PeerId = CurrentApplication.CurrentUserId;
            message.PeerJid = CurrentApplication.CurrentUserJid;
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = Brushes.LightGray;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "message body";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.Black;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Collapsed;

            message.ReceiptImageSource = Helper.GetBitmapImageFromResource("msg_sending.png");

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = false;

            MessagesList.Add(message);


            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , short body, edited
            message = new MessageViewModel();
            message.Id = "2";

            message.PeerId = CurrentApplication.CurrentUserId;
            message.PeerJid = CurrentApplication.CurrentUserJid;
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = Brushes.LightGray;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "message body";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.Black;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Visible; // EDITED

            message.ReceiptImageSource = Helper.GetBitmapImageFromResource("msg_not_read.png");

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = false;

            MessagesList.Add(message);


            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , short body, reply part
            message = new MessageViewModel();
            message.Id = "3";

            message.PeerId = CurrentApplication.CurrentUserId;
            message.PeerJid = CurrentApplication.CurrentUserJid;
            message.PeerDisplayName = "Frederic Reard";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = Brushes.LightGray;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "message body";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.Black;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Visible; // EDITED

            message.ReceiptImageSource = Helper.GetBitmapImageFromResource("msg_read.png");

            message.ReplyPartIsVisible = Visibility.Visible;
            message.ReplyId = "1";
            message.ReplyPeerDisplayName = "Christophe Irles";
            message.ReplyBackgroundColor = Brushes.Gray;
            message.ReplyPeerId = CurrentApplication.CurrentUserId;
            message.ReplyPeerJid = CurrentApplication.CurrentUserJid;
            message.ReplyBody = "reply message body";

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = false;

            MessagesList.Add(message);


            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , short body, File Attachment + File Info
            message = new MessageViewModel();
            message.Id = "4";

            message.PeerId = CurrentApplication.CurrentUserId;
            message.PeerJid = CurrentApplication.CurrentUserJid;
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = Brushes.LightGray;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "message body";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.Black;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Visible; // EDITED

            message.ReceiptImageSource = Helper.GetBitmapImageFromResource("msg_check.png");

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Visible;
            message.FileAttachmentImageSource = Helper.GetBitmapImageFromResource("icon_unknown_blue.png");
            message.FileAttachmentImageHeight = 60;
            message.FileAttachmentImageWidth = 60;

            message.FileInfoIsVisible = Visibility.Visible;
            message.FileId = "1";
            message.FileName = "My word document.ppt";
            message.FileSize = Helper.HumanizeFileSize(10248);

            message.IsEventMessage = false;

            MessagesList.Add(message);

            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , short body, File Attachment without File Info
            message = new MessageViewModel();
            message.Id = "5";

            message.PeerId = CurrentApplication.CurrentUserId;
            message.PeerJid = CurrentApplication.CurrentUserJid;
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = Brushes.LightGray;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "message body";
            message.BodyFontStyle = FontStyles.Normal; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.Black;  // BLACK FOR CURRENT USER

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Visible; // EDITED

            message.ReceiptImageSource = Helper.GetBitmapImageFromResource("msg_sending.png");

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Visible;
            message.FileAttachmentImageSource = Helper.GetBitmapImageFromResource("icon_unknown_blue.png");
            message.FileAttachmentImageHeight = 80;
            message.FileAttachmentImageWidth = 80;

            message.FileInfoIsVisible = Visibility.Collapsed;
            message.FileId = "1";

            message.IsEventMessage = false;

            MessagesList.Add(message);


            // --------------------------------------------------------------------
            // TEST BASIC MSG: short display name , simulate delete message
            message = new MessageViewModel();
            message.Id = "1";

            message.PeerId = CurrentApplication.CurrentUserId;
            message.PeerJid = CurrentApplication.CurrentUserJid;
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = Brushes.LightGray;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Visible;
            message.Body = "This message was deleted";
            message.BodyFontStyle = FontStyles.Italic; // ITALIC FOR DELETED MESSAGE
            message.BodyColor = Brushes.Gray;

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Collapsed;

            message.ReceiptImageSource = Helper.GetBitmapImageFromResource("msg_read.png");

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = false;

            MessagesList.Add(message);

            #endregion FAKE CONVERSATIONS - TEST 'CURRENT USER' DISPLAY

            #region FAKE CONVERSATIONS - TEST 'EVENT' DISPLAY

            // --------------------------------------------------------------------
            // TEST BASIC MSG: event - start conference
            message = new MessageViewModel();
            message.Id = "1";

            message.PeerId = "peerId";
            message.PeerJid = "peerJid";
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Collapsed;

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Collapsed;

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = true;
            message.EventMessageBodyPart1 = message.PeerDisplayName + " has started the conference";
            message.EventMessageBodyPart2IsVisible = String.IsNullOrEmpty(message.EventMessageBodyPart2) ? Visibility.Collapsed : Visibility.Visible;

            MessagesList.Add(message);

            // --------------------------------------------------------------------
            // TEST BASIC MSG: event - end conference
            message = new MessageViewModel();
            message.Id = "1";

            message.PeerId = "peerId";
            message.PeerJid = "peerJid";
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Collapsed;

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Collapsed;

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = true;
            message.EventMessageBodyPart1 = message.PeerDisplayName + " has ended the conference";
            message.EventMessageBodyPart2IsVisible = String.IsNullOrEmpty(message.EventMessageBodyPart2) ? Visibility.Collapsed : Visibility.Visible;

            MessagesList.Add(message);


            // --------------------------------------------------------------------
            // TEST BASIC MSG: event -'call log' - answered
            message = new MessageViewModel();
            message.Id = "1";

            message.PeerId = "peerId";
            message.PeerJid = "peerJid";
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Collapsed;

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Collapsed;

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = true;
            message.EventMessageBodyPart1 = message.PeerDisplayName + " has called you.";
            nbSecs = 658;
            mns = (int)(nbSecs / 60);
            sec = (int)Math.Round(nbSecs - (mns * 60));
            message.EventMessageBodyPart2 = "Duration: " + ((mns > 0) ? mns + ((mns > 1) ? "mns " : "mn ") : "") + ((sec > 0) ? sec + "s" : "");
            message.EventMessageBodyPart2Color = Brushes.Black;
            message.EventMessageBodyPart2IsVisible = String.IsNullOrEmpty(message.EventMessageBodyPart2) ? Visibility.Collapsed : Visibility.Visible;

            MessagesList.Add(message);

            // --------------------------------------------------------------------
            // TEST BASIC MSG: event -'call log' - not answered / failed
            message = new MessageViewModel();
            message.Id = "1";

            message.PeerId = "peerId";
            message.PeerJid = "peerJid";
            message.PeerDisplayName = "Philippe Torrelli";
            message.PeerDisplayNameIsVisible = Visibility.Visible;

            message.BackgroundColor = new BrushConverter().ConvertFromString(AvatarPool.GetColorFromDisplayName(message.PeerDisplayName)) as SolidColorBrush;

            message.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            message.BodyIsVisible = Visibility.Collapsed;

            message.MessageDateTime = utcNow;
            message.MessageDateDisplay = Helpers.Helper.HumanizeDateTime(message.MessageDateTime);

            message.EditedIsVisible = Visibility.Collapsed;

            message.ReplyPartIsVisible = Visibility.Collapsed;

            message.FileAttachmentIsVisible = Visibility.Collapsed;
            message.FileInfoIsVisible = Visibility.Collapsed;

            message.IsEventMessage = true;
            message.EventMessageBodyPart1 = message.PeerDisplayName + " has called you.";
            message.EventMessageBodyPart2 = "Missed call";
            message.EventMessageBodyPart2Color = Brushes.Red;
            message.EventMessageBodyPart2IsVisible = String.IsNullOrEmpty(message.EventMessageBodyPart2) ? Visibility.Collapsed : Visibility.Visible;

            MessagesList.Add(message);

            #endregion FAKE CONVERSATIONS - TEST 'EVENT' DISPLAY
        }
    }
}


