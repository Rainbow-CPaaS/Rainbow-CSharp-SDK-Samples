using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Rainbow;
using Rainbow.Events;
using Rainbow.Model;

using Microsoft.Extensions.Logging;

using System.Web.UI.WebControls;


namespace Sample_Conferences.csproj
{
    public partial class SampleConferencesForm : Form
    {

        // Define log object
        private static readonly ILogger log = Rainbow.LogFactory.CreateLogger<SampleConferencesForm>();

        //Define Rainbow Application Id, Secret Key and Host Name
        const string APP_ID = "YOUR APP ID";
        const string APP_SECRET_KEY = "YOUR SECRET KEY";
        const string HOST_NAME = "sandbox.openrainbow.com";

        const string LOGIN_USER1 = "YOUR LOGIN";
        const string PASSWORD_USER1 = "YOUR PASSWORD";

        // Define Rainbow objects
        Rainbow.Application rainbowApplication;     // To store Rainbow Application object
        Bubbles rainbowBubbles;                     // To store Rainbow Bubbles object

        Contacts rainbowContacts;                   // To store Rainbow Contacts object
        Contact rainbowMyContact;                   // To store My contact (i.e. the one connected to Rainbow Server)

        Conference conferenceInProgress;            // To Store the conference in progress - In this example, we consider to have only ONE Conference in same time

        // Define delegate to update SampleConferencesForm components
        delegate void VoidDelegate();
        delegate void StringArgReturningVoidDelegate(String value);
        delegate void BoolArgReturningVoidDelegate(Boolean value);
        delegate void IntArgReturningVoidDelegate(Int32 value);
        delegate void CallForwardStatusArgReturningVoidDelegate(CallForwardStatus value);
        delegate void NomadicStatusArgReturningVoidDelegate(NomadicStatus value);
        delegate void CallArgReturningVoidDelegate(Call value);


    #region INIT METHODS
        public SampleConferencesForm()
        {
            InitializeComponent();

            // Set default user / pwd
            tbLogin.Text = LOGIN_USER1;
            tbPassword.Text = PASSWORD_USER1;

            log.LogInformation("==============================================================");
            log.LogInformation("SampleConference started");

            InitializeRainbowSDK();
        }

        private void InitializeRainbowSDK()
        {
            rainbowApplication = new Rainbow.Application(); ;

            // Set Application Id, Secret Key and Host Name
            rainbowApplication.SetApplicationInfo(APP_ID, APP_SECRET_KEY);
            rainbowApplication.SetHostInfo(HOST_NAME);

            // Get Rainbow main objects
            rainbowBubbles = rainbowApplication.GetBubbles();
            rainbowContacts = rainbowApplication.GetContacts();

            // EVENTS WE WANT TO MANAGE SOME EVENTS
            rainbowApplication.ConnectionStateChanged += RainbowApplication_ConnectionStateChanged;
            rainbowBubbles.ConferenceUpdated += RainbowBubbles_ConferenceUpdated;

            // Init other objects
        }

    #endregion INIT METHODS

    #region EVENTS FIRED BY RAINBOW SDK
        private void RainbowApplication_ConnectionStateChanged(object sender, Rainbow.Events.ConnectionStateEventArgs e)
        {
            // Add info about connection state
            AddStateLine($"ConnectionStateChanged:{e.State}");
            UpdateLoginButton(e.State);

            // Update layout since we are not connected to the server
            if (e.State == Rainbow.Model.ConnectionState.Connected)
            {
                rainbowBubbles.GetAllBubbles(callbackBubbles =>
                {
                    string logLine;
                    if (callbackBubbles.Result.Success)
                    {
                        logLine = String.Format("Nb Bubbles:{0}", callbackBubbles.Data.Count);
                    }
                    else
                    {
                        logLine = String.Format("Impossible to get Bubbles list:\r\n{0}", Util.SerializeSdkError(callbackBubbles.Result));
                    }
                    AddStateLine(logLine);
                    log.LogWarning(logLine);
                });

                // Check permissions
                UpdateConferenceAvailable(rainbowBubbles.ConferenceAllowed());
                UpdatePersonalConferenceAvailable(rainbowBubbles.PersonalConferenceAllowed());

                // Update "Conference in progress"
            }
            else
            {
                // Reset permissions
                UpdateConferenceAvailable(false);
                UpdatePersonalConferenceAvailable(false);

                // Clear "Conference in progress" layout
                conferenceInProgress = null;
                UpdateConferenceInProgress();
            }

        }

        private void RainbowBubbles_ConferenceUpdated(object sender, ConferenceEventArgs e)
        {
            conferenceInProgress = e.Conference;

            AddStateLine("Conference Updated: " + conferenceInProgress.ToString());
            log.LogDebug("Conference Updated: " + conferenceInProgress.ToString());

            UpdateConferenceInProgress();
        }

    #endregion EVENTS FIRED BY RAINBOW SDK

    #region METHOD TO UPDATE SampleConferencesForm COMPONENTS

        private void UpdateConferenceAvailable(Boolean value)
        {
            // We can update this compoent only if we are in the from thread
            if (cbConferenceAvailable.InvokeRequired)
            {
                BoolArgReturningVoidDelegate d = new BoolArgReturningVoidDelegate(UpdateConferenceAvailable);
                this.Invoke(d, new object[] { value });
            }
            else
            {
                gbConference.Enabled = value;
                cbConferenceAvailable.Checked = value;
            }
        }

        private void UpdatePersonalConferenceAvailable(Boolean value)
        {
            // We can update this compoent only if we are in the from thread
            if (cbPersonalConferenceAvailable.InvokeRequired)
            {
                BoolArgReturningVoidDelegate d = new BoolArgReturningVoidDelegate(UpdatePersonalConferenceAvailable);
                this.Invoke(d, new object[] { value });
            }
            else
            {
                gbPersonalConference.Enabled = value;
                cbPersonalConferenceAvailable.Checked = value;
            }
        }

        private void UpdateConferenceInProgress()
        {
            // We can update this compoent only if we are in the form thread
            if (gbConferenceInProgress.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateConferenceInProgress);
                this.Invoke(d, new object[] {  });
            }
            else
            {
                log.LogDebug("[UpdateConferenceInProgress] - IN");

                if(conferenceInProgress != null)
                {
                    if (conferenceInProgress.Active)
                    {
                        gbConferenceInProgress.Enabled = true;

                        Bubble bubble = rainbowBubbles.GetBubbleByConferenceIdFromCache(conferenceInProgress.Id);

                        if (bubble == null)
                            AddStateLine("Strange ! We don't have a bubble related to this conference ... This should never occurs ...");

                        // Is Personal Conference ?
                        Bubble personaleConferenceBubble = rainbowBubbles.PersonalConferenceGetBubbleFromCache();
                        if ((bubble != null) && (personaleConferenceBubble != null)
                            && (bubble.Id == personaleConferenceBubble.Id))
                            cbIsPersonalConference.Checked = true;
                        else
                            cbIsPersonalConference.Checked = false;


                        // Bubble Id / Name / Owner
                        if(bubble != null)
                        {
                            tbBubbleId.Text = bubble.Id;
                            tbBubbleName.Text = bubble.Name;

                            Contact owner = rainbowContacts.GetContactFromContactId(bubble.Creator);
                            if (owner != null)
                                tbBubbleOwner.Text = owner.DisplayName;
                            else
                                tbBubbleOwner.Text = "";
                        }
                        else
                        {
                            tbBubbleId.Text = "";
                            tbBubbleName.Text = "";
                            tbBubbleOwner.Text = "";
                        }

                        // Talkers
                        if ((conferenceInProgress.Talkers != null)
                            && (conferenceInProgress.Talkers.Count > 0))
                            tbConferenceTalkers.Text = String.Join(",", conferenceInProgress.Talkers.ToArray());
                        else
                            tbConferenceTalkers.Text = "";

                        // Conference - global mute
                        if (conferenceInProgress.Muted)
                            btnConferenceMute.Text = "Unmute";
                        else
                            btnConferenceMute.Text = "Mute";

                        if (cbIsPersonalConference.Checked)
                        {
                            btnConferenceLock.Enabled = true;
                            /// Conference - global lock
                            if (conferenceInProgress.Locked)
                                btnConferenceLock.Text = "Unlock";
                            else
                                btnConferenceLock.Text = "Lock";
                        }
                        else
                            btnConferenceLock.Enabled = false;

                        // Update participants
                        UpdateConferenceParticipants(conferenceInProgress.Participants);

                        // Update publishers
                        UpdateConferencePublishers(conferenceInProgress.Publishers);

                    }
                    else
                        gbConferenceInProgress.Enabled = false;
                }
                else
                    gbConferenceInProgress.Enabled = false;


                if (!gbConferenceInProgress.Enabled)
                {
                    // Reset form to default values
                    cbIsPersonalConference.Checked = false;
                    tbBubbleId.Text = "";
                    tbBubbleName.Text = "";
                    tbBubbleOwner.Text = "";

                    tbConferenceTalkers.Text = "";

                    btnConferenceMute.Text = "Mute";
                    btnConferenceLock.Text = "Lock";

                    // Reset participants
                    UpdateConferenceParticipants(null);

                    // Reset publishers
                    UpdateConferencePublishers(null);
                }

                log.LogDebug("[UpdateConferenceInProgress] - OUT");
            }
        }

        private void UpdateConferenceParticipants(List<Conference.ConferenceParticipant> list)
        {
            log.LogDebug("[UpdateConferenceParticipants] - IN");
            if ( (list != null) && (list.Count > 0) )
            {
                // Try to always display the participant already selected (if any)
                String selectedId = "";
                Int32 selectedIndex = 0;
                Boolean asModerator = false;
                if (cbParticipantsList.SelectedItem != null)
                    selectedId = ((ListItem)cbParticipantsList.SelectedItem).Value;

                int index = 0;
                cbParticipantsList.Items.Clear();
                foreach (Conference.ConferenceParticipant participant in list)
                {
                    cbParticipantsList.Items.Add(new ListItem(participant.Id, participant.Id));
                    if (participant.Privilege == Bubble.MemberPrivilege.Moderator && (participant.Jid_im == rainbowMyContact.Jid_im))
                        asModerator = true;
                    if (participant.Id == selectedId)
                        selectedIndex = index;
                    index++;
                }
                cbParticipantsList.SelectedIndex = selectedIndex;
                cbAsModerator.Checked = asModerator;
            }
            else
            {
                cbAsModerator.Checked = false;

                tbParticipantId.Text = "";
                tbParticipantJid.Text = "";
                tbParticipantPhoneNumber.Text = "";
                cbParticipantModerator.Checked = false;
                cbParticipantMuted.Checked = false;
                cbParticipantHold.Checked = false;
                cbParticipantConnected.Checked = false;

                cbParticipantsList.SelectedIndex = -1;
                cbParticipantsList.Items.Clear();
            }

            // Enable Mute / Drop participant buttons only if moderator
            btnParticipantMute.Enabled = cbAsModerator.Checked;
            btnParticipantDrop.Enabled = cbAsModerator.Checked;

            // Enable Mute / Lock / Stop confrence buttons only if moderator
            btnConferenceMute.Enabled = cbAsModerator.Checked;
            btnConferenceLock.Enabled = cbAsModerator.Checked;
            btnConferenceStop.Enabled = cbAsModerator.Checked;
            log.LogDebug("[UpdateConferenceParticipants] - OUT");
        }

        private void UpdateConferencePublishers(List<Conference.Publisher> list)
        {
            log.LogDebug("[UpdateConferencePublishers] - IN");
            if ((list != null) && (list.Count > 0))
            {
                // Try to always display the publisher already selected (if any)
                String selectedId = "";
                Int32 selectedIndex = 0;
                if (cbPublishersList.SelectedItem != null)
                    selectedId = ((ListItem)cbPublishersList.SelectedItem).Value;

                int index = 0;
                cbPublishersList.Items.Clear();
                foreach (Conference.Publisher publisher in list)
                {
                    cbPublishersList.Items.Add(new ListItem(publisher.Id, publisher.Id));
                    if (publisher.Id == selectedId)
                        selectedIndex = index;
                    index++;
                }
                cbPublishersList.SelectedIndex = selectedIndex;
            }
            else
            {
                tbPublisherId.Text = "";
                tbPublisherJid.Text = "";
                cbPublisherSharing.Checked = false;
                cbPublisherVideo.Checked = false;

                cbPublishersList.SelectedIndex = -1;
                cbPublishersList.Items.Clear();
            }
            log.LogDebug("[UpdateConferencePublishers] - OUT");
        }

        /// <summary>
        /// Permits to update Login / logout btn
        /// </summary>
        /// <param name="connectionState"></param>
        private void UpdateLoginButton(string connectionState)
        {
            // We can update this compoent only if we are in the from thread
            if (tbLogin.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(UpdateLoginButton);
                this.Invoke(d, new object[] { connectionState });
            }
            else
            {
                if (connectionState == Rainbow.Model.ConnectionState.Connected)
                {
                    tbLogin.Enabled = tbPassword.Enabled = true;
                    btnLoginLogout.Enabled = true;
                    btnLoginLogout.Text = "Logout";
                }
                else if (connectionState == Rainbow.Model.ConnectionState.Disconnected)
                {
                    tbLogin.Enabled = tbPassword.Enabled = true;
                    btnLoginLogout.Enabled = true;
                    btnLoginLogout.Text = "Login";
                }
                else if (connectionState == Rainbow.Model.ConnectionState.Connecting)
                {
                    tbLogin.Enabled = tbPassword.Enabled = false;
                    btnLoginLogout.Enabled = false;
                    btnLoginLogout.Text = "Connecting";
                }
            }
        }

        /// <summary>
        /// Permits to add a new sting in the text box at the bottom of the form: it permits to log things happening
        /// </summary>
        /// <param name="info"></param>
        private void AddStateLine(String info)
        {
            // We can update this compoent only if we are in the from thread
            if (tbState.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(AddStateLine);
                this.Invoke(d, new object[] { info });
            }
            else
            {
                DateTime dateTime = DateTime.Now;
                String output = String.Format("{0}{1} - {2}", String.IsNullOrEmpty(tbState.Text)? "" : "\r\n", dateTime.ToString("o"), info);
                tbState.AppendText(output);
            }
        }

    #endregion METHOD TO UPDATE SampleConferencesForm COMPONENTS


    #region EVENTS FIRED BY SampleConferencesForm ELEMENTS

        private void btnLoginLogout_Click(object sender, EventArgs e)
        {
            if (rainbowApplication.IsConnected())
            {
                // We want to logout
                rainbowApplication.Logout(callback =>
                {
                    if (!callback.Result.Success)
                    {
                        string logLine = String.Format("Impossible to logout:\r\n{0}", Util.SerializeSdkError(callback.Result));
                        AddStateLine(logLine);
                        log.LogWarning(logLine);
                    }
                });
            }
            else
            {
                String login = tbLogin.Text;
                String password = tbPassword.Text;

                // We want to login
                rainbowApplication.Login(login, password, callback =>
                {
                    if (callback.Result.Success)
                    {
                        // Since we are connected, we get the current contact object
                        rainbowMyContact = rainbowContacts.GetCurrentContact();
                    }
                    else
                    {
                        string logLine = String.Format("Impossible to login:\r\n{0}", Util.SerializeSdkError(callback.Result));
                        AddStateLine(logLine);
                        log.LogWarning(logLine);
                    }
                });
            }
        }

        private void btnPersonalConferencePhoneNumbers_Click(object sender, EventArgs e)
        {
            rainbowBubbles.PersonalConferenceGetPhoneNumbers(callback =>
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
                        foreach (ConferencePhoneNumber phone in listNear)
                            log.LogDebug("[PersonalConferenceGetPhoneNumbers] - Near phone number:[{0}]", phone.ToString());
                    }
                    else
                        log.LogDebug("[PersonalConferenceGetPhoneNumbers] - there is no phone near near the end user");

                    //get other list of phones
                    listOthers = instantMeetingPhoneNumbers.Others;
                    if ((listOthers != null) && (listOthers.Count > 0))
                    {
                        nbOthers = listOthers.Count;
                        foreach (ConferencePhoneNumber phone in listOthers)
                            log.LogDebug("[PersonalConferenceGetPhoneNumbers] - Other phone number:[{0}]", phone.ToString());
                    }
                    else
                        log.LogDebug("[PersonalConferenceGetPhoneNumbers] - there is no other phone numner ...");

                    AddStateLine(String.Format("Audio phone numbers found: NbNear:[{0}] - NbOthers:[{1}]", nbNear, nbOthers));
                }
                else
                {
                    AddStateLine("Pb to get audio phone numbers ...");
                    log.LogDebug("Pb to get audio phone numbers - error:[{0}]", Util.SerializeSdkError(callback.Result));
                }
            });
        }

        private void btnPersonalConferencePassCodes_Click(object sender, EventArgs e)
        {
            rainbowBubbles.PersonalConferenceGetPassCodes(callback =>
            {
                if (callback.Result.Success)
                {
                    ConferencePassCodes meetingPassCodes = callback.Data;
                    AddStateLine(String.Format("Personal Conference PassCodes:[{0}]", meetingPassCodes.ToString()));
                }
                else
                {
                    AddStateLine("Pb to get Personal Conference PassCodes ...");
                    log.LogDebug("Pb to Personal Conference PassCodes - error:[{0}]", Util.SerializeSdkError(callback.Result));
                }
            });
        }

        private void btnPersonalConferenceResetPassCodes_Click(object sender, EventArgs e)
        {
            rainbowBubbles.PersonalConferenceResetPassCodes(callback =>
            {
                if (callback.Result.Success)
                {
                    ConferencePassCodes meetingPassCodes = callback.Data;
                    AddStateLine(String.Format("Reset - Personal Conference PassCodes:[{0}]", meetingPassCodes.ToString()));
                }
                else
                {
                    AddStateLine("Pb to reset Personal Conference PassCodes ...");
                    log.LogDebug("Pb to reset Personal Conference PassCodes - error:[{0}]", Util.SerializeSdkError(callback.Result));
                }
            });
        }

        private void btnPersonalConferenceUrl_Click(object sender, EventArgs e)
        {
            rainbowBubbles.PersonalConferenceGetPublicUrl(callback =>
            {
                if (callback.Result.Success)
                {
                    String url = callback.Data;
                    AddStateLine(String.Format("Personal Conference URL:[{0}]", url));
                }
                else
                {
                    AddStateLine("Pb to get Personal Conference URL ...");
                    log.LogDebug("Pb to Personal Conference URL - error:[{0}]", Util.SerializeSdkError(callback.Result));
                }
            });
        }

        private void btnPersonalConferenceResetUrl_Click(object sender, EventArgs e)
        {
            rainbowBubbles.PersonalConferenceGenerateNewPublicUrl(callback =>
            {
                if (callback.Result.Success)
                {
                    String url = callback.Data;
                    AddStateLine(String.Format("New Personal Conference URL:[{0}]", url));
                }
                else
                {
                    AddStateLine("Pb to reset Personal Conference URL ...");
                    log.LogDebug("Pb to reset Personal Conference URL - error:[{0}]", Util.SerializeSdkError(callback.Result));
                }
            });
        }

        private void btnPersonalConferenceStart_Click(object sender, EventArgs e)
        {
            rainbowBubbles.PersonalConferenceStart(callback =>
            {
                if (callback.Result.Success)
                {
                    AddStateLine(String.Format("Personal Conference - Start done"));
                }
                else
                {
                    AddStateLine("Pb to start Personal Conference ...");
                    log.LogDebug("Pb to start Personal Conference - error:[{0}]", Util.SerializeSdkError(callback.Result));
                }
            });
        }

        private void btnPersonalConferenceJoin_Click(object sender, EventArgs e)
        {
            Boolean asModerator = cbPersonalConferenceModerator.Checked;
            Boolean muted = cbPersonalConferenceMuted.Checked;
            String phoneNumber = tbPersonalConferencePhoneNumber.Text;

            if (String.IsNullOrEmpty(phoneNumber))
            {
                AddStateLine(String.Format("Personal Conference - Cannot Join - No phone number specified"));
                return;
            }

            rainbowBubbles.PersonalConferenceJoin(asModerator, muted, phoneNumber, "", callback =>
            {
                if (callback.Result.Success)
                {
                    AddStateLine(String.Format("Personal Conference - Join done"));
                }
                else
                {
                    AddStateLine("Pb to join Personal Conference ...");
                    log.LogDebug("Pb to join Personal Conference - error:[{0}]", Util.SerializeSdkError(callback.Result));
                }
            });
        }

        private void btnConferenceMute_Click(object sender, EventArgs e)
        {
            if(conferenceInProgress != null)
            {
                rainbowBubbles.ConferenceMuteOrUnmute(conferenceInProgress.Id, !conferenceInProgress.Muted, callback =>
                {
                    if (callback.Result.Success)
                    {
                        AddStateLine(String.Format("Personal Conference - Mute/Unmute done"));
                    }
                    else
                    {
                        AddStateLine("Pb to mute/unmute Personal Conference ...");
                        log.LogDebug("Pb to mute/unmute Personal Conference - error:[{0}]", Util.SerializeSdkError(callback.Result));
                    }
                });
            }
        }

        private void btnConferenceLock_Click(object sender, EventArgs e)
        {
            if (conferenceInProgress != null)
            {
                if (rainbowBubbles.PersonalConferenceGetId() == conferenceInProgress.Id)
                {
                    rainbowBubbles.PersonalConferenceLockOrUnlock(!conferenceInProgress.Locked, callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddStateLine(String.Format("Personal Conference - Lock/Unlock done"));
                        }
                        else
                        {
                            AddStateLine("Pb to Lock/Unlock Personal Conference ...");
                            log.LogDebug("Pb to Lock/Unlock Personal Conference - error:[{0}]", Util.SerializeSdkError(callback.Result));
                        }
                    });
                }
            }
        }

        private void btnConferenceStop_Click(object sender, EventArgs e)
        {
            if (conferenceInProgress != null)
            {
                rainbowBubbles.ConferenceStop(conferenceInProgress.Id, callback =>
                {
                    if (callback.Result.Success)
                    {
                        AddStateLine(String.Format("Personal Conference - Stop done"));
                    }
                    else
                    {
                        AddStateLine("Pb to stop Personal Conference ...");
                        log.LogDebug("Pb to stop Personal Conference - error:[{0}]", Util.SerializeSdkError(callback.Result));
                    }
                });
            }
        }

        private void cbParticipantsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(conferenceInProgress != null)
            {
                if ( (cbParticipantsList.SelectedIndex < conferenceInProgress.Participants.Count)
                    && (cbParticipantsList.SelectedIndex >= 0) )
                {
                    Conference.ConferenceParticipant participant = conferenceInProgress.Participants[cbParticipantsList.SelectedIndex];

                    tbParticipantId.Text = participant.Id;
                    tbParticipantJid.Text = participant.Jid_im;
                    tbParticipantPhoneNumber.Text = participant.PhoneNumber;

                    cbParticipantModerator.Checked = (participant.Privilege == Bubble.MemberPrivilege.Moderator);
                    cbParticipantMuted.Checked = participant.Muted;
                    cbParticipantHold.Checked = participant.Hold;
                    cbParticipantConnected.Checked = participant.Connected;

                    if(cbParticipantMuted.Checked)
                        btnParticipantMute.Text = "Unmute";
                    else
                        btnParticipantMute.Text = "Mute";
                }
            }
        }

        private void cbPublishersList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (conferenceInProgress != null)
            {
                if ( (cbPublishersList.SelectedIndex < conferenceInProgress.Publishers.Count)
                    && (cbPublishersList.SelectedIndex >= 0) )
                {
                    Conference.Publisher publisher = conferenceInProgress.Publishers[cbPublishersList.SelectedIndex];

                    tbParticipantId.Text = publisher.Id;
                    tbParticipantJid.Text = publisher.Jid_im;

                    cbPublisherSharing.Checked = false;
                    cbPublisherSharing.Checked = false;

                    if ( (publisher.Media != null) && (publisher.Media.Count > 0) )
                    {
                        foreach(String media in publisher.Media)
                        {
                            if (media == "sharing")
                                cbPublisherSharing.Checked = true;
                            else if (media == "video")
                                cbPublisherVideo.Checked = true;
                        }
                    }

                }
            }
        }

        private void btnParticipantMute_Click(object sender, EventArgs e)
        {
            if (conferenceInProgress != null)
            {
                if (cbParticipantsList.SelectedIndex < conferenceInProgress.Participants.Count)
                {
                    Conference.ConferenceParticipant participant = conferenceInProgress.Participants[cbParticipantsList.SelectedIndex];

                    rainbowBubbles.ConferenceMuteOrUnmutParticipant(conferenceInProgress.Id, participant.Id, !participant.Muted, callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddStateLine(String.Format("Personal Conference - Mute/Unmute Participant done"));
                        }
                        else
                        {
                            AddStateLine("Pb to Mute/Unmute Participant ...");
                            log.LogDebug("Pb to Mute/Unmute Participant - error:[{0}]", Util.SerializeSdkError(callback.Result));
                        }
                    });
                }
            }
        }

        private void btnParticipantDrop_Click(object sender, EventArgs e)
        {
            if (conferenceInProgress != null)
            {
                if (cbParticipantsList.SelectedIndex < conferenceInProgress.Participants.Count)
                {
                    Conference.Participant participant = conferenceInProgress.Participants[cbParticipantsList.SelectedIndex];

                    rainbowBubbles.ConferenceDropParticipant(conferenceInProgress.Id, participant.Id, callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddStateLine(String.Format("Personal Conference - Drop Participant done"));
                        }
                        else
                        {
                            AddStateLine("Pb to Drop Participant ...");
                            log.LogDebug("Pb to Drop Participant - error:[{0}]", Util.SerializeSdkError(callback.Result));
                        }
                    });
                }
            }
        }

    #endregion EVENTS FIRED BY SampleConferencesForm ELEMENTS

    }
}
