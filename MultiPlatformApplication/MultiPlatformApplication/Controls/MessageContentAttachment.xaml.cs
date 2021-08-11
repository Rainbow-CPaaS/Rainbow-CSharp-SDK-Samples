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

        Boolean manageDisplay = false;

        String peerJid;
        String attachmentId;
        String attachmentName;
        String attachmentSize;
        String conversationId;

        public MessageContentAttachment()
        {
            InitializeComponent();

            this.BindingContextChanged += MessageContentAttachment_BindingContextChanged;
        }

        private void MessageContentAttachment_BindingContextChanged(object sender, EventArgs e)
        {
            if ( BindingContext != null )
            {
                MessageElementModel message = (MessageElementModel)BindingContext;
                if (message != null)
                {
                    if (message.Content.Attachment != null)
                    {
                        conversationId = message.ConversationId;
                        peerJid = message.Peer.Jid;
                        attachmentId = message.Content.Attachment.Id;
                        attachmentName = message.Content.Attachment.Name;
                        attachmentSize = message.Content.Attachment.Size;

                        ManageDisplay();
                    }
                }
            }
        }

        private void ManageDisplay()
        {
            if(!manageDisplay)
            {
                manageDisplay = true;

                // Manage event(s) from FilePool
                Helper.SdkWrapper.ThumbnailAvailable += SdkWrapper_ThumbnailAvailable;
                Helper.SdkWrapper.FileDescriptorNotAvailable += SdkWrapper_FileDescriptorNotAvailable;

                if (Helper.SdkWrapper.IsThumbnailFileAvailable(conversationId, attachmentId, attachmentName))
                {
                    DisplayThumbnail();
                }
                else if (Helper.SdkWrapper.IsFileDescriptorNotAvailable(attachmentId))
                {
                    DisplayDeletedFile();
                }
                else 
                {
                    if (peerJid == Helper.SdkWrapper.GetCurrentContactJid())
                    {
                        Frame.BackgroundColor = Color.Transparent;
                        Label.TextColor = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageCurrentUserFont");
                    }
                    else
                    {
                        Frame.BackgroundColor = Helper.GetResourceDictionaryById<Color>("ColorAttachmentBackground");
                        Label.TextColor = Helper.GetResourceDictionaryById<Color>("ColorAttachmentText");
                    }

                    // Ask more info about this file
                    Helper.SdkWrapper.AskFileDescriptorDownload(conversationId, attachmentId);

                    // File has no thumbnail or it's not an image file
                    Label.TextType = TextType.Text;
                    Label.Text = attachmentName + " - " + attachmentSize;
                    Label.Opacity = 1;

                    String imageSourceId = Helper.GetFileSourceIdFromFileName(attachmentName);
                    Image.HeightRequest = 30;
                    Image.WidthRequest = 30;
                    Image.Source = Helper.GetImageSourceFromFont(imageSourceId);
                    Image.Margin = new Thickness(0);
                    Image.Opacity = 1;

                    Spinner.IsVisible = false; 
                }
            }
        }

        private void SdkWrapper_FileDescriptorNotAvailable(object sender, Rainbow.Events.IdEventArgs e)
        {
            if (attachmentId == e.Id)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DisplayDeletedFile();
                });
            }
        }

        private void DisplayDeletedFile()
        {
            String label;
            String colorName;

            if (peerJid == Helper.SdkWrapper.GetCurrentContactJid())
            {
                label = Helper.GetLabel("messageSentDeleted");
                colorName = "ColorConversationStreamMessageCurrentUserFont";
            }
            else
            {
                label = Helper.GetLabel("messageReceivedDeleted");
                colorName = "ColorConversationStreamMessageOtherUserFont";
            }

            Frame.BackgroundColor = Color.Transparent;
            Spinner.IsVisible = false;

            Label.TextType = TextType.Html;
            Label.Text = "<i>" + label + "</i>";
            Label.Opacity = 0.5;
            Label.IsVisible = true;
            Label.TextColor = Helper.GetResourceDictionaryById<Color>(colorName);

            Image.HeightRequest = 20;
            Image.WidthRequest = 20;
            Image.Source = Helper.GetImageSourceFromFont("Font_Ban|" + colorName);
            Image.Margin = new Thickness(0, 0, 0, -5);
            Image.Opacity = 0.5;
            Image.Margin = new Thickness(0);
        }

        private void DisplayThumbnail()
        {
            string filePath = Helper.SdkWrapper.GetThumbnailFullFilePath(attachmentId);
            try
            {
                Frame.BackgroundColor = Color.Transparent;
                Spinner.IsVisible = false;

                log.Debug("[DisplayThumbnail] FileId:[{0}] - Use filePath:[{1}]", attachmentId, filePath);
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
                    Image.Opacity = 1;

                    // To center horizontally Imge if its size is small
                    if(Image.WidthRequest < MessageContent.MINIMAL_MESSAGE_WIDTH)
                    {
                        double m = (double)((MessageContent.MINIMAL_MESSAGE_WIDTH - Image.WidthRequest) / 2);
                        Image.Margin = new Thickness(m,0);
                    }

                    Label.IsVisible = false;
                }
            }
            catch { }
        }

        private void SdkWrapper_ThumbnailAvailable(object sender, Rainbow.Events.IdEventArgs e)
        {
            if (attachmentId == e.Id)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DisplayThumbnail();
                });
            }
        }
    }
}