using MultiPlatformApplication.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Core;
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
        
        String validateKeyModifier;
        ICommand validationCommand;

        protected override void OnAttached()
        {
            if ( (Control != null) && (Control is TextBox) )
            {
                textBox = (TextBox)Control;

                validationCommand = MultiPlatformApplication.Effects.Entry.GetValidationCommand(Element);
                if (validationCommand != null)
                {
                    validateKeyModifier = MultiPlatformApplication.Effects.Entry.GetValidationKeyModifier(Element);
                    if (validateKeyModifier == null)
                        validateKeyModifier = "";
                    textBox.KeyUp += TextBox_KeyUp;
                }

                if (MultiPlatformApplication.Effects.Entry.GetNoBorder(Element))
                    NoBorder();
            }
        }

        private static bool IsCtrlKeyPressed()
        {
            var ctrlState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control);
            return (ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }

        private static bool IsShifKeyPressed()
        {
            var ctrlState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift);
            return (ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }

        private static bool IsAltKeyPressed()
        {
            var ctrlState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Menu);
            return (ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }

        private void TextBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {

            if(e.OriginalKey == VirtualKey.Enter)
            {
                bool validate = false;
                if((validateKeyModifier == "shift") && IsShifKeyPressed())
                {
                    validate = true;
                }
                else if((validateKeyModifier == "control") && IsCtrlKeyPressed())
                {
                    validate = true;
                }
                else if((validateKeyModifier == "alt") && IsAltKeyPressed())
                {
                    validate = true;
                }
                else if(validateKeyModifier == "")
                {
                    validate = true;
                }

                if (validate)
                {
                    var command = MultiPlatformApplication.Effects.Entry.GetValidationCommand(Element);
                    if (command != null && command.CanExecute(Element))
                        command.Execute(Element);
                }
            }
        }

        protected override void OnDetached()
        {
            if(textBox != null)
            {
                textBox.KeyUp -= TextBox_KeyUp;
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
