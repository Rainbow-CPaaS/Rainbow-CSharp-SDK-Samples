using Rainbow.SimpleJSON;

namespace BotLibrary.Model
{
    public class BotConfiguration
    {
        /// <summary>
        /// Administrators
        /// </summary>
        public List<Account>? Administrators { get; set; }

        public Boolean GuestsAccepted { get; set; }

        public Boolean InstantMessageAutoAccept { get; set; }

        public Boolean AckMessageAutoAccept { get; set; }

        public Boolean ApplicationMessageAutoAccept { get; set; }

        public Boolean BubbleInvitationAutoAccept { get; set; }

        public Boolean UserInvitationAutoAccept { get; set; }

        public Account? Bot { get; set; }

        public BotConfiguration()
        {
            Administrators = null;
            GuestsAccepted = false;

            InstantMessageAutoAccept = false;
            AckMessageAutoAccept = false;
            ApplicationMessageAutoAccept = false;

            BubbleInvitationAutoAccept = true;
            UserInvitationAutoAccept = true;
            Bot = null;
        }

        public static Boolean FromJsonNode(JSONNode jsonNode, out BotConfiguration botConfiguration)
        {
            botConfiguration = new();
            if (jsonNode is not null)
            {
                var jsonNodeAdministrators = jsonNode["administrators"];
                if (jsonNodeAdministrators?.IsObject == true)
                {
                    var jsonNodeRainbowAccounts = jsonNodeAdministrators["rainbowAccounts"];
                    if (jsonNodeRainbowAccounts?.IsArray == true)
                    {
                        botConfiguration.Administrators = [];

                        foreach (var jsonNodeAccount in jsonNodeRainbowAccounts.Values)
                        {
                            if (Account.FromJsonNode(jsonNodeAccount, out Account account))
                                botConfiguration.Administrators.Add(account);
                        }
                    }

                    if(jsonNodeAdministrators.HasKey("guestsAccepted"))
                        botConfiguration.GuestsAccepted = jsonNodeAdministrators["guestsAccepted"];
                }

                if (jsonNode.HasKey("instantMessageAutoAccept"))
                    botConfiguration.InstantMessageAutoAccept = jsonNode["instantMessageAutoAccept"];

                if (jsonNode.HasKey("ackMessageAutoAccept"))
                    botConfiguration.AckMessageAutoAccept = jsonNode["ackMessageAutoAccept"];

                if (jsonNode.HasKey("applicationMessageAutoAccept"))
                    botConfiguration.ApplicationMessageAutoAccept = jsonNode["applicationMessageAutoAccept"];

                if (jsonNode.HasKey("userInvitationAutoAccept"))
                    botConfiguration.UserInvitationAutoAccept = jsonNode["userInvitationAutoAccept"];

                if (jsonNode.HasKey("bubbleInvitationAutoAccept"))
                    botConfiguration.BubbleInvitationAutoAccept = jsonNode["bubbleInvitationAutoAccept"];

                if(jsonNode.HasKey("bot"))
                {
                    if (Account.FromJsonNode(jsonNode["bot"], out Account account))
                        botConfiguration.Bot = account;
                }

                // At least one administrator must be set or guests accepted
                //if ( (botConfiguration.GuestsAccepted == true) || (botConfiguration.Administrators?.Count > 0))
                //        return true;
                return true;
            }
            return false;
        }
    }
}
