using FFmpeg.AutoGen;
using Rainbow.Example.Common;
using Rainbow.Example.Common.SDL2;
using Rainbow.Medias;
using Rainbow.SimpleJSON;
using Stream = Rainbow.Example.Common.Stream;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Drawing;

namespace ConsoleMediaPlayer
{
    // see https://dev.to/noah11012/using-sdl2-in-c-and-with-c-too-1l72
    // see https://jsayers.dev/tutorials/

    internal partial class ConsoleMediaPlayer
    {
        // --- To store settings and to know which Stream is selected
        private static ExeSettings? _exeSettings;
        private static List<Stream>? _streamsList;
        private static String _streamsFilePath = "";

        private static String autoPlay = "";

        // --- IMedia used / created for Input based on the selected Stream
        private static IMediaAudio? _mediaInputAudio;
        private static IMediaVideo? _mediaInputVideo;

        // --- IMedia used / created to have composition
        private static readonly List<IMediaVideo> _mediaInputVideoListForComposition = [];

        // --- To know if we need to quit the application or not
        private static Boolean canContinue = true;

        // --- To manage Audio Output
        private static SDL2AudioOutput? _sdl2AudioOutput = null;
        private static Device? _audioOutputDevice = null;

        // --- To manage Window Output
        private static readonly Window _outputWindow = new();

        // --- To ensure to launch action on main thread
        private static readonly BlockingCollection<Action> _actions = [];

        static async Task Main()
        {

            // Need to set an unique title
            Console.Title = $"SDK C# - MediaPlayer [{Guid.NewGuid()}]";

            Util.WriteDarkYellow($"{Global.ProductName()} v{Global.FileVersion()}");

            if (!ReadExeSettings())
                return;

            if (!ReadStreamsSettings())
                return;

            if (_exeSettings is null) return;
            if ((_streamsList is null) || (_streamsList.Count == 0)) return;

            PromptStreamsInfo();

            PromptHelpMenu();

            UseStreamSetToAutoPlay();

            await MainLoop();
        }

        static async Task MainLoop()
        {
            // Loop until, ESC is used
            int simulatedKey = 0;

            while (canContinue)
            {
                while (SDL2.SDL_PollEvent(out SDL2.SDL_Event e) > 0)
                {
                    switch (e.type)
                    {
                        case SDL2.SDL_EventType.SDL_QUIT:
                            canContinue = false;
                        break;

                        case SDL2.SDL_EventType.SDL_WINDOWEVENT:
                            switch (e.window.windowEvent)
                            {
                                case SDL2.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                                    if (e.window.windowID == _outputWindow.Id)
                                        _outputWindow.NeedRendereUpdate = true;
                                break;
                            }
                            break;

                        case SDL2.SDL_EventType.SDL_RENDER_TARGETS_RESET:
                            if (e.window.windowID == _outputWindow.Id)
                                _outputWindow.NeedRendereUpdate = true;
                            break;

                        case SDL2.SDL_EventType.SDL_KEYUP:
                            switch (e.key.keysym.sym)
                            {
                                case SDL2.SDL_Keycode.SDLK_ESCAPE:
                                    simulatedKey = (int)ConsoleKey.Escape;
                                    break;

                                default:
                                    simulatedKey = ((int)ConsoleKey.A) - ((int)SDL2.SDL_Keycode.SDLK_a) + ((int)e.key.keysym.sym);
                                    break;
                            }
                            break;
                    }
                }

                Window.CheckUpdateRenderer(_outputWindow);

                // Check Keys fom Console Window
                if (Console.KeyAvailable || simulatedKey!=0)
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
                        var userInput = Console.ReadKey(true);
                        key = (int)userInput.Key;
                    }
                    switch (key)
                    {
                        case (int)ConsoleKey.Escape:
                            canContinue = false;
                            break;

                        case (int)ConsoleKey.H:
                            PromptHelpMenu();
                            break;

                        case (int)ConsoleKey.L:
                            PromptLoadStreamSettings();
                            break;

                        case (int)ConsoleKey.A:
                            PromptManageAudioOutput();
                            break;

                        case (int)ConsoleKey.I:
                            PromptInputStreamSelection();
                            break;

                        case (int)ConsoleKey.F:
                            Window.ToggleFullScreen(_outputWindow);
                            break;

                        case (int)ConsoleKey.C:
                            PromptCancelStreaming();
                            break;

                        case (int)ConsoleKey.S:
                            PromptManageScreen();
                            break;

                        case (int)ConsoleKey.M:
                            PromptManageAudioInput();
                            break;

                        case (int)ConsoleKey.W:
                            PromptManageWebcam();
                            break;

                        case (int)ConsoleKey.T:
                            PromptTest();
                            break;
                    }
                }

                if(canContinue)
                {
                    if (_actions.TryTake(out var action))
                        action.Invoke();
                }
            }

            await CloseMediaInputsAsync();

            // Destroy SDL2 window
            Window.Destroy(_outputWindow);
        }

#region Prompts
        static void PromptHelpMenu()
        {
            Util.WriteGreen($"{Rainbow.Util.CR}Console Window must have the focus to use the keyboard");
            Util.WriteYellow($"[ESC] to quit");
            Util.WriteYellow($"[H] Help message (this one)");
            Util.WriteYellow($"[L] Load / reload stream settings ");
            Util.WriteYellow("");
            Util.WriteYellow($"[F] Full screen - toggle");
            Util.WriteYellow($"[C] Cancel /Stop current streaming");
            Util.WriteYellow("");
            Util.WriteYellow($"[A] Audio Ouput selection");
            Util.WriteYellow($"[I] Input Stream seletion(Audio, Video or Both)");
            Util.WriteYellow("");
            Util.WriteYellow($"[S] Screen management");
            Util.WriteYellow($"[M] Microphone (Audio Input) management");
            Util.WriteYellow($"[W] Webcam management{Rainbow.Util.CR}");
        }

        static void UseStreamSetToAutoPlay()
        {
            if (!String.IsNullOrEmpty(autoPlay))
            {
                var stream = _streamsList?.FirstOrDefault(s => s.Id == autoPlay);
                if (stream is not null)
                {
                    Util.WriteGreen($"Auto play set to Stream:[{autoPlay}]. Trying to start it ...");
                    var _ = UseStreamAsync(stream);
                }
                else
                    Util.WriteRed($"Auto play set to Stream:[{autoPlay}] but config is not correct - ensure this stream is well defined");
            }
        }

        static void PromptLoadStreamSettings()
        {
            ReadStreamsSettings();
            PromptStreamsInfo();

            UseStreamSetToAutoPlay();
        }

        static void PromptCancelStreaming()
        {
            var _ = CloseMediaInputsAsync().ContinueWith((task) =>
            {
                Window.Hide(_outputWindow);
            });
        }

        static void PromptInputStreamSelection()
        {
            if (_streamsList?.Count > 0)
            {
                var selected = false;
                List<Stream>? subStreams = null;
                String category = "";
                while (!selected)
                {
                    Util.WriteYellow($"{Rainbow.Util.CR}Do you want to select Stream with:");
                    Util.WriteYellow($"\t[B] Audio AND Video");
                    Util.WriteYellow($"\t[A] Audio");
                    Util.WriteYellow($"\t[V] Video");
                    Util.WriteYellow($"\t[C] Cancel");

                    var consoleKey = Console.ReadKey(true);
                    switch (consoleKey.Key)
                    {
                        case ConsoleKey.A:
                            category = "audio";
                            selected = true;
                            Util.WriteDarkYellow($"Any with Audio selected ...");
                            subStreams = _streamsList.FindAll(s => s.Media.Contains("audio"));
                            break;

                        case ConsoleKey.V:
                            category = "video";
                            selected = true;
                            Util.WriteDarkYellow($"Any with Video selected ...");
                            subStreams = _streamsList.FindAll(s => s.Media.Contains("video") || s.Media.Contains("composition"));
                            break;

                        case ConsoleKey.B:
                            category = "audio+video";
                            selected = true;
                            Util.WriteDarkYellow($"Audio and Video selected ...");
                            subStreams = _streamsList.FindAll(s => s.Media == "audio+video");
                            break;

                        case ConsoleKey.C:
                            Util.WriteDarkYellow($"Cancelled ...");
                            return;

                        default:
                            selected = false;
                            Util.WriteDarkYellow($"Bad key used ...");
                            break;
                    }
                }

                if (subStreams?.Count > 0)
                {
                    int index;
                    bool withNextPage;
                    bool withPreviousPage;
                    int startIndex;
                    int maxIndex;

                    int page = 0;
                    int count = subStreams.Count;

                    selected = false;
                    while (!selected)
                    {
                        startIndex = page * 10;
                        maxIndex = Math.Min((page+1) * 10, count);
                        withNextPage = (maxIndex < count);
                        withPreviousPage = (page > 0);

                        Util.WriteYellow($"{Rainbow.Util.CR}Select the stream with [{category}]:");
                        for (index = startIndex; index < maxIndex; index++)
                        {
                            var stream = subStreams[index];
                            Util.WriteYellow($"\t[{index - (page * 10)}] {stream}");
                        }

                        if (withPreviousPage)
                            Util.WriteYellow($"\t[P] Previous page");
                        if (withNextPage)
                            Util.WriteYellow($"\t[N] Next page");
                        Util.WriteYellow($"\t[C] Cancel");

                        var consoleKey = Console.ReadKey(true);
                        switch (consoleKey.Key)
                        {
                            case ConsoleKey.C:
                                Util.WriteDarkYellow($"Cancelled ....");
                            return;

                            case ConsoleKey.N:
                                selected = false;
                                if (withNextPage)
                                    page++;
                                else
                                    Util.WriteDarkYellow($"Bad key used ...");
                            break;

                            case ConsoleKey.P:
                                selected = false;
                                if (withPreviousPage)
                                    page--;
                                else
                                    Util.WriteDarkYellow($"Bad key used ...");
                            break;

                            default:
                                if (char.IsDigit(consoleKey.KeyChar))
                                {
                                    var selection = int.Parse(consoleKey.KeyChar.ToString());
                                    if ((selection >= 0) && (selection < maxIndex))
                                    {
                                        var stream = Stream.FromStream(subStreams[selection + (page * 10)]);

                                        // We restrict the use on one media here if specified by end-user
                                        if ((category == "audio") || (category == "video"))
                                            stream.Media = category;

                                        var _ = UseStreamAsync(stream);
                                        return;
                                    }
                                }
                                Util.WriteDarkYellow($"Bad key used ...");
                            break;
                        }
                    }
                }
                else
                    Util.WriteRed($"No streams available");
            }
            else
                Util.WriteRed($"{Rainbow.Util.CR}No streams available");
        }

        static void PromptManageDevice(String type)
        {
            List<Device> devices;

            switch (type)
            {
                case "webcam":
                    devices = Rainbow.Medias.Devices.GetWebcamDevices(false).Select(s => (Device)s).ToList();
                    break;

                case "screen":
                    devices = Rainbow.Medias.Devices.GetScreenDevices(false).Select(s => (Device)s).ToList();
                    break;

                case "microphone":
                    devices = Rainbow.Medias.Devices.GetAudioInputDevices();
                    break;

                default:
                    return;
            }

            // Display info about devices available on this computer
            if (devices.Count > 0)
            {
                Util.WriteBlue($"{Rainbow.Util.CR}List of [{type}] available on this computer:");
                int index = 1;
                foreach (var audioInput in devices)
                    Util.WriteBlue($"{index++} - {audioInput.Name}");
            }
            else
            {
                Util.WriteBlue($"{Rainbow.Util.CR}No [{type}] available on this computer");
                return;
            }

            // Display info about streams defined as device
            var streamAsDevice = _streamsList?.FindAll(s => s.UriType == type);
            if (streamAsDevice?.Count > 0)
            {
                Util.WriteBlue($"{Rainbow.Util.CR}List of streams defined as [{type}] correctly set in config file: (doesn't mean they can be really used)");
                int index = 1;
                foreach (var audioInput in streamAsDevice)
                    Util.WriteBlue($"{index++} - {audioInput}");
            }
            else
                Util.WriteBlue($"{Rainbow.Util.CR}No stream defined as [{type}] correctly set in config file");

            Util.WriteYellow($"{Rainbow.Util.CR}Do you want to add in config file all [{type}s] available on your system ? [Y]");
            Util.WriteRed($"Previous [{type}s] already set will be removed and the file [{_streamsFilePath}] will be totally rewritten.");
            var consoleKey = Console.ReadKey(true);
            if (consoleKey.Key == ConsoleKey.Y)
            {
                _streamsList?.RemoveAll(s => s.UriType == type);
                int index = 1;
                foreach (var device in devices)
                {
                    if ((device is null) || (device.Name is null))
                        continue;

                    Stream stream = new()
                    {
                        Id = type + "_" + index++,
                        Connected = false,
                        Media = type == "microphone" ? "audio" : "video",
                        Uri = device.Name,
                        UriType = type,
                    };
                    _streamsList ??= [];
                    _streamsList.Add(stream);
                }

                // Create array
                _streamsList ??= [];
                var jsonArray = new JSONArray();
                foreach (var stream in _streamsList)
                {
                    var jnode = Stream.ToJsonNode(stream);
                    if (jnode is not null)
                        jsonArray.Add(jnode);
                }

                // Create node with jsonArray
                var jsonNode = new JSONObject();
                jsonNode.Add("streams", jsonArray);

                // Get json string and store it to  file
                var str = jsonNode.ToString(indent: true);
                File.WriteAllText(_streamsFilePath, str);

                Util.WriteYellow($"Update done about [{type}] - file updated [{_streamsFilePath}]");

                PromptStreamsInfo();

            }
            else
                Util.WriteYellow($"No updates about [{type}] done");
        }

        static void PromptManageWebcam() => PromptManageDevice("webcam");

        static void PromptManageScreen() => PromptManageDevice("screen");

        static void PromptManageAudioInput() => PromptManageDevice("microphone");

        static void PromptManageAudioOutput()
        {
            // Get list of audio output devices
            var audioOutputDevices = Rainbow.Medias.Devices.GetAudioOutputDevices();
            int index = 0;
            if (audioOutputDevices?.Count > 0)
            {
                Util.WriteYellow($"{Rainbow.Util.CR}Select an Audio output device:");
                foreach (var audioOutputDevice in audioOutputDevices)
                {
                    if(_audioOutputDevice?.Name == audioOutputDevice?.Name)
                        Util.WriteBlue($"\t[{index}] {audioOutputDevice?.Name} (CURRENT SELECTION)");
                    else
                        Util.WriteYellow($"\t[{index}] {audioOutputDevice?.Name}");
                    index++;
                }
                if (_audioOutputDevice is null)
                    Util.WriteBlue("\t[N] - No audio output (CURRENT SELECTION)");
                else
                    Util.WriteYellow("\t[N] - No audio output");
                Util.WriteYellow("\t[C] - Cancel");

                var audioOutputUpdated = false;
                var selected = false;

                var consoleKey = Console.ReadKey(true);
                if (char.IsDigit(consoleKey.KeyChar))
                {
                    var selection = int.Parse(consoleKey.KeyChar.ToString());
                    if ((selection >= 0) && (selection < index))
                    {
                        selected = true;
                        audioOutputUpdated = _audioOutputDevice?.Path != audioOutputDevices[selection].Path;

                        if (audioOutputUpdated)
                        {
                            _audioOutputDevice = audioOutputDevices[selection];
                            Util.WriteDarkYellow($"Audio Output Device selected: {_audioOutputDevice.Name} ({_audioOutputDevice.Path})]");
                        }
                        else
                            Util.WriteDarkYellow($"Same Audio Output Device selected");
                    }
                    else
                        Util.WriteDarkYellow($"Invalid key used ...");
                }
                else if ((consoleKey.Key == ConsoleKey.N))
                {
                    audioOutputUpdated = true;
                    selected = true;

                    _audioOutputDevice = null;

                    Util.WriteDarkYellow($"No more Audio Output Device used ...");
                }
                else if (consoleKey.Key == ConsoleKey.C)
                {
                    selected = true;
                    Util.WriteDarkYellow($"Cancel used ...");
                }
                else
                    Util.WriteDarkYellow($"Invalid key used ...");


                if (selected)
                {
                    if (audioOutputUpdated)
                    {
                        // Do we find the one we want ?
                        if (_audioOutputDevice is not null)
                        {
                            // Init / start audio output device
                            var newSdl2AudioOutput = new SDL2AudioOutput(_audioOutputDevice);
                            if ((newSdl2AudioOutput is null) || (!newSdl2AudioOutput.Init()) || (!newSdl2AudioOutput.Start()))
                            {
                                Util.WriteRed("Audio output device cannot be init / start");
                                return;
                            }
                            else
                            {
                                var previous = _sdl2AudioOutput;
                                _sdl2AudioOutput = newSdl2AudioOutput;
                                previous?.Dispose();
                            }
                        }
                        else
                        {
                            var previous = _sdl2AudioOutput;
                            _sdl2AudioOutput = null;
                            previous?.Dispose();
                        }
                    }
                }
                else
                {
                    PromptManageAudioOutput();
                }
            }
            else
            {
                // No audio output devices
            }
        }

        static void PromptStreamsInfo()
        {
            if (_streamsList is null) return;

            Util.WriteBlue($"{Rainbow.Util.CR}List of streams correctly set in config file: (doesn't mean they can be really used)");
            int index = 1;
            foreach (var stream in _streamsList)
            {
                Util.WriteBlue($"{index++:00} - {stream}");
                //index++;
            }
        }

        static void PromptTest()
        {
            
        }

#endregion Prompts

#region MediaInput management

        static Device? GetDevice(string name, string type)
        {
            List<Device>? devices = null;

            switch (type)
            {
                case "webcam":
                    devices = Rainbow.Medias.Devices.GetWebcamDevices(false).Select(s => (Device)s).ToList();
                    break;

                case "screen":
                    devices = Rainbow.Medias.Devices.GetScreenDevices(false).Select(s => (Device)s).ToList();
                    break;

                case "microphone":
                    devices = Rainbow.Medias.Devices.GetAudioInputDevices();
                    break;
            }

            return devices?.FirstOrDefault(d => d.Name == name);
        }

        static ScreenDevice? GetScreenDevice(string name)
        {
            var device = GetDevice(name, "screen");
            return (ScreenDevice?)device;
        }

        static WebcamDevice? GetWebcamDevice(string name)
        {
            var device = GetDevice(name, "webcam");
            return (WebcamDevice?)device;

        }

        static Device? GetMicrophoneDevice(string name)
        {
            var device = GetDevice(name, "microphone");
            return device;
        }

        static async Task<IMediaVideo?> GetMediaInputComposition(Stream stream)
        {
            Boolean success = true;
            List<Size> videoSize = [];

            if ( (_streamsList is null) || (stream is null) || (stream.VideoComposition is null) )
                return null;

            Util.WriteGreen($"Trying to create composition ...");
            foreach(var id in stream.VideoComposition)
            {
                Util.WriteGreen($"For composition, creating MediaInput for Stream:[{id}] ...");
                var s = _streamsList.FirstOrDefault(s => s.Id == id);
                if (s is not null)
                {
                    (var _, var iMediaVideo) = await GetMediaInputs(s, false);
                    if (iMediaVideo?.Init(true) == true)
                    {
                        _mediaInputVideoListForComposition.Add(iMediaVideo);

                        var size = new Size(iMediaVideo.Width, iMediaVideo.Height);
                        videoSize.Add(size);
                    }
                    else
                    {
                        // Cannot start 
                        Util.WriteRed($"For composition, MediaInput cannot be created for Stream:[{id}]");
                        success = false;
                        break;
                    }
                }
                else
                {
                    Util.WriteRed($"For composition, no Stream:[{id}] defined ...");
                    success = false;
                    break;
                }
            }

            if(success)
            {
                String? filter = null;
                var result = new MediaFiltered("mediaFiltered", _mediaInputVideoListForComposition, null);

                // Check if we have a filter already defined or we need to create it using a "template" ?
                if (stream.VideoFilterJsonNode is not null)
                {
                    Util.WriteGreen($"For composition, using \"template\" to create video filter ...");

                    // We need to get size of media inputs used.
                    String type = stream.VideoFilterJsonNode["type"];
                    var node = stream.VideoFilterJsonNode["data"];

                    if (node is not null)
                    {
                        Size? size;
                        int fps;

                        switch (type)
                        {
                            case "mosaic":
                                // Get fps
                                fps = node["fps"];
                                if (fps == 0)
                                    fps = 10;

                                // Get size of vignette
                                size = VideoFilter.SizeFromString(node["sizeVignette"]);
                                if (size is null)
                                    size = videoSize[0];

                                // Get layout
                                String layout = node["layout"];
                                if (String.IsNullOrEmpty(layout))
                                    layout = "G";

                                filter = VideoFilter.FilterToMosaic(
                                    srcVideoSize: videoSize,
                                    vignette: size.Value,
                                    layout: layout,
                                    fps: fps
                                    );

                                break;

                            case "overlay":

                                // Get size
                                size = VideoFilter.SizeFromString(node["size"]);
                                if (size is null)
                                    size = videoSize[0];

                                // Get fps
                                fps = node["fps"];
                                if (fps == 0)
                                    fps = 10;

                                // Get overlay size
                                var sizeOverlay = VideoFilter.SizeFromString(node["sizeOverlay"]);
                                if (sizeOverlay == null)
                                    sizeOverlay = new Size((int)(videoSize[0].Width / 4), (int)(videoSize[0].Height / 4));

                                // Get overlay position
                                String positionOverLay = node["positionOverlay"];
                                if (String.IsNullOrEmpty(positionOverLay))
                                    positionOverLay = "TR";

                                filter = VideoFilter.FilterToOverlay(
                                    srcMain: videoSize[0],
                                    srcOverlay: videoSize[1],
                                    dst: size.Value,
                                    dstOverlay: sizeOverlay.Value,
                                    dstOverlayPosition: positionOverLay,
                                    fps: fps
                                    );
                                break;
                        }
                    }
                    if (filter is null)
                        Util.WriteRed($"For composition, using \"template\" cannot create video filter ...");
                    else
                        Util.WriteRed($"For composition, using \"template\" filter created:\r\n{filter}");
                }
                else
                    filter = stream.VideoFilter;

                if (!String.IsNullOrEmpty(filter))
                {
                    if (result.SetVideoFilter(stream.VideoComposition, filter))
                    {
                        if (result.Init(true))
                            return result;
                        else
                            Util.WriteRed($"For composition, cannot init MediaFiltered ...");
                    }
                    else
                        Util.WriteRed($"For composition, video filter defined is incorrect:\r\n{stream.VideoFilter}");
                }
            }

            Util.WriteGreen($"Closing all stream used in composition ...");
            await CloseMediaInputsUsedInCompostionAsync();
            return null;
            
        }

        // Central point to create MediaInput (Audio, Video, Composition)
        static async Task<(IMediaAudio? audioInput, IMediaVideo? videoInput)> GetMediaInputs(Stream stream, bool closePreviousMediaInput)
        {
            IMediaAudio? audioInput = null;
            IMediaVideo? videoInput = null;
            if ((_streamsList is null) || (stream is null)) return (audioInput, videoInput);

            // Close previous media inputs
            if (closePreviousMediaInput)
            {
                await CloseMediaInputsAsync();
                Window.ClearRenderer(_outputWindow);
            }

            Boolean withComposition = stream.Media.Contains("composition");
            Boolean withAudio = stream.Media.Contains("audio");
            Boolean withVideo = withComposition || stream.Media.Contains("video");
            
            var options = stream.UriSettings;

            List<String> settings = [];
            if (options is not null)
            {
                foreach (var key in options.Keys)
                    settings.Add(key + ":" + options[key]);
            }

            String id = withAudio ? (withVideo ? "audio+video" : "audio") : (withVideo ? "video" : "");

            switch (stream.UriType)
            {
                case "webcam":
                    var webcamDevice = GetWebcamDevice(stream.Uri);
                    if (webcamDevice is not null)
                    {
                        videoInput = MediaInput.FromWebcamDevice(webcamDevice, false);
                        if (videoInput is null)
                            Util.WriteRed($"Cannot create and/or start MediaInput using this webcamDevice:[{webcamDevice}]");
                    }
                    else
                        Util.WriteRed($"Cannot get a webcamDevice with uri:[{stream.Uri}]");
                    break;

                case "screen":
                    var screenDevice = GetScreenDevice(stream.Uri);
                    if (screenDevice is not null)
                    {
                        videoInput = MediaInput.FromScreenDevice(screenDevice, false);
                        if(videoInput is null)
                            Util.WriteRed($"Cannot create and/or start MediaInput using this screenDevice:[{screenDevice}]");
                    }
                    else
                        Util.WriteRed($"Cannot get a screenDevice with uri:[{stream.Uri}]");
                    break;

                case "microphone":
                    var microphoneDevice = GetMicrophoneDevice(stream.Uri);
                    if (microphoneDevice is not null)
                    {
                        audioInput = new SDL2AudioInput(microphoneDevice);
                        if (audioInput is null)
                            Util.WriteRed($"Cannot create and/or start MediaInput using this screenDevice:[{microphoneDevice}]");
                    }
                    else
                        Util.WriteRed($"Cannot get a screenDevice with uri:[{stream.Uri}]");
                    break;

                default:
                    if(withComposition)
                    {
                        videoInput = await GetMediaInputComposition(stream);
                    }
                    else
                    {
                        Util.WriteGreen($"Creating InputStreamDevice: {stream} ...");
                        var inputStreamDevice = new InputStreamDevice(stream.Id, stream.Id, stream.Uri, withVideo: withVideo, withAudio: withAudio, loop: true, options: options);

                        Util.WriteGreen($"Creating MediaInput for [{stream.Id}] ...");
                        var mediaInput = new MediaInput(inputStreamDevice, forceLivestream: stream.ForceLiveStream);
                        if (withVideo)
                            videoInput = mediaInput;
                        if (withAudio)
                            audioInput = mediaInput;
                    }
                    break;
                }

            return (audioInput, videoInput);
        }

        static async Task CloseMediaInputsUsedInCompostionAsync()
        {
            // Close MediaInput used in Composition
            if (_mediaInputVideoListForComposition.Count > 0)
            {
                Util.WriteGreen("Stopping previous Video MediaInput used for Composition ...");
                foreach (var mediaInput in _mediaInputVideoListForComposition)
                {
                    mediaInput?.Dispose();
                }
                _mediaInputVideoListForComposition.Clear();
                Util.WriteDarkYellow("Previous Video MediaInput used for Composition are stoppped and disposed");
            }
        }

        static async Task CloseMediaInputsAsync()
        {
            // Close Audio MediaInputs
            if (_mediaInputAudio is not null)
            {
                _mediaInputAudio.OnAudioSample -= MediaInput_OnAudioSample;

                // Clear queue
                _sdl2AudioOutput?.ClearQueue();

                // Check if media input Audio and Video are related
                if ((_mediaInputVideo is not null) && (_mediaInputVideo.Id == _mediaInputAudio.Id))
                {
                    _mediaInputAudio = null;
                    Util.WriteDarkYellow("Previous Audio MediaInput will be stoppped and disposed in same time than the previous Video MediaInput");
                }
                else
                {
                    Util.WriteGreen("Stopping previous Audio MediaInput ...");
                    await _mediaInputAudio.StopAsync();
                    _mediaInputAudio.Dispose();
                    _mediaInputAudio = null;
                    Util.WriteDarkYellow("Previous Audio MediaInput stoppped and disposed");
                }
            }

            // Close Video MediaInputs
            if (_mediaInputVideo is not null)
            {
                // We inform that the video is now stopped
                _outputWindow.VideoStopped = true;

                _mediaInputVideo.OnImage -= MediaInput_OnImage;
                
                Window.DestroyTexture(_outputWindow);

                Util.WriteGreen("Stopping previous Video MediaInput ...");
                _mediaInputVideo.Dispose();
                _mediaInputVideo = null;
                Util.WriteDarkYellow("Previous Video MediaInput stoppped and disposed");
            }

            await CloseMediaInputsUsedInCompostionAsync();
        }

        static async Task UseStreamAsync(Stream? stream)
        {
            if (stream is null) return;

            (var iMediaAudio, var  iMediaVideo) = await GetMediaInputs(stream, true);
            var iMedia = (iMediaAudio is null) ? (IMedia?)iMediaVideo : (IMedia?)iMediaAudio;
            if (iMedia is null) return;

            Util.WriteGreen($"Init/Start MediaInput with [{iMedia.Id}] ...");
            if (iMedia.Init(true))
            {
                _outputWindow.VideoStopped = false;

                // Even if there is no video, we create/show it - it's title permits to know streams currently used
                Window.Create(_outputWindow);
                Window.Show(_outputWindow);
                Window.Restore(_outputWindow);
                Window.Raise(_outputWindow);

                Window.ClearRenderer(_outputWindow);

                if (iMediaAudio is not null)
                {
                    _mediaInputAudio = iMediaAudio;
                    _mediaInputAudio.OnAudioSample += MediaInput_OnAudioSample;
                    _mediaInputAudio.OnEndOfFile += MediaInputAudio_OnEndOfFile;
                }

                if (iMediaVideo is not null)
                {
                    _mediaInputVideo = iMediaVideo;
                    _mediaInputVideo.OnImage += MediaInput_OnImage;
                    _mediaInputVideo.OnEndOfFile += MediaInputVideo_OnEndOfFile;
                }
                
                String title = $"SDK C# v3.x - Streaming Audio:[{((_mediaInputAudio is not null) ? _mediaInputAudio.Name : "NONE")}] - Video:[{((_mediaInputVideo is not null) ? $"{_mediaInputVideo.Name} - {_mediaInputVideo.Width}x{_mediaInputVideo.Height}" : "NONE")}]";
                Window.UpdateTitle(_outputWindow, title);

                Util.WriteDarkYellow($"MediaInput initialized / started");
            }
            else
                Util.WriteRed($"MediaInput cannot be initialized / started !");
        }

#endregion MediaInput management

#region MediaInput Events

        private static void MediaInput_OnAudioSample(string mediaId, uint duration, byte[] sample)
        {
            // /!\ Don't need to use Main Thread
            if (canContinue)
                _sdl2AudioOutput?.QueueSample(sample);
        }

        private static void MediaInput_OnImage(string mediaId, int width, int height, int stride, IntPtr data, AVPixelFormat pixelFormat)
        {
            // /!\ Need to use Main Thread
            _actions.Add( new Action(() =>
            {
                if ( _outputWindow is null || _outputWindow.VideoStopped)
                    return;

                if (_outputWindow.Texture == IntPtr.Zero)
                    Window.CreateTexture(_outputWindow, width, height, pixelFormat);

                Window.UpdateTexture(_outputWindow, stride, data);
                Window.UpdateRenderer(_outputWindow);
            }));
        }

        private static void MediaInputAudio_OnEndOfFile(string mediaId)
        {
            Util.WriteGreen("Audio MediaInput reached End of File");
        }

        private static void MediaInputVideo_OnEndOfFile(string mediaId)
        {
            Util.WriteGreen("Video MediaInput reached End of File");
        }

#endregion MediaInput Events

#region READ CONFIGURATION

        static Boolean ReadStreamsSettings()
        {
            _streamsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}streams.json";
            if (!File.Exists(_streamsFilePath))
            {
                Util.WriteRed($"The file '{_streamsFilePath}' has not been found.");
                return false;
            }

            String jsonConfig = File.ReadAllText(_streamsFilePath);
            var jsonNode = JSON.Parse(jsonConfig);

            if ((jsonNode is null) || (!jsonNode.IsObject))
            {
                Util.WriteRed($"Cannot get JSON data from file '{_streamsFilePath}'.");
                return false;
            }

            var jsonExeSettings = jsonNode["streams"];
            if (jsonExeSettings?.IsArray == true)
            {
                List<Stream> compositionsList = [];
                _streamsList = [];
                foreach (var node in jsonExeSettings.Values)
                {
                    if (Stream.FromJsonNode(node, out Stream stream))
                    {
                        if (stream.Media.Equals("composition"))
                            compositionsList.Add(stream);
                        else
                            _streamsList.Add(stream);
                    }
                }

                // Check if media with composition are really useable (i.e. they reference media known)
                foreach (var stream in compositionsList)
                {
                    if (stream.VideoComposition is null)
                        continue;

                    var streamsFound = _streamsList.Where(x => stream.VideoComposition.Contains(x.Id) == true).ToList();
                    if (streamsFound.Count == stream.VideoComposition.Count)
                        _streamsList.Add(stream);
                }


                if (_streamsList.Count == 0)
                {
                    Util.WriteRed($"Cannot read 'streams' object (no Stream object created) - file:'{_streamsFilePath}'.");
                    return false;
                }
            }
            else
            {
                Util.WriteRed($"Cannot read 'streams' object OR invalid/missing data - file:'{_streamsFilePath}'.");
                return false;
            }

            autoPlay = jsonNode["autoPlay"];

            return true;
        }

        static Boolean ReadExeSettings()
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

            if (ExeSettings.FromJsonNode(jsonNode["exeSettings"], out _exeSettings))
            {
                // Set where log files must be stored
                NLogConfigurator.Directory = _exeSettings.LogFolderPath;

                // Init external librairies: FFmpeg and SDL2
                if (_exeSettings.UseAudioVideo)
                {
                    Rainbow.Medias.Helper.InitExternalLibraries(_exeSettings.FfmpegLibFolderPath, true);
                    //SDL2.SDL_SetHint(SDL2.SDL_HINT_RENDER_DRIVER, "opengles");
                    //SDL2.SDL_SetHint(SDL2.SDL_HINT_RENDER_BATCHING, "1");
                }
            }
            else
            {
                Util.WriteRed($"Cannot read 'exeSettings' object OR invalid/missing data - file:'{exeSettingsFilePath}'.");
                return false;
            }

            return true;
        }

#endregion READ CONFIGURATION

        [DllImport("user32.dll")]
        static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindowByCaption(IntPtr zeroOnly, string lpWindowName);

        public const int SW_RESTORE = 9;
        private static IntPtr consoleHandle = IntPtr.Zero;

        static void FocusConsoleWindow()
        {
            if(consoleHandle == IntPtr.Zero)
                consoleHandle = FindWindowByCaption(IntPtr.Zero, Console.Title);

            FocusWindow(consoleHandle);
        }

        static void FocusWindow(nint handle)
        {
            if (handle != IntPtr.Zero)
            {
                ShowWindowAsync(new HandleRef(null, handle), SW_RESTORE);
                SetForegroundWindow(handle);
            }
        }
    }
}
