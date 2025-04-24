using Microsoft.Extensions.Logging;
using Rainbow;
using Rainbow.Consts;
using Rainbow.Example.Common;
using Terminal.Gui;

internal class BotView: View
{
    internal UserConfig rbAccount;

    private readonly Rainbow.Application rbApplication;
    private readonly AutoReconnection rbAutoReconnection;

    private LoginView? loginView; // To display login/pwd and connection progression

    private HybridTelephonyMakeCallView hybridTelephonyMakeCallView; // To display Hybrid Telephony make call view
    private HybridTelephonyPanelCallView hybridTelephonyPanelCallView; // To display Hybrid Telephony Call in progress
    private HybridTelephonyServiceView hybridTelephonyServiceView; // To display Hybrid Telephony Service status (if any)
    private HybridTelephonyCallForwardView hybridTelephonyCallForwardView; // To display Hybrid Telephony Call Forward settings (if any)
    private HybridTelephonyNomadicView hybridTelephonyNomadicView; // To display Hybrid Telephony Nomadic settings (if any)
    private HybridTelephonyVoiceMailView hybridTelephonyVoiceMailView; // To display Hybrid Telephony Voice Mail settings (if any)

    private readonly ILogger log;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal BotView(UserConfig rbAccount)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        this.rbAccount = rbAccount;

        // Set Rainbow objects / events
        string prefix = rbAccount.Prefix + "_";
        string iniFileName = rbAccount.Prefix + ".ini";

        // We want to log files from SDK for this Bot
        NLogConfigurator.AddLogger(prefix);
        log = Rainbow.LogFactory.CreateLogger(prefix);

        // Create Rainbow SDK objects
        rbApplication = new Rainbow.Application(iniFolderFullPathName: rbAccount.IniFolderPath, iniFileName: iniFileName, loggerPrefix: prefix);
        log.LogInformation("[Terminal Application]: {ProductName} v{ClientVersion}]", Global.ProductName(), Global.FileVersion());
        
        rbAutoReconnection = rbApplication.GetAutoReconnection();

        // Set some Restrictions
        rbApplication.Restrictions.UseHybridTelephony = true;
        rbApplication.Restrictions.UseHubTelephony = false;
        rbApplication.Restrictions.LogRestRequest = true;

        // Set global configuration info
        rbApplication.SetApplicationInfo(Configuration.Credentials.ServerConfig.AppId, Configuration.Credentials.ServerConfig.AppSecret);
        rbApplication.SetHostInfo(Configuration.Credentials.ServerConfig.HostName);

        // We want to receive events from SDK
        rbApplication.AuthenticationFailed += RbApplication_AuthenticationFailed;       // Triggered when the authentication process will fail
        rbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;   // Triggered when the Connection State will change

        rbAutoReconnection.Cancelled += RbAutoReconnection_Cancelled;                       // Triggered when AutoReonnection service is cancelled
        rbAutoReconnection.TokenExpired += RbAutoReconnection_TokenExpired;                 // Triggered when the Security Token is expired

        SetViewLayout();
    }

    private void SetViewLayout()
    {
        Title = rbAccount.Prefix;
        CanFocus = true;

        ColorScheme = Tools.ColorSchemeMain;

        X = 0;
        Width = Dim.Fill();

        Y = 0;
        Height = Dim.Fill(1);

        BorderStyle = LineStyle.Rounded;
        TabStop = TabBehavior.TabStop;

        AddSubViews();
    }

    private void AddSubViews()
    {
        int leftPartPercent = 40;
        int rightPartPercent = 100 - leftPartPercent;

        hybridTelephonyServiceView = new(rbApplication)
        {
            X = 0,
            Y = 1,
            Width = Dim.Percent(leftPartPercent),
            Visible = true
        };

        hybridTelephonyCallForwardView = new(rbApplication)
        {
            X = Pos.Left(hybridTelephonyServiceView),
            Y = Pos.Bottom(hybridTelephonyServiceView) + 1,
            Width = Dim.Percent(leftPartPercent),
            Visible = true
        };
        hybridTelephonyCallForwardView.ErrorOccurred += HybridTelephonyCallForwardView_ErrorOccurred;

        hybridTelephonyNomadicView = new(rbApplication)
        {
            X = Pos.Left(hybridTelephonyCallForwardView),
            Y = Pos.Bottom(hybridTelephonyCallForwardView) + 1,
            Width = Dim.Percent(leftPartPercent),
            Visible = true
        };
        hybridTelephonyNomadicView.ErrorOccurred += HybridTelephonyNomadicView_ErrorOccurred;

        hybridTelephonyVoiceMailView = new(rbApplication)
        {
            X = Pos.Left(hybridTelephonyNomadicView),
            Y = Pos.Bottom(hybridTelephonyNomadicView) + 1,
            Width = Dim.Percent(leftPartPercent),
            Visible = true
        };

        hybridTelephonyMakeCallView = new(rbApplication)
        {
            X = Pos.Right(hybridTelephonyServiceView),
            Y = 1,
            Width = Dim.Percent(rightPartPercent),
            Visible = true
        };
        hybridTelephonyMakeCallView.ErrorOccurred += HybridTelephonyMakeCallView_ErrorOccurred;

        hybridTelephonyPanelCallView = new(rbApplication)
        {
            X = Pos.Right(hybridTelephonyServiceView),
            Y = Pos.Bottom(hybridTelephonyMakeCallView) + 1,
            Width = Dim.Percent(rightPartPercent),
            Visible = true
        };
        hybridTelephonyPanelCallView.ErrorOccurred += HybridTelephonyPanelCallView_ErrorOccurred;


        Add(hybridTelephonyServiceView, hybridTelephonyCallForwardView, hybridTelephonyNomadicView, hybridTelephonyVoiceMailView, hybridTelephonyMakeCallView, hybridTelephonyPanelCallView);

        // Login View
        loginView = new(rbAccount, rbApplication, true)
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Height = Dim.Percent(90),
            Width = Dim.Percent(90)
        };

        Add(loginView);

        UpdateViews(true);
    }

    private void HybridTelephonyPanelCallView_ErrorOccurred(string value)
    {
        Tools.DisplayLoggerAsDialog("Error from 'Call in progress' view", value);
    }

    private void HybridTelephonyMakeCallView_ErrorOccurred(string value)
    {
        Tools.DisplayLoggerAsDialog("Error from 'Make Call' view", value);
    }

    private void HybridTelephonyNomadicView_ErrorOccurred(string value)
    {
        Tools.DisplayLoggerAsDialog("Error from 'Nomadic' view", value);
    }

    private void HybridTelephonyCallForwardView_ErrorOccurred(string value)
    {
        Tools.DisplayLoggerAsDialog("Error from 'Call forward' view", value);
    }

    void UpdateViews(bool loginViewVisible)
    {
        if(loginView is not null) 
            loginView.Visible = loginViewVisible;

        hybridTelephonyServiceView.Visible = !loginViewVisible;
        hybridTelephonyCallForwardView.Visible = !loginViewVisible;
        hybridTelephonyNomadicView.Visible = !loginViewVisible;
        hybridTelephonyMakeCallView.Visible = !loginViewVisible;
        hybridTelephonyPanelCallView.Visible = !loginViewVisible;
        hybridTelephonyVoiceMailView.Visible = !loginViewVisible;
    }

#region Events received from Rainbow SDK
    void RbApplication_ConnectionStateChanged(Rainbow.Model.ConnectionState connectionState)
    {
        if (loginView == null)
            return;

        Terminal.Gui.Application.Invoke(() =>
        {
            switch (connectionState.Status)
            {
                case ConnectionStatus.Connected:
                    Title = rbAccount.Prefix;
                    UpdateViews(false);
                    break;

                case ConnectionStatus.Connecting:
                    Title = $"{rbAccount.Prefix} - [Connecting]";
                    break;

                case ConnectionStatus.Disconnected:
                    Title = $"{rbAccount.Prefix} - [Disconnected]";
                    UpdateViews(true);
                    break;
            }
        });
    }

    void RbApplication_AuthenticationFailed(SdkError sdkError)
    {
        // TODO
    }

    void RbAutoReconnection_TokenExpired()
    {
        // TODO
    }
    
    void RbAutoReconnection_Cancelled(SdkError sdkError)
    {
        // TODO
    }
#endregion Events received from Rainbow SDK
}
