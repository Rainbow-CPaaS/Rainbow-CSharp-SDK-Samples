public class NotificationItemUpdatedEventArgs : EventArgs
{
    public Boolean Accepted { get; set; }

    public Boolean ContactInvitation { get; set; }

    // It's a BubbleId if (ContactInvitation == false), it's a InvitationId if (ContactInvitation == true)
    public String InvitationContextId { get; set; }

    public String InvitingContactId { get; set; }

    public NotificationItemUpdatedEventArgs(Boolean accepted, Boolean contactInvitation, String invitationContextId, String invitingContactId)
    {
        Accepted = accepted;
        ContactInvitation = contactInvitation;
        InvitationContextId = invitationContextId;
        InvitingContactId = invitingContactId;
    }
}

