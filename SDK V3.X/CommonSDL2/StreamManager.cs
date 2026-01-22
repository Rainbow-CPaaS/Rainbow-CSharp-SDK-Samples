using FFmpeg.AutoGen;
using Rainbow.Example.Common;
using Rainbow.Medias;
using System.Drawing;
using Stream = Rainbow.Example.Common.Stream;

namespace Rainbow.Example.CommonSDL2
{
    public class StreamManager
    {
        private readonly SemaphoreSlim semaphoreUseOfStremSlim = new (1, 1);

        // --- To store list of available Streams
        public List<Stream>? streamsList;

        // --- To know which Streams are currently used / played
        public Stream? currentAudioStream;  // Used for audio   => events OnAudioSample / OnAudioStateChanged / OnAudioEndOfFile will be used
        public Stream? currentVideoStream;  // Used for video   => events OnVideoImage / OnVideoStateChanged / OnVideoEndOfFile will be used
        public Stream? currentSharingStream;// Used for sharing => events OnSharingImage / OnSharingStateChanged / OnSharingEndOfFile will be used
        public List<Stream> currentOtherStreams = [];  // Streams to stay opened even if not used for audio/video/sharing

        // --- IMedia used / created 
        private readonly List<IMediaAudio> _mediasForAudio = [];
        private readonly List<IMediaVideo> _mediasForVideo = [];

        // --- IMedia used / created for Input based on the selected Stream
        public IMediaAudio? mediaInputAudio;
        public IMediaVideo? mediaInputVideo;
        public IMediaVideo? mediaInputSharing;

        public event SampleDelegate? OnAudioSample;
        public event StateDelegate? OnAudioStateChanged;
        public event EndOfFileDelegate? OnAudioEndOfFile;

        public event ImageDelegate? OnVideoImage;
        public event StateDelegate? OnVideoStateChanged;
        public event EndOfFileDelegate? OnVideoEndOfFile;

        public event ImageDelegate? OnSharingImage;
        public event StateDelegate? OnSharingStateChanged;
        public event EndOfFileDelegate? OnSharingEndOfFile;

        /// <summary>
        /// To define which stream to use for main medias (audio, video, sharing) depending of "media" parameter
        /// 
        /// Same stream can be used for several main medias.
        /// 
        /// If **media** parameter is set Rainbow.Consts.Media.None, all main medias are stopped/closed.
        /// </summary>
        /// <param name="media"><see cref="int"/>media(s) to use - see <see cref="Rainbow.Consts.Media"/> for possible values</param>
        /// <param name="stream">Stream object to use for media(s) specified. Null can be specified to stoppped/closed specified medias</param>
        /// <returns><see cref="Task"/></returns>
        public async Task UseMainStreamAsync(int media, Stream? stream = null )
        {
            // Ensure to call this method only when it's previous called is already finished
            if (media == Rainbow.Consts.Media.NONE)
                await UseStreamsAsync(null, null, null, currentOtherStreams);
            else
            {
                Stream? streamAudio = Rainbow.Util.MediasWithAudio(media) ? stream : currentAudioStream;
                Stream? streamVideo = Rainbow.Util.MediasWithVideo(media) ? stream : currentVideoStream;
                Stream? streamSharing = Rainbow.Util.MediasWithSharing(media) ? stream : currentSharingStream;

                await UseStreamsAsync(streamAudio, streamVideo, streamSharing, currentOtherStreams);
            }

            
        }

        /// <summary>
        /// List of all Streams to keep opened even if not used for main medias (audio, video, sharing).
        /// 
        /// If you want to keep open a stream used as main media but which is no more needed as main media, you need to add it also in this list.
        /// </summary>
        /// <param name="otherStreams"><see cref="Stream"/>Stream as params. Set Empty to stopped / closed all previous streams specified if they are not used as main media(s)</param>
        /// <returns><see cref="Task"/></returns>
        public async Task UseOtherStreamsAsync(params Stream[] otherStreams)
        {
            await UseStreamsAsync(currentAudioStream, currentVideoStream, currentSharingStream, otherStreams.ToList());
        }

        public async Task UseStreamsAsync(Stream? streamForAudio, Stream? streamForVideo, Stream? streamForSharing, List<Stream> otherStreams)
        {
            await semaphoreUseOfStremSlim.WaitAsync();
            Common.Util.WriteRed("IN - UseStreamsAsync()");

            // Get list of media used
            List<IMediaAudio> audiosUsed = GetListOfAudiosMediaUsed();
            List<IMediaVideo> videosUsed = GetListOfVideosMediaUsed();

            // Clear storage
            _mediasForAudio.Clear();
            _mediasForVideo.Clear();

            // Manage Main Media
            await UseMainMediaStreamAsync(streamForAudio, Rainbow.Consts.Media.AUDIO, audiosUsed, videosUsed);
            await UseMainMediaStreamAsync(streamForVideo, Rainbow.Consts.Media.VIDEO, audiosUsed, videosUsed);
            await UseMainMediaStreamAsync(streamForSharing, Rainbow.Consts.Media.SHARING, audiosUsed, videosUsed);

            currentAudioStream = (mediaInputAudio is null) ? null : streamForAudio;
            currentVideoStream = (mediaInputVideo is null) ? null : streamForVideo; 
            currentSharingStream = (mediaInputSharing is null) ? null : streamForSharing;

            // Manage Other Streams
            currentOtherStreams.Clear();
            foreach(var otherStream in otherStreams)
            {
                if(await UseMediaStreamAsync(otherStream, audiosUsed, videosUsed))
                    currentOtherStreams.Add(otherStream);
            }

            await Task.Delay(200); // Add a small delay to let some times to close windows according streams in progress
            await CloseMediasNotUsedAsync(audiosUsed, videosUsed);

            Common.Util.WriteRed("OUT - UseStreamsAsync()");
            // Unlock the semaphore
            try
            {
                if (semaphoreUseOfStremSlim.CurrentCount == 0)
                    semaphoreUseOfStremSlim.Release();
            }
            catch { }
        }

        private async Task<Boolean> UseMainMediaStreamAsync(Stream? stream, int media, List<IMediaAudio> audiosUsed, List<IMediaVideo> videosUsed)
        {
            IMedia? mediaInput;
            switch (media)
            {
                case Rainbow.Consts.Media.AUDIO:
                    mediaInput = mediaInputAudio;
                    break;
                case Rainbow.Consts.Media.VIDEO:
                    mediaInput = mediaInputVideo;
                    break;
                case Rainbow.Consts.Media.SHARING:
                    mediaInput = mediaInputSharing;
                    break;
                default:
                    Common.Util.WriteRed($"Invalid media specified Media:[{media}]");
                    return false;
            }

            if (stream is null)
            {
                // Close previous Main Video MediaInput
                await StopMainMediaInputAsync(media);
                return true;
            }

            String str = Rainbow.Util.MediasToString(media);
            Boolean needAudio = media == Rainbow.Consts.Media.AUDIO;

            // Do we use the same video input ?
            if (mediaInput?.Id == stream.Id)
            {
                StoreAudioMedia(_mediasForAudio, mediaInput as IMediaAudio);
                StoreVideoMedia(_mediasForVideo, mediaInput as IMediaVideo);

                if (stream.VideoComposition is not null)
                {
                    foreach (var id in stream.VideoComposition)
                    {
                        var subVideo = videosUsed.FirstOrDefault(m => m.Id == id);
                        StoreVideoMedia(_mediasForVideo, subVideo);
                        var subAudio = audiosUsed.FirstOrDefault(m => m.Id == id);
                        StoreAudioMedia(_mediasForAudio, subAudio);
                    }
                }
                return true;
            }

            // Close previous Main MediaInput
            await StopMainMediaInputAsync(media);

            IMediaAudio? iMediaAudio = null;
            IMediaVideo? iMediaVideo = null;
            List<IMediaAudio> subAudios = [];
            List<IMediaVideo> subVideos = [];

            // Try to get video input from videos list
            if(needAudio)
                iMediaAudio = audiosUsed.FirstOrDefault(m => m.Id == stream.Id);
            else
                iMediaVideo = videosUsed.FirstOrDefault(m => m.Id == stream.Id);

            if ( (needAudio && iMediaAudio is null) || ( (!needAudio) && iMediaVideo is null) )
                (iMediaAudio, iMediaVideo, subAudios, subVideos) = await GetMediaInputs(stream, audiosUsed, videosUsed);
            else
            {
                if (needAudio)
                {
                    if (iMediaAudio is IMediaVideo v)
                        iMediaVideo = v;
                    else
                        iMediaVideo = null;
                }
                else
                {
                    if (iMediaVideo is IMediaAudio a)
                        iMediaAudio = a;
                    else
                        iMediaAudio = null;
                }
            }

            if (needAudio)
                mediaInput = iMediaAudio;
            else
                mediaInput = iMediaVideo;

            if (mediaInput is not null)
            {
                if (mediaInput.IsStarted)
                    Common.Util.WriteDarkYellow($"Re-Use {str} MediaInput with [{mediaInput.Id}] ...");
                else
                    Common.Util.WriteDarkYellow($"Trying to Init/start {str} MediaInput with [{mediaInput.Id}] ...");
                if (mediaInput.IsStarted || mediaInput.Init(true))
                {
                    // Store audio/video media
                    StoreAudioMedia(_mediasForAudio, iMediaAudio);
                    StoreAudioMedia(_mediasForAudio, subAudios.ToArray());
                    StoreVideoMedia(_mediasForVideo, iMediaVideo);
                    StoreVideoMedia(_mediasForVideo, subVideos.ToArray());

                    StartMainMediaInputAsync(media, mediaInput);

                    Common.Util.WriteDarkYellow($"MediaInput {str} initialized / started [{mediaInput.Id}]");
                    return true;
                }
            }
            else
            {
                Common.Util.WriteDarkYellow($"No MediaInput for Media:[{Util.MediasToString(media)}]");
            }
            return false;
        }

        private async Task<Boolean> UseMediaStreamAsync(Stream? stream, List<IMediaAudio> audiosUsed, List<IMediaVideo> videosUsed)
        {
            if (stream is null)
                return true; ;

            IMediaAudio? iMediaAudio = null;
            IMediaVideo? iMediaVideo = null;
            List<IMediaAudio> subAudios = [];
            List<IMediaVideo> subVideos = [];

            iMediaAudio = audiosUsed.FirstOrDefault(m => m.Id == stream.Id);
            iMediaVideo = videosUsed.FirstOrDefault(m => m.Id == stream.Id);

            if( (iMediaAudio?.IsStarted == true) || (iMediaVideo?.IsStarted == true) )
            {
                StoreAudioMedia(_mediasForAudio, iMediaAudio);
                StoreVideoMedia(_mediasForVideo, iMediaVideo);

                if (stream.VideoComposition is not null)
                {
                    foreach (var id in stream.VideoComposition)
                    {
                        iMediaAudio = audiosUsed.FirstOrDefault(m => m.Id == id);
                        StoreAudioMedia(_mediasForAudio, iMediaAudio);
                        iMediaVideo = videosUsed.FirstOrDefault(m => m.Id == id);
                        StoreVideoMedia(_mediasForVideo, iMediaVideo);
                    }
                }
                return true;
            }

            (iMediaAudio, iMediaVideo, subAudios, subVideos) = await GetMediaInputs(stream, audiosUsed, videosUsed);

            IMedia? iMedia = iMediaAudio is null ? iMediaVideo : iMediaAudio;
            if (iMedia is not null)
            {
                if (iMedia.IsStarted)
                    Common.Util.WriteDarkYellow($"Re-Use MediaInput with [{iMedia.Id}] ...");
                else
                    Common.Util.WriteDarkYellow($"Trying to Init/startMediaInput with [{iMedia.Id}] ...");
                if (iMedia.IsStarted || iMedia.Init(true))
                {
                    // Store audio/video media
                    StoreAudioMedia(_mediasForAudio, iMediaAudio);
                    StoreAudioMedia(_mediasForAudio, subAudios.ToArray());
                    StoreVideoMedia(_mediasForVideo, iMediaVideo);
                    StoreVideoMedia(_mediasForVideo, subVideos.ToArray());


                    Common.Util.WriteDarkYellow($"MediaInput initialized / started [{iMedia.Id}]");
                    return true;
                }
            }
            return false;
        }

        private async Task<(IMediaVideo? mediaVideo, List<IMediaAudio> subMediaAudioList, List<IMediaVideo> subMediaVideoList)> GetMediaInputForComposition(Stream stream, List<IMediaAudio> audiosUsed, List<IMediaVideo> videosUsed)
        {
            Boolean success = true;
            List<Size> videoSize = [];

            List<IMediaAudio> mediaAudioList = [];
            List<IMediaVideo> mediaVideoList = [];

            if ((streamsList is null) || (stream is null) || (stream.VideoComposition is null))
                return (null, mediaAudioList, mediaVideoList);

            Common.Util.WriteGreen($"Trying to create composition for Stream [{stream.Id}] ...");
            foreach (var id in stream.VideoComposition)
            {
                var s = streamsList.FirstOrDefault(s => s.Id == id);
                if (s is not null)
                {
                    var mi = videosUsed.FirstOrDefault(m => m.Id == id);
                    if (mi is null)
                    {
                        //Common.Util.WriteGreen($"For composition, creating MediaInput for Stream:[{id}] ...");
                        (var iMediaAudio, var iMediaVideo, var subAudios, var subVideos) = await GetMediaInputs(s, audiosUsed, videosUsed);

                        if (iMediaVideo is not null)
                        {
                            if (iMediaVideo.IsStarted || iMediaVideo.Init(true) == true)
                            {
                                // Store audio/video media
                                StoreAudioMedia(mediaAudioList, iMediaAudio);
                                StoreAudioMedia(mediaAudioList, subAudios.ToArray());
                                StoreVideoMedia(mediaVideoList, iMediaVideo);
                                StoreVideoMedia(mediaVideoList, subVideos.ToArray());

                                var size = new Size(iMediaVideo.Width, iMediaVideo.Height);
                                videoSize.Add(size);
                            }
                            else
                            {
                                // Cannot start 
                                Common.Util.WriteRed($"For composition, MediaInput cannot be started for Stream:[{id}]");
                                success = false;
                                break;
                            }
                        }
                        else
                        {
                            // Cannot start 
                            Common.Util.WriteRed($"For composition, MediaInput cannot be created for Stream:[{id}]");
                            success = false;
                            break;
                        }
                    }
                    else
                    {
                        mediaVideoList.Add(mi);

                        var size = new Size(mi.Width, mi.Height);
                        videoSize.Add(size);

                        Common.Util.WriteDarkYellow($"For composition, re-use MediaInput with Stream:[{id}] ...");
                    }
                }
                else
                {
                    Common.Util.WriteRed($"For composition, no Stream:[{id}] defined ...");
                    success = false;
                    break;
                }
            }

            if (success)
            {
                String? filter = null;
                var mediaFiltered = new MediaFiltered(stream.Id, mediaVideoList, null);

                // Check if we have a filter already defined or we need to create it using a "template" ?
                if (stream.VideoFilterJsonNode is not null)
                {
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
                        Common.Util.WriteRed($"For composition, using \"template\" cannot create video filter ...");
                    else
                        Common.Util.WriteDarkYellow($"For composition, using \"template\" filter created:\r\n{filter}");
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
                            return (mediaFiltered, mediaAudioList, mediaVideoList);
                        }
                        else
                            Common.Util.WriteRed($"For composition, cannot init MediaFiltered ...");
                    }
                    else
                        Common.Util.WriteRed($"For composition, video filter defined is incorrect:\r\n{stream.VideoFilter}");
                }
            }
            else
            {
                // TODO - close medias opened for the composition
            }

            return (null, mediaAudioList, mediaVideoList);
        }

        private async Task<(IMediaAudio? audioInput, IMediaVideo? videoInput, List<IMediaAudio> subMediaAudioList, List<IMediaVideo> subMediaVideoList)> GetMediaInputs(Stream? stream, List<IMediaAudio> audiosUsed, List<IMediaVideo> videosUsed)
        {
            IMediaAudio? audioInput = null;
            IMediaVideo? videoInput = null;
            List<IMediaAudio> subMediaAudioList = [];
            List<IMediaVideo> subMediaVideoList = [];
            if ((streamsList is null) || (stream is null)) return (audioInput, videoInput, subMediaAudioList, subMediaVideoList);

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
                            Common.Util.WriteRed($"Cannot create and/or start MediaInput using this webcamDevice:[{webcamDevice}]");
                    }
                    else
                        Common.Util.WriteRed($"Cannot get a webcamDevice with uri:[{stream.Uri}]");
                    break;

                case "screen":
                    var screenDevice = GetScreenDevice(stream.Uri);
                    if (screenDevice is not null)
                    {
                        videoInput = MediaInput.FromScreenDevice(screenDevice, false);
                        if (videoInput is null)
                            Common.Util.WriteRed($"Cannot create and/or start MediaInput using this screenDevice:[{screenDevice}]");
                    }
                    else
                        Common.Util.WriteRed($"Cannot get a screenDevice with uri:[{stream.Uri}]");
                    break;

                case "microphone":
                    var microphoneDevice = GetMicrophoneDevice(stream.Uri);
                    if (microphoneDevice is not null)
                    {
                        audioInput = new SDL2AudioInput(microphoneDevice);
                        if (audioInput is null)
                            Common.Util.WriteRed($"Cannot create and/or start MediaInput using this screenDevice:[{microphoneDevice}]");
                    }
                    else
                        Common.Util.WriteRed($"Cannot get a screenDevice with uri:[{stream.Uri}]");
                    break;

                default:
                    if (withComposition)
                    {
                        (videoInput, subMediaAudioList, subMediaVideoList) = await GetMediaInputForComposition(stream, audiosUsed, videosUsed);
                    }
                    else
                    {
                        // Check if audio can be re-used
                        audioInput = audiosUsed.FirstOrDefault(m => m.Id == stream.Id);

                        // Check if video can be re-used
                        videoInput = videosUsed.FirstOrDefault(m => m.Id == stream.Id);

                        if ((withVideo && videoInput is not null)
                            || (withAudio && audioInput is not null))
                        {
                            if (audioInput is null && videoInput is not null)
                                audioInput = (IMediaAudio)videoInput;

                            Common.Util.WriteDarkYellow($"Not necessary to create new MediaInput {(withVideo ? $"Video [{stream.Id}]" : $"Audio [{stream.Id}]")} - use previous stream");
                            return (audioInput, videoInput, subMediaAudioList, subMediaVideoList);
                        }

                        Common.Util.WriteGreen($"Creating InputStreamDevice: {stream} ...");
                        var inputStreamDevice = new InputStreamDevice(stream.Id, stream.Id, stream.Uri, withVideo: withVideo, withAudio: withAudio, loop: true, options: options);

                        Common.Util.WriteGreen($"Creating MediaInput for [{stream.Id}] ...");
                        var mediaInput = new MediaInput(inputStreamDevice, forceLivestream: stream.ForceLiveStream);
                        if (withVideo)
                            videoInput = mediaInput;
                        if (withAudio)
                            audioInput = mediaInput;
                    }
                    break;
            }

            return (audioInput, videoInput, subMediaAudioList, subMediaVideoList);
        }

        static private void StoreAudioMedia(List<IMediaAudio> storageAudio, params IMediaAudio?[] mediaAudios)
        {
            if (mediaAudios?.Length > 0)
            {
                foreach (var mediaAudio in mediaAudios)
                {
                    if ((mediaAudio is null) || (!mediaAudio.IsStarted))
                        continue;

                    var m = storageAudio.FirstOrDefault(m => m.Id == mediaAudio.Id);
                    if (m is null)
                    {
                        storageAudio.Add(mediaAudio);
                        //Common.Util.WriteGray($"Store Audio [{mediaAudio.Id}]");
                    }
                }
            }
        }

        static private void StoreVideoMedia(List<IMediaVideo> storageVideos, params IMediaVideo?[] mediaVideos)
        {
            if (mediaVideos?.Length > 0)
            {
                foreach (var mediaVideo in mediaVideos)
                {
                    if ((mediaVideo is null) || (!mediaVideo.IsStarted))
                        continue;

                    var m = storageVideos.FirstOrDefault(m => m.Id == mediaVideo.Id);
                    if (m is null)
                    {
                        storageVideos.Add(mediaVideo);
                        //Common.Util.WriteGray($"Store Video [{mediaVideo.Id}]");
                    }
                }
            }
        }

        public List<IMediaVideo> GetListOfVideosMediaUsed()
        {
            List<IMediaVideo> result = [];
            StoreVideoMedia(result, mediaInputVideo);
            StoreVideoMedia(result, mediaInputSharing);
            foreach (var video in _mediasForVideo)
                StoreVideoMedia(result, video);

            return result;
        }

        public List<IMediaAudio> GetListOfAudiosMediaUsed()
        {
            List<IMediaAudio> result = [];
            StoreAudioMedia(result, mediaInputAudio);
            foreach (var audio in _mediasForAudio)
                StoreAudioMedia(result, audio);

            return result;
        }

        private void StartMainMediaInputAsync(int media, IMedia iMedia)
        {
            switch (media)
            {
                case Rainbow.Consts.Media.AUDIO:
                    mediaInputAudio = iMedia as IMediaAudio;
                    if (mediaInputAudio is null) return;

                    OnAudioStateChanged?.Invoke(iMedia.Id, true, false);

                    mediaInputAudio.OnAudioSample += MediaInput_OnAudioSample;
                    mediaInputAudio.OnEndOfFile += MediaInput_OnAudioEndOfFile;
                    break;

                case Rainbow.Consts.Media.VIDEO:
                    mediaInputVideo = iMedia as IMediaVideo;
                    if (mediaInputVideo is null) return;

                    OnVideoStateChanged?.Invoke(iMedia.Id, true, false);

                    mediaInputVideo.OnImage += MediaInput_OnVideoImage;
                    mediaInputVideo.OnEndOfFile += MediaInput_OnVideoEndOfFile;
                    break;

                case Rainbow.Consts.Media.SHARING:
                    mediaInputSharing = iMedia as IMediaVideo;
                    if (mediaInputSharing is null) return;

                    OnSharingStateChanged?.Invoke(iMedia.Id, true, false);

                    mediaInputSharing.OnImage += MediaInput_OnSharingImage;
                    mediaInputSharing.OnEndOfFile += MediaInput_OnSharingEndOfFile;
                    break;
            }
        }

        private async Task StopMainMediaInputAsync(int media)
        {
            switch(media)
            {
                case Rainbow.Consts.Media.AUDIO:
                    // Close Audio MediaInputs
                    if (mediaInputAudio is not null)
                    {
                        mediaInputAudio.OnAudioSample -= MediaInput_OnAudioSample;
                        mediaInputAudio.OnEndOfFile -= MediaInput_OnAudioEndOfFile;

                        OnAudioStateChanged?.Invoke(mediaInputAudio.Id, false, false);

                        mediaInputAudio = null;
                    }
                    else
                        OnAudioStateChanged?.Invoke("", false, false);
                    break;

                case Rainbow.Consts.Media.VIDEO:
                    // Close Video MediaInputs
                    if (mediaInputVideo is not null)
                    {
                        mediaInputVideo.OnImage -= MediaInput_OnVideoImage;
                        mediaInputVideo.OnEndOfFile -= MediaInput_OnVideoEndOfFile;

                        OnVideoStateChanged?.Invoke(mediaInputVideo.Id, false, false);

                        mediaInputVideo = null;
                    }
                    else
                        OnVideoStateChanged?.Invoke("", false, false);
                    break;

                case Rainbow.Consts.Media.SHARING:
                    // Close Sharing MediaInputs
                    if (mediaInputSharing is not null)
                    {
                        mediaInputSharing.OnImage -= MediaInput_OnSharingImage;
                        mediaInputSharing.OnEndOfFile -= MediaInput_OnSharingEndOfFile;

                        OnSharingStateChanged?.Invoke(mediaInputSharing.Id, false, false);

                        mediaInputSharing = null;
                    }
                    else
                        OnSharingStateChanged?.Invoke("", false, false);
                    break;
            }
        }

        private async Task CloseMediasNotUsedAsync(List<IMediaAudio> audioPreviouslyUsed, List<IMediaVideo> videoOrSharingPreviouslyUsed)
        {
            // _mediaForAudio and _mediaVideoOrSharing contains list of medias currently used.

            foreach(var audio in audioPreviouslyUsed)
            {
                var resultAudio = _mediasForAudio.FirstOrDefault(m => m.Id == audio.Id);
                var resultVideo = _mediasForVideo.FirstOrDefault(m => m.Id == audio.Id);
                var resultPreviouslyUsed = videoOrSharingPreviouslyUsed.FirstOrDefault(m => m.Id == audio.Id);
                if ( (resultAudio is null) && (resultVideo is null))
                {
                    if(resultPreviouslyUsed is not null)
                        videoOrSharingPreviouslyUsed.Remove(resultPreviouslyUsed);

                    Common.Util.WriteDarkYellow($"Dispose previous Audio MediaInput Id:{audio.Id} (no more used)");
                    audio.Dispose();
                }
            }

            foreach (var video in videoOrSharingPreviouslyUsed)
            {
                var resultAudio = _mediasForAudio.FirstOrDefault(m => m.Id == video.Id);
                var resultVideo = _mediasForVideo.FirstOrDefault(m => m.Id == video.Id);
                if ((resultAudio is null) && (resultVideo is null))
                {
                    Common.Util.WriteDarkYellow($"Dispose previous Video MediaInput Id:{video.Id} (no more used)");
                    video.Dispose();
                }
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

        private void MediaInput_OnAudioEndOfFile(string mediaId)
        {
            OnAudioEndOfFile?.Invoke(mediaId);
        }

        private void MediaInput_OnVideoImage(string mediaId, int width, int height, int stride, IntPtr data, AVPixelFormat pixelFormat)
        {
            OnVideoImage?.Invoke(mediaId, width, height, stride, data, pixelFormat);
        }

        private void MediaInput_OnVideoEndOfFile(string mediaId)
        {
            OnVideoEndOfFile?.Invoke(mediaId);
        }

        private void MediaInput_OnSharingImage(string mediaId, int width, int height, int stride, IntPtr data, AVPixelFormat pixelFormat)
        {
            OnSharingImage?.Invoke(mediaId, width, height, stride, data, pixelFormat);
        }

        private void MediaInput_OnSharingEndOfFile(string mediaId)
        {
            OnSharingEndOfFile?.Invoke(mediaId);
        }

#endregion MediaInput Events
    }
}
