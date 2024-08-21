using Rainbow;
using Rainbow.Consts;
using Terminal.Gui;

internal class BotView: View
{
    internal RainbowAccount rbAccount;

    private readonly Rainbow.Application rbApplication;
    private readonly AutoReconnection rbAutoReconnection;
    private readonly Contacts rbContacts;

    private LoginView? loginView;
    private PresenceView? presenceView;
    private PresencePanelView? presencePanelView;

    internal BotView(RainbowAccount rbAccount)
    {
        this.rbAccount = rbAccount;

        // Set Rainbow objects / events
        string prefix = rbAccount.BotName + "_";
        string iniFileName = rbAccount.BotName + ".ini";

        // We want to log files from SDK for this Bot
        NLogConfigurator.AddLogger(prefix);

        // Create Rainbow SDK objects
        rbApplication = new Rainbow.Application(iniFileName: iniFileName, loggerPrefix: prefix);

        rbAutoReconnection = rbApplication.GetAutoReconnection();
        rbContacts = rbApplication.GetContacts();

        // Set global configuration info
        rbApplication.SetApplicationInfo(Configuration.RainbowServerConfiguration.AppId, Configuration.RainbowServerConfiguration.AppSecret);
        rbApplication.SetHostInfo(Configuration.RainbowServerConfiguration.HostName);

        // We want to receive events from SDK
        rbApplication.AuthenticationFailed += RbApplication_AuthenticationFailed;       // Triggered when the authentication process will fail
        rbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;   // Triggered when the Connection State will change

        rbAutoReconnection.Cancelled += RbAutoReconnection_Cancelled;                       // Triggered when AutoReonnection service is cancelled
        rbAutoReconnection.TokenExpired += RbAutoReconnection_TokenExpired;                 // Triggered when the Security Token is expired

        SetViewLayout();
    }

    private void SetViewLayout()
    {
        Title = rbAccount.BotName;

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
        // Login View
        loginView = new(rbAccount, rbApplication, true)
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Height = Dim.Percent(90),
            Width = Dim.Percent(90)
        };

        Add(loginView);
    }

    void AddPresenceView()
    {
        if (presenceView == null)
        {
            presenceView = new PresenceView(rbApplication, rbContacts.GetCurrentContact(), true, true)
            {
                X = Pos.Center(),
                Y = 0,
                //Width = 50,
                Height = 1,
            };

            Add(presenceView);
        }
    }

    void RemovePresenceView()
    {
        if (presenceView != null)
        {
            Remove(presenceView);
            presenceView = null;
        }
    }

    void AddPresencePanelView()
    {
        if (presencePanelView == null)
        {
            var contactsInRoster = rbContacts.GetAllContactsInRoster();
            presencePanelView = new(rbApplication, contactsInRoster)
            {
                X = 0,
                Y = Pos.Bottom(presenceView),
            };
            Add(presencePanelView);
        }
    }

    void RemovePresencePanelView()
    {
        if (presencePanelView != null)
        {
            Remove(presencePanelView);
            presencePanelView = null;
        }
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
                    Title = rbAccount.BotName;

                    loginView.Visible = false;
                    AddPresenceView();
                    AddPresencePanelView();
                    break;

                case ConnectionStatus.Connecting:
                    Title = $"{rbAccount.BotName} - [Connecting]";
                    break;

                case ConnectionStatus.Disconnected:
                    Title = $"{rbAccount.BotName} - [Disconnected]";

                    loginView.Visible = true;
                    RemovePresenceView();
                    RemovePresencePanelView();
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
