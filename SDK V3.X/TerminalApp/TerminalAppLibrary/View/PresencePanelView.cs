using Rainbow.Model;
using System.Drawing;
using Terminal.Gui;

public class PresencePanelView: View
{
    private readonly Rainbow.Application rbApplication;
    private List<Contact>? contactsList;

    private readonly Object lockDisplay = new();

    private ContextMenu contextMenu = new();

    private readonly Boolean displayTitle;
    private readonly Button? settings;
    private int nbColumns;

    public PresencePanelView(Rainbow.Application rbApplication, List<Contact> contactsList, int nbColumns = 4, Boolean displayTitle = true)
    {
        this.rbApplication = rbApplication;
        this.displayTitle = displayTitle;

        if(displayTitle)
            Title = $"Presence Panel";
        BorderStyle = LineStyle.RoundedDotted;
        Height = Dim.Fill();
        Width = Dim.Fill();

        settings = new()
        {
            X = Pos.Center(),
            Y = 0,
            Text = "Settings",
        };
        settings.MouseClick += Settings_MouseClick;
        Add(settings);

        Update(contactsList, nbColumns);
    }

    private void Settings_MouseClick(object? sender, MouseEventEventArgs e)
    {
        e.Handled = true;


        List<MenuItem> menuItems = [];
        for (int i = 0; i < 8; i++)
        {
            MenuItem menuItem;
            int help = i+1;
            menuItem = new MenuItem(
                                $"On {help} Column{((help == 1) ? "" : "s")}",
                                $""
                                , () => Update(help)
                                );
            menuItems.Add(menuItem);
        }

        contextMenu = new()
        {
            Position = e.MouseEvent.ScreenPosition,
            MenuItems = new([.. menuItems])
        };

        contextMenu.Show();
    }

    public void Update(int nbColumns)
    {
        if (nbColumns != this.nbColumns)
            Update(contactsList, nbColumns);
    }

    public void Update(List<Contact>? contactsList, int nbColumns)
    {
        lock (lockDisplay)
        {
            // Store new info
            this.contactsList = contactsList;
            this.nbColumns = nbColumns;

            if ((contactsList == null) || (contactsList.Count == 0))
            {
                if (displayTitle)
                    Title = $"Presence Panel - No contact";
                return;
            }

            List<View> subViews = [.. Subviews];
            while (subViews.Count > 0)
            {
                var v = subViews[0];
                subViews.RemoveAt(0);

                if(v != settings)
                    Remove(v);
            }

            if (nbColumns < 1)
                nbColumns = 1;

            int nbContacts = contactsList.Count;
            int splitBy = (int)Math.Ceiling((double)nbContacts / nbColumns);

            int alreadyDisplayed = 0;

            int widthColumn = (int)Math.Ceiling((double)100 / nbColumns);

            Dim? percent = Dim.Percent((int)Math.Ceiling((double)100 / nbColumns), DimPercentMode.ContentSize);
            Dim dimWidth = (percent == null) ? 0 : percent - 1;

            if (displayTitle)
                Title = $"Presence Panel - {contactsList.Count} contacts";
            BorderStyle = LineStyle.RoundedDotted;
            Height = Dim.Fill();
            Width = Dim.Fill();


            for (int col = 0; col < nbColumns; col++)
            {
                int splitByToUse = splitBy;
                // We need to have at least one contact by column
                if ( ( nbContacts - ((col+1) * splitBy) - ((nbColumns - col - 1) * (splitBy - 1))) < 0 )
                    splitByToUse--;

                var list = contactsList.Skip(alreadyDisplayed).Take(splitByToUse).ToList();
                alreadyDisplayed += splitByToUse;

                Pos posX;
                if (col == 0)
                    posX = 0;
                else
                    posX = Pos.Percent(widthColumn * col);

                int index = (settings == null) ? 0 : 1;

                foreach (var contact in list)
                {
                    PresenceView view = new(rbApplication, contact)
                    {
                        X = posX + 1,
                        Y = index,
                        Width = dimWidth,
                        Height = 1,
                    };
                    index++;

                    Add(view);
                }

                if (col > 0)
                {
                    var line = new LineView(Orientation.Vertical)
                    {
                        Y = (settings == null) ? 0 : 1,
                        X = posX
                    };
                    Add(line);
                }
                //Add(scrollView);
            }

            
        }
    }
}
