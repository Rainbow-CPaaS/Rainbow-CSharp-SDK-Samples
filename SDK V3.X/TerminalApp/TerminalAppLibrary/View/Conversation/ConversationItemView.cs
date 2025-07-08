using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;
using Terminal.Gui.Configuration;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

public class ConversationItemView: View
{
    private Rainbow.Application rbApplication;
    private Contacts rbContacts;
    private Bubbles rbBubbles;
    private Conversations rbConversations;

    public Conversation? conversation;
    
    private readonly PresenceView presenceView;
    private readonly Label lblUnread;
    private readonly Label lblLastMessage;

    private Boolean isSelected;
    private readonly View viewIsSelected;


    private Boolean? displayFirstNameFirst = null;

    public event EventHandler<PeerAndMouseEventArgs>? PeerClick;

    public ConversationItemView(Rainbow.Application rbApplication, Conversation? conversation = null, Boolean? isSelected = null)
    {
        this.rbApplication = rbApplication;
        this.conversation = conversation;

        rbContacts = rbApplication.GetContacts();
        rbBubbles = rbApplication.GetBubbles();
        rbConversations = rbApplication.GetConversations();

        rbContacts.UserSettingsUpdated += RbContacts_UserSettingsUpdated;
        rbContacts.ContactsAdded += RbContacts_ContactsAdded;

        displayFirstNameFirst = rbContacts.GetUserSettingBooleanValue(UserSetting.DisplayNameOrderFirstNameFirst);

        CanFocus = true;

        // To show if the conversation is selected or not
        viewIsSelected = new()
        {
            X = 0,
            Y = 0,
            Width = 1,
            Height = Dim.Fill(),
            Text = " "
        };

        presenceView = new(rbApplication)
        {
            X = Pos.Right(viewIsSelected) + 1,
            Y = 0,
            Width = Dim.Func(() => Frame.Width - lblUnread.Frame.Width - 1),
            Height = 1
        };

        lblUnread = new()
        {
            X = Pos.Func(() => Frame.Width - lblUnread.Text.Length),
            Y = 0,
            SchemeName = "Red"
        };

        lblLastMessage = new()
        {
            X = Pos.Right(viewIsSelected) + 1,
            Y = 1,
            Text = "",
            Width = Dim.Fill(),
            Height = 1
        };

        Add(viewIsSelected, presenceView, lblUnread, lblLastMessage);

        Width = Dim.Fill();
        Height = 2;
        SetConversation(conversation, isSelected);

        rbConversations.ConversationUpdated += RbConversations_ConversationUpdated;

        MouseClick += View_MouseClick;
        viewIsSelected.MouseClick += View_MouseClick;
        presenceView.PeerClick += PresenceView_PeerClick;
        lblUnread.MouseClick += View_MouseClick;
        lblLastMessage.MouseClick += View_MouseClick;
    }

    public void SetSelected(Boolean selected)
    {
        isSelected = selected;
        viewIsSelected.SetScheme(SchemeManager.GetScheme((isSelected) ? "OnBrightBlue" : Tools.DEFAULT_SCHEME_NAME));
    }

    private void UpdateDisplay()
    {
        if (conversation is not null)
        {
            SetSelected(isSelected);

            String txt;
            var nb = conversation.UnreadMessageNumber;
            if (nb > 99)
                txt = "99+";
            else if (nb == 0)
                txt = "";
            else
                txt = $"{nb}";

            lblUnread.Text = txt;

            var contact = rbContacts.GetContactByJid(conversation.LastMessagePeerJid);
            if ( (contact is not null) && (displayFirstNameFirst is not null))
            {
                if (contact.Peer.Id == rbContacts.GetCurrentContact().Peer.Id)
                {
                    lblLastMessage.Text = $"Me: {conversation.LastMessageText}";
                }
                else
                {
                    if (displayFirstNameFirst.Value)
                        lblLastMessage.Text = $"{contact.FirstName}: {conversation.LastMessageText}";
                    else
                        lblLastMessage.Text = $"{contact.LastName}: {conversation.LastMessageText}";
                }
            }
            else
                lblLastMessage.Text = (conversation.LastMessageText is null) ? "" : conversation.LastMessageText;
        }
        else
        {
            lblUnread.Text = "";
            lblLastMessage.Text = "";
        }
        lblUnread.Width = lblUnread.Text.Length;
    }

    public void SetConversation(Conversation? conversation, Boolean? isSelected = null)
    {
        this.conversation = conversation;
        presenceView.SetConversation(conversation);

        if (isSelected is not null)
            this.isSelected = isSelected.Value;

        Terminal.Gui.App.Application.Invoke(() =>
        {
            UpdateDisplay();
        });
    }

    private void PresenceView_PeerClick(object? sender, PeerAndMouseEventArgs e)
    {
        PeerClick?.Invoke(sender, e);
    }

    private void View_MouseClick(object? sender, MouseEventArgs e)
    {
        if (conversation != null)
            PeerClick?.Invoke(sender, new PeerAndMouseEventArgs(conversation, e));
    }

    private void RbContacts_UserSettingsUpdated()
    {
        var newValue = rbContacts.GetUserSettingBooleanValue(UserSetting.DisplayNameOrderFirstNameFirst);
        if (displayFirstNameFirst != newValue)
        {
            Terminal.Gui.App.Application.Invoke(() =>
            {
                displayFirstNameFirst = newValue;
                UpdateDisplay();
            });
        }
    }

    private void RbContacts_ContactsAdded(List<Contact> contacts)
    {
        if(conversation?.Peer.Type == EntityType.User)
        {
            var contactFound = contacts.FirstOrDefault(c => c.Peer.Id == conversation?.Peer?.Id);
            if(contactFound is not null)
            {
                Terminal.Gui.App.Application.Invoke(() =>
                {
                    UpdateDisplay();
                });
            }
        }
    }

    private void RbConversations_ConversationUpdated(Conversation conversation)
    {
        // Since we display fully the panel when a conversation is updated, we don't need to do something here
        return;

        //if(conversation.Peer.Id == this.conversation?.Peer.Id)
        //{
        //    Terminal.Gui.Application.Invoke(() =>
        //    {
        //        this.conversation = conversation;
        //        UpdateDisplay();
        //    });
        //}
    }

}

