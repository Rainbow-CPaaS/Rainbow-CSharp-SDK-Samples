using Terminal.Gui;

public class NotificationItemView: View
{
    public String InvitationContextId { get; private set; }

    private readonly Boolean _contactInvitation;
    private readonly String _invitingContactId;

    public event EventHandler<NotificationItemUpdatedEventArgs>? NotificationItemUpdated;

    public NotificationItemView(Rainbow.Application rbApplication, Boolean contactInvitation, String invitationContextId, String invitingContactId)
    {
        _contactInvitation = contactInvitation;
        InvitationContextId = invitationContextId;
        _invitingContactId = invitingContactId;

        List<Cell> cells = [];

        // Get display name of inviting user
        String invitingContactDisplayName;
        var rbContacts = rbApplication.GetContacts();
        var contact = rbContacts.GetContactByIdSync(invitingContactId);
        if (contact != null)
            invitingContactDisplayName = contact.Peer.DisplayName;
        else
            invitingContactDisplayName = invitingContactId;

        if (invitingContactDisplayName.Length > 30)
            invitingContactDisplayName = string.Concat(invitingContactDisplayName.AsSpan(0, 27), "...");


        cells = Tools.ToCellList(invitingContactDisplayName, Tools.ColorSchemeBlueOnGray);

        if (contactInvitation)
        {
            cells.AddRange(Tools.ToCellList(" would like to add you to their network.", Tools.ColorSchemeMain));
        }
        else
        {
            String bubbleDisplayName;
            var rbBubbles = rbApplication.GetBubbles();
            var bubble = rbBubbles.GetBubbleByIdSync(invitationContextId);
            if (bubble != null)
                bubbleDisplayName = bubble.Peer.DisplayName;
            else
                bubbleDisplayName = invitationContextId;

            if (bubbleDisplayName.Length > 30)
                bubbleDisplayName = string.Concat(bubbleDisplayName.AsSpan(0, 27), "...");

            cells.AddRange(Tools.ToCellList(" invites you to the bubble ", Tools.ColorSchemeMain));
            cells.AddRange(Tools.ToCellList(bubbleDisplayName, Tools.ColorSchemeGreenOnGray));
        }

        var label = new TextView()
        {
            X = Pos.Align(Alignment.Start),
            Y = 0,
            Multiline = false,
            ReadOnly = true,
            Height = 1,
            Width = Dim.Fill(25),
            ColorScheme = Tools.ColorSchemeMain,
        };
        label.Load(cells);

        var btnAccept = new Label()
        {
            X = Pos.Right(label),
            Y = 0,
            Height = 1,
            Width = Dim.Auto(DimAutoStyle.Text),
            Text = $" {Emojis.CHECKED_VIOLET}Accept ",
            ColorScheme = Tools.ColorSchemeBlackOnWhite,
        };
        btnAccept.MouseClick += BtnAccept_MouseClick;

        var btnDecline = new Label()
        {
            X = Pos.Right(btnAccept) + 2,
            Y = 0,
            Height = 1,
            Width = Dim.Auto(DimAutoStyle.Text),
            Text = $" {Emojis.CROSSED_RED}Decline ",
            ColorScheme = Tools.ColorSchemeBlackOnWhite,
        };
        btnDecline.MouseClick += BtnDecline_MouseClick;

        Height = 1;

        Add(label, btnAccept, btnDecline);
    }

    private void BtnAccept_MouseClick(object? sender, MouseEventArgs e)
    {
        NotificationItemUpdated?.Invoke(this, new NotificationItemUpdatedEventArgs(true, _contactInvitation, InvitationContextId, _invitingContactId));
    }

    private void BtnDecline_MouseClick(object? sender, MouseEventArgs e)
    {
        NotificationItemUpdated?.Invoke(this, new NotificationItemUpdatedEventArgs(false, _contactInvitation, InvitationContextId, _invitingContactId));
    }
}
