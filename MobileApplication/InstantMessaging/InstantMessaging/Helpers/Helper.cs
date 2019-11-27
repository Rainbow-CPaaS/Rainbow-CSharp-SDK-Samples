using System;
using System.IO;
using System.Reflection;

using Xamarin.Forms;

using Rainbow;
using Rainbow.Helpers;
using Rainbow.Model;

namespace InstantMessaging.Helpers
{
    public static class Helper
    {
        public static String GetTempFolder()
        {
            return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        }
        
        public static MemoryStream GetMemoryStreamFromResource(String resourceName)
        {
            var applicationTypeInfo = Xamarin.Forms.Application.Current.GetType().GetTypeInfo();

            MemoryStream ms = new MemoryStream();
            using (Stream stream = applicationTypeInfo.Assembly.GetManifestResourceStream($"{applicationTypeInfo.Namespace}.{resourceName}"))
            {
                stream.CopyTo(ms);
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
        public static String Humanize(DateTime dt)
        {
            String result;

            DateTime nowInUTC = DateTime.UtcNow;
            DateTime dtInUTC = dt.ToUniversalTime();

            DateTime dtLocalTime = dt.ToLocalTime();

            if (nowInUTC.Day == dtInUTC.Day)
                result = dtLocalTime.ToString("H:mm");
            else if (nowInUTC.Year == dtInUTC.Year)
                result = dtLocalTime.ToString("dd MMM H:mm");
            else
                result = dtLocalTime.ToString("dd MMM yy H:mm");

            return result;
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
                    presenceSource = "presence_online.png";
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

        public static ConversationLight GetConversationFromRBConversation(Rainbow.Model.Conversation rbConversation)
        {
            ConversationLight conversation = null;

            if (rbConversation != null)
            {
                InstantMessaging.App XamarinApplication = (InstantMessaging.App)Xamarin.Forms.Application.Current;
                AvatarPool avatarPool = AvatarPool.Instance;

                conversation = new ConversationLight();


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
                    Rainbow.Model.Contact contact = XamarinApplication.RbContacts.GetContactFromContactId(rbConversation.PeerId);
                    if (contact != null)
                    {
                        conversation.Name = Util.GetContactDisplayName(contact, avatarPool.GetFirstNameFirst());
                        conversation.Topic = "";
                        conversation.Jid = contact.Jid_im;

                        Presence presence = XamarinApplication.RbContacts.GetPresenceFromContactId(rbConversation.PeerId);
                        conversation.PresenceSource = InstantMessaging.Helpers.Helper.GetPresenceSourceFromPresence(presence);

                    }
                    else
                    {
                        //log.DebugFormat("[GetConversationFromRBConversation] - unknown contact - contactId:[{0}]", rbConversation.PeerId);
                        XamarinApplication.RbContacts.GetContactFromContactIdFromServer(rbConversation.PeerId, null);
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
                conversation.MessageTimeDisplay = Helpers.Helper.Humanize(conversation.LastMessageDateTime);
            }
            return conversation;
        }

        public static InstantMessaging.Model.Message GetMessageFromRBMessage(Rainbow.Model.Message rbMessage, String conversationType)
        {
            InstantMessaging.Model.Message message = null;
            if (rbMessage != null)
            {
                InstantMessaging.App XamarinApplication = (InstantMessaging.App)Xamarin.Forms.Application.Current;
                AvatarPool avatarPool = AvatarPool.Instance;

                message = new InstantMessaging.Model.Message();

                Rainbow.Model.Contact contact = XamarinApplication.RbContacts.GetContactFromContactJid(rbMessage.FromJid);
                if (contact != null)
                {
                    message.PeerId = contact.Id;

                    message.PeerDisplayName = Util.GetContactDisplayName(contact, avatarPool.GetFirstNameFirst());
                    message.BackgroundColor = Rainbow.Helpers.AvatarPool.GetColorFromDisplayName(message.PeerDisplayName);
                }
                else
                {
                    message.BackgroundColor = Rainbow.Helpers.AvatarPool.GetColorFromDisplayName("");
                    XamarinApplication.RbContacts.GetContactFromContactJidFromServer(rbMessage.FromJid, null);
                }

                // We have the display name only in Room / Bubble context
                message.PeerDisplayNameIsVisible = (conversationType == Rainbow.Model.Conversation.ConversationType.Room) ? "True" : "False";

                message.Id = rbMessage.Id;
                message.PeerJid = rbMessage.FromJid;
                message.Body = rbMessage.Content;
                message.MessageDateTime = rbMessage.Date;
                message.MessageDateDisplay = Humanize(rbMessage.Date);
            }
            return message;
        }

        public static ImageSource GetBubbleAvatarImageSource(String bubbleId)
        {
            ImageSource result = null;
            if (!String.IsNullOrEmpty(bubbleId))
            {
                String filePath = null;
                try
                {
                    filePath = AvatarPool.Instance.GetBubbleAvatarPath(bubbleId);
                }
                catch (Exception exc)
                {
                    //log.WarnFormat("[SetConversationAvatar] PeerId:[{0}] - exception occurs to create avatar:[{1}]", conversation.PeerId, Util.SerializeException(exc));
                }

                if (!String.IsNullOrEmpty(filePath))
                {
                    if (File.Exists(filePath))
                    {
                        //log.DebugFormat("[SetConversationAvatar] - file used - filePath:[{0}] - PeerId:[{1}]", filePath, conversation.PeerId);
                        try
                        {
                            result = ImageSource.FromFile(filePath);
                        }
                        catch (Exception exc)
                        {
                            //log.WarnFormat("[SetConversationAvatar] PeerId:[{0}] - exception occurs to display avatar[{1}]", conversation.PeerId, Util.SerializeException(exc));
                        }
                    }
                    //else
                    //    log.WarnFormat("[SetConversationAvatar] - file not found - filePath:[{0}] - PeerId:[{1}]", filePath, conversation.PeerId);
                }
            }
            return result;
        }

        public static ImageSource GetContactAvatarImageSource(String contactId)
        {
            ImageSource result = null;
            if (!String.IsNullOrEmpty(contactId))
            {
                String filePath = null;
                try
                {
                    filePath = AvatarPool.Instance.GetContactAvatarPath(contactId);
                }
                catch (Exception exc)
                {
                    //log.WarnFormat("[SetConversationAvatar] PeerId:[{0}] - exception occurs to create avatar:[{1}]", conversation.PeerId, Util.SerializeException(exc));
                }

                if (!String.IsNullOrEmpty(filePath))
                {
                    if (File.Exists(filePath))
                    {
                        //log.DebugFormat("[SetConversationAvatar] - file used - filePath:[{0}] - PeerId:[{1}]", filePath, conversation.PeerId);
                        try
                        {
                            result = ImageSource.FromFile(filePath);
                        }
                        catch (Exception exc)
                        {
                            //log.WarnFormat("[SetConversationAvatar] PeerId:[{0}] - exception occurs to display avatar[{1}]", conversation.PeerId, Util.SerializeException(exc));
                        }
                    }
                    //else
                    //    log.WarnFormat("[SetConversationAvatar] - file not found - filePath:[{0}] - PeerId:[{1}]", filePath, conversation.PeerId);
                }
            }
            return result;
        }

        public static ImageSource GetConversationAvatarImageSource(ConversationLight conversation)
        {
            ImageSource result = null;
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
                    //log.WarnFormat("[SetConversationAvatar] PeerId:[{0}] - exception occurs to create avatar:[{1}]", conversation.PeerId, Util.SerializeException(exc));
                }

                if (!String.IsNullOrEmpty(filePath))
                {
                    if (File.Exists(filePath))
                    {
                        //log.DebugFormat("[SetConversationAvatar] - file used - filePath:[{0}] - PeerId:[{1}]", filePath, conversation.PeerId);
                        try
                        {
                            result = ImageSource.FromFile(filePath);
                        }
                        catch (Exception exc)
                        {
                            //log.WarnFormat("[SetConversationAvatar] PeerId:[{0}] - exception occurs to display avatar[{1}]", conversation.PeerId, Util.SerializeException(exc));
                        }
                    }
                    //else
                    //    log.WarnFormat("[SetConversationAvatar] - file not found - filePath:[{0}] - PeerId:[{1}]", filePath, conversation.PeerId);
                }
            }
            return result;
        }



    }
}
