using UIKit;
using Xamarin.Forms;

[assembly: ExportEffect(typeof(MultiPlatformApplication.IOs.PlatformEffect.IOsEntry), nameof(MultiPlatformApplication.Effects.Entry))]
namespace MultiPlatformApplication.IOs.PlatformEffect
{
    public class IOsEntry : Xamarin.Forms.Platform.iOS.PlatformEffect
    {
        UITextField editText = null;

        

        protected override void OnAttached()
        {

            if ((Control != null) && (Control is UITextField))
            {
                editText = (UITextField)Control;


                //double minWidth = MultiPlatformApplication.Effects.Entry.GetMinimumWidth(Element);
                //if (minWidth != -1)
                //    editText.SetMinWidth((int)minWidth);

                //if (MultiPlatformApplication.Effects.Entry.GetNoBorder(Element) == true)
                //    NoBorder();

                //SetTintColor();


                // If the EditText has by default a text, we set the caret to the end
                /*
                if (editText.Text?.Length > 0)
                    editText.SetSelection(editText.Text.Length);
                    */
            }
        }

        private void SetTintColor()
        {
            /*
            var color = Helpers.Helper.GetResourceDictionaryById<Xamarin.Forms.Color>("ColorMain");
            if (color != null)
            {
                Android.Graphics.Color c = Android.Graphics.Color.ParseColor(color.ToHex());
                editText.BackgroundTintList = ColorStateList.ValueOf(c);
                editText.ForegroundTintList = ColorStateList.ValueOf(c);
            }
            */
        }

        protected override void OnDetached()
        {
        }

        private void NoBorder()
        {
            editText.Layer.BorderWidth = 0;
            editText.BorderStyle = UITextBorderStyle.None;
        }

    }
}
