using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using Rainbow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MessageInputAttachments : ContentView
    {
        List<FileResult> filesResultAttached = new List<FileResult>();

        public event EventHandler<EventArgs> UpdateParentLayout; // To ask parent to update the layout when the size of this UI component has changed

        RelayCommand<object> SelectionCommand;

#region BINDINGS used in XAML

        public FilesAttachmentModel FilesAttached { get; private set; } = new FilesAttachmentModel();

#endregion BINDINGS used in XAML

        public MessageInputAttachments()
        {
            InitializeComponent();

            this.BindingContextChanged += MessageInputAttachments_BindingContextChanged;

            Frame.PropertyChanged += Frame_PropertyChanged; // Need to update the layout when the height is Changed

            AttachmentsListView.ItemSelected += AttachmentsListView_ItemSelected;

            // We prevent MouseOver effect on ListView. "SelecionMode" must also be set to "None"
            AttachmentsListView.On<Windows>().SetSelectionMode(Xamarin.Forms.PlatformConfiguration.WindowsSpecific.ListViewSelectionMode.Inaccessible);

            SelectionCommand = new RelayCommand<object>(new Action<object>(SelectionCommandExecute));
        }

        private void Frame_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsVisible")
                UpdateParentLayout.Raise(this, null);
            else if (e.PropertyName == "Height")
                UpdateParentLayout.Raise(this, null);
        }

        private void SelectionCommandExecute(object obj)
        {
            if (FilesAttached.Items?.Count > 0)
            {
                if (obj is String)
                {
                    String fileId = (String)obj;
                    int index = -1;
                    for (int i = 0; i < FilesAttached.Items.Count; i++)
                    {
                        if (FilesAttached.Items[i].FileId == fileId)
                        {
                            index = i;
                            break;
                        }
                    }

                    if(index != -1)
                    {
                        FilesAttached.RemoveAt(index);
                        filesResultAttached.RemoveAt(index);

                        UpdateFrameSize();
                    }
                }
            }
        }

        private void MessageInputAttachments_BindingContextChanged(object sender, EventArgs e)
        {
            AttachmentsListView.BindingContext = FilesAttached;
            AttachmentsListView.ItemsSource = FilesAttached.Items;
        }

        private void AttachmentsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ((Xamarin.Forms.ListView)sender).SelectedItem = null;
        }

        public bool FileAlreadyAttached(String fullPath)
        {
            foreach(FileResult fileResult in filesResultAttached)
            {
                if (fileResult.FullPath == fullPath)
                    return true;
            }
            return false;
        }

        private void UpdateFrameSize()
        {
            Frame.IsVisible = (FilesAttached.Items.Count > 0);

            // Before to add need element, we check if we need to restrict Height
            if (FilesAttached.Items.Count < 3)
                Frame.HeightRequest = 34 * FilesAttached.Items.Count + 4;
            else
                Frame.HeightRequest = 34 * 2.5 + 4;
        }

        public async void PickFiles()
        {
            List<FileResult> filesList = (await Helper.PickFilesAndShow())?.ToList();

            if (filesList == null)
                return;

            foreach (FileResult fileResult in filesList)
            {
                if (!FileAlreadyAttached(fileResult.FullPath))
                {
                    var stream = await fileResult.OpenReadAsync();
                    if (stream != null)
                    {
                        FileAttachmentModel fileAttachement = new FileAttachmentModel()
                        {
                            FileId = Rainbow.Util.GetGUID(),
                            FileName = fileResult.FileName,
                            FileSize = Helper.HumanizeFileSize(stream.Length),
                            ImageSourceId = Helper.GetFileSourceIdFromFileName(fileResult.FileName),
                            SelectionCommand = this.SelectionCommand
                        };
                        fileAttachement.FileDetails = fileAttachement.FileName + " (" + fileAttachement.FileSize + ")";

                        // Add to the list of FileAttachmentModel
                        FilesAttached.Insert(0, fileAttachement);

                        // Add to the list of FileResult
                        filesResultAttached.Insert(0, fileResult);

                        stream.Close();
                        stream.Dispose();
                    }
                }
            }

            UpdateFrameSize();
        }
    }
}