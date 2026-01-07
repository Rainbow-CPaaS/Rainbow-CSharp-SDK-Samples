using FFmpeg.AutoGen;
using Microsoft.Extensions.Logging.Abstractions;
using Rainbow.Example.Common;
using Rainbow.Medias;
using System.Collections.Generic;
using System.Drawing;
using static DirectShowLib.MediaSubType;
using Stream = Rainbow.Example.Common.Stream;
using Util = Rainbow.Example.Common.Util;

namespace CommonSDL2
{
    public class StreamManager
    {
        private SemaphoreSlim semaphoreUseOfStremSlim = new SemaphoreSlim(1, 1);

        // --- To store list of available Streams
        public List<Stream>? streamsList;

        // --- To know which Streams are currently used / played
        public Stream? currentAudioStream;  // Used for audio   => events OnAudioSample / OnAudioStateChanged / OnAudioEndOfFile will be used
        public Stream? currentVideoStream;  // Used for video   => events OnVideoImage / OnVideoStateChanged / OnVideoEndOfFile will be used
        public Stream? currentSharingStream;// Used for sharing => events OnSharingImage / OnSharingStateChanged / OnSharingEndOfFile will be used
        public Stream[] otherStreams = [];  // Streams to stay opened even if not used for audio/video/sharing

        // --- IMedia used / created 
        private List<IMediaAudio> _mediasForAudio = [];
        private List<IMediaVideo> _mediasForVideo = [];

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

        private void StoreAudioMedia(List<IMediaAudio> storageAudio, params IMediaAudio?[] mediaAudios)
        {
            if (mediaAudios?.Length > 0)
            {
                foreach (var mediaVideo in mediaAudios)
                {
                    if ((mediaVideo is null) || (!mediaVideo.IsStarted))
                        continue;

                    var m = storageAudio.FirstOrDefault(m => m.Id == mediaVideo.Id);
                    if (m is null)
                        storageAudio.Add(mediaVideo);
                }
            }
        }

        private void StoreVideoMedia(List<IMediaVideo> storageVideoOrSharing, params IMediaVideo?[] mediaVideos)
        {
            if (mediaVideos?.Length > 0)
            {
                foreach (var mediaVideo in mediaVideos)
                {
                    if ((mediaVideo is null) || (!mediaVideo.IsStarted))
                        continue;

                    var m = storageVideoOrSharing.FirstOrDefault(m => m.Id == mediaVideo.Id);
                    if (m is null)
                        storageVideoOrSharing.Add(mediaVideo);
                }
            }
        }

        private List<IMediaVideo> GetListOfVideosMediaUsed()
        {
            List<IMediaVideo> result = [];
            StoreVideoMedia(result, mediaInputVideo);
            StoreVideoMedia(result, mediaInputSharing);
            foreach(var video in _mediasForVideo)
                StoreVideoMedia(result, video);

            return result;
        }

        private List<IMediaAudio> GetListOfAudiosMediaUsed()
        {
            List<IMediaAudio> result = [];
            StoreAudioMedia(result, mediaInputAudio);
            foreach (var audio in _mediasForAudio)
                StoreAudioMedia(result, audio);

            return result;
        }

        public async Task UseMainStreamAsync(int media, Stream? stream = null )
        {
            if (media == Rainbow.Consts.Media.NONE)
                await UseStreamsAsync(null, null, null, otherStreams);
            else
            {
                Stream? streamAudio = Rainbow.Util.MediasWithAudio(media) ? stream : currentAudioStream;
                Stream? streamVideo = Rainbow.Util.MediasWithVideo(media) ? stream : currentVideoStream;
                Stream? streamSharing = Rainbow.Util.MediasWithSharing(media) ? stream : currentSharingStream;

                await UseStreamsAsync(streamAudio, streamVideo, streamSharing);
            }
        }

        public async Task UseOtherStreamsAsync(params Stream[] otherStreams)
        {
            await UseStreamsAsync(currentAudioStream, currentVideoStream, currentSharingStream, otherStreams);
        }


        private async Task UseStreamsAsync(Stream? streamForAudio, Stream? streamForVideo, Stream? streamForSharing, params Stream[] otherStreams)
        {
            // Ensure to call this method only when it's previous called is already finished
            await semaphoreUseOfStremSlim.WaitAsync();

            // Get list of media used
            List<IMediaAudio> audiosUsed = GetListOfAudiosMediaUsed();
            List<IMediaVideo> videosUsed = GetListOfVideosMediaUsed();

            // Clear storage
            _mediasForAudio.Clear();
            _mediasForVideo.Clear();

            await UseAudioStreamAsync(streamForAudio, audiosUsed, videosUsed);
            currentAudioStream = (mediaInputAudio is null) ? null : streamForAudio;

            await UseVideoStreamAsync(streamForVideo, audiosUsed, videosUsed);
            currentVideoStream = (mediaInputVideo is null) ? null : streamForVideo;

            await UseSharingStreamAsync(streamForSharing, audiosUsed, videosUsed);
            currentSharingStream = (mediaInputSharing is null) ? null : streamForSharing;

            await Task.Delay(200); // Add a small delay to let some times to close windows according streams in progress
            await CloseMediasNotUsedAsync(audiosUsed, videosUsed);

            // Unlock the semaphore
            try
            {
                if (semaphoreUseOfStremSlim.CurrentCount == 0)
                    semaphoreUseOfStremSlim.Release();
            }
            catch { }
        }

        private async Task<Boolean> UseAudioStreamAsync(Stream? streamForAudio, List<IMediaAudio> audiosUsed, List<IMediaVideo> videosUsed)
        {
            if (streamForAudio is null)
            {
                // Close previous Audio MediaInput
                await StopAudioInputAsync();
                return true;
            }

            // Do we use the same audio input ?
            if (mediaInputAudio?.Id == streamForAudio.Id)
            {
                StoreAudioMedia(_mediasForAudio, mediaInputAudio);
                if(mediaInputAudio is IMediaVideo video)
                    StoreVideoMedia(_mediasForVideo, video);

                if (streamForAudio.VideoComposition is not null)
                {
                    foreach (var id in streamForAudio.VideoComposition)
                    {
                        var subVideo = videosUsed.FirstOrDefault(m => m.Id == id);
                        StoreVideoMedia(_mediasForVideo, subVideo);
                        var subAudio = audiosUsed.FirstOrDefault(m => m.Id == id);
                        StoreAudioMedia(_mediasForAudio, subAudio);
                    }
                }
                return true;
            }

            // Close previous Audio MediaInput
            await StopAudioInputAsync();

            IMediaAudio? iMediaAudio;
            IMediaVideo? iMediaVideo;
            List<IMediaAudio> subAudios = [];
            List<IMediaVideo> subVideos = [];

            // Try to get audio input from audios list
            iMediaAudio = audiosUsed.FirstOrDefault(m => m.Id == streamForAudio.Id);

            // If not exists, try to create it
            if (iMediaAudio is null)
                (iMediaAudio, iMediaVideo, subAudios, subVideos) = await GetMediaInputs(streamForAudio, audiosUsed, videosUsed);
            else
            {
                if (iMediaAudio is IMediaVideo v)
                    iMediaVideo = v;
                else
                    iMediaVideo = null;
            }

            if (iMediaAudio is not null)
            {
                if (iMediaAudio.IsStarted)
                    Util.WriteDarkYellow($"Re-Use Audio MediaInput with [{iMediaAudio.Id}] ...");
                else
                    Util.WriteGreen($"Trying to Init/Start Audio MediaInput with [{iMediaAudio.Id}] ...");
                if (iMediaAudio.IsStarted || iMediaAudio.Init(true))
                {
                    // Store audio/video media
                    StoreAudioMedia(_mediasForAudio, iMediaAudio);
                    StoreAudioMedia(_mediasForAudio, subAudios.ToArray());
                    StoreVideoMedia(_mediasForVideo, iMediaVideo);
                    StoreVideoMedia(_mediasForVideo, subVideos.ToArray());

                    mediaInputAudio = iMediaAudio;

                    OnAudioStateChanged?.Invoke(mediaInputAudio.Id, true, false);

                    mediaInputAudio.OnAudioSample += MediaInput_OnAudioSample;
                    mediaInputAudio.OnEndOfFile += MediaInput_OnAudioEndOfFile;

                    Util.WriteDarkYellow($"MediaInput Audio initialized / started [{iMediaAudio.Id}]");
                    return true;
                }
            }
            return false;
        }

        private async Task<Boolean> UseVideoStreamAsync(Stream? streamForVideo, List<IMediaAudio> audiosUsed, List<IMediaVideo> videosUsed)
        {
            if (streamForVideo is null)
            {
                // Close previous Video MediaInput
                await StopVideoInputAsync();
                return true;
            }

            // Do we use the same video input ?
            if (mediaInputVideo?.Id == streamForVideo.Id)
            {
                StoreVideoMedia(_mediasForVideo, mediaInputVideo);
                if (mediaInputVideo is IMediaAudio audio)
                    StoreAudioMedia(_mediasForAudio, audio);

                if(streamForVideo.VideoComposition is not null)
                { 
                    foreach(var id in streamForVideo.VideoComposition)
                    {
                        var subVideo = videosUsed.FirstOrDefault(m => m.Id == id);
                        StoreVideoMedia(_mediasForVideo, subVideo);
                        var subAudio = audiosUsed.FirstOrDefault(m => m.Id == id);
                        StoreAudioMedia(_mediasForAudio, subAudio);
                    }
                }
                return true;
            }

            // Close previous Audio MediaInput
            await StopVideoInputAsync();

            IMediaAudio? iMediaAudio;
            IMediaVideo? iMediaVideo;
            List<IMediaAudio> subAudios = [];
            List<IMediaVideo> subVideos = [];

            // Try to get video input from videos list
            iMediaVideo = videosUsed.FirstOrDefault(m => m.Id == streamForVideo.Id);

            if (iMediaVideo is null)
                (iMediaAudio, iMediaVideo, subAudios, subVideos) = await GetMediaInputs(streamForVideo, audiosUsed, videosUsed);
            else
            {
                if (iMediaVideo is IMediaAudio a)
                    iMediaAudio = a;
                else
                    iMediaAudio = null;
            }

            if (iMediaVideo is not null)
            {
                if(iMediaVideo.IsStarted)
                    Util.WriteDarkYellow($"Re-Use Video MediaInput with [{iMediaVideo.Id}] ...");
                else
                    Util.WriteDarkYellow($"Trying to Init/start Video MediaInput with [{iMediaVideo.Id}] ...");
                if (iMediaVideo.IsStarted || iMediaVideo.Init(true))
                {
                    // Store audio/video media
                    StoreAudioMedia(_mediasForAudio, iMediaAudio);
                    StoreAudioMedia(_mediasForAudio, subAudios.ToArray());
                    StoreVideoMedia(_mediasForVideo, iMediaVideo);
                    StoreVideoMedia(_mediasForVideo, subVideos.ToArray());

                    mediaInputVideo = iMediaVideo;

                    OnVideoStateChanged?.Invoke(iMediaVideo.Id, true, false);

                    mediaInputVideo.OnImage += MediaInput_OnVideoImage;
                    mediaInputVideo.OnEndOfFile += MediaInput_OnVideoEndOfFile;

                    Util.WriteDarkYellow($"MediaInput Video initialized / started [{iMediaVideo.Id}]");
                    return true;
                }
            }
            return false;
        }

        private async Task<Boolean> UseSharingStreamAsync(Stream? streamForSharing, List<IMediaAudio> audiosUsed, List<IMediaVideo> videosUsed)
        {
            if (streamForSharing is null)
            {
                // Close previous Sharing MediaInput
                await StopSharingInputAsync();
                return true;
            }

            // Do we use the same Sharing input ?
            if (mediaInputSharing?.Id == streamForSharing.Id)
            {
                StoreVideoMedia(_mediasForVideo, mediaInputSharing);
                if (mediaInputSharing is IMediaAudio audio)
                    StoreAudioMedia(_mediasForAudio, audio);

                if (streamForSharing.VideoComposition is not null)
                {
                    foreach (var id in streamForSharing.VideoComposition)
                    {
                        var subVideo = videosUsed.FirstOrDefault(m => m.Id == id);
                        StoreVideoMedia(_mediasForVideo, subVideo);
                        var subAudio = audiosUsed.FirstOrDefault(m => m.Id == id);
                        StoreAudioMedia(_mediasForAudio, subAudio);
                    }
                }
                return true;
            }

            // Close previous Audio MediaInput
            await StopSharingInputAsync();

            IMediaAudio? iMediaAudio;
            IMediaVideo? iMediaSharing;
            List<IMediaAudio> subAudios = [];
            List<IMediaVideo> subVideos = [];

            // Try to get audio input from stored audio list
            iMediaSharing = videosUsed.FirstOrDefault(m => m.Id == streamForSharing.Id);

            if (iMediaSharing is null)
                (iMediaAudio, iMediaSharing, subAudios, subVideos) = await GetMediaInputs(streamForSharing, audiosUsed, videosUsed);
            else
            {
                if (iMediaSharing is IMediaAudio a)
                    iMediaAudio = a;
                else
                    iMediaAudio = null;
            }

            if (iMediaSharing is not null)
            {
                if (iMediaSharing.IsStarted)
                    Util.WriteDarkYellow($"Re-Use Video MediaInput with [{iMediaSharing.Id}] ...");
                else
                    Util.WriteDarkYellow($"Trying to Init/start Video MediaInput with [{iMediaSharing.Id}] ...");
                if (iMediaSharing.IsStarted || iMediaSharing.Init(true))
                {
                    // Store audio/video media
                    StoreAudioMedia(_mediasForAudio, iMediaAudio);
                    StoreAudioMedia(_mediasForAudio, subAudios.ToArray());
                    StoreVideoMedia(_mediasForVideo, iMediaSharing);
                    StoreVideoMedia(_mediasForVideo, subVideos.ToArray());

                    mediaInputSharing = iMediaSharing;

                    OnSharingStateChanged?.Invoke(iMediaSharing.Id, true, false);

                    mediaInputSharing.OnImage += MediaInput_OnSharingImage;
                    mediaInputSharing.OnEndOfFile += MediaInput_OnSharingEndOfFile;

                    Util.WriteDarkYellow($"MediaInput Sharing initialized / started [{iMediaSharing.Id}]");
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

            Util.WriteGreen($"Trying to create composition for Stream [{stream.Id}] ...");
            foreach (var id in stream.VideoComposition)
            {
                var s = streamsList.FirstOrDefault(s => s.Id == id);
                if (s is not null)
                {
                    var mi = videosUsed.FirstOrDefault(m => m.Id == id);
                    if (mi is null)
                    {
                        //Util.WriteGreen($"For composition, creating MediaInput for Stream:[{id}] ...");
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
                                Util.WriteRed($"For composition, MediaInput cannot be started for Stream:[{id}]");
                                success = false;
                                break;
                            }
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
                        mediaVideoList.Add(mi);

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
                        Util.WriteRed($"For composition, using \"template\" cannot create video filter ...");
                    else
                        Util.WriteDarkYellow($"For composition, using \"template\" filter created:\r\n{filter}");
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
                            Util.WriteRed($"For composition, cannot init MediaFiltered ...");
                    }
                    else
                        Util.WriteRed($"For composition, video filter defined is incorrect:\r\n{stream.VideoFilter}");
                }
            }
            else
            {
                // TODO - close medias opened for the composition
            }

            return (null, mediaAudioList, mediaVideoList);
        }

        //private async Task<IMediaVideo?> GetMediaInputComposition(Stream stream)
        //{
        //    Boolean success = true;
        //    List<Size> videoSize = [];

        //    if ((streamsList is null) || (stream is null) || (stream.VideoComposition is null))
        //        return null;

        //    Util.WriteGreen($"Trying to create composition ...");
        //    foreach (var id in stream.VideoComposition)
        //    {
        //        var s = streamsList.FirstOrDefault(s => s.Id == id);
        //        if (s is not null)
        //        {
        //            // Check if this media Input is already in used in the composition
        //            var mi = _mediaVideoListForComposition.FirstOrDefault(m => m.Id == s.Id);
        //            if (mi is null)
        //            {
        //                mi = GetVideoMediaAlreadyOpened(id);
        //                if (mi is not null)
        //                {
        //                    _mediaVideoListForComposition.Add(mi);
        //                    var size = new Size(mi.Width, mi.Height);
        //                    videoSize.Add(size);
        //                }
        //                else
        //                {
        //                    Util.WriteGreen($"For composition, creating MediaInput for Stream:[{id}] ...");
        //                    (var _, var iMediaVideo) = await GetMediaInputs(s, false);
        //                    if (iMediaVideo?.Init(true) == true)
        //                    {
        //                        _mediaVideoListForComposition.Add(iMediaVideo);

        //                        var size = new Size(iMediaVideo.Width, iMediaVideo.Height);
        //                        videoSize.Add(size);
        //                    }
        //                    else
        //                    {
        //                        // Cannot start 
        //                        Util.WriteRed($"For composition, MediaInput cannot be created for Stream:[{id}]");
        //                        success = false;
        //                        break;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                var size = new Size(mi.Width, mi.Height);
        //                videoSize.Add(size);

        //                Util.WriteDarkYellow($"For composition, re-use MediaInput with Stream:[{id}] ...");
        //            }
        //        }
        //        else
        //        {
        //            Util.WriteRed($"For composition, no Stream:[{id}] defined ...");
        //            success = false;
        //            break;
        //        }
        //    }

        //    if (success)
        //    {
        //        // Close media inputs which are not used in current composition
        //        await CloseMediaInputsNotUsedInCompostionAsync(stream.VideoComposition.ToArray());

        //        String? filter = null;
        //        var mediaFiltered = new MediaFiltered("mediaFiltered", _mediaVideoListForComposition, null);

        //        // Check if we have a filter already defined or we need to create it using a "template" ?
        //        if (stream.VideoFilterJsonNode is not null)
        //        {
        //            Util.WriteGreen($"For composition, using \"template\" to create video filter ...");

        //            // We need to get size of media inputs used.
        //            String type = stream.VideoFilterJsonNode["type"];
        //            var node = stream.VideoFilterJsonNode["data"];

        //            if (node is not null)
        //            {
        //                Size? size;
        //                int fps;

        //                switch (type)
        //                {
        //                    case "mosaic":
        //                        // Get fps
        //                        fps = node["fps"];
        //                        if (fps == 0)
        //                            fps = 10;

        //                        // Get size of vignette
        //                        size = VideoFilter.SizeFromString(node["sizeVignette"]);
        //                        size ??= videoSize[0];

        //                        // Get layout
        //                        String layout = node["layout"];
        //                        if (String.IsNullOrEmpty(layout))
        //                            layout = "G";

        //                        filter = VideoFilter.FilterToMosaic(
        //                            srcVideoSize: videoSize,
        //                            vignette: size.Value,
        //                            layout: layout,
        //                            fps: fps
        //                            );

        //                        break;

        //                    case "overlay":

        //                        // Get size
        //                        size = VideoFilter.SizeFromString(node["size"]);
        //                        size ??= videoSize[0];

        //                        // Get fps
        //                        fps = node["fps"];
        //                        if (fps == 0)
        //                            fps = 10;

        //                        // Get overlay size
        //                        var sizeOverlay = VideoFilter.SizeFromString(node["sizeOverlay"]);
        //                        sizeOverlay ??= new Size((int)(videoSize[0].Width / 4), (int)(videoSize[0].Height / 4));

        //                        // Get overlay position
        //                        String positionOverLay = node["positionOverlay"];
        //                        if (String.IsNullOrEmpty(positionOverLay))
        //                            positionOverLay = "TR";

        //                        filter = VideoFilter.FilterToOverlay(
        //                            srcMain: videoSize[0],
        //                            srcOverlay: videoSize[1],
        //                            dst: size.Value,
        //                            dstOverlay: sizeOverlay.Value,
        //                            dstOverlayPosition: positionOverLay,
        //                            fps: fps
        //                            );
        //                        break;
        //                }
        //            }
        //            if (filter is null)
        //                Util.WriteRed($"For composition, using \"template\" cannot create video filter ...");
        //            else
        //                Util.WriteRed($"For composition, using \"template\" filter created:\r\n{filter}");
        //        }
        //        else
        //            filter = stream.VideoFilter;

        //        if (!String.IsNullOrEmpty(filter))
        //        {
        //            if (mediaFiltered.SetVideoFilter(stream.VideoComposition, filter))
        //            {
        //                if (mediaFiltered.Init(true))
        //                {
        //                    // Store filter used
        //                    stream.VideoFilterInConference = filter;
        //                    return mediaFiltered;
        //                }
        //                else
        //                    Util.WriteRed($"For composition, cannot init MediaFiltered ...");
        //            }
        //            else
        //                Util.WriteRed($"For composition, video filter defined is incorrect:\r\n{stream.VideoFilter}");
        //        }
        //    }

        //    Util.WriteGreen($"Closing all stream used in composition ...");
        //    await CloseMediaInputsNotUsedInCompostionAsync();
        //    return null;

        //}

        // Central point to create MediaInput (Audio, Video, Composition)
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

                            Util.WriteDarkYellow($"Not necessary to create new MediaInput {(withVideo ? $"Video [{stream.Id}]" : $"Audio [{stream.Id}]")} - use previous stream");
                            return (audioInput, videoInput, subMediaAudioList, subMediaVideoList);
                        }

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

            return (audioInput, videoInput, subMediaAudioList, subMediaVideoList);
        }

        private async Task StopAudioInputAsync()
        {
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
        }

        private async Task StopVideoInputAsync()
        {
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
        }

        private async Task StopSharingInputAsync()
        {
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

                    Util.WriteDarkYellow($"Dispose previous Audio MediaInput Id:{audio.Id} (no more used)");
                    audio.Dispose();


                }
            }

            foreach (var video in videoOrSharingPreviouslyUsed)
            {
                var resultAudio = _mediasForAudio.FirstOrDefault(m => m.Id == video.Id);
                var resultVideo = _mediasForVideo.FirstOrDefault(m => m.Id == video.Id);
                if ((resultAudio is null) && (resultVideo is null))
                {
                    Util.WriteDarkYellow($"Dispose previous Video MediaInput Id:{video.Id} (no more used)");
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
