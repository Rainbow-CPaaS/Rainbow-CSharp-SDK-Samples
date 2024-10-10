using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

static public class Tools
{
    static public ColorScheme ColorSchemeBlackOnGray    = new(new Terminal.Gui.Attribute(Color.Black, Color.Gray));
    static public ColorScheme ColorSchemeWhiteOnGray    = new(new Terminal.Gui.Attribute(Color.White, Color.Gray));

    static public ColorScheme ColorSchemeBlueOnGray     = new(new Terminal.Gui.Attribute(Color.Blue, Color.Gray));
    static public ColorScheme ColorSchemeGreenOnGray    = new(new Terminal.Gui.Attribute(Color.Green, Color.Gray));
    static public ColorScheme ColorSchemeRedOnGray      = new(new Terminal.Gui.Attribute(Color.Red, Color.Gray));
    
    static public ColorScheme ColorSchemeMain           = new(new Terminal.Gui.Attribute(Color.Black, Color.Gray),     // Normal
                                                            new Terminal.Gui.Attribute(Color.Black, Color.White),    // Focus
                                                            new Terminal.Gui.Attribute(Color.Black, Color.Gray),    // Hot Normal
                                                            new Terminal.Gui.Attribute(Color.DarkGray, Color.Gray), // Disabled
                                                            new Terminal.Gui.Attribute(Color.Black, Color.Gray));   // HotFocus

    static public ColorScheme ColorSchemeBlackOnWhite = new(new Terminal.Gui.Attribute(Color.Black, Color.White));

    static public ColorScheme ColorSchemeContactsPanel  = ColorSchemeRedOnGray;
    static public ColorScheme ColorSchemePresencePanel  = ColorSchemeBlueOnGray;

    public static List<RuneCell> ToRuneCellList(string content, ColorScheme? colorScheme = null)
    {
        return content.EnumerateRunes()
                        .Select(x => new RuneCell { Rune = x, ColorScheme = colorScheme })
                        .ToList();
    }
}

