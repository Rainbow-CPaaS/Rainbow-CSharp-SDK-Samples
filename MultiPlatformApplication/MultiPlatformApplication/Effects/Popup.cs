using MultiPlatformApplication.Events;
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

        private static PopupAction previousContextMenuActionDisplayed = null; // Only one context menu in same time in all the app. We store te action used to display it

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

        public static View GetView(String automationId, ContentPage contentPage = null)
        {
            if(contentPage == null)
                contentPage = GetCurrentContentPage();

            if(contentPage != null)
            {
                if (contentPage.Content.AutomationId == automationId)
                    return contentPage.Content;

                if (contentPage.Content is Layout layout)
                    return GetChildView(layout, automationId);
            }
            return null;
        }

        public static Rect GetRectOfView(View view)
        {
            if (view == null)
                return new Rect();

            ContentPage contentPage = GetCurrentContentPage();
            return GetRectOfView(contentPage, view);
        }

        public static Rect GetRectOfView(String automationId)
        {
            ContentPage contentPage = GetCurrentContentPage();
            View view = GetView(automationId, contentPage);
            if (view != null)
                return GetRectOfView(contentPage, view);
            return new Rect();
        }

        private static void TouchEffect_TouchAction(object sender, TouchActionEventArgs e)
        {
            if (e.Type == TouchActionType.Pressed)
            {
                HideCurrentContextMenu();
            };
        }

        public static void AutoHideForView(View view)
        {
            TouchEffect touchEffect = new TouchEffect();
            touchEffect.TouchAction += TouchEffect_TouchAction;
            Helper.AddEffect(view, touchEffect);
        }

        public static void Hide(String popupAutomationId)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (popupAutomationId == previousContextMenuActionDisplayed?.PopupAutomationId)
                    previousContextMenuActionDisplayed = null;

                HideInternal(popupAutomationId);
            });
        }

        public static void HideCurrentContextMenu()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                HideInternal(previousContextMenuActionDisplayed?.PopupAutomationId);
                previousContextMenuActionDisplayed = null;
            });
        }

        public static void Add(MultiPlatformApplication.Controls.CtrlContentPage contentPage, View view, PopupType type)
        {
            if (contentPage == null)
                contentPage = GetCurrentContentPage();

            if (contentPage != null)
            {
                contentPage.AddViewAsPopupInternal(view);
                SetType(view, type);
            }
        }

        public static void Remove(MultiPlatformApplication.Controls.CtrlContentPage contentPage, View view)
        {
            contentPage.RemoveViewAsPopup(view);
        }

        public static void Remove(MultiPlatformApplication.Controls.CtrlContentPage contentPage, String automationId)
        {
            View view = GetView(automationId, contentPage);
            if(view != null)
                contentPage.RemoveViewAsPopup(view);
        }

        public static void Show(String popupAutomationId, String linkedToAutomationId)
        {
            Show(popupAutomationId, linkedToAutomationId, -1);
        }

        public static void Show(String popupAutomationId, String linkedToAutomationId, int delay)
        {
            Show(popupAutomationId, linkedToAutomationId, LayoutAlignment.Start, false, LayoutAlignment.End, true, new Point(), delay);
        }

        public static void Show(String popupAutomationId, Rect rect)
        {
            Show(popupAutomationId, rect, -1);
        }

        public static void Show(String popupAutomationId, Rect rect, int delay)
        {
            Show(popupAutomationId, rect, LayoutAlignment.Start, false, LayoutAlignment.End, true, new Point(), delay);
        }

        public static void Show(String popupAutomationId, String linkedToAutomationId, LayoutAlignment horizontalLayoutAlignment = LayoutAlignment.Start, Boolean outsideRectHorizontally = false, LayoutAlignment verticalLayoutAlignment = LayoutAlignment.End, Boolean outsideRectVertically = true, Point translation = default, int delay = -1)
        {
            PopupAction popupAction = new PopupAction
            {
                PopupAutomationId = popupAutomationId,
                LinkedToElementName = linkedToAutomationId,
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

        public static void Show(String popupAutomationId, Rect rect, LayoutAlignment horizontalLayoutAlignment = LayoutAlignment.Start, Boolean outsideRectHorizontally = false, LayoutAlignment verticalLayoutAlignment = LayoutAlignment.End, Boolean outsideRectVertically = true, Point translation = default, int delay = -1)
        {
            PopupAction popupAction = new PopupAction
            {
                PopupAutomationId = popupAutomationId,
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
            PopupAction currentContextMenuDisplayed = previousContextMenuActionDisplayed;
            HideCurrentContextMenu();

            ContentPage contentPage = GetCurrentContentPage();
            View view = GetView(newPopupAction.PopupAutomationId, contentPage);
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
                    if (currentContextMenuDisplayed?.PopupAutomationId == newPopupAction.PopupAutomationId)
                    {
                        // We also need to check if it's the same place or not linked to the same element
                        if ( ( (currentContextMenuDisplayed.LinkedToElementName == newPopupAction.LinkedToElementName) && (!String.IsNullOrEmpty(newPopupAction.LinkedToElementName)) )
                            || ( (currentContextMenuDisplayed.Rect == newPopupAction.Rect)  && (!newPopupAction.Rect.IsEmpty) ) )
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

        private static Rect GetRectOfView(ContentPage contentPage, String automationId)
        {
            return GetRectOfView(contentPage, GetView(automationId, contentPage));
        }

        private static View GetChildView(Layout layout, String automationId)
        {
            View result = null;
            if (layout != null)
            {
                var childrens = layout.Children;
                foreach (View child in childrens)
                {
                    if (child.AutomationId == automationId)
                        return child;

                    if (child is Layout childLayout)
                    {
                        result = GetChildView(childLayout, automationId);
                        if (result != null)
                            return result;
                    }
                }
            }
            return null;
        }

        private static MultiPlatformApplication.Controls.CtrlContentPage GetCurrentContentPage()
        {
            // Get current page
            NavigationPage navigationPage = ((App)Xamarin.Forms.Application.Current).NavigationService.CurrentNavigationPage;
            Page page = navigationPage.CurrentPage;

            if ((page != null) && (page is MultiPlatformApplication.Controls.CtrlContentPage contentPage))
                    return contentPage;
            
            return null;
        }

        private static double GetXPosition(ContentPage contentPage, View view, LayoutAlignment layoutAlignment, Rect rect, bool outsideRect, double translateX)
        {
            double X = 0;

            double viewWidth = view.WidthRequest;
            if(viewWidth == -1)
                viewWidth = view.Width;


            // Do we want the popup outside or inside
            if (outsideRect)
            {
                switch (layoutAlignment)
                {
                    case LayoutAlignment.Start:
                    case LayoutAlignment.Fill:
                        X = rect.X - viewWidth;
                        break;

                    case LayoutAlignment.End:
                        X = rect.X + rect.Width;
                        break;

                    case LayoutAlignment.Center:
                        X = popupAction.Rect.X + (popupAction.Rect.Width - viewWidth) / 2;
                        break;
                }
                X += translateX;

                // Sanity check
                if (X + viewWidth + MINIMAL_MARGIN > contentPage.Width)
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
                        X = rect.X + rect.Width - viewWidth;
                        break;

                    case LayoutAlignment.Center:
                        X = popupAction.Rect.X + (popupAction.Rect.Width - viewWidth) / 2;
                        break;
                }
            }

            // Final Sanity check on X
            if (X + viewWidth + MINIMAL_MARGIN > contentPage.Width)
                X = contentPage.Width - viewWidth - MINIMAL_MARGIN;
            if (X < MINIMAL_MARGIN)
                X = MINIMAL_MARGIN;

            return X;
        }

        private static double GetYPosition(ContentPage contentPage, View view, LayoutAlignment layoutAlignment, Rect rect, bool outsideRect, double translateY)
        {
            double Y = 0;

            double viewHeight = view.HeightRequest;
            if (viewHeight == -1)
                viewHeight = view.Height;

            // Do we want the popup outside or inside
            if (outsideRect)
            {
                switch (layoutAlignment)
                {
                    case LayoutAlignment.Start:
                    case LayoutAlignment.Fill:
                        Y = rect.Y - viewHeight;
                        break;

                    case LayoutAlignment.End:
                        Y = rect.Y + rect.Height;
                        break;

                    case LayoutAlignment.Center:
                        Y = rect.Y + (rect.Height - viewHeight) / 2;
                        break;
                }

                Y += translateY;

                // Sanity check
                if (Y + viewHeight + MINIMAL_MARGIN > contentPage.Height)
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
                        Y = rect.Y + rect.Height - viewHeight;
                        break;

                    case LayoutAlignment.Center:
                        Y = rect.Y + (rect.Height - viewHeight) / 2;
                        break;
                }
            }

            // Fianl Sanity check on Y
            if (Y + viewHeight + MINIMAL_MARGIN > contentPage.Height)
                Y = contentPage.Height - viewHeight - MINIMAL_MARGIN;
            if (Y < MINIMAL_MARGIN)
                Y = MINIMAL_MARGIN;

            return Y;
        }

        private static void ProcessPositionAndShowPopup()
        {
            if (Popup.popupAction == null)
                return;

            // Store the context menu currently displayed
            PopupAction newPopupAction = Popup.popupAction;

            // Get ContentPage
            MultiPlatformApplication.Controls.CtrlContentPage contentPage = GetCurrentContentPage();

            // Get the View of the popup
            View view = GetView(newPopupAction.PopupAutomationId, contentPage);
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

                

                

                // Now set 
                Device.BeginInvokeOnMainThread(() =>
                {
                    // Are we dealing with a Context Menu ?
                    if (newPopupAction.PopupType == PopupType.ContextMenu)
                    {
                        // Hide previous context menu - if any (only can be display in same time
                        HideInternal(previousContextMenuActionDisplayed?.PopupAutomationId);

                        // Store context menu
                        previousContextMenuActionDisplayed = newPopupAction;
                    }
                    else // We display an Information popup. Do we have to hide it after a delay ?
                    {
                        if(newPopupAction.Delay > 0 )
                        {
                            Device.StartTimer( TimeSpan.FromMilliseconds(newPopupAction.Delay), () =>
                            {
                                HideInternal(newPopupAction.PopupAutomationId); return false;
                            });
                        }
                    }

                    // Set X and Y Constraints
                    RelativeLayout.SetXConstraint(view, Constraint.RelativeToParent((rl) => X));
                    RelativeLayout.SetYConstraint(view, Constraint.RelativeToParent((rl) => Y));
                    

                    // Show popup
                    view.IsVisible = true;
                    contentPage.GetRelativeLayout().RaiseChild(view);
                    
                    
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

            //if (!(view.Parent is RelativeLayout relativeLayout))
            //    throw new ArgumentException($"Popup element must have a RelativeLayout as parent");

            //if ( !(relativeLayout.Parent is ContentPage))
            //    throw new ArgumentException($"Popup element must have a RelativeLayout then a ContentPage as parent");

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

        private static void HideInternal(String popupAutomationId)
        {
            if (String.IsNullOrEmpty(popupAutomationId))
                return;

            ContentPage contentPage = GetCurrentContentPage();
            View view = GetView(popupAutomationId, contentPage);
            if (view != null)
                view.IsVisible = false;
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
            public String PopupAutomationId;
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
