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

namespace MultiPlatformApplication.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ContactsViewModel : PopupViewModel
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(ContactsViewModel));

        private App XamarinApplication;

        private Boolean firstInitialization = true;

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

        public BasicListModel OrderByOptionsList { get; private set; } = new BasicListModel();

        public BasicListModel FilterOptionsList { get; private set; } = new BasicListModel();

        public MenuItemListModel Menu { get; set; } = new MenuItemListModel();

#endregion BINDINGS used by XAML

        public ContactsViewModel()
        {
            // Get Xamarin Application
            XamarinApplication = (App)Xamarin.Forms.Application.Current;

            //PopupModel.ButtonAcceptCommand = new RelayCommand<object>(new Action<object>(PopupButtonAcceptCommand));
            PopupModel.ButtonCancelCommand = new RelayCommand<object>(new Action<object>(PopupButtonCancelCommand));
        }

        public new void Initialize()
        {
            base.Initialize();

            if (firstInitialization)
            {
                firstInitialization = false;

                // Define menu
                Menu.Command = new RelayCommand<object>(new Action<object>(MenuCommand)); // Command raised when a menu item is selected
                
                Menu.SetDefaulMenuItemtSize(30, 50, 80);
                Menu.TextVisible = true;
                Menu.TextVisibleForSelectedItem = true;
                Menu.AddItem(new MenuItemModel() { Id = "orderby", Label = Helper.GetLabel("firstnameOrder"),  ImageSourceId = "MainImage_sort_white" });
                Menu.AddItem(new MenuItemModel() { Id = "filter", Label = Helper.GetLabel("allFilter"), ImageSourceId = "MainImage_filter-list_white" });

                // Update display Sorting/Grouping on First Name
                UpdateContactsListDisplay("firstname", "all");
            }
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
                    labelMenuFilter = Helper.GetLabel("onlineFilter");
                    break;

                case "offline":
                    labelMenuFilter = Helper.GetLabel("offlineFilter");
                    break;

                case "all":
                default:
                    labelMenuFilter = Helper.GetLabel("allFilter");
                    break;

            }

            // First we sort the contacts list
            switch (currentOrderBy)
            {
                case "company":
                    labelMenuSort = Helper.GetLabel("companyOrder");
                    break;

                case "lastname":
                    labelMenuSort = Helper.GetLabel("lastnameOrder");
                    break;

                case "firstname":
                default:
                    labelMenuSort = Helper.GetLabel("firstnameOrder");
                    break;
            }

            // Update labels of menu
            Menu.Items[0].Label = labelMenuSort;
            Menu.Items[1].Label = labelMenuFilter;
        }

        private void UpdateLabelsForOrderBy()
        {
            OrderByOptionsList.Clear();

            OrderByOptionsList.Add(new BasicListItemModel() { Id = "firstname", Label = Helper.GetLabel("firstnameOrder") });
            OrderByOptionsList.Add(new BasicListItemModel() { Id = "lastname", Label = Helper.GetLabel("lastnameOrder") });
            OrderByOptionsList.Add(new BasicListItemModel() { Id = "company", Label = Helper.GetLabel("companyOrder") });
        }

        private void UpdateLabelsForFilter()
        {
            FilterOptionsList.Clear();

            FilterOptionsList.Add(new BasicListItemModel() { Id = "all", Label = Helper.GetLabel("allFilter") });
            FilterOptionsList.Add(new BasicListItemModel() { Id = "online", Label = Helper.GetLabel("onlineFilter") });
            FilterOptionsList.Add(new BasicListItemModel() { Id = "offline", Label = Helper.GetLabel("offlineFilter") });
        }

#region MANAGE COMMANDS

        void MenuCommand(object obj)
        {
            if(obj is String)
            {
                string id = obj as String;
                switch(id)
                {
                    case "orderby":
                        UpdateLabelsForOrderBy();
                        DisplayPopup("OrderBy", Helper.GetLabel("orderTitle"), Helper.GetLabel("cancel"));
                        break;

                    case "filter":
                        UpdateLabelsForFilter();
                        DisplayPopup("Filter", Helper.GetLabel("filter"), Helper.GetLabel("cancel"));
                        break;
                }
                
                
            }
            Menu.SetItemSelected(-1);

        }

        void PopupButtonCancelCommand(Object obj)
        {
            HidePopup();
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
                        if(conversation != null)
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

        public void SelectedOrderByCommand(Object obj)
        {
            HidePopup();

            if (obj is BasicListItemModel)
            {
                BasicListItemModel item = obj as BasicListItemModel;
                UpdateContactsListDisplay(item.Id, currentFilter);
            }
        }

        public void SelectedFilterCommand(Object obj)
        {
            HidePopup();

            // TODO
            if (obj is BasicListItemModel)
            {
                BasicListItemModel item = obj as BasicListItemModel;
                UpdateContactsListDisplay(currentOrderBy, item.Id);
            }
        }


#endregion MANAGE COMMANDS

    }
}
