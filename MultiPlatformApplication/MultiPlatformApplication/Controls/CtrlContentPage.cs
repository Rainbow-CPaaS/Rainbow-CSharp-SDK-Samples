using MultiPlatformApplication.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace MultiPlatformApplication.Controls
{
    public class CtrlContentPage : Xamarin.Forms.ContentPage
    {
        private RelativeLayout RainbowContentRelativeLayout;
        private Grid RainbowContentPageGrid;

#region TO DEFINE POPUPS FOR THIS PAGE
        public RelativeLayout Popups
        {
            set
            {
                // If we have already views used as popup, removed them
                // NIOTE: We have at least RainbowContentRelativeLayout s children
                if (RainbowContentRelativeLayout.Children.Count > 1)
                {
                    var childrens = RainbowContentRelativeLayout.Children;

                    foreach (View child in childrens)
                        RemoveViewAsPopupInternal(child);
                }

                if (value != null)
                {
                    while (value.Children.Count > 0)
                    {
                        // Get first children and removed from the RelativeLayout provided
                        View view = value.Children[0];
                        value.Children.Remove(view);

                        // Add this view as Popup
                        AddViewAsPopupInternal(view);
                    }
                }
            }
        }
#endregion TO DEFINE POPUPS FOR THIS PAGE


#region TO DEFINE THE CONTENT OF THIS PAGE
        private View _ctrlContent = null;
        public View CtrlContent
        {
            get
            {
                return _ctrlContent;
            }

            set
            {
                // If previous value was not null we remove it from the grid
                if (_ctrlContent != null)
                    RainbowContentPageGrid.Children.Remove(_ctrlContent);

                // Store new value
                _ctrlContent = value;

                // Add it to the grid
                RainbowContentPageGrid.Children.Add(_ctrlContent, 0, 0);
            }
        }
#endregion TO DEFINE THE CONTENT OF THIS PAGE

        public CtrlContentPage()
        {
            // Define grid taking all space with 1 column and 1 row.
            RainbowContentPageGrid = new Grid { Margin = 0, Padding = 0, ColumnSpacing =0, RowSpacing = 0 };
            RainbowContentPageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            RainbowContentPageGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

            // Create Relative Layout which take all space too
            RainbowContentRelativeLayout = new RelativeLayout { Margin = 0, Padding = 0, HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill };
            RainbowContentRelativeLayout.BackgroundColor = Color.Transparent; // Color.DarkGreen;

            // Grid is added to teh relative layout with constraint to tak all available space
            RainbowContentRelativeLayout.Children.Add(RainbowContentPageGrid, Constraint.Constant(0), Constraint.Constant(0), Constraint.RelativeToParent((rl) => rl.Width), Constraint.RelativeToParent((rl) => rl.Height));

            // Set content to this relative layout
            base.Content = RainbowContentRelativeLayout;

            // Avoid overwriting the iOS status bar: https://docs.microsoft.com/en-us/xamarin/xamarin-forms/xaml/xaml-basics/essential-xaml-syntax
            if (Device.RuntimePlatform == Device.iOS)
                Padding = new Thickness(0, 20, 0, 0);

            this.SizeChanged += CtrlContentPage_SizeChanged;
        }

        private void CtrlContentPage_SizeChanged(object sender, EventArgs e)
        {
            Popup.HideCurrentContextMenu();
            Popup.UpdateActivityIndicators();
        }

        internal void RemoveViewAsPopup(View view)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                RemoveViewAsPopupInternal(view);
            });
        }

        internal RelativeLayout GetRelativeLayout()
        {
            return RainbowContentRelativeLayout;
        }

        internal Boolean PopupExists(string automationId)
        {
            if (RainbowContentRelativeLayout.Children.Count > 1)
            {
                var childrens = RainbowContentRelativeLayout.Children;

                foreach (View child in childrens)
                {
                    if (child.AutomationId == automationId)
                        return true;
                }
            }
            return false;
        }

        internal View GetPopup(string automationId)
        {
            if (RainbowContentRelativeLayout.Children.Count > 1)
            {
                var childrens = RainbowContentRelativeLayout.Children;

                foreach (View child in childrens)
                {
                    if (child.AutomationId == automationId)
                        return child;
                }
            }
            return null;
        }

        internal Boolean AddViewAsPopupInternal(View view)
        {
            Boolean result = false;
            if (view != null)
            {
                try
                {
                    // Ensure to have this view not visible by default
                    view.IsVisible = false;

                    // Add the view in the Relative Layout
                    RainbowContentRelativeLayout.Children.Add(view, Constraint.Constant(0), Constraint.Constant(0));

                    result = true;
                }
                catch { }
            }
            return result;
        }

        internal Boolean RemoveViewAsPopupInternal(View view)
        {
            Boolean result = false;
            try
            {
                if ((view != null) && (view != RainbowContentPageGrid))
                {
                    RainbowContentRelativeLayout.Children.Remove(view);

                    result = true;
                }
            }
            catch { }

            return result;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
    }
}
