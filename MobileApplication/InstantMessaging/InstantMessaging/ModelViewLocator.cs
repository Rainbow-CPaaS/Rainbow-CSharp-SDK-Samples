using System;
using System.Collections.Generic;
using System.Text;

namespace InstantMessaging
{
    public static class ViewModelLocator
    {
        static ConversationsViewModel conversationsVM;

        public static ConversationsViewModel ConversationsViewModel
        {
            get
            {
                if (conversationsVM == null)
                {
                    conversationsVM = new ConversationsViewModel();
                }
                return conversationsVM;
            }
        }


    }
}
