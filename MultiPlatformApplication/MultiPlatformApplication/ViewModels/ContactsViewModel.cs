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

namespace MultiPlatformApplication.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ContactsViewModel: Contact
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(ContactsViewModel));

        private Boolean firstInitialization = true;

        List<Contact> contactsList;
        ListView listView;

#region BINDINGS used by XAML

        public ObservableRangeCollection<Contact> Contacts { get; private set; } = new ObservableRangeCollection<Contact>();

        public List<ObjectsGroupingModel<String, Contact>>ContactsGrouped { get; private set; } = new List<ObjectsGroupingModel<String, Contact>>();

#endregion BINDINGS used by XAML


        public ContactsViewModel()
        {
        }

        public void Initialize()
        {
            if(firstInitialization)
            {
                firstInitialization = false;

                // Get contacts from cache
                contactsList = Helper.SdkWrapper.GetAllContactsFromCache();

                // Update display Sorting/Grouping on First Name
                UpdateDisplay("FirstName", true);
            }
        }

        public void UpdateDisplay(String columnToSort = "FirstName", Boolean ascending = true)
        {
            if (contactsList?.Count < 1)
                return;

            ContactsGrouped.Clear();

            // First we sort the contacts list
            switch (columnToSort)
            {
                case "Company":
                    contactsList = contactsList.OrderBy(x => x.CompanyName.ToUpper()).ToList();
                    break;

                case "LastName":
                    contactsList = contactsList.OrderBy(x => x.LastName.ToUpper()).ToList();
                    break;

                case "FirstName":
                default:
                    contactsList = contactsList.OrderBy(x => x.FirstName.ToUpper()).ToList();
                    break;
            }

            // We reverse the result if we don't want sort on ascending
            if (!ascending)
                contactsList.Reverse();

            // Now we group the result
            IEnumerable<IGrouping<String, Contact>> groupedContacts;
                
            switch (columnToSort)
            {
                case "Company":
                    groupedContacts = contactsList.GroupBy(x => (String)(string.IsNullOrEmpty(x.CompanyName) ? "" : x.CompanyName.ToUpper()));
                    break;

                case "LastName":
                    groupedContacts = contactsList.GroupBy(x => (String)(string.IsNullOrEmpty(x.LastName) ? "" : x.LastName[0].ToString().ToUpper()));
                    break;

                case "FirstName":
                default:
                    groupedContacts = contactsList.GroupBy(x => (String)(string.IsNullOrEmpty(x.FirstName) ? "" : x.FirstName[0].ToString().ToUpper()));
                    break;
            }

            foreach (var item in groupedContacts)
            {
                ContactsGrouped.Add(new ObjectsGroupingModel<string, Contact>(item.Key, item.ToList()));
            }
            
        }

    }
}
