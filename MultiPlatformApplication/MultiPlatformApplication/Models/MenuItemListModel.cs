using MultiPlatformApplication.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MultiPlatformApplication.Models
{
    public class MenuItemListModel: ObservableObject
    {
        private double defaultMenuItemImageSize;
        private double defaultMenuItemHeightRequest;
        private double defaultMenuItemWidthRequest;

        private List<MenuItemModel> itemsOriginal;
        private ObservableRangeCollection<MenuItemModel> items;

        private Boolean textVisible;
        private Boolean textVisibleForSelectedItem;

        public ICommand command;

        public ObservableRangeCollection<MenuItemModel> Items
        {
            get { return items; }
            set { SetProperty(ref items, value); }
        }

        public Boolean TextVisible
        {
            get { return textVisible; }
            set { 
                SetProperty(ref textVisible, value);
                UpdateTextOfItems();
            }
        }

        public Boolean TextVisibleForSelectedItem
        {
            get { return textVisibleForSelectedItem; }
            set { 
                SetProperty(ref textVisibleForSelectedItem, value);
                UpdateTextOfItems();
            }
        }

        public ICommand Command
        {
            get { return command; }
            set { SetProperty(ref command, value); }
        }

        public void SetItemSelected(int index)
        {
            if ( (items?.Count > 0) && (items?.Count < index) && index >= 0 )
                SetItemSelected(items[index].Id);
        }

        public void SetItemSelected(String id)
        {
            if (String.IsNullOrEmpty(id))
                return;

            Boolean itemSelected = false;

            if (items?.Count > 0)
            {
                if (TextVisibleForSelectedItem && !TextVisible)
                {
                    foreach (MenuItemModel item in items)
                    {
                        if (item.Id == id)
                        {
                            itemSelected = true;

                            item.IsSelected = true;
                            item.Label = GetOriginalLabelOfItem(id);
                        }
                        else
                        {
                            item.IsSelected = false;
                            item.Label = "";
                        }
                    }
                }
                else
                {
                    foreach (MenuItemModel item in items)
                    {
                        if (item.Id == id)
                            itemSelected = true;
                        item.IsSelected = (item.Id == id);
                    }
                }

                if (itemSelected)
                {
                    if (Command != null && Command.CanExecute(id))
                        Command.Execute(id);
                }
            }
        }

        /// <summary>
        /// The only way to add correctly items in the menu
        /// </summary>
        /// <param name="item">MenuItem to add</param>
        /// <param name="index">Index to add this item. (-1 by defualt => to the end of the menu) </param>
        public void AddItem(MenuItemModel item, int index = -1)
        {
            if ( (index > Items.Count) || (index < -1) )
                return;

            MenuItemModel originalItem = new MenuItemModel(item);


            // Set default values if necessary
            if (originalItem.ImageSize == 0)
                originalItem.ImageSize = defaultMenuItemImageSize;

            if (originalItem.HeightRequest == 0)
                originalItem.HeightRequest = defaultMenuItemHeightRequest;

            if (originalItem.WidthRequest == 0)
                originalItem.WidthRequest = defaultMenuItemWidthRequest;

            // Create displayItem using originalItem
            MenuItemModel displayItem = new MenuItemModel(originalItem);

            displayItem.Command = new RelayCommand<object>(new Action<object>(ItemCommand));


            // Change Text of display item if necessary
            if(originalItem.IsSelected)
            {
                if(!TextVisibleForSelectedItem)
                    displayItem.Label = "";
            }
            else if (!TextVisible)
                displayItem.Label = "";

            if (index == -1)
            {
                itemsOriginal.Add(originalItem);
                Items.Add(displayItem);
            }
            else
            {
                itemsOriginal.Insert(index, originalItem);
                Items.Insert(index, displayItem);
            }


        }

        /// <summary>
        /// To set default Menu Image Size, default Menu Item Height Request and default Menu Item Witdh Request
        /// 
        /// Must be used before to add any element in the Menu
        /// </summary>
        /// <param name="defaultMenuImageSize">To set default Menu Image Size - 30 by default</param>
        /// <param name="defaultMenuItemHeightRequest">To set default Menu Item Height Request - 50 by default</param>
        /// <param name="defaultMenuItemWidthRequest">To set default MenuItem Witdh Request - 100 by default</param>
        public void SetDefaulMenuItemtSize(double defaultMenuImageSize, double defaultMenuItemHeightRequest, double defaultMenuItemWidthRequest)
        {
            this.defaultMenuItemImageSize = defaultMenuImageSize;
            this.defaultMenuItemHeightRequest = defaultMenuItemHeightRequest;
            this.defaultMenuItemWidthRequest = defaultMenuItemWidthRequest;
        }

        public MenuItemListModel()
        {
            this.defaultMenuItemImageSize = 30;
            this.defaultMenuItemHeightRequest = 50;
            this.defaultMenuItemWidthRequest = 100;

            itemsOriginal = new List<MenuItemModel>();

            TextVisible = true;
            TextVisibleForSelectedItem = true;

            Items = new ObservableRangeCollection<MenuItemModel>();
        }

        private void ItemCommand(object obj)
        {
            if ( (obj != null) && (obj is String) )
                SetItemSelected((String)obj);


            if (Command != null && Command.CanExecute(obj))
                Command.Execute(obj);
        }

        private void UpdateTextOfItems()
        {
            if(itemsOriginal?.Count > 0)
            {
                for(int index=0; index < itemsOriginal.Count; index++)
                {
                    if(TextVisible)
                    {
                        items[index].Label = itemsOriginal[index].Label;
                    }
                    else
                    {
                        if(TextVisibleForSelectedItem)
                        {
                            if(items[index].IsSelected)
                                items[index].Label = itemsOriginal[index].Label;
                            else
                                items[index].Label = "";
                        }
                        else
                            items[index].Label = "";
                    }
                }
            }
        }

        private String GetOriginalLabelOfItem(string id)
        {
            foreach (MenuItemModel item in itemsOriginal)
            {
                if (item.Id == id)
                    return item.Label;
            }
            return "";
        }

    }
}
