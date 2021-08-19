using Rainbow;
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
	public partial class MessageOtherUserWithAvatar : ContentView
	{
		public event EventHandler<EventArgs> ActionMenuToDisplay;

		public MessageOtherUserWithAvatar ()
		{
			InitializeComponent ();

			MessageContent.ActionMenuToDisplay += MessageContent_ButtonActionUsed;
		}

		private void MessageContent_ButtonActionUsed(object sender, EventArgs e)
		{
			ActionMenuToDisplay?.Raise(this, null);
		}
	}
}