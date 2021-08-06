using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;

using Rainbow;
using Rainbow.Common;
using Rainbow.Model;

using MultiPlatformApplication.Models;

using NLog;


namespace MultiPlatformApplication.Helpers
{
    public static class Helper
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(Helper));

        private static double density = 0;

        private static readonly App XamarinApplication;

        private static Languages Languages = null;

        private static String AppFolderName = "Rainbow.CSharp.SDK.MultiOS";
        private static String AvatarsFolderName = "Avatars";
        private static String FileStorageFolderName = "FileStorage";
        private static String DataStorageFolderName = "DataStorage";
        private static String ImageStorageFolderName = "ImageStorage";

        public static SdkWrapper SdkWrapper { get; set; }

        static Helper()
        {
            // Get Xamarin Application
            XamarinApplication = (App)Xamarin.Forms.Application.Current;
        }


#region FOLDERS PATH
        
        public static String GetTempFolder()
        {
            return System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
        }

        public static String GetAppFolderPath()
        {
            return Path.Combine(Helper.GetTempFolder(), AppFolderName);
        }

        public static String GetDataStorageFolderPath()
        {
            return Path.Combine(Helper.GetAppFolderPath(), DataStorageFolderName);
        }

        public static String GetAvatarsFolderPath()
        {
            return Path.Combine(Helper.GetAppFolderPath(), AvatarsFolderName);
        }

        public static String GetFileStorageFolderPath()
        {
            return Path.Combine(Helper.GetAppFolderPath(), FileStorageFolderName);
        }

        public static String GetImagesStorageFolderPath()
        {
            return Path.Combine(Helper.GetAppFolderPath(), ImageStorageFolderName);
        }

#endregion FOLDERS PATH

#region EFFECT - ADD / REMOVE
        public static void AddEffect(View view, Effect effect)
        {
            if (view == null)
                return;

            RemoveEffect(view, effect.GetType());
            view.Effects.Add(effect);
        }

        public static void RemoveEffect(View view, Type effectType)
        {
            if (view == null)
                return;
            
            var toRemove = view.Effects.FirstOrDefault(e => e.GetType().Equals(effectType));
            if (toRemove != null)
                view.Effects.Remove(toRemove);
        }

#endregion EFFECT - ADD / REMOVE

        /// <summary>
        /// Get Humanized string of the specifed DateTime. The DateTime must not be gerated than UtcNow ...
        /// 
        /// This method is linked to GetRefreshDelay(DateTime)
        /// </summary>
        /// <param name="dt"><see cref="DateTime"/>DateTime object to humanize</param>
        /// <returns><see cref="String"/>Humanized String</returns>
        public static String HumanizeDateTime(DateTime dt)
        {
            String result;

            DateTime nowInUTC = DateTime.UtcNow;
            DateTime dtInUTC = dt.ToUniversalTime();

            DateTime dtLocalTime = dt.ToLocalTime();

            if (nowInUTC.Day == dtInUTC.Day)
                result = dtLocalTime.ToString("H:mm");
            else if (nowInUTC.Year == dtInUTC.Year)
                result = dtLocalTime.ToString("dd MMM H:mm");
            else
                result = dtLocalTime.ToString("dd MMM yy H:mm");

            return result;
        }

        /// <summary>
        /// Return the value formatted to include at most three
        /// non-zero digits and at most two digits after the
        /// decimal point. Examples:
        ///         1
        ///       123
        ///        12.3
        ///         1.23
        ///         0.12
        /// </summary>
        /// <param name="value"><see cref="double"/>Double value to format</param>
        /// <returns></returns>
        private static string ThreeNonZeroDigits(double value)
        {
            if (value >= 100)
            {
                // No digits after the decimal.
                return value.ToString("0,0");
            }
            else if (value >= 10)
            {
                // One digit after the decimal.
                return value.ToString("0.0");
            }
            else
            {
                // Two digits after the decimal.
                return value.ToString("0.00");
            }
        }

        /// <summary>
        /// Return a string describing the value as a file size. For example, 1.23 MB. 
        /// </summary>
        /// <param name="fileSizeInOctet"><see cref="double"/>Double value to format</param>
        /// <returns></returns>
        public static string HumanizeFileSize(double fileSizeInOctet)
        {
            string[] suffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};
            for (int i = 0; i < suffixes.Length; i++)
            {
                if (fileSizeInOctet <= (Math.Pow(1024, i + 1)))
                {
                    return ThreeNonZeroDigits(fileSizeInOctet /
                        Math.Pow(1024, i)) +
                        " " + suffixes[i];
                }
            }

            return ThreeNonZeroDigits(fileSizeInOctet /
                Math.Pow(1024, suffixes.Length - 1)) +
                " " + suffixes[suffixes.Length - 1];
        }

        /// <summary>
        /// Get the delay (in seconds) to use in order to refresh delay according the specified DateTime
        /// 
        /// This method is linked to Humanize(DateTime)
        /// </summary>
        /// <param name="dt"><see cref="DateTime"/>DateTime object to humanize</param>
        /// <returns><see cref="int"/>Delay in seconds</returns>
        public static int GetRefreshDelay(DateTime dt)
        {
            int result = 120; // Update every 2 mns (2 * 60)
            return result;
        }

        public static String RemoveCRLFFromString(String str)
        {
            if (str == null)
                return "";

            //Remove carriage returns
            return str.Replace("\n", "").Replace("\r", "");
        }

        public static String ReplaceCRLFFromString(String str, string replace)
        {
            if (str == null)
                return "";

            //Replace carriage returns
            String result = str.Replace("\r\n", replace).Replace("\n", replace).Replace("\r", replace);
            return result;
        }

        public static ConversationModel GetConversationFromRBConversation(Rainbow.Model.Conversation rbConversation)
        {
            ConversationModel conversation = null;

            if (rbConversation != null)
            {
                conversation = new ConversationModel();
                conversation.Id = rbConversation.Id;

                conversation.Peer.Id = rbConversation.PeerId;
                conversation.Peer.Type = rbConversation.Type;

                if (rbConversation.Type == Rainbow.Model.Conversation.ConversationType.Room)
                {
                    conversation.Peer.Jid = rbConversation.Jid_im;
                    conversation.Peer.DisplayName= rbConversation.Name;
                    conversation.Topic = rbConversation.Topic;

                }
                else if (rbConversation.Type == Rainbow.Model.Conversation.ConversationType.User)
                {
                    // Get Display name of this user
                    Rainbow.Model.Contact contact = SdkWrapper.GetContactFromContactId(rbConversation.PeerId);
                    if (contact != null)
                    {
                        conversation.Peer.Jid = contact.Jid_im;
                        conversation.Peer.DisplayName= Rainbow.Util.GetContactDisplayName(contact);
                        conversation.Peer.DisplayPresence = true;
                        conversation.Topic = "";
                    }
                    else
                    {
                        //log.Debug("[GetConversationFromRBConversation] - unknown contact - contactId:[{0}]", rbConversation.PeerId);
                        SdkWrapper.GetContactFromContactIdFromServer(rbConversation.PeerId, null);
                    }
                }
                else
                {
                    //TODO ( bot case)
                    //log.Debug("[GetConversationFromRBConversation] Conversation from model not created - Id:[{0}]", rbConversation.Id);
                    return null;
                }

                conversation.NbMsgUnread = rbConversation.UnreadMessageNumber;

                conversation.LastMessage = rbConversation.LastMessageText;
                conversation.LastMessageDateTime = rbConversation.LastMessageDate;
            }
            return conversation;
        }

        public static String GetBubbleEventMessageBody(Rainbow.Model.Contact contact, String bubbleEvent)
        {
            String result = null;

            if(contact != null)
            {
                String displayName = Rainbow.Util.GetContactDisplayName(contact);
                switch (bubbleEvent)
                {
                    case "conferenceEnded":
                        result = GetLabel("conferenceRemoveMsgRoom", "userDisplayName", displayName);
                        break;
                    case "conferenceStarted":
                        result = GetLabel("conferenceAddMsgRoom", "userDisplayName", displayName);
                        break;
                    case "userJoin":
                        result = GetLabel("joinMsgRoom", "userDisplayName", displayName);
                        break;
                    case "userLeave":
                        result = GetLabel("leaveMsgRoom", "userDisplayName", displayName);
                        break;
                    default:
                        result = GetLabel(bubbleEvent, "userDisplayName", displayName);
                        break;
                }
            }

            return result;
        }

        public static String GetCallLogEventMessageBody(CallLogAttachment callLogAttachment)
        {
            String body = "";
            String displayName = "?";

            String currentContactJid = Helper.SdkWrapper.GetCurrentContactJid();

            Rainbow.Model.Contact contact;
            if (callLogAttachment.Caller == currentContactJid)
                contact = Helper.SdkWrapper.GetContactFromContactJid(callLogAttachment.Callee);
            else
                contact = Helper.SdkWrapper.GetContactFromContactJid(callLogAttachment.Caller);

            if (contact != null)
                displayName = Rainbow.Util.GetContactDisplayName(contact);

            if (String.IsNullOrEmpty(displayName))
                displayName = "?";

            switch (callLogAttachment.State)
            {
                case CallLog.LogState.ANSWERED:

                    if (callLogAttachment.Caller == currentContactJid)
                        body = Helper.GetLabel("activeCallRecvMsg", "userDisplayName", displayName);
                    else
                        body = Helper.GetLabel("activeCallMsg", "userDisplayName", displayName);

                    double nbSecs = Math.Round((double)callLogAttachment.Duration / 1000);
                    int mns = (int)(nbSecs / 60);
                    int sec = (int)Math.Round(nbSecs - (mns * 60));
                    body = body + " (" + ((mns > 0) ? mns + ((mns > 1) ? "mns " : "mn ") : "") + ((sec > 0) ? sec + "s" : "") + ")";
                    break;

                case CallLog.LogState.MISSED:
                    body = Helper.GetLabel("missedCall");
                    break;

                case CallLog.LogState.FAILED:
                    body = Helper.GetLabel("noAnswer");
                    break;
            }

            return body;
        }

        public static String GetLabel(String key)
        {
            if (Languages == null)
            {
                // Get Rainbow Languages service
                Languages = Rainbow.Common.Languages.Instance;
            }
            String label = Languages.GetLabel(key);
            if(String.IsNullOrEmpty(label))
                label = "[! " + key + " !]";
            return label;
        }

        public static String GetLabel(String key, String subtituteKey, String subtituteValue)
        {
            if (Languages == null)
            {
                // Get Rainbow Languages service
                Languages = Rainbow.Common.Languages.Instance;
            }
            if(String.IsNullOrEmpty(key))
            {
                return "[! label key is NULL !]";
            }
            
            String label = Languages.GetLabel(key, subtituteKey, subtituteValue);
            if (String.IsNullOrEmpty(label))
                label = "[! " + key + " !]";
            return label;
        }

        public static String GetContactDisplayName(String jid)
        {
            if (String.IsNullOrEmpty(jid))
                return null;

            Rainbow.Model.Contact contact = SdkWrapper.GetContactFromContactJid(jid);

            if (contact == null)
                return null;

            return Rainbow.Util.GetContactDisplayName(contact);
        }

#region PICK FILE(S)

        public static PickOptions GetDefaultPickOptions()
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.UWP, null },       // => OK (can select any files)
                { DevicePlatform.Android, null },   // => OK (can select any files)

                { DevicePlatform.iOS, null },       // TODO - it's working ? (can specify any files ?)
                { DevicePlatform.macOS, null },     // TODO - it's working ? (can specify any files ?)

                { DevicePlatform.tvOS, null },      // it's working ? (can specify any files ?)
                { DevicePlatform.Tizen, null },     // it's working ? (can specify any files ?)
                { DevicePlatform.Unknown, null },   // it's working ? (can specify any files ?)
                { DevicePlatform.watchOS, null },   // it's working ? (can specify any files ?)
            });

            var options = new PickOptions
            {
                FileTypes = customFileType
            };

            return options;
        }

        // To let enduser to pick severalfile using specified options
        public static async Task<IEnumerable<FileResult>> PickFilesAndShow(PickOptions options = null)
        {
            if(options == null)
                options = GetDefaultPickOptions();

            try
            {
                var result = await FilePicker.PickMultipleAsync(options);
                return result;
            }
            catch
            {
                // The user canceled or something went wrong
            }

            return null;
        }

        // To let enduser to pick a file using specified options
        public static async Task<FileResult> PickFileAndShow(PickOptions options = null)
        {
            if (options == null)
                options = GetDefaultPickOptions();

            try
            {
                var result = await FilePicker.PickAsync(options);
                return result;
            }
            catch
            {
                // The user canceled or something went wrong
            }

            return null;
        }

#endregion PICK FILE(S)

#region AVATAR - IMAGE SOURCE

        public static Double GetDensity()
        {
            //  /!\ ON FIRST CALL: Must be called on MAIN thread and as soon as possible !
            if (density == 0)
            {
                try
                {
                    density = DeviceDisplay.MainDisplayInfo.Density;
                }
                catch { }
            }

            if (density == 0)
                density = 1;

            return density;
        }

        public static String GetBubbleAvatarFilePath(String bubbleId)
        {
            String filePath = null;
            if (!String.IsNullOrEmpty(bubbleId))
            {
                try
                {
                    filePath = SdkWrapper.GetBubbleAvatarPath(bubbleId);
                    log.Debug("[GetBubbleAvatarFilePath] Bubble Avatar - Id:[{0}] - FilePath:[{1}]", bubbleId, filePath);
                }
                catch (Exception exc)
                {
                    log.Warn("[GetBubbleAvatarImageSource] bubbleId:[{0}] - exception occurs to create avatar:[{1}]", bubbleId, Rainbow.Util.SerializeException(exc));
                    filePath = SdkWrapper.GetUnknwonBubbleAvatarFilePath();
                }

            }
            return filePath;
        }

        public static String GetContactAvatarFilePath(String contactId)
        {
            String filePath = null;
            if (!String.IsNullOrEmpty(contactId))
            {
                try
                {
                    filePath = SdkWrapper.GetContactAvatarPath(contactId);
                    log.Debug("[GetContactAvatarImageSource] contactId:[{0}] - filePath:[{1}]", contactId, filePath);
                }
                catch (Exception exc)
                {
                    log.Warn("[GetContactAvatarImageSource] contactId:[{0}] - exception occurs to create avatar:[{1}]", contactId, Rainbow.Util.SerializeException(exc));
                    filePath = SdkWrapper.GetUnknwonContactAvatarFilePath();
                }
            }
            return filePath;
        }

        public static String GetConversationAvatarFilePath(ConversationModel conversation)
        {
            String filePath = null;
            if (conversation != null)
            {
                if (conversation.Peer.Type == Rainbow.Model.Conversation.ConversationType.User)
                    filePath = GetContactAvatarFilePath(conversation.Peer.Id);
                else if (conversation.Peer.Type == Rainbow.Model.Conversation.ConversationType.Room)
                    filePath = GetBubbleAvatarFilePath(conversation.Peer.Id);
            }
            return filePath;
        }

        static internal ImageSource GetImageSourceFromFont(String id)
        {

            String name;
            Color color = Color.White;
            String colorStr = "#FFFFFF";

            if (id.StartsWith("font_", StringComparison.InvariantCultureIgnoreCase))
                name = id.Substring(5);
            else
                name = id;

            // Do we have a color specified ?
            int index = name.IndexOf("|");
            if (index > 0)
            {
                colorStr = name.Substring(index+1);
                name = name.Substring(0, index);
            }

            // Is-it a color define in hexa ?
            if(colorStr.StartsWith("#"))
            {
                color = Color.FromHex(colorStr);
            }
            else
            {
                color = GetResourceDictionaryById<Color>(colorStr);
            }

            FontImageSource result = new FontImageSource();
            result.FontFamily = "FontAwesomeSolid5";
            result.Color = color;

            if (iconsFont.ContainsKey(name))
            {
                result.Glyph = iconsFont[name];
            }
            else
            {
                try
                {
                    //Need to transform "f060" to "\uf060" ...
                    result.Glyph = System.Text.RegularExpressions.Regex.Unescape("\\u" + name);
                }
                catch
                {
                    result.Glyph = "\uf128"; // => Question glyph
                }
            }
            return result;
        }

        // Get image source using Id specified - Using ID provided:
        //  - Check first in embedded resource
        //  - Then check from file
        //  - Then check from ResourceDictionary
        static internal ImageSource GetImageSourceFromIdOrFilePath(String id)
        {
            ImageSource result = null;

            if (!String.IsNullOrEmpty(id))
            {
                if (id.StartsWith("font_", StringComparison.InvariantCultureIgnoreCase))
                    return GetImageSourceFromFont(id);

                String filePath = Helper.GetEmbededResourceFullPath(id);
                if (!String.IsNullOrEmpty(filePath))
                    result = ImageSource.FromResource(filePath, typeof(Helper).Assembly);
                else
                {
                    // Check if it's a valid path
                    if (File.Exists(id))
                        result = ImageSource.FromFile(id);
                    else
                        result = GetResourceDictionaryById<ImageSource>(id);
                }
            }

            return result;
        }

        // Get image source using Id from merged resource dictionaries
        private static T GetResourceDictionaryById<T>(string dictionaryId, ref Boolean found, ResourceDictionary dictionary = null) //where T : class
        {
            T result = default(T);

            if (dictionary == null)
                dictionary = App.Current.Resources;


            if (dictionary?.ContainsKey(dictionaryId) == true)
            {
                var obj = dictionary[dictionaryId];
                if (obj is T)
                {
                    found = true;
                    result = (T)obj;
                }
            }

            if (!found)
            {
                // Check on child merged dictionaries
                if (dictionary.MergedDictionaries?.Count > 0)
                {
                    List<ResourceDictionary> list = dictionary.MergedDictionaries.ToList();
                    for (int index = 0; index < dictionary.MergedDictionaries.Count; index++)
                    {
                        result = GetResourceDictionaryById<T>(dictionaryId, ref found, list[index]);
                        if (found)
                            break;
                    }
                }
            }
            return result;
        }

        public static T GetResourceDictionaryById<T>(string dictionaryId, ResourceDictionary dictionary = null) //where T : class
        {
            if (dictionary == null)
                dictionary = App.Current.Resources;

            Boolean found = false;
            return GetResourceDictionaryById<T>(dictionaryId, ref found, dictionary);
        }

#endregion AVATAR - IMAGE SOURCE    


#region RESOURCES - Get access to resources of this assembly
        
        internal static readonly String NamespaceResources = "MultiPlatformApplication.Resources";

        private static Boolean resourcesInit = false;
        private static List<String> resources = null;

        // Get the list of all resources embedded in this package based on the folder path provided (if any)
        static internal List<String> GetEmbeddedResourcesList(String folderPath = "", String extension = null)
        {
            InitResourcesList();

            if (String.IsNullOrEmpty(folderPath))
                return resources;
            else
            {
                List<String> result = new List<string>();

                folderPath = folderPath.Replace("/", ".");

                if (folderPath.StartsWith("."))
                    folderPath = folderPath.Substring(1);

                if (folderPath.EndsWith("."))
                    folderPath = folderPath.Substring(0, folderPath.Length - 1);

                String path = NamespaceResources + "." + folderPath + ".";

                // Loop to find files stored in this path
                foreach (String resource in resources)
                {
                    if (resource.StartsWith(path))
                    {
                        if (String.IsNullOrEmpty(extension))
                            result.Add(resource.Replace(path, ""));
                        else
                        {
                            if (resource.EndsWith("." + extension))
                                result.Add(resource.Replace(path, ""));
                        }
                    }
                }

                return result;
            }
        }

        // Get content as string of the specifiec embedded resource using the specified encoding
        static internal String GetContentOfEmbeddedResource(String resourceName, Encoding encoding)
        {
            String result = null;
            using (Stream stream = GetStreamFromEmbeddedResource(resourceName))
            {
                if (stream != null)
                {
                    stream.Position = 0;
                    using (StreamReader reader = new StreamReader(stream, encoding))
                        return reader.ReadToEnd();
                }

            }
            return result;
        }

        // Get stream of the specified resourceName using "GetResourceFullPath"
        static internal Stream GetStreamFromEmbeddedResource(String resourceName)
        {
            Stream ms = null;
            String resourcePath = GetEmbededResourceFullPath(resourceName);

            if (resourcePath == null)
                return null;

            Stream streamResourceInfo = typeof(Helper).Assembly.GetManifestResourceStream(resourcePath); // NEED TO SPECIFY A CLASS (here 'Helper') INCLUDED IN THE ASSEMBLY WHERE WE WANT TO GET EMBEDDED RESSOURCES 

            if (streamResourceInfo != null)
            {
                // Go to the beginning of the stream
                streamResourceInfo.Position = 0;

                // Create Memory Stream
                ms = new MemoryStream();

                // Copy stream
                streamResourceInfo.CopyTo(ms);

                // Dispose older Stream
                streamResourceInfo.Dispose();

                // Go to the beginning of the new Memory Stream
                ms.Position = 0;

                log.Debug("[GetStreamFromEmbeddedResource] Stream obtainer:[{0}]", resourceName);
            }
            streamResourceInfo.Dispose();

            return ms;
        }

        // Look from all resources to find one endings with "resourceName" specified in parameter
        static internal String GetEmbededResourceFullPath(String resourceName)
        {
            InitResourcesList();

            if (resources == null)
                return null;

            String result = null;

            if (resourceName.StartsWith(NamespaceResources))
            {
                if(resources.Contains(resourceName))
                    result = resourceName;
            }
            else
            {
                foreach (String resource in resources)
                {
                    if (resource.EndsWith("." + resourceName))
                        result = resource;
                }
            }

            log.Debug("[GetEmbededResourceFullPath] resourceName:[{0}] - result:[{1}]", resourceName, result);

            return result;
        }

        // Get resources from assembly
        static internal void InitResourcesList()
        {
            if (resources == null && !resourcesInit)
            {
                // Avoid to do it more than once
                resourcesInit = true;

                String[] rsc = typeof(Helper).Assembly.GetManifestResourceNames(); // NEED TO SPECIFY A CLASS (here 'Helper') INCLUDED IN THE ASSEMBLY WHERE WE WANT TO GET EMBEDDED RESSOURCES 
                if ((rsc != null) && (rsc.Length > 0))
                {
                    // Now ensure to get ONLY wanted resources

                    resources = new List<String>();
                    foreach (String resource in rsc)
                    {
                        if (resource.StartsWith(NamespaceResources))
                            resources.Add(resource);
                    }
                }
            }
        }


#endregion RESOURCES - Get access to resources of this assembly

#region GET PATH TO RESOURCE FOR: Presence icon bullet, Receipt type, doc type

        public static String GetPresenceSourceFromPresence(Rainbow.Model.Presence presence)
        {
            String result = null;
            if (presence != null)
            {
                if (presence.PresenceLevel == Rainbow.Model.PresenceLevel.Online)
                {
                    if (presence.Resource.StartsWith("mobile"))
                        result = "presence_online_mobile.png";
                    else
                        result = "presence_online.png";
                }
                else if (presence.PresenceLevel == Rainbow.Model.PresenceLevel.Offline)
                    result = "presence_offline.png";
                else if (presence.PresenceLevel == Rainbow.Model.PresenceLevel.Away)
                    result = "presence_away.png";
                else if (presence.PresenceLevel == Rainbow.Model.PresenceLevel.Xa)
                    result = "presence_xa.png";
                else if (presence.PresenceLevel == Rainbow.Model.PresenceLevel.Dnd)
                    result = "presence_dnd.png";
                else if (presence.PresenceLevel == Rainbow.Model.PresenceLevel.Busy)
                    result = "presence_busy.png";
            }

            if (!String.IsNullOrEmpty(result))
                return NamespaceResources + ".images.presence." + result;

            return result;
        }

        public static String GetReceiptSourceIdFromReceiptType(String receiptType, String receiptReceived)
        {
            if (String.IsNullOrEmpty(receiptType))
                receiptType = "None";

            if (String.IsNullOrEmpty(receiptReceived))
                receiptReceived = receiptType;

            String result = null;
            switch (receiptReceived)
            {
                case "ClientRead":
                    result = "Font_CheckDouble|#3EA5D8";
                    break;

                case "ClientReceived":
                    if(receiptType != "ClientRead")
                        result = "Font_CheckDouble|#D3D3D3";
                    break;

                case "ServerReceived":
                    if ( (receiptType != "ClientRead") && (receiptType != "ClientReceived"))
                        result = "Font_Check|#D3D3D3";
                    break;

                case "None":
                    if (receiptType == "None")
                        result = "Font_Clock|#D3D3D3";
                    break;
            }
            
            return result;
        }

        public static String GetFileSourceIdFromFileName(String fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return null;

            String result = null;

            String extension = Path.GetExtension(fileName);
            switch (extension)
            {
                case ".doc":
                case ".docm":
                case ".docx":
                case ".dot":
                case ".dotx":
                    result = "Font_FileWord";
                    break;

                case ".pot":
                case ".potm":
                case ".potx":
                case ".pps":
                case ".ppsm":
                case ".ppsx":
                case ".ppt":
                case ".pptm":
                case ".pptx":
                    result = "Font_FilePowerpoint";
                    break;

                case ".xla":
                case ".xlam":
                case ".xlm":
                case ".xls":
                case ".xlsm":
                case ".xlsx":
                case ".xlt":
                case ".xltm":
                case ".xltx":
                    result = "Font_FileExcel";
                    break;

                case ".ai":
                case ".bmp":
                case ".gif":
                case ".ico":
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".ps":
                case ".svg":
                case ".tif":
                case ".tiff":
                    result = "Font_FileImage";
                    break;

                case ".aif":
                case ".cda":
                case ".mid":
                case ".midi":
                case ".mp3":
                case ".mpa":
                case ".ogg":
                case ".wav":
                case ".wma":
                case ".wpl":
                    result = "Font_FileAudio";
                    break;

                case ".3g2":
                case ".3gp":
                case ".avi":
                case ".flv":
                case ".h264":
                case ".m4v":
                case ".mkv":
                case ".mov":
                case ".mp4":
                case ".mpg":
                case ".mpeg":
                case ".rm":
                case ".swf":
                case ".vob":
                case ".wmv":
                    result = "Font_FileVideo";
                    break;

                case ".7z":
                case ".arj":
                case ".deb":
                case ".pkg":
                case ".rar":
                case ".rpm":
                case ".tar.gz":
                case ".z":
                case ".zip":
                    result = "Font_FileArchive";
                    break;

                case ".c":
                case ".cgi":
                case ".pl":
                case ".class":
                case ".cpp":
                case ".cs":
                case ".xaml":
                case ".h":
                case ".java":
                case ".php":
                case ".py":
                case ".sh":
                case ".swift":
                case ".vb":
                    result = "Font_FilePowerpoint";
                    break;

                case ".pdf":
                    result = "Font_FileCode";
                    break;

                case ".txt":
                case ".md":
                case ".json":
                    result = "Font_FileAlt";
                    break;

                default:
                    result = "Font_File";
                    break;
            }

            // Set color to white
            if (!String.IsNullOrEmpty(result))
                result += "|#FFFFFF";

            return result;
        }
       

#endregion GET PATH TO RESOURCE FOR: Presence icon bullet, Receipt type, doc type

#region ICON FONT

        public static Dictionary<String, String> iconsFont = new Dictionary<string, string>()
        {
            // Added in Font Awesome 'manually' using Glyphr Studio: https://www.glyphrstudio.com/online/
            { "Bubble", "\ue004" },
            { "DialPad", "\ue003" },

            // In Font Awesome (5.15.3)
            { "ArrowLeft", "\uf060" },
            { "ArrowRight", "\uf061" },
            { "Ban", "\uf05e" },
            { "Bars", "\uf0c9" },
            { "Bell", "\uf0f3" },
            { "BellSlash", "\uf1f6" },
            { "Check", "\uf00c" },
            { "CheckDouble", "\uf560" },
            { "Clock", "\uf017" },
            { "Calendar", "\uf133" },
            { "CalendarAlt", "\uf073" },
            { "CalendarCheck", "\uf274" },
            { "ChartBar", "\uf080" },
            { "ChessKing", "\uf43f" },
            { "ChessQueen", "\uf445" },
            { "Cloud", "\uf0c2" },
            { "Cog", "\uf013" },
            { "Comment", "\uf075" },
            { "CommentAlt", "\uf27a" },
            { "CommentDots", "\uf4ad" },
            { "Crown", "\uf521" },
            { "EllipsisH", "\uf141" },
            { "EllipsisV", "\uf142" },
            { "Exclamation", "\uf12a" },
            { "ExclamationTriangle", "\uf071" },
            { "File", "\uf15b" },
            { "FileAlt", "\uf15c" },
            { "FileArchive", "\uf1c6" },
            { "FileAudio", "\uf1c7" },
            { "FileCode", "\uf1c9" },
            { "FileExcel", "\uf1c3" },
            { "FileImage", "\uf1c5" },
            { "FilePdf", "\uf1c1" },
            { "FilePowerpoint", "\uf1c4" },
            { "FileVideo", "\uf1c8" },
            { "FileWord", "\uf1c2" },
            { "Filter", "\uf0b0" },
            { "Fire", "\uf06d" },
            { "Flag", "\uf024" },
            { "Gift", "\uf06b" },
            { "InfoCircle", "\uf05a" },
            { "Keyboard", "\uf11c" },
            { "Laugh", "\uf599" },
            { "Lightbulb", "\uf0eb" },
            { "Lock", "\uf023" },
            { "Microphone", "\uf130" },
            { "MicrophoneSlash", "\uf131" },
            { "MobileAlt", "\uf3cd" },
            { "Newspaper", "\uf1ea" },
            { "PaperClip", "\uf0c6" },
            { "PaperPlane", "\uf1d8" },
            { "PhoneVolume", "\uf2a0" },
            { "PuzzlePiece", "\uf12e" },
            { "Question", "\uf128" },
            { "QuestionCircle", "\uf059" },
            { "Share", "\uf064" },
            { "SignInAlt", "\uf2f6" },
            { "SignOutAlt", "\uf2f5" },
            { "SortAlphaDown", "\uf15d" },
            { "Star", "\uf005" },
            { "Store", "\uf54e" },
            { "Times", "\uf00d" },
            { "UnlockAlt", "\uf13e" },
            { "Users", "\uf0c0" },
            { "UserCircle", "\uf2bd" },
            { "UserAlt", "\uf406" },
            { "Voicemail", "\uf897" }
        };

#endregion ICON FONT

    }
}
