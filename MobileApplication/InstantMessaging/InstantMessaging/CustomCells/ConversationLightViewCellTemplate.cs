using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace InstantMessaging.CustomCells
{
    class ConversationLightViewCellTemplate: DataTemplateSelector
    {
        private readonly DataTemplate conversationLightViewCell;

        public ConversationLightViewCellTemplate()
        {
            conversationLightViewCell = new DataTemplate(typeof(InstantMessaging.CustomCells.ConversationLightViewCell));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return conversationLightViewCell;
        }
    }
}
