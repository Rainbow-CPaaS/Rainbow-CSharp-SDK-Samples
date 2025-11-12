using Rainbow.Model;
using Rainbow.SimpleJSON;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BotRAG.Model
{
    public class Elements<T>
    {
        public delegate String GetElementIdDelegate(T element);
        public delegate DateTime GetElementDateDelegate(T element);

        // cf. https://stackoverflow.com/questions/7502615/element-order-in-blockingcollection
        private readonly ConcurrentQueue<T> lowPriority;
        private readonly ConcurrentQueue<T> highPriority;

        private readonly SemaphoreSlim _semaphoreSlim;

        private readonly object _lock = new();

        private readonly Peer bubblePeer;
        private readonly String type;

        private readonly GetElementIdDelegate getElementId;
        private readonly GetElementDateDelegate getElementDate;

        private Boolean _cacheAlreadyFetched = false;

        private T? elementInProgres = default; // To store last element taken for processing

        private int totalElements = 0;      // To store total number of elements to process
        private int elementsProcessed = 0;  // To store nb of elements processed i.e. stored In RAG
        private int elementsSkipped = 0;    // To store nb of elements skipped
        private int elementsDownloaded = 0; // To store nb of elements uploaded (between to store them in RAG)
        private int elementsRagError = 0;   // To store nb resulting to rag error

        public Elements(Peer bubblePeer, String type, SemaphoreSlim semaphoreSlim, GetElementIdDelegate getElementIdDelegate, GetElementDateDelegate getElementDateDelegate)
        {
            this.bubblePeer = bubblePeer ?? throw new ArgumentNullException(nameof(bubblePeer));
            this.type = type ?? throw new ArgumentNullException(nameof(type));
            _semaphoreSlim = semaphoreSlim;

            this.getElementId = getElementIdDelegate ?? throw new ArgumentNullException(nameof(getElementIdDelegate));
            this.getElementDate = getElementDateDelegate ?? throw new ArgumentNullException(nameof(getElementDateDelegate));

            lowPriority = [];
            highPriority = [];


            // /!\ Don't need to gather info status from DB. We do it when enqueueing elements

            //// Need to get already processed elements from DB
            //List<(String status, int count)>? countByStatus = DatabaseManagement.Instance.GetCountByStatusFromType(bubblePeer.Id, type);
            //if(countByStatus is not null)
            //{
            //    foreach((String status, int count) in countByStatus)
            //    {
            //        switch (status)
            //        {
            //            case "skipped":
            //                elementsSkipped = count;
            //                break;
            //            case "downloaded":
            //                elementsDownloaded = count;
            //                break;
            //            case "processed":
            //                elementsProcessed = count;
            //                break;
            //            case "ragError":
            //                elementsRagError = count;
            //                break;
            //            default:
            //                break;
            //        }
            //        totalElements += count;
            //    }
            //}
        }

        /*
        "files":
        {
            "total": 123,
            
            "nbSkipped": 123,

            
            "nbProcessed: 123,
            "nbUploaded": 123,
        },
        */

        public Boolean CacheAlreadyFetched { get => _cacheAlreadyFetched; set => _cacheAlreadyFetched = value; }

        public int NbElementsStored => lowPriority.Count + highPriority.Count;

        public int TotalElements => totalElements;

        public int NbElementsProcessed => elementsProcessed;

        public String LastElementIdProcessed; // TODO
        public String LastElementIdDownloaded; // TODO

        public Dictionary<String, Object> GetStatus()
        {
            return new Dictionary<String, Object>
            {
                { "total", totalElements },
                { "nbSkipped", elementsSkipped },
                { "ragError", elementsRagError },
                { "nbProcessed", elementsProcessed },
                { "nbDownloaded", elementsDownloaded }
            };
                
        }
        
        private void UpdateElementCount(String status)
        {
            switch (status)
            {
                case "skipped":
                    elementsSkipped++;
                    break;
                case "downloaded":
                    elementsDownloaded++;
                    break;
                case "processed":
                    elementsProcessed++;
                    break;
                case "ragError":
                    elementsRagError++;
                    break;
                default:
                    break;
            }
        }

        public void ElementHasBeenManaged(T item, String status, String? ragDocumentId, Boolean updateDb = true) // status can be "skipped", "uploaded", "processed", ...
        {
            lock (_lock)
            {
                UpdateElementCount(status);

                // Update DB
                if (updateDb)
                {
                    String id = getElementId(item);
                    DatabaseManagement.Instance.InsertElement(bubblePeer.Id, type, id, status, ragDocumentId);
                }
            }
        }

        private Boolean NeedToBeQueued(string? status)
        {
            if (status is null)
                return true;
            return !status.Equals("processed");
        }

        public Boolean Enqueue(IEnumerable<T> items, Boolean highPriority = false)
        {
            Boolean oneElementHasNotBeenQueued = false;
            foreach (var item in items)
            {
                if(!Enqueue(item, highPriority))
                {
                    oneElementHasNotBeenQueued = true;
                }

                //if (item is null) continue;

                //String id = getElementId(item);

                //// Check if this element has already been processed
                //var status = DatabaseManagement.Instance.GetElementStatus(bubblePeer.Id, type, id);
                //if(NeedToBeQueued(status))
                //{
                //    if (highPriority)
                //        this.highPriority.Enqueue(item);
                //    else
                //        this.lowPriority.Enqueue(item);

                //    _semaphoreSlim.Release();
                //}
                //else
                //    ElementHasBeenManaged(item, "processed", false);

                //totalElements++;
            }
            return !oneElementHasNotBeenQueued;
        }

        public Boolean Enqueue(T item, Boolean highPriority = false)
        {
            Boolean result = false;

            if (item is null) return result;
            String id = getElementId(item);

            // Check if this element has already been processed
            var status = DatabaseManagement.Instance.GetElementStatus(bubblePeer.Id, type, id);
            if (NeedToBeQueued(status))
            {
                if (highPriority)
                    this.highPriority.Enqueue(item);
                else
                    this.lowPriority.Enqueue(item);

                _semaphoreSlim.Release();
                result = true;
            }
            else
            {
                ElementHasBeenManaged(item, "processed", null, false);
            }
            totalElements++;

            return result;
        }

        public T? Dequeue()
        {
            if (highPriority.TryDequeue(out T? result))
                return result;

            if (lowPriority.TryDequeue(out result))
                return result;
            return default;
        }
    }
}
