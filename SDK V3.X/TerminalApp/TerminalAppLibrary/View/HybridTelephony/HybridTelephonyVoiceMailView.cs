using Rainbow.Model;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

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
    uint nbVM = 0;

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
            SchemeName = "BrightBlue"
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
            SchemeName = "BrightBlue"
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
                lblInactive.Text = Labels.SERVICE_NOT_AVAILABLE;
                lblInactive.SchemeName = "Red";
            }
            else if (!isVMAvailable)
            {
                lblInactive.Text = Labels.VM_NOT_AVAILABLE;
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

        lblNbVM.Height = available ? 1 : 0;
        lblNbVMValue.Text = $"{nbVM}";
        lblNbVMValue.Height = available ? 1 : 0;

        lblPhoneNumber.Height = available ? 1 : 0;
        lblPhoneNumberValue.Text = "" + rbHybridTelephony.GetVoiceMailPhoneNumber();
        lblPhoneNumberValue.Height = available ? 1 : 0;
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

    private void RbHybridTelephony_HybridVoiceMessagesNumberUpdated(VoiceMessageNumber voiceMessageNumber)
    {
        Terminal.Gui.App.Application.Invoke(() =>
        {
            nbVM = voiceMessageNumber.New;
            UpdateDisplay();
        });
    }
}

