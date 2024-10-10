using Rainbow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

public class PeerAndMouseEventEventArgs(Peer peer, MouseEvent me) : MouseEventEventArgs(me)
{
    public Peer Peer { get; set; } = peer;
}
