using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using Microsoft.Extensions.Logging;
using Rainbow;
using Rainbow.Common;
using Rainbow.Model;
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
    public partial class MessageContentReply : ContentView
    {
        private static readonly ILogger log = Rainbow.LogFactory.CreateLogger<MessageContentReply>();

        private static readonly int MAX_IMAGE_SIZE = 60;

        private MessageElementModel message;

        private Boolean initEventsDone = false;

        private String peerJid = null;
        private String attachmentId = null;

        private Boolean usedAsInput = false;

        public MessageContentReply()
        {
            InitializeComponent();

            BowViewForMinimalWidth.MinimumWidthRequest = MessageContent.MINIMAL_MESSAGE_WIDTH;

            this.BindingContextChanged += MessageContentReply_BindingContextChanged;
        }

        public void SetUsageMode(Boolean usedAsInput)
        {
            if (!this.usedAsInput)
                this.usedAsInput = usedAsInput;
        }

        private void InitEvents()
        {
            if (!initEventsDone)
            {
                initEventsDone = true;
                Helper.SdkWrapper.PeerAdded += SdkWrapper_PeerAdded;
                Helper.SdkWrapper.PeerInfoChanged += SdkWrapper_PeerInfoChanged;
            }
        }

        private void MessageContentReply_BindingContextChanged(object sender, EventArgs e)
        {
            if (BindingContext != null)
            {
                try
                {
                    message = (MessageElementModel)BindingContext;
                }
                catch { }

                if ((message != null) && !String.IsNullOrEmpty(message.ConversationId))
                {
                    String backgroundColorKey;
                    if ( (message.Peer.Id != Helper.SdkWrapper.GetCurrentContactId()) || usedAsInput)
                        backgroundColorKey = "ColorConversationStreamMessageOtherUserBackGround"; 
                    else
                        backgroundColorKey = "ColorConversationStreamMessageCurrentUserBackGround";
                    BackgroundColor = Helper.GetResourceDictionaryById<Color>(backgroundColorKey);

                    // By default there is no Image visible
                    Image.IsVisible = true;

                    InitEvents();

                    // We need to get Name and text of the replied message ...
                    Rainbow.Model.Message rbRepliedMessage = Helper.SdkWrapper.GetOneMessageFromConversationIdFromCache(message.ConversationId, message.Reply.Id);

                    if (rbRepliedMessage != null)
                    {
                        log.LogDebug("[MessageContentReply_BindingContextChanged] Id:[{0}] - Reply.Id:[{1}] - Content:[2}]", message.Id, message.Reply.Id, rbRepliedMessage.Content);
                        SetReplyPartOfMessage(rbRepliedMessage);
                    }
                    else
                        AskMessageInfo();
                }
            }
        }

        private void AskMessageInfo()
        {
            Helper.SdkWrapper.GetOneMessageFromConversationId(message.ConversationId, message.Reply.Id, message.Reply.Stamp, callback =>
            {
                if (callback.Result.Success)
                {
                    Rainbow.Model.Message rbMessage = callback.Data;
                    if (rbMessage != null)
                    {
                        log.LogDebug("[AskMessageInfo] Id:[{0}] - Reply.Id:[{1}] - Content:[2}]", message.Id, message.Reply.Id, rbMessage.Content);
                        SetReplyPartOfMessage(rbMessage);
                    }
                }
            });

        }

        private void SdkWrapper_PeerInfoChanged(object sender, Rainbow.Events.PeerEventArgs e)
        {
            if (peerJid == e.Peer.Jid)
                UpdateContactInfo();
        }

        private void SdkWrapper_PeerAdded(object sender, Rainbow.Events.PeerEventArgs e)
        {
            if (peerJid == e.Peer.Jid)
                UpdateContactInfo();
        }

        private Boolean UpdateContactInfo()
        {
            if (message?.Reply?.Peer.Jid != null)
            {
                Rainbow.Model.Contact contactReply = Helper.SdkWrapper.GetContactFromContactJid(message.Reply.Peer.Jid);
                if (contactReply != null)
                {
                    message.Reply.Peer.Id = contactReply.Id;
                    message.Reply.Peer.DisplayName = Rainbow.Util.GetContactDisplayName(contactReply);
                    UpdateDisplay();
                    return true;
                }
            }
            return false;
        }

        private void UpdateDisplay()
        {
            // Ensure to be on Main UI Thread
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => UpdateDisplay());
                return;
            }

            if (message?.Reply?.Peer?.Jid == Helper.SdkWrapper.GetCurrentContactJid())
                LabelDisplayName.Text = Helper.SdkWrapper.GetLabel("me");
            else
                LabelDisplayName.Text = message?.Reply?.Peer?.DisplayName;
            LabelBody.Text = message?.Reply?.Content?.Body;
        }

        private void SetReplyPartOfMessage(Rainbow.Model.Message rbRepliedMessage)
        {
            if (message == null)
                return;

            if (message.Reply == null)
                return;

            // Store Jid of the message sender
            message.Reply.Peer.Jid = rbRepliedMessage.FromJid;
            peerJid = rbRepliedMessage.FromJid;

            if (String.IsNullOrEmpty(rbRepliedMessage.Content))
            {
                if (rbRepliedMessage.FileAttachment != null)
                {
                    message.Reply.Content.Body = rbRepliedMessage.FileAttachment.Name;
                    message.Reply.Content.Attachment = new MessageAttachmentModel()
                    {
                        Id = rbRepliedMessage.FileAttachment.Id,
                        Name = rbRepliedMessage.FileAttachment.Name,
                        Size = Helper.HumanizeFileSize(rbRepliedMessage.FileAttachment.Size)
                    };

                    attachmentId = message.Reply.Content.Attachment.Id;

                    // Need to manage Thumbnail
                    if (Helper.SdkWrapper.IsThumbnailFileAvailable(message.ConversationId, attachmentId, message.Reply.Content.Attachment.Name))
                    {
                        UpdateThumbnailDisplay();
                    }
                    else
                    {
                        // Manage event(s) from FilePool
                        Helper.SdkWrapper.ThumbnailAvailable += SdkWrapper_ThumbnailAvailable;

                        // Ask more info about this file
                        Helper.SdkWrapper.AskFileDescriptorDownload(message.ConversationId, attachmentId);

                        DisplayAttachment(rbRepliedMessage.FileAttachment.Name);
                    }
                }
                else
                    message.Reply.Content.Body = "File id: " + rbRepliedMessage.Id; // Bad display but should permit to debug this situation
            }
            else
                message.Reply.Content.Body = Helper.ReplaceCRLFFromString(rbRepliedMessage.Content, " ");

            log.LogDebug("[SetReplyPartOfMessage] - message.Id:[{0}] - replyMsgId:[{1}] - replyBody:[{2}] - ContactJid:[{3}]", message.Id, rbRepliedMessage.Id, message.Reply.Content.Body, rbRepliedMessage.FromJid);


            if (!UpdateContactInfo())
            {
                log.LogDebug("[SetReplyPartOfMessage] - message.Id:[{0}] - replyMsgId:[{1}] - Unknown Contact Jid:[{2}]", message.Id, rbRepliedMessage.Id, rbRepliedMessage.FromJid);

                // We ask to have more info about this contact using AvatarPool
                Helper.SdkWrapper.AddUnknownContactToPoolByJid(rbRepliedMessage.FromJid);
            }
            else
                UpdateDisplay();
        }

        private void DisplayAttachment(String fileName)
        {
            // Ensure to be on Main UI Thread
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => DisplayAttachment(fileName));
                return;
            }

            String imageSourceId = Helper.GetFileSourceIdFromFileName(fileName);
            Image.HeightRequest = MAX_IMAGE_SIZE;
            Image.WidthRequest = MAX_IMAGE_SIZE;
            Image.Source = Helper.GetImageSourceFromFont(imageSourceId);
            Image.IsVisible = true;
        }

        private void SdkWrapper_ThumbnailAvailable(object sender, Rainbow.Events.IdEventArgs e)
        {
             if (attachmentId == e.Id)
                UpdateThumbnailDisplay();
        }

        private void UpdateThumbnailDisplay()
        {
            // Ensure to be on Main UI Thread
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => UpdateThumbnailDisplay());
                return;
            }

            string filePath = Helper.SdkWrapper.GetThumbnailFullFilePath(attachmentId);
            try
            {
                log.LogDebug("[DisplayThumbnail] FileId:[{0}] - Use filePath:[{1}]", attachmentId, filePath);
                System.Drawing.Size size = ImageTools.GetImageSize(filePath);
                if ((size.Width > 0) && (size.Height > 0))
                {
                    double density = Helper.GetDensity();

                    // Avoid to have a thumbnail too big
                    float scaleWidth = (float)(MAX_IMAGE_SIZE * density) / (float)size.Width;
                    float scaleHeight = (float)(MAX_IMAGE_SIZE * density) / (float)size.Height;
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
                    Image.IsVisible = true;

                    //if we have a thumbnail, we don't display the file name
                    LabelBody.Text = "";
                }
            }
            catch { }
        }
    }
}