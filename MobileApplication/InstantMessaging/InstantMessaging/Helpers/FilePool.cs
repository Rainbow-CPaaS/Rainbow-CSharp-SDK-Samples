using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using Rainbow;
using Rainbow.Model;
using Rainbow.Events;

using log4net;
using System.Threading;

namespace InstantMessaging.Helpers
{
    public sealed class FilePool
    {
        // Thread safe singleton: https://jlambert.developpez.com/tutoriels/dotnet/implementation-pattern-singleton-csharp/

        private static readonly FilePool instance = new FilePool();

        private static readonly ILog log = LogConfigurator.GetLogger(typeof(FilePool));

        private Rainbow.Application application = null;
        private Rainbow.FileStorage fileStorage = null;


        private Dictionary<String, String> filesDescriptorIdByConversation = new Dictionary<string, string>(); // FileDescriptorId / ConversationId
        private Dictionary<String, FileDescriptor> filesDescriptorById = new Dictionary<string, FileDescriptor>(); // FileDescriptorId / FileDescriptor

        private List<String> filesDescriptorToDownload = new List<string>();
        private BackgroundWorker backgroundWorkerFileDescriptotDownload = null;

#region PUBLIC METHODS   

        public static FilePool Instance
        {
            get
            {
                return instance;
            }
        }

        public void SetApplication(ref Application application)
        {
            this.application = application;
            this.fileStorage = application.GetFileStorage();
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

#endregion PUBLIC METHODS   


#region PRIVATE METHODS   

        private void UseFileDescriptorPool()
        {
            if(backgroundWorkerFileDescriptotDownload == null)
            {
                backgroundWorkerFileDescriptotDownload = new BackgroundWorker();
                backgroundWorkerFileDescriptotDownload.DoWork += BackgroundWorkerFileDescriptotDownload_DoWork;
                backgroundWorkerFileDescriptotDownload.RunWorkerCompleted += BackgroundWorkerFileDescriptotDownload_RunWorkerCompleted; ;
                backgroundWorkerFileDescriptotDownload.WorkerSupportsCancellation = true;
            }

            if(!backgroundWorkerFileDescriptotDownload.IsBusy)
                backgroundWorkerFileDescriptotDownload.RunWorkerAsync();
        }

        private void BackgroundWorkerFileDescriptotDownload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (filesDescriptorToDownload.Count > 0)
                UseFileDescriptorPool();
        }

        private void BackgroundWorkerFileDescriptotDownload_DoWork(object sender, DoWorkEventArgs e)
        {
            Boolean continueDownload;
            int index = 0;
            String fileDescriptorId;
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
                    }
                    else
                    {
                        // Download failed - we try for another FileDescriptor
                        index++;
                        log.DebugFormat("[BackgroundWorkerFileDescriptotDownload_DoWork] END - FAILED - fileDescriptorId:[{0}]", fileDescriptorId);
                    }

                }
                continueDownload = (filesDescriptorToDownload.Count > 0);
            }
            while (continueDownload);
        }

        private Boolean DownloadFileDescriptor(String fileDescriptorId)
        {
            return true;
            Boolean downloadResult = false;
            ManualResetEvent manualEvent = new ManualResetEvent(false);

            fileStorage.GetFileDescriptor(fileDescriptorId, callback =>
            {
                //Do we have a correct answer
                if (callback.Result.Success)
                {
                    downloadResult = true;
                    if (filesDescriptorById.ContainsKey(fileDescriptorId))
                        filesDescriptorById.Remove(fileDescriptorId);
                    filesDescriptorById.Add(fileDescriptorId, callback.Data);
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

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static FilePool()
        {
        }

        private FilePool()
        {

        }

#endregion PRIVATE METHODS   
    }
}
