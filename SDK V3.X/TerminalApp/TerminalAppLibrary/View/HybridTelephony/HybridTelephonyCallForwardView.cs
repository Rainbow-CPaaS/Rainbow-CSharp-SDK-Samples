using Rainbow.Consts;
using Rainbow.Delegates;
using Rainbow.Model;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

public partial class HybridTelephonyCallForwardView : View
{
    public event StringDelegate? ErrorOccurred;
    readonly Rainbow.Application rbApplication;
    readonly Rainbow.HybridTelephony rbHybridTelephony;

    readonly Label lblInactive;
    readonly RadioGroup radioGroupEnable;
    readonly RadioGroup radioGroupDestination; 
    readonly TextField textFieldPhoneNumber;
    readonly Button btnSet;
    readonly ItemSelector callForwardSelector;

    private PopoverMenu contextMenu = new();

    readonly List<string> listFwdType = ["Forwarding Immediate", "Forward on busy", "Forward on no reply", "Forward on busy/no reply"];
    int fwdTypeIndexSelected;

    Boolean? serviceAvailable = false;
    Boolean serviceEnabled = false;
    HybridCallForwardStatus? callForwardStatus = null;

    public HybridTelephonyCallForwardView(Rainbow.Application rbApplication)
    {
        this.rbApplication = rbApplication;
        rbHybridTelephony = rbApplication.GetHybridTelephony();

        rbHybridTelephony.HybridTelephonyStatusUpdated += RbHybridTelephony_HybridTelephonyStatusUpdated;
        rbHybridTelephony.HybridPBXAgentInfoUpdated += RbHybridTelephony_HybridPBXAgentInfoUpdated;

        rbHybridTelephony.HybridCallForwardStatusUpdated += RbHybridTelephony_HybridCallForwardStatusUpdated;

        Title = $"Call forward";
        BorderStyle = LineStyle.Dotted;
        CanFocus = true;

        Border.Add(Tools.VerticalExpanderButton());

        radioGroupEnable = new RadioGroup()
        {
            X = 1,
            Y = 1,
            TextAlignment = Alignment.Start,
            Width = Dim.Auto(DimAutoStyle.Content),
            Height = 2,
            RadioLabels = ["Disable", "Enable - Type:"]
        };

        callForwardSelector = new()
        {
            X = Pos.Right(radioGroupEnable) + 1,
            Y = Pos.Bottom(radioGroupEnable) - 1,
            Width = Dim.Fill(),
            CanFocus = true
        };
        callForwardSelector.SetItems(listFwdType);
        callForwardSelector.SelectedItemUpdated += CallFwdSelector_SelectedItemUpdated;

        radioGroupDestination = new RadioGroup()
        {
            X = 5,
            Y = Pos.Bottom(radioGroupEnable),
            TextAlignment = Alignment.Start,
            Width = Dim.Auto(DimAutoStyle.Content),
            Height = 2,
            RadioLabels = ["On Voicemail", "On Phone number:"]
        };

        textFieldPhoneNumber = new TextField
        {
            // Position text field adjacent to the label
            X = Pos.Right(radioGroupDestination) + 1,
            Y = Pos.Bottom(radioGroupDestination) - 1,

            // Fill remaining horizontal space
            Width = Dim.Fill(1),
            Text = "",
            TextAlignment = Alignment.Start,
        };

        btnSet = new Button
        {
            Text = "Set",
            X = Pos.Align(Alignment.Center),
            Y = Pos.Bottom(radioGroupDestination) + 1,
            ShadowStyle = ShadowStyle.None,
            SchemeName = "BrightBlue"
        };

        btnSet.MouseClick += BtnSet_MouseClick;

        lblInactive = new Label
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            TextAlignment = Alignment.Start,
            Width = Dim.Auto(DimAutoStyle.Text),
            Height = 1
        };

        Height = Dim.Auto(DimAutoStyle.Content);
        Width = Dim.Auto(DimAutoStyle.Content);

        Add(radioGroupEnable, callForwardSelector, radioGroupDestination, textFieldPhoneNumber, btnSet, lblInactive);

        UpdateDisplay();
    }

    private void CallFwdSelector_SelectedItemUpdated(object? sender, Item e)
    {
        fwdTypeIndexSelected = (int.Parse(e.Id));
    }

    private void UpdateFwdType(int index)
    {
        fwdTypeIndexSelected = index;
        callForwardSelector.SetItemSelected(index);
    }

    private void UpdateDisplay()
    {
        Boolean available = false;
        if ( (serviceAvailable is not null) && (callForwardStatus is not null))
        {
            available = serviceEnabled && serviceAvailable.Value && callForwardStatus.Available;

            if (!serviceAvailable.Value)
            {
                lblInactive.Text = Labels.SERVICE_NOT_AVAILABLE;
                lblInactive.SchemeName = "Red";
            }
            else if (!callForwardStatus.Available)
            {
                lblInactive.Text = Labels.CALL_FWD_NOT_AVAILABLE;
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
        radioGroupEnable.Height = available ? Dim.Auto(DimAutoStyle.Content) : 0;
        radioGroupDestination.Height = available ? Dim.Auto(DimAutoStyle.Content) : 0;
        textFieldPhoneNumber.Height = available ? 1 : 0;
        callForwardSelector.Height = available ? 1 : 0;
        btnSet.Height = available ? 1 : 0;

        if (available && callForwardStatus is not null)
        {
            if (!callForwardStatus.Activated)
            {
                radioGroupEnable.SelectedItem = 0;
                UpdateFwdType(0);
            }
            else
            {
                radioGroupEnable.SelectedItem = 1;
                UpdateFwdType(FwdTypeToIndex(callForwardStatus.ForwardType));
            }
            var VMAvailable = rbHybridTelephony.VoiceMailAvailable();
            if (VMAvailable)
            {
                radioGroupDestination.RadioLabels = ["On Voicemail", "On Phone number:"];
                radioGroupDestination.Height = 2;

                if (callForwardStatus.VoiceMail)
                    radioGroupDestination.SelectedItem = 0;
                else if (callForwardStatus.PhoneNumber is not null)
                {
                    radioGroupDestination.SelectedItem = 1;
                    textFieldPhoneNumber.Text = callForwardStatus.PhoneNumber;
                }
                else
                    radioGroupDestination.SelectedItem = -1;
            }
            else
            {
                radioGroupDestination.RadioLabels = ["Enable on Phone Number:"];
                radioGroupDestination.Height = 1;

                if (callForwardStatus.PhoneNumber is not null)
                {
                    radioGroupDestination.SelectedItem = 0;
                    textFieldPhoneNumber.Text = callForwardStatus.PhoneNumber;
                }
                else
                    radioGroupDestination.SelectedItem = -1;
            }
        }
    }

    private int FwdTypeToIndex(String forwardType)
    {
        switch (forwardType)
        {
            default:
            case CallForwardType.Immediate:
                return 0;

            case CallForwardType.Busy:
                return  1;

            case CallForwardType.NoReply:
                return 2;

            case CallForwardType.BusyOrNoReply:
                return 3;
        }
    }

    private String IndexToFwdType(int index)
    {
        switch (index)
        {
            default:
            case 0:
                return CallForwardType.Immediate;

            case 1:
                return CallForwardType.Busy;

            case 2:
                return CallForwardType.NoReply;

            case 3:
                return CallForwardType.BusyOrNoReply;
        }
    }

    private void BtnFwdType_MouseClick(object? sender, MouseEventArgs e)
    {
        e.Handled = true;

        List<MenuItemv2> menuItems = [];
        int nb = listFwdType.Count;

        if (rbHybridTelephony.GetHybridPBXAgentInformation()?.Type.Equals("OXO", StringComparison.InvariantCultureIgnoreCase) == true)
            nb = 1;

        for (int i = 0; i < nb; i++)
        {
            int index = i;
            var menuItem = new MenuItemv2(
                                listFwdType[i],
                                $""
                                , () => UpdateFwdType(index)
                                );
            menuItems.Add(menuItem);
        }

        contextMenu = new(menuItems);
        contextMenu.MakeVisible(e.ScreenPosition);
    }

    private void BtnSet_MouseClick(object? sender, MouseEventArgs e)
    {
        // Do we want to disable call fwd ?
        if(radioGroupEnable.SelectedItem == 0)
        {
            Task.Run(async () =>
            {
                var sdkResultBoolean = await rbHybridTelephony.DeactivateHybridCallForwardAsync();
                if(!sdkResultBoolean.Success)
                {
                    UpdateDisplay();
                    Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                }
            });
        }
        else if (radioGroupEnable.SelectedItem == 1)
        {
            var VMAvailable = rbHybridTelephony.VoiceMailAvailable();
            if(VMAvailable)
            {
                // Activate on VM
                if(radioGroupDestination.SelectedItem == 0)
                {
                    Task.Run(async () =>
                    {
                        var sdkResultBoolean = await rbHybridTelephony.ActivateHybridCallForwardOnVoiceMailAsync(callForwardType: IndexToFwdType(fwdTypeIndexSelected));
                        if (!sdkResultBoolean.Success)
                        {
                            UpdateDisplay();
                            Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                        }
                    });
                }
                // Activate on Phone Number
                else if (radioGroupDestination.SelectedItem == 1)
                {
                    Task.Run(async () =>
                    {
                        var sdkResultBoolean = await rbHybridTelephony.ActivateHybridCallForwardOnPhoneNumberAsync(textFieldPhoneNumber.Text, callForwardType: IndexToFwdType(fwdTypeIndexSelected));
                        if (!sdkResultBoolean.Success)
                        {
                            UpdateDisplay();
                            Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                        }
                    });
                }
            }
            else
            {
                // Activate on Phone Number
                if (radioGroupDestination.SelectedItem == 0)
                {
                    Task.Run(async () =>
                    {
                        var sdkResultBoolean = await rbHybridTelephony.ActivateHybridCallForwardOnPhoneNumberAsync(textFieldPhoneNumber.Text, callForwardType: IndexToFwdType(fwdTypeIndexSelected));
                        if (!sdkResultBoolean.Success)
                        {
                            UpdateDisplay();
                            Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                        }
                    });
                }
            }
        }
        else
        {
            // Do nothing since nothing is selected
        }

    }

    private void RbHybridTelephony_HybridCallForwardStatusUpdated(HybridCallForwardStatus callForwardStatus)
    {
        Terminal.Gui.App.Application.Invoke(() =>
        {
            this.callForwardStatus = callForwardStatus;
            UpdateDisplay();
        });
    }

    private void RbHybridTelephony_HybridTelephonyStatusUpdated(Boolean? available)
    {
        Terminal.Gui.App.Application.Invoke(() =>
        {
            serviceAvailable = available;
            UpdateDisplay();
        });
    }

    private void RbHybridTelephony_HybridPBXAgentInfoUpdated(Rainbow.Model.PbxAgentInfo pbxAgentInfo)
    {
        Terminal.Gui.App.Application.Invoke(() =>
        {
            serviceEnabled = pbxAgentInfo.XmppAgentStatus == "started";
            if (serviceEnabled)
                serviceAvailable = true;
            UpdateDisplay();
        });
    }
}

