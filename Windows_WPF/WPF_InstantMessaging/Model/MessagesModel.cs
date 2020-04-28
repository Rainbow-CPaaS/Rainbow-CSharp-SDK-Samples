using System;
using System.Collections.Generic;
using System.Text;

using InstantMessaging.Helpers;
using InstantMessaging.Pool;
using InstantMessaging.ViewModel;


using Rainbow;

using log4net;


namespace InstantMessaging.Model
{
    class MessagesModel
    {
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(LoginModel));
        App CurrentApplication = (App)System.Windows.Application.Current;

        private Bubbles RbBubbles = null;
        private Contacts RbContacts = null;
        private Conversations RbConversations = null;

        private readonly AvatarPool AvatarPool; // To manage avatars

        public RangeObservableCollection<MessageViewModel> MessagesList { get; set; } // Need to be public - Used as Binding from XAML

        public MessagesModel()
        {
            MessagesList = new RangeObservableCollection<MessageViewModel>();

            //if (CurrentApplication.USE_DUMMY_DATA)
            {
                LoadFakeMessages();
                return;
            }
        }

        public void LoadFakeMessages()
        {

            MessageViewModel message;

            message = new MessageViewModel();
            message.Id = "1";
            MessagesList.Add(message);


            message = new MessageViewModel();
            message.Id = "2";
            MessagesList.Add(message);

            message = new MessageViewModel();
            message.Id = "3";
            MessagesList.Add(message);

        }
    }
}


