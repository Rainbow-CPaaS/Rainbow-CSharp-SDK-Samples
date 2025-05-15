using System.Drawing;
using Terminal.Gui;

public  class ScrollableView: View
{
    private int nbVerticalElements = 0;

    public ScrollableView()
    {
        X = 0;
        Y = 0;
        Width = Dim.Fill();
        Height = Dim.Fill();
        CanFocus = true;
        ContentSizeTracksViewport = true;
    }

    public void SetNbVerticalElements(int nb)
    {
        if (nbVerticalElements != nb)
        { 
            nbVerticalElements = nb;
            UpdateContentSize();
        }
    }

    private void UpdateContentSize()
    {
        SetContentSize(new Size(Viewport.Width, (nbVerticalElements < Viewport.Height) ? Viewport.Height : nbVerticalElements));
    }

    protected override bool OnMouseEvent(MouseEventArgs me)
    {
        if (!HasFocus && CanFocus)
        {
            SetFocus();
        }

        if (me.Flags == MouseFlags.WheeledDown)
        {
            if (Viewport.Y + Viewport.Height < GetContentSize().Height)
            {
                ScrollVertical(1);
            }

            return true;
        }

        if (me.Flags == MouseFlags.WheeledUp)
        {
            ScrollVertical(-1);

            return true;
        }

        if (me.Flags == MouseFlags.WheeledRight)
        {
            if (Viewport.X + Viewport.Width < GetContentSize().Width)
            {
                ScrollHorizontal(1);
            }

            return true;
        }
        return false;
    }

    /// <inheritdoc />
    protected override void OnViewportChanged(DrawEventArgs e)
    {
        UpdateContentSize();
    }
}
