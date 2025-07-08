using Rainbow.Delegates;
using Rainbow.Enums;
using Rainbow.Model;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

public partial class HubTelephonyPanelCallView : View
{
    public event StringDelegate? ErrorOccurred;

    readonly Rainbow.Application rbApplication;
    readonly Rainbow.HubTelephony rbHubTelephony;

    readonly HubTelephonyCallView viewCall1;
    readonly HubTelephonyCallView viewCall2;

    readonly View viewTransferOrConference;
    readonly Button btnTransfer;

    readonly Label lblInactive;

    Boolean? serviceAvailable = null;
    Boolean serviceEnabled = false;

    HubCall? pbxCall1;
    HubCall? pbxCall2;

    public HubTelephonyPanelCallView(Rainbow.Application rbApplication)
    {
        this.rbApplication = rbApplication;
        rbHubTelephony = rbApplication.GetHubTelephony();

        rbHubTelephony.TelephonyStatusUpdated += RbHubTelephony_TelephonyStatusUpdated;
        rbHubTelephony.PBXAgentInfoUpdated += RbHubTelephony_PBXAgentInfoUpdated;

        rbHubTelephony.CallUpdated += RbHybridTelephony_HybridCallUpdated;

        Title = Labels.CALL_IN_PROGRESS;
        BorderStyle = LineStyle.Dotted;
        CanFocus = true;

        Border.Add(Tools.VerticalExpanderButton());

        viewCall1 = new(rbApplication, 1)
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(1),
            CanFocus = true,
        };
        viewCall1.ErrorOccurred += ViewCall_ErrorOccurred;

        viewCall2 = new(rbApplication, 2)
        {
            X = 1,
            Y = Pos.Bottom(viewCall1) + 1,
            Width = Dim.Fill(1),
            CanFocus = true,
        };
        viewCall2.ErrorOccurred += ViewCall_ErrorOccurred;

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
            ShadowStyle = ShadowStyle.None,
            SchemeName = "BrightBlue",
            Enabled = false
        };
        btnTransfer.MouseClick += BtnTransfer_MouseClick;
        viewTransferOrConference.Add(btnTransfer);
        
        //btnConference = new()
        //{
        //    X = Pos.Right(btnTransfer) + 2,
        //    Y = 0,
        //    Text = Labels.CONFERENCE,
        //    TextAlignment = Alignment.Center,
        //    ShadowStyle = ShadowStyle.None,
        //    SchemeName = "BrightBlue",
        //    Enabled = false
        //};
        //btnConference.MouseClick += BtnConference_MouseClick;
        //viewTransferOrConference.Add(btnConference);

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

    private void ViewCall_ErrorOccurred(string value)
    {
        ErrorOccurred?.Invoke(value);
    }

    private void BtnTransfer_MouseClick(object? sender, MouseEventArgs e)
    {
        e.Handled = true;
        if ((pbxCall1 != null)
                    && (pbxCall2 != null))
        {
            // Need to get hold call and active call
            HubCall? holdCall = null;
            HubCall? activeCall = null;
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
                    var sdkResultBoolean = await rbHubTelephony.TransferCallAsync(activeCall, holdCall);
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

    private void UpdateDisplay(HubCall call)
    {
        if ((pbxCall2 != null)
                    && (pbxCall2.CallId == call.CallId))
        {
            pbxCall2 = call;
        }
        else if ((pbxCall1 is null) || (pbxCall1.CallId == call.CallId))
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
        btnTransfer.Enabled = enableAdvanceAction;

        // Clear Pbx call if necessary
        if (pbxCall1?.CallStatus == CallStatus.UNKNOWN)
            pbxCall1 = null;

        if (pbxCall2?.CallStatus == CallStatus.UNKNOWN)
            pbxCall2 = null;
    }

    private void RbHubTelephony_TelephonyStatusUpdated(Boolean available)
    {
        Terminal.Gui.App.Application.Invoke(() =>
        {
            serviceAvailable = available;
            UpdateDisplay();
        });
    }

    private void RbHubTelephony_PBXAgentInfoUpdated(Rainbow.Model.PbxAgentInfo pbxAgentInfo)
    {
        Terminal.Gui.App.Application.Invoke(() =>
        {
            serviceEnabled = pbxAgentInfo.XmppAgentStatus == "started";
            if (serviceEnabled)
                serviceAvailable = true;
            UpdateDisplay();
        });
    }

    private void RbHybridTelephony_HybridCallUpdated(HubCall call)
    {
        Terminal.Gui.App.Application.Invoke(() =>
        {
            UpdateDisplay(call);
        });
    }
}

