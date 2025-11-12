using Microsoft.Extensions.Logging;
using NLog.LayoutRenderers;
using NLog.Targets;
using Rainbow;
using Rainbow.Model;
using Rainbow.SimpleJSON;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace BotRAG.Model
{
    public class BubbleElements
    {
        private readonly ILogger _log;

        private readonly static int DELAY_BETWEEN_STATUS_REPORT = 2000; // in ms
        private readonly static int NB_IM_BY_ROW = 100;

        private readonly Timer _sendProgressTimer;
        private Boolean _sendProgressTimerStarted;
        private Boolean _sendProgressNeeded;
        
        private readonly Elements<Message> _messages;
        private readonly Elements<FileDescriptor> _files;
        private readonly Elements<FileDescriptor> _recordings;
        private readonly SemaphoreSlim _semaphoreSlimRecordings;
        private readonly SemaphoreSlim _semaphoreSlimFiles;
        private readonly SemaphoreSlim _semaphoreSlimMessages;
        private readonly object _lock = new();
        private Task _taskManageElements = Task.CompletedTask;

        private CancellationTokenSource _cancellationTokenSource;

        private Boolean _started = false;
        private readonly Peer _bubblePeer;
        private readonly Dictionary<String, (Peer peerContact, String resource, Boolean usingIm)> _peerJidFollowingProgressStatus;

        private readonly Application _rbApplication;
        private readonly Contacts _rbContacts;
        private readonly FileStorage _rbFileStorage;
        private readonly InstantMessaging _rbInstantMessaging;

        private readonly Contact _currentContact;
        private String _previousStatus = "";
        private String _globalPreviousStatus = "";

        public BubbleElements(Application rbApplication, Peer bubblePeer)
        {
            this._rbApplication = rbApplication ?? throw new ArgumentNullException(nameof(rbApplication));
            this._bubblePeer = bubblePeer ?? throw new ArgumentNullException(nameof(bubblePeer));

            _log = LogFactory.CreateLogger<BubbleElements>(rbApplication.LoggerPrefix);
            _log.LogInformation("Created - Peer:[{Peer}]", _bubblePeer.Jid);

            _peerJidFollowingProgressStatus = [];

            _rbContacts = _rbApplication.GetContacts();
            _rbFileStorage = _rbApplication.GetFileStorage();
            _rbInstantMessaging = _rbApplication.GetInstantMessaging();

            _currentContact = _rbContacts.GetCurrentContact();

            _sendProgressTimer = new Timer(
                                _state => CheckProgressStatus(), null,
                                Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            _sendProgressTimerStarted = false;

            _semaphoreSlimMessages = new(0);
            _messages = new Elements<Message>(bubblePeer, "message", _semaphoreSlimMessages, GetMessageId, GetMessageDate);

            _semaphoreSlimFiles = new(0);
            _files = new Elements<FileDescriptor>(bubblePeer, "file", _semaphoreSlimFiles, GetFileId, GetFileDate);

            _semaphoreSlimRecordings = new(0);
            _recordings = new Elements<FileDescriptor>(bubblePeer, "recording", _semaphoreSlimRecordings, GetFileId, GetFileDate);

            StoreStatus();

            _cancellationTokenSource = new();

            // Set event we want to manage from SDK
            _rbInstantMessaging.MessageReceived += RbInstantMessaging_MessageReceived;
            _rbFileStorage.FileStorageUpdated += RbFileStorage_FileStorageUpdated;
            _rbFileStorage.RecordingFileUpdated += RbFileStorage_RecordingFileUpdated;
        }

        public Peer Peer => _bubblePeer;

        public async Task StartAsync()
        {
            if (!_taskManageElements.IsCompleted) return;
            _log.LogInformation("Started - Peer:[{Peer}]", _bubblePeer.Jid);

            if (_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
            }
            var token = _cancellationTokenSource.Token;

            var _1 = EnqueueRecordingsFromCacheAsync();
            var _2 = EnqueueFilesFromCacheAsync();
            var _3 = EnqueueMessagesFromCacheAsync(token);

            _taskManageElements = Task.Run(() => FetchElementsAsync(token));

            await Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            lock (_lock)
            {
                if (!_started) return;
                _log.LogInformation("Stop - Peer:[{Peer}]", _bubblePeer.Jid);
                _started = false;
            }
            _cancellationTokenSource.Cancel();

            await _taskManageElements;
        }

        /// <summary>
        /// To remove all elements related to this bubble from RAG IA 
        /// </summary>
        /// <returns></returns>
        public async Task<SdkResult<Boolean>> RemoveAllDataFromRAGAsync()
        {
            // TODO - remove all elements related to this bubble from RAG IA 
            await Task.CompletedTask;
            return new SdkResult<Boolean>(true);
        }

#region Progress Status

        public void FollowProgressStatus(Peer? peerContact, String ? resource, Boolean follow, Boolean usingIm)
        {
            var jid = peerContact?.Jid;
            if (String.IsNullOrEmpty(jid)) return;
            if (String.IsNullOrEmpty(resource)) return;

            if (!follow)
            {
                _peerJidFollowingProgressStatus.Remove(jid);
                return;
            }

            _peerJidFollowingProgressStatus[jid] = (peerContact, resource, usingIm);

            var _ = SendProgressStatusAsync(jid);
            StartToSendProgressStatus();
        }

        private void StartToSendProgressStatus()
        {
            if (_sendProgressTimerStarted) return;
            _sendProgressTimerStarted = true;

            _sendProgressTimer?.Change(TimeSpan.FromMilliseconds(DELAY_BETWEEN_STATUS_REPORT), TimeSpan.FromMilliseconds(DELAY_BETWEEN_STATUS_REPORT));
        }

        private void StopToSendProgressStatus()
        {
            if (!_sendProgressTimerStarted) return;
            _sendProgressTimerStarted = false;

            _sendProgressTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        private async Task SendProgressStatusAsync(String jid)
        {
            if (String.IsNullOrEmpty(_previousStatus))
                return;
            var message = "{\"status\": " + _previousStatus + "}";
            await SendProgressStatusAsync(jid, message);
        }

        private async Task SendProgressStatusAsync(String jid, String message)
        {
            if (_peerJidFollowingProgressStatus.TryGetValue(jid, out var tuple))
            {
                var peerContact = tuple.peerContact;
                var resource = tuple.resource;
                var usingIm = tuple.usingIm;

                if (usingIm)
                {
                    await _rbApplication.GetInstantMessaging().SendMessageAsync(Peer, message);
                }
                else
                {
                    await _rbApplication.GetInstantMessaging().SendPrivateMessageAsync(peerContact, resource, Peer, message);
                }
            }
        }

        private async Task SendProgressStatusToAllAsync()
        {
            if (_peerJidFollowingProgressStatus.Count == 0) return;

            var message = "{\"status\": " + _previousStatus + "}";

            if (!message.Equals(_globalPreviousStatus))
            {
                _globalPreviousStatus = message;
                foreach (var jid in _peerJidFollowingProgressStatus.Keys)
                    await SendProgressStatusAsync(jid, message);
            }
        }

        private Boolean NeedProgressStatus()
        {
            return _sendProgressNeeded;
        }

        private void CheckProgressStatus()
        {
            // TODO - need to check if need to continue to send progress status
            if (NeedProgressStatus())
            {
                var _ = SendProgressStatusToAllAsync();
            }
            else
                StopToSendProgressStatus();
        }

    #endregion Progress Status

    #region Fetch Elements - Recordings, Files, Messages

        private async Task EnqueueRecordingsFromCacheAsync()
        {
            if (_recordings.CacheAlreadyFetched is false)
            {
                var sdkResult = await _rbFileStorage.GetFileDescriptorsAsync(_bubblePeer, true);
                if (sdkResult.Success)
                {
                    if (!_recordings.Enqueue(sdkResult.Data, false))
                        StoreStatus();
                }

                _log.LogInformation("Cache - Total Recordings:[{Nb}] - Peer:[{Peer}]", sdkResult.Data.Count, _bubblePeer.Jid);

                _recordings.CacheAlreadyFetched = true;
            }

            await Task.CompletedTask;
        }

        private async Task EnqueueFilesFromCacheAsync()
        {
            if (_files.CacheAlreadyFetched is false)
            {
                var sdkResult = await _rbFileStorage.GetFileDescriptorsAsync(_bubblePeer, false);
                if (sdkResult.Success)
                {
                    if (!_files.Enqueue(sdkResult.Data, false))
                        StoreStatus();
                }

                _log.LogInformation("Cache - Total Files:[{Nb}] - Peer:[{Peer}]", sdkResult.Data.Count, _bubblePeer.Jid);

                _files.CacheAlreadyFetched = true;
            }

            await Task.CompletedTask;
        }

        private async Task EnqueueMessagesFromCacheAsync(CancellationToken token)
        {
            if (_messages.CacheAlreadyFetched is false)
            {
                Boolean error = false;
                try
                {
                    SdkResult<List<Rainbow.Model.Message>> mamSdkResult;
                    Boolean canContinue = true;

                    int messagesCount = 0;
                    while (canContinue && !token.IsCancellationRequested)
                    {
                        mamSdkResult = await _rbInstantMessaging.GetOlderMessagesAsync(_bubblePeer, NB_IM_BY_ROW);
                        if (mamSdkResult.Success)
                        {
                            if (mamSdkResult.Data.Count > 0)
                            {
                                // We avoid any message sent by the bot itself
                                var messages = mamSdkResult.Data.Where(m => m.FromContact.Peer.Id != _currentContact.Peer.Id).ToList();

                                if (!_messages.Enqueue(messages, false))
                                    StoreStatus();

                                //_log.LogInformation("Cache - Add Messages:[{Nb}] - Peer:[{Peer}]", mamSdkResult.Data.Count, _bubblePeer.Jid);
                                messagesCount += messages.Count;
                            }
                            else
                                canContinue = false;
                        }
                        else
                        {
                            canContinue = false;
                            error = true;
                        }
                    }

                    _log.LogInformation("Cache - Total Messages:[{Nb}] - Peer:[{Peer}]", messagesCount, _bubblePeer.Jid);

                }
                catch (Exception e)
                {
                    error = true;
                }

                if (error is false)
                    _messages.CacheAlreadyFetched = true;
            }

            await Task.CompletedTask;
        }

        public async Task FetchElementsAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var taskRecordings = FetchRecordingsAsync(token);
                    var taskFiless = FetchFilesAsync(token);
                    var taskMessages = FetchMessagesAsync(token);

                    await Task.WhenAll([taskRecordings, taskFiless, taskMessages]);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        public async Task FetchRecordingsAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var taskResult = await FetchRecordingAsync();
                    _log.LogDebug("Semaphore - BubbleID:[{BubbleId}] - TaskRecordings:[{TaskResult}] - Count:[{Count}]", _bubblePeer.Id, taskResult, _semaphoreSlimRecordings.CurrentCount);
                    // Wait until an element is available
                    await _semaphoreSlimRecordings.WaitAsync(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task<String?> FetchRecordingAsync()
        {
            var fileDescriptor = _recordings.Dequeue();
            if (fileDescriptor is null)
                return null; // No more element

            String status;
            String? ragDocumentId = null;
            // Check numbers of max recording to manage
            if ((Configuration.Instance.MaxRecordingsByBubble != -1) && (_recordings.NbElementsProcessed >= Configuration.Instance.MaxRecordingsByBubble))
            {
                //_log.LogWarning("Recording not managed - Too many already managed - FileId:[{Id}] - Peer:[{Peer}]", fileDescriptor.Id, _bubblePeer.Jid);
                status = "skipped";
            }
            else
            {
                (status, ragDocumentId) = await ManageFileDescriptorAsync(_recordings, fileDescriptor);
            }
            _recordings.ElementHasBeenManaged(fileDescriptor, status, ragDocumentId);
            StoreStatus();
            return status;
        }

        public async Task FetchFilesAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var taskResult = await FetchFileAsync();
                    _log.LogDebug("Semaphore - BubbleID:[{BubbleId}] - TaskFiles:[{TaskResult}] - Count:[{Count}]", _bubblePeer.Id, taskResult, _semaphoreSlimFiles.CurrentCount);
                    // Wait until an element is available
                    await _semaphoreSlimFiles.WaitAsync(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task<String?> FetchFileAsync()
        {
            var fileDescriptor = _files.Dequeue();
            if (fileDescriptor is null) 
                return null; // No more element

            String status;
            String? ragDocumentId = null;
            // Check numbers of max file to manage
            if ((Configuration.Instance.MaxFilesByBubble!= -1) && (_files.NbElementsProcessed >= Configuration.Instance.MaxFilesByBubble))
            {
                //_log.LogWarning("File not managed - Too many already managed - FileId:[{Id}] - Peer:[{Peer}]", fileDescriptor.Id, _bubblePeer.Jid);
                status = "skipped";
            }
            else
            {
                (status, ragDocumentId) = await ManageFileDescriptorAsync(_files, fileDescriptor);
            }
            _files.ElementHasBeenManaged(fileDescriptor, status, ragDocumentId);
            StoreStatus();
            return status;
        }

        public async Task FetchMessagesAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var taskResult = await FetchMessageAsync();
                    _log.LogDebug("Semaphore - BubbleID:[{BubbleId}] - TaskMessages:[{TaskResult}] - Count:[{Count}]", _bubblePeer.Id, taskResult, _semaphoreSlimMessages.CurrentCount);
                    // Wait until an element is available
                    await _semaphoreSlimMessages.WaitAsync(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task<String?> FetchMessageAsync()
        {
            var message = _messages.Dequeue();
            if (message is null) 
                return null; // No more element

            String status;
            String? ragDocumentId = null;
            // Check numbers of max recording to manage
            if ((Configuration.Instance.MaxMessagesByBubble != -1) && (_messages.NbElementsProcessed >= Configuration.Instance.MaxMessagesByBubble))
            {
                status = "skipped";
            }
            else
            {
                (status, ragDocumentId) = await ManageMessageAsync(_messages, message);
            }
            _messages.ElementHasBeenManaged(message, status, ragDocumentId);
            StoreStatus();
            return status;
        }

        public Boolean IsFileManaged(FileDescriptor fileDescriptor)
        {
            if (fileDescriptor is null) return false;

            if (Configuration.Instance.AvoidAudioFile && RAGClient.IsAudioFile(fileDescriptor.TypeMIME))
                return false;

            if (Configuration.Instance.AvoidVideoFile && RAGClient.IsVideoFile(fileDescriptor.TypeMIME))
                return false;

            if (Configuration.Instance.AvoidImageFile && RAGClient.IsImageFile(fileDescriptor.TypeMIME))
                return false;

            if ((Configuration.Instance.AvoidArchiveFile) && RAGClient.IsArchivedFile(fileDescriptor.FileName))
                return false;

            if ((Configuration.Instance.MaxFileSizeInBytes != -1) && (fileDescriptor.Size > Configuration.Instance.MaxFileSizeInBytes))
                return false;

            return true;
        }

        private void StoreStatus()
        {
            lock (_lock)
            {
                var fileStatus = _files.GetStatus();
                var recordingStatus = _recordings.GetStatus();
                var messageStatus = _messages.GetStatus();

                Dictionary<String, Object> status = new()
                {
                    { "recordings", recordingStatus },
                    { "files", fileStatus },
                    { "messages", messageStatus },
                };

                var json = JSON.ToJson(status, indent: true);

                Boolean updated = false;
                // TODO - Need to know when to stop to send progress status ... _sendProgressNeeded must be set to false
                if (!json.Equals(_previousStatus))
                {
                    _previousStatus = json;
                    _sendProgressNeeded = true;
                    StartToSendProgressStatus();
                    updated = true;
                }

                _log.LogDebug("Progress status saved -  BubbleId:[{BubbleId}] - Updated:[{updated}] - Status:[{status}]", updated, _bubblePeer.Id, json);

                try
                {
                    Directory.CreateDirectory($"./bubbles/{_bubblePeer.Id}");

                    File.WriteAllText($"./bubbles/{_bubblePeer.Id}/status.json", json);
                }
                catch
                {

                }
            }
        }

        private async Task<(String status, String? ragDocumentId)> ManageFileDescriptorAsync(Elements<FileDescriptor> _elements, FileDescriptor fileDescriptor)
        {
            String status;
            String? ragDocumentId = null;
            if (!IsFileManaged(fileDescriptor))
            {
                status = "skipped";
            }
            else
            { 
                var sdkResult = await _rbFileStorage.DownloadFileAsync(fileDescriptor, $"./bubbles/{_bubblePeer.Id}", fileDescriptor.FileName);
                if(sdkResult.Success)
                {
                    _elements.ElementHasBeenManaged(fileDescriptor, "downloaded", null);
                    StoreStatus();

                    _log.LogInformation("File uploaded - FileId:[{Id}] - Peer:[{Peer}]", fileDescriptor.Id, _bubblePeer.Jid);

                    string filePath = Path.Combine($".\\bubbles\\{_bubblePeer.Id}", fileDescriptor.FileName);
                    (Boolean result, ragDocumentId) = await StoreFileOnRAGAsync(fileDescriptor, filePath);
                    if(result)
                        status = "processed";
                    
                    else
                        status = "ragError";

                    // Once provided to the RAG, delete the file
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch
                    {
                        _log.LogWarning("File uploaded cannot be deleted - FileId:[{Id}] - Peer:[{Peer}]", fileDescriptor.Id, _bubblePeer.Jid);
                    }
                }
                else
                {
                    // TODO - manage dwl error
                    status = "ragError";
                }
            }
            return (status, ragDocumentId);
        }

        private async Task<(String status, String? ragDocumentId)> ManageMessageAsync(Elements<Message> _elements, Message message)
        {
            String status;
            String? ragDocumentId = null;
            if (String.IsNullOrWhiteSpace(message.BubbleEvent))
            {
                (Boolean result, ragDocumentId) = await StoreMessageOnRAGAsync(message);
                if (result)
                    status = "processed";
                else
                    status = "ragError";
            }
            else
            {
                status = "skipped";
            }
            return (status, ragDocumentId);
        }

        private async Task<(Boolean result, String? ragDocumentId)> StoreMessageOnRAGAsync(Message message)
        {
            return await RAGClient.Instance.UploadMessageThenProcessAsync(_bubblePeer, message);
        }

        private async Task<(Boolean result, String? ragDocumentId)> StoreFileOnRAGAsync(FileDescriptor fileDescriptor, String filePath)
        {
            var owner = await _rbContacts.GetContactByIdInCacheFirstAsync(fileDescriptor.OwnerId);
            if (owner is null)
            {
                _log.LogWarning("File NOT stored on RAG - FileId:[{Id}] - Peer:[{Peer}] - Cannot get Owner", fileDescriptor.Id, _bubblePeer.Jid);
                return (false, null);
            }

            return await RAGClient.Instance.UploadFileThenProcessAsync(_bubblePeer, owner, fileDescriptor, filePath);
        }

#endregion Fetch Elements - Recordings, Files, Messages

#region Events from SDK

        private void RbFileStorage_RecordingFileUpdated(Peer peer, string status, List<FileDescriptor> filesDescriptors)
        {
            if (peer?.Jid != _bubblePeer.Jid) return;

            switch (status)
            {
                case "added":
                    if(!_recordings.Enqueue(filesDescriptors, true))
                        StoreStatus();
                    break;

                case "deleted":
                    // TODO - manage recording deleted
                    break;

                case "updated":
                    // TODO - manage recording updated
                    break;
            }
        }

        private void RbFileStorage_FileStorageUpdated(List<FileDescriptor> filesDescriptors, string status)
        {
            switch(status)
            {
                case "create":
                case "update":
                    if (!_files.Enqueue(filesDescriptors, true))
                        StoreStatus();
                    break;

                case "delete":
                    // TODO - manage file deleted
                    break;

                //case "updated":
                //    // TODO - manage file deleted
                //    break;
            }
        }

        private void RbInstantMessaging_MessageReceived(Message message, bool carbonCopy)
        {
            if(String.IsNullOrEmpty(message.Content))
                return;

            if (message.ToBubble?.Peer?.Jid == _bubblePeer.Jid)
            {
                // TODO - manage message deleted or modified
                if (message.Deleted || message.Modified)
                    return;

                // We avoid any message sent by the bot itself
                if (message.FromContact.Peer.Id != _currentContact.Peer.Id)
                {
                    if (!_messages.Enqueue(message, highPriority: true))
                        StoreStatus();
                }
            }
        }

#endregion Events from SDK

#region Static methods to get Id/Date of Message/FileDescriptor

        static String GetMessageId(Message message) => message?.Id ?? String.Empty;
        static DateTime GetMessageDate(Message message) => message?.Date ?? DateTime.MinValue;

        static String GetFileId(FileDescriptor file) => file?.Id ?? String.Empty;
        static DateTime GetFileDate(FileDescriptor file) => file?.UploadedDate ?? DateTime.MinValue;

#endregion Static methods to get Id/Date of Message/FileDescriptor

    }


}
