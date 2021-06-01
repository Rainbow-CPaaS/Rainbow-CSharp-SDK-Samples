using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Newtonsoft.Json;

using Rainbow;

using NLog;

namespace MassProvisioning
{
    static class DataStorage
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(DataStorage));


#region PUBLIC METHODS

        public static void StoreCompanyToFile(Model.CompanyMassProvisioning company, String filepath)
        {
            StoreObjectToFile(company, filepath);
        }

        public static Model.CompanyMassProvisioning RestoreCompanyFromFile(String filepath)
        {
            return RestoreObjectFromFile<Model.CompanyMassProvisioning>(filepath);
        }

        public static void StoreUserToFile(Model.UserMassProvisioning contact, String filepath)
        {
            StoreObjectToFile(contact, filepath);
        }

        public static Model.UserMassProvisioning RestoreUserFromFile(String filepath)
        {
            return RestoreObjectFromFile<Model.UserMassProvisioning>(filepath);
        }

        public static void StoreUsersToFile(Dictionary<String, Model.UserMassProvisioning> contacts, String filepath)
        {
            StoreObjectToFile(contacts, filepath);
        }

        public static Dictionary<String, Model.UserMassProvisioning> RestoreUsersFromFile(String filepath)
        {
            return RestoreObjectFromFile<Dictionary<String, Model.UserMassProvisioning>>(filepath);
        }


        public static void StoreClassroomsToFile(Dictionary<String, Dictionary<String, List<String>>> classrooms, String filepath)
        {
            StoreObjectToFile(classrooms, filepath);
        }

        public static Dictionary<String, Dictionary<String, List<String>>> RestoreClassroomsFromFile(String filepath)
        {
            return RestoreObjectFromFile<Dictionary<String, Dictionary<String, List<String>>>>(filepath);
        }

#endregion PUBLIC METHODS

        private static void StoreObjectToFile(Object obj, String filepath)
        {
            String jsonString = Rainbow.Util.GetJsonStringFromObject(obj);

            try
            {
                File.WriteAllText(filepath, jsonString);
            }
            catch (Exception e)
            {
                log.Warn("[StoreObjectToFile] Cannot store object - Error[{0}]", Rainbow.Util.SerializeException(e));
            }
        }

        private static T RestoreObjectFromFile<T>(String filepath)
        {
            T result = default(T);

            try
            {
                if (!File.Exists(filepath))
                    return default(T);

                String jsonString = File.ReadAllText(filepath);

                result = JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch (Exception e)
            {
                log.Warn("[RestoreObjectFromFile] Error[{0}]", Rainbow.Util.SerializeException(e));
            }

            return result;
        }


    }
}
