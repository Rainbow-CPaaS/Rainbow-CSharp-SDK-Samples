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
        private readonly DataTemplate messageViewEvent;

        private String currentContactId = null;

        public MessageViewCellTemplate()
        {
            messageViewOtherUser = new DataTemplate(typeof(InstantMessaging.CustomCells.MessageViewCellOtherUser));

            messageViewCurrentUser = new DataTemplate(typeof(InstantMessaging.CustomCells.MessageViewCellCurrentUser));

            messageViewEvent = new DataTemplate(typeof(InstantMessaging.CustomCells.MessageViewCellEvent));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if(item is InstantMessaging.Model.Message)
            {
                InstantMessaging.Model.Message message = item as InstantMessaging.Model.Message;
                if (message.IsEventMessage == "True")
                    return messageViewEvent;
                else if (message.PeerId == GetCurrentContactId())
                    return messageViewCurrentUser;
                return messageViewOtherUser;
            }
            return null;
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
