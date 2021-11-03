using UIKit;
using Xamarin.Forms;

[assembly: ExportEffect(typeof(MultiPlatformApplication.IOs.PlatformEffect.IOsEntry), nameof(MultiPlatformApplication.Effects.Entry))]
namespace MultiPlatformApplication.IOs.PlatformEffect
{
    public class IOsEntry : Xamarin.Forms.Platform.iOS.PlatformEffect
    {
        UITextField editText = null;
        UITextView editView = null;
        

        protected override void OnAttached()
        {
            double minWidth = MultiPlatformApplication.Effects.Entry.GetMinimumWidth(Element);


            if (Control != null)
            {
                if(Control is UITextView view)
                {
                    editView = view;

                    // If the EditText has by default a text, we set the caret to the end
                    if (editView.Text?.Length > 0)
                        editView.SelectedTextRange = editView.GetTextRange(editView.EndOfDocument, editView.EndOfDocument);

                    // Set minimum width
                    if (minWidth != -1)
                        editView.WidthAnchor.ConstraintLessThanOrEqualTo( (System.nfloat) minWidth);
                }
                else if (Control is UITextField field)
                {
                    editText  = field;

                    // If the EditText has by default a text, we set the caret to the end
                    if (editText.Text?.Length > 0)
                        editText.SelectedTextRange = editText.GetTextRange(editText.EndOfDocument, editText.EndOfDocument);


                    // Set minimum width
                    if (minWidth != -1)
                        editText.WidthAnchor.ConstraintLessThanOrEqualTo((System.nfloat)minWidth);
                }


                if (MultiPlatformApplication.Effects.Entry.GetNoBorder(Element) == true)
                    NoBorder();
            }

                //SetTintColor();
        }

        private void SetTintColor()
        {
            
            var color = Helpers.Helper.GetResourceDictionaryById<Xamarin.Forms.Color>("ColorMain");
            if (color != null)
            {
                //editText.tint

                /*
                Android.Graphics.Color c = Android.Graphics.Color.ParseColor(color.ToHex());
                editText.BackgroundTintList = ColorStateList.ValueOf(c);
                editText.ForegroundTintList = ColorStateList.ValueOf(c);
                */
            }
            
        }

        protected override void OnDetached()
        {
        }

        private void NoBorder()
        {
            if (editText != null)
            {
                editText.Layer.BorderWidth = 0;
                editText.BorderStyle = UITextBorderStyle.None;
            }
            else if(editView != null)
            {
                editView.Layer.BorderWidth = 0;
            }
        }

    }
}
