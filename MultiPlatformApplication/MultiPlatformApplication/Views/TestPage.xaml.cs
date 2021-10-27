using MultiPlatformApplication.Controls;
using MultiPlatformApplication.Effects;
using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using MultiPlatformApplication.ViewModels;
using Rainbow.Model;
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
	public partial class TestPage : CtrlContentPage
	{
		private LoginViewModel vm;

		public ContextMenuModel MessageUrgency { get; private set; } = new ContextMenuModel();

		public TestPage()
		{
			InitializeComponent();

			ImageLogo.Command = new RelayCommand<object>(new Action<object>(ImageLogoCommand));

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
			Color backgroundColor;
			String title;
			String label;
			String imageSourceId;

			ContextMenuItemModel  messageUrgencyModelItem;

			String urgencyType;

			urgencyType = UrgencyType.High.ToString();
			Helper.GetUrgencyInfo(urgencyType, out backgroundColor, out color, out title, out label, out imageSourceId);
			messageUrgencyModelItem = new ContextMenuItemModel() { Id = urgencyType, ImageSourceId = imageSourceId, Title = title, Description = label, TextColor = color };
			MessageUrgency.Add(messageUrgencyModelItem);

			urgencyType = UrgencyType.Middle.ToString();
			Helper.GetUrgencyInfo(urgencyType, out backgroundColor, out color, out title, out label, out imageSourceId);
			messageUrgencyModelItem = new ContextMenuItemModel() { Id = urgencyType, ImageSourceId = imageSourceId, Title = title, Description = label, TextColor = color };
			MessageUrgency.Add(messageUrgencyModelItem);

			urgencyType = UrgencyType.Low.ToString();
			Helper.GetUrgencyInfo(urgencyType, out backgroundColor, out color, out title, out label, out imageSourceId);
			messageUrgencyModelItem = new ContextMenuItemModel() { Id = urgencyType, ImageSourceId = imageSourceId, Title = title, Description = label, TextColor = color };
			MessageUrgency.Add(messageUrgencyModelItem);

			urgencyType = UrgencyType.Std.ToString();
			Helper.GetUrgencyInfo(urgencyType, out backgroundColor, out color, out title, out label, out imageSourceId);
			messageUrgencyModelItem = new ContextMenuItemModel() { Id = urgencyType, ImageSourceId = imageSourceId, Title = title, Description = label, TextColor = color };
			MessageUrgency.Add(messageUrgencyModelItem);
		}


        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
			if (e.SelectedItemIndex != -1)
			{
				Popup.HideCurrentContextMenu();
				
				ListView.SelectedItem = null;

				Helper.HapticFeedbackLongPress();

				SetMessageUrgencySelectedItem(e.SelectedItemIndex);
			}
        }

		private void ShowContextMenu()
		{
			Popup.Show("ContextMenuMessageUrgency", "ImageLogo");
		}


		private void ImageLogoCommand(object obj)
        {
			ShowContextMenu();
		}

        protected override void OnAppearing()
		{
			vm.Initialize();
			base.OnAppearing();
		}
	}
}