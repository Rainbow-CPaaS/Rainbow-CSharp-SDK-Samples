using System;
using System.Collections.Generic;
using System.Threading;

using Xamarin.Forms;

using MvvmHelpers;

using Rainbow;
using Rainbow.Helpers;
using Rainbow.Model;

using InstantMessaging.Helpers;

using log4net;
using System.IO;


namespace InstantMessaging
{
    public class ConversationStreamViewModel : BaseViewModel
    {
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(ConversationStreamViewModel));

        private String peerId; // PeerId of this Conversation
        private String conversationId; // ConversationId of this Conversation
        private Conversation rbConversation = null; // Rainbow Conversation object

        private AvatarPool avatarPool;
        private InstantMessaging.App XamarinApplication;


        private List<String> contactsListInvolved = new List<String>(); // To store list of contacts involved in this Conversation

        private Object lockObservableMessagesList = new Object(); // To lock access to the observable collection: 'MessagesList'

        public ConversationLight Conversation { get; } // Need to be public - Used as Binding from XAML
        public ObservableRangeCollection<InstantMessaging.Model.Message> MessagesList { get; } // Need to be public - Used as Binding from XAML

        /// <summary>
        /// Constructor
        /// </summary>
        public ConversationStreamViewModel(String peerId)
        {
            // Get Xamarin Application
            XamarinApplication = (InstantMessaging.App)Xamarin.Forms.Application.Current;

            // Manage event(s) from AvatarPool
            avatarPool = AvatarPool.Instance;
            avatarPool.ContactAvatarChanged += AvatarPool_ContactAvatarChanged;
            avatarPool.BubbleAvatarChanged += AvatarPool_BubbleAvatarChanged;

            // Manage event(s) from InstantMessaging
            XamarinApplication.RbInstantMessaging.MessageReceived += RbInstantMessaging_MessageReceived;

            // Manage event(s) from Contacts
            XamarinApplication.RbContacts.ContactAdded += RbContacts_ContactAdded;
            XamarinApplication.RbContacts.ContactInfoChanged += RbContacts_ContactInfoChanged;

            // Store PeerID and Get conversation Id
            this.peerId = peerId;
            conversationId = XamarinApplication.RbConversations.GetConversationIdByPeerIdFromCache(peerId);

            // Create default MessagesList  object
            MessagesList = new ObservableRangeCollection<InstantMessaging.Model.Message>();

            if (String.IsNullOrEmpty(conversationId))
            {
                //TODO
                Conversation = new ConversationLight();
            }
            else
            {
                // Get Rainbow Conversation object
                rbConversation = XamarinApplication.RbConversations.GetConversationByIdFromCache(conversationId);

                // Get Conversation Model Object using Rainbow Conversation
                Conversation = Helper.GetConversationFromRBConversation(rbConversation);
                if (Conversation != null)
                    Conversation.AvatarSource = Helper.GetConversationAvatarImageSource(Conversation);

                // Get Messages
                List<Rainbow.Model.Message> rbMessagesList = XamarinApplication.RbInstantMessaging.GetAllMessagesFromConversationIdFromCache(conversationId);
                if(rbMessagesList.Count > 0)
                {
                    ResetModelWithRbMessages(rbMessagesList);
                }
                else
                {
                    XamarinApplication.RbInstantMessaging.GetMessagesFromConversationId(conversationId, 20, callback =>
                    {
                        if(callback.Result.Success)
                        {
                            ResetModelWithRbMessages(callback.Data);
                        }
                        else
                        {
                            //TODO
                        }
                    });
                }
            }
        }

#region PRIVATE METHOD

        private void AddContactInvolved(String peerId)
        {
            if (!contactsListInvolved.Contains(peerId))
            {
                log.DebugFormat("[AddContactInvolved] - ContactId:[{0}]", peerId);
                contactsListInvolved.Add(peerId);
            }
        }

        private void ResetModelWithRbMessages(List<Rainbow.Model.Message> rbMessagesList)
        {
            List<InstantMessaging.Model.Message> messagesList = new List<InstantMessaging.Model.Message>();
            foreach (Rainbow.Model.Message rbMessage in rbMessagesList)
            {
                InstantMessaging.Model.Message msg = Helper.GetMessageFromRBMessage(rbMessage, rbConversation.Type);
                if (msg != null)
                {
                    messagesList.Insert(0, msg);
                    msg.AvatarSource = Helper.GetContactAvatarImageSource(msg.PeerId);
                    AddContactInvolved(msg.PeerId);
                }
            }

            lock(lockObservableMessagesList)
                MessagesList.ReplaceRange(messagesList);
        }

        private List<InstantMessaging.Model.Message> GetMessagesByPeerId(String peerId)
        {
            List<InstantMessaging.Model.Message> result = new List<InstantMessaging.Model.Message>();
            lock (lockObservableMessagesList)
            {
                foreach(InstantMessaging.Model.Message msg  in MessagesList)
                {
                    if (msg.PeerId == peerId)
                        result.Add(msg);
                }
            }
            return result;
        }

#endregion PRIVATE METHOD

#region EVENTS FROM RAINBOW SDK
        private void RbInstantMessaging_MessageReceived(object sender, Rainbow.Events.MessageEventArgs e)
        {
            //TODO
        }

        private void RbContacts_ContactInfoChanged(object sender, Rainbow.Events.JidEventArgs e)
        {
            Contact contact = XamarinApplication.RbContacts.GetContactFromContactJid(e.Jid);
            if (contact != null)
            {
                log.DebugFormat("[RbContacts_ContactInfoChanged] contactId:[{0}]", contact.Id);
                if (contactsListInvolved.Contains(contact.Id))
                {
                    log.DebugFormat("[RbContacts_ContactInfoChanged] contactId:[{0}] - Found in conversation stream", contact.Id);
                    String displayName = Util.GetContactDisplayName(contact, AvatarPool.Instance.GetFirstNameFirst());

                    List<InstantMessaging.Model.Message> messages = GetMessagesByPeerId(contact.Id);
                    lock (lockObservableMessagesList)
                    {
                        foreach (InstantMessaging.Model.Message message in messages)
                        {
                            log.DebugFormat("[RbContacts_ContactInfoChanged] contactId:[{0}] - Found in conversation stream - update messageId:[{1}]", contact.Id, message.Id);
                            message.PeerDisplayName = displayName;
                            message.BackgroundColor = Rainbow.Helpers.AvatarPool.GetColorFromDisplayName(message.PeerDisplayName);
                        }
                    }
                }
            }
        }

        private void RbContacts_ContactAdded(object sender, Rainbow.Events.JidEventArgs e)
        {
            Contact contact = XamarinApplication.RbContacts.GetContactFromContactJid(e.Jid);
            if (contact != null)
            {
                log.DebugFormat("[RbContacts_ContactAdded] contactId:[{0}]", contact.Id);
                if (contactsListInvolved.Contains(contact.Id))
                {
                    log.DebugFormat("[RbContacts_ContactAdded] contactId:[{0}] - Found in conversation stream ", contact.Id);
                    ImageSource imageSource = Helper.GetContactAvatarImageSource(contact.Id);
                    String displayName = Util.GetContactDisplayName(contact, AvatarPool.Instance.GetFirstNameFirst());

                    List<InstantMessaging.Model.Message> messages = GetMessagesByPeerId(contact.Id);
                    lock (lockObservableMessagesList)
                    {
                        foreach (InstantMessaging.Model.Message message in messages)
                        {
                            log.DebugFormat("[RbContacts_ContactAdded] contactId:[{0}] - Found in conversation stream - update messageId:[{1}]", contact.Id, message.Id);
                            message.AvatarSource = imageSource;
                            message.PeerDisplayName = displayName;
                            message.BackgroundColor = Rainbow.Helpers.AvatarPool.GetColorFromDisplayName(message.PeerDisplayName);
                            
                        }
                    }
                }
            }
        }

#endregion EVENTS FROM RAINBOW SDK

#region EVENTS FROM AVATAR POOL

        private void AvatarPool_BubbleAvatarChanged(object sender, Rainbow.Events.IdEventArgs e)
        {
            if( (rbConversation != null) 
                && (rbConversation.Type == Rainbow.Model.Conversation.ConversationType.Room)
                && (rbConversation.Id == e.Id) )
            {
                if (Conversation != null)
                    Conversation.AvatarSource = Helper.GetConversationAvatarImageSource(Conversation);
            }
        }

        private void AvatarPool_ContactAvatarChanged(object sender, Rainbow.Events.IdEventArgs e)
        {
            // Check Conversation Avatar
            if ((rbConversation != null)
                && (rbConversation.Type == Rainbow.Model.Conversation.ConversationType.User)
                && (rbConversation.Id == e.Id))
            {
                if (Conversation != null)
                    Conversation.AvatarSource = Helper.GetConversationAvatarImageSource(Conversation);
            }

            // Check Avatar in Messages Stream
            if (contactsListInvolved.Contains(e.Id))
            {
                ImageSource imageSource = Helper.GetContactAvatarImageSource(e.Id);
                if (imageSource != null)
                {
                    List<InstantMessaging.Model.Message> messages = GetMessagesByPeerId(e.Id);
                    lock (lockObservableMessagesList)
                    {
                        foreach (InstantMessaging.Model.Message msg in messages)
                        {
                            msg.AvatarSource = imageSource;
                        }
                    }
                }
            }
        }

#endregion EVENTS FROM AVATAR POOL
    }
}
