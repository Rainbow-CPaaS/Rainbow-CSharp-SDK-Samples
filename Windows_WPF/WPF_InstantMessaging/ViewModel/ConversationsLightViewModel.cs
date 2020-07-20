using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;

using Rainbow;

using InstantMessaging.Helpers;
using InstantMessaging.Model;

using NLog;

namespace InstantMessaging.ViewModel
{
    public class ConversationsLightViewModel : ObservableObject
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(ConversationsLightViewModel));

        public RangeObservableCollection<ConversationLightViewModel> ConversationsLightList { get; set; } // Need to be public - Used as Binding from XAML

        /// Define all commands used in ListView
        /// Left Click on Conversation Item
        public ICommand ItemLeftClick { get; set;  }

        // Define the model
        ConversationsLightModel model;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ConversationsLightViewModel()
        {
            // Create model
            model = new ConversationsLightModel();
            ConversationsLightList = model.ConversationsLightList;

            // Set commands (from Model to ViewModel)
            ItemLeftClick = model.ItemLeftClick;
        }
    }
}
