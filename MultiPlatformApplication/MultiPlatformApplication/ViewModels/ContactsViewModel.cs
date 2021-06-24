using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Xaml;

using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;

using Rainbow;
using Rainbow.Model;

using NLog;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace MultiPlatformApplication.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ContactsViewModel : PopupViewModel
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(ContactsViewModel));

        private Boolean firstInitialization = true;

        List<Contact> contactsList;
        List<Contact> originalContactsList;

        String currentOrderBy;
        String currentFilter;

#region BINDINGS used by XAML

        public ObservableRangeCollection<ContactModel> Contacts { get; private set; } = new ObservableRangeCollection<ContactModel>();

        public BasicListModel OrderByOptionsList { get; private set; } = new BasicListModel();

        public BasicListModel FilterOptionsList { get; private set; } = new BasicListModel();

        public MenuItemListModel Menu { get; set; } = new MenuItemListModel();

#endregion BINDINGS used by XAML


        public ContactsViewModel()
        {
            //PopupModel.ButtonAcceptCommand = new RelayCommand<object>(new Action<object>(PopupButtonAcceptCommand));
            PopupModel.ButtonCancelCommand = new RelayCommand<object>(new Action<object>(PopupButtonCancelCommand));

            OrderByOptionsList.SelectionCommand = new RelayCommand<object>(new Action<object>(OrderBySelectionCommand));
            FilterOptionsList.SelectionCommand = new RelayCommand<object>(new Action<object>(FilterSelectionCommand));
        }

        public void SetListView(ListView listview)
        {
            listview.ItemSelected += Listview_ItemSelected;
        }

        public new void Initialize()
        {
            base.Initialize();

            if (firstInitialization)
            {
                firstInitialization = false;

                // Define menu
                Menu.Command = new RelayCommand<object>(new Action<object>(MenuCommand)); // Command raised when a menu item is selected
                
                Menu.SetDefaulMenuItemtSize(30, 30, 30);
                Menu.TextVisible = false;
                Menu.TextVisibleForSelectedItem = false;
                Menu.AddItem(new MenuItemModel() { Id = "orderby", ImageSourceId = "MainImage_sort_white" });
                Menu.AddItem(new MenuItemModel() { Id = "filter", ImageSourceId = "MainImage_filter-list_white" });

                // Get contacts from cache
                originalContactsList = Helper.SdkWrapper.GetAllContactsFromCache();

                // Remove contacts with "IsTerminated" equals to true and current contact
                String currentContactId = Helper.SdkWrapper.GetCurrentContactId();
                originalContactsList.RemoveAll(x => (x.IsTerminated) || (x.Id == currentContactId));

                // Update display Sorting/Grouping on First Name
                UpdateDisplay("firstname", "all");
            }
        }

        private void UpdateDisplay(String orderBy = "firstname", String filter = "all")
        {
            if ( (currentOrderBy == orderBy) && (currentFilter == filter) )
                return;

            currentOrderBy = orderBy;
            currentFilter = filter;

            if (originalContactsList?.Count < 1)
                return;

            Contacts.Clear();


            // First we filter the list
            switch (filter)
            {
                case "all":
                    contactsList = originalContactsList.ToList();
                    break;

                case "online":
                    contactsList = contactsList.OrderBy(x => x.LastName.ToUpper()).ToList();
                    break;

                case "offline":
                default:
                    contactsList = contactsList.OrderBy(x => x.FirstName.ToUpper()).ToList();
                    break;
            }


            // First we sort the contacts list
            switch (orderBy)
            {
                case "company":
                    contactsList = contactsList.OrderBy(x => x.CompanyName.ToUpper()).ToList();
                    break;

                case "lastname":
                    contactsList = contactsList.OrderBy(x => x.LastName.ToUpper()).ToList();
                    break;

                case "firstname":
                default:
                    contactsList = contactsList.OrderBy(x => x.FirstName.ToUpper()).ToList();
                    break;
            }


            // Now we group the result
            IEnumerable<IGrouping<String, Contact>> groupedContacts;

            switch (orderBy)
            {
                case "company":
                    groupedContacts = contactsList.GroupBy(x => (String)(string.IsNullOrEmpty(x.CompanyName) ? "" : x.CompanyName.ToUpper()));
                    break;

                case "lastname":
                    groupedContacts = contactsList.GroupBy(x => (String)(string.IsNullOrEmpty(x.LastName) ? "" : x.LastName[0].ToString().ToUpper()));
                    break;

                case "firstname":
                default:
                    groupedContacts = contactsList.GroupBy(x => (String)(string.IsNullOrEmpty(x.FirstName) ? "" : x.FirstName[0].ToString().ToUpper()));
                    break;
            }

            foreach (var item in groupedContacts)
            {
                Contacts.Add(new ContactModel("[GROUP]", item.Key));

                foreach (Contact contact in item.ToList())
                {
                    Contacts.Add(new ContactModel(contact.Id, contact.DisplayName, contact.CompanyName));
                }
            }
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


        private void Listview_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            //if (e.SelectedItemIndex != -1)
            //{
            //    if (e.SelectedItemIndex % 2 == 1)
            //    {
            //        UpdateLabelsForOrderBy();
            //        DisplayPopup("OrderBy", Helper.GetLabel("orderTitle"), Helper.GetLabel("cancel"));
            //    }
            //    else
            //    {
            //        UpdateLabelsForSort();
            //        DisplayPopup("Sort", Helper.GetLabel("filter"), Helper.GetLabel("cancel"));
            //    }
            //}

            // Reset selection
            ((ListView)sender).SelectedItem = null;
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

        void OrderBySelectionCommand(Object obj)
        {
            HidePopup();

            if (obj is BasicListItemModel)
            {
                BasicListItemModel item = obj as BasicListItemModel;
                UpdateDisplay(item.Id, currentFilter);
            }
        }

        void FilterSelectionCommand(Object obj)
        {
            // TODO
            HidePopup();

            if (obj is BasicListItemModel)
            {
                BasicListItemModel item = obj as BasicListItemModel;
                UpdateDisplay(currentOrderBy, item.Id);
            }
        }


#endregion MANAGE COMMANDS

    }
}
