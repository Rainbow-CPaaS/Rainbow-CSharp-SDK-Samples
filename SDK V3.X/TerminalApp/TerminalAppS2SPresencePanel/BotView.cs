using EmbedIO;
using Rainbow;
using Rainbow.Consts;
using Rainbow.Example.Common;
using Rainbow.Model;
using Terminal.Gui;

internal class BotView: View
{
    internal UserConfig rbAccount;

    private readonly Rainbow.Application rbApplication;
    private readonly AutoReconnection rbAutoReconnection;
    private readonly Contacts rbContacts;
    private readonly Invitations rbInvitations;
    private readonly Contact currentContact;

    private readonly WebServer webServer;


    private LoginView? loginView;
    private View? panelsSelection;
    
    private NotificationView? notificationView;
    private PresenceView? presenceView;
    private PresencePanelView? presencePanelView;
    private PresencePanelView? contactsPanelView;

    private ContextMenu? contextMenu;

    private Button? testButton;
    private int testNumber = 0;
    private Message message = null;

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

        //rbApplication.Restrictions.AcceptBubbleInvitation = true;
        //rbApplication.Restrictions.AcceptUserInvitation = true;

        // S2S is used - s pecify the callback Url
        rbApplication.SetS2SCallbackUrl(Configuration.ExeSettings. S2SCallbackURL);

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

    void AddPresencePanelView()
    {
        if (presencePanelView == null)
        {
            presencePanelView = new(rbApplication, rosterOnly: true)
            {
                X = 0,
                Y = Pos.Bottom(presenceView) + 1,
            };
            Add(presencePanelView);
        }
        presencePanelView.Visible = true;
    }

    void AddContactsPanelView()
    {
        if (contactsPanelView == null)
        {
            contactsPanelView = new(rbApplication, rosterOnly: false)
            {
                X = 0,
                Y = Pos.Bottom(presenceView) + 1,
            };
            Add(contactsPanelView);
        }
        contactsPanelView.Visible = true;
    }
    
    void AddNotificationView()
    {
        if (notificationView == null)
        {
            notificationView = new NotificationView(rbApplication)
            {
                X = Pos.AnchorEnd() - 2,
                Y = 0
            };
            Add(notificationView);
        }
    }

    void AddPresenceView()
    {
        if (presenceView == null)
        {
            presenceView = new PresenceView(rbApplication, rbContacts.GetCurrentContact())
            {
                X = Pos.Center(),
                Y = 0,
                //Width = 50,
                Height = 1,
            };

            Add(presenceView);

            presenceView.ContactClick += PresenceView_ContactClick;
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
                Width = Dim.Auto(DimAutoStyle.Content)
            };

            var contactsPanelSelection = new Button()
            {
                X = Pos.Align(Alignment.Start),
                Y = 0,
                Height = 1,
                Width = Dim.Auto(DimAutoStyle.Text),
                Text = "Contacts",
                ShadowStyle = ShadowStyle.None,
                ColorScheme = Tools.ColorSchemeBlackOnWhite
            };
            contactsPanelSelection.MouseClick += (sender, me) =>
            {
                if (presencePanelView != null)
                    presencePanelView.Visible = false;
                AddContactsPanelView();
            };

            var presencePanelSelection = new Button()
            {
                X = Pos.Align(Alignment.End),
                Y = 0,
                Height = 1,
                Width = Dim.Auto(DimAutoStyle.Text),
                Text = "Roster",
                ShadowStyle = ShadowStyle.None,
                ColorScheme = Tools.ColorSchemeBlackOnWhite
            };
            presencePanelSelection.MouseClick += (sender, me) =>
            {
                if (contactsPanelView != null)
                    contactsPanelView.Visible = false;
                AddPresencePanelView();
            };

            panelsSelection.Add(contactsPanelSelection, presencePanelSelection);
            Add(panelsSelection);
        }
    }

    void RemovePanelsSelection()
    {
        if (panelsSelection != null)
        {
            Remove(panelsSelection);
            panelsSelection = null;
        }
    }

    void AddTestButton()
    {
        if (testButton == null)
        {
            testButton = new()
            {
                X = 2,
                Y = 1,
                Text = "Test",
                Height = 1,
                Width = Dim.Auto(DimAutoStyle.Text),
                ShadowStyle = ShadowStyle.None,
                ColorScheme = Tools.ColorSchemeBlackOnWhite
            };
            testButton.MouseClick += TestButton_MouseClick;

            Add(testButton);
        }
    }

    void RemoveTestButton()
    {
        if (testButton != null)
        {
            Remove(testButton);
            testButton = null;
        }
    }

    private void TestButton_MouseClick(object? sender, MouseEventEventArgs e)
    {
        if (e.MouseEvent.Flags == MouseFlags.Button1Clicked)
        {
            e.MouseEvent.Handled = true;
        }
    }

    private void PresenceView_ContactClick(object? sender, PeerAndMouseEventEventArgs e)
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

        List<MenuItem> menuItems = [];
        for (int i = 0; i < stdItems.Length; i++)
        {
            var name = stdItems[i];
            MenuItem menuItem;
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

                menuItem = new MenuItem(
                                    name,
                                    ""
                                    , () => UpdatePresence(name)
                                    , shortcutKey: shortcut
                                    ); ;
                menuItems.Add(menuItem);
            }
        }

        // Dispose previoues context menu (if any)
        contextMenu?.Dispose();

        contextMenu = new()
        {
            Position = BotWindow.MousePosition,
        };

        MenuBarItem menuBarItem = new(menuItems.ToArray());
        contextMenu.Show(menuBarItem);
    }

    private void UpdatePresence(String strPresence)
    {
        contextMenu?.Hide();

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

    void RemoveNotificationView()
    {
        if (notificationView != null)
        {
            Remove(notificationView);
            notificationView = null;
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

        Terminal.Gui.Application.Invoke(async () =>
        {
            switch (connectionState.Status)
            {
                case ConnectionStatus.Connected:
                    // Ask more info to the server about contacts invitations
                    var task1 = rbInvitations.GetReceivedInvitationsAsync(InvitationStatus.Pending);
                    var task2 = rbInvitations.GetSentInvitationsAsync(InvitationStatus.Pending);
                    Task[] tasks = [task1, task2];
                    await Task.WhenAll(tasks);

                    Title = rbAccount.Prefix;

                    loginView.Visible = false;
                    AddPresenceView();
                    AddPresencePanelView();
                    AddPanelsSelection();
                    AddNotificationView();

                    //AddTestButton();
                    break;

                case ConnectionStatus.Connecting:
                    Title = $"{rbAccount.Prefix} - [Connecting]";
                    break;

                case ConnectionStatus.Disconnected:
                    Title = $"{rbAccount.Prefix} - [Disconnected]";

                    loginView.Visible = true;
                    RemovePresenceView();
                    RemovePresencePanelView();
                    RemovePanelsSelection();
                    RemoveNotificationView();

                    RemoveTestButton();
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
