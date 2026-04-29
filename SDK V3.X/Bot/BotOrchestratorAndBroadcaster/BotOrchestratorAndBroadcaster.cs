using AdaptiveCards.Templating;
using BotBroadcaster.Model;
using BotLibrary;
using BotLibrary.Model;
using BotOrchestratorAndBroadcaster.Model;
using Microsoft.Extensions.Logging;
using Rainbow;
using Rainbow.Consts;
using Rainbow.Example.Common;
using Rainbow.Model;
using Rainbow.SimpleJSON;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BotOrchestratorAndBroadcaster
{
    public class BotOrchestratorAndBroadcaster: BotLibrary.BotBase
    {
        private String _guid = "";

        private readonly ConcurrentDictionary<String, BotBroadcaster.BotBroadcaster> _broadcasters = [];

        private String _currentConferenceId = "";
        private readonly ConcurrentDictionary<String, Message> _messageByAdminId = []; // Store Message Id (value) by Admin Id (key) to easily retrieve it when it's necessary to update the Adaptive Card.

        private Bubbles? _rbBubbles = null;
        private Contacts? _rbContacts = null;
        private Rainbow.Conferences? _rbConferences = null;
        private InstantMessaging? _rbInstantMessaging = null;

        private Model.BotConfigurationExtended? _currentBotConfigurationExtended = null;
        private readonly FifoSemaphore _semaphoreSendOrUpdateAdaptiveCard = new(1);

        public BotOrchestratorAndBroadcaster()
        {
            if (!Init())
                throw new Exception("Cannot Init BotOrchestratorAndBroadcaster");
        }

        private Boolean Init()
        {
            //TODO - ensure we can create a default Adaptive Card (using default value) to ensure that all embedded resources are well loaded and with the correct format (for example if you have an error in your JSON template, you will see it at the bot startup and not when you send the first message)
            return true;
        }

    #region Override some methods of BotLibrary.BotBase to add specific features in this Bot

        public override Restrictions GetRestrictions()
        {
            _guid = Rainbow.Util.GetGUID();

            var restrictions = base.GetRestrictions();

            // We need to use Conferences in this Bot
            restrictions.UseConferences = true;

            return restrictions;
        }

        public override async Task InstantMessageReceivedAsync(Rainbow.Model.Message message)
        {
            // Check if this messge is coming from admin
            if (!String.IsNullOrEmpty(message?.FromContact?.Peer?.Jid))
            {
                var account = _currentBotConfigurationExtended?.Administrators?.FirstOrDefault(account => account.Jid == message.FromContact.Peer.Jid);
                if(account is not null) // => An admin send us a IM
                {
                    // Is it a response to an adaptive cards ? And if it's the case what is the answer ?
                    (Boolean isAdaptiveCardAnswer, JSONNode? jsonNode) = GetDataFromAdaptiveCardResponse(message);
                    if(isAdaptiveCardAnswer)
                        await UpdateConfigurationBasedOnActionOnAdaptiveCardAsync(jsonNode);
                }
            }
            await Task.CompletedTask;
        }

        public override async Task ConnectedAsync()
        {
            UpdateAdministratorsInfoInConfig();
            UpdateConfigurationFileOnDiskForEachBroadcaster();

            var _ =  StartBroadcastersAsync(); // Must be done after UpdateAdministratorsInfoInConfig() to have correct id, jid for broadcasters

            // once (re)connected - try to get previous message use to set/update AC
            if (_rbInstantMessaging is not null)
            {
                ConsoleAbstraction.WriteYellow($"[{DateTime.Now:HH:mm:ss.fff}][{_credentials.UsersConfig[0].Prefix}] Trying to retrieve message information ...");
                var messages = _messageByAdminId.Values.ToList();
                _messageByAdminId.Clear();
                foreach (var message in messages)
                {
                    if (message is null) continue;
                    
                    var contact = message.FromContact;
                    if (contact is null) continue;

                    var sdkResult = await _rbInstantMessaging.GetOneMessageAsync(contact, message.Id);
                    if(sdkResult.Success)
                    {
                        ConsoleAbstraction.WriteYellow($"[{DateTime.Now:HH:mm:ss.fff}][{_credentials.UsersConfig[0].Prefix}] Message retrieved - Id:[{message.Id}] - Contact:[{contact.ToString()}]");

                        var msgFromServer = sdkResult.Data;
                        _messageByAdminId[msgFromServer.Id] = msgFromServer;
                    }
                    else
                    {
                        ConsoleAbstraction.WriteRed($"[{DateTime.Now:HH:mm:ss.fff}][{_credentials.UsersConfig[0].Prefix}] Message not retrieved - Id:[{message.Id}] - Contact:[{contact.ToString()}]");
                    }
                }
            }

            await SendOrUpdateAdaptiveCardToAllAdminAsync();
        }

        public override async Task BotConfigurationUpdatedAsync(BotConfigurationUpdate botConfigurationUpdate)
        {
            // Ensure to have an object not null
            if (botConfigurationUpdate is null)
                return;

            if (botConfigurationUpdate.Context == "configFile")
            {
                if (_rbBubbles is null)
                {
                    _rbBubbles = Application.GetBubbles();
                    _rbBubbles.BubbleMemberUpdated += Bubbles_BubbleMemberUpdated;
                }

                _rbContacts = Application.GetContacts();
                _rbInstantMessaging = Application.GetInstantMessaging();

                if(_rbConferences is null)
                {
                    _rbConferences = Application.GetConferences();
                    _rbConferences.ConferenceUpdated += Conferences_ConferenceUpdated;
                    _rbConferences.ConferenceRemoved += Conferences_ConferenceRemoved;
                }
            }

            // BotConfigurationExtended object has been created to store data structure specific for this bot
            // We try to parse JSON Node to fill this data structure and if it's correct we update the configuration
            var botConfigurationExtended = Model.BotConfigurationExtended.FromJsonNode(botConfigurationUpdate.JSONNodeBotConfiguration);
            if (botConfigurationExtended is not null)
            {
                _currentBotConfigurationExtended = botConfigurationExtended;
                UpdateAdministratorsInfoInConfig();
                await SendOrUpdateAdaptiveCardToAllAdminAsync();

                if ((botConfigurationUpdate.Context != "configFile") && (botConfigurationUpdate.Context != "reload"))
                    UpdateConfigurationFileOnDisk();

                UpdateConfigurationFileOnDiskForEachBroadcaster();
            }
        }


#endregion Override some methods of BotLibrary.BotBase to add specific features in this Bot


#region Events from SDK

        private async Task StartBroadcastersAsync()
        {

            var broadcastersInfo = _currentBotConfigurationExtended?.Broadcasters.ToList();
            if (broadcastersInfo is null) return;

            foreach (var broadcasterInfo in broadcastersInfo)
            {
                if (broadcasterInfo is null) continue;

                 _broadcasters.TryGetValue(broadcasterInfo.Id, out var broadcaster);

                if(broadcaster is null)
                {
                    // Create credentials
                    Credentials credentials = new()
                    {
                        UsersConfig =
                        [
                            new()
                            {
                                IniFolderPath = Application.GetIniPathFolder(),
                                Prefix = broadcasterInfo.Id,
                                Login = broadcasterInfo.Login,
                                Password = broadcasterInfo.Password,
                                AutoLogin = true, // Is it necessary ?
                            }
                        ],
                        ServerConfig = _credentials.ServerConfig
                    };

                    var configFile = GetBroadcasterConfigurationPathFile(broadcasterInfo.Id);
                    var botConfiguration = JSON.Parse(File.ReadAllText(configFile));

                    broadcaster = new BotBroadcaster.BotBroadcaster();
                    var result = await broadcaster.Configure(credentials.ToJsonNode() ?? new JSONObject(), botConfiguration["botConfiguration"]);
                    if (result)
                    {
                        _broadcasters[broadcasterInfo.Id] = broadcaster;

                        if (!broadcaster.Login())
                            ConsoleAbstraction.WriteRed("Cannot start login process");
                        else
                            ConsoleAbstraction.WriteGreen($"BotBroadcaster for '{broadcasterInfo.Login}' started successfully.");
                    }
                    else
                    {
                        ConsoleAbstraction.WriteRed($"Cannot configure BotBroadcaster for '{broadcasterInfo.Login}'.");
                    }
                }
                else
                {
                    // TODO - check if broadcaster is running / up
                }
            }
        }

        private async void Conferences_ConferenceRemoved(Rainbow.Model.Conference conference)
        {
            Conferences_ConferenceUpdated(conference);
        }

        private async void Conferences_ConferenceUpdated(Rainbow.Model.Conference conference)
        {
            Boolean needToUpdateConf = false;

            if (conference.Active)
            {
                if ( (_currentConferenceId == "") && IsValidConferenceInProgress(conference.Peer.Id))
                {
                    _currentConferenceId = conference.Peer.Id;
                    needToUpdateConf = true;
                }
            }
            else
            {
                if (_currentConferenceId == conference.Peer.Id)
                {
                    _currentConferenceId = "";
                    needToUpdateConf = true;
                }
            }

            if(_currentConferenceId == "")
            {
                _currentConferenceId = GetValidConferenceInProgress();

                if (_currentConferenceId != "")
                    needToUpdateConf = true;
            }
            
            if(needToUpdateConf)
            {
                ConsoleAbstraction.WriteWhite($"[{DateTime.Now:HH:mm:ss.fff}][{_credentials.UsersConfig[0].Prefix}] Conference updated:[{conference.Peer.Id}] - Active:[{conference.Active}] => Need to update configuration");

                if (conference.Active)
                    await AddOrRemoveBroadcastersFromBubbleAsync(conference.Peer.Id, true);
                else
                {
                    // TODO  need to check config
                    await AddOrRemoveBroadcastersFromBubbleAsync(conference.Peer.Id, false);
                }

                await SendOrUpdateAdaptiveCardToAllAdminAsync();

                UpdateConfigurationFileOnDisk();

                UpdateConfigurationFileOnDiskForEachBroadcaster();
            }
            else
                ConsoleAbstraction.WriteWhite($"[{DateTime.Now:HH:mm:ss.fff}][{_credentials.UsersConfig[0].Prefix}] Conference updated:[{conference.Peer.Id}] - Active:[{conference.Active}]");
        }

        private Boolean IsValidConferenceInProgress(String confId)
        {
            if (String.IsNullOrEmpty(confId)) return false;

            String result;
            var conferences = _rbConferences?.GetAllConferences().Where(conf => conf.Active) ?? [];
            if (_currentBotConfigurationExtended?.Conferences.UseAll == true)
                result = conferences?.Where(c => c?.Peer.Id == confId).FirstOrDefault()?.Peer.Id ?? "";
            else
                result = conferences?.Where(c => (_currentBotConfigurationExtended?.Conferences.Selected.Contains(c?.Peer.DisplayName ?? "") == true) && (c?.Peer.Id == confId))?.FirstOrDefault()?.Peer.Id ?? "";
            return result == confId;
        }

        private String GetValidConferenceInProgress()
        {
            String result;
            var conferences = _rbConferences?.GetAllConferences().Where(conf => conf.Active) ?? [];
            if (_currentBotConfigurationExtended?.Conferences.UseAll == true)
                result = conferences?.FirstOrDefault()?.Peer.Id ?? "";
            else
                result = conferences?.Where(c => _currentBotConfigurationExtended?.Conferences.Selected.Contains(c?.Peer.DisplayName ?? "") == true)?.FirstOrDefault()?.Peer.Id ?? "";
            return result;
        }

        private async void Bubbles_BubbleMemberUpdated(Bubble bubble, BubbleMember member)
        {
            Boolean needToUpdateConf = false;

            switch (member.Status)
            {
                case BubbleMemberStatus.Accepted:
                    needToUpdateConf = (member.Privilege == BubbleMemberPrivilege.Owner) || (member.Privilege == BubbleMemberPrivilege.Moderator);
                    break;

                case BubbleMemberStatus.Deleted:
                    needToUpdateConf = true;
                    break;
            }

            if (needToUpdateConf)
            {
                await SendOrUpdateAdaptiveCardToAllAdminAsync();

                UpdateConfigurationFileOnDisk();

                UpdateConfigurationFileOnDiskForEachBroadcaster();
            }
        }

#endregion Events from SDK

        // Method to update the Orchestrator configuration based on action set on the Adaptive Card
        // Once the configuration is done, we send/update all AC known (one for each administrator)
        private async Task UpdateConfigurationBasedOnActionOnAdaptiveCardAsync(JSONNode? jsonNode)
        {
            if ( (_currentBotConfigurationExtended is null) || (jsonNode is null)) return;

            if(jsonNode.IsObject)
            {
                foreach (var broadcaster in  _currentBotConfigurationExtended.Broadcasters)
                {
                    if (broadcaster is null) continue;

                    var login = broadcaster.Login;
                    broadcaster.InConf = jsonNode[$"join_{login}"];
                    broadcaster.AudioStreamId = jsonNode[$"audio_{login}"];
                    broadcaster.VideoStreamId = jsonNode[$"video_{login}"];
                    broadcaster.SharingStreamId = "-";
                }

                _currentBotConfigurationExtended.Broadcasters.FirstOrDefault(broadcaster => broadcaster.Login == jsonNode[$"sharing_bot"])?.SharingStreamId = jsonNode[$"sharing_stream"];

                _currentBotConfigurationExtended.Conferences.UseAll = jsonNode[$"conferences_all"] == "true";

                // We want to store bubble name (and not bubble id)
                Boolean confStillSelected = false;
                List<String> bubblesId = jsonNode[$"conferences_selected"].AsString.Split(",").ToList();
                var bubbles = GetListOfBubblesAsModerator();
                List<String> bubblesName = [];
                foreach (var bubbleId in bubblesId)
                {
                    if (bubbleId == _currentConferenceId)
                        confStillSelected = true;

                    var bubble = bubbles?.FirstOrDefault(b => b.Peer.Id == bubbleId);
                    if (bubble is not null)
                        bubblesName.Add(bubble.Peer.DisplayName);
                }
                _currentBotConfigurationExtended.Conferences.Selected = bubblesName;

                if (!confStillSelected && !_currentBotConfigurationExtended.Conferences.UseAll)
                {
                    var _ = AddOrRemoveBroadcastersFromBubbleAsync(_currentConferenceId, false);
                    _currentConferenceId = "";
                }

                _currentBotConfigurationExtended.Conferences.RemoveAsMemberBroadcaster = jsonNode[$"remove_broadcaster"] == "true";

                // Get first available conference
                if (_currentConferenceId == "")
                {
                    _currentConferenceId = GetValidConferenceInProgress();
                    if (_currentConferenceId != "")
                    {
                        var _ = AddOrRemoveBroadcastersFromBubbleAsync(_currentConferenceId, true);
                    }
                }

                UpdateConfigurationFileOnDisk();
                UpdateConfigurationFileOnDiskForEachBroadcaster();

                await SendOrUpdateAdaptiveCardToAllAdminAsync();
            }
        }

        private void UpdateAdministratorsInfoInConfig()
        {
            if (Application.IsConnected())
            {

                // We need to have full info about Administrators
                _currentBotConfigurationExtended?.Administrators?.ForEach(async (account) =>
                {
                    var contact = await GetContactAsync(account);
                    if (contact is not null)
                    {
                        account.Id = contact.Peer.Id;
                        account.Jid = contact.Peer.Jid;
                        account.FirstName = contact.FirstName;
                        account.LastName = contact.LastName;
                    }
                });


                // We need to have full info about Administrators
                _currentBotConfigurationExtended?.Broadcasters?.ForEach(async (accountAndStreamUsage) =>
                {
                    var contact = await GetContactAsync(accountAndStreamUsage);
                    if (contact is not null)
                    {
                        accountAndStreamUsage.Id = contact.Peer.Id;
                        accountAndStreamUsage.Jid = contact.Peer.Jid;
                        accountAndStreamUsage.FirstName = accountAndStreamUsage.FirstName ?? contact.FirstName;
                        accountAndStreamUsage.LastName = accountAndStreamUsage.LastName ?? contact.LastName;
                    }
                });

            }
        }

        private async Task AddOrRemoveBroadcastersFromBubbleAsync(String bubbleId, Boolean add)
        {
            if ( (_rbBubbles is null) || (_rbContacts is null) || (_currentBotConfigurationExtended is null) ) return;
            var bubble = _rbBubbles.GetBubbleById(bubbleId);
            if (bubble is null) return;

            List<String> bubbleMemberStatusList = [BubbleMemberStatus.Accepted];
            var members = _rbBubbles.GetMembers(bubble, bubbleMemberStatusList: bubbleMemberStatusList) ?? [];
            foreach (var broadcaster in _currentBotConfigurationExtended.Broadcasters)
            {
                if (broadcaster is null) continue;
                var member = members.FirstOrDefault(m => m.Peer.Id == broadcaster.Id);
                if ( (member is null) && add && broadcaster.InConf == "true")
                {
                    var contact = _rbContacts.GetContactById(broadcaster.Id);
                    if (contact is not null)
                        await _rbBubbles.AddMemberAsync(bubble, contact, BubbleMemberPrivilege.User, true);
                }
                else  if ( (!add) && _currentBotConfigurationExtended.Conferences.RemoveAsMemberBroadcaster)
                {
                    var contact = _rbContacts.GetContactById(broadcaster.Id);
                    if (contact is not null)
                        await _rbBubbles.RemoveMemberAsync(bubble, contact);
                }
            }
        }

        private (Boolean isAdaptiveCardAnswer, JSONNode? jsonNode) GetDataFromAdaptiveCardResponse(Message message)
        {
            Boolean isAdaptiveCardAnswer = false;
            JSONNode? jsonNode = null; ;
            if (message.AlternativeContent != null)
            {
                foreach (MessageAlternativeContent alternativeContent in message.AlternativeContent)
                {
                    if (alternativeContent.Type == "rainbow/json")
                    {
                        jsonNode = JSON.Parse(alternativeContent.Content);
                        if (jsonNode["rainbow"]?["value"] == _guid)
                        {
                            isAdaptiveCardAnswer = true;
                            break;
                        }
                    }

                }
            }
            return (isAdaptiveCardAnswer, jsonNode);
        }

        // To update "botConfiguration.json" file on disk with the current configuration stored in _currentBotConfigurationExtended object
        private void UpdateConfigurationFileOnDisk()
        {
            if (_currentBotConfigurationExtended is null) return;

            String botConfigurationFilePath = GetConfigurationPathFile(); // + "_copy";

            var json = new JSONObject();
            json["botConfiguration"] = _currentBotConfigurationExtended.ToJsonNode();
            var content = json.ToString(avoidNull: true, indent: true);
            try
            {
                File.WriteAllText(botConfigurationFilePath, content);
            }
            catch
            {
                ConsoleAbstraction.WriteRed($"Cannot write configuration in file '{botConfigurationFilePath}'.");
            }
        }

        private void UpdateConfigurationFileOnDiskForEachBroadcaster()
        {
            if ( (_currentBotConfigurationExtended is null) || (_rbContacts is null) || (_rbBubbles is null)) return;

            var currentContact = _rbContacts.GetCurrentContact();
            if (currentContact is null) return;

            BotConfiguration botConfiguration = new()
            {
                Administrators = 
                [
                    new()
                    {
                        Id = currentContact.Peer.Id,
                        Jid = currentContact.Peer.Jid,
                        Login = currentContact.LoginEmail,
                        FirstName = currentContact.Peer.DisplayName,
                    }
                ],
                GuestsAccepted = false,
                InstantMessageAutoAccept = false,
                PrivateMessageAutoAccept = false,
                AckMessageAutoAccept = false,
                ApplicationMessageAutoAccept = false,
                UserInvitationAutoAccept = true,
                BubbleInvitationAutoAccept = true
            };

            var jsonNode = new JSONObject();
            jsonNode["botConfiguration"] = new JSONObject();
            //var jsonNodeBotConfiguration = jsonNode["botConfiguration"];

            jsonNode["botConfiguration"] = BotConfiguration.ToJsonNode(botConfiguration);
            var jsonNodeBotConfiguration = jsonNode["botConfiguration"];
            //jsonNodeBotConfiguration["administrators"] = BotConfiguration.ToJsonNode(botConfiguration)?["administrators"];

            jsonNodeBotConfiguration["streams"] = JSON.ToJSONArray<Rainbow.Example.Common.Stream>(_currentBotConfigurationExtended.Streams, Rainbow.Example.Common.Stream.ToJsonNode);

            // Now loop on each Boradcaster to create specific configuration file for each of them
            foreach(var broadcaster in _currentBotConfigurationExtended.Broadcasters)
            {
                if (broadcaster is null) continue;
                
                // Set firstName and lastName
                jsonNodeBotConfiguration["firstName"] = broadcaster.FirstName;
                jsonNodeBotConfiguration["lastName"] = broadcaster.LastName;

                // Create "conferences" node
                jsonNodeBotConfiguration["conferences"] = new JSONArray();

                var jsonConference = new JSONObject();
                jsonNodeBotConfiguration["conferences"].Add(jsonConference);

                var bubbleWithConfInProgress = _rbBubbles.GetBubbleById(_currentConferenceId);

                if (broadcaster.SharingStreamId == "") broadcaster.SharingStreamId = "-";

                if ( ( (broadcaster.InConf == "true") || (broadcaster.SharingStreamId != "-")) && (bubbleWithConfInProgress is not null))
                {
                    jsonConference["id"] = bubbleWithConfInProgress.Peer.Id;
                    jsonConference["jid"] = bubbleWithConfInProgress.Peer.Jid;
                    jsonConference["name"] = bubbleWithConfInProgress.Peer.DisplayName;

                    if (broadcaster.InConf == "true")
                    {
                        jsonConference["audioStreamId"] = (broadcaster.AudioStreamId == "-") ? "" : broadcaster.AudioStreamId;
                        jsonConference["videoStreamId"] = (broadcaster.VideoStreamId == "-") ? "" : broadcaster.VideoStreamId;
                    }
                    else
                    {
                        jsonConference["audioStreamId"] = "";
                        jsonConference["videoStreamId"] = "";
                    }
                    jsonConference["sharingStreamId"] = (broadcaster.SharingStreamId == "-") ? "" : broadcaster.SharingStreamId;
                }
                else
                {
                    jsonConference["name"] = ""; // Set an empty name

                    jsonConference["audioStreamId"] = "";
                    jsonConference["videoStreamId"] = "";
                    jsonConference["sharingStreamId"] = "";
                }

                String broadcasterConfigFilePath = GetBroadcasterConfigurationPathFile(broadcaster.Id);
                var content = jsonNode.ToString(avoidNull: true, indent: true);
                try
                {
                    File.WriteAllText(broadcasterConfigFilePath, content);

                    if (_broadcasters.TryGetValue(broadcaster.Id, out var botBroadcaster) && botBroadcaster is not null)
                    {
                        //if(botBroadcaster.Application.IsConnected())
                        {
                            AsyncHelper.RunSync(async () => await botBroadcaster.BotConfigurationUpdatedAsync(new BotConfigurationUpdate(jsonNode["botConfiguration"], "reload", null)).ConfigureAwait(false));
                        }
                    }
                }
                catch
                {
                    ConsoleAbstraction.WriteRed($"Cannot write configuration in file '{broadcasterConfigFilePath}'.");
                }
            }
        }

        static private String GetBroadcasterConfigurationPathFile(String id) => $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}broadcaster-{id}.json";

        static private String GetConfigurationPathFile() => $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}botConfiguration.json";

        static private String GetACPathFile() => $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}last_AC_sent.json";

        internal async Task<Boolean> ReloadConfigurationFile()
        {
            if (Application.IsConnected())
            {
                ConsoleAbstraction.WriteRed($"{Rainbow.Util.CR}=> ReloadConfigurationFile{Rainbow.Util.CR}");

                String botConfigurationFilePath = GetConfigurationPathFile();
                if (!File.Exists(botConfigurationFilePath))
                {
                    ConsoleAbstraction.WriteRed($"The file '{botConfigurationFilePath}' has not been found.");
                    return false;
                }

                String jsonConfig = File.ReadAllText(botConfigurationFilePath);
                var jsonNode = JSON.Parse(jsonConfig);
                if (jsonNode?["botConfiguration"]?.IsObject != true)
                {
                    ConsoleAbstraction.WriteRed($"Cannot get JSON object 'botConfiguration' from file '{botConfigurationFilePath}'.");
                    return false;
                }
                var jsonNodeBotConfiguration = jsonNode["botConfiguration"];

                await BotConfigurationUpdatedAsync(new BotConfigurationUpdate(jsonNodeBotConfiguration, "reload", null));
                return true;
            }
            return false;
        }

        private async Task SendOrUpdateAdaptiveCardToAllAdminAsync()
        {
            await _semaphoreSendOrUpdateAdaptiveCard.WaitAsync();

            try
            {

                if ((_rbInstantMessaging is null) || (!Application.IsConnected())) return;

                var (body, alternativeContent) = CreateAdaptiveCard();

                // Store last AD sent:
                var acPathFile = GetACPathFile();
                try
                {
                    File.WriteAllText(acPathFile, alternativeContent?[0]?.Content);
                }
                catch { }

                // Send AC to all administrators
                _currentBotConfigurationExtended?.Administrators?.ForEach(async administrator =>
                {
                    Contact? contact = await GetContactAsync(administrator);
                    if (contact is not null)
                    {
                        SdkResult<Message> sdkResult;

                        // We edit previous message if it exists
                        if (_messageByAdminId.TryGetValue(contact.Peer.Id, out Message? message) && (message is not null))
                        {
                            sdkResult = await _rbInstantMessaging.EditMessageAsync(message, body, alternativeContent);
                        }
                        else
                        {
                            sdkResult = await _rbInstantMessaging.SendMessageWithAlternativeContentsAsync(contact, body, alternativeContent);
                        }

                        if (sdkResult.Success)
                        {
                            _messageByAdminId[contact.Peer.Id] = sdkResult.Data;
                        }
                    }
                });
            }
            finally
            {
                try
                {
                    _semaphoreSendOrUpdateAdaptiveCard?.Release();
                }
                catch { }
            }
        }

        static private String RemoveAllComments(String? json)
        {
            if (json is null)
                return "";

            String result = "";
            String[] lines = json.Split([ System.Environment.NewLine ], StringSplitOptions.None);
            foreach (String line in lines)
            {
                if(line.StartsWith("//")) // This line is a comment, we ignore it
                    continue;
                result += line.Trim();
            }
            return result;
        }

        private List<Bubble> GetListOfBubblesAsModerator()
        {
            List<String> memberStatus = [ BubbleMemberStatus.Accepted ];
            List<String> bubbleMemberPrivilegeList = [ BubbleMemberPrivilege.Owner, BubbleMemberPrivilege.Moderator];
            return _rbBubbles?.GetAllBubbles(memberStatus, bubbleMemberPrivilegeList) ?? [];
        }

        private String? AC_CreateData()
        {
            // /!\ No Boolean values here ! Need to use string as "true" or "false" (to lower => case sensitive)

            if (_currentBotConfigurationExtended is null) return null;

            var bubblesList = GetListOfBubblesAsModerator();
            Bubble? currentConference = _rbBubbles?.GetBubbleById(_currentConferenceId);

            var jsonObject = new JSONObject();

            // Manage "labels" node
            String labelFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}{_currentBotConfigurationExtended.Labels}";
            String? jsonLabels;
            if (File.Exists(labelFilePath))
                 jsonLabels = File.ReadAllText(labelFilePath);
            else // Use labels in embedded resource as default value
                jsonLabels = Helper.GetContentOfEmbeddedResource("labels_EN.json", System.Text.Encoding.UTF8);

            var jsonNodeLabels = 
            jsonObject = (JSONObject) JSON.Parse(jsonLabels);

            // Manage "Conferences" node
            var jsonConferences = new JSONObject();
            jsonConferences["useAll"] = _currentBotConfigurationExtended.Conferences.UseAll.ToString().ToLower();
            jsonConferences["removeBroadcasterAsMember"] = _currentBotConfigurationExtended.Conferences.RemoveAsMemberBroadcaster.ToString().ToLower();

            // Need to set bubble id in this JSON and not name
            List<String> bubblesId = [];
            foreach(var bubbleName in _currentBotConfigurationExtended.Conferences.Selected)
            {
                var bubble = bubblesList.FirstOrDefault(b => b.Peer.DisplayName.Equals(bubbleName, StringComparison.InvariantCultureIgnoreCase));
                if (bubble is not null)
                    bubblesId.Add(bubble.Peer.Id);
                else
                {
                    bubble = bubblesList.FirstOrDefault(b => b.Peer.Id.Equals(bubbleName, StringComparison.InvariantCultureIgnoreCase));
                    if (bubble is not null)
                        bubblesId.Add(bubble.Peer.Id);
                }
            }
            jsonConferences["selected"] = String.Join(",", bubblesId);

            var jsonCurrent = new JSONObject();
            jsonCurrent["name"] = currentConference?.Peer?.DisplayName ?? "";
            jsonCurrent["inProgress"] = (currentConference is not null).ToString().ToLower();
            jsonConferences["current"] = jsonCurrent;

            var jsonData = new JSONArray();
            foreach(var bubble in bubblesList)
            {
                if(bubble is null) continue;

                var jsonBubble = new JSONObject();
                jsonBubble["id"] = bubble.Peer.Id;
                jsonBubble["name"] = bubble.Peer.DisplayName;

                jsonData.Add(jsonBubble);
            }
            jsonConferences["data"] = jsonData;
            jsonObject["conferences"] = jsonConferences;

            // Manage "sharingStream" node
            var jsonSharingStream = new JSONObject();

            String broadcasterId = "";
            String streamId = "";
            var sharingBroadcaster =  _currentBotConfigurationExtended.Broadcasters.FirstOrDefault(b => (!String.IsNullOrEmpty(b.SharingStreamId)) && (b.SharingStreamId != "-"));
            if(sharingBroadcaster is not null)
            {
                broadcasterId = sharingBroadcaster.Login;
                streamId = sharingBroadcaster.SharingStreamId;
            }

            if(String.IsNullOrEmpty(broadcasterId))
                broadcasterId = _currentBotConfigurationExtended.Broadcasters[0].Login;

            if (String.IsNullOrEmpty(streamId))
                streamId = "-";

            jsonSharingStream["broadcasterId"] = broadcasterId;
            jsonSharingStream["streamId"] = streamId;
            jsonObject["sharingStream"] = jsonSharingStream;

            // Manage "broadcasters" node
            var jsonBroadcasters = new JSONArray();
            foreach(var broadcaster in _currentBotConfigurationExtended.Broadcasters)
            {
                if(broadcaster is null) continue;
                var jsonBroadcaster = new JSONObject();
                jsonBroadcaster["id"] = broadcaster.Login;
                jsonBroadcaster["name"] = broadcaster.LastName + " " + broadcaster.FirstName;
                jsonBroadcaster["inConf"] = broadcaster.InConf;

                if(String.IsNullOrEmpty(broadcaster.AudioStreamId))
                    jsonBroadcaster["audioStreamId"] = "-";
                else
                    jsonBroadcaster["audioStreamId"] = broadcaster.AudioStreamId;

                if (String.IsNullOrEmpty(broadcaster.VideoStreamId))
                    jsonBroadcaster["videoStreamId"] = "-";
                else
                    jsonBroadcaster["videoStreamId"] = broadcaster.VideoStreamId;

                jsonBroadcasters.Add(jsonBroadcaster);
            }
            jsonObject["broadcasters"] = jsonBroadcasters;

            // Manage "streams" node
            jsonObject["streams"] = JSON.ToJSONArray<Rainbow.Example.Common.Stream>(_currentBotConfigurationExtended.Streams, Rainbow.Example.Common.Stream.ToJsonNode);

            var jsonStreams = new JSONObject();
            
            // Define default stream info
            var jsonStreamInfo = new JSONObject();
            jsonStreamInfo["title"] = "-";
            jsonStreamInfo["value"] = "-";

            var jsonWithAudio = new JSONArray();
            jsonWithAudio.Add(jsonStreamInfo);
            _currentBotConfigurationExtended.Streams.Where(stream => stream.Media.StartsWith("audio")).ToList().ForEach(stream =>
            {
                var jsonStreamInfo = new JSONObject();
                jsonStreamInfo["title"] = stream.Id;
                jsonStreamInfo["value"] = stream.Id;
                jsonWithAudio.Add(jsonStreamInfo);
            });
            jsonStreams["withAudio"] = jsonWithAudio;

            var jsonWithVideo = new JSONArray();
            jsonWithVideo.Add(jsonStreamInfo);
            _currentBotConfigurationExtended.Streams.Where(stream => stream.Media.Contains("video") || stream.Media.Contains("composition")).ToList().ForEach(stream =>
            {
                var jsonStreamInfo = new JSONObject();
                jsonStreamInfo["title"] = stream.Media.Contains("composition") ? "🧱" + stream.Id : stream.Id;
                jsonStreamInfo["value"] = stream.Id;
                jsonWithVideo.Add(jsonStreamInfo);
            });
            jsonStreams["withVideo"] = jsonWithVideo;


            jsonObject["streams"] = jsonStreams;

            // Create JSON String with indentation and without null value (if any)
            String result = jsonObject.ToString(avoidNull: true, indent: true);
            return result;
        }

        private String AC_CreateTableWithBroadcasters()
        {
            if (_currentBotConfigurationExtended is null) return "";

            String? jsonTemplate = Helper.GetContentOfEmbeddedResource("AC_TableRow_Broadcaster.json", System.Text.Encoding.UTF8);
            if(jsonTemplate is null) return "";
            jsonTemplate = RemoveAllComments(jsonTemplate);

            String result = "";
            for(int i = 0; i < _currentBotConfigurationExtended.Broadcasters.Count; i++)
            {
                result += jsonTemplate.Replace("$ID$", i.ToString());
                if (i < _currentBotConfigurationExtended.Broadcasters.Count - 1)
                    result += ",";
                result += System.Environment.NewLine;
            }
            return result;
        }

        private String AC_CreateStreamSelection()
        {
            if (_currentBotConfigurationExtended is null) return "";

            String? jsonData = AC_CreateData();
            if (jsonData is null) return "";

            String? jsonTemplate = Helper.GetContentOfEmbeddedResource("AC_StreamsSelection.json", System.Text.Encoding.UTF8);
            if (jsonTemplate is null) return "";
            jsonTemplate = RemoveAllComments(jsonTemplate);

            var broadcastersTable = AC_CreateTableWithBroadcasters();
            jsonTemplate = jsonTemplate.Replace("$BROADCASTERS$", broadcastersTable);
            jsonTemplate = jsonTemplate.Replace("$GUID$", _guid); // add GUID to ensure we will manage only AC create by this Orchestrator

            // Create a Template instance from the template payload
            AdaptiveCardTemplate template = new(jsonTemplate);

            // "Expand" the template - this generates the final Adaptive Card payload
            string cardJson = template.Expand(jsonData);
            return cardJson;
        }

        private (String? message, List<MessageAlternativeContent>? alternativeContent) CreateAdaptiveCard()
        {
            var cardJson = AC_CreateStreamSelection();

            // Create an Message Alternative Content
            MessageAlternativeContent messageAlternativeContent = new()
            {
                Type = "form/json",
                Content = cardJson
            };

            List<MessageAlternativeContent> alternativeContent = [messageAlternativeContent];
            String message = " "; // Need to be not null / empty

            return (message, alternativeContent);
        }

        // TO USE
        private static (String? message, List<MessageAlternativeContent>? alternativeContent) CreateExampleAdaptiveCard()
        {
            String? message = null;
            List<MessageAlternativeContent>? alternativeContent = null;

            // TO TEST AD
            String? jsonTemplate = Helper.GetContentOfEmbeddedResource("AC_Example_StreamsSelection.json", System.Text.Encoding.UTF8);
            jsonTemplate = RemoveAllComments(jsonTemplate);
            String? jsonData = Helper.GetContentOfEmbeddedResource($"AC_Example_Data.json", System.Text.Encoding.UTF8);

            // Create a Template instance from the template payload
            AdaptiveCardTemplate template = new (jsonTemplate);

            // "Expand" the template - this generates the final Adaptive Card payload
            string cardJson = template.Expand(jsonData);

            // Create an Message Alternative Content
            MessageAlternativeContent messageAlternativeContent = new()
            {
                Type = "form/json",
                Content = cardJson
            };

            alternativeContent = [messageAlternativeContent];
            message = " "; // Need to be not null / empty

            return (message, alternativeContent);
        }

        private async Task<Contact?> GetContactAsync(Account account)
        {
            if (_rbContacts is null)
                return null;

            Contact? result = null;
            if(!String.IsNullOrEmpty(account.Id))
                result = await _rbContacts.GetContactByIdInCacheFirstAsync(account.Id);

            if (result is not null)
                return result;

            if (!String.IsNullOrEmpty(account.Jid))
                result = await _rbContacts.GetContactByJidInCacheFirstAsync(account.Jid);

            if (result is not null)
                return result;

            if (!String.IsNullOrEmpty(account.Login))
                result = _rbContacts.GetAllContacts()?.Find(contact => contact?.LoginEmail?.Equals(account.Login, StringComparison.InvariantCultureIgnoreCase) == true);

            return result;
        }
        
    }
}
