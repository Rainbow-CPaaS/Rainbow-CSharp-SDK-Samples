using Rainbow.SimpleJSON;

namespace BotLibrary.Model
{
    public class Account
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Jid
        /// </summary>
        public string Jid { get; set; }

        /// <summary>
        /// Login
        /// </summary>
        public string Login { get; set; }

        public Account()
        {
            Id = "";
            Jid = "";
            Login = "";
        }

        public static Boolean FromJsonNode(JSONNode jsonNode, out Account account)
        {
            Boolean result = false;
            if (jsonNode is not null)
            {
                account = new()
                {
                    Id = jsonNode["id"],
                    Jid = jsonNode["jid"],
                    Login = jsonNode["login"]
                };

                // Check validity
                if ( String.IsNullOrEmpty(account.Id) 
                    && String.IsNullOrEmpty(account.Jid)
                    && String.IsNullOrEmpty(account.Login) )
                    result = false;
                else
                    result = true;
            }
            else
                account = new();

            return result;
        }

        public override string ToString()
        {
            String result = "";
            if (!String.IsNullOrEmpty(Id))
                result += $"Id:[{Id}] ";

            if (!String.IsNullOrEmpty(Jid))
                result += $"Jid:[{Jid}] ";

            if (!String.IsNullOrEmpty(Id))
                result += $"Login:[{Login}] ";

            return result.Trim();
        }
    }
}
