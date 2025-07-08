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
            McqAccounts = new();
        }

        public static bool FromJsonNode(JSONNode jsonNode, out BotConfigurationExtended botConfigurationExtended)
        {
            botConfigurationExtended = new();
            if (BotLibrary.Model.BotConfiguration.FromJsonNode(jsonNode, out BotLibrary.Model.BotConfiguration botConfiguration))
            {
                botConfigurationExtended.Administrators = botConfiguration.Administrators;
                botConfigurationExtended.GuestsAccepted = botConfiguration.GuestsAccepted;

                botConfigurationExtended.BubbleInvitationAutoAccept = botConfiguration.BubbleInvitationAutoAccept;
                botConfigurationExtended.UserInvitationAutoAccept = botConfiguration.UserInvitationAutoAccept;

                // Parse "streams"
                var jsonNodeStreams = jsonNode["mcqAccounts"];
                if (jsonNodeStreams?.IsArray == true)
                {
                    botConfigurationExtended.McqAccounts = new();
                    foreach (var jsonNodeStream in jsonNodeStreams.Values)
                    {
                        if (Account.FromJsonNode(jsonNodeStream, out Account account))
                            botConfigurationExtended.McqAccounts.Add(account);
                    }
                }

                return true;
            }
            return false;
        }
    }
}
