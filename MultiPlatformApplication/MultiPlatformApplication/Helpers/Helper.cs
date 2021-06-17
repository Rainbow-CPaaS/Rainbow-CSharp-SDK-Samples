using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
        public static string HumanizeFileSize(this double fileSizeInOctet)
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

        public static ConversationModel GetConversationFromRBConversation(Rainbow.Model.Conversation rbConversation)
        {
            ConversationModel conversation = null;

            if (rbConversation != null)
            {
                conversation = new ConversationModel();
                conversation.Id = rbConversation.Id;


                if (rbConversation.Type == Rainbow.Model.Conversation.ConversationType.Room)
                {
                    conversation.Name = rbConversation.Name;
                    conversation.Topic = rbConversation.Topic;
                    conversation.Jid = rbConversation.Jid_im;
                    conversation.PresenceSource = "";

                }
                else if (rbConversation.Type == Rainbow.Model.Conversation.ConversationType.User)
                {
                    // Get Display name of this user
                    Rainbow.Model.Contact contact = XamarinApplication.SdkWrapper.GetContactFromContactId(rbConversation.PeerId);
                    if (contact != null)
                    {
                        conversation.Name = Rainbow.Util.GetContactDisplayName(contact);
                        conversation.Topic = "";
                        conversation.Jid = contact.Jid_im;

                        Presence presence = XamarinApplication.SdkWrapper.GetAggregatedPresenceFromContactId(rbConversation.PeerId);
                        conversation.PresenceSource = Helpers.Helper.GetPresenceSourceFromPresence(presence);

                    }
                    else
                    {
                        //log.Debug("[GetConversationFromRBConversation] - unknown contact - contactId:[{0}]", rbConversation.PeerId);
                        XamarinApplication.SdkWrapper.GetContactFromContactIdFromServer(rbConversation.PeerId, null);
                    }
                }
                else
                {
                    //TODO ( bot case)
                    //log.Debug("[GetConversationFromRBConversation] Conversation from model not created - Id:[{0}]", rbConversation.Id);
                    return null;
                }

                conversation.PeerId = rbConversation.PeerId;

                conversation.Type = rbConversation.Type;
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
                        result = displayName + " has ended the conference";
                        break;
                    case "conferenceStarted":
                        result = displayName + " has started the conference";
                        break;
                    case "userJoin":
                        result = displayName + " has joined the conference";
                        break;
                    case "userLeave":
                        result = displayName + " has left the conference";
                        break;
                    default:
                        result = displayName + " - " + bubbleEvent;
                        break;
                }
            }

            return result;
        }


        public static String GetLabel(String key)
        {
            if (Languages == null)
            {
                // Get Rainbow Languages service
                Languages = Rainbow.Common.Languages.Instance;
            }
            return Languages.GetLabel(key);
        }

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
                    filePath = XamarinApplication.SdkWrapper.GetBubbleAvatarPath(bubbleId);
                    log.Debug("[GetBubbleAvatarFilePath] Bubble Avatar - Id:[{0}] - FilePath:[{1}]", bubbleId, filePath);
                }
                catch (Exception exc)
                {
                    log.Warn("[GetBubbleAvatarImageSource] bubbleId:[{0}] - exception occurs to create avatar:[{1}]", bubbleId, Rainbow.Util.SerializeException(exc));
                    filePath = XamarinApplication.SdkWrapper.GetUnknwonBubbleAvatarFilePath();
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
                    filePath = XamarinApplication.SdkWrapper.GetContactAvatarPath(contactId);
                    log.Debug("[GetContactAvatarImageSource] contactId:[{0}] - filePath:[{1}]", contactId, filePath);
                }
                catch (Exception exc)
                {
                    log.Warn("[GetContactAvatarImageSource] contactId:[{0}] - exception occurs to create avatar:[{1}]", contactId, Rainbow.Util.SerializeException(exc));
                    filePath = XamarinApplication.SdkWrapper.GetUnknwonContactAvatarFilePath();
                }
            }
            return filePath;
        }

        public static String GetConversationAvatarFilePath(ConversationModel conversation)
        {
            String filePath = null;
            if (conversation != null)
            {
                if (conversation.Type == Rainbow.Model.Conversation.ConversationType.User)
                    filePath = GetContactAvatarFilePath(conversation.PeerId);
                else if (conversation.Type == Rainbow.Model.Conversation.ConversationType.Room)
                    filePath = GetBubbleAvatarFilePath(conversation.PeerId);
            }
            return filePath;
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
                foreach (String resource in resources)
                {
                    if (resource == resourceName)
                        result =  resource;
                }
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

        // Get image source using Id from merged resource dictionaries
        static internal ImageSource GetImageSourceFromResourceDictionaryById(ResourceDictionary dictionary, String dictionaryId)
        {
            ImageSource result = null;

            if (dictionary?.ContainsKey(dictionaryId) == true)
            {
                var obj = dictionary[dictionaryId];
                if (obj is ImageSource)
                    result = (ImageSource)obj;
            }

            if (result == null)
            {
                // Check on child merged dictionaries
                if (dictionary.MergedDictionaries?.Count > 0)
                {
                    List<ResourceDictionary> list = dictionary.MergedDictionaries.ToList();
                    for (int index = 0; index < dictionary.MergedDictionaries.Count; index++)
                    {
                        result = GetImageSourceFromResourceDictionaryById(list[index], dictionaryId);
                        if (result != null)
                            break;
                    }
                }
            }

            return result;
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

        public static String GetReceiptSourceFromReceiptType(ReceiptType receiptType)
        {
            String result = null;
            switch (receiptType)
            {
                case ReceiptType.ServerReceived:
                    result = "msg_check.png";
                    break;
                case ReceiptType.ClientReceived:
                    result = "msg_not_read.png";
                    break;
                case ReceiptType.ClientRead:
                    result = "msg_read.png";
                    break;
                case ReceiptType.None:
                    result = "msg_sending.png";
                    break;
            }
            if (!String.IsNullOrEmpty(result))
                return NamespaceResources + ".images.message." + result;

            return null;
        }

        public static String GetFileAttachmentSourceFromFileName(String fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return null;

            String result = null;

            String extension = Path.GetExtension(fileName);
            switch(extension)
            {
                case ".doc":
                case ".docm":
                case ".docx":
                case ".dot":
                case ".dotx":
                    result = "icon_doc_blue.png";
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
                    result = "icon_ppt_blue.png";
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
                    result = "icon_xls_blue.png";
                    break;

                case ".bmp":
                case ".gif":
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".svg":
                case ".tif":
                case ".tiff":
                    result = "icon_image_blue.png";
                    break;

                case ".pdf":
                    result = "icon_pdf_blue.png";
                    break;

                default:
                    result = "icon_unknown_blue.png";
                    break;
            }

            if (!String.IsNullOrEmpty(result))
                return NamespaceResources + ".images.document." + result;

            return result;
        }

#endregion GET PATH TO RESOURCE FOR: Presence icon bullet, Receipt type, doc type

    }
}
