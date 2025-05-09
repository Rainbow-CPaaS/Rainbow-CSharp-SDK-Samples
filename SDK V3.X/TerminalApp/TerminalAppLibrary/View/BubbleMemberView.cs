﻿using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;
using Terminal.Gui;

public class BubbleMemberView: View
{
    private readonly Rainbow.Application rbApplication;
    private readonly Contacts rbContacts;
    private readonly Bubbles rbBubbles;

    private readonly PresenceView presenceView;
    private readonly Label lblPrivilege;

    private readonly Bubble bubble;
    BubbleMember bubbleMember;
    Contact? contact;

    public event EventHandler<PeerAndMouseEventArgs>? PeerClick;

    public BubbleMemberView(Rainbow.Application application, Bubble bubble, BubbleMember bubbleMember)
    {
        rbApplication = application;
        rbContacts = rbApplication.GetContacts();
        rbBubbles = rbApplication.GetBubbles();

        this.bubble = bubble;
        this.bubbleMember = bubbleMember;

        contact = rbContacts.GetContactById(bubbleMember?.Peer?.Id);

        presenceView = new(application, contact)
        {
            X = 1,
            Y = 0,
            Width = Dim.Fill(5),
            Height = 1
        };

        lblPrivilege = new()
        {
            X = Pos.Right(presenceView) + 1,
            Y = presenceView.Y,
            TextAlignment = Alignment.End,
            Width = 4,
            Height = 1
        };

        Add(presenceView, lblPrivilege);

        Height = Dim.Auto(DimAutoStyle.Content);
        Width = Dim.Auto(DimAutoStyle.Content);

        UpdateDisplay(true);

        rbContacts.ContactsAdded += RbContacts_ContactsAdded;
        rbBubbles.BubbleMemberUpdated += RbBubbles_BubbleMemberUpdated;


        MouseClick += View_MouseClick;
        lblPrivilege.MouseClick += View_MouseClick;
        presenceView.PeerClick += PresenceView_PeerClick;
    }

    private void PresenceView_PeerClick(object? sender, PeerAndMouseEventArgs e)
    {
        PeerClick?.Invoke(sender, e);
    }

    private void View_MouseClick(object? sender, MouseEventArgs e)
    {
        if (bubbleMember != null)
            PeerClick?.Invoke(sender, new PeerAndMouseEventArgs(bubbleMember, e));
    }

    private void UpdateDisplay(Boolean updatePresenceView)
    {
        if(updatePresenceView)
            presenceView.SetContact(contact);

        if (bubbleMember is not null)
        {
            String txt;
            if (bubbleMember.Status != BubbleMemberStatus.Accepted)
                txt = Emojis.TIMER;
            else
                txt = "  ";

            switch (bubbleMember.Privilege)
            {
                case BubbleMemberPrivilege.Owner:
                    txt += Emojis.CHESS_KING;
                    break;

                case BubbleMemberPrivilege.Moderator:
                    txt += Emojis.CHESS_TOWER;
                    break;

                case BubbleMemberPrivilege.User:
                    txt += " ";
                    break;
            }
            lblPrivilege.Text = txt;
        }
    }

    private void RbBubbles_BubbleMemberUpdated(Bubble bubble, BubbleMember member)
    {
        if (this.bubble?.Peer?.Id == bubble?.Peer?.Id)
        {
            if (member?.Peer?.Id == bubbleMember?.Peer?.Id)
            {
                bubbleMember = member;

                Terminal.Gui.Application.Invoke(() =>
                {
                    UpdateDisplay(false);
                });
            }
        }
    }

    private void RbContacts_ContactsAdded(List<Contact> contacts)
    {
        var contact = contacts.FirstOrDefault(c => c.Peer.Id == bubbleMember?.Peer?.Id);

        if (contact is not null)
        {
            Boolean updatePresenceView = this.contact is null;

            this.contact = contact;
            Terminal.Gui.Application.Invoke(() =>
            {
                UpdateDisplay(updatePresenceView);
            });
        }
    }
}
