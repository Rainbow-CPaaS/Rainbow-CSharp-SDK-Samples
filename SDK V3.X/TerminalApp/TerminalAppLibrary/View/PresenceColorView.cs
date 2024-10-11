using Rainbow.Consts;
using Rainbow.Model;
using System.Drawing;
using System.Text;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;
using Color = Terminal.Gui.Color;

public class PresenceColorView : View
{
    Color background;
    Color foreground;
    char c = ' ';

    readonly Boolean forCurrentUser;

    public PresenceColorView(Boolean forCurrentUser)
    {
        this.forCurrentUser = forCurrentUser;

        X = Pos.Left(this);
        Y = Pos.Top(this);
        Width = 1;
        Height = 1;
    }

    public override void OnDrawContent(Rectangle bounds)
    {
        base.OnDrawContent(bounds);

        var attr = new Attribute(foreground, background);
        Driver.SetAttribute(attr);
        AddRune(0, 0, (Rune)c);
    }


    public void SetInvitationInProgress()
    {
        c = Emojis.THREE_DOTS[0];
        background = new Color(255, 255, 255);
        foreground = new Color(0, 0, 0);
        SetNeedsDisplay();
    }

    public void SetPresence(Presence? presence)
    {
        char chr = ' ';
        Color? background = null;
        Color? foreground = null;

        if ( (presence == null) || (presence.PresenceLevel == PresenceLevel.Unavailable) )
            background = Tools.LightGray;
        else
        {
            switch (presence.PresenceLevel)
            {
                case PresenceLevel.Online:
                    background = new Color(92, 163, 0);
                    if (presence.Resource.StartsWith("mobile_"))
                    {
                        foreground = new Color(255, 255, 255);
                        chr = 'M';
                    }
                    break;

                case PresenceLevel.Offline:
                case PresenceLevel.Xa:
                    background = new Color(255, 255, 255);
                    if (forCurrentUser)
                    {
                        foreground = new Color(0, 0, 0);
                        chr = 'I';
                    }
                    break;

                case PresenceLevel.Away:
                    background = new Color(242, 199, 68);
                    break;

                case PresenceLevel.Dnd:
                    background = new Color(243, 72, 63);
                    foreground = new Color(255, 255, 255);
                    chr = 'D';
                    break;

                case PresenceLevel.Busy:
                    background = new Color(243, 72, 63);
                    if (presence.PresenceDetails?.Length > 0)
                    {
                        foreground = new Color(255, 255, 255);
                        chr = presence.PresenceDetails.ToUpper()[0];
                    }
                    break;
            }
        }

        if (foreground == null)
            foreground = background;
        SetPresenceColor(background, foreground, chr);
    }

    private void SetPresenceColor(Color? bgd, Color? fgd, char chr)
    {
        if ((bgd == null) || (fgd == null))
            return;

        if ((bgd != background) || (fgd != foreground) || (chr != c))
        {
            background = bgd.Value;
            foreground = fgd.Value;
            c = chr;
        }

        SetNeedsDisplay();
    }
}
