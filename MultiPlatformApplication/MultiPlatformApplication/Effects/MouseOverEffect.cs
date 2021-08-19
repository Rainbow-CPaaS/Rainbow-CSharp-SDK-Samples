using MultiPlatformApplication.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MultiPlatformApplication.Effects
{
    public static class MouseOverEffect
    {
        public static readonly BindableProperty CommandProperty = BindableProperty.Create("Command", typeof(ICommand), typeof(MouseOverEffect), null, propertyChanged: OnhandlerChanged);


        public static ICommand GetCommand(BindableObject view)
        {
            return (ICommand)view.GetValue(CommandProperty);
        }

        public static void SetCommand(BindableObject view, ICommand value)
        {
            view.SetValue(CommandProperty, value);
        }

        static void OnhandlerChanged(BindableObject bindable, object oldValue, object newValue)
        {
            // Useful only in Desktop platforms
            if (!Helper.IsDesktopPlatform())
                return;

            var view = bindable as View;
            if (view == null)
                return;

            ICommand command = (ICommand)newValue;
            if (command != null)
                Helper.AddEffect(view, new ControlMouseOverEffect());
            else
                Helper.RemoveEffect(view, typeof(ControlMouseOverEffect));

        }

        class ControlMouseOverEffect : RoutingEffect
        {
            public ControlMouseOverEffect() : base($"MultiPlatformApplication.CrossEffects.{nameof(MouseOverEffect)}")
            {

            }
        }
    }
}
