using Rainbow.Model;
using System.Text;
using Terminal.Gui.App;
using Terminal.Gui.Configuration;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using TerminalAppLibrary.Model;
using Attribute = Terminal.Gui.Drawing.Attribute;
static public class Tools
{
    static public String DEFAULT_SCHEME_NAME = "Base";

    static public Attribute AttributeBlack = new Attribute(Color.Black, StandardColor.RaisinBlack);
    static public Attribute AttributeRed = new Attribute(Color.Red, StandardColor.RaisinBlack);
    static public Attribute AttributeGreen = new Attribute(Color.Green, StandardColor.RaisinBlack);
    static public Attribute AttributeBrightBlue = new Attribute(Color.BrightBlue, StandardColor.RaisinBlack);

    static public List<Cell> ToCellList(string str, Attribute ? attribute = null)
    {
        return Cell.ToCellList(str, attribute);
    }

    static public void DisplayLoggerAsDialog(string title, string txt, List<ColorSchemeInfo>? colorSchemeInfos = null)
    {
        Application.Invoke(() =>
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
                    loggerView.AddScheme(colorSchemeInfo.Color, colorSchemeInfo.SchemeName, colorSchemeInfo.Keys);
                }
            }

            // Create button
            Button button = new() { Text = "OK", IsDefault = true, ShadowStyle = ShadowStyle.None };
            button.Accepting += (s, e) => { Application.RequestStop(); e.Handled = true; };

            // Add button
            dialog.AddButton(button);

            // Add View
            dialog.Add(loggerView);

            // Add text to Logger
            loggerView.AddText(txt);

            // Display dialog and wait until it's closed
            Terminal.Gui.App.Application.Run(dialog);

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
            SchemeName = Tools.DEFAULT_SCHEME_NAME,
            Keys = ["Resource", "Apply", "Until"]
        };
        ColorSchemeInfo red = new()
        {
            Color = "red",
            SchemeName = "Red",
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
            CollapseString = $" {Emojis.MINUS} ",
            ExpandString = $" {Emojis.PLUS} "
        };
    }

    public static void InitTerminalGuiApplication()
    {
        String tuiConfig = $".{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}tui_config.json";
        if (File.Exists(tuiConfig))
        {
            String jsonConfig = File.ReadAllText(tuiConfig);

            ConfigurationManager.RuntimeConfig = jsonConfig;
            ConfigurationManager.Enable(ConfigLocations.Runtime);
            //ConfigurationManager.Apply();

            //ThemeManager.Theme = "Base";
        }

        Console.OutputEncoding = Encoding.Default;

        Boolean isWindows = false;
        try
        {
            var osDesc = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
            isWindows = osDesc.Contains("windows", StringComparison.InvariantCultureIgnoreCase);
        }
        catch (Exception exc)
        {
        }

        if (isWindows)
        {
            var _forceDriver = "v2net"; /* "v2win"; */
            Terminal.Gui.App.Application.ForceDriver = _forceDriver;
            Terminal.Gui.App.Application.Init(driverName: _forceDriver);
        }

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

