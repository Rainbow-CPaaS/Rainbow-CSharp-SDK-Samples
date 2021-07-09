using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

using MultiPlatformApplication.Models;
using MultiPlatformApplication.Helpers;

namespace MultiPlatformApplication.Controls
{
    public class GroupDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Group { get; set; }
        public DataTemplate Item { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is GroupModel)
            {
                GroupModel model = item as GroupModel;
                if (String.IsNullOrEmpty(model.GroupName))
                    return Item;
                return Group;
            }
            return null;
        }
    }

}
