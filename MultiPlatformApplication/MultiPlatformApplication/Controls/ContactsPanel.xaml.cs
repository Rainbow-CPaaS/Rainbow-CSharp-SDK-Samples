﻿using MultiPlatformApplication.Effects;
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

            vm = new ContactsViewModel();

            BindingContext = vm;
        }

        public void Initialize()
        {
            vm.Initialize();
        }

        private void ContactsListView_Scrolled(object sender, ScrolledEventArgs e)
        {
            Popup.HideCurrentContextMenu();
        }
    }
}