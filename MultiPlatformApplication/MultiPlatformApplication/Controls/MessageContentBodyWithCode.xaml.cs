using MultiPlatformApplication.Effects;
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
    public partial class MessageContentBodyWithCode : ContentView
    {
        private MessageElementModel message = null;

        public MessageContentBodyWithCode()
        {
            InitializeComponent();

            // On Desktop platform, Label can be selected
            if (Helper.IsDesktopPlatform())
                SelectableLabelEffect.SetEnabled(Label, true);

            this.BindingContextChanged += MessageContentBodyWithCode_BindingContextChanged;
        }

        private void MessageContentBodyWithCode_BindingContextChanged(object sender, EventArgs e)
        {
            if ((BindingContext != null) && (message == null))
            {
                message = (MessageElementModel)BindingContext;
                if (message != null)
                {
                    // Manage Image / Uri
                    String body = message.Content?.Body;

                    if(body.StartsWith("/code", StringComparison.InvariantCultureIgnoreCase))
                    {
                        body = body.Substring(6);

                        body = body.Trim().TrimStart('\r', '\n').TrimEnd('\r', '\n');

                        Label.Text = body;
                    }
                }
            }
        }
    }
}