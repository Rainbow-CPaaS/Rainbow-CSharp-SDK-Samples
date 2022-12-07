using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rainbow;
using Rainbow.Model;
using Rainbow.Medias;
using Rainbow.WebRTC;
using static System.Net.Mime.MediaTypeNames;
using WebSocketSharp;

namespace SDK.UIForm.WebRTC
{
    public partial class FormWebRTC : Form
    {
        private static String NONE = "NONE";
        private static String AUDIO = "Audio";
        private static String VIDEO = "Video";
        private static String SHARING = "Sharing";
        private static String MUTED = "Muted";

        private MediaInputStreamsManager _mediaInputStreamsManager;
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

        private String? currentContactId; // The contact Id using the SDK
        private String? currentContactJid; // The contact Jid using the SDK
        private String? selectedContactId; // The contact id selected in the Form
        private String? selectedBubbleId; // // The bubble id selected in the Form

        private SDL2AudioInput? _currentSDL2AudioInput = null; // Used to get stream from Audio Input Device (like microphone)

        private List<Bubble>? bubblesList = null;

        private String _ffmpegLibPath;
        private String? selectedConferenceVideoViewerId;

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

            // Create MediaInputStreamsManager and init necessary events
            _mediaInputStreamsManager = MediaInputStreamsManager.Instance;
            _mediaInputStreamsManager.OnListUpdated += MediaInputStreamsManager_ListUpdated;
            _mediaInputStreamsManager.OnMediaStreamStateChanged += MediaInputStreamsManager_OnMediaStreamStateChanged;

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

            rbWebRTCCommunications = Rainbow.WebRTC.WebRTCCommunications.CreateInstance(rbAplication, _ffmpegLibPath);

            // Get Id/Jid of the current user
            currentContactId = rbContacts.GetCurrentContactId();
            currentContactJid = rbContacts.GetCurrentContactJid();

            // Events related to Rainbow SDK Objects
            rbWebRTCCommunications.CallUpdated += RbCommunication_CallUpdated;

            rbConferences.ConferenceUpdated += RbConferences_ConferenceUpdated;
            rbConferences.ConferenceRemoved += RbConferences_ConferenceRemoved;
            rbConferences.ConferenceSharingTransfertStatusUpdated += RbConferences_ConferenceSharingTranfertStatusUpdated;

            // Fill users Combo Box
            UpdateContactsComboBox();

            // Fill Bubbles Combo Box
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

            // Update elements based on Call info/status
            UpdateUIAccordingCall();
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

            if (medias == Call.Media.AUDIO)
                return AUDIO;

            if (medias == Call.Media.AUDIO + Call.Media.VIDEO)
                return $"{AUDIO}+{VIDEO}";

            if (medias == Call.Media.AUDIO + Call.Media.SHARING)
                return $"{AUDIO}+{SHARING}";

            if (medias == Call.Media.VIDEO)
                return VIDEO;

            if (medias == Call.Media.SHARING)
                return SHARING;

            if (medias == Call.Media.VIDEO + Call.Media.SHARING)
                return $"{VIDEO}+{SHARING}";

            if (medias == Call.Media.AUDIO + Call.Media.VIDEO  + Call.Media.SHARING)
                return $"{AUDIO}+{VIDEO}+{SHARING}";

            AddInformationMessage($"Medias specified {medias} cannot be set to String");
            return "";
        }

        private int MediasToInt(string? medias)
        {
            if(medias == null)
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
                            if (contact.Id == selectedContactId)
                                cb_ContactsList.SelectedIndex = index;

                            cb_ContactsList.Items.Add(item);
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
                        if(currentCall == null)
                            lbl_ConferencesInProgress.Visible = true;

                        foreach (var confId in conferencesInProgress)
                        {
                            var bubble = rbBubbles.GetBubbleByIdFromCache(confId);
                            var item = new ListItem(bubble.Name + " " + bubble.Topic, confId);
                            cb_ConferencesName.Items.Add(item);
                        }

                        if (cb_ConferencesName.SelectedIndex < 0)
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
                    btn_JoinConf.Enabled =  conferencesInProgress.Count > 0; // We can join only if a conf is in progress
                    btn_DeclineConf.Enabled = conferencesInProgress.Count > 0; // We can declient only if a conf is in progress

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
                Boolean enabled = false;
                if ((currentCall != null) && currentCall.IsInProgress() && currentCall.IsConference )
                    enabled = true;

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
            this.BeginInvoke(new Action(() =>
            {
                if ((currentCall != null) && currentCall.IsInProgress())
                {
                    check_LocalAudio.Checked = Rainbow.Util.MediasWithAudio(currentCall.LocalMedias);
                    check_LocalVideo.Checked = Rainbow.Util.MediasWithVideo(currentCall.LocalMedias);
                    check_LocalSharing.Checked = Rainbow.Util.MediasWithSharing(currentCall.LocalMedias);

                    check_LocalAudio.Text = AUDIO + (!currentCall.IsLocalAudioMuted ? "" : $" [{MUTED}]");
                    check_LocalVideo.Text = VIDEO + (!currentCall.IsLocalVideoMuted ? "" : $" [{MUTED}]");
                    check_LocalSharing.Text = SHARING + (!currentCall.IsLocalSharingMuted ? "" : $" [{MUTED}]");

                    check_RemoteAudio.Checked = Rainbow.Util.MediasWithAudio(currentCall.RemoteMedias);
                    check_RemoteVideo.Checked = Rainbow.Util.MediasWithVideo(currentCall.RemoteMedias);
                    check_RemoteSharing.Checked = Rainbow.Util.MediasWithSharing(currentCall.RemoteMedias);
                }
                else
                {
                    check_LocalAudio.Checked = false;
                    check_LocalVideo.Checked = false;
                    check_LocalSharing.Checked = false;

                    check_LocalAudio.Text = AUDIO;
                    check_LocalVideo.Text = VIDEO;
                    check_LocalSharing.Text = SHARING;

                    check_RemoteAudio.Checked = false;
                    check_RemoteVideo.Checked = false;
                    check_RemoteSharing.Checked = false;
                }

                btn_OutputLocalVideoInput.Visible = check_LocalVideo.Checked;
                btn_OutputLocalSharingInput.Visible = check_LocalSharing.Checked;

                btn_OutputRemoteVideoInput.Visible = check_RemoteVideo.Checked;
                btn_OutputRemoteSharingInput.Visible = check_RemoteSharing.Checked;
            }));
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
                    if (!Rainbow.Util.MediasWithAudio(currentCall.LocalMedias))
                        cb_AddMedia.Items.Add(MediasToString(Call.Media.AUDIO));

                    if (rbAplication.IsCapabilityAvailable(Contact.Capability.WebRTCVideo) && !Rainbow.Util.MediasWithVideo(currentCall.LocalMedias))
                        cb_AddMedia.Items.Add(MediasToString(Call.Media.VIDEO));


                    if (rbAplication.IsCapabilityAvailable(Contact.Capability.WebRTCLocalSharing) && !Rainbow.Util.MediasWithSharing(currentCall.LocalMedias))
                        cb_AddMedia.Items.Add(MediasToString(Call.Media.SHARING));

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
                            if (currentCall.IsConference)
                                canNotAddSharing = rbWebRTCCommunications.IsConferenceSharingPublicationInProgress(currentCallId);
                            btn_AddMedia.Enabled = !canNotAddSharing;
                        }

                        btn_AskToShare.Enabled = canNotAddSharing;
                    }
                }

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
                        if (currentCall?.IsConference == true)
                            canNotAddSharing = rbWebRTCCommunications.IsConferenceSharingPublicationInProgress(currentCallId);
                        btn_AddMedia.Enabled = !canNotAddSharing;
                    }

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

                if ((currentCall != null) && currentCall.IsConnected())
                {
                    // /!\ Audio can never be removed

                    cb_RemoveMedia.Items.Clear();
                    if (Rainbow.Util.MediasWithVideo(currentCall.LocalMedias))
                        cb_RemoveMedia.Items.Add(MediasToString(Call.Media.VIDEO));

                    if (Rainbow.Util.MediasWithSharing(currentCall.LocalMedias))
                        cb_RemoveMedia.Items.Add(MediasToString(Call.Media.SHARING));

                    if (cb_RemoveMedia.Items?.Count > 0)
                    {
                        cb_RemoveMedia.SelectedIndex = 0;
                        enabled = true;
                    }
                }

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

        private IMedia ? GetAudioInputMediaStream()
        {
            var id = GetSelectedIdOfComboBoxUsingMediaInputStreamItem(cb_AudioInputs);
            if (String.IsNullOrEmpty(id))
            {
                if(cb_AudioInputs.SelectedItem is String audioInputName)
                {
                    // Check if use the current SDL2 Audio Input
                    if (_currentSDL2AudioInput?.Name == audioInputName)
                        return _currentSDL2AudioInput;

                    if (_currentSDL2AudioInput != null)
                    {
                        _currentSDL2AudioInput.Stop();
                        _currentSDL2AudioInput = null;
                    }

                    if (audioInputName != NONE)
                    {
                        _currentSDL2AudioInput = new SDL2AudioInput(audioInputName, audioInputName, "");
                        if (!_currentSDL2AudioInput.Init())
                            _currentSDL2AudioInput = null;
                    }

                    return _currentSDL2AudioInput;
                }
                else 
                {
                    return null;
                }
            }

            return _mediaInputStreamsManager.GetMediaStream(id);
        }

        private IMedia? GetVideoInputMediaStream()
        {
            var id = GetSelectedIdOfComboBoxUsingMediaInputStreamItem(cb_VideoInputs);
            if (String.IsNullOrEmpty(id))
                return null;

            return _mediaInputStreamsManager.GetMediaStream(id);
        }

        private IMedia? GetSharingInputMediaStream()
        {
            var id = GetSelectedIdOfComboBoxUsingMediaInputStreamItem(cb_SharingInputs);
            if (String.IsNullOrEmpty(id))
                return null;

            return _mediaInputStreamsManager.GetMediaStream(id);
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

        private Dictionary<int, IMedia>? GetMediaInputStreamsSelected(int medias)
        {
            // Get Media Inputs Stream according selection
            Dictionary<int, IMedia>? mediaStreams = new Dictionary<int, IMedia>();
            IMedia? mediaStream;

            // Audio Input Stream
            if (Rainbow.Util.MediasWithAudio(medias))
            {
                mediaStream = GetAudioInputMediaStream();
                mediaStreams.Add(Call.Media.AUDIO, mediaStream);
            }

            // Video Input Stream
            if (Rainbow.Util.MediasWithVideo(medias))
            {
                mediaStream = GetVideoInputMediaStream();
                mediaStreams.Add(Call.Media.VIDEO, mediaStream);
            }

            // Sharing Input Stream
            if (Rainbow.Util.MediasWithSharing(medias))
            {
                mediaStream = GetSharingInputMediaStream();
                mediaStreams.Add(Call.Media.SHARING, mediaStream);
            }

            if (mediaStreams.Count == 0)
                mediaStreams = null;

            return mediaStreams;
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
                if (!rbWebRTCCommunications.MuteVideo(currentCallId, muteOn))
                    AddInformationMessage($"Cannot {muteStr} video media");
            }

            if (media == Call.Media.SHARING)
            {
                if (!rbWebRTCCommunications.MuteSharing(currentCallId, muteOn))
                    AddInformationMessage($"Cannot {muteStr} sharing media");
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


#region EVENTS from Rainbow SDK Objects

        private void RbConferences_ConferenceSharingTranfertStatusUpdated(object? sender, Rainbow.Events.ConferenceSharingTransfertStatusEventArgs e)
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

                    switch (e.ConferenceSharingTranfertStatus)
                    {
                        case ConferenceSharingTranfertStatus.REQUEST:

                            ConferenceSharingTranfertStatus status;

                            message = $"[{name}] wants to start a screen sharing. Do you accept ?";
                            result = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (result == System.Windows.Forms.DialogResult.Yes)
                            {
                                if (rbWebRTCCommunications.RemoveSharing(currentCallId))
                                    status = ConferenceSharingTranfertStatus.ACCEPT;
                                else
                                {
                                    AddInformationMessage("Cannot remove video secondary media");
                                    status = ConferenceSharingTranfertStatus.REFUSE;
                                }

                            }
                            else
                                status = ConferenceSharingTranfertStatus.REFUSE;

                            // We inform the Peer about the answer
                            rbConferences.ConferenceSendSharingTransfertStatus(currentCallId, e.FromJid, e.FromResource, status);
                            break;

                        case ConferenceSharingTranfertStatus.REFUSE:
                            message = $"[{name}] refuses your request to start a screen sharing.";
                            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;

                        case ConferenceSharingTranfertStatus.ACCEPT:
                            message = $"[{name}] has stopped sharing his screen request. You may now share yours.";
                            result = MessageBox.Show(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

                            if (result == System.Windows.Forms.DialogResult.OK)
                            {
                                // Add Sharing
                                var mediaStream = GetSharingInputMediaStream();
                                if (!rbWebRTCCommunications.AddSharing(currentCallId, mediaStream))
                                    AddInformationMessage("Cannot add sharing media");
                            }

                            break;
                    }
                }));
            }
        }

        private void RbConferences_ConferenceRemoved(object? sender, Rainbow.Events.IdEventArgs e)
        {
            conferencesInProgress.Remove(e.Id);

            AddInformationMessage($"[ConferenceRemoved] A conference is no more active - Id:[{e.Id}]");
            UpdateUIAccordingCall();
        }

        private void RbConferences_ConferenceUpdated(object? sender, Rainbow.Events.ConferenceEventArgs e)
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

        private void RbCommunication_CallUpdated(object? sender, Rainbow.Events.CallEventArgs e)
        {
            if (e.Call == null)
                return;

            if (rbContacts == null)
                return;

            // If we don't currently manage a call, we take the first new one into account
            if (currentCallId == null)
                currentCallId = e.Call.Id;

            if (e.Call.Id == currentCallId)
            {
                if (!e.Call.IsInProgress())
                {
                    currentCallId = null;
                    currentCall = null;

                    // The call is NO MORE in Progress => We Rollback presence if any
                    rbContacts.RollbackPresenceSavedFromCurrentContact();
                }
                else 
                {
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

#endregion EVENTS from Rainbow SDK Objects


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
                    rbWebRTCCommunications.ChangeVideo(currentCall.Id, mediaSream);
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
                    rbWebRTCCommunications.ChangeSharing(currentCall.Id, mediaSream);
            }
        }

        private void cb_AudioInputList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var mediaSream = GetAudioInputMediaStream();
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
                if (Rainbow.Util.MediasWithAudio(currentCall.LocalMedias))
                    rbWebRTCCommunications.ChangeAudio(currentCall.Id, mediaSream);
            }

        }

        private void cb_AudioOutputList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // TODO - cb_AudioInputList_SelectedIndexChanged
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
                form.SetMediaVideo(mediaVideo);
                form.Show();
            }
        }

        private void btn_OutputSharingInput_Click(object sender, EventArgs e)
        {
            IMedia? mediaStream = GetSharingInputMediaStream();
            if (mediaStream is IMediaVideo mediaVideo)
            {
                var form = new FormVideoOutputStream();
                form.SetMediaVideo(mediaVideo);
                form.Show();
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
                Dictionary<int, IMedia>? mediaStreams = GetMediaInputStreamsSelected(medias);

                // Save presence for future rollback (once the call is over)
                rbContacts.SavePresenceFromCurrentContactForRollback();

                // TODO - Make call using IMedia list 
                rbWebRTCCommunications.MakeCall(contactId, mediaStreams, subject, callback =>
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
                Dictionary<int, IMedia>? mediaStreams = GetMediaInputStreamsSelected(medias);

                rbContacts.SavePresenceFromCurrentContactForRollback();
                if (rbWebRTCCommunications.AnswerCall(currentCallId, mediaStreams))
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
                if(currentCall.CallStatus == Call.Status.RINGING_INCOMING)
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
                    IMedia? mediaStream;

                    if (media == Call.Media.AUDIO)
                    {
                        mediaStream = GetAudioInputMediaStream();
                        if (!rbWebRTCCommunications.AddAudio(currentCallId, mediaStream))
                            AddInformationMessage("Cannot add audio media");
                    }
                    else if (media == Call.Media.VIDEO)
                    {
                        mediaStream = GetVideoInputMediaStream();
                        if (!rbWebRTCCommunications.AddVideo(currentCallId, mediaStream))
                            AddInformationMessage("Cannot add video media");
                    }
                    else if (media == Call.Media.SHARING)
                    {
                        mediaStream = GetSharingInputMediaStream();
                        if (!rbWebRTCCommunications.AddSharing(currentCallId, mediaStream))
                            AddInformationMessage("Cannot add sharing media");
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

                rbConferences.ConferenceSendSharingTransfertStatus(currentCallId, toJid, toResource, ConferenceSharingTranfertStatus.REQUEST, callback =>
                {
                    if (!callback.Result.Success)
                        AddInformationMessage($"Cannot request sharing transfert: [{Rainbow.Util.SerializeSdkError(callback.Result)}]");
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

        private void btn_OutputRemoteVideoInput_Click(object sender, EventArgs e)
        {

        }

        private void btn_OutputRemoteSharingInput_Click(object sender, EventArgs e)
        {

        }

        private void btn_OutputLocalVideoInput_Click(object sender, EventArgs e)
        {
            IMedia? mediaStream = GetVideoInputMediaStream();
            if (mediaStream is IMediaVideo mediaVideo)
            {
                var form = new FormVideoOutputStream();
                form.SetMediaVideo(mediaVideo);
                form.Show();
            }
        }

        private void btn_OutputLocalSharingInput_Click(object sender, EventArgs e)
        {
            IMedia? mediaStream = GetSharingInputMediaStream();
            if (mediaStream is IMediaVideo mediaVideo)
            {
                var form = new FormVideoOutputStream();
                form.SetMediaVideo(mediaVideo);
                form.Show();
            }
        }

        private void btn_StartMediaInput_Click(object sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.Button btn)
            {
                String? id = null;
                switch(btn.Name)
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

            rbWebRTCCommunications.StartAndJoinConference(selectedBubbleId, mediaStreams, false, 0, false, callback =>
            {
                if (callback.Result.Success)
                {
                    currentCallId = callback.Data;
                }
                else
                {
                    AddInformationMessage($"Cannot start conference: [{Rainbow.Util.SerializeSdkError(callback.Result)}]");
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
            Dictionary<int, IMedia>? mediaStreams = null;
            var mediaStream = GetAudioInputMediaStream();
            if (mediaStream != null)
            {
                mediaStreams = new Dictionary<int, IMedia>();
                mediaStreams.Add(Call.Media.AUDIO, mediaStream);
            }

            // Save presence for future rollback (once the call is over)
            rbContacts.SavePresenceFromCurrentContactForRollback();

            rbWebRTCCommunications.JoinConference(conferenceInProgressId, mediaStreams, false, 0, false, callback =>
            {
                if (callback.Result.Success)
                {
                    currentCallId = callback.Data;
                }
                else
                {
                    AddInformationMessage($"Cannot join conference: [{Rainbow.Util.SerializeSdkError(callback.Result)}]");
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
                    if(!callback.Result.Success)
                        AddInformationMessage($"Cannot declinc conf - {Util.SerializeSdkError(callback.Result)}");
                });
            }
        }

        private void cb_AddMedia_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckIfSharingMediaCanBeAdded();
        }

        private void btn_ConferenceOptions_Click(object sender, EventArgs e)
        {
            if (currentCall?.IsConference == true)
            {
                if (_formConferenceOptions == null)
                {
                    _formConferenceOptions = new FormConferenceOptions();
                    _formConferenceOptions.Initialize(rbAplication);
                }

                // Set conference Info
                _formConferenceOptions.SetConferenceInfo(currentCall.ConferenceId, currentCall.ConferenceJid);

                _formConferenceOptions.WindowState = FormWindowState.Normal;
                _formConferenceOptions.Show();
            }
        }

#endregion EVENTS from UI Elements of the form
    }
}
