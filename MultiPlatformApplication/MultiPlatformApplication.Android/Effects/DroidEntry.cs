using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MultiPlatformApplication.Effects;
using System;
using System.ComponentModel;
using Xamarin.Forms;
using static Android.Views.ViewGroup;

[assembly: ExportEffect(typeof(MultiPlatformApplication.Droid.PlatformEffect.DroidEntry), nameof(MultiPlatformApplication.Effects.Entry))]
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

                double minWidth = MultiPlatformApplication.Effects.Entry.GetMinimumWidth(Element);
                if (minWidth != -1)
                    editText.SetMinWidth((int)minWidth);

                if (MultiPlatformApplication.Effects.Entry.GetNoBorder(Element))
                    NoBorder();

                if (MultiPlatformApplication.Effects.Entry.GetNoTintColored(Element))
                    NoTintColored();


                // If the EditText has by default a text, we set the caret to the end
                if (editText.Text?.Length > 0)
                    editText.SetSelection(editText.Text.Length);
            }
        }


        protected override void OnDetached()
        {
        }

        private void NoBorder()
        {
            editText.Background = null;

            //editText.SetPadding(0, 0, 0, 0);
            //var lp = new MarginLayoutParams(editText.LayoutParameters);
            //lp.SetMargins(0, 0, 0, 0);
            //editText.LayoutParameters = lp;


        }

        private void NoTintColored()
        {
            // Change cursor color in entry
            // https://social.msdn.microsoft.com/Forums/en-US/d975148a-1ce0-4fbe-9928-18cb2b0368fa/change-cursor-color-in-entry?forum=xamarinforms

            try
            {
                //editText.SetTextCursorDrawable(0);

                IntPtr IntPtrtextViewClass = JNIEnv.FindClass(typeof(TextView));
                IntPtr mCursorDrawableResProperty = JNIEnv.GetFieldID(IntPtrtextViewClass, "mCursorDrawableRes", "I");
                JNIEnv.SetField(Control.Handle, mCursorDrawableResProperty, 0);
            }
            catch
            {

            }
        }

    }
}
