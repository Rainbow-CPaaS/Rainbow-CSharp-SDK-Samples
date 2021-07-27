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

        private ConversationsPanel conversationsPanel;
        private ContactsPanel contactsPanel;

        ContentPage contenPage; // Ref. to Content Page: necessary to find the stacklayout where content is switched to dispaly info according menu selection
        Grid gridMainContent; // Grid is used (as content) to dispaly info according menu selection

        View currentView = null; // To know which current view is displayed in the grid row=0 col=0

#region BINDINGS used by XAML

        public MenuItemListModel Menu { get; set; } = new MenuItemListModel();

#endregion


#region PUBLIC METHODS

        public MainPageViewModel(ContentPage contenPage)
        {
            this.contenPage = contenPage;
        }

        public void Initialize()
        {
            if(firstInitialization)
            {
                firstInitialization = false;

                gridMainContent= (Grid)contenPage.FindByName("GridMainContent");

                // Define menu
                Menu.Command = new RelayCommand<object>(new Action<object>(MenuCommand)); // Command raised when a menu item is selected

                Menu.SetDefaulMenuItemtSize(30, 50, 100);
                Menu.TextVisible = false;
                Menu.TextVisibleForSelectedItem = true;
                Menu.AddItem(new MenuItemModel() { Id = "conversations", Label = Helper.GetLabel("conversationsItem"), ImageSourceId = "Font_CommentAlt|#FFFFFF" });
                Menu.AddItem(new MenuItemModel() { Id = "channels", Label = Helper.GetLabel("channels"), ImageSourceId = "Font_Newspaper|#FFFFFF" });
                Menu.AddItem(new MenuItemModel() { Id = "bubbles", Label = Helper.GetLabel("bubbles"), ImageSourceId = "Font_Bubble|#FFFFFF" });
                Menu.AddItem(new MenuItemModel() { Id = "contacts", Label = Helper.GetLabel("contacts"), ImageSourceId = "Font_Users|#FFFFFF" });
                Menu.AddItem(new MenuItemModel() { Id = "calls", Label = Helper.GetLabel("tab-calllogs-title"), ImageSourceId = "Font_Clock|#FFFFFF" });

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
                    if (conversationsPanel == null)
                        conversationsPanel = new ConversationsPanel();
                    
                    conversationsPanel.Initialize();
                    view = conversationsPanel;
                    break;

                case "contacts":
                    if (contactsPanel == null)
                        contactsPanel = new ContactsPanel();

                    contactsPanel.Initialize();
                    view = contactsPanel;
                    break;

                default:
                    view = new Label() { Text = id + " - TO DO", HorizontalOptions = new LayoutOptions(LayoutAlignment.Center, false), VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false), BackgroundColor = Color.White };
                    break;
            }

            if(gridMainContent != null)
            {
                if (currentView != null)
                    gridMainContent.Children.Remove(currentView);

                currentView = view;
                gridMainContent.Children.Add(currentView, 0, 0);
            }
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
