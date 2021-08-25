using System;
using System.Collections.Generic;
using System.IO;

using Rainbow;
using static Rainbow.Common.Avatars;
using Rainbow.Events;
using Rainbow.Model;

namespace MultiPlatformApplication
{
    public class SdkWrapper
    {

        internal Rainbow.Common.Languages RbLanguages = Rainbow.Common.Languages.Instance;

#region PUBLIC EVENT

        // From "File Storage" Service
        public event EventHandler<FileUploadEventArgs> FileUploadUpdated;
        public event EventHandler<FileDownloadEventArgs> FileDownloadUpdated;

        // From "Files Pool" Service
        public event EventHandler<IdEventArgs> ThumbnailAvailable;
        public event EventHandler<IdEventArgs> FileDescriptorAvailable;
        public event EventHandler<IdEventArgs> FileDescriptorNotAvailable;

        // From "Avatars" Service
        public event EventHandler<PeerEventArgs> PeerAvatarUpdated;

        // From "Instant Messaging" Service
        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler<UserTypingEventArgs> UserTypingChanged;
        public event EventHandler<ReceiptReceivedEventArgs> ReceiptReceived;
        public event EventHandler<IdEventArgs> MessagesAllRead;
        public event EventHandler<IdEventArgs> MessagesAllDeleted;

        // From "Contacts" Service
        public event EventHandler<PeerEventArgs> PeerAdded;
        public event EventHandler<PeerEventArgs> PeerInfoChanged;
        public event EventHandler<PresenceEventArgs> ContactPresenceChanged;
        public event EventHandler<PresenceEventArgs> ContactAggregatedPresenceChanged;

        // From "Conversations" Service
        public event EventHandler<ConversationEventArgs> ConversationCreated;
        public event EventHandler<ConversationEventArgs> ConversationRemoved;
        public event EventHandler<ConversationEventArgs> ConversationUpdated;

        // From "Bubbles" Service
        public event EventHandler<BubbleInfoEventArgs> BubbleInfoUpdated;

        // From "Application" Service
        public event EventHandler<ConnectionStateEventArgs> ConnectionStateChanged;
        public event EventHandler<EventArgs> InitializationPerformed;

        // UI purpose
        public event EventHandler<IdEventArgs> StartMessageEdition; // Ask to start the edition of the message using its Id
        public event EventHandler<StringListEventArgs> StopMessageEdition; // Ask to stop the edition of the message using its Id and the new message (if any)
        public event EventHandler<IdEventArgs> StartFileDownload; // Ask to start file download using File Descriptor Id

#endregion PUBLIC EVENT

#region TO RAISE EVENT

    #region UI PURPOSE

        public void OnStartMessageEdition(object sender, IdEventArgs args)
        {
            StartMessageEdition.Raise(sender, args);
        }

        public void OnStopMessageEdition(object sender, StringListEventArgs args)
        {
            StopMessageEdition.Raise(sender, args);
        }

        public void OnStartFileDownload(object sender, IdEventArgs args)
        {
            StartFileDownload.Raise(sender, args);
        }

    #endregion UI PURPOSE

    #region FILE STORAGE SERVICE

        internal void OnFileUploadUpdated(object sender, FileUploadEventArgs args)
        {
            FileUploadUpdated.Raise(sender, args);
        }

        internal void OnFileDownloadUpdated(object sender, FileDownloadEventArgs args)
        {
            FileDownloadUpdated.Raise(sender, args);
        }

    #endregion FILE STORAGE SERVICE

    #region FILE POOL SERVICE

        internal void OnThumbnailAvailable(object sender, IdEventArgs args)
        {
            ThumbnailAvailable.Raise(sender, args);
        }

        internal void OnFileDescriptorAvailable(object sender, IdEventArgs args)
        {
            FileDescriptorAvailable.Raise(sender, args);
        }

        internal void OnFileDescriptorNotAvailable(object sender, IdEventArgs args)
        {
            FileDescriptorNotAvailable.Raise(sender, args);
        }

    #endregion FILE POOL SERVICE

    #region AVATARS SERVICE

        internal void OnPeerAvatarUpdated(object sender, PeerEventArgs args)
        {
            PeerAvatarUpdated.Raise(sender, args);
        }

    #endregion AVATARS SERVICE

    #region APPLICATION SERVICE

        internal void OnConnectionStateChanged(object sender, ConnectionStateEventArgs args)
        {
            ConnectionStateChanged.Raise(sender, args);
        }

        internal void OnInitializationPerformed(object sender, EventArgs args)
        {
            InitializationPerformed.Raise(sender, args);
        }

    #endregion APPLICATION SERVICE

    #region BUBBLES SERVICE

        internal void OnBubbleInfoUpdated(object sender, BubbleInfoEventArgs args)
        {
            BubbleInfoUpdated.Raise(sender, args);
        }

    #endregion BUBBLES SERVICE

    #region CONVERSATIONS SERVICE

        internal void OnConversationCreated(object sender, ConversationEventArgs args)
        {
            ConversationCreated.Raise(sender, args);
        }

        internal void OnConversationRemoved(object sender, ConversationEventArgs args)
        {
            ConversationRemoved.Raise(sender, args);
        }

        internal void OnConversationUpdated(object sender, ConversationEventArgs args)
        {
            ConversationUpdated.Raise(sender, args);
        }

    #endregion CONVERSATIONS SERVICE

    #region CONTACTS SERVICE

        internal void OnContactAdded(object sender, PeerEventArgs args)
        {
            PeerAdded.Raise(sender, args);
        }

        internal void OnContactInfoChanged(object sender, PeerEventArgs args)
        {
            PeerInfoChanged.Raise(sender, args);
        }

        internal void OnContactAggregatedPresenceChanged(object sender, PresenceEventArgs args)
        {
            ContactAggregatedPresenceChanged.Raise(sender, args);
        }

        internal void OnContactPresenceChanged(object sender, PresenceEventArgs args)
        {
            ContactPresenceChanged.Raise(sender, args);
        }

    #endregion CONTACTS SERVICE

    #region INSTANT MESSAGING SERVICE

        internal void OnMessageReceived(object sender, MessageEventArgs args)
        {
            MessageReceived.Raise(sender, args);
        }

        internal void OnUserTypingChanged(object sender, UserTypingEventArgs args)
        {
            UserTypingChanged.Raise(sender, args);
        }
        
        internal void OnReceiptReceived(object sender, ReceiptReceivedEventArgs args)
        {
            ReceiptReceived.Raise(sender, args);
        }

        internal void OnMessagesAllRead(object sender, IdEventArgs args)
        {
            MessagesAllRead.Raise(sender, args);
        }

        internal void OnMessagesAllDeleted(object sender, IdEventArgs args)
        {
            MessagesAllDeleted.Raise(sender, args);
        }

    #endregion INSTANT MESSAGING SERVICE

#endregion TO RAISE EVENT

#region PUBLIC API

    #region STATIC METHODS
        public static String GetColorFromDisplayName(String displayName)
        {
            return Rainbow.Common.Avatars.GetColorFromDisplayName(displayName);
        }

        // NOT NECESSARY TO HAVE THIS VIRTUAL
        public static String GetDarkerColorFromDisplayName(String displayName)
        {
            return Rainbow.Common.Avatars.GetDarkerColorFromDisplayName(displayName);
        }

    #endregion STATIC METHODS

    #region LANGUAGES SERVICE

        public  Boolean SetLanguageId(String languageId)
        {
            return RbLanguages.SetLanguageId(languageId);
        }

        public String GetLabel(String key)
        {
            if (String.IsNullOrEmpty(key))
                return "[! label key is NULL !]";

            String label = RbLanguages.GetLabel(key);
            if (String.IsNullOrEmpty(label))
                label = "[! " + key + " !]";
            return label;
        }

        public String GetLabel(String key, String subtituteKey, String subtituteValue)
        {
            if(String.IsNullOrEmpty(key))
                return "[! label key is NULL !]";
            
            String label = RbLanguages.GetLabel(key, subtituteKey, subtituteValue);
            if (String.IsNullOrEmpty(label))
                label = "[! " + key + " !]";
            return label;
        }

    #endregion LANGUAGES SERVICE

    #region WRAPPER - APPLICATION

        virtual public Restrictions.SDKMessageStorageMode GetMessageStorageMode()
        {
            return Restrictions.SDKMessageStorageMode.Store;
        }

        virtual public Boolean IsInitialized()
        {
            return true;
        }

        virtual public String GetResourceId()
        {
            return "";
        }

        virtual public int GetTimeout()
        {
            return 2000;
        }

        virtual public String GetUserLoginFromCache()
        {
            return null;
        }

        virtual public String GetUserPasswordFromCache()
        {
            return null;
        }

        virtual public String ConnectionState()
        {
            return null;
        }

        virtual public void Login(string login, string password, Action<SdkResult<Boolean>> callback = null)
        {
        }

        virtual public void Logout(Action<SdkResult<Boolean>> callback = null)
        {
        }

    #endregion WRAPPER - APPLICATION

    #region WRAPPER - FILESTORAGE

        virtual public void CreateFileDescriptor(String fileName, long fileSize, String peerId, String peerType, Action<SdkResult<FileDescriptor>> callback)
        {
        }

        virtual public void RemoveFileDescriptor(String fileId, Action<SdkResult<Boolean>> callback)
        {

        }

        virtual public void UploadFile(Stream fileStream, String peerId, String peerType, FileDescriptor fileDescriptor, Action<SdkResult<Boolean>> callbackResult)
        {
        }

        virtual public void DownloadFile(String fileId, String destinationFolder, String destinationFileName, Action<SdkResult<Boolean>> callback)
        {
        }

        virtual public FileDescriptor GetFileDescriptorFromCache(String fileId)
        {
            return null;
        }

        virtual public String GetFolderPathForFilesStorage()
        {
            return "./";
        }


    #endregion WRAPPER - FILESTORAGE

    #region WRAPPER - FILES

        virtual public int MaxThumbnailWidth { get; set; }

        virtual public int MaxThumbnailHeight { get; set; }

        virtual public Boolean IsThumbnailFileAvailable(String conversationId, String fileDescriptorId, String fileName)
        {
            return false;
        }

        virtual public Boolean IsFileDescriptorNotAvailable(String fileDescriptorId)
        {
            return false;
        }

        virtual public void AskFileDescriptorDownload(String conversationId, String fileDescriptorId)
        {
        }

        virtual public String GetThumbnailFullFilePath(String fileDescriptorId)
        {
            return null;
        }

        virtual public String GetConversationIdByFileDescriptorId(String fileDescriptorId)
        {
            return null;
        }

    #endregion WRAPPER - FILES

    #region WRAPPER - AVATARS

        virtual public void AddUnknownContactToPoolByJid(String contactJid)
        {

        }

        virtual public String GetBubbleAvatarPath(String bubbleId, AvatarType avatarType = AvatarType.ROUNDED)
        {
            return null;
        }

        virtual public String GetContactAvatarPath(String contactId, AvatarType avatarType = AvatarType.ROUNDED)
        {
            return null;
        }

        virtual public String GetUnknwonContactAvatarFilePath()
        {
            return null;
        }

        virtual public String GetUnknwonBubbleAvatarFilePath()
        {
            return null;
        }


    #endregion WRAPPER - AVATARS

    #region WRAPPER - INSTANT MESSAGING

        virtual public void DeleteMessage(String conversationId, String messageID, Action<SdkResult<Message>> callback = null)
        {

        }

        virtual public Rainbow.Model.Message GetOneMessageFromConversationIdFromCache(String conversationId, String messageId)
        {
            return null;
        }

        virtual public void GetOneMessageFromConversationId(String conversationId, String messageId, String stamp, Action<SdkResult<Message>> callback)
        {
        }

        virtual public void GetMessagesFromConversationId(string conversationId, int nbMessages, Action<SdkResult<List<Message>>> callback)
        {
        }

        virtual public void EditMessage(String conversationId, String messageID, String newMessage, Action<SdkResult<Message>> callback = null)
        {
        }

        virtual public void ReplyToMessage(String conversationId, String messageID, String replyMessage, Action<SdkResult<Message>> callback = null)
        {

        }

        virtual public bool SendMessage(Conversation conversation, ref Message message)
        {
            return true;
        }

        virtual public void SendMessageToConversationId(String id, String content, UrgencyType urgencyType, List<String> mentions = null, Action<SdkResult<Message>> callbackMessage = null)
        {
        }

        virtual public void SendMessageWithFileToConversationId(String id, Stream stream, String filename, UrgencyType urgencyType, Action<SdkResult<FileDescriptor>> callbackFileDescriptor, Action<SdkResult<Message>> callbackMessage)
        {

        }

        virtual public void SendIsTypingInConversationById(String id, Boolean isTyping)
        {
        }

        virtual public void MarkMessageAsRead(string conversationId, string messageId)
        {
        }

        virtual public void MarkAllMessagesAsRead(String id)
        {
        }

        virtual public List<Rainbow.Model.Message> GetAllMessagesFromConversationIdFromCache(String id)
        {
            return null;
        }

    #endregion WRAPPER - INSTANT MESSAGING

    #region WRAPPER - CONVERSATIONS

        virtual public void GetAllConversations(Action<SdkResult<List<Conversation>>> callback)
        {
        }

        virtual public List<Conversation> GetAllConversationsFromCache()
        {
            return null;
        }

        virtual public Rainbow.Model.Conversation GetConversationByIdFromCache(String conversationId)
        {
            return null;
        }

        virtual public Rainbow.Model.Conversation GetConversationByPeerIdFromCache(String peerId)
        {
            return null;
        }

        virtual public Rainbow.Model.Conversation GetOrCreateConversationFromUserId(string userId)
        {
            return null;
        }

        virtual public Rainbow.Model.Conversation GetOrCreateConversationFromBubbleId(string bubbleId)
        {
            return null;
        }

    #endregion WRAPPER - CONVERSATIONS

    #region WRAPPER - BUBBLES

        virtual public void GetAllBubbles(Action<SdkResult<List<Bubble>>> callback = null)
        {
        }

    #endregion WRAPPER - BUBBLES

    #region WRAPPER - CONTACTS

        virtual public String GetCurrentContactId()
        {
            return null;
        }
        
        virtual public String GetCurrentContactJid()
        {
            return null;
        }

        virtual public List<Contact> GetAllContactsFromCache()
        {
            return null;
        }

        virtual public Rainbow.Model.Contact GetContactFromContactId(String peerId)
        {
            return null;
        }

        virtual public Rainbow.Model.Contact GetContactFromContactJid(String jid)
        {
            return null;
        }

        virtual public Rainbow.Model.Presence GetAggregatedPresenceFromContactId(String peerId)
        {
            return null;
        }

        virtual public void GetContactFromContactIdFromServer(String peerId, Action<SdkResult<Contact>> callback = null)
        {
        }

    #endregion WRAPPER - CONTACTS

#endregion PUBLIC API
    }
}
