using Rainbow.Model;
using Terminal.Gui;

public class PeerAndMouseEventArgs(Peer peer, MouseEventArgs me)
{
    public Peer Peer { get; set; } = peer;
    public MouseEventArgs MouseEvent { get; set; } = me;
}
