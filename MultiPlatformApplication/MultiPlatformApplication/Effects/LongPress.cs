using MultiPlatformApplication.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MultiPlatformApplication.Effects
{
    public class LongPress
    {
        // Based on :
        //      - https://stackoverflow.com/questions/56992033/xamarin-gesturerecognizer-while-holding
        //      - https://www.c-sharpcorner.com/article/longpress-event-for-image/

#region  LongPressCommand Property

        public static readonly BindableProperty LongPressCommandProperty = BindableProperty.Create("LongPressCommand", typeof(ICommand), typeof(LongPress), null, propertyChanged: OnLongPressCommandChanged);


        public static ICommand GetLongPressCommand(BindableObject view)
        {
            return (ICommand)view.GetValue(LongPressCommandProperty);
        }

        public static void SetLongPressCommand(BindableObject view, ICommand value)
        {
            view.SetValue(LongPressCommandProperty, value);
        }

        static void OnLongPressCommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = bindable as View;
            if (view == null)
                return;

            ICommand command = (ICommand)newValue;
            if (command != null)
                Helper.AddEffect(view, new ControlLongPressEffect());
        }

#endregion LongPressCommand Property


#region  PressCommand Property

        public static readonly BindableProperty PressCommandProperty = BindableProperty.Create("PressCommand", typeof(ICommand), typeof(LongPress), null, propertyChanged: OnPressCommandChanged);


        public static ICommand GetPressCommand(BindableObject view)
        {
            return (ICommand)view.GetValue(PressCommandProperty);
        }

        public static void SetPressCommand(BindableObject view, ICommand value)
        {
            view.SetValue(PressCommandProperty, value);
        }

        static void OnPressCommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = bindable as View;
            if (view == null)
                return;

            ICommand command = (ICommand)newValue;
            if (command != null)
                Helper.AddEffect(view, new ControlLongPressEffect());
        }

#endregion PressCommand Property


        class ControlLongPressEffect : RoutingEffect
        {
            public ControlLongPressEffect() : base($"MultiPlatformApplication.CrossEffects.{nameof(LongPress)}")
            {

            }
        }

    }
}
