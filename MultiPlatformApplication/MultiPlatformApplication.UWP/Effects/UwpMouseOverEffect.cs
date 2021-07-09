using MultiPlatformApplication.Effects;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Xamarin.Forms;

[assembly: ResolutionGroupName("MultiPlatformApplication.CrossEffects")]
[assembly: ExportEffect(typeof(MultiPlatformApplication.UWP.PlatformEffect.UwpMouseOverEffect), nameof(MouseOverEffect))]
namespace MultiPlatformApplication.UWP.PlatformEffect
{
    public class UwpMouseOverEffect : Xamarin.Forms.Platform.UWP.PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Control != null)
                Control.PointerEntered += PointerEntered;
            else if (Container != null)
                Container.PointerEntered += PointerEntered;
        }

        protected override void OnDetached()
        {
            if (Control != null)
                Control.PointerEntered -= PointerEntered;
            else if (Container != null)
                Container.PointerEntered -= PointerEntered;
        }

        private void PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var command = MouseOverEffect.GetCommand(Element);
            if (command != null && command.CanExecute(null))
                command.Execute(null);
        }
    }
}
