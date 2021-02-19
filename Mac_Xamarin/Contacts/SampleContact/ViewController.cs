using AppKit;
using Foundation;

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Rainbow;
using Rainbow.Model;

using NLog;


namespace SampleContact
{
    public partial class ViewController : NSViewController
    {
        private static readonly Logger log = Rainbow.LogConfigurator.GetLogger(typeof(ViewController));

        private readonly String LogfileName = "Rainbow_CSharp_Example.log";
        private readonly String LogArchivefileName = "RRainbow_CSharp_Example_{###}.log";
        private readonly String LogConfigurationfileName = "NLogConfiguration.xml";
        private readonly String LogFolderName = "Rainbow.CSharp.SDK";

        internal readonly string APP_ID = "";
        internal readonly string APP_SECRET_KEY = "";
        internal readonly string HOST_NAME = "";

        // Login, Password JId Node to connect on server
        internal readonly string LOGIN = "";
        internal readonly string PASSWORD = "";

        // Define Rainbow objects
        Rainbow.Application rainbowApplication;     // To store Rainbow Application object
        Contact rainbowMyContact;     // To store My contact (i. e. the one connected to the Rainbow Server)
        Rainbow.Contacts rainbowContacts;           // To store Rainbow Contacts object
        List<Contact> rainbowContactsList;     // To store the contacts list of my roster
        List<Contact> rainbowContactsListFound;


        // To associate the combobox index to the id of the contact
        Dictionary<int, string> values = new Dictionary<int, string>();
        Dictionary<int, string> valuesFound = new Dictionary<int, string>();

        #region INIT METHODS

        public ViewController(IntPtr handle) : base(handle)
        {
            InitLogs();

            InitializeRainbowSDK();
        }

        private void InitLogs()
        {
            String logFileName = LogfileName; // File name of the log file
            String archiveLogFileName = LogArchivefileName; // File name of the archive log file
            String logConfigFileName = LogConfigurationfileName; // File path to log configuration

            String logConfigContent; // Content of the log file configuration
            String logFullPathFileName; // Full path to log file
            String archiveLogFullPathFileName; ; // Full path to archive log file 

            try
            {
                // Set full path to log file name
                logFullPathFileName = "./" + LogConfigurationfileName;
                archiveLogFullPathFileName = "./" + archiveLogFileName;

                // Get content of the log file configuration
                Stream stream = GetStreamFromFile(logConfigFileName);
                using (StreamReader sr = new StreamReader(stream))
                {
                    logConfigContent = sr.ReadToEnd();
                }
                stream.Dispose();

                // Load XML in XMLDocument
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(logConfigContent);

                // Set full path to log file in XML element
                XmlElement targetElement = doc["nlog"]["targets"]["target"];
                targetElement.SetAttribute("name", Rainbow.LogConfigurator.GetRepositoryName());    // Set target name equals to RB repository name
                targetElement.SetAttribute("fileName", logFullPathFileName);                        // Set full path to log file
                targetElement.SetAttribute("archiveFileName", archiveLogFullPathFileName);          // Set full path to archive log file

                XmlElement loggerElement = doc["nlog"]["rules"]["logger"];
                loggerElement.SetAttribute("writeTo", Rainbow.LogConfigurator.GetRepositoryName()); // Write to RB repository name

                // Set the configuration
                Rainbow.LogConfigurator.Configure(doc.OuterXml);
            }
            catch { }
        }


        private Stream GetStreamFromFile(String pathFileName)
        {
            return File.OpenRead(pathFileName);
        }

        private void InitializeRainbowSDK()
        {
            rainbowApplication = new Rainbow.Application();

            // Set Application Id, Secret Key and Host Name
            rainbowApplication.SetApplicationInfo(APP_ID, APP_SECRET_KEY);
            rainbowApplication.SetHostInfo(HOST_NAME);

            // Rainbow.Contacts
            rainbowContacts = rainbowApplication.GetContacts();

            // Events we want to manage
            rainbowApplication.ConnectionStateChanged += RainbowApplication_ConnectionStateChanged;

            rainbowContacts.ContactInfoChanged += RainbowContacts_ContactInfoChanged;
            rainbowContacts.RosterContactAdded += RainbowContacts_RosterContactAdded;
            rainbowContacts.RosterContactRemoved += RainbowContacts_RosterContactRemoved;
            rainbowContacts.ContactAvatarChanged += RainbowContacts_ContactAvatarChanged;
            rainbowContacts.ContactAvatarDeleted += RainbowContacts_ContactAvatarRemoved;

            rainbowContactsList = new List<Contact>();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            txt_login.StringValue = LOGIN;
            txt_password.StringValue = PASSWORD;

            // Do any additional setup after loading the view.
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

        partial void ConnectClicked(Foundation.NSObject sender)
        {
            ConnectToRainbow();
        }
    
        partial void GetMyAvatarClicked(NSObject sender)
        {
            GetAvatarFromCurrentContact();
        }

        partial void DeleteMyAvatarClicked(NSObject sender)
        {
            DeleteAvatarFromCurrentContact();
        }

        partial void UpdateMyAvatarClicked(NSObject sender)
        {
            UpdateAvatarFromCurrentContact();
        }

        partial void GetContactsClicked(Foundation.NSObject sender)
        {
            GetAllContacts();
        }

        partial void ContactsListSelectionChanged(NSObject sender)
        {
            BeginInvokeOnMainThread(() => { UpdateDetailsSelectedContact(); });
        }

        partial void UpdateInfoClicked(NSObject sender)
        {
            UpdateInfoFromCurrentContact();
        }

        partial void FoundListSelectionChanged(NSObject sender)
        {
            GetContactFromId();
        }

        partial void GetContactAvatarClicked(NSObject sender)
        {
            GetAvatarFromChosenContact();
        }

        partial void btnRosterClicked(NSObject sender)
        {
            ManageRoster();
        }

        partial void SearchContactClicked(NSObject sender)
        {
            SearchContact();
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
                        string logline = String.Format("Impossible to logout:\r\n{0}", Util.SerializeSdkError(callback.Result));
                        AddStateLine(logline);
                        log.Warn(logline);
                    }
                    else
                        {
                        BeginInvokeOnMainThread(ClearMyContactElements);
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
                        BeginInvokeOnMainThread(UpdateMyContactElements);
                    }
                    else
                    {
                        string logline = String.Format("Impossible to login:\r\n{0}", Util.SerializeSdkError(callback.Result));
                        AddStateLine(logline);
                        log.Warn(logline);
                    }
                });
            }
        }

        private void GetAllContacts()
        {
            if(!rainbowApplication.IsConnected())
                return;
            
            rainbowContacts.GetAllContacts(callback =>
            {
                if (callback.Result.Success)
                {
                    rainbowContactsList.Clear();

                    rainbowContactsList = callback.Data;
                    BeginInvokeOnMainThread(() => UpdateContactsListComboBox());
                } else
                {
                    string logline = String.Format("Impossible to get all contacts:\r\n{0}", Util.SerializeSdkError(callback.Result));
                    AddStateLine(logline);
                    log.Warn(logline);
                }
            });
        }

        private void GetAvatarFromCurrentContact()
        {
            if(!rainbowApplication.IsConnected())
                return;

            rainbowContacts.GetAvatarFromCurrentContact(80, callback =>
            {
                if (callback.Result.Success)
                {
                    byte[] data = callback.Data;

                    if (data != null)
                    {
                        BeginInvokeOnMainThread(() => UpdateMyAvatar(data));
                        AddStateLine("Your avatar has been retrieved");
                    }
                    else
                    {
                        AddStateLine("You don't have an avatar yet");
                    }
                } else
                {
                    string logline = String.Format("Impossible to get my avatar:\r\n{0}", Util.SerializeSdkError(callback.Result));
                    AddStateLine(logline);
                    log.Warn(logline);
                }
            });
        }

        private void DeleteAvatarFromCurrentContact()
        {
            if (!rainbowApplication.IsConnected())
                return;

            rainbowContacts.DeleteAvatarFromCurrentContact(callback =>
            {
                if (callback.Result.Success)
                {
                    AddStateLine("Your avatar has been deleted");
                    BeginInvokeOnMainThread(() => UpdateMyAvatar(null));
                }
                else
                {
                    string logline = String.Format("Impossible to delete your avatar:\r\n{0}", Util.SerializeSdkError(callback.Result));
                    AddStateLine(logline);
                    log.Warn(logline);
                }
            });
        }

        private void UpdateAvatarFromCurrentContact()
        {
            if (!rainbowApplication.IsConnected())
                return;

            var dlg = NSOpenPanel.OpenPanel;
            dlg.CanChooseFiles = true;
            dlg.CanChooseDirectories = true;
            dlg.AllowedFileTypes = new string[] { "png", "jpg" };

            if (dlg.RunModal() == 1)
            {
                var url = dlg.Urls[0];

                if (url != null)
                {
                    var path = url.Path;

                    string ext = Path.GetExtension(path).ToUpper();
                    ext = ext.Substring(1, ext.Length - 1);

                    if (ext == "PNG" || ext == "JPG")
                    {
                        byte[] data = File.ReadAllBytes(path);
                        rainbowContacts.UpdateAvatarFromCurrentContact(ref data, ext, callback =>
                        {
                            if (callback.Result.Success)
                            {
                                AddStateLine("Avatar successfully updated");
                                GetAvatarFromCurrentContact();
                            }
                            else
                            {
                                string logline = String.Format("Impossible to update your avatar:\r\n{0}", Util.SerializeSdkError(callback.Result));
                                AddStateLine(logline);
                                log.Warn(logline);
                            }
                        });
                    }
                    else
                    {
                        AddStateLine($"Image file can only be of type PNG or JPG");
                    }
                }
                else
                {
                    AddStateLine($"Your avatar can't be updated - file doesn't exist");
                }
            }
        }

        private void UpdateInfoFromCurrentContact()
        {
            if (!rainbowApplication.IsConnected())
                return;

            rainbowMyContact.FirstName = txt_firstname.StringValue;
            rainbowMyContact.LastName = txt_lastname.StringValue;
            rainbowMyContact.NickName = "" + txt_nickname.StringValue;
            rainbowMyContact.DisplayName = "" + txt_displayname.StringValue;
            rainbowMyContact.Title = "" + txt_title.StringValue;
            rainbowMyContact.JobTitle = "" + txt_jobtitle.StringValue;

            rainbowContacts.UpdateCurrentContact(rainbowMyContact, callback =>
            {
                if (callback.Result.Success)
                {
                    AddStateLine("Your contact info has been updated");
                }
                else
                {
                    string logline = String.Format("Impossible to update your contact info:\r\n{0}", Util.SerializeSdkError(callback.Result));
                    AddStateLine(logline);
                    log.Warn(logline);
                }
            });
        }

        private void GetAvatarFromChosenContact()
        {
            if (!rainbowApplication.IsConnected())
                return;

            string id = contact_id.StringValue;

            rainbowContacts.GetAvatarFromContactId(id, 80, callback =>
            {
                if (callback.Result.Success)
                {
                    byte[] data = callback.Data;

                    if (data != null)
                    {
                        BeginInvokeOnMainThread(() => UpdateContactAvatar(data));
                        AddStateLine("Avatar has been retrieved");
                    }
                    else
                    {
                        AddStateLine("This contact doesn't have an avatar yet");
                    }
                }
                else
                {
                    string logline = String.Format("Impossible to get the avatar of this contact [{1}]:\r\n{0}", Util.SerializeSdkError(callback.Result), id);
                    AddStateLine(logline);
                    log.Warn(logline);
                }
            });
        }

        private void ManageRoster()
        {
            if (!rainbowApplication.IsConnected())
                return;

            string id = contact_id.StringValue;
            if(!String.IsNullOrEmpty(id))
            {
                Contact contact = rainbowContacts.GetContactFromContactId(id);

                if(contact == null)
                {
                    // We want to add it to the roster
                    Invitations invit = rainbowApplication.GetInvitations();
                    invit.SendInvitationByContactId(id, callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddStateLine($"Invitation sent to this contact [{id}]");
                        }
                        else
                        {
                            string logline = String.Format("Impossible to send an invitation to this contact [{1}]:\r\n{0}", Util.SerializeSdkError(callback.Result), id);
                            AddStateLine(logline);
                            log.Warn(logline);
                        }
                    });
                } else
                {
                    // We cant to remove from the roster
                    rainbowContacts.RemoveFromContactId(id, callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddStateLine($"Contact [{id}] has been removed from your roster");
                        }
                        else
                        {
                            string logline = String.Format("Impossible to remove this contact [{1}] from your roster:\r\n{0}", Util.SerializeSdkError(callback.Result), id);
                            AddStateLine(logline);
                            log.Warn(logline);
                        }
                    });
                }
            }
        }

        private void SearchContact()
        {
            if (!rainbowApplication.IsConnected())
                return;

            string search = txt_search.StringValue;

            if(!String.IsNullOrEmpty(search))
            {
                rainbowContacts.SearchContactsByDisplayName(search, 20, callback =>
                {
                    if(callback.Result.Success)
                    {
                        SearchContactsResult result = callback.Data;
                        rainbowContactsListFound = result.ContactsList;
                        AddStateLine($"Nb Rainbow contacts found with [{search}]: {rainbowContactsListFound.Count}");
                        BeginInvokeOnMainThread(() => UpdateContactsListFoundCombobox());
                    } else
                    {
                        string logline = String.Format("Impossible to search this [{1}]:\r\n{0}", Util.SerializeSdkError(callback.Result), search);
                        AddStateLine(logline);
                        log.Warn(logline);
                    }
                });
            }
        }

        private void GetContactFromId()
        {
            int index = (int)cbContactsListFound.IndexOfSelectedItem;

            string id = valuesFound[index];

            if (rainbowContacts.GetContactFromContactId(id) == null)
            {
                BeginInvokeOnMainThread(() => { btnRoster.Title = "Add"; });
            }
            else
            {
                BeginInvokeOnMainThread(() => { btnRoster.Title = "Remove"; });
            }

            rainbowContacts.GetContactFromContactIdFromServer(id, callback =>
            {
                if (callback.Result.Success)
                {
                    BeginInvokeOnMainThread(() => { UpdateDetailsFoundContact(callback.Data); });
                }
                else
                {
                    string logline = String.Format("Impossible to get info about this contact [{1}]:\r\n{0}", Util.SerializeSdkError(callback.Result), id);
                }
            });
        }

        #endregion UTILS

        #region METHODS TO UPDATE UI

        private void AddStateLine(String info)
        {
            BeginInvokeOnMainThread(() => { txt_state.Value = info + "\r\n" + txt_state.Value; });            
        }

        private void UpdateLoginButton(string connectionState)
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
        }

        private void UpdateMyContactElements()
        {
            box.Hidden = false;

            txt_firstname.StringValue = "" + rainbowMyContact.FirstName;
            txt_lastname.StringValue = "" + rainbowMyContact.LastName;
            txt_nickname.StringValue = "" + rainbowMyContact.NickName;
            txt_displayname.StringValue = "" + rainbowMyContact.DisplayName;
            txt_title.StringValue = "" + rainbowMyContact.Title;
            txt_jobtitle.StringValue = "" + rainbowMyContact.JobTitle;
        }

        private void ClearMyContactElements()
        {
            box.Hidden = true;
            boxContact.Hidden = true;

            txt_firstname.StringValue = "";
            txt_lastname.StringValue = "";
            txt_nickname.StringValue = "";
            txt_displayname.StringValue = "";
            txt_title.StringValue = "";
            txt_jobtitle.StringValue = "";
        }

        private void UpdateMyAvatar(byte[] img)
        {
            if(img == null)
            {
                imgMyAvatar.Image = null;
            } else
            {
                NSData nsdata = NSData.FromArray(img);
                NSImage image = new NSImage(nsdata);
                imgMyAvatar.Image = image;
            }
        }

        private void UpdateContactsListComboBox()
        {
            boxContact.Hidden = false;
            cbContactsList.RemoveAllItems();
            values.Clear();

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

                    values.Add(index,contact.Id);
                    index++;

                    cbContactsList.AddItem(new NSString(displayName));
                }
            }
        }

        private void UpdateDetailsSelectedContact()
        {
            int index = (int) cbContactsList.IndexOfSelectedItem;

            string id = values[index];

            Contact contact = rainbowContacts.GetContactFromContactId(id);

            contact_id.StringValue = "" + contact.Id;
            contact_firstname.StringValue = "" + contact.FirstName;
            contact_lastname.StringValue = "" + contact.LastName;
            contact_nickname.StringValue = "" + contact.NickName;
            contact_jobtitle.StringValue = "" + contact.JobTitle;

            btnRoster.Enabled = true;
            btnRoster.Title = "Remove";
        }

        private void UpdateDetailsFoundContact(Contact contact)
        {     
            contact_id.StringValue = "" + contact.Id;
            contact_firstname.StringValue = "" + contact.FirstName;
            contact_lastname.StringValue = "" + contact.LastName;
            contact_nickname.StringValue = "" + contact.NickName;
            contact_jobtitle.StringValue = "" + contact.JobTitle;
        }

        private void UpdateContactsListFoundCombobox()
        {
            cbContactsListFound.RemoveAllItems();
            valuesFound.Clear();

            int index = 0;

            foreach(Contact contact in rainbowContactsListFound)
            {
                if(contact.Id != rainbowMyContact.Id)
                {
                    string displayname = contact.DisplayName;

                    if(String.IsNullOrEmpty(displayname))
                    {
                        displayname = $"{contact.FirstName} {contact.LastName}";
                    }

                    valuesFound.Add(index, contact.Id);
                    index++;

                    cbContactsListFound.AddItem(new NSString(displayname));
                }
            }
        }

        private void UpdateContactAvatar(byte[] img)
        {
            if (img == null)
            {
                imgContactAvatar.Image = null;
            }
            else
            {
                NSData nsdata = NSData.FromArray(img);
                NSImage image = new NSImage(nsdata);
                imgContactAvatar.Image = image;
            }
        }

        #endregion METHODS TO UPDATE UI  

        #region EVENTS FIRED BY RAINBOW SDK

        private void RainbowContacts_ContactAvatarRemoved(object sender, Rainbow.Events.JidEventArgs e)
        {
            if (e.Jid == rainbowMyContact.Jid_im)
            {
                AddStateLine($"Your avatar has been deleted");
            }
            else
            {
                AddStateLine($"A contact has removed his avatar - JID:[{e.Jid}]");
            }
        }

        private void RainbowContacts_ContactAvatarChanged(object sender, Rainbow.Events.JidEventArgs e)
        {
            if(e.Jid == rainbowMyContact.Jid_im)
            {
                AddStateLine($"The server has confirmed the update of your contact avatar");
            }
            else
            {
                AddStateLine($"A contact has changed his avatar - JID:[{e.Jid}]");
            }
        }

        private void RainbowContacts_RosterContactRemoved(object sender, Rainbow.Events.JidEventArgs e)
        {
            AddStateLine($"A contact has been removed from your roster - JID:[{e.Jid}]");
            GetAllContacts();
        }

        private void RainbowContacts_RosterContactAdded(object sender, Rainbow.Events.JidEventArgs e)
        {
            AddStateLine($"A new contact has been added to your roster - JID:[{e.Jid}]");
            GetAllContacts();
        }

        private void RainbowContacts_ContactInfoChanged(object sender, Rainbow.Events.JidEventArgs e)
        {
            //TODO not implemented
        }

        private void RainbowApplication_ConnectionStateChanged(object sender, Rainbow.Events.ConnectionStateEventArgs e)
        {
            // Add info about connection state
            BeginInvokeOnMainThread(() => { UpdateLoginButton(e.State); });
            AddStateLine($"Connection state changed: {e.State}");
        }

        #endregion EVENTS FIRED BY RAINBOW SDK
    }
}