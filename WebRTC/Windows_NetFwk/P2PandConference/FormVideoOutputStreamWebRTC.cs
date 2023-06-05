using Rainbow;
using Rainbow.Model;
using Rainbow.WebRTC;
using Rainbow.WebRTC.Desktop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SDK.UIForm.WebRTC
{
    public partial class FormVideoOutputStreamWebRTC : Form
    {
        private static String SEPARATOR = "-|-";
        private static String NONE = "NONE";


        private Rainbow.Application? _rbApplication = null;
        private Rainbow.Contacts? _rbContacts = null;

        private Object lockAboutImageManagement = new Object();
        
        private int _diffWidth = 0;
        private int _diffHeight = 0;
        private float _ratio = 0;

        private Rainbow.WebRTC.WebRTCCommunications? _webRTCCommunications = null;
        private MediaStreamTrackDescriptor? mediaInputStreamDescriptor;
        private VideoStreamTrack? videoStreamTrack = null;

        private List<(Boolean remote, int media, String publisherId)> _publicationsList = new();
        private DateTime _firstCheckDateTime = DateTime.MinValue;
        private int _delay = 1000;

        private String? _currentCallId = null;
        private Call? _currentCall = null;

        private String? _currentContactId = null;
        Boolean _currentRemote = false;
        private int _currentMedia = 0;
        private String? _currentPublisherId = null;

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
                    _currentCall = _webRTCCommunications.GetCall(_currentCallId);

                    _webRTCCommunications.CallUpdated += WebRTCCommunications_CallUpdated;

                    // TODO - manage local and remote video
                    //_webRTCCommunications.OnLocalVideo += WebRTCCommunications_OnLocalVideo;
                    //_webRTCCommunications.OnRemoteVideo += WebRTCCommunications_OnRemoteVideo;

                    _webRTCCommunications.OnMediaPublicationUpdated += WebRTCCommunications_OnMediaPublicationUpdated;
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

            if (newMediaInputStreamDescriptor.MediaStreamTrack != null)
            {
                videoStreamTrack = (VideoStreamTrack)newMediaInputStreamDescriptor.MediaStreamTrack;
                videoStreamTrack.OnImage += VideoStreamTrack_OnImage;
            }

            if (_webRTCCommunications != null)
                _initDone = true;
        }

        private void VideoStreamTrack_OnImage(string mediaId, int width, int height, int stride, IntPtr data, FFmpeg.AutoGen.AVPixelFormat pixelFormat)
        {
            UpdatePictureBox(width, height, stride, data, pixelFormat);
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

        private void UpdateComboBox()
        {
            int currentSelectedIndex = cb_ListOfInputStreams.SelectedIndex;
            int selectedIndex = 0;

            lock (lockAboutImageManagement)
            {
                if (_currentCall == null)
                    return;

                cb_ListOfInputStreams.Items.Clear();
                int index = 0;
                

                foreach ((Boolean remote, int media, String publisherId) in _publicationsList)
                {
                    String text;
                    String value;


                    if (remote)
                    {
                        text = $"REMOTE - {((media == Call.Media.VIDEO) ? "VIDEO" : "SHARING")} - {GetPublisherNameFromId(publisherId)}";
                    }
                    else
                    {
                        text = $"LOCAL - {((media == Call.Media.VIDEO) ? "VIDEO" : "SHARING")} - YOU";
                    }

                    value = $"{(remote ? "true" : "false")}{SEPARATOR}{media}{SEPARATOR}{publisherId}";

                    ListItem item = new ListItem(text, value);
                    cb_ListOfInputStreams.Items.Add(item);

                    if ((_currentRemote == remote) && (_currentMedia == media))
                    {
                        if ((_currentPublisherId == null) && remote)
                        {
                            _currentPublisherId = publisherId;
                            selectedIndex = index;
                        }
                        else if(_currentPublisherId == publisherId)
                            selectedIndex = index;
                    }

                    index++;
                }
            }

            if (cb_ListOfInputStreams.Items.Count > 0)
                cb_ListOfInputStreams.SelectedIndex = selectedIndex;
            else
            {
                cb_ListOfInputStreams.Items.Add(NONE);
                cb_ListOfInputStreams.SelectedIndex = 0;
                Helper.UpdatePictureBox(pb_VideoStream, null);
            }
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

        private void UpdateListOfPublications()
        {
            lock (lockAboutImageManagement)
            {
                if ( (_currentCall != null) && (_currentContactId != null) )
                {
                    _publicationsList.Clear();

                    if (_currentCall.IsConference)
                    {

                    }
                    else
                    {
                        if (Util.MediasWithVideo(_currentCall.LocalMedias))
                            _publicationsList.Add((false, Call.Media.VIDEO, _currentContactId));

                        if (Util.MediasWithSharing(_currentCall.LocalMedias))
                            _publicationsList.Add((false, Call.Media.SHARING, _currentContactId));

                        if (Util.MediasWithVideo(_currentCall.RemoteMedias))
                            _publicationsList.Add((true, Call.Media.VIDEO, _currentCall.Participants[0].UserId));

                        if (Util.MediasWithSharing(_currentCall.RemoteMedias))
                            _publicationsList.Add((true, Call.Media.SHARING, _currentCall.Participants[0].UserId));
                    }
                }
            }

            this.BeginInvoke(() => UpdateComboBox());
        }

#region Events from WebRTCCommunications

        private void WebRTCCommunications_OnMediaPublicationUpdated(object? sender, MediaPublicationEventArgs e)
        {
        }

        private void OnVideo(Boolean remote, string callId, string userId, int media, string mediaId, int width, int height, int stride, IntPtr data, FFmpeg.AutoGen.AVPixelFormat pixelFormat)
        {
            if (!_initDone)
                return;

            if ((_currentCallId == callId) && (_currentMedia == media) && (_currentPublisherId == userId))
                UpdatePictureBox(width, height, stride, data, pixelFormat);
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
                    this.Close();
                    return;
                }
                else
                {
                    _currentCall = e.Call;
                    UpdateListOfPublications();
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

                // TODO - manage local and remote video
                //_webRTCCommunications.OnLocalVideo -= WebRTCCommunications_OnLocalVideo;
                //_webRTCCommunications.OnRemoteVideo -= WebRTCCommunications_OnRemoteVideo;

                _webRTCCommunications.OnMediaPublicationUpdated += WebRTCCommunications_OnMediaPublicationUpdated;

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

        private void cb_ListOfInputStreams_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the selectedItem (if any)
            if(cb_ListOfInputStreams.SelectedItem is ListItem item)
            {
                var info = item.Value.Split(SEPARATOR);
                lock (lockAboutImageManagement)
                {
                    _currentRemote = info[0] == "true";
                    _currentMedia = int.Parse(info[1]);
                    _currentPublisherId = info[2];

                    Helper.UpdatePictureBox(pb_VideoStream, null);
                }
            }
        }

#endregion Events from Form elements
    }
}
