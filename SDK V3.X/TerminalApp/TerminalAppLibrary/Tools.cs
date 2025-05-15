using Rainbow.Model;
using System.Text;
using Terminal.Gui;
using TerminalAppLibrary.Model;
using Attribute = Terminal.Gui.Attribute;
static public class Tools
{
    static public Color LightGray = new Color("#DDDDDD");


    static public Attribute AtributeDarkGrayOnGray = new Attribute(Color.DarkGray, LightGray);
    static public Attribute AtributeDarkGrayOnWhite = new Attribute(Color.DarkGray, Color.White);

    static public Attribute AtributeBlackOnGray = new Attribute(Color.Black, LightGray);
    static public Attribute AtributeBlackOnWhite = new Attribute(Color.Black, Color.White);

    static public Attribute AtributeWhiteOnGray = new Attribute(Color.White, LightGray);

    static public Attribute AtributeBlueOnGray = new Attribute(Color.Blue, LightGray);
    static public Attribute AtributeBlueOnWhite = new Attribute(Color.Blue, Color.White);

    static public Attribute AtributeGreenOnGray = new Attribute(Color.Green, LightGray);
    static public Attribute AtributeGreenOnWhite = new Attribute(Color.Green, Color.White);

    static public Attribute AtributeRedOnGray = new Attribute(Color.Red, LightGray);
    static public Attribute AtributeRedOnWhite = new Attribute(Color.Red, Color.White);

    static public ColorScheme ColorSchemeBlackOnGray = new(AtributeBlackOnGray);
    static public ColorScheme ColorSchemeDarkGrayOnGray = new(AtributeDarkGrayOnGray);
    static public ColorScheme ColorSchemeWhiteOnGray = new(AtributeWhiteOnGray);

    static public ColorScheme ColorSchemeBlueOnGray = new(AtributeBlueOnGray,       // Normal
                                                                AtributeBlueOnWhite,    // Focus
                                                                AtributeBlueOnGray,     // Hot Normal
                                                                AtributeDarkGrayOnGray, // Disabled 
                                                                AtributeBlueOnGray);    // HotFocus

    static public ColorScheme ColorSchemeGreenOnGray = new(AtributeGreenOnGray,      // Normal
                                                                AtributeGreenOnWhite,   // Focus
                                                                AtributeGreenOnGray,    // Hot Normal
                                                                AtributeDarkGrayOnGray, // Disabled 
                                                                AtributeGreenOnGray);   // HotFocus

    static public ColorScheme ColorSchemeRedOnGray = new(AtributeRedOnGray,        // Normal
                                                                AtributeRedOnWhite,     // Focus
                                                                AtributeRedOnGray,      // Hot Normal
                                                                AtributeDarkGrayOnGray, // Disabled 
                                                                AtributeRedOnGray);     // HotFocus

    static public ColorScheme ColorSchemeLoggerBlue = new(new Attribute(Color.Blue, Color.White));
    static public ColorScheme ColorSchemeLoggerGreen = new(new Attribute(Color.Green, Color.White));
    static public ColorScheme ColorSchemeLoggerRed = new(new Attribute(Color.Red, Color.White));


    static public ColorScheme ColorSchemeLogger = new(AtributeDarkGrayOnWhite,     // Normal
                                                                AtributeDarkGrayOnWhite,   // Focus
                                                                AtributeDarkGrayOnWhite,   // Hot Normal
                                                                AtributeDarkGrayOnWhite,   // Disabled 
                                                                AtributeDarkGrayOnWhite);    // HotFocus

    static public ColorScheme ColorSchemeMain = new(AtributeBlackOnGray,     // Normal
                                                            AtributeBlackOnWhite,    // Focus
                                                            AtributeBlackOnGray,    // Hot Normal
                                                            AtributeDarkGrayOnGray, // Disabled
                                                            AtributeBlackOnGray);   // HotFocus

    static public ColorScheme ColorSchemeBlackOnWhite = new(new Attribute(Color.Black, Color.White));
    static public ColorScheme ColorSchemeWhiteOnBlack = new(new Attribute(Color.White, Color.Black));
    static public ColorScheme ColorSchemeWhiteOnBlue = new(new Attribute(Color.White, Color.Blue));

    static public ColorScheme ColorSchemeContactsPanel = ColorSchemeRedOnGray;
    static public ColorScheme ColorSchemePresencePanel = ColorSchemeBlueOnGray;

    static public List<Cell> ToCellList(string str, ColorScheme? colorscheme = null)
    {
        return Cell.ToCellList(str, ToAttribute(colorscheme));
    }

    static public Terminal.Gui.Attribute ToAttribute(ColorScheme? colorscheme = null)
    {
        if (colorscheme != null)
            return new Terminal.Gui.Attribute(colorscheme.Normal.Foreground, colorscheme.Normal.Background);
        return new();
    }

    static public void DisplayLoggerAsDialog(string title, string txt, List<ColorSchemeInfo>? colorSchemeInfos = null)
    {

        Terminal.Gui.Application.Invoke(() =>
        {

            // Create dialog
            Dialog dialog = new()
            {
                Title = title,
                ButtonAlignment = Alignment.Center,
                X = Pos.Center(),
                Y = Pos.Center(),
                Width = Dim.Percent(80),
                Height = Dim.Percent(80),
                ShadowStyle = ShadowStyle.None
            };

            LoggerView loggerView = new()
            {
                X = Pos.Center(),
                Y = Pos.Center(),
                Width = Dim.Fill(4),
                Height = Dim.Fill(2),
            };
            if (colorSchemeInfos is not null)
            {
                foreach (var colorSchemeInfo in colorSchemeInfos)
                {
                    loggerView.AddColorScheme(colorSchemeInfo.Color, colorSchemeInfo.ColorScheme, colorSchemeInfo.Keys);
                }
            }

            // Create button
            Button button = new() { Text = "OK", IsDefault = true, ShadowStyle = ShadowStyle.None };
            button.Accepting += (s, e) => { Application.RequestStop(); e.Cancel = true; };

            // Add button
            dialog.AddButton(button);

            // Add View
            dialog.Add(loggerView);

            // Add text to Logger
            loggerView.AddText(txt);

            // Display dialog and wait until it's closed
            Terminal.Gui.Application.Run(dialog);

            // Dispose dialog
            dialog.Dispose();
        });
    }

    static public void DisplayPresenceDetails(Rainbow.Application application, Peer peer)
    {
        if (peer == null)
            return;

        var rbContacts = application.GetContacts();

        List<ColorSchemeInfo> colorSchemeInfos = new();

        ColorSchemeInfo blue = new() {
            Color = "blue",
            ColorScheme = Tools.ColorSchemeLoggerBlue,
            Keys = ["Resource", "Apply", "Until"]
        };
        ColorSchemeInfo red = new()
        {
            Color = "red",
            ColorScheme = Tools.ColorSchemeLoggerRed,
            Keys = ["DisplayName", "Id", "Jid", "Aggregated Presence"]
        };
        colorSchemeInfos.Add(blue);
        colorSchemeInfos.Add(red);

        // Create text
        String txt = $"DisplayName:[{peer.DisplayName}]{Rainbow.Util.CR}Id:[{peer.Id}]{Rainbow.Util.CR}Jid:[{peer.Jid}]";

        // Add all presences
        var presences = rbContacts.GetPresencesList(peer);
        foreach (var presence in presences.Values)
            txt += $"{Rainbow.Util.CR}{Rainbow.Util.CR}{GetPresenceString(presence)}";

        // Add  aggregated presence
        var aggregatedPresence = rbContacts.GetAggregatedPresence(peer);
        txt += $"{Rainbow.Util.CR}{Rainbow.Util.CR}Aggregated Presence:{Rainbow.Util.CR}{GetPresenceString(aggregatedPresence)}";

        var title = "Presence details";
        DisplayLoggerAsDialog(title, txt, colorSchemeInfos);
    }

    public static String GetPresenceString(Presence presence)
    {
        String calendar;
        if (presence.Resource == "calendar")
            calendar = $" - Until:[{presence.Until:o}]";
        else
            calendar = "";

        return $"Resource:[{presence.Resource}] - Apply:[{presence.Apply}]{calendar}{Rainbow.Util.CR}\t{Emojis.TRIANGLE_RIGHT}{presence.PresenceLevel}{((presence.PresenceDetails?.Length > 0) ? (" [" + presence.PresenceDetails) + "]" : "")}";
    }

    public static ExpanderButton VerticalExpanderButton()
    {
        return new ExpanderButton()
        {
            Orientation = Orientation.Vertical,
            Width = 4,
            ColorScheme = Tools.ColorSchemeGreenOnGray,
            CollapseString = $" {Emojis.MINUS} ",
            ExpandString = $" {Emojis.PLUS} ",
        };
    }

    public static void InitTerminalGuiApplication()
    {
        Console.OutputEncoding = Encoding.Default;
        var _forceDriver = "v2net"; // v2win
        Terminal.Gui.Application.ForceDriver = _forceDriver;
        Terminal.Gui.Application.Init(driverName: _forceDriver);
    }

    public static Boolean IsOwner(Rainbow.Application? rbApplication, Bubble? bubble)
    {
        if ((rbApplication is null) || (bubble is null))
            return false;

        var o = rbApplication.GetBubbles().IsOwner(bubble);
        return o is not null && o.Value == true;
    }

    public static Boolean IsModerator(Rainbow.Application? rbApplication, Bubble? bubble)
    {
        if ((rbApplication is null) || (bubble is null))
            return false;

        var isOwner = IsOwner(rbApplication, bubble);
        var isModerator = isOwner;
        if (!isModerator)
        {
            var m = rbApplication.GetBubbles().IsModerator(bubble);
            isModerator = m is not null && m.Value == true;
        }
        return isModerator;
    }
}

