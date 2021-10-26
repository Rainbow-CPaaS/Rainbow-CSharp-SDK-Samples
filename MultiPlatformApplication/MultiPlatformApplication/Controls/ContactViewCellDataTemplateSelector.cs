using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

using MultiPlatformApplication.Models;
using MultiPlatformApplication.Helpers;
using System.Windows.Input;

namespace MultiPlatformApplication.Controls
{
    public class ContactViewCellDataTemplateSelector : DataTemplateSelector
    {

        public BindableProperty Command { get; set; }

        private DataTemplate group;
        private DataTemplate item;

//#region  Command Property

//        public static BindableProperty CommandProperty = BindableProperty.Create("Command", typeof(ICommand), typeof(ContactViewCellDataTemplateSelector), null, propertyChanged: OnCommandPropertyChanged);

//        public static ICommand GetCommand(BindableObject view)
//        {
//            return (ICommand)view.GetValue(CommandProperty);
//        }

//        public static void SetCommand(BindableObject view, ICommand value)
//        {
//            view.SetValue(CommandProperty, value);
//        }

//        static void OnCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
//        {
//            if (bindable is ViewCell viewCell)
//            {
//                if (oldValue != newValue)
//                    SetCommand(viewCell, (ICommand)newValue);
//            }
//        }

//#endregion Command Property


        public ContactViewCellDataTemplateSelector()
        {
            group = new DataTemplate(typeof(ContactViewCellGroup));
            item = new DataTemplate(typeof(ContactViewCellItem));

            //ICommand command = this.Vi
            //item.SetValue(ContactViewCellItem.CommandProperty, Command);
        }

        protected override DataTemplate OnSelectTemplate(object obj, BindableObject container)
        {
            if (obj is GroupModel model)
            {
                if (String.IsNullOrEmpty(model.GroupName))
                    return item;
                return group;
            }
            return null;
        }
    }

}
