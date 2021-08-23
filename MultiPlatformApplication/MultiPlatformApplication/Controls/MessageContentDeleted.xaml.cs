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
    public partial class MessageContentDeleted : ContentView
    {
        Boolean manageDisplay = false;

        String peerJid;


        public MessageContentDeleted()
        {
            InitializeComponent();
            this.BindingContextChanged += MessageContentDeleted_BindingContextChanged;
        }

        private void MessageContentDeleted_BindingContextChanged(object sender, EventArgs e)
        {
            if (BindingContext != null)
            {
                MessageElementModel message = (MessageElementModel)BindingContext;
                if (message != null)
                {
                    peerJid = message.Peer.Jid;

                    ManageDisplay();
                }
            }
        }

        private void ManageDisplay()
        {
            if (!manageDisplay)
            {
                manageDisplay = true;
                DisplayDeletedFile();
            }
        }

        private void DisplayDeletedFile()
        {
            String label;
            String colorName;
            String backgroundColorKey;

            if (peerJid == Helper.SdkWrapper.GetCurrentContactJid())
            {
                label = Helper.SdkWrapper.GetLabel("messageSentDeleted");
                colorName = "ColorConversationStreamMessageCurrentUserFont";
                backgroundColorKey = "ColorConversationStreamMessageCurrentUserBackGround";

            }
            else
            {
                label = Helper.SdkWrapper.GetLabel("messageReceivedDeleted");
                colorName = "ColorConversationStreamMessageOtherUserFont";
                backgroundColorKey = "ColorConversationStreamMessageOtherUserBackGround";
            }

            BackgroundColor = Helper.GetResourceDictionaryById<Color>(backgroundColorKey);

            Label.TextType = TextType.Html;
            Label.Text = "<i>" + label + "</i>";
            Label.Opacity = 0.5;
            Label.TextColor = Helper.GetResourceDictionaryById<Color>(colorName);

            Image.HeightRequest = 20;
            Image.WidthRequest = 20;
            Image.Source = Helper.GetImageSourceFromFont("Font_Ban|" + colorName);
            Image.Margin = new Thickness(0, 0, 0, -5);
            Image.Opacity = 0.5;
        }
    }
}