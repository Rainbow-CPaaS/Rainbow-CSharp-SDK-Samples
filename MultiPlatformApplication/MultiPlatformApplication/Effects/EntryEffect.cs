﻿using MultiPlatformApplication.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MultiPlatformApplication.Effects
{
    public class EntryEffect : RoutingEffect
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
            // Usefull only in desktop context
            if (!Helper.IsDesktopPlatform())
                return;

            var view = bindable as View;
            if (view == null)
                return;

            ICommand command = (ICommand)newValue;
            if (command != null)
                Helper.AddEffect(view, new EntryEffect());
        }

#endregion ValidationCommand Property

#region  BreakLineModifier Property

        public static readonly BindableProperty BreakLineModifierProperty = BindableProperty.Create("BreakLineModifier", typeof(String), typeof(Entry), null, propertyChanged: OnBreakLineModifierChanged);

        public static String GetBreakLineModifier(BindableObject view)
        {
            String value = (String)view.GetValue(BreakLineModifierProperty);
            if(value!= null)
                return value.ToLower();
            return null;
        }

        public static void SetBreakLineModifier(BindableObject view, String value)
        {
            view.SetValue(BreakLineModifierProperty, value);
        }

        static void OnBreakLineModifierChanged(BindableObject bindable, object oldValue, object newValue)
        {
            // Usefull only in desktop context
            if (!Helper.IsDesktopPlatform())
                return;

            var view = bindable as View;
            if (view == null)
                return;

            String BreakLineModifier = (String)newValue;
            if (BreakLineModifier != null)
                Helper.AddEffect(view, new EntryEffect());
        }

#endregion  BreakLineModifier Property

#region  NoBorder Property

        // No border implementation: (Android, iOs, UWP)
        //      https://alexdunn.org/2017/03/08/xamarin-forms-borderless-entry/
        //      https://stackoverflow.com/questions/53095961/how-to-remove-the-border-from-a-entry-control-with-xamarin-forms
        //      
        //      https://github.com/xamarin/XamarinCommunityToolkit/tree/main/src/CommunityToolkit/Xamarin.CommunityToolkit/Effects/RemoveBorder

        public static readonly BindableProperty NoBorderProperty = BindableProperty.Create("NoBorder", typeof(Boolean ?), typeof(Entry), null, propertyChanged: OnNoBorderChanged);

        public static Boolean ? GetNoBorder(BindableObject view)
        {
            return (Boolean ?)view.GetValue(NoBorderProperty);
        }

        public static void SetNoBorder(BindableObject view, Boolean ? value)
        {
            view.SetValue(NoBorderProperty, value);
        }

        static void OnNoBorderChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = bindable as View;
            if (view == null)
                return;

            Boolean ? NoBorder = (Boolean ?)newValue;
            if (NoBorder != null)
                Helper.AddEffect(view, new EntryEffect());
        }

#endregion  NoBorder Property

        public event EventHandler<Rect> KeyboardRectChanged;

        public void OnKeyboardRectChanged(Rect rect)
        {
            KeyboardRectChanged?.Invoke(this, rect);
        }

        public EntryEffect() : base("MultiPlatformApplication.CrossEffects.EntryEffect")
        {
        }
    }
}
