using FFmpeg.AutoGen;
using Rainbow.Example.Common;
using Rainbow.Example.CommonSDL2;
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
        private static List<Stream>? _streamsList;

        private static String _autoPlayAudio = "";
        private static String _autoPlayVideo = "";
        private static String _autoPlaySharing = "";
        
        // --- path to file "streams.json"
        private static String _streamsFilePath = "";

        // --- To know if we need to quit the application or not
        private static Boolean _canContinue = true;

        // --- To manage streams (Audio, Video, Composition)
        private static readonly StreamManager _streamManager = new();

        // --- To manage Audio Output
        private static SDL2AudioOutput? _sdl2AudioOutput = null;
        private static Device? _audioOutputDevice = null;

        // --- To manage Window Output - Video or Sharing
        private static readonly Window _outputVideoWindow = new();
        private static readonly Window _outputSharingWindow = new();

        // --- To ensure to launch action on main thread
        private static readonly BlockingCollection<Action> _actions = [];

        static async Task Main()
        {
            // Need to set an unique title
            ConsoleAbstraction.Title = $"SDK C# - MediaPlayer [{Guid.NewGuid()}]";

            ConsoleAbstraction.WriteDarkYellow($"{Global.ProductName()} v{Global.FileVersion()}");

            if (!ReadExeSettings())
                return;

            if (!ReadStreamsSettings())
                return;

            if (_exeSettings is null) return;
            //if ((_streamManager.streamsList is null) || (_streamManager.streamsList.Count == 0)) return;

            _streamManager.OnStreamOpened += StreamManager_OnStreamOpened;
            _streamManager.OnStreamClosed += StreamManager_OnStreamClosed;

            _streamManager.OnAudioSample += StreamManager_OnAudioSample;
            _streamManager.OnAudioError += StreamManager_OnAudioError;
            _streamManager.OnAudioEndOfFile += StreamManager_OnAudioEndOfFile;

            _streamManager.OnVideoImage += StreamManager_OnVideoImage;
            _streamManager.OnVideoError += StreamManager_OnVideoError;
            _streamManager.OnVideoEndOfFile += StreamManager_OnVideoEndOfFile;

            _streamManager.OnSharingImage += StreamManager_OnSharingImage;
            _streamManager.OnSharingError += StreamManager_OnSharingError;
            _streamManager.OnSharingEndOfFile += StreamManager_OnSharingEndOfFile;

            PromptStreamsInfo();

            PromptHelpMenu();

            UseStreams();

            await MainLoop();
        }

        static async Task MainLoop()
        {
            // Loop until, ESC is used
            int simulatedKey = 0;
            Action? action;

            // We loop until we need to quit AND there is no more action to process (to ensure to deal correctly with window management)
            while (_canContinue || _actions.Count > 0)
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
                                    if (e.window.windowID == _outputVideoWindow.Id)
                                        _outputVideoWindow.NeedRendererUpdate = true;
                                    else if (e.window.windowID == _outputSharingWindow.Id)
                                        _outputSharingWindow.NeedRendererUpdate = true;
                                    break;
                            }
                            break;

                        case SDL2.SDL_EventType.SDL_RENDER_TARGETS_RESET:
                            if (e.window.windowID == _outputVideoWindow.Id)
                                _outputVideoWindow.NeedRendererUpdate = true;
                            else if (e.window.windowID == _outputSharingWindow.Id)
                                _outputSharingWindow.NeedRendererUpdate = true;
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

                Window.CheckUpdateRenderer(_outputVideoWindow);
                Window.CheckUpdateRenderer(_outputSharingWindow);

                // Check Keys fom Console Window
                if (ConsoleAbstraction.KeyAvailable || simulatedKey!=0)
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
                        key = (userInput is null) ? 0 : (int)(userInput.Value.Key);
                    }


                    switch (key)
                    {
                        case (int)ConsoleKey.Escape:
                            PromptCancelStreaming();
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
                            Window.ToggleFullScreen(_outputVideoWindow);
                            break;

                        case (int)ConsoleKey.C:
                            PromptCancelStreaming();
                            break;

                        case (int)ConsoleKey.S:
                            await PromptManageScreenAsync();
                            break;

                        case (int)ConsoleKey.M:
                            await PromptManageAudioInputAsync();
                            break;

                        case (int)ConsoleKey.W:
                            await PromptManageWebcamAsync();
                            break;

                        case (int)ConsoleKey.T:
                            PromptTest();
                            break;
                    }
                }

                if (_actions.TryTake(out action))
                {
                    // /!\ Action must be performed on Main Thread !!!
                    action.Invoke();
                }
            }

            // Destroy SDL2 window
            Window.Destroy(_outputVideoWindow);
            Window.Destroy(_outputSharingWindow);
        }

        static void UseStreams()
        {
            Action action = async () =>
            {

                Dictionary < int, String> streamsToUse = [];
                if (!String.IsNullOrEmpty(_autoPlayAudio))
                {
                    var stream = _streamsList?.FirstOrDefault(s => s.Id == _autoPlayAudio);
                    if(stream is null)
                        ConsoleAbstraction.WriteRed($"Audio - AutoPlay - Stream with Id [{_autoPlayAudio}] not found ");
                    else
                        streamsToUse[Rainbow.Consts.Media.AUDIO] = stream.Id;
                }

                if (!String.IsNullOrEmpty(_autoPlayVideo))
                {
                    var stream = _streamsList?.FirstOrDefault(s => s.Id == _autoPlayVideo);
                    if (stream is null)
                        ConsoleAbstraction.WriteRed($"Video - AutoPlay - Stream with Id [{_autoPlayVideo}] not found ");
                    else
                        streamsToUse[Rainbow.Consts.Media.VIDEO] = stream.Id;
                }

                if (!String.IsNullOrEmpty(_autoPlaySharing))
                {
                    var stream = _streamsList?.FirstOrDefault(s => s.Id == _autoPlaySharing);
                    if (stream is null)
                        ConsoleAbstraction.WriteRed($"Sharing - AutoPlay - Stream with Id [{_autoPlaySharing}] not found ");
                    else
                        streamsToUse[Rainbow.Consts.Media.SHARING] = stream.Id;
                }

                _streamManager.SetNewConfiguration(_streamsList, streamsToUse); 
            };

            Task.Factory.StartNew(action);
        }

    #region StreamManager events

        private static void StreamManager_OnStreamClosed(string streamId, int media)
        {
            switch (media)
            {
                case Rainbow.Consts.Media.AUDIO:
                    if (_sdl2AudioOutput is not null)
                    {
                        ConsoleAbstraction.WriteGray($"Audio is stopped - Stream:[{streamId}] - Clear Sdl2AudioOutput queue");
                        _sdl2AudioOutput.ClearQueue();
                    }

                    // /!\ Need to use Main Thread
                    _actions.Add(new Action(() =>
                    {
                        UpdateVideoWindowTitle();
                        UpdateSharingWindowTitle();
                    }));
                    break;

                case Rainbow.Consts.Media.VIDEO:
                    // We inform that the video is now stopped
                    _outputVideoWindow?.VideoStopped = true;

                    // /!\ Need to use Main Thread
                    _actions.Add(new Action(() =>
                    {
                        if (_outputVideoWindow is null) return;

                        Window.DestroyTexture(_outputVideoWindow);
                        Window.Hide(_outputVideoWindow);

                        ConsoleAbstraction.WriteGray($"Video is stopped - Stream:[{streamId}]");
                    }));
                    break;

                case Rainbow.Consts.Media.SHARING:
                    // We inform that the sharing is now stopped
                    _outputSharingWindow?.VideoStopped = true;

                    // /!\ Need to use Main Thread
                    _actions.Add(new Action(() =>
                    {
                        if (_outputSharingWindow is null) return;

                        Window.DestroyTexture(_outputSharingWindow);
                        Window.Hide(_outputSharingWindow);

                        ConsoleAbstraction.WriteGray($"Sharing is stopped - Stream:[{streamId}]");
                    }));
                    break;
            }
        }

        private static void StreamManager_OnStreamOpened(string streamId, int media)
        {
            switch (media)
            {
                case Rainbow.Consts.Media.AUDIO:
                    ConsoleAbstraction.WriteGray($"Audio is started - Stream:[{streamId}]");

                    // /!\ Need to use Main Thread
                    _actions.Add(new Action(() =>
                    {
                        UpdateVideoWindowTitle();
                    }));
                    break;

                case Rainbow.Consts.Media.VIDEO:
                    // /!\ Need to use Main Thread
                    _actions.Add(new Action(() =>
                    {
                        if (_outputVideoWindow.VideoStopped)
                        {
                            ConsoleAbstraction.WriteGray($"Video is started - Stream:[{streamId}]");
                            
                            Window.Create(_outputVideoWindow);
                            Window.Show(_outputVideoWindow);
                            Window.Restore(_outputVideoWindow);
                            Window.Raise(_outputVideoWindow);

                            Window.ClearRenderer(_outputVideoWindow);

                            UpdateVideoWindowTitle();

                            _outputVideoWindow.VideoStopped = false;
                        }
                    }));
                    break;

                case Rainbow.Consts.Media.SHARING:
                    // /!\ Need to use Main Thread
                    _actions.Add(new Action(() =>
                    {
                        if (_outputSharingWindow.VideoStopped)
                        {
                            ConsoleAbstraction.WriteGray($"Sharing is started - Stream:[{streamId}]");
                            _outputSharingWindow.VideoStopped = false;

                            Window.Create(_outputSharingWindow);
                            Window.Show(_outputSharingWindow);
                            Window.Restore(_outputSharingWindow);
                            Window.Raise(_outputSharingWindow);

                            Window.ClearRenderer(_outputSharingWindow);

                            UpdateSharingWindowTitle();
                        }
                    }));
                    break;
            }
        }

        private static void StreamManager_OnAudioSample(string mediaId, uint duration, byte[] sample)
        {
            // /!\ Don't need to use Main Thread
            if (_canContinue)
                _sdl2AudioOutput?.QueueSample(sample);
        }

        private static void StreamManager_OnAudioEndOfFile(string mediaId)
        {
            ConsoleAbstraction.WriteGreen($"Audio MediaInput Id:{mediaId} reached End of File");
        }

        private static void StreamManager_OnAudioError(string mediaId, string message)
        {
            ConsoleAbstraction.WriteRed($"Audio MediaInput Id:{mediaId} error: {message}");
        }

        private static void StreamManager_OnVideoImage(string mediaId, int width, int height, int stride, nint data, AVPixelFormat pixelFormat)
        {
            // /!\ Need to use Main Thread
            _actions.Add(new Action(() =>
            {
                if (_outputVideoWindow is null || _outputVideoWindow.VideoStopped)
                    return;

                if (_outputVideoWindow.Texture == IntPtr.Zero)
                    Window.CreateTexture(_outputVideoWindow, width, height, pixelFormat);

                Window.UpdateTexture(_outputVideoWindow, stride, data);
                Window.UpdateRenderer(_outputVideoWindow);
            }));
        }

        private static void StreamManager_OnVideoEndOfFile(string mediaId)
        {
            ConsoleAbstraction.WriteGreen($"Video MediaInput Id:{mediaId} reached End of File");
        }

        private static void StreamManager_OnVideoError(string mediaId, string message)
        {
            ConsoleAbstraction.WriteRed($"Video MediaInput Id:{mediaId} error: {message}");
        }

        private static void StreamManager_OnSharingImage(string mediaId, int width, int height, int stride, nint data, AVPixelFormat pixelFormat)
        {
            // /!\ Need to use Main Thread
            _actions.Add(new Action(() =>
            {
                if (_outputSharingWindow is null || _outputSharingWindow.VideoStopped)
                    return;

                if (_outputSharingWindow.Texture == IntPtr.Zero)
                    Window.CreateTexture(_outputSharingWindow, width, height, pixelFormat);

                Window.UpdateTexture(_outputSharingWindow, stride, data);
                Window.UpdateRenderer(_outputSharingWindow);
            }));
        }

        private static void StreamManager_OnSharingEndOfFile(string mediaId)
        {
            ConsoleAbstraction.WriteGreen($"Sharing MediaInput Id:{mediaId} reached End of File");
        }

        private static void StreamManager_OnSharingError(string mediaId, string message)
        {
            ConsoleAbstraction.WriteRed($"Sharing MediaInput Id:{mediaId} error: {message}");
        }

#endregion StreamManager events

#region Prompts
        static void PromptHelpMenu()
        {
            ConsoleAbstraction.WriteGreen($"{Rainbow.Util.CR}Console Window must have the focus to use the keyboard");
            ConsoleAbstraction.WriteYellow($"[ESC] to quit");
            ConsoleAbstraction.WriteYellow($"[H] Help message (this one)");
            ConsoleAbstraction.WriteYellow($"[L] Load / reload stream settings");
            ConsoleAbstraction.WriteYellow("");
            ConsoleAbstraction.WriteYellow($"[F] Full screen - toggle");
            ConsoleAbstraction.WriteYellow($"[C] Cancel /Stop current streaming");
            ConsoleAbstraction.WriteYellow("");
            ConsoleAbstraction.WriteYellow($"[A] Audio Ouput selection");
            ConsoleAbstraction.WriteYellow($"[I] Input Stream selection (Audio, Video or Both)");
            ConsoleAbstraction.WriteYellow("");
            ConsoleAbstraction.WriteYellow($"[S] Screen management");
            ConsoleAbstraction.WriteYellow($"[M] Microphone (Audio Input) management");
            ConsoleAbstraction.WriteYellow($"[W] Webcam management{Rainbow.Util.CR}");
        }

        static void PromptLoadStreamSettings()
        {
            ConsoleAbstraction.WriteYellow("=> PromptLoadStreamSettings()");
            ReadStreamsSettings();
            PromptStreamsInfo();

            UseStreams();
        }

        static void PromptCancelStreaming()
        {
            ConsoleAbstraction.WriteYellow("=> PromptCancelStreaming()");
            _streamManager.SetNewConfiguration(_streamsList, null);
        }

        static void PromptInputStreamSelection()
        {
            /*
            if (_streamsList?.Count > 0)
            {
                var selected = false;
                List<Stream>? subStreams = null;
                String category = "";
                String categoryInfo = "";
                while (!selected)
                {
                    ConsoleAbstraction.WriteYellow($"{Rainbow.Util.CR}Do you want to select Stream with:");
                    ConsoleAbstraction.WriteYellow($"\t[B] (Both) Audio AND Video");
                    ConsoleAbstraction.WriteYellow($"\t[A] Audio");
                    ConsoleAbstraction.WriteYellow($"\t[V] Video");
                    ConsoleAbstraction.WriteYellow($"\t[R] Remove Stream");
                    ConsoleAbstraction.WriteYellow($"\t[C] Cancel");

                    var consoleKey = ConsoleAbstraction.ReadKey();
                    switch (consoleKey?.Key)
                    {
                        case ConsoleKey.A:
                            category = "audio";
                            categoryInfo = "with [audio]";
                            selected = true;
                            ConsoleAbstraction.WriteDarkYellow($"Any with Audio selected ...");
                            subStreams = _streamsList.FindAll(s => s.Media.Contains("audio"));
                            break;

                        case ConsoleKey.V:
                            category = "video";
                            categoryInfo = "with [video]";
                            selected = true;
                            ConsoleAbstraction.WriteDarkYellow($"Any with Video selected ...");
                            subStreams = _streamsList.FindAll(s => s.Media.Contains("video") || s.Media.Contains("composition"));
                            break;

                        case ConsoleKey.B:
                            category = "audio+video";
                            categoryInfo = "with [audio+video]";
                            selected = true;
                            ConsoleAbstraction.WriteDarkYellow($"Audio and Video selected ...");
                            subStreams = _streamsList.FindAll(s => s.Media == "audio+video");
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
                                ConsoleAbstraction.WriteDarkYellow($"Remove stream selected ...");
                            else
                            {
                                ConsoleAbstraction.WriteGreen($"No stream to remove");
                                return;
                            }
                            break;

                        case ConsoleKey.C:
                            ConsoleAbstraction.WriteDarkYellow($"Cancelled ...");
                            return;

                        default:
                            selected = false;
                            ConsoleAbstraction.WriteDarkYellow($"Bad key used ...");
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

                        ConsoleAbstraction.WriteYellow($"{Rainbow.Util.CR}Select the stream {categoryInfo}:");
                        for (index = startIndex; index < maxIndex; index++)
                        {
                            var stream = subStreams[index];
                            ConsoleAbstraction.WriteYellow($"\t[{index - (page * 10)}] {stream}");
                        }

                        if (withPreviousPage)
                            ConsoleAbstraction.WriteYellow($"\t[P] Previous page");
                        if (withNextPage)
                            ConsoleAbstraction.WriteYellow($"\t[N] Next page");
                        ConsoleAbstraction.WriteYellow($"\t[C] Cancel");

                        var consoleKey = ConsoleAbstraction.ReadKey();
                        switch (consoleKey?.Key)
                        {
                            case ConsoleKey.C:
                                ConsoleAbstraction.WriteDarkYellow($"Cancelled ....");
                            return;

                            case ConsoleKey.N:
                                selected = false;
                                if (withNextPage)
                                    page++;
                                else
                                    ConsoleAbstraction.WriteDarkYellow($"Bad key used ...");
                            break;

                            case ConsoleKey.P:
                                selected = false;
                                if (withPreviousPage)
                                    page--;
                                else
                                    ConsoleAbstraction.WriteDarkYellow($"Bad key used ...");
                            break;

                            default:
                                if (char.IsDigit( (consoleKey.Value.KeyChar) ))
                                {
                                    var selection = int.Parse(consoleKey.Value.KeyChar.ToString());
                                    if ((selection >= 0) && (selection < maxIndex))
                                    {
                                        var stream = Stream.FromStream(subStreams[selection + (page * 10)]);

                                        Task? task = null;
                                        switch(category)
                                        {
                                            case "to remove":
                                                if (stream.Id == _streamManager.currentAudioStream?.Id)
                                                    task = _streamManager.UseMainStreamAsync(Rainbow.Consts.Media.AUDIO, null);
                                                else
                                                    task = _streamManager.UseMainStreamAsync(Rainbow.Consts.Media.VIDEO, null);
                                                break;

                                            case "audio":
                                                task = _streamManager.UseMainStreamAsync(Rainbow.Consts.Media.AUDIO, stream);
                                                break;

                                            case "video":
                                                task = _streamManager.UseMainStreamAsync(Rainbow.Consts.Media.VIDEO, stream);
                                                break;

                                            case "audio+video":
                                                task = _streamManager.UseMainStreamAsync(Rainbow.Consts.Media.AUDIO + Rainbow.Consts.Media.VIDEO, stream);
                                                break;

                                            default:
                                                ConsoleAbstraction.WriteRed($"category not managed: {category}");
                                                break;

                                        }
                                        return;
                                    }
                                }
                                ConsoleAbstraction.WriteDarkYellow($"Bad key used ...");
                            break;
                        }
                    }
                }
                else
                    ConsoleAbstraction.WriteRed($"No streams available");
            }
            else
                ConsoleAbstraction.WriteRed($"{Rainbow.Util.CR}No streams available");
            */
        }

        static async Task PromptManageDeviceAsync(String type)
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
                ConsoleAbstraction.WriteBlue($"{Rainbow.Util.CR}List of [{type}] available on this computer:");
                int index = 1;
                foreach (var audioInput in devices)
                    ConsoleAbstraction.WriteBlue($"{index++} - {audioInput.Name}");
            }
            else
            {
                ConsoleAbstraction.WriteBlue($"{Rainbow.Util.CR}No [{type}] available on this computer");
                return;
            }

            // Display info about streams defined as device
            var streamAsDevice = _streamsList?.FindAll(s => s.UriType == type);
            if (streamAsDevice?.Count > 0)
            {
                ConsoleAbstraction.WriteBlue($"{Rainbow.Util.CR}List of streams defined as [{type}] correctly set in config file: (doesn't mean they can be really used)");
                int index = 1;
                foreach (var audioInput in streamAsDevice)
                    ConsoleAbstraction.WriteBlue($"{index++} - {audioInput}");
            }
            else
                ConsoleAbstraction.WriteBlue($"{Rainbow.Util.CR}No stream defined as [{type}] correctly set in config file");

            ConsoleAbstraction.WriteYellow($"{Rainbow.Util.CR}Do you want to add in config file all [{type}s] available on your system ? [Y]");
            ConsoleAbstraction.WriteRed($"Previous [{type}s] already set will be removed and the file [{_streamsFilePath}] will be totally rewritten.");
            var consoleKey = await ConsoleAbstraction.ReadKeyAsync();
            if (consoleKey?.Key == ConsoleKey.Y)
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

                ConsoleAbstraction.WriteYellow($"Update done about [{type}] - file updated [{_streamsFilePath}]");

                PromptStreamsInfo();

            }
            else
                ConsoleAbstraction.WriteYellow($"No updates about [{type}] done");
        }

        static async Task PromptManageWebcamAsync() => await PromptManageDeviceAsync("webcam");

        static async Task PromptManageScreenAsync() => await PromptManageDeviceAsync("screen");

        static async Task PromptManageAudioInputAsync() => await PromptManageDeviceAsync("microphone");

        static void PromptManageAudioOutput()
        {
            // Get list of audio output devices
            var audioOutputDevices = Rainbow.Medias.Devices.GetAudioOutputDevices();
            int index = 0;
            if (audioOutputDevices?.Count > 0)
            {
                ConsoleAbstraction.WriteYellow($"{Rainbow.Util.CR}Select an Audio output device:");
                foreach (var audioOutputDevice in audioOutputDevices)
                {
                    if(_audioOutputDevice?.Name == audioOutputDevice?.Name)
                        ConsoleAbstraction.WriteBlue($"\t[{index}] {audioOutputDevice?.Name} (CURRENT SELECTION)");
                    else
                        ConsoleAbstraction.WriteYellow($"\t[{index}] {audioOutputDevice?.Name}");
                    index++;
                }
                if (_audioOutputDevice is null)
                    ConsoleAbstraction.WriteBlue("\t[N] - No audio output (CURRENT SELECTION)");
                else
                    ConsoleAbstraction.WriteYellow("\t[N] - No audio output");
                ConsoleAbstraction.WriteYellow("\t[C] - Cancel");

                var audioOutputUpdated = false;
                var selected = false;

                var consoleKey = ConsoleAbstraction.ReadKey();
                if (consoleKey.HasValue && char.IsDigit(consoleKey.Value.KeyChar))
                {
                    var selection = int.Parse(consoleKey.Value.KeyChar.ToString());
                    if ((selection >= 0) && (selection < index))
                    {
                        selected = true;
                        audioOutputUpdated = _audioOutputDevice?.Path != audioOutputDevices[selection].Path;

                        if (audioOutputUpdated)
                        {
                            _audioOutputDevice = audioOutputDevices[selection];
                            ConsoleAbstraction.WriteDarkYellow($"Audio Output Device selected: {_audioOutputDevice.Name} ({_audioOutputDevice.Path})]");
                        }
                        else
                            ConsoleAbstraction.WriteDarkYellow($"Same Audio Output Device selected");
                    }
                    else
                        ConsoleAbstraction.WriteDarkYellow($"Invalid key used ...");
                }
                else if (consoleKey.HasValue &&  (consoleKey.Value.Key == ConsoleKey.N))
                {
                    audioOutputUpdated = true;
                    selected = true;

                    _audioOutputDevice = null;

                    ConsoleAbstraction.WriteDarkYellow($"No more Audio Output Device used ...");
                }
                else if (consoleKey.HasValue && consoleKey.Value.Key == ConsoleKey.C)
                {
                    selected = true;
                    ConsoleAbstraction.WriteDarkYellow($"Cancel used ...");
                }
                else
                    ConsoleAbstraction.WriteDarkYellow($"Invalid key used ...");


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
                                ConsoleAbstraction.WriteRed("Audio output device cannot be init / start");
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

            ConsoleAbstraction.WriteBlue($"{Rainbow.Util.CR}List of streams correctly set in config file: (doesn't mean they can be really used)");
            int index = 1;
            foreach (var stream in _streamsList)
            {
                ConsoleAbstraction.WriteBlue($"{index++:00} - {stream}");
                //index++;
            }
        }

        static void PromptTest()
        {
            ConsoleAbstraction.WriteYellow("=> PromptTest()");
        }

#endregion Prompts

        static void UpdateVideoWindowTitle()
        {
            ConsoleAbstraction.WriteGray("UpdateVideoWindowTitle");
            String title = $"SDK C# v3.x - Streaming Audio:[{((_streamManager._streamIdUsedForAudio is not null) ? _streamManager._streamIdUsedForAudio : "NONE")}] - Video:[{((_streamManager._streamIdUsedForVideo is not null) ? $"{_streamManager._streamIdUsedForVideo}" : "NONE")}]";
            Window.UpdateTitle(_outputVideoWindow, title);
        }

        static void UpdateSharingWindowTitle()
        {
            ConsoleAbstraction.WriteGray("UpdateSharingWindowTitle");
            String title = $"SDK C# v3.x - Streaming Audio:[{((_streamManager._streamIdUsedForAudio is not null) ? _streamManager._streamIdUsedForAudio : "NONE")}] - Sharing:[{((_streamManager._streamIdUsedForSharing is not null) ? $"{_streamManager._streamIdUsedForSharing}" : "NONE")}]";
            Window.UpdateTitle(_outputSharingWindow, title);
        }

    #region READ CONFIGURATION

        static Boolean ReadStreamsSettings()
        {
            _streamsFilePath = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}streams.json";
            if (!File.Exists(_streamsFilePath))
            {
                ConsoleAbstraction.WriteRed($"The file '{_streamsFilePath}' has not been found.");
                return false;
            }

            String jsonConfig = File.ReadAllText(_streamsFilePath);
            var jsonNode = JSON.Parse(jsonConfig);

            if ((jsonNode is null) || (!jsonNode.IsObject))
            {
                ConsoleAbstraction.WriteRed($"Cannot get JSON data from file '{_streamsFilePath}'.");
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
                        // Don't add composition nonw - we must ensure to have related video stream to use them
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
                    ConsoleAbstraction.WriteRed($"Cannot read 'streams' object (no Stream object created) - file:'{_streamsFilePath}'.");
                    return false;
                }
            }
            else
            {
                ConsoleAbstraction.WriteRed($"Cannot read 'streams' object OR invalid/missing data - file:'{_streamsFilePath}'.");
                return false;
            }

            _autoPlayAudio = jsonNode["autoPlayAudio"];
            _autoPlayVideo = jsonNode["autoPlayVideo"];
            _autoPlaySharing = jsonNode["autoPlaySharing"];
            return true;
        }

        static Boolean ReadExeSettings()
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
                ConsoleAbstraction.WriteRed($"Cannot read 'exeSettings' object OR invalid/missing data - file:'{exeSettingsFilePath}'.");
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
                consoleHandle = FindWindowByCaption(IntPtr.Zero, ConsoleAbstraction.Title);

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
