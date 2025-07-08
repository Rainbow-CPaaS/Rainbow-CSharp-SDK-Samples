using Rainbow.Model;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;

public class BubblePanel : View
{
    private readonly Object lockDisplay = new();
    private readonly Rainbow.Application rbApplication;
    private readonly Rainbow.Conferences rbConferences;
    private Bubble? bubble;

    private Boolean isOwner;
    private Boolean isModerator;

    private readonly EmojiButtonsPanel emojiButtonsPanel;

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

        emojiButtonsPanel = new()
        {
            X = Pos.AnchorEnd() - 2,
            CanFocus = true,
        };
        emojiButtonsPanel.Add("conference", Emojis.CONFERENCE);
        emojiButtonsPanel.Add("members", Emojis.TWO_MEMBERS, isSelected: true);
        emojiButtonsPanel.Add("settings", Emojis.SETTINGS);
        emojiButtonsPanel.Add("link", Emojis.LINK);
        emojiButtonsPanel.Add("files", Emojis.FILES);

        emojiButtonsPanel.SelectionUpdated += EmojiButtonsPanel_SelectionUpdated;

        Border.Add(emojiButtonsPanel);
        Border.CanFocus = true;

        BorderStyle = LineStyle.Dotted;
        CanFocus = true;

        SetBubble(bubble);
    }

    private void EmojiButtonsPanel_SelectionUpdated(object? sender, string e)
    {
        if (e != optionSelected)
        {
            optionSelected = e;
            UpdateDisplay();
        }
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

            Terminal.Gui.App.Application.Invoke(() =>
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
            Terminal.Gui.App.Application.Invoke(() =>
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
        emojiButtonsPanel.Selected = optionSelected;
        bubbleMembersPanel.SetBubble(bubble);

        Terminal.Gui.App.Application.Invoke(() =>
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
            emojiButtonsPanel.SetVisible("conference", conferenceInProgress);
            bubbleConferencePanel.Visible = optionSelected == "conference";

            bubbleMembersPanel.Visible = optionSelected == "members";

            emojiButtonsPanel.SetVisible("settings", isModerator);
            bubbleSettingsPanel.Visible = optionSelected == "settings";

            emojiButtonsPanel.SetVisible("link", isModerator);
            bubbleLinkPanel.Visible = optionSelected == "link";

            bubbleFilesPanel.Visible = optionSelected == "files";
        }
    }

}
