using MultiPlatformApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Controls
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MessagesStream : ContentView
	{
        String conversationId;
        MessagesStreamViewModel vm;

        public MessagesStream ()
		{
			InitializeComponent ();

            vm = new MessagesStreamViewModel(ContentViewPlatformSpecific);
            this.BindingContextChanged += MessagesStream_BindingContextChanged;
		}

        private void MessagesStream_BindingContextChanged(object sender, EventArgs e)
        {
            if(BindingContext != null)
            {
                String id = (String)BindingContext;
                if( (!String.IsNullOrEmpty(id)) && (id != conversationId) )
                {
                    conversationId = id;
                    vm.Initialize(conversationId);
                }
            }
        }

    }
}