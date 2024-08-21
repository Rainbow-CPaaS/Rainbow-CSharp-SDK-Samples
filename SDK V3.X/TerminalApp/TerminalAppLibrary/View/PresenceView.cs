using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;
using Color = Terminal.Gui.Color;

public class PresenceView: View
{
    private readonly Label labelField;
    private readonly PresenceColorView presenceColorView;
    private ContextMenu contextMenu = new();
    private Point mousePos = default;

    private readonly Rainbow.Application rbApplication;
    private readonly Contacts rbContacts;
    private Contact? contact;

    private Boolean currentUser;
    private readonly Boolean withContextMenu;
    private Boolean withDetails;

    public static ColorScheme AppointementColorScheme = new(new Attribute(Color.Black, Color.Blue));

    public event EventHandler<ContactEventArgs>? ContactClick;

    public PresenceView(Rainbow.Application application, Contact contact, Boolean withContextMenu = false, Boolean withDetails = false)
    {
        rbApplication = application;
        rbContacts = rbApplication.GetContacts();

        Boolean currentUser = (rbContacts.GetCurrentContact().Peer.Id == contact?.Peer.Id);

        this.currentUser = currentUser;
        this.withContextMenu = withContextMenu;
        this.withDetails = withDetails;

        presenceColorView = new(currentUser);

        labelField = new()
        {
            X = Pos.Right(presenceColorView) + 1,
            Y = Pos.Left(presenceColorView),
            Width = Dim.Fill(),
            Height = 1,
            TextAlignment = Alignment.Start,
            Text = contact?.Peer.DisplayName
        };

        Width = Dim.Auto();

        Add(presenceColorView, labelField);

        MouseClick += View_MouseClick;
        labelField.MouseClick += View_MouseClick;
        presenceColorView.MouseClick += View_MouseClick;

        Terminal.Gui.Application.MouseEvent += ApplicationMouseEvent;

        SetContact(contact);

        rbContacts.ContactAggregatedPresenceUpdated += RbContacts_ContactAggregatedPresenceUpdated;
    }

    private void ApplicationMouseEvent(object? sender, MouseEvent a) { mousePos = a.Position; }

    private void UpdatePresence(String strPresence)
    {
        Presence? presence;

        strPresence = strPresence.ToLower();
        if (strPresence.Contains(" - "))
        {
            var info = strPresence.Split(" - ");
            presence = rbContacts.CreatePresence(true, info[0], info[1]);
        }
        else
        {
            if (strPresence == "invisible")
                strPresence = "xa";
            presence = rbContacts.CreatePresence(true, strPresence);
        }

        if (presence != null)
        {
            var _ = rbContacts.SetPresenceLevelAsync(presence);
        }
    }

    private void View_MouseClick(object? sender, MouseEventEventArgs e)
    {
        withDetails = true;
        if (withDetails && e.MouseEvent.Flags == MouseFlags.Button1DoubleClicked)
        {
            DisplayPresenceDetails();
            return;
        }

        if(contact != null)
            ContactClick?.Invoke(sender, new ContactEventArgs(contact));

        if (currentUser && withContextMenu && e.MouseEvent.Flags == MouseFlags.Button3Clicked)
            DisplayContextMenu();
    }

    private void DisplayContextMenu()
    {
        String[] stdItems = ["Online", "Away", "Invisible", "Dnd", "", "Busy - Audio", "Busy - Video", "Busy - Sharing"];

        List<MenuItem> menuItems = [];
        for (int i = 0; i < stdItems.Length; i++)
        {
            var name = stdItems[i];
            MenuItem menuItem;
            if (name == "")
#pragma warning disable CS8625
                menuItems.Add(null);
#pragma warning restore CS8625
            else
            {
                char help;
                if (name.Contains(" - "))
                    help = ' ';
                else
                    help = name[0];

                menuItem = new MenuItem(
                                    $"_{name}",
                                    $"{help}"
                                    , () => UpdatePresence(name)
                                    );
                menuItems.Add(menuItem);
            }
        }


        contextMenu = new()
        {
            Position = mousePos,
            MenuItems = new([.. menuItems])
        };

        contextMenu.Show();
    }

    private static String GetPresenceString(Presence presence)
    {
        String calendar;
        if (presence.Resource == "calendar")
            calendar = $" - Until:[{presence.Until:o}]";
        else
            calendar = "";

        return $"Resource:[{presence.Resource}] - Apply:[{presence.Apply}]{calendar}{Rainbow.Util.CR}\t{Emojis.ARROW_RIGHT}{presence.PresenceLevel}{((presence.PresenceDetails?.Length > 0) ? (" [" + presence.PresenceDetails) + "]" : "")}";
    }

    private void DisplayPresenceDetails()
    {
        if (contact == null)
            return;

        // Create dialog
        Dialog dialog = new()
        {
            Title = "Presence details",
            ButtonAlignment = Alignment.Center,
            X = Pos.Center(),
            Y = Pos.Center(),
            Width = Dim.Percent(80),
            Height = Dim.Percent(80)
        };

        LoggerView loggerView = new()
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Width = Dim.Fill(4),
            Height = Dim.Fill(2),
        };

        loggerView.AddColorScheme("blue", LoggerView.Blue, "Resource", "Apply", "Until");
        loggerView.AddColorScheme("red", LoggerView.Red, "DisplayName", "Id", "Jid", "Aggregated Presence");

        String txt = $"DisplayName:[{contact.Peer.DisplayName}]{Rainbow.Util.CR}Id:[{contact.Peer.Id}]{Rainbow.Util.CR}Jid:[{contact.Peer.Jid}]";

        // Add all presences
        var presences = rbContacts.GetPresencesList(contact);
        foreach (var presence in presences.Values)
            txt += $"{Rainbow.Util.CR}{Rainbow.Util.CR}{GetPresenceString(presence)}";

        // Add  aggregated presence
        var aggregatedPresence = rbContacts.GetAggregatedPresence(contact);
        txt += $"{Rainbow.Util.CR}{Rainbow.Util.CR}Aggregated Presence:{Rainbow.Util.CR}{GetPresenceString(aggregatedPresence)}";

        // Create button
        Button button = new() { Text = "OK", IsDefault = true };
        button.Accept += (s, e) => dialog.RequestStop();

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

    internal void SetContact(Contact? contact)
    {
        this.contact = contact;
        currentUser = (rbContacts.GetCurrentContact().Peer.Id == contact?.Peer.Id);
        SetPresence();
    }

    internal void SetPresence()
    {
        if (contact == null)
            return;

        if (!contact.InRoster == true)
        {
            presenceColorView.SetPresence(null);
            labelField.ColorScheme = null;
            labelField.Text = contact.Peer.DisplayName;
            return;
        }

        Presence? aggregatedPresence;
        Dictionary<String, Presence> presences;
        aggregatedPresence = rbContacts.GetAggregatedPresence(contact);
        presences = rbContacts.GetPresencesList(contact);

        presenceColorView.SetPresence(aggregatedPresence);

        if (presences.TryGetValue("calendar", out var calendarPresence))
        {
            if ((calendarPresence.PresenceLevel != PresenceLevel.Online) && calendarPresence.Until > DateTime.UtcNow)
            {
                labelField.ColorScheme = AppointementColorScheme;
                labelField.Text =  $"{Emojis.CALENDAR} {contact.Peer.DisplayName}";
                
                return;
            }
        }
        labelField.ColorScheme = null;
        labelField.Text = contact.Peer.DisplayName;


        labelField.Width = labelField.Text.Length;
    }

    private void RbContacts_ContactAggregatedPresenceUpdated(Presence presence)
    {
        if (presence.Contact?.Peer?.Id == contact?.Peer.Id)
        {
            Terminal.Gui.Application.Invoke(() =>
            {
                SetPresence();
            });
        }
    }
}
