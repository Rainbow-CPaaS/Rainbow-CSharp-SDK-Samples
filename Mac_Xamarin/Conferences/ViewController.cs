using System;
using System.Collections.Generic;
using System.IO;
using AppKit;
using Foundation;
using Rainbow;
using Rainbow.Model;

namespace SampleConferences
{
    public partial class ViewController : NSViewController
    {
        private static readonly log4net.ILog log = Rainbow.LogConfigurator.GetLogger(typeof(ViewController));

        const string APP_ID = "YOUR_APP_ID";
        const string APP_SECRET_KEY = "YOUR_APP_SECRET_KEY";
        const string HOSTNAME = "sandbox.openrainbow.com";

        const string LOGIN = "YOUR_LOGIN";
        const string PASSWORD = "YOUR_PASSWORD";

        // Define Rainbow objects
        Rainbow.Application rainbowApplication;     // To store Rainbow Application object
        Bubbles rainbowBubbles;                     // To store Rainbow Bubbles object

        Rainbow.Contacts rainbowContacts;           // To store Rainbow Contacts object
        Contact rainbowMyContact;                   // To store My contact (i.e. the one connected to Rainbow Server)

        Conference conferenceInProgress;            // To store the conference in progress - In this example, we consider to have only ONE Conference in same time

        #region INIT METHODS
        public ViewController(IntPtr handle) : base(handle)
        {
            InitializeLog();

            InitializeRainbowSDK();
        }

        private void InitializeLog()
        {
            string logPath = @"./log4netConfiguration.xml";

            FileInfo fileInfo = new FileInfo(logPath);

            // Does the file really exist ?
            if ((fileInfo != null) && File.Exists(fileInfo.FullName))
            {
                // Get repository object used by the SDK itself
                log4net.Repository.ILoggerRepository repository = Rainbow.LogConfigurator.GetRepository();

                // Configure XMLConfigurator using our XML file for our repository
                log4net.Config.XmlConfigurator.Configure(repository, fileInfo);
            }

            log.Info("==========================================");
            log.Info("SampleContact.ViewController started");
        }

        private void InitializeRainbowSDK()
        {
            rainbowApplication = new Rainbow.Application(); ;

            // Set Application Id, Secret Key and Host Name
            rainbowApplication.SetApplicationInfo(APP_ID, APP_SECRET_KEY);
            rainbowApplication.SetHostInfo(HOSTNAME);

            // Get Rainbow main objects
            rainbowBubbles = rainbowApplication.GetBubbles();
            rainbowContacts = rainbowApplication.GetContacts();

            // Events we want to manage
            rainbowApplication.ConnectionStateChanged += RainbowApplication_ConnectionStateChanged;
            rainbowBubbles.ConferenceUpdated += RainbowBubbles_ConferenceUpdated;

            // Init other objects
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Do any additional setup after loading the view.

            txt_login.StringValue = LOGIN;
            txt_password.StringValue = PASSWORD;
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }

        #endregion INIT METHODS


        #region EVENTS FIRED BY UI

        partial void btnLogin_Clicked(NSObject sender)
        {
            ConnectToRainbow();
        }

        partial void btnPersonalConference_PhoneNumbers_Clicked(NSObject sender)
        {
            PersonalConferenceGetPhoneNumbers();
        }

        partial void btnPersonalConference_PassCodes_Clicked(NSObject sender)
        {
            PersonalConferenceGetPasscodes();
        }

        partial void btnPersonalConference_ResetPassCodes_Clicked(NSObject sender)
        {
            PersonalConferenceResetPasscodes();
        }

        partial void btnPersonalConference_Url_Clicked(NSObject sender)
        {
            PersonalConferenceGetUrl();
        }

        partial void btnPersonalConference_ResetUrl_Clicked(NSObject sender)
        {
            PersonalConferenceResetUrl();
        }

        partial void btnPersonalConference_Start_Clicked(NSObject sender)
        {
            PersonalConferenceStart();
        }

        partial void btnPersonalConference_Join_Clicked(NSObject sender)
        {
            PersonalConferenceJoin();
        }

        partial void btnConferenceMute_Clicked(NSObject sender)
        {
            ConferenceMute();
        }

        partial void btnConferenceLock_Clicked(NSObject sender)
        {
            ConferenceLock();
        }

        partial void btnConferenceStop_Clicked(NSObject sender)
        {
            ConferenceStop();
        }

        partial void btnParticipantDrop_Clicked(NSObject sender)
        {
            ParticipantDrop();
        }

        partial void btnParticipantMute_Clicked(NSObject sender)
        {
            ParticipantMute();
        }

        partial void cbParticipantsList_SelectionChanged(NSObject sender)
        {
            if (conferenceInProgress != null)
            {
                if ((cbParticipantsList.SelectedItemIndex < conferenceInProgress.Participants.Count)
                    && (cbParticipantsList.SelectedItemIndex >= 0))
                {
                    Conference.Participant participant = conferenceInProgress.Participants[(int)cbParticipantsList.SelectedItemIndex];

                    txtParticipantId.StringValue = participant.Id;
                    txtParticipantJid.StringValue = participant.Jid_im;
                    txtParticipant_PhoneNumber.StringValue = participant.PhoneNumber;

                    cbParticipantModerator.State = (participant.Moderator ? NSCellStateValue.On : NSCellStateValue.Off);
                    cbParticipantMuted.State = (participant.Muted ? NSCellStateValue.On : NSCellStateValue.Off);
                    cbParticipantHold.State = (participant.Hold ? NSCellStateValue.On : NSCellStateValue.Off);
                    cbParticipantConnected.State = (participant.Connected ? NSCellStateValue.On : NSCellStateValue.Off);

                    if (cbParticipantMuted.State == NSCellStateValue.On)
                        btnParticipantMute.StringValue = "Unmute";
                    else
                        btnParticipantMute.StringValue = "Mute";
                }
            }
        }

        partial void cbPublishersList_SelectionChanged(NSObject sender)
        {
            if (conferenceInProgress != null)
            {
                if ((cbPublishersList.SelectedItemIndex < conferenceInProgress.Publishers.Count)
                    && (cbPublishersList.SelectedItemIndex >= 0))
                {
                    Conference.Publisher publisher = conferenceInProgress.Publishers[(int)cbPublishersList.SelectedItemIndex];

                    txtParticipantId.StringValue = publisher.Id;
                    txtParticipantJid.StringValue = publisher.Jid_im;

                    cbPublisherSharing.State = NSCellStateValue.Off;
                    cbPublisherSharing.State = NSCellStateValue.Off;

                    if ((publisher.Media != null) && (publisher.Media.Count > 0))
                    {
                        foreach (String media in publisher.Media)
                        {
                            if (media == "sharing")
                                cbPublisherSharing.State = NSCellStateValue.On;
                            else if (media == "video")
                                cbPublisherVideo.State = NSCellStateValue.On;
                        }
                    }
                }
            }
        }

        #endregion EVENTS FIRED BY UI


        #region UTILS

        private void ConnectToRainbow()
        {
            if (rainbowApplication.IsConnected())
            {
                rainbowApplication.Logout(callback =>
                {
                    if (!callback.Result.Success)
                    {
                        string logline = String.Format("Impossible to logout:\r\n{0}", Util.SerialiseSdkError(callback.Result));
                        AddStateLine(logline);
                        log.WarnFormat(logline);
                    }
                });
            }
            else
            {
                String login = txt_login.StringValue;
                String password = txt_password.StringValue;

                rainbowApplication.Login(login, password, callback =>
                {
                    if (callback.Result.Success)
                    {
                        rainbowMyContact = rainbowContacts.GetCurrentContact();
                        AddStateLine("Welcome " + rainbowMyContact.DisplayName);
                    }
                    else
                    {
                        string logline = String.Format("Impossible to login:\r\n{0}", Util.SerialiseSdkError(callback.Result));
                        AddStateLine(logline);
                        log.WarnFormat(logline);
                    }
                });
            }
        }

        private void PersonalConferenceGetPhoneNumbers()
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
                            log.DebugFormat("[PersonalConferenceGetPhoneNumbers] - Near phone number:[{0}]", phone.ToString());
                    }
                    else
                        log.DebugFormat("[PersonalConferenceGetPhoneNumbers] - There is no phone near the end user");

                    //get other list of phones
                    listOthers = instantMeetingPhoneNumbers.Others;
                    if ((listOthers != null) && (listOthers.Count > 0))
                    {
                        nbOthers = listOthers.Count;
                        foreach (ConferencePhoneNumber phone in listOthers)
                            log.DebugFormat("[PersonalConferenceGetPhoneNumbers] - Other phone number:[{0}]", phone.ToString());
                    }
                    else
                        log.DebugFormat("[PersonalConferenceGetPhoneNumbers] - there is no other phone number ...");

                    AddStateLine(String.Format("Audio phone numbers found: NbNear:[{0}] - NbOthers:[{1}]", nbNear, nbOthers));
                }
                else
                {
                    AddStateLine("Impossible to get audio phone numbers ...");
                    log.DebugFormat("Impossible to get audio phone numbers - error:[{0}]", Util.SerialiseSdkError(callback.Result));
                }
            });
        }

        private void PersonalConferenceGetPasscodes()
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
                    AddStateLine("Impossible to get Personal Conference PassCodes ...");
                    log.DebugFormat("Impossible to Personal Conference PassCodes - error:[{0}]", Util.SerialiseSdkError(callback.Result));
                }
            });
        }

        private void PersonalConferenceResetPasscodes()
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
                    AddStateLine("Impossible to reset Personal Conference PassCodes ...");
                    log.DebugFormat("Impossible to reset Personal Conference PassCodes - error:[{0}]", Util.SerialiseSdkError(callback.Result));
                }
            });
        }

        private void PersonalConferenceGetUrl()
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
                    AddStateLine("Impossible to get Personal Conference URL ...");
                    log.DebugFormat("Impossible to get Personal Conference URL - error:[{0}]", Util.SerialiseSdkError(callback.Result));
                }
            });
        }

        private void PersonalConferenceResetUrl()
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
                    AddStateLine("Impossible to reset Personal Conference URL ...");
                    log.DebugFormat("Impossible to reset Personal Conference URL - error:[{0}]", Util.SerialiseSdkError(callback.Result));
                }
            });
        }
        
        private void PersonalConferenceStart()
        {
            rainbowBubbles.PersonalConferenceStart(callback =>
            {
                if (callback.Result.Success)
                {
                    AddStateLine(String.Format("Personal Conference - Start done"));
                }
                else
                {
                    AddStateLine("Impossible to start Personal Conference ...");
                    log.DebugFormat("Impossible to start Personal Conference - error:[{0}]", Util.SerialiseSdkError(callback.Result));
                }
            });
        }

        private void PersonalConferenceJoin()
        {
            Boolean asModerator = cbPersonalConferenceModerator.State == NSCellStateValue.On;
            Boolean muted = cbPersonalConferenceMuted.State == NSCellStateValue.On;
            String phoneNumber = txtPersonalConference_PhoneNumber.StringValue;

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
                    AddStateLine("Impossible to join Personal Conference ...");
                    log.DebugFormat("Impossible to join Personal Conference - error:[{0}]", Util.SerialiseSdkError(callback.Result));
                }
            });
        }

        private void ConferenceMute()
        {
            if (conferenceInProgress != null)
            {
                rainbowBubbles.ConferenceMuteOrUnmute(conferenceInProgress.Id, !conferenceInProgress.Muted, callback =>
                {
                    if (callback.Result.Success)
                    {
                        AddStateLine(String.Format("Personal Conference - Mute/Unmute done"));
                    }
                    else
                    {
                        AddStateLine("Impossible to mute/unmute Personal Conference ...");
                        log.DebugFormat("Impossible to mute/unmute Personal Conference - error:[{0}]", Util.SerialiseSdkError(callback.Result));
                    }
                });
            }
        }

        private void ConferenceLock()
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
                            AddStateLine("Impossible to Lock/Unlock Personal Conference ...");
                            log.DebugFormat("Impossible to Lock/Unlock Personal Conference - error:[{0}]", Util.SerialiseSdkError(callback.Result));
                        }
                    });
                }
            }
        }

        private void ConferenceStop()
        {
            if (conferenceInProgress != null)
            {
                rainbowBubbles.ConferenceStop(conferenceInProgress.Id, callback =>
                {
                    if (callback.Result.Success)
                    {
                        AddStateLine(String.Format("Personal Conference - Stopped"));
                    }
                    else
                    {
                        AddStateLine("Impossible to stop Personal Conference ...");
                        log.DebugFormat("Impossible to stop Personal Conference - error:[{0}]", Util.SerialiseSdkError(callback.Result));
                    }
                });
            }
        }

        private void ParticipantMute()
        {
            if (conferenceInProgress != null)
            {
                if (cbParticipantsList.SelectedItemIndex < conferenceInProgress.Participants.Count)
                {
                    Conference.Participant participant = conferenceInProgress.Participants[(int)cbParticipantsList.SelectedItemIndex];

                    rainbowBubbles.ConferenceMuteOrUnmutParticipant(conferenceInProgress.Id, participant.Id, !participant.Muted, callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddStateLine(String.Format("Personal Conference - Mute/Unmute Participant done"));
                        }
                        else
                        {
                            AddStateLine("Pb to Mute/Unmute Participant ...");
                            log.DebugFormat("Pb to Mute/Unmute Participant - error:[{0}]", Util.SerialiseSdkError(callback.Result));
                        }
                    });
                }
            }
        }

        private void ParticipantDrop()
        {
            if (conferenceInProgress != null)
            {
                if (cbParticipantsList.SelectedItemIndex < conferenceInProgress.Participants.Count)
                {
                    Conference.Participant participant = conferenceInProgress.Participants[(int)cbParticipantsList.SelectedItemIndex];

                    rainbowBubbles.ConferenceDropParticipant(conferenceInProgress.Id, participant.Id, callback =>
                    {
                        if (callback.Result.Success)
                        {
                            AddStateLine(String.Format("Personal Conference - Drop Participant done"));
                        }
                        else
                        {
                            AddStateLine("Pb to Drop Participant ...");
                            log.DebugFormat("Pb to Drop Participant - error:[{0}]", Util.SerialiseSdkError(callback.Result));
                        }
                    });
                }
            }
        }

        #endregion UTILS


        #region METHODS TO UPDATE UI

        private void AddStateLine(String info)
        {
            InvokeOnMainThread(() => { txt_state.Value = info + "\r\n" + txt_state.Value; });
        }

        private void UpdateLoginButton(string connectionState)
        {
            // We can only update this component if we are in the from thread
            InvokeOnMainThread(() =>
            {
                if (connectionState == ConnectionState.Connected)
                {
                    btnLogin.Title = "Logout";
                    btnLogin.Enabled = true;
                }
                else if (connectionState == ConnectionState.Disconnected)
                {
                    btnLogin.Title = "Login";
                    btnLogin.Enabled = true;
                }
                else if (connectionState == ConnectionState.Connecting)
                {
                    btnLogin.Title = "Connecting";
                    btnLogin.Enabled = false;
                }
            });
        }

        private void UpdateConferenceAvailable(Boolean value)
        {
            // We can only update this component if we are in the from thread
            InvokeOnMainThread(() =>
            {
                box_webRTC.Hidden = !value;
                cbConferenceAvailable.State = (value ? NSCellStateValue.On : NSCellStateValue.Off);
            });
        }

        private void UpdateConferenceInProgress()
        {
            InvokeOnMainThread(() =>
            {

                log.DebugFormat("[UpdateConferenceInProgress] - IN");

                if (conferenceInProgress != null)
                {
                    if (conferenceInProgress.Active)
                    {
                        box_inProgress.Hidden = false;

                        Bubble bubble = rainbowBubbles.GetBubbleByConferenceIdFromCache(conferenceInProgress.Id);

                        if (bubble == null)
                            AddStateLine("Strange ! We don't have a bubble related to this conference ... This should never occur ...");

                        // Is it a Personal Conference ?
                        Bubble personaleConferenceBubble = rainbowBubbles.PersonalConferenceGetBubbleFromCache();
                        if ((bubble != null) && (personaleConferenceBubble != null)
                            && (bubble.Id == personaleConferenceBubble.Id))
                            cbIsPersonalConference.State = NSCellStateValue.On;
                        else
                            cbIsPersonalConference.State = NSCellStateValue.Off;


                        // Bubble Id / Name / Owner
                        if (bubble != null)
                        {
                            txtBubbleId.StringValue = bubble.Id;
                            txtBubbleName.StringValue = bubble.Name;

                            Contact owner = rainbowContacts.GetContactFromContactId(bubble.Creator);
                            if (owner != null)
                                txtBubbleOwner.StringValue = owner.DisplayName;
                            else
                                txtBubbleOwner.StringValue = "";
                        }
                        else
                        {
                            txtBubbleId.StringValue = "";
                            txtBubbleName.StringValue = "";
                            txtBubbleOwner.StringValue = "";
                        }

                        // Talkers
                        if ((conferenceInProgress.Talkers != null)
                            && (conferenceInProgress.Talkers.Count > 0))
                            txtConferenceTalkers.StringValue = String.Join(",", conferenceInProgress.Talkers.ToArray());
                        else
                            txtConferenceTalkers.StringValue = "";

                        // Conference - global mute
                        if (conferenceInProgress.Muted)
                            btnConferenceMute.StringValue = "Unmute";
                        else
                            btnConferenceMute.StringValue = "Mute";

                        if (cbIsPersonalConference.State == NSCellStateValue.On)
                        {
                            btnConferenceLock.Enabled = true;
                            /// Conference - global lock
                            if (conferenceInProgress.Locked)
                                btnConferenceLock.StringValue = "Unlock";
                            else
                                btnConferenceLock.StringValue = "Lock";
                        }
                        else
                            btnConferenceLock.Enabled = false;

                        // Update participants
                        UpdateConferenceParticipants(conferenceInProgress.Participants);

                        // Update publishers
                        UpdateConferencePublishers(conferenceInProgress.Publishers);

                    }
                    else
                        box_inProgress.Hidden = true;
                }
                else
                    box_inProgress.Hidden = true;


                if (box_inProgress.Hidden)
                {
                    // Reset form to default values
                    cbIsPersonalConference.State = NSCellStateValue.Off;
                    txtBubbleId.StringValue = "";
                    txtBubbleName.StringValue = "";
                    txtBubbleOwner.StringValue = "";

                    txtConferenceTalkers.StringValue = "";

                    btnConferenceMute.StringValue = "Mute";
                    btnConferenceLock.StringValue = "Lock";

                    // Reset participants
                    UpdateConferenceParticipants(null);

                    // Reset publishers
                    UpdateConferencePublishers(null);
                }

                log.DebugFormat("[UpdateConferenceInProgress] - OUT");
            });
        }

        private void UpdateConferenceParticipants(List<Conference.Participant> list)
        {
            InvokeOnMainThread(() =>
            {
                log.DebugFormat("[UpdateConferenceParticipants] - IN");
                if ((list != null) && (list.Count > 0))
                {
                    // Try to always display the participant already selected (if any)
                    String selectedId = "";
                    Int32 selectedIndex = 0;
                    Boolean asModerator = false;
                    if (cbParticipantsList.SelectedItem != null)
                        selectedId = cbParticipantsList.SelectedItem.Title;

                    int index = 0;
                    cbParticipantsList.RemoveAllItems();
                    foreach (Conference.Participant participant in list)
                    {                        
                        cbParticipantsList.AddItem(new NSString(participant.Id));

                        if (participant.Moderator && (participant.Jid_im == rainbowMyContact.Jid_im))
                            asModerator = true;
                        if (participant.Id == selectedId)
                            selectedIndex = index;
                        index++;
                    }
                    //cbParticipantsList.SelectedIndex = selectedIndex;
                    cbAsModerator.State = (asModerator ? NSCellStateValue.On : NSCellStateValue.Off);
                }
                else
                {
                    cbAsModerator.State = NSCellStateValue.Off;

                    txtParticipantId.StringValue = "";
                    txtParticipantJid.StringValue = "";
                    txtParticipant_PhoneNumber.StringValue = "";
                    cbParticipantModerator.State = NSCellStateValue.Off;
                    cbParticipantMuted.State = NSCellStateValue.Off;
                    cbParticipantHold.State = NSCellStateValue.Off;
                    cbParticipantConnected.State = NSCellStateValue.Off;

                    cbParticipantsList.RemoveAllItems();
                }

                // Enable Mute / Drop participant buttons only if moderator
                btnParticipantMute.Enabled = (cbAsModerator.State == NSCellStateValue.On);
                btnParticipantDrop.Enabled = (cbAsModerator.State == NSCellStateValue.On);

                // Enable Mute / Lock / Stop confrence buttons only if moderator
                btnConferenceMute.Enabled = (cbAsModerator.State == NSCellStateValue.On);
                btnConferenceLock.Enabled = (cbAsModerator.State == NSCellStateValue.On);
                btnConferenceStop.Enabled = (cbAsModerator.State == NSCellStateValue.On);
                log.DebugFormat("[UpdateConferenceParticipants] - OUT");
            });
        }

        private void UpdateConferencePublishers(List<Conference.Publisher> list)
        {
            InvokeOnMainThread(() =>
            {
                log.DebugFormat("[UpdateConferencePublishers] - IN");
                if ((list != null) && (list.Count > 0))
                {
                    // Try to always display the publisher already selected (if any)
                    String selectedId = "";
                    Int32 selectedIndex = 0;
                    if (cbPublishersList.SelectedItem != null)
                        selectedId = cbPublishersList.SelectedItem.Title;

                    int index = 0;
                    cbPublishersList.RemoveAllItems();
                    foreach (Conference.Publisher publisher in list)
                    {
                        cbPublishersList.AddItem(new NSString(publisher.Id));
                        if (publisher.Id == selectedId)
                            selectedIndex = index;
                        index++;
                    }
                }
                else
                {
                    txtPublisherId.StringValue = "";
                    txtPublisherJid.StringValue = "";
                    cbPublisherSharing.State = NSCellStateValue.Off;
                    cbPublisherVideo.State = NSCellStateValue.Off;

                    cbPublishersList.RemoveAllItems();
                }
                log.DebugFormat("[UpdateConferencePublishers] - OUT");
            });
        }

        private void UpdatePersonalConferenceAvailable(Boolean value)
        {
            InvokeOnMainThread(() =>
            {
                box_personal.Hidden = !value;

                cbPersonalConferenceAvailable.State = (value ? NSCellStateValue.On : NSCellStateValue.Off);
            });
        }

        #endregion METHODS TO UPDATE UI


        #region EVENTS FIRED BY RAINBOW SDK

        private void RainbowApplication_ConnectionStateChanged(object sender, Rainbow.Events.ConnectionStateEventArgs e)
        {
            // Add info about connection state
            AddStateLine($"ConnectionStateChanged:{e.State}");
            UpdateLoginButton(e.State);

            // Update layout since we are not connected to the server
            if (e.State == Rainbow.Model.ConnectionState.Connected)
            {
                // Check permissions
                UpdateConferenceAvailable(rainbowBubbles.ConferenceAllowed());
                UpdatePersonalConferenceAvailable(rainbowBubbles.PersonalConferenceAllowed());

                // Update "Conference in progress"
                UpdateConferenceInProgress();
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

        private void RainbowBubbles_ConferenceUpdated(object sender, Rainbow.Events.ConferenceEventArgs e)
        {
            conferenceInProgress = e.Conference;

            AddStateLine("Conference Updated: " + conferenceInProgress.ToString());
            log.DebugFormat("Conference Updated: " + conferenceInProgress.ToString());

            UpdateConferenceInProgress();
        }

        #endregion EVENTS FIRED BY RAINBOW SDK
    }
}
