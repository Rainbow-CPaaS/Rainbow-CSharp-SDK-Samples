using MultiPlatformApplication.Events;
using MultiPlatformApplication.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MultiPlatformApplication.Effects
{
    public class Popup
    {
        public static String DEFAULT_ACTIVITY_INDICATOR_AUTOMATION_ID = "RainbowDefaultActivityIndicatorAutomationId";

        public static String DEFAULT_DISCONNECTION_INFORMATION_AUTOMATION_ID = "RainbowDefaultDisconnexionInformationAutomationId";

        private static int MINIMAL_MARGIN = 2;

        private static String contentPageId = null; // Store current content page id
        private static TapGestureRecognizer contentPageTapGestureRecognizer = null; // Tap gesture on content page

        private static PopupAction previousContextMenuActionDisplayed = null; // Only one context menu in same time in all the app. We store te action used to display it

        private static PopupAction popupAction; // Store "Action" (i.e. details) used to display the Popup

        private static Dictionary<String, PopupType> popupList = new Dictionary<string, PopupType>(); // Store by Id popup type

        private static Dictionary<String, PopupAction> activityIndicatorList = new Dictionary<string, PopupAction>(); // Store by AutomationID PopuÂction on ActivityIndicator


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
            // NOT NECESSARY TO BE ON MAIN THREAD

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
            // NOT NECESSARY TO BE ON MAIN THREAD

            if (view == null)
                return new Rect();

            ContentPage contentPage = GetCurrentContentPage();
            return GetRectOfView(contentPage, view);
        }

        public static Rect GetRectOfView(String automationId)
        {
            // NOT NECESSARY TO BE ON MAIN THREAD

            ContentPage contentPage = GetCurrentContentPage();
            View view = GetView(automationId, contentPage);
            if (view != null)
                return GetRectOfView(contentPage, view);
            return new Rect();
        }

        public static void Hide(String popupAutomationId)
        {
            // NOT NECESSARY TO BE ON MAIN THREAD
            HideInternal(popupAutomationId);
        }

        public static void HideCurrentContextMenu()
        {
            // NOT NECESSARY TO BE ON MAIN THREAD
            HideInternal(previousContextMenuActionDisplayed?.PopupAutomationId);
        }

    #region ACTIVITY INDICATOR RELATED

        public static void ShowDefaultActivityIndicator(String linkedToAutomationId = null)
        {
            // NECESSARY TO BE ON MAIN THREAD
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => ShowDefaultActivityIndicator(linkedToAutomationId));
                return;
            }

            MultiPlatformApplication.Controls.CtrlContentPage contentPage = GetCurrentContentPage();

            if (contentPage?.PopupExists(DEFAULT_ACTIVITY_INDICATOR_AUTOMATION_ID) != true)
                AddBasicActivityActivatorUsingDefaultSettings(null, DEFAULT_ACTIVITY_INDICATOR_AUTOMATION_ID);

            Show(DEFAULT_ACTIVITY_INDICATOR_AUTOMATION_ID, linkedToAutomationId, LayoutAlignment.Center, false, LayoutAlignment.Center, false, new Point(), -1);
        }

        public static void ShowActivityIndicator(String popupAutomationId, String linkedToAutomationId)
        {
            // NOT NECESSARY TO BE ON MAIN THREAD
            Show(popupAutomationId, linkedToAutomationId, LayoutAlignment.Center, false, LayoutAlignment.Center, false, new Point(), -1);
        }

        public static void SetDefaultBasicActivityActivator(Color obfuscationColor, double squareSize, float squareCornerRadius, double squareCornerSize, Color squareCornerColor, Color squareBackgroundColor, double indicatorSize, Color indicatorColor, Boolean useDeviceActivityIndicator)
        {
            // NECESSARY TO BE ON MAIN THREAD
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => SetDefaultBasicActivityActivator(obfuscationColor, squareSize, squareCornerRadius, squareCornerSize, squareCornerColor, squareBackgroundColor, indicatorSize, indicatorColor, useDeviceActivityIndicator));
                return;
            }

            MultiPlatformApplication.Controls.CtrlContentPage contentPage = GetCurrentContentPage();

            if (contentPage != null)
            {
                View popupView = contentPage?.GetPopup(DEFAULT_ACTIVITY_INDICATOR_AUTOMATION_ID);
                if (popupView == null)
                {
                    AddBasicActivityActivator(contentPage, DEFAULT_ACTIVITY_INDICATOR_AUTOMATION_ID, obfuscationColor, squareSize, squareCornerRadius, squareCornerSize, squareCornerColor, squareBackgroundColor, indicatorSize, indicatorColor, useDeviceActivityIndicator);
                }
                else
                {
                    if (popupView is ContentView contentView)
                    {
                        popupView.BackgroundColor = obfuscationColor;

                        if (contentView.Content is Frame frame1)
                        {
                            frame1.HeightRequest = squareSize;
                            frame1.WidthRequest = squareSize;
                            frame1.CornerRadius = squareCornerRadius;
                            frame1.BackgroundColor = squareCornerColor;

                            if (frame1.Content is Frame frame2)
                            {
                                frame2.WidthRequest = squareSize - squareCornerSize;
                                frame2.HeightRequest = squareSize - squareCornerSize;
                                frame2.CornerRadius = squareCornerRadius;
                                frame2.BackgroundColor = squareBackgroundColor;

                                // Recreate Content
                                frame2.Content = CreateActivityIndicatorContent(indicatorSize, indicatorColor, useDeviceActivityIndicator);

                                if (useDeviceActivityIndicator)
                                    frame2.Content.SetBinding(ActivityIndicator.IsRunningProperty, new Binding("IsVisible", source: popupView));
                            }
                        }
                    }
                }
            }
        }

        public static void AddBasicActivityActivator(MultiPlatformApplication.Controls.CtrlContentPage contentPage, String automationId, Color obfuscationColor, double squareSize, float squareCornerRadius, double squareCornerSize, Color squareCornerColor, Color squareBackgroundColor, double indicatorSize, Color indicatorColor, Boolean useDeviceActivityIndicator)
        {
            // NECESSARY TO BE ON MAIN THREAD
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => AddBasicActivityActivator(contentPage, automationId, obfuscationColor, squareSize, squareCornerRadius, squareCornerSize, squareCornerColor, squareBackgroundColor, indicatorSize, indicatorColor, useDeviceActivityIndicator));
                return;
            }

            if (contentPage == null)
                contentPage = GetCurrentContentPage();

            if (contentPage?.PopupExists(automationId) == true)
                return;

            if (String.IsNullOrEmpty(automationId))
                return;

            ContentView contentView = new ContentView
            {
                AutomationId = automationId,
                IsVisible = false,

                Margin = 0,
                Padding = 0,
                BackgroundColor = obfuscationColor
            };

            // Create Frame which contains the activity indicator
            Frame frame1 = new Frame
            {
                Margin = 0,
                Padding = 0,
                HasShadow = false,

                WidthRequest = squareSize,
                HeightRequest = squareSize,

                CornerRadius = squareCornerRadius,
                BackgroundColor = squareCornerColor,

                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            // Set frame content
            contentView.Content = frame1;

            // Create Frame which contains the activity indicator
            Frame frame2 = new Frame
            {
                Margin = 0,
                Padding = 0,
                HasShadow = false,

                WidthRequest = squareSize - squareCornerSize,
                HeightRequest = squareSize - squareCornerSize,

                CornerRadius = squareCornerRadius,
                BackgroundColor = squareBackgroundColor,

                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            frame1.Content = frame2;

            // Create activity indicator
            View activityIndicator = CreateActivityIndicatorContent(indicatorSize, indicatorColor, useDeviceActivityIndicator);

            if(useDeviceActivityIndicator)
                activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, new Binding("IsVisible", source: contentView));

            // Set the activityIndicator as content of the frame
            frame2.Content = activityIndicator;

            contentPage.AddViewAsPopupInternal(contentView);
            SetType(contentView, PopupType.ActivityIndicator);
        }

        public static void AddBasicActivityActivatorUsingDefaultSettings(MultiPlatformApplication.Controls.CtrlContentPage contentPage, String automationId)
        {
            // NECESSARY TO BE ON MAIN THREAD
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => AddBasicActivityActivatorUsingDefaultSettings(contentPage, automationId));
                return;
            }

            Color backColor = Helper.GetResourceDictionaryById<Color>("ColorEntryBackground");
            Color color = Helper.GetResourceDictionaryById<Color>("ColorMain");
            Color borderColor = Helper.GetResourceDictionaryById<Color>("ColorEntryPlaceHolder");

            String colorHex = borderColor.ToHex();
            colorHex = "#7F" + colorHex.Substring(3);
            Color obfuscationColor = Color.FromHex(colorHex);

            if (automationId == DEFAULT_ACTIVITY_INDICATOR_AUTOMATION_ID)
                SetDefaultBasicActivityActivator(obfuscationColor, 80, 10, 2, borderColor, backColor, 50, color, true);
            else
                AddBasicActivityActivator(contentPage, automationId, obfuscationColor, 80, 10, 2, borderColor, backColor, 50, color, true);
        }

        public static void HideDefaultActivityIndicator()
        {
            // NOT NECESSARY TO BE ON MAIN THREAD
            Hide(DEFAULT_ACTIVITY_INDICATOR_AUTOMATION_ID);
        }

    #endregion ACTIVITY INDICATOR RELATED

    #region DISCONNEXION INFORMATION RELATED

        public static void ShowDefaultDisconnectionInformation(String linkedToAutomationId = null)
        {
            // NECESSARY TO BE ON MAIN THREAD
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => ShowDefaultDisconnectionInformation(linkedToAutomationId));
                return;
            }

            MultiPlatformApplication.Controls.CtrlContentPage contentPage = GetCurrentContentPage();

            String id = DEFAULT_DISCONNECTION_INFORMATION_AUTOMATION_ID;
            if (contentPage?.PopupExists(id) != true)
                AddBasicDisconnexionInformationUsingDefaultSettings(null, id);

            Show(id, linkedToAutomationId, LayoutAlignment.Center, false, LayoutAlignment.Center, false, new Point(), -1);
        }

        public static void ShowDisconnectionInformation(String popupAutomationId, String linkedToAutomationId)
        {
            // NOT NECESSARY TO BE ON MAIN THREAD
            Show(popupAutomationId, linkedToAutomationId, LayoutAlignment.Center, false, LayoutAlignment.Center, false, new Point(), -1);
        }

        public static void SetDefaultBasicDisconnexionInformation(Color obfuscationColor, double squareSize, float squareCornerRadius, double squareCornerSize, Color squareCornerColor, Color squareBackgroundColor, double indicatorSize, Color indicatorColor, Boolean useDeviceActivityIndicator)
        {
            // NECESSARY TO BE ON MAIN THREAD
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => SetDefaultBasicDisconnexionInformation(obfuscationColor, squareSize, squareCornerRadius, squareCornerSize, squareCornerColor, squareBackgroundColor, indicatorSize, indicatorColor, useDeviceActivityIndicator));
                return;
            }

            MultiPlatformApplication.Controls.CtrlContentPage contentPage = GetCurrentContentPage();

            if (contentPage != null)
            {
                View popupView = contentPage?.GetPopup(DEFAULT_DISCONNECTION_INFORMATION_AUTOMATION_ID);
                if (popupView == null)
                {
                    AddBasicDisconnexionInformation(contentPage, DEFAULT_DISCONNECTION_INFORMATION_AUTOMATION_ID, obfuscationColor, squareSize, squareCornerRadius, squareCornerSize, squareCornerColor, squareBackgroundColor, indicatorSize, indicatorColor, useDeviceActivityIndicator);
                }
                else
                {
                    if (popupView is ContentView contentView)
                    {
                        popupView.BackgroundColor = obfuscationColor;

                        if (contentView.Content is Frame frame1)
                        {
                            frame1.HeightRequest = squareSize;
                            frame1.WidthRequest = squareSize;
                            frame1.CornerRadius = squareCornerRadius;
                            frame1.BackgroundColor = squareCornerColor;

                            if (frame1.Content is Frame frame2)
                            {
                                frame2.WidthRequest = squareSize - squareCornerSize;
                                frame2.HeightRequest = squareSize - squareCornerSize;
                                frame2.CornerRadius = squareCornerRadius;
                                frame2.BackgroundColor = squareBackgroundColor;

                                // Recreate Content
                                frame2.Content = CreateActivityIndicatorContent(indicatorSize, indicatorColor, useDeviceActivityIndicator);

                                if (useDeviceActivityIndicator)
                                    frame2.Content.SetBinding(ActivityIndicator.IsRunningProperty, new Binding("IsVisible", source: popupView));
                            }
                        }
                    }
                }
            }
        }

        public static void AddBasicDisconnexionInformation(MultiPlatformApplication.Controls.CtrlContentPage contentPage, String automationId, Color obfuscationColor, double width, float cornerRadius, double cornerSize, Color squareCornerColor, Color squareBackgroundColor, double indicatorSize, Color indicatorColor, Boolean useDeviceActivityIndicator)
        {
            // NECESSARY TO BE ON MAIN THREAD
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => AddBasicActivityActivator(contentPage, automationId, obfuscationColor, width, cornerRadius, cornerSize, squareCornerColor, squareBackgroundColor, indicatorSize, indicatorColor, useDeviceActivityIndicator));
                return;
            }

            if (contentPage == null)
                contentPage = GetCurrentContentPage();

            if (contentPage?.PopupExists(automationId) == true)
                return;

            if (String.IsNullOrEmpty(automationId))
                return;

            ContentView contentView = new ContentView
            {
                AutomationId = automationId,
                IsVisible = false,

                Margin = 0,
                Padding = 0,
                BackgroundColor = obfuscationColor
            };



            // Create Frame which contains the activity indicator
            Frame frame1 = new Frame
            {
                Margin = 0,
                Padding = 0,
                HasShadow = false,

                WidthRequest = width,
                HeightRequest = indicatorSize + cornerSize,

                CornerRadius = cornerRadius,
                BackgroundColor = squareCornerColor,

                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            // Set frame1 as the content of the contentView
            contentView.Content = frame1;

            // Create Frame which contains the label and the activity indicator
            Frame frame2 = new Frame
            {
                Margin = 0,
                Padding = 0,
                HasShadow = false,

                WidthRequest = width - cornerSize,
                HeightRequest = indicatorSize,

                CornerRadius = cornerRadius,
                BackgroundColor = squareBackgroundColor,

                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            // Set frame2 as the content of the frame1
            frame1.Content = frame2;

            StackLayout stackLayout = new StackLayout
            {
                Margin = 0,
                Padding = 0,
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };

            frame2.Content = stackLayout;

            Color backColor = Helper.GetResourceDictionaryById<Color>("ColorEntryBackground");
            Color color = Helper.GetResourceDictionaryById<Color>("ColorMain");
            Color borderColor = Helper.GetResourceDictionaryById<Color>("ColorEntryPlaceHolder");

            Label label = new Label
            {
                Margin = new Thickness(0, 0, 20, 0),
                Padding = 0,

                Text = Helper.SdkWrapper.GetLabel("noNetwork").Replace("<br/>", "\r\n"),
                TextColor = indicatorColor,

                MaxLines = 2,

                FontSize = Helper.GetResourceDictionaryById<double>("FontSizeMicro"),
                FontAttributes = FontAttributes.None,

                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center,

                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
            };
            stackLayout.Children.Add(label);

            // Create activity indicator
            View activityIndicator = CreateActivityIndicatorContent(indicatorSize, indicatorColor, useDeviceActivityIndicator);

            if (useDeviceActivityIndicator)
                activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, new Binding("IsVisible", source: contentView));

            // Set the activityIndicator as content of the frame
            stackLayout.Children.Add(activityIndicator);

            contentPage.AddViewAsPopupInternal(contentView);
            SetType(contentView, PopupType.Information);
        }

        public static void AddBasicDisconnexionInformationUsingDefaultSettings(MultiPlatformApplication.Controls.CtrlContentPage contentPage, String automationId)
        {
            // NECESSARY TO BE ON MAIN THREAD
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => AddBasicDisconnexionInformationUsingDefaultSettings(contentPage, automationId));
                return;
            }

            Color backColor = Color.Black;
            Color color = Color.White;
            Color borderColor = Color.White;

            String colorHex = Helper.GetResourceDictionaryById<Color>("ColorMainOnOver").ToHex();
            colorHex = "#7F" + colorHex.Substring(3);
            Color obfuscationColor = Color.FromHex(colorHex);

            if (automationId == DEFAULT_DISCONNECTION_INFORMATION_AUTOMATION_ID)
                SetDefaultBasicDisconnexionInformation(obfuscationColor, 300, 10, 2, borderColor, backColor, 50, color, true);
            else
                AddBasicDisconnexionInformation(contentPage, automationId, obfuscationColor, 360, 10, 2, borderColor, backColor, 50, color, true);
        }

        public static void HideDefaultBasicDisconnexionInformation()
        {
            // NOT NECESSARY TO BE ON MAIN THREAD
            Hide(DEFAULT_DISCONNECTION_INFORMATION_AUTOMATION_ID);
        }

    #endregion DISCONNEXION INFORMATION RELATED

        public static void Add(MultiPlatformApplication.Controls.CtrlContentPage contentPage, View view, PopupType type)
        {
            // NECESSARY TO BE ON MAIN THREAD
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => Add(contentPage, view, type));
                return;
            }

            if (contentPage == null)
                contentPage = GetCurrentContentPage();

            if (contentPage != null)
            {
                if (!String.IsNullOrEmpty(view.AutomationId))
                {
                    if (!contentPage.PopupExists(view.AutomationId))
                    {
                        contentPage.AddViewAsPopupInternal(view);
                        SetType(view, type);
                    }
                }
            }
        }

        public static void Remove(MultiPlatformApplication.Controls.CtrlContentPage contentPage, View view)
        {
            // NECESSARY TO BE ON MAIN THREAD
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => Remove(contentPage, view));
                return;
            }
            contentPage.RemoveViewAsPopupInternal(view);
        }

        public static void Remove(MultiPlatformApplication.Controls.CtrlContentPage contentPage, String automationId)
        {
            // NECESSARY TO BE ON MAIN THREAD
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => Remove(contentPage, automationId));
                return;
            }

            View view = GetView(automationId, contentPage);
            if(view != null)
                contentPage.RemoveViewAsPopupInternal(view);
        }

        public static void Show(String popupAutomationId, String linkedToAutomationId)
        {
            // NOT NECESSARY TO BE ON MAIN THREAD
            Show(popupAutomationId, linkedToAutomationId, -1);
        }

        public static void Show(String popupAutomationId, String linkedToAutomationId, int delay)
        {
            // NOT NECESSARY TO BE ON MAIN THREAD
            Show(popupAutomationId, linkedToAutomationId, LayoutAlignment.Start, false, LayoutAlignment.End, true, new Point(), delay);
        }

        public static void Show(String popupAutomationId, Rect rect)
        {
            // NOT NECESSARY TO BE ON MAIN THREAD
            Show(popupAutomationId, rect, -1);
        }

        public static void Show(String popupAutomationId, Rect rect, int delay)
        {
            // NOT NECESSARY TO BE ON MAIN THREAD
            Show(popupAutomationId, rect, LayoutAlignment.Start, false, LayoutAlignment.End, true, new Point(), delay);
        }

        public static void Show(String popupAutomationId, String linkedToAutomationId, LayoutAlignment horizontalLayoutAlignment = LayoutAlignment.Start, Boolean outsideRectHorizontally = false, LayoutAlignment verticalLayoutAlignment = LayoutAlignment.End, Boolean outsideRectVertically = true, Point translation = default, int delay = -1)
        {
            // NOT NECESSARY TO BE ON MAIN THREAD
            PopupAction popupAction = new PopupAction
            {
                PopupAutomationId = popupAutomationId,
                LinkedToAutomationId = linkedToAutomationId,
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
            // NOT NECESSARY TO BE ON MAIN THREAD
            PopupAction popupAction = new PopupAction
            {
                PopupAutomationId = popupAutomationId,
                LinkedToAutomationId = null,
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

        private static View CreateActivityIndicatorContent(double indicatorSize, Color indicatorColor, Boolean useDeviceActivityIndicator)
        {
            // Create activity indicator
            View activityIndicator;
            if (useDeviceActivityIndicator)
            {
                activityIndicator = new ActivityIndicator { Color = indicatorColor };
            }
            else
            {
                FontImageSource fontImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesomeSolid5",
                    Color = indicatorColor,
                    Glyph = Helper.GetGlyph("Spinner"),
                    Size = (double)indicatorSize
                };

                activityIndicator = new Image { Source = fontImageSource };
            }

            // Set common properties
            activityIndicator.Margin = 0;
            activityIndicator.BackgroundColor = Color.Transparent;
            activityIndicator.WidthRequest = indicatorSize;
            activityIndicator.HeightRequest = indicatorSize;
            activityIndicator.HorizontalOptions = LayoutOptions.Center;
            activityIndicator.VerticalOptions = LayoutOptions.Center;

            return activityIndicator;
        }

        private static void TouchEffect_TouchAction(object sender, TouchActionEventArgs e)
        {
            if (e.Type == TouchActionType.Pressed)
            {
                HideCurrentContextMenu();
            };
        }

        private static void PrepareToShow(PopupAction newPopupAction)
        {
            // NECESSARY TO BE ON MAIN THREAD
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => PrepareToShow(newPopupAction));
                return;
            }

            ContentPage contentPage = GetCurrentContentPage();
            View view = GetView(newPopupAction.PopupAutomationId, contentPage);
            if (view != null)
            {
                // Get PopupType
                newPopupAction.PopupType = GetType(view);
                newPopupAction.Date = DateTime.UtcNow;

                // Check if this popup is known
                String id = view.Id.ToString();
                if (!popupList.ContainsKey(id))
                    return;

                if (newPopupAction.PopupType != PopupType.ContextMenu)
                {
                    // Store the popup action
                    popupAction = newPopupAction;

                    ContentPageTapCommand(null);
                }
                else
                {
                    PopupAction currentContextMenuDisplayed = previousContextMenuActionDisplayed;
                    HideCurrentContextMenu();

                    // Se store the popup type
                    newPopupAction.PopupType = popupList[id];

                    // If we ask to show the context menu currently displayed, we hide it instead
                    if (currentContextMenuDisplayed?.PopupAutomationId == newPopupAction.PopupAutomationId)
                    {
                        // We also need to check if it's the same place or not linked to the same element
                        if (((currentContextMenuDisplayed.LinkedToAutomationId == newPopupAction.LinkedToAutomationId) && (!String.IsNullOrEmpty(newPopupAction.LinkedToAutomationId)))
                            || ((currentContextMenuDisplayed.Rect == newPopupAction.Rect) && (!newPopupAction.Rect.IsEmpty)))
                        {
                            TimeSpan duration = newPopupAction.Date - currentContextMenuDisplayed.Date;
                            if (duration.TotalMilliseconds < 250)
                            {
                                previousContextMenuActionDisplayed = null;
                                return;
                            }
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
            Page page = ((App)Xamarin.Forms.Application.Current).NavigationService.CurrentPage;

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

        internal static void UpdateActivityIndicators()
        {
            if(activityIndicatorList.Count > 0)
            {
                foreach (PopupAction popupAction in activityIndicatorList.Values)
                    ProcessPositionAndShowPopup(popupAction);
            }
        }

        private static void ProcessPositionAndShowPopup(PopupAction newPopupAction)
        {
            if (newPopupAction == null)
                return;

            // NECESSARY TO BE ON MAIN THREAD
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => ProcessPositionAndShowPopup(newPopupAction));
                return;
            }

            // Get ContentPage
            MultiPlatformApplication.Controls.CtrlContentPage contentPage = GetCurrentContentPage();

            // Get the View of the popup
            View view = GetView(newPopupAction.PopupAutomationId, contentPage);
            if (view == null)
                return;

            // Get Rect of the linked element (if any)
            if (!String.IsNullOrEmpty(newPopupAction.LinkedToAutomationId))
                newPopupAction.Rect = GetRectOfView(contentPage, newPopupAction.LinkedToAutomationId);

            // If no Rect is defined, we use the Content Page Bounds
            if (newPopupAction.Rect.IsEmpty)
                newPopupAction.Rect = contentPage.Bounds;

            if (popupAction.PopupType != PopupType.ContextMenu)
            {
                if (String.IsNullOrEmpty(newPopupAction.LinkedToAutomationId))
                {
                    // Set X and Y Constraints
                    RelativeLayout.SetXConstraint(view, Constraint.Constant(0));
                    RelativeLayout.SetYConstraint(view, Constraint.Constant(0));

                    // Set Width and HeightConstraints
                    RelativeLayout.SetWidthConstraint(view, Constraint.RelativeToParent((rl) => { return rl.Width; }));
                    RelativeLayout.SetHeightConstraint(view, Constraint.RelativeToParent((rl) => { return rl.Height; }));
                }
                else
                {
                    Rect rect = GetRectOfView(newPopupAction.LinkedToAutomationId);

                    // Set X and Y Constraints
                    RelativeLayout.SetXConstraint(view, Constraint.Constant(rect.X));
                    RelativeLayout.SetYConstraint(view, Constraint.Constant(rect.Y));

                    // Set Width and HeightConstraints
                    RelativeLayout.SetWidthConstraint(view, Constraint.Constant(rect.Width));
                    RelativeLayout.SetHeightConstraint(view, Constraint.Constant(rect.Height));

                    // Add to the list of Activity Indicator
                    if (!activityIndicatorList.ContainsKey(popupAction.PopupAutomationId))
                        activityIndicatorList.Add(popupAction.PopupAutomationId, popupAction);
                }

                if(popupAction.PopupType == PopupType.Information)
                {
                    // We display an Information popup. Do we have to hide it after a delay ?
                    if (newPopupAction.Delay > 0)
                    {
                        Device.StartTimer(TimeSpan.FromMilliseconds(newPopupAction.Delay), () =>
                        {
                            HideInternal(newPopupAction.PopupAutomationId); return false;
                        });
                    }
                }

                // Show popup
                view.IsVisible = true;
                contentPage.GetRelativeLayout().RaiseChild(view);
            }
            else
            {
                // Calculate the new position of the Popup
                double X = 0, Y = 0;

                X = GetXPosition(contentPage, view, newPopupAction.HorizontalLayoutAlignment, newPopupAction.Rect, newPopupAction.OutsideRectHorizontally, popupAction.Translation.X);
                Y = GetYPosition(contentPage, view, newPopupAction.VerticalLayoutAlignment, newPopupAction.Rect, newPopupAction.OutsideRectVertically, popupAction.Translation.Y);

                // Are we dealing with a Context Menu ?
                if (newPopupAction.PopupType == PopupType.ContextMenu)
                {
                    // Hide previous context menu - if any (only one can be display in same time)
                    HideInternal(previousContextMenuActionDisplayed?.PopupAutomationId);

                    // Store context menu
                    previousContextMenuActionDisplayed = newPopupAction;
                }

                // Set X and Y Constraints
                RelativeLayout.SetXConstraint(view, Constraint.RelativeToParent((rl) => X));
                RelativeLayout.SetYConstraint(view, Constraint.RelativeToParent((rl) => Y));

                // Show popup
                view.IsVisible = true;
                contentPage.GetRelativeLayout().RaiseChild(view);
            }
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

            // NECESSARY TO BE ON MAIN THREAD
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => HideInternal(popupAutomationId));
                return;
            }

            // If we add a known activity indicator, remove it from the list
            if (activityIndicatorList.ContainsKey(popupAutomationId))
                activityIndicatorList.Remove(popupAutomationId);


            MultiPlatformApplication.Controls.CtrlContentPage contentPage = GetCurrentContentPage();
            View view = GetView(popupAutomationId, contentPage);

            if (view != null)
            {
                if (view.IsVisible)
                {
                    Boolean needToHide = true;

                    if (popupAutomationId == previousContextMenuActionDisplayed?.PopupAutomationId)
                    {
                        if (previousContextMenuActionDisplayed.Date != DateTime.MinValue)
                        {
                            TimeSpan duration = DateTime.UtcNow - previousContextMenuActionDisplayed.Date;
                            needToHide = (duration.TotalMilliseconds > 100);
                        }
                        else
                            needToHide = false;
                    }

                    if (needToHide)
                    {
                        view.IsVisible = false;
                        //contentPage.GetRelativeLayout().LowerChild(view);
                    }
                }
                else
                {

                }
            }
        }

        private static void ContentPageTapCommand(object obj)
        {
            if (popupAction == null)
                HideCurrentContextMenu();
            else
            {
                ProcessPositionAndShowPopup(popupAction);
                popupAction = null;
            }
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
            public String LinkedToAutomationId;

            public PopupType PopupType = PopupType.Unknown;

            public Rect Rect;

            public Boolean OutsideRectVertically;
            public Boolean OutsideRectHorizontally;

            public LayoutAlignment HorizontalLayoutAlignment;
            public LayoutAlignment VerticalLayoutAlignment;

            public Point Translation;
            public int Delay;

            public DateTime Date;
        }

    }

    public enum PopupType
    {
        ActivityIndicator,
        ContextMenu,
        Information,
        Unknown
    }


}
