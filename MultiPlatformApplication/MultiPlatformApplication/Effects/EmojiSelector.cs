using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace MultiPlatformApplication.Effects
{
    public class EmojiSelector
    {
#region  EntryName Property

        public static readonly BindableProperty EntryNameProperty = BindableProperty.Create("EntryName", typeof(String), typeof(Background), null, propertyChanged: OnEntryNameChanged);

        public static String GetEntryName(BindableObject view)
        {
            return (String)view.GetValue(EntryNameProperty);
        }

        public static void SetEntryName(BindableObject view, String value)
        {
            view.SetValue(EntryNameProperty, value);
        }

        static void OnEntryNameChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = bindable as View;
            if (view == null)
                return;

            // This element is visible only in UWP Context
            if(Device.RuntimePlatform != Device.UWP)
            {
                view.IsVisible = false;
                return;
            }

            String entryName = (String)newValue;
            if (!String.IsNullOrEmpty(entryName))
            {
                view.Effects.Add(new ControlEmojiSelectorEffect());
            }
            else
            {
                var toRemove = view.Effects.FirstOrDefault(e => e is ControlEmojiSelectorEffect);
                if (toRemove != null)
                {
                    view.Effects.Remove(toRemove);
                }
            }
        }

#endregion  EntryName Property


        public static View OnPointerReleased(BindableObject bindable)
        {
            var view = bindable as View;
            if (view == null)
                return null;

            String name = GetEntryName(bindable);

            if (String.IsNullOrEmpty(name))
                return null;

            Element parent = view;
            View entry = null;
            while ( (view.Parent is VisualElement) && (entry == null))
            {
                parent = parent.Parent;

                try
                {
                    entry = parent.FindByName<View>(name);
                }
                catch
                {

                }

                // Don't go deeper than Content Page
                if (parent is ContentPage)
                    break;
            }

            return entry;
        }

        class ControlEmojiSelectorEffect : RoutingEffect
        {
            public ControlEmojiSelectorEffect() : base($"MultiPlatformApplication.CrossEffects.{nameof(EmojiSelector)}")
            {

            }
        }
    }
}
