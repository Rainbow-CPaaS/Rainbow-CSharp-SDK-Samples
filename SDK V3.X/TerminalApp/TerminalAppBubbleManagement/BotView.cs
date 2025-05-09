﻿using Rainbow;
using Rainbow.Consts;
using Rainbow.Example.Common;
using Terminal.Gui;

internal class BotView: View
{
    internal UserConfig rbAccount;

    private readonly Rainbow.Application rbApplication;
    private readonly AutoReconnection rbAutoReconnection;
    private readonly Contacts rbContacts;
    private readonly Bubbles rbBubbles;

    private LoginView? loginView;
    private ItemSelector? bubbleSelector;
    private Label? lblBubble;
    private Label? lblBubbleSelector;
    private Label? lblBubbleSelectorArrow;
    private BubbleMembersPanel? bubbleMembersPanel;

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

        bubbleSelector = new("Select Bubble")
        {
            X = Pos.Center(),
            Y = 1,
            Height = 3,
            Width = 50,
            Visible = false
        };
        bubbleSelector.SelectedItemUpdated += BubbleSelector_ItemSelected;

        bubbleMembersPanel = new(rbApplication, null)
        {
            X = Pos.Center(),
            Y = Pos.Bottom(bubbleSelector),
            Height = Dim.Fill(1),
            Width = 50,
            Visible = false
        };
        Add(bubbleSelector, bubbleMembersPanel);
    }

    private void BubbleSelector_ItemSelected(object? sender, Item item)
    {
        bubbleMembersPanel?.SetBubble(rbBubbles.GetBubbleById(item.Id));
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
                    Title = rbAccount.Prefix;

                    loginView.Visible = false;
                    bubbleSelector.Visible = true;
                    bubbleMembersPanel.Visible = true;

                    var bubbles = rbBubbles.GetAllBubbles();
                    if (bubbles?.Count() > 0)
                    {
                        bubbleSelector.SetItems(bubbles.Select(b => b.Peer).ToList());
                        bubbleMembersPanel.SetBubble(bubbles[0]);
                    }
                    break;

                case ConnectionStatus.Connecting:
                    Title = $"{rbAccount.Prefix} - [Connecting]";
                    break;

                case ConnectionStatus.Disconnected:
                    Title = $"{rbAccount.Prefix} - [Disconnected]";

                    loginView.Visible = true;
                    bubbleSelector.Visible = false;
                    bubbleMembersPanel.Visible = false;
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
