using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

using InstantMessaging.Helpers;
using InstantMessaging.Model;

using Rainbow;

using log4net;

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

        public void SetScrollViewer(ScrollViewer sv)
        {
            messagesModel.SetScrollViewer(sv);
        }

    }
}
