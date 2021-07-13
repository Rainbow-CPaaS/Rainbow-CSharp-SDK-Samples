using Android.Views;
using Android.Widget;
using MultiPlatformApplication.Effects;
using System;
using System.ComponentModel;
using Xamarin.Forms;

[assembly: ResolutionGroupName("MultiPlatformApplication.CrossEffects")]
[assembly: ExportEffect(typeof(MultiPlatformApplication.Droid.PlatformEffect.DroidSelectableLabelEffect), nameof(SelectableLabelEffect))]
namespace MultiPlatformApplication.Droid.PlatformEffect
{
    public class DroidSelectableLabelEffect : Xamarin.Forms.Platform.Android.PlatformEffect
    {
        Boolean baseValue;

        protected override void OnAttached()
        {
            if (Control != null)
            {
                if (Control is TextView)
                {
                    TextView textView = (TextView)Control;
                    // Store base value
                    baseValue = textView.IsTextSelectable;

                    textView.SetTextIsSelectable(true);
                }
            }
        }

        protected override void OnDetached()
        {
            if (Control is TextView)
            {
                TextView textView = (TextView)Control;

                // Restore base value
                textView.SetTextIsSelectable(baseValue);
            }
        }

    }
}
