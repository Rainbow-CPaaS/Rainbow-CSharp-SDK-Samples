using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

public partial class HybridTelephonyServiceView: View
{
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

        Border.GetOrCreateView().Add(Tools.VerticalExpanderButton());

        lblPbxAgent = new Label
        {
            X = 1,
            Y = 1,
            Text = Labels.PBX_AGENT + ":",
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
            SchemeName = "BrightBlue"
        };

        lblPbxId = new Label
        {
            X = 1,
            Y = Pos.Bottom(lblPbxAgentValue),
            Text = Labels.PBX_ID + ":",
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
            serviceEnabled =  pbxAgentInfo.XmppAgentStatus == "started";
            if(serviceEnabled)
                serviceAvailable = true;
            UpdateDisplay();
        });
    }
}

