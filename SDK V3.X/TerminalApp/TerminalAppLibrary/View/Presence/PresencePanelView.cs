﻿using Rainbow.Consts;
using Rainbow.Model;
using Terminal.Gui;

public class PresencePanelView: View
{
    private readonly Rainbow.Application rbApplication;
    private readonly Rainbow.Contacts rbContacts;
    private readonly Rainbow.Favorites rbFavorites;
    private readonly Rainbow.Invitations rbInvitations;
    private Contact? currentContact;

    private readonly Dictionary<string, PresenceView> presenceViewsUnused;

    private List<Contact>? contactsList;
    private readonly Boolean displayRoster;

    private readonly Object lockDisplay = new();

    private PopoverMenu contextMenu = new();

    private readonly Boolean displayTitle;
    private readonly Button? settings;
    private int nbColumns;

    private int forTest_count = 0;

    public PresencePanelView(Rainbow.Application rbApplication, int nbColumns = 4, Boolean displayTitle = true, Boolean rosterOnly = true)
    {
        this.rbApplication = rbApplication;

        rbContacts = rbApplication.GetContacts();
        rbFavorites = rbApplication.GetFavorites();
        rbInvitations = rbApplication.GetInvitations();

        currentContact = rbContacts.GetCurrentContact();

        presenceViewsUnused = [];

        // ask at least once Favorites list
        var _ = rbFavorites.GetFavoritesAsync();

        this.displayTitle = displayTitle;
        displayRoster = rosterOnly;

        if (displayTitle)
        {
            if(displayRoster)
                Title = $"Roster";
            else
                Title = $"Contacts";
        }
        if (displayRoster)
            ColorScheme = Tools.ColorSchemePresencePanel;
        else
            ColorScheme = Tools.ColorSchemeContactsPanel;

        BorderStyle = LineStyle.Dotted;
        Height = Dim.Fill();
        Width = Dim.Fill();

        settings = new()
        {
            X = Pos.Center(),
            Y = 0,
            Text = "Settings",
            ShadowStyle = ShadowStyle.None
        };
        settings.MouseClick += Settings_MouseClick;
        Add(settings);

        
        if (displayRoster)
            contactsList = rbContacts.GetAllContactsInRoster();
        else
            contactsList = rbContacts.GetAllContacts();

        rbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;

        if (!displayRoster)
            rbContacts.ContactsAdded    += ContactsListUpated;
        rbContacts.RosterContactsAdded  += ContactsListUpated;
        rbContacts.RosterContactsRemoved+= ContactsListUpated;

        if(!displayRoster)
        {
            // We have to manage events from Invitations service
            rbInvitations.InvitationCancelled += RbInvitations_InvitationCancelled;
            rbInvitations.InvitationSent += RbInvitations_InvitationSent;
        }

        Update(contactsList, nbColumns);
    }

    private void RbApplication_ConnectionStateChanged(ConnectionState connectionState)
    {
        if(connectionState.Status == ConnectionStatus.Connected)
            currentContact = rbContacts.GetCurrentContact();

        ContactsListUpated(null);
    }

    private void ContactsListUpated(List<Contact> contacts)
    {
        if (displayRoster)
            contactsList = rbContacts.GetAllContactsInRoster();
        else
            contactsList = rbContacts.GetAllContacts();

        Terminal.Gui.Application.Invoke(() =>
        {
            Update(contactsList, nbColumns);
        });
    }

    private void RbInvitations_InvitationCancelled(Rainbow.Model.Invitation invitation)
    {
        ContactsListUpated(null);
    }

    private void RbInvitations_InvitationSent(Invitation invitation)
    {
        currentContact ??= rbContacts.GetCurrentContact();
        if (invitation.InvitingUserId == currentContact?.Peer?.Id)
            ContactsListUpated(null);
    }

    private void Settings_MouseClick(object? sender, MouseEventArgs e)
    {
        e.Handled = true;

        List<MenuItemv2> menuItems = [];
        for (int i = 0; i < 8; i++)
        {
            int help = i+1;
            var menuItem = new MenuItemv2(
                                $"On {help} Column{((help == 1) ? "" : "s")}",
                                $""
                                , () => Update(help)
                                );
            menuItems.Add(menuItem);
        }

        contextMenu = new(menuItems);
        contextMenu.MakeVisible(e.ScreenPosition);
    }

    public void Update(int nbColumns)
    {
        currentContact ??= rbContacts.GetCurrentContact();
        if (nbColumns != this.nbColumns)
            Update(contactsList, nbColumns);
    }

    public void Update(List<Contact>? contactsList, int nbColumns)
    {
        lock (lockDisplay)
        {
            // Store new info
            this.contactsList = contactsList;
            this.nbColumns = nbColumns;

            currentContact ??= rbContacts.GetCurrentContact();
            if (currentContact is null)
                return;

            forTest_count++;

            // Avoid to display the current contact
            if (contactsList != null)
            {
                var contact = contactsList.FirstOrDefault(x => x.Peer.Id == currentContact.Peer.Id);
                if(contact != null)
                    contactsList.Remove(contact);
            }

            if ((contactsList == null) || (contactsList.Count == 0))
            {
                if (displayTitle)
                {
                    if (displayRoster)
                        Title = $"Roster (none)";
                    else
                        Title = $"Contacts (none)";
                }
                return;
            }

            // Remove previous view
            //List<View> subViews = [.. Subviews];
            var subViews = SubViews.ToList();
            while (subViews.Count > 0)
            {
                var v = subViews[0];
                subViews.RemoveAt(0);

                if (v != settings)
                {
                    if (v is PresenceView presenceView)
                    {
                        presenceView.Visible = false;
                        presenceViewsUnused[presenceView.contact.Peer.Id] = presenceView;
                        presenceView.PeerClick -= PresenceView_ContactClick;
                    }
                    else
                        Remove(v);
                }
            }

            if (nbColumns < 1)
                nbColumns = 1;

            int nbContacts = contactsList.Count;
            int splitBy = (int)Math.Ceiling((double)nbContacts / nbColumns);

            int alreadyDisplayed = 0;

            int widthColumn = (int)Math.Ceiling((double)100 / nbColumns);

            Dim? percent = Dim.Percent((int)Math.Ceiling((double)100 / nbColumns), DimPercentMode.ContentSize);
            Dim dimWidth = (percent == null) ? 0 : percent - 1;

            if (displayTitle)
            {
                if(displayRoster)
                    Title = $"Roster ({contactsList.Count})";
                else
                    Title = $"Contacts ({contactsList.Count})";
            }
            BorderStyle = LineStyle.RoundedDotted;
            Height = Dim.Fill();
            Width = Dim.Fill();


            for (int col = 0; col < nbColumns; col++)
            {
                int splitByToUse = splitBy;
                // We need to have at least one contact by column
                if ( ( nbContacts - ((col+1) * splitBy) - ((nbColumns - col - 1) * (splitBy - 1))) < 0 )
                    splitByToUse--;

                var list = contactsList.Skip(alreadyDisplayed).Take(splitByToUse).ToList();
                alreadyDisplayed += splitByToUse;

                Pos posX;
                if (col == 0)
                    posX = 0;
                else
                    posX = Pos.Percent(widthColumn * col);

                int index = (settings == null) ? 0 : 1;

                foreach (var contact in list)
                {
                    if (contact != null)
                    {

                        Boolean newView = false;
                        if (!presenceViewsUnused.TryGetValue(contact.Peer.Id, out PresenceView? view))
                        {
                            view = new(rbApplication, contact);
                            newView = true;
                        }
                        else
                        {
                            presenceViewsUnused.Remove(contact.Peer.Id);
                            view.Visible = true;
                        }

                        view.X = posX + 1;
                        view.Y = index;
                        view.Width = dimWidth;
                        view.Height = 1;

                        index++;

                        view.PeerClick += PresenceView_ContactClick;

                        if (newView)
                            Add(view);

                        // If a contact is not in the roster, we need to update presence display to display correctly invitation status
                        if (!contact.InRoster)
                            view.UpdateDisplay();
                    }
                }

                if (col > 0)
                {
                    var line = new LineView(Orientation.Vertical)
                    {
                        Y = (settings == null) ? 0 : 1,
                        X = posX
                    };
                    Add(line);
                }
            }

            
        }
    }

    private void PresenceView_ContactClick(object? sender, PeerAndMouseEventArgs e)
    {
        if (e.MouseEvent.Flags == MouseFlags.Button1DoubleClicked)
        {
            e.MouseEvent.Handled = true;
            Tools.DisplayPresenceDetails(rbApplication, e.Peer);
        }

        if (e.MouseEvent.Flags == MouseFlags.Button3Clicked)
        {
            e.MouseEvent.Handled = true;
            DisplayContextMenu(e.Peer);
        }
    }

    private void DisplayContextMenu(Peer peer)
    {
        // Dispose previous context menu
        contextMenu?.Dispose();

        List<MenuItemv2> menuItems = [];

        var contact = rbContacts.GetContact(peer);

        MenuItemv2 menuItem;
        var sentInvitation = rbInvitations.GetSentInvitation(peer);
        if( (sentInvitation != null) && (sentInvitation.Status != InvitationStatus.Pending))
            sentInvitation = null;

        var receivedInvitation = rbInvitations.GetReceivedInvitation(peer);

        if (contact.InRoster && (sentInvitation==null) )
        {
            menuItem = new (
                                "Remove for my network",
                                ""
                                , () => { Application.Popover?.Hide(contextMenu); RemoveForRoster(peer); }
                                , key: Key.N
                                );
        }
        else
        {
            if (sentInvitation == null)
            {
                menuItem = new(
                                    "Invite to join my network",
                                    ""
                                    , () => { Application.Popover?.Hide(contextMenu); InviteToNetwork(peer); }
                                    , key: Key.N
                                    );
            }
            else
            {
                menuItem = new(
                                    "Cancel network's invitation",
                                    ""
                                    , () => { Application.Popover?.Hide(contextMenu); CancelInvitationToNetwork(peer); }
                                    , key: Key.N
                                    );
            }
            // 
        }


        menuItems.Add(menuItem);

        menuItems.Add(null);

        var favorite = rbFavorites.GetFavorite(peer);
        menuItem = new (
                    (favorite == null) ? "Add to favorites" : "Remove from favorites",
                    $""
                    , () => { Application.Popover?.Hide(contextMenu); AddOrRemoveFavorite(peer); }
                    , key: Key.F
                    );
        menuItems.Add(menuItem);

        contextMenu = new(menuItems);
        contextMenu.MakeVisible(BotWindow.MousePosition);
        
    }

    private void AddOrRemoveFavorite(Peer peer)
    {
        var favorite = rbFavorites.GetFavorite(peer);
        if (favorite != null)
        {
            var _ = rbFavorites.DeleteFavoriteAsync(favorite);
        }
        else
        {
            var _ = rbFavorites.CreateFavoriteAsync(peer);
        }
    }

    private void RemoveForRoster(Peer peer)
    {
        var _ = rbContacts.RemoveContactFromRosterAsync(peer);
    }

    private void InviteToNetwork(Peer peer)
    {
        var _ = rbInvitations.SendInvitationAsync(peer);
    }

    private void CancelInvitationToNetwork(Peer peer)
    {
        var invitation = rbInvitations.GetSentInvitation(peer);
        var _ = rbInvitations.CancelInvitationAsync(invitation);
    }
}
