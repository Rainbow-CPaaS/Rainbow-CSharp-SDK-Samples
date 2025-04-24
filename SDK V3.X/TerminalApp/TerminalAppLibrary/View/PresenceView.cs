using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;
using Terminal.Gui;
using static System.Net.Mime.MediaTypeNames;

public class PresenceView: View
{
    private Label labelField;
    private PresenceColorView? presenceColorView;

    private Rainbow.Application rbApplication;
    private Contacts rbContacts;
    private Invitations rbInvitations;
    
    internal Contact? contact;
    internal Bubble? bubble;

    public event EventHandler<PeerAndMouseEventEventArgs>? PeerClick;

    public PresenceView(Rainbow.Application application, Bubble? bubble)
    {
        InitPresenceView(application);
        SetBubble(bubble);
    }

    public PresenceView(Rainbow.Application application, Contact? contact)
    {
        InitPresenceView(application);
        SetContact(contact);
    }

    private void InitPresenceView(Rainbow.Application application)
    {
        rbApplication = application;
        rbContacts = rbApplication.GetContacts();
        rbInvitations = rbApplication.GetInvitations();

        labelField = new()
        {
            X = 2,
            Y = 0,
            Width = Dim.Fill(),
            Height = 1,
            TextAlignment = Alignment.Start
        };

        Width = Dim.Fill();
        Height = 1;

        Add(labelField);
        MouseClick += View_MouseClick;
        labelField.MouseClick += View_MouseClick;
    }

    private void View_MouseClick(object? sender, MouseEventArgs e)
    {
        if (contact != null)
            PeerClick?.Invoke(sender, new PeerAndMouseEventEventArgs(contact, e));
    }

    internal void SetContact(Contact? contact)
    {
        if (bubble is not null)
        {
            Remove(presenceColorView);
            presenceColorView = null;
        }
        bubble = null;

        if (contact is not null)
        {
            var displayName = contact?.Peer?.DisplayName;
            var currentUser = (rbContacts.GetCurrentContact().Peer.Id == contact?.Peer.Id);
            presenceColorView = new(currentUser, false);

            // Need to check event ContactAggregatedPresenceUpdated ?
            if (this.contact is null)
                rbContacts.ContactAggregatedPresenceUpdated += RbContacts_ContactAggregatedPresenceUpdated;

            this.contact = contact;
            Add(presenceColorView);
            presenceColorView.MouseClick += View_MouseClick;
            UpdateDisplay();
        }
        else
        {
            Remove(presenceColorView);
            presenceColorView = null;
        }        
    }

    internal void SetBubble(Bubble? bubble)
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
            UpdateDisplay();
        }
        else
        {
            Remove(presenceColorView);
            presenceColorView = null;
        }
    }

    internal String GetDisplayName(Peer peer)
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
                        labelField.ColorScheme = Tools.ColorSchemePresencePanel;
                        labelField.Text = $"{Emojis.CALENDAR} {GetDisplayName(contact.Peer)}";
                        return;
                    }
                }
            }

            labelField.ColorScheme = Tools.ColorSchemeMain;
            labelField.Text = GetDisplayName(contact.Peer);
        }
        else if(bubble is not null)
        {
            presenceColorView.SetPresence(null);
            labelField.ColorScheme = Tools.ColorSchemeMain;
            labelField.Text = GetDisplayName(bubble.Peer);
        }
        else
        {
            labelField.Text = "";
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

    ~PresenceView()
    {
        MouseClick -= View_MouseClick;
        labelField.MouseClick -= View_MouseClick;
        presenceColorView.MouseClick -= View_MouseClick;
    }
}
