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

            // Define MessageInput Constraints
            RelativeLayout.SetYConstraint(MessageInput, Constraint.RelativeToParent((rl) =>
            {
                if ((rl.Height != -1) && (MessageInput.Height != -1))
                {
                    return rl.Height - MessageInput.Height;
                }
                return -1;
            }));

            RelativeLayout.SetWidthConstraint(MessageInput, Constraint.RelativeToParent((rl) =>
            {
                return rl.Width;
            }));


            // Define ContentViewPlatformSpecific Constraints
            RelativeLayout.SetWidthConstraint(ContentViewPlatformSpecific, Constraint.RelativeToParent((rl) =>
            {
                return rl.Width;
            }));

            RelativeLayout.SetHeightConstraint(ContentViewPlatformSpecific, Constraint.RelativeToParent((rl) =>
            {
                if ((rl.Height != -1) && (MessageInput.Height != -1))
                {
                    return rl.Height - MessageInput.Height;
                }
                return -1;
            }));

            vm = new MessagesStreamViewModel(RootElement);
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