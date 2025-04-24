using Rainbow.Delegates;
using Terminal.Gui;

public partial class HybridTelephonyServiceView: View
{
    public const String SERVICE_NOT_AVAILABLE = "Hybrid Telephony service not available.";
    public const String SERVICE_DISABLED = "Hybrid Telephony service momentarily disabled ...";
    public const String FEATURE_CHECKING = "Checking if service/feature is available ...";
    
    public const String CALL_FWD_NOT_AVAILABLE = "Call forward not available.";
    public const String NOMADIC_NOT_AVAILABLE = "Nomadic not available.";
    public const String VM_NOT_AVAILABLE = "Voicemail not available.";

    readonly Rainbow.Application rbApplication;
    readonly Rainbow.HybridTelephony rbHybridTelephony;

    readonly Label lblPbxAgent;
    readonly Label lblPbxAgentValue;
    readonly Label lblPbxId;
    readonly Label lblPbxIdValue;
    readonly Label lblInactive;

    Boolean? serviceAvailable = null;
    Boolean serviceEnabled = false;

    public HybridTelephonyServiceView(Rainbow.Application rbApplication)
    {
        this.rbApplication = rbApplication;
        rbHybridTelephony = rbApplication.GetHybridTelephony();

        rbHybridTelephony.HybridTelephonyStatusUpdated += RbHybridTelephony_HybridTelephonyStatusUpdated;
        rbHybridTelephony.HybridPBXAgentInfoUpdated += RbHybridTelephony_HybridPBXAgentInfoUpdated;

        Title = $"Hybrid Telephony Service";
        BorderStyle = LineStyle.Dotted;
        CanFocus = true;

        Border.Add(Tools.VerticalExpanderButton());

        lblPbxAgent = new Label
        {
            X = 1,
            Y = 1,
            Text = "PBX Agent:",
            TextAlignment = Alignment.End,
            Width = 10,
            Height = 1
        };

        lblPbxAgentValue = new Label
        {
            X = Pos.Right(lblPbxAgent) + 1,
            Y = Pos.Top(lblPbxAgent),
            Text = "" ,
            TextAlignment = Alignment.Start,
            Width = Dim.Auto(DimAutoStyle.Text),
            Height = 1,
            ColorScheme = Tools.ColorSchemeBlueOnGray
        };

        lblPbxId = new Label
        {
            X = 1,
            Y = Pos.Bottom(lblPbxAgentValue),
            Text = "PBX Id:",
            TextAlignment = Alignment.End,
            Width = 10,
            Height = 1
        };

        lblPbxIdValue = new Label
        {
            X = Pos.Right(lblPbxId) + 1,
            Y = Pos.Top(lblPbxId),
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

        Height = 6; // Dim.Auto(DimAutoStyle.Content);
        Width = Dim.Auto(DimAutoStyle.Content);

        Add(lblPbxAgent, lblPbxAgentValue, lblPbxId, lblPbxIdValue, lblInactive);

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        Boolean available = false;
        if (serviceAvailable is not null)
        {
            available = serviceEnabled && serviceAvailable.Value;

            if (!serviceAvailable.Value)
            {
                lblInactive.Text = HybridTelephonyServiceView.SERVICE_NOT_AVAILABLE;
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
        lblPbxAgent.Height = available ? 1 : 0;
        lblPbxAgentValue.Height = available ? 1 : 0;
        lblPbxId.Height = available ? 1 : 0;
        lblPbxIdValue.Height = available ? 1 : 0;

        if (available)
        {
            lblPbxAgentValue.Text = rbHybridTelephony.GetHybridPBXAgentInformation()?.Version;
            lblPbxIdValue.Text = rbHybridTelephony.GetHybridPBXAgentInformation()?.PbxId;
        }
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
            serviceEnabled =  pbxAgentInfo.XmppAgentStatus == "started";
            if(serviceEnabled)
                serviceAvailable = true;
            UpdateDisplay();
        });
    }
}

