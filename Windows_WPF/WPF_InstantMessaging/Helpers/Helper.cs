using System;
using System.IO;
using System.Globalization;
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
            }

            return bitmapImage;
        }

        public static Stream GetStreamFromFile(String path)
        {
            Stream result = null;
            if (File.Exists(path))
            {
                using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    result = new MemoryStream();
                    fs.CopyTo(result);
                    result.Position = 0;
                    fs.Close();
                }
            }
            return result;
        }

        public static BitmapImage BitmapImageFromFile(String filePath)
        {
            return GetBitmapImageFromStream(GetStreamFromFile(filePath));
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
                return value.ToString("0,0", CultureInfo.InvariantCulture) ;
            }
            else if (value >= 10)
            {
                // One digit after the decimal.
                return value.ToString("0.0", CultureInfo.InvariantCulture);
            }
            else
            {
                // Two digits after the decimal.
                return value.ToString("0.00", CultureInfo.InvariantCulture);
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

        public static String GetPresenceSourceFromPresence(Rainbow.Model.Presence presence, Boolean isCurrentUser)
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
                else if (presence.PresenceLevel == Rainbow.Model.PresenceLevel.Dnd)
                    presenceSource = "presence_dnd.png";
                else if (presence.PresenceLevel == Rainbow.Model.PresenceLevel.Busy)
                    presenceSource = "presence_busy.png";
                else if ( (presence.PresenceLevel == Rainbow.Model.PresenceLevel.Xa) // Use XA display only for current user
                    && isCurrentUser)
                    presenceSource = "presence_xa.png";
            }
            return presenceSource;
        }

        public static FavoriteViewModel GetFavoriteFromRbFavorite(Rainbow.Model.Favorite rbFavorite)
        {
            FavoriteViewModel result = null;
            if(rbFavorite != null)
            {
                InstantMessaging.App CurrentApplication = (InstantMessaging.App)System.Windows.Application.Current;
                AvatarPool avatarPool = AvatarPool.Instance;

                result = new FavoriteViewModel();

                Conversation rbConversation = CurrentApplication.RbConversations.GetConversationByPeerIdFromCache(rbFavorite.PeerId);

                if (rbConversation != null)
                {
                    result.IsVisible = true;
                    result.Jid = rbConversation.Jid_im;
                    result.NbMsgUnread = rbConversation.UnreadMessageNumber;

                    if (rbConversation.Type == Rainbow.Model.Conversation.ConversationType.Room)
                    {
                        result.Name = rbConversation.Name;
                        result.PresenceSource = "";
                    }
                    else if (rbConversation.Type == Rainbow.Model.Conversation.ConversationType.User)
                    {
                        // Get Display name of this user
                        Rainbow.Model.Contact contact = CurrentApplication.RbContacts.GetContactFromContactId(rbConversation.PeerId);
                        if (contact != null)
                        {
                            Presence presence = CurrentApplication.RbContacts.GetAggregatedPresenceFromContactId(rbConversation.PeerId);

                            result.Name = Util.GetContactDisplayName(contact, avatarPool.GetFirstNameFirst());
                            result.PresenceSource = InstantMessaging.Helpers.Helper.GetPresenceSourceFromPresence(presence, rbConversation.PeerId == CurrentApplication.CurrentUserId);
                        }
                        else
                        {
                            // We ask to have more info about this contact using AvatarPool
                            log.DebugFormat("[GetFavoriteFromRbFavorite] - unknown contact - contactId:[{0}]", rbConversation.PeerId);
                            avatarPool.AddUnknownContactToPoolById(rbConversation.PeerId);

                            // Try to get info from pool
                            AvatarsData.LightContact lightContact = avatarPool.GetLightContact(rbConversation.PeerId, rbConversation.Jid_im);

                            if (lightContact != null)
                            {
                                result.Name = lightContact.DisplayName;
                                result.PresenceSource = "presence_offline.png";
                            }
                        }
                    }
                    else
                    {
                        //TODO (bot case)
                        log.DebugFormat("[GetFavoriteFromRbFavorite] Conversation from model not created - Id:[{0}]", rbConversation.Id);
                        return null;
                    }
                }
                else
                {
                    Bubble bubble = CurrentApplication.RbBubbles.GetBubbleByIdFromCache(rbFavorite.PeerId);
                    if(bubble != null)
                    {
                        result.IsVisible = true;
                        result.Name = bubble.Name;
                        result.Jid = bubble.Jid;
                        result.NbMsgUnread = 0;
                        result.PresenceSource = "";
                    }
                    else
                    {
                        result.IsVisible = false;
                        result.Name = "";
                        result.Jid = "";
                        result.NbMsgUnread = 0;
                        result.PresenceSource = "";

                        log.WarnFormat("[GetFavoriteFromRbFavorite] Cannot get Conversation or Bubble object from Favorite - FavoriteId:[{0}] - FavoritePeerId:[{1}]", rbFavorite.Id, rbFavorite.PeerId);
                    }

                    //TODO - need to get conversation ?
                }

                result.Id = rbFavorite.Id;
                result.PeerId = rbFavorite.PeerId;
                result.Position = rbFavorite.Position;

                // Name, PresenceSource,  NbMsgUnread and IsVisible are set before
            }
            return result;
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
                        conversation.PresenceSource = InstantMessaging.Helpers.Helper.GetPresenceSourceFromPresence(presence, rbConversation.PeerId == CurrentApplication.CurrentUserId);
                    }
                    else
                    {
                        // We ask to have more info about this contact using AvatarPool
                        log.DebugFormat("[GetConversationFromRBConversation] - unknown contact - contactId:[{0}]", rbConversation.PeerId);
                        avatarPool.AddUnknownContactToPoolById(rbConversation.PeerId);

                        // Try to get info from pool
                        AvatarsData.LightContact lightContact = avatarPool.GetLightContact(rbConversation.PeerId, rbConversation.Jid_im);

                        if (lightContact != null)
                        {
                            conversation.Name = lightContact.DisplayName;
                            conversation.Topic = "";
                            conversation.PeerId = lightContact.Id;
                            conversation.Jid = lightContact.Jid;
                            conversation.PresenceSource = "presence_offline.png";
                        }
                    }
                }
                else
                {
                    //TODO ( bot case)
                    log.DebugFormat("[GetConversationFromRBConversation] Conversation from model not created - Id:[{0}]", rbConversation.Id);
                    return null;
                }

                conversation.Id = rbConversation.Id;
                conversation.PeerId = rbConversation.PeerId;

                conversation.Type = rbConversation.Type;
                conversation.NbMsgUnread = rbConversation.UnreadMessageNumber;

                conversation.LastMessage = rbConversation.LastMessageText;
                conversation.LastMessageDateTime = rbConversation.LastMessageDate;
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
                        //log.DebugFormat("[GetContactAvatarImageSource] - file used - filePath:[{0}] - ContactId:[{1}]", filePath, contactId);
                        result = BitmapImageFromFile(filePath);
                    }
                    else
                        log.WarnFormat("[GetContactAvatarImageSource] - file not found - filePath:[{0}] - ContactId:[{1}]", filePath, contactId);
                }
                else
                    log.WarnFormat("[GetContactAvatarImageSource] - no file path for this ContactId:[{0}]", contactId);
            }
            else
            {
                String filePath = AvatarPool.Instance.GetUnknownAvatarPath();
                if (!String.IsNullOrEmpty(filePath))
                {
                    if (File.Exists(filePath))
                        result = BitmapImageFromFile(filePath);
                    else
                        log.WarnFormat("[GetContactAvatarImageSource] - file not found - filePath:[{0}]", filePath);
                }
                else
                    log.WarnFormat("[GetContactAvatarImageSource] - Cannot get unknown avatar ...");
            }
            return result;
        }

        public static BitmapImage GetConversationAvatarImageSourceByPeerId(String peerId)
        {
            InstantMessaging.App CurrentApplication = (InstantMessaging.App)System.Windows.Application.Current;
            Conversation conversation = CurrentApplication.RbConversations.GetConversationByPeerIdFromCache(peerId);
            if (conversation != null)
                return GetAvatarImageSourceByPeerIdAndType(conversation.PeerId, conversation.Type);
            else
            {
                // This peerId could be a Bubble wihtout an existing conversation
                Bubble bubble = CurrentApplication.RbBubbles.GetBubbleByIdFromCache(peerId);
                if (bubble != null)
                    return GetBubbleAvatarImageSource(peerId);
            }
            return null;
        }

        public static BitmapImage GetAvatarImageSourceByPeerIdAndType(String peerId, String type)
        {
            if (type == Rainbow.Model.Conversation.ConversationType.User)
                return GetContactAvatarImageSource(peerId);
            else if (type == Rainbow.Model.Conversation.ConversationType.Room)
                return GetBubbleAvatarImageSource(peerId);
            return null;
        }

        public static BitmapImage GetConversationAvatarImageSource(ConversationLightViewModel conversation)
        {
            BitmapImage result = null;
            if (conversation != null)
                result = GetAvatarImageSourceByPeerIdAndType(conversation.PeerId, conversation.Type);
            return result;
        }
#endregion AVATAR - IMAGES MANAGEMENT

    }
}
