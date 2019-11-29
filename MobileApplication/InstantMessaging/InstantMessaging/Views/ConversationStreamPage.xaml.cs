using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Rainbow;

using log4net;

namespace InstantMessaging
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConversationStreamPage : ContentPage
    {
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(ConversationStreamPage));

        private ConversationStreamViewModel vm;

        private String peerId;
        private int indexToSee = 0;

        public ConversationStreamPage(String peerId)
        {
            this.peerId = peerId;
            
            InitializeComponent();

            vm = new ConversationStreamViewModel(peerId);
            BindingContext = vm;

            // We want to scroll to the bottom of the Messages List when it's displayed
            this.Appearing += ConversationStreamPage_Appearing;
         
            MessagesListView.ItemAppearing += MessagesListView_ItemAppearing;

            MessagesListView.ItemSelected += MessagesListView_ItemSelected;
            MessagesListView.ItemTapped += MessagesListView_ItemTapped;
        }

#region EVENTS FIRED ON THIS CONTENT PAGE

        private void MessagesListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            MessagesListView.SelectedItem = null;
        }

        private void MessagesListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            MessagesListView.SelectedItem = null;
        }

        private void MessagesListView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            base.OnAppearing();

            if (vm.WaitingScrollingDueToLoadingOlderMessages())
            {
                // We scroll only if there is enough elements ...
                if (vm.MessagesList.Count > 0)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            indexToSee = vm.NbOlderMessagesFound();

                            if (indexToSee >= vm.MessagesList.Count)
                                indexToSee = vm.MessagesList.Count - 1;

                            MessagesListView.ScrollTo(vm.MessagesList[indexToSee], ScrollToPosition.Start, false);

                            // Enable again the MessageListView
                            MessagesListView.IsEnabled = true;

                            vm.ScrollingDueToLoadingOlderMessagesDone();
                        }
                        catch
                        {
                        }
                    });
                }
            }
            else
            {
                // We want to load more message when we are on top of the list
                if (e.ItemIndex == 0)
                {
                    if (vm.CanLoadMoreMessages())
                    {
                        // We lock the MessageListView while we are loading older messages
                        MessagesListView.IsEnabled = false;

                        // Load more messages
                        vm.LoadMoreMessages();
                    }
                }
            }
        }

        private void ConversationStreamPage_Appearing(object sender, EventArgs e)
        {
            // Nothing to do (for the moment)
        }

#endregion EVENTS FIRED ON THIS CONTENT PAGE


    }
}