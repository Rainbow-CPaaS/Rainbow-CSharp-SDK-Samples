using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using MultiPlatformApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TestPage : ContentPage
	{
		private LoginViewModel vm;

		TapGestureRecognizer tapGestureRecognizer;

		public ContextMenuModel MessageUrgency { get; private set; } = new ContextMenuModel();

		public TestPage()
		{
			InitializeComponent();

			tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;
			ImageLogo.GestureRecognizers.Add(tapGestureRecognizer);

			// Set selected item
			SetMessageUrgencyModel();
			SetMessageUrgencySelectedItem(3);

			ListView.ItemSelected += ListView_ItemSelected;

			ContextMenuMessageUrgency.BindingContext = this;

			vm = new LoginViewModel();
			BindingContext = vm;
		}

		private void SetMessageUrgencySelectedItem(int selectedIndex)
        {
			if (selectedIndex == -1)
				return;

			if(MessageUrgency?.Items?.Count > selectedIndex)
            {
				for(int i=0; i< MessageUrgency.Items.Count; i++)
					MessageUrgency.Items[i].IsSelected = (i == selectedIndex);
			}
        }

		private void SetMessageUrgencyModel()
        {
			MessageUrgency.Clear();

			Color color;
			ContextMenuItemModel  messageUrgencyModelItem;

			color = Helper.GetResourceDictionaryById<Color>("ColorUrgencyEmergency");
			messageUrgencyModelItem = new ContextMenuItemModel () { Id = "Emergency", ImageSourceId = "Font_Fire|" + color.ToHex(), Title = Helper.GetLabel("emergencyAlert"), Description = Helper.GetLabel("emergencyAlertInfo"), TextColor = color };
			MessageUrgency.Add(messageUrgencyModelItem);

			color = Helper.GetResourceDictionaryById<Color>("ColorUrgencyImportant");
			messageUrgencyModelItem = new ContextMenuItemModel () { Id = "Important", ImageSourceId = "Font_ExclamationTriangle|" + color.ToHex(), Title = Helper.GetLabel("warningAlert"), Description = Helper.GetLabel("warningAlertInfo"), TextColor = color };
			MessageUrgency.Add(messageUrgencyModelItem);

			color = Helper.GetResourceDictionaryById<Color>("ColorUrgencyInformation");
			messageUrgencyModelItem = new ContextMenuItemModel () { Id = "Information", ImageSourceId = "Font_Lightbulb|" + color.ToHex(), Title = Helper.GetLabel("notifyAlert"), Description = Helper.GetLabel("notifyAlertInfo"), TextColor = color };
			MessageUrgency.Add(messageUrgencyModelItem);

			color = Color.FromHex("#000000");
			messageUrgencyModelItem = new ContextMenuItemModel () { Id = "Standard", ImageSourceId = "Font_CommentAlt|" + color.ToHex(), Title = Helper.GetLabel("standardAlert"), Description = Helper.GetLabel("standardAlertInfo"), TextColor = color };
			MessageUrgency.Add(messageUrgencyModelItem);
		}


        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
			SetMessageUrgencySelectedItem(e.SelectedItemIndex);
			ListView.SelectedItem = null;
			
			HideContextMenu();
        }

        private void HideContextMenu()
        {
			ContextMenuMessageUrgency.IsVisible = false;
		}

		private void DisplayContextMenu()
        {
			ContextMenuMessageUrgency.IsVisible = true;
		}

	
		private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
			DisplayContextMenu();
		}

        protected override void OnAppearing()
		{
			vm.Initialize();
			base.OnAppearing();
		}
	}
}