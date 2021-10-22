using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using UIKit;

[assembly: ResolutionGroupName("MultiPlatformApplication.CrossEffects")]



[assembly: ExportEffect(typeof(MultiPlatformApplication.IOs.PlatformEffect.IOsTouchEffect), nameof(MultiPlatformApplication.Effects.TouchEffect))]

namespace MultiPlatformApplication.IOs.PlatformEffect
{
    public class IOsTouchEffect : Xamarin.Forms.Platform.iOS.PlatformEffect
    {
        UIView view;
        TouchRecognizer touchRecognizer;

        protected override void OnAttached()
        {
            // Get the iOS UIView corresponding to the Element that the effect is attached to
            view = Control == null ? Container : Control;

            // Uncomment this line if the UIView does not have touch enabled by default
            //view.UserInteractionEnabled = true;

            // Get access to the TouchEffect class in the .NET Standard library
            MultiPlatformApplication.Effects.TouchEffect effect = (MultiPlatformApplication.Effects.TouchEffect)Element.Effects.FirstOrDefault(e => e is MultiPlatformApplication.Effects.TouchEffect);

            if (effect != null && view != null)
            {
                // Create a TouchRecognizer for this UIView
                touchRecognizer = new TouchRecognizer(Element, view, effect); 
                view.AddGestureRecognizer(touchRecognizer);
            }
        }

        protected override void OnDetached()
        {
            if (touchRecognizer != null)
            {
                // Clean up the TouchRecognizer object
                touchRecognizer.Detach();

                // Remove the TouchRecognizer from the UIView
                view.RemoveGestureRecognizer(touchRecognizer);
            }
        }
    }
}