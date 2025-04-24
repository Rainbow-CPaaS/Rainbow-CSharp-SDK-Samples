using Terminal.Gui;

public partial class HybridTelephonyVoiceMailView : View
{
    readonly Rainbow.Application rbApplication;
    readonly Rainbow.HybridTelephony rbHybridTelephony;

    readonly Label lblNbVM;
    readonly Label lblNbVMValue;
    readonly Label lblPhoneNumber;
    readonly Label lblPhoneNumberValue;
    readonly Label lblInactive;

    Boolean? serviceAvailable = null;
    Boolean serviceEnabled = false;
    Boolean isVMAvailable = false;
    int nbVM = 0;

    public HybridTelephonyVoiceMailView(Rainbow.Application rbApplication)
    {
        this.rbApplication = rbApplication;
        rbHybridTelephony = rbApplication.GetHybridTelephony();

        rbHybridTelephony.HybridTelephonyStatusUpdated += RbHybridTelephony_HybridTelephonyStatusUpdated;
        rbHybridTelephony.HybridPBXAgentInfoUpdated += RbHybridTelephony_HybridPBXAgentInfoUpdated;

        rbHybridTelephony.HybridVoiceMessagesNumberUpdated += RbHybridTelephony_HybridVoiceMessagesNumberUpdated;

        Title = $"Voicemail";
        BorderStyle = LineStyle.Dotted;
        CanFocus = true;

        Border.Add(Tools.VerticalExpanderButton());

        lblNbVM = new Label
        {
            X = 1,
            Y = 1,
            Text = "Nb voice message(s):",
            TextAlignment = Alignment.End,
            Width = 26,
            Height = 1
        };

        lblNbVMValue = new Label
        {
            X = Pos.Right(lblNbVM) + 1,
            Y = Pos.Top(lblNbVM),
            Text = "",
            TextAlignment = Alignment.Start,
            Width = Dim.Auto(DimAutoStyle.Text),
            Height = 1,
            ColorScheme = Tools.ColorSchemeBlueOnGray
        };

        lblPhoneNumber = new Label
        {
            X = 1,
            Y = Pos.Bottom(lblNbVMValue),
            Text = "Voicemail phone number:",
            TextAlignment = Alignment.End,
            Width = 26,
            Height = 1
        };

        lblPhoneNumberValue = new Label
        {
            X = Pos.Right(lblPhoneNumber) + 1,
            Y = Pos.Top(lblPhoneNumber),
            Text = "",
            TextAlignment = Alignment.Start,
            Width = Dim.Auto(DimAutoStyle.Text),
            Height = 1,
            ColorScheme = Tools.ColorSchemeBlueOnGray
        };

        lblInactive = new Label
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            TextAlignment = Alignment.Start,
            Width = Dim.Auto(DimAutoStyle.Text),
            Height = 1
        };

        Height = 6;
        Width = Dim.Auto(DimAutoStyle.Content);

        Add(lblNbVM, lblNbVMValue, lblPhoneNumber, lblPhoneNumberValue, lblInactive);

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        Boolean available = false;
        if (serviceAvailable is not null)
        {
            isVMAvailable = rbHybridTelephony.VoiceMailAvailable();

            available = serviceEnabled && serviceAvailable.Value && isVMAvailable;

            if (!serviceAvailable.Value)
            {
                lblInactive.Text = HybridTelephonyServiceView.SERVICE_NOT_AVAILABLE;
                lblInactive.ColorScheme = Tools.ColorSchemeRedOnGray;
            }
            else if (!isVMAvailable)
            {
                lblInactive.Text = HybridTelephonyServiceView.VM_NOT_AVAILABLE;
                lblInactive.ColorScheme = Tools.ColorSchemeRedOnGray;
            }
            else if (!available)
            {
                lblInactive.Text = HybridTelephonyServiceView.SERVICE_DISABLED;
                lblInactive.ColorScheme = Tools.ColorSchemeGreenOnGray;
            }
        }
        else
        {
            lblInactive.Text = HybridTelephonyServiceView.FEATURE_CHECKING;
            lblInactive.ColorScheme = Tools.ColorSchemeGreenOnGray;
        }

        lblInactive.Height = available ? 0 : 1;

        lblNbVM.Height = available ? 1 : 0;
        lblNbVMValue.Text = $"{nbVM}";
        lblNbVMValue.Height = available ? 1 : 0;

        lblPhoneNumber.Height = available ? 1 : 0;
        lblPhoneNumberValue.Text = "" + rbHybridTelephony.GetVoiceMailPhoneNumber();
        lblPhoneNumberValue.Height = available ? 1 : 0;
    }

    private void RbHybridTelephony_HybridTelephonyStatusUpdated(Boolean? available)
    {
        Terminal.Gui.Application.Invoke(() =>
        {
            serviceAvailable = available;
            UpdateDisplay();
        });
    }

    private void RbHybridTelephony_HybridPBXAgentInfoUpdated(Rainbow.Model.HybridPbxAgentInfo pbxAgentInfo)
    {
        Terminal.Gui.Application.Invoke(() =>
        {
            serviceEnabled = pbxAgentInfo.XmppAgentStatus == "started";
            if (serviceEnabled)
                serviceAvailable = true;
            UpdateDisplay();
        });
    }

    private void RbHybridTelephony_HybridVoiceMessagesNumberUpdated(int nb)
    {
        Terminal.Gui.Application.Invoke(() =>
        {
            nbVM = nb;
            UpdateDisplay();
        });
    }
}

