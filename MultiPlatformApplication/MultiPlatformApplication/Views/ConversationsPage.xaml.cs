using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Rainbow.Common;
using MultiPlatformApplication.Models;
using MultiPlatformApplication.ViewModels;

namespace MultiPlatformApplication
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConversationsPage : ContentPage
    {
        private App XamarinApplication;

        private ConversationsViewModel vm;

        public ConversationsPage()
        {
            InitializeComponent();

            // Create / Init ViewModel
            vm = new ConversationsViewModel();
            vm.DynamicList.ListView = ConversationsListView; // Need to know the collection view

            BindingContext = vm;

            // Get Xamarin Application
            XamarinApplication = (App)Xamarin.Forms.Application.Current;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            XamarinApplication.CurrentConversationId = null;

            vm.Initialize();

            
        }


        protected override void OnDisappearing()
        {
            base.OnAppearing();
        }

    }
}