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
    public partial class SampleConversationForm : Form
    {
        // Define log object
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(SampleConversationForm));

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
        Favorites rainbowFavorites;                 // To store Rainbow Favorites object

        Contact rainbowMyContact;                   // To store My contact (i.e. the one connected to Rainbow Server)
        List<Contact> rainbowContactsList;          // To store contacts list of my roster
        List<Conversation> rainbowConversationsList;// To store conversations list
        List<Favorite> rainbowFavoritesList;        // To store favorites list

        // Define delegate to update SampleContactForm components
        delegate void StringArgReturningVoidDelegate(string value);
        delegate void BoolArgReturningVoidDelegate(bool value);
        delegate void ContactArgReturningVoidDelegate(Contact value);
        delegate void VoidDelegate();

#region INIT METHODS

        public SampleConversationForm()
        {
            InitializeComponent();
            tbLogin.Text = LOGIN_USER1;
            tbPassword.Text = PASSWORD_USER1;

            log.Info("==============================================================");
            log.Info("SampleConversations started");

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
            rainbowConversations = rainbowApplication.GetConversations();
            rainbowFavorites = rainbowApplication.GetFavorites();

            // EVENTS WE WANT TO MANAGE
            rainbowApplication.ConnectionStateChanged += RainbowApplication_ConnectionStateChanged;

            rainbowContacts.ContactAdded += RainbowContacts_ContactAdded;
            rainbowContacts.ContactRemoved += RainbowContacts_ContactRemoved;

            rainbowConversations.ConversationCreated += RainbowConversations_ConversationCreated;
            rainbowConversations.ConversationRemoved += RainbowConversations_ConversationRemoved;

            rainbowFavorites.FavoriteCreated += RainbowFavorites_FavoriteCreated;
            rainbowFavorites.FavoriteRemoved += RainbowFavorites_FavoriteRemoved;

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
                tbState.AppendText(info + "\r\n");
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
                        string displayName = contact.DisplayName;
                        if (String.IsNullOrEmpty(displayName))
                            displayName = $"{contact.LastName} {contact.FirstName}";
                        ListItem item = new ListItem(displayName, contact.Id);
                        cbContactsList.Items.Add(item);
                    }
                }

                // Select the first contact
                if (cbContactsList.Items.Count > 0)
                    cbContactsList.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Permits to udpate combo box with list of favorites list
        /// </summary>
        private void UpdateFavoritesListComboBox()
        {
            if (cbFavoritesList.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateFavoritesListComboBox);
                this.Invoke(d);
            }
            else
            {
                // Unselect the selected contact from both combo box
                cbFavoritesList.SelectedIndex = -1;

                // First clear combo box
                cbFavoritesList.Items.Clear();

                // Fill the combo bow with new elements
                foreach (Favorite favorite in rainbowFavoritesList)
                {
                    string peerId = favorite.PeerId;

                    log.DebugFormat("UpdateFavoritesListComboBox - peerID:[{0}] - type:[{1}]", peerId, favorite.Type);

                    // Is-it a favorite with a User ?
                    if (favorite.Type == Rainbow.Model.FavoriteType.User)
                    {

                        Contact contact = rainbowContacts.GetContactFromContactId(peerId);
                        //if (contact != null)
                        //{
                        //    string displayName = contact.DisplayName;
                        //    if (String.IsNullOrEmpty(displayName))
                        //        displayName = $"{contact.LastName} {contact.FirstName}";
                        //    //ListItem item = new ListItem(displayName, favorite.Id);
                        //    //cbFavoritesList.Items.Add(item);
                        //}
                    }
                    // Is-it a favorite with a Bubble/Room ?
                    else if (favorite.Type == Rainbow.Model.FavoriteType.Room)
                    {
                        //String conversationId = rainbowConversations.GetConversationIdByPeerIdFromCache(peerId);
                        //Conversation conversation = rainbowConversations.GetConversationByIdFromCache(conversationId);
                        //if (conversation != null)
                        //{
                        //    string displayName = conversation.Name;
                        //    //ListItem item = new ListItem(displayName, favorite.Id);
                        //    //cbFavoritesList.Items.Add(item);
                        //}
                    }
                }

                // Select the first item
                if (cbFavoritesList.Items.Count > 0)
                    cbFavoritesList.SelectedIndex = 0;
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
                            displayName = contact.DisplayName;
                            if (String.IsNullOrEmpty(displayName))
                                displayName = $"{contact.LastName} {contact.FirstName}";
                        }
                        else
                            displayName = conversation.Id;
                    }
                    ListItem item = new ListItem(displayName, conversation.Id);
                    cbConversationsList.Items.Add(item);
                }

                // Select the first item
                if (cbConversationsList.Items.Count > 0)
                    cbConversationsList.SelectedIndex = 0;
            }
        }

        private void EnableAddContactToConversation(bool enable)
        {
            if(btnContactAddToConversation.InvokeRequired)
            {
                BoolArgReturningVoidDelegate d = new BoolArgReturningVoidDelegate(EnableAddContactToConversation);
                this.Invoke(d, new object[] { enable });
            }
            else
            {
                btnContactAddToConversation.Enabled = enable;
            }
        }

        private void EnableAddConversationToFavorite(bool enable)
        {
            if (btnConversationAddToFavorite.InvokeRequired)
            {
                BoolArgReturningVoidDelegate d = new BoolArgReturningVoidDelegate(EnableAddConversationToFavorite);
                this.Invoke(d, new object[] { enable });
            }
            else
            {
                btnConversationAddToFavorite.Enabled = enable;
            }
        }

        private void EnableAddContactToFavorite(bool enable)
        {
            if (btnContactAddToFavorite.InvokeRequired)
            {
                BoolArgReturningVoidDelegate d = new BoolArgReturningVoidDelegate(EnableAddContactToFavorite);
                this.Invoke(d, new object[] { enable });
            }
            else
            {
                btnContactAddToFavorite.Enabled = enable;
            }
        }

        private void CheckContactSelectedAsFavorite()
        {
            if (cbContactsList.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(CheckContactSelectedAsFavorite);
                this.Invoke(d);
            }
            else
            {
                ListItem item = (ListItem)cbContactsList.SelectedItem;
                if (item != null)
                {
                    string id = item.Value;
                    // Check if selected item is in favorites
                    Favorite favorite = rainbowFavorites.GetFavoriteByPeerId(id);
                    EnableAddContactToFavorite(favorite == null);
                }
            }
        }

        private void CheckConversationsSelectedAsFavorite()
        {
            if (cbContactsList.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(CheckConversationsSelectedAsFavorite);
                this.Invoke(d);
            }
            else
            {
                ListItem item = (ListItem)cbConversationsList.SelectedItem;
                if (item != null)
                {
                    string conversationId = item.Value;
                    Conversation conversation = rainbowConversations.GetConversationByIdFromCache(conversationId);
                    if (conversation != null)
                    {
                        // Check if selected item is in favorites
                        Favorite favorite = rainbowFavorites.GetFavoriteByPeerId(conversation.PeerId);
                        EnableAddConversationToFavorite(favorite == null);
                    }
                }
            }
        }

        private void CheckContactSelectedAsConversation()
        {
            if (cbContactsList.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(CheckContactSelectedAsConversation);
                this.Invoke(d);
            }
            else
            {
                ListItem item = (ListItem)cbContactsList.SelectedItem;
                if (item != null)
                {
                    string id = item.Value;
                    // Check if selected item is in conversations
                    String conversationId = rainbowConversations.GetConversationIdByPeerIdFromCache(id);
                    EnableAddContactToConversation(String.IsNullOrEmpty(conversationId));
                }
            }
        }

#endregion METHOD TO UPDATE SampleConversationForm COMPONENTS

#region EVENTS FIRED BY RAINBOW SDK

        private void RainbowApplication_ConnectionStateChanged(object sender, Rainbow.Events.ConnectionStateEventArgs e)
        {
            // Add info about connection state
            AddStateLine($"ConnectionStateChanged:{e.State}");
            UpdateLoginButton(e.State);

            if (e.State == Rainbow.Model.ConnectionState.Connected)
            {
                GetAllContacts();
                GetAllFavorites();
                GetAllConversations();
            }
            else if (e.State == Rainbow.Model.ConnectionState.Disconnected)
            {
                rainbowContactsList.Clear();
                UpdateContactsListComboBox();

                rainbowConversationsList.Clear();
                UpdateConversationsListComboBox();

                rainbowFavoritesList.Clear();
                UpdateFavoritesListComboBox();
            }
        }

        private void RainbowFavorites_FavoriteRemoved(object sender, Rainbow.Events.FavoriteEventArgs e)
        {
            AddStateLine($"A Favorite has been removed:[{e.Favorite.Id}]");
            UpdateFavoritesListComboBox();
            CheckContactSelectedAsFavorite();
            CheckConversationsSelectedAsFavorite();
        }

        private void RainbowFavorites_FavoriteCreated(object sender, Rainbow.Events.FavoriteEventArgs e)
        {
            AddStateLine($"A Favorite has been created:[{e.Favorite.Id}]");
            UpdateFavoritesListComboBox();
            CheckContactSelectedAsFavorite();
            CheckConversationsSelectedAsFavorite();
        }

        private void RainbowConversations_ConversationRemoved(object sender, Rainbow.Events.ConversationEventArgs e)
        {
            AddStateLine($"A Conversation has been removed:[{e.Conversation.Id}]");
            UpdateConversationsListComboBox();
            CheckContactSelectedAsConversation();
        }

        private void RainbowConversations_ConversationCreated(object sender, Rainbow.Events.ConversationEventArgs e)
        {
            AddStateLine($"A Conversation has been created:[{e.Conversation.Id}]");
            UpdateConversationsListComboBox();
            CheckContactSelectedAsConversation();
        }

        private void RainbowContacts_ContactRemoved(object sender, Rainbow.Events.JidEventArgs e)
        {
            AddStateLine($"A Contact has been removed:[{e.Jid}]");
            UpdateContactsListComboBox();
            CheckContactSelectedAsFavorite();
            CheckContactSelectedAsConversation();
        }

        private void RainbowContacts_ContactAdded(object sender, Rainbow.Events.JidEventArgs e)
        {
            AddStateLine($"A Contact has been added:[{e.Jid}]");
            UpdateContactsListComboBox();
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

        private void btnConversationRemove_Click(object sender, EventArgs e)
        {
            ListItem item = (ListItem)cbConversationsList.SelectedItem;
            if (item != null)
            {
                string id = item.Value;
                rainbowConversations.RemoveFromConversationsById(id, callback =>
                {
                    if (callback.Result.Success)
                    {
                        AddStateLine($"conversation[{id}] has been removed");
                        GetAllConversations();
                        CheckContactSelectedAsConversation();
                    }
                    else
                    {
                        string logLine = String.Format("Impossible to remove conversation[{1}]:\r\n{0}", Util.SerialiseSdkError(callback.Result), id);
                        AddStateLine(logLine);
                        log.WarnFormat(logLine);
                    }
                });
            }
        }

        private void btnFavoriteRemove_Click(object sender, EventArgs e)
        {
            ListItem item = (ListItem)cbFavoritesList.SelectedItem;
            if (item != null)
            {
                string id = item.Value;
                rainbowFavorites.DeleteFavorite(rainbowFavorites.GetFavorite(id), callback =>
                {
                    if (callback.Result.Success)
                    {
                        AddStateLine($"Favorite[{id}] has been removed");
                        GetAllFavorites();
                        CheckContactSelectedAsFavorite();
                        CheckConversationsSelectedAsFavorite();
                    }
                    else
                    {
                        string logLine = String.Format("Impossible to remove favorite[{1}]:\r\n{0}", Util.SerialiseSdkError(callback.Result), id);
                        AddStateLine(logLine);
                        log.WarnFormat(logLine);
                    }
                });
            }
        }

        private void btnFavoritePosition_Click(object sender, EventArgs e)
        {
            ListItem item = (ListItem)cbFavoritesList.SelectedItem;
            if (item != null)
            {
                int result;
                string id = item.Value;
                if (int.TryParse(tbFavoritePosition.Text, out result))
                {
                    if (result > -1)
                    {
                        rainbowFavorites.UpdateFavoritePosition(id, result, callback =>
                        {
                            if (callback.Result.Success)
                            {
                                AddStateLine($"Favorite position[{id}] has been updated");
                                GetAllFavorites();
                                CheckContactSelectedAsFavorite();
                                CheckConversationsSelectedAsFavorite();
                            }
                            else
                            {
                                string logLine = String.Format("Impossible to update favorite position [{1}]:\r\n{0}", Util.SerialiseSdkError(callback.Result), id);
                                AddStateLine(logLine);
                                log.WarnFormat(logLine);
                            }
                        });
                    }
                    else
                        AddStateLine("Not a positive integer value specified");
                }
                else
                    AddStateLine("Not a integer value specified");
            }
        }

        private void btnContactAddToConversation_Click(object sender, EventArgs e)
        {
            ListItem item = (ListItem)cbContactsList.SelectedItem;
            if (item != null)
            {
                string id = item.Value;
                rainbowConversations.GetConversationFromContactId(id, callback =>
                {
                    if (callback.Result.Success)
                    {
                        AddStateLine($"Conversation with [{id}] has been created");
                        GetAllConversations();
                        CheckContactSelectedAsConversation();
                    }
                    else
                    {
                        string logLine = String.Format("Impossible to create conversation with [{1}]:\r\n{0}", Util.SerialiseSdkError(callback.Result), id);
                        AddStateLine(logLine);
                        log.WarnFormat(logLine);
                    }
                });
            }
        }

        private void btnContactAddToFavorite_Click(object sender, EventArgs e)
        {
            ListItem item = (ListItem)cbContactsList.SelectedItem;
            if (item != null)
            {
                string id = item.Value;
                rainbowFavorites.CreateFavorite(id, Rainbow.Model.FavoriteType.User, callback =>
                {
                    if (callback.Result.Success)
                    {
                        AddStateLine($"Favortie with [{id}] has been created");
                        GetAllFavorites();
                        CheckContactSelectedAsFavorite();
                    }
                    else
                    {
                        string logLine = String.Format("Impossible to create favorite with [{1}]:\r\n{0}", Util.SerialiseSdkError(callback.Result), id);
                        AddStateLine(logLine);
                        log.WarnFormat(logLine);
                    }
                });
            }
        }

        private void cbFavoritesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = cbFavoritesList.SelectedIndex;
            if (index == -1)
                tbFavoritePosition.Text = "";
            else
                tbFavoritePosition.Text = Convert.ToString(index);
        }

        private void cbContactsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckContactSelectedAsFavorite();
            CheckContactSelectedAsConversation();
        }

        private void btnConversationAddToFavorite_Click(object sender, EventArgs e)
        {
            ListItem item = (ListItem)cbConversationsList.SelectedItem;
            if (item != null)
            {
                string conversationId = item.Value;
                Conversation conversation = rainbowConversations.GetConversationByIdFromCache(conversationId);
                if (conversation == null)
                {
                    AddStateLine($"No conversation object found with this ID:{conversationId}");
                    return;
                }
                string peerId = conversation.PeerId;

                String favoriteType;
                if (conversation.Type == Conversation.ConversationType.User)
                    favoriteType = Rainbow.Model.FavoriteType.User;
                else
                    favoriteType = Rainbow.Model.FavoriteType.Room;

                rainbowFavorites.CreateFavorite(peerId, favoriteType, callback =>
                {
                    if (callback.Result.Success)
                    {
                        AddStateLine($"Favorite with [{conversationId}] has been created");
                        GetAllFavorites();
                        CheckContactSelectedAsFavorite();
                        CheckConversationsSelectedAsFavorite();
                    }
                    else
                    {
                        string logLine = String.Format("Impossible to create favorite with [{1}]:\r\n{0}", Util.SerialiseSdkError(callback.Result), conversationId);
                        AddStateLine(logLine);
                        log.WarnFormat(logLine);
                    }
                });
            }
        }

        private void cbConversationsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckConversationsSelectedAsFavorite();
        }

#endregion EVENTS FIRED BY SampleContactForm ELEMENTS

#region UTIL METHODS

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

        private void GetAllFavorites()
        {
            if (!rainbowApplication.IsConnected())
                return;

            rainbowFavoritesList = rainbowFavorites.GetFavorites();
            AddStateLine($"Nb favorites:{rainbowFavoritesList.Count()}");
            UpdateFavoritesListComboBox();
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

#endregion UTIL METHODS

    }
}
