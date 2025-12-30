using CommonSDL2;
using FFmpeg.AutoGen;
using Rainbow.Example.Common;
using Rainbow.Example.Common.SDL2;
using Rainbow.Medias;
using Rainbow.SimpleJSON;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Stream = Rainbow.Example.Common.Stream;

namespace ConsoleMediaPlayer
{
    // see https://dev.to/noah11012/using-sdl2-in-c-and-with-c-too-1l72
    // see https://jsayers.dev/tutorials/

    internal partial class ConsoleMediaPlayer
    {
        // --- To store settings 
        private static ExeSettings? _exeSettings;
        private static String _autoPlayAudio = "";
        private static String _autoPlayVideo = "";

        // --- path to file "streams.json"
        private static String _streamsFilePath = "";

        // --- To know if we need to quit the application or not
        private static Boolean _canContinue = true;

        // --- To manage streams (Audio, Video, Composition)
        private static StreamManager _streamManager = new();

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
            if ((_streamManager.streamsList is null) || (_streamManager.streamsList.Count == 0)) return;

            _streamManager.OnAudioEndOfFile += StreamManager_OnAudioEndOfFile;
            _streamManager.OnVideoEndOfFile += StreamManager_OnVideoEndOfFile;
            _streamManager.OnVideoStateChanged += StreamManager_OnVideoStateChanged;
            _streamManager.OnAudioStateChanged += StreamManager_OnAudioStateChanged;
            _streamManager.OnImage += StreamManager_OnImage;
            _streamManager.OnAudioSample += StreamManager_OnAudioSample;

            PromptStreamsInfo();

            PromptHelpMenu();

            UseStreamSetToAutoPlay();

            await MainLoop();
        }

        static async Task MainLoop()
        {
            // Loop until, ESC is used
            int simulatedKey = 0;

            while (_canContinue)
            {
                while (SDL2.SDL_PollEvent(out SDL2.SDL_Event e) > 0)
                {
                    switch (e.type)
                    {
                        case SDL2.SDL_EventType.SDL_QUIT:
                            _canContinue = false;
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
                            _canContinue = false;
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

                if(_canContinue)
                {
                    if (_actions.TryTake(out var action))
                        action.Invoke();
                }
            }

            // No more use streams
            await _streamManager.UseStreamsAsync(null, null);

            // Destroy SDL2 window
            Window.Destroy(_outputWindow);
        }

#region StreamManager events

        private static void StreamManager_OnAudioSample(string mediaId, uint duration, byte[] sample)
        {
            // /!\ Don't need to use Main Thread
            if (_canContinue)
                _sdl2AudioOutput?.QueueSample(sample);
        }

        private static void StreamManager_OnImage(string mediaId, int width, int height, int stride, nint data, AVPixelFormat pixelFormat)
        {
            // /!\ Need to use Main Thread
            _actions.Add(new Action(() =>
            {
                if (_outputWindow is null || _outputWindow.VideoStopped)
                    return;

                if (_outputWindow.Texture == IntPtr.Zero)
                    Window.CreateTexture(_outputWindow, width, height, pixelFormat);

                Window.UpdateTexture(_outputWindow, stride, data);
                Window.UpdateRenderer(_outputWindow);
            }));
        }

        private static void StreamManager_OnAudioStateChanged(string mediaId, bool isStarted, bool isPaused)
        {
            UpdateWindowTitle();
        }

        private static void StreamManager_OnVideoStateChanged(string mediaId, bool isStarted, bool isPaused)
        {
            if (isStarted)
            {
                _outputWindow.VideoStopped = false;

                Window.Create(_outputWindow);
                Window.Show(_outputWindow);
                Window.Restore(_outputWindow);
                Window.Raise(_outputWindow);

                Window.ClearRenderer(_outputWindow);

                UpdateWindowTitle();
            }
            else
            {
                // remove any actions
                while (_actions.TryTake(out _)) { }

                // We inform that the video is now stopped
                _outputWindow.VideoStopped = true;

                Window.DestroyTexture(_outputWindow);

                Window.Hide(_outputWindow);
            }
        }

        private static void StreamManager_OnVideoEndOfFile(string mediaId)
        {
            Util.WriteGreen($"Video MediaInput Id:{mediaId} reached End of File");
        }

        private static void StreamManager_OnAudioEndOfFile(string mediaId)
        {
            Util.WriteGreen($"Audio MediaInput Id:{mediaId} reached End of File");
        }

#endregion StreamManager events

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
            Util.WriteYellow($"[I] Input Stream selection (Audio, Video or Both)");
            Util.WriteYellow("");
            Util.WriteYellow($"[S] Screen management");
            Util.WriteYellow($"[M] Microphone (Audio Input) management");
            Util.WriteYellow($"[W] Webcam management{Rainbow.Util.CR}");
        }

        static void UseStreamSetToAutoPlay()
        {
            Stream? audioStream;
            Stream? videoStream;

            audioStream = _streamManager.streamsList?.FirstOrDefault(s => s.Id == _autoPlayAudio);
            videoStream = _streamManager.streamsList?.FirstOrDefault(s => s.Id == _autoPlayVideo);

            Util.WriteGreen($"Auto play set to Audio:[{_autoPlayAudio}] - Video:[{_autoPlayVideo}]. Trying to set config ...");

            var _ = _streamManager.UseStreamsAsync(videoStream, audioStream);
        }

        static void PromptLoadStreamSettings()
        {
            ReadStreamsSettings();
            PromptStreamsInfo();

            UseStreamSetToAutoPlay();
        }

        static void PromptCancelStreaming()
        {
            var _ = _streamManager.UseStreamsAsync(null, null);
        }

        static void PromptInputStreamSelection()
        {
            if (_streamManager.streamsList?.Count > 0)
            {
                var selected = false;
                List<Stream>? subStreams = null;
                String category = "";
                String categoryInfo = "";
                while (!selected)
                {
                    Util.WriteYellow($"{Rainbow.Util.CR}Do you want to select Stream with:");
                    Util.WriteYellow($"\t[B] (Both) Audio AND Video");
                    Util.WriteYellow($"\t[A] Audio");
                    Util.WriteYellow($"\t[V] Video");
                    Util.WriteYellow($"\t[R] Remove Stream");
                    Util.WriteYellow($"\t[C] Cancel");

                    var consoleKey = Console.ReadKey(true);
                    switch (consoleKey.Key)
                    {
                        case ConsoleKey.A:
                            category = "audio";
                            categoryInfo = "with [audio]";
                            selected = true;
                            Util.WriteDarkYellow($"Any with Audio selected ...");
                            subStreams = _streamManager.streamsList.FindAll(s => s.Media.Contains("audio"));
                            break;

                        case ConsoleKey.V:
                            category = "video";
                            categoryInfo = "with [video]";
                            selected = true;
                            Util.WriteDarkYellow($"Any with Video selected ...");
                            subStreams = _streamManager.streamsList.FindAll(s => s.Media.Contains("video") || s.Media.Contains("composition"));
                            break;

                        case ConsoleKey.B:
                            category = "audio+video";
                            categoryInfo = "with [audio+video]";
                            selected = true;
                            Util.WriteDarkYellow($"Audio and Video selected ...");
                            subStreams = _streamManager.streamsList.FindAll(s => s.Media == "audio+video");
                            break;

                        case ConsoleKey.R:
                            category = categoryInfo = "to remove";
                            selected = true;

                            subStreams = [];
                            if (_streamManager.currentAudioStream is not null)
                            {
                                var s = Stream.FromStream(_streamManager.currentAudioStream);
                                s.Media = "audio";
                                subStreams.Add(s);
                            }
                            if (_streamManager.currentVideoStream is not null)
                            {
                                var s = Stream.FromStream(_streamManager.currentVideoStream);
                                s.Media = "video";
                                subStreams.Add(s);
                            }

                            if (subStreams.Count > 0)
                                Util.WriteDarkYellow($"Remove stream selected ...");
                            else
                            {
                                Util.WriteGreen($"No stream to remove");
                                return;
                            }
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

                        Util.WriteYellow($"{Rainbow.Util.CR}Select the stream {categoryInfo}:");
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

                                        Task? task = null;
                                        switch(category)
                                        {
                                            case "to remove":
                                                if (stream.Id == _streamManager.currentAudioStream?.Id)
                                                    task = _streamManager.UseStreamsAsync(_streamManager.currentVideoStream, null);
                                                else
                                                    task = _streamManager.UseStreamsAsync(null, _streamManager.currentAudioStream);
                                                break;

                                            case "audio":
                                                task = _streamManager.UseStreamsAsync(_streamManager.currentVideoStream, stream);
                                                break;
                                            case "video":
                                                task = _streamManager.UseStreamsAsync(stream, _streamManager.currentAudioStream);
                                                break;
                                            case "audio+video":
                                                task = _streamManager.UseStreamsAsync(stream, stream);
                                                break;
                                            default:
                                                Util.WriteRed($"category not managed: {category}");
                                                break;

                                        }
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
            var streamAsDevice = _streamManager.streamsList?.FindAll(s => s.UriType == type);
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
                _streamManager.streamsList?.RemoveAll(s => s.UriType == type);
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
                    _streamManager.streamsList ??= [];
                    _streamManager.streamsList.Add(stream);
                }

                // Create array
                _streamManager.streamsList ??= [];
                var jsonArray = new JSONArray();
                foreach (var stream in _streamManager.streamsList)
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
            if (_streamManager.streamsList is null) return;

            Util.WriteBlue($"{Rainbow.Util.CR}List of streams correctly set in config file: (doesn't mean they can be really used)");
            int index = 1;
            foreach (var stream in _streamManager.streamsList)
            {
                Util.WriteBlue($"{index++:00} - {stream}");
                //index++;
            }
        }

        static void PromptTest()
        {
            
        }

#endregion Prompts

        static void UpdateWindowTitle()
        {
            String title = $"SDK C# v3.x - Streaming Audio:[{((_streamManager.mediaInputAudio is not null) ? _streamManager.mediaInputAudio.Name : "NONE")}] - Video:[{((_streamManager.mediaInputVideo is not null) ? $"{_streamManager.mediaInputVideo.Name} - {_streamManager.mediaInputVideo.Width}x{_streamManager.mediaInputVideo.Height}" : "NONE")}]";
            Window.UpdateTitle(_outputWindow, title);
        }

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
                _streamManager.streamsList = [];
                foreach (var node in jsonExeSettings.Values)
                {
                    if (Stream.FromJsonNode(node, out Stream stream))
                    {
                        if (stream.Media.Equals("composition"))
                            compositionsList.Add(stream);
                        else
                            _streamManager.streamsList.Add(stream);
                    }
                }

                // Check if media with composition are really useable (i.e. they reference media known)
                foreach (var stream in compositionsList)
                {
                    if (stream.VideoComposition is null)
                        continue;

                    var streamsFound = _streamManager.streamsList.Where(x => stream.VideoComposition.Contains(x.Id) == true).ToList();
                    if (streamsFound.Count == stream.VideoComposition.Count)
                        _streamManager.streamsList.Add(stream);
                }


                if (_streamManager.streamsList.Count == 0)
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

            _autoPlayAudio = jsonNode["autoPlayAudio"];
            _autoPlayVideo = jsonNode["autoPlayVideo"];

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
