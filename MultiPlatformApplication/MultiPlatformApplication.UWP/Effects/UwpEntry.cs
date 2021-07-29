using MultiPlatformApplication.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.ViewManagement.Core;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportEffect(typeof(MultiPlatformApplication.UWP.PlatformEffect.UwpEntry), nameof(MultiPlatformApplication.Effects.Entry))]
namespace MultiPlatformApplication.UWP.PlatformEffect
{
    // Based on: https://stackoverflow.com/questions/53095961/how-to-remove-the-border-from-a-entry-control-with-xamarin-forms
    public class UwpEntry : Xamarin.Forms.Platform.UWP.PlatformEffect
    {
        TextBox textBox = null;

        protected override void OnAttached()
        {
            if ( (Control != null) && (Control is TextBox) )
            {
                textBox = (TextBox)Control;

                if (MultiPlatformApplication.Effects.Entry.GetNoBorder(Element))
                    NoBorder();
            }
        }

        protected override void OnDetached()
        {
        }

        private void NoBorder()
        {
            Windows.UI.Xaml.Thickness noThickness = new Windows.UI.Xaml.Thickness(0);
            
            textBox.BorderThickness = noThickness;
            textBox.Padding = noThickness;
            textBox.Margin = noThickness;

            textBox.Background = null;
        }
    }

}
