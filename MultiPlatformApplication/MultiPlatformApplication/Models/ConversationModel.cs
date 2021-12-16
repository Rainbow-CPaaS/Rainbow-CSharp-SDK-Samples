using System;
using System.IO;
using System.Collections.ObjectModel;

using Xamarin.Forms;

using Rainbow;

using Microsoft.Extensions.Logging;
using MultiPlatformApplication.Helpers;
using System.Windows.Input;

namespace MultiPlatformApplication.Models
{
    public class ConversationModel : ObservableObject
    {
        private static readonly ILogger log = Rainbow.LogFactory.CreateLogger<ConversationModel>();

        String id;
        PeerModel peer;

        String topic;
        String lastMessage;
        Int64 nbMsgUnread;
        DateTime lastMessageDateTime;
        String messageTimeDisplay;

        ICommand selectionCommand;

        public string Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        public PeerModel Peer
        {
            get { return peer; }
            set { SetProperty(ref peer, value); }
        }

        //public String BackgroundColor => SdkWrapper.GetColorFromDisplayName(Peer.DisplayName);

        public string Topic
        {
            get { return topic; }
            set { SetProperty(ref topic, value); }
        }

        public Int64 NbMsgUnread
        {
            get { return nbMsgUnread; }
            set { SetProperty(ref nbMsgUnread, value); }
        }

        public String LastMessage
        {
            get { return lastMessage; }
            set { SetProperty(ref lastMessage, value); }
        }

        public DateTime LastMessageDateTime
        {
            get { return lastMessageDateTime; }
            set {
                SetProperty(ref lastMessageDateTime, value);
                MessageTimeDisplay = Helpers.Helper.HumanizeDateTime(LastMessageDateTime);
            }
        }

        public String MessageTimeDisplay
        {
            get { return messageTimeDisplay; }
            set { SetProperty(ref messageTimeDisplay, value); }
        }

        public ICommand SelectionCommand
        {
            get { return selectionCommand; }
            set { SetProperty(ref selectionCommand, value); }
        }

        public ConversationModel()
        {
            Id = "";
            Peer = new PeerModel();

            Topic = "";

            NbMsgUnread = 0;

            LastMessage = "";
            LastMessageDateTime = DateTime.Now;
        }
            
    }
}

