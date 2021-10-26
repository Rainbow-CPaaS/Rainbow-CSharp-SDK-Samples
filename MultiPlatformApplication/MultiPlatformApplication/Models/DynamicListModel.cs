using System;
using System.Linq;
using System.Windows.Input;

using Xamarin.Forms;

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms.Shapes;
using MultiPlatformApplication.Helpers;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;

namespace MultiPlatformApplication.Models
{
    /// <summary>
    /// To manage easily a ListView using a Refresh sytem and automatic scrolling when items are added
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DynamicListModel<T> : ObservableObject
    {
        Boolean askingMoreItems;
        Boolean noMoreItemsAvaialble;
        ICommand askMoreItemsCommand;

        ICommand selectedItemCommand;

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
        /// Command used when we asked to refresh the list view  by code (using ListView.BeginRefresh()) or by gesture
        /// </summary>
        public ICommand AskMoreItemsCommand
        {
            get { return askMoreItemsCommand; }
            set { SetProperty(ref askMoreItemsCommand, value); }
        }

        /// <summary>
        /// Command used when an item has been selected
        /// </summary>
        public ICommand SelectedItemCommand
        {
            get { return selectedItemCommand; }
            set { SetProperty(ref selectedItemCommand, value); }
        }

        /// <summary>
        /// Items list display in the ListView
        /// </summary>
        public ObservableRangeCollection<T> Items { get; set; } = new ObservableRangeCollection<T>();


        /// <summary>
        /// To add a range of items in the list
        /// </summary>
        /// <param name="items">List of items</param>
        /// <param name="indexToAddRange">Position where new items are added</param>
        public void AddRange(List<T> items, int indexToAddRange)
        {
            if (items != null)
                Items.AddRange(items, System.Collections.Specialized.NotifyCollectionChangedAction.Reset, indexToAddRange);
        }

        /// <summary>
        /// To add a range of items in the list
        /// </summary>
        /// <param name="items">List of items</param>
        /// <param name="indexToAddRange">Position where new items are added</param>
        public void Add(T item, int indexToAdd)
        {
            var list = new List<T>();
            list.Add(item);
            AddRange(list, indexToAdd);
        }

        /// <summary>
        /// To replace items of the ListView
        /// </summary>
        /// <param name="items">List of items </param>
        public void ReplaceRange(List<T> items)
        {
            if (items != null)
                Items.ReplaceRange(items);
        }

        public DynamicListModel()
        {
            AskingMoreItems = false;
            NoMoreItemsAvailable = false;
        }

        public void Dispose()
        {
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
