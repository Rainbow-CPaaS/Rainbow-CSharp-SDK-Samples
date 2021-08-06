using MultiPlatformApplication.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace MultiPlatformApplication.Effects
{
    public class ContextMenu
    {

#region  Enabled Property

        public static readonly BindableProperty EnabledProperty = BindableProperty.Create("Enabled", typeof(Boolean), typeof(ContextMenu), false, propertyChanged: OnEnabledChanged);

        public static Boolean GetEnabled(BindableObject view)
        {
            return (Boolean)view.GetValue(EnabledProperty);
        }

        public static void SetEnabled(BindableObject view, Boolean value)
        {
            view.SetValue(EnabledProperty, value);
        }

        static void OnEnabledChanged(BindableObject bindable, object oldValue, object newValue)
        {
            CheckEffect(bindable);
        }

#endregion  Enabled Property



#region  LinkedTo Property

        public static readonly BindableProperty LinkedToProperty = BindableProperty.Create("LinkedTo", typeof(String), typeof(ContextMenu), null, propertyChanged: OnLinkedToChanged);

        public static String GetLinkedTo(BindableObject view)
        {
            return (String)view.GetValue(LinkedToProperty);
        }

        public static void SetLinkedTo(BindableObject view, String value)
        {
            view.SetValue(LinkedToProperty, value);
        }

        static void OnLinkedToChanged(BindableObject bindable, object oldValue, object newValue)
        {
            CheckEffect(bindable);
        }

#endregion  LinkedTo Property


#region  OnLeft Property

        public static readonly BindableProperty OnLeftProperty = BindableProperty.Create("OnLeft", typeof(Boolean ?), typeof(ContextMenu), null, propertyChanged: OnOnLeftChanged);

        public static Boolean ? GetOnLeft(BindableObject view)
        {
            return (Boolean ?)view.GetValue(OnLeftProperty);
        }

        public static void SetOnLeft(BindableObject view, Boolean ? value)
        {
            view.SetValue(OnLeftProperty, value);
        }

        static void OnOnLeftChanged(BindableObject bindable, object oldValue, object newValue)
        {
            CheckEffect(bindable);
        }

#endregion  OnLeft Property


#region  OnTop Property

        public static readonly BindableProperty OnTopProperty = BindableProperty.Create("OnTop", typeof(Boolean ?), typeof(ContextMenu), null, propertyChanged: OnOnTopChanged);

        public static Boolean ? GetOnTop(BindableObject view)
        {
            return (Boolean ?)view.GetValue(OnTopProperty);
        }

        public static void SetOnTop(BindableObject view, Boolean ? value)
        {
            view.SetValue(OnTopProperty, value);
        }

        static void OnOnTopChanged(BindableObject bindable, object oldValue, object newValue)
        {
            CheckEffect(bindable);
        }

#endregion  OnTop Property

        public static void CheckEffect(BindableObject bindable)
        {
            var view = bindable as View;
            if (view == null)
                return;

            if (GetEnabled(bindable))
            {
                //view.StyleId = "ContextMenu";
                view.IsVisible = false;

                Helper.AddEffect(view, new ControlContextMenuEffect());
            }
            else
                Helper.RemoveEffect(view, typeof(ControlContextMenuEffect));
        }

        public static void Hide(BindableObject bindable)
        {
            var view = bindable as View;
            if (view == null)
                return;

            Frame contextMenuBackground = GetContextMenuBackgroundElement(bindable);
            if(contextMenuBackground != null)
                contextMenuBackground.IsVisible = false;

            view.IsVisible = false;
        }

        public static void Display(BindableObject bindable)
        {
            var view = bindable as View;
            if (view == null)
                return;

            if (!GetEnabled(bindable))
            {
                view.IsVisible = false;
                return;
            }

            RelativeLayout relativeLayout = view.Parent as RelativeLayout;
            if (relativeLayout == null)
                return;

            String linkedTo = GetLinkedTo(bindable);
            if (!String.IsNullOrEmpty(linkedTo))
            {
                VisualElement linkedToElement = relativeLayout.FindByName<VisualElement>(linkedTo);
                if (linkedToElement != null)
                {
                    Rectangle rect = GetRectValue(view, linkedToElement);

                    Boolean? onLeft = GetOnLeft(bindable);
                    Boolean? onTop = GetOnTop(bindable);

                    if (onLeft != null)
                    {
                        RelativeLayout.SetXConstraint(view, Constraint.RelativeToParent((rl) =>
                        {
                            double X;

                            if (onLeft == true)
                                X = rect.X;
                            else
                                X = (rect.X + rect.Width) - view.Width;

                        // Avoid to have context menu truncated
                        if (X < 0)
                                X = 2;
                            else if ((X + view.Width) > relativeLayout.Width)
                                X = relativeLayout.Width - view.Width - 2;

                            return X;
                        }));
                    }
                    if (onTop != null)
                    {

                        RelativeLayout.SetYConstraint(view, Constraint.RelativeToParent((rl) =>
                        {
                            double Y;
                            if (onTop == true)
                                Y = rect.Y - view.Height;
                            else
                                Y = rect.Y + rect.Height;

                            if (Y < 0)
                                Y = 2;
                            else if ((Y + view.Height) > relativeLayout.Height)
                                Y = relativeLayout.Height - view.Height - 2;

                            return Y;
                        }));
                    }
                }
            }

            Frame contextMenuBackground = GetContextMenuBackgroundElement(bindable);
            contextMenuBackground.IsVisible = true;


            // Display context menu "on the front"
            relativeLayout.RaiseChild(view);

        }

        public static Frame GetContextMenuBackgroundElement(BindableObject bindable)
        {
            var view = bindable as View;
            if (view == null)
                return null;

            RelativeLayout relativeLayout = view.Parent as RelativeLayout;
            if (relativeLayout == null)
                return null;


            View result = relativeLayout.Children.FirstOrDefault<View>((v) => (v.StyleId == "ContextMenuBackground"));
            Frame contextMenuBackground;

            if (result == null)
            {
                // Create and add context menu background to the relative layout
                contextMenuBackground = new Frame() { StyleId = "ContextMenuBackground", Margin = 0, Padding = 0, BackgroundColor=Color.LightGray, Opacity=0.5, IsVisible = false };

                relativeLayout.Children.Add(contextMenuBackground, null, null, 
                                            Constraint.RelativeToParent((rl) =>
                                            {
                                                return rl.Width;
                                            }),
                                            Constraint.RelativeToParent((rl) =>
                                            {
                                                return rl.Height;
                                            })
                );

            }
            else
            {
                contextMenuBackground = (Frame)result;
            }
            return contextMenuBackground;
        }

        private static Rectangle GetRectValue(Element contextMenu, VisualElement linkedToElement)
        {
            Rectangle rect = new Rectangle();

            if (linkedToElement != null)
            {
                rect.Width = linkedToElement.Width;
                rect.Height = linkedToElement.Height;

                rect.X = linkedToElement.X;
                rect.Y = linkedToElement.Y;

                var parent = (VisualElement)linkedToElement.Parent;
                while (parent != null)
                {
                    rect.X += parent.X;
                    rect.Y += parent.Y;
                    if (parent.Parent == contextMenu.Parent)
                        break;

                    if (!(parent.Parent is VisualElement))
                        break;

                    parent = (VisualElement)parent.Parent;
                }
            }

            return rect;
        }

        class ControlContextMenuEffect : RoutingEffect
        {
            public ControlContextMenuEffect() : base($"MultiPlatformApplication.CrossEffects.{nameof(ContextMenu)}")
            {

            }
        }

    }
}
