using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace MultiPlatformApplication.Effects
{
    public class Background
    {
#region  Color Property

        public static readonly BindableProperty ColorProperty = BindableProperty.Create("Color", typeof(Color), typeof(Background), Color.Transparent, propertyChanged: OnColorChanged);

        public static Color GetColor(BindableObject view)
        {
            return (Color)view.GetValue(ColorProperty);
        }

        public static void SetColor(BindableObject view, Color value)
        {
            view.SetValue(ColorProperty, value);
        }

        private static void OnColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SetBackgroundColor(bindable);
        }

#endregion  Color Property


#region  Enabled Property

        public static readonly BindableProperty EnabledProperty = BindableProperty.Create("Enabled", typeof(Boolean), typeof(Background), false, propertyChanged: OnEnabledChanged);

        public static Boolean GetEnabled(BindableObject view)
        {
            return (Boolean)view.GetValue(EnabledProperty);
        }

        public static void SetEnabled(BindableObject view, Boolean value)
        {
            view.SetValue(EnabledProperty, value);
        }

        private static void OnEnabledChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SetBackgroundColor(bindable);
        }

#endregion  Enabled Property

        private static void SetBackgroundColor(BindableObject bindable)
        {
            var view = bindable as View;
            if (view == null)
                return;

            if (Background.GetEnabled(bindable))
                view.BackgroundColor = Background.GetColor(bindable);
            else
                view.BackgroundColor = Color.Transparent;

        }

    }
}
