using System.Text.RegularExpressions;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

public partial class LoggerView: View
{
    public const String INFO = "INFO";
    public const String DEBUG = "DEBUG";
    public const String WARN = "WARN";

    public static readonly ColorScheme Green = new(new Attribute(Color.Green, Color.Gray));
    public static readonly ColorScheme Blue = new(new Attribute(Color.Blue, Color.Gray));
    public static readonly ColorScheme Red = new(new Attribute(Color.Red, Color.Gray));
    public static readonly ColorScheme White = new(new Attribute(Color.Black, Color.Gray));

    readonly TextView infoText;

    private readonly Object lockRegexes = new();

    private readonly Dictionary<String, List<String>> keysByColor;           // <colorName, List<String>>
    private readonly Dictionary<String, List<Regex>> regexesByColor;         // <colorName, Regex[]>
    private readonly Dictionary<String, ColorScheme> colorSchemesByColor;    // <colorName, ColorScheme>

    public LoggerView()
    {
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
            ColorScheme = White,
        };

        // To dislay with some colors
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
            ColorScheme = White
        };

        // Create "Clear" button
        Button clearInfoTextBtn = new()
        {
            Text = $"{Emojis.CLEAR}Clear",
            Y = 0,
            X = 0,
            ColorScheme = White
        };
        clearInfoTextBtn.Accept += ClearInfo;

        // Create "Copy to clipboard" button
        Button copyToClipboardInfoTextBtn = new()
        {
            Text = $"{Emojis.COPY}Copy ",
            Y = 0,
            X = Pos.AnchorEnd(),
            ColorScheme = White
        };
        copyToClipboardInfoTextBtn.Accept += CopyToClipboard;
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

    private void ClearInfo(object? sender, System.ComponentModel.HandledEventArgs e)
    {
        ClearText();
    }

    private void CopyToClipboard(object? sender, System.ComponentModel.HandledEventArgs e)
    {
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
                List<RuneCell> line = infoText.GetLine(y);

                for (var x = 0; x < line.Count; x++)
                {
                    Boolean oneMatch = false;
                    foreach (string color in matches.Keys)
                    {
                        Match[] match = matches[color];
                        if (match.Any(m => ContainsPosition(m, pos)))
                        {
                            line[x].ColorScheme = colorSchemesByColor[color];
                            oneMatch = true;
                            break;
                        }
                    }
                    if (!oneMatch)
                        line[x].ColorScheme = White;

                    pos++;
                }

                // for the \n or \r\n that exists in Text but not the returned lines
                pos += Environment.NewLine.Length;
            }
        }
    }

#endregion Events received from Terminal.Gui
}
