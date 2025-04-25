using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;
using Terminal.Gui;

public class BubbleMembersPanel: View
{
    public const String LBL_ORGANIZER = "ORGANIZER";
    public const String LBL_ORGANIZERS = "ORGANIZERS";
    public const String LBL_MEMBER = "MEMBER";
    public const String LBL_MEMBERS = "MEMBERS";

    private readonly Rainbow.Application rbApplication;
    private readonly Comparers rbComparers;
    private readonly Bubbles rbBubbles;

    private Bubble? bubble;
    private Contact? currentUser;

    private PopoverMenu? contextMenu;
    private Boolean isModerator;

    private readonly Object lockDisplay = new();
    private readonly Label lblOrganizers;
    private readonly Label lblMembers;
    private readonly Dictionary<String, BubbleMemberView> bubbleMembersView;

    public BubbleMembersPanel(Rainbow.Application application, Bubble? bubble)
    {
        rbApplication = application;
        rbComparers = application.GetComparers();
        rbBubbles = rbApplication.GetBubbles();

        rbBubbles.BubbleMemberUpdated += RbBubbles_BubbleMemberUpdated;
        rbBubbles.BubbleInfoUpdated += RbBubbles_BubbleInfoUpdated;

        Arrangement = ViewArrangement.LeftResizable;
        VerticalScrollBar.AutoShow = true;

        bubbleMembersView = [];

        lblOrganizers = new()
        {
            X = 0,
            Y = 0,
            Text = LBL_ORGANIZERS,
            Height = 0
        };

        lblMembers = new()
        {
            X = 0,
            Y = 0,
            Text = LBL_MEMBERS,
            Height = 0
        };

        Add(lblOrganizers, lblMembers);

        Height = Dim.Auto(DimAutoStyle.Content);
        Width = Dim.Auto(DimAutoStyle.Content);

        BorderStyle = LineStyle.Dotted;
        CanFocus = true;

        SetBubble(bubble);
    }

    private BubbleMemberView AddBubbleMemberView(View previousView, BubbleMember bubbleMember)
    {
        var bubbleMemberView = new BubbleMemberView(rbApplication, bubble, bubbleMember)
        {
            X = 0,
            Y = Pos.Bottom(previousView),
            Width = Dim.Fill()
        };
        bubbleMemberView.PeerClick += BubbleMemberView_PeerClick;
        Add(bubbleMemberView);
        bubbleMembersView.Add(bubbleMember.Peer.Id, bubbleMemberView);
        return bubbleMemberView;
    }

    private void UpdateDisplay()
    {
        lock (lockDisplay)
        {
            currentUser ??= rbApplication.GetContacts().GetCurrentContact();

            // Set default display / value
            lblOrganizers.Height = 0;
            lblMembers.Height = 0;
            lblMembers.Y = 0;
            isModerator = false;

            // Remove previous BubbleMemberView
            foreach (var bubbleMemberView in bubbleMembersView.Values)
            {
                bubbleMemberView.PeerClick -= BubbleMemberView_PeerClick;
                Remove(bubbleMemberView);
            }
            bubbleMembersView.Clear();

            if (bubble is null)
                return;

            var o = rbBubbles.IsOwner(bubble);
            isModerator = o is not null && o.Value == true;
            if (!isModerator)
            {
                var m = rbBubbles.IsModerator(bubble);
                isModerator = m is not null && m.Value == true;
            }

            // Set title
            if (!String.IsNullOrEmpty(bubble.Peer?.DisplayName))
                Title = bubble.Peer.DisplayName;
            else
                Title = "";

            // Get members
            View? previousView = null;
            if (bubble.Users.Count > 0)
            {
                // Get BubbleMember list and sort it
                List<BubbleMember> bubbleMembers = bubble.Users.Values.ToList();
                bubbleMembers.Sort(rbComparers.BubbleMemberComparer);

                int nbOrganizers = 0;
                int nbStdMembers = 0;

                // Manage Owner
                var owner = bubbleMembers.FirstOrDefault(m => m.Privilege == BubbleMemberPrivilege.Owner);
                if (owner is not null)
                {
                    nbOrganizers++;
                    if (previousView is null)
                    {
                        lblOrganizers.Height = 1;
                        previousView = lblOrganizers;
                    }
                    previousView = AddBubbleMemberView(previousView, owner);
                }

                // Manage Moderators
                var moderators = bubbleMembers.Where(m => m.Privilege == BubbleMemberPrivilege.Moderator).ToList();
                if (moderators.Count > 0)
                {
                    // Display Label
                    if (previousView is null)
                    {
                        lblOrganizers.Height = 1;
                        previousView = lblOrganizers;
                    }
                    foreach(var moderator in moderators)
                    {
                        nbOrganizers++;
                        previousView = AddBubbleMemberView(previousView, moderator);
                    }
                }

                // Manage std members
                var stdMembers = bubbleMembers.Where(m => m.Privilege == BubbleMemberPrivilege.User).ToList();
                if (stdMembers.Count > 0)
                {
                    // Display label
                    lblMembers.Height = 1;
                    if (previousView is null)
                        lblMembers.Y = 0;
                    else
                        lblMembers.Y = Pos.Bottom(previousView)+1;
                    previousView = lblMembers;

                    foreach (var stdMember in stdMembers)
                    {
                        nbStdMembers++;
                        previousView = AddBubbleMemberView(previousView, stdMember);
                    }
                }

                if (nbOrganizers > 1)
                    lblOrganizers.Text = $"{LBL_ORGANIZERS} ({nbOrganizers})";
                else
                    lblOrganizers.Text = LBL_ORGANIZER;

                if (nbStdMembers > 1)
                    lblMembers.Text = $"{LBL_MEMBERS} ({nbStdMembers})";
                else
                    lblMembers.Text = LBL_MEMBER;
            }
        }
    }

    private void UpdateMember(BubbleMember bubbleMember, String action)
    {
        if (!isModerator)
            return;

        switch(action)
        {
            case "promote":
                var _1 = rbBubbles.UpdateMemberPrivilegeAsync(bubble, bubbleMember, BubbleMemberPrivilege.Moderator);
                break;

            case "demote":
                var _2 = rbBubbles.UpdateMemberPrivilegeAsync(bubble, bubbleMember, BubbleMemberPrivilege.User);
                break;

            case "remove":
                var _3 = rbBubbles.RemoveMemberAsync(bubble, bubbleMember);
                break;
        }
    }

    private void BubbleMemberView_PeerClick(object? sender, PeerAndMouseEventArgs e)
    {
        // Left click: do nothing
        // Right click:
        //  - if current user is a moderator:
        //      - promote / demote user
        //      - remove member

        if (!isModerator)
        { 
            e.MouseEvent.Handled = true;
            return;
        }

        // Right Click
        if(e.MouseEvent.Flags == MouseFlags.Button3Clicked)
        {
            if (bubble is not null && e.Peer is not null && e.Peer.Id != currentUser.Peer?.Id)
            {
                if (bubble.Users.TryGetValue(e.Peer.Id, out BubbleMember? bubbleMember))
                {
                    List<MenuItemv2> menuItems = [];

                    if(bubbleMember.Privilege == BubbleMemberPrivilege.User)
                    {
                        var item = new MenuItemv2(
                                            "Promote to organizer role"
                                            , ""
                                            , () => UpdateMember(bubbleMember, "promote")
                                            , key: new Key("p").NoShift
                                            );
                        menuItems.Add(item);
                    }
                    else
                    {
                        var item = new MenuItemv2(
                                            "Demote to member role"
                                            , ""
                                            , () => UpdateMember(bubbleMember, "demote")
                                            , key: new Key("d").NoShift
                                            );
                        menuItems.Add(item);
                    }

                    var menuItem = new MenuItemv2(
                                            "Remove member"
                                            , ""
                                            , () => UpdateMember(bubbleMember, "remove")
                                            , key: new Key("r").NoShift
                                            );
                    menuItems.Add(menuItem);

                    contextMenu = new(menuItems);
                    contextMenu.MakeVisible(e.MouseEvent.ScreenPosition);
                }
            }
        }
    }

    public void SetBubble(Bubble ? bubble)
    {
        Terminal.Gui.Application.Invoke(() =>
        {
            this.bubble = bubble;
            UpdateDisplay();
        });        
    }

    protected override bool OnMouseEvent(MouseEventArgs mouseEvent)
    {
        if (mouseEvent.Flags == MouseFlags.WheeledDown)
        {
            ScrollVertical(1);
            return mouseEvent.Handled = true;
        }

        if (mouseEvent.Flags == MouseFlags.WheeledUp)
        {
            ScrollVertical(-1);
            return mouseEvent.Handled = true;
        }
        return false;
    }

    private void RbBubbles_BubbleInfoUpdated(Bubble bubble)
    {
        Terminal.Gui.Application.Invoke(() =>
        {
            this.bubble = bubble;
            UpdateDisplay();
        });
    }

    private void RbBubbles_BubbleMemberUpdated(Bubble bubble, BubbleMember member)
    {
        Terminal.Gui.Application.Invoke(() =>
        {
            this.bubble = bubble;
            UpdateDisplay();
        });
    }
}