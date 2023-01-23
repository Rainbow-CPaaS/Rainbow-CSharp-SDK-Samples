using Rainbow.Medias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SDK.UIForm.WebRTC
{
    public partial class FormMediaFilteredStreams : Form
    {
        private static String NEW = "NEW";

        private MediaInputStreamsManager _mediaInputStreamsManager;

        private String _selectedMediaFilteredId = "";

#region CONSTRUCTOR
        
        public FormMediaFilteredStreams()
        {
            this.HandleCreated += Form_HandleCreated;
            InitializeComponent();

            
        }

        private void Form_HandleCreated(object sender, EventArgs e)
        {
            ResetLabelUsedForInfo();

            // Create MediaInputStreamsManager and init necessary event(s)
            _mediaInputStreamsManager = MediaInputStreamsManager.Instance;
            _mediaInputStreamsManager.OnListUpdatedWithoutFilteredInputs += MediaInputStreamsManager_ListUpdatedWithoutFilteredInputs;
            _mediaInputStreamsManager.OnListUpdatedWithFilteredInputs += MediaInputStreamsManager_ListUpdatedWithFilteredInputs;

            FillListOfMediaInputFiltered();
            FillListOfMediaInputSimple();
        }

#endregion CONSTRUCTOR

#region EVENTS from MediaInputStreamsManager


        private void MediaInputStreamsManager_ListUpdatedWithoutFilteredInputs(object? sender, EventArgs e)
        {
            this.BeginInvoke(new Action(() =>
            {
                FillListOfMediaInputSimple();
            }));
        }

        private void MediaInputStreamsManager_ListUpdatedWithFilteredInputs(object? sender, EventArgs e)
        {
            this.BeginInvoke(new Action(() =>
            {
                FillListOfMediaInputFiltered();
            }));
        }

#endregion EVENTS from MediaInputStreamsManager


#region PRIVATE API

        private void FillListOfMediaInputFiltered()
        {
            var dicoFiltered = _mediaInputStreamsManager.GetList(true, false, false, false);
            FillComboBoxWithMediaInputStream(cb_ListOfInputFilteredStreams, dicoFiltered.Values.ToList());
        }

        private void FillListOfMediaInputSimple()
        {
            var dicoSimple = _mediaInputStreamsManager.GetList(false, true, true, false);
            FillListViewWithMediaInputStream(lv_InputStreams, dicoSimple.Values.ToList());
        }

        private void ResetLabelUsedForInfo()
        {
            lbl_InfoAboutInputStream.Text = "";
            lbl_InfoAboutInputStreamInFilter.Text = "";
        }

        private void SetLabel(Label? lbl, IMedia? mediaStream)
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

        private void FillComboBoxWithMediaInputStream(System.Windows.Forms.ComboBox comboBox, List<IMedia>? mediaList)
        {
            if (comboBox == null)
                return;

            String selectedId = _selectedMediaFilteredId;

            int selectedIndex = 0;

            comboBox.Items.Clear();

            // First add NEW media stream
            comboBox.Items.Add(NEW);

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

            comboBox.SelectedIndex = selectedIndex;
        }

        private void FillListViewWithMediaInputStream(System.Windows.Forms.ListView listView, List<IMedia>? mediaList)
        {
            if (listView == null)
                return;

            listView.Items.Clear();

            if (mediaList != null)
            {
                foreach (var mediaStream in mediaList)
                {
                    MediaInputStreamItem mediaInputStreamItem = new MediaInputStreamItem(mediaStream);
                    listView.Items.Add(mediaInputStreamItem.Id, mediaInputStreamItem.ToString(), 0);
                }
            }
        }

        private void AddNewInputFilteredStream(String newId)
        {
            if (String.IsNullOrEmpty(newId))
                return;

            // Avoid to use same Id
            if (!_mediaInputStreamsManager.CheckIdUnicity(newId))
                return;

            // Get all media stream for the list
            List<MediaInput> mediaInputsList = new List<MediaInput>();

            foreach (var item in lv_InputStreamsInFilter.Items)
            {
                if (item is ListViewItem listViewItem)
                {
                    var id = listViewItem.Name;
                    var mediaStream = _mediaInputStreamsManager.GetMediaStream(id);
                    if ((mediaStream != null) && (mediaStream is MediaInput mediaInput))
                        mediaInputsList.Add(mediaInput);
                }
            }

            if (mediaInputsList.Count > 0)
            {
                MediaFiltered mediaFiltered = new MediaFiltered(newId, mediaInputsList);

                // We set by default a "null" filter with the first mediaInput
                mediaFiltered.SetVideoFilter(new List<string>() { mediaInputsList.First().Id });

                _mediaInputStreamsManager.Add(mediaFiltered);
            }
        }

#endregion PRIVATE API

#region EVENTS from FORM elements

        private void FormMediaFilteredStreams_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void cb_ListOfInputFilteredStreams_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Enabled / Disabled buttons: Add, Remove and Save
            btn_AddNewInputFilteredStream.Enabled = (cb_ListOfInputFilteredStreams.SelectedIndex == 0);
            btn_RemoveInputFilteredStream.Enabled = (cb_ListOfInputFilteredStreams.SelectedIndex != 0);
            btn_SaveInputFilteredStream.Enabled = (cb_ListOfInputFilteredStreams.SelectedIndex != 0);

            // need to fill lv_InputStreamsInFilter with correct values
            if (cb_ListOfInputFilteredStreams.SelectedItem is MediaInputStreamItem mediaInputStreamItem)
            {
                IMedia? mediaStream = _mediaInputStreamsManager.GetMediaStream(mediaInputStreamItem.Id);
                if(mediaStream is MediaFiltered mediaFiltered)
                {
                    lv_InputStreamsInFilter.Items.Clear();
                    var dicoMediaInputs = mediaFiltered.MediaInputs;
                    if(dicoMediaInputs != null)
                    {
                        List<IMedia> mediaList = new List<IMedia>();
                        foreach (var mediaInput in dicoMediaInputs.Values.ToList())
                        {
                            mediaList.Add(mediaInput);
                        }
                        FillListViewWithMediaInputStream(lv_InputStreamsInFilter, mediaList);
                    }
                }
            }
        }
        
        private void lv_InputStreams_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lv_InputStreams.SelectedItems.Count > 0)
            {
                var selectedItem = lv_InputStreams.SelectedItems[0];
                if (selectedItem != null)
                {
                    var id = selectedItem.Name;
                    var mediaStream = _mediaInputStreamsManager.GetMediaStream(id);
                    SetLabel(lbl_InfoAboutInputStream, mediaStream);
                }
            }
            else
                SetLabel(lbl_InfoAboutInputStream, "");
        }

        private void lv_InputStreams_DoubleClick(object sender, EventArgs e)
        {
            if (lv_InputStreams.SelectedItems.Count > 0)
            {
                var selectedItem = lv_InputStreams.SelectedItems[0];
                if(selectedItem != null)
                {
                    var id = selectedItem.Name;
                    Boolean canAdd = true; ;
                    foreach(var item in lv_InputStreamsInFilter.Items)
                    {
                        if (item is ListViewItem listViewItem)
                        {
                            if (listViewItem.Name == selectedItem.Name)
                            {
                                canAdd = false;
                                break;
                            }
                        }
                    }

                    if(canAdd)
                        lv_InputStreamsInFilter.Items.Add(selectedItem.Name, selectedItem.Text, 0);
                }
            }
        }

        private void btn_AddInputStreamInFilter_Click(object sender, EventArgs e)
        {
            lv_InputStreams_DoubleClick(null, null);
        }

        private void lv_InputStreamsInFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lv_InputStreamsInFilter.SelectedItems.Count > 0)
            {
                var selectedItem = lv_InputStreamsInFilter.SelectedItems[0];
                if (selectedItem != null)
                {
                    var id = selectedItem.Name;
                    var mediaStream = _mediaInputStreamsManager.GetMediaStream(id);
                    SetLabel(lbl_InfoAboutInputStreamInFilter, mediaStream);
                }
            }
            else
                SetLabel(lbl_InfoAboutInputStream, "");
        }

        private void lv_InputStreamsInFilter_DoubleClick(object sender, EventArgs e)
        {
            if(lv_InputStreamsInFilter.SelectedItems.Count > 0)
            {
                var item = lv_InputStreamsInFilter.SelectedItems[0];
                lv_InputStreamsInFilter.Items.Remove(item);
            }
        }

        private void btn_RemoveInputStreamInFilter_Click(object sender, EventArgs e)
        {
            lv_InputStreamsInFilter_DoubleClick(null, null);
        }

        private void btn_RemoveInputFilteredStream_Click(object sender, EventArgs e)
        {
            if(cb_ListOfInputFilteredStreams.SelectedItem is MediaInputStreamItem mediaInputStreamItem)
                _mediaInputStreamsManager.Remove(mediaInputStreamItem.Id);
        }

        private void btn_SaveInputFilteredStream_Click(object sender, EventArgs e)
        {
            if (cb_ListOfInputFilteredStreams.SelectedItem is MediaInputStreamItem mediaInputStreamItem)
            {
                var id = mediaInputStreamItem.Id;
                if (_mediaInputStreamsManager.Remove(id))
                {
                    _selectedMediaFilteredId = id;
                    AddNewInputFilteredStream(id);
                }
            }
        }

        private void btn_AddNewInputFilteredStream_Click(object sender, EventArgs e)
        {
            _selectedMediaFilteredId = tb_NameOfNewInputFilteredStream.Text;
            AddNewInputFilteredStream(tb_NameOfNewInputFilteredStream.Text);
        }

#endregion EVENTS from FORM elements
    }
}
