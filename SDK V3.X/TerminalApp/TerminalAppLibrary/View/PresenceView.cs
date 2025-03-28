using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;
using Terminal.Gui;

public class PresenceView: View
{
    private readonly Label labelField;
    private readonly PresenceColorView presenceColorView;

    private readonly Rainbow.Application rbApplication;
    private readonly Contacts rbContacts;
    private readonly Invitations rbInvitations;
    internal Contact contact;

    public event EventHandler<PeerAndMouseEventEventArgs>? ContactClick;

    public PresenceView(Rainbow.Application application, Contact contact)
    {
        rbApplication = application;
        rbContacts = rbApplication.GetContacts();
        rbInvitations = rbApplication.GetInvitations();

        Boolean currentUser = (rbContacts.GetCurrentContact().Peer.Id == contact?.Peer.Id);

        presenceColorView = new(currentUser);

        labelField = new()
        {
            X = Pos.Right(presenceColorView) + 1,
            Y = Pos.Left(presenceColorView),
            Width = Dim.Auto(DimAutoStyle.Text),
            Height = 1,
            TextAlignment = Alignment.Start,
            Text = contact?.Peer.DisplayName
        };

        Width = Dim.Auto(DimAutoStyle.Content);

        Add(presenceColorView, labelField);

        MouseClick                      += View_MouseClick;
        labelField.MouseClick           += View_MouseClick;
        presenceColorView.MouseClick    += View_MouseClick;

        this.contact = contact;
        SetPresence(); ;

        rbContacts.ContactAggregatedPresenceUpdated += RbContacts_ContactAggregatedPresenceUpdated;
    }

    private void View_MouseClick(object? sender, MouseEventEventArgs e)
    {
        if (contact != null)
            ContactClick?.Invoke(sender, new PeerAndMouseEventEventArgs(contact, e.MouseEvent));
    }

    internal void SetContact(Contact? contact)
    {
        this.contact = contact;
        SetPresence();
    }

    internal void SetPresence()
    {
        if (contact == null)
            return;

        if (!contact.InRoster == true)
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

            labelField.ColorScheme = Tools.ColorSchemeMain;
            labelField.Text = contact.Peer.DisplayName;
            
            return;
        }

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
                labelField.Text =  $"{Emojis.CALENDAR} {contact.Peer.DisplayName}";
                return;
            }
        }

        labelField.ColorScheme = Tools.ColorSchemeMain;
        labelField.Text = contact.Peer.DisplayName;
    }

    private void RbContacts_ContactAggregatedPresenceUpdated(Presence presence)
    {
        if (presence.Contact?.Peer?.Id == contact?.Peer.Id)
        {
            contact.InRoster = (presence.PresenceLevel != PresenceLevel.Unavailable);

            Terminal.Gui.Application.Invoke(() =>
            {
                SetPresence();
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
