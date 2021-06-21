using MultiPlatformApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConversationsView : ContentView
    {
        private ConversationsViewModel vm;

        public ConversationsView()
        {
            InitializeComponent();

            // Create / Init ViewModel
            vm = new ConversationsViewModel();
            vm.DynamicList.ListView = ConversationsListView; // Need to know the collection view

            ConversationsListView.BindingContext = vm;
        }

        public void Initialize()
        {
            vm.Initialize();
        }

    }
}