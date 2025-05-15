using Rainbow;
using Rainbow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using static System.Net.Mime.MediaTypeNames;

public class BubblePanel : View
{
    private readonly Object lockDisplay = new();
    private readonly Rainbow.Application rbApplication;
    private readonly Rainbow.Conferences rbConferences;
    private Bubble? bubble;

    private Boolean isOwner;
    private Boolean isModerator;

    private readonly View viewOptions;
    private readonly Label lblConference;
    private readonly Label lblMembers;
    private readonly Label lblSettings;
    private readonly Label lblLink;
    private readonly Label lblFiles;

    private String? optionSelected;
    private Boolean conferenceInProgress = false;

    private readonly BubbleConferencePanel bubbleConferencePanel;
    private readonly BubbleMembersPanel bubbleMembersPanel;
    private readonly BubbleSettingsPanel bubbleSettingsPanel;
    private readonly BubbleLinkPanel bubbleLinkPanel;
    private readonly BubbleFilesPanel bubbleFilesPanel;

    public BubblePanel(Rainbow.Application application, Bubble? bubble)
    {
        rbApplication = application;
        rbConferences = application.GetConferences();

        rbConferences.ConferenceUpdated += RbConferences_ConferenceUpdated;
        rbConferences.ConferenceRemoved += RbConferences_ConferenceRemoved;

        Height = Dim.Fill();
        Width = Dim.Fill();

        bubbleConferencePanel = new(application, null);
        bubbleMembersPanel = new(application, null);
        bubbleSettingsPanel = new(application, null);
        bubbleLinkPanel = new(application, null);
        bubbleFilesPanel = new(application, null);

        Add(bubbleConferencePanel, bubbleMembersPanel, bubbleSettingsPanel, bubbleLinkPanel, bubbleFilesPanel);

        viewOptions = new()
        {
            X = Pos.AnchorEnd() - 2,
            Y = 0,
            Width = Dim.Auto(DimAutoStyle.Content),
            Height = 1
        };

        lblConference = new()
        {
            X = 0,
            Y = 0,
            Width = 4,
            Text = Emojis.CONFERENCE,
            TextAlignment = Alignment.Center
        };
        lblConference.MouseClick += LblConference_MouseClick;

        lblMembers = new()
        {
            X = Pos.Right(lblConference),
            Y = 0,
            Width = 4,
            Text = Emojis.TWO_MEMBERS,
            TextAlignment = Alignment.Center
        };
        lblMembers.MouseClick += LblMembers_MouseClick;

        lblSettings = new()
        {
            X = Pos.Right(lblMembers),
            Y = 0,
            Width = 4,
            Text = Emojis.SETTINGS,
            TextAlignment = Alignment.Center
        };
        lblSettings.MouseClick += LblSettings_MouseClick;

        lblLink = new()
        {
            X = Pos.Right(lblSettings),
            Y = 0,
            Width = 4,
            Text = Emojis.LINK,
            TextAlignment = Alignment.Center
        };
        lblLink.MouseClick += LblLink_MouseClick;

        lblFiles = new()
        {
            X = Pos.Right(lblLink),
            Y = 0,
            Width = 4,
            Text = Emojis.FILES,
            TextAlignment = Alignment.Center
        };        
        lblFiles.MouseClick += LblFiles_MouseClick;

        viewOptions.Add(lblConference, lblMembers, lblSettings, lblLink, lblFiles);
        Border.Add(viewOptions);

        BorderStyle = LineStyle.Dotted;
        CanFocus = true;

        SetBubble(bubble);
    }

    private void RbConferences_ConferenceRemoved(Conference conference)
    {
        if (bubble is null)
            return;

        if (bubble.Peer.Id == conference.Peer.Id)
        {
            conferenceInProgress = false;
            if (optionSelected == "conference")
                optionSelected = "members";

            Terminal.Gui.Application.Invoke(() =>
            {
                UpdateDisplay();
            });
        }
    }

    private void RbConferences_ConferenceUpdated(Conference conference)
    {
        if (bubble is null)
            return;

        if (bubble.Peer.Id == conference.Peer.Id)
        {
            conferenceInProgress = true;
            Terminal.Gui.Application.Invoke(() =>
            {
                UpdateDisplay();
            });
        }
    }

    private Boolean IsOwner()
    {
        isOwner = Tools.IsOwner(rbApplication, bubble);
        return isOwner;
    }

    private Boolean IsModerator()
    {
        isModerator = Tools.IsModerator(rbApplication, bubble);
        return isModerator;
    }

    public void SetBubble(Bubble? bubble)
    {
        optionSelected = "members";
        bubbleMembersPanel.SetBubble(bubble);

        Terminal.Gui.Application.Invoke(() =>
        {
            this.bubble = bubble;
            UpdateDisplay();
        });
    }

    private void UpdateDisplay()
    {
        lock (lockDisplay)
        {
            IsOwner();
            IsModerator();

            // Set title
            if (!String.IsNullOrEmpty(bubble?.Peer?.DisplayName))
                Title = bubble.Peer.DisplayName;
            else
                Title = "";

            // Display correct panel
            lblConference.Visible = true;
            lblConference.Width = conferenceInProgress ? 4 : 0;
            bubbleConferencePanel.Visible = optionSelected == "conference";
            lblConference.ColorScheme = bubbleConferencePanel.Visible ? Tools.ColorSchemeBlackOnWhite : Tools.ColorSchemeMain;

            bubbleMembersPanel.Visible = optionSelected == "members";
            lblMembers.ColorScheme = bubbleMembersPanel.Visible ? Tools.ColorSchemeBlackOnWhite: Tools.ColorSchemeMain;
            
            lblSettings.Width = isModerator ? 4 : 0;
            bubbleSettingsPanel.Visible = optionSelected == "settings";
            lblSettings.ColorScheme = bubbleSettingsPanel.Visible ? Tools.ColorSchemeBlackOnWhite : Tools.ColorSchemeMain;

            lblLink.Width = isModerator ? 4 : 0;
            bubbleLinkPanel.Visible = optionSelected == "link";
            lblLink.ColorScheme = bubbleLinkPanel.Visible ? Tools.ColorSchemeBlackOnWhite : Tools.ColorSchemeMain;

            bubbleFilesPanel.Visible = optionSelected == "files";
            lblFiles.ColorScheme = bubbleFilesPanel.Visible ? Tools.ColorSchemeBlackOnWhite : Tools.ColorSchemeMain;
        }
    }

    private void LblConference_MouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Flags == MouseFlags.Button1Clicked)
        {
            e.Handled = true;
            optionSelected = "conference";
            UpdateDisplay();
        }
    }

    private void LblLink_MouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Flags == MouseFlags.Button1Clicked)
        {
            e.Handled = true;
            optionSelected = "link";
            UpdateDisplay();
        }        
    }

    private void LblFiles_MouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Flags == MouseFlags.Button1Clicked)
        {
            e.Handled = true;
            optionSelected = "files";
            UpdateDisplay();
        }
        
    }

    private void LblSettings_MouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Flags == MouseFlags.Button1Clicked)
        {
            e.Handled = true;
            optionSelected = "settings";
            UpdateDisplay();
        }
    }

    private void LblMembers_MouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Flags == MouseFlags.Button1Clicked)
        {
            e.Handled = true;
            optionSelected = "members";
            UpdateDisplay();
        }
    }
}
