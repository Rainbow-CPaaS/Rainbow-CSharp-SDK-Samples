﻿using MultiPlatformApplication.ViewModels;
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
    public partial class ContactsPanel : Frame
    {
        private readonly ContactsViewModel vm;

        public ContactsPanel()
        {
            InitializeComponent();

            ContactsListView.ItemSelected += ContactsListView_ItemSelected;
            OrderByListView.ItemSelected += OrderByListView_ItemSelected;
            FilterListView.ItemSelected += FilterListView_ItemSelected;

            vm = new ContactsViewModel();
            vm.SetRootView(this); // Need to know the Root Layout

            BindingContext = vm;
        }

        private void FilterListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            vm?.SelectedFilterCommand(e.SelectedItem);

            // Reset selection
            ((ListView)sender).SelectedItem = null;
        }

        private void OrderByListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            vm?.SelectedOrderByCommand(e.SelectedItem);

            // Reset selection
            ((ListView)sender).SelectedItem = null;
        }

        private void ContactsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            vm?.SelectedContactCommand(e.SelectedItem);

            // Reset selection
            ((ListView)sender).SelectedItem = null;
        }



        public void Initialize()
        {
            vm.Initialize();
        }
    }
}