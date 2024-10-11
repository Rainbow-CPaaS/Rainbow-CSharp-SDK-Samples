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

}

