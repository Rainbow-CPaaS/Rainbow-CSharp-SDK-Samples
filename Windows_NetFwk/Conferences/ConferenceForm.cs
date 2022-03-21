using Microsoft.Extensions.Logging;
using Rainbow.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace SampleConferences
{
    public partial class ConferenceForm : Form
    {
        private static readonly ILogger log = Rainbow.LogFactory.CreateLogger<ConferenceForm>();

        // DEFINE SDK OBJECTS
        Rainbow.Application rbAplication;
        Rainbow.Bubbles rbBubbles;
        Rainbow.Contacts rbContacts;
        Rainbow.Conferences rbConferences;

        Boolean usePersonalConference = false;
        String bubbleIdSelected = null;
        Conference conferenceSelected = null;

        public ConferenceForm()
        {
            InitializeComponent();
        }

        public void SetApplication(Rainbow.Application application)
        {
            rbAplication = application;

            // Get Rainbow SDK objects
            rbBubbles = rbAplication.GetBubbles();
            rbContacts = rbAplication.GetContacts();
            rbConferences = rbAplication.GetConferences();

            // Events related to Rainbow SDK
            rbConferences.ConferenceUpdated += RbConferences_ConferenceUpdated;
            rbConferences.ConferenceRemoved += RbConferences_ConferenceRemoved;
            rbConferences.ConferenceParticipantsUpdated += RbConferences_ConferenceParticipantsUpdated;
            rbConferences.ConferenceTalkersUpdated += RbConferences_ConferenceTalkersUpdated;
            rbConferences.ConferencePublishersUpdated += RbConferences_ConferencePublishersUpdated;

            AskBubbles();

            UpdateUIFull();
        }

        private void AskBubbles()
        {
            var bubbles = rbBubbles.GetAllBubblesFromCache();
            if (!(bubbles?.Count > 0))
            {
                rbBubbles.GetAllBubbles(callback =>
                {
                    if (callback.Result.Success)
                    {
                        UpdateUIBubbles();
                        AddInformationMessage($"Nb bubbles:[{callback.Data.Count}]");
                    }
                    else
                        AddInformationMessage($"Cannot get bubbles - Exception:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                });
            }
            else
            {
                UpdateUIBubbles();
                AddInformationMessage($"Nb bubbles [{bubbles.Count}]");
            }
        }

        private void GetSelectedBubble()
        {
            if (this.InvokeRequired)
            {
                FormLogin.VoidDelegate d = new FormLogin.VoidDelegate(GetSelectedBubble);
                this.Invoke(d, new object[] { });
            }
            else
            {
                if (cbBubbles.SelectedItem != null)
                {
                    var item = cbBubbles.SelectedItem as ListItem;
                    bubbleIdSelected = item.Value;
                }
                else
                    bubbleIdSelected = null;

                UpdateUIParticipantsParameters();
            }
        }

        private void UpdateUIFull()
        {
            if (this.InvokeRequired)
            {
                FormLogin.VoidDelegate d = new FormLogin.VoidDelegate(UpdateUIFull);
                this.Invoke(d, new object[] { });
            }
            else
            {
                // Check if "Conference" features are authorized
                cbConferenceFeatureAuthorized.Checked = rbConferences.ConferenceAllowed();
                cbPersonalConferenceFeatureAuthorized.Checked = rbConferences.PersonalConferenceAllowed();

                // Do we use Conference V2 ?
                cbConferenceV2Used.Checked = rbAplication.Restrictions.UseAPIConferenceV2;

                // Enable radio buttons according authorizations
                rbUsePersonalMeeting.Enabled = cbPersonalConferenceFeatureAuthorized.Checked;
                rbUseWebRtc.Enabled = cbConferenceFeatureAuthorized.Checked;
                if(rbUsePersonalMeeting.Enabled)
                    rbUsePersonalMeeting.Checked = true;
                else
                    rbUseWebRtc.Checked = true;

                UpdateUIPersonalConfOptions();
                UpdateUIWebRTCConfOptions();

                UpdateUIParticipantsParameters();
                UpdateUIParticipants();
                UpdateUITalkers();
                UpdateUIPublishers();

                UpdateUIConferenceInProgress();
            }
        }

        private void UpdateUIParticipantsParameters()
        {
            if (this.InvokeRequired)
            {
                FormLogin.VoidDelegate d = new FormLogin.VoidDelegate(UpdateUIParticipantsParameters);
                this.Invoke(d, new object[] { });
            }
            else
            {
                Boolean muteParticipant = false;
                Boolean playSound = false;

                if (rbUsePersonalMeeting.Checked)
                {
                    btSetConferenceParticipantParameters.Enabled = true;

                    rbConferences.PersonalConferenceGetParticipantParameters(out playSound, out muteParticipant);
                }
                else
                {
                    if(bubbleIdSelected != null)
                    {
                        btSetConferenceParticipantParameters.Enabled = rbConferences.ConferenceGetParticipantParameters(bubbleIdSelected, out playSound, out muteParticipant);
                    }
                    else
                        btSetConferenceParticipantParameters.Enabled = false;
                }

                cbPlaySound.Enabled = btSetConferenceParticipantParameters.Enabled;
                cbMuteParticipant.Enabled = btSetConferenceParticipantParameters.Enabled;

                cbPlaySound.Checked = playSound;
                cbMuteParticipant.Checked = muteParticipant;

            }
        }

        private void UpdateUIParticipants()
        {
            if (this.InvokeRequired)
            {
                FormLogin.VoidDelegate d = new FormLogin.VoidDelegate(UpdateUIParticipants);
                this.Invoke(d, new object[] { });
            }
            else
            {
                lbParticipants.Items.Clear();
                if (conferenceSelected != null)
                {
                    Dictionary<String, Conference.Participant> participants;
                    if (conferenceSelected.Id == rbConferences.PersonalConferenceGetId())
                        participants = rbConferences.PersonalConferenceGetParticipantsFromCache();
                    else
                        participants = rbConferences.ConferenceGetParticipantsFromCache(conferenceSelected.Id);

                    if(participants?.Count > 0)
                    {
                        foreach(var participant in participants.Values)
                        {
                            String displayName = GetDisplayName(participant.Id, participant.Jid_im, participant.PhoneNumber);

                            displayName += $" [{participant.Privilege}]";
                            displayName += String.IsNullOrEmpty(participant.PhoneNumber) ? "" : $" [{participant.PhoneNumber}]";
                            displayName += participant.Muted ? " [MUTED]" : "";
                            displayName += participant.Hold ? " [HOLD]" : "";

                            ListItem item = new ListItem(displayName, participant.Id);

                            lbParticipants.Items.Add(item);
                        }
                    }
                }
            }
        }

        private void UpdateUITalkers()
        {
            if (this.InvokeRequired)
            {
                FormLogin.VoidDelegate d = new FormLogin.VoidDelegate(UpdateUITalkers);
                this.Invoke(d, new object[] { });
            }
            else
            {
                lbTalkers.Items.Clear();
                if (conferenceSelected != null)
                {
                    Dictionary<String, Conference.Talker> talkers;
                    if (conferenceSelected.Id == rbConferences.PersonalConferenceGetId())
                        talkers = rbConferences.PersonalConferenceGetTalkersFromCache();
                    else
                        talkers = rbConferences.ConferenceGetTalkersFromCache(conferenceSelected.Id);

                    if (talkers?.Count > 0)
                    {
                        foreach (var talker in talkers.Values)
                        {
                            String displayName = GetDisplayName(talker.Id, talker.Jid_im, talker.PhoneNumber);

                            displayName += String.IsNullOrEmpty(talker.PhoneNumber) ? "" : $" [{talker.PhoneNumber}]";

                            ListItem item = new ListItem(displayName, talker.Id);

                            lbTalkers.Items.Add(item);
                        }
                    }
                }
            }
        }

        private void UpdateUIPublishers()
        {
            if (this.InvokeRequired)
            {
                FormLogin.VoidDelegate d = new FormLogin.VoidDelegate(UpdateUIPublishers);
                this.Invoke(d, new object[] { });
            }
            else
            {
                lbPublishers.Items.Clear();
                if (conferenceSelected != null)
                {
                    Dictionary<String, Conference.Publisher> publishers;
                    if (conferenceSelected.Id == rbConferences.PersonalConferenceGetId())
                        publishers = rbConferences.PersonalConferenceGetPublishersFromCache();
                    else
                        publishers = rbConferences.ConferenceGetPublishersFromCache(conferenceSelected.Id);

                    if (publishers?.Count > 0)
                    {
                        foreach (var publisher in publishers.Values)
                        {
                            String displayName = GetDisplayName(publisher.Id, publisher.Jid_im);

                            displayName += $" [{String.Join(", ", publisher.Media)}]";

                            ListItem item = new ListItem(displayName, publisher.Id);

                            lbPublishers.Items.Add(item);
                        }
                    }
                }
            }
        }

        private void UpdateUIPersonalConfOptions()
        {
            if (this.InvokeRequired)
            {
                FormLogin.VoidDelegate d = new FormLogin.VoidDelegate(UpdateUIPersonalConfOptions);
                this.Invoke(d, new object[] { });
            }
            else
            {
                usePersonalConference = rbUsePersonalMeeting.Checked;

                btnPersonalConferenceGetPhonesList.Enabled = usePersonalConference;
                btnPersonalConferenceGetPassCodes.Enabled = usePersonalConference;
                btnPersonalConferenceResetPassCodes.Enabled = usePersonalConference;

                btnConferenceReject.Visible = !usePersonalConference;
            }
        }

        private void UpdateUIWebRTCConfOptions()
        {
            if (this.InvokeRequired)
            {
                FormLogin.VoidDelegate d = new FormLogin.VoidDelegate(UpdateUIWebRTCConfOptions);
                this.Invoke(d, new object[] { });
            }
            else
            {
                var flag = rbUseWebRtc.Checked;

                cbBubbles.Enabled = flag;
            }
        }

        private void UpdateUIBubbles()
        {
            if (this.InvokeRequired)
            {
                FormLogin.VoidDelegate d = new FormLogin.VoidDelegate(UpdateUIBubbles);
                this.Invoke(d, new object[] { });
            }
            else
            {
                // Clear
                cbBubbles.Items.Clear();

                var bubbles = rbBubbles.GetAllBubblesFromCache();
                ListItem item;
                foreach (var b in bubbles)
                {
                    if (rbBubbles.IsModerator(b) == true)
                    {
                        item = new ListItem(b.Name, b.Id);
                        cbBubbles.Items.Add(item);
                    }
                }
                if( (cbBubbles.Items.Count > 0) && (cbBubbles.SelectedIndex < 0) )
                    cbBubbles.SelectedIndex = 0;

                GetSelectedBubble();
            }

        }

        private void UpdateUIConferenceInProgress()
        {
            if (this.InvokeRequired)
            {
                FormLogin.VoidDelegate d = new FormLogin.VoidDelegate(UpdateUIConferenceInProgress);
                this.Invoke(d, new object[] { });
            }
            else
            {
                cbConferenceInProgress.Items.Clear();

                ListItem selectedItem = null;
                var conferences = rbConferences.ConferenceGetListFromCache();
                if(conferences?.Count > 0)
                {
                    foreach(var conference in conferences)
                    {
                        if (conference.Active)
                        {
                            String name = conference.MediaType + " " + conference.Id;
                            name += conference.Muted ? " [MUTED]" : "";
                            name += conference.Locked ? " [LOCKED]" : "";

                            ListItem item = new ListItem(name, conference.Id);
                            
                            cbConferenceInProgress.Items.Add(item);

                            if (conference.Id == conferenceSelected?.Id)
                                selectedItem = item;
                        }
                    }
                }

                var personalConf = rbConferences.PersonalConferenceGetFromCache();
                if (personalConf?.Active == true)
                {
                    String name = "PersonalConf " + personalConf.Id;
                    name += personalConf.Muted ? " [MUTED]" : "";
                    name += personalConf.Locked ? " [LOCKED]" : "";

                    ListItem item = new ListItem(name, personalConf.Id);
                    cbConferenceInProgress.Items.Add(item);

                    if (personalConf.Id == conferenceSelected?.Id)
                        selectedItem = item;
                }

                Boolean flag = true;
                if (cbConferenceInProgress.Items.Count > 0)
                {
                    flag = true;
                    if (selectedItem != null)
                        cbConferenceInProgress.SelectedItem = selectedItem;
                    else
                    {
                        cbConferenceInProgress.SelectedIndex = 0;
                        //TODO - set current conf
                    }
                }
                else
                {
                    flag = false;
                }

                btnConferenceStart.Enabled = !flag;
                btnConferenceStop.Enabled = flag;
                btnConferenceMute.Enabled = flag;
                btnConferenceLock.Enabled = flag;
                btnConferenceRecordingStart.Enabled = flag;
                btnConferenceRecordingPause.Enabled = flag;
                btnConferenceGetFullSnapshot.Enabled = flag;
                btnConferenceAddPstn.Enabled = flag;
                btnConferenceJoin.Enabled = flag;
                btnConferenceReject.Enabled = flag;

                if (rbAplication.Restrictions.UseAPIConferenceV2)
                {
                    tbConferenceJoinPhoneNumber.Enabled = false;
                    cbConferenceJoinAsModerator.Enabled = false;
                }
                else
                {
                    tbConferenceJoinPhoneNumber.Enabled = true;
                    cbConferenceJoinAsModerator.Enabled = true;
                }


                if (conferenceSelected != null)
                {
                    lblConferenceRecordingStatus.Text = "Recording - status:" + conferenceSelected.RecordStatus;
                }
                else
                    lblConferenceRecordingStatus.Text = "Recording";
            }
        }

        private void AddInformationMessage(string status)
        {
            if(this.InvokeRequired)
            {
                var  d = new FormLogin.StringArgReturningVoidDelegate(AddInformationMessage);
                this.Invoke(d, new object[] { status });
            }
            else
            {
                if (!String.IsNullOrEmpty(status))
                {
                    if (!status.StartsWith("\r\n"))
                        status = "\r\n" + status;

                    tbInformation.Text += "\n" + status;
                }
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
                var contact = rbContacts.GetContactFromContactJid(participantJid);
                if (contact != null)
                    result = Rainbow.Util.GetContactDisplayName(contact);
            }
            else if (rbAplication.Restrictions.UseAPIConferenceV2 &&  !String.IsNullOrEmpty(participantId))
            {
                var contact = rbContacts.GetContactFromContactId(participantId);
                if (contact != null)
                    result = Rainbow.Util.GetContactDisplayName(contact);
            }

            if(String.IsNullOrEmpty(result))
                result = participantId;

            return result;
        }

#region EVENT FROM RAINBOW SDK OBJECTS

        private void RbConferences_ConferencePublishersUpdated(object sender, Rainbow.Events.ConferencePublishersEventArgs e)
        {
            if (conferenceSelected?.Id == e.ConferenceId)
                UpdateUIPublishers();
        }

        private void RbConferences_ConferenceTalkersUpdated(object sender, Rainbow.Events.ConferenceTalkersEventArgs e)
        {
            if (conferenceSelected?.Id == e.ConferenceId)
                UpdateUITalkers();
        }

        private void RbConferences_ConferenceParticipantsUpdated(object sender, Rainbow.Events.ConferenceParticipantsEventArgs e)
        {
            if (conferenceSelected?.Id == e.ConferenceId)
                UpdateUIParticipants();
        }

        private void RbConferences_ConferenceRemoved(object sender, Rainbow.Events.IdEventArgs e)
        {
            if ( (conferenceSelected != null) && (conferenceSelected.Id == e.Id) )
                conferenceSelected = null;

            UpdateUIConferenceInProgress();

            UpdateUIParticipants();
            UpdateUITalkers();
            UpdateUIPublishers();
        }

        private void RbConferences_ConferenceUpdated(object sender, Rainbow.Events.ConferenceEventArgs e)
        {
            if (conferenceSelected == null)
                conferenceSelected = e.Conference;
            else if(conferenceSelected.Id == e.Conference.Id)
                conferenceSelected = e.Conference;


            if (!e.Conference.Active)
                conferenceSelected = null;

            UpdateUIConferenceInProgress();

            UpdateUIParticipants();
            UpdateUITalkers();
            UpdateUIPublishers();
        }

#endregion EVENT FROM RAINBOW SDK OBJECTS



#region EVENT FROM FORM ELEMENTS
        private void rbUsePersonalMeeting_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUIPersonalConfOptions();
            UpdateUIWebRTCConfOptions();

            UpdateUIParticipantsParameters();
        }

        private void rbUseWebRtc_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUIPersonalConfOptions();
            UpdateUIWebRTCConfOptions();

            UpdateUIParticipantsParameters();
        }

        private void btnGetMeetingUrl_Click(object sender, EventArgs e)
        {
            if(usePersonalConference)
            {
                // Personal Conference
                rbConferences.PersonalConferenceGetPublicUrl(callback =>
                {
                    if (callback.Result.Success)
                        AddInformationMessage($"Personal Conf. URL :[{callback.Data.ToString()}]");
                    else
                        AddInformationMessage($"Pb to get Personal Conf URL - error:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                });
                
            }
            else
            {
                // WebRTC Conference
                rbConferences.ConferenceGetPublicUrl(bubbleIdSelected, callback =>
                {
                    if (callback.Result.Success)
                        AddInformationMessage($"Conf. URL :[{callback.Data.ToString()}]");
                    else
                        AddInformationMessage($"Pb to get Conf URL - error:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                });
            }
        }

        private void btnResetMeetingUrl_Click(object sender, EventArgs e)
        {
            if (usePersonalConference)
            {
                // Personal Conference
                rbConferences.PersonalConferenceGenerateNewPublicUrl(callback =>
                {
                    if (callback.Result.Success)
                        AddInformationMessage($"Personal Conf. Reset - New URL :[{callback.Data.ToString()}]");
                    else
                        AddInformationMessage($"Pb to reset Personal Conf URL - error:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                });
            }
            else
            {
                // WebRTC Conference
                rbConferences.ConferenceGenerateNewPublicUrl(bubbleIdSelected, callback =>
                {
                    if (callback.Result.Success)
                        AddInformationMessage($"Conf. Reset - New URL :[{callback.Data.ToString()}]");
                    else
                        AddInformationMessage($"Pb to reset Conf URL - error:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                });
            }
        }

        private void btnPersonalConferenceGetPhonesList_Click(object sender, EventArgs e)
        {
            String log = "";
            rbConferences.PersonalConferenceGetPhoneNumbers(callback =>
            {
                if (callback.Result.Success)
                {
                    PersonalConferencePhoneNumbers instantMeetingPhoneNumbers = callback.Data;

                    List<ConferencePhoneNumber> listNear;
                    List<ConferencePhoneNumber> listOthers;

                    int nbNear = 0;
                    int nbOthers = 0;

                    //get list of near phones
                    listNear = instantMeetingPhoneNumbers.NearEndUser;
                    if ((listNear != null) && (listNear.Count > 0))
                    {
                        nbNear = listNear.Count;
                        log += "\r\nNear phone numbers: [" + String.Join(", ", listNear);
                    }
                    else
                        log += "\r\nNear phone numbers: [NONE]";

                    //get other list of phones
                    listOthers = instantMeetingPhoneNumbers.Others;
                    if ((listOthers != null) && (listOthers.Count > 0))
                    {
                        nbOthers = listOthers.Count;
                        log += "\r\nOther phone numbers: [" + String.Join(", ", listOthers);
                    }
                    else
                        log += "\r\nOther phone numbers: [NONE]";
                    AddInformationMessage(log);
                }
                else
                {
                    AddInformationMessage($"Pb to get audio phone numbers - error:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                }
            });
        }

        private void btnPersonalConferenceGetPassCodes_Click(object sender, EventArgs e)
        {
            rbConferences.PersonalConferenceGetPassCodes(callback =>
            {
                if (callback.Result.Success)
                    AddInformationMessage($"Personal Conf.  PassCodes:[{callback.Data.ToString()}]");
                else
                    AddInformationMessage($"Pb to get Personal Conf - error:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
            });
        }

        private void btnPersonalConferenceResetPassCodes_Click(object sender, EventArgs e)
        {
            rbConferences.PersonalConferenceResetPassCodes(callback =>
            {
                if (callback.Result.Success)
                    AddInformationMessage($"Personal Conf. Reset - New PassCodes:[{callback.Data.ToString()}]");
                else
                    AddInformationMessage($"Pb to Reset Personal Conf - error:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
            });
        }

        private void cbBubbles_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetSelectedBubble();
        }

        private void btSetConferenceParticipantParameters_Click(object sender, EventArgs e)
        {
            if (usePersonalConference)
            {
                rbConferences.PersonalConferenceSetParticipantParameters(cbPlaySound.Checked, cbMuteParticipant.Checked, callback =>
                {
                    if(callback.Result.Success)
                        AddInformationMessage("Personal Conf. participant parameters set done");
                    else
                        AddInformationMessage($"Personal Conf. participant parameters - Error:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                });
            }
            else
            {
                rbConferences.ConferenceSetParticipantParameters(bubbleIdSelected, cbPlaySound.Checked, cbMuteParticipant.Checked, callback =>
                {
                    if (callback.Result.Success)
                        AddInformationMessage("WebRTC Conf. participant parameters set done");
                    else
                        AddInformationMessage($"WebRTC Conf. participant parameters - Error:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                });
            }

           
        }

        private void btnConferenceStart_Click(object sender, EventArgs e)
        {
            // Do we want to start a meeting ?
            if (conferenceSelected == null)
            {
                if (usePersonalConference)
                {
                    rbConferences.PersonalConferenceStart(callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddInformationMessage(String.Format("Personal Conference - Start done"));
                        }
                        else
                        {
                            AddInformationMessage($"Pb to start Personal Conference:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                            log.LogDebug($"Pb to start Personal Conference:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                        }
                    });
                }
                else
                {
                    if(cbBubbles.SelectedItem == null)
                    {
                        log.LogDebug($"No bubble selected to start Conference ...");
                        return;
                    }

                    ListItem item = cbBubbles.SelectedItem as ListItem;

                    rbConferences.ConferenceStart(item.Value, callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddInformationMessage(String.Format("Conference - Start done"));
                        }
                        else
                        {
                            AddInformationMessage($"Pb to start Conference:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                            log.LogDebug($"Pb to start Conference:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                        }
                    });
                }
            }
        }

        private void btnConferenceStop_Click(object sender, EventArgs e)
        {
            if(conferenceSelected?.Active == true)
            {
                if(conferenceSelected.Id == rbConferences.PersonalConferenceGetId())
                {
                    rbConferences.PersonalConferenceStop(callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddInformationMessage(String.Format("Conference - Stop done"));
                        }
                        else
                        {
                            AddInformationMessage($"Pb to stop Conference:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                            log.LogDebug($"Pb to stop Conference:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                        }
                    });
                }
                else
                {
                    String bubbleId = rbConferences.GetBubbleIdByConferenceIdFromCache(conferenceSelected.Id);
                    if(bubbleId == null)
                    {
                        AddInformationMessage($"Cannot get Bubble Id from this Conference Id");
                        return;
                    }

                    rbConferences.ConferenceStop(bubbleId, callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddInformationMessage(String.Format("Conference - Stop done"));
                        }
                        else
                        {
                            AddInformationMessage($"Pb to stop Conference:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                            log.LogDebug($"Pb to stop Conference:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                        }
                    });
                }
            }
        }

        private void btnConferenceReject_Click(object sender, EventArgs e)
        {
            if (conferenceSelected?.Active == true)
            {
                String bubbleId = rbConferences.GetBubbleIdByConferenceIdFromCache(conferenceSelected.Id);
                if (bubbleId == null)
                {
                    AddInformationMessage($"Cannot get Bubble Id from this Conference Id");
                    return;
                }

                rbConferences.ConferenceReject(bubbleId, callback =>
                {
                    if (callback.Result.Success)
                    {
                        AddInformationMessage(String.Format("Conference - Reject done"));
                    }
                    else
                    {
                        AddInformationMessage($"Pb to Reject Conference:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                        log.LogDebug($"Pb to Reject Conference:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                    }
                });
            }
        }
        private void btnConferenceMute_Click(object sender, EventArgs e)
        {
            if (conferenceSelected != null)
            {
                String personalConfId = rbConferences.PersonalConferenceGetId();
                if (conferenceSelected.Id == rbConferences.PersonalConferenceGetId())
                {
                    rbConferences.PersonalConferenceMuteOrUnmute(!conferenceSelected.Muted, callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddInformationMessage($"Personal Conference - Mute / Unmute done");
                        }
                        else
                        {
                            AddInformationMessage($"Personal Conference - Pb Mute / Unmute:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                            log.LogDebug($"Personal Conference - Pb Mute / Unmute:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                        }
                    });
                }
                else
                {
                    var bubbleId = rbConferences.GetBubbleIdByConferenceIdFromCache(conferenceSelected.Id);
                    if (bubbleId == null)
                    {
                        AddInformationMessage($"Cannot get Bubble Id from this Conference Id");
                        return;
                    }
                    rbConferences.ConferenceMuteOrUnmute(bubbleId, !conferenceSelected.Muted, callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddInformationMessage($"Conference - Mute / Unmute done");
                        }
                        else
                        {
                            AddInformationMessage($"Conference - Pb Mute / Unmute:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                            log.LogDebug($"Conference - Pb Mute / Unmute:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                        }
                    });
                }
            }
        }

        private void btnConferenceLock_Click(object sender, EventArgs e)
        {
            if (conferenceSelected != null)
            {
                if (conferenceSelected.Id == rbConferences.PersonalConferenceGetId())
                {
                    rbConferences.PersonalConferenceLockOrUnlock(!conferenceSelected.Locked, callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddInformationMessage($"Personal Conference - Lock / Unlock done");
                        }
                        else
                        {
                            AddInformationMessage($"Personal Conference - Pb Lock / Unlock:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                            log.LogDebug($"Personal Conference - Pb Lock / Unlock:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                        }
                    });
                }
                else
                {
                    var bubbleId = rbConferences.GetBubbleIdByConferenceIdFromCache(conferenceSelected.Id);
                    if (bubbleId == null)
                    {
                        AddInformationMessage($"Cannot get Bubble Id from this Conference Id");
                        return;
                    }
                    rbConferences.ConferenceLockOrUnlocked(bubbleId, !conferenceSelected.Locked, callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddInformationMessage($"Conference - Lock / Unlock done");
                        }
                        else
                        {
                            AddInformationMessage($"Conference - Pb Lock / Unlock:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                            log.LogDebug($"Conference - Pb Lock / Unlock:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                        }
                    });
                }
            }
        }

        private void btnConferenceRecordingStart_Click(object sender, EventArgs e)
        {
            if(conferenceSelected != null)
            {
                if (conferenceSelected.Id == rbConferences.PersonalConferenceGetId())
                {
                    // Can be done ?
                }
                else
                {
                    var bubbleId = rbConferences.GetBubbleIdByConferenceIdFromCache(conferenceSelected.Id);
                    if (bubbleId == null)
                    {
                        AddInformationMessage($"Cannot get Bubble Id from this Conference Id");
                        return;
                    }

                    if (conferenceSelected.RecordStatus != "off")
                    {
                        rbConferences.ConferenceRecordingStop(bubbleId, callback =>
                        {
                            if (callback.Result.Success)
                            {
                                AddInformationMessage($"Conference - Recording Stop done");
                            }
                            else
                            {
                                AddInformationMessage($"Conference - Pb Recording Stop:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                                log.LogDebug($"Conference - Pb Recording Stop:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                            }
                        });
                    }
                    else
                    {
                        rbConferences.ConferenceRecordingStart(bubbleId, callback =>
                        {
                            if (callback.Result.Success)
                            {
                                AddInformationMessage($"Conference - Recording Start done");
                            }
                            else
                            {
                                AddInformationMessage($"Conference - Pb Recording Start:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                                log.LogDebug($"Conference - Pb Recording Start:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                            }
                        });
                    }
                }
            }
        }

        private void btnConferenceRecordingPause_Click(object sender, EventArgs e)
        {
            if (conferenceSelected != null)
            {
                if (conferenceSelected.Id == rbConferences.PersonalConferenceGetId())
                {
                    // Can be done ?
                }
                else
                {
                    var bubbleId = rbConferences.GetBubbleIdByConferenceIdFromCache(conferenceSelected.Id);
                    if (bubbleId == null)
                    {
                        AddInformationMessage($"Cannot get Bubble Id from this Conference Id");
                        return;
                    }

                    if (conferenceSelected.RecordStatus == "pause")
                    {
                        rbConferences.ConferenceRecordingResume(bubbleId, callback =>
                        {
                            if (callback.Result.Success)
                            {
                                AddInformationMessage($"Conference - Recording Resume done");
                            }
                            else
                            {
                                AddInformationMessage($"Conference - Pb Resume Pause:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                                log.LogDebug($"Conference - Pb Resume Pause:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                            }
                        });
                    }
                    else if (conferenceSelected.RecordStatus == "on")
                    {
                        rbConferences.ConferenceRecordingPause(bubbleId, callback =>
                        {
                            if (callback.Result.Success)
                            {
                                AddInformationMessage($"Conference - Recording Pause done");
                            }
                            else
                            {
                                AddInformationMessage($"Conference - Pb Recording Pause:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                                log.LogDebug($"Conference - Pb Recording Pause:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                            }
                        });
                    }
                }
            }
        }

        private void btnConferenceAddPstn_Click(object sender, EventArgs e)
        {
            String phoneNumber = tbConferencePstn.Text;
            if (conferenceSelected != null)
            {
                if (conferenceSelected.Id == rbConferences.PersonalConferenceGetId())
                {
                    // TODO
                }
                else
                {
                    var bubbleId = rbConferences.GetBubbleIdByConferenceIdFromCache(conferenceSelected.Id);

                    rbConferences.ConferenceAddPstnParticipant(bubbleId, phoneNumber, "", callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddInformationMessage($"Conference - Add PSTN done");
                        }
                        else
                        {
                            AddInformationMessage($"Conference - Pb to Add PSTN:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                            log.LogDebug($"Conference - Pb to Add PSTN:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                        }
                    });
                }
            }
        }

        private void btnConferenceJoin_Click(object sender, EventArgs e)
        {
            Boolean asModerator = cbConferenceJoinAsModerator.Checked;
            Boolean muted = cbConferenceJoinMuted.Checked;
            String phoneNumber = tbConferenceJoinPhoneNumber.Text;

            if (conferenceSelected != null)
            {
                if (conferenceSelected.Id == rbConferences.PersonalConferenceGetId())
                {
                    rbConferences.PersonalConferenceJoin(asModerator, muted, phoneNumber, "", callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddInformationMessage($"Personal Conference - Join done");
                        }
                        else
                        {
                            AddInformationMessage($"Personal Conference - Pb to Join:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                            log.LogDebug($"Personal Conference - Pb to Join:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                        }
                    });
                }
                else
                {
                    var bubbleId = rbConferences.GetBubbleIdByConferenceIdFromCache(conferenceSelected.Id);

                    rbConferences.ConferenceJoin(bubbleId, muted, phoneNumber, "", callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddInformationMessage($"Conference - Join done");
                        }
                        else
                        {
                            AddInformationMessage($"Conference - Pb to Join:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                            log.LogDebug($"Conference - Pb to Join:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                        }
                    });
                }
            }
        }

        private void btnConferenceParticipantMute_Click(object sender, EventArgs e)
        {
            String participantId = null;

            // Get participant Id
            if (lbParticipants.SelectedItem != null)
            {
                ListItem item = lbParticipants.SelectedItem as ListItem;
                participantId = item.Value;
            }

            if ( (conferenceSelected != null) && (participantId != null) )
            {
                if (conferenceSelected.Id == rbConferences.PersonalConferenceGetId())
                {
                    var participants = rbConferences.PersonalConferenceGetParticipantsFromCache();
                    if (participants?.ContainsKey(participantId) == true)
                    {
                        var participant = participants[participantId];
                        rbConferences.PersonalConferenceMuteOrUnmuteParticipant(participant.Id, !participant.Muted, callback =>
                        {
                            if (callback.Result.Success)
                            {
                                AddInformationMessage($"Personal Conference - Participant Mute / Unmute done");
                            }
                            else
                            {
                                AddInformationMessage($"Personal Conference - Participant Pb to Mute / Unmute:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                                log.LogDebug($"Personal Conference - Participant Pb to Mute / Unmute:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                            }
                        });
                    }
                }
                else
                {
                    var bubbleId = rbConferences.GetBubbleIdByConferenceIdFromCache(conferenceSelected.Id);
                    if (bubbleId == null)
                    {
                        AddInformationMessage($"Cannot get Bubble Id from this Conference Id");
                        return;
                    }

                    var participants = rbConferences.ConferenceGetParticipantsFromCache(conferenceSelected.Id);

                    if (participants?.ContainsKey(participantId) == true)
                    {
                        var participant = participants[participantId];

                        // Check if it's a PSTN Participant in Conf V2
                        if (rbAplication.Restrictions.UseAPIConferenceV2 && !String.IsNullOrEmpty(participant.PhoneNumber))
                        {
                            rbConferences.ConferenceMuteOrUnmutePstnParticipant(bubbleId, participant.PhoneNumber, !participant.Muted, callback =>
                            {
                                if (callback.Result.Success)
                                {
                                    AddInformationMessage($"Conference - PSTN Participant Mute / Unmute done");
                                }
                                else
                                {
                                    AddInformationMessage($"Conference - PSTN Participant Pb to Mute / Unmute:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                                    log.LogDebug($"Conference - PSTN Participant Pb to Mute / Unmute:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                                }
                            });
                        }
                        else
                        {
                            rbConferences.ConferenceMuteOrUnmuteParticipant(bubbleId, participant.Id, !participant.Muted, callback =>
                            {
                                if (callback.Result.Success)
                                {
                                    AddInformationMessage($"Conference - Participant Mute / Unmute done");
                                }
                                else
                                {
                                    AddInformationMessage($"Conference - Participant Pb to Mute / Unmute:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                                    log.LogDebug($"Conference - Participant Pb to Mute / Unmute:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                                }
                            });
                        }
                    }
                }
            }
        }

        private void btnConferenceParticipantDrop_Click(object sender, EventArgs e)
        {
            String participantId = null;

            // Get participant Id
            if (lbParticipants.SelectedItem != null)
            {
                ListItem item = lbParticipants.SelectedItem as ListItem;
                participantId = item.Value;
            }

            if ((conferenceSelected != null) && (participantId != null))
            {
                if (conferenceSelected.Id == rbConferences.PersonalConferenceGetId())
                {
                    var participants = rbConferences.PersonalConferenceGetParticipantsFromCache();
                    if (participants?.ContainsKey(participantId) == true)
                    {
                        var participant = participants[participantId];
                        rbConferences.PersonalConferenceDropParticipant(participant.Id, callback =>
                        {
                            if (callback.Result.Success)
                            {
                                AddInformationMessage($"Personal Conference - Participant Drop done");
                            }
                            else
                            {
                                AddInformationMessage($"Personal Conference - Participant Pb to Drop:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                                log.LogDebug($"Personal Conference - Participant Pb to Drop:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                            }
                        });
                    }
                    else
                        AddInformationMessage($"Personal Conference - Participant not found");
                }
                else
                {
                    var bubbleId = rbConferences.GetBubbleIdByConferenceIdFromCache(conferenceSelected.Id);
                    if (bubbleId == null)
                    {
                        AddInformationMessage($"Cannot get Bubble Id from this Conference Id");
                        return;
                    }

                    var participants = rbConferences.ConferenceGetParticipantsFromCache(conferenceSelected.Id);
                    if (participants?.ContainsKey(participantId) == true)
                    {
                        var participant = participants[participantId];

                        // Check if it's a PSTN Participant in Conf V2
                        if (rbAplication.Restrictions.UseAPIConferenceV2 && !String.IsNullOrEmpty(participant.PhoneNumber))
                        {
                            rbConferences.ConferenceDropPstnParticipant(bubbleId, participant.PhoneNumber, callback =>
                            {
                                if (callback.Result.Success)
                                {
                                    AddInformationMessage($"Conference - PSTN Participant Drop done");
                                }
                                else
                                {
                                    AddInformationMessage($"Conference - PSTN Participant Pb to Drop:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                                    log.LogDebug($"Conference - PSTN Participant Pb to Drop:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                                }
                            });
                        }
                        else
                        {
                            rbConferences.ConferenceDropParticipant(bubbleId, participant.Id, callback =>
                            {
                                if (callback.Result.Success)
                                {
                                    AddInformationMessage($"Conference - Participant Drop done");
                                }
                                else
                                {
                                    AddInformationMessage($"Conference - Participant Pb to Drop:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                                    log.LogDebug($"Conference - Participant Pb to Drop:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                                }
                            });
                        }
                    }
                    else
                        AddInformationMessage($"Conference - Participant not found");
                }
            }
        }

        private void btnConferenceParticipantDelegate_Click(object sender, EventArgs e)
        {
            String participantId = null;

            // Get participant Id
            if (lbParticipants.SelectedItem != null)
            {
                ListItem item = lbParticipants.SelectedItem as ListItem;
                participantId = item.Value;
            }

            if ((conferenceSelected != null) && (participantId != null))
            {
                if (conferenceSelected.Id == rbConferences.PersonalConferenceGetId())
                {
                    AddInformationMessage($"Personal Conference - There is no deleation in Personal Conferences");
                }
                else
                {
                    var bubbleId = rbConferences.GetBubbleIdByConferenceIdFromCache(conferenceSelected.Id);
                    if (bubbleId == null)
                    {
                        AddInformationMessage($"Cannot get Bubble Id from this Conference Id");
                        return;
                    }

                    var participants = rbConferences.ConferenceGetParticipantsFromCache(conferenceSelected.Id);
                    if (participants?.ContainsKey(participantId) == true)
                    {
                        var participant = participants[participantId];

                        // Check if it's a PSTN Participant in Conf V2
                        if (rbAplication.Restrictions.UseAPIConferenceV2 && !String.IsNullOrEmpty(participant.PhoneNumber))
                        {
                            
                            AddInformationMessage($"Conference - You cannot delegate to a PSTN Participant");
                        
                        }
                        else
                        {
                            rbConferences.ConferenceDelegate(bubbleId, participant.Id, callback =>
                            {
                                if (callback.Result.Success)
                                {
                                    AddInformationMessage($"Conference - Participant Delegate done");
                                }
                                else
                                {
                                    AddInformationMessage($"Conference - Participant Pb to Delegate:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                                    log.LogDebug($"Conference - Participant Pb to Delegate:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                                }
                            });
                        }
                    }
                    else
                        AddInformationMessage($"Conference - Participant not found");
                }
            }
        }

        private void cbConferenceInProgress_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cbConferenceInProgress.SelectedItem != null)
            {
                ListItem item = cbConferenceInProgress.SelectedItem as ListItem;
                if(item.Value != conferenceSelected?.Id)
                {
                    var conferenceId = item.Value;
                    if (conferenceId == rbConferences.PersonalConferenceGetId())
                        conferenceSelected = rbConferences.PersonalConferenceGetFromCache();
                    else
                        conferenceSelected = rbConferences.ConferenceGetByIdFromCache(conferenceId);

                    UpdateUIParticipants();
                    UpdateUITalkers();
                    UpdateUIPublishers();

                    UpdateUIConferenceInProgress();
                }
            }
        }

        private void btnConferencesGet_Click(object sender, EventArgs e)
        {
            rbConferences.GetConferencesInProgress(callback =>
            {

            });
        }

        private void btnConferenceGetFullSnapshot_Click(object sender, EventArgs e)
        {
            if (conferenceSelected?.Active == true)
            {
                if (conferenceSelected.Id == rbConferences.PersonalConferenceGetId())
                {
                    rbConferences.PersonalConferenceGetFullSnapshot(callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddInformationMessage(String.Format("Personal Conference - FullSnapshot done"));
                        }
                        else
                        {
                            AddInformationMessage($"Pb to get FullSnapshot for PersonalConference:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                            log.LogDebug($"Pb to get FullSnapshot for PersonalConference:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                        }
                    });
                }
                else
                {
                    String bubbleId = rbConferences.GetBubbleIdByConferenceIdFromCache(conferenceSelected.Id);
                    if (bubbleId == null)
                    {
                        AddInformationMessage($"Cannot get Bubble Id from this Conference Id");
                        return;
                    }

                    rbConferences.ConferenceGetFullSnapshot(bubbleId, callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddInformationMessage(String.Format("Conference - FullSnapshot done"));
                        }
                        else
                        {
                            AddInformationMessage($"Pb to get FullSnapshot for Conference:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                            log.LogDebug($"Pb to get FullSnapshot for Conference:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                        }
                    });
                }
            }
        }


    }
#endregion EVENT FROM FORM ELEMENTS
}
