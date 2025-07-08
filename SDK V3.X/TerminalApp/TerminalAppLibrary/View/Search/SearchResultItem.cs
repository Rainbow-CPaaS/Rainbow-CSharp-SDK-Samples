using Rainbow.Model;
using Terminal.Gui.ViewBase;

public class SearchResultItem: View
{
    private readonly PresenceView presenceView;

    public event EventHandler<PeerAndMouseEventArgs>? PeerClick;

    public SearchResultItem(Rainbow.Application application, Contact contact)
    {
        presenceView = new(application, contact)
        {
            X = 1,
            Y = 0,
            Width = Dim.Fill(),
            Height = 1
        };

        Add(presenceView);

        Height = 1;
        Width = Dim.Fill();

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
    }
}

