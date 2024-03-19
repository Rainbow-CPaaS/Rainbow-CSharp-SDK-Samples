using Rainbow;
using Rainbow.Model;
using Rainbow.Medias;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rainbow.WebRTC.Abstractions;
using Rainbow.WebRTC;
using Rainbow.WebRTC.Desktop;

namespace SDK.UIForm.WebRTC
{
    public partial class FormWebRTC : Form
    {
        private static String NONE = "NONE";
        private static String AUDIO = "Audio";
        private static String VIDEO = "Video";
        private static String SHARING = "Sharing";
        private static String DATACHANNEL = "DataChannel";
        private static String MUTED = "Muted";

        private Rainbow.WebRTC.Desktop.WebRTCFactory rbWebRTCDesktopFactory;

        private MediaInputStreamsManager _mediaInputStreamsManager;
        private MediaAudioDeviceManager _mediaAudioDeviceManager;

        private FormConferenceOptions? _formConferenceOptions = null;

        // DEFINE SDK OBJECTS
        Rainbow.Application? rbAplication;
        Rainbow.Contacts? rbContacts;
        Rainbow.Conferences? rbConferences;
        Rainbow.Conversations? rbConversations;
        Rainbow.Bubbles? rbBubbles;
        Rainbow.WebRTC.WebRTCCommunications? rbWebRTCCommunications;

        private Call? currentCall = null;
        private String? currentCallId = null;
        private List<String> conferencesInProgress = new List<string>();

        private String? currentContactId;   // The contact Id using the SDK
        private String? currentContactJid;  // The contact Jid using the SDK
        private String? selectedContactId;  // The contact Id selected in the Form
        private String? selectedBubbleId;   // The bubble Id selected in the Form

        private List<Bubble>? bubblesList = null;

        private List<MediaStreamTrackDescriptor> mediaStreamTrackDescriptors = new();
        private Dictionary<String, Rainbow.WebRTC.DataChannel> dataChannels = new();

        private String _ffmpegLibPath;

        private AudioStreamTrack? currentAudioStreamTrack = null;
        private SDL2AudioOutput? currentAudioOutputDevice = null;

        #region CONSTRUCTOR

        public FormWebRTC()
        {
            InitializeComponent();
        }

        #endregion CONSTRUCTOR

        public void Initialize(Rainbow.Application application, String ffmpegLibPath)
        {
            rbAplication = application;
            _ffmpegLibPath = ffmpegLibPath;

            Rainbow.Medias.Helper.InitExternalLibraries(_ffmpegLibPath);

            this.HandleCreated += Form_HandleCreated;
        }

        private void Form_HandleCreated(object sender, EventArgs e)
        {
            // Init Images on buttons
            btn_StartVideoInput.BackgroundImage = Helper.GetBitmapPlay();
            btn_StopVideoInput.BackgroundImage = Helper.GetBitmapStop();
            btn_OutputVideoInput.BackgroundImage = Helper.GetBitmapOutput();

            btn_StartSharingInput.BackgroundImage = Helper.GetBitmapPlay();
            btn_StopSharingInput.BackgroundImage = Helper.GetBitmapStop();
            btn_OutputSharingInput.BackgroundImage = Helper.GetBitmapOutput();

            btn_StartAudioInput.BackgroundImage = Helper.GetBitmapPlay();
            btn_StopAudioInput.BackgroundImage = Helper.GetBitmapStop();
            btn_RefreshAudioInput.BackgroundImage = Helper.GetBitmapRefresh();

            btn_RefreshAudioOutput.BackgroundImage = Helper.GetBitmapRefresh();

            btn_OutputLocalVideoInput.BackgroundImage = Helper.GetBitmapOutput();
            btn_OutputLocalSharingInput.BackgroundImage = Helper.GetBitmapOutput();

            btn_OutputRemoteVideoInput.BackgroundImage = Helper.GetBitmapOutput();
            btn_OutputRemoteSharingInput.BackgroundImage = Helper.GetBitmapOutput();

            // Create MediaInputStreamsManager and init necessary events
            _mediaInputStreamsManager = MediaInputStreamsManager.Instance;
            _mediaInputStreamsManager.OnListUpdated += MediaInputStreamsManager_ListUpdated;
            _mediaInputStreamsManager.OnMediaStreamStateChanged += MediaInputStreamsManager_OnMediaStreamStateChanged;

            // Create MediaInputStreamsManager and init necessary events
            _mediaAudioDeviceManager = MediaAudioDeviceManager.Instance;
            _mediaAudioDeviceManager.OnAudioOutputDeviceChanged += MediaAudioDeviceManager_OnAudioOutputDeviceChanged;
            _mediaAudioDeviceManager.OnAudioInputDeviceChanged += MediaAudioDeviceManager_OnAudioInputDeviceChanged;

            rbWebRTCDesktopFactory = new Rainbow.WebRTC.Desktop.WebRTCFactory();

            FillMainComboBoxWithMediaInputStream();
            // By default a Media Stream if available
            if (cb_VideoInputs.Items.Count > 1)
                cb_VideoInputs.SelectedIndex = 1;

            if (cb_SharingInputs.Items.Count > 2)
                cb_SharingInputs.SelectedIndex = 2;
            else if (cb_SharingInputs.Items.Count > 1)
                cb_SharingInputs.SelectedIndex = 1;

            FillAudioInputComboBox();
            FillAudioOutputComboBox();

            // Get Rainbow SDK objects
            rbBubbles = rbAplication.GetBubbles();
            rbContacts = rbAplication.GetContacts();
            rbConferences = rbAplication.GetConferences();
            rbConversations = rbAplication.GetConversations();

            // Get Id/Jid of the current user
            currentContactId = rbContacts.GetCurrentContactId();
            currentContactJid = rbContacts.GetCurrentContactJid();

            rbWebRTCCommunications = Rainbow.WebRTC.WebRTCCommunications.GetOrCreateInstance(rbAplication, rbWebRTCDesktopFactory);

            // Events related to Rainbow SDK Objects
            rbWebRTCCommunications.CallUpdated += WebRTCCommunications_CallUpdated;
            rbWebRTCCommunications.OnTrack += RbWebRTCCommunications_OnTrack;
            rbWebRTCCommunications.OnDataChannel += RbWebRTCCommunications_OnDataChannel;

            //TODO - Manage RemoteAudio
            //rbWebRTCCommunications.OnRemoteAudio += WebRTCCommunications_OnRemoteAudio;
            rbWebRTCCommunications.OnMediaPublicationUpdated += WebRTCCommunications_OnMediaPublicationUpdated;
            rbWebRTCCommunications.OnMediaServiceUpdated += RbWebRTCCommunications_OnMediaServiceUpdated;

            rbConferences.ConferenceUpdated += Conferences_ConferenceUpdated;
            rbConferences.ConferenceRemoved += Conferences_ConferenceRemoved;
            rbConferences.ConferenceSharingTransferStatusUpdated += Conferences_ConferenceSharingTranferStatusUpdated;

            rbAplication.InitializationPerformed += RbAplication_InitializationPerformed;

            // Fill users Combo Box
            UpdateContactsComboBox();

            AskAllBubbles();

            // Update elements based on Call info/status
            UpdateUIAccordingCall();

            if (_formConferenceOptions == null)
            {
                _formConferenceOptions = new FormConferenceOptions();
                _formConferenceOptions.Initialize(rbAplication, true);

                _formConferenceOptions.OnMediaPublicationSubscription += FormConferenceOptions_OnMediaPublicationSubscription;
                _formConferenceOptions.OnMediaServiceSubscription += FormConferenceOptions_OnMediaServiceSubscription;
                _formConferenceOptions.OnCloseDataChannel += FormConferenceOptions_OnCloseDataChannel;

                _formConferenceOptions.OpenVideoPublication += FormConferenceOptions_OpenVideoPublication;
                _formConferenceOptions.OpenMediaService += FormConferenceOptions_OpenMediaService;
            }
        }

        private void AskAllBubbles()
        {
            if (rbBubbles != null)
            {
                rbBubbles.GetAllBubbles(callback =>
                {

                    if (callback.Result.Success)
                    {
                        bubblesList = callback.Data;
                        UpdateBubblesComboBox();
                    }
                    else
                    {
                        AddInformationMessage("CANNOT GET LIST OF BUBBLES !");
                    }
                });
            }
        }

        private void RbAplication_InitializationPerformed(object? sender, EventArgs e)
        {
            AskAllBubbles();
        }

        private Boolean CanStartConferenceFromBubbleId(String bubbleId)
        {
            if (rbBubbles == null)
                return false;

            // It's possible to start conference if user is the owner or a moderator of this bubble
            Boolean? result = null;

            var bubble = rbBubbles.GetBubbleByIdFromCache(bubbleId);
            if (bubble != null)
                result = rbBubbles.IsModerator(bubble);

            if (result == null)
                return false;

            return result.Value;
        }

        private String GetDisplayName(String? participantId, String? participantJid, String? participantPhoneNumber = null)
        {
            if (rbAplication == null)
                return "";

            if (rbContacts == null)
                return "";

            String result = "";

            if (!String.IsNullOrEmpty(participantJid))
            {
                if (participantJid == currentContactJid)
                    result = "MYSELF";
                else
                {
                    var contact = rbContacts.GetContactFromContactJid(participantJid);
                    if (contact != null)
                        result = Rainbow.Util.GetContactDisplayName(contact);
                    else
                        rbContacts.GetContactFromContactJidFromServer(participantJid); // We ask server here to finally have contacts information
                }
            }
            else if (!String.IsNullOrEmpty(participantId))
            {
                if (participantId == currentContactId)
                    result = "MYSELF";
                else
                {
                    var contact = rbContacts.GetContactFromContactId(participantId);
                    if (contact != null)
                        result = Rainbow.Util.GetContactDisplayName(contact);
                    else
                        rbContacts.GetContactFromContactIdFromServer(participantId); // We ask server here to finally have contacts information
                }
            }
            else if (!String.IsNullOrEmpty(participantPhoneNumber))
            {
                result = participantPhoneNumber;
            }

            if (String.IsNullOrEmpty(result) && !String.IsNullOrEmpty(participantId))
                result = participantId;

            return result;
        }

        private String MediasToString(int medias)
        {
            if (medias == Call.Media.NONE)
                return NONE;

            String result = "";
            if (Util.MediasWithAudio(medias))
                result += AUDIO + "+";

            if (Util.MediasWithVideo(medias))
                result += VIDEO + "+";

            if (Util.MediasWithSharing(medias))
                result += SHARING + "+";

            if (Util.MediasWithDataChannel(medias))
                result += DATACHANNEL + "+";

            if (result == "")
            {
                AddInformationMessage($"Medias specified {medias} cannot be set to String");
                return "";
            }
            return result[..^1];
        }

        private int MediasToInt(string? medias)
        {
            if (medias == null)
                return 0;

            if (medias == NONE)
                return Call.Media.NONE;

            if (medias == AUDIO)
                return Call.Media.AUDIO;

            if (medias == $"{AUDIO}+{VIDEO}")
                return Call.Media.AUDIO + Call.Media.VIDEO;

            if (medias == $"{AUDIO}+{SHARING}")
                return Call.Media.AUDIO + Call.Media.SHARING;

            if (medias == VIDEO)
                return Call.Media.VIDEO;

            if (medias == SHARING)
                return Call.Media.SHARING;

            if (medias == DATACHANNEL)
                return Call.Media.DATACHANNEL;

            if (medias == $"{VIDEO}+{SHARING}")
                return Call.Media.VIDEO + Call.Media.SHARING;

            if (medias == $"{AUDIO}+{VIDEO}+{SHARING}")
                return Call.Media.AUDIO + Call.Media.VIDEO + Call.Media.SHARING;

            AddInformationMessage($"Medias specified {medias} cannot be set to Int");
            return 0;
        }

        private String? GetSelectedIdFromComboBox(ComboBox comboBox)
        {
            ListItem? item = comboBox.SelectedItem as ListItem;
            if (item != null)
                return item.Value;
            return null;
        }

        #region METHODS TO UPDATE UI ELEMENTS OF THE FORM

        // To update the UI interface when the Call is updated
        private void UpdateUIAccordingCall()
        {
            var action = new Action(() =>
            {
                UpdateConversationDetails();

                UpdateConferenceOptions();

                UpdateMakeCallComboBox();
                UpdateAnswerCallComboBox();
                UpdateBubblesComboBox();

                UpdateHangUpAndDeclineButtons();

                UpdateAddMediaComboBox();
                UpdateRemoveMediaComboBox();

                UpdateMuteUnmuteComboBox();

                UpdateLocalAndRemoteMediasCheckbox();
            });

            if (InvokeRequired)
                BeginInvoke(action);
            else
                action.Invoke();
            // TODO - Update other UI elements
        }

        // Update Contacts Combo Box
        private void UpdateContactsComboBox()
        {
            if (rbContacts == null)
                return;

            this.BeginInvoke(new Action(() =>
            {
                var listContacts = rbContacts.GetAllContactsFromCache();

                cb_ContactsList.Items.Clear();

                int index = 0;

                if (listContacts?.Count > 0)
                {
                    cb_ContactsList.Enabled = true;

                    foreach (var contact in listContacts)
                    {
                        if (contact.Id != currentContactId)
                        {
                            ListItem item = new ListItem(Rainbow.Util.GetContactDisplayName(contact), contact.Id);
                            cb_ContactsList.Items.Add(item);

                            if (contact.Id == selectedContactId)
                                cb_ContactsList.SelectedIndex = index;

                            index++;
                        }
                    }

                    if ((cb_ContactsList.SelectedIndex == -1) && (cb_ContactsList.Items?.Count > 0))
                        cb_ContactsList.SelectedIndex = 0;
                }
                else
                {
                    cb_ContactsList.Enabled = false;
                    cb_ContactsList.Items.Add(NONE);
                    selectedContactId = null;
                }

            }));
        }

        // Update Bubbles ComboBox, Start Conf., Join Conf. and Snapshot Conf.
        private void UpdateBubblesComboBox()
        {
            if (bubblesList == null)
                return;

            if (rbBubbles == null)
                return;

            this.BeginInvoke(new Action(() =>
            {
                // Do we need to fill the combo box ?
                if (cb_BubblesList.Items.Count == 0)
                {
                    foreach (var bubble in bubblesList)
                    {
                        ListItem item = new ListItem(bubble.Name, bubble.Id);
                        cb_BubblesList.Items.Add(item);
                    }

                    if (cb_BubblesList.Items?.Count > 0)
                        cb_BubblesList.SelectedIndex = 0;
                }

                // Display name of the Conference / Bubble in progress
                lock (conferencesInProgress)
                {
                    lbl_ConferencesInProgress.Visible = false;
                    cb_ConferencesName.Items.Clear();

                    if (conferencesInProgress.Count > 0)
                    {
                        if (currentCall == null)
                            lbl_ConferencesInProgress.Visible = true;

                        foreach (var confId in conferencesInProgress)
                        {
                            var bubble = rbBubbles.GetBubbleByIdFromCache(confId);
                            var item = new ListItem(bubble.Name + " " + bubble.Topic, confId);
                            cb_ConferencesName.Items.Add(item);
                        }

                        if ((cb_ConferencesName.SelectedIndex < 0) && (cb_ConferencesName.Items.Count > 0))
                            cb_ConferencesName.SelectedIndex = 0;
                    }
                }

                // Enable / Disable buttons: Start conf. / Join conf.
                if (currentCallId != null)
                {
                    btn_JoinConf.Enabled = false;   // We can't join a conf if we are already in a call
                    btn_StartConf.Enabled = false;  // We can't start a conf if we are already in a call

                    btn_DeclineConf.Enabled = false;// We can't declient a conf if we are already in a call
                }
                else
                {
                    btn_JoinConf.Enabled = conferencesInProgress.Count > 0; // We can join only if a conf is in progress
                    btn_DeclineConf.Enabled = conferencesInProgress.Count > 0; // We can declinet only if a conf is in progress

                    if (selectedBubbleId != null)
                    {
                        var couldStart = CanStartConferenceFromBubbleId(selectedBubbleId);
                        if (couldStart)
                            lbl_BubbleInfo.Text = "Owner or Moderator";
                        else
                            lbl_BubbleInfo.Text = "Member";

                        // If a conf in in progress and it's the bubble selected we cannot start a conference)
                        if (conferencesInProgress.Contains(selectedBubbleId))
                            btn_StartConf.Enabled = false;
                        else
                            btn_StartConf.Enabled = couldStart;
                    }
                    else
                        btn_StartConf.Enabled = false;
                }
            }));
        }

        private void UpdateConversationDetails()
        {
            this.BeginInvoke(new Action(() =>
            {
                String txt = "";
                Boolean inProgress = false;

                if ((currentCall != null) && currentCall.IsInProgress() && !currentCall.IsRinging())
                {
                    inProgress = true;

                    if (currentCall.IsConference)
                    {
                        txt = "In Conference in Bubble: ";
                        var bubble = rbBubbles.GetBubbleByIdFromCache(currentCall.ConferenceId);
                        if (bubble != null)
                            txt += bubble.Name;
                        else
                            txt += currentCall.ConferenceId;
                    }
                    else
                    {
                        txt = "In a P2P call with: ";
                        if (currentCall.Participants?.Count > 0)
                        {
                            var participant = currentCall.Participants[0];
                            var displayName = GetDisplayName(participant.UserId, null);
                            if (String.IsNullOrEmpty(displayName))
                                txt += currentCall.ConversationId;
                            else
                                txt += displayName;
                        }
                    }
                }

                lbl_ConversationInProgress.Visible = inProgress;
                lbl_ConversationDetails.Text = txt;
            }));
        }

        // Update Conference options  button
        private void UpdateConferenceOptions()
        {
            if (rbAplication == null)
                return;

            this.BeginInvoke(new Action(() =>
            {
                Boolean enabled = (conferencesInProgress.Count > 0);
                btn_ConferenceOptions.Enabled = enabled;
            }));
        }

        // Update Make Call ComboBox and Make Call button
        private void UpdateMakeCallComboBox()
        {
            if (rbAplication == null)
                return;

            this.BeginInvoke(new Action(() =>
            {
                cb_MakeCallMedias.Items.Clear();
                Boolean enabled = true;

                if ((currentCall != null) && currentCall.IsInProgress())
                    enabled = false;

                cb_MakeCallMedias.Enabled = enabled;
                btn_MakeCall.Enabled = enabled;

                if (enabled)
                {
                    bool canUseAudio;
                    bool canUseVideo;
                    bool canUseSharing;

                    // Audio: audio is allowed ?
                    canUseAudio = rbAplication.IsCapabilityAvailable(Contact.Capability.WebRTCAudio);

                    // Video: video is allowed ?
                    canUseVideo = rbAplication.IsCapabilityAvailable(Contact.Capability.WebRTCVideo);

                    // Sharing: sharing is allowed ?
                    canUseSharing = rbAplication.IsCapabilityAvailable(Contact.Capability.WebRTCLocalSharing);

                    if (canUseAudio)
                    {
                        cb_MakeCallMedias.Items.Add(MediasToString(Call.Media.AUDIO));
                        if (canUseVideo)
                        {
                            cb_MakeCallMedias.Items.Add(MediasToString(Call.Media.AUDIO + Call.Media.VIDEO));
                            //if (canUseSharing)
                            //    cb_MakeCallMedias.Items.Add(MediasToString(Call.Media.AUDIO + Call.Media.VIDEO + Call.Media.SHARING));
                        }

                        if (canUseSharing)
                            cb_MakeCallMedias.Items.Add(MediasToString(Call.Media.AUDIO + Call.Media.SHARING));
                    }

                    //if (canUseVideo)
                    //{
                    //    cb_MakeCallMedias.Items.Add(MediasToString(Call.Media.VIDEO));
                    //    if (canUseSharing)
                    //        cb_MakeCallMedias.Items.Add(MediasToString(Call.Media.VIDEO + Call.Media.SHARING));
                    //}

                    if (canUseSharing)
                        cb_MakeCallMedias.Items.Add(MediasToString(Call.Media.SHARING));

                    if (cb_MakeCallMedias.Items?.Count > 0)
                        cb_MakeCallMedias.SelectedIndex = 0;
                }
            }));
        }

        // Update Answer Call ComboBox + Button and Decline button
        private void UpdateAnswerCallComboBox()
        {
            if (rbAplication == null)
                return;

            this.BeginInvoke(new Action(() =>
            {
                cb_AnswerCallMedias.Items.Clear();

                if ((currentCall != null) && (currentCall.CallStatus == Call.Status.RINGING_INCOMING))
                {
                    String txt = "Incoming call with ";
                    if (currentCall.Participants?.Count > 0)
                    {
                        var participant = currentCall.Participants[0];
                        var displayName = GetDisplayName(participant.UserId, null);
                        if (String.IsNullOrEmpty(displayName))
                            txt += currentCall.ConversationId;
                        else
                            txt += displayName;
                    }
                    lbl_IncomingCall.Text = txt;
                    lbl_IncomingCall.Visible = true;

                    cb_AnswerCallMedias.Enabled = true;
                    btn_AnswerCall.Enabled = true;

                    // We can answer with only media available in the incoming call and if they are allowed

                    bool canUseAudio;
                    bool canUseVideo;
                    bool canUseSharing;

                    // Audio: remote peer must use audio and audio is allowed
                    canUseAudio = Rainbow.Util.MediasWithAudio(currentCall.RemoteMedias) && rbAplication.IsCapabilityAvailable(Contact.Capability.WebRTCAudio);

                    // Video: remote peer must use video and video is allowed
                    canUseVideo = Rainbow.Util.MediasWithVideo(currentCall.RemoteMedias) && rbAplication.IsCapabilityAvailable(Contact.Capability.WebRTCVideo);

                    // Sharing: remote peer must use sharing and sharing is allowed
                    canUseSharing = Rainbow.Util.MediasWithSharing(currentCall.RemoteMedias) && rbAplication.IsCapabilityAvailable(Contact.Capability.WebRTCLocalSharing);

                    if (canUseAudio)
                    {
                        cb_AnswerCallMedias.Items.Add(MediasToString(Call.Media.AUDIO));
                        if (canUseVideo)
                        {
                            cb_AnswerCallMedias.Items.Add(MediasToString(Call.Media.AUDIO + Call.Media.VIDEO));
                            //if (canUseSharing)
                            //    cb_AnswerCallMedias.Items.Add(MediasToString(Call.Media.AUDIO + Call.Media.VIDEO + Call.Media.SHARING));
                        }

                        if (canUseSharing)
                            cb_AnswerCallMedias.Items.Add(MediasToString(Call.Media.AUDIO + Call.Media.SHARING));
                    }

                    //if (canUseVideo)
                    //{
                    //    cb_AnswerCallMedias.Items.Add(MediasToString(Call.Media.VIDEO));
                    //    if (canUseSharing)
                    //        cb_AnswerCallMedias.Items.Add(MediasToString(Call.Media.VIDEO + Call.Media.SHARING));
                    //}

                    if (canUseSharing)
                    {
                        cb_AnswerCallMedias.Items.Add(MediasToString(Call.Media.SHARING));
                    }

                    if (cb_AnswerCallMedias.Items?.Count > 0)
                    {
                        cb_AnswerCallMedias.Items.Add(MediasToString(Call.Media.NONE));
                        cb_AnswerCallMedias.SelectedIndex = 0;
                    }
                }
                else
                {
                    lbl_IncomingCall.Visible = false;

                    cb_AnswerCallMedias.Enabled = false;
                    btn_AnswerCall.Enabled = false;
                }
            }));
        }

        // Update Hang Up and Decline buttons
        private void UpdateHangUpAndDeclineButtons()
        {
            this.BeginInvoke(new Action(() =>
            {
                if (currentCall != null)
                {
                    if (currentCall.CallStatus == Call.Status.RINGING_INCOMING)
                    {
                        btn_HangUp.Enabled = false;
                        btn_DeclineCall.Enabled = true;
                    }
                    else if (currentCall.IsInProgress())
                    {
                        btn_HangUp.Enabled = true;
                        btn_DeclineCall.Enabled = false;
                    }
                }
                else
                {
                    btn_HangUp.Enabled = false;
                    btn_DeclineCall.Enabled = false;
                }
            }));
        }

        // Update local media checkboxes
        private void UpdateLocalAndRemoteMediasCheckbox()
        {
            if ((currentCall != null) && currentCall.IsInProgress() && (rbWebRTCCommunications != null))
            {
                check_LocalAudio.Checked = Rainbow.Util.MediasWithAudio(currentCall.LocalMedias);
                check_LocalVideo.Checked = Rainbow.Util.MediasWithVideo(currentCall.LocalMedias);
                check_LocalSharing.Checked = Rainbow.Util.MediasWithSharing(currentCall.LocalMedias);
                check_LocalDataChannel.Checked = Rainbow.Util.MediasWithDataChannel(currentCall.LocalMedias);

                check_LocalAudio.Text = AUDIO + (!currentCall.IsLocalAudioMuted ? "" : $" [{MUTED}]");
                check_LocalVideo.Text = VIDEO + (!currentCall.IsLocalVideoMuted ? "" : $" [{MUTED}]");
                check_LocalSharing.Text = SHARING + (!currentCall.IsLocalSharingMuted ? "" : $" [{MUTED}]");
                check_LocalDataChannel.Text = DATACHANNEL;

                check_RemoteAudio.Checked = Rainbow.Util.MediasWithAudio(currentCall.RemoteMedias);
                check_RemoteVideo.Checked = Rainbow.Util.MediasWithVideo(currentCall.RemoteMedias);
                check_RemoteSharing.Checked = Rainbow.Util.MediasWithSharing(currentCall.RemoteMedias);
                check_RemoteDataChannel.Checked = Rainbow.Util.MediasWithDataChannel(currentCall.RemoteMedias);

                // This buttons are only visible in P2P context - For Conference "Conference options" btn must be used for same features
                if (currentCall?.IsConference == false)
                {
                    MediaPublication mediaPublication;

                    mediaPublication = new MediaPublication(currentCall.Id, currentCall.PeerId, currentCall.PeerJid, Call.Media.AUDIO);
                    var audioSubscribed = rbWebRTCCommunications.IsSubscribedToMediaPublication(mediaPublication);

                    mediaPublication = new MediaPublication(currentCall.Id, currentCall.PeerId, currentCall.PeerJid, Call.Media.VIDEO);
                    var videoSubscribed = rbWebRTCCommunications.IsSubscribedToMediaPublication(mediaPublication);

                    mediaPublication = new MediaPublication(currentCall.Id, currentCall.PeerId, currentCall.PeerJid, Call.Media.SHARING);
                    var sharingSubscribed = rbWebRTCCommunications.IsSubscribedToMediaPublication(mediaPublication);

                    mediaPublication = new MediaPublication(currentCall.Id, currentCall.PeerId, currentCall.PeerJid, Call.Media.DATACHANNEL);
                    var dataChannelSubscribed = rbWebRTCCommunications.IsSubscribedToMediaPublication(mediaPublication);

                    btn_SubscribeRemoteAudioInput.ForeColor = audioSubscribed ? System.Drawing.Color.DarkRed : System.Drawing.Color.DarkGreen;
                    btn_SubscribeRemoteAudioInput.Text = audioSubscribed ? "Unsubscribe" : "Subscribe";
                    btn_SubscribeRemoteAudioInput.Visible = check_RemoteAudio.Checked;

                    btn_SubscribeRemoteVideoInput.ForeColor = videoSubscribed ? System.Drawing.Color.DarkRed : System.Drawing.Color.DarkGreen;
                    btn_SubscribeRemoteVideoInput.Text = videoSubscribed ? "Unsubscribe" : "Subscribe";
                    btn_SubscribeRemoteVideoInput.Visible = check_RemoteVideo.Checked;

                    btn_SubscribeRemoteSharingInput.ForeColor = sharingSubscribed ? System.Drawing.Color.DarkRed : System.Drawing.Color.DarkGreen;
                    btn_SubscribeRemoteSharingInput.Text = sharingSubscribed ? "Unsubscribe" : "Subscribe";
                    btn_SubscribeRemoteSharingInput.Visible = check_RemoteSharing.Checked;

                    btn_SubscribeRemoteDataChannelInput.ForeColor = dataChannelSubscribed ? System.Drawing.Color.DarkRed : System.Drawing.Color.DarkGreen;
                    btn_SubscribeRemoteDataChannelInput.Text = dataChannelSubscribed ? "Unsubscribe" : "Subscribe";
                    btn_SubscribeRemoteDataChannelInput.Visible = check_RemoteDataChannel.Checked;

                    btn_OutputRemoteVideoInput.Visible = check_RemoteVideo.Checked && videoSubscribed;
                    btn_OutputRemoteSharingInput.Visible = check_RemoteSharing.Checked && sharingSubscribed;
                }
                else
                {
                    btn_SubscribeRemoteAudioInput.Visible = false;

                    btn_SubscribeRemoteVideoInput.Visible = false;
                    btn_SubscribeRemoteSharingInput.Visible = false;
                    btn_SubscribeRemoteDataChannelInput.Visible = false;

                    btn_OutputRemoteVideoInput.Visible = false;
                    btn_OutputRemoteSharingInput.Visible = false;
                }
            }
            else
            {
                check_LocalAudio.Checked = false;
                check_LocalVideo.Checked = false;
                check_LocalSharing.Checked = false;
                check_LocalDataChannel.Checked = false;

                check_LocalAudio.Text = AUDIO;
                check_LocalVideo.Text = VIDEO;
                check_LocalSharing.Text = SHARING;
                check_LocalDataChannel.Text = DATACHANNEL;

                check_RemoteAudio.Checked = false;
                check_RemoteVideo.Checked = false;
                check_RemoteSharing.Checked = false;
                check_RemoteDataChannel.Checked = false;

                btn_SubscribeRemoteAudioInput.Visible = false;
                btn_SubscribeRemoteVideoInput.Visible = false;
                btn_SubscribeRemoteSharingInput.Visible = false;
                btn_SubscribeRemoteDataChannelInput.Visible = false;
                btn_OutputRemoteVideoInput.Visible = false;
                btn_OutputRemoteSharingInput.Visible = false;
            }

            btn_OutputLocalVideoInput.Visible = check_LocalVideo.Checked;
            btn_OutputLocalSharingInput.Visible = check_LocalSharing.Checked;
        }

        // Update Add Media Combo Box and Add Media button
        private void UpdateAddMediaComboBox()
        {
            if (rbAplication == null)
                return;

            this.BeginInvoke(new Action(() =>
            {
                Boolean enabled = false;

                cb_AddMedia.Items.Clear();

                if ((currentCall != null) && currentCall.IsConnected())
                {
                    int localMedias = currentCall.LocalMedias;
                    bool isConference = currentCall.IsConference;

                    if (!Rainbow.Util.MediasWithAudio(localMedias))
                        cb_AddMedia.Items.Add(MediasToString(Call.Media.AUDIO));

                    if (rbAplication.IsCapabilityAvailable(Contact.Capability.WebRTCVideo) && !Rainbow.Util.MediasWithVideo(localMedias))
                        cb_AddMedia.Items.Add(MediasToString(Call.Media.VIDEO));

                    if (rbAplication.IsCapabilityAvailable(Contact.Capability.WebRTCLocalSharing) && !Rainbow.Util.MediasWithSharing(localMedias))
                        cb_AddMedia.Items.Add(MediasToString(Call.Media.SHARING));

                    if (rbAplication.IsCapabilityAvailable(Contact.Capability.WebRTCDataChannel) && !Rainbow.Util.MediasWithDataChannel(localMedias))
                        cb_AddMedia.Items.Add(MediasToString(Call.Media.DATACHANNEL));

                    if (cb_AddMedia.Items?.Count > 0)
                    {
                        enabled = true;
                        cb_AddMedia.SelectedIndex = 0;
                    }

                    if (cb_AddMedia.SelectedItem is String mediaStr)
                    {
                        int media = MediasToInt(mediaStr); // To get the media we want to add
                        Boolean canNotAddSharing = false;
                        if (media == Call.Media.SHARING)
                        {
                            canNotAddSharing = rbWebRTCCommunications.IsSharingPublicationAvailable(currentCallId);
                            if (!isConference)
                                canNotAddSharing = false;

                            btn_AddMedia.Enabled = !canNotAddSharing;
                        }


                        btn_AskToShare.Enabled = canNotAddSharing;
                    }
                }
                else
                    btn_AskToShare.Enabled = enabled;

                cb_AddMedia.Enabled = enabled;
                btn_AddMedia.Enabled = enabled;

            }));
        }

        private void CheckIfSharingMediaCanBeAdded()
        {
            if (rbWebRTCCommunications == null)
                return;

            this.BeginInvoke(new Action(() =>
            {
                if (cb_AddMedia.SelectedItem is String mediaStr)
                {
                    int media = MediasToInt(mediaStr); // To get the media we want to add
                    Boolean canNotAddSharing = false;
                    if (media == Call.Media.SHARING)
                    {
                        canNotAddSharing = rbWebRTCCommunications.IsSharingPublicationAvailable(currentCallId);
                        if (currentCall?.IsConference == false)
                            canNotAddSharing = false;
                        btn_AddMedia.Enabled = !canNotAddSharing;
                    }
                    else
                        btn_AddMedia.Enabled = true;

                    btn_AskToShare.Enabled = canNotAddSharing;
                }
            }));
        }

        // Update Remove Media Combo Box and Remove Media button
        private void UpdateRemoveMediaComboBox()
        {
            this.BeginInvoke(new Action(() =>
            {
                Boolean enabled = false;
                Boolean hideDataChannelElements = false;
                if ((currentCall != null) && currentCall.IsConnected())
                {
                    // /!\ Audio can never be removed

                    cb_RemoveMedia.Items.Clear();
                    if (Rainbow.Util.MediasWithVideo(currentCall.LocalMedias))
                        cb_RemoveMedia.Items.Add(MediasToString(Call.Media.VIDEO));

                    if (Rainbow.Util.MediasWithSharing(currentCall.LocalMedias))
                        cb_RemoveMedia.Items.Add(MediasToString(Call.Media.SHARING));

                    if (Rainbow.Util.MediasWithDataChannel(currentCall.LocalMedias))
                        cb_RemoveMedia.Items.Add(MediasToString(Call.Media.DATACHANNEL));

                    if (cb_RemoveMedia.Items.Count > 0)
                    {
                        cb_RemoveMedia.SelectedIndex = 0;
                        enabled = true;

                        hideDataChannelElements = (cb_RemoveMedia.Items[0] as String) != MediasToString(Call.Media.DATACHANNEL);
                    }
                    else
                    {
                        hideDataChannelElements = true;
                    }
                }
                else
                {
                    hideDataChannelElements = true;
                }

                // Hide elements to send data on DC
                tbDataChannel.Visible = !hideDataChannelElements;
                btn_DataChannelSend.Visible = !hideDataChannelElements;

                cb_RemoveMedia.Enabled = enabled;
                btn_RemoveMedia.Enabled = enabled;

            }));
        }

        // Update Mute / Unmute combo boxex and buttons
        private void UpdateMuteUnmuteComboBox()
        {
            this.BeginInvoke(new Action(() =>
            {
                if ((currentCall != null) && currentCall.IsConnected())
                {
                    cb_MuteMedia.Items.Clear();
                    cb_UnmuteMedia.Items.Clear();

                    if (Rainbow.Util.MediasWithAudio(currentCall.LocalMedias))
                    {
                        if (currentCall.IsLocalAudioMuted)
                            cb_UnmuteMedia.Items.Add(MediasToString(Call.Media.AUDIO));
                        else
                            cb_MuteMedia.Items.Add(MediasToString(Call.Media.AUDIO));
                    }

                    if (Rainbow.Util.MediasWithVideo(currentCall.LocalMedias))
                    {
                        if (currentCall.IsLocalVideoMuted)
                            cb_UnmuteMedia.Items.Add(MediasToString(Call.Media.VIDEO));
                        else
                            cb_MuteMedia.Items.Add(MediasToString(Call.Media.VIDEO));
                    }

                    if (Rainbow.Util.MediasWithSharing(currentCall.LocalMedias))
                    {
                        if (currentCall.IsLocalSharingMuted)
                            cb_UnmuteMedia.Items.Add(MediasToString(Call.Media.SHARING));
                        else
                            cb_MuteMedia.Items.Add(MediasToString(Call.Media.SHARING));
                    }

                    if (cb_MuteMedia.Items?.Count > 0)
                    {
                        btn_MuteMedia.Enabled = true;
                        cb_MuteMedia.Enabled = true;
                        cb_MuteMedia.SelectedIndex = 0;
                    }
                    else
                    {
                        cb_MuteMedia.Enabled = false;
                        btn_MuteMedia.Enabled = false;
                    }

                    if (cb_UnmuteMedia.Items?.Count > 0)
                    {
                        btn_UnmuteMedia.Enabled = true;
                        cb_UnmuteMedia.Enabled = true;
                        cb_UnmuteMedia.SelectedIndex = 0;
                    }
                    else
                    {
                        cb_UnmuteMedia.Enabled = false;
                        btn_UnmuteMedia.Enabled = false;
                    }
                }
                else
                {
                    btn_MuteMedia.Enabled = false;
                    cb_MuteMedia.Enabled = false;
                    btn_UnmuteMedia.Enabled = false;
                    cb_UnmuteMedia.Enabled = false;
                }
            }));
        }

        // Add information message
        private void AddInformationMessage(String message)
        {
            this.BeginInvoke(new Action(() =>
            {
                if (tb_Information.Text?.Length > 0)
                    tb_Information.Text += "\r\n" + message;
                else
                    tb_Information.Text += message;
            }));
        }

        private IMedia? GetAudioInputMediaStream()
        {
            var id = GetSelectedIdOfComboBoxUsingMediaInputStreamItem(cb_AudioInputs);
            if (!String.IsNullOrEmpty(id))
            {
                var result = _mediaInputStreamsManager.GetMediaStream(id);
                if (result != null)
                    return result;

                return _mediaAudioDeviceManager.SetAudioInputDevice(id);
            }

            return null;
        }

        private IAudioStreamTrack? GetAudioInputMediaStreamTrack()
        {
            var id = GetSelectedIdOfComboBoxUsingMediaInputStreamItem(cb_AudioInputs);
            if (!String.IsNullOrEmpty(id))
            {
                IAudioStreamTrack? result = null;
                var media = _mediaInputStreamsManager.GetMediaStream(id);
                if (media != null)
                    result = rbWebRTCDesktopFactory.CreateAudioTrack(media);
                if (result != null)
                    return result;

                // We add it to the manager
                _mediaAudioDeviceManager.SetAudioInputDevice(id);
                var audioInputDevice = _mediaAudioDeviceManager.AudioInputDevice;
                if (audioInputDevice != null)
                    result = rbWebRTCDesktopFactory.CreateAudioTrack(audioInputDevice);
                if (result != null)
                    return result;
            }

            return null;
        }

        private IMedia? GetVideoInputMediaStream()
        {
            var id = GetSelectedIdOfComboBoxUsingMediaInputStreamItem(cb_VideoInputs);
            if (String.IsNullOrEmpty(id))
                return null;

            return _mediaInputStreamsManager.GetMediaStream(id);
        }

        public IVideoStreamTrack? GetVideoMediaStreamTrack(string? id)
        {
            var media = _mediaInputStreamsManager.GetMediaStream(id);
            IVideoStreamTrack? result = null;
            try
            {
                if (media != null)
                    result = rbWebRTCDesktopFactory.CreateVideoTrack(media);
            }
            catch
            {

            }
            return result;
        }

        private IVideoStreamTrack? GetVideoInputMediaStreamTrack()
        {
            var id = GetSelectedIdOfComboBoxUsingMediaInputStreamItem(cb_VideoInputs);
            if (String.IsNullOrEmpty(id))
                return null;

            return GetVideoMediaStreamTrack(id);
        }

        private IMedia? GetSharingInputMediaStream()
        {
            var id = GetSelectedIdOfComboBoxUsingMediaInputStreamItem(cb_SharingInputs);
            if (String.IsNullOrEmpty(id))
                return null;

            return _mediaInputStreamsManager.GetMediaStream(id);
        }

        private IVideoStreamTrack? GetSharingInputMediaStreamTrack()
        {
            var id = GetSelectedIdOfComboBoxUsingMediaInputStreamItem(cb_SharingInputs);
            if (String.IsNullOrEmpty(id))
                return null;

            return GetVideoMediaStreamTrack(id);
        }

        private String GetSelectedIdOfComboBoxUsingMediaInputStreamItem(System.Windows.Forms.ComboBox comboBox)
        {
            String selectedId = "";
            // Get the the selectedItem (if any)
            if (comboBox.SelectedItem != null)
            {
                if (comboBox.SelectedItem is MediaInputStreamItem mediaInputStreamItem)
                    selectedId = mediaInputStreamItem.Id;
                else if (comboBox.SelectedItem is String id)
                    selectedId = id;
            }
            return selectedId;
        }

        private Dictionary<int, IMediaStreamTrack?>? GetMediaInputStreamTracksSelected(int medias)
        {
            // Get Media Inputs Stream according selection
            Dictionary<int, IMediaStreamTrack?>? mediaStreamTracks = new Dictionary<int, IMediaStreamTrack?>();
            IMediaStreamTrack? mediaStreamTrack;

            // Audio Input Stream
            if (Rainbow.Util.MediasWithAudio(medias))
            {
                mediaStreamTrack = GetAudioInputMediaStreamTrack();
                mediaStreamTracks.Add(Call.Media.AUDIO, mediaStreamTrack);
            }

            // Video Input Stream
            if (Rainbow.Util.MediasWithVideo(medias))
            {
                mediaStreamTrack = GetVideoInputMediaStreamTrack();
                mediaStreamTracks.Add(Call.Media.VIDEO, mediaStreamTrack);
            }

            // Sharing Input Stream
            if (Rainbow.Util.MediasWithSharing(medias))
            {
                mediaStreamTrack = GetSharingInputMediaStreamTrack();
                mediaStreamTracks.Add(Call.Media.SHARING, mediaStreamTrack);
            }

            if (mediaStreamTracks.Count == 0)
                mediaStreamTracks = null;

            return mediaStreamTracks;
        }

        private void MuteMedia(int media, Boolean muteOn)
        {
            if (currentCallId == null)
                return;

            if (rbWebRTCCommunications == null)
                return;

            String muteStr = muteOn ? "mute" : "unmut";

            if (media == Call.Media.AUDIO)
            {
                rbWebRTCCommunications.MuteAudio(currentCallId, muteOn, callback =>
                {
                    if (!callback.Result.Success)
                        AddInformationMessage($"Cannot {muteStr} audio media");
                });
            }

            if (media == Call.Media.VIDEO)
            {
                rbWebRTCCommunications.MuteVideo(currentCallId, muteOn, callback =>
                {
                    if (!callback.Result.Success)
                        AddInformationMessage($"Cannot {muteStr} video media");
                });
            }

            if (media == Call.Media.SHARING)
            {
                rbWebRTCCommunications.MuteSharing(currentCallId, muteOn, callback =>
                {
                    if (!callback.Result.Success)
                        AddInformationMessage($"Cannot {muteStr} sharing media");
                });
            }
        }

        private void UpdatePlayAllBtn(string? mediaId, Boolean isStarted, Boolean isPaused)
        {
            Bitmap bitmap;
            if (isPaused)
                bitmap = Helper.GetBitmapPlay();
            else if (isStarted)
                bitmap = Helper.GetBitmapPause();
            else
                bitmap = Helper.GetBitmapPlay();

            String id;
            id = GetSelectedIdOfComboBoxUsingMediaInputStreamItem(cb_VideoInputs);
            if (id == mediaId)
                btn_StartVideoInput.BackgroundImage = bitmap;
            else if (id == NONE)
                btn_StartVideoInput.BackgroundImage = Helper.GetBitmapPlay();

            id = GetSelectedIdOfComboBoxUsingMediaInputStreamItem(cb_SharingInputs);
            if (id == mediaId)
                btn_StartSharingInput.BackgroundImage = bitmap;
            else if (id == NONE)
                btn_StartSharingInput.BackgroundImage = Helper.GetBitmapPlay();

            id = GetSelectedIdOfComboBoxUsingMediaInputStreamItem(cb_AudioInputs);
            if (id == mediaId)
                btn_StartAudioInput.BackgroundImage = bitmap;
            else if (id == NONE)
                btn_StartAudioInput.BackgroundImage = Helper.GetBitmapPlay();
        }

        private void PlayOrPauseMediaInput(string? mediaId)
        {
            IMedia? mediaStream = _mediaInputStreamsManager.GetMediaStream(mediaId);
            if (mediaStream != null)
            {
                if (mediaStream.IsPaused)
                    mediaStream.Resume();
                else if (mediaStream.IsStarted)
                    mediaStream.Pause();
                else
                    mediaStream.Start();
            }
        }

        private void StopMediaInput(string? mediaId)
        {
            _mediaInputStreamsManager.StopMediaStream(mediaId);
        }

        private void FillMainComboBoxWithMediaInputStream()
        {
            var dicoAll = _mediaInputStreamsManager.GetList(true, true, true, false);
            var listAll = dicoAll.Values.ToList();

            FillComboBoxWithMediaInputStream(cb_VideoInputs, listAll);
            cb_VideoInputs_SelectedIndexChanged(null, null);

            FillComboBoxWithMediaInputStream(cb_SharingInputs, listAll);
            cb_SharingInputs_SelectedIndexChanged(null, null);
        }

        private void FillComboBoxWithMediaInputStream(System.Windows.Forms.ComboBox comboBox, List<IMedia>? mediaList, Boolean avoidNone = false)
        {
            if (comboBox == null)
                return;

            String selectedId = GetSelectedIdOfComboBoxUsingMediaInputStreamItem(comboBox);
            int selectedIndex = 0;

            comboBox.Items.Clear();

            // First add NONE media stream
            if (!avoidNone)
                comboBox.Items.Add(NONE);

            int index = 1;
            if (mediaList != null)
            {
                foreach (var mediaStream in mediaList)
                {
                    if (mediaStream.Id == selectedId)
                        selectedIndex = index;

                    MediaInputStreamItem mediaInputStreamItem = new MediaInputStreamItem(mediaStream);
                    comboBox.Items.Add(mediaInputStreamItem);

                    index++;
                }
            }

            if (comboBox.Items.Count > selectedIndex)
                comboBox.SelectedIndex = selectedIndex;
        }

        private void FillAudioInputComboBox()
        {
            var dico = _mediaInputStreamsManager.GetList(false, true, false, true);
            FillComboBoxWithMediaInputStream(cb_AudioInputs, dico.Values.ToList());

            // Add list of Audio Input devices
            var audioInputDevices = Rainbow.Medias.Devices.GetAudioInputDevices();
            if (audioInputDevices?.Count > 0)
            {
                foreach (var device in audioInputDevices)
                    cb_AudioInputs.Items.Add(device.Name);
            }

            if (cb_AudioInputs.Items?.Count > 2)
                cb_AudioInputs.SelectedIndex = 1;
            else if (cb_AudioInputs.Items?.Count > 1)
                cb_AudioInputs.SelectedIndex = 0;
        }

        private void FillAudioOutputComboBox()
        {
            // Clear the list
            cb_AudioOutputs.Items.Clear();

            // Add "NONE" device
            cb_AudioOutputs.Items.Add(NONE);

            // Add list of Audio Output devices
            var audioOutputDevices = Rainbow.Medias.Devices.GetAudioOutputDevices();
            if (audioOutputDevices?.Count > 0)
            {
                foreach (var device in audioOutputDevices)
                    cb_AudioOutputs.Items.Add(device.Name);
            }

            if (cb_AudioOutputs.Items?.Count > 2)
                cb_AudioOutputs.SelectedIndex = 1;
            else if (cb_AudioOutputs.Items?.Count > 1)
                cb_AudioOutputs.SelectedIndex = 0;
        }

        private void SubscribeToP2PMediaPublication(int media)
        {
            if ((currentCall != null) && (!currentCall.IsConference) && (rbWebRTCCommunications != null))
            {
                var mediaPublication = new MediaPublication(currentCall.Id, currentCall.PeerId, currentCall.PeerJid, media);
                var subscribed = rbWebRTCCommunications.IsSubscribedToMediaPublication(mediaPublication);
                if (subscribed)
                    rbWebRTCCommunications.UnsubscribeToMediaPublication(mediaPublication, callback =>
                    {
                        if (!callback.Result.Success)
                        {
                            // TODO -
                        }
                    });
                else
                    rbWebRTCCommunications.SubscribeToMediaPublication(mediaPublication, callback =>
                    {
                        if (!callback.Result.Success)
                        {
                            // TODO -
                        }
                    });
            }
        }

        #endregion METHODS TO UPDATE UI ELEMENTS OF THE FORM


        #region EVENTS from MediaInputStreamsManager

        private void MediaInputStreamsManager_ListUpdated(object? sender, EventArgs e)
        {
            if (!IsHandleCreated)
                return;

            this.BeginInvoke(new Action(() =>
            {
                // Need to update the global list of MediaInputStream
                FillMainComboBoxWithMediaInputStream();

                FillAudioInputComboBox();
            }));
        }

        private void MediaInputStreamsManager_OnMediaStreamStateChanged(string mediaId, bool isStarted, bool isPaused)
        {
            Action action = new Action(() =>
            {
                UpdatePlayAllBtn(mediaId, isStarted, isPaused);
            });

            if (InvokeRequired)
                BeginInvoke(action);
            else
                action.Invoke();
        }

        #endregion EVENTS from MediaInputStreamsManager

        #region EVENTS from MediaAudioDeviceManager

        private void MediaAudioDeviceManager_OnAudioInputDeviceChanged(object? sender, EventArgs e)
        {
            // TODO - MediaAudioDeviceManager_OnAudioInputDeviceChanged
        }

        private void MediaAudioDeviceManager_OnAudioOutputDeviceChanged(object? sender, EventArgs e)
        {
            // Stop to manage Audio Sample
            if (currentAudioStreamTrack != null)
            {
                currentAudioStreamTrack.OnAudioSample -= AudioStreamTrack_OnAudioSample;
                currentAudioStreamTrack = null;
            }

            currentAudioOutputDevice = _mediaAudioDeviceManager.AudioOutputDevice;
            if (currentAudioOutputDevice != null)
            {
                MediaStreamTrackDescriptor? result = mediaStreamTrackDescriptors.Find(m => (m.Media == Call.Media.AUDIO) && (m.PublisherId != currentContactId));
                if ((result != null) && (result.MediaStreamTrack is AudioStreamTrack audioStreamTrack))
                {
                    currentAudioStreamTrack = audioStreamTrack;
                    currentAudioStreamTrack.OnAudioSample += AudioStreamTrack_OnAudioSample;
                }
            }
        }

        private void AudioStreamTrack_OnAudioSample(string mediaId, uint duration, byte[] sample)
        {
            currentAudioOutputDevice?.QueueSample(sample);
        }

        #endregion EVENTS from MediaAudioDeviceManager

        #region EVENTS from Rainbow Conference Object

        private void Conferences_ConferenceSharingTranferStatusUpdated(object? sender, Rainbow.Events.ConferenceSharingTransferStatusEventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if (rbConferences == null)
                return;

            if (e.ConferenceId == currentCallId)
            {
                // We must ensure to use UI Thread here
                this.BeginInvoke(new Action(() =>
                {
                    string name;
                    string message;
                    string caption;
                    DialogResult result;

                    name = GetDisplayName(null, e.FromJid, null);
                    caption = "Screen sharing";

                    switch (e.ConferenceSharingTranferStatus)
                    {
                        case ConferenceSharingTranferStatus.REQUEST:

                            ConferenceSharingTranferStatus status;

                            message = $"[{name}] wants to start a screen sharing. Do you accept ?";
                            result = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (result == System.Windows.Forms.DialogResult.Yes)
                            {
                                if (rbWebRTCCommunications.RemoveSharing(currentCallId))
                                    status = ConferenceSharingTranferStatus.ACCEPT;
                                else
                                {
                                    AddInformationMessage("Cannot remove video secondary media");
                                    status = ConferenceSharingTranferStatus.REFUSE;
                                }

                            }
                            else
                                status = ConferenceSharingTranferStatus.REFUSE;

                            // We inform the Peer about the answer
                            rbConferences.ConferenceSendSharingTransfertStatus(currentCallId, e.FromJid, e.FromResource, status);
                            break;

                        case ConferenceSharingTranferStatus.REFUSE:
                            message = $"[{name}] refuses your request to start a screen sharing.";
                            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;

                        case ConferenceSharingTranferStatus.ACCEPT:
                            message = $"[{name}] has stopped sharing his screen request. You may now share yours.";
                            result = MessageBox.Show(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

                            if (result == System.Windows.Forms.DialogResult.OK)
                            {
                                // Add Sharing
                                var mediaStream = GetSharingInputMediaStreamTrack();
                                if (!rbWebRTCCommunications.AddSharing(currentCallId, mediaStream))
                                    AddInformationMessage("Cannot add sharing media");
                            }

                            break;
                    }
                }));
            }
        }

        private void Conferences_ConferenceRemoved(object? sender, Rainbow.Events.IdEventArgs e)
        {
            conferencesInProgress.Remove(e.Id);

            AddInformationMessage($"[ConferenceRemoved] A conference is no more active - Id:[{e.Id}]");
            UpdateUIAccordingCall();
        }

        private void Conferences_ConferenceUpdated(object? sender, Rainbow.Events.ConferenceEventArgs e)
        {
            lock (conferencesInProgress)
            {
                if (e.Conference.Active)
                {
                    if (!conferencesInProgress.Contains(e.Conference.Id))
                        conferencesInProgress.Add(e.Conference.Id);
                    AddInformationMessage($"[ConferenceUpdated] A conference is active - Id:[{e.Conference.Id}]");
                }
                else
                {
                    conferencesInProgress.Remove(e.Conference.Id);
                    AddInformationMessage($"[ConferenceUpdated] A conference is no more active - Id:[{e.Conference.Id}]");
                }
            }
            UpdateUIAccordingCall();
        }

        #endregion EVENTS from Rainbow Conference Object

        #region EVENTS from Rainbow WebRTCCommunications Object

        private void WebRTCCommunications_OnMediaPublicationUpdated(object? sender, MediaPublicationEventArgs e)
        {
            if (e.MediaPublication.CallId != currentCallId)
                return;

            var action = new Action(() =>
            {
                String publisher = (e.MediaPublication.PublisherId == currentContactId) ? "ME" : e.MediaPublication.PublisherId;
                String media = Util.MediasToString(e.MediaPublication.Media);
                AddInformationMessage($"Mediapublication Updated: Status:[{e.Status}] - Publisher:[{publisher}] - Media:[{media}]");

                UpdateLocalAndRemoteMediasCheckbox();

                if (_formConferenceOptions != null)
                {
                    if ((e.Status == MediaPublicationStatus.CURRENT_USER_SUBSCRIBED)
                        || (e.Status == MediaPublicationStatus.CURRENT_USER_UNSUBSCRIBED)
                        || (e.Status == MediaPublicationStatus.PEER_STOPPED))
                    {
                        Boolean subscribed = (e.Status == MediaPublicationStatus.CURRENT_USER_SUBSCRIBED);
                        _formConferenceOptions.UpdateMediaPublicationSubscription(subscribed, e.MediaPublication);
                    }
                }
            });

            this.BeginInvoke(action);
        }

        private void RbWebRTCCommunications_OnMediaServiceUpdated(object? sender, MediaServiceEventArgs e)
        {
            if (e.MediaService.CallId != currentCallId)
                return;

            var action = new Action(() =>
            {
                String service = e.MediaService.ServiceId;
                String media = Util.MediasToString(Call.Media.VIDEO);
                AddInformationMessage($"MediaService Updated: Status:[{e.Status}] - Service:[{service}] - Media:[{media}]");

                UpdateLocalAndRemoteMediasCheckbox();

                if (_formConferenceOptions != null)
                {
                    if ((e.Status == MediaPublicationStatus.CURRENT_USER_SUBSCRIBED)
                        || (e.Status == MediaPublicationStatus.CURRENT_USER_UNSUBSCRIBED)
                        || (e.Status == MediaPublicationStatus.PEER_STOPPED))
                    {
                        Boolean subscribed = (e.Status == MediaPublicationStatus.CURRENT_USER_SUBSCRIBED);
                        _formConferenceOptions.UpdateMediaServiceSubscription(subscribed, e.MediaService);
                    }
                }
            });

            this.BeginInvoke(action);
        }

        private void RbWebRTCCommunications_OnTrack(string callId, MediaStreamTrackDescriptor mediaStreamTrackDescriptor)
        {
            if (callId == currentCallId)
            {
                String publisher = (mediaStreamTrackDescriptor.PublisherId == currentContactId) ? "ME" : mediaStreamTrackDescriptor.PublisherId;
                String media = Util.MediasToString(mediaStreamTrackDescriptor.Media);


                // Remove (if any) previous Media Strem Track with same publisher and same Media.
                var result = mediaStreamTrackDescriptors.Find(m => (m.PublisherId == mediaStreamTrackDescriptor.PublisherId) && (m.Media == mediaStreamTrackDescriptor.Media));
                if (result != null)
                {
                    AddInformationMessage($"Previous Track has been removed: Publisher:[{publisher}] - Media:[{media}]");
                    mediaStreamTrackDescriptors.Remove(result);
                }

                mediaStreamTrackDescriptors.Add(mediaStreamTrackDescriptor);

                AddInformationMessage($"New Track: Publisher:[{publisher}] - Media:[{media}] - IsMediaService:[{mediaStreamTrackDescriptor.IsMediaService}] - nb MediaStreamTrack:[{mediaStreamTrackDescriptors.Count}]");

                // If Audio TRack has been added by a Peer, we want to hear it in the audio output device
                if ((mediaStreamTrackDescriptor.Media == Call.Media.AUDIO) && (mediaStreamTrackDescriptor.PublisherId != currentContactId))
                {
                    MediaAudioDeviceManager_OnAudioOutputDeviceChanged(this, null);
                }
            }
        }

        private void RbWebRTCCommunications_OnDataChannel(string callId, DataChannelDescriptor dataChannelDescriptor)
        {
            if (callId == currentCallId)
            {
                var publisherId = dataChannelDescriptor.PublisherId;
                var dataChannel = dataChannelDescriptor.DataChannel;
                String publisher = (publisherId == currentContactId) ? "ME" : publisherId;

                // Remove (if any) previous DataChannel with same publisher
                if (dataChannels.ContainsKey(publisherId))
                {
                    AddInformationMessage($"Previous DataChannel has been removed: Publisher:[{publisher}]");
                    dataChannels.Remove(publisherId);
                }

                dataChannels.Add(publisherId, dataChannel);

                dataChannel.OnMessage += (s) =>
                {
                    AddInformationMessage($"DataChannel - OnMessage: [{s}]");
                };


                //Action actionSend = () => dataChannel.Send("text sent every 1s");
                //Action actionDelay = () => CancelableDelay.StartAfter(1000, actionSend);
                //Action actionLoop = () => { };

                //dataChannel.OnOpen += () =>
                //{
                //    actionLoop.Invoke();
                //};

                AddInformationMessage($"New DataChannel: Publisher:[{publisher}] - nb DataChannels:[{dataChannels.Count}]");
            }
        }

        private void WebRTCCommunications_CallUpdated(object? sender, Rainbow.Events.CallEventArgs e)
        {
            if (e.Call == null)
            {
                AddInformationMessage($"Call object is null ...");
                return;
            }

            if (rbContacts == null)
                return;

            // If we don't currently manage a call, we take the first new one into account
            if (currentCallId == null)
            {
                currentCallId = e.Call.Id;
                AddInformationMessage($"Call - new with id:[{currentCallId}]");


            }

            if (e.Call.Id == currentCallId)
            {
                if (!e.Call.IsInProgress())
                {
                    currentCallId = null;
                    currentCall = null;

                    // The call is NO MORE in Progress => We Rollback presence if any
                    rbContacts.RollbackPresenceSavedFromCurrentContact();

                    // Clear all MediaStreamDescriptors
                    while (mediaStreamTrackDescriptors.Count > 0)
                    {
                        var mediaStreamTrackDescriptor = mediaStreamTrackDescriptors[0];
                        mediaStreamTrackDescriptors.RemoveAt(0);

                        mediaStreamTrackDescriptor.MediaStreamTrack?.Dispose();
                        mediaStreamTrackDescriptor.MediaStreamTrack = null;
                        mediaStreamTrackDescriptor = null;
                    }
                    AddInformationMessage("All Tracks has been removed");
                }
                else
                {
                    if (currentCall == null)
                    {
                        currentCall = e.Call;
                        this.BeginInvoke(() => btn_ConferenceOptions_Click(null, null));
                    }
                    else
                        currentCall = e.Call;

                    if (!e.Call.IsRinging())
                    {
                        // The call is NOT IN RINGING STATE  => We update presence according media
                        rbContacts.SetBusyPresenceAccordingMedias(e.Call.LocalMedias);
                    }
                }


                AddInformationMessage($"Call - Status:[{e.Call.CallStatus}] - Local:[{e.Call.LocalMedias}] - Remote:[{e.Call.RemoteMedias}] - Conf.:[{(e.Call.IsConference ? "True - " + e.Call.ConferenceId : "False")}] - IsInitiator.:[{e.Call.IsInitiator}]");
                UpdateUIAccordingCall();
            }

        }

    #endregion EVENTS from Rainbow WebRTCCommunications Object

    #region EVENTS from UI Elements of the form

        private void FormWebRTC_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void btn_ManageInputStreams_Click(object sender, EventArgs e)
        {
            _mediaInputStreamsManager.OpenFormMediaFilteredStreams();
        }

        private void btb_AudioInputListRefresh_Click(object sender, EventArgs e)
        {
            FillAudioInputComboBox();
        }

        private void btb_AudioOutputListRefresh_Click(object sender, EventArgs e)
        {
            FillAudioOutputComboBox();
        }

        private void cb_VideoInputs_SelectedIndexChanged(object sender, EventArgs e)
        {
            var mediaSream = GetVideoInputMediaStream();

            String? mediaSreamId = null;
            Boolean IsStarted = false;
            Boolean IsPaused = false;

            if (mediaSream != null)
            {
                mediaSreamId = mediaSream.Id;
                IsStarted = mediaSream.IsStarted;
                IsPaused = mediaSream.IsPaused;
            }

            UpdatePlayAllBtn(mediaSreamId, IsStarted, IsPaused);


            // Change Audio Stream
            if ((currentCall != null) && currentCall.IsInProgress())
            {
                if (Rainbow.Util.MediasWithVideo(currentCall.LocalMedias))
                {
                    var mediaSreamTrack = GetVideoInputMediaStreamTrack();
                    rbWebRTCCommunications.ChangeVideo(currentCall.Id, mediaSreamTrack);
                }
            }
        }

        private void cb_SharingInputs_SelectedIndexChanged(object sender, EventArgs e)
        {
            var mediaSream = GetSharingInputMediaStream();
            String? mediaSreamId = null;
            Boolean IsStarted = false;
            Boolean IsPaused = false;

            if (mediaSream != null)
            {
                mediaSreamId = mediaSream.Id;
                IsStarted = mediaSream.IsStarted;
                IsPaused = mediaSream.IsPaused;
            }

            UpdatePlayAllBtn(mediaSreamId, IsStarted, IsPaused);

            // Change Audio Stream
            if ((currentCall != null) && currentCall.IsInProgress())
            {
                if (Rainbow.Util.MediasWithSharing(currentCall.LocalMedias))
                {
                    var mediaSreamTrack = GetSharingInputMediaStreamTrack();
                    rbWebRTCCommunications.ChangeSharing(currentCall.Id, mediaSreamTrack);
                }
            }
        }

        private void cb_AudioInputList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var mediaStream = GetAudioInputMediaStream();
            String? mediaSreamId = null;
            Boolean IsStarted = false;
            Boolean IsPaused = false;

            if (mediaStream != null)
            {
                mediaSreamId = mediaStream.Id;
                IsStarted = mediaStream.IsStarted;
                IsPaused = mediaStream.IsPaused;
            }

            UpdatePlayAllBtn(mediaSreamId, IsStarted, IsPaused);

            // Change Audio Stream
            if ((currentCall != null) && currentCall.IsInProgress())
            {
                if (Rainbow.Util.MediasWithAudio(currentCall.LocalMedias))
                {
                    var mediaStreamTrack = GetAudioInputMediaStreamTrack();
                    rbWebRTCCommunications.ChangeAudio(currentCall.Id, mediaStreamTrack);
                }
            }

        }

        private void cb_AudioOutputList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_AudioOutputs.SelectedItem is String deviceName)
            {
                if (deviceName != NONE)
                {
                    _mediaAudioDeviceManager.SetAudioOutputDevice(deviceName);
                }
                else
                {
                    // Close / Dispose previous audio output
                    _mediaAudioDeviceManager.CloseAudioOutputDevice(false);
                }
            }
        }

        private void btn_LoadConfig_Click(object sender, EventArgs e)
        {
            btn_LoadConfig.Enabled = false;
            btn_ManageInputStreams.Enabled = false;

            // We want to load configuration using another thread
            Task newTask = new Task(() =>
            {
                _mediaInputStreamsManager.LoadConfiguration();

                this.BeginInvoke(new Action(() =>
                {
                    btn_LoadConfig.Enabled = true;
                    btn_ManageInputStreams.Enabled = true;

                    if (cb_VideoInputs.Items.Count > 1)
                        cb_VideoInputs.SelectedIndex = 1;

                    if (cb_SharingInputs.Items.Count > 2)
                        cb_SharingInputs.SelectedIndex = 2;
                    else if (cb_SharingInputs.Items.Count > 1)
                        cb_SharingInputs.SelectedIndex = 1;

                }));
            });
            newTask.Start();
        }

        private void btn_OutputVideoInput_Click(object sender, EventArgs e)
        {
            IMedia? mediaStream = GetVideoInputMediaStream();
            if (mediaStream is IMediaVideo mediaVideo)
            {
                var form = new FormVideoOutputStream();
                form.Show();
                form.SetMediaVideo(mediaVideo);
            }
        }

        private void btn_OutputSharingInput_Click(object sender, EventArgs e)
        {
            IMedia? mediaStream = GetSharingInputMediaStream();
            if (mediaStream is IMediaVideo mediaVideo)
            {
                var form = new FormVideoOutputStream();
                form.Show();
                form.SetMediaVideo(mediaVideo);
            }
        }

        private void cb_ContactsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedContactId = GetSelectedIdFromComboBox(cb_ContactsList);
        }

        private void cb_BubblesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedBubbleId = GetSelectedIdFromComboBox(cb_BubblesList);
            UpdateBubblesComboBox();
        }

        private void btn_MakeCall_Click(object sender, EventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if (rbContacts == null)
                return;

            if ((currentCall != null) && currentCall.IsInProgress())
            {
                AddInformationMessage("Cannot make call - a call is already in progress");
                return;
            }

            // Get Contact Id
            String? contactId = GetSelectedIdFromComboBox(cb_ContactsList);
            if (contactId != null)
            {
                // Get Medias
                String? mediaString = cb_MakeCallMedias.SelectedItem as String;
                int medias = MediasToInt(mediaString);

                // Get Subject
                String subject = tb_Subject.Text;

                if (medias == 0)
                {
                    AddInformationMessage("Cannot make call - no media specified");
                    return;
                }

                // Get Media Inputs Stream according selection
                Dictionary<int, IMediaStreamTrack?>? mediaStreamTracks = GetMediaInputStreamTracksSelected(medias);

                // Save presence for future rollback (once the call is over)
                rbContacts.SavePresenceFromCurrentContactForRollback();

                // TODO - Make call using IMedia list 
                rbWebRTCCommunications.MakeCall(contactId, mediaStreamTracks, subject, callback =>
                {
                    if (callback.Result.Success)
                    {
                        currentCallId = callback.Data;
                    }
                    else
                    {
                        // TODO - manage error
                    }
                });

            }
            else
                AddInformationMessage("Cannot make call - no contact specified");
        }

        private void btn_AnswerCall_Click(object sender, EventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if (rbContacts == null)
                return;

            if (currentCallId == null)
                return;

            if ((currentCall != null) && (currentCall.CallStatus != Call.Status.RINGING_INCOMING))
            {
                AddInformationMessage("Cannot answer call - the call is not 'ringing incoming'");
                return;
            }

            // Get Medias
            String? mediaString = cb_AnswerCallMedias.SelectedItem as String;
            if (mediaString != null)
            {
                int medias = MediasToInt(mediaString);

                // Get Media Inputs Stream according selection
                Dictionary<int, IMediaStreamTrack?>? mediaStreamTracks = GetMediaInputStreamTracksSelected(medias);

                rbContacts.SavePresenceFromCurrentContactForRollback();
                if (rbWebRTCCommunications.AnswerCall(currentCallId, mediaStreamTracks))
                {
                    // TODO - Answer call without  error - something to do more ?
                }
                else
                {
                    // TODO - Answer call with error ...
                }
            }
        }

        private void btn_DeclineCall_Click(object sender, EventArgs e)
        {
            if (currentCall != null)
            {
                if (currentCall.CallStatus == Call.Status.RINGING_INCOMING)
                    rbWebRTCCommunications?.RejectCall(currentCallId);
                else
                    AddInformationMessage("Cannot answer call - the call is not 'ringing incoming'");
            }
        }

        private void btn_HangUp_Click(object sender, EventArgs e)
        {
            if ((currentCallId != null) && (currentCall?.IsInProgress() == true))
            {
                rbWebRTCCommunications?.HangUpCall(currentCallId);
            }
            else
            {
                AddInformationMessage("Cannot hangup call - the call is not in progress");
                return;
            }
        }

        private void btn_RemoveMedia_Click(object sender, EventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if (currentCallId == null)
                return;

            if ((currentCall != null) && currentCall.IsConnected())
            {
                if (cb_RemoveMedia.SelectedItem is String mediaStr)
                {
                    int media = MediasToInt(mediaStr); // To get the media we want to remove

                    if (media == Call.Media.VIDEO)
                    {
                        if (!rbWebRTCCommunications.RemoveVideo(currentCallId))
                        {
                            AddInformationMessage("Cannot remove video media");
                        }
                    }
                    else if (media == Call.Media.DATACHANNEL)
                    {
                        if (!rbWebRTCCommunications.RemoveDataChannel(currentCallId))
                        {
                            AddInformationMessage("Cannot remove datachannel media");
                        }
                    }
                    else if (media == Call.Media.SHARING)
                    {
                        if (!rbWebRTCCommunications.RemoveSharing(currentCallId))
                        {
                            AddInformationMessage("Cannot remove sharing media");
                        }
                    }
                }
            }
            else
            {
                AddInformationMessage("Cannot hangup call - the call is not connected");
                return;
            }
        }

        private void btn_AddMedia_Click(object sender, EventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if (currentCallId == null)
                return;

            if ((currentCall != null) && currentCall.IsConnected())
            {
                if (cb_AddMedia.SelectedItem is String mediaStr)
                {
                    int media = MediasToInt(mediaStr); // To get the media we want to add

                    if (media == Call.Media.AUDIO)
                    {
                        var mediaStreamTrack = GetAudioInputMediaStreamTrack();
                        if (!rbWebRTCCommunications.AddAudio(currentCallId, mediaStreamTrack))
                            AddInformationMessage("Cannot add audio media");
                    }
                    else if (media == Call.Media.VIDEO)
                    {
                        var mediaStreamTrack = GetVideoInputMediaStreamTrack();
                        if (!rbWebRTCCommunications.AddVideo(currentCallId, mediaStreamTrack))
                            AddInformationMessage("Cannot add video media");
                    }
                    else if (media == Call.Media.SHARING)
                    {
                        var mediaStreamTrack = GetSharingInputMediaStreamTrack();
                        if (!rbWebRTCCommunications.AddSharing(currentCallId, mediaStreamTrack))
                            AddInformationMessage("Cannot add sharing media");
                    }
                    else if (media == Call.Media.DATACHANNEL)
                    {
                        var dc = rbWebRTCCommunications.AddDataChannel(currentCallId, "data channel description");
                        if (dc == null)
                            AddInformationMessage("Cannot add datachannel media");
                    }
                }
            }
            else
            {
                AddInformationMessage("Cannot hangup call - the call is not connected");
                return;
            }
        }

        private void btn_AskToShare_Click(object sender, EventArgs e)
        {
            if (rbConferences == null)
                return;

            if (currentCall?.IsConference == true)
            {
                (String toJid, String toResource) = rbConferences.ConferenceGetSharingPublisherDetails(currentCallId);

                rbConferences.ConferenceSendSharingTransfertStatus(currentCallId, toJid, toResource, ConferenceSharingTranferStatus.REQUEST, callback =>
                {
                    if (!callback.Result.Success)
                        AddInformationMessage($"Cannot request sharing transfert: [[{callback.Result}]]");
                });
            }
        }

        private void btn_MuteMedia_Click(object sender, EventArgs e)
        {
            if ((currentCall != null) && currentCall.IsConnected())
            {
                if (cb_MuteMedia.SelectedItem is String mediaStr)
                {
                    int media = MediasToInt(mediaStr);
                    MuteMedia(media, true);
                }
            }
            else
            {
                AddInformationMessage("Cannot hangup call - the call is not connected");
                return;
            }
        }

        private void btn_UnmuteMedia_Click(object sender, EventArgs e)
        {
            if ((currentCall != null) && currentCall.IsConnected())
            {
                if (cb_UnmuteMedia.SelectedItem is String mediaStr)
                {
                    int media = MediasToInt(mediaStr);
                    MuteMedia(media, false);
                }
            }
            else
            {
                AddInformationMessage("Cannot hangup call - the call is not connected");
                return;
            }
        }

        private void OpenFormVideoOutputStreamWebRTC(int media, String? publisherId, bool dynamicFeed)
        {
            //TODO - use dynamicFeed
            MediaStreamTrackDescriptor? result;
            if (dynamicFeed)
                result = mediaStreamTrackDescriptors.Find(m => (m.Media == media) && m.DynamicFeed);
            else
                result = mediaStreamTrackDescriptors.Find(m => (m.Media == media) && (m.PublisherId == publisherId));
            if (result == null)
            {
                AddInformationMessage($"Cannot open [{Util.MediasToString(media)}] Publisher:[{publisherId}] - no Media Stream Track Descriptor found");
            }
            else
            {
                var form = new FormVideoOutputStreamWebRTC();
                form.Show();
                form.SetApplicationAndCallId(rbAplication, currentCallId);
                form.SetMediaStreamTrackDescriptor(result);
            }
        }

        private void OpenFormMediaServiceStreamWebRTC(String serviceId)
        {
            //TODO - use dynamicFeed
            MediaStreamTrackDescriptor? result;
            result = mediaStreamTrackDescriptors.Find(m => (m.PublisherId == serviceId));

            if (result == null)
            {
                AddInformationMessage($"Cannot open [{Call.Media.VIDEO}] ServiceId:[{serviceId}] - no Media Stream Track Descriptor found");
            }
            else
            {
                var form = new FormVideoOutputStreamWebRTC();
                form.Show();
                form.SetApplicationAndCallId(rbAplication, currentCallId);
                form.SetMediaStreamTrackDescriptor(result);
            }
        }

        private void btn_OutputRemoteVideoInput_Click(object sender, EventArgs e)
        {
            if (currentCall == null)
                AddInformationMessage("Cannot see remote video - currentCall is null");
            else
                OpenFormVideoOutputStreamWebRTC(Call.Media.VIDEO, currentCall.PeerId, false);
        }

        private void btn_OutputRemoteSharingInput_Click(object sender, EventArgs e)
        {
            if (currentCall == null)
                AddInformationMessage("Cannot see remote sharing - currentCall is null");
            else
                OpenFormVideoOutputStreamWebRTC(Call.Media.SHARING, currentCall.PeerId, false);
        }

        private void btn_SubscribeRemoteSharingInput_Click(object sender, EventArgs e)
        {
            SubscribeToP2PMediaPublication(Call.Media.SHARING);
        }

        private void btn_SubscribeRemoteVideoInput_Click(object sender, EventArgs e)
        {
            SubscribeToP2PMediaPublication(Call.Media.VIDEO);
        }

        private void btn_SubscribeRemoteAudioInput_Click(object sender, EventArgs e)
        {
            SubscribeToP2PMediaPublication(Call.Media.AUDIO);
        }

        private void btn_OutputLocalVideoInput_Click(object sender, EventArgs e)
        {
            if (currentCall == null)
                AddInformationMessage("Cannot see local VIDEO - currentCall is null");
            else
                OpenFormVideoOutputStreamWebRTC(Call.Media.VIDEO, currentContactId, false);
        }

        private void btn_OutputLocalSharingInput_Click(object sender, EventArgs e)
        {
            if (currentCall == null)
                AddInformationMessage("Cannot see local SHARING - currentCall is null");
            else
                OpenFormVideoOutputStreamWebRTC(Call.Media.SHARING, currentContactId, false);
        }

        private void btn_StartMediaInput_Click(object sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.Button btn)
            {
                String? id = null;
                switch (btn.Name)
                {
                    case "btn_StartVideoInput":
                        id = GetSelectedIdOfComboBoxUsingMediaInputStreamItem(cb_VideoInputs);
                        break;

                    case "btn_StartSharingInput":
                        id = GetSelectedIdOfComboBoxUsingMediaInputStreamItem(cb_SharingInputs);
                        break;

                    case "btn_StartAudioInput":
                        id = GetSelectedIdOfComboBoxUsingMediaInputStreamItem(cb_AudioInputs);
                        break;
                }

                PlayOrPauseMediaInput(id);
            }
        }

        private void btn_StoptMediaInput_Click(object sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.Button btn)
            {
                String? id = null;
                switch (btn.Name)
                {
                    case "btn_StopVideoInput":
                        id = GetSelectedIdOfComboBoxUsingMediaInputStreamItem(cb_VideoInputs);
                        break;

                    case "btn_StopSharingInput":
                        id = GetSelectedIdOfComboBoxUsingMediaInputStreamItem(cb_SharingInputs);
                        break;

                    case "btn_StopAudioInput":
                        id = GetSelectedIdOfComboBoxUsingMediaInputStreamItem(cb_AudioInputs);
                        break;
                }

                StopMediaInput(id);
            }
        }

        private void btn_StartConf_Click(object sender, EventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if (rbContacts == null)
                return;

            if ((currentCall != null) && currentCall.IsInProgress())
            {
                AddInformationMessage("Cannot make call - a call is already in progress");
                return;
            }

            if (String.IsNullOrEmpty(selectedBubbleId))
                return;

            // Get Audio Media Input Stream according selection
            Dictionary<int, IMedia>? mediaStreams = null;
            var mediaStream = GetAudioInputMediaStream();
            if (mediaStream != null)
            {
                mediaStreams = new Dictionary<int, IMedia>();
                mediaStreams.Add(Call.Media.AUDIO, mediaStream);
            }

            // Save presence for future rollback (once the call is over)
            rbContacts.SavePresenceFromCurrentContactForRollback();

            // Get subscription info from UI
            //(Boolean autoSubscriptionSharing, Boolean autoSubscriptionVideo, int maxSubscriptionVideo) = GetSubscriptionInfo();

            var mediaStreamTrack = GetAudioInputMediaStreamTrack();
            rbWebRTCCommunications.StartAndJoinConference(selectedBubbleId, mediaStreamTrack, callback =>
            {
                if (callback.Result.Success)
                {
                    currentCallId = callback.Data;
                }
                else
                {
                    AddInformationMessage($"Cannot start conference: [[{callback.Result}]]");
                }
            });
        }

        private void btn_JoinConf_Click(object sender, EventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if (rbContacts == null)
                return;

            if ((currentCall != null) && currentCall.IsInProgress())
            {
                AddInformationMessage("Cannot make call - a call is already in progress");
                return;
            }

            if (conferencesInProgress.Count == 0)
                return;

            String conferenceInProgressId;
            if (cb_ConferencesName.SelectedItem is ListItem item)
                conferenceInProgressId = item.Value;
            else
                return;

            // Get Audio Media Input Stream according selection
            var mediaStreamTrack = GetAudioInputMediaStreamTrack();

            // Save presence for future rollback (once the call is over)
            rbContacts.SavePresenceFromCurrentContactForRollback();

            rbWebRTCCommunications.JoinConference(conferenceInProgressId, mediaStreamTrack, callback =>
            {
                if (callback.Result.Success)
                {
                    currentCallId = callback.Data;
                }
                else
                {
                    AddInformationMessage($"Cannot join conference: [[{callback.Result}]]");
                }
            });
        }

        private void btn_DeclineConf_Click(object sender, EventArgs e)
        {
            String conferenceInProgressId;
            if (cb_ConferencesName.SelectedItem is ListItem item)
                conferenceInProgressId = item.Value;
            else
                return;

            if (currentCall == null)
            {
                rbConferences.ConferenceReject(conferenceInProgressId, callback =>
                {
                    if (!callback.Result.Success)
                        AddInformationMessage($"Cannot decline conf - {callback.Result}");
                });
            }
        }

        private void cb_AddMedia_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckIfSharingMediaCanBeAdded();
        }

        private void btn_ConferenceOptions_Click(object sender, EventArgs e)
        {
            // Set conference Info (if any) and subscription
            if (currentCall?.IsConference == true)
            {
                _formConferenceOptions.CanSubscribe(true);
                _formConferenceOptions.SetConferenceInfo(currentCall.ConferenceId);

                _formConferenceOptions.WindowState = FormWindowState.Normal;
                _formConferenceOptions.Show();
                _formConferenceOptions.Activate();
            }
            else if (conferencesInProgress.Count > 0)
            {
                _formConferenceOptions.CanSubscribe(false);
                _formConferenceOptions.SetConferenceInfo(conferencesInProgress[0]);
            }


        }

        private void btn_Details_Click(object sender, EventArgs e)
        {

            String str = Conversation.ConversationType.User;
            switch(str)
            {
                case Conversation.ConversationType.User:
                    break;
            }

            if ((currentCall != null) && (rbWebRTCCommunications != null))
            {
                var mediaPublicationAvailable = rbWebRTCCommunications.GetMediaPublicationsAvailable(currentCall.Id);
                var mediaPublicationSubscribed = rbWebRTCCommunications.GetMediaPublicationsSubscribed(currentCall.Id);
                AddInformationMessage($"Media Publication -  Available:[{mediaPublicationAvailable.Count}] - Subscribed:[{mediaPublicationSubscribed.Count}]");

                var mediaServiceAvailable = rbWebRTCCommunications.GetMediaServicesAvailable(currentCall.Id);
                var mediaServiceSubscribed = rbWebRTCCommunications.GetMediaServicesSubscribed(currentCall.Id);
                AddInformationMessage($"Media Service -  Available:[{mediaServiceAvailable.Count}] - Subscribed:[{mediaServiceSubscribed.Count}]");

                var allTracks = rbWebRTCCommunications.GetTracks(currentCall.Id);
                var allAudioTracks = rbWebRTCCommunications.GetTracks(currentCall.Id, Call.Media.AUDIO);
                var allVideoTracks = rbWebRTCCommunications.GetTracks(currentCall.Id, Call.Media.VIDEO);
                var allSharingTracks = rbWebRTCCommunications.GetTracks(currentCall.Id, Call.Media.SHARING);
                AddInformationMessage($"All Tracks:[{allTracks.Count}] - Audio:[{allAudioTracks.Count}] - Video:[{allVideoTracks.Count}] - Sharing:[{allSharingTracks.Count}]");

                var allLocalTracks = rbWebRTCCommunications.GetTracks(currentCall.Id, fromCurrentUser: true);
                var allAudioLocalTracks = rbWebRTCCommunications.GetTracks(currentCall.Id, Call.Media.AUDIO, true);
                var allVideoLocalTracks = rbWebRTCCommunications.GetTracks(currentCall.Id, Call.Media.VIDEO, true);
                var allSharingLocalTracks = rbWebRTCCommunications.GetTracks(currentCall.Id, Call.Media.SHARING, true);
                AddInformationMessage($"Local Tracks:[{allLocalTracks.Count}] - Audio:[{allAudioLocalTracks.Count}] - Video:[{allVideoLocalTracks.Count}] - Sharing:[{allSharingLocalTracks.Count}]");

                var allRemoteTracks = rbWebRTCCommunications.GetTracks(currentCall.Id, fromCurrentUser: false);
                var allAudioRemoteTracks = rbWebRTCCommunications.GetTracks(currentCall.Id, Call.Media.AUDIO, false);
                var allVideoRemoteTracks = rbWebRTCCommunications.GetTracks(currentCall.Id, Call.Media.VIDEO, false);
                var allSharingRemoteTracks = rbWebRTCCommunications.GetTracks(currentCall.Id, Call.Media.SHARING, false);
                AddInformationMessage($"Remote Tracks:[{allRemoteTracks.Count}] - Audio:[{allAudioRemoteTracks.Count}] - Video:[{allVideoRemoteTracks.Count}] - Sharing:[{allSharingRemoteTracks.Count}]");

                // By media
                AddInformationMessage($"Audio Tracks:[{allAudioTracks.Count}] - Local:[{allAudioLocalTracks.Count}] - Remote:[{allAudioRemoteTracks.Count}]");
                AddInformationMessage($"Video Tracks:[{allVideoTracks.Count}] - Local:[{allVideoLocalTracks.Count}] - Remote:[{allVideoRemoteTracks.Count}]");
                AddInformationMessage($"Sharing Tracks:[{allSharingTracks.Count}] - Local:[{allSharingLocalTracks.Count}] - Remote:[{allSharingRemoteTracks.Count}]");

                var allDC = rbWebRTCCommunications.GetDataChannels(currentCall.Id, null);
                var allLocalDC = rbWebRTCCommunications.GetDataChannels(currentCall.Id, true);
                var allRemoteDC = rbWebRTCCommunications.GetDataChannels(currentCall.Id, false);
                AddInformationMessage($"All DataChannel:[{allDC.Count}] - Local:[{allLocalDC.Count}] - Remote:[{allRemoteDC.Count}]");
            }
            else
            {
                AddInformationMessage($"Not in a call");
            }
        }

        private void btn_DataChannelSend_Click(object sender, EventArgs e)
        {
            if (tbDataChannel.Text?.Length > 0)
            {
                if (dataChannels.ContainsKey(currentContactId))
                {
                    var dataChannel = dataChannels[currentContactId];
                    dataChannel.Send(tbDataChannel.Text);
                    tbDataChannel.Text = "";
                }
            }
        }

        private void cb_RemoveMedia_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_RemoveMedia.SelectedItem is String mediaStr)
            {
                int media = MediasToInt(mediaStr);
                Boolean visible = (media == Call.Media.DATACHANNEL);
                tbDataChannel.Visible = visible;
                btn_DataChannelSend.Visible = visible;
            }
        }

        private void btn_InformationClear_Click(object sender, EventArgs e)
        {
            tb_Information.Text = "";
        }

    #endregion EVENTS from UI Elements of the form

    #region EVENTS from FormConferenceOptions

        private void FormConferenceOptions_OnMediaPublicationSubscription(object? sender, MediaPublicationEventArgs e)
        {
            // Is-it related to the current call ?
            if (currentCall?.Id == e.MediaPublication.CallId)
            {
                if (e.Status == MediaPublicationStatus.CURRENT_USER_SUBSCRIBED)
                {
                    // If we want to subscribe to its own publication, we must allow it first
                    if (e.MediaPublication.PublisherId == currentContactId)
                        rbWebRTCCommunications?.AllowToSubscribeToHisOwnMediaPublication(e.MediaPublication.CallId, true);
                    rbWebRTCCommunications?.SubscribeToMediaPublication(e.MediaPublication);
                }
                else
                    rbWebRTCCommunications?.UnsubscribeToMediaPublication(e.MediaPublication);
            }
        }

        private void FormConferenceOptions_OnMediaServiceSubscription(object? sender, MediaServiceEventArgs e)
        {
            // Is - it related to teh current call ?
            if (currentCall?.Id == e.MediaService.CallId)
            {
                if (e.Status == MediaPublicationStatus.CURRENT_USER_SUBSCRIBED)
                {
                    rbWebRTCCommunications?.SubscribeToMediaService(e.MediaService);
                }
                else
                    rbWebRTCCommunications?.UnsubscribeToMediaService(e.MediaService);
            }
        }

        private void FormConferenceOptions_OnCloseDataChannel(object? sender, MediaPublicationEventArgs e)
        {
            DataChannelDescriptor? dataChannelDescriptor;
            // Is-it related to the current call ?
            if (currentCall?.Id == e.MediaPublication.CallId)
            {

                var dcList = rbWebRTCCommunications.GetDataChannels(e.MediaPublication.CallId, null);
                dataChannelDescriptor = dcList.Find(dc => dc.PublisherId == e.MediaPublication.PublisherId);


                if (dataChannelDescriptor == null)
                {
                    if (dcList.Count == 1)
                        dataChannelDescriptor = dcList.FirstOrDefault();
                }
                //if (e.Status == MediaPublicationStatus.CURRENT_USER_SUBSCRIBED)
                //{
                //    // Need to get Data Channel ...
                //    var dcList = rbWebRTCCommunications.GetDataChannels(e.MediaPublication.CallId, true);
                //    dataChannelDescriptor = dcList.First();
                //}
                //else
                //{
                //    // Need to get Data Channel ...

                //}

                if (dataChannelDescriptor != null)
                    dataChannelDescriptor.DataChannel.Close();
            }
        }

        private void FormConferenceOptions_OpenVideoPublication(object? sender, (int media, string? publisherId, bool dynamicFeed) e)
        {
            OpenFormVideoOutputStreamWebRTC(e.media, e.publisherId, e.dynamicFeed);
        }

        private void FormConferenceOptions_OpenMediaService(object? sender, string e)
        {
            OpenFormMediaServiceStreamWebRTC(e);
        }

    #endregion EVENTS from FormConferenceOptions


    }
}
