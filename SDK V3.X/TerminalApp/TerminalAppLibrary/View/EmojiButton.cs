using Terminal.Gui;

public class EmojiButton : View
{
    private readonly Object lockDisplay = new();

    private readonly String id;

    private readonly String emoji;
    private Boolean selected = false;
    private Boolean mouseOver = false;

    private readonly Label lbl;
    private readonly ColorScheme colorSchemeDefault;
    private readonly ColorScheme colorSchemeSelected;
    private readonly ColorScheme colorSchemeOver;

    public event EventHandler<String>? ButtonClick;

    public EmojiButton(String id, String emoji, int width, Boolean selected, ColorScheme colorSchemeDefault, ColorScheme colorSchemeSelected, ColorScheme colorSchemeOver)
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
            ColorScheme = colorSchemeDefault
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
            if (selected)
            {
                lbl.Text = $"[{emoji}]";
                lbl.ColorScheme = colorSchemeSelected;
                ColorScheme = colorSchemeSelected;
            }
            else
            {
                if (mouseOver)
                {
                    lbl.Text = $"_{emoji}_";
                    lbl.ColorScheme = colorSchemeOver;
                    ColorScheme = colorSchemeOver;
                }
                else
                {
                    lbl.Text = emoji;
                    lbl.ColorScheme = colorSchemeDefault;
                    ColorScheme = colorSchemeDefault;
                }
            }
            //lbl.SetNeedsDraw();
            //SetNeedsDraw();
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

