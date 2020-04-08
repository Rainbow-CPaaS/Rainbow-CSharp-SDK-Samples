using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

using Rainbow;

using InstantMessaging.Helpers;
using InstantMessaging.Model;
using log4net;

namespace InstantMessaging.ViewModel
{
    public class ConversationsLightViewModel : ObservableObject
    {
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(ConversationsLightViewModel));

        public SortableObservableCollection<ConversationLightViewModel> ConversationsLightList { get; set; } // Need to be public - Used as Binding from XAML

        ConversationsLightModel model;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ConversationsLightViewModel()
        {
            model = new ConversationsLightModel();
            ConversationsLightList = model.ConversationsLightList;
        }

    }
}
