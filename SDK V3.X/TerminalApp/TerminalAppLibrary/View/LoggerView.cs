using System.Text.RegularExpressions;
using Terminal.Gui.App;
using Terminal.Gui.Configuration;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

public partial class LoggerView: View
{
    public const String INFO = "INFO";
    public const String DEBUG = "DEBUG";
    public const String WARN = "WARN";

    readonly TextView infoText;

    private readonly Object lockRegexes = new();

    private readonly Dictionary<String, List<String>> keysById;                     // <id, List<String>>
    private readonly Dictionary<String, List<Regex>> regexesById;                   // <id, Regex[]>
    private readonly Dictionary<String, Terminal.Gui.Drawing.Scheme> schemesById;   // <id, Scheme>

    private readonly Scheme baseScheme;

    public LoggerView(String title = "", Boolean btnsVisible = true)
    {
        baseScheme = SchemeManager.GetScheme(Tools.DEFAULT_SCHEME_NAME);

        CanFocus = true;

        schemesById = [];
        keysById = [];
        regexesById = [];

        // Create Ouput Text Field - to see the progress and eventually error
        infoText = new TextView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            TextAlignment = Alignment.Start,
            Title = title,
            BorderStyle = LineStyle.Dotted,
            ReadOnly = false,
            Visible = true,
        };

        // To display with some colors
        infoText.TextChanged += InfoText_TextChanged;
        infoText.DrawingContent += HighlightTextBasedOnKeywords;
        infoText.DrawComplete += HighlightTextBasedOnKeywords;

        // Create view with buttons
        View btnsView = new()
        {
            X = Pos.Center(),
            Y = Pos.AnchorEnd(),
            Width = 24,
            Height = 1,
            Visible = btnsVisible
        };

        // Create "Clear" button
        Button clearInfoTextBtn = new()
        {
            Text = $"{Emojis.CLEAR}Clear",
            Y = 0,
            X = 0,
            ShadowStyle = ShadowStyle.None
        };
        clearInfoTextBtn.MouseClick += ClearInfoTextBtn_MouseClick;

        // Create "Copy to clipboard" button
        Button copyToClipboardInfoTextBtn = new()
        {
            Text = $"{Emojis.COPY}Copy ",
            Y = 0,
            X = Pos.AnchorEnd(),
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

    public void AddScheme(String id, String schemeName, params String[] key)
    {
        lock(lockRegexes)
            schemesById[id] = SchemeManager.GetScheme(schemeName);

        AddKeyForRegex(id, key);
    }
    
    public void AddKeyForRegex(String color, params String[] key)
    {
        lock (lockRegexes)
        {
            if ((key == null) || (key.Length == 0))
                return;

            if (!keysById.TryGetValue(color, out List<String>? keys))
                keys = [];

            if (!regexesById.TryGetValue(color, out List<Regex> ? regexes))
                regexes = [];

            foreach (var keyItem in key)
            {
                if (keys.Contains(keyItem))
                    continue;

                keys.Add(keyItem);
                regexes.Add(new Regex($@"\b{keyItem}\b", RegexOptions.IgnoreCase));
            }

            keysById[color] = keys;
            regexesById[color] = regexes;
        }
    }

    public void RemoveKeysForRegex(String color)
    {
        lock(lockRegexes)
        {
            keysById.Remove(color);
            regexesById.Remove(color);
        }
    }

    public void RemoveColorScheme(String color)
    {
        lock (lockRegexes)
            schemesById.Remove(color);
        RemoveKeysForRegex(color);
    }

    private void ClearInfoTextBtn_MouseClick(object? sender, MouseEventArgs e)
    {
        e.Handled = true;
        ClearText();
    }

    private void CopyToClipboardInfoTextBtn_MouseClick(object? sender, MouseEventArgs e)
    {
        e.Handled = true;
        Clipboard.TrySetClipboardData(infoText.Text);
    }

    private static bool ContainsPosition(Match m, int pos) { return pos >= m.Index && pos < m.Index + m.Length; }

#region Events received from Terminal.Gui


    private void InfoText_TextChanged(object? sender, EventArgs _2)
    {
        HighlightTextBasedOnKeywords(sender, null);
    }

    private void HighlightTextBasedOnKeywords(object? _1, DrawEventArgs _2)
    {
        lock (lockRegexes)
        {
            Dictionary<String, Match[]> matches = [];
            foreach (string color in regexesById.Keys)
            {
                if (schemesById.ContainsKey(color))
                {
                    Match[] m = regexesById[color].SelectMany(r => r.Matches(infoText.Text)).ToArray();
                    matches.Add(color, m);
                }
            }

            if (matches.Count == 0)
                return;
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
                            cell.Attribute = schemesById[color].Normal;
                            oneMatch = true;
                            break;
                        }
                    }
                    if (!oneMatch)
                        cell.Attribute = baseScheme.Normal;

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
