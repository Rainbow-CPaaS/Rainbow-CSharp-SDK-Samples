using Rainbow;
using Rainbow.Delegates;
using Rainbow.Enums;
using Rainbow.Model;
using Terminal.Gui;

public partial class HybridTelephonyCallView : View
{
    public event StringDelegate? ErrorOccurred;

    readonly Rainbow.Application rbApplication;
    readonly Rainbow.HybridTelephony rbHybridTelephony;

    readonly View viewLeft;
    readonly View viewRight;

    readonly Label lblId;
    readonly TextField textFieldId;
    readonly Label lblStatus;
    readonly TextField textFieldStatus;
    readonly Label lblDevice;
    readonly TextField textFieldDevice;

    readonly Label lblLocalMedia;
    readonly TextField textFieldLocalMedia;
    readonly Label lblRemoteMedia;
    readonly TextField textFieldRemoteMedia;

    readonly Label lblIsConference;
    readonly CheckBox checkBoxIsConference;

    readonly LoggerView participants;

    readonly View viewButtons;
    readonly Button btnAnswer;  // Answer
    readonly Button btnHold;    // Hold / UnHold
    readonly Button btnRelease; // Release
    readonly Button btnToVM;    // ToVM
    readonly Button btnDtmf;    // DTMF
    readonly Label labelDTMF;   
    readonly TextField textFieldDTMF; 

    Call? currentCall;
    Call? otherCall;

    public HybridTelephonyCallView(Rainbow.Application rbApplication, int index)
    {
        this.rbApplication = rbApplication;
        rbHybridTelephony = rbApplication.GetHybridTelephony();

        Title = $"Call {index}";
        BorderStyle = LineStyle.Dotted;
        CanFocus = true;

        viewLeft = new()
        {
            X = 0,
            Y = 0,
            Width = Dim.Percent(50),
            Height = Dim.Auto(DimAutoStyle.Content),
            CanFocus = true,
        };

        lblId = new()
        {
            X = 1,
            Y = 1,
            Text = "ID:",
            TextAlignment = Alignment.End,
            Width = 7,
            Height = 1
        };

        textFieldId = new()
        {
            X = Pos.Right(lblId) + 1,
            Y = Pos.Top(lblId),
            Text = "",
            TextAlignment = Alignment.Start,
            Width = Dim.Fill(1),
            Height = 1,
            ReadOnly = true,
            ColorScheme = Tools.ColorSchemeBlackOnWhite
        };

        lblStatus = new()
        {
            X = 1,
            Y = Pos.Bottom(lblId),
            Text = "Status:",
            TextAlignment = Alignment.End,
            Width = 7,
            Height = 1
        };

        textFieldStatus = new()
        {
            X = Pos.Right(lblStatus) + 1,
            Y = Pos.Top(lblStatus),
            Text = "",
            TextAlignment = Alignment.Start,
            Width = Dim.Fill(1),
            Height = 1,
            ReadOnly = true,
            ColorScheme = Tools.ColorSchemeBlackOnWhite
        };

        lblDevice = new()
        {
            X = 1,
            Y = Pos.Bottom(lblStatus),
            Text = "Device:",
            TextAlignment = Alignment.End,
            Width = 7,
            Height = 1
        };

        textFieldDevice = new()
        {
            X = Pos.Right(lblDevice) + 1,
            Y = Pos.Top(lblDevice),
            Text = "",
            TextAlignment = Alignment.Start,
            Width = Dim.Fill(1),
            Height = 1,
            ReadOnly = true,
            ColorScheme = Tools.ColorSchemeBlackOnWhite
        };

        viewLeft.Add(lblId, textFieldId, lblStatus, textFieldStatus, lblDevice, textFieldDevice);

        viewRight = new()
        {
            X = Pos.Percent(50),
            Y = 0,
            Width = Dim.Percent(50),
            Height = Dim.Auto(DimAutoStyle.Content),
            CanFocus = true,
        };

        lblLocalMedia = new()
        {
            X = 1,
            Y = 1,
            Text = "Local Media:",
            TextAlignment = Alignment.End,
            Width = 15,
            Height = 1
        };

        textFieldLocalMedia = new()
        {
            X = Pos.Right(lblLocalMedia) + 1,
            Y = Pos.Top(lblLocalMedia),
            Text = "",
            TextAlignment = Alignment.Start,
            Width = Dim.Fill(1),
            Height = 1,
            ReadOnly = true,
            ColorScheme = Tools.ColorSchemeBlackOnWhite
        };

        lblRemoteMedia = new()
        {
            X = 1,
            Y = Pos.Bottom(lblLocalMedia),
            Text = "Remote Media:",
            TextAlignment = Alignment.End,
            Width = 15,
            Height = 1
        };

        textFieldRemoteMedia = new()
        {
            X = Pos.Right(lblRemoteMedia) + 1,
            Y = Pos.Top(lblRemoteMedia),
            Text = "",
            TextAlignment = Alignment.Start,
            Width = Dim.Fill(1),
            Height = 1,
            ReadOnly = true,
            ColorScheme = Tools.ColorSchemeBlackOnWhite
        };

        lblIsConference = new()
        {
            X = 1,
            Y = Pos.Bottom(lblRemoteMedia),
            Text = "Is Conference:",
            TextAlignment = Alignment.End,
            Width = 15,
            Height = 1
        };

        checkBoxIsConference = new()
        {
            X = Pos.Right(lblIsConference),
            Y = Pos.Top(lblIsConference),
            Text = " ",
            TextAlignment = Alignment.End,
            Width = 2,
            Height = 1,
            Enabled = false,
        };

        viewRight.Add(lblLocalMedia, textFieldLocalMedia, lblRemoteMedia, textFieldRemoteMedia, lblIsConference, checkBoxIsConference);

        participants = new(title: "Participants", btnsVisible: false)
        {
            X = Pos.Left(viewLeft) + 1,
            Y = Pos.Bottom(viewLeft) + 1,
            Width = Dim.Fill(1),
            Height = 5
        };

        viewButtons = new()
        {
            X = Pos.Center(),
            Y = Pos.Bottom(participants) + 1,
            Width = Dim.Auto(DimAutoStyle.Content),
            Height = Dim.Auto(DimAutoStyle.Content),
            CanFocus = true,
        };

        btnAnswer = new()
        {
            X = 0,
            Y = 0,
            Text = "Answer",
            TextAlignment = Alignment.Center,
            ShadowStyle = ShadowStyle.None,
            ColorScheme = Tools.ColorSchemeBlueOnGray,
            Enabled = false
        };
        btnAnswer.MouseClick += BtnAnswer_MouseClick;

        btnHold = new()
        {
            X = Pos.Right(btnAnswer) + 2,
            Y = 0,
            Text = "Hold",
            TextAlignment = Alignment.Center,
            ShadowStyle = ShadowStyle.None,
            ColorScheme = Tools.ColorSchemeBlueOnGray,
            Enabled = false
        };
        btnHold.MouseClick += BtnHold_MouseClick;

        btnRelease = new()
        {
            X = Pos.Right(btnHold) + 2,
            Y = 0,
            Text = "Release",
            TextAlignment = Alignment.Center,
            ShadowStyle = ShadowStyle.None,
            ColorScheme = Tools.ColorSchemeBlueOnGray,
            Enabled = false
        };
        btnRelease.MouseClick += BtnRelease_MouseClick;

        btnToVM = new()
        {
            X = Pos.Right(btnRelease) + 2,
            Y = 0,
            Text = "To VM",
            TextAlignment = Alignment.Center,
            ShadowStyle = ShadowStyle.None,
            ColorScheme = Tools.ColorSchemeBlueOnGray,
            Enabled = false
        };
        btnToVM.MouseClick += BtnToVM_MouseClick;

        btnDtmf = new()
        {
            X = Pos.Right(btnToVM) + 2,
            Y = 0,
            Text = "Send DTMF",
            TextAlignment = Alignment.Center,
            ShadowStyle = ShadowStyle.None,
            ColorScheme = Tools.ColorSchemeBlueOnGray,
            Enabled = false
        };
        btnDtmf.MouseClick += BtnDtmf_MouseClick;

        labelDTMF = new()
        {
            X = Pos.Right(btnDtmf) + 2,
            Y = 0,
            Text = "DTMF:",
            TextAlignment = Alignment.End,
            Width = 5,
            Height = 1,
            Enabled = false
        };

        textFieldDTMF = new()
        {
            X = Pos.Right(labelDTMF) + 1,
            Y = 0,
            Width = 5,
            Text = "",
            TextAlignment = Alignment.Start,
            ShadowStyle = ShadowStyle.None,
            ColorScheme = Tools.ColorSchemeBlackOnWhite,
            ReadOnly = false,
            Enabled = false
        };
        viewButtons.Add(btnAnswer, btnHold, btnRelease, btnToVM, btnDtmf, labelDTMF, textFieldDTMF);

        Add(viewLeft, viewRight, participants, viewButtons);

        Height = Dim.Auto(DimAutoStyle.Content);
        Width = Dim.Fill(0);

        UpdateDisplay(null, null);
    }

    private void BtnDtmf_MouseClick(object? sender, MouseEventArgs e)
    {
        if (currentCall is not null)
        {
            //if (currentCall.CallStatus == CallStatus.ACTIVE)
            {
                rbHybridTelephony.SendDtmfAsync(currentCall, textFieldDTMF.Text).ContinueWith(task =>
                {
                    var sdkResultBoolean = task.Result;
                    if (!sdkResultBoolean.Success)
                    {
                        Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                    }
                });
            }
        }
    }

    private void BtnToVM_MouseClick(object? sender, MouseEventArgs e)
    {
        if (currentCall is not null)
        {
            //if (currentCall.CallStatus == CallStatus.RINGING_INCOMING)
            {
                rbHybridTelephony.DeflectCallToMevoAsync(currentCall).ContinueWith(task =>
                {
                    var sdkResultBoolean = task.Result;
                    if (!sdkResultBoolean.Success)
                    {
                        Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                    }
                });
            }
        }
    }

    private void BtnRelease_MouseClick(object? sender, MouseEventArgs e)
    {
        if (currentCall is not null)
        {
            //if (currentCall.CallStatus == CallStatus.RINGING_INCOMING)
            {
                rbHybridTelephony.ReleaseCallAsync(currentCall).ContinueWith(task =>
                {
                    var sdkResultBoolean = task.Result;
                    if (!sdkResultBoolean.Success)
                    {
                        Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                    }
                });
            }
        }
    }

    private void BtnHold_MouseClick(object? sender, MouseEventArgs e)
    {
        if(currentCall is not null)
        {
            if(currentCall.CallStatus == CallStatus.PUT_ON_HOLD)
            {
                if (otherCall?.CallStatus == CallStatus.ACTIVE)
                {
                    rbHybridTelephony.AlternateCallAsync(otherCall, currentCall).ContinueWith(task =>
                    {
                        var sdkResultBoolean = task.Result;
                        if (!sdkResultBoolean.Success)
                        {
                            Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                        }
                    });
                }
                else
                {
                    rbHybridTelephony.RetrieveCallAsync(currentCall).ContinueWith(task =>
                    {
                        var sdkResultBoolean = task.Result;
                        if (!sdkResultBoolean.Success)
                        {
                            Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                        }
                    });
                }
            }
            else if (currentCall.CallStatus == CallStatus.ACTIVE)
            {
                if (otherCall?.CallStatus == CallStatus.PUT_ON_HOLD)
                {
                    rbHybridTelephony.AlternateCallAsync(currentCall, otherCall).ContinueWith(task =>
                    {
                        var sdkResultBoolean = task.Result;
                        if (!sdkResultBoolean.Success)
                        {
                            Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                        }
                    });
                }
                else
                {
                    rbHybridTelephony.HoldCallAsync(currentCall).ContinueWith(task =>
                    {
                        var sdkResultBoolean = task.Result;
                        if (!sdkResultBoolean.Success)
                        {
                            Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                        }
                    });
                }
            }
        }
    }

    private void BtnAnswer_MouseClick(object? sender, MouseEventArgs e)
    {
        if (currentCall is not null)
        {
            //if (currentCall.CallStatus == CallStatus.RINGING_INCOMING)
            {
                rbHybridTelephony.AnswerCallAsync(currentCall).ContinueWith(task =>
                {
                    var sdkResultBoolean = task.Result;
                    if (!sdkResultBoolean.Success)
                    {
                        Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                    }
                });
            }
        }
    }

    public void UpdateDisplay(Call? call, Call? otherCall)
    {
        currentCall = call;
        this.otherCall = otherCall;

        // Voice mail is available ?
        var VMAvailable = rbHybridTelephony.VoiceMailAvailable();

        if (call is not null)
        {
            textFieldId.Text = call.Id;
            textFieldStatus.Text = call.CallStatus.ToString();
            textFieldLocalMedia.Text = Util.MediasToString(call.LocalMedias);
            textFieldRemoteMedia.Text = Util.MediasToString(call.RemoteMedias);
            checkBoxIsConference.CheckedState = call.IsConference ? CheckState.Checked : CheckState.UnChecked;

            // List participants
            if ((call.Participants != null) && (call.Participants.Count > 0))
            {
                String output = "";
                int nb = 1;
                foreach (CallParticipant participant in call.Participants)
                {
                    if (nb > 1)
                        output += Util.CR;

                    if (participant.Peer.DisplayName == participant.Peer.PhoneNumber)
                        output += nb + ") " + participant.Peer.DisplayName;
                    else
                        output += nb + ") " + participant.Peer.DisplayName + " - " + participant.Peer.PhoneNumber;
                    nb++;
                }
                participants.ClearText();
                participants.AddText(output);
            }

            switch (call.CallStatus)
            {
                case CallStatus.UNKNOWN:
                    // The call is finished
                    btnAnswer.Enabled = false;
                    btnHold.Enabled = false;
                    btnHold.Text = "Hold";
                    btnRelease.Enabled = false;
                    btnToVM.Enabled = false;
                    btnDtmf.Enabled = false;
                    labelDTMF.Enabled = false;
                    textFieldDTMF.Enabled = false;

                    participants.ClearText();
                    break;

                case CallStatus.RINGING_INCOMING:
                    btnAnswer.Enabled = true;
                    btnHold.Enabled = false;
                    btnHold.Text = "Hold";
                    btnRelease.Enabled = true;
                    btnToVM.Enabled = VMAvailable;
                    btnDtmf.Enabled = false;
                    labelDTMF.Enabled = false;
                    textFieldDTMF.Enabled = false;
                    break;

                case CallStatus.ANSWERING:
                case CallStatus.CONNECTING:
                case CallStatus.DIALING:
                case CallStatus.RINGING_OUTGOING:
                    btnAnswer.Enabled = false;
                    btnHold.Enabled = false;
                    btnHold.Text = "Hold";
                    btnRelease.Enabled = true;
                    btnToVM.Enabled = false;
                    btnDtmf.Enabled = false;
                    labelDTMF.Enabled = false;
                    textFieldDTMF.Enabled = false;
                    break;

                case CallStatus.ACTIVE:
                    btnAnswer.Enabled = false;
                    btnHold.Enabled = !call.IsConference; 
                    btnHold.Text = "Hold";
                    btnRelease.Enabled = true;
                    btnToVM.Enabled = false;
                    btnDtmf.Enabled = true;
                    labelDTMF.Enabled = true;
                    textFieldDTMF.Enabled = true;
                    break;

                case CallStatus.RELEASING:
                    btnAnswer.Enabled = false;
                    btnHold.Enabled = false;
                    btnHold.Text = "Hold";
                    btnRelease.Enabled = false;
                    btnToVM.Enabled = false;
                    btnDtmf.Enabled = false;
                    labelDTMF.Enabled = false;
                    textFieldDTMF.Enabled = false;
                    break;

                case CallStatus.QUEUED_INCOMING:
                case CallStatus.QUEUED_OUTGOING:
                case CallStatus.HOLD:
                    btnAnswer.Enabled = false;
                    btnHold.Enabled = false;
                    btnHold.Text = "Hold";
                    btnRelease.Enabled = true;
                    btnToVM.Enabled = false;
                    btnDtmf.Enabled = false;
                    labelDTMF.Enabled = false;
                    textFieldDTMF.Enabled = false;
                    break;

                case CallStatus.PUT_ON_HOLD:
                    btnAnswer.Enabled = false;
                    btnHold.Enabled = true;
                    btnHold.Text = "Unhold";
                    btnRelease.Enabled = false;
                    btnToVM.Enabled = false;
                    btnDtmf.Enabled = false;
                    labelDTMF.Enabled = false;
                    textFieldDTMF.Enabled = false;
                    break;

                case CallStatus.ERROR:
                    break;
            }

            viewButtons.Draw();
        }

    }

}

