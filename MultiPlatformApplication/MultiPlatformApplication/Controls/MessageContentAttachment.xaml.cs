using MultiPlatformApplication.Effects;
using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using NLog;
using Rainbow;
using Rainbow.Common;
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
    public partial class MessageContentAttachment : ContentView
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(MessageContentAttachment));

        Boolean manageDisplay = false;

        String peerJid;
        String conversationId;

        String attachmentId;
        String attachmentName;
        String attachmentSize;
        String attachmentAction;

        double uploadProgress = 0;
        double downloadProgress = 0;

        Boolean uploadState = false;
        Boolean downloadState = false;
        Boolean thumbnailUsed = false;

        MouseOverAndOutModel mouseCommands;
        TapGestureRecognizer tapGestureRecognizer;

        ImageSource imageSource;
        ImageSource imageSourceOnOver;

        public MessageContentAttachment()
        {
            InitializeComponent();

            this.BindingContextChanged += MessageContentAttachment_BindingContextChanged;
        }

        private void MessageContentAttachment_BindingContextChanged(object sender, EventArgs e)
        {
            if (BindingContext != null)
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
                        attachmentAction = message.Content.Attachment.Action;

                        ManageDisplay();
                    }
                }
            }
        }

        private void ManageDisplay()
        {
            if (!manageDisplay)
            {
                manageDisplay = true;

                // Manage event(s) from FilePool
                Helper.SdkWrapper.ThumbnailAvailable += SdkWrapper_ThumbnailAvailable;
                Helper.SdkWrapper.FileDescriptorNotAvailable += SdkWrapper_FileDescriptorNotAvailable;


                if (attachmentAction?.Equals("upload", StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    Helper.SdkWrapper.FileUploadUpdated += SdkWrapper_FileUploadUpdated;
                    uploadState = true;
                    DisplayProgress(uploadProgress);
                }
                else if (Helper.SdkWrapper.IsThumbnailFileAvailable(conversationId, attachmentId, attachmentName))
                {
                    DisplayThumbnail();
                }
                else if (Helper.SdkWrapper.IsFileDescriptorNotAvailable(attachmentId))
                {
                    DisplayDeletedFile();
                }
                else
                {
                    Helper.SdkWrapper.FileDownloadUpdated += SdkWrapper_FileDownloadUpdated;
                    DisplayAttachment();
                }
            }
        }

         private void DisplayProgress(double progress)
        {
            // The display is nearly the same than a basic attachment - only the spinner is vislble

            DisplayAttachment();

            Spinner.Progress = progress;
            Spinner.IsVisible = true;
        }

        private void DisplayAttachment()
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

            // No Spinner visible
            Spinner.IsVisible = false;

            // File has no thumbnail or it's not an image file
            Label.TextType = TextType.Text;
            Label.Text = attachmentName + " - " + attachmentSize;
            Label.Opacity = 1;

            String imageSourceId = Helper.GetFileSourceIdFromFileName(attachmentName);
            imageSource = Helper.GetImageSourceFromFont(imageSourceId);

            Image.HeightRequest = 30;
            Image.WidthRequest = 30;
            Image.Source = imageSource;
            Image.Margin = new Thickness(0);
            Image.Opacity = 1;

            thumbnailUsed = false;
            AddMouseOverAndOutEffects();
        }

        private void DisplayDeletedFile()
        {
            thumbnailUsed = false;

            String label;
            String colorName;

            if (peerJid == Helper.SdkWrapper.GetCurrentContactJid())
            {
                label = Helper.SdkWrapper.GetLabel("messageSentDeleted");
                colorName = "ColorConversationStreamMessageCurrentUserFont";
            }
            else
            {
                label = Helper.SdkWrapper.GetLabel("messageReceivedDeleted");
                colorName = "ColorConversationStreamMessageOtherUserFont";
            }

            // No Spinner visible
            Spinner.IsVisible = false;

            Frame.BackgroundColor = Color.Transparent;

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

                // No Spinner visible
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

                    imageSource = ImageSource.FromFile(filePath);

                    Image.HeightRequest = (int)Math.Round(h / density);
                    Image.WidthRequest = (int)Math.Round(w / density);
                    Image.Source = imageSource;
                    Image.Opacity = 1;

                    // To center horizontally Imge if its size is small
                    if (Image.WidthRequest < MessageContent.MINIMAL_MESSAGE_WIDTH)
                    {
                        double m = (double)((MessageContent.MINIMAL_MESSAGE_WIDTH - Image.WidthRequest) / 2);
                        Image.Margin = new Thickness(m, 0);
                    }

                    Label.IsVisible = false;
                }

                thumbnailUsed = true;
                AddMouseOverAndOutEffects();
            }
            catch { }
        }

        private void UpdateDisplayDownload()
        {
            // Ensure to be on Main UI Thread
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => UpdateDisplayDownload());
                return;
            }

            if (downloadProgress >= 100)
            {
                downloadState = false;
                Spinner.IsVisible = false;
                Spinner.Progress = 0;
            }
            else if (downloadProgress > Spinner.Progress)
            {
                Spinner.IsVisible = true;
                Spinner.Progress = downloadProgress;
            }
        }

        private void UpdateDisplayUpload()
        {
            // Ensure to be on Main UI Thread
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => UpdateDisplayUpload());
                return;
            }

            if (uploadProgress >= 100)
            {
                uploadState = false;
                Spinner.IsVisible = false;
                Spinner.Progress = 0;
            }
            else if (uploadProgress > Spinner.Progress)
                Spinner.Progress = uploadProgress;
        }

#region EVENTS FROM SDK WRAPPER

        private void SdkWrapper_ThumbnailAvailable(object sender, Rainbow.Events.IdEventArgs e)
        {
            // Ensure to be on Main UI Thread
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => SdkWrapper_ThumbnailAvailable(sender, e));
                return;
            }

            if (attachmentId == e.Id)
                DisplayThumbnail();
        }

        private void SdkWrapper_FileDescriptorNotAvailable(object sender, Rainbow.Events.IdEventArgs e)
        {
            // Ensure to be on Main UI Thread
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => SdkWrapper_FileDescriptorNotAvailable(sender, e));
                return;
            }

            if (attachmentId == e.Id)
                DisplayDeletedFile();
        }

        private void SdkWrapper_FileUploadUpdated(object sender, Rainbow.Events.FileUploadEventArgs e)
        {
            if (attachmentId == e.FileDescriptor.Id)
            {
                if (e.InProgress)
                {
                    if (e?.FileDescriptor?.Size > 0)
                    {
                        double size = e.FileDescriptor.Size;
                        uploadProgress = ((e.SizeUploaded / size) * 100);
                        UpdateDisplayUpload();
                    }
                }
                else if (e.Completed)
                {
                    // Upload finished
                    uploadProgress = 100;
                    UpdateDisplayUpload();
                }
                else
                {
                    // TODO - Error occurred
                }
            }
        }

        private void SdkWrapper_FileDownloadUpdated(object sender, Rainbow.Events.FileDownloadEventArgs e)
        {
            if (attachmentId == e.FileId)
            {
                downloadState = true;

                if (e.InProgress)
                {
                    if (e.FileSize> 0)
                    {
                        double size = e.FileSize;
                        downloadProgress = ((e.SizeDownloaded/ size) * 100);
                        UpdateDisplayDownload();
                    }
                }
                else if (e.Completed)
                {
                    // Upload finished
                    downloadProgress = 100;
                    UpdateDisplayDownload();
                }
                else
                {
                    // TODO - Error occurred
                }
            }
        }

#endregion EVENTS FROM SDK WRAPPER

        #region MOUSE RELATED: OVER - OUT - TAPPED/CLICKED
        private void AddMouseOverAndOutEffects()
        {
            bool removeEffects = true;
            if (Helper.IsDesktopPlatform())
            {
                if (!thumbnailUsed)
                {
                    // If there is no upload and no  download in progress, we want to offer download action 
                    if ((!uploadState) && (!downloadState))
                    {
                        removeEffects = false;

                        // Create Image used when "mouse over"
                        imageSourceOnOver = Helper.GetImageSourceFromFont("Font_FileDownload|" + Label.TextColor.ToHex());

                        // Create Mouse Commands if necessary
                        if (mouseCommands == null)
                        {
                            mouseCommands = new MouseOverAndOutModel();
                            mouseCommands.MouseOverCommand = new RelayCommand<object>(new Action<object>(MouseOverCommand));
                            mouseCommands.MouseOutCommand = new RelayCommand<object>(new Action<object>(MouseOutCommand));
                        }

                        //Add mouse over / out effects
                        MouseOverEffect.SetCommand(Image, mouseCommands.MouseOverCommand);
                        MouseOutEffect.SetCommand(Image, mouseCommands.MouseOutCommand);

                        // Create/Add Tap Gesture Recognizer if necessary
                        if (tapGestureRecognizer == null)
                        {
                            tapGestureRecognizer = new TapGestureRecognizer();
                            tapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;
                            Image.GestureRecognizers.Add(tapGestureRecognizer);
                        }
                    }
                    else
                    {
                        // Remove Image used when "mouse over"
                        imageSourceOnOver = null;
                    }
                }

                if (removeEffects)
                {
                    Helper.RemoveEffect(Image, typeof(MouseOverEffect));
                    Helper.RemoveEffect(Image, typeof(MouseOutEffect));
                }
            }
        }

        private void MouseOverCommand(object obj)
        {
            if ((!uploadState) && (!downloadState) && (!thumbnailUsed))
            {
                Image.Source = imageSourceOnOver;
            }
        }

        private void MouseOutCommand(object obj)
        {
            if ((!uploadState) && (!downloadState) && (!thumbnailUsed))
            {
                Image.Source = imageSource;
            }
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if ((!uploadState) && (!downloadState) && (!thumbnailUsed))
            {
                // Set to default display
                MouseOutCommand(null);

                // Start download
                Helper.SdkWrapper.OnStartFileDownload(this, new IdEventArgs(attachmentId));
            }
        }

#endregion MOUSE RELATED: OVER - OUT - TAPPED/CLICKED
    }
}