using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using MultiPlatformApplication.Views;
using NLog;
using Rainbow;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class MainPageViewModel
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(MainPageViewModel));

        private Boolean firstInitialization = true;
        private String currentMenuIdSelected;

        private ConversationsView conversationsView;

        ContentView contentView; // We need to know the content view to display correct info according menu selection

#region BINDINGS used by XAML

        public MenuItemListModel Menu { get; set; } = new MenuItemListModel();

#endregion


#region PUBLIC METHODS

        public MainPageViewModel(ContentView contentView)
        {
            this.contentView = contentView;
        }

        public void Initialize()
        {
            if(firstInitialization)
            {
                firstInitialization = false;

                // Define menu
                Menu.Command = new RelayCommand<object>(new Action<object>(MenuCommand)); // Command raised when a menu item is selected
                Menu.SetDefaulMenuItemtSize(30, 50, 100);
                Menu.TextVisible = false;
                Menu.TextVisibleForSelectedItem = true;
                Menu.AddItem(new MenuItemModel() { Id = "Conversations", Label = Helper.GetLabel("conversationsItem"), ImageSourceId = "MainImage_chat_white", IsSelected = true });
                Menu.AddItem(new MenuItemModel() { Id = "Channels", Label = Helper.GetLabel("channels"), ImageSourceId = "MainImage_newsfeed_white" });
                Menu.AddItem(new MenuItemModel() { Id = "Bubbles", Label = Helper.GetLabel("bubbles"), ImageSourceId = "MainImage_bubble_white" });
                Menu.AddItem(new MenuItemModel() { Id = "Contacts", Label = Helper.GetLabel("contacts"), ImageSourceId = "MainImage_contacts_white" });
                Menu.AddItem(new MenuItemModel() { Id = "Calls", Label = Helper.GetLabel("tab-calllogs-title"), ImageSourceId = "MainImage_calllog_white" });


                // Display by default "Conversations"
                MenuCommand("Conversations");
            }
            else
            {
                // We need to inform the current content view that we in "Initialize" step

                switch (currentMenuIdSelected)
                {
                    case "Conversations":
                        if (conversationsView != null)
                            conversationsView.Initialize();
                        break;

                    default:
                        
                        break;
                }
            }
        }
#endregion PUBLIC METHODS



        private void MenuCommand(object obj)
        {
            if(obj is String)
            {
                String Id = (String)obj;
                if(Id != currentMenuIdSelected)
                {
                    currentMenuIdSelected = Id;
                    switch (Id)
                    {
                        case "Conversations":
                            if(conversationsView == null)
                                conversationsView = new ConversationsView();
                            contentView.Content = conversationsView;
                            break;

                        default:
                            contentView.Content = new Label() { Text = Id + " - TO DO", HorizontalOptions = new LayoutOptions(LayoutAlignment.Center, false), VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false) };
                            break;
                    }
                }
            }
        }

    }
}
