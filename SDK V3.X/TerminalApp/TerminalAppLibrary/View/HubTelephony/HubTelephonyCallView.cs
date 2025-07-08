using Rainbow;
using Rainbow.Delegates;
using Rainbow.Enums;
using Rainbow.Model;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

public partial class HubTelephonyCallView : View
{
    public event StringDelegate? ErrorOccurred;

    readonly Rainbow.Application rbApplication;
    readonly Rainbow.HubTelephony rbHubTelephony;

    readonly View viewLeft;
    readonly View viewRight;

    readonly Label lblId;
    readonly TextField textFieldId;
    readonly Label lblStatus;
    readonly TextField textFieldStatus;
    readonly Label lblDevice;
    readonly TextField textFieldDevice;

    readonly Label lblType;
    readonly TextField textFieldType;
    readonly Label lblDirection;
    readonly TextField textFieldDirection;

    readonly Label lblParticipant;
    readonly TextField textFieldParticipant;

    readonly View viewButtons;
    readonly Button btnAnswer;  // Answer
    readonly Button btnHold;    // Hold / UnHold
    readonly Button btnRelease; // Release
    readonly Button btnToVM;    // ToVM
    readonly Button btnDtmf;    // DTMF
    readonly Label labelDTMF;   
    readonly TextField textFieldDTMF; 

    HubCall? currentCall;
    HubCall? otherCall;

    public HubTelephonyCallView(Rainbow.Application rbApplication, int index)
    {
        int viewLeftPercent = 65;
        int maxLeftLabelLength = 14;
        int maxRightLabelLength = 15;

        this.rbApplication = rbApplication;
        rbHubTelephony = rbApplication.GetHubTelephony();

        Title = $"Call {index}";
        BorderStyle = LineStyle.Dotted;
        CanFocus = true;

        viewLeft = new()
        {
            X = 0,
            Y = 1,
            Width = Dim.Percent(viewLeftPercent),
            Height = Dim.Auto(DimAutoStyle.Content),
            CanFocus = true,
        };

        lblId = new()
        {
            X = 0,
            Y = 0,
            Text = Labels.CALL_ID + ":",
            TextAlignment = Alignment.End,
            Width = maxLeftLabelLength,
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
            ReadOnly = true
        };

        lblStatus = new()
        {
            X = 0,
            Y = Pos.Bottom(lblId),
            Text = Labels.CALL_STATUS + ":",
            TextAlignment = Alignment.End,
            Width = maxLeftLabelLength,
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
            ReadOnly = true
        };

        lblDevice = new()
        {
            X = 0,
            Y = Pos.Bottom(lblStatus),
            Text = Labels.DEVICE + ":",
            TextAlignment = Alignment.End,
            Width = maxLeftLabelLength,
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
            ReadOnly = true
        };

        viewLeft.Add(lblId, textFieldId, lblStatus, textFieldStatus, lblDevice, textFieldDevice);

        viewRight = new()
        {
            X = Pos.Right(viewLeft),
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Auto(DimAutoStyle.Content),
            CanFocus = true,
        };

        lblType = new()
        {
            X = 0,
            Y = 0,
            Text = Labels.TYPE + ":",
            TextAlignment = Alignment.End,
            Width = maxRightLabelLength,
            Height = 1
        };

        textFieldType = new()
        {
            X = Pos.Right(lblType) + 1,
            Y = Pos.Top(lblType),
            Text = "",
            TextAlignment = Alignment.Start,
            Width = Dim.Fill(1),
            Height = 1,
            ReadOnly = true
        };

        lblDirection = new()
        {
            X = 0,
            Y = Pos.Bottom(lblType),
            Text = Labels.DIRECTION + ":",
            TextAlignment = Alignment.End,
            Width = maxRightLabelLength,
            Height = 1
        };

        textFieldDirection = new()
        {
            X = Pos.Right(lblDirection) + 1,
            Y = Pos.Top(lblDirection),
            Text = "",
            TextAlignment = Alignment.Start,
            Width = Dim.Fill(1),
            Height = 1,
            ReadOnly = true
        };

        viewRight.Add(lblType, textFieldType, lblDirection, textFieldDirection);

        lblParticipant = new()
        {
            Text = Labels.PARTICIPANT + ":",
            TextAlignment = Alignment.End,
            X = 0,
            Y = Pos.Bottom(viewLeft),
            Width = maxLeftLabelLength,
            Height = 1            
        };

        textFieldParticipant = new()
        {
            X = Pos.Right(lblParticipant) + 1,
            Y = lblParticipant.Y,
            Width = Dim.Fill(1),
            Height = 1,
            ReadOnly = true
        };

        viewButtons = new()
        {
            X = Pos.Center(),
            Y = Pos.Bottom(textFieldParticipant) + 1,
            Width = Dim.Auto(DimAutoStyle.Content),
            Height = Dim.Auto(DimAutoStyle.Content),
            CanFocus = true,
        };

        btnAnswer = new()
        {
            X = 0,
            Y = 0,
            Text = Labels.ANSWER,
            TextAlignment = Alignment.Center,
            ShadowStyle = ShadowStyle.None,
            SchemeName = "BrightBlue",
            Enabled = false
        };
        btnAnswer.MouseClick += BtnAnswer_MouseClick;

        btnHold = new()
        {
            X = Pos.Right(btnAnswer) + 2,
            Y = 0,
            Text = Labels.HOLD,
            TextAlignment = Alignment.Center,
            ShadowStyle = ShadowStyle.None,
            SchemeName = "BrightBlue",
            Enabled = false
        };
        btnHold.MouseClick += BtnHold_MouseClick;

        btnRelease = new()
        {
            X = Pos.Right(btnHold) + 2,
            Y = 0,
            Text = Labels.RELEASE,
            TextAlignment = Alignment.Center,
            ShadowStyle = ShadowStyle.None,
            SchemeName = "BrightBlue",
            Enabled = false
        };
        btnRelease.MouseClick += BtnRelease_MouseClick;

        btnToVM = new()
        {
            X = Pos.Right(btnRelease) + 2,
            Y = 0,
            Text = Labels.TO_VM,
            TextAlignment = Alignment.Center,
            ShadowStyle = ShadowStyle.None,
            SchemeName = "BrightBlue",
            Enabled = false
        };
        btnToVM.MouseClick += BtnToVM_MouseClick;

        btnDtmf = new()
        {
            X = Pos.Right(btnToVM) + 2,
            Y = 0,
            Text = Labels.SEND_DTMF,
            TextAlignment = Alignment.Center,
            ShadowStyle = ShadowStyle.None,
            SchemeName = "BrightBlue",
            Enabled = false
        };
        btnDtmf.MouseClick += BtnDtmf_MouseClick;

        labelDTMF = new()
        {
            X = Pos.Right(btnDtmf) + 2,
            Y = 0,
            Text = Labels.DTMF,
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
            ReadOnly = true,
        };
        viewButtons.Add(btnAnswer, btnHold, btnRelease, btnToVM, btnDtmf, labelDTMF, textFieldDTMF);

        Add(viewLeft, viewRight, lblParticipant, textFieldParticipant, viewButtons);

        Height = Dim.Auto(DimAutoStyle.Content);
        Width = Dim.Fill(0);

        UpdateDisplay(null, null);
    }

    public void UpdateDisplay(HubCall? call, HubCall? otherCall)
    {
        currentCall = call;
        this.otherCall = otherCall;

        // Voice mail is available ?
        var VMAvailable = rbHubTelephony.VoiceMailAvailable();

        if (call is not null)
        {
            textFieldId.Text = (call.CallId is null) ? "" : call.CallId;
            textFieldStatus.Text = call.CallStatus.ToString();
            textFieldType.Text = (call.Type is null) ? "" : call.Type;
            textFieldDirection.Text = (call.Direction is null) ? "" : call.Direction;

            var devicesInCall = call.Legs?.Values.Where(l => !l.Op.Equals("ended")).Select(l => l.DeviceId).ToList();
            List<HubDevice>? devicesKnown = null;
            if (devicesInCall?.Count > 0)
                devicesKnown = rbHubTelephony.GetDevices()?.Values.Where(d => devicesInCall.Contains(d.DeviceId)).ToList();
            List<String> deviceName = new();
            if (devicesKnown?.Count > 0)
            {
                foreach (var device in devicesKnown)
                {
                    if (device.IsSipDevice())
                        deviceName.Add(Labels.OFFICE_PHONE);
                    else
                        deviceName.Add(Labels.COMPUTER);
                }
                textFieldDevice.Text = String.Join(", ", deviceName);
            }
            else
                textFieldDevice.Text = "";


            if ((call.EndPoints != null) && (call.EndPoints.Count > 0))
            {
                HubEndPoint endPoint = call.EndPoints.Values.First();
                if (endPoint is not null)
                {
                    String output = Util.LogOnOneLine(((HubPartyInfo)endPoint).ToString());
                    textFieldParticipant.Text = output;
                }
                else
                    textFieldParticipant.Text = "";
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
                    textFieldDTMF.ReadOnly = true;

                    textFieldParticipant.Text = "";
                    break;

                case CallStatus.RINGING_INCOMING:
                    btnAnswer.Enabled = true;
                    btnHold.Enabled = false;
                    btnHold.Text = "Hold";
                    btnRelease.Enabled = true;
                    btnToVM.Enabled = VMAvailable;
                    btnDtmf.Enabled = false;
                    labelDTMF.Enabled = false;
                    textFieldDTMF.ReadOnly = true;
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
                    textFieldDTMF.ReadOnly = true;
                    break;

                case CallStatus.ACTIVE:
                    btnAnswer.Enabled = false;
                    btnHold.Enabled = true; //TODO - check about conference !call.IsConference; 
                    btnHold.Text = "Hold";
                    btnRelease.Enabled = true;
                    btnToVM.Enabled = false;
                    btnDtmf.Enabled = true;
                    labelDTMF.Enabled = true;
                    textFieldDTMF.ReadOnly = false;
                    break;

                case CallStatus.RELEASING:
                    btnAnswer.Enabled = false;
                    btnHold.Enabled = false;
                    btnHold.Text = "Hold";
                    btnRelease.Enabled = false;
                    btnToVM.Enabled = false;
                    btnDtmf.Enabled = false;
                    labelDTMF.Enabled = false;
                    textFieldDTMF.ReadOnly = true;
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
                    textFieldDTMF.ReadOnly = true;
                    break;

                case CallStatus.PUT_ON_HOLD:
                    btnAnswer.Enabled = false;
                    btnHold.Enabled = true;
                    btnHold.Text = "Unhold";
                    //btnRelease.Enabled = false;
                    btnRelease.Enabled = true;
                    btnToVM.Enabled = false;
                    btnDtmf.Enabled = false;
                    labelDTMF.Enabled = false;
                    textFieldDTMF.ReadOnly = true;
                    break;

                case CallStatus.ERROR:
                    break;
            }

            viewButtons.Draw();
        }
    }

    private void BtnDtmf_MouseClick(object? sender, MouseEventArgs e)
    {
        e.Handled = true;
        if (currentCall is not null)
        {
            //if (currentCall.CallStatus == CallStatus.ACTIVE)
            {
                Task.Run(async () =>
                {
                    var sdkResultBoolean = await rbHubTelephony.SendDtmfAsync(currentCall, textFieldDTMF.Text);
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
        e.Handled = true;
        if (currentCall is not null)
        {
            //if (currentCall.CallStatus == CallStatus.RINGING_INCOMING)
            {
                Task.Run(async () =>
                {
                    var sdkResultBoolean = await rbHubTelephony.DeflectCallToMevoAsync(currentCall);
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
        e.Handled = true;
        if (currentCall is not null)
        {
            //if (currentCall.CallStatus == CallStatus.RINGING_INCOMING)
            {
                Task.Run(async () =>
                {
                    var sdkResultBoolean = await rbHubTelephony.ReleaseCallAsync(currentCall);
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
        e.Handled = true;
        if (currentCall is not null)
        {
            if(currentCall.CallStatus == CallStatus.PUT_ON_HOLD)
            {
                Task.Run(async () =>
                {
                    var sdkResultBoolean = await rbHubTelephony.RetrieveCallAsync(currentCall);
                    if (!sdkResultBoolean.Success)
                    {
                        Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                    }
                });

                /*
                if (otherCall?.CallStatus == CallStatus.ACTIVE)
                {
                    Task.Run(async () =>
                    {
                        //var sdkResultBoolean = await rbHubTelephony.AlternateCallAsync(otherCall, currentCall);
                        //if (!sdkResultBoolean.Success)
                        //{
                        //    Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                        //}
                    });
                }
                else
                {
                    Task.Run(async () =>
                    {
                        //var sdkResultBoolean = await rbHubTelephony.RetrieveCallAsync(currentCall);
                        //if (!sdkResultBoolean.Success)
                        //{
                        //    Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                        //}
                    });
                }*/
            }
            else if (currentCall.CallStatus == CallStatus.ACTIVE)
            {
                Task.Run(async () =>
                {
                    var sdkResultBoolean = await rbHubTelephony.HoldCallAsync(currentCall);
                    if (!sdkResultBoolean.Success)
                    {
                        Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                    }
                });

                /*
                if (otherCall?.CallStatus == CallStatus.PUT_ON_HOLD)
                {
                    Task.Run(async () =>
                    {
                        //var sdkResultBoolean = await rbHubTelephony.AlternateCallAsync(currentCall, otherCall);
                        //if (!sdkResultBoolean.Success)
                        //{
                        //    Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                        //}
                    });
                }
                else
                {
                    Task.Run(async () =>
                    {
                        //var sdkResultBoolean = await rbHubTelephony.HoldCallAsync(currentCall);
                        //if (!sdkResultBoolean.Success)
                        //{
                        //    Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                        //}
                    });
                }
                */
            }
        }
    }

    private void BtnAnswer_MouseClick(object? sender, MouseEventArgs e)
    {
        e.Handled = true;
        if (currentCall is not null)
        {
            //if (currentCall.CallStatus == CallStatus.RINGING_INCOMING)
            {
                Task.Run(async () =>
                {
                    var sdkResultBoolean = await rbHubTelephony.AnswerCallAsync(currentCall);
                    if (!sdkResultBoolean.Success)
                    {
                        Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                    }
                });
            }
        }
    }

    

}

