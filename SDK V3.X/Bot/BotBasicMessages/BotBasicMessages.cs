using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace BotBasic
{
    public class BotBasicMessages: BotLibrary.BotBase
    {
#region Messages - AckMessage, ApplicationMessage, InstantMessage, InternalMessage
        public override async Task AckMessageReceivedAsync(Rainbow.Model.AckMessage ackMessage)
        {
            // Here we answer to all AckMessage using a default Result message
            await Application.GetInstantMessaging().AnswerToAckMessageAsync(ackMessage, Rainbow.Enums.MessageType.Result);
        }

        public override async Task ApplicationMessageReceivedAsync(Rainbow.Model.ApplicationMessage applicationMessage)
        {
            // Here we answer to all ApplicationMessage using a default message

            String senderDisplayName = await GetDisplayName(applicationMessage.FromJid, EntityType.User);

            // Create and send an ApplicationMessage as answer
            List<XmlElement> xmlElements = [];
            var el1 = new XmlDocument().CreateElement("elm1");
            el1.InnerText = Rainbow.Util.StringWithCDATA($"Hi, It's an auto-answer from 'BotBasic' SDK C# example. ApplicationMessage has been sent by [{senderDisplayName}].");
            xmlElements.Add(el1);
            await Application.GetInstantMessaging().AnswerToApplicationMessageAsync(applicationMessage, xmlElements);
        }

        public override async Task InstantMessageReceivedAsync(Rainbow.Model.Message message)
        {
            // Here we answer to InstantMessage using a default message

            String senderDisplayName = await GetDisplayName(message.FromContact?.Peer?.Jid, EntityType.User);

            // Create and send an answer
            String answer = $"Hi, It's an auto-answer from 'BotBasic' SDK C# example. InstantMessage received has been sent by {senderDisplayName}";
            if (message.ToBubble is not null)
            {
                answer += $" in Bubble {await GetDisplayName(message.ToBubble.Peer?.Jid, EntityType.Bubble)}";
            }

            await Application.GetInstantMessaging().AnswerToMessageAsync(message, answer);
        }
        #endregion Messages - AckMessage, ApplicationMessage, InstantMessage, InternalMessage

        private async Task<String> GetDisplayName(String? jid, String entityType)
        {
            Peer? peer = null;
            if (jid is not null)
            { 
                switch (entityType)
                {
                    case EntityType.User:
                        peer = await Application.GetContacts().GetContactByJidInCacheFirstAsync(jid);
                        break;

                    case EntityType.Bubble:
                        peer = await Application.GetBubbles().GetBubbleByJidInCacheFirstAsync(jid);
                        break;
                }
            }

            if (String.IsNullOrEmpty(peer?.DisplayName))
                return "an unknown contact";
            else
                return $"[{peer.DisplayName}]";
        }
    }
}
