using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Windows.Forms;
using Rainbow;
using Rainbow.Model;
using Rainbow.WebRTC;

namespace SDK.UIForm.WebRTC
{
    public partial class FormConferenceOptions : Form
    {
        private Rainbow.Application _rbApplication;
        private Rainbow.Bubbles _rbBubbles;
        private Rainbow.Contacts _rbContacts;
        private Rainbow.Conferences _rbConferences;

        private String? _currentUserJid = null;
        private Boolean _participantOfConference = false;
        private Conference? _currentConference = null;
        private String? _currentConfId = null;

        private Boolean _canSubscribeToMediaPublication = false;

        private List<MediaPublication> _mediaPublicationsSubscribed = new List<MediaPublication>();


        public event EventHandler<MediaPublicationEventArgs>? OnMediaSubscription;

        public event EventHandler<(int media, String? publisherId, bool dynamicFeed)>? OpenVideoPublication;

#region CONSTRUCTOR
        public FormConferenceOptions()
        {
            this.HandleCreated += Form_HandleCreated;
            InitializeComponent();
        }

        private void Form_HandleCreated(object sender, EventArgs e)
        {
            btn_OutputRemoteVideoInput.BackgroundImage = Helper.GetBitmapOutput();
            btn_OutputRemoteSharingInput.BackgroundImage = Helper.GetBitmapOutput();
            btn_OutputDynamicFeedInput.BackgroundImage = Helper.GetBitmapOutput();
        }

        public void Initialize(Rainbow.Application application, Boolean canSubscribeToMediaPublication)
        {
            // Get / Store SDK Objects
            _rbApplication = application;
            _rbBubbles = _rbApplication.GetBubbles();
            _rbContacts = _rbApplication.GetContacts();
            _rbConferences = _rbApplication.GetConferences();

            _currentUserJid = _rbContacts.GetCurrentContactJid();

            // Events related to Rainbow SDK
            _rbConferences.ConferenceUpdated += Conferences_ConferenceUpdated;
            _rbConferences.ConferenceRemoved += Conferences_ConferenceRemoved;
            _rbConferences.ConferenceParticipantsUpdated += Conferences_ConferenceParticipantsUpdated;
            _rbConferences.ConferenceTalkersUpdated += Conferences_ConferenceTalkersUpdated;
            _rbConferences.ConferenceMediaPublicationsUpdated += Conferences_ConferenceMediaPublicationsUpdated;

            UpdateUIFull();
        }

        public void CanSubscribe(Boolean canSubscribeToMediaPublication)
        {
            _canSubscribeToMediaPublication = canSubscribeToMediaPublication;
        }

        public void SetConferenceInfo(String confId)
        {
            _currentConfId = confId;
            _currentConference = _rbConferences.ConferenceGetByIdFromCache(confId);

            if (_currentConference != null)
            {
                var participants = _rbConferences.ConferenceGetParticipantsFromCache(_currentConference.Id);
                if ((participants == null) || (participants?.Count == 0))
                {
                    // We need to ask a full snapshot since we kno nothing about participants
                    _rbConferences.ConferenceGetFullSnapshot(confId, callback =>
                    {
                        // What ever the result we update the UI
                        UpdateUIFull();
                    });
                    return;
                }
            }
            UpdateUIFull();
        }

        private Boolean IsMediaSubscribed(String publisherId, int media, bool dynamicFeed)
        {
            Boolean result = false;
            if (media == Call.Media.AUDIO)
                result = _mediaPublicationsSubscribed.Exists(m => m.Media == media);
            else if (dynamicFeed)
                result = _mediaPublicationsSubscribed.Exists(m => m.Media == media && m.DynamicFeed);
            else
                result = _mediaPublicationsSubscribed.Exists(m => m.Media == media && m.PublisherId == publisherId);
            return result;
        }

        private void RemoveMediaSubscribed(String publisherId, int media, bool dynamicFeed)
        {
            if (media == Call.Media.AUDIO)
                _mediaPublicationsSubscribed.RemoveAll(m => m.Media == media);
            else if (dynamicFeed)
                _mediaPublicationsSubscribed.RemoveAll(m => m.Media == media && m.DynamicFeed);
            else
                _mediaPublicationsSubscribed.RemoveAll(m => m.Media == media && m.PublisherId == publisherId);
        }

        // To use when current user subscribed or unsubscribed to a media publication
        public void UpdateMediaPublicationSubscription(Boolean subscribed, MediaPublication mediaPublication)
        {
            if (mediaPublication.CallId != _currentConfId)
                return;

            if (subscribed)
            {
                if (!IsMediaSubscribed(mediaPublication.PublisherId, mediaPublication.Media, mediaPublication.DynamicFeed))
                    _mediaPublicationsSubscribed.Add(mediaPublication);
            }
            else
            {
                if (IsMediaSubscribed(mediaPublication.PublisherId, mediaPublication.Media, mediaPublication.DynamicFeed))
                    RemoveMediaSubscribed(mediaPublication.PublisherId, mediaPublication.Media, mediaPublication.DynamicFeed);
            }

            UpdateSubscriptionButtons();
        }

        #endregion CONSTRUCTOR

        #region PRIVATE methods

        private void UpdateUIFull()
        {
            var action = new Action(() =>
            {
                UpdateUIConferenceInfo();

                UpdateUIParticipants();
                UpdateUIPublishers();
                UpdateUITalkers();

                UpdateSubscriptionButtons();
            });

            if (this.InvokeRequired)
                this.BeginInvoke(action);
            else
                action.Invoke();
        }

        private void UpdateUIMainButtons()
        {
            var enabled = (_currentConference != null) && _participantOfConference;
            btnConferenceStop.Enabled = enabled;
            btnConferenceMute.Enabled = enabled;
            btnConferenceLock.Enabled = enabled;
            btnConferenceRecordingStart.Enabled = enabled;
            btnConferenceRecordingPause.Enabled = enabled;

            UpdateSubscriptionButtons();
        }

        private void UpdateUIConferenceInfo()
        {
            var action = new Action(() =>
            {
                if (_currentConference != null)
                {
                    lbl_ConferenceInProgress.Visible = true;

                    var bubble = _rbBubbles.GetBubbleByIdFromCache(_currentConference.Id);
                    if (bubble != null)
                        lbl_ConferenceDetails.Text = bubble.Name;
                    else
                        lbl_ConferenceDetails.Text = _currentConference.Id;
                }
                else
                {
                    lbl_ConferenceInProgress.Visible = false;
                    lbl_ConferenceDetails.Text = "";
                }
            });
            if (this.InvokeRequired)
                this.BeginInvoke(action);
            else
                action.Invoke();
        }

        private void UpdateUIParticipants()
        {
            var action = new Action(() =>
            {
                lb_Participants.Items.Clear();
                if (_currentConference != null)
                {
                    Dictionary<String, Participant> participants;
                    participants = _rbConferences.ConferenceGetParticipantsFromCache(_currentConference.Id);

                    _participantOfConference = false;
                    if (participants?.Count > 0)
                    {
                        //if (_rbConferences.IsParticipantUsingJid(_currentUserJid, participants))
                        {
                            foreach (var participant in participants.Values)
                            {
                                // Check if we are a member of this conference
                                if (!_participantOfConference)
                                    _participantOfConference = participant.Jid_im == _currentUserJid;

                                String displayName = GetDisplayName(participant.Id, participant.Jid_im, participant.PhoneNumber);

                                displayName += $" [{participant.Privilege}]";
                                displayName += String.IsNullOrEmpty(participant.PhoneNumber) ? "" : $" [{participant.PhoneNumber}]";
                                displayName += participant.Muted ? " [MUTED]" : "";
                                displayName += participant.Hold ? " [HOLD]" : "";

                                ListItem item = new ListItem(displayName, participant.Id);

                                lb_Participants.Items.Add(item);
                            }
                        }
                    }

                    if (!_participantOfConference)
                        _mediaPublicationsSubscribed.Clear();
                }

                var enabled = lb_Participants.Items.Count > 0;
                btn_ParticipantMute.Enabled = enabled;
                btn_ParticipantDelegate.Enabled = enabled;
                btn_ParticipantDrop.Enabled = enabled;

                UpdateUIMainButtons();


            });
            if (this.InvokeRequired)
                this.BeginInvoke(action);
            else
                action.Invoke();


        }

        private void UpdateUITalkers()
        {
            var action = new Action(() =>
            {
                lb_ActiveTalkers.Items.Clear();
                if (_currentConference != null)
                {
                    Dictionary<String, Talker> talkers;
                    talkers = _rbConferences.ConferenceGetTalkersFromCache(_currentConference.Id);

                    if (talkers?.Count > 0)
                    {
                        foreach (var talker in talkers.Values)
                        {
                            String displayName = GetDisplayName(talker.Id, talker.Jid_im, talker.PhoneNumber);

                            displayName += String.IsNullOrEmpty(talker.PhoneNumber) ? "" : $" [{talker.PhoneNumber}]";

                            ListItem item = new ListItem(displayName, talker.Id);

                            lb_ActiveTalkers.Items.Add(item);
                        }
                    }
                }
            });

            if (this.InvokeRequired)
                this.BeginInvoke(action);
            else
                action.Invoke();
        }

        private void UpdateUIPublishers()
        {
            var action = new Action(() =>
            {
                lb_PublishersVideo.Items.Clear();
                lb_PublishersSharing.Items.Clear();
                if (_currentConference != null)
                {
                    List<MediaPublication> mediaPublications;
                    mediaPublications = _rbConferences.ConferenceGetMediaPublicationsFromCache(_currentConference.Id);

                    if (mediaPublications?.Count > 0)
                    {
                        foreach (var mediaPublication in mediaPublications)
                        {
                            String displayName = GetDisplayName(mediaPublication.PublisherId, mediaPublication.PublisherJid_im);

                            ListItem item = new ListItem(displayName, mediaPublication.PublisherId);

                            if (mediaPublication.Media == Call.Media.VIDEO)
                                lb_PublishersVideo.Items.Add(item);
                            else if (mediaPublication.Media == Call.Media.SHARING)
                                lb_PublishersSharing.Items.Add(item);
                        }
                    }
                }

                UpdateSubscriptionButtons();
            });

            if (this.InvokeRequired)
                this.BeginInvoke(action);
            else
                action.Invoke();
        }

        private void UpdateSubscriptionButtons()
        {
            var action = new Action(() =>
            {
                UpdateAudioSubscription();
                UpdateVideoSubscription();
                UpdateSharingSubscription();
            });

            if (this.InvokeRequired)
                this.BeginInvoke(action);
            else
                action.Invoke();
        }

        private void UpdateAudioSubscription()
        {
            if (_canSubscribeToMediaPublication)
            {
                btn_SubscribeRemoteAudioInput.Visible = true;

                Boolean subscribed;
                if ((_currentConfId != null) && (lb_Participants.Items.Count > 0))
                {
                    btn_SubscribeRemoteAudioInput.Enabled = true;

                    var publisherId = _currentConfId;
                    var media = Call.Media.AUDIO;
                    subscribed = IsMediaSubscribed(publisherId, media, false);
                }
                else
                {
                    btn_SubscribeRemoteAudioInput.Enabled = false;
                    subscribed = false;
                }
                btn_SubscribeRemoteAudioInput.Text = subscribed ? "Unsubscribe" : "Subscribe";
                btn_SubscribeRemoteAudioInput.ForeColor = subscribed ? System.Drawing.Color.DarkRed : System.Drawing.Color.DarkGreen;
            }
            else
            {
                btn_SubscribeRemoteAudioInput.Visible = false;
            }
        }

        private void UpdateVideoSubscription()
        {
            if (_canSubscribeToMediaPublication)
            {
                btn_SubscribeRemoteVideoInput.Visible = true;
                var media = Call.Media.VIDEO;

                // Manage Standard Video
                if (lb_PublishersVideo.SelectedItem is ListItem item)
                {
                    var publisherId = item.Value;
                    var subscribed = IsMediaSubscribed(publisherId, media, false);

                    btn_SubscribeRemoteVideoInput.Enabled = true;
                    btn_SubscribeRemoteVideoInput.Text = subscribed ? "Unsubscribe" : "Subscribe";
                    btn_SubscribeRemoteVideoInput.ForeColor = subscribed ? System.Drawing.Color.DarkRed : System.Drawing.Color.DarkGreen;

                    btn_OutputRemoteVideoInput.Visible = subscribed;
                }
                else
                {
                    btn_SubscribeRemoteVideoInput.Enabled = false;
                    btn_OutputRemoteVideoInput.Visible = false;
                }

                // Manage Dynamic Feed Video
                if (lb_PublishersVideo.SelectedItem is ListItem item2)
                {
                    var publisherId = item2.Value;
                    var subscribed = IsMediaSubscribed(publisherId, media, true);

                    btn_SubscribeDynamicFeedInput.Enabled = true;
                    btn_SubscribeDynamicFeedInput.Text = (subscribed ? "Unsubscribe" : "Subscribe") + " Dynamic Feed";
                    btn_SubscribeDynamicFeedInput.ForeColor = subscribed ? System.Drawing.Color.DarkRed : System.Drawing.Color.DarkGreen;

                    btn_OutputDynamicFeedInput.Visible = subscribed;
                }
                else
                {
                    btn_SubscribeDynamicFeedInput.Enabled = false;
                    btn_OutputDynamicFeedInput.Visible = false;
                }
            }
            else
            {
                btn_SubscribeRemoteVideoInput.Visible = false;
                btn_OutputRemoteVideoInput.Visible = false;
                btn_OutputDynamicFeedInput.Visible = false;
            }
        }

        private void UpdateSharingSubscription()
        {
            if (_canSubscribeToMediaPublication)
            {
                btn_SubscribeRemoteSharingInput.Visible = true;

                if (lb_PublishersSharing.SelectedItem is ListItem item)
                {
                    var publisherId = item.Value;
                    var media = Call.Media.SHARING;
                    var subscribed = IsMediaSubscribed(publisherId, media, false);

                    btn_SubscribeRemoteSharingInput.Enabled = true;
                    btn_SubscribeRemoteSharingInput.Text = subscribed ? "Unsubscribe" : "Subscribe";
                    btn_SubscribeRemoteSharingInput.ForeColor = subscribed ? System.Drawing.Color.DarkRed : System.Drawing.Color.DarkGreen;

                    btn_OutputRemoteSharingInput.Visible = subscribed;
                }
                else
                {
                    btn_SubscribeRemoteSharingInput.Enabled = false;
                    btn_OutputRemoteSharingInput.Visible = false;
                }
            }
            else
            {
                btn_SubscribeRemoteSharingInput.Visible = false;
                btn_OutputRemoteSharingInput.Visible = false;

            }
        }

        private void AddInformationMessage(string status)
        {
            var action = new Action(() =>
            {
                if (!String.IsNullOrEmpty(status))
                {
                    if (!status.StartsWith("\r\n"))
                        status = "\r\n" + status;

                    tbInformation.Text += "\n" + status;
                }
            });
            if (this.InvokeRequired)
                this.BeginInvoke(action);
            else
                action.Invoke();
        }

        private String GetDisplayName(String participantId, String participantJid, String participantPhoneNumber = null)
        {
            String result = "";

            if (!String.IsNullOrEmpty(participantPhoneNumber))
            {
                result = participantPhoneNumber;
            }
            else if (!String.IsNullOrEmpty(participantJid))
            {
                var contact = _rbContacts.GetContactFromContactJid(participantJid);
                if (contact != null)
                    result = Rainbow.Util.GetContactDisplayName(contact);
            }
            else if (!String.IsNullOrEmpty(participantId))
            {
                var contact = _rbContacts.GetContactFromContactId(participantId);
                if (contact != null)
                    result = Rainbow.Util.GetContactDisplayName(contact);
            }

            if (String.IsNullOrEmpty(result))
                result = participantId;

            return result;
        }

        private void OpenFormVideoOutputStreamWebRTC(int media, String? publisherId, bool dynamicFeed)
        {
            OpenVideoPublication?.Invoke(this, (media, publisherId, dynamicFeed));
        }

    #endregion PRIVATE methods

    #region EVENTS from SDK Objects

        private void Conferences_ConferenceMediaPublicationsUpdated(object? sender, Rainbow.Events.MediaPublicationsEventArgs e)
        {
            if (_currentConference?.Id == e.ConferenceId)
                UpdateUIPublishers();
        }

        private void Conferences_ConferenceTalkersUpdated(object sender, Rainbow.Events.ConferenceTalkersEventArgs e)
        {
            if (_currentConference?.Id == e.ConferenceId)
                UpdateUITalkers();
        }

        private void Conferences_ConferenceParticipantsUpdated(object sender, Rainbow.Events.ConferenceParticipantsEventArgs e)
        {
            if (_currentConference?.Id == e.ConferenceId)
                UpdateUIParticipants();
        }

        private void Conferences_ConferenceRemoved(object sender, Rainbow.Events.IdEventArgs e)
        {
            if ((_currentConference != null) && (_currentConference.Id == e.Id))
            {
                _currentConference = null;
                _mediaPublicationsSubscribed.Clear();
            }

            UpdateUIFull();
        }

        private void Conferences_ConferenceUpdated(object sender, Rainbow.Events.ConferenceEventArgs e)
        {
            if (_currentConference == null)
                _currentConference = e.Conference;
            else if (_currentConference.Id == e.Conference.Id)
                _currentConference = e.Conference;


            if ((_currentConference != null) && (_currentConference.Id == e.Conference.Id) && (!e.Conference.Active))
            {
                _currentConference = null;
                _mediaPublicationsSubscribed.Clear();
            }

            UpdateUIFull();
        }

        #endregion EVENTS from SDK Objects

        #region EVENTS from FORM elements

        private void FormConferenceOptions_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void btn_ParticipantMute_Click(object sender, EventArgs e)
        {
            String participantId = null;

            // Get participant Id
            if (lb_Participants.SelectedItem != null)
            {
                ListItem item = lb_Participants.SelectedItem as ListItem;
                participantId = item.Value;
            }

            if ((_currentConference != null) && (participantId != null))
            {
                var bubbleId = _rbConferences.GetBubbleIdByConferenceIdFromCache(_currentConference.Id);
                if (bubbleId == null)
                {
                    AddInformationMessage($"Cannot get Bubble Id from this Conference Id");
                    return;
                }

                var participants = _rbConferences.ConferenceGetParticipantsFromCache(_currentConference.Id);

                if (participants?.ContainsKey(participantId) == true)
                {
                    var participant = participants[participantId];

                    // Check if it's a PSTN Participant 
                    if (!String.IsNullOrEmpty(participant.PhoneNumber))
                    {
                        _rbConferences.ConferenceMuteOrUnmutePstnParticipant(bubbleId, participant.PhoneNumber, !participant.Muted, callback =>
                        {
                            if (callback.Result.Success)
                                AddInformationMessage($"Conference - PSTN Participant Mute / Unmute done");
                            else
                                AddInformationMessage($"Conference - PSTN Participant Pb to Mute / Unmute:[{callback.Result}]");
                        });
                    }
                    else
                    {
                        _rbConferences.ConferenceMuteOrUnmuteParticipant(bubbleId, participant.Id, !participant.Muted, callback =>
                        {
                            if (callback.Result.Success)
                                AddInformationMessage($"Conference - Participant Mute / Unmute done");
                            else
                                AddInformationMessage($"Conference - Participant Pb to Mute / Unmute:[{callback.Result}]");
                        });
                    }
                }
            }
        }

        private void btn_ParticipantDelegate_Click(object sender, EventArgs e)
        {
            String participantId = null;

            // Get participant Id
            if (lb_Participants.SelectedItem != null)
            {
                ListItem item = lb_Participants.SelectedItem as ListItem;
                participantId = item.Value;
            }

            if ((_currentConference != null) && (participantId != null))
            {
                var bubbleId = _rbConferences.GetBubbleIdByConferenceIdFromCache(_currentConference.Id);
                if (bubbleId == null)
                {
                    AddInformationMessage($"Cannot get Bubble Id from this Conference Id");
                    return;
                }

                var participants = _rbConferences.ConferenceGetParticipantsFromCache(_currentConference.Id);
                if (participants?.ContainsKey(participantId) == true)
                {
                    var participant = participants[participantId];

                    // Check if it's a PSTN Participant
                    if (!String.IsNullOrEmpty(participant.PhoneNumber))
                    {
                        AddInformationMessage($"Conference - You cannot delegate to a PSTN Participant");
                    }
                    else
                    {
                        _rbConferences.ConferenceDelegate(bubbleId, participant.Id, callback =>
                        {
                            if (callback.Result.Success)
                                AddInformationMessage($"Conference - Participant Delegate done");
                            else
                                AddInformationMessage($"Conference - Participant Pb to Delegate:[{callback.Result}]");
                        });
                    }
                }
                else
                    AddInformationMessage($"Conference - Participant not found");
            }
        }

        private void btn_ParticipantDrop_Click(object sender, EventArgs e)
        {
            String participantId = null;

            // Get participant Id
            if (lb_Participants.SelectedItem != null)
            {
                ListItem item = lb_Participants.SelectedItem as ListItem;
                participantId = item.Value;
            }

            if ((_currentConference != null) && (participantId != null))
            {
                var bubbleId = _rbConferences.GetBubbleIdByConferenceIdFromCache(_currentConference.Id);
                if (bubbleId == null)
                {
                    AddInformationMessage($"Cannot get Bubble Id from this Conference Id");
                    return;
                }

                var participants = _rbConferences.ConferenceGetParticipantsFromCache(_currentConference.Id);
                if (participants?.ContainsKey(participantId) == true)
                {
                    var participant = participants[participantId];

                    // Check if it's a PSTN Participant
                    if (!String.IsNullOrEmpty(participant.PhoneNumber))
                    {
                        _rbConferences.ConferenceDropPstnParticipant(bubbleId, participant.PhoneNumber, callback =>
                        {
                            if (callback.Result.Success)
                                AddInformationMessage($"Conference - PSTN Participant Drop done");
                            else
                                AddInformationMessage($"Conference - PSTN Participant Pb to Drop:[{callback.Result}]");
                        });
                    }
                    else
                    {
                        _rbConferences.ConferenceDropParticipant(bubbleId, participant.Id, callback =>
                        {
                            if (callback.Result.Success)
                                AddInformationMessage($"Conference - Participant Drop done");
                            else
                                AddInformationMessage($"Conference - Participant Pb to Drop:[{callback.Result}]");
                        });
                    }
                }
                else
                    AddInformationMessage($"Conference - Participant not found");
            }
        }

        private void btnConferenceMute_Click(object sender, EventArgs e)
        {
            if (_currentConference != null)
            {
                var bubbleId = _rbConferences.GetBubbleIdByConferenceIdFromCache(_currentConference.Id);
                if (bubbleId == null)
                {
                    AddInformationMessage($"Cannot get Bubble Id from this Conference Id");
                    return;
                }
                _rbConferences.ConferenceMuteOrUnmute(bubbleId, !_currentConference.Muted, callback =>
                {
                    if (callback.Result.Success)
                        AddInformationMessage($"Conference - Mute / Unmute done");
                    else
                        AddInformationMessage($"Conference - Pb Mute / Unmute:[{callback.Result}]");
                });
            }
        }

        private void btnConferenceLock_Click(object sender, EventArgs e)
        {
            if (_currentConference != null)
            {
                var bubbleId = _rbConferences.GetBubbleIdByConferenceIdFromCache(_currentConference.Id);
                if (bubbleId == null)
                {
                    AddInformationMessage($"Cannot get Bubble Id from this Conference Id");
                    return;
                }
                _rbConferences.ConferenceLockOrUnlocked(bubbleId, !_currentConference.Locked, callback =>
                {
                    if (callback.Result.Success)
                        AddInformationMessage($"Conference - Lock / Unlock done");
                    else
                        AddInformationMessage($"Conference - Pb Lock / Unlock:[{callback.Result}]");
                });
            }
        }

        private void btnConferenceStop_Click(object sender, EventArgs e)
        {
            if (_currentConference?.Active == true)
            {
                String bubbleId = _rbConferences.GetBubbleIdByConferenceIdFromCache(_currentConference.Id);
                if (bubbleId == null)
                {
                    AddInformationMessage($"Cannot get Bubble Id from this Conference Id");
                    return;
                }

                _rbConferences.ConferenceStop(bubbleId, callback =>
                {
                    if (callback.Result.Success)
                        AddInformationMessage(String.Format("Conference - Stop done"));
                    else
                        AddInformationMessage($"Pb to stop Conference:[{callback.Result}]");
                });
            }
        }

        private void btnConferenceRecordingStart_Click(object sender, EventArgs e)
        {
            if (_currentConference != null)
            {
                var bubbleId = _rbConferences.GetBubbleIdByConferenceIdFromCache(_currentConference.Id);
                if (bubbleId == null)
                {
                    AddInformationMessage($"Cannot get Bubble Id from this Conference Id");
                    return;
                }

                if (_currentConference.RecordStatus != "off")
                {
                    _rbConferences.ConferenceRecordingStop(bubbleId, callback =>
                    {
                        if (callback.Result.Success)
                            AddInformationMessage($"Conference - Recording Stop done");
                        else
                            AddInformationMessage($"Conference - Pb Recording Stop:[{callback.Result}]");
                    });
                }
                else
                {
                    _rbConferences.ConferenceRecordingStart(bubbleId, callback =>
                    {
                        if (callback.Result.Success)
                            AddInformationMessage($"Conference - Recording Start done");
                        else
                            AddInformationMessage($"Conference - Pb Recording Start:[{callback.Result}]");
                    });
                }
            }
        }

        private void btnConferenceRecordingPause_Click(object sender, EventArgs e)
        {
            if (_currentConference != null)
            {
                var bubbleId = _rbConferences.GetBubbleIdByConferenceIdFromCache(_currentConference.Id);
                if (bubbleId == null)
                {
                    AddInformationMessage($"Cannot get Bubble Id from this Conference Id");
                    return;
                }

                if (_currentConference.RecordStatus == "pause")
                {
                    _rbConferences.ConferenceRecordingResume(bubbleId, callback =>
                    {
                        if (callback.Result.Success)
                            AddInformationMessage($"Conference - Recording Resume done");
                        else
                            AddInformationMessage($"Conference - Pb Resume Pause:[{callback.Result}]");
                    });
                }
                else if (_currentConference.RecordStatus == "on")
                {
                    _rbConferences.ConferenceRecordingPause(bubbleId, callback =>
                    {
                        if (callback.Result.Success)
                            AddInformationMessage($"Conference - Recording Pause done");
                        else
                            AddInformationMessage($"Conference - Pb Recording Pause:[{callback.Result}]");
                    });
                }
            }
        }

        private void lb_ActiveTalkers_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Nothing to do here
        }

        private void lb_PublishersVideo_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateVideoSubscription();
        }

        private void lb_PublishersSharing_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSharingSubscription();
        }


        private void btn_SubscribeRemoteAudioInput_Click(object sender, EventArgs e)
        {
            if (_canSubscribeToMediaPublication)
            {
                var subscribed = IsMediaSubscribed("", Call.Media.AUDIO, false);
                OnMediaSubscription?.Raise(this, new MediaPublicationEventArgs(_currentConfId, "", "", Call.Media.AUDIO, false, subscribed ? MediaPublicationStatus.CURRENT_USER_UNSUBSCRIBED : MediaPublicationStatus.CURRENT_USER_SUBSCRIBED));
            }
        }

        private void btn_SubscribeRemoteVideoInput_Click(object sender, EventArgs e)
        {
            if (_canSubscribeToMediaPublication)
            {
                if (lb_PublishersVideo.SelectedItem is ListItem item)
                {
                    var subscribed = IsMediaSubscribed(item.Value, Call.Media.VIDEO, false);
                    OnMediaSubscription?.Raise(this, new MediaPublicationEventArgs(_currentConfId, item.Value, "", Call.Media.VIDEO, false, subscribed ? MediaPublicationStatus.CURRENT_USER_UNSUBSCRIBED : MediaPublicationStatus.CURRENT_USER_SUBSCRIBED));
                }
            }
        }

        private void btn_SubscribeDynamicFeedInput_Click(object sender, EventArgs e)
        {
            if (_canSubscribeToMediaPublication)
            {
                if (lb_PublishersVideo.SelectedItem is ListItem item)
                {
                    var subscribed = IsMediaSubscribed(item.Value, Call.Media.VIDEO, true);
                    OnMediaSubscription?.Raise(this, new MediaPublicationEventArgs(_currentConfId, item.Value, "", Call.Media.VIDEO, true, subscribed ? MediaPublicationStatus.CURRENT_USER_UNSUBSCRIBED : MediaPublicationStatus.CURRENT_USER_SUBSCRIBED));
                }
            }
        }

        private void btn_SubscribeRemoteSharingInput_Click(object sender, EventArgs e)
        {
            if (_canSubscribeToMediaPublication)
            {
                if (lb_PublishersSharing.SelectedItem is ListItem item)
                {
                    var subscribed = IsMediaSubscribed(item.Value, Call.Media.SHARING, false);
                    OnMediaSubscription?.Raise(this, new MediaPublicationEventArgs(_currentConfId, item.Value, "", Call.Media.SHARING, false, subscribed ? MediaPublicationStatus.CURRENT_USER_UNSUBSCRIBED : MediaPublicationStatus.CURRENT_USER_SUBSCRIBED));
                }
            }
        }

        private void lb_Participants_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btn_OutputRemoteVideoInput_Click(object sender, EventArgs e)
        {
            if (_canSubscribeToMediaPublication)
            {
                if (lb_PublishersVideo.SelectedItem is ListItem item)
                {
                    var publisherId = item.Value;
                    var media = Call.Media.VIDEO;

                    OpenFormVideoOutputStreamWebRTC(media, publisherId, false);
                }
            }
        }

        private void btn_OutputDynamicFeedInput_Click(object sender, EventArgs e)
        {
            if (_canSubscribeToMediaPublication)
            {
                var media = Call.Media.VIDEO;
                OpenFormVideoOutputStreamWebRTC(media, "", true);
            }
        }

        private void btn_OutputRemoteSharingInput_Click(object sender, EventArgs e)
        {
            if (_canSubscribeToMediaPublication)
            {
                if (lb_PublishersSharing.SelectedItem is ListItem item)
                {
                    var publisherId = item.Value;
                    var media = Call.Media.SHARING;
                    OpenFormVideoOutputStreamWebRTC(media, publisherId, false);
                }
            }
        }

    #endregion EVENTS from FORM elements

    }
}
