﻿using System;
using System.Collections.Generic;

using Rainbow;
using Rainbow.Common;
using static Rainbow.Common.Avatars;
using Rainbow.Model;

using MultiPlatformApplication.Helpers;

using NLog;
using System.IO;

namespace MultiPlatformApplication
{
    public class RainbowSdkWrapper : SdkWrapper
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(RainbowSdkWrapper));

        // Define all Rainbow objects we use
        internal Rainbow.Application RbApplication = null;
        internal Rainbow.Bubbles RbBubbles = null;
        internal Rainbow.Contacts RbContacts = null;
        internal Rainbow.Conversations RbConversations = null;
        internal Rainbow.InstantMessaging RbInstantMessaging = null;
        internal Rainbow.FileStorage RbFileStorage = null;

        internal Rainbow.Common.Avatars RbAvatars = null;
        internal Rainbow.Common.Files RbFiles = null;

        public RainbowSdkWrapper()
        {
            RbApplication = new Rainbow.Application(Helper.GetAppFolderPath());
            RbApplication.SetTimeout(10000);

            RbApplication.SetApplicationInfo(ApplicationInfo.APP_ID, ApplicationInfo.APP_SECRET_KEY);
            RbApplication.SetHostInfo(ApplicationInfo.HOST_NAME);

            RbBubbles = RbApplication.GetBubbles();
            RbContacts = RbApplication.GetContacts();
            RbConversations = RbApplication.GetConversations();
            RbInstantMessaging = RbApplication.GetInstantMessaging();
            RbFileStorage = RbApplication.GetFileStorage();

            // Manage "Instant Messaging" service events
            RbInstantMessaging.MessageReceived += RbInstantMessaging_MessageReceived;
            RbInstantMessaging.ReceiptReceived += RbInstantMessaging_ReceiptReceived;
            RbInstantMessaging.MessagesAllRead += RbInstantMessaging_MessagesAllRead;
            RbInstantMessaging.UserTypingChanged += RbInstantMessaging_UserTypingChanged;

            // Manage "Contacts" service events
            RbContacts.ContactAdded += RbContacts_ContactAdded;
            RbContacts.ContactInfoChanged += RbContacts_ContactInfoChanged;
            RbContacts.ContactPresenceChanged += RbContacts_ContactPresenceChanged;

            // Manage "Conversations" service events
            RbConversations.ConversationCreated += RbConversations_ConversationCreated;
            RbConversations.ConversationRemoved += RbConversations_ConversationRemoved;
            RbConversations.ConversationUpdated += RbConversations_ConversationUpdated;

            // Manage "Bubbles" service events
            RbBubbles.BubbleInfoUpdated += RbBubbles_BubbleInfoUpdated;

            // Manage "Application" service events
            RbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;
            RbApplication.InitializationPerformed += RbApplication_InitializationPerformed;

            InitAvatarPool();
            InitFilePool();
        }

#region EVENTS FROM SDK

        private void FilePool_ThumbnailAvailable(object sender, Rainbow.Events.IdEventArgs e)
        {
            OnThumbnailAvailable(sender, e);
        }

        private void FilePool_FileDescriptorAvailable(object sender, Rainbow.Events.IdEventArgs e)
        {
            OnFileDescriptorAvailable(sender, e);
        }

        private void RbAvatars_BubbleAvatarUpdated(object sender, Rainbow.Events.IdEventArgs e)
        {
            OnBubbleAvatarUpdated(sender, e);
        }

        private void RbAvatars_ContactAvatarUpdated(object sender, Rainbow.Events.IdEventArgs e)
        {
            OnContactAvatarUpdated(sender, e);
        }

        private void RbApplication_InitializationPerformed(object sender, EventArgs e)
        {
            OnInitializationPerformed(sender, e);
        }

        private void RbApplication_ConnectionStateChanged(object sender, Rainbow.Events.ConnectionStateEventArgs e)
        {
            OnConnectionStateChanged(sender, e);
        }


        private void RbBubbles_BubbleInfoUpdated(object sender, Rainbow.Events.BubbleInfoEventArgs e)
        {
            OnBubbleInfoUpdated(sender, e);
        }


        private void RbConversations_ConversationUpdated(object sender, Rainbow.Events.ConversationEventArgs e)
        {
            OnConversationUpdated(sender, e);
        }

        private void RbConversations_ConversationRemoved(object sender, Rainbow.Events.ConversationEventArgs e)
        {
            OnConversationRemoved(sender, e);
        }

        private void RbConversations_ConversationCreated(object sender, Rainbow.Events.ConversationEventArgs e)
        {
            OnConversationCreated(sender, e);
        }


        private void RbContacts_ContactAdded(object sender, Rainbow.Events.JidEventArgs e)
        {
            OnContactAdded(sender, e);
        }

        private void RbContacts_ContactInfoChanged(object sender, Rainbow.Events.JidEventArgs e)
        {
            OnContactInfoChanged(sender, e);
        }

        private void RbContacts_ContactPresenceChanged(object sender, Rainbow.Events.PresenceEventArgs e)
        {
            OnContactPresenceChanged(sender, e);
        }


        private void RbInstantMessaging_UserTypingChanged(object sender, Rainbow.Events.UserTypingEventArgs e)
        {
            OnUserTypingChanged(sender, e);
        }

        private void RbInstantMessaging_MessagesAllRead(object sender, Rainbow.Events.IdEventArgs e)
        {
            OnMessagesAllRead(sender, e);
        }

        private void RbInstantMessaging_ReceiptReceived(object sender, Rainbow.Events.ReceiptReceivedEventArgs e)
        {
            OnReceiptReceived(sender, e);
        }

        private void RbInstantMessaging_MessageReceived(object sender, Rainbow.Events.MessageEventArgs e)
        {
            OnMessageReceived(sender, e);
        }

#endregion EVENTS FROM SDK

#region PUBLIC API

    #region WRAPPER - FILES

        public override int MaxThumbnailWidth 
        {
            get { return RbFiles.MaxThumbnailWidth; }
            set { RbFiles.MaxThumbnailWidth = value;  }
        }

        public override int MaxThumbnailHeight
        {
            get { return RbFiles.MaxThumbnailHeight; }
            set { RbFiles.MaxThumbnailHeight = value; }
        }

        public override Boolean IsThumbnailFileAvailable(String conversationId, String fileDescriptorId, String fileName)
        {
            return RbFiles.IsThumbnailFileAvailable(conversationId, fileDescriptorId, fileName) ;
        }

        public override void AskFileDescriptorDownload(String conversationId, String fileDescriptorId)
        {
            RbFiles.AskFileDescriptorDownload(conversationId, fileDescriptorId);
        }

        public override String GetThumbnailFullFilePath(String fileDescriptorId)
        {
            return RbFiles.GetThumbnailFullFilePath(fileDescriptorId);
        }

        public override String GetConversationIdByFileDescriptorId(String fileDescriptorId)
        {
            return RbFiles.GetConversationIdByFileDescriptorId(fileDescriptorId);
        }

    #endregion WRAPPER - FILES


    #region WRAPPER - AVATARS

        public override void AddUnknownContactToPoolByJid(String contactJid)
        {
            RbAvatars.AddUnknownContactToPoolByJid(contactJid);
        }

        public override String GetBubbleAvatarPath(String bubbleId, AvatarType avatarType = AvatarType.ROUNDED)
        {
            
            return RbAvatars.GetBubbleAvatarPath(bubbleId, avatarType);
        }

        public override String GetContactAvatarPath(String contactId, AvatarType avatarType = AvatarType.ROUNDED)
        {
            return RbAvatars.GetContactAvatarPath(contactId, avatarType);
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
            return RbInstantMessaging.GetOneMessageFromConversationIdFromCache(conversationId, messageId);
        }

        public override void GetOneMessageFromConversationId(String conversationId, String messageId, String stamp, Action<SdkResult<Message>> callback)
        {
            RbInstantMessaging.GetOneMessageFromConversationId(conversationId, messageId, stamp, callback);
        }

        public override void GetMessagesFromConversationId(string conversationId, int nbMessages, Action<SdkResult<List<Message>>> callback)
        {
            RbInstantMessaging.GetMessagesFromConversationId(conversationId, nbMessages, callbackInstantMessaging =>
            {
                if (ApplicationInfo.DataStorageUsed)
                {
                    // We store messages for this conversation (on success)
                    if (callbackInstantMessaging.Result.Success)
                        DataStorage.StoreMessages(conversationId, callbackInstantMessaging.Data);

                    // We also store global info
                    StoreGlobalInfo();
                }

                callback?.Invoke(callbackInstantMessaging);
            });

            
        }

        public override void SendMessageToConversationId(String id, String content, UrgencyType urgencyType)
        {
            RbInstantMessaging.SendMessageToConversationId(id, content, null, urgencyType);
        }

        public override void SendIsTypingInConversationById(String id, Boolean isTyping)
        {
            RbInstantMessaging.SendIsTypingInConversationById(id, isTyping, null);
        }

        public override void MarkMessageAsRead(string conversationId, string messageId)
        {
            RbInstantMessaging.MarkMessageAsRead(conversationId, messageId, null);
        }

        public override void MarkAllMessagesAsRead(String id)
        {
            RbInstantMessaging.MarkAllMessagesAsRead(id, null);
        }

        public override List<Rainbow.Model.Message> GetAllMessagesFromConversationIdFromCache(String id)
        {
            return RbInstantMessaging.GetAllMessagesFromConversationIdFromCache(id);
        }

    #endregion WRAPPER - INSTANT MESSAGING


    #region WRAPPER - CONVERSATIONS

        public override void GetAllConversations(Action<SdkResult<List<Conversation>>> callback)
        {
            RbConversations.GetAllConversations(callbackConversations =>
            {
                if (ApplicationInfo.DataStorageUsed)
                {
                    if (callbackConversations.Result.Success)
                        DataStorage.StoreConversations(callbackConversations.Data);

                    // We also store global info
                    StoreGlobalInfo();
                }
                callback?.Invoke(callbackConversations);
            });
        }

        public override List<Conversation> GetAllConversationsFromCache()
        {
            return RbConversations.GetAllConversationsFromCache();
        }

        public override Rainbow.Model.Conversation GetConversationByIdFromCache(String id)
        {
            return RbConversations.GetConversationByIdFromCache(id);
        }

    #endregion WRAPPER - CONVERSATIONS


    #region WRAPPER - BUBBLES

        public override void GetAllBubbles(Action<SdkResult<List<Bubble>>> callback = null)
        {
            RbBubbles.GetAllBubbles(callbackBubbles =>
            {
                if (ApplicationInfo.DataStorageUsed && callbackBubbles.Result.Success)
                    DataStorage.StoreBubbles(callbackBubbles.Data);

                callback?.Invoke(callbackBubbles);
            });
        }

    #endregion WRAPPER - BUBBLES


    #region WRAPPER - CONTACTS

        public override String GetCurrentContactId()
        {
            return RbContacts.GetCurrentContactId();
        }

        public override string GetCurrentContactJid()
        {
            return RbContacts.GetCurrentContactJid();
        }

        public override Rainbow.Model.Contact GetContactFromContactId(String peerId)
        {
            return RbContacts.GetContactFromContactId(peerId);
        }

        public override Rainbow.Model.Contact GetContactFromContactJid(String jid)
        {
            return RbContacts.GetContactFromContactJid(jid);
        }

        public override Rainbow.Model.Presence GetAggregatedPresenceFromContactId(String peerId)
        {
            return RbContacts.GetAggregatedPresenceFromContactId(peerId);
        }

        public override void GetContactFromContactIdFromServer(String peerId, Action<SdkResult<Contact>> callback = null)
        {
            RbContacts.GetContactFromContactIdFromServer(peerId, null);
        }

    #endregion WRAPPER - CONTACTS


    #region WRAPPER - APPLICATION

        public override String GetUserLoginFromCache()
        {
            return RbApplication.GetUserLoginFromCache();
        }

        public override String GetUserPasswordFromCache()
        {
            return RbApplication.GetUserPasswordFromCache(); ;
        }

        public override String ConnectionState()
        {
            return RbApplication.ConnectionState(); ;
        }

        public override void Login(string login, string password, Action<SdkResult<Boolean>> callback = null)
        {
            RbApplication.Login(login, password, callback);
        }

        public override void Logout(Action<SdkResult<Boolean>> callback = null)
        {
            RbApplication.Logout(callback);
        }

    #endregion WRAPPER - APPLICATION


#endregion PUBLIC API

#region PRIVATE API

        private void StoreGlobalInfo()
        {
            if (!ApplicationInfo.DataStorageUsed)
                return;

            // We store a snapshot of presences
            DataStorage.StoreAggregatedPresences(RbContacts.GetAllAggregatedPresences());
            DataStorage.StorePresences(RbContacts.GetAllPresences());

            // We store a snapshot of Contacts
            DataStorage.StoreContacts(RbContacts.GetAllContactsFromCache());
            DataStorage.StoreCurrentContact(RbContacts.GetCurrentContact());

            // We store a snapshot of Bubbles
            DataStorage.StoreBubbles(RbBubbles.GetAllBubblesFromCache());

            // We store a snapshot of Files Descrpitor
            DataStorage.StoreFilesDescriptor(RbFiles.GetAllFilesDescriptorFromCache());
            DataStorage.StoreConversationsByFilesDescriptor(RbFiles.GetAllConversationIdByFilesDescriptorIdFromCache());
        }

        private void InitAvatarPool()
        {
            // Get AvatarPool and initialize it
            string avatarsFolderPath = Helper.GetAvatarsFolderPath();

            // FOR TEST PURPOSE ONLY
            //if (Directory.Exists(avatarsFolderPath))
            //    Directory.Delete(avatarsFolderPath, true);

            // We create Avatars service
            RbAvatars = Avatars.Instance;
            RbAvatars.SetApplication(RbApplication);
            RbAvatars.SetDensity((float)Helper.GetDensity());
            RbAvatars.SetFolderPathUsedToStoreAvatars(avatarsFolderPath);
            RbAvatars.AllowAvatarDownload(true);
            RbAvatars.AllowToAskInfoForUnknownContact(true);
            RbAvatars.AllowToAskInfoAboutUnknowBubble(true);

            RbAvatars.ContactAvatarUpdated += RbAvatars_ContactAvatarUpdated;
            RbAvatars.BubbleAvatarUpdated += RbAvatars_BubbleAvatarUpdated;

            if (!RbAvatars.Initialize())
                log.Error("CANNOT initialize Avatars service ...");
        }

        private void InitFilePool()
        {
            // Get FilePool and initialize it
            string filePoolFolderPath = Helper.GetFileStorageFolderPath();

            RbFiles = Rainbow.Common.Files.Instance;

            RbFiles.SetDensity((float)Helper.GetDensity());
            RbFiles.SetFolderPath(filePoolFolderPath);
            RbFiles.SetApplication(ref RbApplication);

            RbFiles.AllowAutomaticThumbnailDownload = true;
            RbFiles.AutomaticDownloadLimitSizeForImages = 250 * 1024; // 250 Ko

            RbFiles.FileDescriptorAvailable += FilePool_FileDescriptorAvailable;
            RbFiles.ThumbnailAvailable += FilePool_ThumbnailAvailable;

        }

#endregion PRIVATE API

    }
}