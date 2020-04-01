using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using InstantMessaging.Helpers;

namespace InstantMessaging.View
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        App currentApplication = (App)System.Windows.Application.Current;

        ConversationsViewModel conversationsViewModel = null;

        public MainView()
        {
            InitializeComponent();

            // Define the Icon of the Window
            this.Icon = Helper.GetBitmapFrameFromResource("icon_32x32.png");

            conversationsViewModel = new ConversationsViewModel();
            LoadFakeConversations();

            //conversationsViewModel.ResetModelWithRbConversations(currentApplication.RbConversations.GetAllConversationsFromCache());

            UIConversationsList.DataContext = conversationsViewModel;

        }

        private void LoadFakeConversations()
        {
            List<ConversationLight> conversationList = new List<ConversationLight>();

            ConversationLight conversationLight = null;

            // SET NEW FAKE Conversation Light
            conversationLight = new ConversationLight();
            conversationLight.Id = "convID1";
            conversationLight.Jid = "test1@test.fr";
            conversationLight.PeerId = "peerId1";
            conversationLight.Type = "user";
            conversationLight.Name = "User Name1";
            conversationLight.Topic = "";
            conversationLight.PresenceSource = "presence_online.png";
            conversationLight.NbMsgUnread = 1;
            conversationLight.LastMessage = "Last message";
            conversationLight.LastMessageDateTime = DateTime.UtcNow;
            conversationLight.MessageTimeDisplay = "18:39";
            conversationLight.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            conversationList.Add(conversationLight);

            // SET NEW FAKE Conversation Light
            conversationLight = new ConversationLight();
            conversationLight.Id = "convID2";
            conversationLight.Jid = "test2@test.fr";
            conversationLight.PeerId = "peerId2";
            conversationLight.Type = "user";
            conversationLight.Name = "User Name2 avec tres tres tres tres long";
            conversationLight.Topic = "";
            conversationLight.PresenceSource = "presence_busy.png";
            conversationLight.NbMsgUnread = 15;
            conversationLight.LastMessage = "Last message very veryveryveryveryvery long";
            conversationLight.LastMessageDateTime = DateTime.UtcNow;
            conversationLight.MessageTimeDisplay = "17:39";
            conversationLight.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            conversationList.Add(conversationLight);

            // SET NEW FAKE Conversation Light
            conversationLight = new ConversationLight();
            conversationLight.Id = "convID3";
            conversationLight.Jid = "test3@test.fr";
            conversationLight.PeerId = "peerId3";
            conversationLight.Type = "group";
            conversationLight.Name = "Group Name3";
            conversationLight.Topic = "";
            conversationLight.PresenceSource = "";
            conversationLight.NbMsgUnread = 115;
            conversationLight.LastMessage = "Last message";
            conversationLight.LastMessageDateTime = DateTime.UtcNow;
            conversationLight.MessageTimeDisplay = "15:09";
            conversationLight.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            conversationList.Add(conversationLight);

            // SET NEW FAKE Conversation Light
            conversationLight = new ConversationLight();
            conversationLight.Id = "convID4";
            conversationLight.Jid = "test4@test.fr";
            conversationLight.PeerId = "peerId4";
            conversationLight.Type = "group";
            conversationLight.Name = "Group Name4 tres tres tres long";
            conversationLight.Topic = "";
            conversationLight.PresenceSource = "";
            conversationLight.NbMsgUnread = 115;
            conversationLight.LastMessage = "Last message";
            conversationLight.LastMessageDateTime = DateTime.UtcNow;
            conversationLight.MessageTimeDisplay = "30 Mars";
            conversationLight.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            conversationList.Add(conversationLight);

            // SET NEW FAKE Conversation Light
            conversationLight = new ConversationLight();
            conversationLight.Id = "convID5";
            conversationLight.Jid = "test6@test.fr";
            conversationLight.PeerId = "peerId6";
            conversationLight.Type = "group";
            conversationLight.Name = "Group Name6";
            conversationLight.Topic = "";
            conversationLight.PresenceSource = "";
            conversationLight.NbMsgUnread = 115;
            conversationLight.LastMessage = "";
            conversationLight.LastMessageDateTime = DateTime.UtcNow;
            conversationLight.MessageTimeDisplay = "29 Mars";
            conversationLight.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            conversationList.Add(conversationLight);

            // SET NEW FAKE Conversation Light
            conversationLight = new ConversationLight();
            conversationLight.Id = "convID6";
            conversationLight.Jid = "test6@test.fr";
            conversationLight.PeerId = "peerId6";
            conversationLight.Type = "group";
            conversationLight.Name = "Group Name6";
            conversationLight.Topic = "";
            conversationLight.PresenceSource = "";
            conversationLight.NbMsgUnread = 115;
            conversationLight.LastMessage = "Last message";
            conversationLight.LastMessageDateTime = DateTime.UtcNow;
            conversationLight.MessageTimeDisplay = "31/03";
            conversationLight.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            conversationList.Add(conversationLight);


            // SET NEW FAKE Conversation Light
            conversationLight = new ConversationLight();
            conversationLight.Id = "convID7";
            conversationLight.Jid = "test7@test.fr";
            conversationLight.PeerId = "peerId7";
            conversationLight.Type = "group";
            conversationLight.Name = "Group Name7 tres tres tres long";
            conversationLight.Topic = "";
            conversationLight.PresenceSource = "";
            conversationLight.NbMsgUnread = 115;
            conversationLight.LastMessage = "Last message";
            conversationLight.LastMessageDateTime = DateTime.UtcNow;
            conversationLight.MessageTimeDisplay = "31/03/19";
            conversationLight.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            conversationList.Add(conversationLight);

            conversationsViewModel.Conversations.AddRange(conversationList);
        }

    }
}
