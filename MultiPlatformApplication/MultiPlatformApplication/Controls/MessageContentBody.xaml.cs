using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MessageContentBody : ContentView
    {
        private MessageElementModel message = null;

        public MessageContentBody()
        {
            InitializeComponent();
            this.BindingContextChanged += MessageContentBody_BindingContextChanged;
        }

        private void MessageContentBody_BindingContextChanged(object sender, EventArgs e)
        {
            if ( (BindingContext != null) && (message == null) )
            {
                message = (MessageElementModel)BindingContext;
                if (message != null)
                {
                    String colorKey;

                    if (message.Peer.Id == Helper.SdkWrapper.GetCurrentContactId())
                        colorKey = "ColorConversationStreamMessageCurrentUserFont";
                    else
                        colorKey = "ColorConversationStreamMessageOtherUserFont";

                    Label.TextColor = Helper.GetResourceDictionaryById<Color>(colorKey);
                }
            }
        }
    }
}