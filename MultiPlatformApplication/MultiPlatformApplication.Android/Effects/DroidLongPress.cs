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

[assembly: ExportEffect(typeof(MultiPlatformApplication.Droid.PlatformEffect.DroidLongPress), nameof(MultiPlatformApplication.Effects.LongPress))]
namespace MultiPlatformApplication.Droid.PlatformEffect
{
    // Based on: https://stackoverflow.com/questions/53095961/how-to-remove-the-border-from-a-entry-control-with-xamarin-forms

    public class DroidLongPress : Xamarin.Forms.Platform.Android.PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Control != null)
            {
                Control.LongClickable = true;
                Control.LongClick += ControlOrContainer_LongClick;

                Control.Click += ControlOrContainer_Click;
            }
            else if (Container != null)
            {
                Container.LongClickable = true;
                Container.LongClick += ControlOrContainer_LongClick;

                Container.Click += ControlOrContainer_Click;
            }
        }



        protected override void OnDetached()
        {
            if (Control != null)
            {
                Control.LongClick -= ControlOrContainer_LongClick;

                Control.Click -= ControlOrContainer_Click;
            }
            else if (Container != null)
            {
                Container.LongClick -= ControlOrContainer_LongClick;

                Container.Click -= ControlOrContainer_Click;
            }
        }

        private void ControlOrContainer_LongClick(object sender, Android.Views.View.LongClickEventArgs e)
        {
            var command = LongPress.GetLongPressCommand(Element);
            if (command != null && command.CanExecute(Element))
                command.Execute(Element);
        }

        private void ControlOrContainer_Click(object sender, EventArgs e)
        {
            var command = LongPress.GetPressCommand(Element);
            if (command != null && command.CanExecute(Element))
                command.Execute(Element);
        }
    }
}
