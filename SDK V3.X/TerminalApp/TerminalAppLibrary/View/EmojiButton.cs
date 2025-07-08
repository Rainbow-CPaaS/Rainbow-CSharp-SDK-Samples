using Terminal.Gui.Configuration;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

public class EmojiButton : View
{
    private readonly Object lockDisplay = new();

    private readonly String id;

    private readonly String emoji;
    private Boolean selected = false;
    private Boolean mouseOver = false;

    private readonly Label lbl;
    private readonly String colorSchemeDefault;
    private readonly String colorSchemeSelected;
    private readonly String colorSchemeOver;

    public event EventHandler<String>? ButtonClick;

    public EmojiButton(String id, String emoji, int width, Boolean selected, String colorSchemeDefault, String colorSchemeSelected, String colorSchemeOver)
    {
        this.id = id;
        this.emoji = $" {emoji} ";
        this.colorSchemeDefault = colorSchemeDefault;
        this.colorSchemeSelected = colorSchemeSelected;
        this.colorSchemeOver = colorSchemeOver;
        this.selected = selected;

        lbl = new()
        {
            Width = width,
            Height = 1,
            Text = this.emoji,
            TextAlignment = Alignment.Center,
            CanFocus = true,
            SchemeName = colorSchemeDefault
        };

        lbl.MouseClick += EmojiButton_MouseClick;
        lbl.MouseEnter += EmojiButton_MouseEnter;
        lbl.MouseLeave += EmojiButton_MouseLeave;

        Add(lbl);

        Width = width;
        Height = 1;
        CanFocus = true;

        UpdateDisplay();
    }

    public Boolean Selected
    {
        get => selected;
        set
        {
            if (value != selected)
            {
                selected = value;
                UpdateDisplay();
            }
        }
    }

    private void UpdateDisplay()
    {
        lock (lockDisplay)
        {
            String text;
            String schemeName;

            if (selected)
            {
                text = $"[{emoji}]";
                schemeName = colorSchemeSelected;
            }
            else
            {
                text = mouseOver ? $" {emoji} " : emoji;
                schemeName = mouseOver ? colorSchemeOver : colorSchemeDefault;
            }

            lbl.Text = text;
            lbl.SetScheme(SchemeManager.GetScheme(schemeName));
            SetScheme(SchemeManager.GetScheme(schemeName));
        }
    }

    private void EmojiButton_MouseLeave(object? sender, EventArgs e)
    {
        mouseOver = false;
        UpdateDisplay();
    }

    private void EmojiButton_MouseEnter(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        mouseOver = true;
        UpdateDisplay();
    }

    private void EmojiButton_MouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Flags == MouseFlags.Button1Clicked)
        {
            e.Handled = true;
            if (selected) return; // nothing to do

            selected = true;
            UpdateDisplay();
            ButtonClick?.Invoke(this, id);
        }
    }
}

