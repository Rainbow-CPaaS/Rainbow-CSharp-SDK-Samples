using MultiPlatformApplication.Effects;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms;

[assembly: ExportEffect(typeof(MultiPlatformApplication.UWP.PlatformEffect.UwpSelectableLabelEffect), nameof(SelectableLabelEffect))]
namespace MultiPlatformApplication.UWP.PlatformEffect
{
    public class UwpSelectableLabelEffect : Xamarin.Forms.Platform.UWP.PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Control != null)
            {
                if (Control is TextBlock)
                {
                    TextBlock textBlock = (TextBlock)Control;
                    textBlock.IsTextSelectionEnabled = true;
                }
            }
        }

        protected override void OnDetached()
        {
        }

    }
}
