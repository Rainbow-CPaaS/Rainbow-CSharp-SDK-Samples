using System;
using System.Collections.Generic;
using System.Linq;
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
        private String? _currentUserId = null;

        private Boolean _participantOfConference = false;
        private Conference? _currentConference = null;
        private String? _currentConfId = null;

        private Boolean _canSubscribeToMedias = false;
        private Object _lockUI = new object();
        private Boolean _updatingUI = false;

        List<ListItem> _participantsList = new List<ListItem>();

        private List<MediaPublication> _mediaPublicationsSubscribed = new List<MediaPublication>();
        private List<MediaService> _mediaServicesSubscribed = new List<MediaService>();

        private List<String> _unknownContactsById = new List<String>();
        private CancelableDelay? _cancelableDelayForUnknowContacts = null;

        public event EventHandler<MediaPublicationEventArgs>? OnCloseDataChannel;
        public event EventHandler<MediaPublicationEventArgs>? OnMediaPublicationSubscription;
        public event EventHandler<MediaServiceEventArgs>? OnMediaServiceSubscription;

        public event EventHandler<(int media, String? publisherId, bool dynamicFeed)>? OpenVideoPublication;
        public event EventHandler<String>? OpenMediaService;

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
            btn_OutputMediaServiceInput.BackgroundImage = Helper.GetBitmapOutput();

            btn_CloseRemoteDataChannelInput.BackgroundImage = Helper.GetBitmapStop();
        }

        public void Initialize(Rainbow.Application application, Boolean canSubscribeToMediaPublication)
        {
            // Get / Store SDK Objects
            _rbApplication = application;
            _rbBubbles = _rbApplication.GetBubbles();
            _rbContacts = _rbApplication.GetContacts();
            _rbConferences = _rbApplication.GetConferences();

            _currentUserJid = _rbContacts.GetCurrentContactJid();
            _currentUserId = _rbContacts.GetCurrentContactId();

            // Events related to Rainbow SDK
            _rbBubbles.BubbleMemberUpdated += Bubbles_BubbleMemberUpdated;

            _rbConferences.ConferenceUpdated += Conferences_ConferenceUpdated;
            _rbConferences.ConferenceRemoved += Conferences_ConferenceRemoved;
            _rbConferences.ConferenceRaisedHandUpdated += Conferences_ConferenceRaisedHandUpdated;
            _rbConferences.ConferenceParticipantsUpdated += Conferences_ConferenceParticipantsUpdated;
            _rbConferences.ConferenceTalkersUpdated += Conferences_ConferenceTalkersUpdated;
            _rbConferences.ConferenceMediaPublicationsUpdated += Conferences_ConferenceMediaPublicationsUpdated;

            UpdateUIFull();
        }

        public void CanSubscribe(Boolean canSubscribeToMediaPublication)
        {
            _canSubscribeToMedias = canSubscribeToMediaPublication;
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

        private Boolean IsMediaPublicationSubscribed(String publisherId, int media, bool dynamicFeed)
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

        private MediaService? GetMediaService(String serviceId)
        {
            if (_currentConfId != null)
            {
                var msList = _rbConferences.ConferenceGetMediaServicesFromCache(_currentConfId);
                return msList.Find(ms => ms.ServiceId == serviceId);
            }
            return null;
        }

        private Boolean IsMediaServiceSubscribed(String serviceId)
        {
            Boolean result = false;
            result = _mediaServicesSubscribed.Exists(ms => ms.ServiceId == serviceId);

            return result;
        }

        private void RemoveMediaServiceSubscribed(String serviceId)
        {
            _mediaServicesSubscribed.RemoveAll(ms => ms.ServiceId == serviceId);
        }

        private void RemoveMediaPublicationSubscribed(String publisherId, int media, bool dynamicFeed)
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
                if (!IsMediaPublicationSubscribed(mediaPublication.PublisherId, mediaPublication.Media, mediaPublication.DynamicFeed))
                    _mediaPublicationsSubscribed.Add(mediaPublication);
            }
            else
            {
                if (IsMediaPublicationSubscribed(mediaPublication.PublisherId, mediaPublication.Media, mediaPublication.DynamicFeed))
                    RemoveMediaPublicationSubscribed(mediaPublication.PublisherId, mediaPublication.Media, mediaPublication.DynamicFeed);
            }

            UpdateUIFull();
        }

        public void UpdateMediaServiceSubscription(Boolean subscribed, MediaService mediaService)
        {
            if (mediaService.CallId != _currentConfId)
                return;

            if (subscribed)
            {
                if (!IsMediaServiceSubscribed(mediaService.ServiceId))
                    _mediaServicesSubscribed.Add(mediaService);
            }
            else
            {
                if (IsMediaServiceSubscribed(mediaService.ServiceId))
                    RemoveMediaServiceSubscribed(mediaService.ServiceId);
            }

            UpdateUIFull();
        }

        #endregion CONSTRUCTOR

        #region PRIVATE methods

        private void UpdateUIFull()
        {
            var action = new Action(() =>
            {
                lock (_lockUI)
                {
                    if (_updatingUI)
                        return;
                    _updatingUI = true;

                    UpdateUIConferenceInfo();

                    UpdateUIParticipants();
                    UpdateUIMembers();
                    UpdateUIPublishers();
                    UpdateUITalkers();

                    UpdateUIMainButtons();

                    UpdateSubscriptionButtons();

                    _updatingUI = false;
                }
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
        }

        private void UpdateUIConferenceInfo()
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
        }

        private void UpdateUIMembers()
        {
            var action = new Action(() =>
            {
                lb_Members.Items.Clear();
                if (_currentConference != null)
                {
                    List<BubbleMember> members;
                    members = _rbBubbles.GetMembersFromCache(_currentConference.Id);

                    if (members?.Count > 0)
                    {
                        ListItem? currentUserItem = null;
                        ListItem? owner = null;
                        List<ListItem> moderators = new List<ListItem>();
                        List<ListItem> users = new List<ListItem>();

                        //if (_rbConferences.IsParticipantUsingJid(_currentUserJid, participants))
                        {
                            foreach (var member in members)
                            {
                                // Avoid to display a participant in the conf
                                if (_participantsList.Exists(p => p.Value == member.UserId))
                                    continue;

                                String displayName = "";
                                var contact = _rbContacts.GetContactFromContactId(member.UserId);
                                if (contact?.IsTv == true)
                                    displayName = "📺";

                                if (member.Privilege == Bubble.MemberPrivilege.Owner)
                                    displayName += $"💎";
                                else if (member.Privilege == Bubble.MemberPrivilege.Moderator)
                                    displayName += $"👑";
                                else if ((member.Privilege == Bubble.MemberPrivilege.User) || (member.Privilege == Bubble.MemberPrivilege.Member))
                                    displayName += $"🙂";
                                else //if (participant.Privilege == Bubble.MemberPrivilege.Guest)
                                    displayName += $"❓";

                                displayName += GetDisplayName(member.UserId, "");

                                ListItem item = new ListItem(displayName, member.UserId);

                                if (member.UserId == _currentUserId)
                                    currentUserItem = item;
                                else
                                {
                                    if (member.Privilege == Bubble.MemberPrivilege.Owner)
                                        owner = item;
                                    else if (member.Privilege == Bubble.MemberPrivilege.Moderator)
                                        moderators.Add(item);
                                    else
                                        users.Add(item);
                                }
                            }

                            moderators.Sort((i1, i2) => i1.Text.CompareTo(i2.Text));
                            users.Sort((i1, i2) => i1.Text.CompareTo(i2.Text));

                            if (currentUserItem != null)
                                lb_Members.Items.Add(currentUserItem);
                            if (owner != null)
                                lb_Members.Items.Add(owner);

                            lb_Members.Items.AddRange(moderators.ToArray());
                            lb_Members.Items.AddRange(users.ToArray());
                        }
                    }

                }
            });
            if (this.InvokeRequired)
                this.BeginInvoke(action);
            else
                action.Invoke();
        }

        private void UpdateUIParticipants()
        {
            lb_Participants.Items.Clear();
            _participantsList.Clear();
            if (_currentConference != null)
            {
                var participants = _rbConferences.ConferenceGetParticipantsFromCache(_currentConference.Id);
                var participantsRaisedHands = _rbConferences.ConferenceGetRaisedHandsFromCache(_currentConference.Id);

                _participantOfConference = false;
                if (participants?.Count > 0)
                {

                    ListItem? currentUserItem = null;
                    ListItem? owner = null;
                    List<ListItem> moderators = new List<ListItem>();
                    List<ListItem> users = new List<ListItem>();

                    //if (_rbConferences.IsParticipantUsingJid(_currentUserJid, participants))
                    {
                        foreach (var participant in participants.Values)
                        {
                            // Check if we are a member of this conference
                            if (!_participantOfConference)
                                _participantOfConference = participant.Jid_im == _currentUserJid;

                            String displayName = "";
                            var contact = _rbContacts.GetContactFromContactId(participant.Id);
                            if (contact?.IsTv == true)
                                displayName = "📺";

                            if (participant.Privilege == Bubble.MemberPrivilege.Owner)
                                displayName += $"💎";
                            else if (participant.Privilege == Bubble.MemberPrivilege.Moderator)
                                displayName += $"👑";
                            else if ( (participant.Privilege == Bubble.MemberPrivilege.User) || (participant.Privilege == Bubble.MemberPrivilege.Member) )
                                displayName += $"🙂";
                            else //if (participant.Privilege == Bubble.MemberPrivilege.Guest)
                                displayName += $"❓";

                            displayName += participant.Muted ? "🔇" : "🔈";
                            displayName += participant.Hold ? "⌛" : "";

                            if (participantsRaisedHands?.Contains(participant.Id) == true)
                                displayName += "🖐️";
                            else
                                displayName += "";

                            displayName += GetDisplayName(participant.Id, participant.Jid_im, participant.PhoneNumber);
                            displayName += String.IsNullOrEmpty(participant.PhoneNumber) ? "" : $" [{participant.PhoneNumber}]";

                            ListItem item = new ListItem(displayName, participant.Id);

                            if (participant.Jid_im == _currentUserJid)
                                currentUserItem = item;
                            else
                            {
                                if (participant.Privilege == Bubble.MemberPrivilege.Owner)
                                    owner = item;
                                else if (participant.Privilege == Bubble.MemberPrivilege.Moderator)
                                    moderators.Add(item);
                                else
                                    users.Add(item);
                            }

                        }

                        moderators.Sort((i1, i2) => i1.Text.CompareTo(i2.Text));
                        users.Sort((i1, i2) => i1.Text.CompareTo(i2.Text));

                        if (currentUserItem != null)
                            _participantsList.Add(currentUserItem);
                        if (owner != null)
                            _participantsList.Add(owner);

                        _participantsList.AddRange(moderators.ToArray());
                        _participantsList.AddRange(users.ToArray());
                        lb_Participants.Items.AddRange(_participantsList.ToArray());
                    }
                }

                if (!_participantOfConference)
                {
                    _mediaPublicationsSubscribed.Clear();
                    _mediaServicesSubscribed.Clear();

                    _currentConference = null;
                    UpdateUIFull();
                }
            }

            var enabled = lb_Participants.Items.Count > 0;
            btn_ParticipantMute.Enabled = enabled;
            btn_ParticipantDelegate.Enabled = enabled;
            btn_ParticipantDrop.Enabled = enabled;

            btn_ParticipantLowerAllHands.Enabled = enabled;
            btn_ParticipantLowerHand.Enabled = enabled;
            btn_RaiseHand.Enabled = enabled;
        }

        private void UpdateUITalkers()
        {
            lb_ActiveTalkers.Items.Clear();
            if (_currentConference != null)
            {
                Dictionary<String, Talker> talkers;
                talkers = _rbConferences.ConferenceGetTalkersFromCache(_currentConference.Id);

                List<ListItem> list = new List<ListItem>();
                if (talkers?.Count > 0)
                {
                    foreach (var talker in talkers.Values)
                    {
                        String displayName = GetDisplayName(talker.Id, talker.Jid_im, talker.PhoneNumber);
                        displayName += String.IsNullOrEmpty(talker.PhoneNumber) ? "" : $" [{talker.PhoneNumber}]";
                        list.Add(new ListItem(displayName, talker.Id));
                    }
                    list.Sort((i1, i2) => i1.Text.CompareTo(i2.Text));

                    lb_ActiveTalkers.Items.AddRange(list.ToArray());
                }
            }
        }

        private void UpdateUIPublishers()
        {

            lb_PublishersVideo.Items.Clear();
            lb_PublishersSharing.Items.Clear();
            lb_PublishersDataChannel.Items.Clear();
            lb_MediaService.Items.Clear();
            if (_currentConference != null)
            {
                List<MediaPublication> mediaPublications = _rbConferences.ConferenceGetMediaPublicationsFromCache(_currentConference.Id);
                if (mediaPublications?.Count > 0)
                {
                    List<ListItem> videoPublishers = new List<ListItem>();
                    List<ListItem> sharingPublishers = new List<ListItem>();
                    List<ListItem> dataChannelPublishers = new List<ListItem>();
                    foreach (var mediaPublication in mediaPublications)
                    {
                        String displayName = GetDisplayName(mediaPublication.PublisherId, mediaPublication.PublisherJid_im);

                        ListItem item = new ListItem(displayName, mediaPublication.PublisherId);

                        if (mediaPublication.Media == Call.Media.VIDEO)
                            videoPublishers.Add(item);
                        else if (mediaPublication.Media == Call.Media.SHARING)
                            sharingPublishers.Add(item);
                        else if (mediaPublication.Media == Call.Media.DATACHANNEL)
                            dataChannelPublishers.Add(item);
                    }

                    videoPublishers.Sort((i1, i2) => i1.Text.CompareTo(i2.Text));
                    sharingPublishers.Sort((i1, i2) => i1.Text.CompareTo(i2.Text));
                    dataChannelPublishers.Sort((i1, i2) => i1.Text.CompareTo(i2.Text));


                    lb_PublishersVideo.Items.AddRange(videoPublishers.ToArray());
                    lb_PublishersSharing.Items.AddRange(sharingPublishers.ToArray());
                    lb_PublishersDataChannel.Items.AddRange(dataChannelPublishers.ToArray());
                }

                List<MediaService> mediaServices = _rbConferences.ConferenceGetMediaServicesFromCache(_currentConference.Id);
                if (mediaServices?.Count > 0)
                {
                    List<ListItem> mediaServicePublishers = new List<ListItem>();
                    foreach (var mediaService in mediaServices)
                    {
                        String displayName = $"{mediaService.ServiceId} - {mediaService.ServiceType}";

                        ListItem item = new ListItem(displayName, mediaService.ServiceId);
                        mediaServicePublishers.Add(item);
                    }

                    lb_MediaService.Items.AddRange(mediaServicePublishers.ToArray());
                }
            }
        }

        private void UpdateSubscriptionButtons()
        {
            UpdateAudioSubscription();
            UpdateVideoSubscription();
            UpdateSharingSubscription();
            UpdateDataChannelSubscription();
            UpdateMediaServiceSubscription();
        }

        private void UpdateAudioSubscription()
        {
            if (_canSubscribeToMedias)
            {
                btn_SubscribeRemoteAudioInput.Visible = true;

                Boolean subscribed;
                if ((_currentConfId != null) && (lb_Participants.Items.Count > 0))
                {
                    btn_SubscribeRemoteAudioInput.Enabled = true;

                    var publisherId = _currentConfId;
                    var media = Call.Media.AUDIO;
                    subscribed = IsMediaPublicationSubscribed(publisherId, media, false);
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
            if (_canSubscribeToMedias)
            {
                btn_SubscribeRemoteVideoInput.Visible = true;
                var media = Call.Media.VIDEO;

                // Manage Standard Video
                if (lb_PublishersVideo.SelectedItem is ListItem item)
                {
                    var publisherId = item.Value;
                    var subscribed = IsMediaPublicationSubscribed(publisherId, media, false);

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
                    var subscribed = IsMediaPublicationSubscribed(publisherId, media, true);

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
            if (_canSubscribeToMedias)
            {
                btn_SubscribeRemoteSharingInput.Visible = true;

                if (lb_PublishersSharing.SelectedItem is ListItem item)
                {
                    var publisherId = item.Value;
                    var media = Call.Media.SHARING;
                    var subscribed = IsMediaPublicationSubscribed(publisherId, media, false);

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

        private void UpdateDataChannelSubscription()
        {
            if (_canSubscribeToMedias)
            {
                btn_SubscribeRemoteDataChannelInput.Visible = true;

                if (lb_PublishersDataChannel.SelectedItem is ListItem item)
                {
                    var publisherId = item.Value;
                    var media = Call.Media.DATACHANNEL;
                    var subscribed = IsMediaPublicationSubscribed(publisherId, media, false);

                    btn_SubscribeRemoteDataChannelInput.Enabled = true;
                    btn_SubscribeRemoteDataChannelInput.Text = subscribed ? "Unsubscribe" : "Subscribe";
                    btn_SubscribeRemoteDataChannelInput.ForeColor = subscribed ? System.Drawing.Color.DarkRed : System.Drawing.Color.DarkGreen;

                    btn_CloseRemoteDataChannelInput.Visible = subscribed;
                }
                else
                {
                    btn_SubscribeRemoteDataChannelInput.Enabled = false;
                    btn_CloseRemoteDataChannelInput.Visible = false;
                }
            }
            else
            {
                btn_SubscribeRemoteDataChannelInput.Visible = false;
                btn_CloseRemoteDataChannelInput.Visible = false;
            }
        }

        private void UpdateMediaServiceSubscription()
        {
            if (_canSubscribeToMedias)
            {
                btn_SubscribeMediaServiceInput.Visible = true;

                if (lb_MediaService.SelectedItem is ListItem item)
                {
                    var serviceId = item.Value;
                    var subscribed = IsMediaServiceSubscribed(serviceId);

                    btn_SubscribeMediaServiceInput.Enabled = true;
                    btn_SubscribeMediaServiceInput.Text = subscribed ? "Unsubscribe" : "Subscribe";
                    btn_SubscribeMediaServiceInput.ForeColor = subscribed ? System.Drawing.Color.DarkRed : System.Drawing.Color.DarkGreen;

                    btn_OutputMediaServiceInput.Visible = subscribed;
                }
                else
                {
                    btn_SubscribeMediaServiceInput.Enabled = false;
                    btn_OutputMediaServiceInput.Visible = false;
                }
            }
            else
            {
                btn_SubscribeMediaServiceInput.Visible = false;
                btn_OutputMediaServiceInput.Visible = false;
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

        public void AskInfoAboutContactId(String userId)
        {
            lock (_unknownContactsById)
            {
                if (_unknownContactsById.Contains(userId))
                    return;
                _unknownContactsById.Add(userId);
                _rbContacts.GetContactFromContactIdFromServer(userId, callback =>
                {
                    if (callback.Result.Success)
                    {
                        if (_cancelableDelayForUnknowContacts == null)
                            _cancelableDelayForUnknowContacts = CancelableDelay.StartAfter(2000, UpdateUIFull);
                        else
                            _cancelableDelayForUnknowContacts.PostPone();
                    }

                    lock (_unknownContactsById)
                        _unknownContactsById.Remove(userId);
                });
            }
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
                if (participantJid == _rbContacts.GetCurrentContactJid())
                    return "[ ME ]";

                var contact = _rbContacts.GetContactFromContactJid(participantJid);
                if (contact != null)
                    result = Rainbow.Util.GetContactDisplayName(contact);
            }
            else if (!String.IsNullOrEmpty(participantId))
            {
                if (participantId == _rbContacts.GetCurrentContactId())
                    return "[ ME ]";

                var contact = _rbContacts.GetContactFromContactId(participantId);
                if (contact != null)
                    result = Rainbow.Util.GetContactDisplayName(contact);
            }

            if (String.IsNullOrEmpty(result))
            {
                if (!String.IsNullOrEmpty(participantId))
                    AskInfoAboutContactId(participantId);

                result = participantId;
            }

            return result;
        }

        private void OpenFormVideoOutputStreamWebRTC(int media, String? publisherId, bool dynamicFeed)
        {
            OpenVideoPublication?.Invoke(this, (media, publisherId, dynamicFeed));
        }

        private void OpenFormMediaServiceStreamWebRTC(String serviceId)
        {
            OpenMediaService?.Invoke(this, serviceId);
        }

    #endregion PRIVATE methods

    #region EVENTS from SDK Objects

        private void Conferences_ConferenceMediaPublicationsUpdated(object? sender, Rainbow.Events.MediaPublicationsEventArgs e)
        {
            if (_currentConference?.Id == e.ConferenceId)
                UpdateUIFull();
        }

        private void Conferences_ConferenceTalkersUpdated(object sender, Rainbow.Events.ConferenceTalkersEventArgs e)
        {
            if (_currentConference?.Id == e.ConferenceId)
                UpdateUIFull();
        }

        private void Conferences_ConferenceParticipantsUpdated(object sender, Rainbow.Events.ConferenceParticipantsEventArgs e)
        {
            if (_currentConference?.Id == e.ConferenceId)
            {
                UpdateUIFull();
            }
        }

        private void Conferences_ConferenceRaisedHandUpdated(object? sender, Rainbow.Events.ConferenceRaisedHandsEventArgs e)
        {
            if (_currentConference?.Id == e.ConferenceId)
            {
                UpdateUIFull();
            }
        }

        private void Bubbles_BubbleMemberUpdated(object? sender, Rainbow.Events.BubbleMemberEventArgs e)
        {
            if (_currentConference?.Id == e.BubbleId)
            {
                UpdateUIFull();
            }
        }

        private void Conferences_ConferenceRemoved(object sender, Rainbow.Events.IdEventArgs e)
        {
            if ((_currentConference != null) && (_currentConference.Id == e.Id))
            {
                _currentConference = null;
                _mediaPublicationsSubscribed.Clear();
                _mediaServicesSubscribed.Clear();
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
                _mediaServicesSubscribed.Clear();
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


        private void btn_ParticipantLowerHand_Click(object sender, EventArgs e)
        {
            String participantId = null;

            // Get participant Id
            if (lb_Participants.SelectedItem != null)
            {
                ListItem item = lb_Participants.SelectedItem as ListItem;
                participantId = item.Value;
            }

            if (_currentConference == null)
                return;

            _rbConferences.ConferenceLowerHand(_currentConference.Id, participantId, callback =>
            {
                if (callback.Result.Success)
                    AddInformationMessage($"Conference - Hands lowered");
                else
                    AddInformationMessage($"Conference - Hands lowered - Pb:[{callback.Result}]");
            });
        }

        private void btn_ParticipantLowerAllHands_Click(object sender, EventArgs e)
        {
            if (_currentConference == null)
                return;

            _rbConferences.ConferenceLowerAllHands(_currentConference.Id, callback =>
            {
                if (callback.Result.Success)
                    AddInformationMessage($"Conference - All Hands lowered");
                else
                    AddInformationMessage($"Conference - All Hands lowered - Pb:[{callback.Result}]");
            });
        }

        private void btn_RaiseHand_Click(object sender, EventArgs e)
        {
            if (_currentConference == null)
                return;

            _rbConferences.ConferenceRaiseHand(_currentConference.Id, callback =>
            {
                if (callback.Result.Success)
                    AddInformationMessage($"Conference - Hand Raised");
                else
                    AddInformationMessage($"Conference - Hand Raised - Pb:[{callback.Result}]");
            });
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
                _rbConferences.ConferenceGetFullSnapshot(_currentConference.Id);
                return;

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

        private void lb_PublishersDataChannel_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDataChannelSubscription();
        }

        private void lb_MediaService_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateMediaServiceSubscription();
        }

        private void btn_SubscribeRemoteAudioInput_Click(object sender, EventArgs e)
        {
            if (_canSubscribeToMedias)
            {
                var subscribed = IsMediaPublicationSubscribed("", Call.Media.AUDIO, false);
                OnMediaPublicationSubscription?.Raise(this, new MediaPublicationEventArgs(_currentConfId, "", "", Call.Media.AUDIO, false, subscribed ? MediaPublicationStatus.CURRENT_USER_UNSUBSCRIBED : MediaPublicationStatus.CURRENT_USER_SUBSCRIBED));
            }
        }

        private void btn_SubscribeRemoteVideoInput_Click(object sender, EventArgs e)
        {
            if (_canSubscribeToMedias)
            {
                if (lb_PublishersVideo.SelectedItem is ListItem item)
                {
                    var subscribed = IsMediaPublicationSubscribed(item.Value, Call.Media.VIDEO, false);
                    OnMediaPublicationSubscription?.Raise(this, new MediaPublicationEventArgs(_currentConfId, item.Value, "", Call.Media.VIDEO, false, subscribed ? MediaPublicationStatus.CURRENT_USER_UNSUBSCRIBED : MediaPublicationStatus.CURRENT_USER_SUBSCRIBED));
                }
            }
        }

        private void btn_SubscribeMediaServiceInput_Click(object sender, EventArgs e)
        {
            if (_canSubscribeToMedias)
            {
                if (lb_MediaService.SelectedItem is ListItem item)
                {
                    var subscribed = IsMediaServiceSubscribed(item.Value);
                    var ms = GetMediaService(item.Value);
                    if (ms != null)
                        OnMediaServiceSubscription?.Raise(this, new MediaServiceEventArgs(ms, subscribed ? MediaPublicationStatus.CURRENT_USER_UNSUBSCRIBED : MediaPublicationStatus.CURRENT_USER_SUBSCRIBED));
                }
            }
        }

        private void btn_SubscribeDynamicFeedInput_Click(object sender, EventArgs e)
        {
            if (_canSubscribeToMedias)
            {
                if (lb_PublishersVideo.SelectedItem is ListItem item)
                {
                    var subscribed = IsMediaPublicationSubscribed(item.Value, Call.Media.VIDEO, true);
                    OnMediaPublicationSubscription?.Raise(this, new MediaPublicationEventArgs(_currentConfId, item.Value, "", Call.Media.VIDEO, true, subscribed ? MediaPublicationStatus.CURRENT_USER_UNSUBSCRIBED : MediaPublicationStatus.CURRENT_USER_SUBSCRIBED));
                }
            }
        }

        private void btn_SubscribeRemoteSharingInput_Click(object sender, EventArgs e)
        {
            if (_canSubscribeToMedias)
            {
                if (lb_PublishersSharing.SelectedItem is ListItem item)
                {
                    var subscribed = IsMediaPublicationSubscribed(item.Value, Call.Media.SHARING, false);
                    OnMediaPublicationSubscription?.Raise(this, new MediaPublicationEventArgs(_currentConfId, item.Value, "", Call.Media.SHARING, false, subscribed ? MediaPublicationStatus.CURRENT_USER_UNSUBSCRIBED : MediaPublicationStatus.CURRENT_USER_SUBSCRIBED));
                }
            }
        }

        private void btn_SubscribeRemoteDataChannelInput_Click(object sender, EventArgs e)
        {
            if (_canSubscribeToMedias)
            {
                if (lb_PublishersDataChannel.SelectedItem is ListItem item)
                {
                    var subscribed = IsMediaPublicationSubscribed(item.Value, Call.Media.DATACHANNEL, false);
                    OnMediaPublicationSubscription?.Raise(this, new MediaPublicationEventArgs(_currentConfId, item.Value, "", Call.Media.DATACHANNEL, false, subscribed ? MediaPublicationStatus.CURRENT_USER_UNSUBSCRIBED : MediaPublicationStatus.CURRENT_USER_SUBSCRIBED));
                }
            }
        }

        private void btn_CloseRemoteDataChannelInput_Click(object sender, EventArgs e)
        {
            if (_canSubscribeToMedias)
            {
                if (lb_PublishersDataChannel.SelectedItem is ListItem item)
                {
                    var subscribed = IsMediaPublicationSubscribed(item.Value, Call.Media.DATACHANNEL, false);
                    OnCloseDataChannel?.Raise(this, new MediaPublicationEventArgs(_currentConfId, item.Value, "", Call.Media.DATACHANNEL, false, subscribed ? MediaPublicationStatus.CURRENT_USER_UNSUBSCRIBED : MediaPublicationStatus.CURRENT_USER_SUBSCRIBED));
                }
            }
        }

        private void lb_Participants_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btn_OutputRemoteVideoInput_Click(object sender, EventArgs e)
        {
            if (_canSubscribeToMedias)
            {
                if (lb_PublishersVideo.SelectedItem is ListItem item)
                {
                    var publisherId = item.Value;
                    var media = Call.Media.VIDEO;

                    OpenFormVideoOutputStreamWebRTC(media, publisherId, false);
                }
            }
        }

        private void btn_OutputMediaServiceInput_Click(object sender, EventArgs e)
        {
            if (_canSubscribeToMedias)
            {
                if (lb_MediaService.SelectedItem is ListItem item)
                {
                    var publisherId = item.Value;
                    var media = Call.Media.VIDEO;

                    OpenFormMediaServiceStreamWebRTC(publisherId);
                }
            }
        }

        private void btn_OutputDynamicFeedInput_Click(object sender, EventArgs e)
        {
            if (_canSubscribeToMedias)
            {
                var media = Call.Media.VIDEO;
                OpenFormVideoOutputStreamWebRTC(media, "", true);
            }
        }

        private void btn_OutputRemoteSharingInput_Click(object sender, EventArgs e)
        {
            if (_canSubscribeToMedias)
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
