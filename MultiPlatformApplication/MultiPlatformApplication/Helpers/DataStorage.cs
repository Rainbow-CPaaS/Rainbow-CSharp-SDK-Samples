using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Newtonsoft.Json;

using Rainbow;

using Microsoft.Extensions.Logging;
using Rainbow.Model;

namespace MultiPlatformApplication.Helpers
{
    static public class DataStorage
    {
        private static readonly ILogger log = Rainbow.LogFactory.CreateLogger("DataStorage");

        private static String ConversationsStorageName = "ConversationsList.json";
        private static String CurrentContactStorageName = "CurrentContact.json";
        private static String ContactsStorageName = "ContactsList.json";
        private static String BubblesStorageName = "BubblesList.json";
        private static String MessagesStorageName = "MessagesList_{0}.json";
        private static String PresencesStorageName = "PresencesList.json";
        private static String PresencesAggregatedStorageName = "PresencesAggregatedList.json";
        private static String FilesDescriptorStorageName = "FilesDescriptorList.json";
        private static String ConversationsByFileDescriptorStorageName = "ConversationsByFileDescriptorList.json";

        private static Boolean FolderCreated = false;

        private static Boolean CreateDataStorageFolder()
        {
            if (!ApplicationInfo.DataStorageUsed)
                return false;

            if (FolderCreated) 
                return true;

            String folderPath = Helper.GetDataStorageFolderPath();

            try
            {
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                FolderCreated = true;
            }
            catch(Exception exc)
            {
                log.LogError("[CreateDataStorageFolder] Cannot create DataStorage folder - Exception:[{0}]", Rainbow.Util.SerializeException(exc));
            }
            return FolderCreated;
        }

        public static void StoreConversations(List<Rainbow.Model.Conversation> conversations)
        {
            if (!CreateDataStorageFolder())
                return;

            String jsonString = Rainbow.Util.GetJsonStringFromObject(conversations);

            String filepath = Path.Combine(Helper.GetDataStorageFolderPath(), ConversationsStorageName);
            try
            {
                File.WriteAllText(filepath, jsonString);
            }
            catch (Exception e)
            {
                log.LogWarning("[StoreConversations] Cannot store conversations - Error[{0}]", Rainbow.Util.SerializeException(e));
            }
        }

        public static List<Rainbow.Model.Conversation> RetrieveConversations(String fileName = null)
        {
            if (!CreateDataStorageFolder())
                return null;

            List<Rainbow.Model.Conversation> result = null;

            String filepath;
            if (String.IsNullOrEmpty(fileName))
                filepath = Path.Combine(Helper.GetDataStorageFolderPath(), ConversationsStorageName);
            else
                filepath = Path.Combine(Helper.GetDataStorageFolderPath(), fileName);

            try
            {
                if (!File.Exists(filepath))
                    return null;

                String jsonString = File.ReadAllText(filepath);

                result = JsonConvert.DeserializeObject<List<Rainbow.Model.Conversation>>(jsonString);
            }
            catch (Exception e)
            {
                log.LogWarning("[RetrieveConversations] Error[{0}]", Rainbow.Util.SerializeException(e));
            }

            return result;
        }

        public static void StoreBubbles(List<Rainbow.Model.Bubble> bubbles)
        {
            if (!CreateDataStorageFolder())
                return;

            String jsonString = Rainbow.Util.GetJsonStringFromObject(bubbles);

            String filepath = Path.Combine(Helper.GetDataStorageFolderPath(), BubblesStorageName);
            try
            {
                File.WriteAllText(filepath, jsonString);
            }
            catch (Exception e)
            {
                log.LogWarning("[StoreBubbles] Cannot store conversations - Error[{0}]", Rainbow.Util.SerializeException(e));
            }
        }

        public static List<Rainbow.Model.Bubble> RetrieveBubbles(String fileName = null)
        {
            if (!CreateDataStorageFolder())
                return null;

            List<Rainbow.Model.Bubble> result = null;

            String filepath;
            if (String.IsNullOrEmpty(fileName))
                filepath = Path.Combine(Helper.GetDataStorageFolderPath(), BubblesStorageName);
            else
                filepath = Path.Combine(Helper.GetDataStorageFolderPath(), fileName);

            try
            {
                if (!File.Exists(filepath))
                    return null;

                String jsonString = File.ReadAllText(filepath);

                result = JsonConvert.DeserializeObject<List<Rainbow.Model.Bubble>>(jsonString);
            }
            catch (Exception e)
            {
                log.LogWarning("[RetrieveBubbles] Error[{0}]", Rainbow.Util.SerializeException(e));
            }

            return result;
        }

        public static void StoreContacts(List<Rainbow.Model.Contact> contacts)
        {
            if (!CreateDataStorageFolder())
                return;

            String jsonString = Rainbow.Util.GetJsonStringFromObject(contacts);

            String filepath = Path.Combine(Helper.GetDataStorageFolderPath(), ContactsStorageName);
            try
            {
                File.WriteAllText(filepath, jsonString);
            }
            catch (Exception e)
            {
                log.LogWarning("[StoreContacts] Cannot store conversations - Error[{0}]", Rainbow.Util.SerializeException(e));
            }
        }

        public static List<Rainbow.Model.Contact> RetrieveContacts(String fileName = null)
        {
            if (!CreateDataStorageFolder())
                return null;

            List<Rainbow.Model.Contact> result = null;

            String filepath;
            if (String.IsNullOrEmpty(fileName))
                filepath = Path.Combine(Helper.GetDataStorageFolderPath(), ContactsStorageName);
            else
                filepath = Path.Combine(Helper.GetDataStorageFolderPath(), fileName);

            try
            {
                if (!File.Exists(filepath))
                    return null;

                String jsonString = File.ReadAllText(filepath);

                result = JsonConvert.DeserializeObject<List<Rainbow.Model.Contact>>(jsonString);
            }
            catch (Exception e)
            {
                log.LogWarning("[RetrieveBubbles] Error[{0}]", Rainbow.Util.SerializeException(e));
            }

            return result;
        }

        public static void StoreCurrentContact(Rainbow.Model.Contact currentContact)
        {
            if (!CreateDataStorageFolder())
                return;

            String jsonString = Rainbow.Util.GetJsonStringFromObject(currentContact);

            String filepath = Path.Combine(Helper.GetDataStorageFolderPath(), CurrentContactStorageName);
            try
            {
                File.WriteAllText(filepath, jsonString);
            }
            catch (Exception e)
            {
                log.LogWarning("[StoreCurrentContact] Cannot store current contact - Error[{0}]", Rainbow.Util.SerializeException(e));
            }
        }

        public static Rainbow.Model.Contact RetrieveCurrentContact(String fileName = null)
        {
            if (!CreateDataStorageFolder())
                return null;

            Rainbow.Model.Contact result = null;

            String filepath;
            if (String.IsNullOrEmpty(fileName))
                filepath = Path.Combine(Helper.GetDataStorageFolderPath(), CurrentContactStorageName);
            else
                filepath = Path.Combine(Helper.GetDataStorageFolderPath(), fileName);

            try
            {
                if (!File.Exists(filepath))
                    return null;

                String jsonString = File.ReadAllText(filepath);

                result = JsonConvert.DeserializeObject<Rainbow.Model.Contact>(jsonString);
            }
            catch (Exception e)
            {
                log.LogWarning("[RetrieveBubbles] Error[{0}]", Rainbow.Util.SerializeException(e));
            }

            return result;
        }

        public static void StoreMessages(String conversationId, List<Rainbow.Model.Message> messages )
        {
            if (!CreateDataStorageFolder())
                return;

            String jsonString = Rainbow.Util.GetJsonStringFromObject(messages);

            String storageName = String.Format(MessagesStorageName, conversationId);

            String filepath = Path.Combine(Helper.GetDataStorageFolderPath(), storageName);
            try
            {
                File.WriteAllText(filepath, jsonString);
            }
            catch (Exception e)
            {
                log.LogWarning("[StoreMessages] Cannot store messages - ConversationId:[{1}] - Error[{0}]", Rainbow.Util.SerializeException(e), conversationId);
            }
        }

        public static List<Rainbow.Model.Message> RetrieveMessages(String conversationId, String fileName = null)
        {
            if (!CreateDataStorageFolder())
                return null;

            List<Rainbow.Model.Message> result = null;

            String filepath;
            if (String.IsNullOrEmpty(fileName))
                filepath = Path.Combine(Helper.GetDataStorageFolderPath(), String.Format(MessagesStorageName, conversationId));
            else
                filepath = Path.Combine(Helper.GetDataStorageFolderPath(), fileName);

            try
            {
                if (!File.Exists(filepath))
                    return null;

                String jsonString = File.ReadAllText(filepath);

                result = JsonConvert.DeserializeObject<List<Rainbow.Model.Message>>(jsonString);
            }
            catch (Exception e)
            {
                log.LogWarning("[RetrieveMessages] ConversationId:[{1}] - Error[{0}]", Rainbow.Util.SerializeException(e), conversationId);
            }

            return result;
        }

        public static void StorePresences(Dictionary<string, Dictionary<String, Rainbow.Model.Presence>> presences)
        {
            if (!CreateDataStorageFolder())
                return;

            String jsonString = Rainbow.Util.GetJsonStringFromObject(presences);

            String filepath = Path.Combine(Helper.GetDataStorageFolderPath(), PresencesStorageName);
            try
            {
                File.WriteAllText(filepath, jsonString);
            }
            catch (Exception e)
            {
                log.LogWarning("[StorePresences] Cannot store presences - Error[{0}]", Rainbow.Util.SerializeException(e));
            }
        }

        public static Dictionary<string, Dictionary<String, Rainbow.Model.Presence>> RetrievePresences(String fileName = null)
        {
            if (!CreateDataStorageFolder())
                return null;

            Dictionary<string, Dictionary<String, Rainbow.Model.Presence>> result = null;

            String filepath;
            if (String.IsNullOrEmpty(fileName))
                filepath = Path.Combine(Helper.GetDataStorageFolderPath(), PresencesStorageName);
            else
                filepath = Path.Combine(Helper.GetDataStorageFolderPath(), fileName);

            try
            {
                if (!File.Exists(filepath))
                    return null;

                String jsonString = File.ReadAllText(filepath);

                result = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<String, Rainbow.Model.Presence>>>(jsonString);
            }
            catch (Exception e)
            {
                log.LogWarning("[RetrievePresences] Error[{0}]", Rainbow.Util.SerializeException(e));
            }

            return result;
        }

        public static void StoreAggregatedPresences(Dictionary<string, Rainbow.Model.Presence> aggregatePresences)
        {
            if (!CreateDataStorageFolder())
                return;

            String jsonString = Rainbow.Util.GetJsonStringFromObject(aggregatePresences);

            String filepath = Path.Combine(Helper.GetDataStorageFolderPath(), PresencesAggregatedStorageName);
            try
            {
                File.WriteAllText(filepath, jsonString);
            }
            catch (Exception e)
            {
                log.LogWarning("[StoreAggregatedPresences] Cannot store aggregated presences - - Error[{0}]", Rainbow.Util.SerializeException(e));
            }
        }

        public static Dictionary<string, Rainbow.Model.Presence> RetrieveAggregatedPresences(String fileName = null)
        {
            if (!CreateDataStorageFolder())
                return null;

            Dictionary<string, Rainbow.Model.Presence> result = null;

            String filepath;
            if (String.IsNullOrEmpty(fileName))
                filepath = Path.Combine(Helper.GetDataStorageFolderPath(), PresencesAggregatedStorageName);
            else
                filepath = Path.Combine(Helper.GetDataStorageFolderPath(), fileName);

            try
            {
                if (!File.Exists(filepath))
                    return null;

                String jsonString = File.ReadAllText(filepath);

                result = JsonConvert.DeserializeObject<Dictionary<string, Rainbow.Model.Presence>>(jsonString);
            }
            catch (Exception e)
            {
                log.LogWarning("[RetrieveAggregatedPresences] Error[{0}]", Rainbow.Util.SerializeException(e));
            }

            return result;
        }

        public static void StoreFilesDescriptor(Dictionary<String, FileDescriptor> filesDescriptor)
        {
            if (!CreateDataStorageFolder())
                return;

            String jsonString = Rainbow.Util.GetJsonStringFromObject(filesDescriptor);

            String filepath = Path.Combine(Helper.GetDataStorageFolderPath(), FilesDescriptorStorageName);
            try
            {
                File.WriteAllText(filepath, jsonString);
            }
            catch (Exception e)
            {
                log.LogWarning("[StoreFilesDescriptor] Cannot store files descriptor  - - Error[{0}]", Rainbow.Util.SerializeException(e));
            }
        }

        public static Dictionary<String, FileDescriptor> RetrieveFilesDescriptor(String fileName = null)
        {
            if (!CreateDataStorageFolder())
                return null;

            Dictionary<String, FileDescriptor> result = null;

            String filepath;
            if (String.IsNullOrEmpty(fileName))
                filepath = Path.Combine(Helper.GetDataStorageFolderPath(), FilesDescriptorStorageName);
            else
                filepath = Path.Combine(Helper.GetDataStorageFolderPath(), fileName);

            try
            {
                if (!File.Exists(filepath))
                    return null;

                String jsonString = File.ReadAllText(filepath);

                result = JsonConvert.DeserializeObject<Dictionary<String, FileDescriptor>>(jsonString);
            }
            catch (Exception e)
            {
                log.LogWarning("[RetrieveFilesDescriptors] Error[{0}]", Rainbow.Util.SerializeException(e));
            }

            return result;
        }

        public static void StoreConversationsByFilesDescriptor(Dictionary<String, String> conversationsByFilesDescriptor)
        {
            if (!CreateDataStorageFolder())
                return;

            String jsonString = Rainbow.Util.GetJsonStringFromObject(conversationsByFilesDescriptor);

            String filepath = Path.Combine(Helper.GetDataStorageFolderPath(), ConversationsByFileDescriptorStorageName);
            try
            {
                File.WriteAllText(filepath, jsonString);
            }
            catch (Exception e)
            {
                log.LogWarning("[StoreConversationsByFilesDescriptor] Cannot store Conversations By Files Descriptor - - Error[{0}]", Rainbow.Util.SerializeException(e));
            }
        }

        public static Dictionary<String, String> RetrieveConversationsByFilesDescriptor(String fileName = null)
        {
            if (!CreateDataStorageFolder())
                return null;

            Dictionary<String, String> result = null;

            String filepath;
            if (String.IsNullOrEmpty(fileName))
                filepath = Path.Combine(Helper.GetDataStorageFolderPath(), ConversationsByFileDescriptorStorageName);
            else
                filepath = Path.Combine(Helper.GetDataStorageFolderPath(), fileName);

            try
            {
                if (!File.Exists(filepath))
                    return null;

                String jsonString = File.ReadAllText(filepath);

                result = JsonConvert.DeserializeObject<Dictionary<String, String>>(jsonString);
            }
            catch (Exception e)
            {
                log.LogWarning("[RetrieveConversationsByFilesDescriptor] Error[{0}]", Rainbow.Util.SerializeException(e));
            }

            return result;
        }

    }
}
