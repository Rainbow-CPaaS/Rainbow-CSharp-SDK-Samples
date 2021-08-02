using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MultiPlatformApplication.Models
{
    public class MessageInputModel
    {
        public String UrgencySelection { get; set; }

        public ICommand UrgencyCommand { get; set; }

        public ICommand AttachmentCommand { get; set; }

        public ICommand SendCommand { get; set; }

        public ICommand ValidationCommand { get; set; }

        public String ValidationKeyModifier { get; set; }
    }
}
