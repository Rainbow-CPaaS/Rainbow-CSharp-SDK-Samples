using InstantMessaging.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Rainbow.Helpers;

namespace InstantMessaging.CustomCells
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MessageViewCellOtherUser : ViewCell
    {
        public MessageViewCellOtherUser()
        {
            InitializeComponent();
        }

        private void FileAttachment_Tapped(object sender, EventArgs e)
        {
            if (e is Xamarin.Forms.TappedEventArgs)
            {
                TappedEventArgs tap = e as Xamarin.Forms.TappedEventArgs;

                if (tap.Parameter is Message)
                {
                    Message message = tap.Parameter as Message;
                    if (message != null)
                    {
                        //TODO
                    }
                }
            }
        }

    }
}