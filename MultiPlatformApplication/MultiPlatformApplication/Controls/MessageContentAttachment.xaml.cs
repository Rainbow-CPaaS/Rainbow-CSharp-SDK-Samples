using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using NLog;
using Rainbow;
using Rainbow.Common;
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
    public partial class MessageContentAttachment : ContentView
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(MessageContentAttachment));

        private MessageElementModel message = null;

        public MessageContentAttachment()
        {
            InitializeComponent();

            this.BindingContextChanged += MessageContentAttachment_BindingContextChanged;
        }

        private void MessageContentAttachment_BindingContextChanged(object sender, EventArgs e)
        {
            if ( (BindingContext != null) && (message == null) )
            {
                message = (MessageElementModel)BindingContext;
                if (message != null)
                {
                    if (message.Content.Attachment != null)
                    {
                        if (message.Peer.Id == Helper.SdkWrapper.GetCurrentContactId())
                        {
                            Frame.BackgroundColor = Color.Transparent;
                            Label.TextColor = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageCurrentUserFont");
                        }


                        if (Helper.SdkWrapper.IsThumbnailFileAvailable(message.ConversationId, message.Content.Attachment.Id, message.Content.Attachment.Name))
                        {
                            DisplayThumbnail();
                        }
                        else
                        {
                            // Manage event(s) from FilePool
                            Helper.SdkWrapper.ThumbnailAvailable += SdkWrapper_ThumbnailAvailable;

                            // Ask more info about this file
                            Helper.SdkWrapper.AskFileDescriptorDownload(message.ConversationId, message.Content.Attachment.Id);

                            // CASE: File has no thumbnail or it's not an image file
                            Label.Text = message.Content.Attachment.Name + " - " + message.Content.Attachment.Size;

                            String imageSourceId = Helper.GetFileSourceIdFromFileName(message.Content.Attachment.Name);
                            Image.HeightRequest = 30;
                            Image.WidthRequest = 30;
                            Image.Source = Helper.GetImageSourceFromFont(imageSourceId);
                        }
                    }
                }
            }
        }

        private void DisplayThumbnail()
        {
            string filePath = Helper.SdkWrapper.GetThumbnailFullFilePath(message.Content.Attachment.Id);
            try
            {
                log.Debug("[DisplayThumbnail] FileId:[{0}] - Use filePath:[{1}]", message.Content.Attachment.Id, filePath);
                System.Drawing.Size size = ImageTools.GetImageSize(filePath);
                if ((size.Width > 0) && (size.Height > 0))
                {
                    double density = Helper.GetDensity();

                    // Avoid to have a thumbnail too big
                    float scaleWidth = (float)(Helper.SdkWrapper.MaxThumbnailWidth * density) / (float)size.Width;
                    float scaleHeight = (float)(Helper.SdkWrapper.MaxThumbnailHeight * density) / (float)size.Height;
                    float scale = Math.Min(scaleHeight, scaleWidth);

                    // Don't increase size of the thumbnail 
                    if (scale > 1)
                        scale = 1;

                    // Calculate size of the thumbnail
                    int w = (int)(size.Width * scale);
                    int h = (int)(size.Height * scale);

                    Image.HeightRequest = (int)Math.Round(h / density);
                    Image.WidthRequest = (int)Math.Round(w / density);
                    Image.Source = ImageSource.FromFile(filePath);

                    

                    Label.IsVisible = false;
                }
            }
            catch { }
        }

       

        private void SdkWrapper_ThumbnailAvailable(object sender, Rainbow.Events.IdEventArgs e)
        {
            if (message.Content?.Attachment?.Id == e.Id)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DisplayThumbnail();
                });
            }
        }
    }
}