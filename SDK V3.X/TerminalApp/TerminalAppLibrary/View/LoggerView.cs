using System.Text.RegularExpressions;
using Terminal.Gui;

public partial class LoggerView: View
{
    public const String INFO = "INFO";
    public const String DEBUG = "DEBUG";
    public const String WARN = "WARN";

    readonly TextView infoText;

    private readonly Object lockRegexes = new();

    private readonly Dictionary<String, List<String>> keysByColor;           // <colorName, List<String>>
    private readonly Dictionary<String, List<Regex>> regexesByColor;         // <colorName, Regex[]>
    private readonly Dictionary<String, ColorScheme> colorSchemesByColor;    // <colorName, ColorScheme>

    public LoggerView()
    {
        CanFocus = true;
        ColorScheme = Tools.ColorSchemeLogger;

        colorSchemesByColor = [];
        keysByColor = [];
        regexesByColor = [];

        // Create Ouput Text Field - to see the progress and eventually error
        infoText = new TextView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            TextAlignment = Alignment.Start,
            BorderStyle = LineStyle.Dotted,
            ReadOnly = false,
            Visible = true,
            ColorScheme = Tools.ColorSchemeLogger,
        };

        // To display with some colors
        infoText.TextChanged += HighlightTextBasedOnKeywords;
        infoText.DrawContent += HighlightTextBasedOnKeywords;
        infoText.DrawContentComplete += HighlightTextBasedOnKeywords;

        // Create view with buttons
        View btnsView = new()
        {
            X = Pos.Center(),
            Y = Pos.AnchorEnd(),
            Width = 24,
            Height = 1,
            ColorScheme = Tools.ColorSchemeLogger
        };

        // Create "Clear" button
        Button clearInfoTextBtn = new()
        {
            Text = $"{Emojis.CLEAR}Clear",
            Y = 0,
            X = 0,
            ColorScheme = Tools.ColorSchemeLogger,
            ShadowStyle = ShadowStyle.None
        };
        clearInfoTextBtn.MouseClick += ClearInfoTextBtn_MouseClick;

        // Create "Copy to clipboard" button
        Button copyToClipboardInfoTextBtn = new()
        {
            Text = $"{Emojis.COPY}Copy ",
            Y = 0,
            X = Pos.AnchorEnd(),
            ColorScheme = Tools.ColorSchemeLogger,
            ShadowStyle = ShadowStyle.None
        };
        copyToClipboardInfoTextBtn.MouseClick += CopyToClipboardInfoTextBtn_MouseClick;
        
        
        btnsView.Add(clearInfoTextBtn, copyToClipboardInfoTextBtn);

        Add(infoText, btnsView);
    }

    public void ClearText()
    {
        infoText.Text = "";
    }

    public void AddText(String txt)
    {
        infoText.Text = txt + Environment.NewLine + infoText.Text;
    }

    public void AddColorScheme(String color,  ColorScheme colorScheme, params String[] key)
    {
        lock(lockRegexes)
            colorSchemesByColor[color] = colorScheme;

        AddKeyForRegex(color, key);
    }
    
    public void AddKeyForRegex(String color, params String[] key)
    {
        lock (lockRegexes)
        {
            if ((key == null) || (key.Length == 0))
                return;

            if (!keysByColor.TryGetValue(color, out List<String>? keys))
                keys = [];

            if (!regexesByColor.TryGetValue(color, out List<Regex> ? regexes))
                regexes = [];

            foreach (var keyItem in key)
            {
                if (keys.Contains(keyItem))
                    continue;

                keys.Add(keyItem);
                regexes.Add(new Regex($@"\b{keyItem}\b", RegexOptions.IgnoreCase));
            }

            keysByColor[color] = keys;
            regexesByColor[color] = regexes;
        }
    }

    public void RemoveKeysForRegex(String color)
    {
        lock(lockRegexes)
        {
            keysByColor.Remove(color);
            regexesByColor.Remove(color);
        }
    }

    public void RemoveColorScheme(String color)
    {
        lock (lockRegexes)
            colorSchemesByColor.Remove(color);
        RemoveKeysForRegex(color);
    }

    private void ClearInfoTextBtn_MouseClick(object? sender, MouseEventEventArgs e)
    {
        e.MouseEvent.Handled = true;
        ClearText();
    }

    private void CopyToClipboardInfoTextBtn_MouseClick(object? sender, MouseEventEventArgs e)
    {
        e.MouseEvent.Handled = true;
        Clipboard.TrySetClipboardData(infoText.Text);
    }

private static bool ContainsPosition(Match m, int pos) { return pos >= m.Index && pos < m.Index + m.Length; }

#region Events received from Terminal.Gui

    private void HighlightTextBasedOnKeywords(object? sender, EventArgs e)
    {
        lock (lockRegexes)
        {
            Dictionary<String, Match[]> matches = [];
            foreach (string color in regexesByColor.Keys)
            {
                if (colorSchemesByColor.ContainsKey(color))
                {
                    Match[] m = regexesByColor[color].SelectMany(r => r.Matches(infoText.Text)).ToArray();
                    matches.Add(color, m);
                }
            }

            var pos = 0;
            for (var y = 0; y < infoText.Lines; y++)
            {
                List<Cell> line = infoText.GetLine(y);

                for (var x = 0; x < line.Count; x++)
                {
                    var cell = line[x];
                    Boolean oneMatch = false;
                    foreach (string color in matches.Keys)
                    {
                        Match[] match = matches[color];
                        if (match.Any(m => ContainsPosition(m, pos)))
                        {
                            cell.Attribute = Tools.ToAttribute(colorSchemesByColor[color]);
                            oneMatch = true;
                            break;
                        }
                    }
                    if (!oneMatch)
                        cell.Attribute = Tools.ToAttribute(Tools.ColorSchemeLogger);

                    line[x] = cell;
                    pos++;
                }

                // for the \n or \r\n that exists in Text but not the returned lines
                pos += Environment.NewLine.Length;
            }
        }
    }

#endregion Events received from Terminal.Gui
}
