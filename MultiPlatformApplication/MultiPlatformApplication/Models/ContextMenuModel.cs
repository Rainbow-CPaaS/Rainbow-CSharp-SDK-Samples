using MultiPlatformApplication.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms;

namespace MultiPlatformApplication.Models
{
    public class ContextMenuModel : ObservableObject
    {
        private ObservableCollection<ContextMenuItemModel > items;

        public ObservableCollection<ContextMenuItemModel > Items
        {
            get { return items; }
            set { SetProperty(ref items, value); }
        }

        public ContextMenuModel()
        {
            Items = new ObservableCollection<ContextMenuItemModel >();
        }

        public void Clear()
        {
            Items.Clear();
        }

        public void Add(ContextMenuItemModel  item)
        {
            Items.Add(item);
        }
    }

    public class ContextMenuItemModel : ObservableObject
    {
        public String id;
        public String imageSourceId;
        public String title;
        public String description;
        public Color textColor;
        public bool isSelected;

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

    }
}
