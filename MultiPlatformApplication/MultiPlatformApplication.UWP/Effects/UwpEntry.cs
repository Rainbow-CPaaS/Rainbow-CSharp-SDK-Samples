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

        int autoExpandToNbLines = 0;
        double maxHeigthSize = 0;

        protected override void OnAttached()
        {
            if ( (Control != null) && (Control is TextBox) )
            {
                textBox = (TextBox)Control;

                autoExpandToNbLines = MultiPlatformApplication.Effects.Entry.GetAutoExpandToNbLines(Element);
                if (autoExpandToNbLines > 0)
                    textBox.TextChanged += TextBox_TextChanged;

                if (MultiPlatformApplication.Effects.Entry.GetNoBorder(Element))
                    NoBorder();
            }
        }

        private void TextBox_TextChanged(object sender, Windows.UI.Xaml.Controls.TextChangedEventArgs e)
        {
            int count = textBox.Text.Count(f => f == '\r');

            if (count == autoExpandToNbLines - 1)
            {
                maxHeigthSize = Control.ActualHeight;
            }
            else if (count >= autoExpandToNbLines)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (Element is Editor)
                    {
                        Editor editor = (Editor)Element;
                        editor.AutoSize = EditorAutoSizeOption.Disabled;

                        if (maxHeigthSize == 0)
                            Control.MaxHeight = 80; // We need to set a value ...
                        else
                            Control.MaxHeight = maxHeigthSize;

                        ScrollViewer.SetVerticalScrollBarVisibility(Control, Windows.UI.Xaml.Controls.ScrollBarVisibility.Visible);

                        //MultiPlatformApplication.Effects.Entry.ForceParentLayout(Element);
                    }
                });
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (Element is Editor)
                    {
                        ((Editor)Element).AutoSize = EditorAutoSizeOption.TextChanges;

                        ScrollViewer.SetVerticalScrollBarVisibility(Control, Windows.UI.Xaml.Controls.ScrollBarVisibility.Hidden);

                        //MultiPlatformApplication.Effects.Entry.ForceParentLayout(Element);
                    }
                });
            }
        }

        protected override void OnDetached()
        {
            if(textBox != null)
            {
                textBox.TextChanged -= TextBox_TextChanged;
            }
        }

        private void NoBorder()
        {
            Windows.UI.Xaml.Thickness noThickness = new Windows.UI.Xaml.Thickness(0);
            textBox.BorderThickness = noThickness;

            //textBox.Padding = noThickness;
            //textBox.Margin = noThickness;

            //textBox.Background = null;
        }
    }

}
