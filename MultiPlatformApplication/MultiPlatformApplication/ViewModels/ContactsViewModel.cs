using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;

using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;

using Rainbow;
using Rainbow.Model;

using NLog;
using MultiPlatformApplication.Effects;
using MultiPlatformApplication.Controls;

namespace MultiPlatformApplication.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ContactsViewModel : ObservableObject
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(ContactsViewModel));

        private App XamarinApplication;

        private Boolean firstInitialization = true;


        Controls.ContextMenu OrderByContextMenu;
        Controls.ContextMenu FilterContextMenu;

        List<ContactModel> contactsList;
        List<ContactModel> originalContactsList;

        String currentOrderBy;
        String currentFilter;
        Boolean isBusy;

#region BINDINGS used by XAML

        public Boolean IsBusy {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        public ObservableRangeCollection<ContactModel> Contacts { get; private set; } = new ObservableRangeCollection<ContactModel>();

        public MenuItemListModel Menu { get; set; } = new MenuItemListModel();

#endregion BINDINGS used by XAML

        public ContactsViewModel()
        {
            // Get Xamarin Application
            XamarinApplication = (App)Xamarin.Forms.Application.Current;
        }

        public void Initialize()
        {
            if (firstInitialization)
            {
                firstInitialization = false;

                // Define menu
                Menu.Command = new RelayCommand<object>(new Action<object>(MenuCommand)); // Command raised when a menu item is selected

                Menu.SetDefaulMenuItemtSize(30, 50, 80);
                Menu.TextVisible = true;
                Menu.TextVisibleForSelectedItem = true;
                Menu.AddItem(new MenuItemModel() { Id = "orderby", Label = Helper.SdkWrapper.GetLabel("firstnameOrder"), ImageSourceId = "Font_SortAlphaDown|#FFFFFF" });
                Menu.AddItem(new MenuItemModel() { Id = "filter", Label = Helper.SdkWrapper.GetLabel("allFilter"), ImageSourceId = "Font_Filter|#FFFFFF" });

                CreateOrderByContexMenu();

                CreateFilterContexMenu();

                // Update display Sorting/Grouping on First Name
                UpdateContactsListDisplay("firstname", "all");
            }
            else
            {
                //Effects.ContextMenu.Hide(contextMenuOrderBy);
                //Effects.ContextMenu.Hide(contextMenuFilter);
            }
        }

        private void CreateOrderByContexMenu()
        {
            var contextMenuModel = new ContextMenuModel();
            contextMenuModel.Add(new ContextMenuItemModel() { Id = "firstname", Title = Helper.SdkWrapper.GetLabel("firstnameOrder"), IsSelected = true });
            contextMenuModel.Add(new ContextMenuItemModel() { Id = "lastname", Title = Helper.SdkWrapper.GetLabel("lastnameOrder") });
            contextMenuModel.Add(new ContextMenuItemModel() { Id = "company", Title = Helper.SdkWrapper.GetLabel("companyOrder") });

            OrderByContextMenu = new Controls.ContextMenu { AutomationId = "OrderByContextMenu", WidthRequest = 140 };
            OrderByContextMenu.BindingContext = contextMenuModel;

            OrderByContextMenu.Command = new RelayCommand<object>(new Action<object>(OrderByContextMenuCommand));

            Popup.Add(null, OrderByContextMenu, PopupType.ContextMenu);
        }

        private void CreateFilterContexMenu()
        {
            var contextMenuModel = new ContextMenuModel();
            contextMenuModel.Add(new ContextMenuItemModel() { Id = "all", Title = Helper.SdkWrapper.GetLabel("allFilter"), IsSelected = true });
            contextMenuModel.Add(new ContextMenuItemModel() { Id = "online", Title = Helper.SdkWrapper.GetLabel("onlineFilter") });
            contextMenuModel.Add(new ContextMenuItemModel() { Id = "offline", Title = Helper.SdkWrapper.GetLabel("offlineFilter") });

            FilterContextMenu = new Controls.ContextMenu { AutomationId = "FilterContextMenu", WidthRequest = 140 };
            FilterContextMenu.BindingContext = contextMenuModel;

            FilterContextMenu.Command = new RelayCommand<object>(new Action<object>(FilterContextMenuCommand));

            Popup.Add(null, FilterContextMenu, PopupType.ContextMenu);
        }

        private void UpdateContactsListDisplay(String orderBy = "firstname", String filter = "all")
        {
            if ((currentOrderBy == orderBy) && (currentFilter == filter))
                return;

            Boolean orderByChanged = false;
            Boolean filterChanged = false;

            if (currentOrderBy != orderBy)
            {
                currentOrderBy = orderBy;
                orderByChanged = true;
            }

            if (currentFilter != filter)
            {
                currentFilter = filter;
                filterChanged = true;
                orderByChanged = true;
            }

            if (originalContactsList?.Count < 1)
                return;

            Contacts.Clear();
            IsBusy = true;

            UpdateMenuDisplay();

            Task task = new Task(() =>
            {
                // Wait a little to let UI update with Activity indicator
                Thread.Sleep(200);

                // Create original contactsList if necessary
                //if ( (originalContactsList == null) || (originalContactsList.Count == 0) )
                {
                    // Get contacts from cache
                    List<Contact> rbContacts = Helper.SdkWrapper.GetAllContactsFromCache();

                    String currentContactId = Helper.SdkWrapper.GetCurrentContactId();

                    originalContactsList = new List<ContactModel>();
                    foreach (Contact contact in rbContacts)
                    {
                        if( ! ( (contact.IsTerminated) || (contact.Id == currentContactId) ) )
                            originalContactsList.Add(new ContactModel(contact));
                    }
                }


                // First we filter the list
                if (filterChanged)
                {
                    switch (filter)
                    {
                        case "online":
                            contactsList = originalContactsList.FindAll(contact => (contact.Presence == "online") || (contact.Presence == "away"));
                            break;

                        case "offline":
                            contactsList = originalContactsList.FindAll(contact => (contact.Presence == "offline") || (contact.Presence == null));
                            break;

                        case "all":
                        default:
                            contactsList = originalContactsList.ToList();
                            break;

                    }
                }

                // We sort the contacts list
                if (orderByChanged)
                {
                    switch (orderBy)
                    {
                        case "company":
                            contactsList = contactsList.OrderBy(x => x.CompanyForSort).ToList();
                            break;

                        case "lastname":
                            contactsList = contactsList.OrderBy(x => x.LastNameForSort).ToList();
                            break;

                        case "firstname":
                        default:
                            contactsList = contactsList.OrderBy(x => x.FirstNameForSort).ToList();
                            break;
                    }
                }

                // Now we group the result
                IEnumerable<IGrouping<String, ContactModel>> groupedContacts;

                switch (orderBy)
                {
                    case "company":
                        groupedContacts = contactsList.GroupBy(x => (String)(string.IsNullOrEmpty(x.CompanyForSort) ? "" : x.CompanyForSort.ToUpper()));
                        break;

                    case "lastname":
                        groupedContacts = contactsList.GroupBy(x => (String)(string.IsNullOrEmpty(x.LastNameForSort) ? "" : x.LastNameForSort[0].ToString()));
                        break;

                    case "firstname":
                    default:
                        groupedContacts = contactsList.GroupBy(x => (String)(string.IsNullOrEmpty(x.FirstNameForSort) ? "" : x.FirstNameForSort[0].ToString()));
                        break;
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    foreach (var item in groupedContacts)
                    {
                        Contacts.Add(new ContactModel(item.Key));
                        Contacts.AddRange(item.ToList(), System.Collections.Specialized.NotifyCollectionChangedAction.Add);
                    }

                    IsBusy = false;
                });

            });

            
            task.Start();
        }

        private void UpdateMenuDisplay()
        {
            String labelMenuFilter;
            String labelMenuSort;

            switch (currentFilter)
            {
                case "online":
                    labelMenuFilter = Helper.SdkWrapper.GetLabel("onlineFilter");
                    break;

                case "offline":
                    labelMenuFilter = Helper.SdkWrapper.GetLabel("offlineFilter");
                    break;

                case "all":
                default:
                    labelMenuFilter = Helper.SdkWrapper.GetLabel("allFilter");
                    break;

            }

            // First we sort the contacts list
            switch (currentOrderBy)
            {
                case "company":
                    labelMenuSort = Helper.SdkWrapper.GetLabel("companyOrder");
                    break;

                case "lastname":
                    labelMenuSort = Helper.SdkWrapper.GetLabel("lastnameOrder");
                    break;

                case "firstname":
                default:
                    labelMenuSort = Helper.SdkWrapper.GetLabel("firstnameOrder");
                    break;
            }

            // Update labels of menu
            Menu.Items[0].Label = labelMenuSort;
            Menu.Items[1].Label = labelMenuFilter;
        }

        private void SetSelectedItemOnContextMenu(ContextMenuModel contextMenuModel, String selectedId)
        {
            if (String.IsNullOrEmpty(selectedId))
                return;

            if (contextMenuModel?.Items?.Count > 0)
            {
                for (int i = 0; i < contextMenuModel.Items.Count; i++)
                    contextMenuModel.Items[i].IsSelected = (contextMenuModel.Items[i].Id == selectedId);
            }
        }


#region MANAGE COMMANDS

        void MenuCommand(object obj)
        {
            String selectedId = null;
            Rect rect = new Rect();

            if (obj is CustomButton customButton)
            {
                rect = Popup.GetRectOfView(customButton);
                if (customButton.BindingContext is MenuItemModel menuItemModel)
                    selectedId = menuItemModel.Id;
            }

            switch (selectedId)
            {
                case "orderby":
                    Popup.Show("OrderByContextMenu", rect);
                    break;

                case "filter":
                    Popup.Show("FilterContextMenu", rect);
                    break;
            }

            Menu.SetItemSelected(-1);
        }

        public async void SelectedContactCommand(Object obj)
        {
            if (obj is ContactModel)
            {
                ContactModel contact = obj as ContactModel;
                if (contact != null)
                {
                    if (String.IsNullOrEmpty(contact.GroupName))
                    {
                        Conversation conversation = Helper.SdkWrapper.GetOrCreateConversationFromUserId(contact.Peer.Id);
                        if (conversation != null)
                        {
                            XamarinApplication.CurrentConversationId = conversation.Id;
                            await XamarinApplication.NavigationService.NavigateModalAsync("ConversationStreamPage", conversation.Id);
                        }
                        else
                        {
                            // TODO
                        }

                    }
                }
            }
        }

        private void OrderByContextMenuCommand(object obj)
        {
            if(obj is String id)
                UpdateContactsListDisplay(id, currentFilter);
        }

        public void FilterContextMenuCommand(Object obj)
        {
            if (obj is String id)
                UpdateContactsListDisplay(currentOrderBy, id);
        }


#endregion MANAGE COMMANDS

    }
}
