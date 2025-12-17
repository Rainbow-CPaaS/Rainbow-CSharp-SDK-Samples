using BotBroadcaster.Model;
using BotLibrary.Model;
using Rainbow;
using Rainbow.Example.Common;
using Rainbow.Medias;
using Rainbow.Model;
using Rainbow.WebRTC;
using Rainbow.WebRTC.Abstractions;
using Rainbow.WebRTC.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Util = Rainbow.Example.Common.Util;

namespace BotBroadcaster
{
    public class BotBroadcaster: BotLibrary.BotBase
    {
        Task RbTask = Task.CompletedTask;

        Conferences? _rbConferences = null;
        Contacts? _rbContacts = null;
        Bubbles? _rbBubbles = null;

        WebRTCCommunications? _rbWebRTCCommunications = null;
        WebRTCFactory? _rbWebRTCDesktopFactory = null;

        Object _lockBotConfiguration = new();
        
        BotConfigurationUpdate? _currentBotConfigurationUpdate = null;
        BotConfigurationExtended? _currentBotConfigurationExtended = null;
        BotConfigurationUpdate? _nextBotConfigurationUpdate = null;

        Timer? _botConfigurationTimer = null;

        List<String> conferencesInProgress = [];

        Dictionary<string, Call> currentCallsList = new();

        List<StreamAndDevice> _listConnectedDevices = []; // To know the relation between Streams (defined in config) and Device used to connect to the remote stream

        // Method used to update the brodcast configuration
        private void UpdateBroadcastConfiguration()
        {
            // Create timer (if necessary)
            _botConfigurationTimer ??= new Timer(
                                _state => CheckBroadcastConfiguration(), null,
                                Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            lock (_lockBotConfiguration)
            {
                if (_nextBotConfigurationUpdate is not null)
                {
                    _currentBotConfigurationUpdate = _nextBotConfigurationUpdate;
                    _nextBotConfigurationUpdate = null;

                    if(_currentBotConfigurationUpdate is not null)
                        BotConfigurationExtended.FromJsonNode(_currentBotConfigurationUpdate.JSONNodeBotConfiguration, out _currentBotConfigurationExtended);
                }
                else
                {
                    // Current configuration has not been taken into account yet.
                    // We don't continue;
                }
            }

            // Do we have a configuration to set
            if (_currentBotConfigurationUpdate is null)
                return;

            if(_currentBotConfigurationUpdate.Context == "configFile")
                CreateWebRTCEnvironment();


            // CheckBroadcastConfiguration 
            _botConfigurationTimer?.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
        }

        private Boolean IsNewConfigAvailable()
        {
            return _nextBotConfigurationUpdate is not null;
        }


        public Boolean broadcastConfigurationMustBeCheckedAgain = false;

        private void CheckBroadcastConfiguration()
        {
            Boolean result = true;

            if (!RbTask.IsCompleted)
            {
                broadcastConfigurationMustBeCheckedAgain = (currentCallsList.Count == 0);
                return;
            }
            

            Action action = async () =>
            {
                if (IsNewConfigAvailable())
                {
                    UpdateBroadcastConfiguration();
                    return;
                }

                // Ensure to set id / jid for conference if it's not the case
                UpdateConferenceSettings();

                if (currentCallsList.Count > 0)
                {
                    foreach(var call in currentCallsList.Values.ToList())
                    {
                        // STEPS:
                        // 1 - Quit current conference if necessary
                        // 2 - Join conference if necessary
                        // 3 - Connect to necessary medias (connected = true) or used in conference
                        // 4 - Add/Update medias in this order: audio, video and sharing (if possible)
                        // NOTE: Between each steps, check if a new config is available

                        if ((call.CallStatus == Rainbow.Enums.CallStatus.CONNECTING)
                            || (call.CallStatus == Rainbow.Enums.CallStatus.UNKNOWN))
                            continue;

                        result = await QuitConferenceIfNecessaryAsync(call);

                        if (IsNewConfigAvailable())
                            continue;

                        if (result)
                            result = await JoinConferenceIfNecessaryAsync();

                        if (IsNewConfigAvailable())
                            continue;

                        if (result)
                            result = await ConnectOrDisconnectRemoteMediasAsync(call);
                    }
                }
                else
                {
                    await JoinConferenceIfNecessaryAsync();
                    await ConnectOrDisconnectRemoteMediasAsync(null);
                }
            };

            RbTask = Task.Run(action).ContinueWith(task =>
            {
                if (IsNewConfigAvailable())
                    UpdateBroadcastConfiguration();

                if (result && !broadcastConfigurationMustBeCheckedAgain)
                    _botConfigurationTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                else
                {
                    broadcastConfigurationMustBeCheckedAgain = false;
                    _botConfigurationTimer?.Change(TimeSpan.FromMilliseconds(200), Timeout.InfiniteTimeSpan);
                }
            });
        }

        private Bubble? GetBubble(string id, string jid, string name)
        {
            if (_rbBubbles is null)
                return null;

            Bubble? result = null;

            result = _rbBubbles.GetBubbleById(id);
            result ??= _rbBubbles.GetBubbleByJid(jid);
            result ??= _rbBubbles.GetAllBubbles().Find(b => b.Peer.DisplayName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return result;
        }

        private void UpdateConferenceSettings()
        {
            if (_currentBotConfigurationExtended?.Conferences?.Count > 0)
            {
                foreach (var conference in _currentBotConfigurationExtended.Conferences)
                {
                    Bubble? bubble = GetBubble(conference.Id, conference.Jid, conference.Name);
                    if (bubble is null)
                        continue;
                    
                    // If a bubble has been found, about the settings
                    conference.Id = bubble.Peer.Id;
                    conference.Jid = bubble.Peer.Jid;
                    conference.Name = bubble.Peer.DisplayName;
                }
            }
        }

        private async Task<Boolean> QuitConferenceIfNecessaryAsync(Call call)
        {
            if (_rbWebRTCCommunications is null) return false;

            if (call is not null)
            {
                Boolean needToQuit = true;
                if (_currentBotConfigurationExtended?.Conferences?.Count > 0)
                {
                    foreach(var conference in _currentBotConfigurationExtended.Conferences)
                    {
                        if(conference.Id == call.Id)
                        {
                            needToQuit = false;
                            break;
                        }
                    }
                }

                if (needToQuit)
                {
                    var sdkResult = await _rbWebRTCCommunications.HangUpCallAsync(call.Id);
                    return (sdkResult.Success);
                }
            }
            return true;
        }

        private async Task<Boolean> JoinConferenceIfNecessaryAsync()
        {
            if ( (_rbWebRTCCommunications is null) || (_rbWebRTCDesktopFactory is null) ) return false;

            if (_currentBotConfigurationExtended?.Conferences?.Count > 0)
            {
                foreach(var conference in _currentBotConfigurationExtended.Conferences)
                {
                    if(conference?.Id is not null)
                    {
                        if (conferencesInProgress.Contains(conference.Id) && !currentCallsList.ContainsKey(conference.Id))
                        {
                            // TODO - manage presence
                            _rbContacts?.SavePresenceForRollback();

                            // Always join with Audio Empty Track
                            var emptyAudioTrack = _rbWebRTCDesktopFactory.CreateEmptyAudioTrack();
                            var sdkResult = await _rbWebRTCCommunications.JoinConferenceAsync(conference.Id, emptyAudioTrack);
                            return (sdkResult.Success);
                        }
                    }
                }
            }
            return true;
        }

        private async Task<Boolean> ConnectOrDisconnectRemoteMediasAsync(Call? call)
        {
            if ((_rbWebRTCCommunications is null) || (_rbWebRTCDesktopFactory is null)) return false;

            // TODO - Later, needs to manage composition too

            // From configuration get streams to connect
            List<Stream>? allStreamsToConnect = null;
            if (_currentBotConfigurationExtended?.Streams is not null && call is not null)
            {
                allStreamsToConnect = _currentBotConfigurationExtended?.Streams.Values.Where(s => s.Connected).ToList();
                allStreamsToConnect ??= [];

                var conference = _currentBotConfigurationExtended?.Conferences?.FirstOrDefault(c => c.Id == call.Id);

                if (conference is not null)
                {
                    var audioStream = _currentBotConfigurationExtended?.Streams.Values.FirstOrDefault(s => s.Id == conference.AudioStreamId);
                    var videoStream = _currentBotConfigurationExtended?.Streams.Values.FirstOrDefault(s => s.Id == conference.VideoStreamId);
                    var sharingStream = _currentBotConfigurationExtended?.Streams.Values.FirstOrDefault(s => s.Id == conference.SharingStreamId);

                    if (audioStream is not null)
                    {
                        if ( (videoStream is not null) && (videoStream.Id == audioStream.Id))
                            audioStream.MediaInConference = "audio+video";
                        else
                            audioStream.MediaInConference = "audio";
                        allStreamsToConnect.RemoveAll(s => s.Id == audioStream.Id);
                        allStreamsToConnect.Add(audioStream);
                    }

                    if (videoStream is not null)
                    {
                        if ((audioStream is not null) && (videoStream.Id == audioStream.Id))
                        {
                            // Nothing to do here - done previously
                        }
                        else
                        {
                            videoStream.MediaInConference = "video";
                            allStreamsToConnect.RemoveAll(s => s.Id == videoStream.Id);
                            allStreamsToConnect.Add(videoStream);
                        }
                    }

                    if (sharingStream is not null)
                    {
                        sharingStream.MediaInConference = "sharing";
                        allStreamsToConnect.RemoveAll(s => s.Id == sharingStream.Id);
                        allStreamsToConnect.Add(sharingStream);
                    }
                }
            }

            // Add new streams
            if (allStreamsToConnect?.Count > 0)
            {
                foreach (var newStream in allStreamsToConnect)
                {
                    // Check if this stream is already known
                    bool known = false;
                    foreach (var streamAndDeviceConnected in _listConnectedDevices)
                    {
                        known = (streamAndDeviceConnected.Stream?.IsSame(newStream) == true);
                        if(known) break;
                    }

                    if (!known)
                    {
                        // Before we add it, we need first to set MustBeRemovedFromConference for previous items

                        // for Audio
                        if(newStream.MediaInConference?.Contains("audio") == true)
                        {
                            _listConnectedDevices.ForEach(streamAndDevice =>
                            {  
                                if(streamAndDevice.UsedInConference && streamAndDevice.Stream?.MediaInConference?.Contains("audio") == true)
                                {
                                    Util.WriteGreen($"[{BotName}] Audio Stream must be removed from conference - Id:[{streamAndDevice.Stream.Id}] - Uri:[{streamAndDevice.Stream.Uri}]");
                                    streamAndDevice.MustBeRemovedFromConference = true;
                                }
                            });
                        }

                        // for Video
                        if (newStream.MediaInConference?.Contains("video") == true)
                        {
                            _listConnectedDevices.ForEach(streamAndDevice =>
                            {
                                if (streamAndDevice.UsedInConference && streamAndDevice.Stream?.MediaInConference?.Contains("video") == true)
                                {
                                    Util.WriteGreen($"[{BotName}] Video Stream must be removed from conference - Id:[{streamAndDevice.Stream.Id}] - Uri:[{streamAndDevice.Stream.Uri}]");
                                    streamAndDevice.MustBeRemovedFromConference = true;
                                }
                            });
                        }

                        // for sharing
                        if (newStream.MediaInConference?.Contains("sharing") == true)
                        {
                            _listConnectedDevices.ForEach(streamAndDevice =>
                            {
                                if (streamAndDevice.UsedInConference && streamAndDevice.Stream?.MediaInConference?.Contains("sharing") == true)
                                {
                                    Util.WriteGreen($"[{BotName}] Sharing Stream must be removed from conference - Id:[{streamAndDevice.Stream.Id}] - Uri:[{streamAndDevice.Stream.Uri}]");
                                    streamAndDevice.MustBeRemovedFromConference = true;
                                }
                            });
                        }

                        _listConnectedDevices.Add(new StreamAndDevice(newStream, null));
                    }
                }
            }

            // Update list to know which Stream must be removed
            foreach (var streamAndDeviceConnected in _listConnectedDevices)
            {
                bool found = false;
                if (allStreamsToConnect?.Count > 0)
                {
                    // Check if this stream is still used
                    foreach (var newStream in allStreamsToConnect)
                    {
                        found = (streamAndDeviceConnected.Stream?.IsSame(newStream) == true);
                        if (found) break;
                    }
                }

                if(!found)
                {
                    streamAndDeviceConnected.MustBeRemovedFromConference = true;
                    streamAndDeviceConnected.MustBeDeleted = true;
                }
            }

            // Display info
            if (_listConnectedDevices.Count > 0)
            {
                Util.WriteGreen($"[{BotName}] Display Info about StreamAndDevice:");
                foreach (var streamAndDevice in _listConnectedDevices)
                {
                    Util.WriteGreen($"[{BotName}] Id:[{streamAndDevice.Stream.Id}] - MediaInConference[{streamAndDevice.Stream.MediaInConference}] - InConf:[{streamAndDevice.UsedInConference}] - ToRemove:[{streamAndDevice.MustBeRemovedFromConference}] - ToDelet:[{streamAndDevice.MustBeDeleted}]");
                }
            }

            // From the list: try to connect to new stream
            foreach (var streamAndDevice in _listConnectedDevices)
            {
                if (streamAndDevice.Stream is null) continue;
                if (streamAndDevice.MustBeDeleted) continue;

                if ( (streamAndDevice.Device is null) || (streamAndDevice.VideoTrack is null) )
                {
                    Boolean withAudio = streamAndDevice.Stream.Media.Contains("audio", StringComparison.InvariantCultureIgnoreCase);
                    Boolean withVideo = streamAndDevice.Stream.Media.Contains("video", StringComparison.InvariantCultureIgnoreCase);
                    // TODO - manage composition

                    if (streamAndDevice.Device is null)
                        streamAndDevice.Device = new InputStreamDevice(streamAndDevice.Stream.Id, streamAndDevice.Stream.Id, streamAndDevice.Stream.Uri, withVideo, withAudio, options: streamAndDevice.Stream.UriSettings);

                    if (streamAndDevice is null) continue;

                    // TODO - manage several tentatives
                    // Create TRACK
                    if (withAudio && withVideo)
                    {
                        // TODO - audio and video with same stream
                        try
                        {
                            (streamAndDevice.AudioTrack, streamAndDevice.VideoTrack) = _rbWebRTCDesktopFactory.CreateAudioAndVideoTrack(streamAndDevice.Device, false, forceLiveStream: streamAndDevice.Stream.ForceLiveStream);
                            Util.WriteGreen($"[{BotName}] Connection done - Id:[{streamAndDevice.Stream.Id}] - Uri:[{streamAndDevice.Stream.Uri}] - Media:[{streamAndDevice.Stream.Media}]");
                        }
                        catch (Exception e)
                        {
                            Util.WriteRed($"[{BotName}] Connection CANNOT BE DONE - Id:[{streamAndDevice.Stream.Id}] - Uri:[{streamAndDevice.Stream.Uri}] - Media:[{streamAndDevice.Stream.Media}] - Exception:[{e}]");
                        }
                    }
                    else if (withAudio)
                    {
                        try
                        {
                            streamAndDevice.VideoTrack = _rbWebRTCDesktopFactory.CreateAudioTrack(streamAndDevice.Device);
                            Util.WriteGreen($"[{BotName}] Connection done - Id:[{streamAndDevice.Stream.Id}] - Uri:[{streamAndDevice.Stream.Uri}] - Media:[{streamAndDevice.Stream.Media}]");

                        }
                        catch (Exception e)
                        {
                            Util.WriteRed($"[{BotName}] Connection CANNOT BE DONE - Id:[{streamAndDevice.Stream.Id}] - Uri:[{streamAndDevice.Stream.Uri}] - Media:[{streamAndDevice.Stream.Media}] - Exception:[{e}]");
                        }
                    }
                    else
                    {
                        try
                        {
                            streamAndDevice.VideoTrack = _rbWebRTCDesktopFactory.CreateVideoTrack(streamAndDevice.Device, forceLiveStream: streamAndDevice.Stream.ForceLiveStream);
                            Util.WriteGreen($"[{BotName}] Connection done - Id:[{streamAndDevice.Stream.Id}] - Uri:[{streamAndDevice.Stream.Uri}] - Media:[{streamAndDevice.Stream.Media}]");
                        }
                        catch (Exception e)
                        {
                            Util.WriteRed($"[{BotName}] Connection CANNOT BE DONE - Id:[{streamAndDevice.Stream.Id}] - Uri:[{streamAndDevice.Stream.Uri}] - Media:[{streamAndDevice.Stream.Media}] - Exception:[{e}]");
                        }
                    }
                }
            }


            // TODO - add, remove, change medias in conf
            if(call?.IsActive() == true)
            {
                SdkResult<Boolean> sdkResultBoolean;

                //////////////////////////
                //
                // Check by media - AUDIO - START
                //
                var audioStreamAndDevices = _listConnectedDevices.FindAll(streamAndDevice =>
                            (streamAndDevice.AudioTrack is not null)
                            && (streamAndDevice.Stream?.MediaInConference?.Contains("audio", StringComparison.InvariantCultureIgnoreCase) == true));

                // Get stream details
                var streamAndDeviceNewOneToUse = audioStreamAndDevices.FirstOrDefault(s => (!s.UsedInConference) && (!s.MustBeRemovedFromConference));
                var streamAndDevicesCurrentlyUsed = audioStreamAndDevices.FindAll(s => (s.UsedInConference) && (!s.MustBeRemovedFromConference));
                var streamAndDevicesCurrentlyUsedToRemove = audioStreamAndDevices.FindAll(s => (s.UsedInConference) && (s.MustBeRemovedFromConference));

                // Do we have already a video ?
                if (call is not null && Rainbow.Util.MediasWithAudio(call.LocalMedias))
                {
                    if (audioStreamAndDevices?.Count > 0)
                    {
                        if (streamAndDeviceNewOneToUse is not null)
                        {
                            Util.WriteGreen($"[{BotName}] Change Audio in conf - Id:[{streamAndDeviceNewOneToUse.Stream.Id}]");

                            sdkResultBoolean = await _rbWebRTCCommunications.ChangeAudioAsync(call.Id, streamAndDeviceNewOneToUse.AudioTrack as IAudioStreamTrack);
                            if (sdkResultBoolean.Success)
                            {
                                streamAndDeviceNewOneToUse.UsedInConference = true;
                                streamAndDevicesCurrentlyUsed.ForEach(s => { s.UsedInConference = false; s.MustBeRemovedFromConference = true; }); // /!\ Shoud contains none
                                streamAndDevicesCurrentlyUsedToRemove?.ForEach(s => { s.UsedInConference = false; s.MustBeRemovedFromConference = true; });
                            }
                        }
                        else if ((streamAndDevicesCurrentlyUsed.Count == 0) || (streamAndDevicesCurrentlyUsedToRemove.Count != 0))
                        {
                            //Util.WriteGreen($"[{BotName}] Remove Audio in conf - no stream to use");
                            Util.WriteRed($"[{BotName}] Remove Audio in conf - no stream to use");
                            //sdkResultBoolean = await _rbWebRTCCommunications.RemoveVideoAsync(call.Id);
                        }
                        else
                        {
                            Util.WriteGreen($"[{BotName}] Use same Audio in conf");
                        }
                    }
                    else
                    {
                        Util.WriteGreen($"[{BotName}] No Audio in conf");
                        //sdkResultBoolean = await _rbWebRTCCommunications.RemoveVideoAsync(call.Id);
                    }
                }
                else
                {
                   // Thre is always AUDIO ....
                }
                //
                // Check by media - AUDIO - END
                //
                //////////////////////////


                //////////////////////////
                //
                // Check by media - VIDEO - START
                //
                var videoStreamAndDevices = _listConnectedDevices.FindAll(streamAndDevice =>
                            (streamAndDevice.VideoTrack is not null)
                            && ( streamAndDevice.Stream?.MediaInConference?.Contains("video", StringComparison.InvariantCultureIgnoreCase) == true) );

                // Get stream details
                streamAndDeviceNewOneToUse = videoStreamAndDevices.FirstOrDefault(s => (!s.UsedInConference) && (!s.MustBeRemovedFromConference));
                streamAndDevicesCurrentlyUsed = videoStreamAndDevices.FindAll(s => (s.UsedInConference) && (!s.MustBeRemovedFromConference));
                streamAndDevicesCurrentlyUsedToRemove = videoStreamAndDevices.FindAll(s => (s.UsedInConference) && (s.MustBeRemovedFromConference));

                // Do we have already a video ?
                if (call is not null && Rainbow.Util.MediasWithVideo(call.LocalMedias))
                {
                    if (videoStreamAndDevices?.Count > 0)
                    {
                        if (streamAndDeviceNewOneToUse is not null)
                        {
                            Util.WriteGreen($"[{BotName}] Change Video in conf - Id:[{streamAndDeviceNewOneToUse.Stream.Id}]");

                            sdkResultBoolean = await _rbWebRTCCommunications.ChangeVideoAsync(call.Id, streamAndDeviceNewOneToUse.VideoTrack as IVideoStreamTrack);
                            if(sdkResultBoolean.Success)
                            {
                                streamAndDeviceNewOneToUse.UsedInConference = true;
                                streamAndDevicesCurrentlyUsed.ForEach(s => { s.UsedInConference = false; s.MustBeRemovedFromConference = true; }); // /!\ Shoud contains none
                                streamAndDevicesCurrentlyUsedToRemove?.ForEach(s => { s.UsedInConference = false; s.MustBeRemovedFromConference = true; });
                            }
                        }
                        else if ( (streamAndDevicesCurrentlyUsed.Count == 0) || (streamAndDevicesCurrentlyUsedToRemove.Count != 0))
                        {
                            Util.WriteGreen($"[{BotName}] Remove Video in conf - no stream to use");
                            sdkResultBoolean = await _rbWebRTCCommunications.RemoveVideoAsync(call.Id);
                        }
                        else
                        {
                            Util.WriteGreen($"[{BotName}] Use same Video in conf");
                        }
                    }
                    else
                    {
                        Util.WriteGreen($"[{BotName}] Remove Video in conf - no stream defined");
                        sdkResultBoolean = await _rbWebRTCCommunications.RemoveVideoAsync(call.Id);
                    }
                }
                else
                {
                    if (videoStreamAndDevices?.Count > 0 && call is not null)
                    {
                        // Perhaps the current video has been dropped - add it again
                        if ( (streamAndDeviceNewOneToUse is null) && (streamAndDevicesCurrentlyUsed.Count != 0))
                        {
                            streamAndDeviceNewOneToUse = streamAndDevicesCurrentlyUsed[0];
                            streamAndDevicesCurrentlyUsed.RemoveAt(0);
                        }

                        if ( (streamAndDeviceNewOneToUse is not null) 
                            && (streamAndDeviceNewOneToUse.Stream is not null) )
                        {
                            Util.WriteGreen($"[{BotName}] Add Video in conf - Id:[{streamAndDeviceNewOneToUse.Stream.Id}]");
                            sdkResultBoolean = await _rbWebRTCCommunications.AddVideoAsync(call.Id, (IVideoStreamTrack ?) streamAndDeviceNewOneToUse.VideoTrack);
                            if (sdkResultBoolean.Success)
                            {
                                streamAndDeviceNewOneToUse.UsedInConference = true;
                                streamAndDevicesCurrentlyUsed.ForEach(s => { s.UsedInConference = false; s.MustBeRemovedFromConference = true; }); // /!\ Shoud contains none
                                streamAndDevicesCurrentlyUsedToRemove.ForEach(s => { s.UsedInConference = false; s.MustBeRemovedFromConference = true; }); // /!\ Shoud contains none
                            }
                        }
                        else
                        {
                            Util.WriteGreen($"[{BotName}] No Video to use in conf ...");
                        }

                    }
                }
                //
                // Check by media - VIDEO - END
                //
                //////////////////////////

                //////////////////////////
                ///
                // Check by media - SHARING - START
                //
                var sharingStreamAndDevices = _listConnectedDevices.FindAll(streamAndDevice =>
                            (streamAndDevice.VideoTrack is not null)
                            && (streamAndDevice.Stream?.MediaInConference?.Contains("sharing", StringComparison.InvariantCultureIgnoreCase) == true));

                // Get stream details
                streamAndDeviceNewOneToUse = sharingStreamAndDevices.FirstOrDefault(s => (!s.UsedInConference) && (!s.MustBeRemovedFromConference));
                streamAndDevicesCurrentlyUsed = sharingStreamAndDevices.FindAll(s => (s.UsedInConference) && (!s.MustBeRemovedFromConference));
                streamAndDevicesCurrentlyUsedToRemove = sharingStreamAndDevices.FindAll(s => (s.UsedInConference) && (s.MustBeRemovedFromConference));

                // Do we have already a sharing ?
                if (call is not null && Rainbow.Util.MediasWithSharing(call.LocalMedias))
                {
                    if (sharingStreamAndDevices?.Count > 0)
                    {
                        if (streamAndDeviceNewOneToUse is not null)
                        {
                            Util.WriteGreen($"[{BotName}] Change Sharing in conf - Id:[{streamAndDeviceNewOneToUse.Stream.Id}]");

                            sdkResultBoolean = await _rbWebRTCCommunications.ChangeSharingAsync(call.Id, streamAndDeviceNewOneToUse.VideoTrack as IVideoStreamTrack);
                            if (sdkResultBoolean.Success)
                            {
                                streamAndDeviceNewOneToUse.UsedInConference = true;
                                streamAndDevicesCurrentlyUsed.ForEach(s => { s.UsedInConference = false; s.MustBeRemovedFromConference = true; }); // /!\ Shoud contains none
                                streamAndDevicesCurrentlyUsedToRemove?.ForEach(s => { s.UsedInConference = false; s.MustBeRemovedFromConference = true; });
                            }
                        }
                        else if ((streamAndDevicesCurrentlyUsed.Count == 0) || (streamAndDevicesCurrentlyUsedToRemove.Count != 0))
                        {
                            Util.WriteGreen($"[{BotName}] Remove Sharing in conf - no stream to use");
                            sdkResultBoolean = await _rbWebRTCCommunications.RemoveSharingAsync(call.Id);
                        }
                        else
                        {
                            Util.WriteGreen($"[{BotName}] Use same Sharing in conf");
                        }
                    }
                    else
                    {
                        Util.WriteGreen($"[{BotName}] Remove Sharing in conf - no stream defined");
                        sdkResultBoolean = await _rbWebRTCCommunications.RemoveSharingAsync(call.Id);
                    }
                }
                else
                {
                    if (sharingStreamAndDevices?.Count > 0 && call is not null)
                    {
                        // Perhaps the current sharing has been dropped - add it again
                        if ((streamAndDeviceNewOneToUse is null) && (streamAndDevicesCurrentlyUsed.Count != 0))
                        {
                            streamAndDeviceNewOneToUse = streamAndDevicesCurrentlyUsed[0];
                            streamAndDevicesCurrentlyUsed.RemoveAt(0);
                        }

                        if (streamAndDeviceNewOneToUse is not null)
                        {
                            Util.WriteGreen($"[{BotName}] Add Sharing in conf - Id:[{streamAndDeviceNewOneToUse.Stream.Id}]");
                            sdkResultBoolean = await _rbWebRTCCommunications.AddSharingAsync(call.Id, streamAndDeviceNewOneToUse.VideoTrack as IVideoStreamTrack);
                            if (sdkResultBoolean.Success)
                            {
                                streamAndDeviceNewOneToUse.UsedInConference = true;
                                streamAndDevicesCurrentlyUsed.ForEach(s => { s.UsedInConference = false; s.MustBeRemovedFromConference = true; }); // /!\ Shoud contains none
                                streamAndDevicesCurrentlyUsedToRemove.ForEach(s => { s.UsedInConference = false; s.MustBeRemovedFromConference = true; }); // /!\ Shoud contains none
                            }
                        }
                        else
                        {
                            Util.WriteGreen($"[{BotName}] No Sharing to use in conf ...");
                        }

                    }
                }
                //
                // Check by media - SHARING - END
                //
                //////////////////////////



            }
            else if (call?.IsInProgress() == true)
            {
                // Need to do something ?
            }
            else
            {
                _listConnectedDevices.ForEach(s => s.UsedInConference = false );
            }


            // From the list - remove/dispose tracks 
            foreach (var streamAndDevice in _listConnectedDevices)
            {
                if ((streamAndDevice.Stream is null) 
                    || (!streamAndDevice.MustBeDeleted) 
                    || (streamAndDevice.UsedInConference)) continue;

                if (streamAndDevice.VideoTrack is not null)
                {
                    streamAndDevice.VideoTrack.Dispose();
                    streamAndDevice.VideoTrack = null;
                    Util.WriteGreen($"[{BotName}] Track disposed  - Id:[{streamAndDevice.Stream.Id}] - Uri:[{streamAndDevice.Stream.Uri}] - Media:[{streamAndDevice.Stream.Media}]");
                }

                if (streamAndDevice.AudioTrack is not null)
                {
                    streamAndDevice.AudioTrack.Dispose();
                    streamAndDevice.AudioTrack = null;
                    Util.WriteGreen($"[{BotName}] Track disposed  - Id:[{streamAndDevice.Stream.Id}] - Uri:[{streamAndDevice.Stream.Uri}] - Media:[{streamAndDevice.Stream.Media}]");
                }
            }

            // Store only 
            _listConnectedDevices = _listConnectedDevices.FindAll(s => s.MustBeDeleted == false);

            await Task.CompletedTask;
            return true;
        }

        private void CreateWebRTCEnvironment()
        {
            if (_rbWebRTCDesktopFactory is null)
            {
                UnregisterToWebRTCEvents();

                _rbWebRTCDesktopFactory = new();
                _rbWebRTCCommunications = WebRTCCommunications.GetOrCreateInstance(Application, _rbWebRTCDesktopFactory);

                _rbContacts = Application.GetContacts();
                _rbConferences = Application.GetConferences();
                _rbBubbles = Application.GetBubbles();

                RegisterToWebRTCEvents();
            }
        }

        private void RegisterToWebRTCEvents()
        {
            if (_rbWebRTCCommunications is not null)
                _rbWebRTCCommunications.CallUpdated += RbWebRTCCommunications_CallUpdated;

            if (_rbConferences is not null)
            {
                _rbConferences.ConferenceUpdated += RbConferences_ConferenceUpdated;
                _rbConferences.ConferenceRemoved += RbConferences_ConferenceRemoved;
            }
        }

        private void UnregisterToWebRTCEvents()
        {
            if (_rbWebRTCCommunications is not null)
                _rbWebRTCCommunications.CallUpdated -= RbWebRTCCommunications_CallUpdated;

            if (_rbConferences is not null)
            {
                _rbConferences.ConferenceUpdated -= RbConferences_ConferenceUpdated;
                _rbConferences.ConferenceRemoved -= RbConferences_ConferenceRemoved;
            }
        }

#region Events triggered by Rainbow SDK

        private void RbConferences_ConferenceRemoved(Rainbow.Model.Conference conference)
        {
            RbConferences_ConferenceUpdated(conference);
        }

        private void RbConferences_ConferenceUpdated(Rainbow.Model.Conference conference)
        {
            lock (conferencesInProgress)
            {
                if (conference.Active)
                {
                    if (!conferencesInProgress.Contains(conference.Peer.Id))
                    {
                        conferencesInProgress.Add(conference.Peer.Id);
                        Util.WriteBlue($"[{BotName}] [ConferenceUpdated] A conference is active - Id:[{conference.Peer.Id}]");
                    }
                }
                else
                {
                    if (conferencesInProgress.Contains(conference.Peer.Id))
                    {
                        conferencesInProgress.Remove(conference.Peer.Id);
                        Util.WriteBlue($"[{BotName}] [ConferenceUpdated] A conference is NO MORE active - Id:[{conference.Peer.Id}]");
                    }
                }
                CheckBroadcastConfiguration();
            }
        }

        private void RbWebRTCCommunications_CallUpdated(Call call)
        {
            if (call == null)
                return;

            // Add / Update this call
            currentCallsList[call.Id] = call ;

            if (!call.IsInProgress())
            {
                currentCallsList.Remove(call.Id);

                // The call is NO MORE in Progress => We Rollback presence if any
                var _1 = _rbContacts?.RollbackPresenceSavedAsync();

                // TODO - how to manage this:
                _listConnectedDevices.ForEach(s => s.UsedInConference = false);
            }
            else
            {
                if (!call.IsRinging())
                {
                    // The call is NOT IN RINGING STATE  => We update presence according media
                    var _1 = _rbContacts?.SetBusyPresenceAccordingMediasAsync(call.LocalMedias);
                }
            }
                
            Util.WriteBlue($"[{BotName}] [CallUpdated] CallId:[{call.Id}] - Status:[{call.CallStatus}] - Local:[{Rainbow.Util.MediasToString(call.LocalMedias)}] - Remote:[{Rainbow.Util.MediasToString(call.RemoteMedias)}] - Conf.:[{(call.IsConference ? "True" : "False")}] - IsInitiator.:[{call.IsInitiator}]");
            CheckBroadcastConfiguration();
        }

#endregion Events triggered by Rainbow SDK


#region OVERRIDE METHODS OF BotBase
        public override async Task ConnectedAsync()
        {
            this.Application.Restrictions.JoinMultipleConferences = true;

            // Nothing to do here
            await Task.CompletedTask;
        }

        public override async Task StoppedAsync(SdkError sdkError)
        {
            // Here we do nothing special
            await Task.CompletedTask;
        }

        public override async Task BotConfigurationUpdatedAsync(BotConfigurationUpdate botConfigurationUpdate)
        {
            // Ensure to have an object not null
            if (botConfigurationUpdate is null)
                return;

            // BotConfigurationExtended object has been created to store data structure specific for this bot
            // We try to parse JSON Node to fill this data structure and if it's correct we update the broadcast configuration
            if (BotConfigurationExtended.FromJsonNode(botConfigurationUpdate.JSONNodeBotConfiguration, out BotConfigurationExtended botConfigurationExtended))
            {
                lock (_lockBotConfiguration)
                    _nextBotConfigurationUpdate = botConfigurationUpdate;

                UpdateBroadcastConfiguration();
            }

            await Task.CompletedTask;
        }

    #region Invitations - bubble or user
        public override async Task BubbleInvitationReceivedAsync(Rainbow.Model.BubbleInvitation bubbleInvitation)
        {
            // Nothing to do here
            await Task.CompletedTask;
        }

        public override async Task UserInvitationReceivedAsync(Rainbow.Model.Invitation invitation)
        {
            // Nothing to do here
            await Task.CompletedTask;
        }
    #endregion Invitations - bubble or user


    #region Messages - AckMessage, ApplicationMessage, InstantMessage, InternalMessage
        public override async Task AckMessageReceivedAsync(Rainbow.Model.AckMessage ackMessage)
        {
            // Nothing to do here
            await Task.CompletedTask;
        }

        public override async Task ApplicationMessageReceivedAsync(Rainbow.Model.ApplicationMessage applicationMessage)
        {
            // Nothing to do here
            await Task.CompletedTask;
        }

        public override async Task InstantMessageReceivedAsync(Rainbow.Model.Message message)
        {
            // Nothing to do here
            await Task.CompletedTask;
        }

        public override async Task InternalMessageReceivedAsync(InternalMessage internalMessage)
        {
            // Nothing to do here
            await Task.CompletedTask;
        }
    }
    #endregion Messages - AckMessage, ApplicationMessage, InstantMessage, InternalMessage

#endregion OVERRIDE METHODS OF BotBase
}