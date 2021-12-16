using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

using SDK.WpfApp.Model;

using Rainbow;
using Rainbow.Model;
using Rainbow.Wpf;

using Microsoft.Extensions.Logging;

namespace SDK.WpfApp.ViewModel
{
    public class AppViewModel
    {
        private static readonly ILogger log = Rainbow.LogFactory.CreateLogger<AppViewModel>();

        private Rainbow.Application rbApplication;
        private Rainbow.Contacts rbContacts;
        private Rainbow.Wpf.WebRtcControl RbWebRtcControl;
        private Rainbow.Common.Languages rbLanguages;

        private Call currentCall;

        public LanguagesModel LanguagesModel { get; private set; }      // Need to be public and a property - Used as Binding from XAML
        public UsersModel UsersModel { get; private set; }              // Need to be public and a property - Used as Binding from XAML
        public DevicesModel DevicesModel { get; private set; }          // Need to be public and a property - Used as Binding from XAML
        public LoginInfoModel LoginInfoModel { get; private set; }      // Need to be public and a property - Used as Binding from XAML
        public ConversationModel ConversationModel { get; private set; }// Need to be public and a property - Used as Binding from XAML
        public LayoutModel LayoutModel { get; private set; }            // Need to be public and a property - Used as Binding from XAML
        public ConstraintModel ConstraintModel { get; private set; }    // Need to be public and a property - Used as Binding from XAML

        public void SetRbApplication(Rainbow.Application rbApp)
        {
            rbApplication = rbApp;
            rbContacts = rbApp.GetContacts();

            // Manage some events from Rainbow.Application
            rbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;
            rbApplication.InitializationPerformed += RbApplication_InitializationPerformed;

            // Manage some events from Rainbow.Contacts
            rbContacts.PeerAdded += RbContacts_PeerAdded;
        }

        public void SetWebRtcControl(Rainbow.Wpf.WebRtcControl webRtcControl)
        {
            RbWebRtcControl = webRtcControl;

            // Manage some events from  Rainbow.Wpf.WebRtcControl
            RbWebRtcControl.InitializationPerformed += RbWebRtcControl_InitializationPerformed;
            RbWebRtcControl.DevicesChanged += RbWebRtcControl_DevicesChanged;
            RbWebRtcControl.CallUpdated += RbWebRtcControl_CallUpdated;
        }

        public AppViewModel()
        {
            // Create LoginInfoModel
            LoginInfoModel = new LoginInfoModel
            {
                ButtonLoginOrLogoutCommand = new RelayCommand<object>(new Action<object>(LoginOrLogoutCommand), new Predicate<object>(LoginOrLogoutCommandCanExecute))
            };

            // Create ConversationModel
            ConversationModel = new ConversationModel
            {
                ButtonRejectCommand = new RelayCommand<object>(new Action<object>(RejectCommand), new Predicate<object>(RejectCommandCanExecute)),
                ButtonHangUpCommand = new RelayCommand<object>(new Action<object>(HangUpCommand), new Predicate<object>(HangUpCommandCanExecute)),
                ButtonAnswerCommand = new RelayCommand<object>(new Action<object>(AnswerCommand), new Predicate<object>(AnswerCommandCanExecute)),
                ButtonCallCommand = new RelayCommand<object>(new Action<object>(CallCommand), new Predicate<object>(CallCommandCanExecute)),
                ButtonSelectDevicesCommand = new RelayCommand<object>(new Action<object>(SelectDevicesCommand), new Predicate<object>(SelectDevicesCommandCanExecute)),

                ButtonMuteUnmuteAudioCommand = new RelayCommand<object>(new Action<object>(MuteUnmuteAudioCommand), new Predicate<object>(MuteUnmuteAudioCommandCanExecute)),
                ButtonMuteUnmuteVideoCommand = new RelayCommand<object>(new Action<object>(MuteUnmuteVideoCommand), new Predicate<object>(MuteUnmuteVideoCommandCanExecute)),
                ButtonMuteUnmuteSharingCommand = new RelayCommand<object>(new Action<object>(MuteUnmuteSharingCommand), new Predicate<object>(MuteUnmuteSharingCommandCanExecute)),

                ButtonAddAudioCommand = new RelayCommand<object>(new Action<object>(AddAudioCommand), new Predicate<object>(AddAudioCommandCanExecute)),
                ButtonAddRemoveVideoCommand = new RelayCommand<object>(new Action<object>(AddRemoveVideoCommand), new Predicate<object>(AddRemoveVideoCommandCanExecute)),
                ButtonAddRemoveSharingCommand = new RelayCommand<object>(new Action<object>(AddRemoveSharingCommand), new Predicate<object>(AddRemoveSharingCommandCanExecute)),
            };

            // Create LayoutModel
            LayoutModel = new LayoutModel();
            LayoutModel.LocalVideo.ButtonSetCommand = new RelayCommand<object>(new Action<object>(SetLocalVideoCommand));
            LayoutModel.LocalVideo.ButtonPictureInPictureSwitchCommand = new RelayCommand<object>(new Action<object>(SwitchPictureInPictureModeOnLocalVideoCommand));
            LayoutModel.RemoteVideo.ButtonSetCommand = new RelayCommand<object>(new Action<object>(SetRemoteVideoCommand));
            LayoutModel.RemoteVideo.ButtonPictureInPictureSwitchCommand = new RelayCommand<object>(new Action<object>(SwitchPictureInPictureModeOnRemoteVideoCommand));
            LayoutModel.Sharing.ButtonSetCommand = new RelayCommand<object>(new Action<object>(SetSharingCommand));
            LayoutModel.Sharing.ButtonPictureInPictureSwitchCommand = new RelayCommand<object>(new Action<object>(SwitchPictureInPictureModeOnSharingCommand));

            LayoutModel.PropertyChanged += LayoutModel_PropertyChanged;

            // Create DevicesModel
            DevicesModel = new DevicesModel();

            // Create UsersModel
            UsersModel = new UsersModel();

            // Create LanguagesModel
            LanguagesModel = new LanguagesModel()
            {
                ButtonSetCommand = new RelayCommand<object>(new Action<object>(SetLanguageCommand))
            };

            // Get Rainbow Languages service
            rbLanguages = Rainbow.Common.Languages.Instance;
            SetLanguagesList(rbLanguages.GetLanguagesIdList());

            // Create ConstraintModel
            ConstraintModel = new ConstraintModel()
            {
                MaxFrameRateValue = "10",
                WidthValue = "640",
                HeightValue = "480",
                VideoConstraint = true,
                ButtonSetConstraintCommand = new RelayCommand<object>(new Action<object>(SetConstraintCommand), new Predicate<object>(SetConstraintCommandCanExecute))
            };
        }

        private void SetLanguagesList(List<String> languages)
        {
            LanguagesModel.Languages.Clear();

            foreach (String language in languages)
                LanguagesModel.Languages.Add(language);
        }

        private StreamDevice GetDefaultAudioInputDevice(List<StreamDevice> devices)
        {
            foreach (StreamDevice device in devices)
            {
                // FOR TESTS PURPOSE, HERE WE ARE LOOKING FOR A "Realtek" device
                if (device.Label.Contains("Realtek"))
                    return device;
            }

            // We don't find an element - we get the first one
            if (devices.Count > 0)
                return devices[0];

            return null;
        }

        private StreamDevice GetDefaultAudioOutputDevice(List<StreamDevice> devices)
        {
            // FOR TESTS PURPOSE, HERE WE ARE LOOKING FOR A "Realtek" device
            foreach (StreamDevice device in devices)
            {
                if (device.Label.Contains("Realtek"))
                    return device;
            }

            // We don't find an element - we get the first one
            if (devices.Count > 0)
                return devices[0];

            return null;
        }

        private StreamDevice GetDefaultVideoInputDevice(List<StreamDevice> devices)
        {
            // FOR TESTS PURPOSE, HERE WE ARE LOOKING FOR A "Integrated" device
            foreach (StreamDevice device in devices)
            {
                if (device.Label.Contains("Integrated"))
                    return device;
            }

            // We don't find an element - we get the first one
            if (devices.Count > 0)
                return devices[0];

            return null;
        }

#region EVENTS - FROM WebRTCControl
        private void RbWebRtcControl_InitializationPerformed(object sender, EventArgs e)
        {
            // Need to be on UI Thread
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                // SET DEFAULT POSITION - LOCAL VIDEO
                LayoutModel.LocalVideo.LeftPositionUsed = false;
                LayoutModel.LocalVideo.HorizontalValue = "5px";

                LayoutModel.LocalVideo.BottomPositionUsed = false;
                LayoutModel.LocalVideo.VerticalValue = "5px";

                LayoutModel.LocalVideo.WidthValue = "160";
                LayoutModel.LocalVideo.HeightValue = "120";

                LayoutModel.LocalVideo.HorizontalTranslateValue = "0px";
                LayoutModel.LocalVideo.VerticalTranslateValue = "0px";

                LayoutModel.LocalVideo.Hidden = false;

                SetLocalVideoCommand(null);


                // SET DEFAULT POSITION - DISTANT VIDEO
                LayoutModel.RemoteVideo.LeftPositionUsed = true;
                LayoutModel.RemoteVideo.HorizontalValue = "50%";

                LayoutModel.RemoteVideo.BottomPositionUsed = false;
                LayoutModel.RemoteVideo.VerticalValue = "50%";

                LayoutModel.RemoteVideo.WidthValue = "480";
                LayoutModel.RemoteVideo.HeightValue = "360";

                LayoutModel.RemoteVideo.HorizontalTranslateValue = "-50%";
                LayoutModel.RemoteVideo.VerticalTranslateValue = "-50%";

                LayoutModel.RemoteVideo.Hidden = false;

                SetRemoteVideoCommand(null);


                // SET DEFAULT POSITION - Sharing
                LayoutModel.Sharing.LeftPositionUsed = false;
                LayoutModel.Sharing.HorizontalValue = "5px";

                LayoutModel.Sharing.BottomPositionUsed = true;
                LayoutModel.Sharing.VerticalValue = "5px";

                LayoutModel.Sharing.WidthValue = "160";
                LayoutModel.Sharing.HeightValue = "120";

                LayoutModel.Sharing.HorizontalTranslateValue = "0px";
                LayoutModel.Sharing.VerticalTranslateValue = "0px";

                LayoutModel.Sharing.Hidden = false;

                SetSharingCommand(null);


                ConversationModel.WebRtcInitialized = true;
            }));
        }

        private void RbWebRtcControl_DevicesChanged(object sender, EventArgs e)
        {
            // Need to be on UI Thread
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                List<StreamDevice> devices;

                StreamDevice streamDevice;
                DeviceModel deviceModel;

                ////////////////////////////////////
                /////
                // MANAGE AUDIO INPUT DEVICE

                // Get AudioInput devices
                devices = RbWebRtcControl.GetDevices(StreamDevice.KindType.AudioInput);
                
                // Get previous AudioInput Device selected
                streamDevice = RbWebRtcControl.GetDeviceSelected(StreamDevice.KindType.AudioInput);

                // If there is not already AudioInput Device selected select the default on
                if (streamDevice == null)
                {
                    streamDevice = GetDefaultAudioInputDevice(devices);

                    // Select this device
                    if(streamDevice != null)
                        RbWebRtcControl.SelectDevice(StreamDevice.KindType.AudioInput, streamDevice.DeviceId);
                }

                // If we have a stream device already selected or a default one, add it first in the list
                if (streamDevice != null)
                {
                    deviceModel = new DeviceModel { Id = streamDevice.DeviceId, Name = streamDevice.Label };
                    DevicesModel.AudioInputDevices.Add(deviceModel);
                    devices.Remove(streamDevice);

                    ConversationModel.LocalInputAudioSelected = true;
                }

                // Add all others Audio Input devices
                foreach (StreamDevice device in devices)
                {
                    deviceModel = new DeviceModel { Id = device.DeviceId, Name = device.Label };
                    DevicesModel.AudioInputDevices.Add(deviceModel);
                }


                ////////////////////////////////////
                /////
                // MANAGE AUDIO OUTPUT DEVICE

                // Get AudioInput devices
                devices = RbWebRtcControl.GetDevices(StreamDevice.KindType.AudioOutput);

                // Get previous AudioInput Device selected
                streamDevice = RbWebRtcControl.GetDeviceSelected(StreamDevice.KindType.AudioOutput);

                // If there is not already Audio Output Device selected select the default on
                if (streamDevice == null)
                {
                    streamDevice = GetDefaultAudioOutputDevice(devices);

                    // Select this device
                    if (streamDevice != null)
                        RbWebRtcControl.SelectDevice(StreamDevice.KindType.AudioOutput, streamDevice.DeviceId);
                }

                // If we have a stream device already seletc or a default one, add it first in the list
                if (streamDevice != null)
                {
                    deviceModel = new DeviceModel { Id = streamDevice.DeviceId, Name = streamDevice.Label };
                    DevicesModel.AudioOutputDevices.Add(deviceModel);
                    devices.Remove(streamDevice);

                    ConversationModel.MakeCallUsingAudio = true;
                    ConversationModel.DevicesSelected = true;
                }

                // Add all others Audio Output devices
                foreach (StreamDevice device in devices)
                {
                    deviceModel = new DeviceModel { Id = device.DeviceId, Name = device.Label };
                    DevicesModel.AudioOutputDevices.Add(deviceModel);
                }

                ////////////////////////////////////
                /////
                // MANAGE VIDEO INPUT DEVICE

                // Get Video Input devices
                devices = RbWebRtcControl.GetDevices(StreamDevice.KindType.VideoInput);

                // Get previous AudioInput Device selected
                streamDevice = RbWebRtcControl.GetDeviceSelected(StreamDevice.KindType.VideoInput);

                // If there is not already AudioInput Device selected select the default on
                if (streamDevice == null)
                {
                    streamDevice = GetDefaultVideoInputDevice(devices);

                    // Select this device
                    if (streamDevice != null)
                        RbWebRtcControl.SelectDevice(StreamDevice.KindType.VideoInput, streamDevice.DeviceId);
                }

                // If we have a stream device already selected or a default one, add it first in the list
                if (streamDevice != null)
                {
                    deviceModel = new DeviceModel { Id = streamDevice.DeviceId, Name = streamDevice.Label };
                    DevicesModel.VideoInputDevices.Add(deviceModel);
                    devices.Remove(streamDevice);

                    ConversationModel.LocalInputVideoSelected = true;
                    ConversationModel.MakeCallUsingVideo = true;
                }

                // Add all others Audio Input devices
                foreach (StreamDevice device in devices)
                {
                    deviceModel = new DeviceModel { Id = device.DeviceId, Name = device.Label };
                    DevicesModel.VideoInputDevices.Add(deviceModel);
                }

                // We have devices enumaration done at least once
                ConversationModel.DevicesEnumerated = true;

                CommandManager.InvalidateRequerySuggested();
            }));
        }

        private void RbWebRtcControl_CallUpdated(object sender, Rainbow.Events.CallEventArgs e)
        {
            // Need to be on UI Thread
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                // Store the call
                currentCall = e.Call;

                // Update Conversation Model
                ConversationModel.CallId = currentCall.Id;
                ConversationModel.CallInProgress = (currentCall.CallStatus != Call.Status.UNKNOWN);
                ConversationModel.CallRinging = (currentCall.CallStatus == Call.Status.RINGING_INCOMING) || (currentCall.CallStatus == Call.Status.RINGING_OUTGOING);
                ConversationModel.CallIncoming = !currentCall.IsInitiator;

                // // Call in progress
                if (ConversationModel.CallInProgress)
                {
                    // Set remote media
                    ConversationModel.RemoteUsingAudio  = RbWebRtcControl.CallUsingRemoteAudio();
                    ConversationModel.RemoteUsingVideo  = RbWebRtcControl.CallUsingRemoteVideo();
                    ConversationModel.RemoteUsingSharing = RbWebRtcControl.CallUsingRemoteSharing();

                    // Set local media
                    ConversationModel.LocalUsingAudio   = RbWebRtcControl.CallUsingLocalAudio();
                    ConversationModel.LocalUsingVideo   = RbWebRtcControl.CallUsingLocalVideo();
                    ConversationModel.LocalUsingSharing = RbWebRtcControl.CallUsingLocalSharing();

                    // Muted status
                    ConversationModel.LocalAudioMuted = RbWebRtcControl.CallWithLocalAudioMuted();
                    ConversationModel.LocalVideoMuted = RbWebRtcControl.CallWithLocalVideoMuted();
                    ConversationModel.LocalSharingMuted = RbWebRtcControl.CallWithLocalSharingMuted();

                    if (ConversationModel.CallIncoming)
                    {
                        if (ConversationModel.RemoteUsingSharing)
                        {
                            ConversationModel.CanAnswerUsingAudio = false;
                            ConversationModel.CanAnswerUsingVideo = false;

                            ConversationModel.AnswerUsingAudio = false;
                            ConversationModel.AnswerUsingVideo = false;

                            ConversationModel.AnswerUsingSharing = true;
                        }
                        else
                        {
                            ConversationModel.CanAnswerUsingAudio = ConversationModel.RemoteUsingAudio;
                            ConversationModel.CanAnswerUsingVideo = ConversationModel.RemoteUsingVideo;

                            ConversationModel.AnswerUsingAudio = ConversationModel.RemoteUsingAudio;
                            ConversationModel.AnswerUsingVideo = ConversationModel.RemoteUsingVideo;

                            ConversationModel.AnswerUsingSharing = false;
                        }
                    }
                }
                else // Call not in progress
                {
                    ConversationModel.MakeCallUsingAudio = true;
                    ConversationModel.MakeCallUsingVideo = true;
                    ConversationModel.MakeCallUsingSharing = false;

                    ConversationModel.CanAnswerUsingAudio = false;
                    ConversationModel.CanAnswerUsingVideo = false;
                    ConversationModel.AnswerUsingAudio = false;
                    ConversationModel.AnswerUsingVideo = false;
                    ConversationModel.AnswerUsingSharing = false;

                    ConversationModel.RemoteUsingAudio = false;
                    ConversationModel.RemoteUsingVideo = false;
                    ConversationModel.RemoteUsingSharing = false;
                    ConversationModel.LocalUsingAudio = false;
                    ConversationModel.LocalUsingVideo = false;
                    ConversationModel.LocalUsingSharing = false;

                    ConversationModel.LocalAudioMuted = false;
                    ConversationModel.LocalVideoMuted = false;
                }

                log.LogDebug("[CallUpdated] CallId:[{0}] - CallInProgress[{1}] - CallRinging[{2}] - CallIncoming[{3}]", ConversationModel.CallId, ConversationModel.CallInProgress, ConversationModel.CallRinging, ConversationModel.CallIncoming);

                CommandManager.InvalidateRequerySuggested();
            }));
        }
#endregion EVENTS - FROM WebRTCControl

#region EVENTS - FROM Rainbow Application
        private void RbApplication_InitializationPerformed(object sender, EventArgs e)
        {
            // Need to be on UI Thread
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                UsersModel.CurrentContactId = rbContacts.GetCurrentContactId();
                UsersModel.AddContacts(rbContacts.GetAllContactsFromCache());
                LoginInfoModel.InitialisationCompleted = true;

                // We allow to download avatars to all contacts in the roster
                Rainbow.Common.Avatars.Instance.AddInDownloadPoolAllAvatarsFromContactsStoredInCache();

                CommandManager.InvalidateRequerySuggested();
            }));
        }

        private void RbApplication_ConnectionStateChanged(object sender, Rainbow.Events.ConnectionStateEventArgs e)
        {
            // Need to be on UI Thread
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                LoginInfoModel.ConnectionState = e.State;

                CommandManager.InvalidateRequerySuggested();
            }));
        }
        #endregion EVENTS - FROM Rainbow Application

        #region EVENTS - FROM Rainbow Contacts        
        private void RbContacts_PeerAdded(object sender, Rainbow.Events.PeerEventArgs e)
        {
            if (!LoginInfoModel.InitialisationCompleted)
                return;

            // Need to be on UI Thread
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                UsersModel.AddContact(rbContacts.GetContactFromContactJid(e.Peer.Jid));
            }));

        }
#endregion EVENTS - FROM Rainbow Contacts

        private void LayoutModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "AutomaticDisplayMode")
            {
                RbWebRtcControl.SetAutomaticDisplayModeForVideoElements(LayoutModel.AutomaticDisplayMode);

                // If automatic mode is not used, we set video element manually as it's defined in the interface
                if(!LayoutModel.AutomaticDisplayMode)
                {
                    SetRemoteVideoCommand(null);
                    SetLocalVideoCommand(null);
                    SetSharingCommand(null);
                }
            }
        }

#region BUTTONS - Command

        public void RejectCommand(object obj)
        {
            if (currentCall != null)
                RbWebRtcControl.RejectCall(currentCall.Id);
        }

        public void HangUpCommand(object obj)
        {
            if (currentCall != null)
                RbWebRtcControl.HangUpCall(currentCall.Id);
        }

        public void AnswerCommand(object obj)
        {
            int medias = 0;
            if (ConversationModel.AnswerUsingAudio)
                medias += (int)Call.Media.AUDIO;

            if (ConversationModel.AnswerUsingVideo)
                medias += (int)Call.Media.VIDEO;

            if (ConversationModel.AnswerUsingSharing)
                medias += (int)Call.Media.SHARING;

            if (currentCall != null)
                RbWebRtcControl.AnswerCall(currentCall.Id, medias);
        }

        public void CallCommand(object obj)
        {
            if (UsersModel.UserSelected == null)
                return;

            int medias = 0;
            if (ConversationModel.MakeCallUsingAudio)
                medias += (int)Call.Media.AUDIO;

            if (ConversationModel.MakeCallUsingVideo)
                medias += (int)Call.Media.VIDEO;

            if (ConversationModel.MakeCallUsingSharing)
                medias += (int)Call.Media.SHARING;

            RbWebRtcControl.MakeCall(UsersModel.UserSelected.Id, medias, ConversationModel.MakeCallUsingMessage);
        }

        public void SelectDevicesCommand(object obj)
        {
            String audioInputId = DevicesModel.AudioInputDeviceSelected?.Id;
            String audioOutputId = DevicesModel.AudioOutputDeviceSelected?.Id;
            String videoInputId = DevicesModel.VideoInputDeviceSelected?.Id;

            log.LogDebug("DEVICE SELECTED : Audio Input:[{0}] - [{1}]", DevicesModel.AudioInputDeviceSelected?.Id, DevicesModel.AudioInputDeviceSelected?.Name);
            log.LogDebug("DEVICE SELECTED : Audio Output:[{0}] - [{1}]", DevicesModel.AudioOutputDeviceSelected?.Id, DevicesModel.AudioOutputDeviceSelected?.Name);
            log.LogDebug("DEVICE SELECTED : Video Input:[{0}] - [{1}]", DevicesModel.VideoInputDeviceSelected?.Id, DevicesModel.VideoInputDeviceSelected?.Name);

            RbWebRtcControl.SelectDevice(StreamDevice.KindType.AudioInput, audioInputId);
            RbWebRtcControl.SelectDevice(StreamDevice.KindType.AudioOutput, audioOutputId);
            RbWebRtcControl.SelectDevice(StreamDevice.KindType.VideoInput, videoInputId);

            return;
        }

        public void LoginOrLogoutCommand(object obj)
        {
            if (LoginInfoModel.ConnectionState == Rainbow.Model.ConnectionState.Connected)
            {
                // Don't use UI Thread to perform network task
                Task task = new Task(() =>
                {
                    rbApplication.Logout();
                });
                task.Start();

            }
            else if (LoginInfoModel.ConnectionState == Rainbow.Model.ConnectionState.Disconnected)
            {
                // Don't use UI Thread to perform network task
                Task task = new Task(() =>
                {
                    rbApplication.Login(LoginInfoModel.Login, LoginInfoModel.Password);
                });
                task.Start();
            }
        }

        public void SetSharingCommand(object obj)
        {
            RbWebRtcControl.HideVideoElement(VideoElement.Type.SharingVideo, LayoutModel.Sharing.Hidden);

            RbWebRtcControl.SetTranslateVideoElement(VideoElement.Type.SharingVideo, LayoutModel.Sharing.HorizontalTranslateValue, LayoutModel.Sharing.VerticalTranslateValue);

            RbWebRtcControl.SetSizeVideoElement(VideoElement.Type.SharingVideo, LayoutModel.Sharing.WidthValue, LayoutModel.Sharing.HeightValue);

            if (LayoutModel.Sharing.BottomPositionUsed)
            {
                if (LayoutModel.Sharing.LeftPositionUsed)
                {
                    RbWebRtcControl.SetBottomLeftVideoElement(VideoElement.Type.SharingVideo, LayoutModel.Sharing.VerticalValue, LayoutModel.Sharing.HorizontalValue);
                }
                else
                {
                    RbWebRtcControl.SetBottomRightVideoElement(VideoElement.Type.SharingVideo, LayoutModel.Sharing.VerticalValue, LayoutModel.Sharing.HorizontalValue);
                }
            }
            else
            {
                if (LayoutModel.Sharing.LeftPositionUsed)
                {
                    RbWebRtcControl.SetTopLeftVideoElement(VideoElement.Type.SharingVideo, LayoutModel.Sharing.VerticalValue, LayoutModel.Sharing.HorizontalValue);
                }
                else
                {
                    RbWebRtcControl.SetTopRightVideoElement(VideoElement.Type.SharingVideo, LayoutModel.Sharing.VerticalValue, LayoutModel.Sharing.HorizontalValue);
                }
            }
        }

        public void SetRemoteVideoCommand(object obj)
        {
            RbWebRtcControl.HideVideoElement(VideoElement.Type.RemoteVideo, LayoutModel.RemoteVideo.Hidden);

            RbWebRtcControl.SetTranslateVideoElement(VideoElement.Type.RemoteVideo, LayoutModel.RemoteVideo.HorizontalTranslateValue, LayoutModel.RemoteVideo.VerticalTranslateValue);

            RbWebRtcControl.SetSizeVideoElement(VideoElement.Type.RemoteVideo, LayoutModel.RemoteVideo.WidthValue, LayoutModel.RemoteVideo.HeightValue);

            if (LayoutModel.RemoteVideo.BottomPositionUsed)
            {
                if (LayoutModel.RemoteVideo.LeftPositionUsed)
                {
                    RbWebRtcControl.SetBottomLeftVideoElement(VideoElement.Type.RemoteVideo, LayoutModel.RemoteVideo.VerticalValue, LayoutModel.RemoteVideo.HorizontalValue);
                }
                else
                {
                    RbWebRtcControl.SetBottomRightVideoElement(VideoElement.Type.RemoteVideo, LayoutModel.RemoteVideo.VerticalValue, LayoutModel.RemoteVideo.HorizontalValue);
                }
            }
            else
            {
                if (LayoutModel.RemoteVideo.LeftPositionUsed)
                {
                    RbWebRtcControl.SetTopLeftVideoElement(VideoElement.Type.RemoteVideo, LayoutModel.RemoteVideo.VerticalValue, LayoutModel.RemoteVideo.HorizontalValue);
                }
                else
                {
                    RbWebRtcControl.SetTopRightVideoElement(VideoElement.Type.RemoteVideo, LayoutModel.RemoteVideo.VerticalValue, LayoutModel.RemoteVideo.HorizontalValue);
                }
            }
        }

        public void SetLocalVideoCommand(object obj)
        {
            RbWebRtcControl.HideVideoElement(VideoElement.Type.LocalVideo, LayoutModel.LocalVideo.Hidden);

            RbWebRtcControl.SetTranslateVideoElement(VideoElement.Type.LocalVideo, LayoutModel.LocalVideo.HorizontalTranslateValue, LayoutModel.LocalVideo.VerticalTranslateValue);

            RbWebRtcControl.SetSizeVideoElement(VideoElement.Type.LocalVideo, LayoutModel.LocalVideo.WidthValue, LayoutModel.LocalVideo.HeightValue);

            if (LayoutModel.LocalVideo.BottomPositionUsed)
            {
                if (LayoutModel.LocalVideo.LeftPositionUsed)
                {
                    RbWebRtcControl.SetBottomLeftVideoElement(VideoElement.Type.LocalVideo, LayoutModel.LocalVideo.VerticalValue, LayoutModel.LocalVideo.HorizontalValue);
                }
                else
                {
                    RbWebRtcControl.SetBottomRightVideoElement(VideoElement.Type.LocalVideo, LayoutModel.LocalVideo.VerticalValue, LayoutModel.LocalVideo.HorizontalValue);
                }
            }
            else
            {
                if (LayoutModel.LocalVideo.LeftPositionUsed)
                {
                    RbWebRtcControl.SetTopLeftVideoElement(VideoElement.Type.LocalVideo, LayoutModel.LocalVideo.VerticalValue, LayoutModel.LocalVideo.HorizontalValue);
                }
                else
                {
                    RbWebRtcControl.SetTopRightVideoElement(VideoElement.Type.LocalVideo, LayoutModel.LocalVideo.VerticalValue, LayoutModel.LocalVideo.HorizontalValue);
                }
            }
        }

        public void SwitchPictureInPictureModeOnSharingCommand(object obj)
        {
            RbWebRtcControl.SwitchPictureInPictureVideoElement(VideoElement.Type.SharingVideo);
        }

        public void SwitchPictureInPictureModeOnRemoteVideoCommand(object obj)
        {
            RbWebRtcControl.SwitchPictureInPictureVideoElement(VideoElement.Type.RemoteVideo);
        }

        public void SwitchPictureInPictureModeOnLocalVideoCommand(object obj)
        {
            RbWebRtcControl.SwitchPictureInPictureVideoElement(VideoElement.Type.LocalVideo);
        }

        public void AddAudioCommand(object obj)
        {
            RbWebRtcControl.AddLocalAudioToCall(currentCall.Id);
        }

        public void AddRemoveVideoCommand(object obj)
        {
            if (RbWebRtcControl.CallUsingLocalVideo())
                RbWebRtcControl.RemoveLocalVideoFromCall(currentCall.Id);
            else
                RbWebRtcControl.AddLocalVideoToCall(currentCall.Id);
        }

        public void AddRemoveSharingCommand(object obj)
        {
            if(RbWebRtcControl.CallUsingLocalSharing())
                RbWebRtcControl.RemoveLocalSharingFromCall(currentCall.Id);
            else
                RbWebRtcControl.AddLocalSharingToCall(currentCall.Id);
        }

        public void MuteUnmuteAudioCommand(object obj)
        {
            RbWebRtcControl.MuteLocalAudio(currentCall.Id, !ConversationModel.LocalAudioMuted);
        }

        public void MuteUnmuteVideoCommand(object obj)
        {
            RbWebRtcControl.MuteLocalVideo(currentCall.Id, !ConversationModel.LocalVideoMuted);
        }

        public void MuteUnmuteSharingCommand(object obj)
        {
            RbWebRtcControl.MuteLocalSharing(currentCall.Id, !ConversationModel.LocalSharingMuted);
        }

        public void SetConstraintCommand(object obj)
        {
            int media = ConstraintModel.VideoConstraint ? Call.Media.VIDEO : Call.Media.SHARING;

            Dictionary<String, String> constraints = new Dictionary<string, string>();
            constraints.Add("maxFrameRate", ConstraintModel.MaxFrameRateValue);
            constraints.Add("width", ConstraintModel.WidthValue);
            constraints.Add("height", ConstraintModel.HeightValue);

            RbWebRtcControl.SetConstraints(media, constraints, false, callback =>
            {
                if (!callback.Result.Success)
                    log.LogWarning($"[SetConstraintCommand] Cannot set this constraints: maxFrameRate:[{ConstraintModel.MaxFrameRateValue}] - width:[{ConstraintModel.WidthValue}] - height:[{ConstraintModel.HeightValue}]");
            });
        }

        public void SetLanguageCommand(object obj)
        {
            String currentLanguage  = rbLanguages.GetLanguageId();

            string newLanguage = LanguagesModel.LanguageSelected;

            if (rbLanguages.SetLanguageId(newLanguage))
                RbWebRtcControl.SetLabels(rbLanguages.GetLabels());
            else
                LanguagesModel.LanguageSelected = currentLanguage;
        }

#endregion BUTTONS - Command

#region BUTTONS - CanExecute

        public Boolean RejectCommandCanExecute(object obj)
        {
            return ConversationModel.CanAnswerCall;
        }

        public Boolean AnswerCommandCanExecute(object obj)
        {
            return ConversationModel.DevicesSelected && ConversationModel.CanAnswerCall;
        }

        public Boolean HangUpCommandCanExecute(object obj)
        {
            return ConversationModel.CanHangup;
        }

        public Boolean CallCommandCanExecute(object obj)
        {
            return ConversationModel.DevicesSelected && !ConversationModel.CallInProgress;
        }

        public Boolean SelectDevicesCommandCanExecute(object obj)
        {
            return LoginInfoModel.InitialisationCompleted && ConversationModel.DevicesEnumerated;
        }

        public Boolean LoginOrLogoutCommandCanExecute(object obj)
        {
            return LoginInfoModel.CanUseLoginLogoutButton;
        }

        public Boolean AddAudioCommandCanExecute(object obj)
        {
            return ConversationModel.DevicesSelected && ConversationModel.CallActive && !ConversationModel.LocalUsingAudio;
        }

        public Boolean AddRemoveVideoCommandCanExecute(object obj)
        {
            return ConversationModel.DevicesSelected && ConversationModel.CallActive;
        }

        public Boolean AddRemoveSharingCommandCanExecute(object obj)
        {
            return ConversationModel.DevicesSelected && ConversationModel.CallActive && !ConversationModel.RemoteUsingSharing;
        }

        public Boolean MuteUnmuteAudioCommandCanExecute(object obj)
        {
            return ConversationModel.DevicesSelected && ConversationModel.CallActive && ConversationModel.LocalUsingAudio;
        }

        public Boolean MuteUnmuteVideoCommandCanExecute(object obj)
        {
            return ConversationModel.DevicesSelected && ConversationModel.CallActive && ConversationModel.LocalUsingVideo;
        }

        public Boolean MuteUnmuteSharingCommandCanExecute(object obj)
        {
            return ConversationModel.DevicesSelected && ConversationModel.CallActive && ConversationModel.LocalUsingSharing;
        }

        public Boolean SetConstraintCommandCanExecute(object obj)
        {
            return ConversationModel.WebRtcInitialized;
        }

#endregion BUTTONS - CanExecute
    }
}
