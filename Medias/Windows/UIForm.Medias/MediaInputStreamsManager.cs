using System;
using System.Collections.Generic;
using System.Linq;
using Rainbow.Medias;
using Rainbow.Events;
using System.IO;
using System.Windows.Forms;
using Rainbow.SimpleJSON;

namespace SDK.UIForm.WebRTC
{
    public sealed class MediaInputStreamsManager
    {
        private static MediaInputStreamsManager? _instance = null;

        private String _configFilePath;
        private readonly Dictionary<String, MediaInput> _simpleMediaStreams;
        private readonly Dictionary<String, MediaFiltered> _filteredMediaStreams;

        private FormMediaInputStreams? _formMediaInputStreams = null;
        private FormWebcam? _formWebcam = null;
        private FormScreen? _formScreen = null;

#region EVENT(S)

        public event EventHandler? OnListUpdated;
        public event EventHandler? OnListUpdatedWithFilteredInputs;
        public event EventHandler? OnListUpdatedWithoutFilteredInputs;

        public event StateDelegate? OnMediaStreamStateChanged;

        public event EventHandler? OnConfigurationLoaded;

        public event EventHandler<BooleanEventArgs>? OnDisposeMediaStreamUpdated;

#endregion EVENT(S)

#region PUBLIC API

        public static MediaInputStreamsManager Instance
        {
            get {
                if (_instance == null)
                    _instance = new MediaInputStreamsManager();
                return _instance;
            }
        }

        public Dictionary<String, IMedia> GetList(Boolean withFilteredInputs, Boolean withSimpleInputs, Boolean withVideo, Boolean withAudio)
        {
            Dictionary<String, IMedia> result = new Dictionary<string, IMedia>();
            if (withSimpleInputs)
            {
                _simpleMediaStreams.ToList().ForEach(x => {

                    Boolean canAdd = false;

                    if (withAudio && !withVideo)
                    {
                        if (x.Value.HasAudio())
                            canAdd = true;

                    }
                    else if (withVideo && !withAudio)
                    {
                        if (x.Value.HasVideo())
                            canAdd = true;
                    }
                    else if (withAudio && withVideo)
                    {
                        if (x.Value.HasVideo() && x.Value.HasAudio())
                            canAdd = true;
                    }
                    else
                        canAdd = true;

                    if (canAdd)
                        result.Add(x.Key, x.Value);
                });
            }

            if (withFilteredInputs)
            {
                _filteredMediaStreams.ToList().ForEach(x =>
                {
                    Boolean canAdd = false;

                    if (withAudio && !withVideo)
                    {
                        if (x.Value.HasAudio())
                            canAdd = true;

                    }
                    else if (withVideo && !withAudio)
                    {
                        if (x.Value.HasVideo())
                            canAdd = true;
                    }
                    else if(withAudio && withVideo)
                    {
                        if (x.Value.HasVideo() && x.Value.HasAudio())
                            canAdd = true;
                    }
                    else
                        canAdd = true;

                    if (canAdd)
                        result.Add(x.Key, x.Value);
                });
            }

            return result;
        }

        public Boolean CheckIdUnicity(string id)
        {
            // Check Id unicity of all simple MediaStreams
            if (_simpleMediaStreams.ContainsKey(id))
                return false;

            // Check Id unicity of all filtered MediaStreams
            if (_filteredMediaStreams.ContainsKey(id))
                return false;

            return true;
        }

        public Boolean Add(String id, String uri, Boolean useVideo, Boolean useAudio)
        {
            if ( String.IsNullOrEmpty(id) || String.IsNullOrEmpty(uri) )
                return false;

            if (!CheckIdUnicity(id))
                return false;

            var mediaInput = new Rainbow.Medias.MediaInput(new InputStreamDevice(id, id, uri, withVideo: useVideo, withAudio: useAudio, loop: true), loggerPrefix: "");

            if(mediaInput.Init(true))
                return Add(mediaInput);

            return false;
        }

        public Boolean Add(IMedia? mediaStream)
        {
            if ( (mediaStream == null) || (String.IsNullOrEmpty(mediaStream.Id)) )
                return false;

            if (!CheckIdUnicity(mediaStream.Id))
                return false;

            if (mediaStream is MediaFiltered mediaFiltered)
            {
                if ((!mediaFiltered.HasAudio()) && (!mediaFiltered.HasVideo()))
                    return false;

                _filteredMediaStreams.Add(mediaStream.Id, mediaFiltered);
                OnListUpdatedWithFilteredInputs?.Invoke(this, null);
            }
            else if (mediaStream is MediaInput mediaInput)
            {
                if ((!mediaInput.HasAudio()) && (!mediaInput.HasVideo()))
                    return false;

                _simpleMediaStreams.Add(mediaStream.Id, mediaInput);
                OnListUpdatedWithoutFilteredInputs?.Invoke(this, null);
            }

            // We want to know the state of each MediaStream
            mediaStream.OnStateChanged += MediaStream_OnStateChanged;

            OnListUpdated?.Invoke(this, null);

            return true;
        }

        public Boolean Remove(String id, Boolean raiseEvent = true)
        {
            if (String.IsNullOrEmpty(id))
                return false;

            // Check on Filtered MediaStreams
            if (_filteredMediaStreams.ContainsKey(id))
            {
                _filteredMediaStreams.Remove(id, out MediaFiltered? mediaFiltered);
                if (mediaFiltered != null)
                {
                    mediaFiltered.OnStateChanged -= MediaStream_OnStateChanged;
                    mediaFiltered.Dispose();
                }

                if (raiseEvent)
                {
                    OnListUpdatedWithFilteredInputs?.Invoke(this, null);
                    OnListUpdated?.Invoke(this, null);
                }
                return true;
            }

            // TODO - This media is not a Filtered MediaStream BUT if could be one of the MediaStream used IN a Filtered MediaStream

            // Check on Simple MediaStreams
            if (_simpleMediaStreams.ContainsKey(id))
            {
                _simpleMediaStreams.Remove(id, out MediaInput? mediaInput);
                if (mediaInput != null)
                {
                    mediaInput.OnStateChanged -= MediaStream_OnStateChanged;
                    mediaInput.Dispose();
                }

                if (raiseEvent)
                {
                    OnListUpdatedWithoutFilteredInputs?.Invoke(this, null);
                    OnListUpdated?.Invoke(this, null);
                }
                return true;
            }

            return false;
        }

        public Boolean LoadConfiguration()
        {
            String message = "";

            Boolean needtoRaiseForFiltered = false;
            Boolean needtoRaiseForSimple = false;

            // Firts we clear existing MediaStreams => We need to remove them one by one
            while (_filteredMediaStreams.Count > 0)
            {
                needtoRaiseForFiltered = true;
                var first = _filteredMediaStreams.First();
                Remove(first.Key, false);
            }

            while (_simpleMediaStreams.Count > 0)
            {
                needtoRaiseForSimple = true;
                var first = _simpleMediaStreams.First();
                first.Value.Dispose();
                Remove(first.Key, false);
            }

            if (!File.Exists(_configFilePath))
                return false;
            try
            {
                String jsonConfig = File.ReadAllText(_configFilePath);
                var json = JSON.Parse(jsonConfig);

                if (json == null)
                {
                    MessageBox.Show($"Cannot get JSON data from file '{_configFilePath}'.");
                    return false;
                }

                if (json["medias"]?.IsArray == true)
                {
                    var mediaDescriptorList = new List<MediaInputStreamDescriptor>();

                    int index = 0;
                    var list = json["medias"];
                    foreach (var item in list)
                    {
                        index++;

                        MediaInputStreamDescriptor? mediaDescriptor = Helper.GetMediaInputStreamDescriptorFromJson(item);

                        if (mediaDescriptor != null)
                        {
                            if(mediaDescriptor.IsValid())
                            {
                                if(mediaDescriptor.Type == "MultiInputs")
                                {
                                    Boolean canAdd = true;
                                    // Need to get list of MediaInput
                                    List<MediaInput> mediaInputs = new List<MediaInput>();

                                    foreach(string id in mediaDescriptor.MediaInputIdList)
                                    {
                                        IMedia? mediaStream = GetMediaStream(id);
                                        if(mediaStream == null)
                                        {
                                            canAdd = false; ;
                                            message += $"\r\nPb to create FilteredMediaInput [{mediaDescriptor.Id}] - It's using a MediaInput with an unknown Id:[{id}]";
                                        }
                                        else if (mediaStream is MediaInput mediaInput)
                                        {
                                            mediaInputs.Add(mediaInput);
                                        }
                                        else
                                        {
                                            canAdd = false; ;
                                            message += $"\r\nPb to create FilteredMediaInput [{mediaDescriptor.Id}] - It's using a MediaInput [{id}] which is not a MediaInput";
                                        }
                                        if (!canAdd)
                                            break;
                                    }

                                    if (canAdd)
                                    {
                                        // Create MediaFiltered
                                        MediaFiltered mediaFiltered = new MediaFiltered(mediaDescriptor.Id, mediaInputs);
                                        if (!mediaFiltered.SetVideoFilter(mediaDescriptor.MediaInputIdListForVideoFilter, mediaDescriptor.VideoFilter))
                                            message += $"\r\nCannot set video filters on FilteredMediaInput [{mediaDescriptor.Id}]";

                                        if (mediaFiltered.Init())
                                        {
                                            mediaFiltered.OnStateChanged += MediaStream_OnStateChanged;
                                            _filteredMediaStreams.Add(mediaFiltered.Id, mediaFiltered);
                                        }
                                        else
                                            message += $"\r\nPb to init MediaFilteread [{mediaFiltered.Id}]";
                                    }
                                }
                                else if (mediaDescriptor.Type == "FileOrStream")
                                {
                                    var mediaInput = new Rainbow.Medias.MediaInput(new InputStreamDevice(mediaDescriptor.Id, mediaDescriptor.Id, mediaDescriptor.Uri, withVideo: mediaDescriptor.Video, withAudio: mediaDescriptor.Audio, loop: mediaDescriptor.Loop), loggerPrefix: "");
                                    if (mediaInput.Init())
                                    {
                                        mediaInput.OnStateChanged += MediaStream_OnStateChanged;
                                        _simpleMediaStreams.Add(mediaInput.Id, mediaInput);
                                    }
                                    else
                                        message += $"\r\nPb to init Simple MediaInput [{mediaDescriptor.Id}]";
                                }
                            }
                            else
                            {
                                message += $"\r\nInvalid MediaInputStreamDescriptor object - index={index}";
                            }
                        }
                        else
                        {
                            message += $"\r\nCannot create MediaInputStreamDescriptor object from json info - index={index}";
                        }
                    }
                }
                else
                    message += $"Cannot read 'medias' array in '{_configFilePath}'";


            }
            catch (Exception exc)
            {
                message += $"\r\nException occurs when reading '{_configFilePath}' - Exception:[{Rainbow.Util.SerializeException(exc)}]";

            }

            // Raise event(s)
            if ( (_simpleMediaStreams.Count > 0) || needtoRaiseForSimple)
                OnListUpdatedWithoutFilteredInputs?.Invoke(this, null);

            if ( (_filteredMediaStreams.Count > 0) || needtoRaiseForFiltered)
                OnListUpdatedWithFilteredInputs?.Invoke(this, null);

            if ( (_simpleMediaStreams.Count > 0) || (_filteredMediaStreams.Count > 0) || needtoRaiseForSimple || needtoRaiseForFiltered)
                OnListUpdated?.Invoke(this, null);

            if (!String.IsNullOrEmpty(message))
            {
                MessageBox.Show(message);
                return false;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            return true;
        }

        public void SaveConfiguration()
        {
            // First, we loop on Simple MediaStream 
            List<MediaInputStreamDescriptor> mediaInputStreamDescriptors = new List<MediaInputStreamDescriptor>();
            foreach(var stream in _simpleMediaStreams.Values.ToList())
            {
                if (stream is MediaInput mediaInput)
                {
                    MediaInputStreamDescriptor descriptor = new MediaInputStreamDescriptor();
                    descriptor.Id = mediaInput.Id;
                    descriptor.Type = "FileOrStream";
                    descriptor.Uri = mediaInput.Path;
                    descriptor.Video = stream.HasVideo();
                    descriptor.Audio = stream.HasAudio();
                    descriptor.Loop = mediaInput.Loop;

                    mediaInputStreamDescriptors.Add(descriptor);
                }
            }

            // Now we loop on Filtered MediaStream 
            foreach (var stream in _filteredMediaStreams.Values.ToList())
            {
                if (stream is MediaFiltered mediaFiltered)
                {
                    MediaInputStreamDescriptor descriptor = new MediaInputStreamDescriptor();
                    descriptor.Id = mediaFiltered.Id;
                    descriptor.Type = "MultiInputs";
                    
                    descriptor.Video = stream.HasVideo();
                    descriptor.Audio = stream.HasAudio();
                    descriptor.Loop = true;

                    if (mediaFiltered.MediaInputs?.Count > 0)
                        descriptor.MediaInputIdList = mediaFiltered.MediaInputs.Keys.ToList();

                    descriptor.MediaInputIdListForVideoFilter = mediaFiltered.VideoIdUsedInFilter;
                    descriptor.VideoFilter = mediaFiltered.VideoFilter;

                    mediaInputStreamDescriptors.Add(descriptor);
                }
            }

            var strContent = Helper.GetJsonStringFromListOfMediaInputStreamDescriptor(mediaInputStreamDescriptors);
            strContent = "{\r\n\t\"medias\": " + strContent + "\r\n}";
            File.WriteAllText(_configFilePath, strContent);
        }

        public IMedia? GetMediaStream(string? id)
        {
            if (id == null)
                return null;

            if (_filteredMediaStreams.ContainsKey(id))
                return _filteredMediaStreams[id];

            if (_simpleMediaStreams.ContainsKey(id))
                return _simpleMediaStreams[id];

            return null;
        }

        public Boolean StartMediaStream(string? id)
        {
            var stream = GetMediaStream(id);
            if (stream != null)
                return stream.Start();
            return false;
        }

        public Boolean StopMediaStream(string? id)
        {
            Boolean result = false;
            var stream = GetMediaStream(id);
            if (stream != null)
            {
                result = stream.Stop();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return result;
        }

        public void OpenFormMediaFilteredStreams()
        {
            if (_formMediaInputStreams == null)
                _formMediaInputStreams = new FormMediaInputStreams();

            _formMediaInputStreams.WindowState = FormWindowState.Normal;
            _formMediaInputStreams.Show();
            _formMediaInputStreams.Activate();
        }

        public void OpenFormWebcam()
        {
            if (_formWebcam == null)
                _formWebcam = new FormWebcam();

            _formWebcam.WindowState = FormWindowState.Normal;
            _formWebcam.Show();
            _formWebcam.Activate();
        }

        public void OpenFormScreen()
        {
            if (_formScreen == null)
                _formScreen = new FormScreen();

            _formScreen.WindowState = FormWindowState.Normal;
            _formScreen.Show();
            _formScreen.Activate();
        }

#endregion PUBLIC API

#region PRIVATE API

        public void RemoveMediaStream(string? id)
        {
            if (id == null)
                return;

            if (_filteredMediaStreams.ContainsKey(id))
                _filteredMediaStreams.Remove(id);

            if (_simpleMediaStreams.ContainsKey(id))
                _simpleMediaStreams.Remove(id);

            // TODO - Check _filteredMediaStreams using this ID in the filter
        }

        private void MediaStream_OnStateChanged(string mediaId, bool isStarted, bool isPaused)
        {
            OnMediaStreamStateChanged?.Invoke(mediaId, isStarted, isPaused);
        }

        private MediaInputStreamsManager()
        {
            _configFilePath = $".{Path.DirectorySeparatorChar}Resources{Path.DirectorySeparatorChar}medias.json";
            _simpleMediaStreams = new Dictionary<string, MediaInput>();
            _filteredMediaStreams = new Dictionary<string, MediaFiltered>();
        }

#endregion PRIVATE API
    }

}
