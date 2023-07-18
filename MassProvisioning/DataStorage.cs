using MassProvisioning.Model;
using Microsoft.Extensions.Logging;
using Rainbow.SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;

namespace MassProvisioning
{
    static class DataStorage
    {
        private static readonly ILogger log = Rainbow.LogFactory.CreateLogger("DataStorage");


#region PUBLIC METHODS

        public static void StoreCompanyToFile(Model.CompanyMassProvisioning company, String filepath)
        {
            if(CompanyMassProvisioning.TryToJson(company, out String jsonString))
            {
                try
                {
                    File.WriteAllText(filepath, jsonString);
                }
                catch (Exception e)
                {
                    log.LogWarning("[StoreCompanyToFile] Cannot store object - Error[{0}]", Rainbow.Util.SerializeException(e));
                }
            }
        }

        public static Model.CompanyMassProvisioning RestoreCompanyFromFile(String filepath)
        {
            try
            {
                if (!File.Exists(filepath))
                    return null;

                String jsonString = File.ReadAllText(filepath);

                if (CompanyMassProvisioning.TryJsonParse(jsonString, out Model.CompanyMassProvisioning companyMassProvisioning))
                    return companyMassProvisioning;
            }
            catch (Exception e)
            {
                log.LogWarning("[RestoreCompanyFromFile] Error[{0}]", Rainbow.Util.SerializeException(e));
            }

            return null;
        }

        public static void StoreUserToFile(Model.UserMassProvisioning contact, String filepath)
        {
            if (UserMassProvisioning.TryToJson(contact, out String jsonString))
            {
                try
                {
                    File.WriteAllText(filepath, jsonString);
                }
                catch (Exception e)
                {
                    log.LogWarning("[StoreUserToFile] Cannot store object - Error[{0}]", Rainbow.Util.SerializeException(e));
                }
            }
        }

        public static Model.UserMassProvisioning RestoreUserFromFile(String filepath)
        {
            try
            {
                if (!File.Exists(filepath))
                    return null;

                String jsonString = File.ReadAllText(filepath);

                if (UserMassProvisioning.TryJsonParse(jsonString, out Model.UserMassProvisioning userMassProvisioning))
                    return userMassProvisioning;
            }
            catch (Exception e)
            {
                log.LogWarning("[RestoreUserFromFile] Error[{0}]", Rainbow.Util.SerializeException(e));
            }

            return null;
        }

        public static void StoreUsersToFile(Dictionary<String, Model.UserMassProvisioning> contacts, String filepath)
        {
            JSONNode rootNode = new JSONObject();
            JSONNode jsonNode;
            foreach (var id in contacts.Keys)
            { 
                var contact = contacts[id];
                if (UserMassProvisioning.TryToJson(contact, out jsonNode))
                    rootNode.Add(id, jsonNode);
            }

            try
            {
                File.WriteAllText(filepath, rootNode.ToString());
            }
            catch (Exception e)
            {
                log.LogWarning("[StoreUsersToFile] Cannot store object - Error[{0}]", Rainbow.Util.SerializeException(e));
            }
        }

        public static Dictionary<String, Model.UserMassProvisioning> RestoreUsersFromFile(String filepath)
        {
            try
            {
                if (!File.Exists(filepath))
                    return null;

                String jsonString = File.ReadAllText(filepath);

                var rootNode = JSON.Parse(jsonString);
                if(rootNode != null)
                {
                    Dictionary<String, Model.UserMassProvisioning> result = new Dictionary<string, UserMassProvisioning>();
                    Model.UserMassProvisioning userMassProvisioning;

                    foreach (var node in rootNode)
                    {
                        if (UserMassProvisioning.TryJsonParse(node.Value, out userMassProvisioning))
                        {
                            result.Add(node.Key, userMassProvisioning);
                        }
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                log.LogWarning("[RestoreUsersFromFile] Error[{0}]", Rainbow.Util.SerializeException(e));
            }

            return null;

        }


        public static void StoreClassroomsToFile(Dictionary<String, Dictionary<String, List<String>>> classrooms, String filepath)
        {
            var classroomJsonNode = new JSONObject();
            foreach (var id in classrooms.Keys)
            {
                var classroom = classrooms[id];
                var jsonNode = new JSONObject();
                foreach(var classroomItem in classroom)
                {
                    var key = classroomItem.Key;
                    var classroomInfoNode = new JSONArray { AsStringList = classroomItem.Value };
                    jsonNode.Add(key, classroomInfoNode);
                }
                classroomJsonNode.Add(id, jsonNode);
            }

            try
            {
                File.WriteAllText(filepath, classroomJsonNode.ToString());
            }
            catch (Exception e)
            {
                log.LogWarning("[StoreClassroomsToFile] Cannot store object - Error[{0}]", Rainbow.Util.SerializeException(e));
            }
        }

        public static Dictionary<String, Dictionary<String, List<String>>> RestoreClassroomsFromFile(String filepath)
        {
            try
            {
                if (!File.Exists(filepath))
                    return null;

                String jsonString = File.ReadAllText(filepath);

                var rootNode = JSON.Parse(jsonString);
                if (rootNode != null)
                {
                    var  result = new Dictionary<String, Dictionary<String, List<String>>>();

                    foreach (var node in rootNode)
                    {
                        var classroom = new Dictionary<String, List<String>>();
                        foreach (var subNode in node.Value)
                        {
                            classroom.Add(subNode.Key, subNode.Value.AsStringList);
                        }
                        result.Add(node.Key, classroom);
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                log.LogWarning("[RestoreClassroomsFromFile] Error[{0}]", Rainbow.Util.SerializeException(e));
            }
            return null;
        }

#endregion PUBLIC METHODS

    }
}
