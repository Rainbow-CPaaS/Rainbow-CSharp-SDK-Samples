using Rainbow.SimpleJSON;
using Rainbow;
using System;
using System.Collections.Generic;
namespace Sample_Telephony
{
    public class RainbowServer
    {
        public String Name = "";
        public String AppId = "";
        public String AppKey = "";
        public String Hostname = "";
        public List<RainbowAccount> Accounts = new List<RainbowAccount>();

        public static Boolean TryJsonParse(JSONNode jsonNode, out RainbowServer rainbowServer)
        {
            rainbowServer = new RainbowServer();

            if ((jsonNode == null) || (!jsonNode.IsObject))
                return false;

            try
            {
                rainbowServer.Name = UtilJson.AsString(jsonNode, "name");
                rainbowServer.AppId = UtilJson.AsString(jsonNode, "appId");
                rainbowServer.AppKey = UtilJson.AsString(jsonNode, "appKey");
                rainbowServer.Hostname = UtilJson.AsString(jsonNode, "hostname");

                var accountsArray = jsonNode["accounts"];
                if (accountsArray?.IsArray == true)
                {
                    foreach (JSONNode jsonNodeRainbowServeObject in accountsArray)
                    {
                        if (RainbowAccount.TryJsonParse(jsonNodeRainbowServeObject, out RainbowAccount rainbowAccount))
                            rainbowServer.Accounts.Add(rainbowAccount);
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }

    public class RainbowAccount
    {
        public String Name = "";
        public String Login = "";
        public String Password = "";

        public static Boolean TryJsonParse(JSONNode jsonNode, out RainbowAccount rainbowAccount)
        {
            rainbowAccount = new RainbowAccount();

            if ((jsonNode == null) || (!jsonNode.IsObject))
                return false;

            try
            {
                rainbowAccount.Name = UtilJson.AsString(jsonNode, "name");
                rainbowAccount.Login = UtilJson.AsString(jsonNode, "login");
                rainbowAccount.Password = UtilJson.AsString(jsonNode, "password");
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
