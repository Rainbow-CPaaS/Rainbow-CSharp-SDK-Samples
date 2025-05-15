using Rainbow;
using Rainbow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

public class BubbleSettingsPanel: View
{
    private readonly Rainbow.Application rbApplication;
    private readonly Bubbles rbBubbles;

    private Bubble? bubble;
    private readonly Object lockDisplay = new();

    private readonly Label lblInProgress;

    public BubbleSettingsPanel(Rainbow.Application application, Bubble? bubble)
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
            ColorScheme = Tools.ColorSchemeDarkGrayOnGray
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
        Terminal.Gui.Application.Invoke(() =>
        {
            this.bubble = bubble;
            UpdateDisplay();
        });
    }

}
