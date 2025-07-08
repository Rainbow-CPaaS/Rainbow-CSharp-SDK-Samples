using Rainbow.Consts;
using Rainbow.Model;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Configuration;

public class PresenceColorView : View
{
    char c = ' ';

    readonly Boolean forCurrentUser;
    readonly Boolean isBubble;

    public PresenceColorView(Boolean forCurrentUser, Boolean isBubble = false)
    {
        this.forCurrentUser = forCurrentUser;
        this.isBubble = isBubble;

        X = Pos.Left(this);
        Y = Pos.Top(this);
        Width = 1;
        Height = 1;
        Text = " ";
    }

    public void SetInvitationInProgress()
    {
        SetPresenceColor("InvitationInProgress", Emojis.THREE_DOTS[0]);
    }

    public void SetPresence(Presence? presence)
    {
        if(isBubble)
        {
            SetPresenceColor("Bubble", Emojis.DBL_CIRCLE[0]);
            return;
        }

        char chr = ' ';

        if ((presence == null) || (presence.PresenceLevel == PresenceLevel.Unavailable))
            SetPresenceColor("Unavailable", chr);
        else
        {
            switch (presence.PresenceLevel)
            {
                case PresenceLevel.Online:
                    if (presence.Resource.StartsWith("mobile_"))
                        chr = 'M';
                    SetPresenceColor("Online", chr);
                    break;

                case PresenceLevel.Offline:
                case PresenceLevel.Xa:
                    if (forCurrentUser)
                        chr = 'I';
                    SetPresenceColor("Offline", chr);
                    break;

                case PresenceLevel.Away:
                    SetPresenceColor("Away", chr);
                    break;

                case PresenceLevel.Dnd:
                    chr = 'D';
                    SetPresenceColor("DndOrBusy", chr);
                    break;

                case PresenceLevel.Busy:
                    if (presence.PresenceDetails?.Length > 0)
                    {
                        if (presence.PresenceDetails == PresenceDetails.Phone)
                            chr = 'A';
                        else
                            chr = presence.PresenceDetails.ToUpper()[0];
                    }
                    SetPresenceColor("DndOrBusy", chr);
                    break;
            }
        }
    }

    private void SetScheme(string schemeName)
    {
        if (SchemeName != schemeName)
        {
            SchemeName = schemeName;
            Scheme scheme;
            switch (schemeName)
            {
                case "InvitationInProgress":
                    scheme = SchemeManager.GetScheme("PresenceInvitationInProgress");
                    break;
                case "Bubble":
                    scheme = SchemeManager.GetScheme(Tools.DEFAULT_SCHEME_NAME);
                    break;
                case "Unavailable":
                    scheme = SchemeManager.GetScheme(Tools.DEFAULT_SCHEME_NAME); // /!\ seems not necessary to use specific Scheme
                    break;
                case "Online":
                    scheme = SchemeManager.GetScheme("PresenceOnline");
                    break;
                case "Offline":
                    scheme = SchemeManager.GetScheme("PresenceOffline");
                    break;
                case "Away":
                    scheme = SchemeManager.GetScheme("PresenceAway");
                    break;
                case "DndOrBusy":
                    scheme = SchemeManager.GetScheme("PresenceDndOrBusy");
                    break;

                default:
                    // This case should never occurs - just in case
                    scheme = SchemeManager.GetScheme(Tools.DEFAULT_SCHEME_NAME);
                    break;
            }

            SetScheme(scheme);
        }
    }

    private void SetPresenceColor(String schemeName, char chr)
    {
        SetScheme(schemeName);

        if (chr != c)
        {
            c = chr;
            Text = chr.ToString();
        }
    }
}
