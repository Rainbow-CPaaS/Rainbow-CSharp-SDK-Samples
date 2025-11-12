using Microsoft.Extensions.Logging;
using Rainbow;
using Rainbow.Model;
using Rainbow.SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotRAG
{
    public class RAGClient
    {
        private static RAGClient? _instance = null;
        private ILogger _log;

        private Application? _rbApplication;
        private String _bearerToken = String.Empty;
        
        private System.Net.Http.HttpClient? _httpClient = null;
        private CancellationTokenSource _tokenSource;

        private readonly Object _lock = new();
        private readonly Dictionary<String, String> _rabConversationByBubbleJid = [];

        /// <summary>
        /// Get instance of <see cref="RAGClient"/> service
        /// </summary>
        public static RAGClient Instance
        {
            get
            {
                _instance ??= new();
                return _instance;
            }
        }

        private RAGClient()
        {
            _tokenSource = new CancellationTokenSource();
            _tokenSource.CancelAfter(TimeSpan.FromMilliseconds(Timeout.Infinite));
        }

        public void SetApplication(Application rbApplication)
        {
            if (_rbApplication is not null) return;

            _rbApplication = rbApplication;

            _log = LogFactory.CreateLogger<RAGClient>(rbApplication.LoggerPrefix);
            _log.LogInformation("Application set");
        }

        public async Task<Boolean> ConnectAsync()
        {
            // Actually connection and reconnection requests are the same ...
            var result = await RenewTokenAsync();
            return result;
        }

        public async Task<Boolean> CheckTokenAsync()
        {
            if (String.IsNullOrEmpty(_bearerToken)) return false;

            var uri = $"https://{Configuration.Instance.Host}/api/v1/auth/user_token";
            using System.Net.Http.HttpClient client = new();

            // Bearer Token header if needed
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _bearerToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using HttpResponseMessage response = await client.GetAsync(uri);
            //var token = await response.Content.ReadAsStringAsync();
            return (response.StatusCode != System.Net.HttpStatusCode.OK);
        }

        public async Task<Boolean> RenewTokenAsync()
        {
            var result = await CheckTokenAsync();
            if(result)
                return true;

            Dictionary<string, string> dict = new()
            {
                { "username", Configuration.Instance.Login},
                { "password", Configuration.Instance.Password }
            };

            var uri = $"https://{Configuration.Instance.Host}/token";

            using System.Net.Http.HttpClient client = new();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using HttpResponseMessage response = await client.PostAsync(uri, new FormUrlEncodedContent(dict));
            var token = await response.Content.ReadAsStringAsync();

            var jsonNode = JSON.Parse(token);
            String access_token = jsonNode["access_token"];
            String token_type = jsonNode["token_type"];

            if (!token_type.Equals("bearer", StringComparison.InvariantCultureIgnoreCase))
                return false;

            _bearerToken = access_token;
            return true;
        }

        public async Task<(Boolean result, String? ragDocumentId)> UploadMessageThenProcessAsync(Peer peerBubbleContext, Rainbow.Model.Message message)
        {
            using var stream = GenerateStreamFromString(message.Content);
            if (stream is not null)
            {
                String subpath = String.Empty;
                var metadata = GenerateMetaData(peerBubbleContext, message);

                return await UploadFileThenProcessAsync(stream, $"message_{message.Id}.txt", "text/plain", metadata, subpath);
            }
            return (false, null);
        }

        public async Task<(Boolean result, String? ragDocumentId)> UploadFileThenProcessAsync(Peer peerBubbleContext, Contact fileOwner, FileDescriptor fileDescriptor, String filePath)
        {
            using var stream = GenerateStreamFromFile(filePath);
            if (stream is not null)
            {
                String subpath = String.Empty;
                var metadata = GenerateMetaData(peerBubbleContext, fileDescriptor, fileOwner);

                return await UploadFileThenProcessAsync(stream, fileDescriptor.Id+Path.GetExtension(filePath), fileDescriptor.TypeMIME, metadata, subpath);
            }
            return (false, null);
        }

        private async Task<(Boolean result, String? ragDocumentId)> UploadFileThenProcessAsync(Stream stream, String streamName, String typeMime, Dictionary<String, String> metadata, String? subpath = null)
        {
            if (!(await RenewTokenAsync())) return (false, null);

            string uri;
            if (IsArchivedFile(streamName))
                uri = $"https://{Configuration.Instance.Host}/api/v1/{Configuration.Instance.Repository}/files/upload_unzip_then_process_any";
            else
                uri = $"https://{Configuration.Instance.Host}/api/v1/{Configuration.Instance.Repository}/files/upload_then_process_any";

            //var isAudioFile = IsAudioFile(typeMime);
            var isVideoFile = IsVideoFile(typeMime);
            var isImageFile = IsImageFile(typeMime);
            var isArchiveFile = IsArchivedFile(streamName);

            // Set default Query parameters
            List<KeyValuePair<string, string>> queryParameters = new()
            {
                new KeyValuePair<string, string>("run_translate", isImageFile ? "false" : "true"),
                new KeyValuePair<string, string>("run_enrich", (isImageFile || isVideoFile) ? "false" : "true"),
                new KeyValuePair<string, string>("run_chunk", "true"),
                new KeyValuePair<string, string>("run_relation", (isImageFile || isVideoFile || isArchiveFile) ? "false" : "true"), // TODO - to remove when bug fixed on RAG
                new KeyValuePair<string, string>("process_images", isImageFile ? "true" : "false"),
                new KeyValuePair<string, string>("with_queuing", "false")
            };

            if(!String.IsNullOrEmpty(subpath))
                queryParameters.Add(new KeyValuePair<string, string>("subpath", subpath));

            // Add Metadata parameters
            if (metadata != null)
            {
                foreach(var kvp in metadata)
                {
                    //// Escape " in key and value
                    //var key = kvp.Key.Replace("\"", "\\\"");
                    //var value = kvp.Value.Replace("\"", "\\\"");
                    queryParameters.Add(new KeyValuePair<string, string>("custom_fields", kvp.Key));
                    queryParameters.Add(new KeyValuePair<string, string>("custom_values", kvp.Value));
                }
            }

            // Add query parameters to URI
            var urlEncodedContent = new FormUrlEncodedContent(queryParameters);
            string query = await urlEncodedContent.ReadAsStringAsync();
            var uriBuilder = new UriBuilder(uri)
            {
                Query = query
            };
            uri = uriBuilder.Uri.ToString();

            using var formContent = new MultipartFormDataContent();
            formContent.Headers.ContentType.MediaType = "multipart/form-data";
            formContent.Add(new StreamContent(stream), "file", streamName);

            using var client = new System.Net.Http.HttpClient();

            client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _bearerToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));


            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(TimeSpan.FromMilliseconds(Timeout.Infinite));
            try
            {
                var response = await client.PostAsync(uri, formContent, tokenSource.Token);
                string result = await response.Content.ReadAsStringAsync(tokenSource.Token);

                if (!response.IsSuccessStatusCode)
                {
                    _log.LogWarning("ragError - upload_then_process_any failed - Uri:[{Uri}] - StatusCode:[{StatusCode}] - ResponseContent:[{Content}] ", uri, response.StatusCode, result);
                    return (false, null);
                }
                else
                {
                    var jsonNode = JSON.Parse(result);
                    var ragDocumentId = jsonNode?["id_document"];

                    result = result.TrimEnd('\r').TrimEnd('\n');

                    _log.LogDebug("upload_then_process_any - Result:[{result}]", result);
                    return (true, ragDocumentId);
                }
            }
            catch (Exception _ex)
            {
                _log.LogWarning("ragError - upload_then_process_any failed - Uri:[{Uri}] - Exception:[{Exception}]", uri, _ex);
                return (false, null);
            }
        }

        private System.Net.Http.HttpClient GetHttpClient()
        {
            if(_httpClient is null)
            {
                _httpClient = new System.Net.Http.HttpClient();
                _httpClient.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);
            }
            return _httpClient;
        }

        private HttpRequestMessage GetHttpRequestMessage(HttpMethod httpMethod, String uri, String? content = null, String contentType = "application/json")
        {
            var request = new HttpRequestMessage()
            {
                Method = httpMethod,
                RequestUri = new Uri(uri)
            };
            if (!String.IsNullOrEmpty(content))
            {
                request.Content = new StringContent(content);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            }
            request.Headers.Add("Authorization", "Bearer " + _bearerToken);

            
            return request;
        }

        public async Task<(Boolean result, Stream? stream, StreamReader? streamReader)> Query(Peer peerContactAsking, Peer peerBubbleContext, String query, Boolean forceCreate = false)
        {
            if (!(await RenewTokenAsync())) return (false, null, null);

            String uri;
            String? ragConversationId;

            String keyForRagConversation = $"{peerContactAsking}_{peerBubbleContext.Jid}";

            lock (_lock)
            {
                if (_rabConversationByBubbleJid.TryGetValue(keyForRagConversation, out ragConversationId) && (!forceCreate))
                    uri = $"https://{Configuration.Instance.Host}/api/v1/{Configuration.Instance.Repository}/qwassistant/call_assistant_conversation_turn";
                else
                    uri = $"https://{Configuration.Instance.Host}/api/v1/{Configuration.Instance.Repository}/qwassistant/call_assistant_conversation_creation";
            }

            // We want to creat a new ragConversationId
            if (forceCreate)
                ragConversationId = null;

            String? content = GenerateBodyPrompt(peerContactAsking, peerBubbleContext, query, ragConversationId) ;
            if (content is null) return (false, null, null);

            // Create HttpClient and HttpRequestMessage
            var httpClient = GetHttpClient();
            var httpRequestMessage = GetHttpRequestMessage(HttpMethod.Post, uri, content);
            try
            {
                var stopWatch = Stopwatch.StartNew();
                var response = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, _tokenSource.Token);
                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(_tokenSource.Token);
                    _log.LogWarning("ragError - call_assistant_conversation failed - Uri:[{Uri}] - StatusCode:[{StatusCode}] - Body:[{Body}]] - ResponseContent:[{Content}] ", uri, response.StatusCode, content, responseContent);
                    if (!forceCreate)
                        return (await Query(peerContactAsking, peerBubbleContext, query, true));
                    else
                        return (false, null, null);
                }
                else
                {
                    // Stop Watch and get elapsed time
                    stopWatch.Stop();
                    TimeSpan t = TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds);
                    string elapsedTime = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                            t.Hours,
                                            t.Minutes,
                                            t.Seconds,
                                            t.Milliseconds);
                    _log.LogInformation("call_assistant_conversation - Uri:[{Uri}] - Body:[{Content}] - ElapsedTime:[{ElapsedTime}] (to get header)", uri, content, elapsedTime);

                    var stream = await response.Content.ReadAsStreamAsync(_tokenSource.Token);
                    StreamReader streamReader = new(stream, true);

                    /// Read first line to get ragConversationId (if necessary)
                    var lineFromRag = await streamReader.ReadLineAsync();
                    if ((ragConversationId is null) && (lineFromRag is not null))
                    {
                        var jsonNode = JSON.Parse(lineFromRag);
                        if (jsonNode is not null)
                        {
                            ragConversationId = jsonNode["conversation_id"];
                            if (!String.IsNullOrEmpty(ragConversationId))
                                _rabConversationByBubbleJid[keyForRagConversation] = ragConversationId;
                        }
                    }

                    return (true, stream, streamReader);
                }
            }
            catch (Exception _ex)
            {
                _log.LogWarning("ragError - call_assistant_conversation failed - Uri:[{Uri}] - Exception:[{Exception}]", uri, _ex);

                if(!forceCreate)
                    return (await Query(peerContactAsking, peerBubbleContext, query, true));
                else
                    return (false, null, null);
            }
        }

        //public async Task UploadFileThenProcessAsync(String folderPath, String fileName, Dictionary<String, String>? metadata  = null)
        //{
        //    string uri = $"https://{_host}/api/v1/{_repository}/files/upload_then_process_any";
        //    fileName = "camera.png";
        //    folderPath = "C:\\media\\";
        //    string result = "";


        //    // Set default Query parameters
        //    Dictionary<string, string> queryParameters = new()
        //    {
        //        { "run_translate", "true" },
        //        { "run_enrich", "true" },
        //        { "run_chunk", "true" },
        //        { "run_relation", "true" },
        //        { "process_images", "true" },
        //        { "with_queuing", "false" }
        //    };

        //    if(metadata is null) // FOR TEST PURPOSES ONLY
        //    {
        //        metadata = new()
        //        {
        //            { "companyId", "compId" },
        //            { "bubbleId", "bubbleId" },
        //            { "messageId", "" },
        //            { "ownerId", "ownerId" },
        //            { "ownerFirstName", "firstName" },
        //            { "ownerLastName", "lastName" },
        //            { "uploadDate", JSON.ParseDateTimeToString(DateTime.UtcNow) }
        //        };
        //    }

        //    // Add Metadata parameters
        //    String? customFields = null;
        //    String? customValues = null;
        //    if (metadata != null)
        //    {
        //        customFields ="\"" + String.Join("\",\"", metadata.Keys) + "\"";
        //        customValues = "\"" + String.Join("\",\"", metadata.Values) + "\"";

        //        queryParameters.Add("custom_fields", customFields);
        //        queryParameters.Add("custom_values", customValues);
        //    }

        //    // Add query parameters to URI
        //    var urlEncodedContent = new FormUrlEncodedContent(queryParameters);
        //    string query = await urlEncodedContent.ReadAsStringAsync();

        //    var uriBuilder = new UriBuilder(uri)
        //    {
        //        Query = query
        //    };

        //    uri = uriBuilder.Uri.ToString();


        //    using (var formContent = new MultipartFormDataContent())
        //    {
        //        formContent.Headers.ContentType.MediaType = "multipart/form-data";

        //        Stream fileStream = System.IO.File.OpenRead(Path.Combine(folderPath, fileName));
        //        formContent.Add(new StreamContent(fileStream), "file", fileName);

        //        using (var client = new System.Net.Http.HttpClient())
        //        {
        //            // Bearer Token header if needed
        //            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _bearerToken);
        //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));

        //            try
        //            {
        //                var message = await client.PostAsync(uri, formContent);
        //                result = await message.Content.ReadAsStringAsync();
        //            }
        //            catch (Exception ex)
        //            {
        //                // Do what you want if it fails.
        //                throw ex;
        //            }
        //        }
        //    }


        //}

#region STATIC Methods

        internal static String ReplaceValues(String? content, Dictionary<string, string> values)
        {
            if (content == null) return "";
            if ((values is null) || (values.Count == 0)) return content;
            foreach(var kvp in values)
                content = content.Replace($"{{{kvp.Key}}}", kvp.Value);
            return content;
        }
        
        internal static Boolean IsArchivedFile(String fileName)
        {
            return Configuration.Instance.ArchiveFiles.Contains(GetFileExtension(fileName));
        }

        internal static Boolean IsImageFile(String typeMime)
        {
            if(String.IsNullOrEmpty(typeMime)) return false;
            return typeMime.StartsWith("image", StringComparison.InvariantCultureIgnoreCase);
        }

        internal static Boolean IsVideoFile(String typeMime)
        {
            if (String.IsNullOrEmpty(typeMime)) return false;
            return typeMime.StartsWith("video", StringComparison.InvariantCultureIgnoreCase);
        }

        internal static Boolean IsAudioFile(String typeMime)
        {
            if (String.IsNullOrEmpty(typeMime)) return false;
            return typeMime.StartsWith("audio", StringComparison.InvariantCultureIgnoreCase);
        }

        private static string GetFileExtension(String fileName)
        {
            return Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        }

        private static String? GenerateBodyPrompt(Peer peerContactAsking, Peer peerBubbleContext, String query, String? conversationId)
        {
            String? content;
            try
            {
                content = File.ReadAllText(Configuration.Instance.PromptBodyFile);
            }
            catch
            {
                return null;
            }

            if (String.IsNullOrEmpty(content)) return "";

            Dictionary<String, String> values = new()
            {
                { "query", JSONNode.Escape(query)},
                { "llm", Configuration.Instance.Llm},
                { "bubbleId", peerBubbleContext.Id }
            };

            if (conversationId is null) 
                values.Add("userIdOrConversationId", $"\"user_id\" : \"{peerContactAsking.Id}\"");
            else
                values.Add("userIdOrConversationId", $"\"conversation_id\" : \"{conversationId}\""); // Here it's the id create by the RAG to identify the conversation

            content = ReplaceValues(content, values);
            return content.Replace("\r", "").Replace("\n", "");
        }

        private static Dictionary<String, String> GenerateMetaData(String companyId, String bubbleId, String ownerId, String ownerFirstName, String ownerLastName, String messageId, DateTime? messageDate, String fileId, DateTime? uploadDate)
        {
            Dictionary<String, String> result = new();
            result.Add("bubbleId", bubbleId);
            // TODO - to add others values when bug fixed on RAG
            result.Add("companyId", companyId);
            result.Add("ownerId", ownerId);
            result.Add("ownerFirstName", ownerLastName);
            if (!String.IsNullOrEmpty(messageId))
                result.Add("messageId", messageId);
            if (messageDate is not null)
                result.Add("messageDate", JSON.ParseDateTimeToString(messageDate.Value));
            if (!String.IsNullOrEmpty(fileId))
                result.Add("fileId", fileId);
            if (uploadDate is not null)
                result.Add("uploadDate", JSON.ParseDateTimeToString(uploadDate.Value));

            return result;
        }

        private static Dictionary<String, String> GenerateMetaData(Peer peerBubbleContext, FileDescriptor fileDescriptor, Contact fileOwner)
        {
            return GenerateMetaData(fileOwner.CompanyId, 
                peerBubbleContext.Id, 
                fileOwner.Peer.Id, 
                fileOwner.FirstName, 
                fileOwner.LastName, 
                "", 
                null, 
                fileDescriptor.Id, 
                fileDescriptor.UploadedDate);
        }

        private static Dictionary<String, String> GenerateMetaData(Peer peerBubbleContext, Rainbow.Model.Message message)
        {
            return GenerateMetaData(message.FromContact.CompanyId, 
                peerBubbleContext.Id, 
                message.FromContact.Peer.Id, 
                message.FromContact.FirstName, 
                message.FromContact.LastName, 
                message.Id, 
                message.Date, 
                "", 
                null);
        }

        private static MemoryStream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private static FileStream? GenerateStreamFromFile(string filePath)
        {
            try
            {
                return System.IO.File.OpenRead(filePath);
            }
            catch
            {
                return null;
            }
        }

#endregion STATIC Methods
    }
}
