using Rainbow;
using Rainbow.Consts;
using Rainbow.Example.Common;
using Rainbow.Model;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
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
    private View? panelsSelection;
    
    private NotificationView? notificationView;
    private PresenceView? presenceView;
    private PresencePanelView? presencePanelView;
    private PresencePanelView? contactsPanelView;

    private PopoverMenu? contextMenu;

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
        rbApplication.Restrictions.LogEvent = true;
        rbApplication.Restrictions.UseHubTelephony = true;

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
        Add(loginView);

        AddPresenceView();
        AddPresencePanelView();
        AddPanelsSelection();
        AddNotificationView();
        AddContactsPanelView();
    }

    void AddPresencePanelView()
    {
        if (presencePanelView == null)
        {
            presencePanelView = new(rbApplication, rosterOnly: true)
            {
                X = 0,
                Y = Pos.Bottom(presenceView) + 1,
                Visible = false
            };
            Add(presencePanelView);
        }
    }

    void AddContactsPanelView()
    {
        if (contactsPanelView == null)
        {
            contactsPanelView = new(rbApplication, rosterOnly: false)
            {
                X = 0,
                Y = Pos.Bottom(presenceView) + 1,
                Visible = false
            };
            Add(contactsPanelView);
        }
    }
    
    void AddNotificationView()
    {
        if (notificationView == null)
        {
            notificationView = new NotificationView(rbApplication)
            {
                X = Pos.AnchorEnd() - 2,
                Y = 0,
                Visible = false
            };
            Add(notificationView);
        }
    }

    void AddPresenceView()
    {
        if (presenceView == null)
        {
            presenceView = new PresenceView(rbApplication)
            {
                X = Pos.Center(),
                Y = 0,
                Width = 50,
                Height = 1,
                Visible = false
            };

            Add(presenceView);

            presenceView.PeerClick += PresenceView_ContactClick;
        }
    }

    void AddPanelsSelection()
    {
        if (panelsSelection == null)
        {
            panelsSelection = new()
            {
                X = Pos.Center(),
                Y = 1,
                Height = 1,
                Width = Dim.Auto(DimAutoStyle.Content),
                Visible = false
            };

            var contactsPanelSelection = new Button()
            {
                X = Pos.Align(Alignment.Start),
                Y = 0,
                Height = 1,
                Width = Dim.Auto(DimAutoStyle.Text),
                Text = "Contacts",
                ShadowStyle = ShadowStyle.None,
            };
            contactsPanelSelection.MouseClick += (sender, me) =>
            {
                presencePanelView.Visible = false;
                contactsPanelView.Visible = true;
            };

            var presencePanelSelection = new Button()
            {
                X = Pos.Align(Alignment.End),
                Y = 0,
                Height = 1,
                Width = Dim.Auto(DimAutoStyle.Text),
                Text = "Roster",
                ShadowStyle = ShadowStyle.None,
            };
            presencePanelSelection.MouseClick += (sender, me) =>
            {
                presencePanelView.Visible = true;
                contactsPanelView.Visible = false;
            };

            panelsSelection.Add(contactsPanelSelection, presencePanelSelection);
            Add(panelsSelection);
        }
    }

    private void TestButton_MouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Flags == MouseFlags.Button1Clicked)
        {
            e.Handled = true;
        }
    }

    private void PresenceView_ContactClick(object? sender, PeerAndMouseEventArgs e)
    {
        if (e.MouseEvent.Flags == MouseFlags.Button3Clicked)
        {
            e.MouseEvent.Handled = true;
            DisplayPresenceContextMenu();
            return;
        }

        if(e.MouseEvent.Flags == MouseFlags.Button1DoubleClicked)
        {
            e.MouseEvent.Handled = true;
            Tools.DisplayPresenceDetails(rbApplication, e.Peer);
            return;
        }

    }

    private void DisplayPresenceContextMenu()
    {
        String[] stdItems = ["Online", "Away", "Invisible", "Dnd", "", "Busy - Audio", "Busy - Video", "Busy - Sharing"];

        List<MenuItemv2> menuItems = [];
        for (int i = 0; i < stdItems.Length; i++)
        {
            var name = stdItems[i];
            MenuItemv2 menuItem;
            if (name == "")
#pragma warning disable CS8625
                menuItems.Add(null);
#pragma warning restore CS8625
            else
            {
                char help;
                Key shortcut;
                if (name.Contains(" - "))
                {
                    help = ' ';
#pragma warning disable CS8600
                    shortcut = null;
#pragma warning restore CS8600
                }
                else
                {
                    help = name[0];
                    shortcut = new Key(help).NoShift;
                }

                menuItem = new (
                                    name,
                                    ""
                                    , () => UpdatePresence(name)
                                    , key: shortcut
                                    ); ;
                menuItems.Add(menuItem);
            }
        }

        // Dispose previoues context menu (if any)
        contextMenu?.Dispose();

        contextMenu = new(menuItems);
        contextMenu.MakeVisible(BotWindow.MousePosition);
    }

    private void UpdatePresence(String strPresence)
    {
        Terminal.Gui.App.Application.Popover?.Hide(contextMenu);

        Presence? presence;

        strPresence = strPresence.ToLower();
        if (strPresence.Contains(" - "))
        {
            var info = strPresence.Split(" - ");
            presence = rbContacts.CreatePresence(info[0], info[1]);
        }
        else
        {
            if (strPresence == "invisible")
                strPresence = "xa";
            presence = rbContacts.CreatePresence(strPresence);
        }

        if (presence != null)
        {
            var _ = rbContacts.SetPresenceLevelAsync(presence);
        }
    }

    void ToggleVisibility(bool visible)
    {
        loginView.Visible = visible;
        presenceView.Visible = !visible;
        presencePanelView.Visible = !visible;
        panelsSelection.Visible = !visible;
        notificationView.Visible = !visible;
    }

#region Events received from Rainbow SDK
    void RbApplication_ConnectionStateChanged(Rainbow.Model.ConnectionState connectionState)
    {
        if (loginView == null)
            return;

        Terminal.Gui.App.Application.Invoke(() =>
        {
            switch (connectionState.Status)
            {
                case ConnectionStatus.Connected:
                    // Ask more info to the server about contacts invitations
                    var _1 =  rbInvitations.GetReceivedInvitationsAsync(InvitationStatus.Pending);
                    var _2 = rbInvitations.GetSentInvitationsAsync(InvitationStatus.Pending);

                    Title = rbAccount.Prefix;

                    ToggleVisibility(false);
                    presenceView.SetContact(rbContacts.GetCurrentContact());

                    break;

                case ConnectionStatus.Connecting:
                    Title = $"{rbAccount.Prefix} - [Connecting]";
                    break;

                case ConnectionStatus.Disconnected:
                    Title = $"{rbAccount.Prefix} - [Disconnected]";

                    ToggleVisibility(true);
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
