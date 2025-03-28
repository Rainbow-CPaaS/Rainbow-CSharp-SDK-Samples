using Rainbow.Consts;
using Rainbow.Example.Common;
using Terminal.Gui;

internal class BotView: View
{
    internal UserConfig rbAccount;

    private readonly Rainbow.Application rbApplication;

    private LoginView? loginView;

    internal BotView(UserConfig rbAccount)
    {
        this.rbAccount = rbAccount;

        // Set Rainbow objects / events
        string prefix = rbAccount.Prefix + "_";
        string iniFileName = rbAccount.Prefix + ".ini";

        // We want to log files from SDK for this Bot
        NLogConfigurator.AddLogger(prefix);

        // Create Rainbow SDK objects
        rbApplication = new Rainbow.Application(iniFolderFullPathName: rbAccount.IniFolderPath, iniFileName: iniFileName, loggerPrefix: prefix);

        // Set global configuration info
        rbApplication.SetApplicationInfo(Configuration.Credentials.ServerConfig.AppId, Configuration.Credentials.ServerConfig.AppSecret);
        rbApplication.SetHostInfo(Configuration.Credentials.ServerConfig.HostName);

        // We want to receive events from SDK
        rbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;   // Triggered when the Connection State will change

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
                    break;

                case ConnectionStatus.Connecting:
                    Title = $"{rbAccount.Prefix} - [Connecting]";
                    break;

                case ConnectionStatus.Disconnected:
                    Title = $"{rbAccount.Prefix} - [Disconnected]";
                    break;
            }
        });
    }

#endregion Events received from Rainbow SDK
}
