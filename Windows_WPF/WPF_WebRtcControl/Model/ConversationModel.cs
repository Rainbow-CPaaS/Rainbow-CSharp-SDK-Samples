using System;
using System.Windows.Input;

namespace SDK.WpfApp.Model
{
    public class ConversationModel : ObservableObject
    {
        Boolean m_webRtcInitialized;

        Boolean m_callInProgress;
        Boolean m_callRinging;
        Boolean m_callIncoming;

        Boolean m_callActive; // Computed from others properties - CallInProgress && !CallRinging
        Boolean m_canAnswerCall; // Computed from others properties
        Boolean m_canHangUp; // Computed from others properties

        String m_callId;

        Boolean m_devicesChanged;
        Boolean m_devicesSelected;

        Boolean m_localInputAudioSelected;
        Boolean m_localInputVideoSelected;

        Boolean m_makeCallUsingAudio;
        Boolean m_makeCallUsingVideo;
        Boolean m_makeCallUsingSharing;

        Boolean m_answerUsingAudio;
        Boolean m_answerUsingVideo;
        Boolean m_answerUsingSharing;
        Boolean m_canAnswerUsingAudio;
        Boolean m_canAnswerUsingVideo;

        Boolean m_localUsingAudio;
        Boolean m_localUsingVideo;
        Boolean m_localUsingSharing;

        Boolean m_localAudioMuted;
        Boolean m_localVideoMuted;
        Boolean m_localSharingMuted;

        Boolean m_remoteUsingAudio;
        Boolean m_remoteUsingVideo;
        Boolean m_remoteUsingSharing;

        ICommand m_buttonSelectDevicesCommand;

        ICommand m_buttonCallCommand;
        ICommand m_buttonAnswerCommand;
        ICommand m_buttonHangUpCommand;
        ICommand m_buttonRejectCommand;
        
        ICommand m_buttonMuteUnmuteAudioCommand;
        ICommand m_buttonMuteUnmuteVideoCommand;
        ICommand m_buttonMuteUnmuteSharingCommand;

        ICommand m_buttonAddAudioCommand;
        ICommand m_buttonAddRemoveVideoCommand;
        ICommand m_buttonAddRemoveSharingCommand;

        /// <summary>
        /// WebRtcInitialized - To kno if WebRTC has been initialized
        /// </summary>
        public Boolean WebRtcInitialized
        {
            get { return m_webRtcInitialized; }
            internal set { SetProperty(ref m_webRtcInitialized, value); }
        }

        /// <summary>
        /// CallId - Call Id
        /// </summary>
        public String CallId
        {
            get { return m_callId; }
            internal set { SetProperty(ref m_callId, value); }
        }

        /// <summary>
        /// CanAnswerCall - to know if we can answer a call (Computed from others properties)
        /// </summary>
        public Boolean CanAnswerCall
        {
            get { return m_canAnswerCall; }
            private set { SetProperty(ref m_canAnswerCall, value); }
        }

        /// <summary>
        /// CanHangup - to know if we can hangup a call (Computed from others properties)
        /// </summary>
        public Boolean CanHangup
        {
            get { return m_canHangUp; }
            private set { SetProperty(ref m_canHangUp, value); }
        }

        /// <summary>
        /// CallInProgress - to know if a call is in progress
        /// </summary>
        public Boolean CallInProgress
        {
            get { return m_callInProgress; }
            set
            {
                SetProperty(ref m_callInProgress, value);
                CheckCanAnswerCall();
                CheckCanHangup();
                CheckCallActive();
            }
        }

        /// <summary>
        /// CallRinging - A call in progress is in ringing state
        /// </summary>
        public Boolean CallRinging
        {
            get { return m_callRinging; }
            set
            {
                SetProperty(ref m_callRinging, value);
                CheckCanAnswerCall();
                CheckCanHangup();
                CheckCallActive();
            }
        }

        /// <summary>
        /// CallIncoming - Is an incoming call or an outgoing call ?
        /// </summary>
        public Boolean CallIncoming
        {
            get { return m_callIncoming; }
            set
            {
                SetProperty(ref m_callIncoming, value);
                CheckCanAnswerCall();
                CheckCanHangup();
            }
        }

        public Boolean CallActive
        {
            get { return m_callActive; }
            private set { SetProperty(ref m_callActive, value); }
        }

        /// <summary>
        /// To know that devices has been selected
        /// </summary>
        public Boolean DevicesSelected
        {
            get { return m_devicesSelected; }
            set { SetProperty(ref m_devicesSelected, value); }
        }

        /// <summary>
        /// To know when we received once a list of devices availables
        /// </summary>
        public Boolean DevicesEnumerated
        {
            get { return m_devicesChanged; }
            set { SetProperty(ref m_devicesChanged, value); }
        }

        /// <summary>
        /// LocalInputAudioAvailable - Do we have choose to use local input audio in the conversation ?
        /// </summary>
        public Boolean LocalInputAudioSelected
        {
            get { return m_localInputAudioSelected; }
            set
            {
                SetProperty(ref m_localInputAudioSelected, value);
            }
        }

        /// <summary>
        /// LocalInputVideoSelected - Do we have choose to use local input video in the conversation ?
        /// </summary>
        public Boolean LocalInputVideoSelected
        {
            get { return m_localInputVideoSelected; }
            set
            {
                SetProperty(ref m_localInputVideoSelected, value);
            }
        }

        /// <summary>
        /// Do we want to use audio to make the call ?
        /// </summary>
        public Boolean MakeCallUsingAudio
        {
            get { return m_makeCallUsingAudio; }
            set
            {
                SetProperty(ref m_makeCallUsingAudio, value);
                if (!value)
                    MakeCallUsingVideo = false;
            }
        }

        /// <summary>
        /// Do we want to use video to make the call ?
        /// </summary>
        public Boolean MakeCallUsingVideo
        {
            get { return m_makeCallUsingVideo; }
            set
            {
                SetProperty(ref m_makeCallUsingVideo, value);
                if (value)
                    MakeCallUsingAudio = true;
            }
        }

        /// <summary>
        /// Do we want to use sharing to make the call ?
        /// </summary>
        public Boolean MakeCallUsingSharing
        {
            get { return m_makeCallUsingSharing; }
            set
            {
                SetProperty(ref m_makeCallUsingSharing, value);
                if (value)
                {
                    MakeCallUsingAudio = false;
                    MakeCallUsingVideo = false;
                }
            }
        }

        /// <summary>
        /// Do we want to use audio to answer the call ?
        /// </summary>
        public Boolean AnswerUsingAudio
        {
            get { return m_answerUsingAudio; }
            set
            {
                SetProperty(ref m_answerUsingAudio, value);
                if (!value)
                    AnswerUsingVideo = false;
            }
        }

        /// <summary>
        /// Do we want to use video to answer the call ?
        /// </summary>
        public Boolean AnswerUsingVideo
        {
            get { return m_answerUsingVideo; }
            set
            {
                SetProperty(ref m_answerUsingVideo, value);
                if (value)
                    AnswerUsingAudio = true;
            }
        }

        /// <summary>
        /// Do we want to use Sharing to answer the call ?
        /// </summary>
        public Boolean AnswerUsingSharing
        {
            get { return m_answerUsingSharing; }
            set { SetProperty(ref m_answerUsingSharing, value); }
        }

        /// <summary>
        /// Can we answer to the call in audio ?
        /// </summary>
        public Boolean CanAnswerUsingAudio
        {
            get { return m_canAnswerUsingAudio; }
            set { SetProperty(ref m_canAnswerUsingAudio, value);  }
        }

        /// <summary>
        /// Can we answer to the call in video ?
        /// </summary>
        public Boolean CanAnswerUsingVideo
        {
            get { return m_canAnswerUsingVideo; }
            set { SetProperty(ref m_canAnswerUsingVideo, value); }
        }

        /// <summary>
        /// Does the local use audio in the conversation ?
        /// </summary>
        public Boolean LocalUsingAudio
        {
            get { return m_localUsingAudio; }
            set
            {
                SetProperty(ref m_localUsingAudio, value);
            }
        }

        /// <summary>
        /// Does the local use video in the conversation ?
        /// </summary>
        public Boolean LocalUsingVideo
        {
            get { return m_localUsingVideo; }
            set
            {
                SetProperty(ref m_localUsingVideo, value);
            }
        }

        /// <summary>
        /// Does the local use sharing in the conversation ?
        /// </summary>
        public Boolean LocalUsingSharing
        {
            get { return m_localUsingSharing; }
            set
            {
                SetProperty(ref m_localUsingSharing, value);
            }
        }

        /// <summary>
        /// Does the local audio is muted ?
        /// </summary>
        public Boolean LocalAudioMuted
        {
            get { return m_localAudioMuted; }
            set { SetProperty(ref m_localAudioMuted, value); }
        }

        /// <summary>
        /// Does the local video is muted ?
        /// </summary>
        public Boolean LocalVideoMuted
        {
            get { return m_localVideoMuted; }
            set { SetProperty(ref m_localVideoMuted, value); }
        }

        /// <summary>
        /// Does the local Sharing is muted ?
        /// </summary>
        public Boolean LocalSharingMuted
        {
            get { return m_localSharingMuted; }
            set { SetProperty(ref m_localSharingMuted, value); }
        }

        /// <summary>
        /// Does the remote use audio in the conversation ?
        /// </summary>
        public Boolean RemoteUsingAudio
        {
            get { return m_remoteUsingAudio; }
            set
            {
                SetProperty(ref m_remoteUsingAudio, value);
            }
        }

        /// <summary>
        /// Does the remote use video in the conversation ?
        /// </summary>
        public Boolean RemoteUsingVideo
        {
            get { return m_remoteUsingVideo; }
            set
            {
                SetProperty(ref m_remoteUsingVideo, value);
            }
        }

        /// <summary>
        /// Does the remote use sharing in the conversation ?
        /// </summary>
        public Boolean RemoteUsingSharing
        {
            get { return m_remoteUsingSharing; }
            set
            {
                SetProperty(ref m_remoteUsingSharing, value);
            }
        }

        /// <summary>
        /// To manage action on the Call button
        /// </summary>
        public ICommand ButtonCallCommand
        {
            get { return m_buttonCallCommand; }
            set { m_buttonCallCommand = value; }
        }

        /// <summary>
        /// To manage action on the Answer button
        /// </summary>
        public ICommand ButtonAnswerCommand
        {
            get { return m_buttonAnswerCommand; }
            set { m_buttonAnswerCommand = value; }
        }

        /// <summary>
        /// To manage action on the HangUp button
        /// </summary>
        public ICommand ButtonHangUpCommand
        {
            get { return m_buttonHangUpCommand; }
            set { m_buttonHangUpCommand = value; }
        }

        /// <summary>
        /// To manage action on the Reject button
        /// </summary>
        public ICommand ButtonRejectCommand
        {
            get { return m_buttonRejectCommand; }
            set { m_buttonRejectCommand = value; }
        }

        /// <summary>
        /// To manage action on the select devices button
        /// </summary>
        public ICommand ButtonSelectDevicesCommand
        {
            get { return m_buttonSelectDevicesCommand; }
            set { m_buttonSelectDevicesCommand = value; }
        }

        /// <summary>
        /// To add audio media in the current conversation
        /// </summary>
        public ICommand ButtonAddAudioCommand
        {
            get { return m_buttonAddAudioCommand; }
            set { m_buttonAddAudioCommand = value; }
        }

        /// <summary>
        /// To add / remove video media in the current conversation
        /// </summary>
        public ICommand ButtonAddRemoveVideoCommand
        {
            get { return m_buttonAddRemoveVideoCommand; }
            set { m_buttonAddRemoveVideoCommand = value; }
        }

        /// <summary>
        /// To add / remove sharing media in the current conversation
        /// </summary>
        public ICommand ButtonAddRemoveSharingCommand
        {
            get { return m_buttonAddRemoveSharingCommand; }
            set { m_buttonAddRemoveSharingCommand = value; }
        }

        /// <summary>
        /// To Mute / Umute Local Audio
        /// </summary>
        public ICommand ButtonMuteUnmuteAudioCommand
        {
            get { return m_buttonMuteUnmuteAudioCommand; }
            set { m_buttonMuteUnmuteAudioCommand = value; }
            
        }

        /// <summary>
        /// To Mute / Umute Local Video
        /// </summary>
        public ICommand ButtonMuteUnmuteVideoCommand
        {
            get { return m_buttonMuteUnmuteVideoCommand; }
            set { m_buttonMuteUnmuteVideoCommand = value; }
        }

        /// <summary>
        /// To Mute / Umute Local Sharing
        /// </summary>
        public ICommand ButtonMuteUnmuteSharingCommand
        {
            get { return m_buttonMuteUnmuteSharingCommand; }
            set { m_buttonMuteUnmuteSharingCommand = value; }
        }

        private void CheckCanAnswerCall()
        {
            CanAnswerCall = WebRtcInitialized
                            && CallInProgress && CallIncoming && CallRinging;
        }

        private void CheckCanHangup()
        {
            CanHangup = WebRtcInitialized && (
                    (CallInProgress && (!CallIncoming) && CallRinging)
                        || (CallInProgress && (!CallRinging)));
        }

        private void CheckCallActive()
        {
            CallActive = CallInProgress && !CallRinging;
        }

        public ConversationModel()
        {
            CallInProgress = CallIncoming = CallRinging = false;

            LocalInputAudioSelected = LocalInputVideoSelected = false;

            RemoteUsingAudio = RemoteUsingVideo = RemoteUsingSharing = false;

            AnswerUsingAudio = AnswerUsingVideo = AnswerUsingSharing = false;
        }
    }
}
