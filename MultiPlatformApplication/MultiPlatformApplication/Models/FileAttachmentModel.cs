using MultiPlatformApplication.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MultiPlatformApplication.Models
{
    public class FilesAttachmentModel : FileAttachmentModel
    {
        private ObservableRangeCollection<FileAttachmentModel> items;

        public ObservableRangeCollection<FileAttachmentModel> Items
        {
            get { return items; }
            set { SetProperty(ref items, value); }
        }

        public FilesAttachmentModel()
        {
            Items = new ObservableRangeCollection<FileAttachmentModel>();
        }

        public void RemoveAt(int index)
        {
            Items.RemoveAt(index);
        }

        public void Insert(int index, FileAttachmentModel item)
        {
            Items.Insert(index, item);
        }
    }

    public class FileAttachmentModel: ObservableObject
    {
        String fileId;
        String imageSourceId;
        String filename;
        String fileSize;
        String fileDetails;

        ICommand selectionCommand;

        public string FileId
        {
            get { return fileId; }
            set { SetProperty(ref fileId, value); }
        }

        public string ImageSourceId
        {
            get { return imageSourceId; }
            set { SetProperty(ref imageSourceId, value); }
        }

        public string FileName
        {
            get { return filename; }
            set { SetProperty(ref filename, value); }
        }

        public string FileSize
        {
            get { return fileSize; }
            set { SetProperty(ref fileSize, value); }
        }

        public string FileDetails
        {
            get { return fileDetails; }
            set { SetProperty(ref fileDetails, value); }
        }

        public ICommand SelectionCommand
        {
            get { return selectionCommand; }
            set { SetProperty(ref selectionCommand, value); }
        }

    }
}
