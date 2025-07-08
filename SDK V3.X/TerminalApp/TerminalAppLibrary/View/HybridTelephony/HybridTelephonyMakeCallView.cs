using Rainbow.Consts;
using Rainbow.Delegates;
using Rainbow.Enums;
using Rainbow.Model;
using System.Data;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

public partial class HybridTelephonyMakeCallView : View
{
    private const String DESKTOP = "desktop";
    public event StringDelegate? ErrorOccurred;

    const string LBL_ANY = "Any";

    readonly Rainbow.Application rbApplication;
    readonly Rainbow.HybridTelephony rbHybridTelephony;
    readonly Rainbow.Contacts rbContacts;

    readonly View viewLeft;
    readonly View viewRight;

    readonly View viewAutoAccept;
    readonly ItemSelector autoAcceptCallOnDesktop;
    readonly Button btnAutoAccept;

    readonly Label lblPhoneNumber;
    readonly TextField textFieldPhoneNumber;
    String? currentResource;

    readonly Label lblSubject;
    readonly TextField textFieldSubject;
    readonly Label lblResource;
    readonly Label lblResourceSelection;
    readonly Label lblCorrelator;
    readonly TextField textFieldCorrelator;
    readonly Button btnMakeCall;

    readonly Label lblInactive;

    private PopoverMenu contextMenu = new();

    Boolean? serviceAvailable = null;
    Boolean serviceEnabled = false;

    Call? pbxCall1;
    Call? pbxCall2;

    public HybridTelephonyMakeCallView(Rainbow.Application rbApplication)
    {
        this.rbApplication = rbApplication;
        rbHybridTelephony = rbApplication.GetHybridTelephony();
        rbContacts = rbApplication.GetContacts();

        rbContacts.UserSettingsUpdated += RbContacts_UserSettingsUpdated;

        rbHybridTelephony.HybridTelephonyStatusUpdated += RbHybridTelephony_HybridTelephonyStatusUpdated;
        rbHybridTelephony.HybridPBXAgentInfoUpdated += RbHybridTelephony_HybridPBXAgentInfoUpdated;

        rbHybridTelephony.HybridCallUpdated += RbHybridTelephony_HybridCallUpdated;

        Title = $"Make Call";
        BorderStyle = LineStyle.Dotted;
        CanFocus = true;

        Border.Add(Tools.VerticalExpanderButton());

        viewAutoAccept = new()
        {
            X = Pos.Center(),
            Y = 0,
            Width = Dim.Auto(DimAutoStyle.Content),
            Height = 1,
            CanFocus = true,
        };
        List<String> options = [Labels.ON, Labels.OFF];
        autoAcceptCallOnDesktop = new(Labels.AUTO_ACCEPT_CALL_UCAAS)
        {
            X = 0,
            Y = 0,
            Width = 60,
            Height = 1,
            CanFocus = true
        };
        autoAcceptCallOnDesktop.SetItems(options);

        btnAutoAccept = new()
        {
            X = Pos.Right(autoAcceptCallOnDesktop),
            Y = 0,
            Text = "Set",
            ShadowStyle = ShadowStyle.None,
            SchemeName = "BrightBlue",
            Enabled = true,
        };
        btnAutoAccept.MouseClick += BtnAutoAccept_MouseClick;

        viewAutoAccept.Add(autoAcceptCallOnDesktop, btnAutoAccept);

        viewLeft = new()
        {
            X = 0,
            Y = Pos.Bottom(viewAutoAccept),
            Width = Dim.Percent(50),
            Height = Dim.Auto(DimAutoStyle.Content),
            CanFocus = true,
        };

        lblPhoneNumber = new()
        {
            X = 1,
            Y = 1,
            Text = "Phone number:",
            TextAlignment = Alignment.End,
            Width = 15,
            Height = 1
        };

        textFieldPhoneNumber = new()
        {
            X = Pos.Right(lblPhoneNumber) + 1,
            Y = Pos.Top(lblPhoneNumber),
            Text = "",
            TextAlignment = Alignment.Start,
            Width = Dim.Fill(1),
            Height = 1,
            ReadOnly = false,
        };

        lblSubject = new()
        {
            X = 1,
            Y = Pos.Bottom(lblPhoneNumber),
            Text = "Subject:",
            TextAlignment = Alignment.End,
            Width = 15,
            Height = 1
        };

        textFieldSubject = new()
        {
            X = Pos.Right(lblSubject) + 1,
            Y = Pos.Top(lblSubject),
            Text = "",
            TextAlignment = Alignment.Start,
            Width = Dim.Fill(1),
            Height = 1,
            ReadOnly = false,
        };

        viewLeft.Add(lblPhoneNumber, textFieldPhoneNumber, lblSubject, textFieldSubject);

        viewRight = new()
        {
            X = Pos.Percent(50),
            Y = Pos.Bottom(viewAutoAccept),
            Width = Dim.Percent(50),
            Height = Dim.Auto(DimAutoStyle.Content),
            CanFocus = true,
        };

        lblResource = new()
        {
            X = 1,
            Y = 1,
            Text = "Resource:",
            TextAlignment = Alignment.End,
            Width = 15,
            Height = 1
        };

        lblResourceSelection = new()
        {
            Text = "",
            X = Pos.Right(lblResource) + 1,
            Y = Pos.Top(lblResource),
            ShadowStyle = ShadowStyle.None,
            SchemeName = "BrightBlue"
        };
        lblResourceSelection.MouseClick += LblResourceSelection_MouseClick;

        lblCorrelator = new()
        {
            X = 1,
            Y = 2,
            Text = "Correlator:",
            TextAlignment = Alignment.End,
            Width = 15,
            Height = 1
        };

        textFieldCorrelator = new()
        {
            X = Pos.Right(lblCorrelator) + 1,
            Y = Pos.Top(lblCorrelator),
            Text = "",
            TextAlignment = Alignment.Start,
            Width = Dim.Fill(1),
            Height = 1,
            ReadOnly = false,
        };

        viewRight.Add(lblResource, lblResourceSelection, lblCorrelator, textFieldCorrelator);

        btnMakeCall = new()
        {
            Text = "Call",
            X = Pos.Center(),
            Y = Pos.Bottom(viewRight) + 1,
            ShadowStyle = ShadowStyle.None,
            SchemeName = "BrightBlue",
            Enabled = true,
        };
        btnMakeCall.MouseClick += BtnMakeCall_MouseClick;

        lblInactive = new Label
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            TextAlignment = Alignment.Start,
            Width = Dim.Auto(DimAutoStyle.Text),
            Height = 1
        };

        Add(viewAutoAccept, viewLeft, viewRight, btnMakeCall, lblInactive);

        Height = Dim.Auto(DimAutoStyle.Content);
        Width = Dim.Fill(0);

        UpdateDisplay();
        UpdateResource(LBL_ANY);
    }

    private void RbContacts_UserSettingsUpdated()
    {
        Terminal.Gui.App.Application.Invoke(() =>
        {
            UpdateDisplay();
        });
    }

    private void BtnAutoAccept_MouseClick(object? sender, MouseEventArgs e)
    {
        var item = autoAcceptCallOnDesktop.ItemSelected;
        if (item is not null)
        {
            var _ = rbContacts.UpdateUserSettingAsync(UserSetting.AutoAnswer, (item.Id == "0"));
            if (item.Id == "0")
            {
                _ = rbContacts.UpdateUserSettingAsync(UserSetting.AutoAnswerByDeviceType, DESKTOP);
            }
        }
    }

    private Boolean IsAutoAcceptUserSettingSet()
    {
        Boolean? autoAnswer = rbContacts.GetUserSettingBooleanValue(UserSetting.AutoAnswer);
        String ? autoAnswerByDeviceType = rbContacts.GetUserSettingStringValue(UserSetting.AutoAnswerByDeviceType);
        return (autoAnswer is not null) && (autoAnswer.Value)
            && (autoAnswerByDeviceType is not null) && (autoAnswerByDeviceType == DESKTOP);
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

            if(available)
            {
                autoAcceptCallOnDesktop.SetItemSelected(IsAutoAcceptUserSettingSet() ? 0 : 1 );
            }
        }
        else
        {
            lblInactive.Text = Labels.FEATURE_CHECKING;
            lblInactive.SchemeName = "Green";
        }

        // /!\ DUE TO RAINBOW DESKTOP / WEBCLIENT RESTRICTION, Resource is not taken well into account - we hide it fro the moment
        // Instead we can set "Auto-Accept" 
        lblResource.Height = 0;
        lblResourceSelection.Height = 0;

        lblInactive.Height = available ? 0 : 1;
        viewAutoAccept.Height = available ? 1 : 0;
        viewLeft.Height = available ? Dim.Auto(DimAutoStyle.Content) : 0;
        viewRight.Height = available ? Dim.Auto(DimAutoStyle.Content) : 0;
        btnMakeCall.Height = available ? 1 : 0;
    }

    private void BtnMakeCall_MouseClick(object? sender, MouseEventArgs e)
    {
        e.Handled = true;

        var resource = currentResource;
        if (resource == LBL_ANY)
            resource = "";

        // We need to know if a call in already in progress or not and get its CallId
        Boolean callInProgress = false;
        Call? call = null;

        if ((pbxCall1 != null) && (pbxCall1.CallStatus == CallStatus.ACTIVE))
        {
            callInProgress = true;
            call = pbxCall1;
        }
        else if ((pbxCall2 != null) && (pbxCall2.CallStatus == CallStatus.ACTIVE))
        {
            callInProgress = true;
            call = pbxCall2;
        }

        // If there is not a call in progress we make a simple call
        if (!callInProgress)
        {
            Task.Run(async () =>
            {
                var sdkResultBoolean = await rbHybridTelephony.MakeCallAsync(textFieldPhoneNumber.Text, subject: textFieldSubject.Text, resource: resource, correlatorData: textFieldCorrelator.Text);
                if (!sdkResultBoolean.Success)
                {
                    Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                }
            });
        }
        else
        {
            Task.Run(async () =>
            {
                var sdkResultBoolean = await rbHybridTelephony.ConsultationCallAsync(call, textFieldPhoneNumber.Text, correlatorData: textFieldCorrelator.Text);
                if (!sdkResultBoolean.Success)
                {
                    Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                }
            });
        }
    }

    private void UpdateResource(String resource)
    {
        currentResource = resource;
        lblResourceSelection.Text = currentResource + " " + Emojis.TRIANGLE_DOWN;
    }

    private void LblResourceSelection_MouseClick(object? sender, MouseEventArgs e)
    {
        e.Handled = true;

        var rbContacts = rbApplication.GetContacts();
        var presences = rbContacts.GetPresencesList();

        // Avoid specific resources used for presence
        var exceptionList = Rainbow.Contacts.SPECIFIC_PRESENCE_RESOURCES.ToList();

        // Avoid default resource
        exceptionList.Add(Rainbow.Contacts.DEFAULT_RESOURCE);

        // Avoid resource of the SDK itself
        exceptionList.Add(rbApplication.GetResource());

        // Get list of resources used by the current user avoiding ones in 'exceptionList'
        var resources = presences.Values.Select(p => p.Resource).Except(exceptionList).ToList();

        MenuItemv2 menuItem;
        List<MenuItemv2> menuItems = [];
        int nb = resources.Count;

        menuItem = new ("Any",
                        "",
                        () => UpdateResource(LBL_ANY)
                        );
        menuItems.Add(menuItem);

        for (int i = 0; i < nb; i++)
        {
            int index = i;
            String txt = resources[i];
            menuItem = new (txt,
                            "",
                            () => UpdateResource(txt)
                            );
            menuItems.Add(menuItem);
        }

        contextMenu = new(menuItems);
        contextMenu.MakeVisible(e.ScreenPosition);
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

    private void RbHybridTelephony_HybridCallUpdated(Call call)
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
            pbxCall2 = call;

        // Can we perform another call ?
        //      - No call in progress
        //      - Only one call in progress and ACTIVE
        Boolean activeCall = false;
        int nbCall = 0;
        if (pbxCall1 != null)
        {
            if (pbxCall1.CallStatus == CallStatus.ACTIVE)
                activeCall = true;

            if (pbxCall1.CallStatus != CallStatus.UNKNOWN)
                nbCall++;
        }
        if (pbxCall2 != null)
        {
            if (pbxCall2.CallStatus == CallStatus.ACTIVE)
                activeCall = true;

            if (pbxCall2.CallStatus != CallStatus.UNKNOWN)
                nbCall++;
        }

        Boolean enableAdvanceAction = false;
        if ((pbxCall1 != null)
            && (pbxCall2 != null))
        {
            enableAdvanceAction = ((pbxCall1.CallStatus == CallStatus.ACTIVE) && (pbxCall2.CallStatus == CallStatus.PUT_ON_HOLD))
                                    || ((pbxCall1.CallStatus == CallStatus.PUT_ON_HOLD) && (pbxCall2.CallStatus == CallStatus.ACTIVE));
        }

        // We cannot make another call if advanced actions are available
        if (enableAdvanceAction)
        {
            // We cannot perform a MakeCall
            btnMakeCall.Enabled = false;
        }
        else
        {
            // We can perhaps perform a MakeCall
            btnMakeCall.Enabled = (activeCall && (nbCall == 1)) || (nbCall == 0);
        }

        // Clear Pbx call if necessary
        if (pbxCall1?.CallStatus == CallStatus.UNKNOWN)
            pbxCall1 = null;

        if (pbxCall2?.CallStatus == CallStatus.UNKNOWN)
            pbxCall2 = null;
    }
}

