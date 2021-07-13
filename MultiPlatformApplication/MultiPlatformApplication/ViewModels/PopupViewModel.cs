using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using NLog;
using Rainbow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class PopupViewModel: ObservableObject
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(PopupViewModel));

        private Boolean firstInitialization = true;
        
        View rootView; // Root view which contains popup structure
        View popupBackgroundView; // View use as popup background
        View popupRootView; // root tohe the popup view

        Dictionary<String, View> popupItems = new Dictionary<string, View>(); // Dictionary of popup items using their name as key


        public PopupModel PopupModel { get; internal set; } = new PopupModel();

        public PopupViewModel()
        {
            
        }

        public void SetRootView(View rootView)
        {
            this.rootView = rootView;
        }

        public void Initialize()
        {
            if (firstInitialization)
            {
                firstInitialization = false;

                if (rootView != null)
                {

                    //// Get background view used in the Popup structure
                    //popupBackgroundView = (View)rootView.FindByName("PopupBackground");

                    //// Get root view used in the Popup structure
                    //popupRootView = (View)rootView.FindByName("PopupRoot");

                    //// Get all items used as poup content - AutomationId is used to get their names
                    //if (popupRootView != null)
                    //{
                    //    StackLayout stackLayout = (StackLayout)popupRootView.FindByName("PopupItems");

                    //    if (stackLayout?.Children?.Count > 0)
                    //    {
                    //        String name;
                    //        foreach (View item in stackLayout.Children)
                    //        {
                    //            name = item.AutomationId;
                    //            if (!String.IsNullOrEmpty(name))
                    //                popupItems.Add(name, item);
                    //        }
                    //    }
                    //}
                }
            }

            //HidePopup();
        }

        public Boolean DisplayPopup(String itemName, String title = null, String cancelText = null, String acceptText = null)
        {
            if (popupItems.ContainsKey(itemName))
            {
                popupItems[itemName].IsVisible = true;

                if (popupBackgroundView != null)
                    popupBackgroundView.IsVisible = true;

                if (popupRootView != null)
                    popupRootView.IsVisible = true;


                // Set text
                PopupModel.Title = title;
                PopupModel.CancelText = cancelText;
                PopupModel.AcceptText = acceptText;

                return true;
            }

            return false;
        }

        public void HidePopup()
        {
            if (popupBackgroundView != null)
                popupBackgroundView.IsVisible = false;

            if (popupRootView != null)
            {
                popupRootView.IsVisible = false;
                HideAllPopupItems();
            }
        }

        private void HideAllPopupItems()
        {
            foreach (View item in popupItems.Values)
            {
                item.IsVisible = false;
            }
        }
    }
}
