using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Rainbow;
using Rainbow.Model;

using log4net;
using log4net.Config;
using System.Web.UI.WebControls;

namespace Sample_Contacts
{
    public partial class SampleInstantMessagingForm : Form
    {
        // Define log object
        private static readonly ILog log = LogManager.GetLogger(typeof(SampleInstantMessagingForm));

        //Define Rainbow Application Id, Secret Key and Host Name
        const string APP_ID = "YOUR APP ID";
        const string APP_SECRET_KEY = "YOUR SECRET KEY";
        const string HOST_NAME = "sandbox.openrainbow.com";

        const string LOGIN_USER1 = "YOUR LOGIN";
        const string PASSWORD_USER1 = "YOUR PASSWORD";

        // Define Rainbow objects
        Rainbow.Application rainbowApplication;     // To store Rainbow Application object
        Contacts rainbowContacts;                   // To store Rainbow Contacts object
        Conversations rainbowConversations;         // To store Rainbow Conversations object
        InstantMessaging rainbowInstantMessaging;   // To store Rainbow InstantMessaging object

        Contact rainbowMyContact;                   // To store My contact (i.e. the one connected to Rainbow Server)
        List<Contact> rainbowContactsList;          // To store contacts list of my roster
        List<Conversation> rainbowConversationsList;// To store conversations list

        //
        bool contactSelected = false;
        String idSelected = null;
        bool selectionCorrect = false;

        // Define delegate to update SampleContactForm components
        delegate void StringArgReturningVoidDelegate(string value);
        delegate void BoolArgReturningVoidDelegate(bool value);
        delegate void ContactArgReturningVoidDelegate(Contact value);
        delegate void PresenceArgReturningVoidDelegate(Presence value);
        delegate void VoidDelegate();

    #region INIT METHODS

        public SampleInstantMessagingForm()
        {
            InitializeComponent();

            tbLogin.Text = LOGIN_USER1;
            tbPassword.Text = PASSWORD_USER1;

            SetDefaultPresenceList();

            InitializeLog();

            InitializeRainbowSDK();
        }

        private void InitializeLog()
        {
            string logPath = @".\..\..\log4netConfiguration.xml";
            FileInfo fileInfo = new FileInfo(logPath);

            bool result = File.Exists(logPath);

            if (fileInfo != null)
                XmlConfigurator.Configure(fileInfo);

            log.Info("==============================================================");
            log.Info("SampleInstantMessaging started");
        }

        private void InitializeRainbowSDK()
        {
            rainbowApplication = new Rainbow.Application(); ;

            // Set Application Id, Secret Key and Host Name
            rainbowApplication.SetApplicationInfo(APP_ID, APP_SECRET_KEY);
            rainbowApplication.SetHostInfo(HOST_NAME);

            // Get Rainbow main objects
            rainbowContacts = rainbowApplication.GetContacts();
            rainbowConversations = rainbowApplication.GetConversations();
            rainbowInstantMessaging = rainbowApplication.GetInstantMessaging();

            // EVENTS WE WANT TO MANAGE
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
        }

        private void RainbowInstantMessaging_ReceiptReceived(object sender, Rainbow.Events.ReceiptReceivedEventArgs e)
        {
            //e.MessageId
        }

        private void RainbowInstantMessaging_UserTypingChanged(object sender, Rainbow.Events.UserTypingEventArgs e)
        {
            //e.ContactJid
            //e.ConversationId

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
                        AddStateLine($"ContactSelected - No conversation found ...");
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
                        AddStateLine($"ConversationSelected - No conversation found ...");
                        return;
                    }
                }

                if (concernCurrentSelection)
                {
                    if (e.IsTyping)
                        AddStateLine($"[{contactDisplayName}] is typing in the CURRENT SELECTED conversation.");
                    else
                        AddStateLine($"[{contactDisplayName}] is no more typing in the CURRENT SELECTED conversation.");
                }
                else
                {
                    if (e.IsTyping)
                        AddStateLine($"[{contactDisplayName}] is typing in ANOTHER conversation.");
                    else
                        AddStateLine($"[{contactDisplayName}] is no more typing in ANOTHER conversation.");
                }
            }
        }

        private void RainbowInstantMessaging_MessageReceived(object sender, Rainbow.Events.MessageEventArgs e)
        {
            //e.ContactJid
            //e.ConversationId
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
                        AddStateLine($"ContactSelected - No conversation found ...");
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
                        AddStateLine($"ConversationSelected - No conversation found ...");
                        return;
                    }
                }

                string msg = $"[{SerializeDateTime(e.Message.Date)}] [{contactDisplayName}]: [{e.Message.Content}]";
                if (concernCurrentSelection)
                    AddMessageInStream(msg);
                else
                    AddStateLine($"Message received in another conversation - {msg}");
            }
        }

    #endregion INIT METHODS

    #region METHOD TO UPDATE SampleConversationForm COMPONENTS

        /// <summary>
        /// Permits to add a new sting in the text box at the bottom of the form: it permits to log things happening
        /// </summary>
        /// <param name="info"></param>
        private void AddStateLine(String info)
        {
            // We can update this compoent only if we are in the from thread
            if (tbState.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(AddStateLine);
                this.Invoke(d, new object[] { info });
            }
            else
            {
                tbState.AppendText(info + "\r\n");
            }
        }

        /// <summary>
        /// Permits to add the new message received in the stream
        /// </summary>
        /// <param name="info"></param>

        private void AddMessageInStream(String msg)
        {
            if (tbMessagesExchanged.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(AddMessageInStream);
                this.Invoke(d, new object[] { msg });
            }
            else
            {
                tbMessagesExchanged.AppendText(msg + "\r\n");
            }
        }

        private void ClearMessageStream()
        {
            if (tbMessagesExchanged.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(ClearMessageStream);
                this.Invoke(d);
            }
            else
            {
                tbMessagesExchanged.Text = "";
            }
        }

        /// <summary>
        /// Permits to update Login / logout btn
        /// </summary>
        /// <param name="connectionState"></param>
        private void UpdateLoginButton(string connectionState)
        {
            // We can update this compoent only if we are in the from thread
            if (tbLogin.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(UpdateLoginButton);
                this.Invoke(d, new object[] { connectionState });
            }
            else
            {
                if(connectionState == Rainbow.Model.ConnectionState.Connected)
                {
                    tbLogin.Enabled = true;
                    btnLoginLogout.Text = "Logout";
                }
                else if (connectionState == Rainbow.Model.ConnectionState.Disconnected)
                {
                    tbLogin.Enabled = true;
                    btnLoginLogout.Text = "Login";
                }
                else if (connectionState == Rainbow.Model.ConnectionState.Connecting)
                {
                    tbLogin.Enabled = false;
                    btnLoginLogout.Text = "Connecting";
                }
            }
        }

        /// <summary>
        /// Permits to udpate combo box with list of contacts list
        /// </summary>
        private void UpdateContactsListComboBox()
        {
            if (cbContactsList.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateContactsListComboBox);
                this.Invoke(d);
            }
            else
            {
                // Unselect the selected contact from both combo box
                cbContactsList.SelectedIndex = -1;

                // First clear combo box
                cbContactsList.Items.Clear();

                // Fill the combo bow with new elements
                foreach (Contact contact in rainbowContactsList)
                {
                    // Avoid to add my contact in the list
                    if (contact.Id != rainbowMyContact.Id)
                    {
                        string displayName = GetContactDisplayName(contact);
                        ListItem item = new ListItem(displayName, contact.Id);
                        cbContactsList.Items.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Permits to udpate combo box with list of conversations list
        /// </summary>
        private void UpdateConversationsListComboBox()
        {
            if (cbConversationsList.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateConversationsListComboBox);
                this.Invoke(d);
            }
            else
            {
                // Unselect the selected contact from both combo box
                cbConversationsList.SelectedIndex = -1;

                // First clear combo box
                cbConversationsList.Items.Clear();

                // Fill the combo bow with new elements
                foreach (Conversation conversation in rainbowConversationsList)
                {
                    string displayName = conversation.Name;

                    if (String.IsNullOrEmpty(displayName))
                    {
                        Contact contact = rainbowContacts.GetContactFromContactId(conversation.PeerId);
                        if (contact != null)
                        {
                            displayName = GetContactDisplayName(contact);
                        }
                        else
                            displayName = conversation.Id;
                    }
                    ListItem item = new ListItem(displayName, conversation.Id);
                    cbConversationsList.Items.Add(item);
                }
            }
        }

        private void SetPresenceDisplay(Presence presence)
        {
            if (cbPresenceList.InvokeRequired)
            {
                PresenceArgReturningVoidDelegate d = new PresenceArgReturningVoidDelegate(SetPresenceDisplay);
                this.Invoke(d, new object[] { presence });
            }
            else
            {
                ListItem item;
                int i, nb;
                string str;

                str = SerializePresence(presence);
                nb = cbPresenceList.Items.Count;

                for(i=0; i<nb; i++)
                {
                    item = (ListItem)cbPresenceList.Items[i];
                    if (item.Text == str)
                    {
                        cbPresenceList.SelectedIndex = i;
                        AddStateLine($"You change your presence to [{str}]");
                        return;
                    }
                }
                // We don't find a match ...
                cbPresenceList.SelectedIndex = -1;
                AddStateLine($"/!\\ You change your presence to [{str}] but we don't manage this case ...");
            }
        }

        private void SetDefaultPresenceList()
        {
            if (cbPresenceList.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(SetDefaultPresenceList);
                this.Invoke(d);
            }
            else
            {
                cbPresenceList.SelectedIndex = -1;
                cbPresenceList.Items.Clear();

                cbPresenceList.Items.Add(new ListItem("online"));
                cbPresenceList.Items.Add(new ListItem("away"));
                cbPresenceList.Items.Add(new ListItem("busy-audio"));
                cbPresenceList.Items.Add(new ListItem("busy-video"));
                cbPresenceList.Items.Add(new ListItem("busy-sharing"));
                cbPresenceList.Items.Add(new ListItem("dnd"));
                cbPresenceList.Items.Add(new ListItem("dnd-presentation"));
                cbPresenceList.Items.Add(new ListItem("xa"));
            }
        }
 
        private void UpdateSelectionInfo()
        {
            if (tbSelectionInfo.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateSelectionInfo);
                this.Invoke(d);
            }
            else
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

                            tbSelectionInfo.Text = $"Contact:[{displayName}]";
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
                                {
                                    displayName = GetContactDisplayName(contact);
                                }
                                else
                                    displayName = conversation.PeerId; //TO DO - search more info about this contact

                                tbSelectionInfo.Text = $"Conversation-User:[{displayName}]";
                                selectionCorrect = true;
                            }
                            else if (conversation.Type == Conversation.ConversationType.Bot)
                            {
                                displayName = conversation.Name;
                                if (String.IsNullOrEmpty(displayName))
                                    displayName = conversation.PeerId;
                                tbSelectionInfo.Text = $"Conversation-Bot:[{displayName}]";
                                selectionCorrect = true;
                            }
                            else
                            {
                                AddStateLine("Case not managed yet ...");
                                reset = true;
                            }
                        }
                        else
                        {
                            AddStateLine("Conversation is null ...");
                            reset = true;
                        }
                    }
                }
                if(reset)
                {
                    selectionCorrect = false;
                    tbSelectionInfo.Text = "No selection";
                    AddStateLine("Reset contact or conversation selection");
                }
                if(contactId != null)
                {
                    // Get and display contact presence
                    Presence presence = rainbowContacts.GetPresenceFromContactId(contactId);
                    if (presence == null) // It means this user is offline
                        presence = new Presence(PresenceLevel.Offline, "");
                    tbContactPresence.Text = SerializePresence(presence);
                }
                else
                    tbContactPresence.Text = "no presence in this context";
            }
        }

    #endregion METHOD TO UPDATE SampleConversationForm COMPONEnTS

    #region EVENTS FIRED BY RAINBOW SDK

        private void RainbowApplication_ConnectionStateChanged(object sender, Rainbow.Events.ConnectionStateEventArgs e)
        {
            // Add info about connection state
            AddStateLine($"ConnectionStateChanged:{e.State}");
            UpdateLoginButton(e.State);

            if (e.State == Rainbow.Model.ConnectionState.Connected)
            {
                GetAllContacts();
                GetAllConversations();
            }
            else if (e.State == Rainbow.Model.ConnectionState.Disconnected)
            {
                rainbowContactsList.Clear();
                UpdateContactsListComboBox();

                rainbowConversationsList.Clear();
                UpdateConversationsListComboBox();
            }
        }

        private void RainbowContacts_ContactPresenceChanged(object sender, Rainbow.Events.PresenceEventArgs e)
        {
            if (e.Jid == rainbowMyContact.Jid_im)
            {
                AddStateLine($"Your presence changed to [{SerializePresence(e.Presence)}]");
            }
            else
            {
                Contact contact = rainbowContacts.GetContactFromContactJid(e.Jid);
                if(contact == null)
                    AddStateLine($"Presence changed for [{e.Jid}]: {SerializePresence(e.Presence)}");
                else
                    AddStateLine($"Presence changed for [{GetContactDisplayName(contact)}]: {SerializePresence(e.Presence)}");
            }
        }

        private void RainbowContacts_ContactRemoved(object sender, Rainbow.Events.JidEventArgs e)
        {
            AddStateLine($"A Contact has been removed:[{e.Jid}]");
            UpdateContactsListComboBox();
        }

        private void RainbowContacts_ContactAdded(object sender, Rainbow.Events.JidEventArgs e)
        {
            AddStateLine($"A Contact has been added:[{e.Jid}]");
            UpdateContactsListComboBox();
        }

        private void RainbowConversations_ConversationRemoved(object sender, Rainbow.Events.ConversationEventArgs e)
        {
            AddStateLine($"A Conversation has been removed:[{e.Conversation.Id}]");
            UpdateConversationsListComboBox();
        }

        private void RainbowConversations_ConversationCreated(object sender, Rainbow.Events.ConversationEventArgs e)
        {
            AddStateLine($"A Conversation has been created:[{e.Conversation.Id}]");
            UpdateConversationsListComboBox();
        }

        
    #endregion EVENTS FIRED BY RAINBOW SDK

    #region EVENTS FIRED BY SampleContactForm ELEMENTS

        private void btnLoginLogout_Click(object sender, EventArgs e)
        {
            if(rainbowApplication.IsConnected())
            {
                // We want to logout
                rainbowApplication.Logout(callback =>
                {
                    if (!callback.Result.Success)
                    {
                        string logLine = String.Format("Impossible to logout:\r\n{0}", Util.SerialiseSdkError(callback.Result));
                        AddStateLine(logLine);
                        log.WarnFormat(logLine);
                    }
                });
            }
            else
            {
                String login = tbLogin.Text;
                String password = tbPassword.Text;

                // We want to login
                rainbowApplication.Login(login, password, callback =>
                {
                    if (callback.Result.Success)
                    {
                        // Since we are connected, we get the current contact object
                        rainbowMyContact = rainbowContacts.GetCurrentContact();
                    }
                    else
                    {
                        string logLine = String.Format("Impossible to login:\r\n{0}", Util.SerialiseSdkError(callback.Result));
                        AddStateLine(logLine);
                        log.WarnFormat(logLine);
                    }
                });
            }
        }

        private void cbContactsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListItem item = (ListItem)cbContactsList.SelectedItem;
            if(item != null)
            {
                // Unselect conversations list
                cbConversationsList.SelectedIndex = -1;

                idSelected = item.Value;
                contactSelected = true;
                UpdateSelectionInfo();

                ClearMessageStream();
            }
        }

        private void cbConversationsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListItem item = (ListItem)cbConversationsList.SelectedItem;
            if (item != null)
            {
                // Unselect contacts list
                cbContactsList.SelectedIndex = -1;

                idSelected = item.Value;
                contactSelected = false;
                UpdateSelectionInfo();

                ClearMessageStream();
            }
        }

        private void cbPresenceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!rainbowApplication.IsConnected())
                return;

            ListItem item = (ListItem)cbPresenceList.SelectedItem;
            if (item != null)
            {
                Presence presence = UnserializePresence(item.Text);
                if (presence == null)
                {
                    string logLine = String.Format("Impossible to unserialize presence: [{0}]", item.Text);
                    AddStateLine(logLine);
                    log.WarnFormat(logLine);
                    return;
                }

                rainbowContacts.SetPresenceLevel(presence, 5, callback =>
                {
                    if (callback.Result.Success)
                    {
                        AddStateLine($"Presence set to [{item.Text}]");
                    }
                    else
                    {
                        string logLine = String.Format("Impossible to set presence :\r\n{0}", Util.SerialiseSdkError(callback.Result));
                        AddStateLine(logLine);
                        log.WarnFormat(logLine);
                    }
                });

            }
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            if (!rainbowApplication.IsConnected())
                return;

            if (String.IsNullOrEmpty(tbMessage.Text))
                return;

            if (selectionCorrect)
            {
                string textToSend = tbMessage.Text;
                tbMessage.Text = "";

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

        private void tbMessage_TextChanged(object sender, EventArgs e)
        {
            if (!rainbowApplication.IsConnected())
                return;

            if (selectionCorrect && cbIsTypingStatus.Checked)
            {
                string conversationId;
                if (contactSelected)
                    conversationId = rainbowConversations.GetConversationIdByPeerIdFromCache(idSelected);
                else
                    conversationId = idSelected;

                if (!String.IsNullOrEmpty(conversationId))
                {
                    rainbowInstantMessaging.SendIsTypingInConversationById(conversationId, !String.IsNullOrEmpty(tbMessage.Text), callback =>
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

        private void cbIsTypingStatus_CheckedChanged(object sender, EventArgs e)
        {
            tbMessage_TextChanged(sender, e);
        }

    #endregion EVENTS FIRED BY SampleContactForm ELEMENTS

    #region UTIL METHODS

        private string SerializePresence(Presence presence)
        {
            string result;

            if (presence.PresenceLevel == Rainbow.Model.PresenceLevel.Online)
                result = Rainbow.Model.PresenceLevel.Online;
            else if (presence.PresenceLevel == Rainbow.Model.PresenceLevel.Offline)
                result = Rainbow.Model.PresenceLevel.Offline;
            else if (presence.PresenceLevel == Rainbow.Model.PresenceLevel.Xa)
                result = Rainbow.Model.PresenceLevel.Xa;
            else if (presence.PresenceLevel == Rainbow.Model.PresenceLevel.Away)
                result = Rainbow.Model.PresenceLevel.Away;
            else if (presence.PresenceLevel == Rainbow.Model.PresenceLevel.Dnd)
            {
                if( (presence.PresenceDetails != null) && (presence.PresenceDetails.Length > 0) )
                    result = Rainbow.Model.PresenceLevel.Dnd + "-" + PresenceDetails.Presentation;
                else
                    result = Rainbow.Model.PresenceLevel.Dnd;
            }
            else if (presence.PresenceLevel == Rainbow.Model.PresenceLevel.Busy)
            {
                if ((presence.PresenceDetails != null) && (presence.PresenceDetails.Length > 0))
                    result = Rainbow.Model.PresenceLevel.Busy + "-" + presence.PresenceDetails;
                else
                    result = Rainbow.Model.PresenceLevel.Busy;
            }
            else
                result = "unknown";

            return result;
        }

        private Presence UnserializePresence(string str)
        {
            string presenceLevel  = "";
            string presenceDetails = "";

            if (str == Rainbow.Model.PresenceLevel.Online)
                presenceLevel = Rainbow.Model.PresenceLevel.Online;
            else if (str == Rainbow.Model.PresenceLevel.Offline)
                presenceLevel = Rainbow.Model.PresenceLevel.Offline;
            else if (str == Rainbow.Model.PresenceLevel.Xa)
                presenceLevel = Rainbow.Model.PresenceLevel.Xa;
            else if (str == Rainbow.Model.PresenceLevel.Away)
                presenceLevel = Rainbow.Model.PresenceLevel.Away;
            else if (str.StartsWith(Rainbow.Model.PresenceLevel.Dnd))
            {
                presenceLevel = Rainbow.Model.PresenceLevel.Dnd;
                if (str.Length > 4)
                    presenceDetails = str.Substring(4); ;
            }
            else if (str.StartsWith(Rainbow.Model.PresenceLevel.Busy))
                {
                presenceLevel = Rainbow.Model.PresenceLevel.Busy;
                if (str.Length > 5)
                    presenceDetails = str.Substring(5);
            }
            else
                return null;

            return new Presence(presenceLevel, presenceDetails);
        }

        private string GetContactDisplayName(Contact contact)
        {
            string displayName = null;
            if (contact!= null)
            {
                displayName = contact.DisplayName;
                if (String.IsNullOrEmpty(displayName))
                    displayName = $"{contact.LastName} {contact.FirstName}";
            }
            return displayName;
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
                    AddStateLine($"Nb contacts in roster:{rainbowContactsList.Count()}");
                    UpdateContactsListComboBox();
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
                    rainbowConversationsList = callback.Data;
                    AddStateLine($"Nb conversations:{rainbowConversationsList.Count()}");
                    UpdateConversationsListComboBox();
                }
                else
                {
                    string logLine = String.Format("Impossible to get all conversations:\r\n{0}", Util.SerialiseSdkError(callback.Result));
                    AddStateLine(logLine);
                    log.WarnFormat(logLine);
                }
            });
        }

        private string SerializeDateTime(DateTime dateTime)
        {

            return dateTime.ToUniversalTime()
                        .ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        #endregion UTIL METHODS


    }
}
