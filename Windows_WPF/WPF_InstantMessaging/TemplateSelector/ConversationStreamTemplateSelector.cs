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

        ResourceDictionary messageEventDictionary = null;
        DataTemplate messageEventDataTemplate = null;

        public ConversationStreamTemplateSelector()
        {
            // Create 'Message Current User' Data Template using the correct Dictionary
            messageCurrentUserDictionary = new ResourceDictionary
            {
                Source = new Uri("/ResourceDictionaries/MessageCurrentUserDataTemplate.xaml", UriKind.RelativeOrAbsolute)
            };
            messageCurrentUserDataTemplate = messageCurrentUserDictionary["MessageCurrentUserDataTemplate"] as DataTemplate;


            // Create 'Message Other User' Data Template using the correct Dictionary
            messageOtherUserDictionary = new ResourceDictionary
            {
                Source = new Uri("/ResourceDictionaries/MessageOtherUserDataTemplate.xaml", UriKind.RelativeOrAbsolute)
            };
            messageOtherUserDataTemplate = messageOtherUserDictionary["MessageOtherUserDataTemplate"] as DataTemplate;


            // Create 'Message Event' Data Template using the correct Dictionary
            messageEventDictionary = new ResourceDictionary
            {
                Source = new Uri("/ResourceDictionaries/MessageEventDataTemplate.xaml", UriKind.RelativeOrAbsolute)
            };
            messageEventDataTemplate = messageEventDictionary["MessageEventDataTemplate"] as DataTemplate;


            // Clean dictionaries
            messageCurrentUserDictionary = null;
            messageOtherUserDictionary = null;
            messageEventDictionary = null;
        }


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            DataTemplate result = null;
            if (item != null && item is MessageViewModel)
            {
                MessageViewModel message = item as MessageViewModel;

                // TEST PURPOSE: Display each template
                if (message.Id == "1")
                    result = messageCurrentUserDataTemplate;
                else if (message.Id == "2")
                    result = messageOtherUserDataTemplate;
                else
                    result = messageEventDataTemplate;
            }
            return result;
        }
    }
}
