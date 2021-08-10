using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using InstantMessaging.Helpers;
using Rainbow;

namespace InstantMessaging
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConversationsPage : ContentPage
    {
        //private AvatarPool avatarPool;

        private ConversationsViewModel vm;

        private InstantMessaging.App XamarinApplication;

        public ConversationsPage()
        {
            InitializeComponent();

            this.Appearing += ConversationsPage_Appearing;

            // Get Xamarin Application
            XamarinApplication = (InstantMessaging.App)Xamarin.Forms.Application.Current;

            // Manage event(s) from Rainbow SDK about CONVERSATIONS
            XamarinApplication.RbConversations.ConversationCreated += RbConversations_ConversationCreated;
            XamarinApplication.RbConversations.ConversationRemoved += RbConversations_ConversationRemoved;
            XamarinApplication.RbConversations.ConversationUpdated += RbConversations_ConversationUpdated;

            // Manage event(s) from Rainbow SDK about CONTACTS
            XamarinApplication.RbContacts.ContactPresenceChanged += RbContacts_ContactPresenceChanged;
            XamarinApplication.RbContacts.ContactAdded += RbContacts_ContactAdded;
            XamarinApplication.RbContacts.ContactInfoChanged += RbContacts_ContactInfoChanged;

            XamarinApplication.RbBubbles.BubbleInfoUpdated += RbBubbles_BubbleInfoUpdated;

            // Create ViewModel
            vm = new ConversationsViewModel();

            // Set the ViewModel as BindingContext
            BindingContext = vm;

            // Initialize the ViewModel
            vm.ResetModelWithRbConversations(XamarinApplication.RbConversations.GetAllConversationsFromCache());

            // Now allow Avatar download
            Rainbow.Helpers.AvatarPool.Instance.AllowAvatarDownload(true);

            // Now allow to ask server info about unknown contacts
            Rainbow.Helpers.AvatarPool.Instance.AllowToAskInfoForUnknownContact(true);
        }

        void ShowConversationStreamPage(String conversationId)
        {
            XamarinApplication.CurrentConversationId = conversationId;

            Device.BeginInvokeOnMainThread(async () =>
            {
                if (XamarinApplication.ConversationStreamPageList == null)
                    XamarinApplication.ConversationStreamPageList = new Dictionary<String, ConversationStreamPage>();
                
                ConversationStreamPage conversationStreamPage;
                if (XamarinApplication.ConversationStreamPageList.ContainsKey(conversationId))
                    conversationStreamPage = XamarinApplication.ConversationStreamPageList[conversationId];
                else
                {
                    conversationStreamPage = new ConversationStreamPage(conversationId);
                    XamarinApplication.ConversationStreamPageList.Add(conversationId, conversationStreamPage);

                    //TODO - avoid to have too many Page in this list
                }

                await Navigation.PushAsync(conversationStreamPage, false);
            });

        }

#region EVENTS FIRED BY XAML

        private void ConversationsPage_Appearing(object sender, EventArgs e)
        {
            XamarinApplication.CurrentConversationId = null;
        }

        void ConversationsListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ConversationsListView.SelectedItem = null;
        }

        void ConversationsListView_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if(e.Item != null)
            {
                if(e.Item is ConversationLight)
                {
                    ConversationLight conversation = e.Item as ConversationLight;

                    if (conversation != null)
                        ShowConversationStreamPage(conversation.Id);
                }
            }

            ConversationsListView.SelectedItem = null;
        }

#endregion EVENTS FIRED BY XAML

#region EVENTS FIRED BY RAINBOW SDK
        
        private void RbBubbles_BubbleInfoUpdated(object sender, Rainbow.Events.BubbleInfoEventArgs e)
        {
            vm.UpdateConversationNameByPeerId(e.BubbleId, e.Name);
        }

        private void RbContacts_ContactAdded(object sender, Rainbow.Events.JidEventArgs e)
        {
            Rainbow.Model.Contact contact = XamarinApplication.RbContacts.GetContactFromContactJid(e.Jid);
            if (contact != null)
                vm.UpdateConversationNameByPeerId(contact.Id, Util.GetContactDisplayName(contact));
        }


        private void RbContacts_ContactInfoChanged(object sender, Rainbow.Events.JidEventArgs e)
        {
            Rainbow.Model.Contact contact = XamarinApplication.RbContacts.GetContactFromContactJid(e.Jid);
            if (contact != null)
                vm.UpdateConversationNameByPeerId(contact.Id, Util.GetContactDisplayName(contact));
        }

        private void RbContacts_ContactPresenceChanged(object sender, Rainbow.Events.PresenceEventArgs e)
        {
            vm.UpdateModelUsingPresence(e.Jid, e.Presence);
        }

        private void RbConversations_ConversationUpdated(object sender, Rainbow.Events.ConversationEventArgs e)
        {
            if (e.Conversation != null)
            {
                vm.UpdateRBConversationToModel(e.Conversation);
            }
        }

        private void RbConversations_ConversationRemoved(object sender, Rainbow.Events.ConversationEventArgs e)
        {
            if (e.Conversation != null)
            {
                vm.RemoveRbConversationFromModel(e.Conversation.Id);
            }
        }

        private void RbConversations_ConversationCreated(object sender, Rainbow.Events.ConversationEventArgs e)
        {
            if (e.Conversation != null)
            {
                vm.AddRBConversationToModel(e.Conversation);
            }
        }

#endregion EVENTS FIRED BY RAINBOW SDK
    }
}