﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Rainbow;
using Rainbow.Model;

namespace SDK.UIForm.WebRTC
{
    public partial class FormConferenceOptions : Form
    {
        private Rainbow.Application _rbApplication;
        private Rainbow.Bubbles _rbBubbles;
        private Rainbow.Contacts _rbContacts;
        private Rainbow.Conferences _rbConferences;


        private Conference ? _currentConference = null;
        private String? _currentConfId = null;
        private String? _currentConfJid = null;

#region CONSTRUCTOR
        public FormConferenceOptions()
        {
            this.HandleCreated += Form_HandleCreated;
            InitializeComponent();
        }

        public void Initialize(Rainbow.Application application)
        {
            // Get / Store SDK Objects
            _rbApplication = application;
            _rbBubbles = _rbApplication.GetBubbles();
            _rbContacts = _rbApplication.GetContacts();
            _rbConferences = _rbApplication?.GetConferences();


            // Events related to Rainbow SDK
            _rbConferences.ConferenceUpdated += Conferences_ConferenceUpdated;
            _rbConferences.ConferenceRemoved += Conferences_ConferenceRemoved;
            _rbConferences.ConferenceParticipantsUpdated += Conferences_ConferenceParticipantsUpdated;
            _rbConferences.ConferenceTalkersUpdated += Conferences_ConferenceTalkersUpdated;
            _rbConferences.ConferencePublishersUpdated += Conferences_ConferencePublishersUpdated;
        }

        public void SetConferenceInfo(String confId, String confJid)
        {
            _currentConfId = confId;
            _currentConfJid = confJid;

            _currentConference = _rbConferences.ConferenceGetByIdFromCache(confId);

            UpdateUIFull();
        }

#endregion CONSTRUCTOR


#region PRIVATE methods

        private void UpdateUIFull()
        {
            var action = new Action(() =>
            {
                UpdateUIConferenceInfo();

                UpdateUIParticipants();
                UpdateUITalkers();
                UpdateUIPublishers();
            });

            if (this.InvokeRequired)
                this.BeginInvoke(action);
            else
                action.Invoke();
        }

        private void UpdateUIConferenceInfo()
        {
            var action = new Action(() =>
            {
                if (_currentConference != null)
                {
                    lbl_ConferenceInProgress.Visible = true;

                    var bubble =_rbBubbles.GetBubbleByIdFromCache(_currentConference.Id);
                    if(bubble != null)
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
                    Dictionary<String, Conference.Participant> participants;
                    participants = _rbConferences.ConferenceGetParticipantsFromCache(_currentConference.Id);

                    if (participants?.Count > 0)
                    {
                        foreach (var participant in participants.Values)
                        {
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
                    Dictionary<String, Conference.Talker> talkers;
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
                    Dictionary<String, Conference.Publisher> publishers;
                    publishers = _rbConferences.ConferenceGetPublishersFromCache(_currentConference.Id);

                    if (publishers?.Count > 0)
                    {
                        foreach (var publisher in publishers.Values)
                        {
                            String displayName = GetDisplayName(publisher.Id, publisher.Jid_im);

                            ListItem item = new ListItem(displayName, publisher.Id);

                            foreach (var pubMedia in publisher.Medias)
                            {
                                if (pubMedia.Media.Equals("sharing", StringComparison.InvariantCultureIgnoreCase))
                                    lb_PublishersSharing.Items.Add(item);
                                else
                                    lb_PublishersVideo.Items.Add(item);
                            }
                        }
                    }
                }
            });

            if (this.InvokeRequired)
                this.BeginInvoke(action);
            else
                action.Invoke();
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

            if(String.IsNullOrEmpty(result))
                result = participantId;

            return result;
        }

#endregion PRIVATE methods

#region EVENTS from SDK Objects

        private void Conferences_ConferencePublishersUpdated(object sender, Rainbow.Events.ConferencePublishersEventArgs e)
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
                _currentConference = null;

            UpdateUIFull();
        }

        private void Conferences_ConferenceUpdated(object sender, Rainbow.Events.ConferenceEventArgs e)
        {
            if (_currentConference == null)
                _currentConference = e.Conference;
            else if (_currentConference.Id == e.Conference.Id)
                _currentConference = e.Conference;


            if (!e.Conference.Active)
                _currentConference = null;

            UpdateUIFull();
        }

#endregion EVENTS from SDK Objects


#region EVENTS from FORM elements

        private void Form_HandleCreated(object sender, EventArgs e)
        {

        }

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
                                AddInformationMessage($"Conference - PSTN Participant Pb to Mute / Unmute:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                        });
                    }
                    else
                    {
                        _rbConferences.ConferenceMuteOrUnmuteParticipant(bubbleId, participant.Id, !participant.Muted, callback =>
                        {
                            if (callback.Result.Success)
                                AddInformationMessage($"Conference - Participant Mute / Unmute done");
                            else
                                AddInformationMessage($"Conference - Participant Pb to Mute / Unmute:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
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
                                AddInformationMessage($"Conference - Participant Pb to Delegate:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
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
                                AddInformationMessage($"Conference - PSTN Participant Pb to Drop:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                        });
                    }
                    else
                    {
                        _rbConferences.ConferenceDropParticipant(bubbleId, participant.Id, callback =>
                        {
                            if (callback.Result.Success)
                                AddInformationMessage($"Conference - Participant Drop done");
                            else
                                AddInformationMessage($"Conference - Participant Pb to Drop:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
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
                        AddInformationMessage($"Conference - Pb Mute / Unmute:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
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
                        AddInformationMessage($"Conference - Pb Lock / Unlock:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
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
                        AddInformationMessage($"Pb to stop Conference:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
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
                            AddInformationMessage($"Conference - Pb Recording Stop:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                    });
                }
                else
                {
                    _rbConferences.ConferenceRecordingStart(bubbleId, callback =>
                    {
                        if (callback.Result.Success)
                            AddInformationMessage($"Conference - Recording Start done");
                        else
                            AddInformationMessage($"Conference - Pb Recording Start:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
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
                            AddInformationMessage($"Conference - Pb Resume Pause:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                    });
                }
                else if (_currentConference.RecordStatus == "on")
                {
                    _rbConferences.ConferenceRecordingPause(bubbleId, callback =>
                    {
                        if (callback.Result.Success)
                            AddInformationMessage($"Conference - Recording Pause done");
                        else
                            AddInformationMessage($"Conference - Pb Recording Pause:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                    });
                }
            }
        }

#endregion EVENTS from FORM elements
    }
}