﻿using MultiPlatformApplication.Helpers;
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
    public partial class MessageContentForward : ContentView
    {
        Boolean manageDisplay = false;

        String peerJid;

        public MessageContentForward()
        {
            InitializeComponent();

            this.BindingContextChanged += MessageContentForward_BindingContextChanged;
        }

        private void MessageContentForward_BindingContextChanged(object sender, EventArgs e)
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
                DisplayForwarded();
            }
        }

        private void DisplayForwarded()
        {
            String label;
            String colorName;

            label = Helper.SdkWrapper.GetLabel("msgForwarded");
            if (peerJid == Helper.SdkWrapper.GetCurrentContactJid())
                colorName = "ColorConversationStreamMessageCurrentUserFont";
            else
                colorName = "ColorConversationStreamMessageOtherUserFont";

            //Label.TextType = TextType.Html;
            Label.Text = label;
            Label.Opacity = 0.5;
            Label.TextColor = Helper.GetResourceDictionaryById<Color>(colorName);

            Image.HeightRequest = 20;
            Image.WidthRequest = 20;
            Image.Source = Helper.GetImageSourceFromFont("Font_ArrowRight|" + colorName);
            Image.Margin = new Thickness(0, 0, 0, -5);
            Image.Opacity = 0.5;
        }
    }
}