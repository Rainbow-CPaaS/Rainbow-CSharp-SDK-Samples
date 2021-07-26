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
    public partial class MessageContentUrgency : ContentView
    {
        private MessageElementModel message = null;

        public MessageContentUrgency()
        {
            InitializeComponent();
            this.BindingContextChanged += MessageContentUrgency_BindingContextChanged;
        }

        private void MessageContentUrgency_BindingContextChanged(object sender, EventArgs e)
        {
            if ( (BindingContext != null) && (message == null) )
            {
                message = (MessageElementModel)BindingContext;
                if (message?.Content.Urgency != Rainbow.Model.UrgencyType.Std)
                {
                    String backgroudColorKey = null;
                    String colorKey = null;
                    Color color;
                    String labelKey = null;
                    String imageSourceKey = null;

                    switch (message.Content.Urgency)
                    {
                        case Rainbow.Model.UrgencyType.High:
                            backgroudColorKey = "ColorBackgroundUrgencyEmergency";
                            colorKey = "ColorUrgencyEmergency";
                            labelKey = "emergencyAlert";
                            imageSourceKey = "Font_Fire";
                            break;

                        case Rainbow.Model.UrgencyType.Middle:
                            backgroudColorKey = "ColorBackgroundUrgencyImportant";
                            colorKey = "ColorUrgencyImportant";
                            labelKey = "warningAlert";
                            imageSourceKey = "Font_ExclamationTriangle";
                            break;

                        case Rainbow.Model.UrgencyType.Low:
                            backgroudColorKey = "ColorBackgroundUrgencyInformation";
                            colorKey = "ColorUrgencyInformation";
                            labelKey = "notifyAlert";
                            imageSourceKey = "Font_Lightbulb";
                            break;
                    }

                    color = Helper.GetResourceDictionaryById<Color>(colorKey);

                    StackLayout.BackgroundColor = Helper.GetResourceDictionaryById<Color>(backgroudColorKey);

                    Label.TextColor = color;
                    Label.Text = Helper.GetLabel(labelKey);

                    Image.Source = Helper.GetImageSourceFromFont(imageSourceKey + "|" + color.ToHex());
                }
            }
        }
    }
}