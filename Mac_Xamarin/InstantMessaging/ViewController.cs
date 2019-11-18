using System;
using System.Collections.Generic;
using System.IO;
using AppKit;
using Foundation;
using Rainbow;
using Rainbow.Events;
using Rainbow.Model;

namespace SampleInstantMessaging
{
    public partial class ViewController : NSViewController
    {

        private static readonly log4net.ILog log = Rainbow.LogConfigurator.GetLogger(typeof(ViewController));

        const string APP_ID = "YOUR_APP_ID";
        const string APP_SECRET_KEY = "YOUR_APP_SECRET_KEY";
        const string HOSTNAME = "sandbox.openrainbow.com";

        const string LOGIN = "YOUR_LOGIN";
        const string PASSWORD = "YOUR_PASSWORD";

        // Define Rainbow objects
        Application rainbowApplication;                 // To store Rainbow Application object
        Rainbow.Contacts rainbowContacts;               // To store Rainbow Contacts object
        Conversations rainbowConversations;             // To store Rainbow Conversations object
        InstantMessaging rainbowInstantMessaging;       // To store Rainbow InstantMessaging object

        Contact rainbowMyContact;               // To store My contact (i. e. the one connected to the Rainbow Server)
        List<Contact> rainbowContactsList;      // To store contacts list of my roster
        List<Conversation> rainbowConvList;     // To store conversations list

        // Facilitator objects for this sample
        bool contactSelected = false;
        String idSelected = null;
        bool selectionCorrect = false;
        String lastMessageIdReceived = null;

        // To associate the combobox index to the id of the contact
        Dictionary<int, string> valuesContact = new Dictionary<int, string>();
        Dictionary<int, string> valuesConversations = new Dictionary<int, string>();

        #region INIT METHODS

        public ViewController(IntPtr handle) : base(handle)
        {
            InitializeLog();

            InitializeRainbowSDK();
        }

        private void InitializeLog()
        {
            string logPath = @"./../../../../../log4netConfiguration.xml";

            FileInfo fileInfo = new FileInfo(logPath);

            // Does the file really exist ?
            if ((fileInfo != null) && File.Exists(fileInfo.FullName))
            {
                // Get repository object used by the SDK itself
                log4net.Repository.ILoggerRepository repository = Rainbow.LogConfigurator.GetRepository();

                // Configure XMLConfigurator using our XML file for our repository
                log4net.Config.XmlConfigurator.Configure(repository, fileInfo);
            }
            log.Info("==========================================");
            log.Info("SampleConversation.ViewController started");
        }

        private void InitializeRainbowSDK()
        {
            rainbowApplication = new Rainbow.Application();

            // Set Application Id, Secret Key and Host Name
            rainbowApplication.SetApplicationInfo(APP_ID, APP_SECRET_KEY);
            rainbowApplication.SetHostInfo(HOSTNAME);

            // Get Rainbow main objects
            rainbowContacts = rainbowApplication.GetContacts();
            rainbowConversations = rainbowApplication.GetConversations();
            rainbowInstantMessaging = rainbowApplication.GetInstantMessaging();

            // Events we want to manage
            rainbowApplication.ConnectionStateChanged += RainbowApplication_ConnectionStateChanged;

            rainbowContacts.ContactAdded += RainbowContacts_ContactAdded;
            rainbowContacts.ContactRemoved += RainbowContacts_ContactRemoved;
            rainbowContacts.ContactPresenceChanged += RainbowContacts_ContactPresenceChanged;

            rainbowConversations.ConversationCreated += RainbowConversations_ConversationCreated;
            rainbowConversations.ConversationRemoved += RainbowConversations_ConversationRemoved;

            rainbowInstantMessaging.MessageReceived += RainbowInstantMessaging_MessageReceived;
            rainbowInstantMessaging.ReceiptReceived += RainbowInstantMessaging_ReceiptReceived;
            rainbowInstantMessaging.UserTypingChanged += RainbowInstantMessaging_UserTypingChanged;

            rainbowContactsList = new List<Contact>();
            rainbowConvList = new List<Conversation>();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Do any additional setup after loading the view.
            txt_login.StringValue = LOGIN;
            txt_password.StringValue = PASSWORD;
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }

        #endregion INIT METHODS

        #region EVENTS FIRED BY UI

        partial void btnLogin_Clicked(NSObject sender)
        {
            ConnectToRainbow();
        }

        partial void cbPresence_SelectionChanged(NSObject sender)
        {
            int index = (int) cbPresence.IndexOfSelectedItem;

            if(index > -1)
            {
                Presence presence = Util.UnserializePresence(cbPresence.SelectedItem.Title);
                if(presence == null)
                {
                    string logLine = String.Format("Impossible to unserialize presence: [{0}]", cbPresence.SelectedItem.Title);
                    AddStateLine(logLine);
                    log.WarnFormat(logLine);
                    return;
                }

                rainbowContacts.SetPresenceLevel(presence, 5, callback =>
                {
                    if (callback.Result.Success)
                    {
                        AddStateLine($"Presence set to [{cbPresence.SelectedItem.Title}]");
                    } else
                    {
                        string logLine = String.Format("Impossible to set presence:\r\n[{0}]", Util.SerialiseSdkError(callback.Result));
                        AddStateLine(logLine);
                        log.WarnFormat(logLine);
                    }
                });
            }
        }

        partial void cbConversations_SelectionChanged(NSObject sender)
        {
            int index = (int)cbConversations.IndexOfSelectedItem;

            if(index > -1)
            {
                // Unselect contacts list
                cbContacts.SelectItem(-1);

                idSelected = valuesConversations[index];
                contactSelected = false;
                UpdateSelectionInfo();

                ClearMessageStream();
                lastMessageIdReceived = null;
            }
        }

        partial void cbContacts_SelectionChanged(NSObject sender)
        {
            int index = (int)cbContacts.IndexOfSelectedItem;

            if (index > -1)
            {
                // Unselect conversations list
                cbConversations.SelectItem(-1);

                idSelected = valuesContact[index];
                contactSelected = true;
                UpdateSelectionInfo();

                ClearMessageStream();
                lastMessageIdReceived = null;
            }
        }

        partial void btnLoadMessages_Clicked(NSObject sender)
        {
            LoadOlderMessages();
        }

        partial void btnSendMessage_Clicked(NSObject sender)
        {
            if (String.IsNullOrEmpty(txtMessageToSend.StringValue))
                return;

            if (selectionCorrect)
            {
                string textToSend = txtMessageToSend.StringValue;
                txtMessageToSend.StringValue = "";

                if (contactSelected)
                {
                    rainbowInstantMessaging.SendMessageToContactId(idSelected, textToSend, callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddStateLine($"Message sent successfully to contact [{idSelected}]");

                            string msg = $"[{SerializeDateTime(DateTime.Now)}] [YOU]: [{textToSend}]";
                            AddMessageInStream(msg);
                        }
                        else
                        {
                            string logLine = String.Format("Impossible to send message to contact [{1}]:\r\n{0}", Util.SerialiseSdkError(callback.Result), idSelected);
                            AddStateLine(logLine);
                            log.WarnFormat(logLine);
                        }
                    });
                }
                else
                {
                    rainbowInstantMessaging.SendMessageToConversationId(idSelected, textToSend, callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddStateLine($"Message sent successfully to conversation [{idSelected}]");

                            string msg = $"[{SerializeDateTime(DateTime.Now)}] [YOU]: [{textToSend}]";
                            AddMessageInStream(msg);
                        }
                        else
                        {
                            string logLine = String.Format("Impossible to send message to conversation [{1}]:\r\n{0}", Util.SerialiseSdkError(callback.Result), idSelected);
                            AddStateLine(logLine);
                            log.WarnFormat(logLine);
                        }
                    });
                }
            }
        }

        partial void btnIsTyping_CheckedChanged(NSObject sender)
        {
            SendIsTyping();
        }

        partial void txtMessageToSend_TextChanged(NSObject sender)
        {
            SendIsTyping();
        }

        partial void btnMarkLastMessage_Clicked(NSObject sender)
        {
            MarkLastMessage();
        }

        #endregion EVENTS FIRED BY UI

        #region UTILS

        private void ConnectToRainbow()
        {
            if (rainbowApplication.IsConnected())
            {
                rainbowApplication.Logout(callback =>
                {
                    if (!callback.Result.Success)
                    {
                        string logline = String.Format("Impossible to logout:\r\n{0}", Util.SerialiseSdkError(callback.Result));
                        AddStateLine(logline);
                        log.WarnFormat(logline);
                    }
                });
            }
            else
            {
                String login = txt_login.StringValue;
                String password = txt_password.StringValue;

                rainbowApplication.Login(login, password, callback =>
                {
                    if (callback.Result.Success)
                    {
                        rainbowMyContact = rainbowContacts.GetCurrentContact();
                        AddStateLine("Welcome " + rainbowMyContact.DisplayName);
                    }
                    else
                    {
                        string logline = String.Format("Impossible to login:\r\n{0}", Util.SerialiseSdkError(callback.Result));
                        AddStateLine(logline);
                        log.WarnFormat(logline);
                    }
                });
            }
        }

        private void GetAllContacts()
        {
            if (!rainbowApplication.IsConnected())
                return;

            rainbowContacts.GetAllContacts(callback =>
            {
                if (callback.Result.Success)
                {
                    rainbowContactsList = callback.Data;
                    AddStateLine($"Nb contacts in roster: {rainbowContactsList.Count}");
                    UpdateContactsCombobox();
                }
                else
                {
                    string logLine = String.Format("Impossible to get all contacts:\r\n{0}", Util.SerialiseSdkError(callback.Result));
                    AddStateLine(logLine);
                    log.WarnFormat(logLine);
                }
            });
        }

        private void GetAllConversations()
        {
            if (!rainbowApplication.IsConnected())
                return;

            rainbowConversations.GetAllConversations(callback =>
            {
                if (callback.Result.Success)
                {
                    rainbowConvList = callback.Data;
                    AddStateLine($"Nb conversations: {rainbowContactsList.Count}");
                    UpdateConversationsCombobox();
                }
                else
                {
                    string logLine = String.Format("Impossible to get all conversations:\r\n{0}", Util.SerialiseSdkError(callback.Result));
                    AddStateLine(logLine);
                    log.WarnFormat(logLine);
                }
            });
        }

        private void GetAllPresence()
        {
            BeginInvokeOnMainThread(() =>
            {
                cbPresence.RemoveAllItems();

                cbPresence.AddItem("online");
                cbPresence.AddItem("away");
                cbPresence.AddItem("busy-audio");
                cbPresence.AddItem("busy-video");
                cbPresence.AddItem("busy-sharing");
                cbPresence.AddItem("dnd");
                cbPresence.AddItem("dnd-presentation");
                cbPresence.AddItem("xa");
            });
        }

        private string GetContactDisplayName(Contact contact)
        {
            string displayName = null;
            if(contact != null)
            {
                displayName = contact.DisplayName;

                if (String.IsNullOrEmpty(displayName))
                    displayName = $"{contact.FirstName} {contact.LastName}";
            }

            return displayName;
        }

        private void LoadOlderMessages()
        {
            if (selectionCorrect)
            {
                Conversation conversation;
                string contactDisplayName = "";
                if (contactSelected)
                {
                    string conversationId = rainbowConversations.GetConversationIdByPeerIdFromCache(idSelected);
                    conversation = rainbowConversations.GetConversationByIdFromCache(conversationId);

                    if (conversation == null)
                    {
                        AddStateLine($"Conversation not found for this contact: [{idSelected}]");
                        return;
                    }
                    else
                    {
                        Contact contact = rainbowContacts.GetContactFromContactId(idSelected);
                        contactDisplayName = GetContactDisplayName(contact);
                    }
                }
                else
                {
                    conversation = rainbowConversations.GetConversationByIdFromCache(idSelected);
                    if (conversation == null)
                    {
                        AddStateLine($"Conversation not found for this conversation ID: [{idSelected}]");
                        return;
                    }
                    else
                    {
                        contactDisplayName = conversation.Name;
                        if (String.IsNullOrEmpty(contactDisplayName))
                        {
                            contactDisplayName = conversation.Id;
                        }
                    }
                }

                if (conversation != null)
                {
                    rainbowInstantMessaging.GetMessagesFromConversationId(conversation.Id, 20, callback =>
                    {
                        if (callback.Result.Success)
                        {
                            InvokeOnMainThread(() => boxMessages.Hidden = false);
                            List<Rainbow.Model.Message> list = callback.Data;
                            int nb = list.Count;

                            if (nb > 0)
                            {
                                AddStateLine($"We found [{nb}] older messages in this conversation");

                                string msgList = "";
                                string newLine = "\r\n\r\n=========================================================================\r\n";

                                foreach (Message msg in list)
                                    msgList = $"{newLine}{msg.ToString()}" + msgList;

                                AddOlderMessagesInStream(msgList);
                            }
                            else
                            {
                                AddStateLine("There is no more older messages in this conversation");
                            }
                        }
                        else
                        {
                            string logLine = String.Format("Impossible to get older messages from this conversation[{1}]: \r\n{0}", Util.SerialiseSdkError(callback.Result), conversation.Id);
                            AddStateLine(logLine);
                            log.WarnFormat(logLine);
                        }
                    });
                }
            }
        }

        private string SerializeDateTime(DateTime date)
        {
            return date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        private void SendIsTyping()
        {
            var oui = btnIsTyping.State;
            if (selectionCorrect && (btnIsTyping.State == NSCellStateValue.On))
            {
                string conversationId;
                if (contactSelected)
                    conversationId = rainbowConversations.GetConversationIdByPeerIdFromCache(idSelected);
                else
                    conversationId = idSelected;

                if (!String.IsNullOrEmpty(conversationId))
                {
                    rainbowInstantMessaging.SendIsTypingInConversationById(conversationId, !String.IsNullOrEmpty(txtMessageToSend.StringValue), callback =>
                    {
                        if (!callback.Result.Success)
                        {
                            string logLine = String.Format("Impossible to send 'isTyping' to conversation [{1}]:\r\n{0}", Util.SerialiseSdkError(callback.Result), conversationId);
                            AddStateLine(logLine);
                            log.WarnFormat(logLine);
                        }
                    });
                }
            }
        }

        private void MarkLastMessage()
        {
            if(lastMessageIdReceived == null)
            {
                AddStateLine($"There is no message to mark as read");
                return;
            }

            if (selectionCorrect)
            {
                Conversation conversation;
                if (contactSelected)
                {
                    string conversationId = rainbowConversations.GetConversationIdByPeerIdFromCache(idSelected);
                    conversation = rainbowConversations.GetConversationByIdFromCache(conversationId);

                    if (conversation == null)
                    {
                        AddStateLine($"Conversation not found for this contact: [{idSelected}]");
                        return;
                    }
                }
                else
                {
                    conversation = rainbowConversations.GetConversationByIdFromCache(idSelected);
                    if (conversation == null)
                    {
                        AddStateLine($"Conversation not found for this conversation ID: [{idSelected}]");
                        return;
                    }
                }

                if (conversation != null)
                {
                    rainbowInstantMessaging.MarkMessageAsRead(conversation.Id, lastMessageIdReceived, callback =>
                    {
                        if (!callback.Result.Success)
                        {
                            string logLine = String.Format("Impossible to mark message [{1}] as read: \r\n{0}", Util.SerialiseSdkError(callback.Result), lastMessageIdReceived);
                            AddStateLine(logLine);
                            log.WarnFormat(logLine);
                        }
                        else
                        {
                            AddStateLine("Message marked as read");
                        }
                    });
                }

            }
        }

        #endregion

        #region METHODS TO UPDATE UI

        private void AddStateLine(String info)
        {
            InvokeOnMainThread(() => { txt_state.Value = info + "\r\n" + txt_state.Value; });
        }

        private void AddOlderMessagesInStream(string msgList)
        {
            InvokeOnMainThread(() =>
            {
                txtMessagesExchanged.Value = msgList + "\r\n" + txtMessagesExchanged.Value;
            });
        }

        private void AddMessageInStream(string msg)
        {
            InvokeOnMainThread(() =>
            {
                txtMessagesExchanged.Value += "\r\n" + msg;
            });
        }

        private void ClearMessageStream()
        {
            InvokeOnMainThread(() =>
            {
                txtMessagesExchanged.Value = "";
            });
        }

        private void UpdateLoginButton(string connectionState)
        {
            InvokeOnMainThread(() =>
            {
                if (connectionState == ConnectionState.Connected)
                {
                    btnLogin.Title = "Logout";
                    btnLogin.Enabled = true;
                }
                else if (connectionState == ConnectionState.Disconnected)
                {
                    btnLogin.Title = "Login";
                    btnLogin.Enabled = true;
                }
                else if (connectionState == ConnectionState.Connecting)
                {
                    btnLogin.Title = "Connecting";
                    btnLogin.Enabled = false;
                }
            });
        }

        private void UpdateSelectionInfo()
        {
            InvokeOnMainThread(() =>
            {
                bool reset = false;
                string contactId = null;

                if (idSelected == null)
                    reset = true;
                else
                {
                    if (contactSelected)
                    {
                        Contact contact = rainbowContacts.GetContactFromContactId(idSelected);

                        if (contact != null)
                        {
                            string displayName = GetContactDisplayName(contact);
                            txtSelectionInfo.StringValue = $"Contact: [{displayName}]";
                            selectionCorrect = true;

                            contactId = idSelected;

                        }
                        else
                            reset = true;
                    }
                    else
                    {
                        Conversation conversation = rainbowConversations.GetConversationByIdFromCache(idSelected);

                        if (conversation != null)
                        {
                            string displayName;

                            if (conversation.Type == Conversation.ConversationType.User)
                            {
                                Contact contact = rainbowContacts.GetContactFromContactId(conversation.PeerId);
                                contactId = conversation.PeerId;

                                if (contact != null)
                                    displayName = GetContactDisplayName(contact);
                                else
                                    displayName = conversation.PeerId;

                                txtSelectionInfo.StringValue = $"Conversation-User: [{displayName}]";
                                selectionCorrect = true;
                            }
                            else if (conversation.Type == Conversation.ConversationType.Room)
                            {
                                displayName = conversation.Name;
                                txtSelectionInfo.StringValue = $"Conversation-Room: [{displayName}]";
                                selectionCorrect = true;
                            }
                            else if (conversation.Type == Conversation.ConversationType.Bot)
                            {
                                displayName = conversation.Name;
                                if (String.IsNullOrEmpty(displayName))
                                    displayName = conversation.PeerId;
                                txtSelectionInfo.StringValue = $"Conversation-Bot: [{displayName}]";
                                selectionCorrect = true;
                            }
                            else
                            {
                                AddStateLine("Case not managed yet");
                                reset = true;
                            }
                        }
                        else
                        {
                            AddStateLine("Conversation is null");
                            reset = true;
                        }
                    }
                }

                if (reset)
                {
                    selectionCorrect = false;
                    txtSelectionInfo.StringValue = $"No selection";
                    AddStateLine("Reset contact or conversation selection");
                }

                UpdateContactPresence(contactId);
            });
        }

        private void UpdateContactPresence(String contactId)
        {
            InvokeOnMainThread(() =>
            {
                if (contactId != null)
                {
                    // Get and display contact presence
                    Presence presence = rainbowContacts.GetPresenceFromContactId(contactId);
                    if (presence == null) // It means the contact is offline
                        presence = new Presence(PresenceLevel.Offline, "");
                    txtContactPresence.StringValue = Util.SerializePresence(presence);
                }
                else
                {
                    txtContactPresence.StringValue = "No presence in this context";
                }
            });
        }

        private void UpdateContactsCombobox()
        {
            InvokeOnMainThread(() =>
            {
                cbContacts.RemoveAllItems();
                valuesContact.Clear();

                int index = 0;

                foreach (Contact contact in rainbowContactsList)
                {
                    if (contact.Id != rainbowMyContact.Id)
                    {
                        string displayName = contact.DisplayName;

                        if (String.IsNullOrEmpty(displayName))
                        {
                            displayName = $"{contact.FirstName} {contact.LastName}";
                        }

                        valuesContact.Add(index, contact.Id);
                        index++;

                        cbContacts.AddItem(new NSString(displayName));
                    }
                }

                if (cbContacts.IndexOfSelectedItem > 0)
                    cbContacts.SelectItem(0);
            });
        }

        private void UpdateConversationsCombobox()
        {
            InvokeOnMainThread(() =>
            {
                cbConversations.RemoveAllItems();
                valuesConversations.Clear();

                int index = 0;

                foreach (Conversation conv in rainbowConvList)
                {
                    string displayName = conv.Name;

                    if (String.IsNullOrEmpty(displayName))
                    {
                        Contact contact = rainbowContacts.GetContactFromContactId(conv.PeerId);
                        if (contact != null)
                        {
                            displayName = contact.DisplayName;

                            if (String.IsNullOrEmpty(displayName))
                                displayName = $"{contact.FirstName} {contact.LastName}";
                        }
                        else
                        {
                            displayName = conv.Id;
                        }
                    }

                    valuesConversations.Add(index, conv.Id);
                    index++;

                    cbConversations.AddItem(new NSString(displayName));
                }
                if (cbConversations.IndexOfSelectedItem > 0)
                    cbConversations.SelectItem(0);
            });
        }

        #endregion METHOS TO UPDATE UI

        #region EVENTS FIRED BY RAINBOW SDK

        private void RainbowApplication_ConnectionStateChanged(object sender, Rainbow.Events.ConnectionStateEventArgs e)
        {
            // Add info about connection state
            UpdateLoginButton(e.State);
            AddStateLine($"Connection state changed: {e.State}");

            if (e.State == ConnectionState.Connected)
            {
                InvokeOnMainThread(() => box.Hidden = false);

                GetAllContacts();
                GetAllConversations();
                GetAllPresence();

            }
            else if (e.State == ConnectionState.Disconnected)
            {
                InvokeOnMainThread(() =>
                {
                    box.Hidden = true;
                    boxMessages.Hidden = true;
                });

                rainbowContactsList.Clear();
                UpdateContactsCombobox();

                rainbowConvList.Clear();
                UpdateConversationsCombobox();
            }
        }

        private void RainbowContacts_ContactRemoved(object sender, Rainbow.Events.JidEventArgs e)
        {
            AddStateLine($"A contact has been removed from your roster - JID:[{e.Jid}]");
            GetAllContacts();
        }

        private void RainbowContacts_ContactAdded(object sender, Rainbow.Events.JidEventArgs e)
        {
            AddStateLine($"A new contact has been added to your roster - JID:[{e.Jid}]");
            GetAllContacts();
        }

        private void RainbowConversations_ConversationCreated(object sender, Rainbow.Events.ConversationEventArgs e)
        {
            AddStateLine($"A conversation has been created: [{e.Conversation.Id}]");
            GetAllConversations();
        }

        private void RainbowConversations_ConversationRemoved(object sender, Rainbow.Events.ConversationEventArgs e)
        {
            AddStateLine($"A conversation has been removed: [{e.Conversation.Id}]");
            GetAllConversations();
        }

        private void RainbowContacts_ContactPresenceChanged(object sender, PresenceEventArgs e)
        {
            if(e.Jid == rainbowMyContact.Jid_im)
            {
                AddStateLine($"Your presence changed to [{Util.SerializePresence(e.Presence)}]");
            } else
            {
                Contact contact = rainbowContacts.GetContactFromContactJid(e.Jid);
                if (contact == null)
                    AddStateLine($"Presence changed for [{e.Jid}]: {Util.SerializePresence(e.Presence)}");
                else
                {
                    AddStateLine($"Presence changed for [{GetContactDisplayName(contact)}]: {Util.SerializePresence(e.Presence)}");
                    UpdateContactPresence(contact.Id);
                }
            }
        }

        private void RainbowInstantMessaging_UserTypingChanged(object sender, UserTypingEventArgs e)
        {
            bool concernCurrentSelection = false;
            string contactDisplayName = "";

            // Check if event recevied concern the form selection or not
            if (selectionCorrect)
            {
                Conversation conversation = rainbowConversations.GetConversationByIdFromCache(e.ConversationId);
                if (contactSelected)
                {
                    if (conversation != null)
                    {
                        concernCurrentSelection = ((conversation.Type == Conversation.ConversationType.User) && (idSelected == conversation.PeerId));

                        Contact contact = rainbowContacts.GetContactFromContactId(idSelected);
                        contactDisplayName = GetContactDisplayName(contact);
                    }
                    else
                    {
                        AddStateLine($"ContactSelected - No conversation found");
                        return;
                    }
                }
                else
                {
                    concernCurrentSelection = (idSelected == e.ConversationId);
                    if (conversation != null)
                    {
                        if (conversation.Type == Conversation.ConversationType.User)
                        {
                            Contact contact = rainbowContacts.GetContactFromContactId(conversation.PeerId);
                            contactDisplayName = GetContactDisplayName(contact);
                        }
                        else
                        {
                            contactDisplayName = conversation.Name;
                            if (String.IsNullOrEmpty(contactDisplayName))
                                contactDisplayName = conversation.Id;
                        }
                    }
                    else
                    {
                        AddStateLine($"ConversationSelected - No conversation found");
                        return;
                    }
                }

                if (concernCurrentSelection)
                {
                    if (e.IsTyping)
                        AddStateLine($"[{contactDisplayName}] is typing in the CURRENT SELECTED conversation.");
                }
                else
                {
                    if (e.IsTyping)
                        AddStateLine($"[{contactDisplayName}] is typing in ANOTHER conversation.");
                }
            }
        }

        private void RainbowInstantMessaging_ReceiptReceived(object sender, ReceiptReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void RainbowInstantMessaging_MessageReceived(object sender, MessageEventArgs e)
        {
            bool concernCurrentSelection = false;
            string contactDisplayName = "";

            // Check if event received concerns the selection or not
            if (selectionCorrect)
            {
                Conversation conv = rainbowConversations.GetConversationByIdFromCache(e.ConversationId);

                if (contactSelected)
                {
                    if (conv != null)
                    {
                        concernCurrentSelection = ((conv.Type == Conversation.ConversationType.User) && (idSelected == conv.PeerId));
                        Contact contact = rainbowContacts.GetContactFromContactId(idSelected);
                        contactDisplayName = GetContactDisplayName(contact);
                    }
                    else
                    {
                        AddStateLine($"ContactSelected - No conversation found");
                        return;
                    }
                } else
                {
                    concernCurrentSelection = (idSelected == e.ConversationId);
                    if (conv != null)
                    {
                        if (conv.Type == Conversation.ConversationType.User)
                        {
                            Contact contact = rainbowContacts.GetContactFromContactId(conv.PeerId);
                            contactDisplayName = GetContactDisplayName(contact);
                        } else
                        {
                            contactDisplayName = conv.Name;
                            if (String.IsNullOrEmpty(contactDisplayName))
                                contactDisplayName = conv.Id;
                        }
                    } else
                    {
                        AddStateLine($"ConversationSelected - No conversation found");
                        return;
                    }
                }

                string msg = $"[{SerializeDateTime(e.Message.Date)}] [{contactDisplayName}]: [{e.Message.Content}]";

                if(concernCurrentSelection)
                {
                    AddMessageInStream(msg);
                    lastMessageIdReceived = e.Message.Id;
                } else
                {
                    AddStateLine($"Message received in another conversation - {msg}");
                }
            }
        }

        #endregion EVENTS FIRED BY RAINBOW SDK
    }
}
