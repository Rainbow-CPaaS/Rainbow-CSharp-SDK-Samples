using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

using MultiPlatformApplication.Models;
using MultiPlatformApplication.Helpers;

namespace MultiPlatformApplication.Controls
{
    public class MessageDataTemplateSelector: DataTemplateSelector
    {
        public DataTemplate MessageEvent { get; set; }
        public DataTemplate MessageCurrentUser { get; set; }
        public DataTemplate MessageOtherUser { get; set; }

        private String currentContactId = null;

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is MessageModel)
            {
                MessageModel message = item as MessageModel;
                if (message.IsEventMessage == "True")
                    return MessageEvent;
                else if (message.PeerId == GetCurrentContactId())
                    return MessageCurrentUser;
                return MessageOtherUser;
            }
            return null;
        }

        private String GetCurrentContactId()
        {
            if (currentContactId == null)
                currentContactId = Helper.SdkWrapper.GetCurrentContactId();

            return currentContactId;
        }
    }
}
