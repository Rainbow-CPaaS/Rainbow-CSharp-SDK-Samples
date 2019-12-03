using System;

using Rainbow.Helpers;

namespace InstantMessaging.Model
{
    public class ConversationStream : ObservableObject
    {
        String loadingIndicatorIsVisibleheaderIsVisible;
        String listViewIsEnabled;

        public string LoadingIndicatorIsVisible
        {
            get { return loadingIndicatorIsVisibleheaderIsVisible; }
            set { SetProperty(ref loadingIndicatorIsVisibleheaderIsVisible, value); }
        }

        public string ListViewIsEnabled
        {
            get { return listViewIsEnabled; }
            set { SetProperty(ref listViewIsEnabled, value); }
        }
    }
}
