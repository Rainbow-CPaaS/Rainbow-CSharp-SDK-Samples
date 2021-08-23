using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Rainbow;
using Rainbow.Common;
using static Rainbow.Common.Avatars;
using Rainbow.Model;

using MultiPlatformApplication.Helpers;

using NLog;

namespace MultiPlatformApplication
{
    public class TestSdkWrapper: SdkWrapper
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(TestSdkWrapper));

        // /!\ Necessary only to get correct path/folders about Avatars
        Rainbow.Application RbApplication;
        Rainbow.Common.Avatars RbAvatars;

        Contact currentContact;
        Dictionary<String, Contact> contactsById;
        Dictionary<String, String> contactsIdByJid;

        Dictionary<string, Dictionary<String, Rainbow.Model.Presence>> presencesByBasicJid;
        Dictionary<string, Rainbow.Model.Presence> aggregatePresencesByBasicJid;

        Dictionary<String, Bubble> bubblesById;
        Dictionary<String, String> bubblesIdByJid;

        Dictionary<String, Conversation> conversationsById;
        Dictionary<String, String> conversationsIdByJid;

        Dictionary<String, List<Rainbow.Model.Message>> messagesByConversationId;

        Dictionary<String, FileDescriptor> filesDescriptorById;
        Dictionary<String, String > conversationIdByFilesDescriptorId;

        String connectionState = Rainbow.Model.ConnectionState.Disconnected;

        public TestSdkWrapper()
        {
            // We just need to create it to use it in "Avatars" constructor
            RbApplication = new Rainbow.Application(Helper.GetAppFolderPath());
            RbApplication.SetApplicationInfo(ApplicationInfo.APP_ID, ApplicationInfo.APP_SECRET_KEY);
            RbApplication.SetHostInfo(ApplicationInfo.HOST_NAME);

            // Get AvatarPool and initialize it
            string avatarsFolderPath = Helper.GetAvatarsFolderPath();

            RbAvatars = Avatars.Instance;
            RbAvatars.SetApplication(RbApplication);
            RbAvatars.SetDensity((float)Helper.GetDensity());
            RbAvatars.SetFolderPathUsedToStoreAvatars(avatarsFolderPath);

            if (!RbAvatars.Initialize())
                log.Error("CANNOT initialize Avatars service ...");

            // Set default max size to Thumbnail
            MaxThumbnailWidth = MaxThumbnailHeight = 300;
        }

#region PUBLIC API


    #region WRAPPER - APPLICATION

        public override String GetUserLoginFromCache()
        {
            // TODO
            return "Using offline mode";
        }

        public override String GetUserPasswordFromCache()
        {
            // TODO
            return "Fake pwd";
        }

        public override String ConnectionState()
        {
            return connectionState;
        }

        public override void Login(string login, string password, Action<SdkResult<Boolean>> callback = null)
        {
            connectionState = Rainbow.Model.ConnectionState.Connecting;
            OnConnectionStateChanged(this, new Rainbow.Events.ConnectionStateEventArgs(connectionState));

            Task task = new Task(() =>
            {
                // Retrieve data - currentContact
                currentContact = DataStorage.RetrieveCurrentContact();

                // Retrieve data - contacts
                List<Rainbow.Model.Contact> contacts = DataStorage.RetrieveContacts();
                contactsById = contacts.ToDictionary(contact => contact.Id, contact => contact);
                contactsIdByJid = contacts.ToDictionary(contact => contact.Jid_im, contact => contact.Id);
                contacts.Clear();

                // Retrieve data - Presences and Aggregated Presences
                presencesByBasicJid = DataStorage.RetrievePresences();
                aggregatePresencesByBasicJid = DataStorage.RetrieveAggregatedPresences();

                // Retrieve data - bubbles
                List<Rainbow.Model.Bubble> bubbles = DataStorage.RetrieveBubbles();
                bubblesById = bubbles.ToDictionary(bubble => bubble.Id, bubble => bubble);
                bubblesIdByJid = bubbles.ToDictionary(bubble => bubble.Jid, bubble => bubble.Id);
                bubbles.Clear();

                // Retrieve data - conversations
                List<Rainbow.Model.Conversation> conversations = DataStorage.RetrieveConversations();
                conversationsById = conversations.ToDictionary(conversation => conversation.Id, conversation => conversation);
                conversationsIdByJid = conversations.ToDictionary(conversation => conversation.Jid_im, conversation => conversation.Id);
                conversations.Clear();

                // Retrieve data - FilesDescriptor
                filesDescriptorById = DataStorage.RetrieveFilesDescriptor();
                conversationIdByFilesDescriptorId = DataStorage.RetrieveConversationsByFilesDescriptor();

                connectionState = Rainbow.Model.ConnectionState.Connected;
                OnConnectionStateChanged(this, new Rainbow.Events.ConnectionStateEventArgs(connectionState));
                OnInitializationPerformed(this, null);
            });
            task.Start();
        }

        public override void Logout(Action<SdkResult<Boolean>> callback = null)
        {
            connectionState = Rainbow.Model.ConnectionState.Disconnected;
            OnConnectionStateChanged(this, new Rainbow.Events.ConnectionStateEventArgs(connectionState));
        }

    #endregion WRAPPER - APPLICATION

    #region WRAPPER - FILES

        public override int MaxThumbnailWidth { get; set; }

        public override int MaxThumbnailHeight { get; set; }

        public override Boolean IsThumbnailFileAvailable(String conversationId, String fileDescriptorId, String fileName)
        {
            bool result = false;

            string path;

            String fileNameOnDisk = fileDescriptorId + Path.GetExtension(fileName);
            String folderPathThumbnails = Path.Combine(Helper.GetFileStorageFolderPath(), "Thumbnails");

            // We check on file system if already in cache - Thumbnails folder first
            path = Path.Combine(folderPathThumbnails, fileNameOnDisk);
            if (File.Exists(path))
            {
                //log.Debug("[IsThumbnailFileAvailable] Thumbnail exists for file descriptor id:[{0}] - path:[{1}]", fileDescriptorId, path);
                result = true;
            }
            else
            {
                //log.Warn("[IsThumbnailFileAvailable] Thumbnail not exists for file descriptor id:[{0}] - path:[{1}]", fileDescriptorId, path);

                folderPathThumbnails = Path.Combine(Helper.GetFileStorageFolderPath(), "Files");
                path = Path.Combine(folderPathThumbnails, fileNameOnDisk);
                if (File.Exists(path))
                {
                    //log.Debug("[IsThumbnailFileAvailable] Thumbnail exists for file descriptor id:[{0}] - path:[{1}]", fileDescriptorId, path);
                    result = true;
                }
                else
                    log.Warn("[IsThumbnailFileAvailable] Thumbnail not exists for file descriptor id:[{0}] - path:[{1}]", fileDescriptorId, path);
            }
            return result;
        }

        public override String GetThumbnailFullFilePath(String fileDescriptorId)
        {
            if(filesDescriptorById?.ContainsKey(fileDescriptorId) == true)
            {
                FileDescriptor fileDescriptor = filesDescriptorById[fileDescriptorId];

                String fileNameOnDisk = fileDescriptorId + Path.GetExtension(fileDescriptor.Name);


                // We check on file system if already in cache - Thumbnails folder first
                String folderPathThumbnails = Path.Combine(Helper.GetFileStorageFolderPath(), "Thumbnails");
                String path = Path.Combine(folderPathThumbnails, fileNameOnDisk);
                if (File.Exists(path))
                {
                    //log.Debug("[GetThumbnailFullFilePath] File found for file descriptor id:[{0}] - path:[{1}]", fileDescriptorId, path);
                    return path;
                }
                else
                {
                    folderPathThumbnails = Path.Combine(Helper.GetFileStorageFolderPath(), "Files");
                    path = Path.Combine(folderPathThumbnails, fileNameOnDisk);
                    if (File.Exists(path))
                    {
                        //log.Debug("[GetThumbnailFullFilePath] File found for file descriptor id:[{0}] - path:[{1}]", fileDescriptorId, path);
                        return path;
                    }
                    else
                        log.Warn("[GetThumbnailFullFilePath] Thumbnail not exists for file descriptor id:[{0}] - path:[{1}]", fileDescriptorId, path);
                }
            }
            else
            {
                log.Warn("[GetThumbnailFullFilePath] Cannot found file descriptor id:[{0}]", fileDescriptorId);
            }
            return null;
        }

        public override void AskFileDescriptorDownload(String conversationId, String fileDescriptorId)
        {
            // Nothing to do here
        }

        public override String GetConversationIdByFileDescriptorId(String fileDescriptorId)
        {
            if (conversationIdByFilesDescriptorId?.ContainsKey(fileDescriptorId) == true)
                return conversationIdByFilesDescriptorId[fileDescriptorId];

            return null;
        }

    #endregion WRAPPER - FILES

    #region WRAPPER - AVATARS

        public override void AddUnknownContactToPoolByJid(String contactJid)
        {
            // Nothing to do here
        }

        public override String GetBubbleAvatarPath(String bubbleId, AvatarType avatarType = AvatarType.ROUNDED)
        {
            String filepath = RbAvatars.GetTheoricBubbleAvatarPath(bubbleId, avatarType);

            if (File.Exists(filepath))
                return filepath;

            switch (avatarType)
            {
                case AvatarType.ROUNDED:
                    return GetBubbleAvatarPath(bubbleId, AvatarType.INITIALS);

                case AvatarType.BIG:
                    return GetBubbleAvatarPath(bubbleId, AvatarType.STANDARD);

                case AvatarType.STANDARD:
                    return GetBubbleAvatarPath(bubbleId, AvatarType.ORIGINAL);

                case AvatarType.ORIGINAL:
                    return GetBubbleAvatarPath(bubbleId, AvatarType.INITIALS);
                    
                case AvatarType.INITIALS:
                    return GetUnknwonContactAvatarFilePath();
            }


            return null;
        }

        public override String GetContactAvatarPath(String contactId, AvatarType avatarType = AvatarType.ROUNDED)
        {
            String filepath = RbAvatars.GetTheoricContactAvatarPath(contactId, avatarType);

            if (File.Exists(filepath))
                return filepath;

            switch(avatarType)
            {
                case AvatarType.ROUNDED:
                    return GetContactAvatarPath(contactId, AvatarType.INITIALS);

                case AvatarType.BIG:
                    return GetContactAvatarPath(contactId, AvatarType.STANDARD);

                case AvatarType.STANDARD:
                    return GetContactAvatarPath(contactId, AvatarType.ORIGINAL);

                case AvatarType.ORIGINAL:
                    return GetContactAvatarPath(contactId, AvatarType.INITIALS);

                case AvatarType.INITIALS:
                    return GetUnknwonContactAvatarFilePath();
            }
            

            return null;
        }

        public override String GetUnknwonContactAvatarFilePath()
        {
            return RbAvatars.GetUnknwonContactAvatarFilePath();
        }

        public override String GetUnknwonBubbleAvatarFilePath()
        {
            return RbAvatars.GetUnknwonBubbleAvatarFilePath();
        }

    #endregion WRAPPER - AVATARS

    #region WRAPPER - INSTANT MESSAGING

        public override Rainbow.Model.Message GetOneMessageFromConversationIdFromCache(String conversationId, String messageId)
        {
            List<Rainbow.Model.Message> messages = GetAllMessagesFromConversationIdFromCache(conversationId);
            if (messages != null)
            {
                foreach (Message message in messages)
                {
                    if (message.Id == messageId)
                        return message;
                }
            }
            return null;
        }

        public override void GetOneMessageFromConversationId(String conversationId, String messageId, String stamp, Action<SdkResult<Message>> callback)
        {
            Task task = new Task(() =>
            {
                Message message = GetOneMessageFromConversationIdFromCache(conversationId, messageId);

                if(message != null)
                    callback?.Invoke(new SdkResult<Message>(message));
                else
                    callback?.Invoke(new SdkResult<Message>("No message found"));
            });
            task.Start();
        }

        public override void GetMessagesFromConversationId(string conversationId, int nbMessages, Action<SdkResult<List<Message>>> callback)
        {
            Task task = new Task(() =>
            {
                System.Threading.Thread.Sleep(2000); // Simulate delay

                List<Rainbow.Model.Message> messages = GetAllMessagesFromConversationIdFromCache(conversationId);

                // We limit to 20 messages
                if ((messages != null) && (messages.Count > 20 ))
                    messages = messages.GetRange(0, 20);

                if (messages == null)
                    callback?.Invoke(new SdkResult<List<Message>>("No messages"));
                else
                    callback?.Invoke(new SdkResult<List<Message>>(messages));
            });
            task.Start();
        }

        public override void SendMessageToConversationId(String id, String content, UrgencyType urgencyType, List<String> mentions = null, Action<SdkResult<Message>> callbackMessage = null)
        {
            // TODO
        }

        public override void SendMessageWithFileToConversationId(String id, Stream stream, String filename, UrgencyType urgencyType, Action<SdkResult<FileDescriptor>> callbackFileDescriptor, Action<SdkResult<Message>> callbackMessage)
        {
            // TODO
        }

        public override void SendIsTypingInConversationById(String id, Boolean isTyping)
        {
            // TODO
        }

        public override void MarkMessageAsRead(string conversationId, string messageId)
        {
            // TODO
        }

        public override void MarkAllMessagesAsRead(String id)
        {
            // TODO
        }

        public override List<Rainbow.Model.Message> GetAllMessagesFromConversationIdFromCache(String id)
        {
            if(messagesByConversationId?.ContainsKey(id) == true)
                return messagesByConversationId[id];

            List<Rainbow.Model.Message> messages = DataStorage.RetrieveMessages(id);
            if(messages != null)
            {
                // We limit to 20 messages
                if (messages.Count > 20)
                    messages = messages.GetRange(0, 20);

                if (messagesByConversationId == null)
                    messagesByConversationId = new Dictionary<string, List<Message>>();
                messagesByConversationId.Add(id, messages);
            }
            return messages;
        }

    #endregion WRAPPER - INSTANT MESSAGING

    #region WRAPPER - CONVERSATIONS

        public override void GetAllConversations(Action<SdkResult<List<Conversation>>> callback)
        {
            if (conversationsById != null)
                callback?.Invoke(new SdkResult<List<Conversation>>(conversationsById.Values.ToList()));
            else
                callback?.Invoke(new SdkResult<List<Conversation>>("No conversations."));
        }

        public override List<Conversation> GetAllConversationsFromCache()
        {
            List<Conversation> conversations = conversationsById?.Values.ToList();
            //if (conversations != null)
            //    conversations = conversations.GetRange(0, 5);
            return conversations;
        }

        public override Rainbow.Model.Conversation GetConversationByIdFromCache(String id)
        {
            if (conversationsById?.ContainsKey(id) == true)
                return conversationsById[id];
            return null;
        }

        public override Rainbow.Model.Conversation GetConversationByPeerIdFromCache(String peerId)
        {
            if(conversationsById != null)
            {
                foreach(Conversation conversation in conversationsById.Values)
                {
                    if (conversation.PeerId == peerId)
                        return conversation;
                }
            }
            return null;
        }

        public override Rainbow.Model.Conversation GetOrCreateConversationFromUserId(string userId)
        {
            return GetConversationByPeerIdFromCache(userId);
        }

        public override Rainbow.Model.Conversation GetOrCreateConversationFromBubbleId(string bubbleId)
        {
            return GetConversationByPeerIdFromCache(bubbleId);
        }

    #endregion WRAPPER - CONVERSATIONS

    #region WRAPPER - BUBBLES

        public override void GetAllBubbles(Action<SdkResult<List<Bubble>>> callback = null)
        {
            if(bubblesById != null)
                callback?.Invoke(new SdkResult<List<Bubble>>(bubblesById.Values.ToList()));
            else
                callback?.Invoke(new SdkResult<List<Bubble>>("No bubbles."));
        }

    #endregion WRAPPER - BUBBLES

    #region WRAPPER - CONTACTS

        public override String GetCurrentContactId()
        {
            return currentContact?.Id;
        }

        public override string GetCurrentContactJid()
        {
            return currentContact?.Jid_im;
        }
        
        public override List<Contact> GetAllContactsFromCache()
        {
            return contactsById.Values.ToList<Contact>();
        }

        public override Rainbow.Model.Contact GetContactFromContactId(String peerId)
        {
            if (contactsById.ContainsKey(peerId))
                return contactsById[peerId];
            
            return null;
        }

        public override Rainbow.Model.Contact GetContactFromContactJid(String jid)
        {
            if (contactsIdByJid.ContainsKey(jid))
                return GetContactFromContactId(contactsIdByJid[jid]);
            return null;
        }

        public override Rainbow.Model.Presence GetAggregatedPresenceFromContactId(String peerId)
        {
            Contact contact = GetContactFromContactId(peerId);
            if(contact != null)
            {
                String basicJid = Rainbow.Util.GetBasicNodeJid(contact.Jid_im);
                if (aggregatePresencesByBasicJid.ContainsKey(basicJid))
                    return aggregatePresencesByBasicJid[basicJid];
            }
            return null;
        }

        public override void GetContactFromContactIdFromServer(String peerId, Action<SdkResult<Contact>> callback = null)
        {
            Contact result = GetContactFromContactId(peerId);
            if (result != null)
                callback?.Invoke(new SdkResult<Contact>(result));
             else
                callback?.Invoke(new SdkResult<Contact>("Contact not found"));
        }

    #endregion WRAPPER - CONTACTS



#endregion PUBLIC API
    }
}
