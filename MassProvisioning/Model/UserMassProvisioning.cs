
using Rainbow;
using Rainbow.SimpleJSON;
using System;

namespace MassProvisioning.Model
{
    public class UserMassProvisioning
    {
        public String Id { get; set; }

        public String FirstName { get; set; }

        public String LastName { get; set; }

        public String Email { get; set; }

        public String Password { get; set; }

        /// <summary>
        /// Converts the specified JSON String to its <see cref="UserMassProvisioning"/> equivalent and returns a value that indicates whetever the conversion succeeded
        /// </summary>
        /// <param name="jsonString"><see cref="String"/>JSON String</param>
        /// <param name="userMassProvisioning"><see cref="UserMassProvisioning"/>AlertHistory object</param>
        /// <returns><see cref="Boolean"/> - True on success</returns>
        public static Boolean TryJsonParse(string jsonString, out UserMassProvisioning userMassProvisioning)
        {
            return TryJsonParse(JSON.Parse(jsonString), out userMassProvisioning);
        }

        /// <summary>
        /// Converts the specified <see cref="JSONNode"/> to its <see cref="UserMassProvisioning"/> equivalent and returns a value that indicates whetever the conversion succeeded
        /// </summary>
        /// <param name="jsonNode"><see cref="JSONNode"/>JSONNode object</param>
        /// <param name="userMassProvisioning"><see cref="UserMassProvisioning"/>UserMassProvisioning object</param>
        /// <returns><see cref="Boolean"/> - True on success</returns>
        public static Boolean TryJsonParse(JSONNode jsonNode, out UserMassProvisioning userMassProvisioning)
        {
            userMassProvisioning = null;

            if ((jsonNode == null) || (!jsonNode.IsObject))
                return false;

            try
            {
                userMassProvisioning = new UserMassProvisioning();
                userMassProvisioning.Id = UtilJson.AsString(jsonNode, "id");
                userMassProvisioning.FirstName = UtilJson.AsString(jsonNode, "firstName");
                userMassProvisioning.LastName = UtilJson.AsString(jsonNode, "lastName");
                userMassProvisioning.Email = UtilJson.AsString(jsonNode, "email");
                userMassProvisioning.Password = UtilJson.AsString(jsonNode, "password");
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Converts the specified <see cref="UserMassProvisioning"/> to its JSON String equivalent and returns a value that indicates whetever the conversion succeeded
        /// </summary>
        /// <param name="userMassProvisioning"><see cref="UserMassProvisioning"/>UserMassProvisioning object</param>
        /// <param name="jsonString"><see cref="String"/>String</param>
        /// <returns><see cref="Boolean"/> - True on success</returns>
        public static Boolean TryToJson(UserMassProvisioning userMassProvisioning, out String jsonString)
        {
            try
            {
                if (TryToJson(userMassProvisioning, out JSONNode jsonNode))
                {
                    jsonString = jsonNode.ToString();
                    return true;
                }
            }
            catch { }

            jsonString = "";
            return false;
        }

        /// <summary>
        /// Converts the specified <see cref="UserMassProvisioning"/> to its <see cref="JSONNode"/> equivalent and returns a value that indicates whetever the conversion succeeded
        /// </summary>
        /// <param name="userMassProvisioning"><see cref="UserMassProvisioning"/>UserMassProvisioning object</param>
        /// <param name="jsonNode"><see cref="JSONNode"/>JSONNode object</param>
        /// <returns><see cref="Boolean"/> - True on success</returns>
        public static Boolean TryToJson(UserMassProvisioning userMassProvisioning, out JSONNode jsonNode)
        {
            jsonNode = "";

            if (userMassProvisioning == null)
                return false;
            try
            {
                jsonNode = new JSONObject();

                UtilJson.AddNode(jsonNode, "id", userMassProvisioning.Id);
                UtilJson.AddNode(jsonNode, "firstName", userMassProvisioning.FirstName);
                UtilJson.AddNode(jsonNode, "lastName", userMassProvisioning.LastName);
                UtilJson.AddNode(jsonNode, "email", userMassProvisioning.Email);
                UtilJson.AddNode(jsonNode, "password", userMassProvisioning.Password);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
