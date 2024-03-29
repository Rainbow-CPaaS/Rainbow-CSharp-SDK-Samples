﻿using MultiPlatformApplication.Helpers;
using Rainbow;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace MultiPlatformApplication.Models
{
    public class ContextMenuModel : ObservableObject
    {
        internal ICommand Command;
        internal ICommand CommandSelection;
        private ObservableCollection<ContextMenuItemModel > items;

        public event EventHandler<EventArgs> ItemVisibilityChanged;


        public ObservableCollection<ContextMenuItemModel > Items
        {
            get { return items; }
            set { SetProperty(ref items, value); }
        }

        public ContextMenuModel()
        {
            Items = new ObservableCollection<ContextMenuItemModel >();
            CommandSelection = new RelayCommand<object>(CommandSelectionAction);
        }

        private void CommandSelectionAction(object obj)
        {
            if (obj is View view)
            {
                if (view.BindingContext is ContextMenuItemModel model)
                {
                    if (Command?.CanExecute(model.Id) == true)
                        Command.Execute(model.Id);
                }
            }
        }

        internal String GetSelectedItemId()
        {
            ContextMenuItemModel item = GetSelectedItem();
            if(item != null)
                return item.id;
            return null;
        }

        internal ContextMenuItemModel GetSelectedItem()
        {
            if (Items != null)
            {
                foreach (var item in Items)
                {
                    if (item.IsSelected)
                        return item;
                }
            }
            return null;
        }

        internal void SetSelectedItem(String itemId)
        {
            if (Items != null)
            {
                foreach (var item in Items)
                    item.IsSelected = item.Id == itemId;
            }
        }

        public void Clear()
        {
            if (Items != null)
            {
                foreach (var item in Items)
                    item.PropertyChanged -= Item_PropertyChanged;

                Items.Clear();
            }
        }

        public void Add(ContextMenuItemModel  item)
        {
            if (item != null)
            {
                item.Command = CommandSelection;
                Items.Add(item);
                item.PropertyChanged += Item_PropertyChanged;
            }
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsVisible")
                ItemVisibilityChanged.Raise(this, null);
        }
    }

    public class ContextMenuItemModel : ObservableObject
    {
        public String id;
        public String imageSourceId;
        public String title;
        public String description;
        public Color textColor;
        public bool isSelected = false;
        public bool isVisible = true;

        public ICommand command;

        public String Id
        {
            get
            {
                return id;
            }

            set
            {
                SetProperty(ref id, value);
            }
        }

        public String ImageSourceId
        {
            get
            {
                return imageSourceId;
            }

            set
            {
                SetProperty(ref imageSourceId, value);
            }
        }

        public String Title
        {
            get
            {
                return title;
            }

            set
            {
                SetProperty(ref title, value);
            }
        }

        public String Description
        {
            get
            {
                return description;
            }

            set
            {
                SetProperty(ref description, value);
            }
        }

        public Color TextColor
        {
            get
            {
                return textColor;
            }

            set
            {
                SetProperty(ref textColor, value);
            }
        }

        public bool IsSelected
        {
            get
            {
                return isSelected;
            }

            set
            {
                SetProperty(ref isSelected, value);
            }
        }

        public bool IsVisible
        {
            get
            {
                return isVisible;
            }

            set
            {
                SetProperty(ref isVisible, value);
            }
        }

        //  /!\ Must be PUBLIC !!!
        public ICommand Command
        {
            get
            {
                return command;
            }

            set
            {
                SetProperty(ref command, value);
            }
        }

    }
}
