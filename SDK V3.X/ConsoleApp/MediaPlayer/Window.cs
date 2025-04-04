using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMediaPlayer
{
    public class Window
    {
        public IntPtr Handle = IntPtr.Zero;
        public uint Id = 0;
        public IntPtr Renderer = IntPtr.Zero;
        public IntPtr Texture = IntPtr.Zero;

        public Boolean FullScreen = false; // True if the window is full screen

        public Boolean NeedRendereUpdate = false; // True if it's necessary to update the renderer

        public Boolean VideoStopped = true;

    }
}
