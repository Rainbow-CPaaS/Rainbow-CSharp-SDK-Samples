using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using Microsoft.Extensions.Logging;
using Rainbow;
using Rainbow.Common;
using Rainbow.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ConversationStreamViewModel: MessageElementModel
    {
        private static readonly ILogger log = Rainbow.LogFactory.CreateLogger<ConversationStreamViewModel>();

        private ConversationModel conversationModel;

#region BINDINGS used in XAML

        public NavigationModel Navigation { get; private set; } = new NavigationModel();

        public ConversationModel Conversation {
                get { return conversationModel; }
                set { SetProperty(ref conversationModel, value); }
            }

        public List<MessageElementModel> MessagesList { get; private set; } = new List<MessageElementModel>();

#endregion BINDINGS used in XAML

        private static readonly int NB_MESSAGE_LOADED_BY_ROW = 20;

        private App XamarinApplication;

        private String conversationId;
        private Boolean firstInitialization = true;
        private String currentContactId;
        private String currentContactJid;
        private Conversation rbConversation = null; // Rainbow Conversation object

        private Object lockObservableMessagesList = new Object(); // To lock access to the observable collection: 'MessagesList'

#region PUBLIC METHODS
        /// <summary>
        /// Constructor
        /// </summary>
        public ConversationStreamViewModel()
        {
            App XamarinApplication = (App)Xamarin.Forms.Application.Current;
            Navigation.BackCommand = new RelayCommand<object>(new Action<object>(BackCommand));
        }

        ~ConversationStreamViewModel()
        {

        }

        public void Initialize(String conversationId)
        {
            if (firstInitialization)
            {
                firstInitialization = false;
                this.conversationId = conversationId;

                InitializeSdkObjectsAndEvents();
            }

            // Mark all msg as read
            MarkAllAsRead();
        }

        public void Uninitialize()
        {
            UnnitializeSdkObjectsAndEvents();

            MessagesList.Clear();
            MessagesList = null;
        }

        public void MarkAllAsRead()
        {
            try
            {
                Helper.SdkWrapper.MarkAllMessagesAsRead(Conversation?.Id);
            }
            catch
            {
            }
        }

        public void SetIsTyping(bool isTyping)
        {
            Helper.SdkWrapper.SendIsTypingInConversationById(Conversation.Id, isTyping);
        }

        public void SendMessage(String content)
        {
            Helper.SdkWrapper.SendMessageToConversationId(Conversation.Id, content, UrgencyType.Std);
            SetIsTyping(false);
        }

#endregion PUBLIC METHODS


#region EVENTS FROM CollectionView

    #region  COMMANDS

        private async void BackCommand(object obj)
        {
            await XamarinApplication.NavigationService.GoBack();
        }

    #endregion  COMMANDS

#endregion EVENTS FROM CollectionView


#region PRIVATE METHOD

        private void InitializeSdkObjectsAndEvents()
        {
            // Get Xamarin Application
            XamarinApplication = (App)Xamarin.Forms.Application.Current;

            currentContactId = Helper.SdkWrapper.GetCurrentContactId();
            currentContactJid = Helper.SdkWrapper.GetCurrentContactJid();

            if (!String.IsNullOrEmpty(conversationId))
            {
                // Get Rainbow Conversation object
                rbConversation = Helper.SdkWrapper.GetConversationByIdFromCache(conversationId);

                // Get Conversation Model Object using Rainbow Conversation
                Conversation = Helper.GetConversationFromRBConversation(rbConversation);
            }
        }

        private void UnnitializeSdkObjectsAndEvents()
        {
           
        }

#endregion PRIVATE METHOD



    }
}
