using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MultiPlatformApplication.Effects
{
    public static class MouseOutEffect
    {
        public static readonly BindableProperty CommandProperty = BindableProperty.Create("Command", typeof(ICommand), typeof(MouseOutEffect), null, propertyChanged: OnhandlerChanged);


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
            // USEFULL ONLY IN UWP context - Could be also adde in WPF
            if (Device.RuntimePlatform != Device.UWP)
                return;

            // TODO: perhaps could be usefull in Chrome book context too

            var view = bindable as View;
            if (view == null)
                return;

            ICommand command = (ICommand)newValue;
            if (command != null)
            {
                view.Effects.Add(new ControlMouseOutEffect());
            }
            else
            {
                var toRemove = view.Effects.FirstOrDefault(e => e is ControlMouseOutEffect);
                if (toRemove != null)
                {
                    view.Effects.Remove(toRemove);
                }
            }

        }

        class ControlMouseOutEffect : RoutingEffect
        {
            public ControlMouseOutEffect() : base($"MultiPlatformApplication.CrossEffects.{nameof(MouseOutEffect)}")
            {

            }
        }
    }
}
