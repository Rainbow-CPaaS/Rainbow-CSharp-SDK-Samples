using Rainbow.Consts;
using Rainbow.Delegates;
using Rainbow.Model;
using System.Data;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

public partial class HubTelephonyMakeCallView : View
{
    private const String LBL_DESKTOP = "desktop";
    public event StringDelegate? ErrorOccurred;

    const string LBL_ANY = "Any";

    readonly Rainbow.Application rbApplication;
    readonly Rainbow.HubTelephony rbHubTelephony;
    readonly Rainbow.Contacts rbContacts;

    Contact? currentContact = null;

    //HubDevice? selectedCurrentDevice = null;
    HubDevice? selectedPhoneUsed = null;
    String? selectedResource = null;
    List<String> resources = new List<String>();

    readonly View viewLeft;
    readonly View viewRight;

    readonly View viewCurrentDevice;
    readonly ItemSelector itemSelectorCurrentDevice;
    readonly Button btnCurrentDeviceAccept;

    readonly View viewAnonymousCall;
    readonly ItemSelector itemSelectorAnonymousCall;
    readonly Button btnAnonymousCallAccept;

    readonly View viewAutoAccept;
    readonly ItemSelector autoAcceptCallOnDesktop;
    readonly Button btnAutoAccept;

    readonly Label lblPhoneNumber;
    readonly TextField textFieldPhoneNumber;

    readonly CheckBox cbAutoAccept;

    readonly Label lblPhoneUsed;
    readonly ItemSelector itemSelectorPhoneUsed;
    
    readonly Label lblResource;
    readonly ItemSelector itemSelectorResources;

    readonly Label lblMakeCallInfo;
    readonly Button btnMakeCall;
    readonly Label lblInactive;

    private PopoverMenu contextMenu = new();

    Boolean? serviceAvailable = null;
    Boolean serviceEnabled = false;

    public HubTelephonyMakeCallView(Rainbow.Application rbApplication)
    {
        this.rbApplication = rbApplication;
        rbHubTelephony = rbApplication.GetHubTelephony();
        rbContacts = rbApplication.GetContacts();

        rbContacts.UserSettingsUpdated += RbContacts_UserSettingsUpdated;
        rbContacts.ContactPresenceUpdated += RbContacts_ContactPresenceUpdated;

        rbHubTelephony.TelephonyStatusUpdated += RbHubTelephony_TelephonyStatusUpdated;
        rbHubTelephony.PBXAgentInfoUpdated += RbHubTelephony_PBXAgentInfoUpdated;

        rbHubTelephony.CurrentDeviceUpdated += RbHubTelephony_CurrentDeviceUpdated;
        rbHubTelephony.DevicesAvailableUpdated += RbHubTelephony_DevicesAvailableUpdated;
        rbHubTelephony.DeviceStateUpdated += RbHubTelephony_DeviceStateUpdated;

        rbHubTelephony.CallLineIdentificationRestrictionUpdated += RbHubTelephony_CallLineIdentificationRestrictionUpdated;


        rbHubTelephony.CallUpdated += RbHubTelephony_CallUpdated;

        Title = $"Make Call";
        BorderStyle = LineStyle.Dotted;
        CanFocus = true;

        Border.Add(Tools.VerticalExpanderButton());

        viewCurrentDevice = new()
        {
            X = Pos.Center(),
            Y = 0,
            Width = Dim.Auto(DimAutoStyle.Content),
            Height = 1,
            CanFocus = true,
        };
        itemSelectorCurrentDevice = new(Labels.CURRENT_PHONE)
        {
            X = 0,
            Y = 0,
            Width = 60,
            Height = 1,
            CanFocus = true
        };
        btnCurrentDeviceAccept = new()
        {
            X = Pos.Right(itemSelectorCurrentDevice),
            Y = 0,
            Text = "Set",
            ShadowStyle = ShadowStyle.None,
            SchemeName = "BrightBlue",
            Enabled = true,
        };
        btnCurrentDeviceAccept.MouseClick += BtnCurrentDeviceAccept_MouseClick; ;
        viewCurrentDevice.Add(itemSelectorCurrentDevice, btnCurrentDeviceAccept);

        viewAnonymousCall = new()
        {
            X = Pos.Center(),
            Y = Pos.Bottom(viewCurrentDevice),
            Width = Dim.Auto(DimAutoStyle.Content),
            Height = 1,
            CanFocus = true,
        };

        itemSelectorAnonymousCall = new(Labels.ANONYMOUS_CALL)
        {
            X = 0,
            Y = 0,
            Width = 60,
            Height = 1,
            CanFocus = true
        };
        List<String> options = [Labels.ON, Labels.OFF];
        itemSelectorAnonymousCall.SetItems(options);

        btnAnonymousCallAccept = new()
        {
            X = Pos.Right(itemSelectorAnonymousCall),
            Y = 0,
            Text = "Set",
            ShadowStyle = ShadowStyle.None,
            SchemeName = "BrightBlue",
            Enabled = true,
        };
        btnAnonymousCallAccept.MouseClick += BtnAnonymousCallAccept_MouseClick;
        viewAnonymousCall.Add(itemSelectorAnonymousCall, btnAnonymousCallAccept);

        viewAutoAccept = new()
        {
            X = Pos.Center(),
            Y = Pos.Bottom(viewAnonymousCall),
            Width = Dim.Auto(DimAutoStyle.Content),
            Height = 1,
            CanFocus = true,
        };
        
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

        lblPhoneUsed = new()
        {
            X = 1,
            Y = Pos.Bottom(lblPhoneNumber),
            Text = "Phone:",
            TextAlignment = Alignment.End,
            Width = 15,
            Height = 1
        };

        itemSelectorPhoneUsed = new()
        {
            X = Pos.Right(lblPhoneUsed) + 1,
            Y = Pos.Top(lblPhoneUsed),
            Text = "",
            Width = Dim.Fill(1),
            Height = 1
        };
        itemSelectorPhoneUsed.SelectedItemUpdated += ItemSelectorDevices_SelectedItemUpdated;

        viewLeft.Add(lblPhoneNumber, textFieldPhoneNumber, lblPhoneUsed, itemSelectorPhoneUsed);

        viewRight = new()
        {
            X = Pos.Percent(50),
            Y = Pos.Bottom(viewAutoAccept),
            Width = Dim.Percent(50),
            Height = Dim.Auto(DimAutoStyle.Content),
            CanFocus = true,
        };

        cbAutoAccept = new()
        {
            Text = Labels.AUTO_ACCEPT,
            TextAlignment = Alignment.Center,
            Y = 1,
            Height = 1,
            Width = Dim.Fill(),
        };

        lblResource = new()
        {
            X = 1,
            Y = 2,
            Text = "Resource:",
            TextAlignment = Alignment.End,
            Width = 15,
            Height = 1
        };

        itemSelectorResources = new()
        {
            Text = "",
            X = Pos.Right(lblResource) + 1,
            Y = Pos.Top(lblResource),
            Width = Dim.Fill(1),
            Height = 1
        };
        itemSelectorResources.SelectedItemUpdated += ItemSelectorResources_SelectedItemUpdated;
        viewRight.Add(cbAutoAccept, lblResource, itemSelectorResources);

        lblMakeCallInfo = new()
        {
            X = Pos.Center(),
            Y = Pos.Bottom(viewLeft),
            SchemeName = "Red",
        };

        btnMakeCall = new()
        {
            Text = "Call",
            X = Pos.Center(),
            Y = Pos.Bottom(viewLeft) + 1,
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

        Add(viewCurrentDevice, viewAnonymousCall, viewAutoAccept, viewLeft, viewRight, lblMakeCallInfo, btnMakeCall, lblInactive);

        Height = Dim.Auto(DimAutoStyle.Content);
        Width = Dim.Fill(0);

        UpdateDisplay();
    }

    private Boolean IsAutoAcceptUserSettingSet()
    {
        Boolean? autoAnswer = rbContacts.GetUserSettingBooleanValue(UserSetting.AutoAnswer);
        String ? autoAnswerByDeviceType = rbContacts.GetUserSettingStringValue(UserSetting.AutoAnswerByDeviceType);
        return (autoAnswer is not null) && (autoAnswer.Value)
            && (autoAnswerByDeviceType is not null) && (autoAnswerByDeviceType == LBL_DESKTOP);
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

        lblInactive.Height = available ? 0 : 1;
        viewCurrentDevice.Height = available ? 1 : 0;
        viewAutoAccept.Height = available ? 1 : 0;
        viewLeft.Height = available ? Dim.Auto(DimAutoStyle.Content) : 0;
        viewRight.Height = available ? Dim.Auto(DimAutoStyle.Content) : 0;
        btnMakeCall.Height = available ? 1 : 0;
    }
    
    private void UpdateHubDevices()
    {
        var devices = rbHubTelephony.GetDevices()?.Values;
        var currentDevice = rbHubTelephony.GetCurrentDevice();

        if(selectedPhoneUsed is null)
            selectedPhoneUsed = currentDevice;
        //if(selectedCurrentDevice is null)
        //    selectedCurrentDevice = currentDevice;

        HubDevice? firstDevice = null;
        List<Item> items = new();
        Item? itemSelectedPhoneUsed = null;
        Item? itemSelectedCurrentPhone = null;
        if (devices?.Count > 0)
        {
            foreach (var device in devices)
            {
                String text = "";
                String info = "";
                if(device.IsSipDevice())
                {
                    text = Labels.OFFICE_PHONE;
                    if (device.InService != true)
                        info = $" - {Labels.OUT_OF_SERVICE}";
                }
                else
                    text = Labels.COMPUTER;

                // We want to store the first device available
                firstDevice ??= device;

                var item = new Item()
                {
                    Id = device.DeviceId,
                    Text = $"{text}{info}",
                };

                items.Add(item);
                if (item.Id == selectedPhoneUsed?.DeviceId)
                    itemSelectedPhoneUsed = item;

                if (item.Id == currentDevice?.DeviceId)
                    itemSelectedCurrentPhone = item;
            }
        }

        Terminal.Gui.App.Application.Invoke(() =>
        {

            if (items.Count > 0)
            {
                // Phone used
                itemSelectorPhoneUsed.SetItems(items);
                if (itemSelectedPhoneUsed is not null)
                    itemSelectorPhoneUsed.SetItemSelected(itemSelectedPhoneUsed);
                else
                {
                    itemSelectorPhoneUsed.SetItemSelected(0);
                    selectedPhoneUsed = firstDevice;
                }

                // Current Phone
                itemSelectorCurrentDevice.SetItems(items);
                if (itemSelectedCurrentPhone is not null)
                    itemSelectorCurrentDevice.SetItemSelected(itemSelectedCurrentPhone);
                else
                {
                    itemSelectorCurrentDevice.SetItemSelected(0);
                    //selectedCurrentDevice = firstDevice;
                }
            }
            else
            {
                itemSelectorPhoneUsed.SetItems((List<Item>?) null);
                selectedPhoneUsed = null;

                itemSelectorCurrentDevice.SetItems((List<Item>?) null);
                //selectedCurrentDevice = null;
            }

            CheckDeviceAndResourceSelection();
        });
    }

    private void UpdateResources()
    {
        // Get list of resources
        var resources = rbContacts.GetResources();

        // We want only web client or desktop client
        resources = resources.Where(s => s.StartsWith("web") || s.StartsWith("desk")).OrderDescending().ToList();

        if(!this.resources.SequenceEqual(resources))
        {
            this.resources = resources;

            if (resources.Count > 0)
            {
                if (!resources.Contains(selectedResource))
                    selectedResource = resources[0];
            }
            else
                selectedResource = null;

            Terminal.Gui.App.Application.Invoke(() =>
            {
                itemSelectorResources.SetItems(this.resources);
                if (selectedResource is not null)
                    itemSelectorResources.SetItemSelected(selectedResource);

                CheckDeviceAndResourceSelection();
            });
        }

    }

private void CheckDeviceAndResourceSelection()
    {
        if(selectedPhoneUsed is null)
        {
            // Hide resource selection
            lblResource.Height = 0;
            itemSelectorResources.Height = 0;

            // Display info about Make Calle
            lblMakeCallInfo.Height = 1;
            lblMakeCallInfo.Text = "To make a call, a phone must be selected";
            return;
        }

        if (selectedPhoneUsed.IsSipDevice())
        {
            // Hide resource selection
            lblResource.Height = 0;
            itemSelectorResources.Height = 0;

            if (selectedPhoneUsed.InService == true)
            {
                lblMakeCallInfo.Height = 0;
            }
            else
            {
                lblMakeCallInfo.Height = 1;
                lblMakeCallInfo.Text = "To make a call, the Office Phone must be in service";
            }
        }
        else
        {
            // Display resource selection
            lblResource.Height = 1;
            itemSelectorResources.Height = 1;

            if (String.IsNullOrEmpty(selectedResource))
            {
                lblMakeCallInfo.Height = 1;
                lblMakeCallInfo.Text = "To make a call with the Computer, a resource must be specified";
            }
            else
            {
                // Current Phone must no be a SIP Device
                var currentDevice = rbHubTelephony.GetCurrentDevice();
                if(currentDevice is null || currentDevice.IsSipDevice())
                {
                    lblMakeCallInfo.Height = 1;
                    lblMakeCallInfo.Text = "To make a call with the Computer, it must be set as current phone";
                }
                else
                    lblMakeCallInfo.Height = 0;
            }
        }

        // Enable MakeCall button
        btnMakeCall.Enabled = lblMakeCallInfo.Height == 0;
    }

#region Events from UI

    private void BtnCurrentDeviceAccept_MouseClick(object? sender, MouseEventArgs e)
    {
        e.Handled = true;
        var item = itemSelectorCurrentDevice.ItemSelected;
        if (item is not null)
        {
            var devices = rbHubTelephony.GetDevices()?.Values;
            var device = devices?.FirstOrDefault(d => d.DeviceId == item.Id);
            if (device is not null)
            {
                //selectedCurrentDevice = device;

                Task.Run(async () =>
                {
                    var sdkResultBoolean = await rbHubTelephony.SetCurrentDeviceAsync(device);
                    if (!sdkResultBoolean.Success)
                    {
                        Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                    }
                });
            }
        }
    }

    private void BtnAnonymousCallAccept_MouseClick(object? sender, MouseEventArgs e)
    {
        e.Handled = true;
        var item = itemSelectorAnonymousCall.ItemSelected;
        if (item is not null)
        {
            Task.Run(async () =>
            {
                var sdkResultBoolean = await rbHubTelephony.SetCallLineIdentificationRestrictionAsync((item.Id == "0"));
                if (!sdkResultBoolean.Success)
                {
                    Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                }
            });
        }
    }

    private void BtnAutoAccept_MouseClick(object? sender, MouseEventArgs e)
    {
        e.Handled = true;
        var item = autoAcceptCallOnDesktop.ItemSelected;
        if (item is not null)
        {
            Task.Run(async () =>
            {
                var sdkResultBoolean = await rbContacts.UpdateUserSettingAsync(UserSetting.AutoAnswer, (item.Id == "0"));
                if (!sdkResultBoolean.Success)
                {
                    Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                }

                if (item.Id == "0")
                {
                    sdkResultBoolean = await rbContacts.UpdateUserSettingAsync(UserSetting.AutoAnswerByDeviceType, LBL_DESKTOP);
                    if (!sdkResultBoolean.Success)
                    {
                        Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
                    }
                }
            });
        }
    }

    private void BtnMakeCall_MouseClick(object? sender, MouseEventArgs e)
    {
        e.Handled = true;

        Task.Run(async () =>
        {
            var sdkResultBoolean = await rbHubTelephony.MakeCallAsync(textFieldPhoneNumber.Text, device: selectedPhoneUsed, resource: selectedResource, callerAutoAnswer: cbAutoAccept.CheckedState == CheckState.Checked);
            if (!sdkResultBoolean.Success)
            {
                Rainbow.Util.RaiseEvent(() => ErrorOccurred, rbApplication, sdkResultBoolean.Result.ToString());
            }
        });

    }

    private void ItemSelectorResources_SelectedItemUpdated(object? sender, Item e)
    {
        selectedResource = e?.Text;
        CheckDeviceAndResourceSelection();
    }

    private void ItemSelectorDevices_SelectedItemUpdated(object? sender, Item e)
    {
        var id = String.IsNullOrEmpty(e?.Id) ? "" : e.Id;
        var devices = rbHubTelephony.GetDevices();
        if (devices?.TryGetValue(id, out var device) == true)
            selectedPhoneUsed = device;
        else
            selectedPhoneUsed = null; // Should not occurred
        CheckDeviceAndResourceSelection();
    }

#endregion Events from UI

#region Events from SDK

    private void RbContacts_ContactPresenceUpdated(Presence presence)
    {
        currentContact ??= rbContacts.GetCurrentContact();
        if(presence.Contact.Peer.Id == currentContact.Peer.Id)
            UpdateResources();
    }

    private void RbContacts_UserSettingsUpdated()
    {
        Terminal.Gui.App.Application.Invoke(() =>
        {
            UpdateDisplay();
        });
    }

    private void RbHubTelephony_CallLineIdentificationRestrictionUpdated(bool value)
    {
        Terminal.Gui.App.Application.Invoke(() =>
        {
            itemSelectorAnonymousCall.SetItemSelected(value ? 0 : 1);
        });
    }

    private void RbHubTelephony_CallUpdated(HubCall call)
    {
        // TODO
        /*
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
        */
    }

    private void RbHubTelephony_CurrentDeviceUpdated(HubDevice hubDevice)
    {
        UpdateHubDevices();
    }

    private void RbHubTelephony_DevicesAvailableUpdated(Dictionary<string, HubDevice> available, Dictionary<string, HubDevice> added, Dictionary<string, HubDevice> removed)
    {
        UpdateHubDevices();
    }

    private void RbHubTelephony_DeviceStateUpdated(HubDeviceState hubDeviceState)
    {
        UpdateHubDevices();
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

#endregion Events from SDK
    
}

