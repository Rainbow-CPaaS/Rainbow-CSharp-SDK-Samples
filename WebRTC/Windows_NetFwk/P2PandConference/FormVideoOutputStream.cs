using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FFmpeg.AutoGen;
using Rainbow.WebRTC;
using Rainbow.Medias;
using static Rainbow.Model.Call;

namespace SDK.UIForm.WebRTC
{
    public partial class FormVideoOutputStream : Form
    {
        private static String NONE = "NONE";

        private Object lockAboutImageManagement = new Object();
        
        private MediaInputStreamsManager _mediaInputStreamsManager;
        private int _diffWidth = 0;
        private int _diffHeight = 0;
        private float _ratio = 0;

        private IMediaVideo? _currentMediaVideo = null;
        private Boolean _needToStoreMediaVideo = false;

        public FormVideoOutputStream()
        {
            this.HandleCreated += Form_HandleCreated;
            InitializeComponent();
        }

        private void Form_HandleCreated(object sender, EventArgs e)
        {
            // Init Images on buttons
            btn_StartVideo.BackgroundImage = Helper.GetBitmapPlay();
            btn_StopVideo.BackgroundImage = Helper.GetBitmapStop();

            // Create MediaInputStreamsManager and init necessary events
            _mediaInputStreamsManager = MediaInputStreamsManager.Instance;
            _mediaInputStreamsManager.OnListUpdated += MediaInputStreamsManager_ListUpdated;
            _mediaInputStreamsManager.OnMediaStreamStateChanged += MediaInputStreamsManager_OnMediaStreamStateChanged;

            _ratio = ((float)pb_VideoStream.Size.Width / (float)pb_VideoStream.Size.Height);

            _diffWidth = this.Width - pb_VideoStream.Size.Width;
            _diffHeight = this.Height - pb_VideoStream.Size.Height;

            FillMainComboBoxWithMediaInputStream();

        }

        private void FillCodecList()
        {
            var list = Rainbow.WebRTC.Helper.WebRTCVideoFormatsSupported();
            foreach(var element in list)
            {
                ListItem item = new ListItem(element.ToString(), element.ToString());
                cb_Codecs.Items.Add(item);
            }
        }

        private void FillMainComboBoxWithMediaInputStream()
        {
            var dicoAll = _mediaInputStreamsManager.GetList(true, true, true, false);
            FillComboBoxWithMediaInputStream(cb_ListOfInputStreams, dicoAll.Values.ToList());

            if (_needToStoreMediaVideo)
                _needToStoreMediaVideo = false;
            else
                cb_ListOfInputStreams_SelectedIndexChanged(null, null);
        }

        private void FillComboBoxWithMediaInputStream(System.Windows.Forms.ComboBox comboBox, List<IMedia>? mediaList, Boolean avoidNone = false)
        {
            if (comboBox == null)
                return;

            String selectedId = GetSelectedIdOfComboBoxWithMediaStream(comboBox);
            int selectedIndex = 0;

            comboBox.Items.Clear();

            // First add NONE media stream
            if (!avoidNone)
                comboBox.Items.Add(NONE);

            int index = 1;
            if (mediaList != null)
            {
                foreach (var mediaStream in mediaList)
                {
                    if (mediaStream.Id == selectedId)
                        selectedIndex = index;

                    MediaInputStreamItem mediaInputStreamItem = new MediaInputStreamItem(mediaStream);
                    comboBox.Items.Add(mediaInputStreamItem);

                    index++;
                }
            }

            if (comboBox.Items.Count > selectedIndex)
                comboBox.SelectedIndex = selectedIndex;
        }

        private String GetSelectedIdOfComboBoxWithMediaStream(System.Windows.Forms.ComboBox comboBox)
        {
            String selectedId = "";

            if (_needToStoreMediaVideo && (_currentMediaVideo != null))
                return _currentMediaVideo.Id;

            // Get the the selectedItem (if any)
            if (comboBox.SelectedItem != null)
            {
                if (comboBox.SelectedItem is MediaInputStreamItem mediaInputStreamItem)
                    selectedId = mediaInputStreamItem.Id;
            }
            return selectedId;
        }

        public void SetMediaVideo(IMediaVideo? mediaVideo, Boolean updateSelectedItem = true)
        {
            lock (lockAboutImageManagement)
            {
                _needToStoreMediaVideo = false;

                if (_currentMediaVideo != null)
                    _currentMediaVideo.OnImage -= MediaVideo_OnImage;
                

                if (mediaVideo != null)
                {
                    _currentMediaVideo = mediaVideo;
                    _currentMediaVideo.OnImage += MediaVideo_OnImage;

                    UpdatePlayBtn(mediaVideo.Id, mediaVideo.IsStarted, mediaVideo.IsPaused);

                    // We set a null image 
                    Helper.UpdatePictureBox(pb_VideoStream, null);


                    if (cb_Codecs.Items.Count == 0)
                        FillCodecList();

                    // Set selection in Codecs list
                    var codecID = mediaVideo.VideoCodecId.ToString();
                    int index = 0;
                    foreach(var item in cb_Codecs.Items)
                    {
                        if(item is ListItem listItem)
                        {
                            if (listItem.Value == codecID)
                                break;
                        }
                        index++;
                    }
                    if (cb_Codecs.Items.Count > index)
                        cb_Codecs.SelectedIndex = index;

                    // Do we need to set the selected item ?
                    if (updateSelectedItem)
                    {
                        if (cb_ListOfInputStreams.Items.Count > 0)
                        {
                            index = 0;
                            for (int i = 0; i < cb_ListOfInputStreams.Items.Count; i++)
                            {
                                if (cb_ListOfInputStreams.Items[i] is MediaInputStreamItem mediaInputStreamItem)
                                {
                                    if (_currentMediaVideo.Id == mediaInputStreamItem.Id)
                                    {
                                        index = i;
                                        break;
                                    }
                                }
                            }
                            cb_ListOfInputStreams.SelectedIndex = index;
                        }
                        else
                        {
                            if (updateSelectedItem)
                                _needToStoreMediaVideo = true;
                        }
                    }
                }
                else
                {
                    if (cb_ListOfInputStreams.Items.Count > 0)
                        cb_ListOfInputStreams.SelectedIndex = 0;

                    Helper.UpdatePictureBox(pb_VideoStream, null);
                    btn_StartVideo.BackgroundImage = Helper.GetBitmapPlay();
                }
            }
        }

        private void UpdatePlayBtn(string mediaId, Boolean isStarted, Boolean isPaused)
        {
            if (_currentMediaVideo?.Id == mediaId)
            {
                Bitmap bitmap;
                if (isPaused)
                    bitmap = Helper.GetBitmapPlay();
                else if (isStarted)
                    bitmap = Helper.GetBitmapPause();
                else
                    bitmap = Helper.GetBitmapPlay();

                btn_StartVideo.BackgroundImage = bitmap;
            }
        }

#region Events from MediaInputStreamsManager

        private void MediaInputStreamsManager_ListUpdated(object? sender, EventArgs e)
        {
            if (!IsHandleCreated)
                return;
            
            this.BeginInvoke(new Action(() =>
            {
                FillMainComboBoxWithMediaInputStream();
            }));
        }

        private void MediaInputStreamsManager_OnMediaStreamStateChanged(string mediaId, bool isStarted, bool isPaused)
        {
            Action action = new Action(() =>
            {
                UpdatePlayBtn(mediaId, isStarted, isPaused);
            });

            if (InvokeRequired)
                BeginInvoke(action);
            else
                action.Invoke();
        }

#endregion Events from MediaVideo

#region Events from MediaVideo

        private void MediaVideo_OnImage(string mediaId, int width, int height, int stride, IntPtr data, FFmpeg.AutoGen.AVPixelFormat pixelFormat)
        {
            lock (lockAboutImageManagement)
            {
                if (_currentMediaVideo?.Id == mediaId)
                {
                    Bitmap? bitmap = Helper.BitmapFromImageData(width, height, stride, data, pixelFormat);
                    Helper.UpdatePictureBox(pb_VideoStream, bitmap);
                }
            }
        }

#endregion Events from MediaVideo

#region Events from Form elements

        private void FormVideoOutputStream_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(_currentMediaVideo != null)
                _currentMediaVideo.OnImage -= MediaVideo_OnImage;
        }

        private void FormVideoOutputStream_ClientSizeChanged(object sender, EventArgs e)
        {
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
            // Get the the selectedItem (if any)
            if (cb_ListOfInputStreams.SelectedItem != null)
            {
                if (cb_ListOfInputStreams.SelectedItem is MediaInputStreamItem mediaInputStreamItem)
                {
                    var id = mediaInputStreamItem.Id;
                    var mediaStream = _mediaInputStreamsManager.GetMediaStream(id);
                    if (mediaStream is IMediaVideo mediaVideo)
                    {
                        SetMediaVideo(mediaVideo, false);
                        return;
                    }
                }
                
            }
            SetMediaVideo(null, false);
        }

        private void btn_StartVideo_Click(object sender, EventArgs e)
        {
            if(_currentMediaVideo != null)
            { 
                if (_currentMediaVideo.IsPaused)
                    _currentMediaVideo.Resume();
                else if (_currentMediaVideo.IsStarted)
                    _currentMediaVideo.Pause();
                else
                    _currentMediaVideo.Start();
            }
        }

        private void btn_StopVideo_Click(object sender, EventArgs e)
        {
            if (_currentMediaVideo != null)
                _currentMediaVideo.Stop();
        }

        private void cb_Codecs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cb_Codecs.SelectedItem is ListItem listItem)
            {
                var id = listItem.Value;

                if (Enum.TryParse(id, out AVCodecID codecId))
                {
                    var result = _currentMediaVideo?.SetVideoCodec(codecId);
                }
            }
        }

        private void cb_BindVideoSample_CheckedChanged(object sender, EventArgs e)
        {
            if (_currentMediaVideo == null)
                return;

            _currentMediaVideo.OnVideoSampleConverted -= MediaVideo_OnVideoSampleConverted;
            if ( cb_BindVideoSample.Checked )
                _currentMediaVideo.OnVideoSampleConverted += MediaVideo_OnVideoSampleConverted;
                
        }

        private void MediaVideo_OnVideoSampleConverted(string mediaId, uint duration, byte[] sample)
        {
            // Nothing to do;
        }


#endregion Events from Form elements
    }
}
