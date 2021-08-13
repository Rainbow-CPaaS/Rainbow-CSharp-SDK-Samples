using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using NLog;
using Rainbow;
using Rainbow.Events;
using Rainbow.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace MultiPlatformApplication.Services
{
    /// <summary>
    /// To manage files upload one by one (singleton)
    /// </summary>
    public sealed class FilesUpload
    {
        private  readonly Logger log = LogConfigurator.GetLogger(typeof(FilesUpload));

#region PUBLIC EVENTS
        /// <summary>
        /// Event raised when a file descriptor has been created just before the procees of file upload 
        /// 
        /// Information provided in the event: PeerId and FileDescriptorId
        /// </summary>
        public event EventHandler<StringListEventArgs> FileDescriptorCreated;

        /// <summary>
        /// Event raised when a file descriptor cannot be created.
        /// 
        /// Information provided in the event: PeerId and FileName
        /// </summary>
        public event EventHandler<StringListEventArgs> FileDescriptorNotCreated;

        /// <summary>
        /// Event raised when the file upload process cannot start
        /// 
        /// Information provided in the event: PeerId, FileName and FileDescriptorId
        /// </summary>
        public event EventHandler<StringListEventArgs> UploadNotPerformed;

#endregion PUBLIC EVENTS

        private static FilesUpload instance = null;
        private Object lockFiles = new Object();

        private Dictionary<String, FileUploadModel> FilesByFullPath = new Dictionary<String, FileUploadModel>(); // Dictionary of FileUploadModel by FileFullPath (MAIN DICTIONARY)
        private Dictionary<String, String> FileFullPathByFileDescriptorId = new Dictionary<String, String>(); // Dictionary of FileFullPath by FileDescriptorId
        private Dictionary<String, List<String>> FilesFullPathListByPeerId = new Dictionary<String, List<String>>(); // Dictionary of List<FileFullPath> by PeerId
        
        private BackgroundWorker backgroundWorkerFileDescriptor = null;
        private BackgroundWorker backgroundWorkerUpload = null;

        private Boolean initEventsDone = false;
        private String uploadFileDescriptorIdInProgress = null;


#region BACKGROUND WORKER  - TO UPLOAD FILE
        private void UseUploadPool()
        {
            if (!CanUseUploadPool())
                return;

            if (backgroundWorkerUpload == null)
            {
                backgroundWorkerUpload = new BackgroundWorker();
                backgroundWorkerUpload.DoWork += BackgroundWorkerUpload_DoWork;
                backgroundWorkerUpload.RunWorkerCompleted += BackgroundWorkerUpload_RunWorkerCompleted;
                backgroundWorkerUpload.WorkerSupportsCancellation = true;
            }

            if (!backgroundWorkerUpload.IsBusy)
                backgroundWorkerUpload.RunWorkerAsync();
        }

        private void BackgroundWorkerUpload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UseUploadPool();
        }

        private void BackgroundWorkerUpload_DoWork(object sender, DoWorkEventArgs e)
        {
            FileUploadModel fileUploadModel = null;
            lock (lockFiles)
            {
                if (uploadFileDescriptorIdInProgress != null)
                    return;

                if (FilesByFullPath.Count > 0)
                {
                    // Take first element
                    fileUploadModel = FilesByFullPath.First().Value;

                    // Store the FileDescriptor Id which will be uploaded
                    uploadFileDescriptorIdInProgress = fileUploadModel.FileDescriptor.Id;
                }
            }

            // Start the upload
            if(fileUploadModel != null)
                Helper.SdkWrapper.UploadFile(fileUploadModel.Stream, fileUploadModel.PeerId, fileUploadModel.PeerType, fileUploadModel.FileDescriptor, null);
        }

        private void UploadCannnotBeDone(FileUploadModel fileUploadModel)
        {
            // Close the stream
            CloseStream(fileUploadModel);

            // Remove this file from dictionaries
            RemoveFileByFullPath(fileUploadModel.FileFullPath);

            // Need to inform that this file cannot be uploaded
            UploadNotPerformed.Raise(this, new StringListEventArgs(new List<String>() { fileUploadModel.PeerId, fileUploadModel.FileName, fileUploadModel.FileDescriptor?.Id }));
        }

        private Boolean CanUseUploadPool()
        {
            Boolean result = false;

            // If we are connected, chcek if at least a file must be uploade
            lock (lockFiles)
            {
                if (Helper.SdkWrapper.IsInitialized())
                    result = (FilesByFullPath.Count > 0) && (uploadFileDescriptorIdInProgress == null);
            }
            return result;
        }

#endregion BACKGROUND WORKER  - TO UPLOAD FILE


#region BACKGROUND WORKER  - TO GET FILE DESCRIPTOR

        private void UseFileDescriptorPool()
        {
            if (!CanUseFileDescriptorPool())
                return;

            if (backgroundWorkerFileDescriptor == null)
            {
                backgroundWorkerFileDescriptor = new BackgroundWorker();
                backgroundWorkerFileDescriptor.DoWork += BackgroundWorkerFileDescriptor_DoWork;
                backgroundWorkerFileDescriptor.RunWorkerCompleted += BackgroundWorkerFileDescriptor_RunWorkerCompleted;
                backgroundWorkerFileDescriptor.WorkerSupportsCancellation = true;
            }

            if (!backgroundWorkerFileDescriptor.IsBusy)
                backgroundWorkerFileDescriptor.RunWorkerAsync();
        }

        private void BackgroundWorkerFileDescriptor_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UseFileDescriptorPool();
        }

        private void BackgroundWorkerFileDescriptor_DoWork(object sender, DoWorkEventArgs e)
        {
            List<FileUploadModel> filesUploadModelList = new List<FileUploadModel>();
            
            // We ask one by one a file descriptor
            lock (lockFiles)
            {
                foreach (FileUploadModel fileUploadModel in FilesByFullPath.Values)
                {
                    if (fileUploadModel.FileDescriptor == null)
                        filesUploadModelList.Add(fileUploadModel);
                }
            }

            if(filesUploadModelList.Count > 0)
            {
                Boolean fileDescriptorCreated = false;
                ManualResetEvent manualResetEvent = new ManualResetEvent(false);

                foreach (FileUploadModel fileUploadModel in filesUploadModelList)
                {
                    try
                    {
                        // We create file descriptor one by one so we wiil have to wait before to continue
                        manualResetEvent.Reset();

                        Helper.SdkWrapper.CreateFileDescriptor(fileUploadModel.FileName, fileUploadModel.FileSize, fileUploadModel.PeerId, fileUploadModel.PeerType, callback =>
                        {
                            if (callback.Result.Success)
                            {
                                fileDescriptorCreated = true;

                                // Store file descriptor
                                fileUploadModel.FileDescriptor = callback.Data;

                                // Store the correlation between FileDescriptor.Id and FileFullPath
                                FileFullPathByFileDescriptorId.Add(fileUploadModel.FileDescriptor.Id, fileUploadModel.FileFullPath);

                                // Create Rainbow.Message
                                Conversation conversation = Helper.SdkWrapper.GetConversationByPeerIdFromCache(fileUploadModel.PeerId);
                                fileUploadModel.RbMessage = CreateRbMessage(fileUploadModel, conversation.Jid_im);

                                // Inform that a file descriptor has been created
                                Task task = new Task(() =>
                                {
                                    FileDescriptorCreated.Raise(instance, new StringListEventArgs(new List<String>() { fileUploadModel.PeerId, fileUploadModel.FileDescriptor.Id }));
                                });
                                task.Start();
                            }
                            else
                            {
                                // Cannot create File Descriptor
                                FileDescriptorCannnotBeCreated(fileUploadModel);
                            }

                            // Stop to wait
                            manualResetEvent.Set();
                        });

                        // We create file descriptor one by one so we wait before to continue
                        if (!manualResetEvent.WaitOne(Helper.SdkWrapper.GetTimeout()))
                        {
                            // Cannot create File Descriptor
                            FileDescriptorCannnotBeCreated(fileUploadModel);
                        }
                    }
                    catch
                    {
                        // Cannot create File Descriptor
                        FileDescriptorCannnotBeCreated(fileUploadModel);
                    }

                }

                // Since we have created a file descriptor, we ask the pool to start upload
                if(fileDescriptorCreated)
                    UseUploadPool();
            }
        }

        private void FileDescriptorCannnotBeCreated(FileUploadModel fileUploadModel)
        {
            // Close the stream
            CloseStream(fileUploadModel);

            // Remove this file from dictionaries
            RemoveFileByFullPath(fileUploadModel?.FileFullPath);

            // Need to inform that this file cannot be uploaded 
            FileDescriptorNotCreated.Raise(instance, new StringListEventArgs(new List<String>() { fileUploadModel.PeerId, fileUploadModel.FileName }));
        }

        private  Boolean CanUseFileDescriptorPool()
        {
            Boolean result = false;

            // If we are connected, chcek if at least a file descriptor is missing
            if(Helper.SdkWrapper.IsInitialized())
            {
                lock(lockFiles)
                {
                    foreach(FileUploadModel fileUploadModel in FilesByFullPath.Values)
                    {
                        if(fileUploadModel.FileDescriptor == null)
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            return result;
        }

#endregion BACKGROUND WORKER - TO GET FILE DESCRIPTOR


#region EVENTS FROM SDK WRAPPER

        private void SdkWrapper_FileUploadUpdated(object sender, FileUploadEventArgs e)
        {
            if(e.FileDescriptor.Id == uploadFileDescriptorIdInProgress)
            {
                if(!e.InProgress)
                {
                    FileUploadModel fileUploadModel = GetFileUploadByFileDescriptorId(uploadFileDescriptorIdInProgress);
                    if (e.Completed)
                    {
                        // Send message
                        Conversation conversation = Helper.SdkWrapper.GetConversationByPeerIdFromCache(fileUploadModel.PeerId);
                        Rainbow.Model.Message message = fileUploadModel.RbMessage;
                        Helper.SdkWrapper.SendMessage(conversation, ref message);

                        // Remove this file from dictionaries
                        RemoveFileByFullPath(fileUploadModel.FileFullPath); // The stream is closed in this method
                    }
                    else
                    { 
                        UploadCannnotBeDone(fileUploadModel);
                    }

                    // The upload is finished
                    uploadFileDescriptorIdInProgress = null;

                    // Use upload pool
                    UseUploadPool();
                }
            }
        }

#endregion EVENTS FROM SDK WRAPPER


#region PUBLIC METHODS

        public static FilesUpload Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FilesUpload();
                }
                return instance;
            }
        }

        // Must be called on UI THREAD
        public void AddFiles(List<FileUploadModel> files)
        {
            // We will need to deal with some dkWrapper events
            InitSdkWrapperEvents();

            lock (lockFiles)
            {
                if (files?.Count > 0)
                {
                    List<String> filesFullPathList;

                    foreach (FileUploadModel fileUploadModel in files)
                    {
                        if (!FilesByFullPath.ContainsKey(fileUploadModel.FileFullPath))
                        {
                            // Update Dictionary of FilesByFullPath
                            FilesByFullPath.Add(fileUploadModel.FileFullPath, fileUploadModel);

                            // Update Dictionary of FileFullPathByConversationId
                            if (FilesFullPathListByPeerId.ContainsKey(fileUploadModel.PeerId))
                            {
                                filesFullPathList = FilesFullPathListByPeerId[fileUploadModel.PeerId];
                                filesFullPathList.Add(fileUploadModel.FileFullPath);
                            }
                            else
                            {
                                filesFullPathList = new List<string>();
                                filesFullPathList.Add(fileUploadModel.FileFullPath);
                                FilesFullPathListByPeerId.Add(fileUploadModel.PeerId, filesFullPathList);
                            }
                        }
                    }
                }
            }
            // Use file descriptor pool => to start the creation of File Descriptor
            UseFileDescriptorPool();
        }

        public FileUploadModel GetFileUploadByFileDescriptorId(String fileDescriptorId)
        {
            FileUploadModel result = null;
            lock (lockFiles)
            {
                if(FileFullPathByFileDescriptorId.ContainsKey(fileDescriptorId))
                {
                    String fullPath = FileFullPathByFileDescriptorId[fileDescriptorId];
                    if (FilesByFullPath.ContainsKey(fullPath))
                        result = FilesByFullPath[fullPath];
                    else
                        FileFullPathByFileDescriptorId.Remove(fileDescriptorId);

                }
            }
            return result;
        }

        public List<FileUploadModel> GetFilesUploadByPeerId(String conversationId)
        {
            List<FileUploadModel> result = null;
            
            lock (lockFiles)
            {
                if (FilesFullPathListByPeerId.ContainsKey(conversationId))
                {
                    List<String> filesFullPathList = FilesFullPathListByPeerId[conversationId];
                    result = new List<FileUploadModel>();

                    foreach(String fullPath in filesFullPathList)
                    {
                        if (FilesByFullPath.ContainsKey(fullPath))
                            result.Add(FilesByFullPath[fullPath]);
                    }
                }
            }
            return result;
        }

#endregion PUBLIC METHODS


#region PRIVATE METHODS

        private void InitSdkWrapperEvents()
        {
            if (!initEventsDone)
            {
                initEventsDone = true;
                Helper.SdkWrapper.FileUploadUpdated += SdkWrapper_FileUploadUpdated;
            }
        }

        private void RemoveFileByFullPath(String fullPath)
        {
            if (String.IsNullOrEmpty(fullPath))
                return;

            lock (lockFiles)
            {
                if(FilesByFullPath.ContainsKey(fullPath))
                {
                    FileUploadModel fileUploadModel = FilesByFullPath[fullPath];

                    if (fileUploadModel != null)
                    {
                        CloseStream(fileUploadModel);

                        // Update FileFullPathByFileDescriptorId Dictionary
                        String fileDescriptorId = fileUploadModel?.FileDescriptor?.Id;
                        if (fileDescriptorId != null)
                        {
                            if (FileFullPathByFileDescriptorId.ContainsKey(fileDescriptorId))
                                FileFullPathByFileDescriptorId.Remove(fileDescriptorId);
                        }

                        // Update FilesFullPathListByConversationId Dictionary
                        String conversationId = fileUploadModel.PeerId;
                        if (conversationId != null)
                        {
                            if (FilesFullPathListByPeerId.ContainsKey(conversationId))
                            {
                                List<String> fullPathList = FilesFullPathListByPeerId[conversationId];

                                if (fullPathList?.Contains(fullPath) == true)
                                    fullPathList.Remove(fullPath);

                                if (fullPathList?.Count == 0)
                                    FilesFullPathListByPeerId.Remove(conversationId);
                            }
                        }

                        // Update FilesByFullPath Dictionary
                        FilesByFullPath.Remove(fullPath);
                    }
                }
            }
        }

        private Rainbow.Model.Message CreateRbMessage(FileUploadModel fileUploadModel, String toJid)
        {
            String fromJid = Helper.SdkWrapper.GetCurrentContactJid();
            String fromResource = Helper.SdkWrapper.GetResourceId();
            Restrictions.SDKMessageStorageMode messageStorageMode = Helper.SdkWrapper.GetMessageStorageMode();

            // Create Rainbow.Message
            Rainbow.Model.Message message = Rainbow.Model.Message.FromTextAndFileDescriptor(fromJid, fromResource, toJid, fileUploadModel.PeerType, "", fileUploadModel.Urgency, null, null, messageStorageMode, fileUploadModel.FileDescriptor);
            message.Content = "";

            return message;
        }

        private void CloseStream(FileUploadModel fileUploadModel)
        {
            if (fileUploadModel?.Stream != null)
            {
                try
                {
                    fileUploadModel.Stream.Close();
                    fileUploadModel.Stream.Dispose();
                    fileUploadModel.Stream = null;
                }
                catch
                { }
            }
        }

        private FilesUpload()
        {
            
        }

#endregion PRIVATE METHODS
    }
}
