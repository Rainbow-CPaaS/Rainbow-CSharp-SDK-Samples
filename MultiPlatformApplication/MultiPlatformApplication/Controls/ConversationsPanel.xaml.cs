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
    public partial class ConversationsPanel : ContentView
    {
        private ConversationsViewModel vm;

        public ConversationsPanel()
        {
            InitializeComponent();

            ConversationsListView.ItemSelected += ConversationsListView_ItemSelected;

            // Create / Init ViewModel
            vm = new ConversationsViewModel();
            vm.DynamicList.ListView = ConversationsListView; // Need to know the collection view

            ConversationsListView.BindingContext = vm;
        }

        private void ConversationsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            vm.SelectedConversationCommand(e.SelectedItem);

            //Reset selection
            ((ListView)sender).SelectedItem = null;
        }

        public void Initialize()
        {
            vm.Initialize();
        }

    }
}