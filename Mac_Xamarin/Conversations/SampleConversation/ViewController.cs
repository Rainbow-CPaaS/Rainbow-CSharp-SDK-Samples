using AppKit;
using Foundation;

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Rainbow;
using Rainbow.Model;

using NLog;


namespace SampleConversation
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
        Application rainbowApplication;         // To store Rainbow Application object
        Rainbow.Contacts rainbowContacts;     // To store Rainbow Contacts object
        Conversations rainbowConversations;     // To store Rainbow Conversations object
        Favorites rainbowFavorites;             // To store Rainbow Favorites object

        Contact rainbowMyContact;               // To store My contact (i. e. the one connected to the Rainbow Server)
        List<Contact> rainbowContactsList;      // To store contacts list of my roster
        List<Conversation> rainbowConvList;     // To store conversations list
        List<Favorite> rainbowFavList;          // To store favorites list


        // To associate the combobox index to the id of the contact
        Dictionary<int, string> valuesContact = new Dictionary<int, string>();
        Dictionary<int, string> valuesConversations = new Dictionary<int, string>();
        Dictionary<int, string> valuesFavorites = new Dictionary<int, string>();

        delegate void VoidDelegate();

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

            // Get Rainbow main objects
            rainbowContacts = rainbowApplication.GetContacts();
            rainbowConversations = rainbowApplication.GetConversations();
            rainbowFavorites = rainbowApplication.GetFavorites();

            // Events we want to manage
            rainbowApplication.ConnectionStateChanged += RainbowApplication_ConnectionStateChanged;

            rainbowContacts.RosterContactAdded += RainbowContacts_RosterContactAdded;
            rainbowContacts.RosterContactRemoved += RainbowContacts_RosterContactRemoved;

            rainbowConversations.ConversationCreated += RainbowConversations_ConversationCreated;
            rainbowConversations.ConversationRemoved += RainbowConversations_ConversationRemoved;

            rainbowFavorites.FavoriteCreated += RainbowFavorites_FavoriteCreated;
            rainbowFavorites.FavoriteRemoved += RainbowFavorites_FavoriteRemoved;

            rainbowContactsList = new List<Contact>();
            rainbowConvList = new List<Conversation>();
            rainbowFavList = new List<Favorite>();
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

        partial void btnLoginClicked(Foundation.NSObject sender)
        {
            ConnectToRainbow();
        }

        partial void btnAddConversation_Clicked(NSObject sender)
        {
            AddConversation();
        }

        partial void btnRemoveConversations_Clicked(NSObject sender)
        {
            RemoveConversation();
        }

        partial void btnContacts_AddToFav_Clicked(NSObject sender)
        {
            int index = (int)cbContacts.IndexOfSelectedItem;

            if (index > -1)
            {
                string id = valuesContact[index];
                CreateFavorite(id, FavoriteType.User);
            }
        }

        partial void btnConversations_AddToFav_Clicked(NSObject sender)
        {
            int index = (int)cbConversations.IndexOfSelectedItem;

            if(index > -1)
            {
                string id = valuesConversations[index];
                Conversation conv = rainbowConversations.GetConversationByIdFromCache(id);

                if(conv == null)
                {
                    AddStateLine($"No conversation found with this ID: {id}");
                    return;
                }

                string peerId = conv.PeerId;

                String favType;

                if (conv.Type == Conversation.ConversationType.User)
                    favType = FavoriteType.User;
                else
                    favType = FavoriteType.Room;

                CreateFavorite(peerId, favType);
            }
        }

        partial void btnUpdateFavPosition_Clicked(NSObject sender)
        {
            UpdateFavoritePosition();
        }

        partial void btnFavoriteRemove_Clicked(NSObject sender)
        {
            RemoveFavorite();
        }

        partial void cbContacts_SelectionChanged(NSObject sender)
        {
            CheckContactSelectedAsFavorite();
            CheckContactSelectedAsConversation();
        }

        partial void cbConversations_SelectionChanged(NSObject sender)
        {
            CheckConversationsSelectedAsFavorite();
        }

        partial void cbFavorites_SelectionChanged(NSObject sender)
        {
            int index = (int)cbFavorites.IndexOfSelectedItem;

            if (index < 0)
            {
                txtPosition.StringValue = "";
            }
            else
            {
                txtPosition.StringValue = index.ToString();
            }
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
                        string logline = String.Format("Impossible to login:\r\n{0}", Util.SerializeSdkError(callback.Result));
                        AddStateLine(logline);
                        log.Warn(logline);
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
                    string logLine = String.Format("Impossible to get all contacts:\r\n{0}", Util.SerializeSdkError(callback.Result));
                    AddStateLine(logLine);
                    log.Warn(logLine);
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
                    string logLine = String.Format("Impossible to get all conversations:\r\n{0}", Util.SerializeSdkError(callback.Result));
                    AddStateLine(logLine);
                    log.Warn(logLine);
                }
            });
        }

        private void GetAllFavorites()
        {
            if (!rainbowApplication.IsConnected())
                return;

            rainbowFavList = rainbowFavorites.GetFavorites();
            
            if(rainbowFavList != null)
            {
                AddStateLine($"Nb favorites: {rainbowFavList.Count}");
                UpdateFavoritesCombobox();
            }
            else
            {
                string logLine = String.Format("Impossible to get all favorites");
                AddStateLine(logLine);
                log.Warn(logLine);
            }
        }

        private void CheckContactSelectedAsFavorite()
        {
            int index = (int) cbContacts.IndexOfSelectedItem;

            if(index > -1) {
                string id = valuesContact[index];

                // Check if the selected item is in favorites
                Favorite fav = rainbowFavorites.GetFavoriteByPeerId(id);
                EnableButton(btnContacts_AddToFav, fav == null);
            }
        }

        private void CheckConversationsSelectedAsFavorite()
        {
            int index = (int)cbConversations.IndexOfSelectedItem;

            if (index > -1)
            {
                string id = valuesConversations[index];

                Conversation conv = rainbowConversations.GetConversationByIdFromCache(id);
                if(conv != null)
                {
                    // Check if the selected item is in favorites
                    Favorite fav = rainbowFavorites.GetFavoriteByPeerId(conv.PeerId);
                    EnableButton(btnConversations_AddToFav, fav == null);
                }
            }
        }

        private void CheckContactSelectedAsConversation()
        {
            int index = (int)cbContacts.IndexOfSelectedItem;

            if (index > -1)
            {
                string id = valuesContact[index];

                // Check if the selected item is in conversations
                String convId = rainbowConversations.GetConversationIdByPeerIdFromCache(id);
                EnableButton(btnContact_AddConv, String.IsNullOrEmpty(convId));
            }
        }

        private void AddConversation()
        {
            int index = (int)cbContacts.IndexOfSelectedItem;

            if(index > -1)
            {
                string id = valuesContact[index];

                rainbowConversations.GetConversationFromContactId(id, callback =>
                {
                    if (callback.Result.Success)
                    {
                        AddStateLine($"A conversation with [{id}] has been created");
                        GetAllConversations();
                    }
                    else
                    {
                        string logLine = String.Format("Impossible to create conversation with [{1}]:\r\n", Util.SerializeSdkError(callback.Result), id);
                        AddStateLine(logLine);
                        log.Warn(logLine);
                    }
                });
            }
        }

        private void RemoveConversation()
        {
            int index = (int)cbConversations.IndexOfSelectedItem;

            if (index > -1)
            {
                string id = valuesConversations[index];

                rainbowConversations.RemoveFromConversationsById(id, callback =>
                {
                    if (callback.Result.Success)
                    {
                        AddStateLine($"Conversation with [{id}] has been removed");
                        GetAllConversations();
                    }
                    else
                    {
                        string logLine = String.Format("Impossible to remove conversation with [{1}]:\r\n", Util.SerializeSdkError(callback.Result), id);
                        AddStateLine(logLine);
                        log.Warn(logLine);
                    }
                });
            }
        }

        private void CreateFavorite(string id, string type)
        {
            rainbowFavorites.CreateFavorite(id, type, callback =>
            {
                if (callback.Result.Success)
                {
                    AddStateLine($"Favorite with [{id}] has been created");
                    GetAllFavorites();
                }
                else
                {
                    string logLine = String.Format("Impossible to create favorite with [{1}]:\r\n", Util.SerializeSdkError(callback.Result), id);
                    AddStateLine(logLine);
                    log.Warn(logLine);
                }
            });

        }

        private void RemoveFavorite()
        {
            int index = (int)cbFavorites.IndexOfSelectedItem;

            if (index > -1)
            {
                string id = valuesFavorites[index];
                rainbowFavorites.DeleteFavorite(rainbowFavorites.GetFavorite(id), callback =>
                {
                    if (callback.Result.Success)
                    {
                        AddStateLine($"Favorite with [{id}] has been removed");
                        GetAllFavorites();
                    }
                    else
                    {
                        string logLine = String.Format("Impossible to remove favorite with [{1}]:\r\n", Util.SerializeSdkError(callback.Result), id);
                        AddStateLine(logLine);
                        log.Warn(logLine);
                    }
                });
            }
        }

        private void UpdateFavoritePosition()
        {
            int index = (int)cbFavorites.IndexOfSelectedItem;

            if (index > -1)
            {
                int result;
                string id = valuesFavorites[index];

                if (int.TryParse(txtPosition.StringValue, out result))
                {
                    if (result > -1)
                    {
                        rainbowFavorites.UpdateFavoritePosition(id, result, callback =>
                        {
                            if (callback.Result.Success)
                            {
                                AddStateLine($"Favorite position of [{id}] has been updated to [{result}]");
                                GetAllFavorites();
                            }
                            else
                            {
                                string logLine = String.Format("Impossible to update favorite position of [{1}]:\r\n{0]", Util.SerializeSdkError(callback.Result), id);
                                AddStateLine(logLine);
                                log.Warn(logLine);
                            }
                        });
                    }
                    else
                    {
                        AddStateLine("Not a positive integer value specified");
                    }
                }
                else
                {
                    AddStateLine("Not an interger value specified");
                }
            }
        }

        #endregion UTILS


        #region METHODS TO UPDATE UI

        private void AddStateLine(String info)
        {
            InvokeOnMainThread(() => { txt_state.Value = info + "\r\n" + txt_state.Value; });
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

                if(cbContacts.IndexOfSelectedItem > 0)
                    cbContacts.SelectItem(0);

                CheckContactSelectedAsFavorite();
                CheckContactSelectedAsConversation();
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

                CheckConversationsSelectedAsFavorite();
                CheckContactSelectedAsConversation();
            });
        }

        private void UpdateFavoritesCombobox()
        {
            InvokeOnMainThread(() =>
            {
                // Clear the combobox
                cbFavorites.RemoveAllItems();
                valuesFavorites.Clear();

                int index = 0;

                // Fill the combobox with new elements
                foreach (Favorite fav in rainbowFavList)
                {
                    string peerId = fav.PeerId;

                    // Is it a favorite with a User?
                    if (fav.Type == FavoriteType.User)
                    {
                        Contact contact = rainbowContacts.GetContactFromContactId(peerId);
                        if (contact != null)
                        {
                            string displayName = contact.DisplayName;

                            if (String.IsNullOrEmpty(displayName))
                                displayName = $"{contact.FirstName} {contact.LastName}";

                            valuesFavorites.Add(index, fav.Id);
                            index++;

                            cbFavorites.AddItem(new NSString(displayName));
                        }
                    }
                    // Is it a favorite with a Bubble/Room?
                    else if (fav.Type == FavoriteType.Room)
                    {
                        string convId = rainbowConversations.GetConversationIdByPeerIdFromCache(peerId);
                        Conversation conv = rainbowConversations.GetConversationByIdFromCache(convId);

                        if (conv != null)
                        {
                            string displayName = conv.Name;

                            valuesFavorites.Add(index, fav.Id);
                            index++;

                            cbFavorites.AddItem(new NSString(displayName));
                        }
                    }
                }
                CheckConversationsSelectedAsFavorite();
                CheckContactSelectedAsFavorite();
            });
        }

        private void EnableButton(NSButton btn, bool enable)
        {
            InvokeOnMainThread(() =>
            {
                btn.Enabled = enable;
            });
        }   

        #endregion METHODS TO UPDATE UI


        #region EVENTS FIRED BY RAINBOW SDK

        private void RainbowApplication_ConnectionStateChanged(object sender, Rainbow.Events.ConnectionStateEventArgs e)
        {
            // Add info about connection state
            UpdateLoginButton(e.State);
            AddStateLine($"Connection state changed: {e.State}");

            if(e.State == ConnectionState.Connected)
            {
                InvokeOnMainThread(() =>
                {
                    box.Hidden = false;
                                
                    GetAllContacts();
                    GetAllConversations();
                    GetAllFavorites();
                });
            } else if(e.State == ConnectionState.Disconnected)
            {
                InvokeOnMainThread(() =>
                {
                    box.Hidden = true;

                    rainbowContactsList.Clear();
                    UpdateContactsCombobox();

                    rainbowConvList.Clear();
                    UpdateConversationsCombobox();

                    rainbowFavList.Clear();
                    UpdateFavoritesCombobox();
                });
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

        private void RainbowFavorites_FavoriteCreated(object sender, Rainbow.Events.FavoriteEventArgs e)
        {
            AddStateLine($"A favorite has been created: [{e.Favorite.Id}]");
            GetAllFavorites();
        }

        private void RainbowFavorites_FavoriteRemoved(object sender, Rainbow.Events.FavoriteEventArgs e)
        {
            AddStateLine($"A favorite has been removed: [{e.Favorite.Id}]");
            GetAllFavorites();
        }

        #endregion EVENTS FIRED BY RAINBOW SDK
    }
}
