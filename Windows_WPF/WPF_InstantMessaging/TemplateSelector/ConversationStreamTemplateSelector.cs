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
        private readonly App CurrentApplication = (App)System.Windows.Application.Current;

        private readonly ResourceDictionary messageCurrentUserDictionary = null;
        private readonly DataTemplate messageCurrentUserDataTemplate = null;

        private readonly ResourceDictionary messageOtherUserDictionary = null;
        private readonly DataTemplate messageOtherUserDataTemplate = null;

        private readonly ResourceDictionary messageEventDictionary = null;
        private readonly DataTemplate messageEventDataTemplate = null;

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

                if (message.IsEventMessage)
                    result = messageEventDataTemplate;
                else
                {
                    if (message.PeerId == CurrentApplication.CurrentUserId)
                        result = messageCurrentUserDataTemplate;
                    else
                        result = messageOtherUserDataTemplate;
                }
                    
            }
            return result;
        }
    }
}
