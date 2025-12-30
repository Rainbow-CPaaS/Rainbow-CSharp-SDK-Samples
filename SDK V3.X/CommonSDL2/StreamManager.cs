using FFmpeg.AutoGen;
using Rainbow.Example.Common;
using Rainbow.Medias;
using System.Drawing;
using Stream = Rainbow.Example.Common.Stream;
using Util = Rainbow.Example.Common.Util;

namespace CommonSDL2
{
    public class StreamManager
    {
        public List<Stream>? streamsList;
        
        // --- IMedia used / created to have composition
        private List<IMediaVideo> _mediaInputVideoListForComposition = [];

        // --- To know which Stream is currently used / played
        public Stream? currentAudioStream; // Audio Stream currently used / played
        public Stream? currentVideoStream; // Video Stream currently used / played

        // --- IMedia used / created for Input based on the selected Stream
        public IMediaAudio? mediaInputAudio;
        public IMediaVideo? mediaInputVideo;

        public event SampleDelegate? OnAudioSample;
        public event ImageDelegate? OnImage;

        public event StateDelegate? OnAudioStateChanged;
        public event StateDelegate? OnVideoStateChanged;

        public event EndOfFileDelegate? OnAudioEndOfFile;
        public event EndOfFileDelegate? OnVideoEndOfFile;

        public void SetStreamsList(List<Stream>? streamsList)
        {
            this.streamsList = streamsList;
        }

        public async Task UseAudioStreamAsync(Stream? streamForAudio)
        {
            // --- Now Manage Audio ---
            if ((streamForAudio?.IsSame(currentAudioStream) == true)
                && (mediaInputAudio?.IsStarted == true)) // Ensure to have audio media input really started
                return;

            // Close previous Audio MediaInput
            await CloseAudioInputsAsync();

            if (streamForAudio is null)
                return;

            IMediaAudio? iMediaAudio;

            // Check Video Media Input
            if (mediaInputVideo?.Id == streamForAudio.Id)
            {
                iMediaAudio = (IMediaAudio)mediaInputVideo;
            }
            else
            {
                // Check video used in the composition
                var iMediaVideo = _mediaInputVideoListForComposition.FirstOrDefault(m => m.Id == streamForAudio.Id);
                if (iMediaVideo is not null)
                    iMediaAudio = (IMediaAudio)iMediaVideo;
                else
                    (iMediaAudio, _) = await GetMediaInputs(streamForAudio);
            }

            if (iMediaAudio is not null)
            {
                Util.WriteGreen($"Init/Start Audio MediaInput with [{iMediaAudio.Id}] ...");
                if (iMediaAudio.IsStarted || iMediaAudio.Init(true))
                {
                    currentAudioStream = streamForAudio;
                    mediaInputAudio = iMediaAudio;

                    OnAudioStateChanged?.Invoke(mediaInputAudio.Id, true, false);

                    mediaInputAudio.OnAudioSample += MediaInput_OnAudioSample;
                    mediaInputAudio.OnEndOfFile += MediaInputAudio_OnEndOfFile;
                }
            }
        }

        // Play streams specified. Previous streams are closed if needed.
        public async Task UseStreamsAsync(Stream? streamForVideo, Stream? streamForAudio = null)
        {
            Boolean withComposition = false;
            IMediaAudio? iMediaAudio;
            IMediaVideo? iMediaVideo;

            // --- Manage Video in priority ---
            if (streamForVideo is null)
            {
                await CloseVideoInputsAsync();
                await CloseMediaInputsNotUsedInCompostionAsync();

                await UseAudioStreamAsync(streamForAudio);
            }
            else
            {
                // Check if the same stream / config is used or not for video
                if (streamForVideo.IsSame(currentVideoStream))
                {
                    await UseAudioStreamAsync(streamForAudio);
                }
                else
                {
                    withComposition = streamForVideo.Media.Contains("composition");
                    if (!withComposition)
                    {
                        // Close media inputs previously used in composition
                        await CloseMediaInputsNotUsedInCompostionAsync();
                    }

                    (iMediaAudio, iMediaVideo) = await GetMediaInputs(streamForVideo);

                    if (iMediaVideo is not null)
                    {
                        if (iMediaVideo.Id != mediaInputVideo?.Id || withComposition)
                            await CloseVideoInputsAsync();

                        Util.WriteGreen($"Init/Start Video MediaInput with [{iMediaVideo.Id}] ...");
                        if (iMediaVideo.IsStarted || iMediaVideo.Init(true))
                        {
                            currentVideoStream = streamForVideo;

                            mediaInputVideo = iMediaVideo;
                            mediaInputVideo.OnImage += MediaInput_OnImage;
                            mediaInputVideo.OnEndOfFile += MediaInputVideo_OnEndOfFile;

                            OnVideoStateChanged?.Invoke(iMediaVideo.Id, true, false);

                            Util.WriteDarkYellow($"MediaInput initialized / started");
                        }
                        else
                        {
                            Util.WriteRed($"MediaInput cannot be initialized / started !");
                        }

                    }
                    else
                    {
                        await CloseVideoInputsAsync();
                    }

                    

                    await UseAudioStreamAsync(streamForAudio);
                }
            }
        }

        public async Task<IMediaVideo?> GetMediaInputComposition(Stream stream)
        {
            Boolean success = true;
            List<Size> videoSize = [];

            if ((streamsList is null) || (stream is null) || (stream.VideoComposition is null))
                return null;

            Util.WriteGreen($"Trying to create composition ...");
            foreach (var id in stream.VideoComposition)
            {

                var s = streamsList.FirstOrDefault(s => s.Id == id);
                if (s is not null)
                {
                    // Check if this media Input is already in used in the composition
                    var mi = _mediaInputVideoListForComposition.FirstOrDefault(m => m.Id == s.Id);
                    if (mi is null)
                    {
                        Util.WriteGreen($"For composition, creating MediaInput for Stream:[{id}] ...");
                        (var _, var iMediaVideo) = await GetMediaInputs(s);
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
                        var size = new Size(mi.Width, mi.Height);
                        videoSize.Add(size);

                        Util.WriteDarkYellow($"For composition, re-use MediaInput with Stream:[{id}] ...");
                    }
                }
                else
                {
                    Util.WriteRed($"For composition, no Stream:[{id}] defined ...");
                    success = false;
                    break;
                }
            }

            if (success)
            {
                // Close media inputs which are not used in current composition
                await CloseMediaInputsNotUsedInCompostionAsync(stream.VideoComposition.ToArray());

                String? filter = null;
                var mediaFiltered = new MediaFiltered("mediaFiltered", _mediaInputVideoListForComposition, null);

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
                                size ??= videoSize[0];

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
                                size ??= videoSize[0];

                                // Get fps
                                fps = node["fps"];
                                if (fps == 0)
                                    fps = 10;

                                // Get overlay size
                                var sizeOverlay = VideoFilter.SizeFromString(node["sizeOverlay"]);
                                sizeOverlay ??= new Size((int)(videoSize[0].Width / 4), (int)(videoSize[0].Height / 4));

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
                    if (mediaFiltered.SetVideoFilter(stream.VideoComposition, filter))
                    {
                        if (mediaFiltered.Init(true))
                        {
                            // Store filter used
                            stream.VideoFilterInConference = filter;
                            return mediaFiltered;
                        }
                        else
                            Util.WriteRed($"For composition, cannot init MediaFiltered ...");
                    }
                    else
                        Util.WriteRed($"For composition, video filter defined is incorrect:\r\n{stream.VideoFilter}");
                }
            }

            Util.WriteGreen($"Closing all stream used in composition ...");
            await CloseMediaInputsNotUsedInCompostionAsync();
            return null;

        }

        // Central point to create MediaInput (Audio, Video, Composition)
        private async Task<(IMediaAudio? audioInput, IMediaVideo? videoInput)> GetMediaInputs(Stream? stream)
        {
            IMediaAudio? audioInput = null;
            IMediaVideo? videoInput = null;
            if ((streamsList is null) || (stream is null)) return (audioInput, videoInput);

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
                        if (videoInput is null)
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
                    if (withComposition)
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

        private async Task CloseAudioInputsAsync()
        {
            currentAudioStream = null;

            // Close Audio MediaInputs
            if (mediaInputAudio is not null)
            {
                mediaInputAudio.OnAudioSample -= MediaInput_OnAudioSample;
                mediaInputAudio.OnEndOfFile -= MediaInputAudio_OnEndOfFile;

                OnAudioStateChanged?.Invoke(mediaInputAudio.Id, false, false);

                // Check if media input Audio and Video are related
                var iMediaVideo = _mediaInputVideoListForComposition.FirstOrDefault(m => m.Id == mediaInputAudio.Id);
                if ((iMediaVideo is not null)
                    || ((mediaInputVideo is not null) && (mediaInputVideo.Id == mediaInputAudio.Id)))
                {
                    mediaInputAudio = null;
                    Util.WriteDarkYellow("Previous Audio MediaInput will be stoppped and disposed in same time than the previous Video MediaInput");
                }
                else
                {
                    Util.WriteGreen($"Stopping previous Audio MediaInput Id:{mediaInputAudio.Id} ...");
                    await mediaInputAudio.StopAsync();
                    mediaInputAudio.Dispose();
                    mediaInputAudio = null;
                    Util.WriteDarkYellow("Previous Audio MediaInput stoppped and disposed");
                }
            }
        }

        private async Task CloseVideoInputsAsync()
        {
            currentVideoStream = null;
            // Close Video MediaInputs
            if (mediaInputVideo is not null)
            {
                mediaInputVideo.OnImage -= MediaInput_OnImage;
                mediaInputVideo.OnEndOfFile -= MediaInputVideo_OnEndOfFile;

                OnVideoStateChanged?.Invoke(mediaInputVideo.Id, false, false);

                Util.WriteGreen($"Stopping previous Video MediaInput Id:{mediaInputVideo.Id} ...");
                mediaInputVideo.Dispose();
                mediaInputVideo = null;
                Util.WriteDarkYellow("Previous Video MediaInput stoppped and disposed");

                await Task.Delay(200);
            }
            else
                OnVideoStateChanged?.Invoke("", false, false);
        }

        // Close Media Input which are not used in the composition.
        private async Task CloseMediaInputsNotUsedInCompostionAsync(params String[] idsInComposition)
        {
            // Close MediaInput used in Composition
            if (_mediaInputVideoListForComposition.Count > 0)
            {
                List<IMediaVideo> mediaVideos = [];

                //Util.WriteGreen("Stopping previous Video MediaInput no more used...");
                foreach (var mediaInput in _mediaInputVideoListForComposition)
                {
                    if (mediaInput is not null)
                    {
                        if (!idsInComposition.Contains(mediaInput.Id))
                        {
                            Util.WriteDarkYellow($"Stop / Dispose previous Video MediaInput Id:{mediaInput.Id} (no more used in composition)");
                            mediaInput.Dispose();
                        }
                        else
                        {
                            // Store MediaInput not closed
                            mediaVideos.Add(mediaInput);
                        }
                    }
                }

                _mediaInputVideoListForComposition = mediaVideos;
                //Util.WriteDarkYellow("Previous Video MediaInput used for Composition  no more used are stoppped and disposed");
            }
        }

#region Device 

        public static Device? GetDevice(string name, string type)
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

        public static ScreenDevice? GetScreenDevice(string name)
        {
            var device = GetDevice(name, "screen");
            return (ScreenDevice?)device;
        }

        public static WebcamDevice? GetWebcamDevice(string name)
        {
            var device = GetDevice(name, "webcam");
            return (WebcamDevice?)device;

        }

        public static Device? GetMicrophoneDevice(string name)
        {
            var device = GetDevice(name, "microphone");
            return device;
        }

#endregion Device 

#region MediaInput Events

        private void MediaInput_OnAudioSample(string mediaId, uint duration, byte[] sample)
        {
            OnAudioSample?.Invoke(mediaId, duration, sample);
        }

        private void MediaInput_OnImage(string mediaId, int width, int height, int stride, IntPtr data, AVPixelFormat pixelFormat)
        {
            OnImage?.Invoke(mediaId, width, height, stride, data, pixelFormat);
        }

        private void MediaInputAudio_OnEndOfFile(string mediaId)
        {
            OnAudioEndOfFile?.Invoke(mediaId);
        }

        private void MediaInputVideo_OnEndOfFile(string mediaId)
        {
            OnVideoEndOfFile?.Invoke(mediaId);
        }

#endregion MediaInput Events
    }
}
