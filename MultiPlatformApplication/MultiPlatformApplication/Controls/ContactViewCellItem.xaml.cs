using MultiPlatformApplication.Effects;
using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ContactViewCellItem : ViewCell
    {
        Boolean wasPressed;

        public ContactViewCellItem()
        {
            InitializeComponent();
            AddTouchEffect();
        }

        private void AddTouchEffect()
        {
            TouchEffect touchEffect = new TouchEffect();
            touchEffect.TouchAction += TouchEffect_TouchAction;
            Helper.AddEffect(this.View, touchEffect);
        }

        private void TouchEffect_TouchAction(object sender, Events.TouchActionEventArgs e)
        {
            switch (e.Type)
            {
                case TouchActionType.Entered:
                case TouchActionType.Exited:
                case TouchActionType.Cancelled:
                    wasPressed = false;
                    break;
                case TouchActionType.Pressed:
                    Popup.HideCurrentContextMenu();
                    wasPressed = true;
                    break;
                case TouchActionType.Released:
                    if (wasPressed)
                    {
                        wasPressed = false;
                        if (BindingContext is ContactModel model)
                        {
                            if ((model.SelectionCommand != null) && model.SelectionCommand.CanExecute(BindingContext))
                                model.SelectionCommand.Execute(BindingContext);
                        }
                    }
                    break;
            }
        }
    }
}