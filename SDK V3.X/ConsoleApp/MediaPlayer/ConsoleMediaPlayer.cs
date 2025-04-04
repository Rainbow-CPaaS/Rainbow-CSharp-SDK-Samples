using FFmpeg.AutoGen;
using Rainbow.Example.Common;
using Rainbow.Medias;
using Rainbow.SimpleJSON;
using Stream = Rainbow.Example.Common.Stream;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace ConsoleMediaPlayer
{
    // see https://dev.to/noah11012/using-sdl2-in-c-and-with-c-too-1l72
    // see https://jsayers.dev/tutorials/

    internal class ConsoleMediaPlayer
    {
        // --- To store settings and to know which Stream is selected
        private static ExeSettings? _exeSettings;
        private static List<Stream>? _streamsList;
        private static String _streamsFilePath = "";
        private static Stream? _currentStream;

        // --- IMedia used / created for Input based on the selected Stream
        private static IMediaAudio? _mediaInputAudio;
        private static IMediaVideo? _mediaInputVideo;

        // --- To know if we need to quit the application or not
        private static Boolean canContinue = true;

        // --- To manage Audio Output
        private static SDL2AudioOutput? _sdl2AudioOutput = null;
        private static Device? _audioOutputDevice = null;

        // --- To manage Window Output
        private static Window _outputWindow = new Window();

        // --- To ensure to launch action on main thread
        private static BlockingCollection<Action> _actions = new();

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

            PromptInfoMenu();

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

                CheckUpdateWindowRenderer(_outputWindow);

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

                        case (int)ConsoleKey.I:
                            PromptInfoMenu();
                            break;

                        case (int)ConsoleKey.A:
                            PromptManageAudioOutput();
                            break;

                        case (int)ConsoleKey.C:
                            PromptStreamSelection();
                            break;

                        case (int)ConsoleKey.F:
                            ToggleFullScreen(_outputWindow);
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

            // Destroy SDL2 window
            DestroyWindow(_outputWindow);

            await CloseMediaInputAsync(true, true);
        }

#region Prompts
        static void PromptInfoMenu()
        {
            Util.WriteGreen($"{Rainbow.Util.CR}Console Window must have the focus to use the keyboard");
            Util.WriteYellow($"[ESC] to quit");
            Util.WriteYellow($"[I] Information message (this one)");
            Util.WriteYellow("");
            Util.WriteYellow($"[F] Full screen - toggle");
            Util.WriteYellow("");
            Util.WriteYellow($"[A] Audio Ouput selection");
            Util.WriteYellow($"[C] Choose Input Stream (Audio, Video or Both)");
            Util.WriteYellow("");
            Util.WriteYellow($"[S] Screen management");
            Util.WriteYellow($"[M] Microphone (Audio Input) management");
            Util.WriteYellow($"[W] Webcam management{Rainbow.Util.CR}");
        }

        static void PromptStreamSelection()
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
                            subStreams = _streamsList.FindAll(s => s.Media.Contains("video"));
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

        static async Task<(IMediaAudio?, IMediaVideo?)> GetMediaInput(Stream stream)
        {
            IMediaAudio? iMediaAudio = null;
            IMediaVideo? iMediaVideo = null;
            if ((_streamsList is null) || (stream is null)) return (iMediaAudio, iMediaVideo);

            _currentStream = stream;

            Boolean withAudio = _currentStream.Media.Contains("audio");
            Boolean withVideo = _currentStream.Media.Contains("video");

            await CloseMediaInputAsync(withAudio, withVideo);

            ClearWindowRenderer(_outputWindow);

            var options = _currentStream.UriSettings;

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
                        iMediaVideo = MediaInput.FromWebcamDevice(webcamDevice, false);
                        if (iMediaVideo is null)
                            Util.WriteRed($"Cannot create and/or start MediaInput using this webcamDevice:[{webcamDevice}]");
                    }
                    else
                        Util.WriteRed($"Cannot get a webcamDevice with uri:[{stream.Uri}]");
                    break;

                case "screen":
                    var screenDevice = GetScreenDevice(stream.Uri);
                    if (screenDevice is not null)
                    {
                        iMediaVideo = MediaInput.FromScreenDevice(screenDevice, false);
                        if(iMediaVideo is null)
                            Util.WriteRed($"Cannot create and/or start MediaInput using this screenDevice:[{screenDevice}]");
                    }
                    else
                        Util.WriteRed($"Cannot get a screenDevice with uri:[{stream.Uri}]");
                    break;

                case "microphone":
                    var microphoneDevice = GetMicrophoneDevice(stream.Uri);
                    if (microphoneDevice is not null)
                    {
                        iMediaAudio = new SDL2AudioInput(microphoneDevice);
                        if (iMediaAudio is null)
                            Util.WriteRed($"Cannot create and/or start MediaInput using this screenDevice:[{microphoneDevice}]");
                    }
                    else
                        Util.WriteRed($"Cannot get a screenDevice with uri:[{stream.Uri}]");
                    break;

                default:
                    Util.WriteGreen($"Creating InputStreamDevice: {_currentStream} ...");
                    var inputStreamDevice = new InputStreamDevice(id, _currentStream.Id, _currentStream.Uri, withVideo: withVideo, withAudio: withAudio, loop: true, options: options);

                    Util.WriteGreen($"Creating MediaInput for [{id}] ...");
                    var mediaInput = new MediaInput(inputStreamDevice, forceLivestream: _currentStream.ForceLiveStream);
                    if(withVideo)
                        iMediaVideo = mediaInput;
                    if(withAudio)
                        iMediaAudio = mediaInput;
                    break;
            }

            return (iMediaAudio, iMediaVideo);
        }

        static async Task CloseMediaInputAsync(Boolean audio, Boolean video)
        {
            // Remove event(s)
            if(audio && _mediaInputAudio is not null)
                _mediaInputAudio.OnAudioSample -= MediaInput_OnAudioSample;
            if (video && _mediaInputVideo is not null)
            {
                _mediaInputVideo.OnImage -= MediaInput_OnImage;
                DestroyTexture(_outputWindow);
            }

            if(audio)
            {
                // Clear queue
                _sdl2AudioOutput?.ClearQueue();

                if(_mediaInputAudio is not null)
                {
                    Util.WriteGreen("Stopping previous Audio MediaInput ...");
                    if ((_mediaInputVideo is not null) && (_mediaInputVideo.Id == _mediaInputAudio.Id))
                    {
                        _mediaInputAudio = null;
                        Util.WriteDarkYellow("Previous Audio MediaInput stoppped");
                    }
                    else
                    {
                        await _mediaInputAudio.StopAsync();
                        _mediaInputAudio.Dispose();
                        _mediaInputAudio = null;
                        Util.WriteDarkYellow("Previous Audio MediaInput stoppped and disposed");
                    }
                }
            }

            if(video)
            {
                if (_mediaInputVideo is not null)
                {

                    Util.WriteGreen("Stopping previous Video MediaInput ...");
                    if ((_mediaInputAudio is not null) && (_mediaInputVideo.Id == _mediaInputAudio.Id))
                    {
                        _mediaInputVideo = null;
                        Util.WriteDarkYellow("Previous Video MediaInput stoppped");
                    }
                    else
                    {
                        await _mediaInputVideo.StopAsync();
                        _mediaInputVideo.Dispose();
                        _mediaInputVideo = null;
                        Util.WriteDarkYellow("Previous Video MediaInput stoppped and disposed");
                    }
                    
                }
            }
        }

        static async Task UseStreamAsync(Stream stream)
        {
            if (stream is null) return;

            (var iMediaAudio, var  iMediaVideo) = await GetMediaInput(stream);
            var iMedia = (iMediaAudio is null) ? (IMedia?)iMediaVideo : (IMedia?)iMediaAudio;
            if (iMedia is null) return;

            Util.WriteGreen($"Init/Start MediaInput with [{iMedia.Id}] ...");
            if (iMedia.Init(true))
            {
                // Even if there is no video, we create/show it - it's title permits to know streams currently used
                CreateWindow(_outputWindow);
                ShowWindow(_outputWindow);
                ClearWindowRenderer(_outputWindow);

                if (iMediaAudio is not null)
                {
                    _mediaInputAudio = iMediaAudio;
                    _mediaInputAudio.OnAudioSample += MediaInput_OnAudioSample;
                }

                if (iMediaVideo is not null)
                {
                    _mediaInputVideo = iMediaVideo;
                    _mediaInputVideo.OnImage += MediaInput_OnImage;
                }

                UpdateWindowTitle(_outputWindow);

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
                if (_outputWindow is null)
                    return;

                if (_outputWindow.Texture == IntPtr.Zero)
                    CreateTexture(_outputWindow, width, height, pixelFormat);

                UpdateTexture(_outputWindow, stride, data);
                UpdateWindowRenderer(_outputWindow);
            }));
        }

#endregion MediaInput Events

#region Window / Renderer / Texture

        static void ToggleFullScreen(Window window)
        {
            if (window is not null && window.Handle != IntPtr.Zero)
            {
                window.FullScreen = !window.FullScreen;
                SDL2.SDL_SetWindowFullscreen(window.Handle, (uint)(window.FullScreen ? SDL2.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP : 0));
                RestoreWindow(window); // If the windows was minimized, it's no more the case
                RaiseWindow(window);  // To have the window on top - like SetForegroundWindow()
            }
        }

        static void CreateWindow(Window window)
        {
            if(window is null || window.Handle != IntPtr.Zero)
                return;

            var flags = SDL2.SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL2.SDL_WindowFlags.SDL_WINDOW_SHOWN;
            window.Handle = SDL2.SDL_CreateWindow("Output", SDL2.SDL_WINDOWPOS_UNDEFINED, SDL2.SDL_WINDOWPOS_UNDEFINED, 800, 600, flags);

            if (window.Handle != IntPtr.Zero)
            {
                window.Id = SDL2.SDL_GetWindowID(window.Handle);
                window.Renderer = SDL2.SDL_CreateRenderer(window.Handle, -1,
                        SDL2.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL2.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
                ClearWindowRenderer(window);
            }
        }

        static void DestroyWindow(Window window)
        {
            if (window is null) return;

            DestroyTexture(window);
            DestroyRenderer(window);

            if (window.Handle != IntPtr.Zero)
            {
                SDL2.SDL_DestroyWindow(window.Handle);
                window.Handle = IntPtr.Zero;
            }
        }

        static void HideWindow(Window window)
        {
            if (window is not null && window.Handle != IntPtr.Zero)
                SDL2.SDL_HideWindow(window.Handle);
        }

        static void ShowWindow(Window window)
        {
            if (window is not null && window.Handle != IntPtr.Zero)
                SDL2.SDL_ShowWindow(window.Handle);
        }

        static void RaiseWindow(Window window)
        {
            if (window is not null && window.Handle != IntPtr.Zero)
                SDL2.SDL_RaiseWindow(window.Handle);
        }

        static void RestoreWindow(Window window)
        {
            if (window is not null && window.Handle != IntPtr.Zero)
                SDL2.SDL_RestoreWindow(window.Handle);
        }

        static void UpdateWindowTitle(Window window)
        {
            if (window is not null && window.Handle != IntPtr.Zero)
            {
                String title = $"SDK C# v3.x - Streaming Audio:[{((_mediaInputAudio is not null) ? _mediaInputAudio.Name : "NONE")}] - Video:[{((_mediaInputVideo is not null) ? $"{_mediaInputVideo.Name} - {_mediaInputVideo.Width}x{_mediaInputVideo.Height}" : "NONE")}]";
                SDL2.SDL_SetWindowTitle(window.Handle, title);
            }
        }

        static void CreateTexture(Window window, int w, int h, AVPixelFormat pixelFormat)
        {
            if( (window is not null && window.Renderer != IntPtr.Zero) )
            {
                // Destroy previous texture
                DestroyTexture(window);

                var sdlFormat = SDL2Helper.GetPixelFormat(pixelFormat);
                if (sdlFormat == SDL2.SDL_PIXELFORMAT_UNKNOWN)
                {
                    Util.WriteRed($"Cannot get SDL pixel format using ffmpeg video foramt:[{pixelFormat}]");
                    return;
                }

                // Create texture
                window.Texture = SDL2.SDL_CreateTexture(window.Renderer, sdlFormat, (int)SDL2.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, w, h);
                if (window.Texture == IntPtr.Zero)
                    Util.WriteRed($"Cannot create texture");
            }
            else
                Util.WriteRed($"Cannot create texture - No window renderer");
        }

        static void DestroyTexture(Window window)
        {
            if ((window is not null && window.Texture != IntPtr.Zero))
            {
                SDL2.SDL_DestroyTexture(window.Texture);
                window.Texture = IntPtr.Zero;
            }
        }

        static void DestroyRenderer(Window window)
        {
            if ((window is not null && window.Renderer != IntPtr.Zero))
            {
                SDL2.SDL_DestroyRenderer(window.Renderer);
                window.Renderer = IntPtr.Zero;
            }
        }

        static void UpdateTexture(Window window,int stride, IntPtr data)
        {
            if (window is not null && window.Texture != IntPtr.Zero)
            {
                var _ = SDL2.SDL_UpdateTexture(window.Texture, IntPtr.Zero, data, stride);
            }
        }

        static void ClearWindowRenderer(Window window)
        {
            if (window is not null && window.Renderer != IntPtr.Zero)
            { 
                if (SDL2.SDL_RenderClear(window.Renderer) == 0)
                    SDL2.SDL_RenderPresent(window.Renderer);
            }
        }

        static void UpdateWindowRenderer(Window window)
        {
            if (window is not null && window.Renderer != IntPtr.Zero && window.Texture != IntPtr.Zero)
            {
                window.NeedRendereUpdate = false;
                if (SDL2.SDL_RenderCopy(window.Renderer, window.Texture, IntPtr.Zero, IntPtr.Zero) == 0)
                    SDL2.SDL_RenderPresent(window.Renderer);
            }
        }

        static void CheckUpdateWindowRenderer(Window window)
        {
            if (window is not null && window.NeedRendereUpdate)
                UpdateWindowRenderer(window);
        }

#endregion Window / Renderer / Texture

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
                _streamsList = [];
                foreach (var node in jsonExeSettings.Values)
                {
                    if (Stream.FromJsonNode(node, out Stream stream))
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
                    SDL2.SDL_SetHint(SDL2.SDL_HINT_RENDER_DRIVER, "opengles");
                    SDL2.SDL_SetHint(SDL2.SDL_HINT_RENDER_BATCHING, "1");
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
        public static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr zeroOnly, string lpWindowName);
        public const int SW_RESTORE = 9;
        static void FocusConsoleWindow()
        {
            // /!\ It's highly recommended to set a unique Title to the console to have this working
            IntPtr handle = FindWindowByCaption(IntPtr.Zero, Console.Title);
            ShowWindowAsync(new HandleRef(null, handle), SW_RESTORE);
            SetForegroundWindow(handle);
        }

    }
}
