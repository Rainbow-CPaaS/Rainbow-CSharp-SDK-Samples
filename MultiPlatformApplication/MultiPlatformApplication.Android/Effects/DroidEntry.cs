using Android.Content.Res;
using Android.Widget;
using Xamarin.Forms;

[assembly: ExportEffect(typeof(MultiPlatformApplication.Droid.PlatformEffect.DroidEntry), nameof(MultiPlatformApplication.Effects.EntryEffect))]
namespace MultiPlatformApplication.Droid.PlatformEffect
{
    // Based on: https://stackoverflow.com/questions/53095961/how-to-remove-the-border-from-a-entry-control-with-xamarin-forms

    public class DroidEntry : Xamarin.Forms.Platform.Android.PlatformEffect
    {
        EditText editText = null;

        protected override void OnAttached()
        {

            if ((Control != null) && (Control is EditText))
            {
                editText = (EditText)Control;

                if (MultiPlatformApplication.Effects.EntryEffect.GetNoBorder(Element) == true)
                    NoBorder();

                SetTintColor();


                // If the EditText has by default a text, we set the caret to the end
                if (editText.Text?.Length > 0)
                    editText.SetSelection(editText.Text.Length);
            }
        }

        private void SetTintColor()
        {
            var color = Helpers.Helper.GetResourceDictionaryById<Xamarin.Forms.Color>("ColorMain");
            if (color != null)
            {
                Android.Graphics.Color c = Android.Graphics.Color.ParseColor(color.ToHex());
                editText.BackgroundTintList = ColorStateList.ValueOf(c);
                editText.ForegroundTintList = ColorStateList.ValueOf(c);
            }
        }

        protected override void OnDetached()
        {
        }

        private void NoBorder()
        {
            editText.Background = null;
        }

    }
}
