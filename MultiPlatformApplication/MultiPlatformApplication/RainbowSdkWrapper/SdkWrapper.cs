﻿using System;
using System.Collections.Generic;

using Rainbow;
using static Rainbow.Common.Avatars;
using Rainbow.Events;
using Rainbow.Model;

namespace MultiPlatformApplication
{
    public class SdkWrapper
    {

#region PUBLIC EVENT

        // From "Files" Service
        public event EventHandler<IdEventArgs> ThumbnailAvailable;
        public event EventHandler<IdEventArgs> FileDescriptorAvailable;

        // From "Avatars" Service
        public event EventHandler<IdEventArgs> ContactAvatarUpdated;
        public event EventHandler<IdEventArgs> BubbleAvatarUpdated;

        // From "Instant Messaging" Service
        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler<UserTypingEventArgs> UserTypingChanged;
        public event EventHandler<ReceiptReceivedEventArgs> ReceiptReceived;
        public event EventHandler<IdEventArgs> MessagesAllRead;
        public event EventHandler<IdEventArgs> MessagesAllDeleted;

        // From "Contacts" Service
        public event EventHandler<JidEventArgs> ContactAdded;
        public event EventHandler<JidEventArgs> ContactInfoChanged;
        public event EventHandler<PresenceEventArgs> ContactPresenceChanged;

        // From "Conversations" Service
        public event EventHandler<ConversationEventArgs> ConversationCreated;
        public event EventHandler<ConversationEventArgs> ConversationRemoved;
        public event EventHandler<ConversationEventArgs> ConversationUpdated;

        // From "Bubbles" Service
        public event EventHandler<BubbleInfoEventArgs> BubbleInfoUpdated;

        // From "Application" Service
        public event EventHandler<ConnectionStateEventArgs> ConnectionStateChanged;
        public event EventHandler<EventArgs> InitializationPerformed;
#endregion PUBLIC EVENT



#region TO RAISE EVENT

    #region AVATARS SERVICE

        internal void OnThumbnailAvailable(object sender, IdEventArgs args)
        {
            ThumbnailAvailable.Raise(sender, args);
        }

        internal void OnFileDescriptorAvailable(object sender, IdEventArgs args)
        {
            FileDescriptorAvailable.Raise(sender, args);
        }

    #endregion AVATARS SERVICE


    #region AVATARS SERVICE

        internal void OnContactAvatarUpdated(object sender, IdEventArgs args)
        {
            ContactAvatarUpdated.Raise(sender, args);
        }

        internal void OnBubbleAvatarUpdated(object sender, IdEventArgs args)
        {
            BubbleAvatarUpdated.Raise(sender, args);
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

        internal void OnContactAdded(object sender, JidEventArgs args)
        {
            ContactAdded.Raise(sender, args);
        }

        internal void OnContactInfoChanged(object sender, JidEventArgs args)
        {
            ContactInfoChanged.Raise(sender, args);
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


    #region WRAPPER - FILES

        virtual public int MaxThumbnailWidth { get; set; }

        virtual public int MaxThumbnailHeight { get; set; }

        virtual public Boolean IsThumbnailFileAvailable(String conversationId, String fileDescriptorId, String fileName)
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

        virtual public void SendMessageToConversationId(String id, String content, UrgencyType urgencyType)
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

        virtual public Rainbow.Model.Conversation GetConversationByIdFromCache(String id)
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


    #region WRAPPER - APPLICATION

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

#endregion PUBLIC API
    }
}