using System;
using System.Collections.Generic;
using System.Text;

using InstantMessaging.Helpers;

using Rainbow;

using log4net;
using InstantMessaging.Model;

namespace InstantMessaging.ViewModel
{
    class ConversationStreamViewModel
    {
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(ConversationStreamViewModel));

        public RangeObservableCollection<MessageViewModel> MessagesList { get; set; } // Need to be public - Used as Binding from XAML

        public ConversationLightViewModel Conversation { get; } // Need to be public - Used as Binding from XAML


        MessagesModel messagesModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public ConversationStreamViewModel()
        {
            messagesModel = new MessagesModel();
            MessagesList = messagesModel.MessagesList;

            Conversation = null;
        }

    }
}
