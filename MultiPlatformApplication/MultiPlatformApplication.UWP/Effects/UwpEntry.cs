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

[assembly: ExportEffect(typeof(MultiPlatformApplication.UWP.PlatformEffect.UwpEntry), nameof(MultiPlatformApplication.Effects.EntryEffect))]
namespace MultiPlatformApplication.UWP.PlatformEffect
{
    // Based on: https://stackoverflow.com/questions/53095961/how-to-remove-the-border-from-a-entry-control-with-xamarin-forms
    public class UwpEntry : Xamarin.Forms.Platform.UWP.PlatformEffect
    {
        TextBox textBox = null;

        String breakLineModifier;
        ICommand validationCommand;

        protected override void OnAttached()
        {
            if ( (Control != null) && (Control is TextBox) )
            {
                textBox = (TextBox)Control;

                validationCommand = MultiPlatformApplication.Effects.EntryEffect.GetValidationCommand(Element);
                if (validationCommand != null)
                {
                    breakLineModifier = MultiPlatformApplication.Effects.EntryEffect.GetBreakLineModifier(Element);
                    if (breakLineModifier == null)
                        breakLineModifier = "";
                    textBox.KeyUp += TextBox_KeyUp;
                }

                if (MultiPlatformApplication.Effects.EntryEffect.GetNoBorder(Element) == true)
                    NoBorder();

                // If the textbox has by default a text, we set the caret to the end
                if (textBox.Text?.Length > 0)
                    textBox.SelectionStart = textBox.Text.Length;
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
            // Checkt if "Enter" key has been pressed
            if(e.OriginalKey == VirtualKey.Enter)
            {
                bool shiftPressed = false;
                bool ctrlPressed = false;
                bool altPressed = false;
                bool validate = true;

                if ( (breakLineModifier == "shift") && (shiftPressed = IsShifKeyPressed()) )
                {
                    validate = false;
                }
                else if ( (breakLineModifier == "control") && (ctrlPressed = IsCtrlKeyPressed()) )
                {
                    validate = false;
                }
                else if ( (breakLineModifier == "alt") && (altPressed = IsAltKeyPressed()) )
                {
                    validate = false;
                }
                else if ( (breakLineModifier == "") && !(shiftPressed || ctrlPressed || altPressed))
                {
                    validate = false;
                }

                if (validate)
                {
                    if (validationCommand != null && validationCommand.CanExecute(Element))
                        validationCommand.Execute(Element);
                }
            }
        }

        protected override void OnDetached()
        {
            if (textBox != null)
            {
                if (validationCommand != null)
                    textBox.KeyUp -= TextBox_KeyUp;

                validationCommand = null;
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
