using MultiPlatformApplication.Controls;
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
        private ContactsPanel contactsPanel;

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
                Menu.AddItem(new MenuItemModel() { Id = "conversations", Label = Helper.GetLabel("conversationsItem"), ImageSourceId = "MainImage_chat_white" });
                Menu.AddItem(new MenuItemModel() { Id = "channels", Label = Helper.GetLabel("channels"), ImageSourceId = "MainImage_newsfeed_white" });
                Menu.AddItem(new MenuItemModel() { Id = "bubbles", Label = Helper.GetLabel("bubbles"), ImageSourceId = "MainImage_bubble_white" });
                Menu.AddItem(new MenuItemModel() { Id = "contacts", Label = Helper.GetLabel("contacts"), ImageSourceId = "MainImage_contacts_white" });
                Menu.AddItem(new MenuItemModel() { Id = "calls", Label = Helper.GetLabel("tab-calllogs-title"), ImageSourceId = "MainImage_calllog_white" });

                // Select "conversations" as selected item
                Menu.SetItemSelected("conversations");
            }
            else
            {
                // We need to inform the current content view that we are in "Initialize" step
                DisplayMenu(currentMenuIdSelected);
            }
        }
        #endregion PUBLIC METHODS


        private void DisplayMenu(String id)
        {

            View view;
            switch (id)
            {
                case "conversations":
                    if (conversationsView == null)
                        conversationsView = new ConversationsView();

                    conversationsView.Initialize();
                    view = conversationsView;
                    break;

                case "contacts":
                    if (contactsPanel == null)
                        contactsPanel = new ContactsPanel();

                    contactsPanel.Initialize();
                    view = contactsPanel;
                    break;

                default:
                    view = new Label() { Text = id + " - TO DO", HorizontalOptions = new LayoutOptions(LayoutAlignment.Center, false), VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false) };
                    break;
            }

            if (contentView.Content != view)
                contentView.Content = view;

        }

        private void MenuCommand(object obj)
        {
            if(obj is String)
            {
                String id = (String)obj;
                if(id != currentMenuIdSelected)
                {
                    currentMenuIdSelected = id;
                    DisplayMenu(id);
                }
            }
        }

    }
}
