using Rainbow.SimpleJSON;
using Rainbow;
using System;

namespace MassProvisioning.Model
{
    public class CompanyMassProvisioning
    {
        public String Name { get; set; }

        public String Country { get; set; }

        public String State { get; set; }

        /// <summary>
        /// Converts the specified JSON String to its <see cref="CompanyMassProvisioning"/> equivalent and returns a value that indicates whetever the conversion succeeded
        /// </summary>
        /// <param name="jsonString"><see cref="String"/>JSON String</param>
        /// <param name="companyMassProvisioning"><see cref="CompanyMassProvisioning"/>CompanyMassProvisioning object</param>
        /// <returns><see cref="Boolean"/> - True on success</returns>
        public static Boolean TryJsonParse(string jsonString, out CompanyMassProvisioning companyMassProvisioning)
        {
            return TryJsonParse(JSON.Parse(jsonString), out companyMassProvisioning);
        }

        /// <summary>
        /// Converts the specified <see cref="JSONNode"/> to its <see cref="CompanyMassProvisioning"/> equivalent and returns a value that indicates whetever the conversion succeeded
        /// </summary>
        /// <param name="jsonNode"><see cref="JSONNode"/>JSONNode object</param>
        /// <param name="companyMassProvisioning"><see cref="CompanyMassProvisioning"/>CompanyMassProvisioning object</param>
        /// <returns><see cref="Boolean"/> - True on success</returns>
        public static Boolean TryJsonParse(JSONNode jsonNode, out CompanyMassProvisioning companyMassProvisioning)
        {
            companyMassProvisioning = null;

            if ((jsonNode == null) || (!jsonNode.IsObject))
                return false;

            try
            {
                companyMassProvisioning = new CompanyMassProvisioning();
                companyMassProvisioning.Name = UtilJson.AsString(jsonNode, "name");
                companyMassProvisioning.Country = UtilJson.AsString(jsonNode, "country");
                companyMassProvisioning.State = UtilJson.AsString(jsonNode, "state");
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Converts the specified <see cref="CompanyMassProvisioning"/> to its JSON String equivalent and returns a value that indicates whetever the conversion succeeded
        /// </summary>
        /// <param name="companyMassProvisioning"><see cref="CompanyMassProvisioning"/>CompanyMassProvisioning object</param>
        /// <param name="jsonString"><see cref="String"/>String</param>
        /// <returns><see cref="Boolean"/> - True on success</returns>
        public static Boolean TryToJson(CompanyMassProvisioning companyMassProvisioning, out String jsonString)
        {
            try
            {
                if (TryToJson(companyMassProvisioning, out JSONNode jsonNode))
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
        /// Converts the specified <see cref="CompanyMassProvisioning"/> to its <see cref="JSONNode"/> equivalent and returns a value that indicates whetever the conversion succeeded
        /// </summary>
        /// <param name="companyMassProvisioning"><see cref="CompanyMassProvisioning"/>CompanyMassProvisioning object</param>
        /// <param name="jsonNode"><see cref="JSONNode"/>JSONNode object</param>
        /// <returns><see cref="Boolean"/> - True on success</returns>
        public static Boolean TryToJson(CompanyMassProvisioning companyMassProvisioning, out JSONNode jsonNode)
        {
            jsonNode = "";

            if (companyMassProvisioning == null)
                return false;
            try
            {
                jsonNode = new JSONObject();

                UtilJson.AddNode(jsonNode, "name", companyMassProvisioning.Name);
                UtilJson.AddNode(jsonNode, "country", companyMassProvisioning.Country);
                UtilJson.AddNode(jsonNode, "state", companyMassProvisioning.State);
            }
            catch
            {
                return false;
            }
            return true;
        }

    }
}
