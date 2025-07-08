using Microsoft.Extensions.Logging;
using Rainbow;
using Rainbow.Consts;
using Rainbow.Example.Common;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;

internal class BotView: View
{
    internal UserConfig rbAccount;
    
    private readonly ILogger log;

    private readonly Rainbow.Application rbApplication;
    private readonly AutoReconnection rbAutoReconnection;

    private LoginView? loginView; // To display login/pwd and connection progression

    private HubTelephonyServiceView hubTelephonyServiceView;          // To display Hub Telephony Service status (if any)
    private HubTelephonyMakeCallView hubTelephonyMakeCallView;        // To display Hub Telephony Make call view
    private HubTelephonyPanelCallView hubTelephonyPanelCallView;      // To display Hub Telephony Call in progress
    /*
    private HybridTelephonyCallForwardView hubTelephonyCallForwardView;  // To display Hub Telephony Call Forward settings (if any)
    private HybridTelephonyVoiceMailView hubTelephonyVoiceMailView;      // To display Hub Telephony Voice Mail settings (if any)
    */

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
        log.LogInformation("[Terminal Application]: {ProductName} v{ClientVersion}", Global.ProductName(), Global.FileVersion());

        rbAutoReconnection = rbApplication.GetAutoReconnection();

        // Set some Restrictions
        rbApplication.Restrictions.UseHybridTelephony = false;
        rbApplication.Restrictions.UseHubTelephony = true;
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

        hubTelephonyServiceView = new(rbApplication)
        {
            X = 0,
            Y = 1,
            Width = Dim.Percent(leftPartPercent),
            Visible = false
        };
        Add(hubTelephonyServiceView);

        hubTelephonyMakeCallView = new(rbApplication)
        {
            X = Pos.Right(hubTelephonyServiceView),
            Y = 1,
            Width = Dim.Percent(rightPartPercent),
            Visible = false
        };
        hubTelephonyMakeCallView.ErrorOccurred += HubTelephonyMakeCallView_ErrorOccurred;
        Add(hubTelephonyMakeCallView);

        hubTelephonyPanelCallView = new(rbApplication)
        {
            X = Pos.Right(hubTelephonyServiceView),
            Y = Pos.Bottom(hubTelephonyMakeCallView),
            Width = Dim.Percent(rightPartPercent),
            Visible = false
        };
        hubTelephonyPanelCallView.ErrorOccurred += HubTelephonyPanelCallView_ErrorOccurred;
        Add(hubTelephonyPanelCallView);

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

    private void HubTelephonyPanelCallView_ErrorOccurred(string value)
    {
        Tools.DisplayLoggerAsDialog("Error from 'Call In progress' view", value);
    }

    private void HubTelephonyMakeCallView_ErrorOccurred(string value)
    {
        Tools.DisplayLoggerAsDialog("Error from 'Make Call' view", value);
    }

    void UpdateViews(bool loginViewVisible)
    {
        if (loginView is not null)
            loginView.Visible = loginViewVisible;

        hubTelephonyServiceView.Visible = !loginViewVisible;
        hubTelephonyMakeCallView.Visible = !loginViewVisible;
        hubTelephonyPanelCallView.Visible = !loginViewVisible;
    }

#region Events received from Rainbow SDK
    void RbApplication_ConnectionStateChanged(Rainbow.Model.ConnectionState connectionState)
    {
        if (loginView == null)
            return;

        Terminal.Gui.App.Application.Invoke(async () =>
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
