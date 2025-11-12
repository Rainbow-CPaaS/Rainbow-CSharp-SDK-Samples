using Microsoft.Extensions.Logging;
using Rainbow;
using Rainbow.Model;
using Rainbow.SimpleJSON;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace BotRAG.Model
{
    public class BubblesManagement
    {
        private static BubblesManagement? _instance = null;

        private ILogger _log;
        private Boolean _started = false;
        private readonly Object _lock = new();

        private List<String> _bubblesNameAllowed = [];
        private List<String> _bubblesNameAllowedUsingWildCard = [];
        private readonly Dictionary<String, BubbleElements> _bubblesElements = [];

        private Application? _rbApplication;
        private Bubbles? _rbBubbles;

        /// <summary>
        /// Get instance of <see cref="BubblesManagement"/> service
        /// </summary>
        public static BubblesManagement Instance
        {
            get
            {
                _instance ??= new();
                return _instance;
            }
        }

        public void FollowBubbleStatus(Peer? peerContact, String? contactResource, Peer? peerBubble, Boolean follow, Boolean usingIm)
        {
            if (peerBubble is null) return;

            if(_bubblesElements.TryGetValue(peerBubble.Jid, out var bubbleElements))
                bubbleElements?.FollowProgressStatus(peerContact, contactResource, follow, usingIm);

        }

        public void SetApplication(Application rbApplication)
        {
            if (_rbApplication is not null) return;

            _rbApplication = rbApplication;
            _rbBubbles = rbApplication.GetBubbles();

            _log = LogFactory.CreateLogger<BubblesManagement>(rbApplication.LoggerPrefix);
            _log.LogInformation("[BubblesManagement] Application set");

            _rbBubbles.BubbleAffiliationChanged += Bubbles_BubbleAffiliationChanged;
        }

        public void ConfigurationUpdated()
        {
            lock (_lock)
            {
                _bubblesNameAllowed.Clear();
                _bubblesNameAllowedUsingWildCard.Clear();

                List<String>? allowed = Configuration.Instance.BubbleNamesAllowed;

                if (allowed?.Count > 0)
                {
                    foreach (var b in allowed)
                    {
                        if (b.EndsWith("*"))
                            _bubblesNameAllowedUsingWildCard.Add(b[..^1]);
                        else
                            _bubblesNameAllowed.Add(b);
                    }
                }
            }

            UpdateBubblesConfiguration();
        }

        private Boolean IsBubbleAllowed(Peer? peerBubble)
        {
            lock (_lock)
            {
                if (peerBubble is null) return false;

                var b = _bubblesNameAllowed.FirstOrDefault(b => b.Equals(peerBubble.DisplayName, StringComparison.InvariantCultureIgnoreCase));
                if (b is not null)
                    return true;

                b = _bubblesNameAllowedUsingWildCard.FirstOrDefault(b => b.StartsWith(peerBubble.DisplayName, StringComparison.InvariantCultureIgnoreCase));
                if (b is not null)
                    return true;

                if ((_bubblesNameAllowed.Count > 0) || (_bubblesNameAllowedUsingWildCard.Count > 0))
                {
                    //_log.LogWarning("Bubbles is not allowed / managed - Bubble:[{Id}] - [{DisplayName}]", peerBubble.Id, Rainbow.Util.LogString(peerBubble.DisplayName));
                    return false;
                }

                return true;
            }
        }

        private async Task ManageBubbleAsync(Peer peerBubble, Boolean removeIt)
        {
            // Get associated bubbleElements (if any)
            Boolean alreadyKnown = _bubblesElements.TryGetValue(peerBubble.Jid, out BubbleElements? bubbleElements);

            if(removeIt)
            {
                if (bubbleElements is null) return;

                _bubblesElements.Remove(peerBubble.Jid);
                await bubbleElements.StopAsync();
                await bubbleElements.RemoveAllDataFromRAGAsync();
            }
            else
            {
                if (IsBubbleAllowed(peerBubble))
                {
                    if (!alreadyKnown)
                    {
                        bubbleElements = new BubbleElements(_rbApplication, peerBubble);
                        _bubblesElements[peerBubble.Jid] = bubbleElements;
                    }

                    if (_started)
                        bubbleElements?.StartAsync();
                    return;
                }
            }
        }

        private void UpdateBubblesConfiguration()
        {
            if (_rbBubbles is null) return;

            // Add existing bubbles if necessary
            var bubbles = _rbBubbles.GetAllBubbles();
            foreach (var b in bubbles)
            {
                var _ = ManageBubbleAsync(b, false);
            }

            // Stop existing BubbleElements if necessary
            var a = _bubblesElements?.Values.ToArray();
            if (a?.Length > 0)
            {
                foreach (var bubbleElements in a)
                {
                    if (!IsBubbleAllowed(bubbleElements.Peer))
                    {
                        var _ = ManageBubbleAsync(bubbleElements.Peer, true);
                    }
                }
            }
        }

        private async void Bubbles_BubbleAffiliationChanged(Rainbow.Model.BubbleAffiliation bubbleAffiliation)
        {
            var bubbleJid = bubbleAffiliation.Bubble?.Peer?.Jid;
            if (String.IsNullOrEmpty(bubbleJid) || (_rbApplication is null)) return;

            await ManageBubbleAsync(bubbleAffiliation.Bubble, !bubbleAffiliation.HasJoined);
        }

        public async Task StartAsync()
        {
            if (_started) return;
            _started = true;

            UpdateBubblesConfiguration();

            
            await Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            if (!_started) return;
            _started = false;

            var a = _bubblesElements?.Values.ToArray();
            if (a?.Length > 0)
            {
                foreach (var _bubbleElements in a)
                {
                    var _ = _bubbleElements.StopAsync();
                }
            }

            await Task.CompletedTask;
        }

        private BubblesManagement()
        {
            
        }
    }
}
