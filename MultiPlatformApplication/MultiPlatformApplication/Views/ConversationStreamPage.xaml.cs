using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Rainbow;

using MultiPlatformApplication.Models;
using MultiPlatformApplication.ViewModels;

using NLog;


namespace MultiPlatformApplication.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConversationStreamPage : ContentPage
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(ConversationStreamPage));

        private ConversationStreamViewModel vm;
        private String conversationId;

        public ConversationStreamPage(String conversationId)
        {
            InitializeComponent();

            MessagesListView.ItemSelected += MessagesListView_ItemSelected;

            this.conversationId = conversationId;
            
            vm = new ConversationStreamViewModel();
            vm.DynamicList.ListView = MessagesListView; // Need to know the collection view

            BindingContext = vm;

            BtnIMSend.Clicked += BtnIMSend_Clicked;
        }

        private void MessagesListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // Reset selection
            ((ListView)sender).SelectedItem = null;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            vm.Initialize(conversationId);
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            if (Parent == null)
                DisposeBindingContext();
        }

        protected void DisposeBindingContext()
        {
            if (BindingContext is IDisposable disposableBindingContext)
            {
                disposableBindingContext.Dispose();
            }

            if (vm != null)
            {
                vm.Uninitialize();
                vm = null;
            }
            BindingContext = null;

        }

        ~ConversationStreamPage()
        {
            DisposeBindingContext();
        }



#region EVENTS FIRED ON THIS CONTENT PAGE

        private void EntryIm_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue != e.OldTextValue)
            {
                if (String.IsNullOrEmpty(e.NewTextValue))
                    vm.SetIsTyping(false);
                else
                    vm.SetIsTyping(e.NewTextValue.Length > 0);
            }
        }

        private void BtnIMSend_Clicked(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(EntryIM.Text))
                return;

            vm.SendMessage(EntryIM.Text);
            EntryIM.Text = "";
        }

#endregion EVENTS FIRED ON THIS CONTENT PAGE



    }
}