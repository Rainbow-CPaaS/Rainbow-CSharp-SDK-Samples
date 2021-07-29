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

[assembly: ExportEffect(typeof(MultiPlatformApplication.UWP.PlatformEffect.UwpEmojiSelector), nameof(EmojiSelector))]
namespace MultiPlatformApplication.UWP.PlatformEffect
{
    public class UwpEmojiSelector : Xamarin.Forms.Platform.UWP.PlatformEffect
    {
        VisualElement visualElement = null;
        View entry;

        InputInjector inputInjector = null;
        InjectedInputKeyboardInfo leftWindowsDown, leftWindowsUp = null;
        InjectedInputKeyboardInfo periodDown, periodUp = null;

        Boolean needUnfocus = false;
        Boolean needEmojiSelectorDisplay = false;

        protected override void OnAttached()
        {
            if (Element != null)
            {
                if (Element is VisualElement)
                    visualElement = Element as VisualElement;
            }

            if (Control != null)
                Control.PointerReleased += PointerReleased;
            else if (Container != null)
                Container.PointerReleased += PointerReleased;
        }

        protected override void OnDetached()
        {
            if (Control != null)
                Control.PointerReleased -= PointerReleased;
            else if (Container != null)
                Container.PointerReleased -= PointerReleased;

            visualElement = null;
            inputInjector = null;

            leftWindowsDown = leftWindowsUp = null;
            periodDown = periodUp = null;
        }

        private void PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (entry == null)
            {
                entry = EmojiSelector.OnPointerReleased(visualElement);

                // If we don't find an entry, it means that the effet has been badly used => bad entry name refrenced ?
                if (entry == null)
                    return;

                entry.Focused += Entry_Focused;
                entry.Unfocused += Entry_Unfocused;
            }

            if (entry.IsFocused)
            {
                needUnfocus = true;
                Device.BeginInvokeOnMainThread(() =>
                {
                    entry.Unfocus();
                });
            }
            else
            {
                needEmojiSelectorDisplay = true;
                Device.BeginInvokeOnMainThread(() =>
                {
                    entry.Focus();
                });
            }
        }

        private void Entry_Unfocused(object sender, FocusEventArgs e)
        {
            if(needUnfocus)
            {
                needEmojiSelectorDisplay = true;
                Device.BeginInvokeOnMainThread(() =>
                {
                    entry.Focus();
                });
            }
            else
            {
                needEmojiSelectorDisplay = false;
            }
        }

        private void Entry_Focused(object sender, FocusEventArgs e)
        {
            if (needEmojiSelectorDisplay)
                DisplayEmojiSelector();

            needUnfocus = false;
        }

        private void DisplayEmojiSelector()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (inputInjector == null)
                {
                    try
                    {
                        inputInjector = InputInjector.TryCreate();
                    }
                    catch
                    {
                        // Nothing to do
                    }
                }

                // If we cannot create InputInjector, we display Emoji Selector in a different way
                if (inputInjector == null)
                {
                    CoreInputView.GetForCurrentView().TryShow(CoreInputViewKind.Emoji);
                    return;
                }

                // If necessary create first input keyboard info
                if (leftWindowsDown == null)
                {
                    leftWindowsDown = new InjectedInputKeyboardInfo();
                    leftWindowsDown.VirtualKey = (ushort)(VirtualKey.LeftWindows);
                    leftWindowsDown.KeyOptions = InjectedInputKeyOptions.None;
                }

                if (leftWindowsUp == null)
                {
                    leftWindowsUp = new InjectedInputKeyboardInfo();
                    leftWindowsUp.VirtualKey = (ushort)(VirtualKey.LeftWindows);
                    leftWindowsUp.KeyOptions = InjectedInputKeyOptions.KeyUp;
                }

                if (periodDown == null)
                {
                    periodDown = new InjectedInputKeyboardInfo();
                    periodDown.VirtualKey = 0xBE;
                    periodDown.KeyOptions = InjectedInputKeyOptions.None;
                }

                if (periodUp == null)
                {
                    periodUp = new InjectedInputKeyboardInfo();
                    periodUp.VirtualKey = 0xBE;
                    periodUp.KeyOptions = InjectedInputKeyOptions.KeyUp;
                }
                

                // Send keyboard inputs
                inputInjector.InjectKeyboardInput(new[] { leftWindowsDown, periodDown, periodUp, leftWindowsUp });
            });
        }
    }
}
