﻿using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms;

[assembly: ExportEffect(typeof(MultiPlatformApplication.IOs.PlatformEffect.IOsEntry), nameof(MultiPlatformApplication.Effects.EntryEffect))]
namespace MultiPlatformApplication.IOs.PlatformEffect
{
    public class IOsEntry : Xamarin.Forms.Platform.iOS.PlatformEffect
    {
        private NSObject _keyboardUp, _keyboardDown;

        Editor editor = null;
        int maxLines = 5;

        System.nfloat maxHeight;
        UITextField editText = null;
        UITextView editView = null;

        MultiPlatformApplication.Effects.EntryEffect entryEffect;

        protected override void OnAttached()
        {
            if (Element is Editor edit)
                editor = edit;

            if (Control != null)
            {
                if(Control is UITextView view)
                {
                    editView = view;

                    // If the EditText has by default a text, we set the caret to the end
                    if (editView.Text?.Length > 0)
                        editView.SelectedTextRange = editView.GetTextRange(editView.EndOfDocument, editView.EndOfDocument);

                    if (maxLines > 0)
                    {
                        maxHeight = editView.Font.LineHeight * maxLines;
                        editView.Changed += EditView_Changed;
                    }
                    
                }
                else if (Control is UITextField field)
                {
                    editText  = field;

                    // If the EditText has by default a text, we set the caret to the end
                    if (editText.Text?.Length > 0)
                        editText.SelectedTextRange = editText.GetTextRange(editText.EndOfDocument, editText.EndOfDocument);
                }

                if ((editView != null) || (editText != null))
                {
                    entryEffect = (MultiPlatformApplication.Effects.EntryEffect)Element.Effects.FirstOrDefault(e => e is MultiPlatformApplication.Effects.EntryEffect);

                    if (MultiPlatformApplication.Effects.EntryEffect.GetNoBorder(Element) == true)
                        NoBorder();

                    RegisterKeyboardObserver();
                }
            }

                //SetTintColor();
        }

        private void EditView_Changed(object sender, System.EventArgs e)
        {
            if (editView.ContentSize.Height > maxHeight)
            {
                editor.AutoSize = EditorAutoSizeOption.Disabled;
                editView.ScrollEnabled = true;
            }
            else
            {
                editor.AutoSize = EditorAutoSizeOption.TextChanges;
                editView.ScrollEnabled = false;
            }
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
            RemoveKeyboardObserver();
        }

        public void RegisterKeyboardObserver()
        {
            if (_keyboardUp == null && _keyboardDown == null)
            {
                _keyboardUp = NSNotificationCenter
                    .DefaultCenter
                    .AddObserver(UIKeyboard.WillShowNotification, KeyboardUpNotification);
                _keyboardDown = NSNotificationCenter
                    .DefaultCenter
                    .AddObserver(UIKeyboard.WillHideNotification, KeyboardDownNotification);
            }
        }

        private void KeyboardUpNotification(NSNotification notification)
        {
            var keyboardFrame = UIKeyboard.BoundsFromNotification(notification);
            entryEffect.OnKeyboardRectChanged(new Rect(keyboardFrame.X, keyboardFrame.Y, keyboardFrame.Width, keyboardFrame.Height));
        }

        private void KeyboardDownNotification(NSNotification notification)
        {
            entryEffect.OnKeyboardRectChanged(new Rect(0, 0, 0, 0));
        }

        private void RemoveKeyboardObserver()
        {
            if (_keyboardUp != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_keyboardUp);
                _keyboardUp.Dispose();
                _keyboardUp = null;
            }

            if (_keyboardDown != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_keyboardDown);
                _keyboardDown.Dispose();
                _keyboardDown = null;
            }
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
