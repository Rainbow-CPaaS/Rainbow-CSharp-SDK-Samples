using BotLibrary.Model;
using BotRAG;
using BotRAG.Model;
using Microsoft.Extensions.Logging;
using Rainbow;
using Rainbow.Model;
using Rainbow.SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotRag
{
    public class BotRAG: BotLibrary.BotBase
    {
        private String[] DOC_REFS_START = ["[#", "[", "("];
        private String[] DOC_REFS_END = ["]", ")"];

        public Configuration Configuration = new Configuration();
        private FileStorage? _rbFileStorage;

    #region Messages -  ApplicationMessage, InstantMessage

        public override async Task ApplicationMessageReceivedAsync(Rainbow.Model.ApplicationMessage applicationMessage)
        {
            await Task.CompletedTask;
        }

        public override async Task InstantMessageReceivedAsync(Rainbow.Model.Message iMessage)
        {
            var _ = AskRag(iMessage, true); 
            await Task.CompletedTask;
        }

        public override async Task PrivateMessageReceivedAsync(Rainbow.Model.Message iMessage)
        {
            var _ = AskRag(iMessage, false);
            await Task.CompletedTask;
        }

        private async Task AskRag(Rainbow.Model.Message iMessage, Boolean isIM)
        {
            // Need to check the content of the message
            String query = iMessage.Content;
            if (String.IsNullOrEmpty(query))
                return;

            query = query.Replace("&quot;", "\""); // => Should be no more necessary
            if (query.Equals("{\"status\": \"ask\"}")) // Ask to follow status
            {
                BubblesManagement.Instance.FollowBubbleStatus(iMessage.FromContact, iMessage.FromResource, iMessage.ToBubble, true, isIM);
            }
            else if (query.Equals("{\"status\": \"cancel\"}")) // Cancel to follow status
            {
                BubblesManagement.Instance.FollowBubbleStatus(iMessage.FromContact, iMessage.FromResource, iMessage.ToBubble, false, isIM);
            }
            else // It's a query to RAG
            {
               

                Dictionary<String, String> documents = []; // by ref Id (i.e doc_0) and ragDocumentationId

                Peer peerContactAsking = iMessage.FromContact;
                Peer peerBubbleContext = iMessage.ToBubble;

                (Boolean result, Stream? stream, StreamReader? streamReader) = await RAGClient.Instance.Query(peerContactAsking, peerBubbleContext, query);

                if (result && (stream is not null) && (streamReader is not null))
                {
                    int nbLinesFromRag = 1;
                    String? lineFromRag;
                    StringBuilder stringBuilder = new();
                    List<String> docRef = [];
                    int chunckIndex = 0;

                    String ragAnswerContent;

                    while ((lineFromRag = await streamReader.ReadLineAsync()) != null)
                    {
                        log.LogDebug("lineFromRag:[{line}]", lineFromRag);

                        // Skip first line (???)
                        //if (nbLinesFromRag == 1)
                        //{
                        //    nbLinesFromRag++;
                        //    continue;
                        //}
                        //else
                        {
                            var jsonNode = JSON.Parse(lineFromRag);
                            if (jsonNode is not null)
                            {
                                // Get content
                                JSONNode conv = jsonNode["conversation"];

                                if (conv.IsArray)
                                {
                                    for (int i = 0; i < conv.Count; i++)
                                    {
                                        if (conv[i]["role"] == "tool")
                                        {
                                            // Here, we get references to document. We get info from DB
                                            _rbFileStorage ??= Application.GetFileStorage();

                                            // We try to get info about documents used
                                            var jDocs = conv[i]["artifact"]?["docs"];
                                            if(jDocs is not null)
                                            {
                                                foreach(var key in jDocs.Keys)
                                                {
                                                    String ragDocumentationId = jDocs[key]?["IDQES"];
                                                    (String? type, String? id) = DatabaseManagement.Instance.GetElementByRagDocumentId(peerBubbleContext.Id, ragDocumentationId);

                                                    String docTitle = "";

                                                    if ((type == "F") || (type == "R"))
                                                    {
                                                        var fd = _rbFileStorage.GetFileDescriptor(id);
                                                        if (fd is not null)
                                                            docTitle = fd.FileName;
                                                        else

                                                            docTitle = (type == "F") ? "File" : "Recording";
                                                    }
                                                    else if (type == "M")
                                                    {
                                                        docTitle = "Message in bubble";
                                                    }
                                                    else
                                                        docTitle = "";

                                                    log.LogDebug("For [{key}] associated IDQES:[{ragDocumentationId}] , Document Type:[{type}] Id:[{id}] => docTitle:[{docTitle}]", key, ragDocumentationId, type, id, docTitle);
                                                    documents[key] = docTitle;
                                                }
                                            }
                                            continue;
                                        }
                                        else if (conv[i]["role"] == "assistant")
                                        {
                                            // We take content only from the assistant
                                            ragAnswerContent = conv[i]["content"];

                                            // We take assumption here thar ref. to doc are treated like this:
                                            /*
                                            
                                            " [#"
                                            "doc"
                                            "_"
                                            "2"
                                            "]"

                                            OR

                                            " ("
                                            "doc"
                                            "_"
                                            "2"
                                            "]"


                                            */


                                            if (String.IsNullOrEmpty(ragAnswerContent)) continue;
                                            
                                            // Append content
                                            stringBuilder.Append(ragAnswerContent);

                                            // START - Check doc Ref
                                            switch(docRef.Count)
                                            {
                                                case 0:
                                                    if (DOC_REFS_START.Any(ragAnswerContent.EndsWith))
                                                        docRef.Add(ragAnswerContent);
                                                    break;
                                                case 1:
                                                    if (ragAnswerContent.Equals("doc"))
                                                        docRef.Add(ragAnswerContent);
                                                    else
                                                        docRef.Clear();
                                                    break;
                                                case 2:
                                                    if (ragAnswerContent.Equals("_"))
                                                        docRef.Add(ragAnswerContent);
                                                    else
                                                        docRef.Clear();
                                                    break;
                                                case 3:
                                                    docRef.Add(ragAnswerContent);
                                                    break;
                                                case 4:
                                                    if (DOC_REFS_END.Any(ragAnswerContent.StartsWith))
                                                        docRef.Add(ragAnswerContent);
                                                    else
                                                        docRef.Clear();
                                                    break;
                                            }

                                            // If we have 5 elements, we have a doc reference
                                            if(docRef.Count == 5)
                                            {
                                                var keyFound = docRef[1] + docRef[2] + docRef[3];
                                                if(documents.ContainsKey(keyFound))
                                                {
                                                    var title = documents[keyFound];
                                                    if(String.IsNullOrWhiteSpace(title))
                                                    {
                                                        log.LogDebug("Doc ref. found [{keyFound}] but related title is empty..", keyFound);
                                                        docRef.Clear();
                                                    }
                                                    else
                                                    {
                                                        var strDocRef = String.Join("", docRef);
                                                        docRef.Clear();

                                                        var msg = stringBuilder.ToString();
                                                        msg = msg.Replace(strDocRef, $"({title})");

                                                        log.LogDebug("Doc ref. found [{keyFound}]- replaced by [{}]..", keyFound, title);

                                                        AnswerToMessage(iMessage, chunckIndex, false, msg, isIM);
                                                        chunckIndex++;
                                                        stringBuilder.Clear();

                                                        continue;
                                                    }

                                                }
                                            }
                                            // END - Check doc Ref

                                            // Do we reach the size of the chunck ?
                                            if ( (docRef.Count == 0) && (stringBuilder.Length > Configuration.Instance.ChunckSize))
                                            {
                                                AnswerToMessage(iMessage, chunckIndex, false, stringBuilder.ToString(), isIM);
                                                chunckIndex++;
                                                stringBuilder.Clear();
                                            }
                                        }
                                    }
                                }
                                //else
                                //{
                                //    // We take content only from the assistant
                                //    if (conv["role"] != "assistant")
                                //        continue;

                                //    ragAnswerContent = conv["content"];
                                //    if (String.IsNullOrEmpty(ragAnswerContent)) continue;
                                //    // Append content
                                //    stringBuilder.Append(ragAnswerContent);
                                //    // Do we reach the size of the chunck ?
                                //    if (stringBuilder.Length > Configuration.Instance.ChunckSize)
                                //    {
                                //        AnswerToMessage(iMessage, chunckIndex, false, stringBuilder.ToString(), isIM);
                                //        chunckIndex++;
                                //        stringBuilder.Clear();
                                //    }
                                //}
                            }
                            nbLinesFromRag++;
                        }
                    }

                    // Send last message
                    AnswerToMessage(iMessage, chunckIndex, true, stringBuilder.ToString(), isIM);

                    streamReader.Dispose();
                    stream.Dispose();
                }
                else
                {
                    // Need to send back an empty answer to avoid the waiting spinner on the client side
                    AnswerToMessage(iMessage, 0, true, "", isIM);
                }

            }
            await Task.CompletedTask;
        }

        private void AnswerToMessage(Rainbow.Model.Message iMessage, int chunckIndex, Boolean finalChunck, String content, Boolean isIM)
        {
            if (isIM)
            {
                var _ = Application.GetInstantMessaging().AnswerToMessageAsync(iMessage, content);
            }
            else
            {
                Dictionary<String, Object> ragAnswer = [];
                Dictionary<String, Object> message = [];
                ragAnswer["chunckIndex"] = chunckIndex;
                ragAnswer["finalChunk"] = finalChunck;
                ragAnswer["content"] = content;
                message["message"] = ragAnswer;

                var messageJson = JSON.ToJson(message);
                var _ = Application.GetInstantMessaging().AnswerToPrivateMessageAsync(iMessage, messageJson);
            }
        }

    #endregion Messages - ApplicationMessage, InstantMessage

        public override async Task BotConfigurationUpdatedAsync(BotConfigurationUpdate botConfigurationUpdate)
        {
            // Ensure to have an object not null
            if (botConfigurationUpdate is null)
                return;

            Configuration.Instance.Update(botConfigurationUpdate.JSONNodeBotConfiguration["ragConfig"]);

            BubblesManagement.Instance.ConfigurationUpdated();

            await Task.CompletedTask;
        }

        public override async Task ConnectedAsync()
        {
            RAGClient.Instance.SetApplication(this.Application);

            BubblesManagement.Instance.SetApplication(this.Application);
            var _1 = BubblesManagement.Instance.StartAsync();

            await Task.CompletedTask;
        }
    }
}
