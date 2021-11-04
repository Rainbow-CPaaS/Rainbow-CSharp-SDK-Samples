using MultiPlatformApplication.Effects;
using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using Rainbow.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MessageContentBody : ContentView
    {
        Object lockEditor = new Object();
        private MessageElementModel message = null;

        public MessageContentBody()
        {
            InitializeComponent();

            // On Desktop platform, Label can be selected
            if (Helper.IsDesktopPlatform())
                SelectableLabelEffect.SetEnabled(Label, true);

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
                    String backgroundColorKey;

                    if (message.Peer.Id == Helper.SdkWrapper.GetCurrentContactId())
                    {
                        colorKey = "ColorConversationStreamMessageCurrentUserFont";
                        backgroundColorKey = "ColorConversationStreamMessageCurrentUserBackGround";
                    }
                    else
                    {
                        colorKey = "ColorConversationStreamMessageOtherUserFont";
                        backgroundColorKey = "ColorConversationStreamMessageOtherUserBackGround";
                    }

                    Label.TextColor = Helper.GetResourceDictionaryById<Color>(colorKey);
                    BackgroundColor = Helper.GetResourceDictionaryById<Color>(backgroundColorKey);
                }
            }
        }

    }
}