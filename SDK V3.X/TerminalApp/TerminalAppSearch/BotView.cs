using Rainbow;
using Rainbow.Consts;
using Rainbow.Example.Common;
using Rainbow.Model;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

internal class BotView: View
{
    internal UserConfig rbAccount;

    private readonly Rainbow.Application rbApplication;
    private readonly AutoReconnection rbAutoReconnection;
    private readonly Contacts rbContacts;
    private readonly Invitations rbInvitations;
    private readonly Contact currentContact;

    private LoginView? loginView;

    private SearchResultsView? searchResultsView;
    private View? viewFieldSearch;
    private Label? lblFieldSearch;
    private TextField? textFieldSearch;

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

        rbApplication.Restrictions.LogRestRequest = true;

        rbAutoReconnection = rbApplication.GetAutoReconnection();
        rbContacts = rbApplication.GetContacts();
        rbInvitations = rbApplication.GetInvitations();
        currentContact = rbContacts.GetCurrentContact();

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
        // Login View
        loginView = new(rbAccount, rbApplication, true)
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Height = Dim.Percent(90),
            Width = Dim.Percent(90)
        };

        viewFieldSearch = new()
        {
            X = Pos.Center(),
            Y = 1,
            Width = Dim.Percent(80),
            Height = 1,
            Visible = false,
            CanFocus = true,
        };

        lblFieldSearch = new()
        {
            X = 0,
            Y = 0,
            Text = "Search by name:",
            Width = Dim.Auto(DimAutoStyle.Text),
            Height = 1
        };

        textFieldSearch = new()
        {
            X = Pos.Right(lblFieldSearch) + 1,
            Y = 0,
            Width = Dim.Fill(1),
            Text = "",
            TextAlignment = Alignment.Start,
            ReadOnly = false
        };
        textFieldSearch.TextChanged += TextFieldSearch_TextChanged;
        viewFieldSearch.Add(lblFieldSearch, textFieldSearch);

        searchResultsView = new(rbApplication)
        { 
            X = Pos.Center(), 
            Y = Pos.Bottom(viewFieldSearch),
            Height = Dim.Fill(2),
            Width = viewFieldSearch.Width,
            Visible = false
        };

        Add(loginView, viewFieldSearch, searchResultsView);
    }

    private void TextFieldSearch_TextChanged(object? sender, EventArgs e)
    {
        if (searchResultsView is null)
            return;

        searchResultsView.SearchMembersByName(textFieldSearch.Text);
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

                    loginView.Visible = false;
                    viewFieldSearch.Visible = true;
                    searchResultsView.Visible = true;
                    break;

                case ConnectionStatus.Connecting:
                    Title = $"{rbAccount.Prefix} - [Connecting]";
                    break;

                case ConnectionStatus.Disconnected:
                    Title = $"{rbAccount.Prefix} - [Disconnected]";

                    loginView.Visible = true;
                    viewFieldSearch.Visible = false;
                    searchResultsView.Visible = false;
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
