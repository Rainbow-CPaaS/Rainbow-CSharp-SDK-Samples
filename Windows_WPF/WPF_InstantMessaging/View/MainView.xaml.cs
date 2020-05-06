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
using InstantMessaging.ViewModel;

using Rainbow.Events;

namespace InstantMessaging.View
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        App currentApplication = (App)System.Windows.Application.Current;

        ConversationsLightViewModel conversationsViewModel = null;
        FavoritesViewModel favoritesViewModel = null;
        ConversationStreamViewModel conversationStreamViewModel = null;

        /// <summary>
        /// Event raised when a Conversation is selected form Conversations List
        /// </summary>
        public event EventHandler<IdEventArgs> ConversationSelectedFromConversationsList;

        /// <summary>
        /// Event raised when a Favorite is selected form Favorites List
        /// </summary>
        public event EventHandler<IdEventArgs> FavoriteSelectedFromFavoritesList;

        public MainView()
        {
            InitializeComponent();

            // Define the Icon of the Window
            this.Icon = Helper.GetBitmapFrameFromResource("icon_32x32.png");

            this.Loaded += MainView_Loaded;

        }

        private void MainView_Loaded(object sender, RoutedEventArgs e)
        {
            // Must be created first since other ViewModel could use this ViewModel
            conversationStreamViewModel = new ConversationStreamViewModel();
            conversationStreamViewModel.SetScrollViewer(UIConversationStreamScrollViewer);
            UIConversationStream.DataContext = conversationStreamViewModel;

            conversationsViewModel = new ConversationsLightViewModel();
            UIConversationsList.DataContext = conversationsViewModel;

            favoritesViewModel = new FavoritesViewModel();
            UIFavoritesList.DataContext = favoritesViewModel;
        }

        public void SetConversationIdSelectionFromConversationsList(String id)
        {
            ConversationSelectedFromConversationsList?.Invoke(this, new IdEventArgs(id));
        }

        public void SetFavoriteIdSelectionFromFavoritesList(String id)
        {
            FavoriteSelectedFromFavoritesList?.Invoke(this, new IdEventArgs(id));
        }
    }
}
