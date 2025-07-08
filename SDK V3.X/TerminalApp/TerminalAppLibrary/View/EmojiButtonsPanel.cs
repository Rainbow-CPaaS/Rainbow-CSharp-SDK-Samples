using Terminal.Gui.ViewBase;

public class EmojiButtonsPanel: View
{
    const int BTN_WIDTH = 6;
    private String? selected = null;
    private int width = 0;
    private Dictionary<String, EmojiButton> emojiButtonsList;
    private EmojiButton? lastEmojiButton;

    private readonly String colorSchemeDefault;
    private readonly String colorSchemeSelected;
    private readonly String colorSchemeOver;

    public event EventHandler<String>? SelectionUpdated;

    public EmojiButtonsPanel(String? colorSchemeDefault = null, String? colorSchemeSelected = null, String? colorSchemeOver = null)
    {
        this.colorSchemeDefault ??= Tools.DEFAULT_SCHEME_NAME;
        this.colorSchemeSelected ??= "OnLightBlue";
        this.colorSchemeOver ??= "OnLightBlue";

        emojiButtonsList = new();

        lastEmojiButton = null;

        Width = width;
        Height = 1;

        CanFocus = true;
    }

    public void SetVisible(string id, Boolean visible)
    {
        if (emojiButtonsList.TryGetValue(id, out var button))
        {
            if(visible && button.Width != BTN_WIDTH)
            {
                button.Width = BTN_WIDTH;
                
                width += BTN_WIDTH;
                Width = width;

                SetNeedsDraw();
            }
            else if ((!visible) && button.Width != 0)
            {
                button.Width = 0;
                width -= BTN_WIDTH;
                Width = width;

                SetNeedsDraw();
            }
        }
    }

    public String Selected
    {
        get => selected;
        set
        {
            if(value != selected)
            {
                foreach(var button in emojiButtonsList)
                {
                    button.Value.Selected = (button.Key == value);
                }
                selected = value;
            }
        }
    }

    public Boolean Add(String id, String emoji, int width = BTN_WIDTH, Boolean isSelected = false)
    {
        if (emojiButtonsList.ContainsKey(id))
            return false;

        this.width += width;
        Width = this.width;

        if (isSelected)
            selected = id;

        EmojiButton emojiButton = new(id, emoji, width, isSelected, colorSchemeDefault, colorSchemeSelected, colorSchemeOver);
        emojiButton.X = lastEmojiButton is null ? 0 : Pos.Right(lastEmojiButton);

        emojiButton.ButtonClick += EmojiButton_ButtonClick;

        Add(emojiButton);
        emojiButtonsList.Add(id, emojiButton);

        lastEmojiButton = emojiButton;

        
        return true;
    }

    private void EmojiButton_ButtonClick(object? sender, string e)
    {
        foreach (var button in emojiButtonsList)
        {
            if (button.Key != e)
                button.Value.Selected = false;
        }
        if (e != selected)
        {
            selected = e;
            SelectionUpdated?.Invoke(this, selected);
        }
    }
}
