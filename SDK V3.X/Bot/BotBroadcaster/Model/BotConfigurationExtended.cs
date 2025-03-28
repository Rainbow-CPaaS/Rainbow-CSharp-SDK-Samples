using Rainbow.Example.Common;
using Rainbow.SimpleJSON;
using System;
using System.Collections.Generic;

namespace BotBroadcaster.Model
{
    public class BotConfigurationExtended: BotLibrary.Model.BotConfiguration
    {
        public Dictionary<String, Stream> Streams { get; set; }

        public Conference? Conference { get; set; }

        public BotConfigurationExtended() : base()
        {
            Streams = new();
            Conference = new();
        }

        public static Boolean FromJsonNode(JSONNode jsonNode, out BotConfigurationExtended botConfigurationExtended)
        {
            botConfigurationExtended = new();
            if (BotLibrary.Model.BotConfiguration.FromJsonNode(jsonNode, out BotLibrary.Model.BotConfiguration botConfiguration))
            {
                botConfigurationExtended.Administrators = botConfiguration.Administrators;
                botConfigurationExtended.GuestsAccepted = botConfiguration.GuestsAccepted;

                botConfigurationExtended.BubbleInvitationAutoAccept = botConfiguration.BubbleInvitationAutoAccept;
                botConfigurationExtended.UserInvitationAutoAccept = botConfiguration.UserInvitationAutoAccept;

                // Parse "streams"
                var jsonNodeStreams = jsonNode["streams"];
                if (jsonNodeStreams?.IsArray == true)
                {
                    botConfigurationExtended.Streams = new();
                    foreach (var jsonNodeStream in jsonNodeStreams.Values)
                    {
                        if (Stream.FromJsonNode(jsonNodeStream, out Stream stream))
                            botConfigurationExtended.Streams[stream.Id] = stream;
                    }
                }

                // Parse "conference"
                if (Conference.FromJsonNode(jsonNode["conference"], out Conference? conference))
                    botConfigurationExtended.Conference = conference;

                return true;
            }
            return false;
        }
    }
}
