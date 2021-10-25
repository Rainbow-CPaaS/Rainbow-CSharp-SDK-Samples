using MultiPlatformApplication.Events;
using MultiPlatformApplication.Helpers;
using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace MultiPlatformApplication.Effects
{
    public class TouchEffect : RoutingEffect
    {

#region  ValidationCommand Property

        public static readonly BindableProperty CommandProperty = BindableProperty.Create("Command", typeof(ICommand), typeof(TouchEffect), null, propertyChanged: OnCommandChanged);

        public static ICommand GetCommand(BindableObject view)
        {
            return (ICommand)view.GetValue(CommandProperty);
        }

        public static void SetCommand(BindableObject view, ICommand value)
        {
            view.SetValue(CommandProperty, value);
        }

        static void OnCommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = bindable as View;
            if (view == null)
                return;

            ICommand command = (ICommand)newValue;
            if (command != null)
                Helper.AddEffect(view, new TouchEffect());
        }

#endregion ValidationCommand Property


        // Based on: https://github.com/xamarin/xamarin-forms-samples/tree/main/Effects/TouchTrackingEffect

        public event EventHandler<TouchActionEventArgs> TouchAction;

        public TouchEffect() : base("MultiPlatformApplication.CrossEffects.TouchEffect")
        {
        }

        public bool Capture { set; get; }

        public void OnTouchAction(Element element, TouchActionEventArgs args)
        {
            TouchAction?.Invoke(element, args);

            if (args.Type == TouchActionType.Pressed)
            {
                if(element.GetValue(CommandProperty) is ICommand iCommand)
                {
                    if (iCommand?.CanExecute(element) == true)
                        iCommand.Execute(element);
                }
            }
        }
    }

    public enum TouchActionType
    {
        Entered,
        Pressed,
        Moved,
        Released,
        Exited,
        Cancelled
    }

    public enum TouchMouseButton
    {
        Unknown,
        Left,
        Middle,
        Right
    }
}
