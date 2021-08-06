using MultiPlatformApplication.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MultiPlatformApplication.Effects
{
    public static class SelectableLabelEffect
    {
        public static readonly BindableProperty EnabledProperty = BindableProperty.Create("Enabled", typeof(Boolean), typeof(SelectableLabelEffect), false, propertyChanged: OnEnabledChanged);


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
            var view = bindable as View;
            if (view == null)
                return;

            if (newValue != null)
            {
                Boolean enabled = (Boolean)newValue;
                if(enabled)
                    Helper.AddEffect(view, new ControlSelectableLabelEffect());
                else
                    Helper.RemoveEffect(view, typeof(ControlSelectableLabelEffect));
            }
        }

        class ControlSelectableLabelEffect : RoutingEffect
        {
            public ControlSelectableLabelEffect() : base($"MultiPlatformApplication.CrossEffects.{nameof(SelectableLabelEffect)}")
            {

            }
        }
    }
}
