using MultiPlatformApplication.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace MultiPlatformApplication.Models
{
    public class BasicListModel: ObservableObject
    {
        private BasicListItemModel selectedItem;
        private ObservableCollection<BasicListItemModel> items;

        private ICommand selectionCommand;

        public BasicListItemModel SelectedItem
        {
            get { return selectedItem; }
            set {

                if (value != null)
                    SelectionCommand?.Execute(value);

                selectedItem = null;
                SetProperty(ref selectedItem, value);
            }
        }

        public ObservableCollection<BasicListItemModel> Items
        {
            get { return items; }
            set { SetProperty(ref items, value); }
        }

        public ICommand SelectionCommand
        {
            get { return selectionCommand; }
            set { SetProperty(ref selectionCommand, value); }
        }


        public BasicListModel()
        {
            Items = new ObservableCollection<BasicListItemModel>();
        }

        public void Clear()
        {
            Items.Clear();
        }

        public void Add(BasicListItemModel item)
        {
            Items.Add(item);
        }
    }

    public class BasicListItemModel: ObservableObject
    {

        private String id;
        private String label;

        public String Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        public String Label
        {
            get { return label; }
            set { SetProperty(ref label, value); }
        }
    }
}
