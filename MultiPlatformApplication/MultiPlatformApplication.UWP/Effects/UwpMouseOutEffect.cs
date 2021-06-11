using MultiPlatformApplication.Effects;
using Windows.UI.Xaml.Input;
using Xamarin.Forms;

[assembly: ExportEffect(typeof(MultiPlatformApplication.UWP.PlatformEffect.UwpMouseOutEffect), nameof(MouseOutEffect))]
namespace MultiPlatformApplication.UWP.PlatformEffect
{
    public class UwpMouseOutEffect : Xamarin.Forms.Platform.UWP.PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Control != null)
                Control.PointerExited += PointerExited;
            else if (Container != null)
                Container.PointerExited += PointerExited;
        }

        protected override void OnDetached()
        {
            if (Control != null)
                Control.PointerExited -= PointerExited;
            else if (Container != null)
                Container.PointerExited -= PointerExited;
        }

        private void PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var command = MouseOutEffect.GetCommand(Element);
            if (command != null && command.CanExecute(null))
                command.Execute(null);
        }
    }
}
