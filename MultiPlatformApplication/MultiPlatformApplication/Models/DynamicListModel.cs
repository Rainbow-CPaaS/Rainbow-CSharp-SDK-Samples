using System;
using System.Linq;
using System.Windows.Input;

using Xamarin.Forms;

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms.Shapes;
using MultiPlatformApplication.Helpers;

namespace MultiPlatformApplication.Models
{
    public enum ScrollTo
    {
        START,
        END,
        END_OF_RANGE_ADDED
    }

    /// <summary>
    /// To manage easily a ListView using a Refresh sytem and automatic scrolling when items are added
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DynamicListModel<T> : ObservableObject
    {
        // For scrolloning management we nneed to use two varaiables
        int indexToScroll = 0; // To store the index where we want to scroll to
        Boolean askingScrolling; // To store the info taht we want to scroll to an element

        Boolean askingMoreItems;
        Boolean noMoreItemsAvaialble;

        ListView listView;

        ICommand askMoreItemsCommand;

        /// <summary>
        /// Are we asking to more items in the list view (acessing a server for example) ?
        /// </summary>
        public Boolean AskingMoreItems
        {
            get { return askingMoreItems; }
            set { SetProperty(ref askingMoreItems, value); }
        }

        /// <summary>
        /// Does the list view contains all neessary items or some more could be added ?
        /// </summary>
        public Boolean NoMoreItemsAvailable
        {
            get { return noMoreItemsAvaialble; }
            set { SetProperty(ref noMoreItemsAvaialble, value); }
        }

        /// <summary>
        /// Items list display in the ListView
        /// </summary>
        public ObservableRangeCollection<T> Items { get; set; } = new ObservableRangeCollection<T>();

        /// <summary>
        /// Command used when we asked to refresh the list view  by code (using ListView.BeginRefresh()) or by gesture
        /// </summary>
        public ICommand AskMoreItemsCommand
        {
            get { return askMoreItemsCommand; }
            set { SetProperty(ref askMoreItemsCommand, value); }
        }

        /// <summary>
        /// Items view related to this dynamic list
        /// </summary>
        public ListView ListView
        {
            get { return listView; }
            set { 
                SetProperty(ref listView, value); 
                if (listView != null)
                {
                    listView.ItemAppearing += ListView_ItemAppearing;

                    // ON UWP when the first element appears, if there is some items avaialble we ask to load more
                    // We need to manage like this because there is no gesture to raise "RefreshCommand"
                    if (Device.RuntimePlatform == Device.UWP)
                        listView.Scrolled += ListView_Scrolled;
                }
            }
        }

#region EVENTS RAISED BY ListView OBJECT

        private void AskToScrollToIndex()
        {
            if (Items.Count > 0)
            {
                ScrollToPosition position;
                if (indexToScroll == Items.Count - 1)
                    position = ScrollToPosition.MakeVisible;
                else
                    position = ScrollToPosition.Start;
                ListView.ScrollTo(Items[indexToScroll], position, false);
            }
        }

        private void ListView_Scrolled(object sender, ScrolledEventArgs e)
        {
            // ON UWP when the first element appears, if there is still some items available, we ask to load more
            if ( (e.ScrollY <= 5) 
                && (!NoMoreItemsAvailable) 
                && (! (AskingMoreItems || askingScrolling) ) )
            {
                AskingMoreItems = true;
                ListView.BeginRefresh();
            }
        }

        private void ListView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            if (AskingMoreItems || askingScrolling)
            {

                if(e.ItemIndex == indexToScroll)
                {
                    askingScrolling = false;
                    AskingMoreItems = false;
                    ListView.EndRefresh();
                }
                else
                    AskToScrollToIndex();
            }
        }

#endregion EVENTS RAISED BY ListView OBJECT


        /// <summary>
        /// To add a range of items in the ListView and scroll to the correct position
        /// </summary>
        /// <param name="items">List of items</param>
        /// <param name="indexToAddRange">Position where new items are added</param>
        /// <param name="scrollTo">To indicate where to scroll to once items are added</param>
        public void AddRangeAndScroll(List<T> items, int indexToAddRange, ScrollTo scrollTo)
        {
            if (items != null)
            {
                switch(scrollTo)
                {
                    case ScrollTo.START:
                        indexToScroll = 0;
                        break;

                    case ScrollTo.END:
                        indexToScroll = items.Count  + Items.Count - 1;
                        break;

                    case ScrollTo.END_OF_RANGE_ADDED:
                        indexToScroll = items.Count - 1;
                        break;
                }

                askingScrolling = true;
                Items.AddRange(items, System.Collections.Specialized.NotifyCollectionChangedAction.Reset, indexToAddRange);

                if( (Device.RuntimePlatform == Device.iOS)
                    || (Device.RuntimePlatform == Device.Android) )
                    AskToScrollToIndex();
            }
        }

        /// <summary>
        /// To replace items of the ListView and scroll to the correct position
        /// </summary>
        /// <param name="items">List of items </param>
        /// <param name="scrollTo">To indicate where to scroll to once items are added</param>
        public void ReplaceRangeAndScroll(List<T> items, ScrollTo scrollTo)
        {
            if (items != null)
            {
                switch (scrollTo)
                {
                    case ScrollTo.END_OF_RANGE_ADDED: // Not really
                    case ScrollTo.START:
                        indexToScroll = 0;
                        break;

                    case ScrollTo.END:
                        indexToScroll = items.Count - 1;
                        break;
                }

                askingScrolling = true;
                Items.ReplaceRange(items);

                if ((Device.RuntimePlatform == Device.iOS)
                    || (Device.RuntimePlatform == Device.Android))
                    AskToScrollToIndex();
            }
        }

        /// <summary>
        /// To scroll to the start of the ListView
        /// </summary>
        public void ScrollToStart()
        {
            ScrollToIndex(0, ScrollToPosition.Start);
        }

        /// <summary>
        /// To scroll to the end of the ListView
        /// </summary>
        public void ScrollToEnd()
        {
            if(Items?.Count > 0)
                ScrollToIndex(Items.Count-1, ScrollToPosition.MakeVisible);
        }

        /// <summary>
        /// To scroll to the specified index of the ListView
        /// </summary>
        /// <param name="index">Index to scroll to</param>
        /// <param name="position">Position of the index once scrolled to</param>
        public void ScrollToIndex(int index, ScrollToPosition position)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    askingScrolling = true;
                    indexToScroll = index;

                    ListView.ScrollTo(Items[index], position, false);
                }
                catch
                {
                }
            });
            
        }

        public DynamicListModel()
        {
            AskingMoreItems = false;
            NoMoreItemsAvailable = false;
        }

        public void Dispose()
        {
            if (ListView != null)
            {
                listView.ItemAppearing -= ListView_ItemAppearing;
                ListView = null;
            }

            if (Items != null)
            {
                Items.Clear();
                Items = null;
            }
        }

        ~DynamicListModel()
        {
            Dispose();
        }

    }
}
