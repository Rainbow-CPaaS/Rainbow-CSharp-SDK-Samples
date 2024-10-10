using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

public class NotificationPanelView: View
{
    private readonly Rainbow.Application _rbApplication;
    private readonly Rainbow.Bubbles _rbBubbles;
    private readonly Rainbow.Invitations _rbInvitations;
    private readonly List<NotificationItemView> _items;

    public NotificationPanelView(Rainbow.Application rbApplication)
    {
        _rbApplication = rbApplication;
        _rbBubbles = rbApplication.GetBubbles();
        _rbInvitations = rbApplication.GetInvitations();

        _items = [];
    }

    public void AddNotification(Boolean contactInvitation, String invitationContextId, String contactId)
    {
        var item = new NotificationItemView(_rbApplication, contactInvitation, invitationContextId, contactId)
        {
            X = Pos.Center(),
            Y = _items.Count * 2,
            Width = Dim.Percent(100),
            Height = Dim.Percent(100),
        };
        item.NotificationItemUpdated += Item_NotificationItemUpdated;
        _items.Add(item);
        Add(item);
    }

    public void RemoveNotificationFromDisplay(String invitationContextId)
    {
        var item = _items.FirstOrDefault(i => i.InvitationContextId == invitationContextId);
        if(item != null)
        {
            // Remove the item from the display
            _items.Remove(item);
            Remove(item);

            // Update the display for others
            int index = 0;
            foreach(var itemDisplayd in _items)
            {
                itemDisplayd.Y = 2 + index * 2;
                index++;
            }
        }
    }

    private void Item_NotificationItemUpdated(object? sender, NotificationItemUpdatedEventArgs e)
    {
        Boolean success;
        if (e.ContactInvitation)
        {
            var invitation = _rbInvitations.GetInvitation(e.InvitationContextId);
            if (e.Accepted)
            {
                var sdkResult = Rainbow.AsyncHelper.RunSync(async () => await _rbInvitations.AcceptReceivedPendingInvitationAsync(invitation).ConfigureAwait(false));
                success = sdkResult.Success;
            }
            else
            {
                var sdkResult = Rainbow.AsyncHelper.RunSync(async () => await _rbInvitations.DeclineReceivedPendingInvitationAsync(invitation).ConfigureAwait(false));
                success = sdkResult.Success;
            }
        }
        else // Bubble invitation
        {
            var bubble = _rbBubbles.GetBubbleById(e.InvitationContextId);
            if (e.Accepted)
            {
                var sdkResult = Rainbow.AsyncHelper.RunSync(async () => await _rbBubbles.AcceptInvitationAsync(bubble).ConfigureAwait(false));
                success = sdkResult.Success;
            }
            else
            {
                var sdkResult = Rainbow.AsyncHelper.RunSync(async () => await _rbBubbles.DeclineInvitationAsync(bubble).ConfigureAwait(false));
                success = sdkResult.Success;
            }
        }

        // On success we don't need to update display
        //  - RemoveNotificationFromDisplay() should be called by NotificationView
        
        // On failure, we should display an error message
        if (!success)
        {
            // TODO
        }
            
    }

}

