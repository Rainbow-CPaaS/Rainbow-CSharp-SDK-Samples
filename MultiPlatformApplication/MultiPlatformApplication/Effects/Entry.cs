using MultiPlatformApplication.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MultiPlatformApplication.Effects
{
    public class Entry
    {

#region  ValidationCommand Property

        public static readonly BindableProperty ValidationCommandProperty = BindableProperty.Create("ValidationCommand", typeof(ICommand), typeof(Entry), null, propertyChanged: OnValidationCommandChanged);


        public static ICommand GetValidationCommand(BindableObject view)
        {
            return (ICommand)view.GetValue(ValidationCommandProperty);
        }

        public static void SetValidationCommand(BindableObject view, ICommand value)
        {
            view.SetValue(ValidationCommandProperty, value);
        }

        static void OnValidationCommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            // USEFULL ONLY IN UWP context - Could be also added in WPF and MacOS
            if (Device.RuntimePlatform != Device.UWP)
                return;

            var view = bindable as View;
            if (view == null)
                return;

            ICommand command = (ICommand)newValue;
            if (command != null)
                Helper.AddEffect(view, new ControlEntryEffect());
        }

#endregion ValidationCommand Property

#region  ValidationKeyModifier Property

        public static readonly BindableProperty ValidationKeyModifierProperty = BindableProperty.Create("ValidationKeyModifier", typeof(String), typeof(Entry), null, propertyChanged: OnValidationKeyModifierChanged);

        public static String GetValidationKeyModifier(BindableObject view)
        {
            String value = (String)view.GetValue(ValidationKeyModifierProperty);
            if(value!= null)
                return value.ToLower();
            return null;
        }

        public static void SetValidationKeyModifier(BindableObject view, String value)
        {
            view.SetValue(ValidationKeyModifierProperty, value);
        }

        static void OnValidationKeyModifierChanged(BindableObject bindable, object oldValue, object newValue)
        {
            // USEFULL ONLY IN UWP context - Could be also added in WPF and MacOS
            if (Device.RuntimePlatform != Device.UWP)
                return;

            var view = bindable as View;
            if (view == null)
                return;

            String ValidationKeyModifier = (String)newValue;
            if (ValidationKeyModifier != null)
                Helper.AddEffect(view, new ControlEntryEffect());
        }

#endregion  ValidationKeyModifier Property


#region  NoBorder Property

        // No border implementation: (Android, iOs, UWP)
        //      https://alexdunn.org/2017/03/08/xamarin-forms-borderless-entry/
        //      https://stackoverflow.com/questions/53095961/how-to-remove-the-border-from-a-entry-control-with-xamarin-forms

        public static readonly BindableProperty NoBorderProperty = BindableProperty.Create("NoBorder", typeof(Boolean), typeof(Entry), false, propertyChanged: OnNoBorderChanged);

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
                Helper.AddEffect(view, new ControlEntryEffect());
        }

#endregion  NoBorder Property


#region  NoTintColored Property

        public static readonly BindableProperty NoTintColoredProperty = BindableProperty.Create("NoTintColored", typeof(Boolean), typeof(Entry), false, propertyChanged: OnNoTintColoredChanged);

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
                Helper.AddEffect(view, new ControlEntryEffect());
        }

#endregion  NoTintColored Property

        class ControlEntryEffect : RoutingEffect
        {
            public ControlEntryEffect() : base($"MultiPlatformApplication.CrossEffects.{nameof(Entry)}")
            {

            }
        }
    }
}
