using Rainbow.Medias;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SDK.UIForm.WebRTC
{
    public partial class FormWebcam : Form
    {
        private WebcamDevice? _webcamDevice = null;
        private MediaInput? _mediaInput = null;
        

        private Object lockAboutImageManagement = new Object();

        private MediaInputStreamsManager _mediaInputStreamsManager;
        private int _x = 0;
        private int _y = 0;
        private int _diffHeight = 0;
        private int _diffWidth = 0;
        private float _ratio = 0;
        Boolean _initDone = false;

        public FormWebcam()
        {
            this.HandleCreated += Form_HandleCreated;
            InitializeComponent();
        }

        private void RefreshWebCamList()
        {
            if (!IsHandleCreated)
                return;

            var action = new Action(() =>
            {
                CloseMediaInput();

                cb_Webcams.Items.Clear();

                // Add first NONE
                cb_Webcams.Items.Add(Helper.NONE);

                var webcamsList = Devices.GetWebcamDevices();
                if (webcamsList != null)
                {
                    foreach (var webcam in webcamsList)
                    {
                        var item = new ListItem(webcam.Name, webcam.Id);
                        cb_Webcams.Items.Add(item);
                    }

                    if (cb_Webcams.Items?.Count > 0)
                        cb_Webcams.SelectedIndex = 0;
                }
            });

            if(this.InvokeRequired)
                this.BeginInvoke(action);
            else
                action.Invoke();
        }

        private void UpdatePlayBtn(string mediaId, Boolean isStarted, Boolean isPaused)
        {
            if (_webcamDevice == null)
                return;

            if (_webcamDevice.Id != mediaId)
                return;

            Bitmap bitmap;
            if (isPaused)
                bitmap = Helper.GetBitmapPlay();
            else if (isStarted)
                bitmap = Helper.GetBitmapPause();
            else
                bitmap = Helper.GetBitmapPlay();

            btn_StartWebcam.BackgroundImage = bitmap;

            if(!isStarted)
                Helper.UpdatePictureBox(pb_VideoStream, null);
        }

        private void CloseMediaInput()
        {
            if (_mediaInput != null)
            {
                _webcamDevice = null;

                _mediaInput.OnStateChanged -= MediaVideo_OnStateChanged;
                _mediaInput.OnImage -= MediaVideo_OnImage;

                _mediaInput.Dispose();
                _mediaInput = null;
            }

            btn_StartWebcam.BackgroundImage = Helper.GetBitmapPlay();

            // Set image to null
            Helper.UpdatePictureBox(pb_VideoStream, null);
        }

        private void CreateMediaInput()
        {
            if (!IsHandleCreated)
                return;

            var action = new Action(() =>
            {
                CloseMediaInput();

                lbl_Info.Text = "";
                btn_AddNewInputStream.Enabled = false;

                if (cb_Webcams.SelectedItem is ListItem item)
                    _webcamDevice = Devices.GetWebcamDeviceByName(item.Text);

                if (_webcamDevice != null)
                {
                    _mediaInput = new MediaInput(_webcamDevice);
                    if (_mediaInput.Init())
                    {
                        btn_AddNewInputStream.Enabled = true;
                        _mediaInput.OnStateChanged += MediaVideo_OnStateChanged;
                        _mediaInput.OnImage += MediaVideo_OnImage;
                    }
                    else
                    {
                        lbl_Info.Text = "Cannot init this webcam";
                        _mediaInput.Dispose();
                        _mediaInput = null;
                    }
                }
            });

            if (this.InvokeRequired)
                this.BeginInvoke(action);
            else
                action.Invoke();
        }

#region EVENTS from MediaVideo

        private void MediaVideo_OnImage(string mediaId, int width, int height, int stride, IntPtr data, FFmpeg.AutoGen.AVPixelFormat pixelFormat)
        {
            lock (lockAboutImageManagement)
            {
                var bitmap = Helper.BitmapFromImageData(width, height, stride, data, pixelFormat);
                Helper.UpdatePictureBox(pb_VideoStream, bitmap);
            }
        }

        private void MediaVideo_OnStateChanged(string mediaId, bool IsStarted, bool IsPaused)
        {
            Action action = new Action(() =>
            {
                UpdatePlayBtn(mediaId, IsStarted, IsPaused);
            });

            if (InvokeRequired)
                BeginInvoke(action);
            else
                action.Invoke();
        }

#endregion EVENTS from MediaVideo

#region EVENTS from MediaInputStreamsManager

        private void MediaInputStreamsManager_ListUpdated(object? sender, EventArgs e)
        {
            // TODO - Something to do here ?

            //if (!IsHandleCreated)
            //    return;

            //this.BeginInvoke(new Action(() =>
            //{
                
            //}));
        }

#endregion EVENTS from MediaInputStreamsManager


#region EVENTS from FORM elements

        private void Form_HandleCreated(object? sender, EventArgs e)
        {
            btn_StartWebcam.BackgroundImage = Helper.GetBitmapPlay();
            btn_StopWebcam.BackgroundImage = Helper.GetBitmapStop();
            btn_RefreshWebcam.BackgroundImage = Helper.GetBitmapRefresh();

            // Create MediaInputStreamsManager and init necessary events
            _mediaInputStreamsManager = MediaInputStreamsManager.Instance;
            _mediaInputStreamsManager.OnListUpdated += MediaInputStreamsManager_ListUpdated;
            _mediaInputStreamsManager.OnMediaStreamStateChanged += MediaVideo_OnStateChanged;

            _ratio = ((float)pb_VideoStream.Size.Width / (float)pb_VideoStream.Size.Height);

            _diffWidth = this.Width - pb_VideoStream.Size.Width;
            _diffHeight = this.Height - pb_VideoStream.Size.Height;

            _x = pb_VideoStream.Left;
            _y= pb_VideoStream.Top;

            lbl_WebcamMaxFps.Text = "";
            lbl_Info.Text = "";

            RefreshWebCamList();

            _initDone = true;
        }

        private void FormWebcam_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseMediaInput();

            e.Cancel = true;
            this.Hide();
        }

        private void FormWebcam_Shown(object sender, EventArgs e)
        {
            // TODO - FormWebcam_Shown => Avoir to lock webcam if form is not used
            //RefreshWebCamList();
        }

        private void FormWebcam_ClientSizeChanged(object sender, EventArgs e)
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

            pb_VideoStream.SetBounds(_x, _y, width, height);
        }

        private void btn_StartWebcam_Click(object sender, EventArgs e)
        {
            if(_mediaInput == null)
                CreateMediaInput();

            if (_mediaInput != null)
            {
                if (_mediaInput.IsPaused)
                    _mediaInput.Resume();
                else if (_mediaInput.IsStarted)
                    _mediaInput.Pause();
                else
                    _mediaInput.Start();
            }
        }

        private void btn_StopWebcam_Click(object sender, EventArgs e)
        {
            if(_mediaInput != null)
                _mediaInput.Stop();
        }

        private void btn_RefreshWebcam_Click(object sender, EventArgs e)
        {
            RefreshWebCamList();
        }

        private void cb_Webcams_SelectedIndexChanged(object sender, EventArgs e)
        {
            CreateMediaInput();
        }

        private void cb_WebcamSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            // TODO - cb_WebcamSize_SelectedIndexChanged - not managed yet
        }

        private void btn_WebcamSet_Click(object sender, EventArgs e)
        {
            // TODO - cb_WebcamSize_SelectedIndexChanged - not managed yet
        }

        private void btn_AddNewInputStream_Click(object sender, EventArgs e)
        {
            // TODO - btn_AddNewInputStream_Click

            if (String.IsNullOrEmpty(tb_NameOfNewInputStream.Text))
                return;

            if (_mediaInput == null)
            {
                CreateMediaInput();
                if (_mediaInput == null)
                    return;
            }
            else
                _mediaInput.Stop();

            _webcamDevice.Id = tb_NameOfNewInputStream.Text;
            var newMediaInput = new MediaInput(_webcamDevice);

            if (newMediaInput?.Init() == true)
            {
                if (_mediaInputStreamsManager.Add(newMediaInput))
                    Close();
                else
                    newMediaInput.Dispose();
            }
        }


#endregion EVENTS from FORM elements


    }
}