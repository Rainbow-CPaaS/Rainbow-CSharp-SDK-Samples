using ConsoleWebRTC;
using FFmpeg.AutoGen;
using Rainbow;
using Rainbow.Consts;
using Rainbow.Enums;
using Rainbow.Example.Common;
using Rainbow.Example.CommonSDL2;
using Rainbow.Medias;
using Rainbow.Model;
using Rainbow.SimpleJSON;
using Rainbow.WebRTC;
using Rainbow.WebRTC.Abstractions;
using Rainbow.WebRTC.Desktop;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Stream = Rainbow.Example.Common.Stream;



// Need to set an unique title
ConsoleAbstraction.Title = $"SDK C# - WebRTC [{Guid.NewGuid()}]";

ExeSettings? exeSettings = null;
List<Stream>? streamsList = null;
Credentials? credentials = null;

ConsoleAbstraction.WriteDarkYellow($"{Global.ProductName()} v{Global.FileVersion()}");

if ( (!ReadExeSettings()) || (exeSettings is null) )
    return;

if ( (!ReadStreamsSettings()) || (streamsList is null))
    return;

if ( (!ReadCredentials()) || (credentials is null))
    return;

var CR = Rainbow.Util.CR;

ConsoleAbstraction.WriteRed($"Account used: [{credentials.UsersConfig[0].Login}]");

// Init external libraries
ConsoleAbstraction.WriteGreen($"Initializing external libraries ...");
Rainbow.Medias.Helper.InitExternalLibraries(exeSettings.FfmpegLibFolderPath, true);
ConsoleAbstraction.WriteBlue($"External libraries initialized");

// Set folder path from logs
NLogConfigurator.Directory = exeSettings.LogFolderPath;

// Add logger for the prefix specified
NLogConfigurator.AddLogger(credentials.UsersConfig[0].Prefix + "_");

// Instead of a Microphone, an audio stream can be used 
Stream? audioStream = null;

// Instead of a Webcam, an video stream can be used for Video
String? videoFilePath = null;
Stream? videoStream = null;

// Instead of a Screen, an video stream can be used for Sharing
String? sharingFilePath = null;
Stream? sharingStream = null;

var videoStreams = streamsList?.FindAll(s => s.Media.Contains("video")).Select(s => s.Uri).ToList();
if(videoStreams?.Count > 1)
{
    videoFilePath = videoStreams[0];
    sharingFilePath = videoStreams[1];
}
else if (videoStreams?.Count > 0)
{
    videoFilePath = sharingFilePath = videoStreams[0];
}

// Define objects
String appId = credentials.ServerConfig.AppId; // To set according your settings
String appSecretKey = credentials.ServerConfig.AppSecret; // To set according your settings
String hostName = credentials.ServerConfig.HostName; // To set according your settings

String login = credentials.UsersConfig[0].Login; // To set according your settings
String password = credentials.UsersConfig[0].Password; // To set according your settings

Boolean endProgram = false;// To know when to quit the app

String? currentCallId = null;
Call? currentCall = null;
List<String> conferencesInProgress = [];

Device? currentAudioInput = null;
Boolean useEmptyTrack = true;   // By default use empty track for audio input
Device? currentAudioOutput = null;
Device? currentWebCam = null;
Device? currentScreen = null;

SDL2AudioOutput? sdl2AudioOutput = null;

List<WebcamDevice> webcamDevices;
List<ScreenDevice> screenDevices;
List<Device> audioInputDevices;
List<Device> audioOutputDevices;
DataChannel? dcLocal = null;
DataChannel? dcRemote = null;
String dcFile = Path.Combine(exeSettings.LogFolderPath, "dc_output.txt");

TimeSpan delayDCMsg = TimeSpan.FromMilliseconds(2000);
Timer dcSendWatcher = new Timer((_state) => SendDataOnDc(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

IAudioStreamTrack? audioTrack = null;

IVideoStreamTrack? videoTrack = null;
IVideoStreamTrack? sharingTrack = null;

AudioStreamTrack? audioRemoteTrack = null;

VideoStreamTrack? videoRemoteTrack = null;
VideoStreamTrack? sharingRemoteTrack = null;

// To ensure to launch action on main thread
BlockingCollection<Action> _actions = new();

// To manage Window (Video or Sharing output)
Window _windowVideo = new();
Window _windowSharing = new();

Object lockWindowVideo = new();
IntPtr windowVideo = IntPtr.Zero;
IntPtr windowVideoRenderer = IntPtr.Zero;
IntPtr windowVideoTexture = IntPtr.Zero;

Object lockWindowSharing = new();
IntPtr windowSharing = IntPtr.Zero;
IntPtr windowSharingRenderer = IntPtr.Zero;
IntPtr windowSharingTexture = IntPtr.Zero;

Task RbTask = Task.CompletedTask;
/*
Restrictions restrictions = new()
{
    // Access to services:
    UseAdministration = false,
    UseAlerts = false,
    UseBubbles = true,
    UseCallsLog = false,
    UseChannels = false,
    UseConferences = true,
    UseCustomerCare = false,
    UseFavorites = false,
    UseFileStorage = false,
    UseGroups = false,
    UseHubTelephony = false,
    UseHybridTelephony = false,
    UseInstantMessaging = false,
    UseInvitations = true,
    UseRPC = false,
    UseSMS = false,
    UseWebRTC = true,

    // Options
    AcceptBubbleInvitation = true,
    AcceptUserInvitation = true,

    // Logs
    LogRestRequest = true,
    LogEvent = true
};
*/

// Set restrictions
Rainbow.Restrictions restrictions = new(true)
{
    AcceptBubbleInvitation = true,
    AcceptUserInvitation = true,

    LogRestRequest = true,
    LogEvent = true,
    LogEventParameters = true,
    LogEventRaised = true,
};

// Create Rainbow Application ROOT object
var RbApplication = new Rainbow.Application(exeSettings.LogFolderPath, restrictions: restrictions);

// Create Rainbow SDK objects
var RbConferences       = RbApplication.GetConferences();
var RbContacts          = RbApplication.GetContacts();
var RbBubbles           = RbApplication.GetBubbles();
var RbAutoReconnection  = RbApplication.GetAutoReconnection();
var RbHubTelephony      = RbApplication.GetHubTelephony();

var RbWebRTCDesktopFactory = new Rainbow.WebRTC.Desktop.WebRTCFactory();

// By default use empty track for audio input
currentAudioInput = null;
useEmptyTrack = true;   
audioTrack = RbWebRTCDesktopFactory.CreateEmptyAudioTrack();

WebRTCCommunications? RbWebRTCCommunications;
try
{
    RbWebRTCCommunications = Rainbow.WebRTC.WebRTCCommunications.GetOrCreateInstance(RbApplication, RbWebRTCDesktopFactory);
}
catch
{
    RbWebRTCCommunications = null;
}

if(RbWebRTCCommunications is null)
{
    ConsoleAbstraction.WriteRed("Cannot create WebRTCCommunications service ... We quit.");
    return;
}

var log = Rainbow.LogFactory.CreateLogger("FFmpeg", RbApplication.LoggerPrefix);
Rainbow.Medias.Helper.SetFFmpegLog(log, Microsoft.Extensions.Logging.LogLevel.Debug);

// Set events that we want to follow
RbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged; // Triggered when the Connection State will change
RbAutoReconnection.Cancelled += RbAutoReconnection_Cancelled; // Triggered when AutoReonnection service is cancelled

RbConferences.ConferenceUpdated += RbConferences_ConferenceUpdated;
RbConferences.ConferenceRemoved += RbConferences_ConferenceRemoved;

RbWebRTCCommunications.CallUpdated += RbWebRTCCommunications_CallUpdated;
RbWebRTCCommunications.OnMediaPublicationUpdated += RbWebRTCCommunications_OnMediaPublicationUpdated;
RbWebRTCCommunications.OnDataChannel += RbWebRTCCommunications_OnDataChannel;
RbWebRTCCommunications.OnTrack += RbWebRTCCommunications_OnTrack;

// Set global configuration info
RbApplication.SetApplicationInfo(appId, appSecretKey);
RbApplication.SetHostInfo(hostName);

// We want to know when a screen, a webcam or Audio device is added / removed
Rainbow.Medias.Devices.OnScreenDeviceAdded += Devices_OnScreenDeviceAdded;
Rainbow.Medias.Devices.OnScreenDeviceRemoved += Devices_OnScreenDeviceRemoved;
Rainbow.Medias.Devices.OnWebcamDeviceAdded += Devices_OnWebcamDeviceAdded;
Rainbow.Medias.Devices.OnWebcamDeviceRemoved += Devices_OnWebcamDeviceRemoved;
Rainbow.Medias.Devices.OnAudioInputDeviceAdded += Devices_OnAudioInputDeviceAdded;
Rainbow.Medias.Devices.OnAudioInputDeviceRemoved += Devices_OnAudioInputDeviceRemoved;
Rainbow.Medias.Devices.OnAudioOutputDeviceAdded += Devices_OnAudioOutputDeviceAdded;
Rainbow.Medias.Devices.OnAudioOutputDeviceRemoved += Devices_OnAudioOutputDeviceRemoved;

MenuDisplayInfo();

// Start login
ConsoleAbstraction.WriteGreen("Starting login ...");
var loginTask = RbApplication.LoginAsync(login, password); // We don't wait here since we want to quit at any time using ESC key

await MainLoop();

Window.Destroy(_windowVideo);
Window.Destroy(_windowSharing);

if (!String.IsNullOrEmpty(currentCallId))
    await RbWebRTCCommunications.HangUpCallAsync(currentCallId);

await RbApplication.LogoutAsync();

async Task MainLoop()
{
    int simulatedKey = 0;

    do
    {
        while (SDL2.SDL_PollEvent(out SDL2.SDL_Event e) > 0)
        {
            switch (e.type)
            {
                case SDL2.SDL_EventType.SDL_QUIT:
                    // Does nothing - it's necessary to unsubscribe to media publication
                    break;

                case SDL2.SDL_EventType.SDL_WINDOWEVENT:
                    switch (e.window.windowEvent)
                    {
                        case SDL2.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                            if (e.window.windowID == _windowVideo.Id)
                                _windowVideo.NeedRendererUpdate = true;
                            else if (e.window.windowID == _windowSharing.Id)
                                _windowSharing.NeedRendererUpdate = true;
                            break;
                        case SDL2.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                            if (e.window.windowID == _windowVideo.Id)
                            {
                                var _ = UnsubscribeToMediaAsync(Media.VIDEO);
                            }
                            else if (e.window.windowID == _windowSharing.Id)
                            {
                                var _ = UnsubscribeToMediaAsync(Media.SHARING);
                            }
                            break;
                    }
                    break;

                case SDL2.SDL_EventType.SDL_RENDER_TARGETS_RESET:
                    if (e.window.windowID == _windowVideo.Id)
                        _windowVideo.NeedRendererUpdate = true;
                    else if (e.window.windowID == _windowSharing.Id)
                        _windowSharing.NeedRendererUpdate = true;
                    break;


                case SDL2.SDL_EventType.SDL_KEYUP:
                    switch (e.key.keysym.sym)
                    {
                        case SDL2.SDL_Keycode.SDLK_ESCAPE:
                            simulatedKey = (int)ConsoleKey.Escape;
                            break;

                        case SDL2.SDL_Keycode.SDLK_f:
                            if (e.window.windowID == _windowVideo.Id)
                                Window.ToggleFullScreen(_windowVideo);
                            else if (e.window.windowID == _windowSharing.Id)
                                Window.ToggleFullScreen(_windowSharing);
                            break;

                        default:
                            simulatedKey = ((int)ConsoleKey.A) - ((int)SDL2.SDL_Keycode.SDLK_a) + ((int)e.key.keysym.sym);
                            break;
                    }
                    break;
            }
        }

        Window.CheckUpdateRenderer(_windowVideo);
        Window.CheckUpdateRenderer(_windowSharing);

        CheckInputKey(simulatedKey);
        simulatedKey = 0;

        if (!endProgram)
        {
            if (_actions.TryTake(out var action))
                action.Invoke();
        }

    } while (!endProgram); // Loop until we want to quit

    await Task.CompletedTask;
}

void CheckInputKey(int simulatedKey)
{
    if (ConsoleAbstraction.KeyAvailable || simulatedKey != 0)
    {
        int key;
        if (simulatedKey != 0)
        {
            key = simulatedKey;
            simulatedKey = 0;
            FocusConsoleWindow();
        }
        else
        {
            var userInput = ConsoleAbstraction.ReadKey();
            key = userInput.HasValue ? (int)userInput.Value.Key :-1;
        }
        switch (key)
        {
            case (int)ConsoleKey.Escape:
                MenuEscape();
                return;

            case (int)ConsoleKey.I: // Info
                MenuDisplayInfo();
                return;

            case (int)ConsoleKey.H: // Info
                MenuHubTelephony();
                return;

            case (int)ConsoleKey.C: // Conference - Join/Quit or Start
                MenuConference();
                break;

            case (int)ConsoleKey.P: // P2P - HangUp or Start
                MenuP2P();
                break;

            case (int)ConsoleKey.A: // Audio input/output selection.
                MenuAudioStream();
                break;

            case (int)ConsoleKey.V: // Video selection.
                MenuVideoStream();
                break;

            case (int)ConsoleKey.S: // Sharing selection.
                MenuSharingStream();
                break;

            case (int)ConsoleKey.D: // DataChannel: Add/Remove 
                MenuDataChannel();
                break;

            case (int)ConsoleKey.L: // List devices used and conference info
                MenuListDevicesAndConferenceInfo();
                break;

            case (int)ConsoleKey.O: // Update options for RTP Audio/Video stream
                MenuRTPStreamOptions();
                break;

            case (int)ConsoleKey.M:
                MenuMediaPublications();
                break;
        }
    }
}

void MenuDisplayInfo()
{
    ConsoleAbstraction.WriteDarkYellow($"{CR}{Global.ProductName()} v{Global.FileVersion()}");
    ConsoleAbstraction.WriteYellow("[ESC] at anytime to quit");
    ConsoleAbstraction.WriteYellow("[I] (Info) Display this info");

    //ConsoleAbstraction.WriteYellow("");
    //ConsoleAbstraction.WriteYellow("[H] (Hub Telephony)");

    ConsoleAbstraction.WriteYellow("");
    ConsoleAbstraction.WriteYellow("[C] (Conference) to start, join or quit a conference");
    ConsoleAbstraction.WriteYellow("[P] (P2P) to start or hang up a WebRTC P2P call");

    ConsoleAbstraction.WriteYellow("");
    ConsoleAbstraction.WriteYellow("[A] (Audio) to manage Audio (Input / Ouput)");
    ConsoleAbstraction.WriteYellow("[V] (Video) to manage Video");
    ConsoleAbstraction.WriteYellow("[S] (Sharing) to manage Sharing");
    ConsoleAbstraction.WriteYellow("[D] (DataChannel) to manage DataChannel");

    ConsoleAbstraction.WriteYellow("");
    ConsoleAbstraction.WriteYellow("[M] (MediaPublication) to subscribe/unsubscribe to media publication");

    ConsoleAbstraction.WriteYellow("");
    ConsoleAbstraction.WriteYellow("[L] (List) to list devices used/available and call info (conference or P2P)");
    ConsoleAbstraction.WriteYellow("[O] (Options) to update options for RTP Audio/Video streams (very advanced tests)");
}

void MenuMediaPublications()
{
    if (!RbTask.IsCompleted)
    {
        ConsoleAbstraction.WriteRed("Task is already in progress");
        return;
    }

    if(String.IsNullOrEmpty(currentCallId))
    {
        ConsoleAbstraction.WriteRed("currentCallId is null/empty...");
        return;
    }
    Action action = async () =>
    {
        var publications = RbWebRTCCommunications.GetMediaPublicationsAvailable(currentCallId);
        if (publications?.Count > 0)
        {
            var currentUserId = RbContacts.GetCurrentContact().Peer.Id;
            Boolean selected = false;

            while (!selected)
            {
                ConsoleAbstraction.WriteYellow($"Select MediaPublication to subscribe / unsubscribe:");
                ConsoleAbstraction.WriteDarkYellow($"NOTE: For VIDEO only can one be subscribed - if any, the previous one is replaced by the new one");
                int index = 0;
                foreach (var pub in publications)
                {
                    var subscribeStatus = (RbWebRTCCommunications.IsSubscribedToMediaPublication(pub)) ? "UNSUBSCRIBE" : "SUBSCRIBE";

                    if ((pub.Peer.Type == Rainbow.Consts.EntityType.DynamicFeed))
                    {
                        ConsoleAbstraction.WriteYellow($"\t[{index++}] - {subscribeStatus} TO - Media:[DynamicFeed]");
                    }
                    else
                    {
                        var displayname = (pub.Peer.Id == currentUserId) ? "YOURSELF" : pub.Peer.DisplayName;
                        ConsoleAbstraction.WriteYellow($"\t[{index++}] - {subscribeStatus} TO - Media:[{Rainbow.Util.MediasToString(pub.Media)}] - Peer:[{displayname}]");
                    }
                }


                ConsoleAbstraction.WriteYellow($"\t[C] - Cancel");
                var consoleKey = ConsoleAbstraction.ReadKey();
                if (consoleKey.HasValue && char.IsDigit(consoleKey.Value.KeyChar))
                {
                    var selection = int.Parse(consoleKey.Value.KeyChar.ToString());
                    if ((selection >= 0) && (selection < index))
                    {
                        selected = true;
                        var pub = publications[selection];


                        if ((pub.Peer.Type == Rainbow.Consts.EntityType.DynamicFeed))
                        {
                            ConsoleAbstraction.WriteYellow($"MediaPublication selected: [DynamicFeed]");
                        }
                        else
                        {
                            var displayname = (pub.Peer.Id == currentUserId) ? "YOURSELF" : pub.Peer.DisplayName;
                            ConsoleAbstraction.WriteDarkYellow($"MediaPublication selected: Media:[{Rainbow.Util.MediasToString(pub.Media)}] - Peer:[{displayname}]");
                        }

                        if (RbWebRTCCommunications.IsSubscribedToMediaPublication(pub))
                        {
                            ConsoleAbstraction.WriteGreen($"Asking to unsubscribe ...");
                            var sdkResult = await RbWebRTCCommunications.UnsubscribeToMediaPublicationAsync(pub);
                            if (sdkResult.Success)
                                ConsoleAbstraction.WriteDarkYellow("MediaPublication has been unsubscribed");
                            else
                                ConsoleAbstraction.WriteDarkYellow($"MediaPublication has NOT been unsubscribed:{sdkResult.Result}");
                        }
                        else
                        {
                            // We ensure we can subscribe to our own subscription
                            RbWebRTCCommunications.AllowToSubscribeToHisOwnMediaPublication(currentCallId, true);

                            // In this application, we restrict subscription to only one VIDEO MediaPublication
                            var mediaPublicationsSubcribed = RbWebRTCCommunications.GetMediaPublicationsSubscribed(currentCallId);
                            var previousMP = mediaPublicationsSubcribed?.Find(mp => (mp.Media == Media.VIDEO));// && (mp.Peer.Id != pub.Peer.Id));
                            if ((pub.Media == Media.VIDEO && previousMP is not null))
                            {
                                ConsoleAbstraction.WriteGreen($"First asking to unsubscribe to previous Video MediaPublication ...");

                                var sdkResult = await RbWebRTCCommunications.UnsubscribeToMediaPublicationAsync(previousMP);
                                if (sdkResult.Success)
                                {
                                    ConsoleAbstraction.WriteDarkYellow("Previous Video MediaPublication has been unsubscribed");

                                    ConsoleAbstraction.WriteGreen($"Asking to subscribe to the new Video MediaPublication ...");
                                    sdkResult = await RbWebRTCCommunications.SubscribeToMediaPublicationAsync(pub, MediaSubStreamLevel.HIGH);
                                    if (sdkResult.Success)
                                        ConsoleAbstraction.WriteDarkYellow("MediaPublication has been subscribed");
                                    else
                                        ConsoleAbstraction.WriteDarkYellow($"MediaPublication has NOT been subscribed:{sdkResult.Result}");
                                }
                                else
                                    ConsoleAbstraction.WriteDarkYellow($"Previous Video MediaPublication has NOT been unsubscribed:{sdkResult.Result}");
                            }
                            else
                            {
                                // /!\ Need to use Main Thread
                                if (pub.Media == Media.VIDEO)
                                {
                                    _actions.Add(new Action(() =>
                                    {
                                        Window.Create(_windowVideo);
                                    }));
                                }
                                else if (pub.Media == Media.SHARING)
                                {
                                    _actions.Add(new Action(() =>
                                    {
                                        Window.Create(_windowSharing);
                                    }));
                                }

                                ConsoleAbstraction.WriteGreen($"Asking to subscribe ...");
                                var sdkResult = await RbWebRTCCommunications.SubscribeToMediaPublicationAsync(pub, MediaSubStreamLevel.HIGH);
                                if (sdkResult.Success)
                                    ConsoleAbstraction.WriteDarkYellow("MediaPublication has been subscribed");
                                else
                                    ConsoleAbstraction.WriteDarkYellow($"MediaPublication has NOT been subscribed:{sdkResult.Result}");
                            }
                        }
                    }
                    else
                        ConsoleAbstraction.WriteDarkYellow($"Bad key used ...");
                }
                else if (consoleKey.HasValue && consoleKey.Value.Key == ConsoleKey.C)
                {
                    selected = true;
                    ConsoleAbstraction.WriteDarkYellow($"Cancel used ...");
                }
                else
                    ConsoleAbstraction.WriteDarkYellow($"Bad key used ...");
            }
        }
        else
        {
            ConsoleAbstraction.WriteRed("No MediaPublication available");
        }
    };
    RbTask = Task.Run(action);
}

void MenuP2P()
{
    if (!RbTask.IsCompleted)
    {
        ConsoleAbstraction.WriteRed("Task is already in progress");
        return;
    }

    Action action = async () =>
    {

        if ((currentCall != null) && (!currentCall.IsConference) && (currentCall.IsInProgress() && (currentCall.CallStatus != CallStatus.RINGING_INCOMING) ))
        {
            ConsoleAbstraction.WriteGreen("Asking to leave P2P...");
            if (currentCall.Id != null)
            {
                await RbWebRTCCommunications.HangUpCallAsync(currentCall.Id);
                ConsoleAbstraction.WriteDarkYellow($"P2P has been left");
            }
            else
                ConsoleAbstraction.WriteRed($"Cannot leave P2P - currentCallId object null");
        }
        else if ((currentCall != null) && (!currentCall.IsConference) && (currentCall.CallStatus == CallStatus.RINGING_INCOMING))
        {
            await AnswerP2PAsync();
        }
        else if (currentCall == null)
        {
            if (audioTrack is null)
            {
                ConsoleAbstraction.WriteRed("You cannot start a P2P call. You don't have created an Audio Input Track");
                return;
            }

            var canContinue = true;
            while (canContinue)
            {
                Contact? result = null;
                ConsoleAbstraction.WriteYellow($"{Rainbow.Util.CR}Enter a text to search a User: (empty text to cancel)");
                var str = ConsoleAbstraction.ReadLine();
                if (String.IsNullOrEmpty(str))
                {
                    canContinue = false;
                    ConsoleAbstraction.WriteDarkYellow("Cancel search ...");
                    break;
                }
                else
                {
                    var contactsList = RbContacts.GetAllContacts();
                    result = contactsList.Find(contact => contact.Peer.DisplayName.Contains(str, StringComparison.InvariantCultureIgnoreCase));
                }

                if (result == null)
                    ConsoleAbstraction.WriteDarkYellow($"No contact found with [{str}] ...");
                else
                {
                    ConsoleAbstraction.WriteDarkYellow($"First contact found:[{result.ToString(DetailsLevel.Small)}]");
                    ConsoleAbstraction.WriteYellow($"Do you want to make P2P call with this contact ? [Y]");

                    var keyInfo = ConsoleAbstraction.ReadKey();
                    if (keyInfo?.Key == ConsoleKey.Y)
                    {
                        // Set tracks used to make P2P call
                        Dictionary<int, IMediaStreamTrack?>? mediaStreamTracks = new()
                        {
                            {Media.AUDIO, audioTrack }
                        };

                        if (videoTrack is not null)
                        {
                            ConsoleAbstraction.WriteYellow($"Do you want to add video to this P2P call ? [Y]");
                            keyInfo = ConsoleAbstraction.ReadKey();
                            if (keyInfo?.Key == ConsoleKey.Y)
                            {
                                mediaStreamTracks.Add(Media.VIDEO, videoTrack);
                            }
                        }

                        // We save current presence
                        var presence = RbContacts.GetPresence();
                        RbContacts.SavePresenceForRollback();
                        ConsoleAbstraction.WriteBlue($"SavePresenceForRollback - Presence:[{presence.ToString(DetailsLevel.Small)}]");


                        ConsoleAbstraction.WriteGreen($"Asking to start P2P ...");
                        var sdkResult = await RbWebRTCCommunications.MakeCallAsync(result.Peer.Id, mediaStreamTracks);
                        if (sdkResult.Success)
                        {
                            ConsoleAbstraction.WriteDarkYellow($"P2P call has been started");
                            canContinue = false;
                            break;
                        }
                        else
                            ConsoleAbstraction.WriteRed($"Cannot make Call:[{sdkResult.Result}]");
                    }
                    else
                        ConsoleAbstraction.WriteDarkYellow("Conference not started ...");
                }
            }
        }
    };

    RbTask = Task.Run(action);
}

void MenuHubTelephony()
{
    if (!RbTask.IsCompleted)
    {
        ConsoleAbstraction.WriteRed("Task is already in progress");
        return;
    }

    Action action = async () =>
    {
        //ConsoleAbstraction.WriteGreen("Registering as Hub Telephony Device ...");
        //var sdkResultBoolean = await RbHubTelephony.RegisterAsDeviceAsync();
        //if(sdkResultBoolean.Success)
        //    ConsoleAbstraction.WriteBlue($"Registration Hub Telephony Device done");
        //else
        //    ConsoleAbstraction.WriteRed($"Cannot register as Hub Telephony Device - Error[{sdkResultBoolean.Result}]");
    };

    RbTask = Task.Run(action);
}

void MenuConference()
{
    if (!RbTask.IsCompleted)
    {
        ConsoleAbstraction.WriteRed("Task is already in progress");
        return;
    }

    Action action = async () =>
    {
        if ( (currentCall != null) && (currentCall.IsConference) )
        {
            ConsoleAbstraction.WriteGreen("Asking to leave conference ...");
            if (currentCall.Id != null)
            {
                await RbWebRTCCommunications.HangUpCallAsync(currentCall.Id);
                ConsoleAbstraction.WriteDarkYellow($"Conference has been left");
            }
            else
                ConsoleAbstraction.WriteRed($"Cannot leave Conference - currentCallId object null");
        }
        else if (conferencesInProgress.Count > 0)
        {
            ConsoleAbstraction.WriteGreen("Asking to join conference ...");
            await JoinConferenceAsync(RbConferences.GetConferenceById(conferencesInProgress[0]));
        }
        else if (currentCall == null)
        {
            if (audioTrack is null)
            {
                ConsoleAbstraction.WriteRed("You cannot start a conference. You don't have created an Audio Input Track");
                return;
            }

            var canContinue = true;
            while (canContinue)
            {
                Bubble? result = null;
                ConsoleAbstraction.WriteYellow($"{Rainbow.Util.CR}Enter a text to search a Bubble: (empty text to cancel)");
                var str = ConsoleAbstraction.ReadLine();
                if (String.IsNullOrEmpty(str))
                {
                    canContinue = false;
                    ConsoleAbstraction.WriteDarkYellow("Cancel search ...");
                    break;
                }
                else
                {
                    List<String> memberStatus = new() { BubbleMemberStatus.Accepted };
                    var bubblesList = RbBubbles.GetAllBubbles(bubbleMemberStatusList: memberStatus);
                    result = bubblesList.Find(contact => contact.Peer.DisplayName.Contains(str, StringComparison.InvariantCultureIgnoreCase));
                }

                if (result == null)
                    ConsoleAbstraction.WriteDarkYellow($"No bubble found with [{str}] ...");
                else
                {
                    ConsoleAbstraction.WriteDarkYellow($"First bubble found:[{result.ToString(DetailsLevel.Small)}]");
                    ConsoleAbstraction.WriteYellow($"Do you want to start conference in this bubble ? [Y]");

                    var keyInfo = ConsoleAbstraction.ReadKey();
                    if (keyInfo?.Key == ConsoleKey.Y)
                    {
                        ConsoleAbstraction.WriteGreen($"Asking to start and join conference ...");
                        var sdkResult = await RbWebRTCCommunications.StartAndJoinConferenceAsync(result.Peer.Id, audioTrack);
                        if (sdkResult.Success)
                        {
                            ConsoleAbstraction.WriteDarkYellow($"Conference has been started");
                            canContinue = false;
                            break;
                        }
                        else
                            ConsoleAbstraction.WriteRed($"Cannot start Conference:[{sdkResult.Result}]");
                    }
                    else
                        ConsoleAbstraction.WriteDarkYellow("Conference not started ...");
                }
            }
        }
    };

    RbTask = Task.Run(action);
}

void MenuDataChannel()
{
    if (!RbTask.IsCompleted)
    {
        ConsoleAbstraction.WriteRed("Task is already in progress");
        return;
    }

    Action action = async () =>
    {
        if (currentCall?.IsActive() == true)
        {
            if (Rainbow.Util.MediasWithDataChannel(currentCall.LocalMedias))
            {
                ConsoleAbstraction.WriteGreen("Asking to remove DataChannel ...");
                await RemoveDataChannelAsync();
            }
            else
            {
                ConsoleAbstraction.WriteGreen("Asking to add DataChannel ...");
                await AddDataChannelAsync();
            }
        }
        else
            ConsoleAbstraction.WriteRed($"Conference is not active");
    };
    RbTask = Task.Run(action);
}

void MenuAudioStream()
{
    Boolean? manageInput = null;
    if (!RbTask.IsCompleted)
    {
        ConsoleAbstraction.WriteRed("Task is already in progress");
        return;
    }

    Action action = async () =>
    {
        var canContinue = true;
        while (canContinue)
        {
            ConsoleAbstraction.WriteYellow("");
            ConsoleAbstraction.WriteYellow($"Manage Audio stream:");
            ConsoleAbstraction.WriteYellow($"\t[I] Manage Audio INPUT stream");
            ConsoleAbstraction.WriteYellow($"\t[O] Manage Audio OUTPUT stream");
            ConsoleAbstraction.WriteYellow($"\t[C] Cancel");

            var keyInfo = ConsoleAbstraction.ReadKey();
            switch (keyInfo?.Key)
            {
                case ConsoleKey.I:
                    canContinue = false;
                    manageInput = true;
                    ConsoleAbstraction.WriteDarkYellow($"Audio INPUT stream selected ...");
                    break;

                case ConsoleKey.O:
                    canContinue = false;
                    manageInput = false;
                    ConsoleAbstraction.WriteDarkYellow($"Audio OUTPUT stream selected ...");
                    break;

                case ConsoleKey.C:
                    canContinue = false;
                    ConsoleAbstraction.WriteDarkYellow($"Cancel used ...");
                    break;

                default:
                    canContinue = true;
                    ConsoleAbstraction.WriteDarkYellow($"Bad key used ...");
                    break;
            }

            if (manageInput == null)
                return;

            if (manageInput == true)
                await SubMenuAudioInputStream();
            else
                SubMenuAudioOutputStream();
        }
    };

    RbTask = Task.Run(action);
}

async Task SubMenuAudioInputStream()
{
    var canSelectAudioInputDevice = false;
    if (currentCall?.IsActive() == true)
    {
        if (Rainbow.Util.MediasWithAudio(currentCall.LocalMedias))
        {
            var canContinue = true;
            while (canContinue)
            {
                ConsoleAbstraction.WriteYellow("");
                if (audioTrack is not null)
                {
                    if (useEmptyTrack)
                        ConsoleAbstraction.WriteYellow($"A call is in progress WITH your EMPTY Audio track");
                    else //if (currentAudioInput != null)
                        ConsoleAbstraction.WriteYellow($"A call is in progress WITH your AUDIO Input: {currentAudioInput?.Name} [{currentAudioInput?.Path}]");
                }
                ConsoleAbstraction.WriteYellow("\t[S] - Select another one and use it in the call");
                ConsoleAbstraction.WriteYellow("\t[C] - Cancel");
                var keyInfo = ConsoleAbstraction.ReadKey();
                switch (keyInfo?.Key)
                {
                    case ConsoleKey.S:
                        canContinue = false;
                        canSelectAudioInputDevice = true;
                        break;

                    case ConsoleKey.C:
                        canContinue = false;
                        ConsoleAbstraction.WriteDarkYellow($"Cancel used ...");
                        break;

                    default:
                        canContinue = true;
                        ConsoleAbstraction.WriteDarkYellow($"Bad key used ...");
                        break;

                }
            }
        }
        else
        {
            var canContinue = true;
            while (canContinue)
            {
                ConsoleAbstraction.WriteYellow("");
                if (audioTrack is null)
                    ConsoleAbstraction.WriteYellow($"A call is in progress:");
                else
                {
                    if(useEmptyTrack)
                        ConsoleAbstraction.WriteYellow($"A call is in progress WITHOUT your AUDIO INPUT: EMPTY TRACK");
                    else //if(currentAudioInput is not null)
                        ConsoleAbstraction.WriteYellow($"A call is in progress WITHOUT your AUDIO INPUT: {currentAudioInput?.Name} [{currentAudioInput?.Path}]");
                    ConsoleAbstraction.WriteYellow("\t[A] - Add it to the call");
                }
                ConsoleAbstraction.WriteYellow("\t[S] - Select stream and use it as AUDIO INPUT in the call");
                ConsoleAbstraction.WriteYellow("\t[C] - Cancel");
                var keyInfo = ConsoleAbstraction.ReadKey();
                switch (keyInfo?.Key)
                {
                    case ConsoleKey.A:
                        if (currentAudioInput == null)
                        {
                            canContinue = true;
                            ConsoleAbstraction.WriteDarkYellow($"Bad key used ...");
                        }
                        else
                        {
                            canContinue = false;
                            ConsoleAbstraction.WriteGreen("Asking to add AUDIO INPUT ...");
                            await AddAudioInputAsync();
                        }
                        break;

                    case ConsoleKey.S:
                        canContinue = false;
                        canSelectAudioInputDevice = true;
                        ConsoleAbstraction.WriteDarkYellow($"Select a stream ...");
                        break;

                    case ConsoleKey.C:
                        canContinue = false;
                        ConsoleAbstraction.WriteDarkYellow($"Cancel used ...");
                        break;

                    default:
                        canContinue = true;
                        ConsoleAbstraction.WriteDarkYellow($"Bad key used ...");
                        break;

                }
            }
        }
    }
    else
        canSelectAudioInputDevice = true;

    if (!canSelectAudioInputDevice)
        return;

    audioInputDevices = Rainbow.Medias.Devices.GetAudioInputDevices();
    Boolean audioInputUpdated = false;
    Boolean selected = false;
    while (!selected)
    {
        ConsoleAbstraction.WriteYellow("");
        if (audioTrack is null)
            ConsoleAbstraction.WriteYellow($"Current stream used has AUDIO INPUT: NONE");
        else
        {
            if (useEmptyTrack)
                ConsoleAbstraction.WriteYellow($"Current stream used has AUDIO INPUT: EMPTY TRACK");
            else
                ConsoleAbstraction.WriteYellow($"Current stream used has AUDIO INPUT: {currentAudioInput?.Name} [{currentAudioInput?.Path}]");
        }
        ConsoleAbstraction.WriteYellow($"Select stream to use has AUDIO INPUT:");
        int index = 0;
        if (audioInputDevices?.Count > 0)
        {
            foreach (var audioInput in audioInputDevices)
                ConsoleAbstraction.WriteYellow($"\t[{index++}] - {audioInput.Name} ({audioInput.Path})");
        }

        if(streamsList?.Count > 0)
            ConsoleAbstraction.WriteYellow($"\t[S] - Select Audio stream");

        ConsoleAbstraction.WriteYellow($"\t[E] - Use empty track");

        if (currentCall == null)
            ConsoleAbstraction.WriteYellow($"\t[N] - No audio input");
            

        ConsoleAbstraction.WriteYellow("\t[C] - Cancel");
        var consoleKey = ConsoleAbstraction.ReadKey();
        if (consoleKey.HasValue && char.IsDigit(consoleKey.Value.KeyChar))
        {
            var selection = int.Parse(consoleKey.Value.KeyChar.ToString());
            if ((selection >= 0) && (selection < index))
            {
                selected = true;
                useEmptyTrack = false;
                audioInputUpdated = currentAudioInput?.Path != audioInputDevices?[selection]?.Path;
                if (audioInputUpdated)
                {
                    currentAudioInput = audioInputDevices?[selection];
                    ConsoleAbstraction.WriteDarkYellow($"Stream used as Audio Input selected: {currentAudioInput?.Name} ({currentAudioInput?.Path})]");
                }
                else
                    ConsoleAbstraction.WriteDarkYellow($"Same Stream selected");
            }
        }
        else if (consoleKey.HasValue && (consoleKey.Value.Key == ConsoleKey.S) && (streamsList?.Count > 0))
        {
            var result = SelectStream("audio", audioStream, "audio");
            if (result is not null)
            {
                selected = true;
                if ((audioStream is null) || (audioStream.Id != result.Id))
                {
                    useEmptyTrack = false;
                    audioInputUpdated = true;
                    audioStream = result;
                    currentAudioInput = new InputStreamDevice("audio_stream", "audio_stream", audioStream.Uri, withVideo: false, withAudio: true);
                    ConsoleAbstraction.WriteDarkYellow($"Stream used as Audio Input selected: {currentAudioInput.Name} ({currentAudioInput.Path})]");
                }
                else
                    ConsoleAbstraction.WriteDarkYellow($"Same Audio Stream selected");
            }
        }
        else if (consoleKey.HasValue && (consoleKey.Value.Key == ConsoleKey.N) && (currentCall is null))
        {
            audioInputUpdated = true;
            currentAudioInput = null;
            useEmptyTrack = false;
            selected = true;
            ConsoleAbstraction.WriteDarkYellow($"No more audio input used ...");
        }
        else if (consoleKey.HasValue && consoleKey.Value.Key == ConsoleKey.E)
        {
            audioInputUpdated = true;
            currentAudioInput = null;
            useEmptyTrack = true;
            selected = true;
            ConsoleAbstraction.WriteDarkYellow($"Use empty track ...");
        }

        else if (consoleKey.HasValue && consoleKey.Value.Key == ConsoleKey.C)
        {
            selected = true;
            ConsoleAbstraction.WriteDarkYellow($"Cancel used ...");
        }
        else
            ConsoleAbstraction.WriteDarkYellow($"Invalid key used ...");
    }

    if (audioInputUpdated)
    {
        var previousAudioTrack = audioTrack;

        //if ( (currentAudioInput == null) && (!useEmptyTrack))
        //{
        //    //else
        //    //{
        //    //    if ((currentCall?.IsActive() == true) && Rainbow.Util.MediasWithAudio(currentCall.LocalMedias))
        //    //    {
        //    //        ConsoleAbstraction.WriteGreen("Asking to remove Audio Input from conference ...");
        //    //        await RemoveAudioInputAsync();
        //    //    }

                    
        //    //}

        //    ConsoleAbstraction.WriteGreen("Asking to dispose of previous Audio input track...");
        //    previousAudioTrack?.Dispose();
        //    previousAudioTrack = null;
        //    ConsoleAbstraction.WriteDarkYellow($"Previous Audio track disposed");
        //}
        //else 
        {
            if (useEmptyTrack)
            {
                try
                {
                    ConsoleAbstraction.WriteGreen("Asking to create new EMPTY Audio track...");
                    audioTrack = RbWebRTCDesktopFactory.CreateEmptyAudioTrack();

                    ConsoleAbstraction.WriteDarkYellow($"New Audio EMPTY track created");
                }
                catch (Exception exc)
                {
                    ConsoleAbstraction.WriteRed($"Cannot create EMPTY Audio track: Exception: [{exc}]");
                    audioTrack = previousAudioTrack;
                    return;
                }
            }
            else
            {
                if (currentAudioInput is null)
                {
                    audioTrack = null;
                }
                else
                {
                    try
                    {
                        ConsoleAbstraction.WriteGreen("Asking to create new Audio track...");
                        audioTrack = RbWebRTCDesktopFactory.CreateAudioTrack(currentAudioInput);

                        ConsoleAbstraction.WriteDarkYellow($"New Audio track created");
                    }
                    catch (Exception exc)
                    {
                        ConsoleAbstraction.WriteRed($"Cannot create Audio track: Exception: [{exc}]");
                        audioTrack = previousAudioTrack;
                        return;
                    }
                }
            }

            Boolean success = true;

            // Update call (if any) with this new track
            if ( (currentCall?.IsActive() == true) && (audioTrack is not null))
            {
                if (Rainbow.Util.MediasWithAudio(currentCall.LocalMedias))
                {
                    ConsoleAbstraction.WriteGreen("Asking to update Audio in the call ...");
                    success = await UpdateAudioInputAsync();
                }
                else
                {
                    ConsoleAbstraction.WriteGreen("Asking to add Audio in the call ...");
                    success = await AddAudioInputAsync();
                }
            }

            if (success)
            {
                ConsoleAbstraction.WriteGreen("Asking to dispose of previous Audio track...");
                previousAudioTrack?.Dispose();
                previousAudioTrack = null;
                ConsoleAbstraction.WriteDarkYellow($"Previous Audio track disposed");
            }
        }
    }
}

void SubMenuAudioOutputStream()
{
    audioOutputDevices = Rainbow.Medias.Devices.GetAudioOutputDevices();
    Boolean audioOutputUpdated = false;
    Boolean selected = false;
    while (!selected)
    {
        ConsoleAbstraction.WriteYellow("");
        if (currentAudioOutput != null)
            ConsoleAbstraction.WriteYellow($"Current stream used has AUDIO OUPUT: {currentAudioOutput.Name} [{currentAudioOutput.Path}]");
        ConsoleAbstraction.WriteYellow($"Select stream to use has AUDIO OUPUT:");
        int index = 0;
        if (audioOutputDevices?.Count > 0)
        {
            foreach (var audioOutput in audioOutputDevices)
                ConsoleAbstraction.WriteYellow($"\t[{index++}] - {audioOutput.Name} ({audioOutput.Path})");
        }

        ConsoleAbstraction.WriteYellow("\t[N] - No audio output");
        ConsoleAbstraction.WriteYellow("\t[C] - Cancel");

        var consoleKey = ConsoleAbstraction.ReadKey();
        if (consoleKey.HasValue && char.IsDigit(consoleKey.Value.KeyChar))
        {
            var selection = int.Parse(consoleKey.Value.KeyChar.ToString());
            if ((selection >= 0) && (selection < index))
            {
                selected = true;
                audioOutputUpdated = currentAudioOutput?.Path != audioOutputDevices?[selection]?.Path;
                if (audioOutputUpdated)
                {
                    currentAudioOutput = audioOutputDevices?[selection];
                    ConsoleAbstraction.WriteDarkYellow($"Stream used as Audio Output selected: {currentAudioOutput?.Name} ({currentAudioOutput?.Path})]");
                }
                else
                    ConsoleAbstraction.WriteDarkYellow($"Same Stream selected");
            }
        }
        else if (consoleKey.HasValue && (consoleKey.Value.Key == ConsoleKey.N))
        {
            audioOutputUpdated = true;
            currentAudioOutput = null;
            selected = true;
            ConsoleAbstraction.WriteDarkYellow($"No more audio output used ...");
        }

        else if (consoleKey.HasValue && consoleKey.Value.Key == ConsoleKey.C)
        {
            selected = true;
            ConsoleAbstraction.WriteDarkYellow($"Cancel used ...");
        }
        else
            ConsoleAbstraction.WriteDarkYellow($"Invalid key used ...");
    }

    if (audioOutputUpdated)
    {
        var previousSdl2AudioOutput = sdl2AudioOutput;
        SDL2AudioOutput? newSdl2AudioOutput = null;

        if (currentAudioOutput is not null)
        {
            newSdl2AudioOutput = new SDL2AudioOutput(currentAudioOutput);
            if (newSdl2AudioOutput?.Init() == true)
            {
                newSdl2AudioOutput.Start();
            }
        }

        // Store new one
        sdl2AudioOutput = newSdl2AudioOutput;

        // Close previous one
        previousSdl2AudioOutput?.Dispose();
    }
}

// Media: 'audio', 'video' (or 'composition' but not yet supported)
// Context: Audio, Video, Sharing
Stream? SelectStream(string media, Stream? currentStreamSelected, String context ) 
{
    Stream? stream = null;
    if (String.IsNullOrEmpty(media))
        return stream;

    media = media.ToLower();
    context = context.ToUpper();

    var subList = streamsList?.FindAll(s => s.Media.Contains(media)).ToList();
    if(subList?.Count > 0)
    {
        Boolean selected = false;
        while (!selected)
        {
            ConsoleAbstraction.WriteYellow("");
            if (currentStreamSelected != null)
                ConsoleAbstraction.WriteYellow($"Current stream used for {context}: {currentStreamSelected.Id} {((currentStreamSelected.UriType == "other") ? "" : "[" + currentStreamSelected.UriType + "] ")}({currentStreamSelected.Uri})");

            ConsoleAbstraction.WriteYellow($"Select stream to use has {context}:");
            int index = 0;
            foreach (var item in subList)
                ConsoleAbstraction.WriteYellow($"\t[{index++}] - {item.Id} {((item.UriType == "other") ? "" : "[" + item.UriType + "] ")}({item.Uri})");
            ConsoleAbstraction.WriteYellow("\t[C] - Cancel");

            var consoleKey = ConsoleAbstraction.ReadKey();
            if (consoleKey.HasValue && char.IsDigit(consoleKey.Value.KeyChar))
            {
                var selection = int.Parse(consoleKey.Value.KeyChar.ToString());
                if ((selection >= 0) && (selection < index))
                {
                    selected = true;
                    stream = subList[selection];
                    //ConsoleAbstraction.WriteDarkYellow($"Stream used as {media.ToUpper()} selected: {stream.Id} {((stream.UriType == "other") ? "" : "[" + stream.UriType + "] ")}({stream.Uri})");
                }
            }
            else if (consoleKey.HasValue && consoleKey.Value.Key == ConsoleKey.C)
            {
                selected = true;
                ConsoleAbstraction.WriteDarkYellow($"Cancel used ...");
            }
        }
    }

    return stream;
}

void MenuVideoStream()
{
    if (!RbTask.IsCompleted)
    {
        ConsoleAbstraction.WriteRed("Task is already in progress");
        return;
    }

    Action action = async () =>
    {
        var canSelectVideoDevice = false;
        if (currentCall?.IsActive() == true)
        {
            if (Rainbow.Util.MediasWithVideo(currentCall.LocalMedias))
            {
                var canContinue = true;
                while (canContinue)
                {
                    ConsoleAbstraction.WriteYellow("");
                    if (currentWebCam == null)
                        ConsoleAbstraction.WriteYellow($"A call is in progress WITH your VIDEO");
                    else
                        ConsoleAbstraction.WriteYellow($"A call is in progress WITH your VIDEO: {currentWebCam.Name} [{currentWebCam.Path}]");
                    ConsoleAbstraction.WriteYellow("\t[R] - Remove it from call");
                    ConsoleAbstraction.WriteYellow("\t[S] - Select another one and use it in the call");
                    ConsoleAbstraction.WriteYellow("\t[C] - Cancel");
                    var keyInfo = ConsoleAbstraction.ReadKey();
                    switch (keyInfo?.Key)
                    {
                        case ConsoleKey.R:
                            canContinue = false;
                            ConsoleAbstraction.WriteGreen("Asking to remove VIDEO ...");
                            await RemoveVideoAsync();
                            break;

                        case ConsoleKey.S:
                            canContinue = false;
                            canSelectVideoDevice = true;
                            break;

                        case ConsoleKey.C:
                            canContinue = false;
                            ConsoleAbstraction.WriteDarkYellow($"Cancel used ...");
                            break;

                        default:
                            canContinue = true;
                            ConsoleAbstraction.WriteDarkYellow($"Bad key used ...");
                            break;

                    }
                }
            }
            else
            {
                var canContinue = true;
                while (canContinue)
                {
                    ConsoleAbstraction.WriteYellow("");
                    if (currentWebCam == null)
                        ConsoleAbstraction.WriteYellow($"A call is in progress:");
                    else
                    {
                        ConsoleAbstraction.WriteYellow($"A call is in progress WITHOUT your VIDEO: {currentWebCam.Name} [{currentWebCam.Path}]");
                        ConsoleAbstraction.WriteYellow("\t[A] - Add it to the call");
                    }
                    ConsoleAbstraction.WriteYellow("\t[S] - Select stream and use it as VIDEO in the call");
                    ConsoleAbstraction.WriteYellow("\t[C] - Cancel");
                    var keyInfo = ConsoleAbstraction.ReadKey();
                    switch (keyInfo?.Key)
                    {
                        case ConsoleKey.A:
                            if (currentWebCam == null)
                            {
                                canContinue = true;
                                ConsoleAbstraction.WriteDarkYellow($"Bad key used ...");
                            }
                            else
                            {
                                canContinue = false;
                                ConsoleAbstraction.WriteGreen("Asking to add VIDEO ...");
                                await AddVideoAsync();
                            }
                            break;

                        case ConsoleKey.S:
                            canContinue = false;
                            canSelectVideoDevice = true;
                            ConsoleAbstraction.WriteDarkYellow($"Select a stream ...");
                            break;

                        case ConsoleKey.C:
                            canContinue = false;
                            ConsoleAbstraction.WriteDarkYellow($"Cancel used ...");
                            break;

                        default:
                            canContinue = true;
                            ConsoleAbstraction.WriteDarkYellow($"Bad key used ...");
                            break;

                    }
                }
            }
        }
        else
            canSelectVideoDevice = true;

        if (!canSelectVideoDevice)
            return;

        webcamDevices = Rainbow.Medias.Devices.GetWebcamDevices(false);
        Boolean webcamUpdated = false;
        Boolean selected = false;
        while (!selected)
        {
            ConsoleAbstraction.WriteYellow("");
            if (currentWebCam != null)
                ConsoleAbstraction.WriteYellow($"Current stream used has Video: {currentWebCam.Name} [{currentWebCam.Path}]");
            ConsoleAbstraction.WriteYellow($"Select stream to use has Video:");
            int index = 0;
            if (webcamDevices?.Count > 0)
            {
                foreach (var webcam in webcamDevices)
                    ConsoleAbstraction.WriteYellow($"\t[{index++}] - {webcam.Name} ({webcam.Path})");
            }

            if (streamsList?.Count > 0)
                ConsoleAbstraction.WriteYellow($"\t[S] - Select Video stream");

            ConsoleAbstraction.WriteYellow($"\t[N] - No video");
            ConsoleAbstraction.WriteYellow("\t[C] - Cancel");
            var consoleKey = ConsoleAbstraction.ReadKey();
            if (consoleKey.HasValue && char.IsDigit(consoleKey.Value.KeyChar))
            {
                var selection = int.Parse(consoleKey.Value.KeyChar.ToString());
                if ((selection >= 0) && (selection < index))
                {
                    selected = true;
                    webcamUpdated = currentWebCam?.Path != webcamDevices?[selection]?.Path;
                    if (webcamUpdated)
                    {
                        currentWebCam = webcamDevices?[selection];
                        ConsoleAbstraction.WriteDarkYellow($"Stream used as Video selected: {currentWebCam?.Name} ({currentWebCam?.Path})]");
                    }
                    else
                        ConsoleAbstraction.WriteDarkYellow($"Same Stream selected");
                }
            }
            else if (consoleKey.HasValue && (consoleKey.Value.Key == ConsoleKey.S) && (streamsList?.Count > 0))
            {
                var result = SelectStream("video", videoStream, "video");
                if (result is not null)
                {
                    selected = true;
                    if ((videoStream is null) || (videoStream.Id != result.Id))
                    {
                        useEmptyTrack = false;
                        webcamUpdated = true;
                        videoStream = result;
                        currentWebCam = new InputStreamDevice("video_stream", "video_stream", videoStream.Uri, withVideo: true, withAudio: false);
                        ConsoleAbstraction.WriteDarkYellow($"Stream used as Video selected: {currentWebCam.Name} ({currentWebCam.Path})]");
                    }
                    else
                        ConsoleAbstraction.WriteDarkYellow($"Same Video Stream selected");
                }
            }
            else if (consoleKey.HasValue && consoleKey.Value.Key == ConsoleKey.N)
            {
                webcamUpdated = true;
                currentWebCam = null;
                videoStream = null;
                selected = true;
                ConsoleAbstraction.WriteDarkYellow($"No more video used ...");
            }
            else if (consoleKey.HasValue && consoleKey.Value.Key == ConsoleKey.C)
            {
                selected = true;
                ConsoleAbstraction.WriteDarkYellow($"Cancel used ...");
            }
            else
                ConsoleAbstraction.WriteDarkYellow($"Invalid key used ...");
        }

        if (webcamUpdated)
        {
            var previousVideoTrack = videoTrack;

            if (currentWebCam == null)
            {
                if ((currentCall?.IsActive() == true) && Rainbow.Util.MediasWithVideo(currentCall.LocalMedias))
                {
                    ConsoleAbstraction.WriteGreen("Asking to remove Video from call ...");
                    await RemoveVideoAsync();
                }

                ConsoleAbstraction.WriteGreen("Asking to dispose of previous video track...");
                previousVideoTrack?.Dispose();
                previousVideoTrack = null;
                ConsoleAbstraction.WriteDarkYellow($"Previous Video track disposed");
            }
            else // currentWebCam != null
            {
                try
                {
                    ConsoleAbstraction.WriteGreen("Asking to create new video track...");
                    videoTrack = RbWebRTCDesktopFactory.CreateVideoTrack(currentWebCam, false);

                    ConsoleAbstraction.WriteDarkYellow($"New Video track created");
                }
                catch (Exception exc)
                {
                    ConsoleAbstraction.WriteRed($"Cannot create video track: Exception: [{exc}]");
                    videoTrack = previousVideoTrack;
                    return;
                }

                Boolean success = true;

                // Update call (if any) with this new track
                if (currentCall?.IsActive() == true)
                {
                    if (Rainbow.Util.MediasWithVideo(currentCall.LocalMedias))
                    {
                        ConsoleAbstraction.WriteGreen("Asking to update Video in the call ...");
                        success = await UpdateVideoAsync();
                    }
                    else
                    {
                        ConsoleAbstraction.WriteGreen("Asking to add Video in the call ...");
                        success = await AddVideoAsync();
                    }
                }

                if (success)
                {
                    ConsoleAbstraction.WriteGreen("Asking to dispose of previous video track...");
                    previousVideoTrack?.Dispose();
                    previousVideoTrack = null;
                    ConsoleAbstraction.WriteDarkYellow($"Previous Video track disposed");
                }
            }
        }
    };
    RbTask = Task.Run(action);
}

void MenuSharingStream()
{
    if (!RbTask.IsCompleted)
    {
        ConsoleAbstraction.WriteRed("Task is already in progress");
        return;
    }

    Action action = async () =>
    {
        var canSelectSharingDevice = false;
        if (currentCall?.IsActive() == true)
        {
            if (Rainbow.Util.MediasWithSharing(currentCall.LocalMedias))
            {
                var canContinue = true;
                while (canContinue)
                {
                    ConsoleAbstraction.WriteYellow("");
                    if (currentScreen == null)
                        ConsoleAbstraction.WriteYellow($"A call is in progress WITH your Sharing");
                    else
                        ConsoleAbstraction.WriteYellow($"A call is in progress WITH your Sharing: {currentScreen.Name} [{currentScreen.Path}]");
                    ConsoleAbstraction.WriteYellow("\t[R] - Remove it from call");
                    ConsoleAbstraction.WriteYellow("\t[S] - Select another one and use it in the call");
                    ConsoleAbstraction.WriteYellow("\t[C] - Cancel");
                    var keyInfo = ConsoleAbstraction.ReadKey();
                    switch (keyInfo?.Key)
                    {
                        case ConsoleKey.R:
                            canContinue = false;
                            ConsoleAbstraction.WriteGreen("Asking to remove Sharing ...");
                            await RemoveSharingAsync();
                            break;

                        case ConsoleKey.S:
                            canContinue = false;
                            canSelectSharingDevice = true;
                            break;

                        case ConsoleKey.C:
                            canContinue = false;
                            ConsoleAbstraction.WriteDarkYellow($"Cancel used ...");
                            break;

                        default:
                            canContinue = true;
                            ConsoleAbstraction.WriteDarkYellow($"Bad key used ...");
                            break;

                    }
                }
            }

            else
            {
                var canContinue = true;
                while (canContinue)
                {
                    ConsoleAbstraction.WriteYellow("");
                    if (currentScreen == null)
                        ConsoleAbstraction.WriteYellow($"A call is in progress:");
                    else
                    {
                        ConsoleAbstraction.WriteYellow($"A call is in progress WITHOUT your Sharing: {currentScreen.Name} [{currentScreen.Path}]");
                        ConsoleAbstraction.WriteYellow("\t[A] - Add it to the call");
                    }
                    ConsoleAbstraction.WriteYellow("\t[S] - Select stream and use it as Sharing in the call");
                    ConsoleAbstraction.WriteYellow("\t[C] - Cancel");
                    var keyInfo = ConsoleAbstraction.ReadKey();
                    switch (keyInfo?.Key)
                    {
                        case ConsoleKey.A:
                            if (currentScreen == null)
                            {
                                canContinue = true;
                                ConsoleAbstraction.WriteDarkYellow($"Bad key used ...");
                            }
                            else
                            {
                                canContinue = false;
                                //if (Rainbow.Util.MediasWithSharing(currentCall.RemoteMedias))
                                //    ConsoleAbstraction.WriteRed($"A call is in progress with already a sharing. Cannot set another one");
                                //else
                                {
                                    ConsoleAbstraction.WriteGreen("Asking to add sharing ...");
                                    await AddSharingAsync();
                                }
                            }
                            break;

                        case ConsoleKey.S:
                            canContinue = false;
                            canSelectSharingDevice = true;
                            ConsoleAbstraction.WriteDarkYellow($"Select a stream ...");
                            break;

                        case ConsoleKey.C:
                            canContinue = false;
                            ConsoleAbstraction.WriteDarkYellow($"Cancel used ...");
                            break;

                        default:
                            canContinue = true;
                            ConsoleAbstraction.WriteDarkYellow($"Bad key used ...");
                            break;

                    }
                }
            }
        }
        else
            canSelectSharingDevice = true;

        if (!canSelectSharingDevice)
            return;

        screenDevices = Rainbow.Medias.Devices.GetScreenDevices(false);
        Boolean screenUpdated = false;
        Boolean selected = false;
        while (!selected)
        {
            ConsoleAbstraction.WriteYellow("");
            if (currentScreen != null)
                ConsoleAbstraction.WriteYellow($"Current stream used has Sharing: {currentScreen.Name} [{currentScreen.Path}]");
            ConsoleAbstraction.WriteYellow($"Select stream to use has Sharing:");
            int index = 0;
            if (screenDevices?.Count > 0)
            {
                foreach (var screen in screenDevices)
                    ConsoleAbstraction.WriteYellow($"\t[{index++}] - {screen.Name} ({screen.Path})");
            }

            if (streamsList?.Count > 0)
                ConsoleAbstraction.WriteYellow($"\t[S] - Select Sharing stream");

            ConsoleAbstraction.WriteYellow($"\t[N] - No sharing");
            ConsoleAbstraction.WriteYellow("\t[C] - Cancel");
            var consoleKey = ConsoleAbstraction.ReadKey();
            if (consoleKey.HasValue && char.IsDigit(consoleKey.Value.KeyChar))
            {
                var selection = int.Parse(consoleKey.Value.KeyChar.ToString());
                if ((selection >= 0) && (selection < index))
                {
                    selected = true;
                    screenUpdated = currentScreen?.Name != screenDevices?[selection]?.Name;
                    if (screenUpdated)
                    {
                        currentScreen = screenDevices?[selection];
                        ConsoleAbstraction.WriteDarkYellow($"Stream used as Sharing selected: {currentScreen?.Name} ({currentScreen?.Path})]");
                    }
                    else
                        ConsoleAbstraction.WriteDarkYellow($"Same Stream selected");
                }
            }
            else if (consoleKey.HasValue && (consoleKey.Value.Key == ConsoleKey.S) && (streamsList?.Count > 0))
            {
                var result = SelectStream("video", sharingStream, "sharing");
                if (result is not null)
                {
                    selected = true;
                    if ((sharingStream is null) || (sharingStream.Id != result.Id))
                    {
                        useEmptyTrack = false;
                        screenUpdated = true;
                        sharingStream = result;
                        currentScreen = new InputStreamDevice("sharing_stream", "sharing_stream", sharingStream.Uri, withVideo: true, withAudio: false);
                        ConsoleAbstraction.WriteDarkYellow($"Stream used as Video selected: {currentScreen.Name} ({currentScreen.Path})]");
                    }
                    else
                        ConsoleAbstraction.WriteDarkYellow($"Same Video Stream selected");
                }
            }
            else if (consoleKey.HasValue && consoleKey.Value.Key == ConsoleKey.N)
            {
                screenUpdated = true;
                currentScreen = null;
                selected = true;
                ConsoleAbstraction.WriteDarkYellow($"No more sharing used ...");
            }
            else if (consoleKey.HasValue && consoleKey.Value.Key == ConsoleKey.C)
            {
                selected = true;
                ConsoleAbstraction.WriteDarkYellow($"Cancel used ...");
            }
            else
                ConsoleAbstraction.WriteDarkYellow($"Invalid key used ...");
        }

        if (screenUpdated)
        {
            var previousSharingTrack = sharingTrack;

            if (currentScreen == null)
            {
                if ((currentCall?.IsActive() == true) && Rainbow.Util.MediasWithSharing(currentCall.LocalMedias))
                {
                    ConsoleAbstraction.WriteGreen("Asking to remove Sharing from call ...");
                    await RemoveSharingAsync();
                }

                ConsoleAbstraction.WriteGreen("Asking to dispose of previous Sharing track...");
                previousSharingTrack?.Dispose();
                previousSharingTrack = null;
                ConsoleAbstraction.WriteDarkYellow($"Previous Sharing track disposed");
            }
            else // currentScreen != null
            {
                Boolean success = true;
                //if ((currentCall?.IsActive() == true) && Rainbow.Util.MediasWithSharing(currentCall.RemoteMedias) && !Rainbow.Util.MediasWithSharing(currentCall.LocalMedias))
                //{
                //    ConsoleAbstraction.WriteRed($"A call is in progress with already a sharing. Cannot set another one");
                //}
                //else
                {
                    try
                    {
                        ConsoleAbstraction.WriteGreen("Asking to create new Sharing track...");
                        sharingTrack = RbWebRTCDesktopFactory.CreateVideoTrack(currentScreen, true);

                        ConsoleAbstraction.WriteDarkYellow($"New Sharing track created");

                        // Update call (if any) with this new track
                        if (currentCall?.IsActive() == true)
                        {
                            if (Rainbow.Util.MediasWithSharing(currentCall.LocalMedias))
                            {
                                ConsoleAbstraction.WriteGreen("Asking to update sharing in the call ...");
                                success = await UpdateSharingAsync();
                            }
                            else
                            {
                                ConsoleAbstraction.WriteGreen("Asking to add sharing in the call ...");
                                success = await AddSharingAsync();
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        ConsoleAbstraction.WriteRed($"Cannot create sharing track: Exception: [{exc}]");
                        sharingTrack = previousSharingTrack;
                        return;
                    }
                }

                if (success)
                {
                    ConsoleAbstraction.WriteGreen("Asking to dispose of previous sharing track...");
                    previousSharingTrack?.Dispose();
                    previousSharingTrack = null;
                    ConsoleAbstraction.WriteDarkYellow($"Previous sharing track disposed");
                }
            }
        }
    };
    RbTask = Task.Run(action);
}

void MenuListDevicesAndConferenceInfo()
{
    ConsoleAbstraction.WriteGreen($"{CR}Asking to list devices and call info (conference or P2P) ...");
    ListUserInfo();
    ConsoleAbstraction.WriteYellow("");
    ListDevicesUsed();
    ConsoleAbstraction.WriteYellow("");
    DisplayConferenceInfo();
    ConsoleAbstraction.WriteYellow("");
    DisplayP2PInfo();
    ConsoleAbstraction.WriteYellow("");
    ListAudioInputDevices();
    ConsoleAbstraction.WriteYellow("");
    ListAudioOutputDevices();
    ConsoleAbstraction.WriteYellow("");
    ListScreens();
    ConsoleAbstraction.WriteYellow("");
    ListWebCams();
}

void MenuRTPStreamOptions()
{
    if(RbWebRTCDesktopFactory is not null)
    {
        var audioOptionsFilename = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}rtpAudioOptions.json";
        if (File.Exists(audioOptionsFilename))
        {
            try
            {
                var content = File.ReadAllText(audioOptionsFilename);
                Dictionary<String, String>? audioOptions = JSON.Parse(content);

                if (audioOptions?.Count > 0)
                {
                    RbWebRTCDesktopFactory.SetRTPAudioOptions(audioOptions);
                    ConsoleAbstraction.WriteDarkYellow("RTP Audio Options updated");
                }
                else
                {
                    RbWebRTCDesktopFactory.SetRTPAudioOptions(null);
                    ConsoleAbstraction.WriteDarkYellow("RTP Audio Options has been reset");
                }
            }
            catch
            {

            }
        }

        var videoOptionsFilename = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}rtpVideoOptions.json";
        if (File.Exists(videoOptionsFilename))
        {
            try
            {
                var content = File.ReadAllText(videoOptionsFilename);
                Dictionary<String, String>? videoOptions = JSON.Parse(content);
                if (videoOptions?.Count > 0)
                {
                    RbWebRTCDesktopFactory.SetRTPVideoOptions(videoOptions);
                    ConsoleAbstraction.WriteDarkYellow("RTP Video Options updated");
                }
                else
                {
                    RbWebRTCDesktopFactory.SetRTPVideoOptions(null);
                    ConsoleAbstraction.WriteDarkYellow("RTP Video Options has been reset");
                }
            }
            catch
            {

            }
        }


    }
}

void DisplayP2PInfo()
{
    ConsoleAbstraction.WriteDarkYellow("P2P call:");
    if ( (currentCall?.IsActive() == true) && (!currentCall.IsConference))
    {
        ConsoleAbstraction.WriteYellow($"\t{currentCall}");
    }
    else
    {
        ConsoleAbstraction.WriteYellow("\tYou are not in a P2P call");
    }
}

void DisplayConferenceInfo()
{
    ConsoleAbstraction.WriteDarkYellow("Conference:");
    if ( (currentCall?.IsActive() == true) && (currentCall.IsConference))
    {
        ConsoleAbstraction.WriteYellow($"{currentCall}");
    }
    else
    {
        ConsoleAbstraction.WriteYellow("\tYou are not in a conference");
        if (conferencesInProgress.Count > 0)
            ConsoleAbstraction.WriteYellow($"\tConference(s) in progress - Nb:[{conferencesInProgress.Count}]");
        else
            ConsoleAbstraction.WriteYellow($"\tNo conference in progress");
    }
}

void ListUserInfo()
{
    if (RbApplication.IsConnected())
        ConsoleAbstraction.WriteDarkYellow($"Connected with:[{RbContacts.GetCurrentContact().Peer.DisplayName}]");
    else
        ConsoleAbstraction.WriteDarkYellow("Current user is not connected ...");
}

async Task JoinConferenceAsync(Conference conference)
{
    if ((currentCallId == null) && (conference != null))
    {
        var presence = RbContacts.GetPresence();
        RbContacts.SavePresenceForRollback();
        ConsoleAbstraction.WriteBlue($"SavePresenceForRollback - Presence:[{presence.ToString(DetailsLevel.Small)}]");

        SdkResult<String> sdkResult;
        if (audioTrack is null)
        {
            ConsoleAbstraction.WriteGreen($"Joining conference without AUDIO INPUT ...");
            sdkResult = await RbWebRTCCommunications.JoinConferenceAsync(conference.Peer.Id, Media.VIDEO + Media.DATACHANNEL);
        }
        else
        {
            if(useEmptyTrack)
            {
                ConsoleAbstraction.WriteGreen($"Joining conference with AUDIO INPUT (EMPTY TRACK) ...");
                sdkResult = await RbWebRTCCommunications.JoinConferenceAsync(conference.Peer.Id, audioTrack);
            }
            else
            {
                ConsoleAbstraction.WriteGreen($"Joining conference with AUDIO INPUT ...");
                sdkResult = await RbWebRTCCommunications.JoinConferenceAsync(conference.Peer.Id, audioTrack);
            }
        }

        if(sdkResult.Success)
            ConsoleAbstraction.WriteDarkYellow($"Conference has been joined: [{conference.ToString(DetailsLevel.Medium)}]");
        else
            ConsoleAbstraction.WriteRed($"Cannot join Conference:[{sdkResult.Result}]");
    }
    else
        ConsoleAbstraction.WriteRed($"Cannot join Conference - current call in progress or conference object null");
}

async Task AnswerP2PAsync()
{
    if ((currentCall is not null) && (currentCall.CallStatus == CallStatus.RINGING_INCOMING))
    {
        if (audioTrack is null)
        {
            ConsoleAbstraction.WriteRed($"You don't have create an Audio Input Track - you cannot take the call");

            ConsoleAbstraction.WriteGreen($"{CR}Asking to reject P2P call...");
            await RbWebRTCCommunications.RejectCallAsync(currentCall.Id);
            ConsoleAbstraction.WriteDarkYellow($"P2P has been rejected");
        }
        else
        {
            // Set tracks used to make P2P call
            Dictionary<int, IMediaStreamTrack?>? mediaStreamTracks = new()
            {
                {Media.AUDIO, audioTrack }
            };

            if ((Rainbow.Util.MediasWithVideo(currentCall.RemoteMedias) && (videoTrack is not null)))
            {
                ConsoleAbstraction.WriteYellow($"Do you want to answer with video to this P2P call ? [Y]");
                var keyInfo = ConsoleAbstraction.ReadKey();
                if (keyInfo?.Key == ConsoleKey.Y)
                {
                    mediaStreamTracks.Add(Media.VIDEO, videoTrack);
                }
            }

            // We save current presence
            var presence = RbContacts.GetPresence();
            RbContacts.SavePresenceForRollback();
            ConsoleAbstraction.WriteBlue($"SavePresenceForRollback - Presence:[{presence.ToString(DetailsLevel.Small)}]");

            ConsoleAbstraction.WriteGreen($"{CR}Answering P2P call...");
            var sdkResult = await RbWebRTCCommunications.AnswerCallAsync(currentCall.Id, mediaStreamTracks);
            if (sdkResult.Success)
                ConsoleAbstraction.WriteDarkYellow($"P2P call has been answered");
            else
                ConsoleAbstraction.WriteRed($"Cannot answer Call:[{sdkResult.Result}]");
        }
    }
    else
        ConsoleAbstraction.WriteRed($"Cannot answer call - current call is nul or not RINGING_INCOMING");
}

void MenuEscape()
{
    if (!RbTask.IsCompleted)
    {
        ConsoleAbstraction.WriteRed("Task is already in progress");
        return;
    }

    Action action = async () =>
    {
        ConsoleAbstraction.WriteGreen($"Asked to end process using [ESC] key");
        if (RbApplication.IsConnected())
        {
            if (currentCall?.Id != null)
            {
                ConsoleAbstraction.WriteGreen("Asking to leave call ...");
                await RbWebRTCCommunications.HangUpCallAsync(currentCall.Id);
                ConsoleAbstraction.WriteDarkYellow($"CAll has been left");
            }

            ConsoleAbstraction.WriteGreen($"Asked to quit - Logout");
            var sdkResultBoolean = await RbApplication.LogoutAsync();
            if (!sdkResultBoolean.Success)
                ConsoleAbstraction.WriteRed($"Cannot logout - SdkError:[{sdkResultBoolean.Result}]");
        }
        System.Environment.Exit(0);
    };
    RbTask = Task.Run(action);
}

async Task UnsubscribeToMediaAsync(int media)
{
    if (String.IsNullOrEmpty(currentCallId)) return;

    var publications = RbWebRTCCommunications.GetMediaPublicationsSubscribed(currentCallId);
    if (publications?.Count > 0)
    {
        foreach (var pub in publications)
        {
            if (pub.Media == media)
            {
                ConsoleAbstraction.WriteGreen($"Asking to unsubscribe {Rainbow.Util.MediasToString(media)} MediaPublication ...");
                var sdkResult = await RbWebRTCCommunications.UnsubscribeToMediaPublicationAsync(pub);
                if (sdkResult.Success)
                    ConsoleAbstraction.WriteDarkYellow($"Previous {Rainbow.Util.MediasToString(media)} MediaPublication has been unsubscribed");
                else
                    ConsoleAbstraction.WriteDarkYellow($"Previous {Rainbow.Util.MediasToString(media)} MediaPublication has NOT been unsubscribed:{sdkResult.Result}");
                return;
            }
        }
    }
}

#region AUDIO INPUT - add, remove in the call

async Task<Boolean> AddAudioInputAsync()
{
    if (currentCall?.IsActive() == true)
    {
        if (audioTrack == null)
        {
            ConsoleAbstraction.WriteRed($"Cannot add AUDIO Input - No Track available");
            return false;
        }

        var sdkResult = await RbWebRTCCommunications.AddAudioAsync(currentCall.Id, audioTrack, "Audio name", "Audio desc", null);
        if (sdkResult.Success)
        {
            ConsoleAbstraction.WriteDarkYellow($"Audio Input added in call and started");
            return true;
        }
        else
        {
            ConsoleAbstraction.WriteRed($"Cannot add Audio Input - SdkError:[{sdkResult.Result}]");
            return false;
        }
    }
    else
    {
        ConsoleAbstraction.WriteRed($"Cannot add Audio Input - call is not active");
        return true;
    }
}

async Task<Boolean> UpdateAudioInputAsync()
{
    if (currentCall?.IsActive() == true)
    {
        if (audioTrack == null)
        {
            ConsoleAbstraction.WriteRed($"Cannot add AUDIO Input - No Track available");
            return false;
        }

        var sdkResult = await RbWebRTCCommunications.ChangeAudioAsync(currentCall.Id, audioTrack);
        if (sdkResult.Success)
        {
            ConsoleAbstraction.WriteDarkYellow($"AUDIO Input updated in call and started");
            return true;
        }
        else
        {
            ConsoleAbstraction.WriteRed($"Cannot update AUDIO Input - SdkError:[{sdkResult.Result}]");
            return false;
        }
    }
    else
    {
        ConsoleAbstraction.WriteRed($"Cannot update AUDIO Input - call is not active");
        return false;
    }
}

#endregion AUDIO INPUT - add, remove in the call

#region DATACHANNEL - add, remove in the call, send / receive data
async Task<Boolean> RemoveDataChannelAsync()
{
    if (currentCall?.IsActive() == true)
    {
        // Update timer to send data on the DC
        dcSendWatcher.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        var sdkResult = await RbWebRTCCommunications.RemoveDataChannelAsync(currentCall.Id);
        if (sdkResult.Success)
        {
            ConsoleAbstraction.WriteDarkYellow($"DataChannel removed from call");
            dcLocal = null;
            return true;
        }
        else
        {
            ConsoleAbstraction.WriteRed($"Cannot remove DataChannel - SdkError:[{sdkResult.Result}]");
            return false;
        }
    }
    else
    {
        ConsoleAbstraction.WriteRed($"Cannot remove DataChannel - call is not active");
        return false;
    }
}

async Task<Boolean> AddDataChannelAsync()
{
    if (currentCall?.IsActive() == true)
    {
        var sdkResult = await RbWebRTCCommunications.AddDataChannelAsync(currentCall.Id, "DC name", "DC desc", null, null);
        if (sdkResult.Success)
        {
            ConsoleAbstraction.WriteDarkYellow($"DataChannel added in call and started");

            if(dcLocal != null)
            {
                dcLocal.OnClose -= DcLocal_OnClose;
            }

            dcLocal = sdkResult.Data;
            dcLocal.OnClose += DcLocal_OnClose;

            // Update time to send data on the DC
            dcSendWatcher.Change(TimeSpan.Zero, delayDCMsg);
            return true;
        }
        else
        {
            ConsoleAbstraction.WriteRed($"Cannot add DataChannel - SdkError:[{sdkResult.Result}]");
            return false;
        }
    }
    else
    {
        ConsoleAbstraction.WriteRed($"Cannot add DataChannel - call is not active");
        return false;
    }
}

void SendDataOnDc()
{
    if (dcLocal?.ReadyState == DataChannelState.open)
    {
        dcLocal?.Send($"msg [{Rainbow.Util.GetGUID()}]");
    }
}

#endregion DATACHANNEL - add, remove in the call, send / receive data

#region LOCAL VIDEO - add, remove, update in the call
async Task<Boolean> RemoveVideoAsync()
{
    if (currentCall?.IsActive() == true)
    {
        var sdkResult = await RbWebRTCCommunications.RemoveVideoAsync(currentCall.Id);
        if (sdkResult.Success)
        {
            ConsoleAbstraction.WriteDarkYellow($"Video removed from call");
            return true;
        }
        else
        {
            ConsoleAbstraction.WriteRed($"Cannot remove video - SdkError:[{sdkResult.Result}]");
            return false;
        }
    }
    else
    { 
        ConsoleAbstraction.WriteRed($"Cannot remove video - call is not active");
        return false;
    }
}

async Task<Boolean> AddVideoAsync()
{
    if (currentCall?.IsActive() == true)
    {
        if (videoTrack == null)
        {
            ConsoleAbstraction.WriteRed($"Cannot add video - No Video Track available");
            return false;
        }

        var sdkResult = await RbWebRTCCommunications.AddVideoAsync(currentCall.Id, videoTrack, "Video name", "Video desc", null);
        if (sdkResult.Success)
        {
            ConsoleAbstraction.WriteDarkYellow($"Video added in call and started");
            return true;
        }
        else
        {
            ConsoleAbstraction.WriteRed($"Cannot add video - SdkError:[{sdkResult.Result}]");
            return false;
        }
    }
    else
    { 
        ConsoleAbstraction.WriteRed($"Cannot add video - call is not active");
        return true;
    }
}

async Task<Boolean> UpdateVideoAsync()
{
    if (currentCall?.IsActive() == true)
    {
        if (videoTrack == null)
        {
            ConsoleAbstraction.WriteRed($"Cannot add video - No Video Track available");
            return false;
        }

        var sdkResult = await RbWebRTCCommunications.ChangeVideoAsync(currentCall.Id, videoTrack);
        if (sdkResult.Success)
        {
            ConsoleAbstraction.WriteDarkYellow($"Video updated in call and started");
            return true;
        }
        else
        {
            ConsoleAbstraction.WriteRed($"Cannot update video - SdkError:[{sdkResult.Result}]");
            return false;
        }
    }
    else
    {
        ConsoleAbstraction.WriteRed($"Cannot update video - call is not active");
        return false;
    }
}

#endregion LOCAL VIDEO - add, remove, update in the call

#region LOCAL SHARING - add, remove, update in the call

async Task<Boolean> RemoveSharingAsync()
{
    if (currentCall?.IsActive() == true)
    {
        var sdkResult = await RbWebRTCCommunications.RemoveSharingAsync(currentCall.Id);
        if (sdkResult.Success)
        {
            ConsoleAbstraction.WriteDarkYellow($"Sharing removed from call");
            return true;
        }
        else
        { 
            ConsoleAbstraction.WriteRed($"Cannot remove Sharing - SdkError:[{sdkResult.Result}]");
            return false;
        }
    }
    else
    {
        ConsoleAbstraction.WriteRed($"Cannot remove Sharing - call is not active");
        return false;
    }
}

async Task<Boolean> AddSharingAsync()
{
    if (currentCall?.IsActive() == true)
    {
        if (sharingTrack == null)
        {
            ConsoleAbstraction.WriteRed($"Cannot add sharing - No Sharing Track available");
            return false;
        }

        var sdkResult = await RbWebRTCCommunications.AddSharingAsync(currentCall.Id, sharingTrack, "Sharing name", "Sharing desc", null);
        if (sdkResult.Success)
        { 
            ConsoleAbstraction.WriteDarkYellow($"Sharing added in call and started");
            return true;
        }
        else
        { 
            ConsoleAbstraction.WriteRed($"Cannot add Sharing - SdkError:[{sdkResult.Result}]");
            return false;
        }
    }
    else
    { 
        ConsoleAbstraction.WriteRed($"Cannot add Sharing - call is not active");
        return false;
    }
}

async Task<Boolean> UpdateSharingAsync()
{
    if (currentCall?.IsActive() == true)
    {
        if (sharingTrack == null)
        {
            ConsoleAbstraction.WriteRed($"Cannot add sharing - No Sharing Track available");
            return false;
        }

        var sdkResult = await RbWebRTCCommunications.ChangeSharingAsync(currentCall.Id, sharingTrack);
        if (sdkResult.Success)
        {
            ConsoleAbstraction.WriteDarkYellow($"Sharing updated in call and started");
            return true;
        }
        else
        {
            ConsoleAbstraction.WriteRed($"Cannot update Sharing - SdkError:[{sdkResult.Result}]");
            return false;
        }
    }
    else
    {
        ConsoleAbstraction.WriteRed($"Cannot update Sharing - call is not active");
        return false;
    }
}

#endregion SHARING - add, remove, update in the call

#region DEVICES - List audio output, audio input, webcam, screens

void ListDevicesUsed()
{
    ConsoleAbstraction.WriteDarkYellow("List devices used:");

    //Audio Input
    if (audioTrack is null )
    {
        ConsoleAbstraction.WriteYellow("\t AUDIO IN  - stream used: NONE");
    }
    else
    {
        if (useEmptyTrack)
            ConsoleAbstraction.WriteYellow($"\t AUDIO IN  - stream used: EMPTY TRACK");
        else if (currentAudioInput != null)
            ConsoleAbstraction.WriteYellow($"\t AUDIO IN  - stream used: {currentAudioInput.Name} [{currentAudioInput.Path}]");
        else
        {
            ConsoleAbstraction.WriteRed($"\t AUDIO IN  - stream used: bad config ....");
        }
        if ((currentCall?.IsActive() == true) && Rainbow.Util.MediasWithAudio(currentCall.LocalMedias))
            ConsoleAbstraction.WriteYellow($"\t\t => this stream is currently used in the call");
    }

    //Audio Output
    if (currentAudioOutput == null)
        ConsoleAbstraction.WriteYellow("\t AUDIO OUT - stream used: NONE");
    else
    {
        ConsoleAbstraction.WriteYellow($"\t AUDIO OUT - stream used: {currentAudioOutput.Name} [{currentAudioOutput.Path}]");
        if ((currentCall?.IsActive() == true) && Rainbow.Util.MediasWithAudio(currentCall.LocalMedias))
            ConsoleAbstraction.WriteYellow($"\t\t => this stream is currently used in the call");
    }

    // Video
    if (currentWebCam == null)
        ConsoleAbstraction.WriteYellow("\t VIDEO     - stream used: NONE");
    else
    {
        ConsoleAbstraction.WriteYellow($"\t VIDEO     - stream used: {currentWebCam.Name} [{currentWebCam.Path}]");
        if ((currentCall?.IsActive() == true) && Rainbow.Util.MediasWithVideo(currentCall.LocalMedias))
            ConsoleAbstraction.WriteYellow($"\t\t => this stream is currently used in the call");
    }
    // Sharing
    if (currentScreen == null)
        ConsoleAbstraction.WriteYellow("\t SHARING   - stream used: NONE");
    else
    {
        ConsoleAbstraction.WriteYellow($"\t SHARING   - stream used : {currentScreen.Name} [{currentScreen.Path}]");
        if ((currentCall?.IsActive() == true) && Rainbow.Util.MediasWithSharing(currentCall.LocalMedias))
            ConsoleAbstraction.WriteYellow($"\t\t => this stream is currently used in the call");
    }

    // DC
    if (dcLocal == null)
        ConsoleAbstraction.WriteYellow("\t DC        - stream used: NONE");
    else
    {
        ConsoleAbstraction.WriteYellow($"\t DC       - stream used: YES");
        if ((currentCall?.IsActive() == true) && Rainbow.Util.MediasWithDataChannel(currentCall.LocalMedias))
            ConsoleAbstraction.WriteYellow($"\t\t => this stream is currently used in the call");
    }
}

void ListAudioOutputDevices()
{
    var audioOutputs = Devices.GetAudioOutputDevices();
    if (audioOutputs?.Count > 0)
    {
        ConsoleAbstraction.WriteDarkYellow($"Audio OUTPUT(s) available:");
        foreach (var audio in audioOutputs)
        {
            ConsoleAbstraction.WriteYellow($"\t[{audio}]");
        }
    }
    else
        ConsoleAbstraction.WriteYellow($"No Audio OUTPUT available");
}

void ListAudioInputDevices()
{
    var audioInputs = Devices.GetAudioInputDevices();
    if (audioInputs?.Count > 0)
    {
        ConsoleAbstraction.WriteDarkYellow($"Audio INPUT(s) available:");
        foreach (var audio in audioInputs)
        {
            ConsoleAbstraction.WriteYellow($"\t[{audio}]");
        }
    }
    else
        ConsoleAbstraction.WriteYellow($"No Audio INPUT available");
}

void ListWebCams()
{
    var webcams = Devices.GetWebcamDevices(false);
    if (webcams?.Count > 0)
    {
        ConsoleAbstraction.WriteDarkYellow($"Webcam(s) available:");
        foreach (var webcam in webcams)
        {
            ConsoleAbstraction.WriteYellow($"\tWebcam:[{webcam}]");
        }
    }
    else
        ConsoleAbstraction.WriteYellow($"No Webcam available");
}

void ListScreens()
{
    var screens = Devices.GetScreenDevices(false);
    if (screens?.Count > 0)
    {
        ConsoleAbstraction.WriteDarkYellow($"Screen(s) available:");
        foreach (var screen in screens)
            ConsoleAbstraction.WriteYellow($"\tScreen:[{screen}]");
    }
    else
        ConsoleAbstraction.WriteYellow($"No Screen available");
}

#endregion DEVICES - List audio output, audio input, webcam, screens

#region DEFINE METHODS TO RECEIVE EVENTS FROM RAINBOW SDK (Core)

void RbApplication_ConnectionStateChanged(Rainbow.Model.ConnectionState connectionState)
{
    ConsoleAbstraction.WriteDateTime();
    switch (connectionState.Status)
    {
        case ConnectionStatus.Connected:
            ConsoleAbstraction.WriteBlue($"[Application.ConnectionStateChanged] Connection Status: [{connectionState.Status}]");
            break;

        case ConnectionStatus.Disconnected:
            ConsoleAbstraction.WriteRed($"[Application.ConnectionStateChanged] Connection Status: [{connectionState.Status}]");
            break;

        case ConnectionStatus.Connecting:
        default:
            ConsoleAbstraction.WriteBlue($"[Application.ConnectionStateChanged] - Connection Status: [{connectionState.Status}] - AutoReconnection.CurrentNbAttempts: [{RbAutoReconnection.CurrentNbAttempts}]");
            break;
    }
}

void RbAutoReconnection_Cancelled(SdkError sdkError)
{
    ConsoleAbstraction.WriteDateTime();
    if (sdkError.Type != SdkErrorType.NoError)
        ConsoleAbstraction.WriteRed($"[AutoReconnection.Cancelled] SdkError:[{sdkError}]");
    else
        ConsoleAbstraction.WriteBlue($"[AutoReconnection.Cancelled]");

    // If the AutoReconnection service is cancelled, we quit the process
    endProgram = true;
}

void RbConferences_ConferenceUpdated(Conference conference)
{
    lock (conferencesInProgress)
    {
        if (conference.Active)
        {
            if (!conferencesInProgress.Contains(conference.Peer.Id))
            {
                conferencesInProgress.Add(conference.Peer.Id);
                ConsoleAbstraction.WriteDateTime();
                ConsoleAbstraction.WriteBlue($"[ConferenceUpdated] A conference is active - Id:[{conference.Peer.Id}]");
            }
        }
        else
        {
            if (conferencesInProgress.Contains(conference.Peer.Id))
            {
                conferencesInProgress.Remove(conference.Peer.Id);
                ConsoleAbstraction.WriteDateTime();
                ConsoleAbstraction.WriteBlue($"[ConferenceUpdated] A conference is NO MORE active - Id:[{conference.Peer.Id}]");
            }
        }
    }
}

void RbConferences_ConferenceRemoved(Conference conference)
{
    ConsoleAbstraction.WriteDateTime();
    RbConferences_ConferenceUpdated(conference);
}

#endregion DEFINE METHODS TO RECEIVE EVENTS FROM RAINBOW SDK  (Core)

#region DEFINE METHODS TO RECEIVE EVENTS FROM SDK WebRTC

async void RbWebRTCCommunications_CallUpdated(Call call)
{
    if (call == null)
    {
        ConsoleAbstraction.WriteRed($"[CallUpdated] Call object is null ...");
        return;
    }

    // If we don't currently manage a call, we take the first new one into account
    if (currentCallId == null)
    {
        currentCallId = call.Id;
        if (!call.IsConference)
        {
            if(call.IsRinging())
                ConsoleAbstraction.WriteBlue($"[CallUpdated] New P2P call with:[{call.Peer.ToString(DetailsLevel.Small)}] - Medias:[{Rainbow.Util.MediasToString(call.RemoteMedias)}]");
        }
    }

    if (call.Id == currentCallId)
    {
        if (!call.IsInProgress())
        {
            currentCallId = null;
            ConsoleAbstraction.WriteBlue($"[CallUpdated] Reset current Call");
            currentCall = null;

            // Hide any window
            _actions.Add(new Action(() =>
            {
                Window.Hide(_windowSharing);
                Window.Hide(_windowVideo);
            }));

            // The call is NO MORE in Progress => We Rollback presence if any
            var presenceRollback = RbContacts.GetPresenceSavedForRollback();
            if (presenceRollback is not null)
            {
                var sdkResult = await RbContacts.RollbackPresenceSavedAsync();
                if (sdkResult.Success)
                    ConsoleAbstraction.WriteBlue($"Rollback presence to [{presenceRollback.ToString(DetailsLevel.Small)}]");
                else
                    ConsoleAbstraction.WriteRed($"Cannot rollback presence to [{presenceRollback.ToString(DetailsLevel.Small)}] - Error:[{sdkResult.Result}]");
            }
        }
        else
        {
            currentCall = call;

            if ( !call.IsRinging() )
            {
                // The call is NOT IN RINGING STATE  => We update presence according media
                var sdkResult = await RbContacts.SetBusyPresenceAccordingMediasAsync(call.LocalMedias);
                if (sdkResult.Success)
                {
                    var presence = sdkResult.Data;
                    ConsoleAbstraction.WriteBlue($"Busy presence updated according local media:[{Rainbow.Util.MediasToString(call.LocalMedias)}] - Presence:[{presence.ToString(DetailsLevel.Small)}]");
                }
            }
        }
        ConsoleAbstraction.WriteDateTime();
        ConsoleAbstraction.WriteBlue($"[CallUpdated] CallId:[{call.Id}] - Status:[{call.CallStatus}] - Local:[{Rainbow.Util.MediasToString(call.LocalMedias)}] - Remote:[{Rainbow.Util.MediasToString(call.RemoteMedias)}] - Conf.:[{(call.IsConference ? "True" : "False")}] - IsInitiator.:[{call.IsInitiator}]");
    }
}

void RbWebRTCCommunications_OnMediaPublicationUpdated(MediaPublication mediaPublication, Rainbow.WebRTC.MediaPublicationStatus status)
{
    ConsoleAbstraction.WriteDateTime();
    ConsoleAbstraction.WriteBlue($"[OnMediaPublicationUpdated] Status:[{status}] - MediaPublication:[{mediaPublication.ToString(DetailsLevel.Full)}]");
}

void RbWebRTCCommunications_OnDataChannel(string callId, Rainbow.WebRTC.DataChannelDescriptor dataChannelDescriptor)
{
    if (dataChannelDescriptor is null)
        return;

    // When a new DC is available (which is not the one created by the current user) we want to listen to it
    if(dataChannelDescriptor.Peer?.Id != RbContacts.GetCurrentContact()?.Peer?.Id)
    {
        if (dcRemote is not null)
        {
            dcRemote.OnMessage -= DcRemote_OnMessage;
            dcRemote.OnClose -= DcRemote_OnClose;
        }

        dcRemote = dataChannelDescriptor.DataChannel;
        if (dcRemote is not null)
        {
            dcRemote.OnMessage += DcRemote_OnMessage;
            dcRemote.OnClose += DcRemote_OnClose;
        }
    }

    ConsoleAbstraction.WriteDateTime();
    ConsoleAbstraction.WriteBlue($"[OnDataChannel] {dataChannelDescriptor.Peer}{Rainbow.Util.LogElementSeparator()}Label:{dataChannelDescriptor.DataChannel.Label}");
}

void RbWebRTCCommunications_OnTrack(string callId, MediaStreamTrackDescriptor mediaStreamTrackDescriptor)
{
    if (mediaStreamTrackDescriptor is null)
        return;

    ConsoleAbstraction.WriteDateTime();
    ConsoleAbstraction.WriteBlue($"[OnTrack] {(mediaStreamTrackDescriptor.MediaStreamTrack is null ? "REMOVED" : "ADDED / UPDATED")} {mediaStreamTrackDescriptor.ToString()}");

    // We don't want here to manage local track
    if (mediaStreamTrackDescriptor.LocalTrack)
        return;

    switch (mediaStreamTrackDescriptor.Media)
    {
        case Media.AUDIO:
            if (audioRemoteTrack is not null)
                audioRemoteTrack.OnAudioSample -= AudioRemoteTrack_OnAudioSample;

            if (mediaStreamTrackDescriptor.MediaStreamTrack is AudioStreamTrack audioTrack)
            {
                audioRemoteTrack = audioTrack;
                audioRemoteTrack.OnAudioSample += AudioRemoteTrack_OnAudioSample;
            }
            else
            {
                // audio remote track has been removed
                audioRemoteTrack = null;
            }
            break;

        case Media.VIDEO:
            if (videoRemoteTrack is not null)
            {
                _actions.Add(new Action(() =>
                {
                    _windowVideo.VideoStopped = true;
                    if(videoRemoteTrack is not null)
                        videoRemoteTrack.OnImage -= VideoRemoteTrack_OnImage;
                }));
            }

            if (mediaStreamTrackDescriptor.MediaStreamTrack is VideoStreamTrack videoTrack)
            {
                _actions.Add(new Action(() =>
                {
                    Window.UpdateTitle(_windowVideo, "VIDEO");
                    Window.Show(_windowVideo);

                    videoRemoteTrack = videoTrack;
                    if (videoRemoteTrack is not null)
                        videoRemoteTrack.OnImage += VideoRemoteTrack_OnImage;
                    _windowVideo.VideoStopped = false;
                }));
            }
            else
            {
                _actions.Add(new Action(() =>
                {
                    Window.Hide(_windowVideo);
                    // video remote track has been removed
                    videoRemoteTrack = null;
                }));
            }
            break;

        case Media.SHARING:
            if (sharingRemoteTrack is not null)
            {
                _actions.Add(new Action(() =>
                {
                    _windowSharing.VideoStopped = true;
                    if (sharingRemoteTrack is not null)
                        sharingRemoteTrack.OnImage -= SharingRemoteTrack_OnImage;
                }));
            }

            if (mediaStreamTrackDescriptor.MediaStreamTrack is VideoStreamTrack sharingTrack)
            {
                _actions.Add(new Action(() =>
                {
                    Window.UpdateTitle(_windowSharing, "SHARING");
                    Window.Show(_windowSharing);

                    sharingRemoteTrack = sharingTrack;
                    if (sharingRemoteTrack is not null)
                        sharingRemoteTrack.OnImage += SharingRemoteTrack_OnImage;
                    _windowSharing.VideoStopped = false;
                }));
            }
            else
            {
                _actions.Add(new Action(() =>
                {
                    Window.Hide(_windowSharing);
                    // sharing remote track has been removed
                    sharingRemoteTrack = null;
                }));
            }
            break;
    }
}

#endregion DEFINE METHODS TO RECEIVE EVENTS FROM SDK WebRTC

#region DataChannel events

void DcRemote_OnMessage(string message)
{
    if (String.IsNullOrEmpty(dcFile))
        return;
    try
    {
        message += Rainbow.Util.CR;
        File.AppendAllText(dcFile, message, System.Text.Encoding.UTF8);
    }
    catch
    {

    }
}

void DcRemote_OnClose()
{
    ConsoleAbstraction.WriteDateTime();
    ConsoleAbstraction.WriteBlue($"[OnClose] DataChannel Remote");
}

void DcLocal_OnClose()
{
    ConsoleAbstraction.WriteDateTime();
    ConsoleAbstraction.WriteBlue($"[OnClose] DataChannel Local");
}

#endregion DataChannel events

#region REMOTE TRACKS - Audio, Video, Sharing

void AudioRemoteTrack_OnAudioSample(string mediaId, uint duration, byte[] sample)
{
    sdl2AudioOutput?.QueueSample(sample);
}

void VideoRemoteTrack_OnImage(string mediaId, int width, int height, int stride, nint data, AVPixelFormat pixelFormat)
{
    // /!\ Need to use Main Thread
    _actions.Add(new Action(() =>
    {
        if (_windowVideo is null || _windowVideo.VideoStopped || endProgram)
            return;

        if (_windowSharing.Texture == IntPtr.Zero)
            Window.CreateTexture(_windowVideo, width, height, pixelFormat);

        Window.UpdateTexture(_windowVideo, stride, data);
        Window.UpdateRenderer(_windowVideo);
    }));
}

void SharingRemoteTrack_OnImage(string mediaId, int width, int height, int stride, nint data, AVPixelFormat pixelFormat)
{
    // /!\ Need to use Main Thread
    _actions.Add(new Action(() =>
    {
        if (_windowSharing is null || _windowSharing.VideoStopped || endProgram)
            return;

        if (_windowSharing.Texture == IntPtr.Zero)
            Window.CreateTexture(_windowSharing, width, height, pixelFormat);

        Window.UpdateTexture(_windowSharing, stride, data);
        Window.UpdateRenderer(_windowSharing);
    }));
}

#endregion REMOTE TRACKS - Audio, Video, Sharing

#region DEVICES  events
void Devices_OnWebcamDeviceRemoved(WebcamDevice webcamDevice)
{
    ConsoleAbstraction.WriteDateTime();
    ConsoleAbstraction.WriteBlue($"[OnWebcamDeviceRemoved] {webcamDevice}");
    ListWebCams();
}

void Devices_OnWebcamDeviceAdded(WebcamDevice webcamDevice)
{
    ConsoleAbstraction.WriteDateTime();
    ConsoleAbstraction.WriteBlue($"[OnWebcamDeviceAdded] {webcamDevice}");
    ListWebCams();
}

void Devices_OnScreenDeviceRemoved(ScreenDevice screenDevice)
{
    ConsoleAbstraction.WriteDateTime();
    ConsoleAbstraction.WriteBlue($"[OnScreenDeviceRemoved] {screenDevice}");
    ListScreens();
}

void Devices_OnScreenDeviceAdded(ScreenDevice screenDevice)
{
    ConsoleAbstraction.WriteDateTime();
    ConsoleAbstraction.WriteBlue($"[OnScreenDeviceAdded] {screenDevice}");
    ListScreens();
}

void Devices_OnAudioOutputDeviceRemoved(Device device)
{
    ConsoleAbstraction.WriteDateTime();
    ConsoleAbstraction.WriteBlue($"[OnAudioOutputDeviceRemoved] {device}");
    ListAudioOutputDevices();
}

void Devices_OnAudioOutputDeviceAdded(Device device)
{
    ConsoleAbstraction.WriteDateTime();
    ConsoleAbstraction.WriteBlue($"[OnAudioOutputDeviceAdded] {device}");
    ListAudioOutputDevices();
}

void Devices_OnAudioInputDeviceRemoved(Device device)
{
    ConsoleAbstraction.WriteDateTime();
    ConsoleAbstraction.WriteBlue($"[OnAudioInputDeviceRemoved] {device}");
    ListAudioInputDevices();
}

void Devices_OnAudioInputDeviceAdded(Device device)
{
    ConsoleAbstraction.WriteDateTime();
    ConsoleAbstraction.WriteBlue($"[OnAudioInputDeviceAdded] {device}");
    ListAudioInputDevices();
}
#endregion DEVICES  events

Boolean ReadExeSettings()
{
    String exeSettingsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}exeSettings.json";
    if (!File.Exists(exeSettingsFilePath))
    {
        ConsoleAbstraction.WriteRed($"The file '{exeSettingsFilePath}' has not been found.");
        return false;
    }

    String jsonConfig = File.ReadAllText(exeSettingsFilePath);
    var jsonNode = JSON.Parse(jsonConfig);

    if ((jsonNode is null) || (!jsonNode.IsObject))
    {
        ConsoleAbstraction.WriteRed($"Cannot get JSON data from file '{exeSettingsFilePath}'.");
        return false;
    }

    if (ExeSettings.FromJsonNode(jsonNode["exeSettings"], out exeSettings))
    {
        // Set where log files must be stored
        NLogConfigurator.Directory = exeSettings.LogFolderPath;

        // Init external librairies: FFmpeg and SDL2
        if (exeSettings.UseAudioVideo)
            Rainbow.Medias.Helper.InitExternalLibraries(exeSettings.FfmpegLibFolderPath, true);
    }
    else
    {
        ConsoleAbstraction.WriteRed($"Cannot read 'exeSettings' object OR invalid/missing data - file:'{exeSettingsFilePath}'.");
        return false;
    }

    return true;
}

Boolean ReadStreamsSettings()
{
    var streamsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}streams.json";
    if (!File.Exists(streamsFilePath))
    {
        ConsoleAbstraction.WriteRed($"The file '{streamsFilePath}' has not been found.");
        return false;
    }

    String jsonConfig = File.ReadAllText(streamsFilePath);
    var jsonNode = JSON.Parse(jsonConfig);

    if ((jsonNode is null) || (!jsonNode.IsObject))
    {
        ConsoleAbstraction.WriteRed($"Cannot get JSON data from file '{streamsFilePath}'.");
        return false;
    }

    var jsonExeSettings = jsonNode["streams"];
    if (jsonExeSettings?.IsArray == true)
    {
        streamsList = [];
        foreach (var node in jsonExeSettings.Values)
        {
            if (Stream.FromJsonNode(node, out Stream stream))
                streamsList.Add(stream);
        }

        if (streamsList.Count == 0)
        {
            ConsoleAbstraction.WriteRed($"Cannot read 'streams' object (no Stream object created) - file:'{streamsFilePath}'.");
            return false;
        }
    }
    else
    {
        ConsoleAbstraction.WriteRed($"Cannot read 'streams' object OR invalid/missing data - file:'{streamsFilePath}'.");
        return false;
    }

    return true;
}

Boolean ReadCredentials()
{
    var credentialsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}credentials.json";
    if (!File.Exists(credentialsFilePath))
    {
        ConsoleAbstraction.WriteRed($"The file '{credentialsFilePath}' has not been found.");
        return false;
    }

    String jsonConfig = File.ReadAllText(credentialsFilePath);
    var jsonNode = JSON.Parse(jsonConfig);

    if (!Credentials.FromJsonNode(jsonNode["credentials"], out credentials))
    {
        ConsoleAbstraction.WriteRed($"Cannot read 'credentials' object OR invalid/missing data.");
        return false;
    }

    return true;
}


[DllImport("user32.dll")]
static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);
[DllImport("user32.dll")]
[return: MarshalAs(UnmanagedType.Bool)]
static extern bool SetForegroundWindow(IntPtr hWnd);
[DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
static extern IntPtr FindWindowByCaption(IntPtr zeroOnly, string lpWindowName);
const int SW_RESTORE = 9;

static void FocusConsoleWindow()
{
    // /!\ It's highly recommended to set a unique Title to the console to have this working
    IntPtr handle = FindWindowByCaption(IntPtr.Zero, ConsoleAbstraction.Title);
    ShowWindowAsync(new HandleRef(null, handle), SW_RESTORE);
    SetForegroundWindow(handle);
}