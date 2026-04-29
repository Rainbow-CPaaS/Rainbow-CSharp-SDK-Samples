using BotLibrary.Model;
using Rainbow.Example.Common;
using Rainbow.SimpleJSON;
using System;
using System.Collections.Generic;

namespace BotAdaptiveCards.Model
{
    public class BotConfigurationExtended : BotLibrary.Model.BotConfiguration
    {
        public List<Account> McqAccounts { get; set; }


        public BotConfigurationExtended() : base()
        {
            McqAccounts = [];
        }

        public static bool FromJsonNode(JSONNode jsonNode, out BotConfigurationExtended botConfigurationExtended)
        {
            botConfigurationExtended = new();

            var botConfiguration = BotConfiguration.FromJsonNode(jsonNode);
            if (botConfiguration is null) return false;

            botConfigurationExtended.Administrators = botConfiguration.Administrators;
            botConfigurationExtended.GuestsAccepted = botConfiguration.GuestsAccepted;

            botConfigurationExtended.BubbleInvitationAutoAccept = botConfiguration.BubbleInvitationAutoAccept;
            botConfigurationExtended.UserInvitationAutoAccept = botConfiguration.UserInvitationAutoAccept;

            var jsonNodeStreams = jsonNode["mcqAccounts"];
            if (jsonNodeStreams?.IsArray == true)
            {
                botConfigurationExtended.McqAccounts = [];
                foreach (var jsonNodeStream in jsonNodeStreams.Values)
                {
                    var account = Account.FromJsonNode(jsonNodeStream);
                    if(account?.IsValid() == true)
                        botConfigurationExtended.McqAccounts.Add(account);
                }
            }

            return true;
        }
    }
}
