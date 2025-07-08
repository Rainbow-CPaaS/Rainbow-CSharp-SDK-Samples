using Rainbow;
using Rainbow.Consts;
using Rainbow.Example.Common;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;

internal class BotView: View
{
    internal UserConfig rbAccount;

    private readonly Rainbow.Application rbApplication;
    private readonly AutoReconnection rbAutoReconnection;
    private readonly Contacts rbContacts;
    private readonly Bubbles rbBubbles;
    private readonly Conversations rbConversations;

    private LoginView? loginView;

    private View? viewRight;
    private ItemSelector? bubbleSelector;
    private BubblePanel? bubblePanel;

    private View? viewLeft;
    private ConversationPanelView? conversationPanelView;

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
        rbBubbles = rbApplication.GetBubbles();
        rbConversations = rbApplication.GetConversations();

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

        viewLeft = new()
        {
            X = 0,
            Y = 1,
            Width = Dim.Percent(50) - 1,
            Height = Dim.Fill(),
            Visible = false,
            CanFocus = true
        };

        conversationPanelView = new(rbApplication)
        {
            Y = 1
        };
        conversationPanelView.PeerClick += ConversationPanelView_PeerClick;

        viewLeft.Add(conversationPanelView);

        viewRight = new()
        {
            X = Pos.Right(viewLeft) + 2,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Visible = false
        };

        bubbleSelector = new("Select Bubble")
        {
            X = 0,
            Y = 0,
            Height = 1,
            Width = Dim.Fill()
        };
        bubbleSelector.SelectedItemUpdated += BubbleSelector_ItemSelected;

        bubblePanel = new(rbApplication, null)
        {
            X = 0,
            Y = Pos.Bottom(bubbleSelector),
            Height = Dim.Fill(),
            Width = Dim.Fill()
        };
        viewRight.Add(bubbleSelector, bubblePanel);

        Add(viewLeft, viewRight);

        CanFocus = true;
    }

    private void ConversationPanelView_PeerClick(object? sender, PeerAndMouseEventArgs e)
    {
        if( (e.Peer.Type == EntityType.Bubble) && (bubbleSelector is not null) )
        {
            var bubble = rbBubbles.GetBubbleById(e.Peer.Id);
            bubbleSelector.SetItemSelected(bubble);
        }
    }

    private void BubbleSelector_ItemSelected(object? sender, Item item)
    {
        var bubble = rbBubbles.GetBubbleById(item.Id);
        bubblePanel?.SetBubble(bubble);

        var conversation = rbConversations.GetConversation(bubble);
        if (conversation is not null)
            conversationPanelView?.SetSelectedConversation(conversation);
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
                    Title = rbAccount.Prefix;

                    loginView.Visible = false;
                    viewRight.Visible = true;
                    viewLeft.Visible = true;

                    List<String> bubbleMemberStatusList = [BubbleMemberStatus.Accepted];
                    var bubbles = rbBubbles.GetAllBubbles(bubbleMemberStatusList);
                    bubbleSelector?.SetItems(bubbles?.Select(b => b.Peer).ToList());
                    break;

                case ConnectionStatus.Connecting:
                    Title = $"{rbAccount.Prefix} - [Connecting]";
                    break;

                case ConnectionStatus.Disconnected:
                    Title = $"{rbAccount.Prefix} - [Disconnected]";

                    loginView.Visible = true;
                    viewRight.Visible = false;
                    viewLeft.Visible = false;
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
