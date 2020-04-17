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
        
        public MainView()
        {
            InitializeComponent();

            // Define the Icon of the Window
            this.Icon = Helper.GetBitmapFrameFromResource("icon_32x32.png");

            this.Loaded += MainView_Loaded;

        }

        private void MainView_Loaded(object sender, RoutedEventArgs e)
        {
            conversationsViewModel = new ConversationsLightViewModel();
            UIConversationsList.DataContext = conversationsViewModel;

            favoritesViewModel = new FavoritesViewModel();
            UIFavoritesList.DataContext = favoritesViewModel;
        }
    }
}
