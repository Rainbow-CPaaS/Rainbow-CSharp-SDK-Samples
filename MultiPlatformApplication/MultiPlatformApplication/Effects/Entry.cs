using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace MultiPlatformApplication.Effects
{
    public class Entry
    {
#region  NoBorder Property

        public static readonly BindableProperty NoBorderProperty = BindableProperty.Create("NoBorder", typeof(Boolean), typeof(Background), false, propertyChanged: OnNoBorderChanged);

        public static Boolean GetNoBorder(BindableObject view)
        {
            return (Boolean)view.GetValue(NoBorderProperty);
        }

        public static void SetNoBorder(BindableObject view, Boolean value)
        {
            view.SetValue(NoBorderProperty, value);
        }

        static void OnNoBorderChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = bindable as View;
            if (view == null)
                return;

            Boolean NoBorder = (Boolean)newValue;
            if (NoBorder)
                AddEffect(view);
        }

#endregion  NoBorder Property


#region  NoTintColored Property

        public static readonly BindableProperty NoTintColoredProperty = BindableProperty.Create("NoTintColored", typeof(Boolean), typeof(Background), false, propertyChanged: OnNoTintColoredChanged);

        public static Boolean GetNoTintColored(BindableObject view)
        {
            return (Boolean)view.GetValue(NoTintColoredProperty);
        }

        public static void SetNoTintColored(BindableObject view, Boolean value)
        {
            view.SetValue(NoTintColoredProperty, value);
        }

        static void OnNoTintColoredChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = bindable as View;
            if (view == null)
                return;


            Boolean NoTintColored = (Boolean)newValue;
            if (NoTintColored)
                AddEffect(view);
        }

#endregion  NoTintColored Property

        private static void AddEffect(View view)
        {
            RemoveEffect(view);
            view.Effects.Add(new ControlEntryEffect());
        }

        private static void RemoveEffect(View view)
        {
            if (view == null)
                return;

            var toRemove = view.Effects.FirstOrDefault(e => e is ControlEntryEffect);
            if (toRemove != null)
                view.Effects.Remove(toRemove);
        }

        class ControlEntryEffect : RoutingEffect
        {
            public ControlEntryEffect() : base($"MultiPlatformApplication.CrossEffects.{nameof(Entry)}")
            {

            }
        }
    }
}
