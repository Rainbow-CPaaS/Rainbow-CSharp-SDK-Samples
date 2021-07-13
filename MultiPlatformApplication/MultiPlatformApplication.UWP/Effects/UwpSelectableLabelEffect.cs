using MultiPlatformApplication.Effects;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms;

[assembly: ExportEffect(typeof(MultiPlatformApplication.UWP.PlatformEffect.UwpSelectableLabelEffect), nameof(SelectableLabelEffect))]
namespace MultiPlatformApplication.UWP.PlatformEffect
{
    public class UwpSelectableLabelEffect : Xamarin.Forms.Platform.UWP.PlatformEffect
    {
        Boolean baseValue;

        protected override void OnAttached()
        {
            if (Control != null)
            {
                if (Control is TextBlock)
                {
                    TextBlock textBlock = (TextBlock)Control;

                    // Store base value
                    baseValue = textBlock.IsTextSelectionEnabled;

                    textBlock.IsTextSelectionEnabled = SelectableLabelEffect.GetEnabled(Element);
                }
            }
        }

        protected override void OnDetached()
        {
            if (Control is TextBlock)
            {
                // Restore base value
                TextBlock textBlock = (TextBlock)Control;
                textBlock.IsTextSelectionEnabled = baseValue;
            }
        }

    }
}
