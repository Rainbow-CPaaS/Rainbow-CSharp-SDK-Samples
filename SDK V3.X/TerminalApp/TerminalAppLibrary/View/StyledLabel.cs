using Terminal.Gui;

public class StyledLabel : Label
{
    public TextStyle Style = TextStyle.None;

    public override ColorScheme? ColorScheme
    {
        get
        {
            var baseScheme = base.ColorScheme;
            if (baseScheme is null) return null;
            return baseScheme with
            {
                Normal = baseScheme.Normal with { TextStyle = Style },
                Focus = baseScheme.Focus with { TextStyle = Style },
                HotNormal = baseScheme.HotNormal with { TextStyle = Style },
                HotFocus = baseScheme.HotFocus with { TextStyle = Style }
            };
        }
        set => base.ColorScheme = value;
    }
}