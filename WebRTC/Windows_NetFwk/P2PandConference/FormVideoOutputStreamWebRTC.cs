using Rainbow;
using Rainbow.Model;
using Rainbow.WebRTC;
using Rainbow.WebRTC.Desktop;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SDK.UIForm.WebRTC
{
    public partial class FormVideoOutputStreamWebRTC : Form
    {
        private Rainbow.Application? _rbApplication = null;
        private Rainbow.Contacts? _rbContacts = null;

        private Object lockAboutImageManagement = new Object();

        private int _diffWidth = 0;
        private int _diffHeight = 0;
        private float _ratio = 0;

        private Rainbow.WebRTC.WebRTCCommunications? _webRTCCommunications = null;
        private MediaStreamTrackDescriptor? mediaInputStreamDescriptor;
        private MediaPublication? mediaPublication;
        private MediaService? mediaService;
        private VideoStreamTrack? videoStreamTrack = null;

        private String? _currentCallId = null;
        private String? _currentContactId = null;
        Boolean _initDone = false;

        public FormVideoOutputStreamWebRTC()
        {
            this.HandleCreated += Form_HandleCreated;
            InitializeComponent();
        }

        private void Form_HandleCreated(object sender, EventArgs e)
        {
            _ratio = ((float)pb_VideoStream.Size.Width / (float)pb_VideoStream.Size.Height);

            _diffWidth = this.Width - pb_VideoStream.Size.Width;
            _diffHeight = this.Height - pb_VideoStream.Size.Height;
        }

        public void SetApplicationAndCallId(Rainbow.Application rbApp, String callId)
        {
            if (rbApp != null)
            {
                _rbApplication = rbApp;
                _rbContacts = _rbApplication.GetContacts();

                _currentContactId = _rbContacts.GetCurrentContactId();

                _webRTCCommunications = Rainbow.WebRTC.WebRTCCommunications.GetOrCreateInstance(_rbApplication, null);
                if (_webRTCCommunications != null)
                {
                    _currentCallId = callId;

                    // Show only Hold / Unhold in Conference
                    var call = _webRTCCommunications.GetCall(callId);
                    btn_Hold.Visible = (call?.IsConference == true);
                    btn_Unhold.Visible = (call?.IsConference == true);

                    _webRTCCommunications.CallUpdated += WebRTCCommunications_CallUpdated;
                    _webRTCCommunications.OnMediaPublicationUpdated += WebRTCCommunications_OnMediaPublicationUpdated;
                    _webRTCCommunications.OnMediaServiceUpdated += WebRTCCommunications_OnMediaServiceUpdated;
                }
            }
        }

        public void SetMediaStreamTrackDescriptor(MediaStreamTrackDescriptor? newMediaInputStreamDescriptor)
        {
            // Dispose previous VideoStreamTrack
            if (videoStreamTrack != null)
            {
                videoStreamTrack.OnImage -= VideoStreamTrack_OnImage;
                videoStreamTrack.Dispose();
            }

            if (newMediaInputStreamDescriptor == null)
                return;

            mediaInputStreamDescriptor = newMediaInputStreamDescriptor;


            if(mediaInputStreamDescriptor.IsMediaService)
            {
                mediaPublication = null;

                var mediaServices = _webRTCCommunications.GetMediaServicesAvailable(_currentCallId);
                mediaService = mediaServices.Find(ms => (ms.ServiceId == mediaInputStreamDescriptor.PublisherId));
            }
            else
            {
                mediaService = null;

                var mediaPublications = _webRTCCommunications.GetMediaPublicationsSubscribed(_currentCallId);
                mediaPublication = mediaPublications.Find(mp => (mp.PublisherId == mediaInputStreamDescriptor.PublisherId) && (mp.Media == mediaInputStreamDescriptor.Media));
            }


            if (newMediaInputStreamDescriptor.MediaStreamTrack != null)
            {
                videoStreamTrack = (VideoStreamTrack)newMediaInputStreamDescriptor.MediaStreamTrack;
                videoStreamTrack.OnImage += VideoStreamTrack_OnImage;
            }

            if (_webRTCCommunications != null)
                _initDone = true;

            this.BeginInvoke(() =>
            {
                String name;
                if (mediaInputStreamDescriptor.IsMediaService)
                {
                    name = "Video compositor";
                }
                else
                {
                    if (mediaInputStreamDescriptor.DynamicFeed)
                        name = "Dynamic feed";
                    else
                        name = GetPublisherNameFromId(mediaInputStreamDescriptor.PublisherId);
                }

                String media = mediaInputStreamDescriptor.Media == Call.Media.VIDEO ? "VIDEO" : "SHARING";

                lbl_VideoStreamTrack.Text = $"{media} - {name}";
            });
        }

        private String GetPublisherNameFromId(String id)
        {
            if (id == _currentContactId)
                return "YOU";

            String result = id;
            var contact = _rbContacts?.GetContactFromContactId(id);
            result = Util.GetContactDisplayName(contact);

            if (String.IsNullOrEmpty(result) || (result == "?"))
            {
                _rbContacts?.GetContactFromContactIdFromServer(id); // Ask server for more info
                result = id;
            }

            return result;
        }

        private void UpdatePictureBox(int width, int height, int stride, IntPtr data, FFmpeg.AutoGen.AVPixelFormat pixelFormat)
        {
            if (!_initDone)
                return;

            lock (lockAboutImageManagement)
            {
                Bitmap? bitmap = Helper.BitmapFromImageData(width, height, stride, data, pixelFormat);
                Helper.UpdatePictureBox(pb_VideoStream, bitmap);
            }
        }

    #region Events from WebRTCCommunications

        private void VideoStreamTrack_OnImage(string mediaId, int width, int height, int stride, IntPtr data, FFmpeg.AutoGen.AVPixelFormat pixelFormat)
        {
            UpdatePictureBox(width, height, stride, data, pixelFormat);
        }

        private void WebRTCCommunications_OnMediaPublicationUpdated(object? sender, MediaPublicationEventArgs e)
        {
            if (mediaPublication == null)
                return;

            if ((e.MediaPublication.PublisherId == mediaPublication.PublisherId)
                && (e.MediaPublication.Media == mediaPublication.Media)
                && (e.Status == MediaPublicationStatus.PEER_STOPPED) )
            {
                this.BeginInvoke(() => Close());
            }
        }

        private void WebRTCCommunications_OnMediaServiceUpdated(object? sender, MediaServiceEventArgs e)
        {
            if (mediaService == null)
                return;

            if (e.MediaService.ServiceId == mediaService.ServiceId)
            {
                this.BeginInvoke(() => Close());
            }
        }

        private void WebRTCCommunications_CallUpdated(object? sender, Rainbow.Events.CallEventArgs e)
        {
            if (!_initDone)
                return;

            if (_currentCallId == null)
                _currentCallId = e.Call.Id;

            if (e.Call.Id == _currentCallId)
            {
                if (!e.Call.IsInProgress())
                {
                    _initDone = false;
                    this.BeginInvoke(() => Close());
                }
            }
        }

    #endregion Events from WebRTCCommunications

    #region Events from Form elements

        private void FormVideoOutputStreamWebRTC_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Remove all events used
            if (_webRTCCommunications != null)
            {
                _webRTCCommunications.CallUpdated -= WebRTCCommunications_CallUpdated;
                _webRTCCommunications.OnMediaPublicationUpdated -= WebRTCCommunications_OnMediaPublicationUpdated;
                _webRTCCommunications.OnMediaServiceUpdated -= WebRTCCommunications_OnMediaServiceUpdated;

                _webRTCCommunications = null;
            }
        }

        private void FormVideoOutputStreamWebRTC_ClientSizeChanged(object sender, EventArgs e)
        {
            if (!_initDone)
                return;

            int width = this.Width - _diffWidth;
            int height = this.Height - _diffHeight;

            int calculateWidth = (int)Math.Round(height * _ratio);
            if (calculateWidth > width)
            {
                height = (int)Math.Round(width / _ratio);
            }
            else
                width = calculateWidth;

            int x = (int)Math.Ceiling((double)(this.Width - width - (_diffWidth / 2)) / 2);
            int y = (int)Math.Ceiling((double)(this.Height - height) / 2);

            pb_VideoStream.SetBounds(x, y, width, height);
        }

        private void btn_Unhold_Click(object sender, EventArgs e)
        {
            if (mediaPublication != null)
                _webRTCCommunications.HoldMediaPublication(mediaPublication, false, null);
            else if (mediaService != null)
                _webRTCCommunications.HoldMediaService(mediaService, false, null);
        }

        private void btn_Hold_Click(object sender, EventArgs e)
        {
            if (mediaPublication != null)
                _webRTCCommunications.HoldMediaPublication(mediaPublication, true, null);
            else if (mediaService != null)
                _webRTCCommunications.HoldMediaService(mediaService, true, null);
        }

    #endregion Events from Form elements

    }
}
