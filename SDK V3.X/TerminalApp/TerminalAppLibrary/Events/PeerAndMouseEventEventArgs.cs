using Rainbow.Model;
using Terminal.Gui;

public class PeerAndMouseEventEventArgs(Peer peer, MouseEvent me) : MouseEventEventArgs(me)
{
    public Peer Peer { get; set; } = peer;
}
