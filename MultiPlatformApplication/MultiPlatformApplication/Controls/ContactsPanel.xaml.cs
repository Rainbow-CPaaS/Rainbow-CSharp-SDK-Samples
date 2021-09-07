using MultiPlatformApplication.Effects;
using MultiPlatformApplication.Events;
using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ContactsPanel : ContentView
    {
        private readonly ContactsViewModel vm;

        public ContactsPanel()
        {
            InitializeComponent();

            ContactsListView.ItemSelected += ContactsListView_ItemSelected;
            Popup.AutoHideForView(ContactsListView);

            vm = new ContactsViewModel();

            BindingContext = vm;
        }

        private void ContactsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItemIndex != -1)
            {
                Popup.HideCurrentContextMenu();

                vm?.SelectedContactCommand(e.SelectedItem);

                // Reset selection
                ((ListView)sender).SelectedItem = null;
            }
        }

        public void Initialize()
        {
            vm.Initialize();
        }
    }
}