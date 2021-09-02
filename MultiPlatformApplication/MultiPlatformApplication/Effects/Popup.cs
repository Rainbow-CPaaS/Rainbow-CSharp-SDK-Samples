using MultiPlatformApplication.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MultiPlatformApplication.Effects
{
    public class Popup
    {
        private static int MINIMAL_MARGIN = 2;

        private static String contentPageId = null; // Store current content page id
        private static TapGestureRecognizer contentPageTapGestureRecognizer = null; // Tap gesture on content page

        private static String contextMenuDisplayed = null; // Only one context menu in same time in all the app. We store its name

        private static PopupAction popupAction; // Store "Action" (i.e. details) used to display the Popup

        private static Dictionary<String, PopupType> popupList = new Dictionary<string, PopupType>(); // Store by Id popup type

#region Type Property

        public static readonly BindableProperty TypeProperty = BindableProperty.Create("Type", typeof(PopupType), typeof(Popup), PopupType.Unknown, propertyChanged: OnTypeChanged);

        public static PopupType GetType(BindableObject view)
        {
            return (PopupType)view.GetValue(TypeProperty);
        }

        public static void SetType(BindableObject view, PopupType value)
        {
            view.SetValue(TypeProperty, value);
        }

        private static void OnTypeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            CheckEffect(bindable);
        }

#endregion Type Property

#region PUBLIC API

        public static View GetView(String viewName)
        {
            ContentPage contentPage = GetCurrentContentPage();
            return contentPage?.FindByName<View>(viewName);
        }

        public static Rect GetRectOfView(View view)
        {
            if (view == null)
                return new Rect();

            ContentPage contentPage = GetCurrentContentPage();
            return GetRectOfView(contentPage, view);
        }

        public static Rect GetRectOfView(String viewName)
        {
            ContentPage contentPage = GetCurrentContentPage();
            View view = contentPage?.FindByName<View>(viewName);
            if (view != null)
                return GetRectOfView(contentPage, view);
            return new Rect();
        }

        public static void Hide(String popupName)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                HideInternal(popupName);
            });
        }

        public static void Show(String popupName, String linkedToElementName)
        {
            Show(popupName, linkedToElementName, -1);
        }

        public static void Show(String popupName, String linkedToElementName, int delay)
        {
            Show(popupName, linkedToElementName, LayoutAlignment.Start, false, LayoutAlignment.End, true, new Point(), delay);
        }

        public static void Show(String popupName, Rect rect)
        {
            Show(popupName, rect, -1);
        }

        public static void Show(String popupName, Rect rect, int delay)
        {
            Show(popupName, rect, LayoutAlignment.Start, false, LayoutAlignment.End, true, new Point(), delay);
        }

        public static void Show(String popupName, String linkedToElementName, LayoutAlignment horizontalLayoutAlignment = LayoutAlignment.Start, Boolean outsideRectHorizontally = false, LayoutAlignment verticalLayoutAlignment = LayoutAlignment.End, Boolean outsideRectVertically = true, Point translation = default, int delay = -1)
        {
            PopupAction popupAction = new PopupAction
            {
                PopupName = popupName,
                LinkedToElementName = linkedToElementName,
                Rect = new Rect(),
                OutsideRectHorizontally = outsideRectHorizontally,
                OutsideRectVertically = outsideRectVertically,
                HorizontalLayoutAlignment = horizontalLayoutAlignment,
                VerticalLayoutAlignment = verticalLayoutAlignment,
                Translation = translation,
                Delay = delay
            };
            PrepareToShow(popupAction);
        }

        public static void Show(String popupName, Rect rect, LayoutAlignment horizontalLayoutAlignment = LayoutAlignment.Start, Boolean outsideRectHorizontally = false, LayoutAlignment verticalLayoutAlignment = LayoutAlignment.End, Boolean outsideRectVertically = true, Point translation = default, int delay = -1)
        {
            PopupAction popupAction = new PopupAction
            {
                PopupName = popupName,
                LinkedToElementName = null,
                Rect = rect,
                OutsideRectHorizontally = outsideRectHorizontally,
                OutsideRectVertically = outsideRectVertically,
                HorizontalLayoutAlignment = horizontalLayoutAlignment,
                VerticalLayoutAlignment = verticalLayoutAlignment,
                Translation = translation,
                Delay = delay
            };
            PrepareToShow(popupAction);
        }

#endregion PUBLIC API

        private static void PrepareToShow(PopupAction newPopupAction)
        {
            String currentContextMenuDisplayed = contextMenuDisplayed;
            HideCurrentContextMenu();

            ContentPage contentPage = GetCurrentContentPage();
            View view = contentPage?.FindByName<View>(newPopupAction.PopupName);
            if (view != null)
            {
                // Check if this popup is known
                String id = view.Id.ToString();
                if (!popupList.ContainsKey(id))
                    return;

                // Se store the popup type
                newPopupAction.PopupType = popupList[id];

                if (newPopupAction.PopupType == PopupType.ContextMenu)
                {
                    // If we ask to show the context menu currently displayed, we hide it instead
                    if (currentContextMenuDisplayed == newPopupAction.PopupName)
                    {
                        HideCurrentContextMenu();
                        return;
                    }
                }

                // Store the popup action
                popupAction = newPopupAction;

                // Add Tap gesture on content page - if necessary
                AddTapGestureToContentPage(contentPage);

                // On Desktop platform (at least in UWP) there is event propagation so we don't need to call directly ContentPageTapCommand
                if (!Helper.IsDesktopPlatform())
                    ContentPageTapCommand(null);
            }
        }

        private static Rect GetRectOfView(ContentPage contentPage, View view)
        {
            Rect result = new Rect();
            if(view != null)
                result = Helper.GetRelativePosition(view, (VisualElement)contentPage);
            return result;
        }

        private static Rect GetRectOfView(ContentPage contentPage, String viewName)
        {
            return GetRectOfView(contentPage, contentPage?.FindByName<View>(viewName));
        }

        private static ContentPage GetCurrentContentPage()
        {
            // Get current page
            Page page = Application.Current.MainPage;
            if ((page != null) && (page is NavigationPage navigationPage))
                page = navigationPage.CurrentPage;

            if ((page != null) && (page is ContentPage contentPage))
                    return contentPage;
            
            return null;
        }

        private static double GetXPosition(ContentPage contentPage, View view, LayoutAlignment layoutAlignment, Rect rect, bool outsideRect, double translateX)
        {
            double X = 0;

            // Do we want the popup outside or inside
            if (outsideRect)
            {
                switch (layoutAlignment)
                {
                    case LayoutAlignment.Start:
                    case LayoutAlignment.Fill:
                        X = rect.X - view.Width;
                        break;

                    case LayoutAlignment.End:
                        X = rect.X + rect.Width;
                        break;

                    case LayoutAlignment.Center:
                        X = popupAction.Rect.X + (popupAction.Rect.Width - view.Width) / 2;
                        break;
                }
                X += translateX;

                // Sanity check
                if (X + view.Width + MINIMAL_MARGIN > contentPage.Width)
                {
                    if (layoutAlignment != LayoutAlignment.End)
                        X = GetXPosition(contentPage, view, LayoutAlignment.End, rect, outsideRect, translateX);
                }

            }
            else // We want the popup inside
            {
                switch (layoutAlignment)
                {
                    case LayoutAlignment.Start:
                    case LayoutAlignment.Fill:
                        X = rect.X;
                        break;

                    case LayoutAlignment.End:
                        X = rect.X + rect.Width - view.Width;
                        break;

                    case LayoutAlignment.Center:
                        X = popupAction.Rect.X + (popupAction.Rect.Width - view.Width) / 2;
                        break;
                }
            }

            return X;
        }

        private static double GetYPosition(ContentPage contentPage, View view, LayoutAlignment layoutAlignment, Rect rect, bool outsideRect, double translateY)
        {
            double Y = 0;

            // Do we want the popup outside or inside
            if (outsideRect)
            {
                switch (layoutAlignment)
                {
                    case LayoutAlignment.Start:
                    case LayoutAlignment.Fill:
                        Y = rect.Y - view.Height;
                        break;

                    case LayoutAlignment.End:
                        Y = rect.Y + rect.Height;
                        break;

                    case LayoutAlignment.Center:
                        Y = rect.Y + (rect.Height - view.Height) / 2;
                        break;
                }

                Y += translateY;

                // Sanity check
                if (Y + view.Height + MINIMAL_MARGIN > contentPage.Height)
                {
                    if (layoutAlignment == LayoutAlignment.End)
                        Y = GetYPosition(contentPage, view, LayoutAlignment.Start, rect, outsideRect, translateY);
                }
            }
            else // We want the popup inside
            { 
                switch (layoutAlignment)
                {
                    case LayoutAlignment.Start:
                    case LayoutAlignment.Fill:
                        Y = rect.Y;
                        break;

                    case LayoutAlignment.End:
                        Y = rect.Y + rect.Height - view.Height;
                        break;

                    case LayoutAlignment.Center:
                        Y = rect.Y + (rect.Height - view.Height) / 2;
                        break;
                }
            }

            return Y;
        }

        private static void ProcessPositionAndShowPopup()
        {
            if (Popup.popupAction == null)
                return;

            // Store the context menu currently displayed
            PopupAction newPopupAction = Popup.popupAction;

            // Get ContentPage
            ContentPage contentPage = GetCurrentContentPage();

            // Get the View of the popup
            View view = contentPage?.FindByName<View>(newPopupAction.PopupName);
            if(view != null)
            {
                // Get Rect of the linked element (if any)
                if(!String.IsNullOrEmpty(newPopupAction.LinkedToElementName))
                    newPopupAction.Rect = GetRectOfView(contentPage, newPopupAction.LinkedToElementName);

                // If no Rect is defined, we use the Content Page Bounds
                if (newPopupAction.Rect.IsEmpty)
                    newPopupAction.Rect = contentPage.Bounds;

                // Calculate the new position of the Popup
                double X = 0, Y = 0;

                X = GetXPosition(contentPage, view, newPopupAction.HorizontalLayoutAlignment, newPopupAction.Rect, newPopupAction.OutsideRectHorizontally, popupAction.Translation.X);
                Y = GetYPosition(contentPage, view, newPopupAction.VerticalLayoutAlignment, newPopupAction.Rect, newPopupAction.OutsideRectVertically, popupAction.Translation.Y);

                // Sanity check on X
                if (X + view.Width + MINIMAL_MARGIN > contentPage.Width)
                    X = contentPage.Width - view.Width - MINIMAL_MARGIN;
                if (X < MINIMAL_MARGIN)
                    X = MINIMAL_MARGIN;

                // Sanity check on Y
                if (Y + view.Height + MINIMAL_MARGIN > contentPage.Height)
                    Y = contentPage.Height - view.Height - MINIMAL_MARGIN;
                if (Y < MINIMAL_MARGIN)
                    Y = MINIMAL_MARGIN;

                // Now set 
                Device.BeginInvokeOnMainThread(() =>
                {
                    // Are we dealing with a Context Menu ?
                    if (newPopupAction.PopupType == PopupType.ContextMenu)
                    {
                        // Hide previous context menu - if any (only can be display in same time
                        HideInternal(contextMenuDisplayed);

                        // Store new context menu
                        contextMenuDisplayed = newPopupAction.PopupName;
                    }
                    else // We display an Information popup. Do we have to hide it after a delay ?
                    {
                        if(newPopupAction.Delay > 0 )
                        {
                            Device.StartTimer( TimeSpan.FromMilliseconds(newPopupAction.Delay), () =>
                            {
                                HideInternal(newPopupAction.PopupName); return false;
                            });
                        }
                    }

                    // Set X and Y Constraints
                    RelativeLayout.SetXConstraint(view, Constraint.RelativeToParent((rl) => X));
                    RelativeLayout.SetYConstraint(view, Constraint.RelativeToParent((rl) => Y));

                    // Show popup
                    view.IsVisible = true;
                });
            }
            // Set action to null
            popupAction = null;
        }

        private static void CheckEffect(BindableObject bindable)
        {
            var view = bindable as View;
            if (view == null)
                return;

            if (!(view.Parent is RelativeLayout relativeLayout))
                throw new ArgumentException($"Popup element must have a RelativeLayout as parent");

            if ( !(relativeLayout.Parent is ContentPage))
                throw new ArgumentException($"Popup element must have a RelativeLayout then a ContentPage as parent");

            // Store its Popup type (by Id)
            String id = view.Id.ToString();
            if (popupList.ContainsKey(id))
                popupList.Remove(id);
            popupList.Add(id, GetType(view));


        }

        private static Boolean AddTapGestureToContentPage(ContentPage contentPage)
        {
            if (contentPage != null)
            {
                if (Popup.contentPageId != contentPage.Id.ToString())
                {
                    if (!String.IsNullOrEmpty(Popup.contentPageId))
                    {
                        // TODO: NEED TO CLEAN PREVIOUS OBJECTS
                        // We have to remove TapGesture ... But how ?
                    }

                    // Store new content page Id
                    Popup.contentPageId = contentPage.Id.ToString();

                    // Create tap gesture if necessary
                    if (contentPageTapGestureRecognizer == null)
                    {
                        contentPageTapGestureRecognizer = new TapGestureRecognizer();
                        contentPageTapGestureRecognizer.Command = new RelayCommand<object>(new Action<object>(Popup.ContentPageTapCommand));
                    }

                    // Add tap gesture
                    contentPage.Content.GestureRecognizers.Add(contentPageTapGestureRecognizer);

                    return true;
                }
            }
            return false;
        }

        private static void HideInternal(String popupName)
        {
            if (String.IsNullOrEmpty(popupName))
                return;

            ContentPage contentPage = GetCurrentContentPage();
            View view = contentPage?.FindByName<View>(popupName);
            if (view != null)
                view.IsVisible = false;
        }

        private static void HideCurrentContextMenu()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                HideInternal(contextMenuDisplayed);
                contextMenuDisplayed = null;
            });
        }

        private static void ContentPageTapCommand(object obj)
        {
            if (popupAction == null)
                HideCurrentContextMenu();
            else
                ProcessPositionAndShowPopup();
        }

        private enum PopupActionType
        {
            UsingLinkTo,
            UsingRect,
            UsingLayoutAlignment
        }

        private class PopupAction
        {
            public String PopupName;
            public String LinkedToElementName;

            public PopupType PopupType = PopupType.Unknown;

            public Rect Rect;

            public Boolean OutsideRectVertically;
            public Boolean OutsideRectHorizontally;

            public LayoutAlignment HorizontalLayoutAlignment;
            public LayoutAlignment VerticalLayoutAlignment;

            public Point Translation;
            public int Delay;
        }

    }

    public enum PopupType
    {
        ContextMenu,
        Information,
        Unknown
    }


}
