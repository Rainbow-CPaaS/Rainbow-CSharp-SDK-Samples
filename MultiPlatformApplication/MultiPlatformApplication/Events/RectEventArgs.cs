using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MultiPlatformApplication.Events
{
    public class RectEventArgs : EventArgs
    {
        public RectEventArgs(Rect Rect)
        {
            this.Rect = Rect;
        }

        public Rect Rect { private set; get; }
    }
}
