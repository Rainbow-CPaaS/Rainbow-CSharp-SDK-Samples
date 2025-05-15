using Rainbow.Consts;
using Rainbow.Model;
using Terminal.Gui;

public class NotificationView: View
{
    private readonly Rainbow.Application rbApplication;
    private readonly Rainbow.Bubbles rbBubbles;
    private readonly Rainbow.Contacts rbContacts;
    private readonly Rainbow.Invitations rbInvitations;

    private readonly List <(String bubbleId, String invitingContactId)> bubblesInvitations;
    private readonly List<(String invitationId, String invitingContactId)> contactsInvitations;

    private int counter;
    private Contact? currentContact;

    private readonly NotificationPanelView notificationPanelView;

    public NotificationView(Rainbow.Application rbApplication)
    {
        this.rbApplication = rbApplication;
        rbBubbles = rbApplication.GetBubbles();
        rbContacts = rbApplication.GetContacts();
        rbInvitations = rbApplication.GetInvitations();

        currentContact = rbContacts.GetCurrentContact();

        bubblesInvitations  = [];
        contactsInvitations = [];

        counter = 0;

        //Visible = false;
        Width = 5; 
        Height = 1;

        CheckCounter(0);
        TextAlignment = Alignment.End;

        MouseClick += NotificationView_MouseClick;

        // We have to manage events from Invitations service
        rbInvitations.InvitationAccepted    += RbInvitations_InvitationAccepted;
        rbInvitations.InvitationCancelled   += RbInvitations_InvitationCancelled;
        rbInvitations.InvitationReceived    += RbInvitations_InvitationReceived;
        rbInvitations.InvitationDeclined    += RbInvitations_InvitationDeclined;

        // We have to manage events from Bubbles service
        rbBubbles.BubbleMemberUpdated       += RbBubbles_BubbleMemberUpdated;
        rbBubbles.BubbleInvitationAccepted  += RbBubbles_BubbleInvitationAccepted;
        rbBubbles.BubbleInvitationReceived  += RbBubbles_BubbleInvitationReceived;
        rbBubbles.BubbleInvitationRejected  += RbBubbles_BubbleInvitationRejected;

        notificationPanelView = new NotificationPanelView(rbApplication)
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Width = Dim.Fill(2),
            Height = Dim.Fill(4),
        };

        Initialized += NotificationView_Initialized;
    }

    private void NotificationView_MouseClick(object? sender, MouseEventArgs e)
    {
        // Create dialog
        Dialog dialog = new()
        {
            Title = "Notifications",
            ButtonAlignment = Alignment.Center,
            X = Pos.Center(),
            Y = Pos.Center(),
            Width = Dim.Percent(80),
            Height = Dim.Percent(80),
            ShadowStyle = ShadowStyle.None
        };

        // Create button
        Button button = new() { Text = "OK", IsDefault = true, ShadowStyle = ShadowStyle.None};
        button.MouseClick += (s, e) => { e.Handled = true; dialog.RequestStop(); };

        // Add button
        dialog.AddButton(button);

        // Add View
        dialog.Add(notificationPanelView);

        // Display dialog and wait until it's closed
        Terminal.Gui.Application.Run(dialog);

        dialog.Remove(notificationPanelView);

        // Dispose dialog
        dialog.Dispose();
    }

    private void NotificationView_Initialized(object? sender, EventArgs e)
    {
        // Get pending contact invitations received
        var invitations = rbInvitations.GetReceivedInvitations(InvitationStatus.Pending);
        foreach (var invitation in invitations)
            AddInvitation(true, invitation.Id, invitation.InvitingUserId);

        // Get Pending bubble invitations
        List<BubbleInvitation> bubblesInvitations = rbBubbles.GetPendingBubbleInvitations();
        foreach (var bubbleInvitation in bubblesInvitations)
        {
            AddInvitation(false, bubbleInvitation.Bubble.Peer.Id, bubbleInvitation.Contact.Peer.Id);
        }
    }

    public NotificationPanelView NotificationPanelView
    {  
        get
        {
            return notificationPanelView;
        }
    }

    private void AddInvitation(Boolean contactInvitation, String invitationContextId, String contactId)
    {
        if (contactInvitation)
        {
            // Check if not already displayed
            var invit = contactsInvitations.FirstOrDefault(o => o.invitationId == invitationContextId);
            if (invit != default)
                return;

            contactsInvitations.Add((invitationContextId, contactId));
        }
        else
        {
            // Check if not already displayed
            var invit = bubblesInvitations.FirstOrDefault(o => o.bubbleId == invitationContextId);
            if (invit != default)
                return;

            bubblesInvitations.Add((invitationContextId, contactId));
        }
        Terminal.Gui.Application.Invoke(() =>
        {
            CheckCounter(++counter);

            notificationPanelView.AddNotification(contactInvitation, invitationContextId, contactId);
        });
    }

    private void RemoveInvitation(Boolean contactInvitation, String invitationContextId)
    {
        Terminal.Gui.Application.Invoke(() =>
        {
            int nb;
            if (contactInvitation)
                nb = contactsInvitations.RemoveAll((o) => o.invitationId == invitationContextId);
            else
                nb = bubblesInvitations.RemoveAll((o) => o.bubbleId == invitationContextId);

            if (nb > 0)
            {
                CheckCounter(--counter);

                notificationPanelView.RemoveNotificationFromDisplay(invitationContextId);
            }
        });
    }

    private void CheckCounter(int counter)
    {
        if (counter < 0)
            counter = 0;

        this.counter = counter;
        if(counter > 0)
            Text = counter + " " + Emojis.WARNING;
        else
            Text = Emojis.WARNING;
        Visible = true;
    }

#region Events from RB Bubbles service

    private void RbBubbles_BubbleMemberUpdated(Rainbow.Model.Bubble bubble, Rainbow.Model.BubbleMember member)
    {
        //if (member.Peer.Id == currentContactId)
        //{
        //    if ((member.Status == BubbleMemberStatus.Deleted) || (member.Status == BubbleMemberStatus.Unsubscribed))
        //        RemoveInvitation(false, bubble.Peer.Id);
        //}
    }

    private void RbBubbles_BubbleInvitationAccepted(BubbleInvitation bubbleInvitation)
    {
        RemoveInvitation(false, bubbleInvitation.Bubble.Peer.Id);
    }

    private void RbBubbles_BubbleInvitationReceived(BubbleInvitation bubbleInvitation)
    {
        AddInvitation(false, bubbleInvitation.Bubble.Peer.Id, bubbleInvitation.Contact.Peer.Id);
    }

    private void RbBubbles_BubbleInvitationRejected(BubbleInvitation bubbleInvitation)
    {
        RemoveInvitation(false, bubbleInvitation.Bubble.Peer.Id);
    }

#endregion Events from RB Bubbles service

#region Events from RB Invitations service

    private void RbInvitations_InvitationAccepted(Rainbow.Model.Invitation invitation)
    {
        currentContact ??= rbContacts.GetCurrentContact();
        // We remove from the view only invitation where the current user is invited
        if (invitation.InvitedUserId == currentContact?.Peer.Id)
            RemoveInvitation(true, invitation.Id);
    }

    private void RbInvitations_InvitationCancelled(Rainbow.Model.Invitation invitation)
    {
        RemoveInvitation(true, invitation.Id);
    }

    private void RbInvitations_InvitationReceived(Rainbow.Model.Invitation invitation)
    {
        AddInvitation(true, invitation.Id, invitation.InvitingUserId);
    }

    private void RbInvitations_InvitationDeclined(Rainbow.Model.Invitation invitation)
    {
        RemoveInvitation(true, invitation.Id);
    }

#endregion Events from RB Invitations service
}
