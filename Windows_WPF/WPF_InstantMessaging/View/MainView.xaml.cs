﻿using System;
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
        TestViewModel testViewModel = null;

        public MainView()
        {
            InitializeComponent();

            // Define the Icon of the Window
            this.Icon = Helper.GetBitmapFrameFromResource("icon_32x32.png");

            conversationsViewModel = new ConversationsLightViewModel();
            favoritesViewModel = new FavoritesViewModel();

            //conversationsViewModel.ResetModelWithRbConversations(currentApplication.RbConversations.GetAllConversationsFromCache());

            UIConversationsList.DataContext = conversationsViewModel;
            UIFavoritesList.DataContext = favoritesViewModel;


            testViewModel = new TestViewModel();
            RightMainGrid.DataContext = testViewModel;
        }



    }
}