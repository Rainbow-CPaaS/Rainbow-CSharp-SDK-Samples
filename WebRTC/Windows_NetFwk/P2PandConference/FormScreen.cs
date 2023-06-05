using Rainbow.Medias;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SDK.UIForm.WebRTC
{
    public partial class FormScreen : Form
    {
        private ScreenDevice? _screenDevice = null;
        private MediaInput? _mediaInput = null;
        

        private Object lockAboutImageManagement = new Object();

        private MediaInputStreamsManager _mediaInputStreamsManager;
        private int _x = 0;
        private int _y = 0;
        private int _diffHeight = 0;
        private int _diffWidth = 0;
        private float _ratio = 0;
        Boolean _initDone = false;

        public FormScreen()
        {
            this.HandleCreated += Form_HandleCreated;
            InitializeComponent();
        }

        private void RefreshScreensList()
        {
            if (!IsHandleCreated)
                return;

            var action = new Action(() =>
            {
                CloseMediaInput();

                cb_Screens.Items.Clear();

                // Add first NONE
                cb_Screens.Items.Add(Helper.NONE);

                var screenssList = Devices.GetScreenDevices();
                if (screenssList != null)
                {
                    foreach (var screen in screenssList)
                    {
                        var item = new ListItem(screen.Name, screen.Id);
                        cb_Screens.Items.Add(item);
                    }

                    if (cb_Screens.Items?.Count > 0)
                        cb_Screens.SelectedIndex = 0;
                }
            });

            if(this.InvokeRequired)
                this.BeginInvoke(action);
            else
                action.Invoke();
        }

        private void UpdatePlayBtn(string mediaId, Boolean isStarted, Boolean isPaused)
        {
            if (_screenDevice == null)
                return;

            if (_screenDevice.Id != mediaId)
                return;

            Bitmap bitmap;
            if (isPaused)
                bitmap = Helper.GetBitmapPlay();
            else if (isStarted)
                bitmap = Helper.GetBitmapPause();
            else
                bitmap = Helper.GetBitmapPlay();

            btn_StartScreen.BackgroundImage = bitmap;

            if(!isStarted)
                Helper.UpdatePictureBox(pb_VideoStream, null);
        }

        private void CloseMediaInput()
        {
            if (_mediaInput != null)
            {
                _screenDevice = null;

                _mediaInput.OnStateChanged -= MediaVideo_OnStateChanged;
                _mediaInput.OnImage -= MediaVideo_OnImage;

                _mediaInput.Dispose();
                _mediaInput = null;
            }

            btn_StartScreen.BackgroundImage = Helper.GetBitmapPlay();

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

                if (cb_Screens.SelectedItem is ListItem item)
                    _screenDevice = Devices.GetScreenDeviceByName(item.Text);

                if (_screenDevice != null)
                {
                    _mediaInput = new MediaInput(_screenDevice);
                    if (_mediaInput.Init())
                    {
                        btn_AddNewInputStream.Enabled = true;
                        _mediaInput.OnStateChanged += MediaVideo_OnStateChanged;
                        _mediaInput.OnImage += MediaVideo_OnImage;
                    }
                    else
                    {
                        lbl_Info.Text = "Cannot init this screen";
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
            btn_StartScreen.BackgroundImage = Helper.GetBitmapPlay();
            btn_StopScreen.BackgroundImage = Helper.GetBitmapStop();
            btn_RefreshScreen.BackgroundImage = Helper.GetBitmapRefresh();

            // Create MediaInputStreamsManager and init necessary events
            _mediaInputStreamsManager = MediaInputStreamsManager.Instance;
            _mediaInputStreamsManager.OnListUpdated += MediaInputStreamsManager_ListUpdated;
            _mediaInputStreamsManager.OnMediaStreamStateChanged += MediaVideo_OnStateChanged;

            lbl_ScreenMaxFps.Text = "";
            lbl_Info.Text = "";

            _ratio = ((float)pb_VideoStream.Size.Width / (float)pb_VideoStream.Size.Height);

            _diffWidth = this.Width - pb_VideoStream.Size.Width;
            _diffHeight = this.Height - pb_VideoStream.Size.Height;

            _x = pb_VideoStream.Left;
            _y = pb_VideoStream.Top;

            RefreshScreensList();

            _initDone = true;
        }

        private void FormScreen_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseMediaInput();

            e.Cancel = true;
            this.Hide();
        }

        private void FormScreen_Shown(object sender, EventArgs e)
        {
            // TODO - FormScreen_Shown => Avoir to lock Screen if form is not used
            //RefreshScreenList();
        }


        private void FormScreen_ClientSizeChanged(object sender, EventArgs e)
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

        private void btn_StartScreen_Click(object sender, EventArgs e)
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

        private void btn_StopScreen_Click(object sender, EventArgs e)
        {
            if(_mediaInput != null)
                _mediaInput.Stop();
        }

        private void btn_RefreshScreen_Click(object sender, EventArgs e)
        {
            RefreshScreensList();
        }

        private void cb_Screens_SelectedIndexChanged(object sender, EventArgs e)
        {
            CreateMediaInput();
        }

        private void cb_ScreenSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            // TODO - cb_ScreenSize_SelectedIndexChanged - not managed yet
        }

        private void btn_ScreenSet_Click(object sender, EventArgs e)
        {
            // TODO - cb_ScreenSize_SelectedIndexChanged - not managed yet
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

            _screenDevice.Id = tb_NameOfNewInputStream.Text;
            var newMediaInput = new MediaInput(_screenDevice);

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