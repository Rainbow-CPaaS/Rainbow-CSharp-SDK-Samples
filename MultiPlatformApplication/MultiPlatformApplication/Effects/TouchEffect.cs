using MultiPlatformApplication.Events;
using System;
using Xamarin.Forms;

namespace MultiPlatformApplication.Effects
{
    public class TouchEffect : RoutingEffect
    {
        // Based on: https://github.com/xamarin/xamarin-forms-samples/tree/main/Effects/TouchTrackingEffect

        public event EventHandler<TouchActionEventArgs> TouchAction;

        public TouchEffect() : base("MultiPlatformApplication.CrossEffects.TouchEffect")
        {
        }

        public bool Capture { set; get; }

        public void OnTouchAction(Element element, TouchActionEventArgs args)
        {
            TouchAction?.Invoke(element, args);
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
