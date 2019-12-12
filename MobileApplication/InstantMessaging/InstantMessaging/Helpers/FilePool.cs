using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;

using Rainbow;
using Rainbow.Model;
using Rainbow.Events;

using log4net;
using System.Threading.Tasks;

namespace Rainbow.Helpers
{
    public sealed class FilePool
    {
        // Thread safe singleton: https://jlambert.developpez.com/tutoriels/dotnet/implementation-pattern-singleton-csharp/

        private static readonly FilePool instance = new FilePool();

        private static readonly ILog log = LogConfigurator.GetLogger(typeof(FilePool));

        private AvatarPool avatarPool;

        private Rainbow.Application application = null;
        private Rainbow.FileStorage fileStorage = null;


        private Dictionary<String, String> filesDescriptorIdByConversation = new Dictionary<string, string>(); // FileDescriptorId / ConversationId
        private Dictionary<String, String> filesDescriptorNameById = new Dictionary<string, String>(); // FileDescriptorId / file name
        private Dictionary<String, FileDescriptor> filesDescriptorById = new Dictionary<string, FileDescriptor>(); // FileDescriptorId / FileDescriptor

        private List<String> filesDescriptorToDownload = new List<string>();
        private BackgroundWorker backgroundWorkerFileDescriptorDownload = null;

        private List<String> thumbnailDownloaded = new List<string>();
        private List<String> thumbnailDirectToDownload = new List<string>();

        private List<String> thumbnailToDownload = new List<string>();
        private List<String> thumbnailDownloading = new List<string>();

        private BackgroundWorker backgroundWorkerThumbnailDownload = null;

        private Boolean initDone = false;
        private String folderPath = null;

        private String folderPathThumbnails = null;
        private String folderPathFiles = null;

#region PUBLIC EVENTS
        /// <summary>
        /// The event that is raised when a thumbnail has been downloaded successfully and is available
        /// </summary>
        public event EventHandler<IdEventArgs> ThumbnailAvailable;

        /// <summary>
        /// The event that is raised when a FileDescriptor is available
        /// </summary>
        public event EventHandler<IdEventArgs> FileDescriptorAvailable;

#endregion


#region PUBLIC METHODS   

        public static FilePool Instance
        {
            get
            {
                return instance;
            }
        }

        public Boolean AllowAutomaticThumbnailDownload { get; set; }

        public int AutomaticDownloadLimitSizeForImages { get; set; }

        public int MaxThumbnailWidth { get; set; }

        public int MaxThumbnailHeight { get; set; }

        public Boolean IsThumbnailFileAvailable(String conversationId, String fileDescriptorId, String fileName)
        {
            bool result = false;
            if(thumbnailDownloaded.Contains(fileDescriptorId))
                result = true;
            else
            {
                string path;
                String fileNameOnDisk = fileDescriptorId + Path.GetExtension(fileName);

                // We check on file system if already in cache - Thumbnails folder first
                path = Path.Combine(folderPathThumbnails, fileNameOnDisk);
                if (File.Exists(path))
                {
                    result = true;
                    log.DebugFormat("[IsThumbnailFileAvailable] File already downloaded in Thumbnails part - FileId:[{0}]", fileDescriptorId);
                }
                else 
                {
                    // We check on file system if already in cache - Files folder now
                    path = Path.Combine(folderPathFiles, fileNameOnDisk);
                    if (File.Exists(path))
                    {
                        result = true;
                        thumbnailDirectToDownload.Add(fileDescriptorId);
                        log.DebugFormat("[IsThumbnailFileAvailable] File already downloaded in Files part - FileId:[{0}]", fileDescriptorId);
                    }
                }

                // If we have found it, we need to ask FileDescriptor but set is has already downloaded
                if (result)
                {
                    if (filesDescriptorNameById.ContainsKey(fileDescriptorId))
                        filesDescriptorNameById.Remove(fileDescriptorId);
                    filesDescriptorNameById.Add(fileDescriptorId, fileNameOnDisk);

                    thumbnailDownloaded.Add(fileDescriptorId);
                    AskFileDescriptorDownload(conversationId, fileDescriptorId);
                }
            }
            return result;
        }

        public String GetThumbnailFileName(String fileDescriptorId)
        {
            if (filesDescriptorNameById.ContainsKey(fileDescriptorId))
                return filesDescriptorNameById[fileDescriptorId];
            return null;
        }

        public String GetThumbnailFullFilePath(String fileDescriptorId)
        {
            if (thumbnailDownloaded.Contains(fileDescriptorId))
            {
                String fileName = GetThumbnailFileName(fileDescriptorId);
                if (fileName != null)
                {
                    if (thumbnailDirectToDownload.Contains(fileDescriptorId))
                        return Path.Combine(this.folderPathFiles, fileName);
                    else
                        return Path.Combine(this.folderPathThumbnails, fileName);
                }
            }
            return null;
        }

        public void SetFolderPath(String folderPath)
        {
            this.folderPath = folderPath;

            try
            {
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                folderPathFiles = Path.Combine(folderPath, "Files");
                if (!Directory.Exists(folderPathFiles))
                    Directory.CreateDirectory(folderPathFiles);

                folderPathThumbnails = Path.Combine(folderPath, "Thumbnails");
                if (!Directory.Exists(folderPathThumbnails))
                    Directory.CreateDirectory(folderPathThumbnails);

                log.WarnFormat("[SetFolderPath] FileStorage - folderPath:[{0}] - Files:[{1}] - Thumbnails:[{2}]", folderPath, folderPathFiles, folderPathThumbnails);
            }
            catch (Exception exc)
            {
                log.WarnFormat("[SetFolderPath] Impossible to create path to store file thumbnail :\r\n{0}", Util.SerializeException(exc));
            }
        }

        public void SetApplication(ref Application application)
        {
            this.application = application;
            fileStorage = application.GetFileStorage();
            

            application.ConnectionStateChanged += Application_ConnectionStateChanged;

            fileStorage.FileDownloadUpdated += FileStorage_FileDownloadUpdated;
        }

        public void AskFileDescriptorDownload(String conversationId, String fileDescriptorId)
        {
            // Does this File Descriptor not already get ?
            if (!filesDescriptorById.ContainsKey(fileDescriptorId))
            {
                // Add (if necessary) in the list of FileDescriptor to download
                if (!filesDescriptorToDownload.Contains(fileDescriptorId))
                    filesDescriptorToDownload.Add(fileDescriptorId);

                // Add (if necessary) to link FileDescriptor and Conversation
                if (!filesDescriptorIdByConversation.ContainsKey(fileDescriptorId))
                    filesDescriptorIdByConversation.Add(fileDescriptorId, conversationId);

                UseFileDescriptorPool();
            }
        }

        public String GetConversationIdForFileDescriptorId(String fileDescriptorId)
        {
            if (filesDescriptorIdByConversation.ContainsKey(fileDescriptorId))
                return filesDescriptorIdByConversation[fileDescriptorId];
            return null;
        }

        public void AskThumbnailDownload(String fileDescriptorId)
        {
            // Does this File Descriptor not already get ?
            if ((!thumbnailDownloaded.Contains(fileDescriptorId)) 
                && (!thumbnailToDownload.Contains(fileDescriptorId)))
            {
                FileDescriptor fileDescriptor = filesDescriptorById[fileDescriptorId];

                if (fileDescriptor != null)
                {
                    if(IsImage(fileDescriptor.Name) && (fileDescriptor.Size <= AutomaticDownloadLimitSizeForImages))
                    {
                        thumbnailToDownload.Add(fileDescriptorId);
                        thumbnailDirectToDownload.Add(fileDescriptorId);
                    }
                    else if(fileDescriptor.ThumbnailAvailable)
                        thumbnailToDownload.Add(fileDescriptorId);
                }
            }
        }

        public Boolean IsImage(String fileName)
        {
            String ext = Path.GetExtension(fileName).ToLower();
            return (ext == ".png") || (ext == ".jpg") || (ext == ".jpeg") || (ext == ".bmp") || (ext == ".gif");
        }

#endregion PUBLIC METHODS   


#region EVENT FIRED BY SDK

        private void Application_ConnectionStateChanged(object sender, ConnectionStateEventArgs e)
        {
            if (!application.IsInitialized())
                initDone = false;
        }

        
        private void FileStorage_FileDownloadUpdated(object sender, FileDownloadEventArgs e)
        {
            if(thumbnailDownloading.Contains(e.FileId))
            {
                if(!e.InProgress)
                {
                    if (e.Completed)
                    {
                        //thumbnailDirectToDownload.Remove(e.FileId);

                        if (!thumbnailDownloaded.Contains(e.FileId))
                            thumbnailDownloaded.Add(e.FileId);

                        log.DebugFormat("[FileStorage_FileDownloadUpdated] Download done - fileDescriptorId:[{0}]", e.FileId);

                        // Need to scale thumbnail according density and max size expected
                        double density = avatarPool.GetDensity();
                        double maxWidth = MaxThumbnailWidth * density;
                        double maxHeight = MaxThumbnailHeight * density;

                        String filePath = GetThumbnailFullFilePath(e.FileId);
                        try
                        {
                            using (Stream stream = new MemoryStream(File.ReadAllBytes(filePath)))
                            {
                                System.Drawing.Size size = avatarPool.GetSize(stream);
                                
                                double width = size.Width * density;
                                double height = size.Height * density;

                                double scaleWidth = density;
                                double scaleHeight = density;
                                double scaleUsed;

                                if (width > maxWidth)
                                    scaleWidth = maxWidth / size.Width;

                                if (height > maxHeight)
                                    scaleHeight = maxHeight / size.Height;

                                if (scaleWidth > scaleHeight)
                                    scaleUsed = scaleHeight;
                                else
                                    scaleUsed = scaleWidth;

                                // Scale thumbnail
                                int widthUsed = (int)Math.Floor(size.Width * scaleUsed);
                                int heightUsed = (int)Math.Floor(size.Height * scaleUsed);

                                stream.Seek(0, SeekOrigin.Begin);

                                Stream resultStream = avatarPool.GetScaled(stream, widthUsed, heightUsed);

                                // Delete previous file (if any)
                                if (File.Exists(filePath))
                                    File.Delete(filePath);

                                // Save new image
                                using (Stream file = File.Create(filePath))
                                    resultStream.CopyTo(file);

                                resultStream.Close();

                                log.DebugFormat("[FileStorage_FileDownloadUpdated] scale thumbnail according density - fileDescriptorId:[{0}] - density:[{1}]", e.FileId, density);
                            }
                        }
                        catch { 

                        }

                        
                        Task task = new Task(() =>
                        {
                            ThumbnailAvailable.Invoke(this, new IdEventArgs(e.FileId));
                        });
                        task.Start();
                    }
                }
            }
        }

#endregion EVENT FIRED BY SDK

#region PRIVATE METHODS   

        private Boolean InitDone()
        {
            if (!initDone)
            {
                if(String.IsNullOrEmpty(folderPath))
                    return false;

                if (!application.IsConnected())
                    return false;

                initDone = true;
            }
            
            return initDone;
        }

    #region FileDescriptor pool

        private void UseFileDescriptorPool()
        {
            if (!InitDone())
                return;

            if (!AllowAutomaticThumbnailDownload)
                return;

            if (backgroundWorkerFileDescriptorDownload == null)
            {
                backgroundWorkerFileDescriptorDownload = new BackgroundWorker();
                backgroundWorkerFileDescriptorDownload.DoWork += BackgroundWorkerFileDescriptotDownload_DoWork;
                backgroundWorkerFileDescriptorDownload.RunWorkerCompleted += BackgroundWorkerFileDescriptotDownload_RunWorkerCompleted; ;
                backgroundWorkerFileDescriptorDownload.WorkerSupportsCancellation = true;
            }

            if (!backgroundWorkerFileDescriptorDownload.IsBusy)
                backgroundWorkerFileDescriptorDownload.RunWorkerAsync();
        }

        private void BackgroundWorkerFileDescriptotDownload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (InitDone())
            {
                if (filesDescriptorToDownload.Count > 0)
                    UseFileDescriptorPool();
                else
                    UseThumbnailPool();
            }
        }

        private void BackgroundWorkerFileDescriptotDownload_DoWork(object sender, DoWorkEventArgs e)
        {
            Boolean continueDownload;
            int index = 0;
            String fileDescriptorId;

            if (!InitDone())
                return;

            do
            {
                if (filesDescriptorToDownload.Count > 0)
                {
                    // Check index
                    if (index >= filesDescriptorToDownload.Count)
                        index = 0;

                    fileDescriptorId = filesDescriptorToDownload[index];
                    if (DownloadFileDescriptor(fileDescriptorId))
                    {
                        filesDescriptorToDownload.Remove(fileDescriptorId);
                        log.DebugFormat("[BackgroundWorkerFileDescriptotDownload_DoWork] END - SUCCESS - fileDescriptorId:[{0}]", fileDescriptorId);

                        Task task = new Task(() =>
                        {
                            FileDescriptorAvailable.Invoke(this, new IdEventArgs(fileDescriptorId));
                        });
                        task.Start();
                    }
                    else
                    {
                        // Download failed - we try for another FileDescriptor
                        index++;
                        log.DebugFormat("[BackgroundWorkerFileDescriptotDownload_DoWork] END - FAILED - fileDescriptorId:[{0}]", fileDescriptorId);

                        // FOR TEST PURPOSE
                        filesDescriptorToDownload.Remove(fileDescriptorId);
                    }

                }
                continueDownload = (filesDescriptorToDownload.Count > 0) && InitDone();
            }
            while (continueDownload);
        }

        private Boolean DownloadFileDescriptor(String fileDescriptorId)
        {
            Boolean downloadResult = false;
            ManualResetEvent manualEvent = new ManualResetEvent(false);

            fileStorage.GetFileDescriptor(fileDescriptorId, callback =>
            {
                //Do we have a correct answer
                if (callback.Result.Success)
                {
                    downloadResult = true;
                    FileDescriptor fileDescriptor = callback.Data;

                    log.DebugFormat("[DownloadFileDescriptor] fileDescriptor:[{0}]", fileDescriptor.ToString());

                    if (filesDescriptorById.ContainsKey(fileDescriptorId))
                        filesDescriptorById.Remove(fileDescriptorId);
                    filesDescriptorById.Add(fileDescriptorId, fileDescriptor);

                    if (filesDescriptorNameById.ContainsKey(fileDescriptorId))
                        filesDescriptorNameById.Remove(fileDescriptorId);
                    filesDescriptorNameById.Add(fileDescriptorId, fileDescriptorId + Path.GetExtension(fileDescriptor.Name));

                    AskThumbnailDownload(fileDescriptorId);
                }
                else
                {
                    log.WarnFormat("[DownloadFileDescriptor] Not possible to get file descriptor:[{0}]", Util.SerialiseSdkError(callback.Result));
                }

                manualEvent.Set();
            });

            manualEvent.WaitOne();
            manualEvent.Dispose();

            return downloadResult;
        }

    #endregion FileDescriptor pool

    #region Thumbnail pool

        private void UseThumbnailPool()
        {
            if (!InitDone())
                return;

            if (filesDescriptorToDownload.Count != 0)
                return;

            if(backgroundWorkerThumbnailDownload == null)
            {
                backgroundWorkerThumbnailDownload = new BackgroundWorker();
                backgroundWorkerThumbnailDownload.DoWork += BackgroundWorkerThumbnailDownload_DoWork; ;
                backgroundWorkerThumbnailDownload.RunWorkerCompleted += BackgroundWorkerThumbnailDownload_RunWorkerCompleted;
                backgroundWorkerThumbnailDownload.WorkerSupportsCancellation = true;
            }

            if(!backgroundWorkerThumbnailDownload.IsBusy)
                backgroundWorkerThumbnailDownload.RunWorkerAsync();
        }
        
        private void BackgroundWorkerThumbnailDownload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (InitDone())
            {
                if ((thumbnailToDownload.Count > 0) && AllowAutomaticThumbnailDownload && (filesDescriptorToDownload.Count == 0))
                        UseThumbnailPool();
            }
        }

        private void BackgroundWorkerThumbnailDownload_DoWork(object sender, DoWorkEventArgs e)
        {
            Boolean continueDownload;
            int index = 0;
            String fileDescriptorId;

            if (!InitDone())
                return;

            if (!AllowAutomaticThumbnailDownload)
                return;

            if (filesDescriptorToDownload.Count != 0)
                return;

            do
            {
                if (thumbnailToDownload.Count > 0)
                {
                    // Check index
                    if (index >= thumbnailToDownload.Count)
                        index = 0;

                    fileDescriptorId = thumbnailToDownload[index];
                    if (DownloadThumbnail(fileDescriptorId))
                    {
                        thumbnailToDownload.Remove(fileDescriptorId);
                        log.DebugFormat("[BackgroundWorkerThumbnailDownload_DoWork] SUCCESS - fileDescriptorId:[{0}]", fileDescriptorId);
                    }
                    else
                    {
                        // Download failed - we try for another FileDescriptor
                        index++;
                        log.DebugFormat("[BackgroundWorkerThumbnailDownload_DoWork] FAILED - fileDescriptorId:[{0}]", fileDescriptorId);

                        // FOR TEST PURPOSE
                        thumbnailToDownload.Remove(fileDescriptorId);
                    }

                }
                continueDownload = InitDone() && (thumbnailToDownload.Count > 0) && AllowAutomaticThumbnailDownload && (filesDescriptorToDownload.Count == 0);
            }
            while (continueDownload);
        }

        private Boolean DownloadThumbnail(String fileDescriptorId)
        {
            Boolean downloadResult = false;
            ManualResetEvent manualEvent = new ManualResetEvent(false);

            String filename = GetThumbnailFileName(fileDescriptorId);

            if (filename == null)
            {
                log.WarnFormat("[DownloadThumbnail] Not possible to get filename associated to this thumbnail - fileDescriptorId:[{0}]", fileDescriptorId);
                return false;
            }

            if (thumbnailDownloading.Contains(fileDescriptorId))
                return false;

            thumbnailDownloading.Add(fileDescriptorId);

            if (thumbnailDirectToDownload.Contains(fileDescriptorId))
            {
                fileStorage.DownloadFile(fileDescriptorId, this.folderPathFiles, filename, callback =>
                {
                    downloadResult = callback.Result.Success;
                    if(!callback.Result.Success)
                        log.WarnFormat("[DownloadThumbnail] Not possible to get direct file download:[{0}]", Util.SerialiseSdkError(callback.Result));
                    manualEvent.Set();
                });
            }
            else
            {
                fileStorage.DownloadThumbnailFile(fileDescriptorId, this.folderPathThumbnails, filename, callback =>
                {
                    downloadResult = callback.Result.Success;
                    if (!callback.Result.Success)
                        log.WarnFormat("[DownloadThumbnail] Not possible to get thumbnail:[{0}]", Util.SerialiseSdkError(callback.Result));
                    manualEvent.Set();
                });
            }

            manualEvent.WaitOne();
            manualEvent.Dispose();

            if (!downloadResult)
                thumbnailDownloading.Remove(fileDescriptorId);

            return downloadResult;
        }
        
    #endregion Thumbnail pool

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static FilePool()
        {
        }

        private FilePool()
        {
            avatarPool = AvatarPool.Instance;

            MaxThumbnailWidth = 400;
            MaxThumbnailHeight = 400;
        }

#endregion PRIVATE METHODS   
    }
}
