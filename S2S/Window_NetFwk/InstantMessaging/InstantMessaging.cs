using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Rainbow;
using Rainbow.Model;

using Microsoft.Extensions.Logging;

using System.Web.UI.WebControls;

using EmbedIO;
using Swan;

namespace Sample_InstantMessaging
{
    public partial class SampleInstantMessagingForm : Form
    {
        // Define log object
        private static readonly ILogger log = Rainbow.LogFactory.CreateLogger<SampleInstantMessagingForm>();

        //Define Rainbow Application Id, Secret Key and Host Name
        const string APP_ID = "YOUR APP ID";
        const string APP_SECRET_KEY = "YOUR SECRET KEY";
        const string HOST_NAME = "sandbox.openrainbow.com";

        const string LOGIN_USER1 = "YOUR LOGIN";
        const string PASSWORD_USER1 = "YOUR PASSWORD";

        // Define Rainbow objects
        Rainbow.Application rainbowApplication;     // To store Rainbow Application object
        Contacts rainbowContacts;                   // To store Rainbow Contacts object
        Bubbles rainbowBubbles;                     // To store Rainbow Bubbles object
        Conversations rainbowConversations;         // To store Rainbow Conversations object
        InstantMessaging rainbowInstantMessaging;   // To store Rainbow InstantMessaging object

        Contact rainbowMyContact;                   // To store My contact (i.e. the one connected to Rainbow Server)
        List<Bubble> rainbowBubblesList;            // To store bubbles list
        List<Contact> rainbowContactsList;          // To store contacts list of my roster
        List<Conversation> rainbowConversationsList;// To store conversations list

        // Facilitator objects for this sample
        bool contactSelected = false;
        String idSelected = null;
        bool selectionCorrect = false;
        string lastMessageIDReceived = null;
        WebServer webServer = null;

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

            log.LogInformation("==============================================================");
            log.LogInformation("SampleInstantMessaging started");

            InitializeRainbowSDK();
        }

        private void InitializeRainbowSDK()
        {
            rainbowApplication = new Rainbow.Application(); ;

            // Set Application Id, Secret Key and Host Name
            rainbowApplication.SetApplicationInfo(APP_ID, APP_SECRET_KEY);
            rainbowApplication.SetHostInfo(HOST_NAME);

            // Get Rainbow main objects
            rainbowContacts = rainbowApplication.GetContacts();
            rainbowBubbles = rainbowApplication.GetBubbles();
            rainbowConversations = rainbowApplication.GetConversations();
            rainbowInstantMessaging = rainbowApplication.GetInstantMessaging();

            // EVENTS WE WANT TO MANAGE
            rainbowApplication.ConnectionStateChanged += RainbowApplication_ConnectionStateChanged;
            rainbowApplication.InitializationPerformed += RainbowApplication_InitializationPerformed;

            rainbowContacts.RosterPeerAdded += RainbowContacts_RosterPeerAdded;
            rainbowContacts.RosterPeerRemoved += RainbowContacts_RosterPeerRemoved;

            rainbowContacts.ContactPresenceChanged += RainbowContacts_ContactPresenceChanged;

            rainbowConversations.ConversationCreated += RainbowConversations_ConversationCreated;
            rainbowConversations.ConversationRemoved += RainbowConversations_ConversationRemoved;

            rainbowInstantMessaging.MessageReceived += RainbowInstantMessaging_MessageReceived;
            rainbowInstantMessaging.ReceiptReceived += RainbowInstantMessaging_ReceiptReceived;
            rainbowInstantMessaging.UserTypingChanged += RainbowInstantMessaging_UserTypingChanged;

            rainbowContactsList = new List<Contact>();
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
                tbState.AppendText("\r\n" + info);
            }
        }

        /// <summary>
        /// Permits to add the new message received in the stream
        /// </summary>
        /// <param name="info"></param>

        private void AddMessageInStream(String message)
        {
            if (tbMessagesExchanged.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(AddMessageInStream);
                this.Invoke(d, new object[] { message });
            }
            else
            {
                tbMessagesExchanged.AppendText("\r\n" + message);
            }
        }

        private void AddOlderMessagesInStrem(String messages)
        {
            if (tbMessagesExchanged.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(AddOlderMessagesInStrem);
                this.Invoke(d, new object[] { messages });
            }
            else
            {
                tbMessagesExchanged.Text = messages + tbMessagesExchanged.Text;

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
                    if (contact.Id != rainbowMyContact?.Id)
                    {
                        string displayName = GetContactDisplayName(contact);
                        System.Web.UI.WebControls.ListItem item = new System.Web.UI.WebControls.ListItem(displayName, contact.Id);
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

                str = PresenceToString(presence);
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
                cbPresenceList.Items.Add(new ListItem("busy[audio]"));
                cbPresenceList.Items.Add(new ListItem("busy[video]"));
                cbPresenceList.Items.Add(new ListItem("busy[sharing]"));
                cbPresenceList.Items.Add(new ListItem("dnd"));
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
                            else if (conversation.Type == Conversation.ConversationType.Room)
                            {
                                displayName = conversation.Name;
                                tbSelectionInfo.Text = $"Conversation-Room:[{displayName}]";
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
                    Presence presence = rainbowContacts.GetAggregatedPresenceFromContactId(contactId);
                    if (presence == null) // It means this user is offline
                        presence = rainbowContacts.CreatePresence(false, PresenceLevel.Offline, "");
                    tbContactPresence.Text = PresenceToString(presence);
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
            AddStateLine($"ConnectionStateChanged:{e.ConnectionState.State}");
            UpdateLoginButton(e.ConnectionState.State);

            if (e.ConnectionState.State == Rainbow.Model.ConnectionState.Connected)
            {
               // Need to wait the end of the init step
            }
            else if (e.ConnectionState.State == Rainbow.Model.ConnectionState.Disconnected)
            {
                if (rainbowContactsList != null)
                {
                    rainbowContactsList.Clear();
                    UpdateContactsListComboBox();
                }

                if (rainbowConversationsList != null)
                {
                    rainbowConversationsList.Clear();
                    UpdateConversationsListComboBox();
                }
            }
        }

        private void RainbowApplication_InitializationPerformed(object sender, EventArgs e)
        {
            GetAllContacts();
            GetAllBubbles();
        }


        private void RainbowContacts_ContactPresenceChanged(object sender, Rainbow.Events.PresenceEventArgs e)
        {
            if (e.Presence.BasicNodeJid == Util.GetBasicNodeJid(rainbowMyContact?.Jid_im))
            {
                AddStateLine($"Your presence changed to [{PresenceToString(e.Presence)}]");
            }
            else
            {
                Contact contact = rainbowContacts.GetContactFromContactJid(e.Presence.BasicNodeJid);
                if(contact == null)
                    AddStateLine($"Presence changed for [{e.Presence.BasicNodeJid}]: {PresenceToString(e.Presence)}");
                else
                    AddStateLine($"Presence changed for [{GetContactDisplayName(contact)}]: {PresenceToString(e.Presence)}");
            }
        }

        private void RainbowContacts_RosterPeerRemoved(object sender, Rainbow.Events.PeerEventArgs e)
        {
            AddStateLine($"A Contact has been removed:[{e.Peer.Jid}]");
            UpdateContactsListComboBox();
        }

        private void RainbowContacts_RosterPeerAdded(object sender, Rainbow.Events.PeerEventArgs e)
        {
            AddStateLine($"A Contact has been added:[{e.Peer.Jid}]");
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

                string msg;
                if (e.CarbonCopy)
                    msg = $"[{SerializeDateTime(e.Message.Date)}] [YOU]: [{e.Message.Content}]";
                else
                    msg = $"[{SerializeDateTime(e.Message.Date)}] [{contactDisplayName}]: [{e.Message.Content}]";
                if (concernCurrentSelection)
                {
                    AddMessageInStream(msg);
                    lastMessageIDReceived = e.Message.Id;
                }
                else
                    AddStateLine($"Message received in another conversation - {msg}");
            }
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
                        string logLine = String.Format("Impossible to logout:\r\n{0}", (callback.Result));
                        AddStateLine(logLine);
                        log.LogWarning(logLine);
                    }
                });
            }
            else
            {
                // Check event mode used:
                if(cbUseS2SEventMode.Checked)
                {
                    // We want to use S2S Event mode
                    rainbowApplication.Restrictions.EventMode = Restrictions.SDKEventMode.S2S;

                    // We must specify also the callback URL
                    String callbackUrl = tbCallbackUrl.Text;
                    rainbowApplication.SetS2SCallbackUrl(callbackUrl);

                    // We must also create and start a local web server - here we use 9870 for our local web server
                    if (webServer == null)
                    {
                        webServer = CreateWebServer("http://localhost:9870", callbackUrl);
                        webServer.RunAsync();
                    }
                }
                else
                {
                    // We use XMMPP mode (std mode)
                    rainbowApplication.Restrictions.EventMode = Restrictions.SDKEventMode.XMPP;
                }


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
                        string logLine = String.Format("Impossible to login:\r\n{0}", (callback.Result));
                        AddStateLine(logLine);
                        log.LogWarning(logLine);
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
                lastMessageIDReceived = null;
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
                lastMessageIDReceived = null;
            }
        }

        private void cbPresenceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!rainbowApplication.IsConnected())
                return;

            ListItem item = (ListItem)cbPresenceList.SelectedItem;
            if (item != null)
            {
                Presence presence = StringToPresence(item.Text);
                if (presence == null)
                {
                    string logLine = String.Format("Impossible to unserialize presence: [{0}]", item.Text);
                    AddStateLine(logLine);
                    log.LogWarning(logLine);
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
                        string logLine = String.Format("Impossible to set presence :\r\n{0}", (callback.Result));
                        AddStateLine(logLine);
                        log.LogWarning(logLine);
                    }
                });

            }
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            if (!rainbowApplication.IsConnected())
                return;

            String filePath = tbFilePath.Text;
            Boolean canSendFile = false;
            canSendFile = cbWithFile.Checked && (!String.IsNullOrEmpty(filePath)) && File.Exists(filePath);


            if (String.IsNullOrEmpty(tbMessage.Text) && !canSendFile)
                return;

            // Uncheck file option
            cbWithFile.Checked = false;

            if (selectionCorrect)
            {
                string textToSend = tbMessage.Text;
                tbMessage.Text = "";

                if (contactSelected)
                {
                    if (canSendFile)
                    {
                        rainbowInstantMessaging.SendMessageWithFileToContactId(idSelected, textToSend, filePath, UrgencyType.Std, null, 
                            callbackFileDescriptor =>
                            {
                                if (callbackFileDescriptor.Result.Success)
                                {
                                    AddStateLine($"File has been uploaded to server - File:[{filePath}]");
                                }
                                else
                                {
                                    string logLine = String.Format("Impossible to upload file [{1}]:\r\n{0}", (callbackFileDescriptor.Result), idSelected);
                                    AddStateLine(logLine);
                                    log.LogWarning(logLine);
                                }
                            },
                            callbackMessage =>
                            {
                                if (callbackMessage.Result.Success)
                                {
                                    AddStateLine($"Message sent successfully to contact [{idSelected}] with File:[{filePath}]");
                                }
                                else
                                {
                                    string logLine = String.Format("Impossible to send message to contact [{1}]:\r\n{0}", (callbackMessage.Result), idSelected);
                                    AddStateLine(logLine);
                                    log.LogWarning(logLine);
                                }
                            }
                        );
                    }
                    else
                    {
                        if (canSendFile)
                        {
                            rainbowInstantMessaging.SendMessageWithFileToContactId(idSelected, textToSend, filePath, UrgencyType.Std, null,
                                callbackFileDescriptor =>
                                {
                                    if (callbackFileDescriptor.Result.Success)
                                    {
                                        AddStateLine($"File has been uploaded to server - File:[{filePath}]");
                                    }
                                    else
                                    {
                                        string logLine = String.Format("Impossible to upload file [{1}]:\r\n{0}", (callbackFileDescriptor.Result), idSelected);
                                        AddStateLine(logLine);
                                        log.LogWarning(logLine);
                                    }
                                },
                                callbackMEssage =>
                                {
                                    if (callbackMEssage.Result.Success)
                                    {
                                        AddStateLine($"Message sent successfully to contact [{idSelected}] with File:[{filePath}]");
                                    }
                                    else
                                    {
                                        string logLine = String.Format("Impossible to send message to contact [{1}]:\r\n{0}", (callbackMEssage.Result), idSelected);
                                        AddStateLine(logLine);
                                        log.LogWarning(logLine);
                                    }
                                }
                            );
                        }
                        else
                        {
                            rainbowInstantMessaging.SendMessageToContactId(idSelected, textToSend, UrgencyType.Std, null, callback =>
                            {
                                if (callback.Result.Success)
                                {
                                    AddStateLine($"Message sent successfully to contact [{idSelected}]");
                                }
                                else
                                {
                                    string logLine = String.Format("Impossible to send message to contact [{1}]:\r\n{0}", (callback.Result), idSelected);
                                    AddStateLine(logLine);
                                    log.LogWarning(logLine);
                                }
                            });
                        }
                    }
                }
                else
                {
                    rainbowInstantMessaging.SendMessageToConversationId(idSelected, textToSend, null, UrgencyType.Std, null, callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddStateLine($"Message sent successfully to conversation [{idSelected}]");
                        }
                        else
                        {
                            string logLine = String.Format("Impossible to send message to conversation [{1}]:\r\n{0}", (callback.Result), idSelected);
                            AddStateLine(logLine);
                            log.LogWarning(logLine);
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
                            string logLine = String.Format("Impossible to send 'isTyping' to conversation [{1}]:\r\n{0}", (callback.Result), conversationId);
                            AddStateLine(logLine);
                            log.LogWarning(logLine);
                        }
                    });
                }
            }
        }

        private void cbIsTypingStatus_CheckedChanged(object sender, EventArgs e)
        {
            tbMessage_TextChanged(sender, e);
        }

        private void btnMarkLastMessageRead_Click(object sender, EventArgs e)
        {
            if (!rainbowApplication.IsConnected())
                return;

            if (lastMessageIDReceived == null)
            {
                AddStateLine($"Ther is no message to mark as read ...");
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
                    rainbowInstantMessaging.MarkMessageAsRead(conversation.Id, lastMessageIDReceived, callback =>
                    {
                        if (!callback.Result.Success)
                        {
                            string logLine = String.Format("Impossible to mark message [{1}] as read :\r\n{0}", (callback.Result), lastMessageIDReceived);
                            AddStateLine(logLine);
                            log.LogWarning(logLine);
                        }
                        else
                        {
                            AddStateLine("Message marked as read");
                        }
                    });
                }
            }

        }

        private void btnLoadOlderMessages_Click(object sender, EventArgs e)
        {
            if (!rainbowApplication.IsConnected())
                return;

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
                }

                if (conversation != null)
                {
                    rainbowInstantMessaging.GetMessagesFromConversationId(conversation.Id, 20, callback =>
                    {
                        if (callback.Result.Success)
                        {
                            List<Rainbow.Model.Message> list = callback.Data;
                            int nb = list.Count;
                            if (nb > 0)
                            {
                                AddStateLine($"We found [{nb}] older messages in this conversation");

                                string msgList = "";
                                string newLine = "\r\n\r\n*****************************************************************************************\r\n";
                                foreach (Rainbow.Model.Message message in list)
                                    msgList = $"{newLine}{message.ToString()}" + msgList;
                                AddOlderMessagesInStrem(msgList);
                            }
                            else
                            {
                                AddStateLine("There is no more older message in this conversation");
                            }
                        }
                        else
                        {
                            string logLine = String.Format("Impossible to get older messages from conversatiob[{1}] :\r\n{0}", (callback.Result), conversation.Id);
                            AddStateLine(logLine);
                            log.LogWarning(logLine);
                        }
                    });
                }
            }
        }

        private void btnBrowseFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    tbFilePath.Text  = openFileDialog.FileName;
            }
        }

    #endregion EVENTS FIRED BY SampleContactForm ELEMENTS

    #region UTIL METHODS

        private string GetContactDisplayName(Contact contact)
        {
            return Rainbow.Util.GetContactDisplayName(contact);
        }

        private void GetAllContacts()
        {
            if (!rainbowApplication.IsConnected())
                return;

            rainbowContacts.GetAllContactsInRosterFromServer(callback =>
            {
                if (callback.Result.Success)
                {
                    rainbowContactsList = callback.Data;
                    AddStateLine($"Nb contacts in roster:{rainbowContactsList.Count()}");
                    UpdateContactsListComboBox();
                }
                else
                {
                    string logLine = String.Format("Impossible to get all contacts:\r\n{0}", (callback.Result));
                    AddStateLine(logLine);
                    log.LogWarning(logLine);
                }
            });
        }

        private void GetAllBubbles()
        {
            if (!rainbowApplication.IsConnected())
                return;

            rainbowBubbles.GetAllBubbles(callback =>
            {
                if (callback.Result.Success)
                {
                    rainbowBubblesList = callback.Data;
                    AddStateLine($"Nb bubbles:{rainbowBubblesList.Count()}");
                    GetAllConversations();
                }
                else
                {
                    string logLine = String.Format("Impossible to get all bubbles:\r\n{0}", (callback.Result));
                    AddStateLine(logLine);
                    log.LogWarning(logLine);
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
                    string logLine = String.Format("Impossible to get all conversations:\r\n{0}", (callback.Result));
                    AddStateLine(logLine);
                    log.LogWarning(logLine);
                }
            });
        }

        private string SerializeDateTime(DateTime dateTime)
        {

            return dateTime.ToUniversalTime()
                        .ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        private WebServer CreateWebServer(string url, String callbackUrl)
        {
            Uri uri = new Uri(callbackUrl);
            String callbackAbsolutePath = uri.AbsolutePath;

            WebServer server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO)
                    //.WithStaticFolder("/", HtmlRootPath, true, m => { })
                    //.WithContentCaching(UseFileCache)
                    )

                // First, we will configure our web server by adding Modules.
                .WithLocalSessionManager()
                .WithModule(new CallbackWebModule(callbackAbsolutePath, rainbowApplication));

            server.WithStaticFolder("/", "../../webSiteContent", true, configure =>
            {

            });

            // Listen for state changes.
            //server.StateChanged += (s, e) => { $"WebServer New State - {e.NewState}"; }

            return server;
        }

        private String PresenceToString(Presence presence)
        {
            if (presence.PresenceLevel == PresenceLevel.Online)
                return "Online";
            else if (presence.PresenceLevel == PresenceLevel.Xa)
                return "Offline";
            else if (presence.PresenceLevel == PresenceLevel.Away)
                return "Away";
            else if (presence.PresenceLevel == PresenceLevel.Dnd)
                return "Dnd";

            else if (presence.PresenceLevel == PresenceLevel.Busy)
            {
                if (presence?.PresenceDetails.Length > 0)
                    return $"Busy[{presence.PresenceDetails}]";
                return "Busy";
            }

            // Set Offline by default
            return "Offline";
        }

        private Presence StringToPresence(String strPresence)
        {
            if (String.IsNullOrEmpty(strPresence))
                return rainbowContacts.CreatePresence(false, PresenceLevel.Offline, "");

            String presenceLevel;
            String presenceDetails;
            int index = strPresence.IndexOf("[");
            if (index > 0)
            {
                presenceLevel = strPresence.Substring(0, index);
                presenceDetails = strPresence.Substring(index + 1, strPresence.Length - 1 - (index + 1));
            }
            else
            {
                presenceLevel = strPresence.ToLower();
                presenceDetails = "";
            }

            return rainbowContacts.CreatePresence(false, presenceLevel, presenceDetails);
        }



        #endregion UTIL METHODS


    }
}
