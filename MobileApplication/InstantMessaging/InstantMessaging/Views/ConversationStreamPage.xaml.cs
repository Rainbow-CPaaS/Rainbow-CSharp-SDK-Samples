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

        private String conversationId;
        private int indexToSee = 0;
        private bool scrollToTheEnd = false;

        public ConversationStreamPage(String conversationId)
        {
            this.conversationId = conversationId;
            
            InitializeComponent();

            vm = new ConversationStreamViewModel(conversationId);
            BindingContext = vm;

            // We want to scroll to the bottom of the Messages List when it's displayed
            this.Appearing += ConversationStreamPage_Appearing;

            MessagesListView.BindingContextChanged += MessagesListView_BindingContextChanged; ;
            

            MessagesListView.ItemAppearing += MessagesListView_ItemAppearing;

            MessagesListView.ItemSelected += MessagesListView_ItemSelected;
            MessagesListView.ItemTapped += MessagesListView_ItemTapped;

            BtnIMSend.Clicked += BtnIMSend_Clicked;
        }

        private void MessagesListView_BindingContextChanged(object sender, EventArgs e)
        {
            
        }

        private void MessagesListView_ChildAdded(object sender, ElementEventArgs e)
        {
            
        }

        #region EVENTS FIRED ON THIS CONTENT PAGE

        private void BtnIMSend_Clicked(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(EntryIM.Text))
                return;

            vm.SendMessage(EntryIM.Text);
            EntryIM.Text = "";
        }

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
            if (vm.NewMessageAdded())
            {
                if (vm.MessagesList.Count > 0)
                {
                    ScrollToMessageIndex(vm.MessagesList.Count - 1, ScrollToPosition.MakeVisible, () =>
                    {
                        vm.ScrollToNewMessageAddedDone();
                    });
                }
                else
                    vm.ScrollToNewMessageAddedDone();
            }
            // Do we need to scroll to the end of the list ?
            else if (scrollToTheEnd)
            {
                ScrollToMessageIndex(vm.MessagesList.Count - 1, ScrollToPosition.MakeVisible, () =>
                {
                    scrollToTheEnd = false;
                });
            }
            // Do we need to scroll due to the loading of older messages ?
            else if (vm.WaitingScrollingDueToLoadingOlderMessages())
            {
                indexToSee = vm.NbOlderMessagesFound();

                if (indexToSee >= vm.MessagesList.Count)
                    indexToSee = vm.MessagesList.Count - 1;

                ScrollToMessageIndex(indexToSee, ScrollToPosition.Start, () =>
                {
                    vm.ScrollingDueToLoadingOlderMessagesDone();
                });
            }
            // We want to load more message when we are on top of the list
            else if (e.ItemIndex == 0)
            {
                if (vm.CanLoadMoreMessages())
                {
                    // Load more messages
                    vm.LoadMoreMessages();
                }
            }
        }

        private void ConversationStreamPage_Appearing(object sender, EventArgs e)
        {
            // When the message list appears we want to scroll to the last message
            scrollToTheEnd = (vm.MessagesList.Count > 0);
        }

#endregion EVENTS FIRED ON THIS CONTENT PAGE

        private void ScrollToMessageIndex(int index, ScrollToPosition position, Action callback = null)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    MessagesListView.ScrollTo(vm.MessagesList[index], position, false);
                    callback?.Invoke();
                }
                catch
                {
                }
            });
        }

    }
}