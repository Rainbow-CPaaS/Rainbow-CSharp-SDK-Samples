using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace InstantMessaging
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConversationStreamPage : ContentPage
    {
        private ConversationStreamViewModel vm;

        private String peerId;

        public ConversationStreamPage(String peerId)
        {
            this.peerId = peerId;
            
            InitializeComponent();

            vm = new ConversationStreamViewModel(peerId);
            BindingContext = vm;
        }
    }
}