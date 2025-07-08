using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

public class BubbleMembersPanel: View
{
    public const String LBL_ORGANIZER = "ORGANIZER";
    public const String LBL_ORGANIZERS = "ORGANIZERS";
    public const String LBL_ROOM = "RAINBOW ROOM EQUIPMENT";
    public const String LBL_ROOMS = "RAINBOW ROOM EQUIPMENTS";
    public const String LBL_MEMBER = "MEMBER";
    public const String LBL_MEMBERS = "MEMBERS";
    public const String LBL_WAITING_ROOM = "IN WAITING ROOM";

    private readonly Rainbow.Application rbApplication;
    private readonly Comparers rbComparers;
    private readonly Contacts rbContacts;
    private readonly Bubbles rbBubbles;

    private Bubble? bubble;
    private Contact? currentUser;
    private ContactsInLobby? contactsInLobby;

    private PopoverMenu? contextMenu;
    private Boolean isOwner;
    private Boolean isModerator;

    private readonly Object lockDisplay = new();

    private readonly ScrollableView scrollableView;
    private readonly Dictionary<String, BubbleMemberView> bubbleMembersViewList;
    private readonly Label lblWaitingRoom;
    private readonly Label lblOrganizers;
    private readonly Label lblRooms;
    private readonly Label lblMembers;

    public BubbleMembersPanel(Rainbow.Application application, Bubble? bubble)
    {
        rbApplication = application;

        rbComparers = application.GetComparers();
        rbBubbles = rbApplication.GetBubbles();
        rbContacts = rbApplication.GetContacts();
        rbComparers.ContactComparerUpdated += RbComparers_ContactComparerUpdated;

        rbBubbles.BubbleMemberUpdated += RbBubbles_BubbleMemberUpdated;
        rbBubbles.BubbleInfoUpdated += RbBubbles_BubbleInfoUpdated;
        rbBubbles.BubbleLobbyUpdated += RbBubbles_BubbleLobbyUpdated;
        rbBubbles.UnknownContactsFound += RbBubbles_UnknownContactsFound;

        VerticalScrollBar.AutoShow = true;

        bubbleMembersViewList = [];

        scrollableView = new();
        scrollableView.VerticalScrollBar.AutoShow = true;

        lblWaitingRoom = new()
        {
            X = 0,
            Y = 0,
            Text = LBL_WAITING_ROOM,
            Height = 0
        };

        lblOrganizers = new()
        {
            X = 0,
            Y = 0,
            Text = LBL_ORGANIZERS,
            Height = 0
        };

        lblRooms = new()
        {
            X = 0,
            Y = 0,
            Text = LBL_ROOMS,
            Height = 0
        };

        lblMembers = new()
        {
            X = 0,
            Y = 0,
            Text = LBL_MEMBERS,
            Height = 0
        };

        scrollableView.Add(lblWaitingRoom, lblOrganizers, lblRooms, lblMembers);

        Add(scrollableView);

        Height = Dim.Fill();
        Width = Dim.Fill();

        //BorderStyle = LineStyle.Dotted;
        CanFocus = true;

        SetBubble(bubble);
    }

    private BubbleMemberView? AddBubbleMemberView(View previousView, BubbleMember bubbleMember)
    {
        if (bubble is null)
            return null;
        var bubbleMemberView = new BubbleMemberView(rbApplication, bubble, bubbleMember)
        {
            X = 0,
            Y = Pos.Bottom(previousView),
            Width = Dim.Fill(),
            Height = 1
        };
        bubbleMemberView.PeerClick += BubbleMemberView_PeerClick;
        scrollableView.Add(bubbleMemberView);
        bubbleMembersViewList.Add(bubbleMember.Peer.Id, bubbleMemberView);
        return bubbleMemberView;
    }

    private BubbleMemberView? AddBubbleMemberView(View previousView, Contact contact)
    {
        if (bubble is null)
            return null;
        var bubbleMemberView = new BubbleMemberView(rbApplication, bubble, contact)
        {
            X = 0,
            Y = Pos.Bottom(previousView),
            Width = Dim.Fill(),
            Height = 1
        };
        bubbleMemberView.PeerClick += BubbleMemberView_PeerClick;
        scrollableView.Add(bubbleMemberView);
        bubbleMembersViewList.Add(contact.Peer.Id, bubbleMemberView);
        return bubbleMemberView;
    }

    private Boolean IsOwner()
    {
        isOwner = Tools.IsOwner(rbApplication, bubble);
        return isOwner;
    }

    private Boolean IsModerator()
    {
        isModerator = Tools.IsModerator(rbApplication, bubble);
        return isModerator;
    }

    private void UpdateDisplay()
    {
        lock (lockDisplay)
        {
            currentUser ??= rbApplication.GetContacts().GetCurrentContact();

            // Set default display / value
            lblWaitingRoom.Height = 0;
            lblOrganizers.Height = 0;
            lblRooms.Height = 0;
            lblRooms.Y = 0;
            lblMembers.Height = 0;
            lblMembers.Y = 0;

            // Remove previous BubbleMemberView
            foreach (var bubbleMemberView in bubbleMembersViewList.Values)
            {
                bubbleMemberView.PeerClick -= BubbleMemberView_PeerClick;
                Remove(bubbleMemberView);
            }
            bubbleMembersViewList.Clear();

            if (bubble is null)
            {
                scrollableView.SetNbVerticalElements(0);
                return;
            }

            IsOwner();
            IsModerator();

            View? previousView = null;

            // Waiting room
            int nbInWaitingRoom = 0;
            if (contactsInLobby?.Contacts?.Count > 0)
            {
                if (previousView is null)
                {
                    lblWaitingRoom.Height = 1;
                    previousView = lblWaitingRoom;
                    lblWaitingRoom.Y = 0;
                }
                else
                    lblWaitingRoom.Y = Pos.Bottom(previousView) + 1;

                foreach (var contact in contactsInLobby.Contacts)
                {
                    var newView = AddBubbleMemberView(previousView, contact);
                    if (newView is not null)
                    {
                        nbInWaitingRoom++;
                        previousView = newView;
                    }
                }
            }

            // Get members
            if (bubble.Users.Count > 0)
            {
                // Get BubbleMember list and sort it
                List<BubbleMember> bubbleMembers = bubble.Users.Values.ToList();
                bubbleMembers.Sort(rbComparers.BubbleMemberComparer);

                int nbOrganizers = 0;
                int nbRooms = 0;
                int nbStdMembers = 0;

                var rooms = rbBubbles.GetTvs(bubble);
                var roomsId = rooms.Select(r => r.Peer.Id);

                // Manage Owner
                var owner = bubbleMembers.FirstOrDefault(m => m.Privilege == BubbleMemberPrivilege.Owner);
                if ( (owner is not null) && !roomsId.Contains(owner.Peer.Id))
                {
                    lblOrganizers.Height = 1;
                    if (previousView is null)
                    {
                        previousView = lblOrganizers;
                        lblOrganizers.Y = 0;
                    }
                    else
                        lblOrganizers.Y = Pos.Bottom(previousView) + 1;
                    previousView = lblOrganizers;

                    var newView = AddBubbleMemberView(previousView, owner);
                    if (newView is not null)
                    {
                        nbOrganizers++;
                        previousView = newView;
                    }
                }

                // Manage Moderators
                var moderators = bubbleMembers.Where(m => m.Privilege == BubbleMemberPrivilege.Moderator).ToList();
                // Remove rooms from moderators' list
                moderators = moderators.Where(m => !roomsId.Contains(m.Peer.Id)).ToList();
                if (moderators.Count > 0)
                {
                    lblOrganizers.Height = 1;
                    // Display Label
                    if (previousView is null)
                    {
                        previousView = lblOrganizers;
                        lblOrganizers.Y = 0;
                    }
                    else if(owner is null)
                    {
                        lblOrganizers.Y = Pos.Bottom(previousView) + 1;
                        previousView = lblOrganizers;
                    }
                    else
                    {
                        // Nothing to do
                    }

                    foreach (var moderator in moderators)
                    {
                        var newView = AddBubbleMemberView(previousView, moderator);
                        if (newView is not null)
                        {
                            nbOrganizers++;
                            previousView = newView;
                        }
                    }
                }

                // Manage Room
                if (rooms.Count > 0)
                {
                    lblRooms.Height = 1;
                    if (previousView is null)
                        lblRooms.Y = 0;
                    else
                        lblRooms.Y = Pos.Bottom(previousView) + 1;
                    previousView = lblRooms;

                    foreach (var room in rooms)
                    {
                        var newView = AddBubbleMemberView(previousView, room);
                        if (newView is not null)
                        {
                            nbRooms++;
                            previousView = newView;
                        }
                    }
                }

                // Manage std members
                var stdMembers = bubbleMembers.Where(m => m.Privilege == BubbleMemberPrivilege.User).ToList();
                // Remove rooms from stdMembers' list
                stdMembers = stdMembers.Where(m => !roomsId.Contains(m.Peer.Id)).ToList();
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
                        var newView = AddBubbleMemberView(previousView, stdMember);
                        if (newView is not null)
                        {
                            nbStdMembers++;
                            previousView = newView;
                        }
                    }
                }

                if (nbOrganizers > 1)
                    lblOrganizers.Text = $"{LBL_ORGANIZERS} ({nbOrganizers})";
                else
                    lblOrganizers.Text = LBL_ORGANIZER;

                if (nbRooms > 1)
                    lblRooms.Text = $"{LBL_ROOMS} ({nbRooms})";
                else
                    lblRooms.Text = LBL_ROOM;

                if (nbStdMembers > 1)
                    lblMembers.Text = $"{LBL_MEMBERS} ({nbStdMembers})";
                else
                    lblMembers.Text = LBL_MEMBER;

                var nbElements = ((nbOrganizers > 0) ? (1 + nbOrganizers) : 0)
                                  + ((nbRooms > 0) ? (2 + nbRooms) : 0)
                                  + ((nbStdMembers > 0) ? (2 + nbStdMembers) : 0);
                scrollableView.SetNbVerticalElements(nbElements);
            }
        }
    }

    private void UpdateMemberPrivilege(BubbleMember bubbleMember, String action)
    {
        if (!isModerator)
            return;

        switch(action)
        {
            case "owner":
                Task.Run(async () =>
                {
                    var sdkResult = await rbBubbles.CheckChangeOwnerAsync(bubble, bubbleMember);
                    if(sdkResult.Success) 
                    {
                        var _0 = rbBubbles.ChangeOwnerAsync(bubble, bubbleMember);
                    }
                });
                break;

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

    private void AcceptContact(Peer peer, Boolean accept)
    {
        if (accept)
        {
            var _ = rbBubbles.AcceptUsersInLobbyAsync(bubble, [peer.Id]);
        }
        else
        {
            var _ = rbBubbles.DenyUsersInLobbyAsync(bubble, [peer.Id]);
        }
    }

    public void SetBubble(Bubble ? bubble)
    {
        if( (bubble?.HasLobby == true) && IsModerator())
        {
            Task.Run(async () =>
            {
                var sdkResult = await rbBubbles.GetContactsInLobbyAsync(bubble);
                if (sdkResult.Success)
                {
                    contactsInLobby = sdkResult.Data;
                    if (contactsInLobby.Contacts?.Count > 0)
                    {
                        Terminal.Gui.App.Application.Invoke(() =>
                        {
                            this.bubble = bubble;
                            scrollableView.Viewport = scrollableView.Viewport with { Y = 0 };
                            UpdateDisplay();
                        });
                    }
                }
            });
        }
        else
            contactsInLobby = null;


        Terminal.Gui.App.Application.Invoke(() =>
        {
            this.bubble = bubble;
            scrollableView.Viewport = scrollableView.Viewport with { Y = 0 };
            UpdateDisplay();
        });        
    }

    private void BubbleMemberView_PeerClick(object? sender, PeerAndMouseEventArgs e)
    {
        // Left click: do nothing

        // Right click:
        //  - if current user is a moderator (and selected member is not owner)
        //      - promote / demote user
        //      - remove member
        //  - if current user is owner:
        //      - give ownership to a moderator

        if (!isModerator)
        {
            e.MouseEvent.Handled = true;
            return;
        }

        // Right Click
        if (e.MouseEvent.Flags == MouseFlags.Button3Clicked)
        {
            if (bubble is not null && e.Peer is not null && e.Peer.Id != currentUser?.Peer?.Id)
            {
                // Cannot manage member if bublle is associated to a Hub Telephony Group
                if(bubble.IsOwnedByGroup)
                {
                    e.MouseEvent.Handled = true;
                    return;
                }

                if (bubble.Users.TryGetValue(e.Peer.Id, out BubbleMember? bubbleMember))
                {
                    List<MenuItemv2> menuItems = [];
                    MenuItemv2 item;

                    if (bubbleMember.Privilege == BubbleMemberPrivilege.User)
                    {
                        item = new MenuItemv2(
                                            "Promote to organizer role"
                                            , ""
                                            , () => UpdateMemberPrivilege(bubbleMember, "promote")
                                            , key: new Key("p").NoShift
                                            );
                        menuItems.Add(item);
                    }
                    else if (bubbleMember.Privilege == BubbleMemberPrivilege.Moderator)
                    {
                        if (isOwner)
                        {
                            item = new MenuItemv2(
                                            "Give Ownership"
                                            , ""
                                            , () => UpdateMemberPrivilege(bubbleMember, "owner")
                                            , key: new Key("o").NoShift
                                            );
                            menuItems.Add(item);
                        }
                        else
                        {
                            item = new MenuItemv2(
                                                "Demote to member role"
                                                , ""
                                                , () => UpdateMemberPrivilege(bubbleMember, "demote")
                                                , key: new Key("d").NoShift
                                                );
                            menuItems.Add(item);
                        }
                    }

                    if (bubbleMember.Privilege != BubbleMemberPrivilege.Owner)
                    {
                        var menuItem = new MenuItemv2(
                                            "Remove member"
                                            , ""
                                            , () => UpdateMemberPrivilege(bubbleMember, "remove")
                                            , key: new Key("r").NoShift
                                            );
                        menuItems.Add(menuItem);
                    }

                    if (menuItems.Count > 0)
                    {
                        contextMenu = new(menuItems);
                        contextMenu.MakeVisible(e.MouseEvent.ScreenPosition);
                    }
                }
                else
                {
                    // It's a user in the waiting room

                    List<MenuItemv2> menuItems = [];
                    MenuItemv2 item;

                    item = new MenuItemv2(
                                            "Deny"
                                            , ""
                                            , () => AcceptContact(e.Peer, false)
                                            , key: new Key("d").NoShift
                                            );
                    menuItems.Add(item);

                    item = new MenuItemv2(
                                            "Accept"
                                            , ""
                                            , () => AcceptContact(e.Peer, true)
                                            , key: new Key("a").NoShift
                                            );
                    menuItems.Add(item);

                    contextMenu = new(menuItems);
                    contextMenu.MakeVisible(e.MouseEvent.ScreenPosition);

                }
            }
        }

        e.MouseEvent.Handled = true;
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

    private void RbComparers_ContactComparerUpdated()
    {
        if (bubble is not null)
            SetBubble(bubble);
    }

    private void RbBubbles_UnknownContactsFound()
    {
        SetBubble(bubble);
    }

    private void RbBubbles_BubbleLobbyUpdated(ContactsInLobby contactsInLobby)
    {
        this.contactsInLobby = contactsInLobby;
        Terminal.Gui.App.Application.Invoke(() =>
        {
            UpdateDisplay();
        });
    }

    private void RbBubbles_BubbleInfoUpdated(Bubble bubble)
    {
        Terminal.Gui.App.Application.Invoke(() =>
        {
            this.bubble = bubble;
            UpdateDisplay();
        });
    }

    private void RbBubbles_BubbleMemberUpdated(Bubble bubble, BubbleMember member)
    {
        Terminal.Gui.App.Application.Invoke(() =>
        {
            this.bubble = bubble;
            UpdateDisplay();
        });
    }
}