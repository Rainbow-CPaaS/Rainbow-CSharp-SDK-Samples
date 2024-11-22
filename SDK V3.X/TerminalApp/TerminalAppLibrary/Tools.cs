using Rainbow.Model;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;
static public class Tools
{
    static public Color LightGray = new Color("#DDDDDD");

    static public Attribute AtributeBlackOnWhite        = new Attribute(Color.Black, Color.White);
    static public Attribute AtributeDarkGrayOnWhite = new Attribute(Color.DarkGray, Color.White);

    static public Attribute AtributeBlackOnGray         = new Attribute(Color.Black, LightGray);
    static public Attribute AtributeWhiteOnGray         = new Attribute(Color.White, LightGray);
    static public Attribute AtributeBlueOnGray          = new Attribute(Color.Blue, LightGray);
    static public Attribute AtributeGreenOnGray         = new Attribute(Color.Green, LightGray);
    static public Attribute AtributeRedOnGray           = new Attribute(Color.Red, LightGray);

    static public ColorScheme ColorSchemeBlackOnGray    = new(AtributeBlackOnGray);
    static public ColorScheme ColorSchemeWhiteOnGray    = new(AtributeWhiteOnGray);

    static public ColorScheme ColorSchemeBlueOnGray     = new(AtributeBlueOnGray);
    static public ColorScheme ColorSchemeGreenOnGray    = new(AtributeGreenOnGray);
    static public ColorScheme ColorSchemeRedOnGray      = new(AtributeRedOnGray);

    static public ColorScheme ColorSchemeLoggerBlue     = new (new Attribute(Color.Blue, Color.White));
    static public ColorScheme ColorSchemeLoggerGreen    = new(new Attribute(Color.Green, Color.White));
    static public ColorScheme ColorSchemeLoggerRed      = new(new Attribute(Color.Red, Color.White));


    static public ColorScheme ColorSchemeLogger         = new(AtributeDarkGrayOnWhite,     // Normal
                                                                AtributeDarkGrayOnWhite,   // Focus
                                                                AtributeDarkGrayOnWhite,   // Hot Normal
                                                                AtributeDarkGrayOnWhite,   // Disabled 
                                                                AtributeDarkGrayOnWhite);    // HotFocus

    static public ColorScheme ColorSchemeMain           = new(AtributeBlackOnGray,     // Normal
                                                            AtributeBlackOnWhite,    // Focus
                                                            AtributeBlackOnGray,    // Hot Normal
                                                            new Terminal.Gui.Attribute(Color.DarkGray, LightGray), // Disabled
                                                            AtributeBlackOnGray);   // HotFocus

    static public ColorScheme ColorSchemeBlackOnWhite = new(new Terminal.Gui.Attribute(Color.Black, Color.White));

    static public ColorScheme ColorSchemeContactsPanel  = ColorSchemeRedOnGray;
    static public ColorScheme ColorSchemePresencePanel  = ColorSchemeBlueOnGray;

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


    static public void DisplayPresenceDetails(Rainbow.Application application, Peer peer)
    {
        if (peer == null)
            return;

        var rbContacts = application.GetContacts();

        // Create dialog
        Dialog dialog = new()
        {
            Title = "Presence details",
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

        loggerView.AddColorScheme("blue", Tools.ColorSchemeLoggerBlue, "Resource", "Apply", "Until");
        loggerView.AddColorScheme("red", Tools.ColorSchemeLoggerRed, "DisplayName", "Id", "Jid", "Aggregated Presence");

        String txt = $"DisplayName:[{peer.DisplayName}]{Rainbow.Util.CR}Id:[{peer.Id}]{Rainbow.Util.CR}Jid:[{peer.Jid}]";

        // Add all presences
        var presences = rbContacts.GetPresencesList(peer);
        foreach (var presence in presences.Values)
            txt += $"{Rainbow.Util.CR}{Rainbow.Util.CR}{GetPresenceString(presence)}";

        // Add  aggregated presence
        var aggregatedPresence = rbContacts.GetAggregatedPresence(peer);
        txt += $"{Rainbow.Util.CR}{Rainbow.Util.CR}Aggregated Presence:{Rainbow.Util.CR}{GetPresenceString(aggregatedPresence)}";

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

}

