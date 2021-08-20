using MultiPlatformApplication.Effects;
using System;
using Xamarin.Forms;

namespace MultiPlatformApplication.Events
{
    public class TouchActionEventArgs : EventArgs
    {
        public TouchActionEventArgs(long id, TouchActionType type, Point location, bool isInContact, TouchMouseButton touchMouseButton = TouchMouseButton.Unknown)
        {
            Id = id;
            Type = type;
            Location = location;
            IsInContact = isInContact;
            MouseButton = touchMouseButton;
        }

        public long Id { private set; get; }

        public TouchActionType Type { private set; get; }

        public Point Location { private set; get; }

        public bool IsInContact { private set; get; }

        public TouchMouseButton MouseButton { private set; get; }
    }
}
