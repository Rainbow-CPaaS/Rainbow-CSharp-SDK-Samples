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
using System.Threading.Tasks;

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


        private Object lockObservableMessagesList = new Object(); // To lock access to the observable collection: 'MessagesList'
        private Object lockContactsListInvolved = new Object(); // To lock access to: 'contactsListInvolved'

        private List<String> contactsListInvolved = new List<String>(); // To store list of contacts involved in this Conversation (by Jid)


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
                if (rbMessagesList.Count > 0)
                {
                    ResetModelWithRbMessages(rbMessagesList);
                }
                else
                {
                    Task task = new Task(() =>
                        XamarinApplication.RbInstantMessaging.GetMessagesFromConversationId(conversationId, 20, callback =>
                        {
                            if (callback.Result.Success)
                            {
                                ResetModelWithRbMessages(callback.Data);
                            }
                            else
                            {
                                //TODO
                            }
                        })
                    );
                    task.Start();
                }
            }
        }

#region PRIVATE METHOD

        private void AddContactInvolved(String peerJid)
        {
            lock (lockContactsListInvolved)
            {
                if (!contactsListInvolved.Contains(peerJid))
                {
                    //log.DebugFormat("[AddContactInvolved] - ContactJid:[{0}]", peerJid);
                    contactsListInvolved.Add(peerJid);
                }
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
                    AddContactInvolved(msg.PeerJid);
                }
            }
            lock (lockObservableMessagesList)
                MessagesList.ReplaceRange(messagesList);

        }

        private List<InstantMessaging.Model.Message> GetMessagesByPeerJid(String peerJid)
        {
            List<InstantMessaging.Model.Message> result = new List<InstantMessaging.Model.Message>();
            lock (lockObservableMessagesList)
            {
                foreach(InstantMessaging.Model.Message msg  in MessagesList)
                {
                    if (msg.PeerJid == peerJid)
                        result.Add(msg);
                }
            }
            return result;
        }

        private void UpdateMessagesForJid(String peerJid, bool updateAvatar, bool updateDisplayName )
        {
            if (contactsListInvolved.Contains(peerJid))
            {
                Contact contact = XamarinApplication.RbContacts.GetContactFromContactJid(peerJid);
                if (contact != null)
                {
                    log.DebugFormat("[UpdateMessagesForJid] peerJid:[{0}] - peerId:[{1}] - updateAvatar[{2}] - updateDisplayName:[{3}]", peerJid, contact.Id, updateAvatar, updateDisplayName);
                    ImageSource imageSource;
                    String displayName;
                    
                    if (updateAvatar)
                        imageSource = Helper.GetContactAvatarImageSource(contact.Id);
                    else
                        imageSource = null;

                    if (updateDisplayName)
                        displayName = Util.GetContactDisplayName(contact, AvatarPool.Instance.GetFirstNameFirst());
                    else
                        displayName = null;

                    List<InstantMessaging.Model.Message> messages = GetMessagesByPeerJid(peerJid);
                    lock (lockObservableMessagesList)
                    {
                        foreach (InstantMessaging.Model.Message message in messages)
                        {
                            if (updateAvatar)
                                message.AvatarSource = imageSource;

                            if (updateDisplayName)
                            {
                                message.PeerDisplayName = displayName;
                                message.BackgroundColor = Rainbow.Helpers.AvatarPool.GetColorFromDisplayName(message.PeerDisplayName);
                            }

                        }
                    }
                }
                else
                    log.WarnFormat("[UpdateMessagesForJid] peerJid:[{0}] found but related contact not found", peerJid);
            }
            else
                log.WarnFormat("[UpdateMessagesForJid] peerJid:[{0}] not found in this conversationId:[{1}]", peerJid, peerId);
        }

#endregion PRIVATE METHOD

#region EVENTS FROM RAINBOW SDK
        private void RbInstantMessaging_MessageReceived(object sender, Rainbow.Events.MessageEventArgs e)
        {
            //TODO
        }

        private void RbContacts_ContactInfoChanged(object sender, Rainbow.Events.JidEventArgs e)
        {
            // Need to update this contact in each messages (not avatar part)
            UpdateMessagesForJid(e.Jid, false, true);
        }

        private void RbContacts_ContactAdded(object sender, Rainbow.Events.JidEventArgs e)
        {
            // Need to update this contact in each messages (avatar + display name)
            UpdateMessagesForJid(e.Jid, true, true);
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

            // Need to update this contact in each messages (just avatar part)
            Contact contact = XamarinApplication.RbContacts.GetContactFromContactId(e.Id);
            if (contact != null)
                UpdateMessagesForJid(contact.Jid_im, true, false);
        }

#endregion EVENTS FROM AVATAR POOL
    }
}
