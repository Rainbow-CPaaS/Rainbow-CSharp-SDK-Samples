using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

using InstantMessaging.Pool;
using InstantMessaging.ViewModel;

using Rainbow;
using Rainbow.Model;

using log4net;


namespace InstantMessaging.Helpers
{
    public static class Helper
    {
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(Helper));

        public static String GetTempFolder()
        {
            return System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
        }

        public static BitmapFrame GetBitmapFrameFromResource(String resourceName)
        {
            BitmapFrame bitmapFrame = null;
            Stream stream = GetMemoryStreamFromResource(resourceName);
            if (stream != null)
            {
                bitmapFrame = BitmapFrame.Create(stream);
                //stream.Dispose();
            }
            return bitmapFrame;
        }

        public static BitmapImage GetBitmapImageFromStream(Stream stream)
        {
            BitmapImage bitmapImage = null;
            if (stream != null)
            {
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream();
                stream.CopyTo(bitmapImage.StreamSource);
                bitmapImage.StreamSource.Position = 0;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        public static BitmapImage GetBitmapImageFromResource(String resourceName)
        {

            BitmapImage bitmapImage = null;
            Stream stream = GetMemoryStreamFromResource(resourceName);
            if(stream != null)
            {
                bitmapImage = GetBitmapImageFromStream(stream);
                stream.Dispose();
                //bitmapImage = new BitmapImage();
                //bitmapImage.BeginInit();
                //stream.CopyTo(bitmapImage.StreamSource);
                //bitmapImage.StreamSource.Position = 0;
                //bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        public static Stream GetMemoryStreamFromResource(String resourceName)
        {
            Stream ms = null;
            StreamResourceInfo streamResourceInfo = System.Windows.Application.GetResourceStream(new Uri($"/Resources/{resourceName}", UriKind.RelativeOrAbsolute));
            
            if( (streamResourceInfo != null)
                && (streamResourceInfo.Stream != null) )
            {
                // Go to the beginning of the stream
                streamResourceInfo.Stream.Position = 0;

                // Create Memory Stream
                ms = new MemoryStream();

                // Copy stream
                streamResourceInfo.Stream.CopyTo(ms);

                // Dispose older Stream
                streamResourceInfo.Stream.Dispose();

                // Go to the beginning of the new Memory Stream
                ms.Position = 0;
                //ms.Seek(0, SeekOrigin.Begin);
            }

            return ms;
        }

        /// <summary>
        /// Get Humanized string of the specifed DateTime. The DateTime must not be gerated than UtcNow ...
        /// 
        /// This method is linked to GetRefreshDelay(DateTime)
        /// </summary>
        /// <param name="dt"><see cref="DateTime"/>DateTime object to humanize</param>
        /// <returns><see cref="String"/>Humanized String</returns>
        public static String HumanizeDateTime(DateTime dt)
        {
            String result;

            DateTime nowInUTC = DateTime.UtcNow;
            DateTime dtInUTC = dt.ToUniversalTime();
            DateTime dtLocalTime = dt.ToLocalTime();

            if (nowInUTC.Date  == dt.Date)
                result = dtLocalTime.ToString("H:mm");
            else if (nowInUTC.Year == dtInUTC.Year)
                result = dtLocalTime.ToString("dd MMM");
            else
                result = dtLocalTime.ToString("dd MMM yy");

            return result;
        }

        /// <summary>
        /// Return the value formatted to include at most three
        /// non-zero digits and at most two digits after the
        /// decimal point. Examples:
        ///         1
        ///       123
        ///        12.3
        ///         1.23
        ///         0.12
        /// </summary>
        /// <param name="value"><see cref="double"/>Double value to format</param>
        /// <returns></returns>
        private static string ThreeNonZeroDigits(double value)
        {
            if (value >= 100)
            {
                // No digits after the decimal.
                return value.ToString("0,0");
            }
            else if (value >= 10)
            {
                // One digit after the decimal.
                return value.ToString("0.0");
            }
            else
            {
                // Two digits after the decimal.
                return value.ToString("0.00");
            }
        }

        /// <summary>
        /// Return a string describing the value as a file size. For example, 1.23 MB. 
        /// </summary>
        /// <param name="fileSizeInOctet"><see cref="double"/>Double value to format</param>
        /// <returns></returns>
        public static string HumanizeFileSize(this double fileSizeInOctet)
        {
            string[] suffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};
            for (int i = 0; i < suffixes.Length; i++)
            {
                if (fileSizeInOctet <= (Math.Pow(1024, i + 1)))
                {
                    return ThreeNonZeroDigits(fileSizeInOctet /
                        Math.Pow(1024, i)) +
                        " " + suffixes[i];
                }
            }

            return ThreeNonZeroDigits(fileSizeInOctet /
                Math.Pow(1024, suffixes.Length - 1)) +
                " " + suffixes[suffixes.Length - 1];
        }

        /// <summary>
        /// Get the delay (in seconds) to use in order to refresh delay according the specified DateTime
        /// 
        /// This method is linked to Humanize(DateTime)
        /// </summary>
        /// <param name="dt"><see cref="DateTime"/>DateTime object to humanize</param>
        /// <returns><see cref="int"/>Delay in seconds</returns>
        public static int GetRefreshDelay(DateTime dt)
        {
            int result = 120; // Update every 2 mns (2 * 60)
            return result;
        }

        public static String GetPresenceSourceFromPresence(Rainbow.Model.Presence presence)
        {
            String presenceSource = "";
            if (presence != null)
            {
                if (presence.PresenceLevel == Rainbow.Model.PresenceLevel.Online)
                {
                    if(presence.Resource.StartsWith("mobile"))
                        presenceSource = "presence_online_mobile.png";
                    else
                        presenceSource = "presence_online.png";
                }
                else if (presence.PresenceLevel == Rainbow.Model.PresenceLevel.Offline)
                    presenceSource = "presence_offline.png";
                else if (presence.PresenceLevel == Rainbow.Model.PresenceLevel.Away)
                    presenceSource = "presence_away.png";
                else if (presence.PresenceLevel == Rainbow.Model.PresenceLevel.Xa)
                    presenceSource = "presence_xa.png";
                else if (presence.PresenceLevel == Rainbow.Model.PresenceLevel.Dnd)
                    presenceSource = "presence_dnd.png";
                else if (presence.PresenceLevel == Rainbow.Model.PresenceLevel.Busy)
                    presenceSource = "presence_busy.png";
            }
            return presenceSource;
        }

        public static ConversationLightViewModel GetConversationFromRBConversation(Rainbow.Model.Conversation rbConversation)
        {
            ConversationLightViewModel conversation = null;

            if (rbConversation != null)
            {
                InstantMessaging.App CurrentApplication = (InstantMessaging.App)System.Windows.Application.Current;
                AvatarPool avatarPool = AvatarPool.Instance;

                conversation = new ConversationLightViewModel();

                if (rbConversation.Type == Rainbow.Model.Conversation.ConversationType.Room)
                {
                    conversation.Name = rbConversation.Name;
                    conversation.Topic = rbConversation.Topic;
                    conversation.Jid = rbConversation.Jid_im;
                    conversation.PresenceSource = "";

                }
                else if (rbConversation.Type == Rainbow.Model.Conversation.ConversationType.User)
                {
                    // Get Display name of this user
                    Rainbow.Model.Contact contact = CurrentApplication.RbContacts.GetContactFromContactId(rbConversation.PeerId);
                    if (contact != null)
                    {
                        conversation.Name = Util.GetContactDisplayName(contact, avatarPool.GetFirstNameFirst());
                        conversation.Topic = "";
                        conversation.Jid = contact.Jid_im;

                        Presence presence = CurrentApplication.RbContacts.GetAggregatedPresenceFromContactId(rbConversation.PeerId);
                        conversation.PresenceSource = InstantMessaging.Helpers.Helper.GetPresenceSourceFromPresence(presence);

                    }
                    else
                    {
                        //log.DebugFormat("[GetConversationFromRBConversation] - unknown contact - contactId:[{0}]", rbConversation.PeerId);
                        CurrentApplication.RbContacts.GetContactFromContactIdFromServer(rbConversation.PeerId, null);
                    }
                }
                else
                {
                    //TODO ( bot case)
                    //log.DebugFormat("[GetConversationFromRBConversation] Conversation from model not created - Id:[{0}]", rbConversation.Id);
                    return null;
                }

                conversation.Id = rbConversation.Id;
                conversation.PeerId = rbConversation.PeerId;

                conversation.Type = rbConversation.Type;
                conversation.NbMsgUnread = rbConversation.UnreadMessageNumber;

                conversation.LastMessage = rbConversation.LastMessageText;
                conversation.LastMessageDateTime = rbConversation.LastMessageDate;

                // Humanized the DateTime
                conversation.LastMessageTimeDisplay = Helpers.Helper.HumanizeDateTime(conversation.LastMessageDateTime);
            }
            return conversation;
        }

        public static String GetBubbleEventMessageBody(Contact contact, String bubbleEvent)
        {
            String result = null;

            if(contact != null)
            {
                String displayName = Util.GetContactDisplayName(contact, AvatarPool.Instance.GetFirstNameFirst());
                switch (bubbleEvent)
                {
                    case "conferenceEnded":
                        result = displayName + " has ended the conference";
                        break;
                    case "conferenceStarted":
                        result = displayName + " has started the conference";
                        break;
                    case "userJoin":
                        result = displayName + " has joined the conference";
                        break;
                    case "userLeave":
                        result = displayName + " has left the conference";
                        break;
                    default:
                        result = displayName + " - " + bubbleEvent;
                        break;
                }
            }

            return result;
        }

#region AVATAR - IMAGES MANAGEMENT

        public static BitmapImage BitmapImageFromFile(String filePath)
        {
            BitmapImage result = null;
            try
            {
                result = new BitmapImage();
                result.BeginInit();
                result.UriSource = new Uri(filePath, UriKind.RelativeOrAbsolute);
                result.EndInit();
                return result;
            }
            catch (Exception exc)
            {
                log.WarnFormat("[FromFile] Exception to create Image from file - filePath:[{0}] - Exception:[{1}]", filePath, Util.SerializeException(exc));
            }
            return result;
        }

        public static BitmapImage GetBubbleAvatarImageSource(String bubbleId)
        {
            BitmapImage result = null;
            if (!String.IsNullOrEmpty(bubbleId))
            {
                String filePath = null;
                try
                {
                    filePath = AvatarPool.Instance.GetBubbleAvatarPath(bubbleId);
                }
                catch (Exception exc)
                {
                    log.WarnFormat("[GetBubbleAvatarImageSource] bubbleId:[{0}] - exception occurs to create avatar:[{1}]", bubbleId, Util.SerializeException(exc));
                }

                if (!String.IsNullOrEmpty(filePath))
                {
                    if (File.Exists(filePath))
                    {
                        result = BitmapImageFromFile(filePath);
                    }
                    //else
                    //    log.WarnFormat("[SetConversationAvatar] - file not found - filePath:[{0}] - PeerId:[{1}]", filePath, conversation.PeerId);
                }
            }
            return result;
        }

        public static BitmapImage GetContactAvatarImageSource(String contactId)
        {
            BitmapImage result = null;
            if (!String.IsNullOrEmpty(contactId))
            {
                String filePath = null;
                try
                {
                    filePath = AvatarPool.Instance.GetContactAvatarPath(contactId);
                }
                catch (Exception exc)
                {
                    log.WarnFormat("[GetContactAvatarImageSource] contactId:[{0}] - exception occurs to create avatar:[{1}]", contactId, Util.SerializeException(exc));
                }

                if (!String.IsNullOrEmpty(filePath))
                {
                    if (File.Exists(filePath))
                    {
                        //log.DebugFormat("[SetConversationAvatar] - file used - filePath:[{0}] - PeerId:[{1}]", filePath, conversation.PeerId);
                        result = BitmapImageFromFile(filePath);
                    }
                    //else
                    //    log.WarnFormat("[SetConversationAvatar] - file not found - filePath:[{0}] - PeerId:[{1}]", filePath, conversation.PeerId);
                }
            }
            return result;
        }

        public static BitmapImage GetConversationAvatarImageSource(ConversationLightViewModel conversation)
        {
            BitmapImage result = null;
            if (conversation != null)
            {
                String filePath = null;
                try
                {
                    if (conversation.Type == Rainbow.Model.Conversation.ConversationType.User)
                        filePath = AvatarPool.Instance.GetContactAvatarPath(conversation.PeerId);
                    else if (conversation.Type == Rainbow.Model.Conversation.ConversationType.Room)
                        filePath = AvatarPool.Instance.GetBubbleAvatarPath(conversation.PeerId);
                }
                catch (Exception exc)
                {
                    log.WarnFormat("[GetConversationAvatarImageSource] PeerId:[{0}] - exception occurs to create avatar:[{1}]", conversation.PeerId, Util.SerializeException(exc));
                }

                if (!String.IsNullOrEmpty(filePath))
                {
                    if (File.Exists(filePath))
                    {
                        //log.DebugFormat("[SetConversationAvatar] - file used - filePath:[{0}] - PeerId:[{1}]", filePath, conversation.PeerId);
                        result = BitmapImageFromFile(filePath);
                    }
                    //else
                    //    log.WarnFormat("[SetConversationAvatar] - file not found - filePath:[{0}] - PeerId:[{1}]", filePath, conversation.PeerId);
                }
            }
            return result;
        }
#endregion AVATAR - IMAGES MANAGEMENT

    }
}
