using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using InstantMessaging.Helpers;

using Rainbow;
using Rainbow.Events;
using Rainbow.Model;

using log4net;

namespace InstantMessaging.Pool
{
    internal class AvatarsData
    {
        // Store initials and displayName of contact using contact Id as key
        public Dictionary<String, Tuple<String, String>> contactsInfo = new Dictionary<String, Tuple<String, String>>();

        // Store up to 4 members (using contact id) in a bubble.
        public Dictionary<String, List<String>> bubblesMember = new Dictionary<String, List<String>>();
    }

    public sealed class AvatarPool : IImageManagement
    {
        // Thread safe singleton: https://jlambert.developpez.com/tutoriels/dotnet/implementation-pattern-singleton-csharp/

        private static readonly AvatarPool instance = new AvatarPool();

        private static readonly ILog log = LogConfigurator.GetLogger(typeof(AvatarPool));

        private static readonly String UNKNOW_CONTACT_NAME = "?";

        private IImageManagement imageManagement = null;

        private static readonly List<String> colorsList = new List<String>      { "#FF4500", "#D38700", "#348833", "#007356", "#00B2A9", "#00B0E5", "#1B85E0", "#6639B7", "#91278A", "#CF0072", "#A50034", "#D20000" };
        private static readonly List<String> darkerColorsList = new List<String> { "#EB2700", "#B56900", "#166A15", "#005538", "#00948B", "#0092C7", "#1464A8", "#481B99", "#73096C", "#B10054", "#870016", "#B40000" };

        private Rainbow.Application application = null;
        private Rainbow.Contacts contacts = null;
        private Rainbow.Bubbles bubbles = null;
        private Rainbow.Conversations conversations = null;

        private int avatarSize = 60;
        private int avatarFontSize = 24;
        private String avatarFontFamilyName = "GenericSansSerif";

        

        private Boolean allowAvatarDownload = false;
        private BackgroundWorker backgroundWorkerDownload = null;
        private readonly List<String> contactsWithAvatarToDwl = new List<String>(); // Store Id of contacts with Avatar not already downloaded
        private readonly List<String> contactsWithoutAvatar = new List<String>(); // Store Id of contacts without Avatar

        private readonly List<String> bubblesWithAvatarToDwl = new List<String>(); // Store Id of bubbles with Avatar not already downloaded
        private readonly List<String> bubblesWithoutAvatar = new List<String>(); // Store Id of bubbles without Avatar

        private readonly AvatarsData avatarsData = new AvatarsData(); // To store data related to avatars: initials/DisplayName, Bubbles members
        private BackgroundWorker backgroundWorkerUnkownContact = null;

        private Boolean allowToAskContactInfo = false;
        private readonly List<String> contactsUnknownById = new List<String>(); // Store Id of unknow contacts
        private readonly List<String> contactsUnknownByJid = new List<String>(); // Store Jid of unknow contacts

#region Define folder path variables
        // Store folders path where Contact Avatars are stored
        private String folderContactAvatarInitials = null;
        private String folderContactAvatarOriginals = null;
        private String folderContactAvatarRounded = null;

        // Store folders path where Bubble Avatars are stored
        private String folderBubbleAvatarInitials = null;
        private String folderBubbleAvatarOriginals = null;
        private String folderBubbleAvatarRounded = null;

#endregion Define folder path variables

        private Boolean firstNameFirst = true;
        private Boolean initDone = false;


#region PUBLIC EVENTS
        /// <summary>
        /// The event that is raised when a contact avatar is updated (in term of display)
        /// 
        /// It happens when an avatar is added / deleted / updated
        /// 
        /// Contact's Initials are used if the contacts avatr doesn't exits
        /// </summary>
        public event EventHandler<IdEventArgs> ContactAvatarChanged;

        /// <summary>
        /// The event that is raised when a bubble avatar is updated (in term of display)
        /// 
        /// It happens when the bubble's avatar itself is added / deleted / updated
        /// 
        /// It happesn also if the bubble has no avatar but one of member has changed one of its avatar
        /// </summary>
        public event EventHandler<IdEventArgs> BubbleAvatarChanged;

#endregion

#region EVENT FIRED BY RAINBOW SDK

        private void Contacts_ContactInfoChanged(object sender, JidEventArgs e)
        {
            if (!InitDone())
                return;

            Contact contact = contacts.GetContactFromContactJid(e.Jid);
            if (contact != null)
            {
                String displayName = Util.GetContactDisplayName(contact, firstNameFirst);
                String initials = Util.GetContactInitials(contact, firstNameFirst);

                Boolean needInitialUpdate = true;

                if (avatarsData.contactsInfo.ContainsKey(contact.Id))
                {
                    Tuple<String, String> tuple = avatarsData.contactsInfo[contact.Id];
                    if ((tuple.Item1 == initials) && (tuple.Item2 == displayName))
                        needInitialUpdate = false;
                }

                if (needInitialUpdate)
                {
                    avatarsData.contactsInfo.Remove(contact.Id);
                    avatarsData.contactsInfo.Add(contact.Id, new Tuple<string, string>(initials, displayName));

                    String pathInitials = Path.Combine(folderContactAvatarInitials, contact.Id + ".png");
                    try
                    {
                        if (File.Exists(pathInitials))
                            File.Delete(pathInitials);
                    }
                    catch (Exception exc)
                    {
                        log.WarnFormat("[Contacts_ContactInfoChanged] - Impossible to delete Image file - Exception:[{0}]", Util.SerializeException(exc));
                    }

                    // Check if we have a rounded avatar. If not we need to raise ContactAvatarChanged
                    String pathRounded = Path.Combine(folderContactAvatarRounded, contact.Id + ".png");
                    if (!File.Exists(pathRounded))
                        ContactAvatarChanged?.Invoke(this, new IdEventArgs(contact.Id));

                    CheckBubbleAvatarImpactRelatedToContactId(contact.Id);
                }

            }
        }

        private void Contacts_ContactAdded(object sender, JidEventArgs e)
        {
            if (!InitDone())
                return;

            Contact contact = contacts.GetContactFromContactJid(e.Jid);
            if (contact != null)
            {
                log.DebugFormat("[Contacts_ContactAdded] Contact - Id:[{0}] - Jid:[{1}] - DisplayName:[{2}]", contact.Id, contact.Jid_im, Util.GetContactDisplayName(contact, AvatarPool.Instance.GetFirstNameFirst()));

                // Raise event ContactAvatarChanged
                ContactAvatarChanged?.Invoke(this, new IdEventArgs(contact.Id));
            }
        }

        private void Contacts_RosterContactRemoved(object sender, JidEventArgs e)
        {
            if (!InitDone())
                return;

            Contact contact = contacts.GetContactFromContactJid(e.Jid);
            if (contact != null)
                log.DebugFormat("[Contacts_RosterContactRemoved] Contact - Id:[{0}] - Jid:[{1}] - DisplayName:[{2}]", contact.Id, contact.Jid_im, Util.GetContactDisplayName(contact, AvatarPool.Instance.GetFirstNameFirst()));

        }

        private void Contacts_RosterContactAdded(object sender, JidEventArgs e)
        {
            if (!InitDone())
                return;

            Contact contact = contacts.GetContactFromContactJid(e.Jid);
            if (contact != null)
                log.DebugFormat("[Contacts_RosterContactAdded] Contact - Id:[{0}] - Jid:[{1}] - DisplayName:[{2}]", contact.Id, contact.Jid_im, Util.GetContactDisplayName(contact, AvatarPool.Instance.GetFirstNameFirst()));

        }

        private void Contacts_ContactAvatarChanged(object sender, JidEventArgs e)
        {
            if (!InitDone())
                return;

            String contactId = contacts.GetContactIdFromContactJid(e.Jid);
            if (contactId != null)
            {
                if (contactsWithoutAvatar.Contains(contactId))
                    contactsWithoutAvatar.Remove(contactId);

                // Add to download list this avatar
                AddContactAvatarToDownload(contactId);

                // Here don't check bubble avatar impact - we are doing it already in AddContactAvatarToDownload(...)
            }
        }

        private void Contacts_ContactAvatarDeleted(object sender, JidEventArgs e)
        {
            if (!InitDone())
                return;

            // Store info about this contact wihtout avatar
            String contactId = contacts.GetContactIdFromContactJid(e.Jid);
            if (contactId != null)
            {
                if (!contactsWithoutAvatar.Contains(contactId))
                    contactsWithoutAvatar.Add(contactId);

                // Remove from folder the file associated
                try
                {
                    String filePath;

                    filePath = Path.Combine(folderContactAvatarOriginals, contactId + ".png");
                    if (File.Exists(filePath))
                        File.Delete(filePath);

                    filePath = Path.Combine(folderContactAvatarRounded, contactId + ".png");
                    if (File.Exists(filePath))
                        File.Delete(filePath);

                }
                catch (Exception exc)
                {
                    log.WarnFormat("[Contacts_ContactAvatarDeleted] - Impossible to delete Image files - Exception:[{0}]", Util.SerializeException(exc));
                    // Nothing special to handle here
                }

                // Raise event ContactAvatarChanged
                ContactAvatarChanged?.Invoke(this, new IdEventArgs(contactId));

                CheckBubbleAvatarImpactRelatedToContactId(contactId);
            }
        }

        private void Bubbles_BubbleAvatarUpdated(object sender, BubbleAvatarEventArgs e)
        {
            if (!InitDone())
                return;

            if (e.Status.ToLower() == "updated")
            {
                // If the avatar has been updated, we remove the info about avatar not available in this bubble
                if (bubblesWithoutAvatar.Contains(e.BubbleId))
                    bubblesWithoutAvatar.Remove(e.BubbleId);
                AddBubbleAvatarToDownload(e.BubbleId);
            }
            else 
            {
                // The avatar has been deleted
                if (!bubblesWithoutAvatar.Contains(e.BubbleId))
                    bubblesWithoutAvatar.Add(e.BubbleId);

                // Remove from folder the file associated
                try
                {
                    String filePath;

                    filePath = Path.Combine(folderBubbleAvatarOriginals, e.BubbleId + ".png");
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);

                        filePath = Path.Combine(folderBubbleAvatarRounded, e.BubbleId + ".png");
                        if (File.Exists(filePath))
                            File.Delete(filePath);
                    }
                }
                catch (Exception exc)
                {
                    log.WarnFormat("[Bubbles_BubbleAvatarUpdated] - Impossible to delete Image files - Exception:[{0}]", Util.SerializeException(exc));
                    // Nothing special to handle here
                }

                // Raise event BubbleAvatarChanged
                BubbleAvatarChanged?.Invoke(this, new IdEventArgs(e.BubbleId));
            }
        }

        private void Bubbles_BubbleMemberUpdated(object sender, BubbleMemberEventArgs e)
        {
            if (!InitDone())
                return;

            String bubbleId = e.BubbleId;
            Boolean specificAvatar;
            Boolean membersUpdated = false;

            // Check if this bubble has a specific avatar
            String filePath = Path.Combine(folderBubbleAvatarOriginals, e.BubbleId + ".png");
            specificAvatar = File.Exists(filePath);

            // We update bubble's members only if we already manage it
            if (avatarsData.bubblesMember.ContainsKey(bubbleId))
            {
                // Check if the update involves member added/removed/updated
                if (!String.IsNullOrEmpty(e.Status))
                {
                    String contactId = contacts.GetContactIdFromContactJid(e.ContactJid);
                    List<String> members = avatarsData.bubblesMember[bubbleId];

                    if (contactId != null)
                    {
                        if (e.Status == Bubble.MemberStatus.Unsubscribed)
                        {
                            if (members.Contains(contactId))
                            {
                                members.Remove(contactId);
                                membersUpdated = true;
                            }
                        }
                        else if (e.Status == Bubble.MemberStatus.Accepted)
                        {
                            if ((members.Count < 4) && (!members.Contains(contactId)))
                            {
                                members.Add(contactId);
                                membersUpdated = true;
                            }
                        }

                        if (membersUpdated)
                        {
                            // We need to ensure to have a correct member list
                            Bubble bubble = bubbles.GetBubbleByIdFromCache(bubbleId);
                            if ((bubble.Users.Count > members.Count) && (members.Count < 3))
                            {
                                foreach (Bubble.Member member in bubble.Users)
                                {
                                    if (member.Status == Bubble.MemberStatus.Accepted)
                                    {
                                        if (!members.Contains(member.UserId))
                                        {
                                            members.Add(member.UserId);
                                        }

                                        if (members.Count == 4)
                                            break;
                                    }
                                }
                            }

                            // Now update the  store
                            avatarsData.bubblesMember.Remove(bubbleId);
                            avatarsData.bubblesMember.Add(bubbleId, members);
                        }
                    }
                }
            }

            if ((!specificAvatar) && (membersUpdated))
            {
                try
                {
                    // Need to delete rounded avatar to ensure to regenerate the correct one
                    filePath = Path.Combine(folderBubbleAvatarRounded, e.BubbleId + ".png");
                    if (File.Exists(filePath))
                        File.Delete(filePath);

                }
                catch(Exception exc)
                {
                    log.WarnFormat("[Bubbles_BubbleMemberUpdated] - Impossible to delete Image files - Exception:[{0}]", Util.SerializeException(exc));
                }

                // Raise event BubbleAvatarChanged
                BubbleAvatarChanged?.Invoke(this, new IdEventArgs(bubbleId));
            }
        }

        private void Application_ConnectionStateChanged(object sender, ConnectionStateEventArgs e)
        {
            if (!application.IsInitialized())
                initDone = false;
        }

        private void Application_InitialisationPerformed(object sender, EventArgs e)
        {
            if (!application.IsInitialized())
                initDone = false;
            else
            {
                log.DebugFormat("[Application_InitialisationPerformed");
                List<Rainbow.Model.Conversation> list = conversations.GetAllConversationsFromCache();
                foreach (Rainbow.Model.Conversation conversation in list)
                {
                    if (conversation.Type == Rainbow.Model.Conversation.ConversationType.Room)
                    {
                        StoreBubbleMembers(conversation.PeerId);
                    }
                }
            }
        }

#endregion EVENT FIRED BY RAINBOW SDK

#region PUBLIC METHODS    

        public void SetImageManagement(IImageManagement imageManagement)
        {
            this.imageManagement = imageManagement;
        }

        public void AllowAvatarDownload(Boolean value)
        {
            allowAvatarDownload = value;

            if (InitDone() && allowAvatarDownload)
            {
                UseDownloaderPool();
            }
        }

        public void AllowToAskInfoForUnknownContact(Boolean value)
        {
            allowToAskContactInfo = value;

            if (InitDone() && allowAvatarDownload)
            {
                UseUnknowContactsPool();
            }
        }

        public void SetApplication(ref Rainbow.Application application)
        {
            this.application = application;
            this.contacts = application.GetContacts();
            this.bubbles = application.GetBubbles();
            this.conversations = application.GetConversations();

            // Manage necessary events from Application
            application.ConnectionStateChanged += Application_ConnectionStateChanged;
            application.InitializationPerformed += Application_InitialisationPerformed;

            // Manage necessary events from Contacts
            contacts.ContactAdded += Contacts_ContactAdded;
            contacts.ContactInfoChanged += Contacts_ContactInfoChanged;
            contacts.ContactAvatarChanged += Contacts_ContactAvatarChanged;
            contacts.ContactAvatarDeleted += Contacts_ContactAvatarDeleted;

            contacts.RosterContactAdded += Contacts_RosterContactAdded;
            contacts.RosterContactRemoved += Contacts_RosterContactRemoved;

            // Manage necessary events from Bubbles
            bubbles.BubbleAvatarUpdated += Bubbles_BubbleAvatarUpdated;
            bubbles.BubbleMemberUpdated += Bubbles_BubbleMemberUpdated;
        }

        public void SetFolderPath(String path)
        {
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                // ----- DEFINE CONTACTS FOLDER

                String contactsFolder = Path.Combine(path, "Contacts");
                if (!Directory.Exists(contactsFolder))
                    Directory.CreateDirectory(contactsFolder);

                folderContactAvatarOriginals = Path.Combine(contactsFolder, "Originals");
                if (!Directory.Exists(folderContactAvatarOriginals))
                    Directory.CreateDirectory(folderContactAvatarOriginals);

                folderContactAvatarRounded = Path.Combine(contactsFolder, "Rounded");
                if (!Directory.Exists(folderContactAvatarRounded))
                    Directory.CreateDirectory(folderContactAvatarRounded);

                folderContactAvatarInitials = Path.Combine(contactsFolder, "Initials");
                if (!Directory.Exists(folderContactAvatarInitials))
                    Directory.CreateDirectory(folderContactAvatarInitials);

                // ----- DEFINE BUBBLES FOLDER

                String bubblesFolder = Path.Combine(path, "Bubbles");
                if (!Directory.Exists(bubblesFolder))
                    Directory.CreateDirectory(bubblesFolder);

                folderBubbleAvatarOriginals = Path.Combine(bubblesFolder, "Originals");
                if (!Directory.Exists(folderBubbleAvatarOriginals))
                    Directory.CreateDirectory(folderBubbleAvatarOriginals);

                folderBubbleAvatarRounded = Path.Combine(bubblesFolder, "Rounded");
                if (!Directory.Exists(folderBubbleAvatarRounded))
                    Directory.CreateDirectory(folderBubbleAvatarRounded);

                folderBubbleAvatarInitials = Path.Combine(bubblesFolder, "Initials");
                if (!Directory.Exists(folderBubbleAvatarInitials))
                    Directory.CreateDirectory(folderBubbleAvatarInitials);
            }
            catch (Exception exc)
            {
                log.WarnFormat("Impossible to create directory to store Avatar:\r\n{0}", Util.SerializeException(exc));
            }
        }

        public void SetAvatarSize(int value)
        {
            avatarSize = value;
        }

        public void SetFont(String name, int size)
        {
            avatarFontFamilyName = name;
            avatarFontSize = size;
        }

        public void SetFirstNameFirst(bool value)
        {
            firstNameFirst = value;
        }

        public bool GetFirstNameFirst()
        {
            return firstNameFirst;
        }

        public Stream GetContactAvatar(String contactId)
        {
            String path = GetContactAvatarPath(contactId);
            Stream stream = null;
            try
            {
                stream = new MemoryStream(File.ReadAllBytes(path));
            }
            catch { }
            return stream;
        }

        public Stream GetBubbleAvatar(String bubbleId)
        {
            String path = GetBubbleAvatarPath(bubbleId);
            Stream stream = null;
            try
            {
                stream = new MemoryStream(File.ReadAllBytes(path));
            }
            catch { }
            return stream;
        }

        public static String GetColorFromDisplayName(String displayName)
        {
            return colorsList[ColorIndexFromDisplayName(displayName)];
        }

        public static String GetDarkerColorFromDisplayName(String displayName)
        {
            return darkerColorsList[ColorIndexFromDisplayName(displayName)];
        }

        public List<String> GetBubblesListForContactID(String contactId)
        {
            List<String> result = new List<string>();
            if (contactId != null)
            {
                List<String> bubblesId = avatarsData.bubblesMember.Keys.ToList<String>();
                List<String> members;
                foreach (String bubbleId in bubblesId)
                {
                    members = avatarsData.bubblesMember[bubbleId];
                    if (members.Contains(contactId))
                        result.Add(bubbleId);
                }
            }
            return result;
        }

        public String GetUnknownAvatarPath()
        {
            // Get unknown avatar
            String unknownPath = Path.Combine(folderContactAvatarInitials, "unknown.png");
            if (!File.Exists(unknownPath))
            {
                // Create unknown avatar
                Stream stream = GetRoundedAvatarUsingInitials(UNKNOW_CONTACT_NAME, GetColorFromDisplayName(UNKNOW_CONTACT_NAME), "#FFFFFF");
                if ((stream != null) && (stream.Length != 0))
                {
                    using (Stream file = File.Create(unknownPath))
                    {
                        stream.CopyTo(file);
                        // Return to first position
                        stream.Position = 0;
                    }
                }
            }

            return unknownPath;
        }

        public String GetContactAvatarPath(String contactId)
        {
            if (!InitDone())
                return null;

            String pathRounded;
            String pathOriginal;
            String pathInitials;

            // Check rounded avatar
            pathRounded = Path.Combine(folderContactAvatarRounded, contactId + ".png");
            if (File.Exists(pathRounded))
                return pathRounded;

            // Since there is no rounded avatar, we'll dwl the avatar
            AddContactAvatarToDownload(contactId);

            //// Check original avatar
            pathOriginal = Path.Combine(folderContactAvatarOriginals, contactId + ".png");
            //if (File.Exists(pathOriginal))
            //{
            //    avatar = GetRoundedAvatarUsingFile(pathOriginal);
            //    if (avatar != null)
            //    {
            //        avatar.Save(pathRounded);
            //        avatar.Dispose();
            //        return pathRounded;
            //    }
            //}

            // Check avatar with initials
            pathInitials = Path.Combine(folderContactAvatarInitials, contactId + ".png");
            if (File.Exists(pathInitials))
            {
                // We always recreate "Initial Avatar" if we don't know first its initials / displayName
                if (avatarsData.contactsInfo.ContainsKey(contactId))
                    return pathInitials;
                else
                {
                    try
                    {
                        File.Delete(pathInitials);
                    }
                    catch
                    {
                    }
                }
            }

            // We need to create avatar using initials
            Contact contact = contacts.GetContactFromContactId(contactId);
            if (contact != null)
            {
                String displayName = Util.GetContactDisplayName(contact, firstNameFirst);
                String initials = Util.GetContactInitials(contact, firstNameFirst);

                avatarsData.contactsInfo.Remove(contactId);
                avatarsData.contactsInfo.Add(contactId, new Tuple<string, string>(initials, displayName));

                Stream stream = GetAvatarUsingInitials(initials, GetColorFromDisplayName(displayName), "#FFFFFF");
                // Save as "original" image if necessary
                if (!File.Exists(pathOriginal))
                {
                    if ((stream != null) && (stream.Length != 0))
                    {
                        using (Stream file = File.Create(pathOriginal))
                        {
                            stream.CopyTo(file);
                            // Return to first position
                            stream.Position = 0;
                        }
                    }
                }

                // Create "initials" image and save it
                if (!File.Exists(pathInitials))
                {
                    stream = GetRoundedFromSquareImage(stream);
                    if ((stream != null) && (stream.Length != 0))
                    {
                        using (Stream file = File.Create(pathInitials))
                        {
                            stream.CopyTo(file);
                            // Return to first position
                            stream.Position = 0;
                        }
                    }
                }
                return pathInitials;
            }
            else
            {
                // Add this contactId to pool of unknown contact
                AddUnknownContactToPoolById(contactId);

                // Get unknown avatar
                String unknownPath = GetUnknownAvatarPath();

                log.DebugFormat("[GetContactAvatarPath] Get Unknown Avatar for ContactId:[{0}]", contactId);
                return unknownPath;
            }
        }

        public String GetBubbleAvatarPath(String bubbleId)
        {
            if (!InitDone())
                return null;

            List<String> membersId = null;
            // If we don't have any members yet about this bubble, we store them
            if (!avatarsData.bubblesMember.ContainsKey(bubbleId))
                membersId = StoreBubbleMembers(bubbleId);

            // Check rounded avatar
            String path = Path.Combine(folderBubbleAvatarRounded, bubbleId + ".png");
            if (File.Exists(path))
                return path;

            // Since there is no rounded avatar, we'll dwl the avatar
            AddBubbleAvatarToDownload(bubbleId);

            // Store bubble members (iy any)
            if (membersId == null)
                membersId = StoreBubbleMembers(bubbleId);

            // Create bubble avatar using members
            if (membersId != null)
                path = CreateAndGetRoundedBubbleAvatarPathUsingMembers(bubbleId, membersId);

            return path;
        }

        public static AvatarPool Instance
        {
            get
            {
                return instance;
            }
        }

#endregion PUBLIC METHODS    

#region INTERFACE IImageManagement

        public Double GetDensity()
        {
            if (imageManagement != null)
                return imageManagement.GetDensity();
            return 1;
        }

        public Stream DrawImage(Stream msSrc, Stream msDst, float xDest, float yDest)
        {
            if (imageManagement != null)
                return imageManagement.DrawImage(msSrc, msDst, xDest, yDest);
            return null;
        }

        public Stream GetTranslated(Stream ms, float x, float y)
        {
            if (imageManagement != null)
                return imageManagement.GetTranslated(ms, x, y);
            return null;
        }

        public Stream GetArcPartFromSquareImage(Stream ms, int partNumber, int nbParts)
        {
            if (imageManagement != null)
                return imageManagement.GetArcPartFromSquareImage(ms, partNumber, nbParts);
            return null;
        }

        public Stream GetSquareAndScaled(Stream ms, int imgSize)
        {
            if (imageManagement != null)
                return imageManagement.GetSquareAndScaled(ms, imgSize);
            return null;
        }

        public Stream GetFilledCircleWithCenteredText(int imgSize, String rgbCircleColor, String txt, String rgbTextColor, string fontFamilyName, int fontSize)
        {
            if (imageManagement != null)
                return imageManagement.GetFilledCircleWithCenteredText(imgSize, rgbCircleColor, txt, rgbTextColor, fontFamilyName, fontSize);
            return null;
        }

        public Stream GetRoundedFromSquareImage(Stream stream)
        {
            if(imageManagement != null)
                return imageManagement.GetRoundedFromSquareImage(stream);
            return null;
        }

        public Stream GetScaled(Stream stream, int width, int height)
        {
            if (imageManagement != null)
                return imageManagement.GetScaled(stream, width, height);
            return null;
        }

        public System.Drawing.Size GetSize(Stream stream)
        {
            if (imageManagement != null)
                return imageManagement.GetSize(stream);
            return new System.Drawing.Size();
        }
        

#endregion INTERFACE IImageManagement

#region PRIVATE METHODS    

         private static int ColorIndexFromDisplayName(String displayName)
        {
            int result = 0;
            String name;

            if (String.IsNullOrEmpty(displayName))
                name = UNKNOW_CONTACT_NAME; // To use common "unknown" contact color / display
            else
                name = displayName.ToUpper();
            long sum = 0;
            int i, nb;
            if (name != null)
            {
                nb = name.Length;
                for (i = 0; i < nb; i++)
                    sum += name[i];

                result = (int)(sum % colorsList.Count);
            }
            return result;
        }

        private Stream GetContactAvatarPartUsingFile(String filePath, int partNumber, int nbParts)
        {
            Stream contactAvatar = GetContactAvatarUsingFile(filePath);
            return GetArcPartFromSquareImage(contactAvatar, partNumber, nbParts);
        }

        private String CreateAndGetRoundedBubbleAvatarPathUsingMembers(String bubbleId, List<String> members)
        {
            String path;
            Stream imageResult = null;

            int index = 1;
            int nb = members.Count;

            if (nb == 1)
            {
                imageResult = GetContactAvatar(members[0]);
            }
            else
            {
                foreach (String contactId in members)
                {
                    path = Path.Combine(folderContactAvatarOriginals, contactId + ".png");
                    if (!File.Exists(path))
                        path = GetContactAvatarPath(contactId);

                    using(Stream stream = GetContactAvatarPartUsingFile(path, index, nb))
                    {
                        if (index == 1)
                        {
                            imageResult = new MemoryStream();
                            stream.CopyTo(imageResult);
                            imageResult.Position = 0;
                        }
                        else
                        {
                            imageResult = DrawImage(stream, imageResult, 0, 0);
                        }
                    }
                    index++;
                }
            }

            path = Path.Combine(folderBubbleAvatarRounded, bubbleId + ".png");
            try
            {
                // Delete previous file (if nay)
                if (File.Exists(path))
                    File.Delete(path);

                // Save new image
                using (Stream file = File.Create(path))
                    imageResult.CopyTo(file);

                imageResult.Dispose();
            }
            catch (Exception exc)
            {
                log.WarnFormat("[GetRoundedBubbleAvatarPath] - Impossible to delete Image file - Exception:[{0}]", Util.SerializeException(exc));
            }

            return path;
        }

        private Stream GetContactAvatarUsingFile(String filePath)
        {
            Stream result = null;
            if (File.Exists(filePath))
            {
                try
                {
                    using (Stream stream = new MemoryStream(File.ReadAllBytes(filePath)))
                    {
                        result = GetSquareAndScaled(stream, (int)(avatarSize * GetDensity()));
                    }
                }
                catch { }
            }
            return result;
        }

        private Stream GetAvatarUsingInitials(String initials, String rgbBackgroungColor, String rgbTextColor)
        {
            int size = (int) (avatarSize * GetDensity());
            return GetFilledCircleWithCenteredText(size, rgbBackgroungColor, initials, rgbTextColor, avatarFontFamilyName, avatarFontSize);
        }

        private void CheckBubbleAvatarImpactRelatedToContactId(String contactId)
        {
            // Check if some bubbles are involved too
            List<String> bubblesId = GetBubblesListForContactID(contactId);
            if ((bubblesId != null) && (bubblesId.Count > 0))
            {
                String filePath;
                foreach (String bubbleId in bubblesId)
                {
                    // If the bubble has not an avatar we need to update it
                    if (bubblesWithoutAvatar.Contains(bubbleId))
                    {
                        filePath = Path.Combine(folderBubbleAvatarOriginals, bubbleId + ".png");
                        if (!File.Exists(filePath))
                        {
                            try
                            {
                                // We must delete rounded avatar (if any)
                                filePath = Path.Combine(folderBubbleAvatarRounded, bubbleId + ".png");
                                if (File.Exists(filePath))
                                    File.Delete(filePath);

                            }
                            catch (Exception exc)
                            {
                                log.WarnFormat("[CheckBubbleAvatarImpactRelatedToContactId] - Impossible to delete Image files - Exception:[{0}]", Util.SerializeException(exc));
                            }
                            // Raise event BubbleAvatarChanged
                            BubbleAvatarChanged?.Invoke(this, new IdEventArgs(bubbleId));
                        }
                    }
                }
            }
        }

        private List<String> StoreBubbleMembers(String bubbleId)
        {
            List<String> membersId;
            if (avatarsData.bubblesMember.ContainsKey(bubbleId))
            {
                membersId = avatarsData.bubblesMember[bubbleId];
            }
            else
            {
                membersId = new List<String>();
                Bubble bubble = bubbles.GetBubbleByIdFromCache(bubbleId);
                if (bubble != null)
                {
                    List<Bubble.Member> members = bubble.Users;

                    int nb = 1;
                    foreach (Bubble.Member member in members)
                    {
                        if (member.Status == Bubble.MemberStatus.Accepted)
                        {
                            nb++;
                            membersId.Add(member.UserId);

                            // We want only 4 members
                            if (nb > 4)
                                break;
                        }
                    }
                    avatarsData.bubblesMember.Add(bubbleId, membersId);
                }
            }
            return membersId;
        }

        private Stream GetRoundedAvatarUsingFile(String filePath)
        {
            Stream stream = GetRoundedFromSquareImage(GetContactAvatarUsingFile(filePath));
            return stream;
        }

        private Stream GetRoundedAvatarUsingInitials(String initials, String rgbBackgroungColor, String rgbTextColor)
        {
            Stream stream = GetRoundedFromSquareImage(GetAvatarUsingInitials(initials, rgbBackgroungColor, rgbTextColor));
            return stream;
        }

        private Boolean InitDone()
        {
            if (!initDone)
            {
                if (contacts == null)
                    return false;
                if (!application.IsConnected())
                    return false;
                if ((folderBubbleAvatarOriginals == null)
                    || (folderContactAvatarInitials == null)
                    || (folderContactAvatarOriginals == null)
                    || (folderContactAvatarRounded == null))
                    return false;
            }
            initDone = true;
            return initDone;
        }

        public void AddUnknownContactToPoolById(String contactId)
        {
            if (!contactsUnknownById.Contains(contactId))
            {
                log.DebugFormat("[AddUnknownContactToPoolById] contactId:[{0}]", contactId);
                contactsUnknownById.Add(contactId);
                UseUnknowContactsPool();
            }
        }

        public void AddUnknownContactToPoolByJid(String contactJid)
        {
            if (!contactsUnknownByJid.Contains(contactJid))
            {
                log.DebugFormat("[AddUnknownContactToPoolByJid] contactJid:[{0}]", contactJid);
                contactsUnknownByJid.Add(contactJid);
                UseUnknowContactsPool();
            }
        }

        private void UseUnknowContactsPool()
        {
            if (!allowToAskContactInfo)
                return;

            if (backgroundWorkerUnkownContact == null)
            {
                backgroundWorkerUnkownContact = new BackgroundWorker();
                backgroundWorkerUnkownContact.DoWork += BackgroundWorkerUnkownContact_DoWork;
                backgroundWorkerUnkownContact.WorkerSupportsCancellation = true;
                backgroundWorkerUnkownContact.RunWorkerCompleted += BackgroundWorkerUnkownContact_RunWorkerCompleted;
            }

            if (!backgroundWorkerUnkownContact.IsBusy)
                backgroundWorkerUnkownContact.RunWorkerAsync();
        }

        private void BackgroundWorkerUnkownContact_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if( (contactsUnknownById.Count > 0) ||(contactsUnknownByJid.Count > 0) )
                UseUnknowContactsPool();
        }

        private void BackgroundWorkerUnkownContact_DoWork(object sender, DoWorkEventArgs e)
        {
            Boolean continueWork;
            int index = 0;
            String contactId;
            String contactJid;

            log.DebugFormat("[BackgroundWorkerUnkownContact_DoWork] - IN - nbUnknownById:[{0}] - nbUnknownByJid:[{1}]", contactsUnknownById.Count, contactsUnknownByJid.Count);
            do
            {
                if (contactsUnknownById.Count > 0)
                {
                    // Check index
                    if (index >= contactsUnknownById.Count)
                        index = 0;

                    contactId = contactsUnknownById[index];
                    log.DebugFormat("[BackgroundWorkerUnkownContact_DoWork] Ask contact info - START - ContactId:[{0}]", contactId);
                    if (AskContactInfoById(contactId))
                    {
                        contactsUnknownById.Remove(contactId);
                        log.DebugFormat("[BackgroundWorkerUnkownContact_DoWork] Ask contact info - END - SUCCESS - ContactId:[{0}]", contactId);
                    }
                    else
                    {
                        // Download failed - we try for another contact
                        index++;
                        log.DebugFormat("[BackgroundWorkerDonwload_DoWork] Ask contact info- END - FAILED - ContactId:[{0}]", contactId);
                    }
                }
                else if (contactsUnknownByJid.Count > 0)
                {
                    // Check index
                    if (index >= contactsUnknownByJid.Count)
                        index = 0;

                    contactJid = contactsUnknownByJid[index];
                    log.DebugFormat("[BackgroundWorkerUnkownContact_DoWork] Ask contact info - START - ContactJid:[{0}]", contactJid);
                    if (AskContactInfoByJid(contactJid))
                    {
                        contactsUnknownByJid.Remove(contactJid);
                        log.DebugFormat("[BackgroundWorkerUnkownContact_DoWork] Ask contact info - END - SUCCESS - ContactJid:[{0}]", contactJid);
                    }
                    else
                    {
                        // Download failed - we try for another contact
                        index++;
                        log.DebugFormat("[BackgroundWorkerDonwload_DoWork] Ask contact info- END - FAILED - ContactJid:[{0}]", contactJid);
                    }
                }

                continueWork = allowToAskContactInfo 
                    && ( (contactsUnknownById.Count > 0) || (contactsUnknownByJid.Count > 0) );

            }
            while (continueWork);

            log.Debug("[BackgroundWorkerUnkownContact_DoWork] - OUT");
        }

        private Boolean AskContactInfoById(String contactId)
        {
            Boolean result = false;
            ManualResetEvent manualEvent = new ManualResetEvent(false);

            contacts.GetContactFromContactIdFromServer(contactId, callback =>
            {
                result = callback.Result.Success;
                manualEvent.Set();
            });

            manualEvent.WaitOne();
            manualEvent.Dispose();

            return result;
        }

        private Boolean AskContactInfoByJid(String contactJid)
        {
            Boolean result = false;
            ManualResetEvent manualEvent = new ManualResetEvent(false);

            contacts.GetContactFromContactJidFromServer(contactJid, callback =>
            {
                result = callback.Result.Success;
                manualEvent.Set();
            });

            manualEvent.WaitOne();
            manualEvent.Dispose();

            return result;
        }

        private void AddContactAvatarToDownload(String contactId)
        {
            // This contact has no avatar - we do nothing more
            if (contactsWithoutAvatar.Contains(contactId))
                return;

            if (!contactsWithAvatarToDwl.Contains(contactId))
            {
                contactsWithAvatarToDwl.Add(contactId);

                UseDownloaderPool();
            }
        }

        private void AddBubbleAvatarToDownload(String bubbleId)
        {
            // This bubble has no avatar - we do nothing more
            if (bubblesWithoutAvatar.Contains(bubbleId))
                return;

            if (!bubblesWithAvatarToDwl.Contains(bubbleId))
            {
                bubblesWithAvatarToDwl.Add(bubbleId);
                UseDownloaderPool();
            }
        }

        private void UseDownloaderPool()
        {
            if (!allowAvatarDownload)
                return;

            if(backgroundWorkerDownload == null)
            {
                backgroundWorkerDownload = new BackgroundWorker();
                backgroundWorkerDownload.DoWork += BackgroundWorkerDonwload_DoWork;
                backgroundWorkerDownload.RunWorkerCompleted += BackgroundWorkerDownload_RunWorkerCompleted;
                backgroundWorkerDownload.WorkerSupportsCancellation = true;
            }

            if(!backgroundWorkerDownload.IsBusy)
                backgroundWorkerDownload.RunWorkerAsync();
        }

        private void BackgroundWorkerDownload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((bubblesWithAvatarToDwl.Count > 0) || (contactsWithAvatarToDwl.Count > 0))
                UseDownloaderPool();
        }

        private void BackgroundWorkerDonwload_DoWork(object sender, DoWorkEventArgs e)
        {
            Boolean continueDownload;
            int indexBubble = 0;
            int indexContact = 0;

            String bubbleId;
            String contactId;

            do
            {
                // WE DOWNLOAD FIRST AVATAR FOR BUBBLES
                if (bubblesWithAvatarToDwl.Count > 0)
                {
                    // Check index
                    if (indexBubble >= bubblesWithAvatarToDwl.Count)
                        indexBubble = 0;

                    bubbleId = bubblesWithAvatarToDwl[indexBubble];
                    log.DebugFormat("[BackgroundWorkerDonwload_DoWork] Download Bubble Avatar - START - Bubble:[{0}]", bubbleId);
                    if (DownloadBubbleAvatar(bubbleId))
                    {
                        bubblesWithAvatarToDwl.Remove(bubbleId);
                        log.DebugFormat("[BackgroundWorkerDonwload_DoWork] Download Bubble Avatar - END - SUCCESS - Bubble:[{0}]", bubbleId);
                    }
                    else
                    {
                        // Download failed - we try for another contact
                        indexBubble++;
                        log.DebugFormat("[BackgroundWorkerDonwload_DoWork] Download Bubble Avatar - END - FAILED - Bubble:[{0}]", bubbleId);
                    }
                }
                else
                {
                    log.InfoFormat("[BackgroundWorkerDonwload_DoWork] NO MORE BUBBLE AVATAR TO DWL");

                    if (contactsWithAvatarToDwl.Count > 0)
                    {
                        // Check index
                        if (indexContact >= contactsWithAvatarToDwl.Count)
                            indexContact = 0;

                        contactId = contactsWithAvatarToDwl[indexContact];
                        log.DebugFormat("[BackgroundWorkerDonwload_DoWork] Download Contact Avatar - START - Contact:[{0}]", contactId);
                        if (DownloadContactAvatar(contactId))
                        {
                            contactsWithAvatarToDwl.Remove(contactId);
                            log.DebugFormat("[BackgroundWorkerDonwload_DoWork] Download Contact Avatar - END - SUCCESS - Contact:[{0}]", contactId);
                        }
                        else
                        {
                            // Download failed - we try for another contact
                            indexContact++;
                            log.DebugFormat("[BackgroundWorkerDonwload_DoWork] Download Contact Avatar - END - FAILED - Contact:[{0}]", contactId);
                        }
                    }
                    else
                    {
                        log.InfoFormat("[BackgroundWorkerDonwload_DoWork] NO MORE CONTACT AVATAR TO DWL");
                    }
                }

                continueDownload = allowAvatarDownload && ((bubblesWithAvatarToDwl.Count > 0) || (contactsWithAvatarToDwl.Count > 0));

            }
            while (continueDownload);
        }

        private Boolean DownloadContactAvatar(String contactId)
        {
            Boolean downloadResult = false;
            ManualResetEvent manualEvent = new ManualResetEvent(false);

            contacts.GetAvatarFromContactId(contactId, avatarSize, callback =>
            {
                //Do we have a correct answer
                if (callback.Result.Success)
                {
                    String pathOriginal = Path.Combine(folderContactAvatarOriginals, contactId + ".png");
                    String pathRounded = Path.Combine(folderContactAvatarRounded, contactId + ".png");

                    byte[] data = callback.Data;

                    downloadResult = true;

                    if (data == null)
                    {
                        if (!contactsWithoutAvatar.Contains(contactId))
                            contactsWithoutAvatar.Add(contactId);
                    }
                    else
                    {
                        try
                        {
                            // Delete previous files
                            if (File.Exists(pathOriginal))
                                File.Delete(pathOriginal);

                            if (File.Exists(pathRounded))
                                File.Delete(pathRounded);

                            // Save original avatar
                            File.WriteAllBytes(pathOriginal, data);

                            // Create rounded avatar and save it to file
                            Stream stream = GetRoundedAvatarUsingFile(pathOriginal);
                            using (Stream file = File.Create(pathRounded))
                            {
                                stream.CopyTo(file);
                                // Return to first position
                                stream.Position = 0;
                            }

                            Task task = new Task(() =>
                            {
                                // Raise event ContactAvatarChanged
                                ContactAvatarChanged?.Invoke(this, new IdEventArgs(contactId));
                            });
                            task.Start();

                            // Now check any impact on others bubblre
                            CheckBubbleAvatarImpactRelatedToContactId(contactId);
                        }
                        catch (Exception exc)
                        {
                            log.WarnFormat("[DownloadContactAvatar] Impossible to delete Images files - exception:[{0}]", Util.SerializeException(exc));
                        }
                    }
                }
                else
                {
                    log.WarnFormat("[DownloadContactAvatar]Not possible to dwl avatar:[{0}]", Util.SerialiseSdkError(callback.Result));
                }

                manualEvent.Set();
            });
            
            manualEvent.WaitOne();
            manualEvent.Dispose();

            return downloadResult;
        }

        private Boolean DownloadBubbleAvatar(String bubbleId)
        {
            Boolean downloadResult = false;
            ManualResetEvent manualEvent = new ManualResetEvent(false);

            bubbles.GetAvatarFromBubbleId(bubbleId, avatarSize, callback =>
            {
                //Do we have a correct answer
                if (callback.Result.Success)
                {
                    String pathOriginal = Path.Combine(folderBubbleAvatarOriginals, bubbleId + ".png");
                    String pathRounded = Path.Combine(folderBubbleAvatarRounded, bubbleId + ".png");

                    byte[] data = callback.Data;

                    downloadResult = true;

                    if (data == null)
                    {
                        if (!bubblesWithoutAvatar.Contains(bubbleId))
                            bubblesWithoutAvatar.Add(bubbleId);
                    }
                    else
                    {
                            
                        try
                        {
                            // Delete previous files
                            if (File.Exists(pathOriginal))
                                File.Delete(pathOriginal);

                            if (File.Exists(pathRounded))
                                File.Delete(pathRounded);


                            // Save original avatar
                            File.WriteAllBytes(pathOriginal, data);

                            // Create rounded avatar and save it to file
                            Stream stream = GetRoundedAvatarUsingFile(pathOriginal);
                            using (Stream file = File.Create(pathRounded))
                            {
                                stream.CopyTo(file);
                                // Return to first position
                                stream.Position = 0;
                            }
                            Task task = new Task(() =>
                            {
                                // Raise event ContactAvatarChanged
                                BubbleAvatarChanged?.Invoke(this, new IdEventArgs(bubbleId));
                            });
                            task.Start();
                        }
                        catch (Exception exc)
                        {
                            log.WarnFormat("[DownloadBubbleAvatar] Impossible to delete Images files - exception:[{0}]", Util.SerializeException(exc));
                        }
                    }
                }
                else
                {
                    log.WarnFormat("[DownloadBubbleAvatar] Not possible to dwl avatar:[{0}]", Util.SerialiseSdkError(callback.Result));
                }
                manualEvent.Set();
            });
            
            manualEvent.WaitOne();
            manualEvent.Dispose();

            return downloadResult;
        }

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static AvatarPool()
        {
        }

        private AvatarPool()
        {

        }

#endregion PRIVATE METHODS    
    }
}
