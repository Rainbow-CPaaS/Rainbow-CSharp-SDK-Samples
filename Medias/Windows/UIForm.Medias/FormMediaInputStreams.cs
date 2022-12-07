using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using DirectShowLib.MultimediaStreaming;
using EmguFFmpeg;
using FFmpeg.AutoGen;
using Rainbow;
using Rainbow.Medias;
using Rainbow.WebRTC;
using SIPSorcery.net.RTP;
using static Rainbow.Model.Call;

namespace SDK.UIForm.WebRTC
{
    public partial class FormMediaInputStreams : Form
    {
        private static String NONE = "NONE";

        private FormMediaFilteredStreams? _formMediaFilteredStreams = null;
        private MediaInputStreamsManager _mediaInputStreamsManager;
        private Dictionary<String, Bitmap> _currentImageByMediaStreamId = new Dictionary<String, Bitmap>();
        private String[] _currentMediaStreamIdView = new String[9];
        private Boolean[] _currentMediaStreamIsViewing = new Boolean[9];
        
        private MediaFiltered? _currentMediaStreamFiltered;
        private String _currentMediaStreamFilteredIdSelected = "";
        private Bitmap? _currentMediaStreamFilteredBitmap = null;
        private Object lockAboutImageManagement = new Object();

        private IMediaAudio? _currentAudioInput = null;
        private SDL2AudioOutput? _currentAudioOutput = null;

        private Dictionary<String, FormVideoOutputStream>? _videoStreamUsedInOutput = new Dictionary<string, FormVideoOutputStream>();

#region CONSTRUCTOR

        public FormMediaInputStreams()
        {
            this.HandleCreated += Form_HandleCreated;
            InitializeComponent();
        }

        private void Form_HandleCreated(object sender, EventArgs e)
        {
            // Init Images on buttons
            btn_AudioInputListRefresh.BackgroundImage = Helper.GetBitmapRefresh();
            btn_AudioOutputListRefresh.BackgroundImage = Helper.GetBitmapRefresh();

            btn_ViewVideoFilter.BackgroundImage = Helper.GetBitmapViewOff();
            btn_StartVideoFilter.BackgroundImage = Helper.GetBitmapPlay();
            btn_StopVideoFilter.BackgroundImage = Helper.GetBitmapStop();
            btn_OutputVideoFilter.BackgroundImage = Helper.GetBitmapOutput();

            btn_ViewVideo01.BackgroundImage = Helper.GetBitmapViewOff();
            btn_StartVideo01.BackgroundImage = Helper.GetBitmapPlay();
            btn_StopVideo01.BackgroundImage = Helper.GetBitmapStop();
            btn_OutputVideo01.BackgroundImage = Helper.GetBitmapOutput();

            btn_ViewVideo02.BackgroundImage = Helper.GetBitmapViewOff();
            btn_StartVideo02.BackgroundImage = Helper.GetBitmapPlay();
            btn_StopVideo02.BackgroundImage = Helper.GetBitmapStop();
            btn_OutputVideo02.BackgroundImage = Helper.GetBitmapOutput();

            btn_ViewVideo03.BackgroundImage = Helper.GetBitmapViewOff();
            btn_StartVideo03.BackgroundImage = Helper.GetBitmapPlay();
            btn_StopVideo03.BackgroundImage = Helper.GetBitmapStop();
            btn_OutputVideo03.BackgroundImage = Helper.GetBitmapOutput();

            btn_ViewVideo04.BackgroundImage = Helper.GetBitmapViewOff();
            btn_StartVideo04.BackgroundImage = Helper.GetBitmapPlay();
            btn_StopVideo04.BackgroundImage = Helper.GetBitmapStop();
            btn_OutputVideo04.BackgroundImage = Helper.GetBitmapOutput();

            btn_ViewVideo05.BackgroundImage = Helper.GetBitmapViewOff();
            btn_StartVideo05.BackgroundImage = Helper.GetBitmapPlay();
            btn_StopVideo05.BackgroundImage = Helper.GetBitmapStop();
            btn_OutputVideo05.BackgroundImage = Helper.GetBitmapOutput();

            btn_ViewVideo06.BackgroundImage = Helper.GetBitmapViewOff();
            btn_StartVideo06.BackgroundImage = Helper.GetBitmapPlay();
            btn_StopVideo06.BackgroundImage = Helper.GetBitmapStop();
            btn_OutputVideo06.BackgroundImage = Helper.GetBitmapOutput();

            btn_ViewVideo07.BackgroundImage = Helper.GetBitmapViewOff();
            btn_StartVideo07.BackgroundImage = Helper.GetBitmapPlay();
            btn_StopVideo07.BackgroundImage = Helper.GetBitmapStop();
            btn_OutputVideo07.BackgroundImage = Helper.GetBitmapOutput();

            btn_ViewVideo08.BackgroundImage = Helper.GetBitmapViewOff();
            btn_StartVideo08.BackgroundImage = Helper.GetBitmapPlay();
            btn_StopVideo08.BackgroundImage = Helper.GetBitmapStop();
            btn_OutputVideo08.BackgroundImage = Helper.GetBitmapOutput();

            btn_ViewVideo09.BackgroundImage = Helper.GetBitmapViewOff();
            btn_StartVideo09.BackgroundImage = Helper.GetBitmapPlay();
            btn_StopVideo09.BackgroundImage = Helper.GetBitmapStop();
            btn_OutputVideo09.BackgroundImage = Helper.GetBitmapOutput();


            ResetLabelUsedForInfo();

            // Init _currentMediaStreamIdView and _currentMediaStreamIsViewing array
            Array.Fill(_currentMediaStreamIdView, "");
            Array.Fill(_currentMediaStreamIsViewing, true);

            // Create MediaInputStreamsManager and init necessary events
            _mediaInputStreamsManager = MediaInputStreamsManager.Instance;
            _mediaInputStreamsManager.OnListUpdated += MediaInputStreamsManager_ListUpdated;
            _mediaInputStreamsManager.OnMediaStreamStateChanged += MediaInputStreamsManager_OnMediaStreamStateChanged;

            // We fill forms elements about Input streams and Audio Outputs devices
            CancelableDelay.StartAfter(200, () =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    FillFormElementsAfterMediaInputStreamListUpdate();

                    FillAudioOutputComboBox();

                    InitComboBoxAfterMediaInputStreamListUpdate();
                }));
            });
        }

#endregion CONSTRUCTOR

#region PRIVATE API

        private void ResetLabelUsedForInfo()
        {
            lbl_InfoAboutNewInputStream.Text = "";
            lbl_InfoAboutInputStreamFromList.Text = "";

            lbl_InfoFilterOnMultiStream.Text = "";

            lbl_InfoVideo01.Text = "";
            lbl_InfoVideo02.Text = "";
            lbl_InfoVideo03.Text = "";
            lbl_InfoVideo04.Text = "";
            lbl_InfoVideo05.Text = "";
            lbl_InfoVideo06.Text = "";
            lbl_InfoVideo07.Text = "";
            lbl_InfoVideo08.Text = "";
            lbl_InfoVideo09.Text = "";

            lbl_InfoVideoFilter.Text = "";
        }

        private void SetLabel(Label? lbl, IMedia ? mediaStream)
        {
            if (mediaStream == null)
                SetLabel(lbl, "No MediaStream available ...", true);
            else
            {
                var msg = mediaStream.ToString();
                if (msg == null)
                    msg = "";

                if (!String.IsNullOrEmpty(mediaStream.Path))
                    msg += $"\r\n{mediaStream.Path}";

                SetLabel(lbl, msg);
            }
        }

        private void SetLabel(Label? lbl, String msg, Boolean isWarning = false)
        {
            if (lbl == null)
                return;

            lbl.Text = msg;
            lbl.ForeColor = isWarning ? System.Drawing.Color.Red : System.Drawing.SystemColors.Highlight; ;
        }

        private void FillAudioInputComboBox()
        {
            var dico = _mediaInputStreamsManager.GetList(false, true, false, true);
            FillComboBoxWithMediaInputStream(cb_AudioInputList, dico.Values.ToList());

            // Add list of Audio Input devices
            var audioInputDevices = Rainbow.Medias.Devices.GetAudioInputDevices();
            if (audioInputDevices?.Count > 0)
            {
                foreach (var device in audioInputDevices)
                    cb_AudioInputList.Items.Add(device.Name);
            }
        }

        private void FillAudioOutputComboBox()
        {
            // Clear the list
            cb_AudioOutputList.Items.Clear();

            // Add "NONE" device
            cb_AudioOutputList.Items.Add(NONE);

            // Add list of Audio Output devices
            var audioOutputDevices = Rainbow.Medias.Devices.GetAudioOutputDevices();
            if (audioOutputDevices?.Count > 0)
            {
                foreach (var device in audioOutputDevices)
                    cb_AudioOutputList.Items.Add(device.Name);
            }

            cb_AudioOutputList.SelectedIndex = 0;
        }

        private void FillMainComboBoxWithMediaInputStream()
        {
            var dicoAll = _mediaInputStreamsManager.GetList(true, true, cb_FilterOnVideo.Checked, cb_FilterOnAudio.Checked);
            FillComboBoxWithMediaInputStream(cb_ListOfInputStreams, dicoAll.Values.ToList());

            cb_ListOfInputStreams_SelectedIndexChanged(null, null);
        }

        private void FillComboBoxVideoWithMediaInputStream(List<IMedia>? mediaList)
        {
            FillComboBoxWithMediaInputStream(cb_SelectVideo01, mediaList);
            FillComboBoxWithMediaInputStream(cb_SelectVideo02, mediaList);
            FillComboBoxWithMediaInputStream(cb_SelectVideo03, mediaList);
            FillComboBoxWithMediaInputStream(cb_SelectVideo04, mediaList);
            FillComboBoxWithMediaInputStream(cb_SelectVideo05, mediaList);
            FillComboBoxWithMediaInputStream(cb_SelectVideo06, mediaList);
            FillComboBoxWithMediaInputStream(cb_SelectVideo07, mediaList);
            FillComboBoxWithMediaInputStream(cb_SelectVideo08, mediaList);
            FillComboBoxWithMediaInputStream(cb_SelectVideo09, mediaList);
        }

        private String GetSelectedIdOfComboBoxWithMediaStream(System.Windows.Forms.ComboBox comboBox)
        {
            String selectedId = "";
            // Get the the selectedItem (if any)
            if (comboBox.SelectedItem != null)
            {
                if (comboBox.SelectedItem is MediaInputStreamItem mediaInputStreamItem)
                    selectedId = mediaInputStreamItem.Id;
            }
            return selectedId;
        }

        private int GetIndexFromControl(System.Windows.Forms.Control control)
        {
            // Get index
            String s = control.Name.Substring(control.Name.Length - 1);
            if (int.TryParse(s, out int index))
                return index-1;
            return -1;
        }

        private String GetMediaInputStreamIdRelatedToControl(System.Windows.Forms.Control control)
        {
            // Get index
            int index = GetIndexFromControl(control);
            if(index > -1)
                return _currentMediaStreamIdView[index];
            return "";
        }

        private IMedia? GetMediaInputStreamRelatedToControl(System.Windows.Forms.Control control)
        {
            if (_mediaInputStreamsManager == null)
                return null;
            
            String id = GetMediaInputStreamIdRelatedToControl(control);
            return _mediaInputStreamsManager.GetMediaStream(id);
        }

        private void FillComboBoxWithMediaInputStream(System.Windows.Forms.ComboBox comboBox, List<IMedia>? mediaList, Boolean avoidNone = false)
        {
            if (comboBox == null)
                return;

            String selectedId = GetSelectedIdOfComboBoxWithMediaStream(comboBox);
            int selectedIndex = 0;

            comboBox.Items.Clear();

            // First add NONE media stream
            if(!avoidNone)
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

            if(comboBox.Items.Count > selectedIndex)
                comboBox.SelectedIndex = selectedIndex;
        }

        private void UpdatePlayAllBtn(string mediaId, Boolean isStarted, Boolean isPaused)
        {
            Bitmap bitmap;
            if (isPaused)
                bitmap = Helper.GetBitmapPlay();
            else if (isStarted)
                bitmap = Helper.GetBitmapPause();
            else
                bitmap = Helper.GetBitmapPlay();

            for (int i = 0; i < 9; i++)
            {
                if (_currentMediaStreamIdView[i] == mediaId)
                {
                    var btn = GetPlayBtnByIndex(i);
                    if (btn != null)
                        btn.BackgroundImage = bitmap;
                }
                UpdatePictureBoxByIndex(i);
            }

            UpdatePlayBtnForMediaFiltered();
        }

        private void UpdatePlayBtnForMediaFiltered()
        {
            if (_currentMediaStreamFiltered != null)
            {
                if (_currentMediaStreamFiltered.IsPaused)
                {
                    btn_StartVideoFilter.BackgroundImage = Helper.GetBitmapPlay();
                    Helper.UpdatePictureBox(pb_MultiStream, _currentMediaStreamFilteredBitmap);
                }
                else if (_currentMediaStreamFiltered.IsStarted)
                    btn_StartVideoFilter.BackgroundImage = Helper.GetBitmapPause();
                else
                    btn_StartVideoFilter.BackgroundImage = Helper.GetBitmapPlay();
            }
            else
            {
                btn_StartVideoFilter.BackgroundImage = Helper.GetBitmapPlay();
            }
        }

        private void UpdateViewBtnByIndex(int index, Boolean viewSetByUser)
        {
            // viewSetByUser = true => User has clicked on a "view" button
            // viewSetByUser = false => a new selection has been done on a combobox (specified by it's id).

            if(!viewSetByUser)
            {
                // TODO - We check if this media is already seen in another index
                // Necessary ?
            }

            var btn = GetViewBtnByIndex(index);
            if(btn != null)
            {
                if (_currentMediaStreamIsViewing[index])
                    btn.BackgroundImage = Helper.GetBitmapViewOff();
                else
                {
                    btn.BackgroundImage = Helper.GetBitmapViewOn();
                }
            }
            UpdatePictureBoxByIndex(index);


            // Get all list of MediaStream Id we want to view
            List<String> mediaIdListToView = new List<String>();
            for (int i = 0; i < 9; i++)
            {
                if (_currentMediaStreamIsViewing[i])
                {
                    string id = _currentMediaStreamIdView[i];
                    if(!String.IsNullOrEmpty(id))
                        mediaIdListToView.Add(id);
                }
            }

            // We get all Media Stream with video. We loop on all of them to register/unregister events
            var mediaStreamsListWithVideo = _mediaInputStreamsManager.GetList(false, true, true, false).Values.ToList();
            foreach(var mediaStream in mediaStreamsListWithVideo)
            {
                if(mediaStream is IMediaVideo mediaVideo)
                {
                    mediaVideo.OnImage -= MediaVideo_OnImage;
                    if (mediaIdListToView.Contains(mediaStream.Id))
                    {
                        // We want to view this media stream
                        mediaVideo.OnImage += MediaVideo_OnImage;
                    }
                }
            }
        }

        private ComboBox? GetComboBoxByIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return cb_SelectVideo01;
                case 1:
                    return cb_SelectVideo02;
                case 2:
                    return cb_SelectVideo03;
                case 3:
                    return cb_SelectVideo04;
                case 4:
                    return cb_SelectVideo05;
                case 5:
                    return cb_SelectVideo06;
                case 6:
                    return cb_SelectVideo07;
                case 7:
                    return cb_SelectVideo08;
                case 8:
                    return cb_SelectVideo09;
                default:
                    return null;
            }
        }

        private PictureBox? GetPictureBoxByIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return pb_Video01;
                case 1:
                    return pb_Video02;
                case 2:
                    return pb_Video03;
                case 3:
                    return pb_Video04;
                case 4:
                    return pb_Video05;
                case 5:
                    return pb_Video06;
                case 6:
                    return pb_Video07;
                case 7:
                    return pb_Video08;
                case 8:
                    return pb_Video09;
                default:
                    return null;
            }
        }

        private Button? GetViewBtnByIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return btn_ViewVideo01;
                case 1:
                    return btn_ViewVideo02;
                case 2:
                    return btn_ViewVideo03;
                case 3:
                    return btn_ViewVideo04;
                case 4:
                    return btn_ViewVideo05;
                case 5:
                    return btn_ViewVideo06;
                case 6:
                    return btn_ViewVideo07;
                case 7:
                    return btn_ViewVideo08;
                case 8:
                    return btn_ViewVideo09;
                default:
                    return null;
            }
        }

        private Button? GetStopBtnByIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return btn_StopVideo01;
                case 1:
                    return btn_StopVideo02;
                case 2:
                    return btn_StopVideo03;
                case 3:
                    return btn_StopVideo04;
                case 4:
                    return btn_StopVideo05;
                case 5:
                    return btn_StopVideo06;
                case 6:
                    return btn_StopVideo07;
                case 7:
                    return btn_StopVideo08;
                case 8:
                    return btn_StopVideo09;
                default:
                    return null;
            }
        }

        private Button? GetPlayBtnByIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return btn_StartVideo01;
                case 1:
                    return btn_StartVideo02;
                case 2:
                    return btn_StartVideo03;
                case 3:
                    return btn_StartVideo04;
                case 4:
                    return btn_StartVideo05;
                case 5:
                    return btn_StartVideo06;
                case 6:
                    return btn_StartVideo07;
                case 7:
                    return btn_StartVideo08;
                case 8:
                    return btn_StartVideo09;
                default:
                    return null;
            }
        }

        private Label? GetLabelInfoViewByIndex(int index)
        {
            switch(index)
            {
                case 0:
                    return lbl_InfoVideo01;
                case 1:
                    return lbl_InfoVideo02;
                case 2:
                    return lbl_InfoVideo03;
                case 3:
                    return lbl_InfoVideo04;
                case 4:
                    return lbl_InfoVideo05;
                case 5:
                    return lbl_InfoVideo06;
                case 6:
                    return lbl_InfoVideo07;
                case 7:
                    return lbl_InfoVideo08;
                case 8:
                    return lbl_InfoVideo09;
                default:
                    return null;
            }
        }

        private void UpdatePictureBoxByIndex(int index)
        {
            if (index > -1)
            {
                string mediaId = _currentMediaStreamIdView[index];
                var mediaStream = _mediaInputStreamsManager.GetMediaStream(mediaId);
                if(mediaStream != null)
                {
                    var pictureBox = GetPictureBoxByIndex(index);
                    if (pictureBox == null)
                        return;

                    if (!_currentMediaStreamIsViewing[index])
                        pictureBox.Image = Helper.GetBitmapViewOff();
                    else if (mediaStream.IsPaused)
                    {
                        if(_currentImageByMediaStreamId.ContainsKey(mediaId))
                            pictureBox.Image = _currentImageByMediaStreamId[mediaId];
                        else
                            pictureBox.Image = Helper.GetBitmapPause(false);
                    }
                    else
                        pictureBox.Image = null;
                    
                }
            }
        }

        private void StopAudioEvent()
        {
            if (_currentAudioInput != null)
            {
                _currentAudioInput.OnAudioSample -= MediaAudio_OnAudioSample;
            }
        }

        private void StartAudioEvent()
        {
            if ((_currentAudioOutput != null) 
                && (_currentAudioInput != null))
            {
                _currentAudioInput.OnAudioSample += MediaAudio_OnAudioSample;
                
            }
        }

        private void FillFormElementsAfterMediaInputStreamListUpdate()
        {
            // Need to update the global list of MediaInputStream
            FillMainComboBoxWithMediaInputStream();

            var dicoSimple = _mediaInputStreamsManager.GetList(false, true, false, false);
            FillComboBoxVideoWithMediaInputStream(dicoSimple.Values.ToList());

            var dicoFiltered = _mediaInputStreamsManager.GetList(true, false, false, false);
            FillComboBoxWithMediaInputStream(cb_SelectMultiStream, dicoFiltered.Values.ToList());

            FillAudioInputComboBox();
        }

        private void InitComboBoxAfterMediaInputStreamListUpdate()
        {
            // Set SelectedIndex (if necessary) to each MediaInput
            for (int index = 0; index < 9; index++)
            {
                var combo = GetComboBoxByIndex(index);
                if (combo != null)
                {
                    if (combo.Items.Count > index + 1)
                        combo.SelectedIndex = index + 1;
                }
            }

            // Set SelectedIndex (if necessary) for MediaInput Filtered
            if (cb_SelectMultiStream.Items.Count > 1)
                cb_SelectMultiStream.SelectedIndex = 1;
        }

#endregion PRIVATE API

#region EVENTS from MediaVideo and MediaAudio

        private void MediaVideo_OnImage(string mediaId, int width, int height, int stride, IntPtr data, FFmpeg.AutoGen.AVPixelFormat pixelFormat)
        {
            lock (lockAboutImageManagement)
            {
                Bitmap? bitmap = null;
                for (int i = 0; i < 9; i++)
                {
                    if (_currentMediaStreamIsViewing[i] && _currentMediaStreamIdView[i] == mediaId)
                    {
                        if (bitmap == null)
                            bitmap = Helper.BitmapFromImageData(width, height, stride, data, pixelFormat);

                        var pictureBox = GetPictureBoxByIndex(i);
                        if (pictureBox != null)
                        {
                            // We want to store the last bitmap to manage "pause"
                            if (bitmap != null)
                            {
                                _currentImageByMediaStreamId.Remove(mediaId);
                                _currentImageByMediaStreamId.Add(mediaId, bitmap);
                            }
                            Helper.UpdatePictureBox(pictureBox, bitmap);
                        }
                    }
                }
            }
        }

        private void MediaVideoFilterd_OnImage(string mediaId, int width, int height, int stride, IntPtr data, FFmpeg.AutoGen.AVPixelFormat pixelFormat)
        {
            if (mediaId == _currentMediaStreamFilteredIdSelected)
            {
                Bitmap? bitmap = Helper.BitmapFromImageData(width, height, stride, data, pixelFormat);
                if (bitmap != null)
                {
                    _currentMediaStreamFilteredBitmap = bitmap;
                    Helper.UpdatePictureBox(pb_MultiStream, bitmap);
                }
            }
        }

        private void MediaAudio_OnAudioSample(string mediaId, uint duration, byte[] sample)
        {
            if (_currentAudioOutput != null)
            {
                _currentAudioOutput.QueueSample(sample);
            }
        }

#endregion EVENTS from MediaVideo

#region EVENTS from MediaInputStreamsManager

        private void MediaInputStreamsManager_ListUpdated(object? sender, EventArgs e)
        {
            if (!IsHandleCreated)
                return;

            this.BeginInvoke(new Action(() =>
            {
                FillFormElementsAfterMediaInputStreamListUpdate();
            }));
        }

        private void MediaInputStreamsManager_OnMediaStreamStateChanged(string mediaId, bool IsStarted, bool IsPaused)
        {
            Action action = new Action(() =>
            {
                UpdatePlayAllBtn(mediaId, IsStarted, IsPaused);
            });

            if (InvokeRequired)
                BeginInvoke(action);
            else
                action.Invoke();
        }

#endregion EVENTS from MediaInputStreamsManager

#region EVENTS from FORM elements

        private void FormInputStreams_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void btn_LoadConfig_Click(object sender, EventArgs e)
        {

            // Check state of 
            var btnWebCamStatus = btn_ManageWebcam.Enabled;
            var btnScreenStatus = btn_ManageScreen.Enabled;
            var btnAudioStatus = btn_ManageAudio.Enabled;


            btn_LoadConfig.Enabled = false;
            btn_SaveConfig.Enabled = false;
            btn_ManageWebcam.Enabled = false;
            btn_ManageScreen.Enabled = false;
            btn_ManageAudio.Enabled = false;


            // We want to load configuration using another thread
            Task newTask = new Task(() =>
            {
                _mediaInputStreamsManager.LoadConfiguration();

                this.BeginInvoke(new Action(() =>
                {
                    btn_LoadConfig.Enabled = true;
                    btn_SaveConfig.Enabled = true;

                    if(btnWebCamStatus)
                        btn_ManageWebcam.Enabled = true;

                    if (btnScreenStatus)
                        btn_ManageScreen.Enabled = true;

                    if (btnAudioStatus)
                        btn_ManageAudio.Enabled = true;

                    InitComboBoxAfterMediaInputStreamListUpdate();

                }));
            });
            newTask.Start();
        }

        private void btn_SaveConfig_Click(object sender, EventArgs e)
        {
            _mediaInputStreamsManager.SaveConfiguration();
        }

        private void tb_BrowseForNewInputStreamUri_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    tb_NewxInputStreamUri.Text = openFileDialog.FileName;
                    btn_InfoAboutNewInputStream_Click(null, null);
                }
            }
        }

        private void btn_InfoAboutNewInputStream_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(tb_NewxInputStreamUri.Text))
                return;

            var device = new InputStreamDevice("test", "test", tb_NewxInputStreamUri.Text, true, true, true);
            var mediaInputStream = new MediaInput(device);
            if (mediaInputStream?.Init() == true)
                SetLabel(lbl_InfoAboutNewInputStream, mediaInputStream);
            else
                SetLabel(lbl_InfoAboutNewInputStream, "Cannot create / init this Input stream", true);
        }

        private void btn_AddNewInputStream_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(tb_NameOfNewInputStream.Text))
                return;

            if (String.IsNullOrEmpty(tb_NewxInputStreamUri.Text))
                return;

            var result = _mediaInputStreamsManager.Add(tb_NameOfNewInputStream.Text, tb_NewxInputStreamUri.Text, cb_VideoUsedInNewInputStream.Checked, cb_AudioUsedInNewInputStream.Checked);
        }

        private void btn_SetFilterOnList_Click(object sender, EventArgs e)
        {
            FillMainComboBoxWithMediaInputStream();
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
                    SetLabel(lbl_InfoAboutInputStreamFromList, mediaStream);
                    return;
                }
            }
            SetLabel(lbl_InfoAboutInputStreamFromList, "");
        }

        private void cb_SelectVideoXX_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get Id of the Media Stream selected in this ComboBox
            String selectedId = "";
            if (sender is System.Windows.Forms.ComboBox comboBox)
                selectedId = GetSelectedIdOfComboBoxWithMediaStream(comboBox);
            else
                return;

            if (sender is System.Windows.Forms.Control control)
            {
                // According Control (using it's name) get the index (for 0 to 8)
                int index = GetIndexFromControl(control);

                // Store the Id of the Media Stream selected according the index
                _currentMediaStreamIdView[index] = selectedId;

                string txtForLabel = ""; // To update info about this media stream

                IMedia ? mediaStream = GetMediaInputStreamRelatedToControl(control);
                if (mediaStream != null)
                {
                    if (mediaStream.HasVideo() && (mediaStream is IMediaVideo mediaVideo))
                    {
                        txtForLabel = $"{mediaVideo.Width}x{mediaVideo.Height} - {mediaVideo.Fps.ToString("0.##")} Fps";
                    }
                    else if (mediaStream.HasAudio() && (mediaStream is IMediaAudio mediaAudio))
                    {
                        txtForLabel = $"Audio";
                    }

                    UpdatePlayAllBtn(mediaStream.Id, mediaStream.IsStarted, mediaStream.IsPaused);
                    UpdateViewBtnByIndex(index, false);
                }
                else
                {
                    var btn = GetPlayBtnByIndex(index);
                    if(btn != null)
                        btn.BackgroundImage = Helper.GetBitmapPlay();
                    var pb = GetPictureBoxByIndex(index);
                    if (pb != null)
                        pb.Image = null;
                }

                // Set info about this media stream using its Label
                SetLabel(GetLabelInfoViewByIndex(index), txtForLabel);
            }
        }

        private void btn_StartVideoXX_Click(object sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.Control control)
            {
                IMedia? mediaStream = GetMediaInputStreamRelatedToControl(control);
                if (mediaStream != null)
                {
                    if (mediaStream.IsPaused)
                        mediaStream.Resume();
                    else if (mediaStream.IsStarted)
                        mediaStream.Pause();
                    else
                        mediaStream.Start();

                    //UpdatePlayAllBtn(mediaStream.Id, mediaStream.IsStarted, mediaStream.IsPaused);
                }
            }
        }

        private void btn_StopVideoXX_Click(object sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.Control control)
            {
                var mediaId = GetMediaInputStreamIdRelatedToControl(control);
                _mediaInputStreamsManager.StopMediaStream(mediaId);
            }
        }

        private void btn_OuputVideoXX_Click(object sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.Control control)
            {
                IMedia? mediaStream = GetMediaInputStreamRelatedToControl(control);
                if (mediaStream is IMediaVideo mediaVideo)
                {
                    var form = new FormVideoOutputStream();
                    form.SetMediaVideo(mediaVideo);
                    form.Show();
                }
            }
        }

        private void btn_ViewVideoXX_Click(object sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.Control control)
            {
                // According Control (using it's name) get the index (for 0 to 8)
                int index = GetIndexFromControl(control);
                var wasViewing = _currentMediaStreamIsViewing[index];

                // Store the wiewing state
                _currentMediaStreamIsViewing[index] = !wasViewing;

                UpdateViewBtnByIndex(index, true);
            }
        }

        private void btn_RemoveInputStream_Click(object sender, EventArgs e)
        {
            // TODO - btn_RemoveInputStream_Click
            if(cb_ListOfInputStreams.SelectedItem is MediaInputStreamItem mediaInputStreamItem)
            {
                _mediaInputStreamsManager.Remove(mediaInputStreamItem.Id);
            }
        }

#endregion EVENTS from FORM elements

#region EVENTS from FORM elements - to manage MediaFiltered

        private void btn_ManageMultiStream_Click(object sender, EventArgs e)
        {
            if (_formMediaFilteredStreams == null)
                _formMediaFilteredStreams = new FormMediaFilteredStreams();

            _formMediaFilteredStreams.WindowState = FormWindowState.Normal;
            _formMediaFilteredStreams.Show();
        }

        private void cb_SelectMultiStream_SelectedIndexChanged(object sender, EventArgs e)
        {
            cb_SourcesOnFilter.Items.Clear();
            cb_ListOnFilters.Items.Clear();
            SetLabel(lbl_InfoFilterOnMultiStream, "");

            // We check if the current MediaStream Filterd is already set
            // We have to stop events if it's the case
            if(_currentMediaStreamFiltered != null)
                _currentMediaStreamFiltered.OnImage -= MediaVideoFilterd_OnImage;

            // Reset previous selection
            _currentMediaStreamFiltered = null;

            if (cb_SelectMultiStream.SelectedItem is MediaInputStreamItem mediaInputStreamItem)
            {
                var id = mediaInputStreamItem.Id;
                IMedia? media = _mediaInputStreamsManager.GetMediaStream(id);
                if(media is MediaFiltered mediaFiltered)
                {
                    // Store the Media Filtered selected
                    _currentMediaStreamFiltered = mediaFiltered;

                    // Fill list of video sources
                    List<IMedia> mediasList = new List<IMedia>();
                    var dicoMediaInput = mediaFiltered.MediaInputs;
                    if(dicoMediaInput != null)
                    {
                        foreach(var mediaInput in dicoMediaInput.Values)
                        {
                            if(mediaInput != null)
                            {
                                mediasList.Add(mediaInput);
                            }
                        }
                    }
                    FillComboBoxWithMediaInputStream(cb_SourcesOnFilter, mediasList, true);
                    lbl_NbSourcesInMultiStream.Text = $"Videos Source-Nb [{cb_SourcesOnFilter.Items.Count}]:";

                    // Fill  list of video used in the filter
                    mediasList.Clear();
                    if (mediaFiltered.VideoIdUsedInFilter != null)
                    {
                        foreach (string mediaId in mediaFiltered.VideoIdUsedInFilter)
                        {
                            media = _mediaInputStreamsManager.GetMediaStream(mediaId);
                            if (media != null)
                                mediasList.Add(media);
                        }
                    }
                    FillComboBoxWithMediaInputStream(cb_ListOnFilters, mediasList, true);
                    lbl_NbUsedInFilterInMultiStream.Text = $"Videos in filter-Nb [{cb_ListOnFilters.Items.Count}]:";

                    // Set the filter used
                    tb_FilterOnMultiStream.Text = mediaFiltered.VideoFilter;
                }
            }
            else
            {
                btn_StartVideoFilter.BackgroundImage = Helper.GetBitmapPlay();
                pb_MultiStream.Image = null;
            }

            // If we have selected a MediaStream Filtered  we want to manage events AND store its ID
            if (_currentMediaStreamFiltered != null)
            {
                _currentMediaStreamFilteredIdSelected = _currentMediaStreamFiltered.Id;
                _currentMediaStreamFiltered.OnImage += MediaVideoFilterd_OnImage;
            }
            else
                _currentMediaStreamFilteredIdSelected = "";
        }

        private void cb_SourcesOnFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_SourcesOnFilter.SelectedItem is MediaInputStreamItem mediaInputStreamItem)
            {
                var id = mediaInputStreamItem.Id;
                IMedia? media = _mediaInputStreamsManager.GetMediaStream(id);
                SetLabel(lbl_InfoFilterOnMultiStream, media);
            }
        }

        private void btn_AddSourceForFilter_Click(object sender, EventArgs e)
        {
            if (cb_SourcesOnFilter.SelectedItem is MediaInputStreamItem mediaInputStreamItem)
            {
                var id = mediaInputStreamItem.Id;

                Boolean canAdd = true;
                foreach(var item in cb_ListOnFilters.Items)
                {
                    if(item is MediaInputStreamItem streamItem)
                    {
                        if(streamItem.Id == id)
                        {
                            canAdd = false;
                            break;
                        }
                    }
                }
                if(canAdd)
                {
                    IMedia? media = _mediaInputStreamsManager.GetMediaStream(id);
                    cb_ListOnFilters.Items.Add(new MediaInputStreamItem(media));

                    lbl_NbUsedInFilterInMultiStream.Text = $"Videos in filter-Nb [{cb_ListOnFilters.Items.Count}]:";

                    if (cb_ListOnFilters.Items.Count > 0)
                        cb_ListOnFilters.SelectedIndex = cb_ListOnFilters.Items.Count - 1;
                }
            }
        }

        private void btn_AddAllSourcesForFilter_Click(object sender, EventArgs e)
        {
            cb_ListOnFilters.Items.Clear();
            foreach (var item in cb_SourcesOnFilter.Items)
            {
                if (item is MediaInputStreamItem streamItem)
                {
                    cb_ListOnFilters.Items.Add(streamItem);
                }
            }
            if(cb_ListOnFilters.Items.Count > 0)
                cb_ListOnFilters.SelectedIndex = 0;

            lbl_NbUsedInFilterInMultiStream.Text = $"Videos in filter-Nb [{cb_ListOnFilters.Items.Count}]:";
        }

        private void cb_ListOnFilters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_ListOnFilters.SelectedItem is MediaInputStreamItem mediaInputStreamItem)
            {
                var id = mediaInputStreamItem.Id;
                IMedia? media = _mediaInputStreamsManager.GetMediaStream(id);

                    
                SetLabel(lbl_InfoFilterOnMultiStream, media);
            }
        }

        private void btn_RemoveSourceForFilter_Click(object sender, EventArgs e)
        {
            cb_ListOnFilters.Items.RemoveAt(cb_ListOnFilters.SelectedIndex);
            if (cb_ListOnFilters.Items.Count > 0)
                cb_ListOnFilters.SelectedIndex = 0;

            lbl_NbUsedInFilterInMultiStream.Text = $"Videos in filter-Nb [{cb_ListOnFilters.Items.Count}]:";
        }

        private void btn_RemoveAllSourcesForFilter_Click(object sender, EventArgs e)
        {
            cb_ListOnFilters.Items.Clear();

            lbl_NbUsedInFilterInMultiStream.Text = $"Videos in filter-Nb [{cb_ListOnFilters.Items.Count}]:";
        }

        private void btn_SetFilterOnMultiStream_Click(object sender, EventArgs e)
        {
            if (_currentMediaStreamFiltered == null)
                return;

            List<String> mediaIdList = new List<string>();
            foreach (var item in cb_ListOnFilters.Items)
            {
                if (item is MediaInputStreamItem streamItem)
                {
                    mediaIdList.Add(streamItem.Id);
                }
            }
            if(! _currentMediaStreamFiltered.SetVideoFilters(mediaIdList, tb_FilterOnMultiStream.Text))
                SetLabel(lbl_InfoFilterOnMultiStream, "Cannot set this filter - Check:\r\nTo have enough video source(s)\r\nTo use a correct filter string", true);
            else
                SetLabel(lbl_InfoFilterOnMultiStream, "Filter hes been set");
        }

        private void btn_ClearFilterOnMultiStream_Click(object sender, EventArgs e)
        {
            tb_FilterOnMultiStream.Text = "";
        }

        private void btn_CurrentFilterOnMultiStream_Click(object sender, EventArgs e)
        {
            if (_currentMediaStreamFiltered == null)
                return;

            tb_FilterOnMultiStream.Text = _currentMediaStreamFiltered.VideoFilter;

            // Fill  list of video used in the filter
            List<IMedia> mediasList = new List<IMedia>();
            if (_currentMediaStreamFiltered.VideoIdUsedInFilter != null)
            {
                foreach (string mediaId in _currentMediaStreamFiltered.VideoIdUsedInFilter)
                {
                    var media = _mediaInputStreamsManager.GetMediaStream(mediaId);
                    if (media != null)
                        mediasList.Add(media);
                }
            }
            FillComboBoxWithMediaInputStream(cb_ListOnFilters, mediasList, true);
            lbl_NbUsedInFilterInMultiStream.Text = $"Videos in filter-Nb [{cb_ListOnFilters.Items.Count}]:";

            SetLabel(lbl_InfoFilterOnMultiStream, "");
        }

        private void btn_StartVideoFilter_Click(object sender, EventArgs e)
        {
            if (_currentMediaStreamFiltered != null)
            {

                if (_currentMediaStreamFiltered.IsPaused)
                    _currentMediaStreamFiltered.Resume();
                else if (_currentMediaStreamFiltered.IsStarted)
                    _currentMediaStreamFiltered.Pause();
                else
                    _currentMediaStreamFiltered.Start();
            }

            UpdatePlayBtnForMediaFiltered();
        }

        private void btn_StopVideoFilter_Click(object sender, EventArgs e)
        {
            if (_currentMediaStreamFiltered != null)
                _currentMediaStreamFiltered.Stop();

            UpdatePlayBtnForMediaFiltered();
        }

        private void btn_ViewVideoFilter_Click(object sender, EventArgs e)
        {

        }

        private void btn_OutputVideoFilter_Click(object sender, EventArgs e)
        {
            if (_currentMediaStreamFiltered != null)
            {
                if (_currentMediaStreamFiltered is IMediaVideo mediaVideo)
                {
                    var form = new FormVideoOutputStream();
                    form.SetMediaVideo(mediaVideo);
                    form.Show();
                }
            }
        }

#endregion EVENTS from FORM elements - to manage MediaFiltered

#region EVENTS from FORM elements - to manage Audio (input / output)

        private void btn_AudioInputListRefresh_Click(object sender, EventArgs e)
        {
            FillAudioInputComboBox();
        }

        private void btn_AudioOutputListRefresh_Click(object sender, EventArgs e)
        {
            FillAudioOutputComboBox();
        }

        private void btn_SetAudioSettings_Click(object sender, EventArgs e)
        {
            if (cb_AudioInputList.SelectedIndex > 0)
            {
                var inputItem = cb_AudioInputList.SelectedItem;
                if (inputItem is MediaInputStreamItem mediaInputStreamItem)
                {
                    var media = _mediaInputStreamsManager.GetMediaStream(mediaInputStreamItem.Id);

                    if (media is IMediaAudio mediaAudio)
                    {
                        if ((_currentAudioInput == null) || (_currentAudioInput.Id != mediaAudio.Id))
                        {
                            StopAudioEvent();

                            _currentAudioInput = mediaAudio;
                        }
                    }
                }
                else if (inputItem is String audioInputDeviceName)
                {
                    // TODO - manage audioInputDeviceName
                }
            }
            else
            {
                StopAudioEvent();
            }

            if (cb_AudioOutputList.SelectedIndex > 0)
            {
                var outputItem = cb_AudioOutputList.SelectedItem;
                if(outputItem is String audioOutputDeviceName)
                {
                    if ( (_currentAudioOutput == null) || (_currentAudioOutput.Id != audioOutputDeviceName))
                    {
                        if (_currentAudioOutput != null)
                        {
                            StopAudioEvent();
                            _currentAudioOutput.Stop();
                        }

                        SDL2AudioOutput audioOutput = new SDL2AudioOutput(audioOutputDeviceName, audioOutputDeviceName);
                        if (audioOutput.Init())
                        {
                            audioOutput.Start();
                            _currentAudioOutput = audioOutput;
                        }
                        else
                        {
                            // TODO - need to inform of the pb (cannot init audio output)
                        }
                    }
                }
            }
            else
            {
                if (_currentAudioOutput != null)
                {
                    StopAudioEvent();
                    _currentAudioOutput.Stop();
                }
                _currentAudioOutput = null;
            }

            StartAudioEvent();
        }

        private void cb_AudioInputList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cb_DisposeMediaStream_CheckedChanged(object sender, EventArgs e)
        {
            _mediaInputStreamsManager.SetDisposeMediaStreamWhenStopped(cb_DisposeMediaStream.Checked);
        }

        private void btn_ManageWebcam_Click(object sender, EventArgs e)
        {
            _mediaInputStreamsManager.OpenFormWebcam();
        }

        private void btn_ManageScreen_Click(object sender, EventArgs e)
        {
            _mediaInputStreamsManager.OpenFormScreen();
        }

        private void btn_ManageAudio_Click(object sender, EventArgs e)
        {

        }

#endregion EVENTS from FORM elements - to manage Audio (input / output)
    }
}
