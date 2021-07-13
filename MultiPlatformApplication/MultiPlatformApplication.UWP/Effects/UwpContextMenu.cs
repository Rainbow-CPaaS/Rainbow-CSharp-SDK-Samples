
using MultiPlatformApplication.Effects;
using Windows.UI.Xaml.Input;
using Xamarin.Forms;

[assembly: ExportEffect(typeof(MultiPlatformApplication.UWP.PlatformEffect.UwpContextMenu), nameof(ContextMenu))]
namespace MultiPlatformApplication.UWP.PlatformEffect
{
    public class UwpContextMenu : Xamarin.Forms.Platform.UWP.PlatformEffect
    {
        VisualElement visualElement = null; 
        Frame contextMenuBackground = null;

        protected override void OnAttached()
        {
            if (Element != null)
            {
                if (Element is VisualElement)
                {
                    visualElement = Element as VisualElement;
                    visualElement.PropertyChanged += VisualElement_PropertyChanged;
                    
                }
            }
        }

        private void VisualElement_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsVisible")
            {
                if (visualElement.IsVisible)
                    ContextMenu.Display(visualElement);
                else
                    ContextMenu.Hide(visualElement);
            }
            else if (e.PropertyName == "Width")
            {
                // We need to create Background as soon as possible
                if (visualElement.Width > 0)
                {
                    if (contextMenuBackground == null)
                    {
                        contextMenuBackground = ContextMenu.GetContextMenuBackgroundElement(visualElement);
                        TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                        tapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;
                        contextMenuBackground.GestureRecognizers.Add(tapGestureRecognizer);
                    }
                }
            }
        }

        private void TapGestureRecognizer_Tapped(object sender, System.EventArgs e)
        {
            ContextMenu.Hide(visualElement);
        }

        protected override void OnDetached()
        {
            if (visualElement != null)
            {
                visualElement.PropertyChanged -= VisualElement_PropertyChanged;
            }
        }


    }
}
