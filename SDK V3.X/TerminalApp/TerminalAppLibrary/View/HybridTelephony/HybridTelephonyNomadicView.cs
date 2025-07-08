using Rainbow.Delegates;
using Rainbow.Model;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

public partial class HybridTelephonyNomadicView: View
{
    public event StringDelegate? ErrorOccurred;

    readonly Rainbow.Application rbApplication;
    readonly Rainbow.HybridTelephony rbHybridTelephony;

    readonly Label lblInactive;
    readonly RadioGroup radioGroup;
    readonly TextField textFieldPhoneNumber;
    readonly Button btnSet;

    Boolean? serviceAvailable = null;
    Boolean serviceEnabled = false;
    HybridNomadicStatus? nomadicStatus = null;

    public HybridTelephonyNomadicView(Rainbow.Application rbApplication)
    {
        this.rbApplication = rbApplication;
        rbHybridTelephony = rbApplication.GetHybridTelephony();

        rbHybridTelephony.HybridTelephonyStatusUpdated += RbHybridTelephony_HybridTelephonyStatusUpdated;
        rbHybridTelephony.HybridPBXAgentInfoUpdated += RbHybridTelephony_HybridPBXAgentInfoUpdated;
        rbHybridTelephony.HybridNomadicStatusUpdated += RbHybridTelephony_HybridNomadicStatusUpdated; ;

        Title = $"Nomadic";
        BorderStyle = LineStyle.Dotted;
        CanFocus = true;

        Border.Add(Tools.VerticalExpanderButton());

        radioGroup = new RadioGroup()
        {
            X = 1,
            Y = 1,
            TextAlignment = Alignment.End,
            Width = Dim.Auto(DimAutoStyle.Content),
            Height = 3,
            RadioLabels = ["Enable on Office Phone", "Enable on Computer", "Enable on Phone Number:"],
        };

        textFieldPhoneNumber = new TextField
        {
            // Position text field adjacent to the label
            X = Pos.Right(radioGroup) + 1,
            Y = Pos.Bottom(radioGroup) - 1,

            // Fill remaining horizontal space
            Width = Dim.Fill(1),
            Text = "",
            TextAlignment = Alignment.Start,
        };

        btnSet = new Button
        {
            Text = "Set",
            X = Pos.Align(Alignment.Center),
            Y = Pos.Bottom(radioGroup) + 1,
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
            Height = 1,
        };


        Height = Dim.Auto(DimAutoStyle.Content);
        Width = Dim.Auto(DimAutoStyle.Content);

        Add(radioGroup, textFieldPhoneNumber, btnSet, lblInactive);

        UpdateDisplay();
    }

    private void BtnSet_MouseClick(object? sender, MouseEventArgs e)
    {
        var canUseOfficePhone = !rbHybridTelephony.IsVirtualTerminal();
        var computerModeAvailable = rbHybridTelephony.HybridNomadicOnComputerAvailable();

        // Do we want to disable call fwd ?
        if (radioGroup.SelectedItem == 0)
        {
            if(canUseOfficePhone)
            {
                Task.Run(async () =>
                {
                    var sdkResultBoolean = await rbHybridTelephony.DeactivateHybridNomadicStatusAsync();
                    if (!sdkResultBoolean.Success)
                    {
                        UpdateDisplay();
                        Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                    }
                });
            }
            else if (computerModeAvailable)
            {
                Task.Run(async () =>
                {
                    var sdkResultBoolean = await rbHybridTelephony.ActivateHybridNomadicStatusToComputerAsync();
                    if (!sdkResultBoolean.Success)
                    {
                        UpdateDisplay();
                        Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                    }
                });
            }
            else
            {
                Task.Run(async () =>
                {
                    var sdkResultBoolean = await rbHybridTelephony.ActivateHybridNomadicStatusToPhoneNumberAsync(textFieldPhoneNumber.Text);
                    if (!sdkResultBoolean.Success)
                    {
                        UpdateDisplay();
                        Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                    }
                });
            }
        }
        else if (radioGroup.SelectedItem == 1)
        {
            if (canUseOfficePhone && computerModeAvailable)
            {
                Task.Run(async () =>
                {
                    var sdkResultBoolean = await rbHybridTelephony.ActivateHybridNomadicStatusToComputerAsync();
                    if (!sdkResultBoolean.Success)
                    {
                        UpdateDisplay();
                        Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                    }
                });
            }
            else if (canUseOfficePhone || computerModeAvailable)
            {
                Task.Run(async () =>
                {
                    var sdkResultBoolean = await rbHybridTelephony.ActivateHybridNomadicStatusToPhoneNumberAsync(textFieldPhoneNumber.Text);
                    if (!sdkResultBoolean.Success)
                    {
                        UpdateDisplay();
                        Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                    }
                });
            }
        }
        else if (radioGroup.SelectedItem == 2)
        {
            Task.Run(async () =>
            {
                var sdkResultBoolean = await rbHybridTelephony.ActivateHybridNomadicStatusToPhoneNumberAsync(textFieldPhoneNumber.Text);
                if (!sdkResultBoolean.Success)
                {
                    UpdateDisplay();
                    Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                }
            });
        }
        else
        {
            // Do nothing since nothing is selected
        }
    }

    private void UpdateDisplay()
    {
        Boolean available = false;
        if ((serviceAvailable is not null) && (nomadicStatus is not null))
        {
            available = serviceEnabled && serviceAvailable.Value && nomadicStatus.Available;

            if (!serviceAvailable.Value)
            {
                lblInactive.Text = Labels.SERVICE_NOT_AVAILABLE;
                lblInactive.SchemeName = "Red";
            }
            else if (!nomadicStatus.Available)
            {
                lblInactive.Text = Labels.NOMADIC_NOT_AVAILABLE;
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
        radioGroup.Height = available ? 1 : 0;
        textFieldPhoneNumber.Height = available ? 1 : 0;
        btnSet.Height = available ? 1 : 0;

        textFieldPhoneNumber.Text = (nomadicStatus?.PhoneNumber is null) ? "" : nomadicStatus.PhoneNumber;

        if (nomadicStatus?.Available == true)
        {
            var officePhoneAvailable = rbHybridTelephony.HybridNomadicOnOfficePhoneAvailable();
            var computerModeAvailable = rbHybridTelephony.HybridNomadicOnComputerAvailable();
            var otherPhoneAvailable = rbHybridTelephony.HybridNomadicOnOtherPhoneAvailable();

            int count = 0;
            List<String> labels = new();
            if (officePhoneAvailable)
            {
                count++;
                labels.Add("Enable on Office Phone");
            }
            if (computerModeAvailable)
            {
                count++;
                labels.Add("Enable on Computer");
            }

            if (otherPhoneAvailable)
            {
                textFieldPhoneNumber.Height = 1;
                textFieldPhoneNumber.Y = Pos.Top(radioGroup) + count;

                count++;
                labels.Add("Enable on Phone Number:");
            }
            else
                textFieldPhoneNumber.Height = 0;

            btnSet.Height = count > 1 ? 1 : 0;

            radioGroup.RadioLabels = labels.ToArray();
            radioGroup.Height = labels.Count;

            if (nomadicStatus.ComputerMode && computerModeAvailable)
            {
                if(officePhoneAvailable)
                    radioGroup.SelectedItem = 1;
                else
                    radioGroup.SelectedItem = 0;
            }
            else if (nomadicStatus.MakeCallInitiatorIsMain && officePhoneAvailable)
            {
                radioGroup.SelectedItem = 0;
            }
            else if (!String.IsNullOrEmpty(nomadicStatus.PhoneNumber))
            {
                if (officePhoneAvailable && computerModeAvailable)
                    radioGroup.SelectedItem = 2;
                else if (officePhoneAvailable || computerModeAvailable)
                    radioGroup.SelectedItem = 1;
                else
                    radioGroup.SelectedItem = 0;
            }
            else
                radioGroup.SelectedItem = -1;
        }

    }

    private void RbHybridTelephony_HybridTelephonyStatusUpdated(Boolean? available)
    {
        Terminal.Gui.App.Application.Invoke(() =>
        {
            serviceAvailable = available;
            UpdateDisplay();
        });
    }

    private void RbHybridTelephony_HybridNomadicStatusUpdated(HybridNomadicStatus nomadicStatus)
    {
        Terminal.Gui.App.Application.Invoke(() =>
        {
            this.nomadicStatus = nomadicStatus;
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

