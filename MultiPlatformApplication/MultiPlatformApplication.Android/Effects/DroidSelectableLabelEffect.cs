using Android.Widget;
using MultiPlatformApplication.Effects;
using Xamarin.Forms;

[assembly: ResolutionGroupName("MultiPlatformApplication.CrossEffects")]
[assembly: ExportEffect(typeof(MultiPlatformApplication.Droid.PlatformEffect.DroidSelectableLabelEffect), nameof(SelectableLabelEffect))]
namespace MultiPlatformApplication.Droid.PlatformEffect
{
    public class DroidSelectableLabelEffect : Xamarin.Forms.Platform.Android.PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Control != null)
            {
                if (Control is TextView)
                {
                    TextView textView = (TextView)Control;
                    textView.SetTextIsSelectable(true);
                }
            }
        }

        protected override void OnDetached()
        {
        }

    }
}
