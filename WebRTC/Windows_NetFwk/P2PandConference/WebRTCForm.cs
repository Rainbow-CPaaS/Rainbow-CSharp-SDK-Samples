using Rainbow.Model;
using Rainbow.WebRTC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace Sample_P2PandConference
{
    public partial class WebRTCForm : Form
    {
        // Define delegates to update forms elements from any thread
        public delegate void VoidDelegate();
        public delegate void StringArgReturningVoidDelegate(string value);

        private const String NONE_DEVICE = "[NONE]";
        private const String FILE_DEVICE = "[FILE]";

        // DEFINE SDK OBJECTS
        Rainbow.Application? rbAplication;
        Rainbow.Contacts? rbContacts;
        Rainbow.Conferences? rbConferences;
        Rainbow.Bubbles? rbBubbles;
        Rainbow.WebRTC.Devices? rbDevices;
        Rainbow.WebRTC.WebRTCCommunications? rbWebRTCCommunications;

        private Call? currentCall = null;
        private String? currentCallId = null;
        private String? conferenceInProgressId = null;

        private String? currentContactId; // The contact Id using the SDK
        private String? currentContactJid; // The contact Jid  using the SDK
        private String? selectedContactId; // The contact id selected in the Form
        private String? selectedBubbleId; // // The bubble id selected in the Form

        private Boolean firstAudioPlayBackDevicesInit;
        private Boolean firstAudioRecordingDevicesInit;
        private Boolean firstVideoRecordingDevicesInit;
        private Boolean firstVideoSecondaryRecordingDevicesInit;

        private Boolean firstContactsListInit = false;
        private Boolean firstBubblesListInit = false;

        private List<Bubble>? bubblesList = null;
        private List<Conference.Talker>? conferenceTalkers = null;
        private List<Conference.Participant>? conferenceParticipants = null;

        private List<Conference.Publisher>? conferencePublishers = null;
        private String? selectedConferenceParticipantId;
        private String? selectedConferenceVideoPublisherId;
        private String? selectedConferenceSharingPublisherId;
        private String? selectedConferenceVideoViewerId;

        public WebRTCForm()
        {
            InitializeComponent();
        }

        public void Initialize(Rainbow.Application application)
        {
            rbAplication = application;

            // Get Rainbow SDK objects
            rbContacts = rbAplication.GetContacts();
            rbConferences = rbAplication.GetConferences();
            rbBubbles = rbAplication.GetBubbles();
            currentContactId = rbContacts.GetCurrentContactId();
            currentContactJid = rbContacts.GetCurrentContactJid();
            Rainbow.WebRTC.WebRTCCommunications.USE_ONLY_MICROPHONE_OR_HEADSET = false; // We don't use AUDIO only
            rbWebRTCCommunications = Rainbow.WebRTC.WebRTCCommunications.CreateInstance(rbAplication, ApplicationInfo.FFMPEG_LIB_PATH);
            rbDevices = rbWebRTCCommunications.GetDevices();

            // EVents related to Rainbow SDK
            rbWebRTCCommunications.CallUpdated += RbCommunication_CallUpdated;
            rbWebRTCCommunications.ConferenceMediaPublicationUpdated += RbWebRTCCommunications_ConferenceMediaPublicationUpdated;

            rbWebRTCCommunications.OnVideoLocalSourceRawSample += RbCommunication_OnVideoLocalSourceRawExtSample;
            rbWebRTCCommunications.OnVideoRemoteSourceRawSample += RbCommunication_OnVideoRemoteSourceRawExtSample;
            rbWebRTCCommunications.OnSharingLocalSourceRawSample += RbCommunication_OnSharingLocalSourceRawExtSample;
            rbWebRTCCommunications.OnSharingRemoteSourceRawSample += RbCommunication_OnSharingRemoteSourceRawExtSample;

            rbConferences.ConferenceUpdated += RbConferences_ConferenceUpdated;
            rbConferences.ConferenceRemoved += RbConferences_ConferenceRemoved;
            rbConferences.ConferenceParticipantsUpdated += RbConferences_ConferenceParticipantsUpdated;
            rbConferences.ConferenceTalkersUpdated += RbConferences_ConferenceTalkersUpdated;
            rbConferences.ConferencePublishersUpdated += RbConferences_ConferencePublishersUpdated;
            rbConferences.ConferenceSharingTransfertStatusUpdated += RbConferences_ConferenceSharingTranfertStatusUpdated;

            // Form Events related to Audio devices selection
            cb_AudioPlaybackDevices.SelectedIndexChanged += Cb_AudioPlaybackDevices_SelectedIndexChanged;
            cb_AudioSecondaryPlaybackDevices.SelectedIndexChanged += Cb_AudioSecondaryPlaybackDevices_SelectedIndexChanged;
            cb_AudioRecordingDevices.SelectedIndexChanged += Cb_AudioRecordingDevices_SelectedIndexChanged;

            // Form Events related to Contact selection
            cb_ContactsList.SelectedIndexChanged += Cb_ContactsList_SelectedIndexChanged;

            // Form Events related to Bubble selection
            cb_BubblesList.SelectedIndexChanged += Cb_BubblesList_SelectedIndexChanged;

            // Fill audio devices combo box
            firstAudioPlayBackDevicesInit = true;
            firstAudioRecordingDevicesInit = true;
            UpdateAudioPlayBackDevices();
            UpdateAudioRecordingDevices();

            UpdateAudioSecondaryPlayBackDevices();

            // Fill video devices combo box
            firstVideoRecordingDevicesInit = true;
            firstVideoSecondaryRecordingDevicesInit = true;
            UpdateVideoRecordingDevices();
            UpdateVideoSecondaryRecordingDevices();

            // Fill users Combo Box
            firstContactsListInit = true;
            UpdateContactsComboBox();

            // Fill Bubbles Combo Box
            rbBubbles.GetAllBubbles(callback =>
            {
                
                if (callback.Result.Success)
                {
                    firstBubblesListInit = true;
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


    #region EVENT FROM RAINBOW SDK OBJECTS

        private void RbConferences_ConferenceSharingTranfertStatusUpdated(object? sender, Rainbow.Events.ConferenceSharingTransfertStatusEventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if (rbConferences == null)
                return;

            if(e.ConferenceId == currentCallId)
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
                                // get Video Secondary Recording device
                                var device = GetVideoSecondaryRecordingDeviceSelected();

                                // Add Sharing
                                if (!rbWebRTCCommunications.AddSharing(currentCallId, device))
                                    AddInformationMessage("Cannot add video secondary media");
                            }

                            break;
                    }
                }));
            }
        }

        private void RbConferences_ConferenceRemoved(object? sender, Rainbow.Events.IdEventArgs e)
        {
            if (e.Id == conferenceInProgressId)
                conferenceInProgressId = null;

            AddInformationMessage($"[ConferenceRemoved] A conference is no more active - Id:[{e.Id}]");
            UpdateUIAccordingCall();
        }

        private void RbConferences_ConferenceUpdated(object? sender, Rainbow.Events.ConferenceEventArgs e)
        {
            if (e.Conference.Active)
            {
                conferenceInProgressId = e.Conference.Id;
                AddInformationMessage($"[ConferenceUpdated] A conference is active - Id:[{ e.Conference.Id}]");
            }
            else
            {
                conferenceInProgressId = null;
                AddInformationMessage($"[ConferenceUpdated] A conference is no more active - Id:[{ e.Conference.Id}]");
            }
            UpdateUIAccordingCall();
        }

        private void RbConferences_ConferencePublishersUpdated(object? sender, Rainbow.Events.ConferencePublishersEventArgs e)
        {
            if (e.ConferenceId == conferenceInProgressId)
            {
                conferencePublishers = e.Publishers.Values.ToList();
                UpdateUIAccordingCall();
            }
        }

        private void RbConferences_ConferenceTalkersUpdated(object? sender, Rainbow.Events.ConferenceTalkersEventArgs e)
        {
            if (e.ConferenceId == conferenceInProgressId)
            {
                conferenceTalkers = e.Talkers.Values.ToList();
                UpdateUIAccordingCall();
            }
        }

        private void RbConferences_ConferenceParticipantsUpdated(object? sender, Rainbow.Events.ConferenceParticipantsEventArgs e)
        {
            if (e.ConferenceId == conferenceInProgressId)
            {
                conferenceParticipants = e.Participants.Values.ToList();
                UpdateUIAccordingCall();
            }
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

        private void RbWebRTCCommunications_ConferenceMediaPublicationUpdated(object? sender, Rainbow.Events.ConferenceMediaPublicationUpdatedEventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if (currentCallId == e.ConferenceMediaPublication.ConferenceId)
            {
                AddInformationMessage($"Media Publication - Status:[{e.Status}] - PublisherId:[{e.ConferenceMediaPublication.PublisherId}] - Media:[{MediasToString(e.ConferenceMediaPublication.Media)}]");

               
                if (e.ConferenceMediaPublication.Media == Call.Media.VIDEO)
                {
                    // We want to view the last Video subscription
                    if (e.Status == ConferenceMediaPublicationStatus.CURRENT_USER_SUBSCRIBED)
                    {
                        selectedConferenceVideoViewerId = e.ConferenceMediaPublication.PublisherId;
                        AddInformationMessage($"ConferenceVideoViewerId is now:[{selectedConferenceVideoViewerId}]");
                    }
                    // We want to view the first available Video media subscription if the current one is no more avaialble
                    else if ((e.ConferenceMediaPublication.PublisherId == selectedConferenceVideoViewerId) && (e.Status == ConferenceMediaPublicationStatus.CURRENT_USER_UNSUBSCRIBED))
                    {
                        var listMediaPublicationsSubscribed = rbWebRTCCommunications.GetConferenceMediaPublicationsSubscribed(currentCallId);
                        if (listMediaPublicationsSubscribed != null)
                        {
                            foreach (var mediaPublicationsSubscribed in listMediaPublicationsSubscribed)
                            {
                                if ((mediaPublicationsSubscribed.Media == Call.Media.VIDEO) && (mediaPublicationsSubscribed.PublisherId != selectedConferenceVideoViewerId))
                                {
                                    selectedConferenceVideoViewerId = mediaPublicationsSubscribed.PublisherId;
                                    AddInformationMessage($"ConferenceVideoViewerId is now:[{selectedConferenceVideoViewerId}]");
                                    UpdateUIAccordingCall();
                                    return;
                                }
                            }
                        }
                        // We have found no valid Video Media Publication
                        selectedConferenceVideoViewerId = "";
                        AddInformationMessage($"ConferenceVideoViewerId is now:[{selectedConferenceVideoViewerId}]");
                    }
                }

                UpdateUIAccordingCall();
            }
        }

        private void RbCommunication_OnVideoLocalSourceRawExtSample(string callId, string userId, uint durationMilliseconds, int width, int height, int stride, IntPtr sample, SIPSorceryMedia.Abstractions.VideoPixelFormatsEnum pixelFormat)
        {
            UpdatePictureBox(picture_VideoLocal, callId, width, height, stride, sample, pixelFormat);
        }

        private void RbCommunication_OnVideoRemoteSourceRawExtSample(string callId, string userId, uint durationMilliseconds, int width, int height, int stride, IntPtr sample, SIPSorceryMedia.Abstractions.VideoPixelFormatsEnum pixelFormat)
        {
            if (callId == conferenceInProgressId)
            {
                // We want to view the video of the selected publisher
                if(userId == selectedConferenceVideoViewerId)
                    UpdatePictureBox(picture_VideoRemote, callId, width, height, stride, sample, pixelFormat);
            }
            else
            {
                UpdatePictureBox(picture_VideoRemote, callId, width, height, stride, sample, pixelFormat);
            }
        }

        private void RbCommunication_OnSharingLocalSourceRawExtSample(string callId, string userId, uint durationMilliseconds, int width, int height, int stride, IntPtr sample, SIPSorceryMedia.Abstractions.VideoPixelFormatsEnum pixelFormat)
        {
            UpdatePictureBox(picture_SharingLocal, callId, width, height, stride, sample, pixelFormat);
        }

        private void RbCommunication_OnSharingRemoteSourceRawExtSample(string callId, string userId, uint durationMilliseconds, int width, int height, int stride, IntPtr sample, SIPSorceryMedia.Abstractions.VideoPixelFormatsEnum pixelFormat)
        {
            UpdatePictureBox(picture_SharingRemote, callId, width, height, stride, sample, pixelFormat);
        }

        private void UpdatePictureBox(PictureBox pictureBox, string callId, int width, int height, int stride, IntPtr sample, SIPSorceryMedia.Abstractions.VideoPixelFormatsEnum pixelFormat)
        {
            //if(rawImage.pixelFormat == VideoPixelFormatsEnum.Rgb)
            //{
            //    Bitmap bmpImage = new Bitmap(rawImage.Width, rawImage.Height, rawImage.Stride, PixelFormat.Format24bppRgb, rawImage.Sample);
            //}
            
            if ((callId == currentCallId) && (pixelFormat == SIPSorceryMedia.Abstractions.VideoPixelFormatsEnum.Rgb))
            {
                try
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        //unsafe
                        {
                            //fixed (byte* s = sample)
                            {
                                try
                                {
                                    Bitmap bmpImage = new Bitmap(width, height, stride, PixelFormat.Format24bppRgb, sample);
                                    pictureBox.Image = bmpImage;
                                }
                                catch 
                                {

                                }
                            }
                        }
                    }));
                }
                catch
                {

                }
                
            }
        }

    #endregion EVENT FROM RAINBOW SDK OBJECTS

    #region EVENT FROM FORM ELEMENTS

        private void cb_VideoSecondaryRecordingDevices_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if (currentCall?.IsInProgress() == true)
            {
                Device? device = GetVideoSecondaryRecordingDeviceSelected();
                if (rbWebRTCCommunications.ChangeVideoSecondaryRecordingDevice(currentCall.Id, device))
                    AddInformationMessage($"Change to video secondary recording device:[{device?.Name}]");
                else
                    AddInformationMessage($"Cannot change of video secondary recording device");
            }
        }

        private void cb_VideoRecordingDevices_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if (currentCall?.IsInProgress() == true)
            {
                Device? device = GetVideoRecordingDeviceSelected();
                if (rbWebRTCCommunications.ChangeVideoRecordingDevice(currentCall.Id, device))
                    AddInformationMessage($"Change to video recording device:[{device?.Name}]");
                else
                    AddInformationMessage($"Cannot change of video recording device");
            }
        }

        private void Cb_AudioRecordingDevices_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (rbWebRTCCommunications == null)

                return;
            if (currentCall?.IsInProgress() == true)
            {
                Device? device = GetAudioRecordingDeviceSelected();
                if (rbWebRTCCommunications.ChangeAudioRecordingDevice(currentCall.Id, device))
                    AddInformationMessage($"Change to audio recording device:[{device?.Name}]");
                else
                    AddInformationMessage($"Cannot change of audio recording device");
            }
        }

        private void Cb_AudioPlaybackDevices_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if (currentCall?.IsInProgress() == true)
            {
                Device? device = GetAudioPlaybackDeviceSelected();
                if(rbWebRTCCommunications.ChangeAudioPlayBackDevice(currentCall.Id, device))
                    AddInformationMessage($"Change to audio plaback device:[{device?.Name}]");
                else
                    AddInformationMessage($"Cannot change of audio dlayback device");
            }
        }

        private void Cb_AudioSecondaryPlaybackDevices_SelectedIndexChanged(object? sender, EventArgs e)
        {
            //string deviceName = cb_AudioSecondaryPlaybackDevices.SelectedItem as String;
            //if (deviceName != selectedAudioSecondaryPlaybackDevice)
            //{
            //    selectedAudioSecondaryPlaybackDevice = deviceName;
            //    rbDevices.SelectMainAudioSecondaryPlaybackDeviceByName(deviceName);
            //    AddInformationMessage($"Audio Secondary Playback Device selected:[{deviceName}]");
            //}
        }

        private void Check_VideoRecordingFileWithAudio_CheckedChanged(object? sender, EventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            // We disable Audio Recoding device selection
            cb_AudioRecordingDevices.Enabled = !check_VideoRecordingFileWithAudio.Checked;

            // We set to NONE the device selected
            cb_AudioRecordingDevices.SelectedItem = NONE_DEVICE;

            if (currentCall?.IsInProgress() == true)
            {
                // Remove previous Video Recording device - must be done first
                rbWebRTCCommunications.ChangeVideoRecordingDevice(currentCall.Id, null);

                // Remove previous Audio Recording device
                rbWebRTCCommunications.ChangeAudioRecordingDevice(currentCall.Id, null);

                // Set new one
                Device? device = GetVideoRecordingDeviceSelected();
                if (rbWebRTCCommunications.ChangeVideoRecordingDevice(currentCall.Id, device))
                    AddInformationMessage($"Change to video recording device:[{device?.Name}] (with Audio)");
                else
                    AddInformationMessage($"Cannot change of video recording device (with Audio)");
            }
        }
        
        private void Cb_ContactsList_SelectedIndexChanged(object? sender, EventArgs e)
        {
            selectedContactId = GetSelectedContactId();
        }

        private void Cb_BubblesList_SelectedIndexChanged(object? sender, EventArgs e)
        {
            selectedBubbleId = GetSelectedBubbleId();
            UpdateBubblesComboBox();
        }

        private void btn_StartConf_Click(object? sender, EventArgs e)
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

            // Select devices
            List<Device> selectedDevices = new List<Device>();
            Device? device;

            // Audio playback
            device = GetAudioPlaybackDeviceSelected();
            if (device != null)
                selectedDevices.Add(device);

            // Audio Recording
            device = GetAudioRecordingDeviceSelected();
            if (device != null)
                selectedDevices.Add(device);

            // Save presence for future rollback (once the call is over)
            rbContacts.SavePresenceFromCurrentContactForRollback();

            // Get subscription info from UI
            (Boolean autoSubscriptionSharing, Boolean autoSubscriptionVideo, int maxSubscriptionVideo) = GetSubscriptionInfo();

            rbWebRTCCommunications.StartAndJoinConference(selectedBubbleId, selectedDevices, cb_SubscribeVideoLocal.Checked, cb_SubscribeSharingLocal.Checked, autoSubscriptionVideo, maxSubscriptionVideo, autoSubscriptionSharing, callback =>
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

        private void btn_JoinConf_Click(object? sender, EventArgs e)
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

            if (String.IsNullOrEmpty(conferenceInProgressId))
                return;

            // Select devices
            List<Device> selectedDevices = new List<Device>();
            Device? device;

            // Audio playback
            device = GetAudioPlaybackDeviceSelected();
            if (device != null)
                selectedDevices.Add(device);

            // Audio Recording
            device = GetAudioRecordingDeviceSelected();
            if (device != null)
                selectedDevices.Add(device);

            // Save presence for future rollback (once the call is over)
            rbContacts.SavePresenceFromCurrentContactForRollback();

            // Get subscription info from UI
            (Boolean autoSubscriptionSharing, Boolean autoSubscriptionVideo, int maxSubscriptionVideo) = GetSubscriptionInfo();

            rbWebRTCCommunications.JoinConference(conferenceInProgressId, selectedDevices, cb_SubscribeVideoLocal.Checked, cb_SubscribeSharingLocal.Checked, autoSubscriptionVideo, maxSubscriptionVideo, autoSubscriptionSharing, callback =>
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

        private void btn_MakeCall_Click(object? sender, EventArgs e)
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
            String? contactId = GetSelectedContactId();
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

                // Select devices according medias
                List<Device> selectedDevices = new List<Device>();
                Device? device;

                // Audio devices
                if (Rainbow.Util.MediasWithAudio(medias))
                {
                    // Audio playback
                    device = GetAudioPlaybackDeviceSelected();
                    if (device != null)
                        selectedDevices.Add(device);

                    // Audio Recording
                    device = GetAudioRecordingDeviceSelected();
                    if (device != null)
                        selectedDevices.Add(device);
                }

                // Video device
                if (Rainbow.Util.MediasWithVideo(medias))
                {
                    // Video Recording
                    device = GetVideoRecordingDeviceSelected();
                    if (device != null)
                        selectedDevices.Add(device);
                }

                // Video Secondary device
                if (Rainbow.Util.MediasWithSharing(medias))
                {
                    // Video Secondary Recording
                    device = GetVideoSecondaryRecordingDeviceSelected();
                    if (device != null)
                        selectedDevices.Add(device);
                }


                // Save presence for future rollback (once the call is over)
                rbContacts.SavePresenceFromCurrentContactForRollback();

                rbWebRTCCommunications.MakeCall(contactId, medias, selectedDevices, subject, callback =>
                {
                    if (callback.Result.Success)
                    {
                        currentCallId = callback.Data;

                        // Manage video and sharing subscription - local
                        rbWebRTCCommunications.P2PSubscribeToVideoLocalRawSample(currentCallId, cb_SubscribeVideoLocal.Checked);
                        rbWebRTCCommunications.P2PSubscribeToSharingLocalRawSample(currentCallId, cb_SubscribeSharingLocal.Checked);

                        // Manage video and sharing subscription - remote
                        rbWebRTCCommunications.P2PSubscribeToVideoRemoteRawSample(currentCallId, cb_P2PSubscribeVideoRemote.Checked);
                        rbWebRTCCommunications.P2PSubscribeToSharingRemoteRawSample(currentCallId, cb_P2PSubscribeSharingRemote.Checked);
                    }
                    else
                    {

                    }
                });

            }
            else
                AddInformationMessage("Cannot make call - no contact specified");

        }

        private void btn_HangUp_Click(object? sender, EventArgs e)
        {
            if ( (currentCallId != null) && (currentCall?.IsInProgress() == true))
            {
                rbWebRTCCommunications?.HangUpCall(currentCallId);
            }
            else
            {
                AddInformationMessage("Cannot hangup call - the call is not progress");
                return;
            }
        }

        private void btn_AnswerCall_Click(object? sender, EventArgs e)
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

                // Select devices according medias
                List<Device> selectedDevices = new List<Device>();
                Device? device;

                if (Rainbow.Util.MediasWithAudio(medias))
                {
                    // Audio playback
                    device = GetAudioPlaybackDeviceSelected();
                    if (device != null)
                        selectedDevices.Add(device);

                    // Audio Recording
                    device = GetAudioRecordingDeviceSelected();
                    if (device != null)
                        selectedDevices.Add(device);
                }

                if (Rainbow.Util.MediasWithVideo(medias))
                {
                    // Video Recording
                    device = GetVideoRecordingDeviceSelected();
                    if (device != null)
                        selectedDevices.Add(device);
                }


                // Video Secondary device
                if (Rainbow.Util.MediasWithSharing(medias))
                {
                    // Video Secondary Recording
                    device = GetVideoSecondaryRecordingDeviceSelected();
                    if (device != null)
                        selectedDevices.Add(device);
                }

                // Manage video and sharing subscription - local
                rbWebRTCCommunications.P2PSubscribeToVideoLocalRawSample(currentCallId, cb_SubscribeVideoLocal.Checked);
                rbWebRTCCommunications.P2PSubscribeToSharingLocalRawSample(currentCallId, cb_SubscribeSharingLocal.Checked);

                // Manage video and sharing subscription - remote
                rbWebRTCCommunications.P2PSubscribeToVideoRemoteRawSample(currentCallId, cb_P2PSubscribeVideoRemote.Checked);
                rbWebRTCCommunications.P2PSubscribeToSharingRemoteRawSample(currentCallId, cb_P2PSubscribeSharingRemote.Checked);


                rbContacts.SavePresenceFromCurrentContactForRollback();
                if (rbWebRTCCommunications.AnswerCall(currentCallId, medias, selectedDevices))
                {
                    // Something to do ?
                }
            }
        }

        private void btn_Decline_Click(object? sender, EventArgs e)
        {
            if ((currentCall != null) && (currentCall.CallStatus != Call.Status.RINGING_INCOMING))
            {
                AddInformationMessage("Cannot answer call - the call is not 'ringing incoming'");
                return;
            }
            if(currentCallId != null)
                rbWebRTCCommunications?.HangUpCall(currentCallId);
        }

        private void btn_MuteMedia_Click(object? sender, EventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if (currentCallId == null)
                return;

            if ((currentCall != null) && currentCall.IsConnected())
            {
                if(cb_MuteMedia.SelectedItem is String mediaStr)
                {
                    int media = MediasToInt(mediaStr);

                    if (media == Call.Media.AUDIO)
                    {
                        rbWebRTCCommunications.MuteAudio(currentCallId, true, callback =>
                        {
                            if(!callback.Result.Success)
                                AddInformationMessage("Cannot mute audio media");
                        });
                    }

                    if (media == Call.Media.VIDEO)
                    {
                        if (!rbWebRTCCommunications.MuteVideo(currentCallId, true))
                            AddInformationMessage("Cannot mute video media");
                    }

                    if (media == Call.Media.SHARING)
                    {
                        if (!rbWebRTCCommunications.MuteSharing(currentCallId, true))
                            AddInformationMessage("Cannot mute video media");
                    }
                }
            }
            else
            {
                AddInformationMessage("Cannot hangup call - the call is not connected");
                return;
            }
        }

        private void btn_UnmuteMedia_Click(object? sender, EventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if (currentCallId == null)
                return;

            if ((currentCall != null) && currentCall.IsConnected())
            {
                if (cb_UnmuteMedia.SelectedItem is String mediaStr)
                {
                    int media = MediasToInt(mediaStr);

                    if (media == Call.Media.AUDIO)
                    {
                        rbWebRTCCommunications.MuteAudio(currentCallId, false, callback =>
                        {
                            if (!callback.Result.Success)
                                AddInformationMessage("Cannot unmute audio media");
                        });
                    }

                    if (media == Call.Media.VIDEO)
                    {
                        if (!rbWebRTCCommunications.MuteVideo(currentCallId, false))
                            AddInformationMessage("Cannot unmute video media");
                    }

                    if (media == Call.Media.SHARING)
                    {
                        if (!rbWebRTCCommunications.MuteSharing(currentCallId, false))
                            AddInformationMessage("Cannot unmute sharing media");
                    }
                }
            }
            else
            {
                AddInformationMessage("Cannot hangup call - the call is not connected");
                return;
            }
        }

        private void btn_RemoveMedia_Click(object? sender, EventArgs e)
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
                        if (rbWebRTCCommunications.RemoveVideo(currentCallId))
                        {
                            // If we remove video, we cannot have a Video Recording device with audio set 
                            check_VideoRecordingFileWithAudio.Checked = false;
                        }
                        else
                        {
                            AddInformationMessage("Cannot remove video media");
                            
                        }
                    }
                    else if (media == Call.Media.SHARING)
                    {
                        if (!rbWebRTCCommunications.RemoveSharing(currentCallId))
                        {
                            AddInformationMessage("Cannot remove video secondary media");

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

        private void btn_AddMedia_Click(object? sender, EventArgs e)
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
                    Device? device; // To get the device used for this media


                    if (media == Call.Media.AUDIO)
                    {
                        // Select devices according medias
                        List<Device> selectedAudioDevices = new List<Device>();

                        // Audio playback
                        device = GetAudioPlaybackDeviceSelected();

                        // Audio Recording
                        var deviceRecording = GetAudioRecordingDeviceSelected();

                        if (!rbWebRTCCommunications.AddAudio(currentCallId, deviceRecording, device))
                            AddInformationMessage("Cannot add audio media");
                    }
                    else if (media == Call.Media.VIDEO)
                    {
                        // Video Recording device
                        device = GetVideoRecordingDeviceSelected();

                        if (!rbWebRTCCommunications.AddVideo(currentCallId, device))
                            AddInformationMessage("Cannot add video media");
                    }
                    else if (media == Call.Media.SHARING)
                    {
                        // Video Secondary Recording device
                        device = GetVideoSecondaryRecordingDeviceSelected();

                        if (!rbWebRTCCommunications.AddSharing(currentCallId, device))
                            AddInformationMessage("Cannot add video secondary media");
                    }
                }

            }
            else
            {
                AddInformationMessage("Cannot hangup call - the call is not connected");
                return;
            }
        }

        private void cb_AddMedia_SelectedIndexChanged(object? sender, EventArgs e)
        {
            CheckIfSharingMediaCanBeAdded();
        }

        private void cb_SubscribeVideoLocal_CheckedChanged(object? sender, EventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if ((currentCall != null) && currentCall.IsConnected())
            {
                if(currentCall.IsConference)
                    rbWebRTCCommunications.ConferenceSubscribeToVideoLocalRawSample(currentCall.Id, cb_SubscribeVideoLocal.Checked);
                else
                    rbWebRTCCommunications.P2PSubscribeToVideoLocalRawSample(currentCall.Id, cb_SubscribeVideoLocal.Checked);
            }
        }

        private void cb_SubscribeSharingLocal_CheckedChanged(object? sender, EventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if ((currentCall != null) && currentCall.IsConnected())
            {
                if (currentCall.IsConference)
                    rbWebRTCCommunications.ConferenceSubscribeToSharingLocalRawSample(currentCall.Id, cb_SubscribeSharingLocal.Checked);
                else
                    rbWebRTCCommunications.P2PSubscribeToSharingLocalRawSample(currentCall.Id, cb_SubscribeSharingLocal.Checked);
            }
        }

        private void cb_SubscribeVideoRemote_CheckedChanged(object? sender, EventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if ((currentCall != null) && currentCall.IsConnected() && !currentCall.IsConference)
                rbWebRTCCommunications.P2PSubscribeToVideoRemoteRawSample(currentCall.Id, cb_P2PSubscribeVideoRemote.Checked);
        }

        private void cb_SubscribeSharingRemote_CheckedChanged(object? sender, EventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if ((currentCall != null) && currentCall.IsConnected() && !currentCall.IsConference)
                rbWebRTCCommunications.P2PSubscribeToSharingRemoteRawSample(currentCall.Id, cb_P2PSubscribeSharingRemote.Checked);
        }

        private void btn_BrowseAudioRecordingFile_Click(object? sender, EventArgs e)
        {
            BrowseToSelectFile(tb_AudioRecordingFile, "Audio Recording file");
        }

        private void btn_BrowseVideoRecordingFile_Click(object? sender, EventArgs e)
        {
            BrowseToSelectFile(tb_VideoRecordingFile, "Video Recording file");
        }

        private void btn_BrowseVideoSecondaryRecordingFile_Click(object? sender, EventArgs e)
        {
            BrowseToSelectFile(tb_VideoSecondaryRecordingFile, "Video Secondary Recording file");
        }

        private void lb_Participants_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if(conferenceParticipants == null)
                return;

            if (currentCall == null)
                return;

            ListItem? item = lb_Participants.SelectedItem as ListItem;
            if(item != null)
            {
                // Store Id of the Participant selected
                selectedConferenceParticipantId = item.Value;

                foreach (var participant in conferenceParticipants)
                {
                    if(participant.Id == selectedConferenceParticipantId)
                    {
                        btn_MuteParticipant.Enabled = !participant.Muted;
                        btn_UnmuteParticipant.Enabled = participant.Muted;

                        // To delegate: we can't delegate ourselves
                        //          AND current call has not been initiated by current user
                        //          AND the particpant must be an moderator
                        if ( (participant.Id != currentContactId) 
                                && (currentCall.IsInitiator)
                                && (participant.Privilege == Bubble.MemberPrivilege.Moderator) )
                        {
                            btn_DelegateParticipant.Enabled = true;
                        }
                        else
                            btn_DelegateParticipant.Enabled = false;

                        return;
                    }
                }
            }
        }

        private void btn_MuteParticipant_Click(object? sender, EventArgs e)
        {
            if (rbConferences == null)
                return;

            if(selectedConferenceParticipantId != null)
            {
                rbConferences.ConferenceMuteOrUnmuteParticipant(conferenceInProgressId, selectedConferenceParticipantId, true, callback =>
                {
                    if (!callback.Result.Success)
                        AddInformationMessage($"Cannot mute ParticipantId:[{selectedConferenceParticipantId}] - Error:[{Rainbow.Util.SerializeSdkError(callback.Result)}");
                }); 
            }
        }

        private void btn_UnmuteParticipant_Click(object? sender, EventArgs e)
        {
            if (rbConferences == null)
                return;

            if (selectedConferenceParticipantId != null)
            {
                rbConferences.ConferenceMuteOrUnmuteParticipant(conferenceInProgressId, selectedConferenceParticipantId, false, callback =>
                {
                    if (!callback.Result.Success)
                        AddInformationMessage($"Cannot unmute ParticipantId:[{selectedConferenceParticipantId}] - Error:[{Rainbow.Util.SerializeSdkError(callback.Result)}");
                });
            }
        }

        private void btn_DelegateParticipant_Click(object? sender, EventArgs e)
        {
            if (rbConferences == null)
                return;

            if (selectedConferenceParticipantId != null)
            {
                rbConferences.ConferenceDelegate(conferenceInProgressId, selectedConferenceParticipantId, callback =>
                {
                    if (!callback.Result.Success)
                        AddInformationMessage($"Cannot Delegate to ParticipantId:[{selectedConferenceParticipantId}] - Error:[{Rainbow.Util.SerializeSdkError(callback.Result)}");
                });
            }
        }

        private void lb_Subscribers_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateVideoPublisherBtns();
        }

        private void btn_UnsubscribeSharingMedia_Click(object? sender, EventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if ((currentCallId != null) && (currentCallId == conferenceInProgressId))
            {
                if (selectedConferenceSharingPublisherId != null)
                    rbWebRTCCommunications.UnsubscribeToConferenceMediaPublication(currentCallId, selectedConferenceSharingPublisherId, Call.Media.SHARING, callback =>
                    {
                        if (!callback.Result.Success)
                            AddInformationMessage($"Cannot unsubscribe [{MediasToString(Call.Media.SHARING)}] to [{selectedConferenceSharingPublisherId}] - [{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                    });
            }
        }

        private void btn_SubscribeSharingMedia_Click(object? sender, EventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if ((currentCallId != null) && (currentCallId == conferenceInProgressId))
            {
                if (selectedConferenceSharingPublisherId != null)
                    rbWebRTCCommunications.SubscribeToConferenceMediaPublication(currentCallId, selectedConferenceSharingPublisherId, Call.Media.SHARING, callback =>
                    {
                        if(!callback.Result.Success)
                            AddInformationMessage($"Cannot subscribe [{MediasToString(Call.Media.SHARING)}] to [{selectedConferenceSharingPublisherId}] - [{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                    });
                        
            }
        }

        private void btn_SubscribeVideoMedia_Click(object? sender, EventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if ((currentCallId != null) && (currentCallId == conferenceInProgressId))
            {
                if (selectedConferenceVideoPublisherId != null)
                    rbWebRTCCommunications.SubscribeToConferenceMediaPublication(currentCallId, selectedConferenceVideoPublisherId, Call.Media.VIDEO, callback =>
                    {
                        if (!callback.Result.Success)
                            AddInformationMessage($"Cannot subscribe [{MediasToString(Call.Media.VIDEO)}] to [{selectedConferenceVideoPublisherId}] - [{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                    });
            }
        }

        private void btn_UnubscribeVideoMedia_Click(object? sender, EventArgs e)
        {
            if (rbWebRTCCommunications == null)
                return;

            if ((currentCallId != null) && (currentCallId == conferenceInProgressId))
            {
                if (selectedConferenceVideoPublisherId != null)
                    rbWebRTCCommunications.UnsubscribeToConferenceMediaPublication(currentCallId, selectedConferenceVideoPublisherId, Call.Media.VIDEO, callback =>
                    {
                        if (!callback.Result.Success)
                            AddInformationMessage($"Cannot unsubscribe [{MediasToString(Call.Media.VIDEO)}] to [{selectedConferenceVideoPublisherId}] - [{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                    });
            }
        }

        private void btn_ViewVideoMedia_Click(object? sender, EventArgs e)
        {
            // TODO - btn_ViewVideoMedia_Click
            if ((currentCallId != null) && (currentCallId == conferenceInProgressId))
            {
                var listItem = lb_VideoPublishers.SelectedItem as ListItem;
                if (listItem != null)
                {
                    // Store Id of the Participant selected
                    selectedConferenceVideoViewerId = listItem.Value;
                    AddInformationMessage($"ConferenceVideoViewerId is now:[{selectedConferenceVideoViewerId}]");

                    UpdateUIAccordingCall();
                }
            }
        }

        private void btn_AskToShare_Click(object? sender, EventArgs e)
        {
            if (rbConferences == null)
                return;

            if ((currentCallId != null) && (currentCallId == conferenceInProgressId))
            {
                (String toJid, String toResource) = rbConferences.ConferenceGetSharingPublisherDetails(currentCallId);

                rbConferences.ConferenceSendSharingTransfertStatus(currentCallId, toJid, toResource, ConferenceSharingTranfertStatus.REQUEST, callback =>
                {
                    if (!callback.Result.Success)
                        AddInformationMessage($"Cannot request sharing transfert: [{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                });
            }
        }

    #endregion EVENT FROM FORM ELEMENTS

        private void BrowseToSelectFile(System.Windows.Forms.TextBox targetTextBox, String messageInfoHeader)
        {
            
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                //openFileDialog.Filter = "Image PNG (*.png)|*.png|Image JPG (*.jpg)|*.jpg";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    if (filePath != null)
                    {
                        if (System.IO.File.Exists(filePath))
                        {
                            targetTextBox.Text = filePath;
                            return;
                        }
                    }
                }
            }
            AddInformationMessage($"{messageInfoHeader} can't be updated - file not exists");
        }

        // Get Audio Playback Device selected
        private Device? GetAudioPlaybackDeviceSelected()
        {
            if (rbDevices == null)
                return null;

            String? deviceName;
            Device? device = null;

            // Audio playback
            if (cb_AudioPlaybackDevices.Items?.Count > 0)
            {
                deviceName = cb_AudioPlaybackDevices.SelectedItem as String;
                
                if ((deviceName != null) && (deviceName != NONE_DEVICE))
                    device = rbDevices.GetAudioPlaybackDeviceByName(deviceName);
            }
            return device;
        }

        // Get Audio Recording Device selected
        private Device? GetAudioRecordingDeviceSelected()
        {
            if (rbDevices == null)
                return null;

            String? deviceName;
            Device? device = null;

            // Audio Recording
            if (cb_AudioRecordingDevices.Items?.Count > 0)
            {
                deviceName = cb_AudioRecordingDevices.SelectedItem as String;

                if ( (deviceName == null) || (deviceName == NONE_DEVICE) )
                {
                    // Nothing to do more
                }
                else if (deviceName == FILE_DEVICE)
                {
                    // Check file path / config
                    string path = tb_AudioRecordingFile.Text;
                    if (path?.Length > 0)
                        device = (Device)(new DeviceAudioFile("audioFile", "audioFile", path, check_AudioRecordingFileLoop.Checked));
                }
                else
                {
                    device = rbDevices.GetAudioRecordingDeviceByName(deviceName);
                }
            }

            return device;
        }

        // Get Video Recording Device selected
        private Device? GetVideoRecordingDeviceSelected()
        {
            if (rbDevices == null)
                return null;

            String? deviceName;
            Device? device = null;

            // Audio Recording
            if (cb_VideoRecordingDevices.Items?.Count > 0)
            {
                deviceName = cb_VideoRecordingDevices.SelectedItem as String;

                if ((deviceName == null) || (deviceName == NONE_DEVICE))
                {
                    // Nothing to do more
                }
                else if (deviceName == FILE_DEVICE)
                {
                    // Check file path / config
                    string path = tb_VideoRecordingFile.Text;
                    if (path?.Length > 0)
                        device = (Device)(new DeviceVideoFile("videoFile", "videoFile", path, check_VideoRecordingFileWithAudio.Checked, check_VideoRecordingFileLoop.Checked));
                }
                else
                {
                    device = rbDevices.GetVideoRecordingDeviceByName(deviceName);
                }
            }

            return device;
        }

        // Get Video Secondary Recording Device selected
        private Device? GetVideoSecondaryRecordingDeviceSelected()
        {
            if (rbDevices == null)
                return null;

            String? deviceName;
            Device? device = null;

            // Audio Recording
            if (cb_VideoSecondaryRecordingDevices.Items?.Count > 0)
            {
                deviceName = cb_VideoSecondaryRecordingDevices.SelectedItem as String;

                if ((deviceName == null) || (deviceName == NONE_DEVICE))
                {
                    // Nothing to do more
                }
                else if (deviceName == FILE_DEVICE)
                {
                    // Check file path / config
                    string path = tb_VideoSecondaryRecordingFile.Text;
                    if (path?.Length > 0)
                        device = (Device)(new DeviceVideoFile("videoFile", "videoFile", path, false, check_VideoSecondaryRecordingFileLoop.Checked));
                }
                else
                {
                    device = rbDevices.GetVideoRecordingDeviceByName(deviceName);
                }
            }

            return device;
        }

        // Get contact selected
        private String? GetSelectedContactId()
        {
            ListItem? item = cb_ContactsList.SelectedItem as ListItem;
            if (item != null)
                return item.Value;
            return null;
        }

        // Get bubble selected
        private String? GetSelectedBubbleId()
        {
            ListItem? item = cb_BubblesList.SelectedItem as ListItem;
            if (item != null)
                return item.Value;
            return null;
        }

        private (Boolean autoSubscriptionSharing, Boolean autoSubscriptionVideo, int maxSubscriptionVideo) GetSubscriptionInfo()
        {
            // Get subscription info from UI
            Boolean autoSubscriptionSharing = cb_AutoSubscriptionSharing.Checked;
            Boolean autoSubscriptionVideo = cb_AutoSubscriptionVideo.Checked;
            
            int maxSubscriptionVideo;
            if (!int.TryParse(tb_MaxSubscriptionVideo.Text, out maxSubscriptionVideo))
            {
                maxSubscriptionVideo = 4;
                tb_MaxSubscriptionVideo.Text = $"{maxSubscriptionVideo}";
            }

            return (autoSubscriptionSharing, autoSubscriptionVideo, maxSubscriptionVideo);
        }

        private Boolean CanStartConferenceFromBubbleId(String bubbleId)
        {
            if (rbBubbles == null)
                return false;

            // It's possible to start conference if user is the owner or a moderator of this bubble
            Boolean result = false;

            var bubble = rbBubbles.GetBubbleByIdFromCache(bubbleId);
            if (bubble != null)
                result = rbBubbles.IsModerator(bubble) == true;

            return result;
        }

        // To update the UI interface when the Call is updated
        private void UpdateUIAccordingCall()
        {
            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateUIAccordingCall);
                this.Invoke(d, new object[] { });
            }
            else
            {
                UpdateMakeCallComboBox();
                UpdateAnswerCallComboBox();
                UpdateBubblesComboBox();

                UpdateParticipantsListBox();
                UpdatePublishersListBox();
                UpdateActiveTalker();

                UpdateHangUpAndDeclineButtons();

                UpdateMuteUnmuteComboBox();

                UpdateAddMediaComboBox();
                UpdateRemoveMediaComboBox();

                UpdateLocalMediasCheckbox();
                UpdateRemoteMediasCheckbox();

                UpdateLoopCheckbox();

                UpdatePictureBoxes();

                UpdateP2PSubscriptionForRemoteVideoAndSharing();

                // TODO - Update other UI elements
            }
        }

        // Update Audio Playback Devices
        private void UpdateAudioPlayBackDevices()
        {
            if (rbDevices == null)
                return;

            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateAudioPlayBackDevices);
                this.Invoke(d, new object[] { });
            }
            else
            {
                UpdateDeviceComboBox(rbDevices.GetAudioPlaybackDevices(), false, cb_AudioPlaybackDevices);
                if (firstAudioPlayBackDevicesInit)
                {
                    firstAudioPlayBackDevicesInit = false;
                    SetDefaultComboboxValue("plantronic", cb_AudioPlaybackDevices);
                }
            }
        }

        // Update Audio Secondary Playback Devices
        private void UpdateAudioSecondaryPlayBackDevices()
        {
            if (rbDevices == null)
                return;

            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateAudioSecondaryPlayBackDevices);
                this.Invoke(d, new object[] { });
            }
            else
                UpdateDeviceComboBox(rbDevices.GetAudioPlaybackDevices(), false, cb_AudioSecondaryPlaybackDevices);
        }

        // Update Audio Recording Devices
        private void UpdateAudioRecordingDevices()
        {
            if (rbDevices == null)
                return;

            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateAudioRecordingDevices);
                this.Invoke(d, new object[] { });
            }
            else
            {
                UpdateDeviceComboBox(rbDevices.GetAudioRecordingDevices(), true, cb_AudioRecordingDevices);
                if (firstAudioRecordingDevicesInit)
                {
                    firstAudioRecordingDevicesInit = false;
                    SetDefaultComboboxValue("plantronic", cb_AudioRecordingDevices);
                }
            }
        }

        // Update Video Recording Devices
        private void UpdateVideoRecordingDevices()
        {
            if (rbDevices == null)
                return;

            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateVideoRecordingDevices);
                this.Invoke(d, new object[] { });
            }
            else
            {
                UpdateDeviceComboBox(rbDevices.GetVideoRecordingDevices(), true, cb_VideoRecordingDevices);
                if (firstVideoRecordingDevicesInit)
                {
                    firstVideoRecordingDevicesInit = false;
                    SetDefaultComboboxValue(FILE_DEVICE, cb_VideoRecordingDevices);
                }
            }
        }

        // Update Video Secondary Recording Devices
        private void UpdateVideoSecondaryRecordingDevices()
        {
            if (rbDevices == null)
                return;

            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateVideoSecondaryRecordingDevices);
                this.Invoke(d, new object[] { });
            }
            else
            {
                UpdateDeviceComboBox(rbDevices.GetVideoRecordingDevices(), true, cb_VideoSecondaryRecordingDevices);
                if (firstVideoSecondaryRecordingDevicesInit)
                {
                    firstVideoSecondaryRecordingDevicesInit = false;
                    SetDefaultComboboxValue(FILE_DEVICE, cb_VideoSecondaryRecordingDevices);
                }
            }
        }

        // Update Contacts Combo Box
        private void UpdateContactsComboBox()
        {
            if (rbContacts == null)
                return;

            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateContactsComboBox);
                this.Invoke(d, new object[] { });
            }
            else
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

                    if ( (cb_ContactsList.SelectedIndex == -1) && (cb_ContactsList.Items?.Count > 0) )
                        cb_ContactsList.SelectedIndex = 0;

                    if(firstContactsListInit)
                    {
                        firstContactsListInit = false;
                        SetDefaultComboboxValue("user2", cb_ContactsList);
                    }

                }
                else
                {
                    cb_ContactsList.Enabled = false;
                    cb_ContactsList.Items.Add("No user");
                    selectedContactId = null;
                }

            }
        }

        // Add information message
        private void AddInformationMessage(String message)
        {
            if (this.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(AddInformationMessage);
                this.Invoke(d, new object[] { message });
            }
            else
            {
                if (tb_Information.Text?.Length > 0)
                    tb_Information.Text += "\r\n" + message;
                else
                    tb_Information.Text += message;
            }
        }

        // Update Make Call ComboBox and Make Call button
        private void UpdateMakeCallComboBox()
        {
            if (rbAplication == null)
                return;

            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateMakeCallComboBox);
                this.Invoke(d, new object[] { });
            }
            else
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
                    canUseSharing =  rbAplication.IsCapabilityAvailable(Contact.Capability.WebRTCLocalSharing);

                    if (canUseAudio)
                    {
                        cb_MakeCallMedias.Items.Add(MediasToString(Call.Media.AUDIO));
                        if (canUseVideo)
                        {
                            cb_MakeCallMedias.Items.Add(MediasToString(Call.Media.AUDIO + Call.Media.VIDEO));
                            if (canUseSharing)
                                cb_MakeCallMedias.Items.Add(MediasToString(Call.Media.AUDIO + Call.Media.VIDEO + Call.Media.SHARING));
                        }

                        if (canUseSharing)
                            cb_MakeCallMedias.Items.Add(MediasToString(Call.Media.AUDIO + Call.Media.SHARING));
                    }

                    if (canUseVideo)
                    {
                        cb_MakeCallMedias.Items.Add(MediasToString(Call.Media.VIDEO));
                        if (canUseSharing)
                            cb_MakeCallMedias.Items.Add(MediasToString(Call.Media.VIDEO + Call.Media.SHARING));
                    }

                    if (canUseSharing)
                        cb_MakeCallMedias.Items.Add(MediasToString(Call.Media.SHARING));

                    if (cb_MakeCallMedias.Items?.Count > 0)
                        cb_MakeCallMedias.SelectedIndex = 0;
                }
            }
        }

        // Update Answer Call ComboBox and Answer Call button
        private void UpdateAnswerCallComboBox()
        {
            if (rbAplication == null)
                return;

            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateAnswerCallComboBox);
                this.Invoke(d, new object[] { });
            }
            else
            {
                cb_AnswerCallMedias.Items.Clear();

                if ((currentCall != null) && (currentCall.CallStatus == Call.Status.RINGING_INCOMING))
                {
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
                            if(canUseSharing)
                                cb_AnswerCallMedias.Items.Add(MediasToString(Call.Media.AUDIO + Call.Media.VIDEO + Call.Media.SHARING));
                        }

                        if (canUseSharing)
                            cb_AnswerCallMedias.Items.Add(MediasToString(Call.Media.AUDIO + Call.Media.SHARING));
                    }

                    if(canUseVideo)
                    {
                        cb_AnswerCallMedias.Items.Add(MediasToString(Call.Media.VIDEO));
                        if (canUseSharing)
                                cb_AnswerCallMedias.Items.Add(MediasToString(Call.Media.VIDEO + Call.Media.SHARING));
                    }

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
                    cb_AnswerCallMedias.Enabled = false;
                    btn_AnswerCall.Enabled = false;
                }
            }
        }

        // Update Bubbles ComboBox, Start Conf., Join Conf. and Snapshot Conf.
        private void UpdateBubblesComboBox()
        {
            if (bubblesList == null)
                return;

            if (rbBubbles == null)
                return;

            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateBubblesComboBox);
                this.Invoke(d, new object[] { });
            }
            else
            {
                if (firstBubblesListInit)
                {
                    // Do we need to fill the combo box ?
                    if (cb_BubblesList.Items.Count == 0)
                    {
                        foreach (var bubble in bubblesList)
                        {
                            ListItem item = new ListItem(bubble.Name, bubble.Id);
                            cb_BubblesList.Items.Add(item);
                        }

                        if(cb_BubblesList.Items?.Count > 0)
                            cb_BubblesList.SelectedIndex = 0;
                    }

                    // Display name of the Conference / Bubble in progress
                    if (conferenceInProgressId != null)
                    {
                        var bubble = rbBubbles.GetBubbleByIdFromCache(conferenceInProgressId);
                        tb_ConferenceName.Text = bubble.Name + " " + bubble.Topic;
                    }
                    else
                        tb_ConferenceName.Text = "";

                    // Enable / Disable buttons: Start conf. / Join conf.
                    if (currentCallId != null)
                    {
                        btn_JoinConf.Enabled = false;   // We can't join a conf if we are already in a call
                        btn_StartConf.Enabled = false;  // We can't start a conf if we are already in a call
                    }
                    else
                    {
                        btn_JoinConf.Enabled = (conferenceInProgressId != null); // We can join only if a conf is in progress
                        
                        if (selectedBubbleId != null)
                        {
                            var couldStart = CanStartConferenceFromBubbleId(selectedBubbleId);
                            if (couldStart)
                                tb_BubbleInfo.Text = "Owner or Moderator";
                            else
                                tb_BubbleInfo.Text = "Member";

                            // If a conf in in progress and it's the bubble selected we cannot start a conference)
                            if (selectedBubbleId == conferenceInProgressId)
                                btn_StartConf.Enabled = false;
                            else
                                btn_StartConf.Enabled = couldStart;
                        }
                        else
                            btn_StartConf.Enabled = false;
                    }
                }
                else
                {
                    // Since we don't have bubble list yet we can't start or join conf.
                    btn_JoinConf.Enabled = false;
                    btn_StartConf.Enabled = false;
                }
            }
        }

        // Update Participants List Box, Mute, Unmute, Delegate
        private void UpdateParticipantsListBox()
        {
            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateParticipantsListBox);
                this.Invoke(d, new object[] { });
            }
            else
            {
                lb_Participants.Items.Clear();

                if ( (currentCallId!= null) && (currentCallId == conferenceInProgressId) )
                {
                    if(conferenceParticipants?.Count > 0)
                    {
                        int index = 0; // We want to reselect the same user even if the list is updated (if it's possible)
                        int selectedIndex = -1;
                        foreach(var participant in conferenceParticipants)
                        {
                            if (participant.Id == selectedConferenceParticipantId)
                                selectedIndex = index;
                            index++;

                            String displayName = GetDisplayName(participant.Id, participant.Jid_im, participant.PhoneNumber);

                            displayName += $" [{participant.Privilege}]";
                            displayName += String.IsNullOrEmpty(participant.PhoneNumber) ? "" : $" [{participant.PhoneNumber}]";
                            displayName += participant.Muted ? " [MUTED]" : "";
                            displayName += participant.Hold ? " [HOLD]" : "";

                            ListItem item = new ListItem(displayName, participant.Id);

                            lb_Participants.Items.Add(item);
                        }
                        if(selectedIndex != -1)
                            lb_Participants.SelectedIndex = selectedIndex;

                        return;
                    }
                }

                selectedConferenceParticipantId = "";

                btn_MuteParticipant.Enabled = false;
                btn_UnmuteParticipant.Enabled = false;
                btn_DelegateParticipant.Enabled = false;

            }
        }


        private void UpdateVideoPublisherBtns()
        {
            if (rbWebRTCCommunications == null)
                return;

            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateVideoPublisherBtns);
                this.Invoke(d, new object[] { });
            }
            else
            {
                var listItem = lb_VideoPublishers.SelectedItem as ListItem;
                if (listItem != null)
                {
                    // Store Id of the Participant selected
                    selectedConferenceVideoPublisherId = listItem.Value;

                    var mediaPublication = new ConferenceMediaPublication(currentCallId, selectedConferenceVideoPublisherId, Call.Media.VIDEO);
                    btn_SubscribeVideoMedia.Enabled = rbWebRTCCommunications.CanSubscribeToConferenceMediaPublication(mediaPublication);
                    btn_UnsubscribeVideoMedia.Enabled = rbWebRTCCommunications.HasSubscribedToConferenceMediaPublication(mediaPublication);

                    // TODO - manage btn View Subscribed Video
                    btn_ViewVideoMedia.Enabled = btn_UnsubscribeVideoMedia.Enabled && (selectedConferenceVideoViewerId != selectedConferenceVideoPublisherId);
                }
            }
        }

        // Update Publishers List Box, Subscribe, Unsubscribe, View
        private void UpdatePublishersListBox()
        {
            if (rbWebRTCCommunications == null)
                return;

            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdatePublishersListBox);
                this.Invoke(d, new object[] { });
            }
            else
            {
                lb_VideoPublishers.Items.Clear();
                tb_RemoteSharingInfo.Text = "";

                if ((currentCallId != null) && (currentCallId == conferenceInProgressId))
                {
                    ConferenceMediaPublication mediaPublication;

                    if (conferencePublishers?.Count > 0)
                    {
                        int index = 0; // We want to reselect the same user even if the list is updated (if it's possible)
                        int selectedIndex = -1;
                        foreach (var publisher in conferencePublishers)
                        {
                            if(publisher.GetMedia("video") != null)
                            {
                                if (publisher.Id == selectedConferenceVideoPublisherId)
                                    selectedIndex = index;
                                index++;

                                String displayName = GetDisplayName(publisher.Id, publisher.Jid_im);

                                if (rbWebRTCCommunications.HasSubscribedToConferenceMediaPublication(new ConferenceMediaPublication(currentCallId, publisher.Id, Call.Media.VIDEO)))
                                {
                                    displayName += " [SUBSCRIBED]";
                                    if (publisher.Id == selectedConferenceVideoViewerId)
                                        displayName += " [VIEWING]";
                                }

                                ListItem item = new ListItem(displayName, publisher.Id);
                                lb_VideoPublishers.Items.Add(item);
                            }

                            if (publisher.GetMedia("sharing") != null)
                            {
                                selectedConferenceSharingPublisherId = publisher.Id;
                                String displayName = GetDisplayName(publisher.Id, publisher.Jid_im);
                                tb_RemoteSharingInfo.Text = displayName;
                            }
                        }

                        if ( (selectedIndex != -1) && lb_VideoPublishers.Items?.Count > selectedIndex)
                            lb_VideoPublishers.SelectedIndex = selectedIndex;

                        UpdateVideoPublisherBtns();
                    }

                    // Sharing Subscription(s) stuff
                    tb_RemoteSharingInfo.Visible = true;
                    btn_SubscribeSharingMedia.Visible = true;
                    btn_UnsubscribeSharingMedia.Visible = true;

                    mediaPublication = new ConferenceMediaPublication(currentCallId, selectedConferenceSharingPublisherId, Call.Media.SHARING);
                    btn_SubscribeSharingMedia.Enabled = rbWebRTCCommunications.CanSubscribeToConferenceMediaPublication(mediaPublication);
                    btn_UnsubscribeSharingMedia.Enabled = rbWebRTCCommunications.HasSubscribedToConferenceMediaPublication(mediaPublication);
                }
                else
                {
                    // Video Subscription(s) stuff
                    btn_SubscribeVideoMedia.Enabled = false;
                    btn_UnsubscribeVideoMedia.Enabled = false;
                    btn_ViewVideoMedia.Enabled = false;


                    // Sharing Subscription(s) stuff
                    tb_RemoteSharingInfo.Visible = false;
                    btn_SubscribeSharingMedia.Visible = false;
                    btn_UnsubscribeSharingMedia.Visible = false;
                }
            }
        }

        // Update Active Speaker
        private void UpdateActiveTalker()
        {
            if (rbContacts == null)
                return;

            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateActiveTalker);
                this.Invoke(d, new object[] { });
            }
            else
            {
                String result = "";
                if ((currentCallId != null) && (currentCallId == conferenceInProgressId))
                {
                    if (conferenceTalkers?.Count > 0)
                    {
                        List<String> resultList = new List<String>();
                        foreach(var talker in conferenceTalkers)
                        {
                            var contact = rbContacts.GetContactFromContactId(talker.Id);
                            if (contact != null)
                                resultList.Add($"[{Rainbow.Util.GetContactDisplayName(contact)}]");
                            else
                            {
                                resultList.Add($"[{talker.Id}]");

                                // We ask server to provide more info - and on success update the UI
                                rbContacts.GetContactFromContactIdFromServer(talker.Id, callback =>
                                {
                                    if (callback.Result.Success)
                                        UpdateActiveTalker();
                                });
                            }
                        }
                        if (resultList.Count > 0)
                            result = String.Join(" - ", resultList);
                    }
                }
                tb_ActiveSpeaker.Text = result;
            }
        }

        // Update Hang Up and Decline buttons
        private void UpdateHangUpAndDeclineButtons()
        {
            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateHangUpAndDeclineButtons);
                this.Invoke(d, new object[] { });
            }
            else
            {
                if (currentCall != null)
                {
                    if (currentCall.CallStatus == Call.Status.RINGING_INCOMING)
                    {
                        btn_HangUp.Enabled = false;
                        btn_Decline.Enabled = true;
                    }
                    else if (currentCall.IsInProgress())
                    {
                        btn_HangUp.Enabled = true;
                        btn_Decline.Enabled = false;
                    }
                }
                else
                {
                    btn_HangUp.Enabled = false;
                    btn_Decline.Enabled = false;
                }
            }
        }

        // Update Add Media Combo Box and Add Media button
        private void UpdateAddMediaComboBox()
        {
            if (rbAplication == null)
                return;

            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateAddMediaComboBox);
                this.Invoke(d, new object[] { });
            }
            else
            {
                Boolean enabled = false;

                if ((currentCall != null) && currentCall.IsConnected())
                {
                    cb_AddMedia.Items.Clear();

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

                    cb_AddMedia.Enabled = enabled;
                    btn_AddMedia.Enabled = enabled;

                    CheckIfSharingMediaCanBeAdded();
                }
            }
        }

        private void CheckIfSharingMediaCanBeAdded()
        {
            if (rbWebRTCCommunications == null)
                return;

            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(CheckIfSharingMediaCanBeAdded);
                this.Invoke(d, new object[] { });
            }
            else
            {
                if (cb_AddMedia.SelectedItem is String mediaStr)
                {
                    int media = MediasToInt(mediaStr); // To get the media we want to add
                    Boolean canNotAddSharing = false;
                    if (media == Call.Media.SHARING)
                    {
                        if ((currentCallId != null) && (currentCallId == conferenceInProgressId))
                            canNotAddSharing = rbWebRTCCommunications.IsConferenceSharingPublicationInProgress(currentCallId);
                        btn_AddMedia.Enabled = !canNotAddSharing;
                    }

                    btn_AskToShare.Enabled = canNotAddSharing;
                }
            }
        }

        // Update Remove Media Combo Box and Remove Media button
        private void UpdateRemoveMediaComboBox()
        {
            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateRemoveMediaComboBox);
                this.Invoke(d, new object[] { });
            }
            else
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

            }
        }

        // Update local media checkboxes
        private void UpdateLocalMediasCheckbox()
        {
            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateLocalMediasCheckbox);
                this.Invoke(d, new object[] { });
            }
            else
            {
                if ((currentCall != null) && currentCall.IsInProgress())
                {
                    check_LocalAudio.Checked =  Rainbow.Util.MediasWithAudio(currentCall.LocalMedias);
                    check_LocalVideo.Checked = Rainbow.Util.MediasWithVideo(currentCall.LocalMedias);
                    check_LocalSharing.Checked = Rainbow.Util.MediasWithSharing(currentCall.LocalMedias);

                    check_LocalAudioMuted.Checked = currentCall.IsLocalAudioMuted;
                    check_LocalVideoMuted.Checked = currentCall.IsLocalVideoMuted;
                    check_LocalSharingMuted.Checked = currentCall.IsLocalSharingMuted;
                }
                else
                {
                    check_LocalAudio.Checked = false;
                    check_LocalVideo.Checked = false;
                    check_LocalSharing.Checked = false;

                    check_LocalAudioMuted.Checked = false;
                    check_LocalVideoMuted.Checked = false;
                    check_LocalSharingMuted.Checked = false;
                }
            }
        }

        // Update remote media checkboxes
        private void UpdateRemoteMediasCheckbox()
        {
            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateRemoteMediasCheckbox);
                this.Invoke(d, new object[] { });
            }
            else
            {
                if ((currentCall != null) && currentCall.IsInProgress())
                {
                    check_RemoteAudio.Checked = Rainbow.Util.MediasWithAudio(currentCall.RemoteMedias);
                    check_RemoteVideo.Checked = Rainbow.Util.MediasWithVideo(currentCall.RemoteMedias);
                    check_RemoteSharing.Checked = Rainbow.Util.MediasWithSharing(currentCall.RemoteMedias);
                }
                else
                {
                    check_RemoteAudio.Checked = false;
                    check_RemoteVideo.Checked = false;
                    check_RemoteSharing.Checked = false;
                }
            }
        }

        // Update checboxes for loop purpose
        private void UpdateLoopCheckbox()
        {
            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateLoopCheckbox);
                this.Invoke(d, new object[] { });
            }
            else
            {
                Boolean disabled = (currentCall != null) && currentCall.IsInProgress();

                check_AudioRecordingFileLoop.Enabled = !disabled;
                check_VideoRecordingFileLoop.Enabled = !disabled;
                check_VideoSecondaryRecordingFileLoop.Enabled = !disabled;
            }
        }

        private void UpdatePictureBoxes()
        {
            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdatePictureBoxes);
                this.Invoke(d, new object[] { });
            }
            else
            {
                if ( (currentCall == null) || (currentCall?.IsInProgress() != true) )
                {
                    picture_VideoLocal.Image = null;
                    picture_VideoRemote.Image = null;

                    picture_SharingLocal.Image = null;
                    picture_SharingRemote.Image = null;
                }
                else
                {
                    if(!Rainbow.Util.MediasWithVideo(currentCall.LocalMedias))
                        picture_VideoLocal.Image = null;

                    if (!Rainbow.Util.MediasWithSharing(currentCall.LocalMedias))
                        picture_SharingLocal.Image = null;


                    if (!Rainbow.Util.MediasWithVideo(currentCall.RemoteMedias))
                        picture_VideoRemote.Image = null;

                    if (!Rainbow.Util.MediasWithSharing(currentCall.RemoteMedias))
                        picture_SharingRemote.Image = null;
                }
            }
        }

        private void UpdateP2PSubscriptionForRemoteVideoAndSharing()
        {
            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdatePictureBoxes);
                this.Invoke(d, new object[] { });
            }
            else
            {
                Boolean isP2P = true;
                if ((currentCall?.IsInProgress() == true) && currentCall.IsConference)
                    isP2P = false;

                cb_P2PSubscribeVideoRemote.Visible = isP2P;
                cb_P2PSubscribeSharingRemote.Visible = isP2P;
            }
        }

        // Update Mute / Unmute combo boxex and buttons
        private void UpdateMuteUnmuteComboBox()
        {
            if (this.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateMuteUnmuteComboBox);
                this.Invoke(d, new object[] { });
            }
            else
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
            }
        }

        private void SetDefaultComboboxValue(string containValue, ComboBox comboBox)
        {
            if (comboBox == null)
                return;

            if (comboBox.Items.Count > 0)
            {
                for(int i = 0; i < comboBox.Items.Count; i++)
                {
                    var str = comboBox.Items[i].ToString();
                    if ( (str != null) && (str.IndexOf(containValue, StringComparison.InvariantCultureIgnoreCase) != -1))
                    {
                        comboBox.SelectedIndex = i;
                        return;
                    }
                }
            }
        }

        // Methods used to fill devices combo box
        private void UpdateDeviceComboBox(List<Device> devices, Boolean withFileDevice, ComboBox comboBox)
        {
            comboBox.Items.Clear();

            if (devices?.Count > 0)
            {
                comboBox.Enabled = true;
                int index = 0;
                foreach (var device in devices)
                {
                    comboBox.Items.Add(device.Name);
                    index++;
                }

                // Add also NONE_DEVICE
                comboBox.Items.Add(NONE_DEVICE);

                if(withFileDevice)
                {
                    // Add also FILE_DEVICE
                    comboBox.Items.Add(FILE_DEVICE);
                }

                if (comboBox.Items.Count > 0)
                    comboBox.SelectedIndex = 0;
            }
            else
            {
                comboBox.Enabled = false;
                comboBox.Items.Add("No device found");
            }
        }

        private String GetDisplayName(String? participantId, String? participantJid, String? participantPhoneNumber = null)
        {
            if (rbAplication == null)
                return "";

            if (rbContacts == null)
                return "";

            String result = "";

            if (!String.IsNullOrEmpty(participantPhoneNumber))
            {
                result = participantPhoneNumber;
            }
            else if (!String.IsNullOrEmpty(participantJid))
            {
                if (participantJid == currentContactJid)
                    result = "MYSELF";
                else
                {
                    var contact = rbContacts.GetContactFromContactJid(participantJid);
                    if (contact != null)
                        result = Rainbow.Util.GetContactDisplayName(contact);
                }
            }
            else if (rbAplication.Restrictions.UseAPIConferenceV2 && !String.IsNullOrEmpty(participantId))
            {
                if (participantId == currentContactId)
                    result = "MYSELF";
                else
                {
                    var contact = rbContacts.GetContactFromContactId(participantId);
                    if (contact != null)
                        result = Rainbow.Util.GetContactDisplayName(contact);
                }
            }

            if (String.IsNullOrEmpty(result) && !String.IsNullOrEmpty(participantId))
                result = participantId;

            return result;
        }
        
        private String MediasToString(int medias)
        {
            if (medias == Call.Media.NONE)
                return "None";

            if (medias == Call.Media.AUDIO)
                return "Audio";

            if (medias == Call.Media.AUDIO + Call.Media.VIDEO)
                return "Audio+Video";

            if (medias == Call.Media.AUDIO + Call.Media.SHARING)
                return "Audio+Sharing";

            if (medias == Call.Media.VIDEO)
                return "Video";

            if (medias == Call.Media.SHARING)
                return "Sharing";

            if (medias == Call.Media.VIDEO + Call.Media.SHARING)
                return "Video+Sharing";

            if (medias == Call.Media.AUDIO + Call.Media.VIDEO  + Call.Media.SHARING)
                return "Audio+Video+Sharing";

            AddInformationMessage($"Medias specified {medias} cannot be set to String");
            return "";
        }

        private int MediasToInt(string? medias)
        {
            if (medias == null)
                return 0;

            if (medias == "None")
                return Call.Media.NONE;

            if (medias == "Audio")
                return Call.Media.AUDIO;

            if (medias == "Audio+Video")
                return Call.Media.AUDIO + Call.Media.VIDEO;

            if (medias == "Audio+Sharing")
                return Call.Media.AUDIO + Call.Media.SHARING;

            if (medias == "Video")
                return Call.Media.VIDEO;

            if (medias == "Sharing")
                return Call.Media.SHARING;

            if (medias == "Video+Sharing")
                return Call.Media.VIDEO + Call.Media.SHARING;

            if (medias == "Audio+Video+Sharing")
                return Call.Media.AUDIO + Call.Media.VIDEO + Call.Media.SHARING;

            AddInformationMessage($"Medias specified {medias} cannot be set to Int");
            return 0;
        }




    }
}
