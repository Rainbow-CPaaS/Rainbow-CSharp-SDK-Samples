using ConsoleWebRTC;
using FFmpeg.AutoGen;
using Rainbow;
using Rainbow.Consts;
using Rainbow.Enums;
using Rainbow.Medias;
using Rainbow.Model;
using Rainbow.SimpleJSON;
using Rainbow.WebRTC;
using Rainbow.WebRTC.Abstractions;
using Rainbow.WebRTC.Desktop;

using Rainbow.Console;
using Stream = Rainbow.Console.Stream;
using Util = Rainbow.Console.Util;

ExeSettings? exeSettings = null;
List<Stream>? streamsList = null;
Credentials? credentials = null;

Util.WriteDarkYellow($"{Global.ProductName()} v{Global.FileVersion()}");

if ( (!ReadExeSettings()) || (exeSettings is null) )
    return;

if ( (!ReadStreamsSettings()) || (streamsList is null))
    return;

if ( (!ReadCredentials()) || (credentials is null))
    return;

Util.WriteRed($"Account used: [{credentials.UsersConfig[0].Login}]");

// Init external libraries
Util.WriteGreen($"Initializing external libraries ...");
Rainbow.Medias.Helper.InitExternalLibraries(exeSettings.FfmpegLibFolderPath, true);
Util.WriteBlue($"External libraries initialized");

// Set folder path from logs
NLogConfigurator.Directory = exeSettings.LogFolderPath;

// Add logger for the prefix specified
NLogConfigurator.AddLogger(credentials.UsersConfig[0].Prefix + "_");

// Instead of a Microphone, an audio file can be used - we use the first audio stream defined
String? audioFilePath = streamsList?.FindAll(s => s.Media.Contains("audio")).Select(s => s.Uri).FirstOrDefault();

// Instead of a Webcam or a Screen, an video file can be used for Video or Sharing - we use the two first video stream defined
String? videoFilePath = null;
String? sharingFilePath = null;
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
Boolean useEmptyTrack = false;
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

// To manage Window and Video Output
Object lockWindowVideo = new();
IntPtr windowVideo = IntPtr.Zero;
IntPtr windowVideoRenderer = IntPtr.Zero;
IntPtr windowVideoTexture = IntPtr.Zero;

Object lockWindowSharing = new();
IntPtr windowSharing = IntPtr.Zero;
IntPtr windowSharingRenderer = IntPtr.Zero;
IntPtr windowSharingTexture = IntPtr.Zero;

Task RbTask = Task.CompletedTask;

// Create Rainbow Application ROOT object
var RbApplication = new Rainbow.Application(exeSettings.LogFolderPath, loggerPrefix: credentials.UsersConfig[0].Prefix + "_");

// Define Restrictions
RbApplication.Restrictions.LogRestRequest = true;
RbApplication.Restrictions.LogEvent = true;

RbApplication.Restrictions.UseBubbles = true;
RbApplication.Restrictions.AcceptBubbleInvitation = true;
RbApplication.Restrictions.AcceptUserInvitation = true;
RbApplication.Restrictions.UseWebRTC = true;

// Create Rainbow SDK objects
var RbConferences = RbApplication.GetConferences();
var RbContacts = RbApplication.GetContacts();
var RbBubbles = RbApplication.GetBubbles();
var RbAutoReconnection = RbApplication.GetAutoReconnection();

var RbWebRTCDesktopFactory = new Rainbow.WebRTC.Desktop.WebRTCFactory();
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
    Util.WriteRed("Cannot create WebRTCCommunications service ... We quit.");
    return;
}

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
Util.WriteGreen("Starting login ...");
var loginTask = RbApplication.LoginAsync(login, password); // We don't wait here since we want to quit at any time using ESC key

await MainLoop();

DestroyWindows();

if (String.IsNullOrEmpty(currentCallId))
    await RbWebRTCCommunications.HangUpCallAsync(currentCallId);

await RbApplication.LogoutAsync();

async Task MainLoop()
{
    Boolean windowResized = false;
    Boolean windowEvent = false;
    do
    {
        while (SDL2.SDL_PollEvent(out SDL2.SDL_Event e) > 0)
        {
            switch (e.type)
            {
                case SDL2.SDL_EventType.SDL_QUIT:
                    endProgram = true;
                    break;

                case SDL2.SDL_EventType.SDL_WINDOWEVENT:
                    windowEvent = true;
                    switch (e.window.windowEvent)
                    {
                        case SDL2.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                            windowResized = true;
                            break;
                        default:
                            break;
                    }
                    break;

                case SDL2.SDL_EventType.SDL_KEYUP:
                    switch (e.key.keysym.sym)
                    {
                        case SDL2.SDL_Keycode.SDLK_ESCAPE:
                            endProgram = true;
                            break;
                    }
                    break;
            }
        }

        if (windowResized)
        {
            windowResized = false;
            windowEvent = false;
            lock (lockWindowVideo)
                UpdateWindowRenderer(windowVideoTexture, windowVideoRenderer);

            lock (lockWindowSharing)
                UpdateWindowRenderer(windowSharingTexture, windowSharingRenderer);
        }
        else if(windowEvent)
        {
            windowEvent = false;
            lock (lockWindowVideo)
                SDL2.SDL_RenderPresent(windowVideoRenderer);

            lock (lockWindowSharing)
                SDL2.SDL_RenderPresent(windowSharingRenderer);
        }

        CheckInputKey();

    } while (!endProgram); // Loop until we want to quit

    await Task.CompletedTask;
}

void CheckInputKey()
{
    while (Console.KeyAvailable)
    {
        var userInput = Console.ReadKey(true);
        
        switch (userInput.Key)
        {
            case ConsoleKey.Escape:
                MenuEscape();
                return;

            case ConsoleKey.I: // Info
                MenuDisplayInfo();
                return;

            case ConsoleKey.C: // Conference - Join/Quit or Start
                MenuConference();
                break;

            case ConsoleKey.A: // Audio input/output selection.
                MenuAudioStream();
                break;

            case ConsoleKey.V: // Video selection.
                MenuVideoStream();
                break;

            case ConsoleKey.S: // Sharing selection.
                MenuSharingStream();
                break;

            case ConsoleKey.D: // DataChannel: Add/Remove 
                MenuDataChannel();
                break;

            case ConsoleKey.L: // List devices used and conference info
                MenuListDevicesAndConferenceInfo();
                break;

            case ConsoleKey.M:
                MenuMediaPublications();
                break;
        }
    }
}

void MenuDisplayInfo()
{
    Util.WriteYellow("");
    Util.WriteYellow("[ESC] at anytime to quit");
    Util.WriteYellow("[I] (Info) Display this info");

    Util.WriteYellow("");
    Util.WriteYellow("[C] (Conference) to start, join or quit a conference");

    Util.WriteYellow("");
    Util.WriteYellow("[A] (Audio) to manage Audio (Input / Ouput)");
    Util.WriteYellow("[V] (Video) to manage Video");
    Util.WriteYellow("[S] (Sharing) to manage Sharing");
    Util.WriteYellow("[D] (DataChannel) to manage DataChannel");

    Util.WriteYellow("");
    Util.WriteYellow("[M] (MediaPublication) to subscribe/unsubscribe to media publication");

    Util.WriteYellow("");
    Util.WriteYellow("[L] (List) to list devices used and conference info");
}

void MenuMediaPublications()
{
    if (!RbTask.IsCompleted)
    {
        Util.WriteRed("Task is already in progress");
        return;
    }

    if(String.IsNullOrEmpty(currentCallId))
    {
        Util.WriteRed("currentCallId is null/empty...");
        return;
    }

    var publications = RbWebRTCCommunications.GetMediaPublicationsAvailable(currentCallId);
    if(publications?.Count > 0)
    {
        var currentUserId = RbContacts.GetCurrentContact().Peer.Id;

        Boolean selected = false;
        while (!selected)
        {
            Util.WriteYellow($"Select MediaPublication to subscribe / unsubscribe:");
            Util.WriteDarkYellow($"NOTE: For VIDEO only can one be subscribed - if any, the previous one is replaced by the new one");
            int index = 0;
            foreach (var pub in publications)
            {
                var displayname = (pub.Peer.Id == currentUserId) ? "YOURSELF" : pub.Peer.DisplayName;

                if (RbWebRTCCommunications.IsSubscribedToMediaPublication(pub))
                    Util.WriteYellow($"\t[{index++}] - UNSUBSCRIBE TO - Media:[{Rainbow.Util.MediasToString(pub.Media)}] - Peer:[{displayname}]");
                else
                    Util.WriteYellow($"\t[{index++}] - SUBSCRIBE   TO - Media:[{Rainbow.Util.MediasToString(pub.Media)}] - Peer:[{displayname}]");
            }
            Util.WriteYellow($"\t[C] - Cancel");
            var consoleKey = Console.ReadKey(true);
            if (char.IsDigit(consoleKey.KeyChar))
            {
                var selection = int.Parse(consoleKey.KeyChar.ToString());
                if ((selection >= 0) && (selection < index))
                {
                    selected = true;
                    var pub = publications[selection];

                    Util.WriteDarkYellow($"MediaPublication selected: Media:[{Rainbow.Util.MediasToString(pub.Media)}] - Peer:[{pub.Peer.DisplayName}]");

                    if (RbWebRTCCommunications.IsSubscribedToMediaPublication(pub))
                    {
                        Util.WriteGreen($"Asking to unsubscribe ...");
                        RbWebRTCCommunications.UnsubscribeToMediaPublicationAsync(pub).ContinueWith(async task =>
                        {
                            var sdkResult = await task;
                            if (sdkResult.Success)
                                Util.WriteDarkYellow("MediaPublication has been unsubscribed");
                            else
                                Util.WriteDarkYellow($"MediaPublication has NOT been unsubscribed:{sdkResult.Result}");
                        });
                    }
                    else
                    {
                        // We ensure we can subscribe to our own subscription
                        RbWebRTCCommunications.AllowToSubscribeToHisOwnMediaPublication(currentCallId, true);

                        // In this application, we restrict subscription to only one VIDEO MediaPublication
                        var mediaPublicationsSubcribed = RbWebRTCCommunications.GetMediaPublicationsSubscribed(currentCallId);
                        var previousMP = mediaPublicationsSubcribed?.Find(mp => (mp.Media == Media.VIDEO) && (mp.Peer.Id != pub.Peer.Id));
                        if ((pub.Media == Media.VIDEO && previousMP is not null))
                        {
                            Util.WriteGreen($"First asking to unsubscribe to previous Vide MediaPublication ...");

                            RbWebRTCCommunications.UnsubscribeToMediaPublicationAsync(previousMP).ContinueWith(async task =>
                            {
                                var sdkResult = await task;
                                if (sdkResult.Success)
                                {
                                    Util.WriteDarkYellow("Previous Video MediaPublication has been unsubscribed");

                                    Util.WriteGreen($"Asking to subscribe to the new Video MediaPublication ...");
                                    sdkResult = await RbWebRTCCommunications.SubscribeToMediaPublicationAsync(pub);
                                    if (sdkResult.Success)
                                        Util.WriteDarkYellow("MediaPublication has been subscribed");
                                    else
                                        Util.WriteDarkYellow($"MediaPublication has NOT been subscribed:{sdkResult.Result}");
                                }
                                else
                                    Util.WriteDarkYellow($"Previous Video MediaPublication has NOT been unsubscribed:{sdkResult.Result}");
                            });
                        }
                        else
                        {
                            if(pub.Media == Media.VIDEO)
                                CreateWindow(ref windowVideo, ref windowVideoRenderer);
                            else if (pub.Media == Media.SHARING)
                                CreateWindow(ref windowSharing, ref windowSharingRenderer);

                            Util.WriteGreen($"Asking to subscribe ...");
                            RbWebRTCCommunications.SubscribeToMediaPublicationAsync(pub, MediaSubStreamLevel.HIGH).ContinueWith(async task =>
                            {
                                var sdkResult = await task;
                                if (sdkResult.Success)
                                    Util.WriteDarkYellow("MediaPublication has been subscribed");
                                else
                                    Util.WriteDarkYellow($"MediaPublication has NOT been subscribed:{sdkResult.Result}");
                            });
                        }
                    }
                }
                else
                    Util.WriteDarkYellow($"Bad key used ...");
            }
            else if (consoleKey.Key == ConsoleKey.C)
            {
                selected = true;
                Util.WriteDarkYellow($"Cancel used ...");
            }
            else
                Util.WriteDarkYellow($"Bad key used ...");
        }
    }
    else
    {
        Util.WriteRed("No MediaPublication available");
    }
}

void MenuConference()
{
    if (!RbTask.IsCompleted)
    {
        Util.WriteRed("Task is already in progress");
        return;
    }

    Action action = async () =>
    {
        if (currentCall != null)
        {
            Util.WriteGreen("Asking to leave conference ...");
            if (currentCall.Id != null)
            {
                await RbWebRTCCommunications.HangUpCallAsync(currentCall.Id);
                Util.WriteDarkYellow($"Conference has been left");
            }
            else
                Util.WriteRed($"Cannot leave Conference - currentCallId object null");
        }
        else if (conferencesInProgress.Count > 0)
        {
            Util.WriteGreen("Asking to join conference ...");
            await JoinConferenceAsync(RbConferences.GetConferenceById(conferencesInProgress[0]));
        }
        else
        {
            if (audioTrack is null)
            {
                Util.WriteRed("You cannot start a conference. You don't have created an Audio Input Track");
                return;
            }

            var canContinue = true;
            while (canContinue)
            {
                Bubble? result = null;
                Util.WriteYellow($"{Rainbow.Util.CR}Enter a text to search a Bubble: (empty text to cancel)");
                var str = Console.ReadLine();
                if (String.IsNullOrEmpty(str))
                {
                    canContinue = false;
                    Util.WriteDarkYellow("Cancel search ...");
                    break;
                }
                else
                {
                    List<String> memberStatus = new() { BubbleMemberStatus.Accepted };
                    var bubblesList = RbBubbles.GetAllBubbles(memberStatus: memberStatus);
                    result = bubblesList.Find(contact => contact.Peer.DisplayName.Contains(str, StringComparison.InvariantCultureIgnoreCase));
                }

                if (result == null)
                    Util.WriteDarkYellow($"No bubble found with [{str}] ...");
                else
                {
                    Util.WriteDarkYellow($"First bubble found:[{result.ToString(DetailsLevel.Small)}]");
                    Util.WriteYellow($"Do you want to start conference in this bubble ? [Y]");

                    var keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Y)
                    {
                        Util.WriteGreen($"Asking to start and join conference ...");
                        var sdkResult = await RbWebRTCCommunications.StartAndJoinConferenceAsync(result.Peer.Id, audioTrack);
                        if (sdkResult.Success)
                        {
                            Util.WriteDarkYellow($"Conference has been started");
                            canContinue = false;
                            break;
                        }
                        else
                            Util.WriteRed($"Cannot start Conference:[{sdkResult.Result}]");
                    }
                    else
                        Util.WriteDarkYellow("Conference not started ...");
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
        Util.WriteRed("Task is already in progress");
        return;
    }

    Action action = async () =>
    {
        if (currentCall?.IsActive() == true)
        {
            if (Rainbow.Util.MediasWithDataChannel(currentCall.LocalMedias))
            {
                Util.WriteGreen("Asking to remove DataChannel ...");
                await RemoveDataChannelAsync();
            }
            else
            {
                Util.WriteGreen("Asking to add DataChannel ...");
                await AddDataChannelAsync();
            }
        }
        else
            Util.WriteRed($"Conference is not active");
    };
    RbTask = Task.Run(action);
}

void MenuAudioStream()
{
    Boolean? manageInput = null;
    if (!RbTask.IsCompleted)
    {
        Util.WriteRed("Task is already in progress");
        return;
    }

    Action action = async () =>
    {
        var canContinue = true;
        while (canContinue)
        {
            Util.WriteYellow("");
            Util.WriteYellow($"Manage Audio stream:");
            Util.WriteYellow($"\t[I] Manage Audio INPUT stream");
            Util.WriteYellow($"\t[O] Manage Audio OUTPUT stream");
            Util.WriteYellow($"\t[C] Cancel");

            var keyInfo = Console.ReadKey(true);
            switch (keyInfo.Key)
            {
                case ConsoleKey.I:
                    canContinue = false;
                    manageInput = true;
                    Util.WriteDarkYellow($"Audio INPUT stream selected ...");
                    break;

                case ConsoleKey.O:
                    canContinue = false;
                    manageInput = false;
                    Util.WriteDarkYellow($"Audio OUTPUT stream selected ...");
                    break;

                case ConsoleKey.C:
                    canContinue = false;
                    Util.WriteDarkYellow($"Cancel used ...");
                    break;

                default:
                    canContinue = true;
                    Util.WriteDarkYellow($"Bad key used ...");
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
                Util.WriteYellow("");
                if (audioTrack is not null)
                {
                    if (useEmptyTrack)
                        Util.WriteYellow($"A conference is in progress WITH your EMPTY Audio track");
                    else //if (currentAudioInput != null)
                        Util.WriteYellow($"A conference is in progress WITH your AUDIO Input: {currentAudioInput.Name} [{currentAudioInput.Path}]");
                }
                Util.WriteYellow("\t[S] - Select another one and use it in the conference");
                Util.WriteYellow("\t[C] - Cancel");
                var keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.S:
                        canContinue = false;
                        canSelectAudioInputDevice = true;
                        break;

                    case ConsoleKey.C:
                        canContinue = false;
                        Util.WriteDarkYellow($"Cancel used ...");
                        break;

                    default:
                        canContinue = true;
                        Util.WriteDarkYellow($"Bad key used ...");
                        break;

                }
            }
        }
        else
        {
            var canContinue = true;
            while (canContinue)
            {
                Util.WriteYellow("");
                if (audioTrack is null)
                    Util.WriteYellow($"A conference is in progress:");
                else
                {
                    if(useEmptyTrack)
                        Util.WriteYellow($"A conference is in progress WITHOUT your AUDIO INPUT: EMPTY TRACK");
                    else //if(currentAudioInput is not null)
                        Util.WriteYellow($"A conference is in progress WITHOUT your AUDIO INPUT: {currentAudioInput.Name} [{currentAudioInput.Path}]");
                    Util.WriteYellow("\t[A] - Add it to the conference");
                }
                Util.WriteYellow("\t[S] - Select stream and use it as AUDIO INPUT in the conference");
                Util.WriteYellow("\t[C] - Cancel");
                var keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.A:
                        if (currentAudioInput == null)
                        {
                            canContinue = true;
                            Util.WriteDarkYellow($"Bad key used ...");
                        }
                        else
                        {
                            canContinue = false;
                            Util.WriteGreen("Asking to add AUDIO INPUT ...");
                            await AddAudioInputAsync();
                        }
                        break;

                    case ConsoleKey.S:
                        canContinue = false;
                        canSelectAudioInputDevice = true;
                        Util.WriteDarkYellow($"Select a stream ...");
                        break;

                    case ConsoleKey.C:
                        canContinue = false;
                        Util.WriteDarkYellow($"Cancel used ...");
                        break;

                    default:
                        canContinue = true;
                        Util.WriteDarkYellow($"Bad key used ...");
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
        Util.WriteYellow("");
        if (audioTrack is null)
            Util.WriteYellow($"Current stream used has AUDIO INPUT: NONE");
        else
        {
            if (useEmptyTrack)
                Util.WriteYellow($"Current stream used has AUDIO INPUT: EMPTY TRACK");
            else
                Util.WriteYellow($"Current stream used has AUDIO INPUT: {currentAudioInput.Name} [{currentAudioInput.Path}]");
        }
        Util.WriteYellow($"Select stream to use has AUDIO INPUT:");
        int index = 0;
        if (audioInputDevices?.Count > 0)
        {
            foreach (var audioInput in audioInputDevices)
                Util.WriteYellow($"\t[{index++}] - {audioInput.Name} ({audioInput.Path})");
        }

        if (!String.IsNullOrWhiteSpace(audioFilePath))
            Util.WriteYellow($"\t[*] - Audio file: {audioFilePath})");

        Util.WriteYellow($"\t[E] - Use empty track");

        if (currentCall == null)
            Util.WriteYellow($"\t[N] - No audio input");
            

        Util.WriteYellow("\t[C] - Cancel");
        var consoleKey = Console.ReadKey(true);
        if (char.IsDigit(consoleKey.KeyChar))
        {
            var selection = int.Parse(consoleKey.KeyChar.ToString());
            if ((selection >= 0) && (selection < index))
            {
                selected = true;
                useEmptyTrack = false;
                audioInputUpdated = currentAudioInput?.Path != audioInputDevices[selection].Path;
                if (audioInputUpdated)
                {
                    currentAudioInput = audioInputDevices[selection];
                    Util.WriteDarkYellow($"Stream used as Audio Input selected: {currentAudioInput.Name} ({currentAudioInput.Path})]");
                }
                else
                    Util.WriteDarkYellow($"Same Stream selected");
            }
        }
        else if ((consoleKey.KeyChar == '*') && !String.IsNullOrWhiteSpace(audioFilePath))
        {
            selected = true;
            useEmptyTrack = false;
            audioInputUpdated = currentAudioInput?.Path != audioFilePath;
            if (audioInputUpdated)
            {
                currentAudioInput = new InputStreamDevice("audio_file", "audio_file", audioFilePath, withVideo: false, withAudio: true);
                Util.WriteDarkYellow($"Stream used as Audio Input selected: {currentAudioInput.Name} ({currentAudioInput.Path})]");
            }
            else
                Util.WriteDarkYellow($"Same Stream selected");
        }
        else if ( (consoleKey.Key == ConsoleKey.N) && (currentCall is null))
        {
            audioInputUpdated = true;
            currentAudioInput = null;
            useEmptyTrack = false;
            selected = true;
            Util.WriteDarkYellow($"No more audio input used ...");
        }
        else if (consoleKey.Key == ConsoleKey.E)
        {
            audioInputUpdated = true;
            currentAudioInput = null;
            useEmptyTrack = true;
            selected = true;
            Util.WriteDarkYellow($"Use empty track ...");
        }

        else if (consoleKey.Key == ConsoleKey.C)
        {
            selected = true;
            Util.WriteDarkYellow($"Cancel used ...");
        }
        else
            Util.WriteDarkYellow($"Invalid key used ...");
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
        //    //        Util.WriteGreen("Asking to remove Audio Input from conference ...");
        //    //        await RemoveAudioInputAsync();
        //    //    }

                    
        //    //}

        //    Util.WriteGreen("Asking to dispose of previous Audio input track...");
        //    previousAudioTrack?.Dispose();
        //    previousAudioTrack = null;
        //    Util.WriteDarkYellow($"Previous Audio track disposed");
        //}
        //else 
        {
            if (useEmptyTrack)
            {
                try
                {
                    Util.WriteGreen("Asking to create new EMPTY Audio track...");
                    audioTrack = RbWebRTCDesktopFactory.CreateEmptyAudioTrack();

                    Util.WriteDarkYellow($"New Audio EMPTY track created");
                }
                catch (Exception exc)
                {
                    Util.WriteRed($"Cannot create EMPTY Audio track: Exception: [{exc}]");
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
                        Util.WriteGreen("Asking to create new Audio track...");
                        audioTrack = RbWebRTCDesktopFactory.CreateAudioTrack(currentAudioInput);

                        Util.WriteDarkYellow($"New Audio track created");
                    }
                    catch (Exception exc)
                    {
                        Util.WriteRed($"Cannot create Audio track: Exception: [{exc}]");
                        audioTrack = previousAudioTrack;
                        return;
                    }
                }
            }

            Boolean success = true;

            // Update conference (if any) with this new track
            if ( (currentCall?.IsActive() == true) && (audioTrack is not null))
            {
                if (Rainbow.Util.MediasWithAudio(currentCall.LocalMedias))
                {
                    Util.WriteGreen("Asking to update Audio in the conference ...");
                    success = await UpdateAudioInputAsync();
                }
                else
                {
                    Util.WriteGreen("Asking to add Audio in the conference ...");
                    success = await AddAudioInputAsync();
                }
            }

            if (success)
            {
                Util.WriteGreen("Asking to dispose of previous Audio track...");
                previousAudioTrack?.Dispose();
                previousAudioTrack = null;
                Util.WriteDarkYellow($"Previous Audio track disposed");
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
        Util.WriteYellow("");
        if (currentAudioOutput != null)
            Util.WriteYellow($"Current stream used has AUDIO OUPUT: {currentAudioOutput.Name} [{currentAudioOutput.Path}]");
        Util.WriteYellow($"Select stream to use has AUDIO OUPUT:");
        int index = 0;
        if (audioOutputDevices?.Count > 0)
        {
            foreach (var audioOutput in audioOutputDevices)
                Util.WriteYellow($"\t[{index++}] - {audioOutput.Name} ({audioOutput.Path})");
        }

        Util.WriteYellow("\t[N] - No audio output");
        Util.WriteYellow("\t[C] - Cancel");

        var consoleKey = Console.ReadKey(true);
        if (char.IsDigit(consoleKey.KeyChar))
        {
            var selection = int.Parse(consoleKey.KeyChar.ToString());
            if ((selection >= 0) && (selection < index))
            {
                selected = true;
                audioOutputUpdated = currentAudioOutput?.Path != audioOutputDevices[selection].Path;
                if (audioOutputUpdated)
                {
                    currentAudioOutput = audioOutputDevices[selection];
                    Util.WriteDarkYellow($"Stream used as Audio Output selected: {currentAudioOutput.Name} ({currentAudioOutput.Path})]");
                }
                else
                    Util.WriteDarkYellow($"Same Stream selected");
            }
        }
        else if ((consoleKey.Key == ConsoleKey.N))
        {
            audioOutputUpdated = true;
            currentAudioOutput = null;
            selected = true;
            Util.WriteDarkYellow($"No more audio output used ...");
        }

        else if (consoleKey.Key == ConsoleKey.C)
        {
            selected = true;
            Util.WriteDarkYellow($"Cancel used ...");
        }
        else
            Util.WriteDarkYellow($"Invalid key used ...");
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

void MenuVideoStream()
{
    if (!RbTask.IsCompleted)
    {
        Util.WriteRed("Task is already in progress");
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
                    Util.WriteYellow("");
                    if (currentWebCam == null)
                        Util.WriteYellow($"A conference is in progress WITH your VIDEO");
                    else
                        Util.WriteYellow($"A conference is in progress WITH your VIDEO: {currentWebCam.Name} [{currentWebCam.Path}]");
                    Util.WriteYellow("\t[R] - Remove it from conference");
                    Util.WriteYellow("\t[S] - Select another one and use it in the conference");
                    Util.WriteYellow("\t[C] - Cancel");
                    var keyInfo = Console.ReadKey(true);
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.R:
                            canContinue = false;
                            Util.WriteGreen("Asking to remove VIDEO ...");
                            await RemoveVideoAsync();
                            break;

                        case ConsoleKey.S:
                            canContinue = false;
                            canSelectVideoDevice = true;
                            break;

                        case ConsoleKey.C:
                            canContinue = false;
                            Util.WriteDarkYellow($"Cancel used ...");
                            break;

                        default:
                            canContinue = true;
                            Util.WriteDarkYellow($"Bad key used ...");
                            break;

                    }
                }
            }
            else
            {
                var canContinue = true;
                while (canContinue)
                {
                    Util.WriteYellow("");
                    if (currentWebCam == null)
                        Util.WriteYellow($"A conference is in progress:");
                    else
                    {
                        Util.WriteYellow($"A conference is in progress WITHOUT your VIDEO: {currentWebCam.Name} [{currentWebCam.Path}]");
                        Util.WriteYellow("\t[A] - Add it to the conference");
                    }
                    Util.WriteYellow("\t[S] - Select stream and use it as VIDEO in the conference");
                    Util.WriteYellow("\t[C] - Cancel");
                    var keyInfo = Console.ReadKey(true);
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.A:
                            if (currentWebCam == null)
                            {
                                canContinue = true;
                                Util.WriteDarkYellow($"Bad key used ...");
                            }
                            else
                            {
                                canContinue = false;
                                Util.WriteGreen("Asking to add VIDEO ...");
                                await AddVideoAsync();
                            }
                            break;

                        case ConsoleKey.S:
                            canContinue = false;
                            canSelectVideoDevice = true;
                            Util.WriteDarkYellow($"Select a stream ...");
                            break;

                        case ConsoleKey.C:
                            canContinue = false;
                            Util.WriteDarkYellow($"Cancel used ...");
                            break;

                        default:
                            canContinue = true;
                            Util.WriteDarkYellow($"Bad key used ...");
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
            Util.WriteYellow("");
            if (currentWebCam != null)
                Util.WriteYellow($"Current stream used has Video: {currentWebCam.Name} [{currentWebCam.Path}]");
            Util.WriteYellow($"Select stream to use has Video:");
            int index = 0;
            if (webcamDevices?.Count > 0)
            {
                foreach (var webcam in webcamDevices)
                    Util.WriteYellow($"\t[{index++}] - {webcam.Name} ({webcam.Path})");
            }

            if (!String.IsNullOrWhiteSpace(videoFilePath))
                Util.WriteYellow($"\t[*] - Video file: {videoFilePath})");
            Util.WriteYellow($"\t[N] - No video");
            Util.WriteYellow("\t[C] - Cancel");
            var consoleKey = Console.ReadKey(true);
            if (char.IsDigit(consoleKey.KeyChar))
            {
                var selection = int.Parse(consoleKey.KeyChar.ToString());
                if ((selection >= 0) && (selection < index))
                {
                    selected = true;
                    webcamUpdated = currentWebCam?.Path != webcamDevices[selection].Path;
                    if (webcamUpdated)
                    {
                        currentWebCam = webcamDevices[selection];
                        Util.WriteDarkYellow($"Stream used as Video selected: {currentWebCam.Name} ({currentWebCam.Path})]");
                    }
                    else
                        Util.WriteDarkYellow($"Same Stream selected");
                }
            }
            else if ((consoleKey.KeyChar == '*') && !String.IsNullOrWhiteSpace(videoFilePath))
            {
                selected = true;
                webcamUpdated = currentWebCam?.Path != videoFilePath;
                if (webcamUpdated)
                {
                    currentWebCam = new InputStreamDevice("video_file", "video_file", videoFilePath, withVideo: true, withAudio: false);
                    Util.WriteDarkYellow($"Stream used as Video selected: {currentWebCam.Name} ({currentWebCam.Path})]");
                }
                else
                    Util.WriteDarkYellow($"Same Stream selected");
            }
            else if (consoleKey.Key == ConsoleKey.N)
            {
                webcamUpdated = true;
                currentWebCam = null;
                selected = true;
                Util.WriteDarkYellow($"No more video used ...");
            }
            else if (consoleKey.Key == ConsoleKey.C)
            {
                selected = true;
                Util.WriteDarkYellow($"Cancel used ...");
            }
            else
                Util.WriteDarkYellow($"Invalid key used ...");
        }

        if (webcamUpdated)
        {
            var previousVideoTrack = videoTrack;

            if (currentWebCam == null)
            {
                if ((currentCall?.IsActive() == true) && Rainbow.Util.MediasWithVideo(currentCall.LocalMedias))
                {
                    Util.WriteGreen("Asking to remove Video from conference ...");
                    await RemoveVideoAsync();
                }

                Util.WriteGreen("Asking to dispose of previous video track...");
                previousVideoTrack?.Dispose();
                previousVideoTrack = null;
                Util.WriteDarkYellow($"Previous Video track disposed");
            }
            else // currentWebCam != null
            {
                try
                {
                    Util.WriteGreen("Asking to create new video track...");
                    videoTrack = RbWebRTCDesktopFactory.CreateVideoTrack(currentWebCam, false);

                    Util.WriteDarkYellow($"New Video track created");
                }
                catch (Exception exc)
                {
                    Util.WriteRed($"Cannot create video track: Exception: [{exc}]");
                    videoTrack = previousVideoTrack;
                    return;
                }

                Boolean success = true;

                // Update conference (if any) with this new track
                if (currentCall?.IsActive() == true)
                {
                    if (Rainbow.Util.MediasWithVideo(currentCall.LocalMedias))
                    {
                        Util.WriteGreen("Asking to update Video in the conference ...");
                        success = await UpdateVideoAsync();
                    }
                    else
                    {
                        Util.WriteGreen("Asking to add Video in the conference ...");
                        success = await AddVideoAsync();
                    }
                }

                if (success)
                {
                    Util.WriteGreen("Asking to dispose of previous video track...");
                    previousVideoTrack?.Dispose();
                    previousVideoTrack = null;
                    Util.WriteDarkYellow($"Previous Video track disposed");
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
        Util.WriteRed("Task is already in progress");
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
                    Util.WriteYellow("");
                    if (currentScreen == null)
                        Util.WriteYellow($"A conference is in progress WITH your Sharing");
                    else
                        Util.WriteYellow($"A conference is in progress WITH your Sharing: {currentScreen.Name} [{currentScreen.Path}]");
                    Util.WriteYellow("\t[R] - Remove it from conference");
                    Util.WriteYellow("\t[S] - Select another one and use it in the conference");
                    Util.WriteYellow("\t[C] - Cancel");
                    var keyInfo = Console.ReadKey(true);
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.R:
                            canContinue = false;
                            Util.WriteGreen("Asking to remove Sharing ...");
                            await RemoveSharingAsync();
                            break;

                        case ConsoleKey.S:
                            canContinue = false;
                            canSelectSharingDevice = true;
                            break;

                        case ConsoleKey.C:
                            canContinue = false;
                            Util.WriteDarkYellow($"Cancel used ...");
                            break;

                        default:
                            canContinue = true;
                            Util.WriteDarkYellow($"Bad key used ...");
                            break;

                    }
                }
            }

            else
            {
                var canContinue = true;
                while (canContinue)
                {
                    Util.WriteYellow("");
                    if (currentScreen == null)
                        Util.WriteYellow($"A conference is in progress:");
                    else
                    {
                        Util.WriteYellow($"A conference is in progress WITHOUT your Sharing: {currentScreen.Name} [{currentScreen.Path}]");
                        Util.WriteYellow("\t[A] - Add it to the conference");
                    }
                    Util.WriteYellow("\t[S] - Select stream and use it as Sharing in the conference");
                    Util.WriteYellow("\t[C] - Cancel");
                    var keyInfo = Console.ReadKey(true);
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.A:
                            if (currentScreen == null)
                            {
                                canContinue = true;
                                Util.WriteDarkYellow($"Bad key used ...");
                            }
                            else
                            {
                                canContinue = false;
                                if (Rainbow.Util.MediasWithSharing(currentCall.RemoteMedias))
                                    Util.WriteRed($"A conference is in progress with already a sharing. Cannot set another one");
                                else
                                {
                                    Util.WriteGreen("Asking to add sharing ...");
                                    await AddSharingAsync();
                                }
                            }
                            break;

                        case ConsoleKey.S:
                            canContinue = false;
                            canSelectSharingDevice = true;
                            Util.WriteDarkYellow($"Select a stream ...");
                            break;

                        case ConsoleKey.C:
                            canContinue = false;
                            Util.WriteDarkYellow($"Cancel used ...");
                            break;

                        default:
                            canContinue = true;
                            Util.WriteDarkYellow($"Bad key used ...");
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
            Util.WriteYellow("");
            if (currentScreen != null)
                Util.WriteYellow($"Current stream used has Sharing: {currentScreen.Name} [{currentScreen.Path}]");
            Util.WriteYellow($"Select stream to use has Sharing:");
            int index = 0;
            if (screenDevices?.Count > 0)
            {
                foreach (var screen in screenDevices)
                    Util.WriteYellow($"\t[{index++}] - {screen.Name} ({screen.Path})");
            }

            if (!String.IsNullOrWhiteSpace(sharingFilePath))
                Util.WriteYellow($"\t[*] - Sharing file: {sharingFilePath})");
            Util.WriteYellow($"\t[N] - No sharing");
            Util.WriteYellow("\t[C] - Cancel");
            var consoleKey = Console.ReadKey(true);
            if (char.IsDigit(consoleKey.KeyChar))
            {
                var selection = int.Parse(consoleKey.KeyChar.ToString());
                if ((selection >= 0) && (selection < index))
                {
                    selected = true;
                    screenUpdated = currentScreen?.Name != screenDevices[selection].Name;
                    if (screenUpdated)
                    {
                        currentScreen = screenDevices[selection];
                        Util.WriteDarkYellow($"Stream used as Sharing selected: {currentScreen.Name} ({currentScreen.Path})]");
                    }
                    else
                        Util.WriteDarkYellow($"Same Stream selected");
                }
            }
            else if ((consoleKey.KeyChar == '*') && !String.IsNullOrWhiteSpace(sharingFilePath))
            {
                selected = true;
                screenUpdated = currentScreen?.Path != sharingFilePath;
                if (screenUpdated)
                {
                    currentScreen = new InputStreamDevice("sharing_file", "sharing_file", sharingFilePath, withVideo: true, withAudio: false);
                    Util.WriteDarkYellow($"Stream used as Sharing selected: {currentScreen.Name} ({currentScreen.Path})]");
                }
                else
                    Util.WriteDarkYellow($"Same Stream selected");
            }
            else if (consoleKey.Key == ConsoleKey.N)
            {
                screenUpdated = true;
                currentScreen = null;
                selected = true;
                Util.WriteDarkYellow($"No more sharing used ...");
            }
            else if (consoleKey.Key == ConsoleKey.C)
            {
                selected = true;
                Util.WriteDarkYellow($"Cancel used ...");
            }
            else
                Util.WriteDarkYellow($"Invalid key used ...");
        }

        if (screenUpdated)
        {
            var previousSharingTrack = sharingTrack;

            if (currentScreen == null)
            {
                if ((currentCall?.IsActive() == true) && Rainbow.Util.MediasWithSharing(currentCall.LocalMedias))
                {
                    Util.WriteGreen("Asking to remove Sharing from conference ...");
                    await RemoveSharingAsync();
                }

                Util.WriteGreen("Asking to dispose of previous Sharing track...");
                previousSharingTrack?.Dispose();
                previousSharingTrack = null;
                Util.WriteDarkYellow($"Previous Sharing track disposed");
            }
            else // currentScreen != null
            {
                Boolean success = true;
                if ((currentCall?.IsActive() == true) && Rainbow.Util.MediasWithSharing(currentCall.RemoteMedias) && !Rainbow.Util.MediasWithSharing(currentCall.LocalMedias))
                {
                    Util.WriteRed($"A conference is in progress with already a sharing. Cannot set another one");
                }
                else
                {
                    try
                    {
                        Util.WriteGreen("Asking to create new Sharing track...");
                        sharingTrack = RbWebRTCDesktopFactory.CreateVideoTrack(currentScreen, true);

                        Util.WriteDarkYellow($"New Sharing track created");

                        // Update conference (if any) with this new track
                        if (currentCall?.IsActive() == true)
                        {
                            if (Rainbow.Util.MediasWithSharing(currentCall.LocalMedias))
                            {
                                Util.WriteGreen("Asking to update sharing in the conference ...");
                                success = await UpdateSharingAsync();
                            }
                            else
                            {
                                Util.WriteGreen("Asking to add sharing in the conference ...");
                                success = await AddSharingAsync();
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        Util.WriteRed($"Cannot create sharing track: Exception: [{exc}]");
                        sharingTrack = previousSharingTrack;
                        return;
                    }
                }

                if (success)
                {
                    Util.WriteGreen("Asking to dispose of previous sharing track...");
                    previousSharingTrack?.Dispose();
                    previousSharingTrack = null;
                    Util.WriteDarkYellow($"Previous sharing track disposed");
                }
            }
        }
    };
    RbTask = Task.Run(action);
}

void MenuListDevicesAndConferenceInfo()
{
    Util.WriteGreen("Asking to list devices and conference info ...");
    ListUserInfo();
    Util.WriteYellow("");
    ListDevicesUsed();
    Util.WriteYellow("");
    DisplayConferenceInfo();
    Util.WriteYellow("");
    ListAudioInputDevices();
    Util.WriteYellow("");
    ListAudioOutputDevices();
    Util.WriteYellow("");
    ListScreens();
    Util.WriteYellow("");
    ListWebCams();
}

void DisplayConferenceInfo()
{
    Util.WriteDarkYellow("Conference:");
    if (currentCall?.IsActive() == true)
    {
        Util.WriteYellow($"{currentCall}");
    }
    else
    {
        Util.WriteYellow("\tYou are not in a conference");
        if (conferencesInProgress.Count > 0)
            Util.WriteYellow($"\tConference(s) in progress: [{conferencesInProgress.Count}]");
    }
}

void ListUserInfo()
{
    if (RbApplication.IsConnected())
        Util.WriteDarkYellow($"Connected with:[{RbContacts.GetCurrentContact().Peer.DisplayName}]");
    else
        Util.WriteDarkYellow("Current user is not connected ...");
}

async Task JoinConferenceAsync(Conference conference)
{
    if ((currentCallId == null) && (conference != null))
    {
        RbContacts.SavePresenceForRollback();

        SdkResult<String> sdkResult;
        if (audioTrack is null)
        {
            Util.WriteGreen($"Joining conference without AUDIO INPUT ...");
            sdkResult = await RbWebRTCCommunications.JoinConferenceAsync(conference.Peer.Id, Media.VIDEO + Media.DATACHANNEL);
        }
        else
        {
            if(useEmptyTrack)
            {
                Util.WriteGreen($"Joining conference with AUDIO INPUT (EMPTY TRACK) ...");
                sdkResult = await RbWebRTCCommunications.JoinConferenceAsync(conference.Peer.Id, audioTrack);
            }
            else
            {
                Util.WriteGreen($"Joining conference with AUDIO INPUT ...");
                sdkResult = await RbWebRTCCommunications.JoinConferenceAsync(conference.Peer.Id, audioTrack);
            }
        }

        if(sdkResult.Success)
            Util.WriteDarkYellow($"Conference has been joined: [{conference.ToString(DetailsLevel.Medium)}]");
        else
            Util.WriteRed($"Cannot join Conference:[{sdkResult.Result}]");
    }
    else
        Util.WriteRed($"Cannot join Conference - current call in progress or conference object null");
}

void MenuEscape()
{
    if (!RbTask.IsCompleted)
    {
        Util.WriteRed("Task is already in progress");
        return;
    }

    Action action = async () =>
    {
        Util.WriteGreen($"Asked to end process using [ESC] key");
        if (RbApplication.IsConnected())
        {
            if (currentCall?.Id != null)
            {
                Util.WriteGreen("Asking to leave conference ...");
                await RbWebRTCCommunications.HangUpCallAsync(currentCall.Id);
                Util.WriteDarkYellow($"Conference has been left");
            }

            Util.WriteGreen($"Asked to quit - Logout");
            var sdkResultBoolean = await RbApplication.LogoutAsync();
            if (!sdkResultBoolean.Success)
                Util.WriteRed($"Cannot logout - SdkError:[{sdkResultBoolean.Result}]");
        }
        System.Environment.Exit(0);
    };
    RbTask = Task.Run(action);
}

#region AUDIO INPUT - add, remove in the conference

async Task<Boolean> AddAudioInputAsync()
{
    if (currentCall?.IsActive() == true)
    {
        if (audioTrack == null)
        {
            Util.WriteRed($"Cannot add AUDIO Input - No Track available");
            return false;
        }

        var sdkResult = await RbWebRTCCommunications.AddAudioAsync(currentCall.Id, audioTrack, "Audio name", "Audio desc", null);
        if (sdkResult.Success)
        {
            Util.WriteDarkYellow($"Audio Input added in conference and started");
            return true;
        }
        else
        {
            Util.WriteRed($"Cannot add Audio Input - SdkError:[{sdkResult.Result}]");
            return false;
        }
    }
    else
    {
        Util.WriteRed($"Cannot add Audio Input - conference is not active");
        return true;
    }
}

async Task<Boolean> UpdateAudioInputAsync()
{
    if (currentCall?.IsActive() == true)
    {
        if (audioTrack == null)
        {
            Util.WriteRed($"Cannot add AUDIO Input - No Track available");
            return false;
        }

        var sdkResult = await RbWebRTCCommunications.ChangeAudioAsync(currentCall.Id, audioTrack);
        if (sdkResult.Success)
        {
            Util.WriteDarkYellow($"AUDIO Input updated in conference and started");
            return true;
        }
        else
        {
            Util.WriteRed($"Cannot update AUDIO Input - SdkError:[{sdkResult.Result}]");
            return false;
        }
    }
    else
    {
        Util.WriteRed($"Cannot update AUDIO Input - conference is not active");
        return false;
    }
}

#endregion AUDIO INPUT - add, remove in the conference

#region DATACHANNEL - add, remove in the conference, send / receive data
async Task<Boolean> RemoveDataChannelAsync()
{
    if (currentCall?.IsActive() == true)
    {
        // Update timer to send data on the DC
        dcSendWatcher.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        var sdkResult = await RbWebRTCCommunications.RemoveDataChannelAsync(currentCall.Id);
        if (sdkResult.Success)
        {
            Util.WriteDarkYellow($"DataChannel removed from conference");
            dcLocal = null;
            return true;
        }
        else
        {
            Util.WriteRed($"Cannot remove DataChannel - SdkError:[{sdkResult.Result}]");
            return false;
        }
    }
    else
    {
        Util.WriteRed($"Cannot remove DataChannel - conference is not active");
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
            Util.WriteDarkYellow($"DataChannel added in conference and started");

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
            Util.WriteRed($"Cannot add DataChannel - SdkError:[{sdkResult.Result}]");
            return false;
        }
    }
    else
    {
        Util.WriteRed($"Cannot add DataChannel - conference is not active");
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

#endregion DATACHANNEL - add, remove in the conference, send / receive data

#region VIDEO - add, remove, update in the conference
async Task<Boolean> RemoveVideoAsync()
{
    if (currentCall?.IsActive() == true)
    {
        var sdkResult = await RbWebRTCCommunications.RemoveVideoAsync(currentCall.Id);
        if (sdkResult.Success)
        {
            Util.WriteDarkYellow($"Video removed from conference");
            return true;
        }
        else
        {
            Util.WriteRed($"Cannot remove video - SdkError:[{sdkResult.Result}]");
            return false;
        }
    }
    else
    { 
        Util.WriteRed($"Cannot remove video - conference is not active");
        return false;
    }
}

async Task<Boolean> AddVideoAsync()
{
    if (currentCall?.IsActive() == true)
    {
        if (videoTrack == null)
        {
            Util.WriteRed($"Cannot add video - No Video Track available");
            return false;
        }

        var sdkResult = await RbWebRTCCommunications.AddVideoAsync(currentCall.Id, videoTrack, "Video name", "Video desc", null);
        if (sdkResult.Success)
        {
            Util.WriteDarkYellow($"Video added in conference and started");
            return true;
        }
        else
        {
            Util.WriteRed($"Cannot add video - SdkError:[{sdkResult.Result}]");
            return false;
        }
    }
    else
    { 
        Util.WriteRed($"Cannot add video - conference is not active");
        return true;
    }
}

async Task<Boolean> UpdateVideoAsync()
{
    if (currentCall?.IsActive() == true)
    {
        if (videoTrack == null)
        {
            Util.WriteRed($"Cannot add video - No Video Track available");
            return false;
        }

        var sdkResult = await RbWebRTCCommunications.ChangeVideoAsync(currentCall.Id, videoTrack);
        if (sdkResult.Success)
        {
            Util.WriteDarkYellow($"Video updated in conference and started");
            return true;
        }
        else
        {
            Util.WriteRed($"Cannot update video - SdkError:[{sdkResult.Result}]");
            return false;
        }
    }
    else
    {
        Util.WriteRed($"Cannot update video - conference is not active");
        return false;
    }
}

#endregion VIDEO - add, remove, update in the conference

#region SHARING - add, remove, update in the conference

async Task<Boolean> RemoveSharingAsync()
{
    if (currentCall?.IsActive() == true)
    {
        var sdkResult = await RbWebRTCCommunications.RemoveSharingAsync(currentCall.Id);
        if (sdkResult.Success)
        {
            Util.WriteDarkYellow($"Sharing removed from conference");
            return true;
        }
        else
        { 
            Util.WriteRed($"Cannot remove Sharing - SdkError:[{sdkResult.Result}]");
            return false;
        }
    }
    else
    {
        Util.WriteRed($"Cannot remove Sharing - conference is not active");
        return false;
    }
}

async Task<Boolean> AddSharingAsync()
{
    if (currentCall?.IsActive() == true)
    {
        if (sharingTrack == null)
        {
            Util.WriteRed($"Cannot add sharing - No Sharing Track available");
            return false;
        }

        var sdkResult = await RbWebRTCCommunications.AddSharingAsync(currentCall.Id, sharingTrack, "Sharing name", "Sharing desc", null);
        if (sdkResult.Success)
        { 
            Util.WriteDarkYellow($"Sharing added in conference and started");
            return true;
        }
        else
        { 
            Util.WriteRed($"Cannot add Sharing - SdkError:[{sdkResult.Result}]");
            return false;
        }
    }
    else
    { 
        Util.WriteRed($"Cannot add Sharing - conference is not active");
        return false;
    }
}

async Task<Boolean> UpdateSharingAsync()
{
    if (currentCall?.IsActive() == true)
    {
        if (sharingTrack == null)
        {
            Util.WriteRed($"Cannot add sharing - No Sharing Track available");
            return false;
        }

        var sdkResult = await RbWebRTCCommunications.ChangeSharingAsync(currentCall.Id, sharingTrack);
        if (sdkResult.Success)
        {
            Util.WriteDarkYellow($"Sharing updated in conference and started");
            return true;
        }
        else
        {
            Util.WriteRed($"Cannot update Sharing - SdkError:[{sdkResult.Result}]");
            return false;
        }
    }
    else
    {
        Util.WriteRed($"Cannot update Sharing - conference is not active");
        return false;
    }
}

#endregion SHARING - add, remove, update in the conference

#region DEVICES - List audio output, audio input, webcam, screens

void ListDevicesUsed()
{
    Util.WriteDarkYellow("List devices used:");

    //Audio Input
    if (audioTrack is null )
    {
        Util.WriteYellow("\t AUDIO IN  - stream used: NONE");
    }
    else
    {
        if (useEmptyTrack)
            Util.WriteYellow($"\t AUDIO IN  - stream used: EMPTY TRACK");
        else if (currentAudioInput != null)
            Util.WriteYellow($"\t AUDIO IN  - stream used: {currentAudioInput.Name} [{currentAudioInput.Path}]");
        else
        {
            Util.WriteRed($"\t AUDIO IN  - stream used: bad config ....");
        }
        if ((currentCall?.IsActive() == true) && Rainbow.Util.MediasWithAudio(currentCall.LocalMedias))
            Util.WriteYellow($"\t\t => this stream is currently used in the conference");
    }

    //Audio Output
    if (currentAudioOutput == null)
        Util.WriteYellow("\t AUDIO OUT - stream used: NONE");
    else
    {
        Util.WriteYellow($"\t AUDIO OUT - stream used: {currentAudioOutput.Name} [{currentAudioOutput.Path}]");
        if ((currentCall?.IsActive() == true) && Rainbow.Util.MediasWithAudio(currentCall.LocalMedias))
            Util.WriteYellow($"\t\t => this stream is currently used in the conference");
    }

    // Video
    if (currentWebCam == null)
        Util.WriteYellow("\t VIDEO     - stream used: NONE");
    else
    {
        Util.WriteYellow($"\t VIDEO     - stream used: {currentWebCam.Name} [{currentWebCam.Path}]");
        if ((currentCall?.IsActive() == true) && Rainbow.Util.MediasWithVideo(currentCall.LocalMedias))
            Util.WriteYellow($"\t\t => this stream is currently used in the conference");
    }
    // Sharing
    if (currentScreen == null)
        Util.WriteYellow("\t SHARING   - stream used: NONE");
    else
    {
        Util.WriteYellow($"\t SHARING   - stream used : {currentScreen.Name} [{currentScreen.Path}]");
        if ((currentCall?.IsActive() == true) && Rainbow.Util.MediasWithSharing(currentCall.LocalMedias))
            Util.WriteYellow($"\t\t => this stream is currently used in the conference");
    }

    // DC
    if (dcLocal == null)
        Util.WriteYellow("\t DC        - stream used: NONE");
    else
    {
        Util.WriteYellow($"\t DC       - stream used: YES");
        if ((currentCall?.IsActive() == true) && Rainbow.Util.MediasWithDataChannel(currentCall.LocalMedias))
            Util.WriteYellow($"\t\t => this stream is currently used in the conference");
    }
}

void ListAudioOutputDevices()
{
    var audioOutputs = Devices.GetAudioOutputDevices();
    if (audioOutputs?.Count > 0)
    {
        Util.WriteDarkYellow($"Audio OUTPUT(s) available:");
        foreach (var audio in audioOutputs)
        {
            Util.WriteYellow($"\t[{audio}]");
        }
    }
    else
        Util.WriteYellow($"No Audio OUTPUT available");
}

void ListAudioInputDevices()
{
    var audioInputs = Devices.GetAudioInputDevices();
    if (audioInputs?.Count > 0)
    {
        Util.WriteDarkYellow($"Audio INPUT(s) available:");
        foreach (var audio in audioInputs)
        {
            Util.WriteYellow($"\t[{audio}]");
        }
    }
    else
        Util.WriteYellow($"No Audio INPUT available");
}

void ListWebCams()
{
    var webcams = Devices.GetWebcamDevices(false);
    if (webcams?.Count > 0)
    {
        Util.WriteDarkYellow($"Webcam(s) available:");
        foreach (var webcam in webcams)
        {
            Util.WriteYellow($"\tWebcam:[{webcam}]");
        }
    }
    else
        Util.WriteYellow($"No Webcam available");
}

void ListScreens()
{
    var screens = Devices.GetScreenDevices(false);
    if (screens?.Count > 0)
    {
        Util.WriteDarkYellow($"Screen(s) available:");
        foreach (var screen in screens)
            Util.WriteYellow($"\tScreen:[{screen}]");
    }
    else
        Util.WriteYellow($"No Screen available");
}

#endregion DEVICES - List audio output, audio input, webcam, screens

#region Window / Renderer / Texture

void CreateWindow(ref IntPtr wdw, ref IntPtr renderer)
{
    if (wdw != IntPtr.Zero)
        return;

    var flags = SDL2.SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL2.SDL_WindowFlags.SDL_WINDOW_SHOWN;
    wdw = SDL2.SDL_CreateWindow("Output", SDL2.SDL_WINDOWPOS_UNDEFINED, SDL2.SDL_WINDOWPOS_UNDEFINED, 800, 600, flags);

    if (wdw != IntPtr.Zero)
    {
        renderer = SDL2.SDL_CreateRenderer(wdw, -1,
                SDL2.SDL_RendererFlags.SDL_RENDERER_ACCELERATED
                | SDL2.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
        ClearWindowRenderer(renderer);
    }
}

void DestroyWindows()
{
    if (windowVideoTexture != IntPtr.Zero)
        SDL2.SDL_DestroyTexture(windowVideoTexture);
    if (windowSharingTexture != IntPtr.Zero)
        SDL2.SDL_DestroyTexture(windowSharingTexture);

    if (windowVideoRenderer != IntPtr.Zero)
        SDL2.SDL_DestroyRenderer(windowVideoRenderer);
    if (windowSharingRenderer != IntPtr.Zero)
        SDL2.SDL_DestroyRenderer(windowSharingRenderer);

    if (windowVideo != IntPtr.Zero)
        SDL2.SDL_DestroyWindow(windowVideo);
    if (windowSharing != IntPtr.Zero)
        SDL2.SDL_DestroyWindow(windowSharing);
}

void HideWindow(IntPtr wdw)
{
    if (wdw != IntPtr.Zero)
        SDL2.SDL_HideWindow(wdw);
}

void ShowWindow(IntPtr wdw)
{
    if (wdw != IntPtr.Zero)
        SDL2.SDL_ShowWindow(wdw);
}

void UpdateWindowTitle(IntPtr wdw, String title)
{
    if (wdw != IntPtr.Zero)
    {
        var s = $"SDK C# v3.x - " + title;
        SDL2.SDL_SetWindowTitle(wdw, s);
    }
}

void CreateTexture(ref IntPtr texture, IntPtr renderer, int w, int h, AVPixelFormat pixelFormat)
{
    if (renderer != IntPtr.Zero)
    {
        // Destroy previous texture
        if (texture != IntPtr.Zero)
        {
            SDL2.SDL_DestroyTexture(texture);
            texture = IntPtr.Zero;
        }

        var sdlFormat = SDL2Helper.GetPixelFormat(pixelFormat);
        if (sdlFormat == SDL2.SDL_PIXELFORMAT_UNKNOWN)
        {
            Util.WriteRed($"Cannot get SDL pixel format using ffmpeg video foramt:[{pixelFormat}]");
            return;
        }

        // Create texture
        texture = SDL2.SDL_CreateTexture(renderer, sdlFormat, (int)SDL2.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, w, h);
        if (texture == IntPtr.Zero)
            Util.WriteRed($"Cannot create texture");
    }
    else
        Util.WriteRed($"Cannot create texture - No window renderer");
}

void DestroyTexture(ref IntPtr texture)
{
    if (texture != IntPtr.Zero)
    {
        SDL2.SDL_DestroyTexture(texture);
        texture = IntPtr.Zero;
    }
}

void UpdateTexture(IntPtr texture, int stride, IntPtr data)
{
    if (texture != IntPtr.Zero)
    {
        var _ = SDL2.SDL_UpdateTexture(texture, IntPtr.Zero, data, stride);
    }
}

void ClearWindowRenderer(IntPtr renderer)
{
    if (renderer != IntPtr.Zero)
    {
        if (SDL2.SDL_RenderClear(renderer) == 0)
            SDL2.SDL_RenderPresent(renderer);
    }
}

void UpdateWindowRenderer(IntPtr texture, IntPtr renderer)
{
    if ((renderer != IntPtr.Zero) && (texture != IntPtr.Zero))
    {
        if (SDL2.SDL_RenderCopy(renderer, texture, IntPtr.Zero, IntPtr.Zero) == 0)
            SDL2.SDL_RenderPresent(renderer);
    }
}

#endregion Window / Renderer / Texture

#region DEFINE METHODS TO RECEIVE EVENTS FROM RAINBOW SDK (Core)

void RbApplication_ConnectionStateChanged(Rainbow.Model.ConnectionState connectionState)
{
    Util.WriteDateTime();
    switch (connectionState.Status)
    {
        case ConnectionStatus.Connected:
            Util.WriteBlue($"[Application.ConnectionStateChanged] Connection Status: [{connectionState.Status}]");
            break;

        case ConnectionStatus.Disconnected:
            Util.WriteRed($"[Application.ConnectionStateChanged] Connection Status: [{connectionState.Status}]");
            break;

        case ConnectionStatus.Connecting:
        default:
            Util.WriteBlue($"[Application.ConnectionStateChanged] - Connection Status: [{connectionState.Status}] - AutoReconnection.CurrentNbAttempts: [{RbAutoReconnection.CurrentNbAttempts}]");
            break;
    }
}

void RbAutoReconnection_Cancelled(SdkError sdkError)
{
    Util.WriteDateTime();
    if (sdkError.Type != SdkErrorType.NoError)
        Util.WriteRed($"[AutoReconnection.Cancelled] SdkError:[{sdkError}]");
    else
        Util.WriteBlue($"[AutoReconnection.Cancelled]");

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
                Util.WriteDateTime();
                Util.WriteBlue($"[ConferenceUpdated] A conference is active - Id:[{conference.Peer.Id}]");
            }
        }
        else
        {
            if (conferencesInProgress.Contains(conference.Peer.Id))
            {
                conferencesInProgress.Remove(conference.Peer.Id);
                Util.WriteDateTime();
                Util.WriteBlue($"[ConferenceUpdated] A conference is NO MORE active - Id:[{conference.Peer.Id}]");
            }
        }
    }
}

void RbConferences_ConferenceRemoved(Conference conference)
{
    Util.WriteDateTime();
    RbConferences_ConferenceUpdated(conference);
}

#endregion DEFINE METHODS TO RECEIVE EVENTS FROM RAINBOW SDK  (Core)

#region DEFINE METHODS TO RECEIVE EVENTS FROM SDK WebRTC

void RbWebRTCCommunications_CallUpdated(Call call)
{
    if (call == null)
    {
        Util.WriteRed($"[CallUpdated] Call object is null ...");
        return;
    }

    // If we don't currently manage a call, we take the first new one into account
    if (currentCallId == null)
    {
        currentCallId = call.Id;
        Util.WriteBlue($"[CallUpdated] New call with id:[{currentCallId}]");
    }

    if (call.Id == currentCallId)
    {
        if (!call.IsInProgress())
        {
            currentCallId = null;
            Util.WriteBlue($"[CallUpdated] Reset current Call");
            currentCall = null;

            // The call is NO MORE in Progress => We Rollback presence if any
            var _ = RbContacts.RollbackPresenceSavedAsync();
        }
        else
        {
            currentCall = call;

            if (!call.IsRinging())
            {
                // The call is NOT IN RINGING STATE  => We update presence according media
                var _ = RbContacts.SetBusyPresenceAccordingMediasAsync(call.LocalMedias);
            }
        }
        Util.WriteDateTime();
        Util.WriteBlue($"[CallUpdated] CallId:[{call.Id}] - Status:[{call.CallStatus}] - Local:[{Rainbow.Util.MediasToString(call.LocalMedias)}] - Remote:[{Rainbow.Util.MediasToString(call.RemoteMedias)}] - Conf.:[{(call.IsConference ? "True" : "False")}] - IsInitiator.:[{call.IsInitiator}]");
    }

}

void RbWebRTCCommunications_OnMediaPublicationUpdated(MediaPublication mediaPublication, Rainbow.WebRTC.MediaPublicationStatus status)
{
    Util.WriteDateTime();
    Util.WriteBlue($"[OnMediaPublicationUpdated] Status:[{status}] - MediaPublication:[{mediaPublication}]");
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

    Util.WriteDateTime();
    Util.WriteBlue($"[OnDataChannel] {dataChannelDescriptor.Peer}{Rainbow.Util.LogElementSeparator()}Label:{dataChannelDescriptor.DataChannel.Label}");
}

void RbWebRTCCommunications_OnTrack(string callId, MediaStreamTrackDescriptor mediaStreamTrackDescriptor)
{
    if (mediaStreamTrackDescriptor is null)
        return;

    Util.WriteDateTime();
    Util.WriteBlue($"[OnTrack] {(mediaStreamTrackDescriptor.MediaStreamTrack is null ? "REMOVED" : "ADDED / UPDATED")} {Rainbow.Util.MediasToString(mediaStreamTrackDescriptor.Media)} {(mediaStreamTrackDescriptor.LocalTrack ? "LOCAL" : "REMOTE")} track - Peer:[{mediaStreamTrackDescriptor.Peer.DisplayName}] ");

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
                videoRemoteTrack.OnImage -= VideoRemoteTrack_OnImage;

            if (mediaStreamTrackDescriptor.MediaStreamTrack is VideoStreamTrack videoTrack)
            {
                UpdateWindowTitle(windowVideo, "VIDEO");
                ShowWindow(windowVideo);

                videoRemoteTrack = videoTrack;
                videoRemoteTrack.OnImage += VideoRemoteTrack_OnImage;
            }
            else
            {
                HideWindow(windowVideo);
                // video remote track has been removed
                videoRemoteTrack = null;
            }
            break;

        case Media.SHARING:
            if (sharingRemoteTrack is not null)
                sharingRemoteTrack.OnImage -= SharingRemoteTrack_OnImage;

            if (mediaStreamTrackDescriptor.MediaStreamTrack is VideoStreamTrack sharingTrack)
            {
                UpdateWindowTitle(windowSharing, "SHARING");
                ShowWindow(windowSharing);

                sharingRemoteTrack = sharingTrack;
                sharingRemoteTrack.OnImage += SharingRemoteTrack_OnImage;
            }
            else
            {
                HideWindow(windowSharing);
                // sharing remote track has been removed
                sharingRemoteTrack = null;
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
    Util.WriteDateTime();
    Util.WriteBlue($"[OnClose] DataChannel Remote");
}

void DcLocal_OnClose()
{
    Util.WriteDateTime();
    Util.WriteBlue($"[OnClose] DataChannel Local");
}

#endregion DataChannel events

#region REMOTE TRACKS - Audio, Video, Sharing

void AudioRemoteTrack_OnAudioSample(string mediaId, uint duration, byte[] sample)
{
    sdl2AudioOutput?.QueueSample(sample);
}

void VideoRemoteTrack_OnImage(string mediaId, int width, int height, int stride, nint data, AVPixelFormat pixelFormat)
{
    if (!endProgram)
    {
        lock (lockWindowVideo)
        {
            if (windowVideoTexture == IntPtr.Zero)
            CreateTexture(ref windowVideoTexture, windowVideoRenderer, width, height, pixelFormat);
        
            UpdateTexture(windowVideoTexture, stride, data);
            UpdateWindowRenderer(windowVideoTexture, windowVideoRenderer);
        }
    }
}

void SharingRemoteTrack_OnImage(string mediaId, int width, int height, int stride, nint data, AVPixelFormat pixelFormat)
{
    if (!endProgram)
    {
        lock (lockWindowSharing)
        {
            if (windowSharingTexture == IntPtr.Zero)
                CreateTexture(ref windowSharingTexture, windowSharingRenderer, width, height, pixelFormat);
            UpdateTexture(windowSharingTexture, stride, data);
            UpdateWindowRenderer(windowSharingTexture, windowSharingRenderer);
        }
    }
}

#endregion REMOTE TRACKS - Audio, Video, Sharing

#region DEVICES  events
void Devices_OnWebcamDeviceRemoved(WebcamDevice webcamDevice)
{
    Util.WriteDateTime();
    Util.WriteBlue($"[OnWebcamDeviceRemoved] {webcamDevice}");
    ListWebCams();
}

void Devices_OnWebcamDeviceAdded(WebcamDevice webcamDevice)
{
    Util.WriteDateTime();
    Util.WriteBlue($"[OnWebcamDeviceAdded] {webcamDevice}");
    ListWebCams();
}

void Devices_OnScreenDeviceRemoved(ScreenDevice screenDevice)
{
    Util.WriteDateTime();
    Util.WriteBlue($"[OnScreenDeviceRemoved] {screenDevice}");
    ListScreens();
}

void Devices_OnScreenDeviceAdded(ScreenDevice screenDevice)
{
    Util.WriteDateTime();
    Util.WriteBlue($"[OnScreenDeviceAdded] {screenDevice}");
    ListScreens();
}

void Devices_OnAudioOutputDeviceRemoved(Device device)
{
    Util.WriteDateTime();
    Util.WriteBlue($"[OnAudioOutputDeviceRemoved] {device}");
    ListAudioOutputDevices();
}

void Devices_OnAudioOutputDeviceAdded(Device device)
{
    Util.WriteDateTime();
    Util.WriteBlue($"[OnAudioOutputDeviceAdded] {device}");
    ListAudioOutputDevices();
}

void Devices_OnAudioInputDeviceRemoved(Device device)
{
    Util.WriteDateTime();
    Util.WriteBlue($"[OnAudioInputDeviceRemoved] {device}");
    ListAudioInputDevices();
}

void Devices_OnAudioInputDeviceAdded(Device device)
{
    Util.WriteDateTime();
    Util.WriteBlue($"[OnAudioInputDeviceAdded] {device}");
    ListAudioInputDevices();
}
#endregion DEVICES  events

Boolean ReadExeSettings()
{
    String exeSettingsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}exeSettings.json";
    if (!File.Exists(exeSettingsFilePath))
    {
        Util.WriteRed($"The file '{exeSettingsFilePath}' has not been found.");
        return false;
    }

    String jsonConfig = File.ReadAllText(exeSettingsFilePath);
    var jsonNode = JSON.Parse(jsonConfig);

    if ((jsonNode is null) || (!jsonNode.IsObject))
    {
        Util.WriteRed($"Cannot get JSON data from file '{exeSettingsFilePath}'.");
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
        Util.WriteRed($"Cannot read 'exeSettings' object OR invalid/missing data - file:'{exeSettingsFilePath}'.");
        return false;
    }

    return true;
}

Boolean ReadStreamsSettings()
{
    var streamsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}streams.json";
    if (!File.Exists(streamsFilePath))
    {
        Util.WriteRed($"The file '{streamsFilePath}' has not been found.");
        return false;
    }

    String jsonConfig = File.ReadAllText(streamsFilePath);
    var jsonNode = JSON.Parse(jsonConfig);

    if ((jsonNode is null) || (!jsonNode.IsObject))
    {
        Util.WriteRed($"Cannot get JSON data from file '{streamsFilePath}'.");
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
            Util.WriteRed($"Cannot read 'streams' object (no Stream object created) - file:'{streamsFilePath}'.");
            return false;
        }
    }
    else
    {
        Util.WriteRed($"Cannot read 'streams' object OR invalid/missing data - file:'{streamsFilePath}'.");
        return false;
    }

    return true;
}

Boolean ReadCredentials()
{
    var credentialsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}credentials.json";
    if (!File.Exists(credentialsFilePath))
    {
        Util.WriteRed($"The file '{credentialsFilePath}' has not been found.");
        return false;
    }

    String jsonConfig = File.ReadAllText(credentialsFilePath);
    var jsonNode = JSON.Parse(jsonConfig);

    if (!Credentials.FromJsonNode(jsonNode["credentials"], out credentials))
    {
        Util.WriteRed($"Cannot read 'credentials' object OR invalid/missing data.");
        return false;
    }

    return true;
}