using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using InstantMessaging.ViewModel;

namespace InstantMessaging.TemplateSelector
{
    class ConversationStreamTemplateSelector : DataTemplateSelector
    {
        ResourceDictionary messageCurrentUserDictionary = null;
        DataTemplate messageCurrentUserDataTemplate = null;

        ResourceDictionary messageOtherUserDictionary = null;
        DataTemplate messageOtherUserDataTemplate = null;

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null && item is MessageViewModel)
            {
                // Create Dictionaries (if necessary)
                if (messageCurrentUserDictionary == null)
                {
                    messageCurrentUserDictionary = new ResourceDictionary
                    {
                        Source = new Uri("/ResourceDictionaries/MessageCurrentUserDataTemplate.xaml", UriKind.RelativeOrAbsolute)
                    };
                }

                if (messageOtherUserDictionary == null)
                {
                    messageOtherUserDictionary = new ResourceDictionary
                    {
                        Source = new Uri("/ResourceDictionaries/MessageOtherUserDataTemplate.xaml", UriKind.RelativeOrAbsolute)
                    };
                }

                // Create DataTemplates (if necessary)
                if ((messageCurrentUserDataTemplate == null) && (messageCurrentUserDictionary != null))
                {
                    messageCurrentUserDataTemplate = messageCurrentUserDictionary["MessageCurrentUserDataTemplate"] as DataTemplate;
                }

                if ((messageOtherUserDataTemplate == null) && (messageOtherUserDictionary != null))
                {
                    messageOtherUserDataTemplate = messageOtherUserDictionary["MessageOtherUserDataTemplate"] as DataTemplate;
                }

                MessageViewModel message = item as MessageViewModel;

                if (message.Id == "1") // TEST TO CHECK TEMPLATES SELECTION 
                    return messageCurrentUserDataTemplate;
                else
                    return messageOtherUserDataTemplate;
            }
            return null;
        }
    }
}
