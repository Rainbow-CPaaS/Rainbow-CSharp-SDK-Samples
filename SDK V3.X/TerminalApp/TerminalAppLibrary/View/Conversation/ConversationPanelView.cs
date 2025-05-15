using Rainbow;
using Rainbow.Model;
using Terminal.Gui;


public class ConversationPanelView: View
{
    private Rainbow.Application rbApplication;
    private Conversations rbConversations;

    private List<Conversation>? conversationsList;
    private Conversation? selectedConversation;
    private readonly Dictionary<String, ConversationItemView> conversationItemViewList;

    private Boolean activeConversationsOpened = false;
    private Label lblActiveConversations;
    private ScrollableView scrollableViewActiveConversations;

    private Boolean recentConversationsOpened = true;
    private Label lblRecentConversations;
    private ScrollableView scrollableViewRecentConversations;

    private ConversationItemView? selectedConversationItemView = null;

    public event EventHandler<PeerAndMouseEventArgs>? PeerClick;

    public ConversationPanelView(Rainbow.Application rbApplication)
    {
        this.rbApplication = rbApplication;

        rbConversations = rbApplication.GetConversations();

        rbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;

        rbConversations.ConversationCreated += RbConversations_ConversationRemovedOrCreated;
        rbConversations.ConversationRemoved += RbConversations_ConversationRemovedOrCreated;
        rbConversations.ConversationUpdated += RbConversations_ConversationUpdated;

        //Get all conversations
        conversationsList = rbConversations.GetAllConversations();

        conversationItemViewList = [];

        CanFocus = true;

        /*
        lblActiveConversations = new()
        {
            X = 0,
            Y = 0,
            ColorScheme = Tools.ColorSchemeBlueOnGray,
            Height = 0,
        };
        scrollableViewActiveConversations = new()
        {
            X = 0,
            Y = Pos.Bottom(lblActiveConversations),
        };
        scrollableViewActiveConversations.VerticalScrollBar.AutoShow = true;
        */
        
        lblRecentConversations = new()
        {
            X = 0,
            Y = 0,
            //ColorScheme = Tools.ColorSchemeBlueOnGray,
            Title = "Recent Conversations",
            BorderStyle = LineStyle.Dotted,
            CanFocus = true,
            Height = Dim.Fill(),
            Width = Dim.Fill()
        };
        lblRecentConversations.Border.Add(Tools.VerticalExpanderButton());

        scrollableViewRecentConversations = new()
        {
            X = 0,
            Y = 0,
        };
        scrollableViewRecentConversations.VerticalScrollBar.AutoShow = true;

        lblRecentConversations.Add(scrollableViewRecentConversations);

        Add(lblRecentConversations);

        Width = Dim.Fill();
        Height = Dim.Fill();

        UpdateDisplay();
    }

    public Boolean SetSelectedConversation(Conversation? conversation)
    {
        if (conversation is null)
            return false;

        if (selectedConversationItemView?.conversation?.Peer?.Id != conversation.Peer.Id)
        {
            selectedConversationItemView?.SetSelected(false);

            if (conversationItemViewList.TryGetValue(conversation.Peer.Id, out var conversationItemView))
            {
                conversationItemView.SetSelected(true);
                selectedConversationItemView = conversationItemView;

                return true;
            }
        }

        return false;
    }

    public void UpdateDisplay()
    {
        if(this.conversationsList is not null)
        {
            //Remove previous ConversationItemView
            foreach (var conversationItemView in conversationItemViewList.Values)
            {
                conversationItemView.PeerClick -= ConversationItemView_PeerClick;
                Remove(conversationItemView);
                //conversationItemView?.Dispose();
            }
            conversationItemViewList.Clear();
        }

        // Store new list
        this.conversationsList = rbConversations.GetAllConversations();

        View previousView;
        if (conversationsList?.Count > 0)
        {
            //lblRecentConversations.Height = 1;

            previousView = null;
            int count = 0;
            int height = 2;
            foreach (var conversation in conversationsList)
            {
                var newView = AddConversationItemView(previousView, conversation);
                if (newView is not null)
                {
                    count++;
                    var line = new LineView(Orientation.Horizontal)
                    {
                        X = 0,
                        Y = Pos.Bottom(newView),
                        Width = Dim.Fill(1),
                        ColorScheme = Tools.ColorSchemeDarkGrayOnGray
                    };
                    scrollableViewRecentConversations.Add(line);
                    previousView = line;

                }
            }

            var nbElements = count * (height + 1);
            scrollableViewRecentConversations.SetNbVerticalElements(nbElements);
        }
        else
        {
        }
    }

    private ConversationItemView? AddConversationItemView(View? previousView, Conversation conversation, Boolean? isSelected = null)
    {
        if (conversation is null)
            return null;
        var conversationItemView = new ConversationItemView(rbApplication, conversation, isSelected)
        {
            X = 0,
            Y = previousView is null ? 0: Pos.Bottom(previousView),
            Width = Dim.Fill(2),
        };
        conversationItemView.PeerClick += ConversationItemView_PeerClick;
        scrollableViewRecentConversations.Add(conversationItemView);
        conversationItemViewList.Add(conversation.Peer.Id, conversationItemView);
        return conversationItemView;
    }

    private void ConversationItemView_PeerClick(object? sender, PeerAndMouseEventArgs e)
    {
        // TODO

        switch(e.MouseEvent.Flags)
        {
            case MouseFlags.Button3Clicked: // Right click

                break;

            case MouseFlags.Button1Clicked: // Left click

                var conversation = rbConversations.GetConversation(e.Peer);
                if(SetSelectedConversation(conversation))
                    PeerClick?.Invoke(this, e);
                break;
        }
        e.MouseEvent.Handled = true;
    }

    private void RbConversations_ConversationRemovedOrCreated(Conversation conversation)
    {
        Terminal.Gui.Application.Invoke(() =>
        {
            UpdateDisplay();
        });
    }

    private void RbConversations_ConversationUpdated(Conversation conversation)
    {
        //TODO - it's possible to enhance that with a re-order of the display

        // Here we display all the list
        RbConversations_ConversationRemovedOrCreated(conversation);

    }

    private void RbApplication_ConnectionStateChanged(ConnectionState connectionState)
    {
        Terminal.Gui.Application.Invoke(() =>
        {
            UpdateDisplay();
        });
    }
}
