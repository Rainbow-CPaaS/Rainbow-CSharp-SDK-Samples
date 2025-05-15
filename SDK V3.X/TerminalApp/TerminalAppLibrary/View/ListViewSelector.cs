using Terminal.Gui;

public class ListViewSelector: ListView
{
    public event EventHandler<EventArgs> SameItemSelected;

    public override bool OnSelectedChanged()
    {
        var result = base.OnSelectedChanged();
        if (!result)
            SameItemSelected?.Invoke(this, EventArgs.Empty);
        return result;
    }
}

