using MultiPlatformApplication.Controls;
using MultiPlatformApplication.Effects;
using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using Rainbow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage3 : CtrlContentPage
    {
        int counter = 0;
        int dynamicContextMenuCounter = 0;
        Boolean newPopupAdded = false;

        ContextMenuModel contextMenuModel;
        View dynamicContextMenu = null;

        public TestPage3()
        {
            InitializeComponent();

            CustomButton1.Command = new RelayCommand<object>(new Action<object>(CustomButton1Command));
            CustomButton2.Command = new RelayCommand<object>(new Action<object>(CustomButton2Command));

            InformationButton1.Command = new RelayCommand<object>(new Action<object>(InformationButton1Command));
            InformationButton2.Command = new RelayCommand<object>(new Action<object>(InformationButton2Command));

            AddPopupButton.Command = new RelayCommand<object>(new Action<object>(AddPopupButtonCommand));
            DisplayPopupButton.Command = new RelayCommand<object>(new Action<object>(DisplayPopupButtonCommand));
            UpdatePopupModelButton.Command = new RelayCommand<object>(new Action<object>(UpdatePopupModelButtonCommand));

            Logo.Command = new RelayCommand<object>(new Action<object>(LogoCommand));
        }

        private void LogoCommand(object obj)
        {
            Popup.Show("ContextMenu1", null, LayoutAlignment.Center, false,  LayoutAlignment.Start, false, new Point(0,10));
        }

        private void CustomButton1Command(object obj)
        {
            if (counter % 4 == 0)
                Popup.Show("ContextMenu1", "CustomButton1", LayoutAlignment.Start, false, LayoutAlignment.End, true, new Point(), -1);
            else if (counter % 4 == 1)
                Popup.Show("ContextMenu1", "CustomButton1", LayoutAlignment.End, false, LayoutAlignment.End, true, new Point(), -1);
            else if (counter % 4 == 2)
                Popup.Show("ContextMenu1", "CustomButton1", LayoutAlignment.End, false, LayoutAlignment.Start, true, new Point(), -1);
            else
                Popup.Show("ContextMenu1", "CustomButton1", LayoutAlignment.Start, false, LayoutAlignment.Start, true, new Point(), -1);

            counter++;
        }

        private void CustomButton2Command(object obj)
        {
            if (counter % 4 == 0)
                Popup.Show("ContextMenu2", "CustomButton2", LayoutAlignment.Start, false, LayoutAlignment.End, false, new Point(), -1);
            else if (counter % 4 == 1)
                Popup.Show("ContextMenu2", "CustomButton2", LayoutAlignment.End, false, LayoutAlignment.End, false, new Point(), -1);
            else if (counter % 4 == 2)
                Popup.Show("ContextMenu2", "CustomButton2", LayoutAlignment.End, false, LayoutAlignment.Start, false, new Point(), -1);
            else
                Popup.Show("ContextMenu2", "CustomButton2", LayoutAlignment.Start, false, LayoutAlignment.Start, false, new Point(), -1);

            counter++;
        }

        private void InformationButton1Command(object obj)
        {
            Popup.Show("Information1", "InformationButton1", 5000);
        }

        private void InformationButton2Command(object obj)
        {
            Popup.Show("Information2", "InformationButton2", 2000);
        }


        private void UpdateLabel()
        {
            if (newPopupAdded)
                AddPopupButton.Text = "Remove Popup added";
            else
                AddPopupButton.Text = "Add new Popup";
        }

        private void AddPopupButtonCommand(object obj)
        {
            if (!newPopupAdded)
            {
                if (dynamicContextMenuCounter % 3 == 2)
                {
                    contextMenuModel = new ContextMenuModel();
                    contextMenuModel.Add(new ContextMenuItemModel() { Id = "firstname", Title = Helper.SdkWrapper.GetLabel("firstnameOrder") });
                    contextMenuModel.Add(new ContextMenuItemModel() { Id = "lastname", Title = Helper.SdkWrapper.GetLabel("lastnameOrder") });
                    contextMenuModel.Add(new ContextMenuItemModel() { Id = "company", Title = Helper.SdkWrapper.GetLabel("companyOrder") });

                    dynamicContextMenu = new Controls.ContextMenu { AutomationId = "DynamicContextMenu", WidthRequest = 140 };
                    dynamicContextMenu.BindingContext = contextMenuModel;
                }
                else if (dynamicContextMenuCounter % 3 == 1)
                {
                    Color color;
                    String colorHex;
                    String imageSourceId;

                    contextMenuModel = new ContextMenuModel();

                    color = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageOtherUserFont");
                    colorHex = color.ToHex();

                    imageSourceId = "Font_PencilAlt|" + colorHex;
                    contextMenuModel.Add(new ContextMenuItemModel() { Id = "edit", ImageSourceId = imageSourceId, Title = Helper.SdkWrapper.GetLabel("edit") });

                    imageSourceId = "Font_FileDownload|" + colorHex;
                    contextMenuModel.Add(new ContextMenuItemModel() { Id = "download", ImageSourceId = imageSourceId, Title = Helper.SdkWrapper.GetLabel("download") });

                    imageSourceId = "Font_Reply|" + colorHex;
                    contextMenuModel.Add(new ContextMenuItemModel() { Id = "reply", ImageSourceId = imageSourceId, Title = Helper.SdkWrapper.GetLabel("replyToMessage") });

                    imageSourceId = "Font_ArrowRight|" + colorHex;
                    contextMenuModel.Add(new ContextMenuItemModel() { Id = "forward", ImageSourceId = imageSourceId, Title = Helper.SdkWrapper.GetLabel("forwardMessage") });

                    imageSourceId = "Font_Copy|" + colorHex;
                    contextMenuModel.Add(new ContextMenuItemModel() { Id = "copy", ImageSourceId = imageSourceId, Title = Helper.SdkWrapper.GetLabel("copy") });

                    imageSourceId = "Font_TrashAlt|" + colorHex;
                    contextMenuModel.Add(new ContextMenuItemModel() { Id = "delete", ImageSourceId = imageSourceId, Title = Helper.SdkWrapper.GetLabel("delete") });

                    imageSourceId = "Font_CloudDownloadAlt|" + colorHex;
                    contextMenuModel.Add(new ContextMenuItemModel() { Id = "save", ImageSourceId = imageSourceId, Title = Helper.SdkWrapper.GetLabel("save") });

                    dynamicContextMenu = new Controls.ContextMenu { AutomationId = "DynamicContextMenu", WidthRequest = 180 };
                    dynamicContextMenu.BindingContext = contextMenuModel;
                }
                else if (dynamicContextMenuCounter % 3 == 0)
                {
                    Color color;
                    Color backgroundColor;
                    String title;
                    String label;
                    String imageSourceId;

                    ContextMenuItemModel messageUrgencyModelItem;

                    String urgencyType;

                    contextMenuModel = new ContextMenuModel();

                    urgencyType = UrgencyType.High.ToString();
                    Helper.GetUrgencyInfo(urgencyType, out backgroundColor, out color, out title, out label, out imageSourceId);
                    messageUrgencyModelItem = new ContextMenuItemModel() { Id = urgencyType, ImageSourceId = imageSourceId, Title = title, Description = label, TextColor = color };
                    contextMenuModel.Add(messageUrgencyModelItem);

                    urgencyType = UrgencyType.Middle.ToString();
                    Helper.GetUrgencyInfo(urgencyType, out backgroundColor, out color, out title, out label, out imageSourceId);
                    messageUrgencyModelItem = new ContextMenuItemModel() { Id = urgencyType, ImageSourceId = imageSourceId, Title = title, Description = label, TextColor = color };
                    contextMenuModel.Add(messageUrgencyModelItem);

                    urgencyType = UrgencyType.Low.ToString();
                    Helper.GetUrgencyInfo(urgencyType, out backgroundColor, out color, out title, out label, out imageSourceId);
                    messageUrgencyModelItem = new ContextMenuItemModel() { Id = urgencyType, ImageSourceId = imageSourceId, Title = title, Description = label, TextColor = color };
                    contextMenuModel.Add(messageUrgencyModelItem);

                    urgencyType = UrgencyType.Std.ToString();
                    Helper.GetUrgencyInfo(urgencyType, out backgroundColor, out color, out title, out label, out imageSourceId);
                    messageUrgencyModelItem = new ContextMenuItemModel() { Id = urgencyType, ImageSourceId = imageSourceId, Title = title, Description = label, TextColor = color };
                    contextMenuModel.Add(messageUrgencyModelItem);

                    dynamicContextMenu = new Controls.ContextMenu { AutomationId = "DynamicContextMenu", WidthRequest = 280 };
                    dynamicContextMenu.BindingContext = contextMenuModel;
                }

                // Increase counter
                dynamicContextMenuCounter++;

                if (dynamicContextMenu != null)
                {
                    Popup.Add(this, dynamicContextMenu, PopupType.ContextMenu);
                    newPopupAdded = !newPopupAdded;
                }
            }
            else
            {
                Popup.Remove(this, "DynamicContextMenu");
                newPopupAdded = !newPopupAdded;

                dynamicContextMenu = null;
            }

            UpdateLabel();
            DisplayPopupButton.IsEnabled = newPopupAdded;
            UpdatePopupModelButton.IsEnabled = newPopupAdded;
        }

        private void DisplayPopupButtonCommand(object obj)
        {
            if (newPopupAdded)
            {
                Popup.Show("DynamicContextMenu", "DisplayPopupButton");
            }
        }

        private void UpdatePopupModelButtonCommand(object obj)
        {
            if(contextMenuModel?.Items?.Count >= 2)
            {
                contextMenuModel.Items[1].IsVisible = false;
            }
        }
        
    }
}