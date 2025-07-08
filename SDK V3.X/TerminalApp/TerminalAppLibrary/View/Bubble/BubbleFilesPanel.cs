using Rainbow;
using Rainbow.Model;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

public  class BubbleFilesPanel: View
{
    private readonly Rainbow.Application rbApplication;
    private readonly Bubbles rbBubbles;

    private Bubble? bubble;
    private readonly Object lockDisplay = new();

    private readonly Label lblInProgress;

    public BubbleFilesPanel(Rainbow.Application application, Bubble? bubble)
    {
        rbApplication = application;
        rbBubbles = rbApplication.GetBubbles();
        this.bubble = bubble;

        lblInProgress = new()
        {
            X = Pos.Center(),
            Y = 0,
            Width = Dim.Fill(),
            Height = 1,
            Text = "Not done yet ...",
            TextAlignment = Alignment.Center,
            SchemeName = "DarkGray"
        };
        Add(lblInProgress);

        Height = Dim.Fill();
        Width = Dim.Fill();

        CanFocus = true;
    }

    private void UpdateDisplay()
    {
        lock (lockDisplay)
        {
        }
    }

    public void SetBubble(Bubble? bubble)
    {
        Terminal.Gui.App.Application.Invoke(() =>
        {
            this.bubble = bubble;
            UpdateDisplay();
        });
    }
}
