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
    public partial class SampleContactForm : Form
    {
        // Define log object
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(SampleContactForm));

        //Define Rainbow Application Id, Secret Key and Host Name
        const string APP_ID = "YOUR APP ID";
        const string APP_SECRET_KEY = "YOUR SECRET KEY";
        const string HOST_NAME = "sandbox.openrainbow.com";

        const string LOGIN_USER1 = "YOUR LOGIN";
        const string PASSWORD_USER1 = "YOUR PASSWORD";

        // Define Rainbow objects
        Rainbow.Application rainbowApplication; // To store Rainbow Application object
        Contacts rainbowContacts;               // To store Rainbow Contacts object
        Contact rainbowMyContact;               // To store My contact (i.e. the one connected to Rainbow Server)
        List<Contact> rainbowContactsList;      // To store contacts list of my roster
        List<Contact> rainbowContactsListFound; // To store contacts list found by search

        // Define delegate to update SampleContactForm components
        delegate void StringArgReturningVoidDelegate(string value);
        delegate void ImageArgReturningVoidDelegate(System.Drawing.Image value);
        delegate void ContactArgReturningVoidDelegate(Contact value);
        delegate void VoidDelegate();

    #region INIT METHODS

        public SampleContactForm()
        {
            InitializeComponent();
            tbLogin.Text = LOGIN_USER1;
            tbPassword.Text = PASSWORD_USER1;

            log.Info("==============================================================");
            log.Info("SampleContact started");

            InitializeRainbowSDK();
        }

        private void InitializeRainbowSDK()
        {
            rainbowApplication = new Rainbow.Application(); ;

            // Set Application Id, Secret Key and Host Name
            rainbowApplication.SetApplicationInfo(APP_ID, APP_SECRET_KEY);
            rainbowApplication.SetHostInfo(HOST_NAME);

            // Rainbow.Contacts
            rainbowContacts = rainbowApplication.GetContacts();


            // EVENTS WE WANT TO MANAGE
            rainbowApplication.ConnectionStateChanged += RainbowApplication_ConnectionStateChanged;

            rainbowContacts.ContactInfoChanged += RainbowContacts_ContactInfoChanged;
            rainbowContacts.RosterContactAdded += RainbowContacts_RosterContactAdded;
            rainbowContacts.RosterContactRemoved += RainbowContacts_RosterContactRemoved;

            rainbowContacts.ContactAvatarChanged += RainbowContacts_ContactAvatarChanged;
            rainbowContacts.ContactAvatarDeleted += RainbowContacts_ContactAvatarDeleted;

            rainbowContactsList = new List<Contact>();
            rainbowContactsListFound = new List<Contact>(); ;
        }

    #endregion INIT METHODS
        
    #region METHOD TO UPDATE SampleContactForm COMPONEnTS

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
            if (tbState.InvokeRequired)
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
        /// Permits to display info about My Contact
        /// </summary>
        private void UpdateMyContactElements()
        {
            if (tbState.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateMyContactElements);
                this.Invoke(d);
            }
            else
            {
                tbFirstName.Text = rainbowMyContact.FirstName;
                tbLastName.Text = rainbowMyContact.LastName;
                tbDisplayName.Text = rainbowMyContact.DisplayName;
                tbNickName.Text = rainbowMyContact.NickName;
                tbTitle.Text = rainbowMyContact.Title;
                tbJobTitle.Text = rainbowMyContact.JobTitle;
            }
        }

        /// <summary>
        /// Permits to update avatar display of my contact
        /// </summary>
        private void UpdateMyAvatar(System.Drawing.Image img)
        {
            if (pbMyContact.InvokeRequired)
            {
                ImageArgReturningVoidDelegate d = new ImageArgReturningVoidDelegate(UpdateMyAvatar);
                this.Invoke(d, new object[] { img });
            }
            else
            {
                pbMyContact.Image = img;
            }
        }

        /// <summary>
        /// Permits to update avatar display of the selected contact
        /// </summary>
        private void UpdateContactAvatar(System.Drawing.Image img)
        {
            if (pbMyContact.InvokeRequired)
            {
                ImageArgReturningVoidDelegate d = new ImageArgReturningVoidDelegate(UpdateContactAvatar);
                this.Invoke(d, new object[] { img });
            }
            else
            {
                pbContact.Image = img;
            }
        }

        /// <summary>
        /// Permits to udpate combo bow with list of contacts list
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
                cbContactsListSearch.SelectedIndex = -1;

                // First clear combo box
                cbContactsList.Items.Clear();

                // Fill the combo bow with new elements
                foreach(Contact contact in rainbowContactsList)
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
                {
                    cbContactsList.SelectedIndex = 0;

                    // Allow to remove this contact from roster
                    btnRosterAddRemove.Enabled = true;
                    btnRosterAddRemove.Text = "Remove";
                }
                else
                    btnRosterAddRemove.Enabled = false;
            }
        }

        /// <summary>
        /// Permits to udpate combo box with list of contacts list found by search
        /// </summary>
        private void UpdateContactsListFoundComboBox()
        {
            if (cbContactsListSearch.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateContactsListFoundComboBox);
                this.Invoke(d);
            }
            else
            {
                // Unselect the selected contact from both combo box
                cbContactsList.SelectedIndex = -1;
                cbContactsListSearch.SelectedIndex = -1;

                // First clear combo box
                cbContactsListSearch.Items.Clear();

                // Fill the combo bow with new elements
                foreach (Contact contact in rainbowContactsListFound)
                {
                    // Avoid to add my contact in the list
                    if (contact.Id != rainbowMyContact.Id)
                    {
                        string displayName = contact.DisplayName;
                        if (String.IsNullOrEmpty(displayName))
                            displayName = $"{contact.LastName} {contact.FirstName}";
                        ListItem item = new ListItem(displayName, contact.Id);
                        cbContactsListSearch.Items.Add(item);
                    }
                }

                // Select the first contact
                if (cbContactsListSearch.Items.Count > 0)
                {
                    cbContactsListSearch.SelectedIndex = 0;
                    
                }
                else
                    btnRosterAddRemove.Enabled = false;
            }
        }

        /// <summary>
        /// Permits to udpate info about selected contact
        /// </summary>
        private void UpdateDetailsWithContactSelected(Contact contact)
        {
            if (tbContactId.InvokeRequired)
            {
                ContactArgReturningVoidDelegate d = new ContactArgReturningVoidDelegate(UpdateDetailsWithContactSelected);
                this.Invoke(d, new object[] { contact });
            }
            else
            {
                tbContactId.Text = contact.Id;
                tbContactJid.Text = contact.Jid_im;
                tbContactFirstName.Text = contact.FirstName;
                tbContactLastName.Text = contact.LastName;
                tbContactDisplayName.Text = contact.DisplayName;
                tbContactNickName.Text = contact.NickName;
                tbContactTitle.Text = contact.Title;
                tbContactJobTitle.Text = contact.JobTitle;
            }
        }

    #endregion METHOD TO UPDATE SampleContactForm COMPONEnTS

    #region EVENTS FIRED BY RAINBOW SDK

        private void RainbowApplication_ConnectionStateChanged(object sender, Rainbow.Events.ConnectionStateEventArgs e)
        {
            // Add info about connection state
            AddStateLine($"ConnectionStateChanged:{e.State}");
            UpdateLoginButton(e.State);
        }

        private void RainbowContacts_ContactAvatarDeleted(object sender, Rainbow.Events.JidEventArgs e)
        {
            if (e.Jid == rainbowMyContact.Jid_im)
                AddStateLine($"The server has confirmed the delete of my contact avatar");
            else
                AddStateLine($"A contact has deleted its avatar - JID:[{e.Jid}]");
        }

        private void RainbowContacts_ContactAvatarChanged(object sender, Rainbow.Events.JidEventArgs e)
        {
            if(e.Jid == rainbowMyContact.Jid_im)
                AddStateLine($"The server has confirmed the update of my contact avatar");
            else
                AddStateLine($"A contact has changed its avatar - JID:[{e.Jid}]");
        }

        private void RainbowContacts_RosterContactRemoved(object sender, Rainbow.Events.JidEventArgs e)
        {
            AddStateLine($"A contact has been removed from your roster - JID:[{e.Jid}]");
            GetAllContacts();
        }

        private void RainbowContacts_RosterContactAdded(object sender, Rainbow.Events.JidEventArgs e)
        {
            AddStateLine($"A new contact has been added in your roster - JID:[{e.Jid}]");
            GetAllContacts();
        }

        private void RainbowContacts_ContactInfoChanged(object sender, Rainbow.Events.JidEventArgs e)
        {
            if (e.Jid == rainbowMyContact.Jid_im)
                AddStateLine($"The server has confirmed the update of my contact information");
            else
                AddStateLine($"A contact has changed its information - JID:[{e.Jid}]");
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
                    if(!callback.Result.Success)
                        log.WarnFormat("Impossible to logout:\r\n{0}", Util.SerialiseSdkError(callback.Result));
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

                        if (String.IsNullOrEmpty(rainbowMyContact.Timezone))
                            rainbowMyContact.Timezone = "Europe/Paris";

                        UpdateMyContactElements();
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

        private void btnMyContactUpdate_Click(object sender, EventArgs e)
        {
            if (!rainbowApplication.IsConnected())
                return;

            rainbowMyContact.FirstName = tbFirstName.Text;
            rainbowMyContact.LastName = tbLastName.Text;
            rainbowMyContact.DisplayName = tbDisplayName.Text;
            rainbowMyContact.NickName = tbNickName.Text;
            rainbowMyContact.Title = tbTitle.Text;
            rainbowMyContact.JobTitle = tbJobTitle.Text;

            rainbowContacts.UpdateCurrentContact(rainbowMyContact, callback =>
            {
                if(callback.Result.Success)
                {
                    AddStateLine("My Contact has been well updated");
                }
                else
                {
                    string logLine = String.Format("Impossible to update avatar:\r\n{0}", Util.SerialiseSdkError(callback.Result));
                    AddStateLine(logLine);
                    log.WarnFormat(logLine);
                }
            });
        }

        private void btnAvatarGet_Click(object sender, EventArgs e)
        {
            GetAvatarFromCurrentContact();
        }

        private void btnAvatarDelete_Click(object sender, EventArgs e)
        {
            if (!rainbowApplication.IsConnected())
                return;

            rainbowContacts.DeleteAvatarFromCurrentContact(callback =>
            {
                if(callback.Result.Success)
                {
                    AddStateLine("Avatar has been deleted");
                    UpdateMyAvatar(null);
                }
                else
                {
                    string logLine = String.Format("Impossible to update avatar:\r\n{0}", Util.SerialiseSdkError(callback.Result));
                    AddStateLine(logLine);
                    log.WarnFormat(logLine);
                }
            });

        }

        private void btnAvatarUpdate_Click(object sender, EventArgs e)
        {
            if (!rainbowApplication.IsConnected())
                return;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image PNG (*.png)|*.png|Image JPG (*.jpg)|*.jpg";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    string extension;
                    if (filePath != null)
                    {
                        if (File.Exists(filePath))
                        {
                            extension = Path.GetExtension(filePath).ToUpper();
                            extension = extension.Substring(1, extension.Length - 1);
                            if ((extension == "PNG") || (extension == "JPG"))
                            {
                                byte[] data = File.ReadAllBytes(filePath);

                                rainbowContacts.UpdateAvatarFromCurrentContact(ref data, extension, callback =>
                                {
                                    if (callback.Result.Success)
                                    {
                                        AddStateLine($"Avatar updated successfully");
                                        GetAvatarFromCurrentContact();
                                    }
                                    else
                                    {
                                        string logLine = String.Format("Impossible to update avatar:\r\n{0}", Util.SerialiseSdkError(callback.Result));
                                        AddStateLine(logLine);
                                        log.WarnFormat(logLine);
                                    }
                                });
                            }
                            else
                                AddStateLine($"Image file can be PNG or JPG only");
                        }
                        else
                            AddStateLine($"Avatar can't be updated - file not exists");
                    }
                }
            }
        }

        private void btnContactsList_Click(object sender, EventArgs e)
        {
            GetAllContacts();
        }

        private void cbContactsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListItem item = (ListItem)cbContactsList.SelectedItem;
            if(item != null)
            {
                cbContactsListSearch.SelectedIndex = -1;

                string id = item.Value;
                Contact selectedContact = rainbowContacts.GetContactFromContactId(id);
                if(selectedContact != null)
                    UpdateDetailsWithContactSelected(selectedContact);
            }
        }

        private void btnContactAvatar_Click(object sender, EventArgs e)
        {
            if (!rainbowApplication.IsConnected())
                return;

            string id = tbContactId.Text;
            if(!String.IsNullOrEmpty(id))
            {
                rainbowContacts.GetAvatarFromContactId(id, 80, callback =>
                {
                    if (callback.Result.Success)
                    {
                        byte[] data = callback.Data;
                        System.Drawing.Image img = GetImageFromBytes(ref data);
                        if (img != null)
                        {
                            UpdateContactAvatar(img);
                            AddStateLine("Avatar has been retrieved from this contact");
                        }
                        else
                            AddStateLine("This contact has no avatar");
                    }
                    else
                    {
                        string logLine = String.Format("Impossible to get avatar of this contact:\r\n{0}", Util.SerialiseSdkError(callback.Result));
                        AddStateLine(logLine);
                        log.WarnFormat(logLine);
                    }
                });
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (!rainbowApplication.IsConnected())
                return;

            string search = tbSearch.Text;
            if(!String.IsNullOrEmpty(search))
            {
                rainbowContacts.SearchContactsByDisplayName(search, 20, callback =>
                {
                    if (callback.Result.Success)
                    {
                        SearchContactsResult result = callback.Data;
                        rainbowContactsListFound = result.ContactsList;
                        AddStateLine($"Nb rainbow contacts found with [{search}]: {rainbowContactsListFound.Count}");
                        UpdateContactsListFoundComboBox();
                    }
                    else
                    {
                        string logLine = String.Format("Impossible to get avatar of this contact:\r\n{0}", Util.SerialiseSdkError(callback.Result));
                        AddStateLine(logLine);
                        log.WarnFormat(logLine);
                    }
                });
            }
        }

        private void cbContactsListSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListItem item = (ListItem)cbContactsListSearch.SelectedItem;
            if (item != null)
            {
                cbContactsList.SelectedIndex = -1;

                string id = item.Value;

                // Allow to add or remove in roster the selected contact
                btnRosterAddRemove.Enabled = true;
                if (rainbowContacts.GetContactFromContactId(id) == null)
                    btnRosterAddRemove.Text = "Add";
                else
                    btnRosterAddRemove.Text = "Remove";

                rainbowContacts.GetContactFromContactIdFromServer(id, callback =>
                {
                    if(callback.Result.Success)
                    {
                        UpdateDetailsWithContactSelected(callback.Data);
                    }
                    else
                    {
                        string logLine = String.Format("Impossible to get info about this contact [{1}]:\r\n{0}", Util.SerialiseSdkError(callback.Result), id);
                        AddStateLine(logLine);
                        log.WarnFormat(logLine);
                    }
                });
            }
        }

        private void btnRosterAddRemove_Click(object sender, EventArgs e)
        {
            if (!rainbowApplication.IsConnected())
                return;

            String id = tbContactId.Text;
            if(!String.IsNullOrEmpty(id))
            {
                Contact contact = rainbowContacts.GetContactFromContactId(id);
                if(contact == null)
                {
                    // We want to add it in the roster
                    Invitations invitations = rainbowApplication.GetInvitations();
                    invitations.SendInvitationByContactId(id, callback =>
                    {
                        if(callback.Result.Success)
                            AddStateLine($"Invitation sent successfully to this contact [{id}]");
                        else
                        {
                            string logLine = String.Format("Impossible to send invitation to this contact [{1}]:\r\n{0}", Util.SerialiseSdkError(callback.Result), id);
                            AddStateLine(logLine);
                            log.WarnFormat(logLine);
                        }
                    });
                }
                else
                {
                    // We want to remove it from the roster
                    rainbowContacts.RemoveFromContactId(id, callback =>
                    {
                        if (callback.Result.Success)
                            AddStateLine($"Contact [{id}] has been removed from your roster");
                        else
                        {
                            string logLine = String.Format("Impossible to remove this contact [{1}] from your roster:\r\n{0}", Util.SerialiseSdkError(callback.Result), id);
                            AddStateLine(logLine);
                            log.WarnFormat(logLine);
                        }
                    });
                }
            }
        }

    #endregion EVENTS FIRED BY SampleContactForm ELEMENTS

    #region UTIL METHODS

        private void GetAvatarFromCurrentContact()
        {
            if (!rainbowApplication.IsConnected())
                return;

            rainbowContacts.GetAvatarFromCurrentContact(80, callback =>
            {
                if (callback.Result.Success)
                {
                    byte[] data = callback.Data;
                    System.Drawing.Image img = GetImageFromBytes(ref data);
                    if (img != null)
                    {
                        UpdateMyAvatar(img);
                        AddStateLine("Avatar has been retrieved");
                    }
                    else
                        AddStateLine("My contact doesn't have an avatar yet.");
                }
                else
                {
                    string logLine = String.Format("Impossible to get avatar of my contact:\r\n{0}", Util.SerialiseSdkError(callback.Result));
                    AddStateLine(logLine);
                    log.WarnFormat(logLine);
                }
            });
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

        private System.Drawing.Image GetImageFromBytes(ref byte[] data)
        {
            System.Drawing.Image result = null;
            try
            {
                using (var ms = new MemoryStream(data))
                    result = System.Drawing.Image.FromStream(ms);
            }
            catch (Exception e)
            {
                log.WarnFormat("[GetImageFromBytes] Exception\r\n", Rainbow.Util.SerializeException(e));
            }
            return result;
        }

        #endregion UTIL METHODS


    }
}
