using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;
using System.Collections.Immutable;
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

    private void UpdateDisplay()
    {
        lock (lockDisplay)
        {
            // Set default display
            
            lblOrganizers.Height = 0;
            lblMembers.Height = 0;
            lblMembers.Y = 0;

            // Remove previous BubbleMemberView
            foreach (var bubbleMemberView in bubbleMembersView.Values)
            {
                Remove(bubbleMemberView);
            }
            bubbleMembersView.Clear();

            if (bubble is null)
                return;

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

                    var bubbleMemberView = new BubbleMemberView(rbApplication, bubble, owner)
                    {
                        X = 0,
                        Y = Pos.Bottom(previousView),
                        Width = Dim.Fill()
                    };
                    Add(bubbleMemberView);
                    bubbleMembersView.Add(owner.Peer.Id, bubbleMemberView);

                    previousView = bubbleMemberView;
                }

                // Manage Moderators
                var moderators = bubbleMembers.Where(m => m.Privilege == BubbleMemberPrivilege.Moderator).ToList();
                if (moderators is not null)
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
                        var bubbleMemberView = new BubbleMemberView(rbApplication, bubble, moderator)
                        {
                            X = 0,
                            Y = Pos.Bottom(previousView),
                            Width = Dim.Fill()
                        };
                        Add(bubbleMemberView);
                        bubbleMembersView.Add(moderator.Peer.Id, bubbleMemberView);
                        previousView = bubbleMemberView;
                    }
                }

                // Manage std members
                var stdMembers = bubbleMembers.Where(m => m.Privilege == BubbleMemberPrivilege.User).ToList();
                if (stdMembers is not null)
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
                        var bubbleMemberView = new BubbleMemberView(rbApplication, bubble, stdMember)
                        {
                            X = 0,
                            Y = Pos.Bottom(previousView),
                            Width = Dim.Fill()
                        };
                        Add(bubbleMemberView);
                        bubbleMembersView.Add(stdMember.Peer.Id, bubbleMemberView);
                        previousView = bubbleMemberView;
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