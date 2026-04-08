using Rainbow.Delegates;
using Rainbow.Enums;
using Rainbow.Model;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

public partial class HybridTelephonyPanelCallView : View
{
    public event StringDelegate? ErrorOccurred;

    readonly Rainbow.Application rbApplication;
    readonly Rainbow.HybridTelephony rbHybridTelephony;

    readonly HybridTelephonyCallView viewCall1;
    readonly HybridTelephonyCallView viewCall2;

    readonly View viewTransferOrConference;
    readonly Button btnTransfer;
    readonly Button btnConference;

    readonly Label lblInactive;

    Boolean? serviceAvailable = null;
    Boolean serviceEnabled = false;

    Call? pbxCall1;
    Call? pbxCall2;

    public HybridTelephonyPanelCallView(Rainbow.Application rbApplication)
    {
        this.rbApplication = rbApplication;
        rbHybridTelephony = rbApplication.GetHybridTelephony();

        rbHybridTelephony.HybridTelephonyStatusUpdated += RbHybridTelephony_HybridTelephonyStatusUpdated;
        rbHybridTelephony.HybridPBXAgentInfoUpdated += RbHybridTelephony_HybridPBXAgentInfoUpdated;

        rbHybridTelephony.HybridCallUpdated += RbHybridTelephony_HybridCallUpdated;

        Title = Labels.CALL_IN_PROGRESS;
        BorderStyle = LineStyle.Dotted;
        CanFocus = true;

        Border.GetOrCreateView().Add(Tools.VerticalExpanderButton());

        viewCall1 = new(rbApplication, 1)
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(1),
            CanFocus = true,
        };

        viewCall2 = new(rbApplication, 2)
        {
            X = 1,
            Y = Pos.Bottom(viewCall1) + 1,
            Width = Dim.Fill(1),
            CanFocus = true,
        };

        viewTransferOrConference = new()
        {
            X = Pos.Center(),
            Y = Pos.Bottom(viewCall2),
            Width = Dim.Auto(DimAutoStyle.Content),
            Height = Dim.Auto(DimAutoStyle.Content),
            CanFocus = true,
        };

        btnTransfer = new()
        {
            X = 0,
            Y = 0,
            Text = Labels.TRANSFER,
            TextAlignment = Alignment.Center,
            ShadowStyle = ShadowStyles.None,
            SchemeName = "BrightBlue",
            Enabled = false
        };
        btnTransfer.MouseEvent += BtnTransfer_MouseEvent;

        btnConference = new()
        {
            X = Pos.Right(btnTransfer) + 2,
            Y = 0,
            Text = Labels.CONFERENCE,
            TextAlignment = Alignment.Center,
            ShadowStyle = ShadowStyles.None,
            SchemeName = "BrightBlue",
            Enabled = false
        };
        btnConference.MouseEvent += BtnConference_MouseEvent;

        viewTransferOrConference.Add(btnTransfer, btnConference);

        lblInactive = new Label
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            TextAlignment = Alignment.Start,
            Width = Dim.Auto(DimAutoStyle.Text),
            Height = 1
        };

        Width = Dim.Auto(DimAutoStyle.Content);
        Height = Dim.Auto(DimAutoStyle.Content);

        Add(viewCall1, viewCall2, viewTransferOrConference, lblInactive);

        UpdateDisplay();
    }

    private void BtnConference_MouseEvent(object? sender, Mouse e)
    {
        if ((pbxCall1 != null)
                    && (pbxCall2 != null))
        {
            // Need to get hold call and active call
            Call? holdCall = null;
            Call? activeCall = null;
            Boolean canDoAction = false;

            if ((pbxCall1.CallStatus == CallStatus.ACTIVE) && (pbxCall2.CallStatus == CallStatus.PUT_ON_HOLD))
            {
                holdCall = pbxCall2;
                activeCall = pbxCall1;
                canDoAction = true;
            }
            else if ((pbxCall1.CallStatus == CallStatus.PUT_ON_HOLD) && (pbxCall2.CallStatus == CallStatus.ACTIVE))
            {
                holdCall = pbxCall1;
                activeCall = pbxCall2;
                canDoAction = true;
            }

            if (canDoAction)
            {
                // Conference call
                Task.Run(async () =>
                {
                    var sdkResultBoolean = await rbHybridTelephony.ConferenceCallAsync(activeCall, holdCall);
                    if (!sdkResultBoolean.Success)
                    {
                        Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                    }
                });
            }
        }
    }

    private void BtnTransfer_MouseEvent(object? sender, Mouse e)
    {
        if ((pbxCall1 != null)
                    && (pbxCall2 != null))
        {
            // Need to get hold call and active call
            Call? holdCall = null;
            Call? activeCall = null;
            Boolean canDoAction = false;

            if ((pbxCall1.CallStatus == CallStatus.ACTIVE) && (pbxCall2.CallStatus == CallStatus.PUT_ON_HOLD))
            {
                holdCall = pbxCall2;
                activeCall = pbxCall1;
                canDoAction = true;
            }
            else if ((pbxCall1.CallStatus == CallStatus.PUT_ON_HOLD) && (pbxCall2.CallStatus == CallStatus.ACTIVE))
            {
                holdCall = pbxCall1;
                activeCall = pbxCall2;
                canDoAction = true;
            }

            if (canDoAction)
            {
                // Transfer call
                Task.Run(async () =>
                {
                    var sdkResultBoolean = await rbHybridTelephony.TransferCallAsync(activeCall, holdCall);
                    if (!sdkResultBoolean.Success)
                    {
                        Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                    }
                });
            }
        }
    }

    private void UpdateDisplay()
    {
        Boolean available = false;
        if (serviceAvailable is not null)
        {
            available = serviceEnabled && serviceAvailable.Value;

            if (!serviceAvailable.Value)
            {
                lblInactive.Text = Labels.SERVICE_NOT_AVAILABLE;
                lblInactive.SchemeName = "Red";
            }
            else if (!available)
            {
                lblInactive.Text = Labels.SERVICE_DISABLED;
                lblInactive.SchemeName = "Green";
            }
        }
        else
        {
            lblInactive.Text = Labels.FEATURE_CHECKING;
            lblInactive.SchemeName = "Green";
        }

        lblInactive.Height = available ? 0 : 1;
        viewCall1.Height = available ? Dim.Auto(DimAutoStyle.Content) : 0;
        viewCall2.Height = available ? Dim.Auto(DimAutoStyle.Content) : 0;
        viewTransferOrConference.Height = available ? 1 : 0;
    }

    private void UpdateDisplay(Call call)
    {
        if ((pbxCall2 != null)
                    && (pbxCall2.Id == call.Id))
        {
            pbxCall2 = call;
        }
        else if ((pbxCall1 is null) || (pbxCall1.Id == call.Id))
        {
            pbxCall1 = call;
        }
        else
        {
            pbxCall2 = call;
        }

        viewCall1.UpdateDisplay(pbxCall1, pbxCall2);
        viewCall2.UpdateDisplay(pbxCall2, pbxCall1);


        Boolean enableAdvanceAction = false;
        if ((pbxCall1 != null)
            && (pbxCall2 != null))
        {
            enableAdvanceAction = ((pbxCall1.CallStatus == CallStatus.ACTIVE) && (pbxCall2.CallStatus == CallStatus.PUT_ON_HOLD))
                                    || ((pbxCall1.CallStatus == CallStatus.PUT_ON_HOLD) && (pbxCall2.CallStatus == CallStatus.ACTIVE));
        }
        btnTransfer.Enabled = btnConference.Enabled = enableAdvanceAction;

        // Clear Pbx call if necessary
        if (pbxCall1?.CallStatus == CallStatus.UNKNOWN)
            pbxCall1 = null;

        if (pbxCall2?.CallStatus == CallStatus.UNKNOWN)
            pbxCall2 = null;
    }

    private void RbHybridTelephony_HybridTelephonyStatusUpdated(Boolean? available)
    {
        Tools.Application.Invoke(() =>
        {
            serviceAvailable = available;
            UpdateDisplay();
        });
    }

    private void RbHybridTelephony_HybridPBXAgentInfoUpdated(Rainbow.Model.PbxAgentInfo pbxAgentInfo)
    {
        Tools.Application.Invoke(() =>
        {
            serviceEnabled = pbxAgentInfo.XmppAgentStatus == "started";
            if (serviceEnabled)
                serviceAvailable = true;
            UpdateDisplay();
        });
    }

    private void RbHybridTelephony_HybridCallUpdated(Call call)
    {
        Tools.Application.Invoke(() =>
        {
            UpdateDisplay(call);
        });
    }
}

