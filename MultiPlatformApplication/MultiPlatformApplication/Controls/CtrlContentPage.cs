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
        }

        internal void AddViewAsPopup(View view)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                AddViewAsPopupInternal(view);
            });
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

        internal void AddViewAsPopupInternal(View view)
        {
            if (view != null)
            {
                // Ensure to have this view not visible by default
                view.IsVisible = false;

                // Add the view in the Relative Layout
                RainbowContentRelativeLayout.Children.Add(view, Constraint.Constant(0), Constraint.Constant(0));
            }
        }

        private void RemoveViewAsPopupInternal(View view)
        {
            if ( (view != null) && (view != RainbowContentPageGrid) )
                RainbowContentRelativeLayout.Children.Remove(view);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
    }
}
