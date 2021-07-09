using MultiPlatformApplication.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MultiPlatformApplication.Models
{
    public class DynamicStreamModel: ObservableObject
    {
        Boolean askingMoreItems;
        Boolean moreItemsAvailable;
        Boolean userAskingToScroll;
        Boolean codeAskingToScroll;
        double deltaYFromTheBottom;

        ICommand askMoreItemsCommand;

        /// <summary>
        /// Are we asking to scroll the view to the correct position ?
        /// </summary>
        public Boolean UserAskingToScroll
        {
            get { return userAskingToScroll; }
            set { SetProperty(ref userAskingToScroll, value); }
        }


        /// <summary>
        /// Are we asking to scroll the view to the correct position by code ? ?
        /// </summary>
        public Boolean CodeAskingToScroll
        {
            get { return codeAskingToScroll; }
            set { SetProperty(ref codeAskingToScroll, value); }
        }


        /// <summary>
        /// Store Delta Y from the bottom of the content in the scroll view
        /// </summary>
        public double DeltaYFromTheBottom
        {
            get { return deltaYFromTheBottom; }
            set { SetProperty(ref deltaYFromTheBottom, value); }
        }


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
        public Boolean MoreItemsAvailable
        {
            get { return moreItemsAvailable; }
            set { SetProperty(ref moreItemsAvailable, value); }
        }

        /// <summary>
        /// Command used when we asked to refresh the list view  by code (using ListView.BeginRefresh()) or by gesture
        /// </summary>
        public ICommand AskMoreItemsCommand
        {
            get { return askMoreItemsCommand; }
            set { SetProperty(ref askMoreItemsCommand, value); }
        }

        public DynamicStreamModel()
        {
            AskingMoreItems = true;
            moreItemsAvailable = true;
        }
    }
}
