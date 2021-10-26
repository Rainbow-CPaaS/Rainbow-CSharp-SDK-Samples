using System;
using System.Collections.Generic;

using Xamarin.Essentials;

using Xamarin.Forms.Xaml;

using Rainbow;

using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;

using NLog;


namespace MultiPlatformApplication.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ConversationsViewModel : ConversationModel
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(ConversationsViewModel));

        private App XamarinApplication;
        
        private Boolean firstInitialization = true;

        private readonly Object lockObservableConversations = new Object(); // To lock access to the observable collection: 'Conversations'

        // To know the dateTime of the most recent message received - needed to update the layout
        private DateTime mostRecentDateTimeMessage = DateTime.MinValue;
        private CancelableDelay delayUpdateForDateTimePurpose = null;

#region BINDINGS used by XAML

        // We have a dynamic list of 'ConversationModel' to manage
        public DynamicListModel<ConversationModel> DynamicList { get; private set; } = new DynamicListModel<ConversationModel>();
#endregion


#region PUBLIC METHODS

        public ConversationsViewModel()
        {
            DynamicList.SelectedItemCommand = new RelayCommand<object>(SelectedConversationCommand);
        }

        public void Initialize()
        {
            if(firstInitialization)
            {
                firstInitialization = false;
                
                InitializeSdkObjectsAndEvents();

                ConversationLoadMoreItems(null);
            }
        }

#endregion PUBLIC METHODS


#region MANAGE COMMANDS 

        public async void SelectedConversationCommand(Object obj)
        {
            if (obj is ConversationModel)
            {
                String conversationId = ((ConversationModel)obj)?.Id;
                if (String.IsNullOrEmpty(conversationId))
                    return;

                XamarinApplication.CurrentConversationId = conversationId;
                await XamarinApplication.NavigationService.NavigateAsync("ConversationStreamPage", conversationId);

            }
        }

        void ConversationLoadMoreItems(Object obj)
        {
            List<Rainbow.Model.Conversation> rbConversations = Helper.SdkWrapper.GetAllConversationsFromCache();
            if (rbConversations != null)
            {
                ResetModelWithRbConversations(rbConversations);
            }
        }

#endregion MANAGE COMMANDS FROM DynamicList


#region PRIVATE METHODS

        private void InitializeSdkObjectsAndEvents()
        {
            // Get Xamarin Application
            XamarinApplication = (App)Xamarin.Forms.Application.Current;

            // Manage event(s) from Rainbow SDK about CONVERSATIONS
            Helper.SdkWrapper.ConversationCreated += RbConversations_ConversationCreated;
            Helper.SdkWrapper.ConversationRemoved += RbConversations_ConversationRemoved;
            Helper.SdkWrapper.ConversationUpdated += RbConversations_ConversationUpdated;

            // Manage event(s) from Rainbow SDK about CONTACTS
            Helper.SdkWrapper.PeerAdded += RbContacts_PeerAdded;
            Helper.SdkWrapper.PeerInfoChanged += RbContacts_PeerInfoChanged;

            // Manage event(s) from Rainbow SDK about BUBBLES
            Helper.SdkWrapper.BubbleInfoUpdated += RbBubbles_BubbleInfoUpdated;
        }

        /// <summary>
        /// Replace the current ViewModel by all Rainbow Conversations provided
        /// </summary>
        /// <param name="rbConversations">List of Rainbow Conversations</param>
        private void ResetModelWithRbConversations(List<Rainbow.Model.Conversation> rbConversations)
        {
            List<ConversationModel> conversationList = new List<ConversationModel>();
            ConversationModel newConversation;
            bool itemAdded;

            foreach (Rainbow.Model.Conversation rbConversation in rbConversations)
            {
                newConversation = Helper.GetConversationFromRBConversation(rbConversation);
                if (newConversation != null)
                {
                    newConversation.SelectionCommand = DynamicList.SelectedItemCommand;

                    // /!\ We must not start Avatar dwl here. We must only do it once they are stored in "Conversations" collection
                    var nb = conversationList.Count;
                    if (nb == 0)
                        conversationList.Add(newConversation);
                    else
                    {
                        itemAdded = false;
                        for (int i = 0; i < nb; i++)
                        {
                            if (newConversation.LastMessageDateTime.ToUniversalTime() > conversationList[i].LastMessageDateTime.ToUniversalTime())
                            {
                                conversationList.Insert(i, newConversation);
                                itemAdded = true;
                                break;
                            }
                        }
                        if (!itemAdded)
                            conversationList.Add(newConversation);
                    }
                }
            }

            lock (lockObservableConversations)
            {
                DynamicList.ReplaceRange(conversationList);
            }

            log.Debug("[ResetModelWithRbConversations] nb RBConversations:[{0}] - nb conversationList:[{1}] - nb Conversations:[{2}]", rbConversations?.Count, conversationList.Count, DynamicList.Items.Count);

            // Check if we need to update the view due to DateTime purpose
            //if (conversationList.Count > 0)
            //    CheckIfUpdateModelForDateTimePurpose(conversationList[0].LastMessageDateTime);
        }

        /// <summary>
        /// Add one RB Conversation in the ViewModel
        /// </summary>
        /// <param name="rbConversation">a Rainbow Conversation object</param>
        private void AddRBConversationToModel(Rainbow.Model.Conversation rbConversation)
        {
            if (rbConversation != null)
            {
                // Now add it to the model
                ConversationModel newConversation;
                newConversation = Helper.GetConversationFromRBConversation(rbConversation);
                if (newConversation != null)
                {
                    newConversation.SelectionCommand = DynamicList.SelectedItemCommand;
                    AddConversationToModel(newConversation);
                }
            }
        }

        private void UpdateRBConversationToModel(Rainbow.Model.Conversation rbConversation)
        {
            // Ensure to be on Main UI Thread
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => UpdateRBConversationToModel(rbConversation));
                return;
            }


            log.Debug("[UpdateRBConversationToModel] - IN");
            if (rbConversation != null)
            {
                // Now add it to the model
                ConversationModel newConversation = Helper.GetConversationFromRBConversation(rbConversation);
                newConversation.SelectionCommand = DynamicList.SelectedItemCommand;

                ConversationModel oldConversation = GetConversationById(newConversation.Id);

                if (oldConversation == null)
                    AddConversationToModel(newConversation);
                else
                {
                    lock (lockObservableConversations)
                    {
                        Boolean needChangeOrder = (oldConversation.LastMessageDateTime != newConversation.LastMessageDateTime);

                        oldConversation.Id = newConversation.Id;
                        oldConversation.Peer = newConversation.Peer;

                        oldConversation.NbMsgUnread = newConversation.NbMsgUnread;

                        oldConversation.LastMessage = newConversation.LastMessage;
                        oldConversation.LastMessageDateTime = newConversation.LastMessageDateTime;

                        if (needChangeOrder)
                        {
                            int oldIndex = 0;
                            int newIndex = -1;

                            int nb = DynamicList.Items.Count;

                            // Get old Index
                            for (int i = 0; i < nb; i++)
                            {
                                if (DynamicList.Items[i].Id == oldConversation.Id)
                                {
                                    oldIndex = i;
                                    break;
                                }
                            }

                            // Try to update order only if are not already the first row
                            if (oldIndex != 0)
                            {
                                // Get new Index
                                for (int i = 0; i < nb; i++)
                                {
                                    if (oldConversation.LastMessageDateTime.ToUniversalTime() > DynamicList.Items[i].LastMessageDateTime.ToUniversalTime())
                                    {
                                        newIndex = i;
                                        break;
                                    }
                                }

                                // If we have found no new index, we need to move it to the end
                                if (newIndex == -1)
                                    newIndex = nb - 1;

                                // Update order only if indexes are different
                                if (oldIndex != newIndex)
                                    DynamicList.Items.Move(oldIndex, newIndex);

                                log.Debug("[UpdateRBConversationToModel] MOVED - oldIndex:[{0}] - newIndex:[{1}] - nb:[{2}] - TopDate:[{3}] - LastMessageDateTime:[{4}]", oldIndex, newIndex, nb, DynamicList.Items[0].LastMessageDateTime.ToString("o"), oldConversation.LastMessageDateTime.ToString("o"));
                            }
                            else
                                log.Debug("[UpdateRBConversationToModel] NOT MOVED - oldIndex:[{0}] - nb:[{1}] - TopDate:[{2}] - LastMessageDateTime:[{3}]", oldIndex, nb, DynamicList.Items[0].LastMessageDateTime.ToString("o"), oldConversation.LastMessageDateTime.ToString("o"));
                        }
                    }

                    // Check if we need to update the view due to DateTime purpose
                    CheckIfUpdateModelForDateTimePurpose(newConversation.LastMessageDateTime);
                }
            }
            log.Debug("[UpdateRBConversationToModel] - OUT");
        }

        /// <summary>
        /// Remove one RB Conversation (by ID) from the ViewModel
        /// </summary>
        /// <param name="id">Rainbow Conversation's ID</param>
        private void RemoveRbConversationFromModel(String id)
        {
            ConversationModel conversation = GetConversationById(id);

            lock (lockObservableConversations)
                DynamicList.Items.Remove(conversation);
        }

        /// <summary>
        /// Add one Conversation Model in the ViewModel
        /// </summary>
        /// <param name="rbConversation">a Rainbow Conversation object</param>
        private void AddConversationToModel(ConversationModel newConversation)
        {
            if (newConversation != null)
            {
                // First remove this conversation if already in the model
                RemoveRbConversationFromModel(newConversation.Id);

                lock (lockObservableConversations)
                {
                    Boolean itemAdded = false;
                    int nb = DynamicList.Items.Count;
                    for (int i = 0; i < nb; i++)
                    {
                        if (newConversation.LastMessageDateTime.ToUniversalTime() > DynamicList.Items[i].LastMessageDateTime.ToUniversalTime())
                        {
                            DynamicList.Items.Insert(i, newConversation);
                            itemAdded = true;
                            break;
                        }
                    }
                    if (!itemAdded)
                    {
                        DynamicList.Items.Add(newConversation);
                    }

                    // Check if we need to update the view due to DateTime purpose
                    CheckIfUpdateModelForDateTimePurpose(newConversation.LastMessageDateTime);
                }
            }
        }

        private ConversationModel GetConversationById(String id)
        {
            ConversationModel conversationFound = null;
            lock (lockObservableConversations)
            {
                foreach (ConversationModel conversation in DynamicList.Items)
                {
                    if (conversation.Id == id)
                    {
                        conversationFound = conversation;
                        break;
                    }
                }
            }
            return conversationFound;
        }

        private void CheckIfUpdateModelForDateTimePurpose(DateTime dt)
        {
            int delay;

            // do we have a DateTime more recent ?
            if (dt.ToUniversalTime() > mostRecentDateTimeMessage.ToUniversalTime())
            {
                delay = Helper.GetRefreshDelay(dt);
                mostRecentDateTimeMessage = dt;

                // If there is not already a delay in progress, we ask a new one
                if (delayUpdateForDateTimePurpose == null)
                {
                    log.Debug("[CheckIfUpdateModelForDateTimePurpose] - Start Delay:[{0}] - DateTime:[{1}] - CASE MORE RECENT", delay, dt.ToString("o"));
                    delayUpdateForDateTimePurpose = CancelableDelay.StartAfter(delay * 1000, () => UpdateModelForDateTimePurpose());
                }
                // There is already a delay running. So we need to cancel it and update display now
                else
                {
                    log.Debug("[CheckIfUpdateModelForDateTimePurpose] - Stop current delay and ask update now - DateTime:[{0}]", dt.ToString("o"));

                    // We stop the current "update delay"
                    delayUpdateForDateTimePurpose.Cancel();

                    // We ask the update now 
                    UpdateModelForDateTimePurpose();
                }
            }
            // We have not a more recent DateTime
            else
            {
                delay = Helper.GetRefreshDelay(mostRecentDateTimeMessage);
                // If there is not already a delay in progress, we ask a new one
                if ((delayUpdateForDateTimePurpose == null) || (!delayUpdateForDateTimePurpose.IsRunning()))
                {
                    log.Debug("[CheckIfUpdateModelForDateTimePurpose] - Start Delay:[{0}] - DateTime:[{1}] - CASE OLDER", delay, dt.ToString("o"));
                    delayUpdateForDateTimePurpose = CancelableDelay.StartAfter(delay * 1000, () => UpdateModelForDateTimePurpose());
                }
            }
        }

        private void UpdateModelForDateTimePurpose()
        {
            log.Debug("[UpdateModelForDateTimePurpose] - IN");
            lock (lockObservableConversations)
            {
                foreach (ConversationModel conversation in DynamicList.Items)
                    conversation.MessageTimeDisplay = Helpers.Helper.HumanizeDateTime(conversation.LastMessageDateTime);
            }

            // We ask the next update 
            CheckIfUpdateModelForDateTimePurpose(mostRecentDateTimeMessage);
            log.Debug("[UpdateModelForDateTimePurpose] - OUT");
        }

#endregion PRIVATE METHODS


#region EVENTS FIRED BY RAINBOW SDK

        private void RbBubbles_BubbleInfoUpdated(object sender, Rainbow.Events.BubbleInfoEventArgs e)
        {
            var conversation = Helper.SdkWrapper.GetOrCreateConversationFromBubbleId(e.BubbleId);
            if (conversation != null)
                UpdateRBConversationToModel(conversation);
        }

        private void RbContacts_PeerAdded(object sender, Rainbow.Events.PeerEventArgs e)
        {
            var conversation = Helper.SdkWrapper.GetConversationByPeerIdFromCache(e.Peer.Id);
            if (conversation != null)
                UpdateRBConversationToModel(conversation);
        }

        private void RbContacts_PeerInfoChanged(object sender, Rainbow.Events.PeerEventArgs e)
        {
            var conversation = Helper.SdkWrapper.GetConversationByPeerIdFromCache(e.Peer.Id);
            if (conversation != null)
                UpdateRBConversationToModel(conversation);
        }

        private void RbConversations_ConversationUpdated(object sender, Rainbow.Events.ConversationEventArgs e)
        {
            if (e.Conversation != null)
                UpdateRBConversationToModel(e.Conversation);
        }

        private void RbConversations_ConversationRemoved(object sender, Rainbow.Events.ConversationEventArgs e)
        {
            // Ensure to be on Main UI Thread
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => RbConversations_ConversationRemoved(sender, e));
                return;
            }

            if (e.Conversation != null)
                RemoveRbConversationFromModel(e.Conversation.Id);
        }

        private void RbConversations_ConversationCreated(object sender, Rainbow.Events.ConversationEventArgs e)
        {
            // Ensure to be on Main UI Thread
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => RbConversations_ConversationCreated(sender, e));
                return;
            }

            if (e.Conversation != null)
                AddRBConversationToModel(e.Conversation);
        }

#endregion EVENTS FIRED BY RAINBOW SDK

    }
}
