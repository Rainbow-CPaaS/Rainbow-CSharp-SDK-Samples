using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

using InstantMessaging.Model;

namespace InstantMessaging.CustomCells
{
    class MessageViewCellTemplate : DataTemplateSelector
    {
        private readonly DataTemplate messageViewCurrentUser;
        private readonly DataTemplate messageViewOtherUser;

        private String currentContactId = null;

        public MessageViewCellTemplate()
        {
            messageViewOtherUser = new DataTemplate(typeof(InstantMessaging.CustomCells.MessageViewCellOtherUser));

            messageViewCurrentUser = new DataTemplate(typeof(InstantMessaging.CustomCells.MessageViewCellCurrentUser));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if(item is InstantMessaging.Model.Message)
            {
                InstantMessaging.Model.Message message = item as InstantMessaging.Model.Message;
                if (message.PeerId == GetCurrentContactId())
                    return messageViewCurrentUser;
            }
            return messageViewOtherUser;
        }

        private String GetCurrentContactId()
        {
            if(currentContactId == null)
            {
                InstantMessaging.App XamarinApplication = (InstantMessaging.App)Xamarin.Forms.Application.Current;
                currentContactId = XamarinApplication.RbContacts.GetCurrentContactId();
            }
            return currentContactId;
        }
    }
}
