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

using log4net;
using log4net.Config;
using System.Web.UI.WebControls;
using System.IO;

namespace Sample_Telephony
{
    public partial class SampleTelephonyForm : Form
    {
        // Define log object
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(SampleTelephonyForm));

        //Define Rainbow Application Id, Secret Key and Host Name
        const string APP_ID = "YOUR APP ID";
        const string APP_SECRET_KEY = "YOUR SECRET KEY";
        const string HOST_NAME = "sandbox.openrainbow.com";

        const string LOGIN_USER1 = "YOUR LOGIN";
        const string PASSWORD_USER1 = "YOUR PASSWORD";

        // Define Rainbow objects
        Rainbow.Application rainbowApplication;     // To store Rainbow Application object
        Telephony rainbowTelephony;                 // To store Rainbow Telephony object

        List<VoiceMessage> voiceMessagesList;       // To store Voice Messages List of the current user
        Call pbxCall1;                              // To store PBX Call
        Call pbxCall2;                              // To store PBX Call

        Contacts rainbowContacts;                   // To store Rainbow Contacts object

        Contact rainbowMyContact;                   // To store My contact (i.e. the one connected to Rainbow Server)

        // Define delegate to update SampleTelephonyForm components
        delegate void VoidDelegate();
        delegate void StringArgReturningVoidDelegate(String value);
        delegate void BoolArgReturningVoidDelegate(Boolean value);
        delegate void IntArgReturningVoidDelegate(Int32 value);
        delegate void CallForwardStatusArgReturningVoidDelegate(CallForwardStatus value);
        delegate void NomadicStatusArgReturningVoidDelegate(NomadicStatus value);
        delegate void CallArgReturningVoidDelegate(Call value);


    #region INIT METHODS
        public SampleTelephonyForm()
        {
            InitializeComponent();

            // Set default user / pwd
            tbLogin.Text = LOGIN_USER1;
            tbPassword.Text = PASSWORD_USER1;
            cbCall1Dtmf.SelectedIndex = 0;
            cbCall2Dtmf.SelectedIndex = 0;

            log.Info("==============================================================");
            log.Info("SampleTelephony started");

            InitializeRainbowSDK();
        }

        private void InitializeRainbowSDK()
        {
            rainbowApplication = new Rainbow.Application(); ;

            // Set Application Id, Secret Key and Host Name
            rainbowApplication.SetApplicationInfo(APP_ID, APP_SECRET_KEY);
            rainbowApplication.SetHostInfo(HOST_NAME);

            // Get Rainbow main objects
            rainbowTelephony = rainbowApplication.GetTelephony();
            rainbowContacts = rainbowApplication.GetContacts();
            
            // EVENTS WE WANT TO MANAGE SOME EVENTS
            rainbowApplication.ConnectionStateChanged += RainbowApplication_ConnectionStateChanged;

            rainbowTelephony.TelephonyStatusUpdated += RainbowTelephony_TelephonyStatusUpdated;
            rainbowTelephony.CallForwardStatusUpdated += RainbowTelephony_CallForwardStatusUpdated;
            rainbowTelephony.NomadicStatusUpdated += RainbowTelephony_NomadicStatusUpdated;
            rainbowTelephony.VoiceMessagesNumberUpdated += RainbowTelephony_VoiceMessagesNumberUpdated;

            rainbowTelephony.CallUpdated += RainbowTelephony_CallUpdated;
            rainbowTelephony.CallFailed += RainbowTelephony_CallFailed;

            // Init other objects
            voiceMessagesList = null;

            pbxCall1 = null;
            pbxCall2 = null;
        }

    #endregion INIT METHODS


    #region METHOD TO UPDATE SampleTelephonyForm COMPONENTS

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

        private void UpdateTelephonyService(bool enable)
        {
            // We can update this compoent only if we are in the from thread
            if (cbTelephonyService.InvokeRequired)
            {
                BoolArgReturningVoidDelegate d = new BoolArgReturningVoidDelegate(UpdateTelephonyService);
                this.Invoke(d, new object[] { enable });
            }
            else
            {
                cbTelephonyService.Checked = enable;
                tbMakeCall.Enabled = enable;
                btMakeCall.Enabled = enable;
            }
        }

        private void UpdatePBXAgentVersion(String str)
        {
            // We can update this compoent only if we are in the from thread
            if (cbTelephonyService.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(UpdatePBXAgentVersion);
                this.Invoke(d, new object[] { str });
            }
            else
            {
                tbPBXAgent.Text = str;
            }
        }

        private void UpdateVoiceMailService(String phoneNumber)
        {
            // We can update this compoent only if we are in the from thread
            if (tbVoiceMessagePhoneNumber.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(UpdateVoiceMailService);
                this.Invoke(d, new object[] { phoneNumber });
            }
            else
            {
                tbVoiceMessagePhoneNumber.Text = phoneNumber;
                cbVoiceMail.Checked = gbVoiceMail.Enabled =!(String.IsNullOrEmpty(phoneNumber));
            }
        }

        private void UpdateVMNbMessages(Int32 nb)
        {
            // We can update this compoent only if we are in the from thread
            if (tbNbVoiceMessages.InvokeRequired)
            {
                IntArgReturningVoidDelegate d = new IntArgReturningVoidDelegate(UpdateVMNbMessages);
                this.Invoke(d, new object[] { nb });
            }
            else
            {
                tbNbVoiceMessages.Text = nb.ToString();
            }
        }

        private void UpdateVMMessages()
        {
            // We can update this compoent only if we are in the from thread
            if (tbVoiceMessagePhoneNumber.InvokeRequired)
            {
                VoidDelegate d = new VoidDelegate(UpdateVMMessages);
                this.Invoke(d, null );
            }
            else
            {
                // Check if PBX uses is an OXO system or not
                // Some Voice Message features are available only on OXO
                PbxAgentInfo pbxAgentInfo = rainbowTelephony.GetPBXAgentInformation();
                Boolean isOXO = pbxAgentInfo.IsOXO;

                lblVMInfo.Visible = (!isOXO);

                int nb = 0;
                if(voiceMessagesList != null)
                {
                    nb = voiceMessagesList.Count();
                }

                btVMDownload.Enabled = btVMDelete.Enabled = cbVoiceMessages.Enabled = (nb > 0);
            }
        }

        private void UpdateCallForwardStatus(CallForwardStatus callForwardStatus)
        {
            // We can update this compoent only if we are in the from thread
            if (cbCallFwd.InvokeRequired)
            {
                CallForwardStatusArgReturningVoidDelegate d = new CallForwardStatusArgReturningVoidDelegate(UpdateCallForwardStatus);
                this.Invoke(d, new object[] { callForwardStatus });
            }
            else
            {
                Boolean available = false;
                Boolean activated = false;
                Boolean toMV = false;
                Boolean VMAvailable = false;
                String phoneNumber = "";
                if (callForwardStatus != null)
                {
                    available = callForwardStatus.Available;
                    activated = callForwardStatus.Activated;
                    toMV = callForwardStatus.VoiceMail;
                    phoneNumber = callForwardStatus.PhoneNumber;

                    VMAvailable = rainbowTelephony.VoiceMailAvailable();
                }

                cbCallFwd.Checked = available;
                gbCallFwd.Enabled = available;

                if (available)
                {
                    // Does the VM is available ?
                    rbCallFwdToVm.Enabled = VMAvailable;

                    if (toMV)
                        rbCallFwdToVm.Checked = true;
                    else if (!String.IsNullOrEmpty(phoneNumber))
                    {
                        rbCallFwdToPhoneNumber.Checked = true;
                        tbCallFwdPhoneNumber.Text = phoneNumber;
                    }
                    else
                        rbCallFwdDisable.Checked = true;
                }
                else
                {
                    rbCallFwdDisable.Checked = true;
                    rbCallFwdToVm.Enabled = true;
                }

            }
        }

        private void UpdateNomadicStatus(NomadicStatus nomadicStatus)
        {
            // We can update this compoent only if we are in the from thread
            if (cbNomadic.InvokeRequired)
            {
                NomadicStatusArgReturningVoidDelegate d = new NomadicStatusArgReturningVoidDelegate(UpdateNomadicStatus);
                this.Invoke(d, new object[] { nomadicStatus });
            }
            else
            {
                Boolean available = false;
                Boolean activated = false;
                Boolean makeCallInitiatorIsMain = false;
                Boolean computerMode = false;
                Boolean computerModeAvailable = false;
                String phoneNumber = "";

                if (nomadicStatus != null)
                {
                    available = nomadicStatus.Available;
                    activated = nomadicStatus.Activated;
                    makeCallInitiatorIsMain = nomadicStatus.MakeCallInitiatorIsMain;
                    computerMode = nomadicStatus.ComputerMode;
                    phoneNumber = nomadicStatus.PhoneNumber;

                    // Check if computer mode is available
                    computerModeAvailable = rainbowTelephony.NomadicOnComputerAvailable();
                    rbNomadicToComputer.Enabled = computerModeAvailable;
                }

                // Does nomadic service is available ?
                cbNomadic.Checked = available;
                gbNomadic.Enabled = available;

                if (available)
                {
                    // Can we use computer mode ?
                    rbNomadicToComputer.Enabled = computerModeAvailable;

                    if (computerMode)
                        rbNomadicToComputer.Checked = true;
                    else if (makeCallInitiatorIsMain)
                    {
                        rbNomadicToOfficePhone.Checked = true;
                    }
                    else if (!String.IsNullOrEmpty(phoneNumber))
                    {
                        rbNomadicToPhoneNumber.Checked = true;
                    }

                    tbNomadicPhoneNumber.Text = phoneNumber;
                }
                else
                {
                    rbNomadicToComputer.Enabled = true;
                }
            }
        }

        private void UpdateCall(Call call)
        {
            // We can update this compoent only if we are in the from thread
            if (tbCall1Id.InvokeRequired)
            {
                CallArgReturningVoidDelegate d = new CallArgReturningVoidDelegate(UpdateCall);
                this.Invoke(d, new object[] { call });
            }
            else
            {
                Boolean VMAvailable = false;
                Boolean manageCall1 = false;


                log.DebugFormat("UpdateCall: [{0}]", call.ToString());

                // Voice mail is available ?
                VMAvailable = rainbowTelephony.VoiceMailAvailable();

                // Do we need to update Call 1 or Call 2 ?
                if ( (pbxCall2 != null)
                    && (pbxCall2.Id == call.Id))
                {
                    manageCall1 = false;
                }
                else if (pbxCall1 == null)
                {
                    manageCall1 = true;
                }
                else if (pbxCall1.Id == call.Id)
                {
                    manageCall1 = true;
                }

                // Update form elements
                if (manageCall1)
                {
                    // Store call
                    pbxCall1 = call;

                    tbCall1Id.Text = call.Id;
                    tbCall1Status.Text = call.CallStatus.ToString();
                    tbCall1LocalMedia.Text = call.LocalMedia.ToString();
                    tbCall1RemoteMedia.Text = call.RemoteMedia.ToString();
                    tbCall1DeviceType.Text = call.DeviceType;
                    cbCall1IsConference.Checked = call.IsConference;

                    // List participants
                    if ( (pbxCall1.Participants != null) && (pbxCall1.Participants.Count > 0)  )
                    {
                        String output = "";
                        int nb = 1;
                        foreach(CallParticipant participant in pbxCall1.Participants)
                        {
                            if (nb > 1)
                                output += "\r\n";
                            output += nb + ") " +participant.DisplayName + " - " + participant.PhoneNumber;
                            nb++;
                        }
                        tbCall1Participants.Text = output;
                    }

                    // Manage buttons
                    cbCall1Dtmf.Visible = (call.CallStatus == Call.Status.ACTIVE);

                    switch (call.CallStatus)
                    {
                        case Call.Status.UNKNOWN:
                            // The call is finished
                            btn1Call1.Text = "Answer";
                            btn1Call1.Enabled = false;

                            btn2Call1.Text = "To VM";
                            btn2Call1.Enabled = false;

                            btn3Call1.Text = "Release";
                            btn3Call1.Enabled = false;

                            // No more manage this call
                            pbxCall1 = null;
                            break;

                        case Call.Status.RINGING_INCOMING:
                            btn1Call1.Text = "Answer";
                            btn1Call1.Enabled = true;

                            btn2Call1.Text = "To VM";
                            btn2Call1.Enabled = VMAvailable;

                            btn3Call1.Text = "Release";
                            btn3Call1.Enabled = true;

                            break;

                        case Call.Status.ANSWERING:
                        case Call.Status.CONNECTING:
                        case Call.Status.DIALING:
                        case Call.Status.RINGING_OUTGOING:
                            btn1Call1.Text = "Answer";
                            btn1Call1.Enabled = false;

                            btn2Call1.Text = "To VM";
                            btn2Call1.Enabled = false;

                            btn3Call1.Text = "Release";
                            btn3Call1.Enabled = true;

                            break;

                        case Call.Status.ACTIVE:
                            btn1Call1.Text = "Hold";
                            btn1Call1.Enabled = true;

                            btn2Call1.Text = "DTMF";
                            btn2Call1.Enabled = true;

                            btn3Call1.Text = "Release";
                            btn3Call1.Enabled = true;

                            break;

                        case Call.Status.RELEASING:
                            btn1Call1.Text = "Hold";
                            btn1Call1.Enabled = false;

                            btn2Call1.Text = "DTMF";
                            btn2Call1.Enabled = false;

                            btn3Call1.Text = "Release";
                            btn3Call1.Enabled = false;
                            break;

                        case Call.Status.QUEUED_INCOMING:
                        case Call.Status.QUEUED_OUTGOING:
                        case Call.Status.HOLD:
                            btn1Call1.Text = "Hold";
                            btn1Call1.Enabled = false;

                            btn2Call1.Text = "DTMF";
                            btn2Call1.Enabled = false;

                            btn3Call1.Text = "Release";
                            btn3Call1.Enabled = true;
                            break;

                        case Call.Status.PUT_ON_HOLD:
                            btn1Call1.Text = "Unhold";
                            btn1Call1.Enabled = true;

                            btn2Call1.Text = "DTMF";
                            btn2Call1.Enabled = false;

                            btn3Call1.Text = "Release";
                            btn3Call1.Enabled = false;
                            break;

                        case Call.Status.ERROR:
                            break;
                    }
                }
                else // Manage Call 2
                {
                    // Store call
                    pbxCall2 = call;

                    tbCall2Id.Text = call.Id;
                    tbCall2Status.Text = call.CallStatus.ToString();
                    tbCall2LocalMedia.Text = call.LocalMedia.ToString();
                    tbCall2RemoteMedia.Text = call.RemoteMedia.ToString();
                    tbCall2DeviceType.Text = call.DeviceType;
                    cbCall2IsConference.Checked = call.IsConference;

                    // List participants
                    if ((pbxCall2.Participants != null) && (pbxCall2.Participants.Count > 0))
                    {
                        String output = "";
                        int nb = 1;
                        foreach (CallParticipant participant in pbxCall2.Participants)
                        {
                            if (nb > 1)
                                output += "\r\n";
                            output += nb + ") " + participant.DisplayName + " - " + participant.PhoneNumber;
                            nb++;
                        }
                        tbCall2Participants.Text = output;
                    }

                    // Manage buttons
                    cbCall2Dtmf.Visible = (call.CallStatus == Call.Status.ACTIVE);

                    switch (call.CallStatus)
                    {
                        case Call.Status.UNKNOWN:
                            // The call is finished
                            btn1Call2.Text = "Answer";
                            btn1Call2.Enabled = false;

                            btn2Call2.Text = "To VM";
                            btn2Call2.Enabled = false;

                            btn3Call2.Text = "Release";
                            btn3Call2.Enabled = false;

                            // No more manage this call
                            pbxCall2 = null;
                            break;

                        case Call.Status.RINGING_INCOMING:
                            btn1Call2.Text = "Answer";
                            btn1Call2.Enabled = true;

                            btn2Call2.Text = "To VM";
                            btn2Call2.Enabled = VMAvailable;

                            btn3Call2.Text = "Release";
                            btn3Call2.Enabled = true;

                            break;

                        case Call.Status.ANSWERING:
                        case Call.Status.CONNECTING:
                        case Call.Status.DIALING:
                        case Call.Status.RINGING_OUTGOING:
                            btn1Call2.Text = "Answer";
                            btn1Call2.Enabled = false;

                            btn2Call2.Text = "To VM";
                            btn2Call2.Enabled = false;

                            btn3Call2.Text = "Release";
                            btn3Call2.Enabled = true;

                            break;

                        case Call.Status.ACTIVE:
                            btn1Call2.Text = "Hold";
                            btn1Call2.Enabled = true;

                            btn2Call2.Text = "DTMF";
                            btn2Call2.Enabled = true;

                            btn3Call2.Text = "Release";
                            btn3Call2.Enabled = true;

                            break;

                        case Call.Status.RELEASING:
                            btn1Call2.Text = "Hold";
                            btn1Call2.Enabled = false;

                            btn2Call2.Text = "DTMF";
                            btn2Call2.Enabled = false;

                            btn3Call2.Text = "Release";
                            btn3Call2.Enabled = false;
                            break;

                        case Call.Status.QUEUED_INCOMING:
                        case Call.Status.QUEUED_OUTGOING:
                        case Call.Status.HOLD:
                            btn1Call2.Text = "Hold";
                            btn1Call2.Enabled = false;

                            btn2Call2.Text = "DTMF";
                            btn2Call2.Enabled = false;

                            btn3Call2.Text = "Release";
                            btn3Call2.Enabled = true;
                            break;

                        case Call.Status.PUT_ON_HOLD:
                            btn1Call2.Text = "Unhold";
                            btn1Call2.Enabled = true;

                            btn2Call2.Text = "DTMF";
                            btn2Call2.Enabled = false;

                            btn3Call2.Text = "Release";
                            btn3Call2.Enabled = false;
                            break;

                        case Call.Status.ERROR:
                            break;
                    }
                }

                // Can we make a call ?
                if( ( (pbxCall1 != null) && (pbxCall1.CallStatus == Call.Status.ACTIVE) )
                    || ( (pbxCall2 != null) && (pbxCall2.CallStatus == Call.Status.ACTIVE) ) )
                    btMakeCall.Enabled = false;
                else
                    btMakeCall.Enabled = true;

                Boolean enableAdvanceAction = false;
                if ( (pbxCall1 != null)
                    && (pbxCall2 != null) )
                {
                    enableAdvanceAction = ((pbxCall1.CallStatus == Call.Status.ACTIVE) && (pbxCall2.CallStatus == Call.Status.PUT_ON_HOLD))
                                            || ((pbxCall1.CallStatus == Call.Status.PUT_ON_HOLD) && (pbxCall2.CallStatus == Call.Status.ACTIVE));
                }
                btCallTransfer.Enabled = btCallConference.Enabled = enableAdvanceAction;

            }
        }
    #endregion METHOD TO UPDATE SampleTelephonyForm COMPONENTS


    #region EVENTS FIRED BY RAINBOW SDK
        private void RainbowApplication_ConnectionStateChanged(object sender, Rainbow.Events.ConnectionStateEventArgs e)
        {
            // Add info about connection state
            AddStateLine($"ConnectionStateChanged:{e.State}");
            UpdateLoginButton(e.State);

            // Update layout since we are not connected to the server
            if (e.State != Rainbow.Model.ConnectionState.Connected)
            {
                RainbowTelephony_TelephonyStatusUpdated(this, new TelephonyStatusEventArgs(false));
                
                // Need default constructor in SDK - to add in new release
                RainbowTelephony_CallForwardStatusUpdated(this, new CallForwardStatusEventArgs(new CallForwardStatus()));
                RainbowTelephony_NomadicStatusUpdated(this, new NomadicStatusEventArgs(new NomadicStatus()));
            }

        }

        private void RainbowTelephony_TelephonyStatusUpdated(object sender, Rainbow.Events.TelephonyStatusEventArgs e)
        {
            Boolean telephonyServiceEnabled = e.Enabled;
            String pbxVersion = "";
            String vmPhoneNumber = "";

            if (telephonyServiceEnabled)
            {
                // Get PBX Version
                PbxAgentInfo pbxAgentInfo = rainbowTelephony.GetPBXAgentInformation();
                if (pbxAgentInfo != null)
                    pbxVersion = pbxAgentInfo.Version;

                // Voice mail
                vmPhoneNumber = rainbowTelephony.GetVoiceMailPhoneNumber();
            }

            // Update Form
            UpdateTelephonyService(telephonyServiceEnabled);
            UpdatePBXAgentVersion(pbxVersion);
            UpdateVoiceMailService(vmPhoneNumber);
        }

        private void RainbowTelephony_CallForwardStatusUpdated(object sender, CallForwardStatusEventArgs e)
        {
            UpdateCallForwardStatus(e.CallForwardStatus);
        }

        private void RainbowTelephony_NomadicStatusUpdated(object sender, NomadicStatusEventArgs e)
        {
            UpdateNomadicStatus(e.NomadicStatus);
        }

        private void RainbowTelephony_VoiceMessagesNumberUpdated(object sender, VoiceMessagesNumberEventArgs e)
        {
            UpdateVMNbMessages(e.Nb);

            // Check if PBX uses is an OXO system or not
            // Some Voice Message features are available only on OXO
            PbxAgentInfo pbxAgentInfo = rainbowTelephony.GetPBXAgentInformation();
            Boolean isOXO = pbxAgentInfo.IsOXO;

            voiceMessagesList = null;

            if (isOXO)
            {
                rainbowTelephony.GetVoiceMessagesList(callback =>
                {
                    if (callback.Result.Success)
                    {
                        voiceMessagesList = callback.Data;
                    }
                    else
                    {
                        String output = String.Format("Impossible to get Voice Messages - error:[{0}]", Util.SerialiseSdkError(callback.Result));
                        AddStateLine(output);
                        log.ErrorFormat(output);
                    }
                });

            }

            UpdateVMMessages();
        }

        private void RainbowTelephony_CallFailed(object sender, Rainbow.Events.CallIdEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void RainbowTelephony_CallUpdated(object sender, Rainbow.Events.CallEventArgs e)
        {
            UpdateCall(e.Call);
        }

    #endregion EVENTS FIRED BY RAINBOW SDK

    #region EVENTS FIRED BY SampleTelephonyForm ELEMENTS
        private void btnLoginLogout_Click(object sender, EventArgs e)
        {
            if (rainbowApplication.IsConnected())
            {
                // We want to logout
                rainbowApplication.Logout(callback =>
                {
                    if (!callback.Result.Success)
                    {
                        string logLine = String.Format("Impossible to logout:\r\n{0}", Util.SerialiseSdkError(callback.Result));
                        AddStateLine(logLine);
                        log.WarnFormat(logLine);
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
                        string logLine = String.Format("Impossible to login:\r\n{0}", Util.SerialiseSdkError(callback.Result));
                        AddStateLine(logLine);
                        log.WarnFormat(logLine);
                    }
                });
            }
        }

        private void btCallFwdSet_Click(object sender, EventArgs e)
        {
            String output = null;
            if (rbCallFwdDisable.Checked)
            {
                rainbowTelephony.DeactivateCallForward(callback =>
                {
                    if (callback.Result.Success)
                    {
                        output = "Call fwd deactivated";
                    } else
                    {
                        output = String.Format("Not possible to deactivate call forward - error:[{0}]", Util.SerialiseSdkError(callback.Result));
                        log.ErrorFormat(output);
                    }
                    AddStateLine(output);
                });
            }
            else if (rbCallFwdToVm.Checked)
            {
                rainbowTelephony.ActivateCallForwardOnVoiceMail(callback =>
                {
                    if (callback.Result.Success)
                    {
                        output = "Call fwd to VM activated";
                    }
                    else
                    {
                        output = String.Format("Not possible to activate call forward to VM - error:[{0}]", Util.SerialiseSdkError(callback.Result));
                        log.ErrorFormat(output);
                    }
                    AddStateLine(output);
                });
            }
            else if (rbCallFwdToPhoneNumber.Checked)
            {
                String phoneNumber = tbCallFwdPhoneNumber.Text;
                rainbowTelephony.ActivateCallForwardOnPhoneNumner(phoneNumber, callback =>
                {
                    if (callback.Result.Success)
                    {
                        output = String.Format("Call fwd to [{0}] activated", phoneNumber);
                    }
                    else
                    {
                        output = String.Format("Not possible to activate call forward to phone number [{1}] - error:[{0}]", Util.SerialiseSdkError(callback.Result), phoneNumber);
                        log.ErrorFormat(output);
                    }
                    AddStateLine(output);
                });

            }
                
        }

        private void btNomadicSet_Click(object sender, EventArgs e)
        {
            String output = null;
            if (rbNomadicToOfficePhone.Checked)
            {
                rainbowTelephony.DeactivateNomadicStatus(callback =>
                {
                    if (callback.Result.Success)
                    {
                        output = "Nomadic set to Office Phone";
                    }
                    else
                    {
                        output = String.Format("Not possible to set Nomadic to Office Phone - error:[{0}]", Util.SerialiseSdkError(callback.Result));
                        log.ErrorFormat(output);
                    }
                    AddStateLine(output);
                });
            }
            else if (rbNomadicToComputer.Checked)
            {
                rainbowTelephony.ActivateNomadicStatusToComputer(callback =>
                {
                    if (callback.Result.Success)
                    {
                        output = "Nomadic set to Computer";
                    }
                    else
                    {
                        output = String.Format("Not possible to set Nomadic to Computer - error:[{0}]", Util.SerialiseSdkError(callback.Result));
                        log.ErrorFormat(output);
                    }
                    AddStateLine(output);
                });
            }
            else if(rbNomadicToPhoneNumber.Checked)
            {
                String phoneNumber = tbNomadicPhoneNumber.Text;
                rainbowTelephony.ActivateNomadicStatusToPhoneNumber(phoneNumber, callback =>
                {
                    if (callback.Result.Success)
                    {
                        output = String.Format("Nomadic set to [{0}]", phoneNumber);
                    }
                    else
                    {
                    output = String.Format("Not possible to set Nomadic to phone number [{1}] - error:[{0}]", Util.SerialiseSdkError(callback.Result), phoneNumber);
                        log.ErrorFormat(output);
                    }
                    AddStateLine(output);
                });
            }
        }

    #endregion EVENTS FIRED BY SampleTelephonyForm ELEMENTS


    #region UTIL METHODS

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
                if(String.IsNullOrEmpty(tbState.Text))
                    tbState.AppendText(info);
                else
                    tbState.AppendText("\r\n" + info);
            }
        }


    #endregion UTIL METHODS

        private void btn1Call1_Click(object sender, EventArgs e)
        {
            if(pbxCall1 != null)
            {
                if(pbxCall1.CallStatus == Call.Status.PUT_ON_HOLD)
                {
                    // Unhold call
                    rainbowTelephony.RetrieveCall(pbxCall1.Id, callback =>
                    {
                        if (!callback.Result.Success)
                        {
                            String output = String.Format("Impossible to Unhold call [{0}] - Error:[{1}]", pbxCall1.Id, Util.SerialiseSdkError(callback.Result));
                            AddStateLine(output);
                            log.ErrorFormat(output);
                        }
                    });
                }
                else if (pbxCall1.CallStatus == Call.Status.ACTIVE)
                {
                    // Hold call
                    rainbowTelephony.HoldCall(pbxCall1.Id, callback =>
                    {
                        if(!callback.Result.Success)
                        {
                            String output = String.Format("Impossible to hold call [{0}] - Error:[{1}]", pbxCall1.Id, Util.SerialiseSdkError(callback.Result));
                            AddStateLine(output);
                            log.ErrorFormat(output);
                        }
                    });
                }
                else if (pbxCall1.CallStatus == Call.Status.RINGING_INCOMING)
                {
                    // Answer call
                    rainbowTelephony.AnswerCall(pbxCall1.Id, callback =>
                    {
                        if (!callback.Result.Success)
                        {
                            String output = String.Format("Impossible to answer call [{0}] - Error:[{1}]", pbxCall1.Id, Util.SerialiseSdkError(callback.Result));
                            AddStateLine(output);
                            log.ErrorFormat(output);
                        }
                    });
                }
            }
        }

        private void btn2Call1_Click(object sender, EventArgs e)
        {
            if (pbxCall1 != null)
            {
                if (pbxCall1.CallStatus == Call.Status.ACTIVE)
                {
                    // Send DTMF
                    rainbowTelephony.SendDtmf(pbxCall1.Id, cbCall1Dtmf.SelectedIndex.ToString(), callback =>
                    {
                        if (!callback.Result.Success)
                        {
                            String output = String.Format("Impossible to sedn DTMF [{0}] - Error:[{1}]", pbxCall1.Id, Util.SerialiseSdkError(callback.Result));
                            AddStateLine(output);
                            log.ErrorFormat(output);
                        }
                    });

                }
                else if (pbxCall1.CallStatus == Call.Status.RINGING_INCOMING)
                {
                    // Deflect call to voice mail
                    rainbowTelephony.DeflectCallToMevo(pbxCall1.Id, callback =>
                    {
                        if (!callback.Result.Success)
                        {
                            String output = String.Format("Impossible to deflect call to voice mail [{0}] - Error:[{1}]", pbxCall1.Id, Util.SerialiseSdkError(callback.Result));
                            AddStateLine(output);
                            log.ErrorFormat(output);
                        }
                    });
                }
            }
        }

        private void btn3Call1_Click(object sender, EventArgs e)
        {
            if (pbxCall1 != null)
            {
                // Release call
                rainbowTelephony.ReleaseCall(pbxCall1.Id, callback =>
                {
                    if (!callback.Result.Success)
                    {
                        String output = String.Format("Impossible to release call [{0}] - Error:[{1}]", pbxCall1.Id, Util.SerialiseSdkError(callback.Result));
                        AddStateLine(output);
                        log.ErrorFormat(output);
                    }
                });
            }
        }

        private void btn1Call2_Click(object sender, EventArgs e)
        {
            if (pbxCall2 != null)
            {
                if (pbxCall2.CallStatus == Call.Status.PUT_ON_HOLD)
                {
                    // Unhold call
                    rainbowTelephony.RetrieveCall(pbxCall2.Id, callback =>
                    {
                        if (!callback.Result.Success)
                        {
                            String output = String.Format("Impossible to Unhold call [{0}] - Error:[{1}]", pbxCall2.Id, Util.SerialiseSdkError(callback.Result));
                            AddStateLine(output);
                            log.ErrorFormat(output);
                        }
                    });
                }
                else if (pbxCall2.CallStatus == Call.Status.ACTIVE)
                {
                    // Hold call
                    rainbowTelephony.HoldCall(pbxCall2.Id, callback =>
                    {
                        if (!callback.Result.Success)
                        {
                            String output = String.Format("Impossible to hold call [{0}] - Error:[{1}]", pbxCall2.Id, Util.SerialiseSdkError(callback.Result));
                            AddStateLine(output);
                            log.ErrorFormat(output);
                        }
                    });
                }
                else if (pbxCall2.CallStatus == Call.Status.RINGING_INCOMING)
                {
                    // Answer call
                    rainbowTelephony.AnswerCall(pbxCall2.Id, callback =>
                    {
                        if (!callback.Result.Success)
                        {
                            String output = String.Format("Impossible to answer call [{0}] - Error:[{1}]", pbxCall2.Id, Util.SerialiseSdkError(callback.Result));
                            AddStateLine(output);
                            log.ErrorFormat(output);
                        }
                    });
                }
            }
        }

        private void btn2Call2_Click(object sender, EventArgs e)
        {
            if (pbxCall2 != null)
            {
                if (pbxCall2.CallStatus == Call.Status.ACTIVE)
                {
                    // Send DTMF
                    rainbowTelephony.SendDtmf(pbxCall2.Id, cbCall2Dtmf.SelectedIndex.ToString(), callback =>
                    {
                        if (!callback.Result.Success)
                        {
                            String output = String.Format("Impossible to sedn DTMF [{0}] - Error:[{1}]", pbxCall2.Id, Util.SerialiseSdkError(callback.Result));
                            AddStateLine(output);
                            log.ErrorFormat(output);
                        }
                    });

                }
                else if (pbxCall2.CallStatus == Call.Status.RINGING_INCOMING)
                {
                    // Deflect call to voice mail
                    rainbowTelephony.DeflectCallToMevo(pbxCall2.Id, callback =>
                    {
                        if (!callback.Result.Success)
                        {
                            String output = String.Format("Impossible to deflect call to voice mail [{0}] - Error:[{1}]", pbxCall2.Id, Util.SerialiseSdkError(callback.Result));
                            AddStateLine(output);
                            log.ErrorFormat(output);
                        }
                    });
                }
            }
        }

        private void btn3Call2_Click(object sender, EventArgs e)
        {
            if (pbxCall2 != null)
            {
                // Release call
                rainbowTelephony.ReleaseCall(pbxCall2.Id, callback =>
                {
                    if (!callback.Result.Success)
                    {
                        String output = String.Format("Impossible to release call [{0}] - Error:[{1}]", pbxCall2.Id, Util.SerialiseSdkError(callback.Result));
                        AddStateLine(output);
                        log.ErrorFormat(output);
                    }
                });
            }
        }

        private void btCallTransfer_Click(object sender, EventArgs e)
        {
            if((pbxCall1 != null)
                    && (pbxCall2 != null))
            {
                // Need to get hold call id and active call id
                String holdCallId = null;
                String activeCallId = null;
                Boolean canDoAction = false;

                if ((pbxCall1.CallStatus == Call.Status.ACTIVE) && (pbxCall2.CallStatus == Call.Status.PUT_ON_HOLD))
                {
                    holdCallId = pbxCall2.Id;
                    activeCallId = pbxCall1.Id;
                    canDoAction = true;
                }
                else if ((pbxCall1.CallStatus == Call.Status.PUT_ON_HOLD) && (pbxCall2.CallStatus == Call.Status.ACTIVE))
                {
                    holdCallId = pbxCall1.Id;
                    activeCallId = pbxCall2.Id;
                    canDoAction = true;
                }

                if(canDoAction)
                {
                    // Transfer call
                    rainbowTelephony.TransferCall(activeCallId, holdCallId, callback =>
                    {
                        if (!callback.Result.Success)
                        {
                            String output = String.Format("Impossible to tranfer the call - Error:[{0}]", Util.SerialiseSdkError(callback.Result));
                            AddStateLine(output);
                            log.ErrorFormat(output);
                        }
                    });
                }
            }
        }

        private void btCallConference_Click(object sender, EventArgs e)
        {
            if ((pbxCall1 != null)
                    && (pbxCall2 != null))
            {
                // Need to get hold call id and active call id
                String holdCallId = null;
                String activeCallId = null;
                Boolean canDoAction = false;

                if ((pbxCall1.CallStatus == Call.Status.ACTIVE) && (pbxCall2.CallStatus == Call.Status.PUT_ON_HOLD))
                {
                    holdCallId = pbxCall2.Id;
                    activeCallId = pbxCall1.Id;
                    canDoAction = true;
                }
                else if ((pbxCall1.CallStatus == Call.Status.PUT_ON_HOLD) && (pbxCall2.CallStatus == Call.Status.ACTIVE))
                {
                    holdCallId = pbxCall1.Id;
                    activeCallId = pbxCall2.Id;
                    canDoAction = true;
                }

                if (canDoAction)
                {
                    // Conference call
                    rainbowTelephony.ConferenceCall(activeCallId, holdCallId, callback =>
                    {
                        if (!callback.Result.Success)
                        {
                            String output = String.Format("Impossible to create the conference- Error:[{0}]", Util.SerialiseSdkError(callback.Result));
                            AddStateLine(output);
                            log.ErrorFormat(output);
                        }
                    });
                }
            }
        }

        private void btMakeCall_Click(object sender, EventArgs e)
        {
            if(!String.IsNullOrEmpty(tbMakeCall.Text))
            {
                // Make call
                rainbowTelephony.MakeCall(tbMakeCall.Text, callback =>
                {
                    if (!callback.Result.Success)
                    {
                        String output = String.Format("Impossible to make a call - Error:[{0}]", Util.SerialiseSdkError(callback.Result));
                        AddStateLine(output);
                        log.ErrorFormat(output);
                    }
                });
            }
        }
    }
}
