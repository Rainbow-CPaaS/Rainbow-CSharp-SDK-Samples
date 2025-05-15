using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;
using Terminal.Gui;

public class PresenceView: View
{
    private Label labelDisplayName;
    private Label labelCompany;

    private PresenceColorView? presenceColorView;

    private Rainbow.Application rbApplication;
    private Contacts rbContacts;
    private Bubbles rbBubbles;
    private Invitations rbInvitations;

    internal String? currentCompanyId;
    internal Contact? contact;
    internal Bubble? bubble;

    public event EventHandler<PeerAndMouseEventArgs>? PeerClick;

#pragma warning disable CS8618
    public PresenceView(Rainbow.Application application)
#pragma warning restore CS8618
    {
        InitPresenceView(application);
    }

#pragma warning disable CS8618
    public PresenceView(Rainbow.Application application, Conversation? conversation)
#pragma warning restore CS8618
    {
        InitPresenceView(application);
        SetConversation(conversation);
    }

#pragma warning disable CS8618
    public PresenceView(Rainbow.Application application, Bubble? bubble)
#pragma warning restore CS8618
    {
        InitPresenceView(application);
        SetBubble(bubble);
    }

#pragma warning disable CS8618
    public PresenceView(Rainbow.Application application, Contact? contact)
#pragma warning restore CS8618
    {
        InitPresenceView(application);
        SetContact(contact);
    }

    private Boolean IsInSameCompany(Contact contact)
    {
        if (contact.CompanyName is null)
            return true;

        currentCompanyId ??= rbContacts.GetCompany()?.Id;
        return currentCompanyId is not null && (currentCompanyId == contact.CompanyId);
    }

    private void InitPresenceView(Rainbow.Application application)
    {
        rbApplication = application;
        rbContacts = rbApplication.GetContacts();
        rbBubbles = rbApplication.GetBubbles();
        rbInvitations = rbApplication.GetInvitations();

        labelDisplayName = new()
        {
            X = 2,
            Y = 0,
            Width = Dim.Auto(DimAutoStyle.Text),
            Height = 1,
            TextAlignment = Alignment.Start
        };

        labelCompany = new()
        {
            X = Pos.Right(labelDisplayName) + 1,
            Y = 0,
            Width = Dim.Auto(DimAutoStyle.Text),
            Height = 1,
            TextAlignment = Alignment.Start,
            ColorScheme = Tools.ColorSchemeDarkGrayOnGray
        };


        Width = Dim.Fill();
        Height = 1;

        Add(labelDisplayName, labelCompany);
        MouseClick += View_MouseClick;
        labelDisplayName.MouseClick += View_MouseClick;
    }

    private void View_MouseClick(object? sender, MouseEventArgs e)
    {
        if (contact != null)
            PeerClick?.Invoke(sender, new PeerAndMouseEventArgs(contact, e));
        else if (bubble != null)
            PeerClick?.Invoke(sender, new PeerAndMouseEventArgs(bubble, e));
    }

    public void SetContact(Contact? contact)
    {
        if (bubble is not null)
        {
            Remove(presenceColorView);
            presenceColorView = null;
        }
        bubble = null;

        if (contact is not null)
        {
            // Get contact from cache
            var newContact = rbContacts.GetContactById(contact.Peer?.Id);
            if(newContact is not null)
                contact = newContact;

            var displayName = contact.Peer?.DisplayName;
            var currentUser = (rbContacts.GetCurrentContact().Peer.Id == contact.Peer?.Id);
            presenceColorView = new(currentUser, false);

            // Need to check event ContactAggregatedPresenceUpdated ?
            if (this.contact is null)
                rbContacts.ContactAggregatedPresenceUpdated += RbContacts_ContactAggregatedPresenceUpdated;

            this.contact = contact;
            Add(presenceColorView);
            presenceColorView.MouseClick += View_MouseClick;
            Terminal.Gui.Application.Invoke(() =>
            {
                UpdateDisplay();
            });
        }
        else
        {
            Remove(presenceColorView);
            presenceColorView = null;
        }        
    }

    public void SetBubble(Bubble? bubble)
    {
        if (contact is not null)
        {
            rbContacts.ContactAggregatedPresenceUpdated -= RbContacts_ContactAggregatedPresenceUpdated;
            Remove(presenceColorView);
            presenceColorView = null;
        }
        contact = null;

        if (bubble is not null)
        {
            presenceColorView = new(false, true);
            this.bubble = bubble;
            Add(presenceColorView);
            presenceColorView.MouseClick += View_MouseClick;
            Terminal.Gui.Application.Invoke(() =>
            {
                UpdateDisplay();
            });
        }
        else
        {
            Remove(presenceColorView);
            presenceColorView = null;
        }
    }

    public void SetConversation(Conversation? conversation)
    {
        if (conversation?.Peer.Type == EntityType.Bubble)
        {
            var bubble = rbBubbles.GetBubble(conversation.Peer);
            SetBubble(bubble);
        }
        else if (conversation?.Peer.Type == EntityType.User)
        {
            var contact = rbContacts.GetContact(conversation.Peer);
            SetContact(contact);
        }
    }

    internal static String GetDisplayName(Peer peer)
    {
        if (String.IsNullOrEmpty(peer?.DisplayName))
            return "";
        else
            return peer.DisplayName;
    }

    internal void UpdateDisplay()
    {
        if (presenceColorView is null)
            return;

        if (contact is not null)
        {
            

            if (IsInSameCompany(contact))
            { 
                labelCompany.Text = contact.GuestMode ? "(is guest)" : "";
                    }
            else
                labelCompany.Text = $"({contact.CompanyName}{(contact.GuestMode ? " - is guest": "")})";

            if (!contact.InRoster)
            {
                var invitation = rbInvitations.GetSentInvitation(contact);
                if ((invitation != null) && (invitation.Status != InvitationStatus.Pending))
                    invitation = null;

                if (invitation == null)
                {
                    presenceColorView.SetPresence(null);
                }
                else
                    presenceColorView.SetInvitationInProgress();
            }
            else
            {

                Presence? aggregatedPresence;
                Dictionary<String, Presence> presences;
                aggregatedPresence = rbContacts.GetAggregatedPresence(contact);
                presences = rbContacts.GetPresencesList(contact);

                presenceColorView.SetPresence(aggregatedPresence);

                if (presences.TryGetValue("calendar", out var calendarPresence))
                {
                    if ((calendarPresence.PresenceLevel != PresenceLevel.Online) && calendarPresence.Until > DateTime.UtcNow)
                    {
                        labelDisplayName.ColorScheme = Tools.ColorSchemePresencePanel;
                        labelDisplayName.Text = $"{Emojis.CALENDAR} {GetDisplayName(contact.Peer)}";
                        return;
                    }
                }
            }

            labelDisplayName.ColorScheme = Tools.ColorSchemeMain;
            labelDisplayName.Text = GetDisplayName(contact.Peer);
            
        }
        else if(bubble is not null)
        {
            presenceColorView.SetPresence(null);
            labelDisplayName.ColorScheme = Tools.ColorSchemeMain;
            labelDisplayName.Text = GetDisplayName(bubble.Peer);
        }
        else
        {
            labelDisplayName.Text = "";
        }
    }

    private void RbContacts_ContactAggregatedPresenceUpdated(Presence presence)
    {
        if((contact is not null) && (presence.Contact?.Peer?.Id == contact.Peer?.Id))
        {
            contact.InRoster = (presence.PresenceLevel != PresenceLevel.Unavailable);

            Terminal.Gui.Application.Invoke(() =>
            {
                UpdateDisplay();
            });
        }
    }
}
