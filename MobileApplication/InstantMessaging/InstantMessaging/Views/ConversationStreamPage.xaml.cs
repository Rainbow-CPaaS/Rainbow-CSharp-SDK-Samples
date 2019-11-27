using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace InstantMessaging
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConversationStreamPage : ContentPage
    {
        private ConversationStreamViewModel vm;

        private String peerId;

        private Boolean needToScrollAtBottom = false;

        public ConversationStreamPage(String peerId)
        {
            this.peerId = peerId;
            
            InitializeComponent();

            vm = new ConversationStreamViewModel(peerId);
            BindingContext = vm;

            this.Appearing += ConversationStreamPage_Appearing;

            
            MessagesListView.ItemAppearing += MessagesListView_ItemAppearing;

        }

        #region EVENTS FIRED ON THIS CONTENT PAGE

        private void MessagesListView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            base.OnAppearing();

            if (needToScrollAtBottom)
            {
                if (vm.MessagesList.Count > 0)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            MessagesListView.ScrollTo(vm.MessagesList[vm.MessagesList.Count - 1], ScrollToPosition.MakeVisible, false);
                            needToScrollAtBottom = false;
                        }
                        catch
                        {
                        }

                    });
                }
            }
        }

        private void ConversationStreamPage_Appearing(object sender, EventArgs e)
        {
            needToScrollAtBottom = true;
        }

#endregion EVENTS FIRED ON THIS CONTENT PAGE

#region EVENTS FIRED BY XAML

        void MessagesListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            MessagesListView.SelectedItem = null;
        }

        void MessagesListView_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            MessagesListView.SelectedItem = null;
        }

#endregion EVENTS FIRED BY XAML
    }
}