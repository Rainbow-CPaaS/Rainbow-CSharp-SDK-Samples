using Rainbow.Model;
using System.Collections.ObjectModel;
using System.Drawing;
using Terminal.Gui;

public class Item
{
    public String Id { get; set; }
    public string Text { get; set; }

    public Item()
    {
        Id = Text = "";
    }

    public Item(String id, String text)
    {
        Id = id;
        Text = text;
    }

    public Item(Peer peer)
    {
        if (peer is null)
            Id = Text = "";
        else
        {
            Id = peer.Id;
            Text = peer.DisplayName;
        }
    }

    public override string ToString()
    {
        if (String.IsNullOrEmpty(Text))
            return Id;
        return Text;
    }
}

public class ItemSelector: View
{
    private Label lbl;
    private Label lblSelector;
    private Label lblArrow;
    private PopoverMenu contextMenu;

    private ListViewSelector categoryList;
    private int maxItemLength = 0;

    List<Item>? items;
    Item? itemSelected;


    public event EventHandler<Item>? SelectedItemUpdated;

    public Item? ItemSelected
    {
        get => itemSelected;
    }

    public ItemSelector(String? label = null, List<Item>? items = null, Item? itemSelected = null) 
    {
        lbl = new()
        {
            X = 0,
            Y = 0,
            Height = 1,
            Width = (label is null) ? 0 : label.Length + 1,
            Text = $"{label}:",
            ColorScheme = Tools.ColorSchemeBlackOnGray
        };

        lblSelector = new()
        {
            X = Pos.Right(lbl) + 1,
            Y = 0,
            Height = 1,
            Width = Dim.Func(() => 
                {
                    var textLength = (lblSelector is null)? 0 : lblSelector.Text.Length;
                    var spaceAvailable = Frame.Width - lbl.Frame.Width - ( (lblArrow is null) ? 0 : lblArrow.Frame.Width) - 2;
                    if (spaceAvailable < 0) spaceAvailable = 0;
                    return Math.Min(textLength, spaceAvailable); 
                }),
            ColorScheme = Tools.ColorSchemeBlueOnGray
        };
        lblSelector.MouseClick += View_MouseClick;

        lblArrow = new()
        {
            X = Pos.Right(lblSelector) + 1,
            Y = 0,
            Height = 1,
            Width = 2,
            Text = Emojis.TRIANGLE_DOWN,
            ColorScheme = Tools.ColorSchemeBlueOnGray
        };
        lblArrow.MouseClick += View_MouseClick;

        Add(lbl, lblSelector, lblArrow);

        Height = 1;
        Width = Dim.Fill();

        // Create categoryList
        categoryList = new()
        {
            X = Pos.Func(() => lblSelector.FrameToScreen().X),
            Y = Pos.Func(() => lblSelector.FrameToScreen().Y + 1),
            Width = Dim.Fill(
                        Dim.Func(() =>
                        {
                            var delta = (categoryList?.VerticalScrollBar.Visible == true) ? 4 : 3;
                            var nbElements = maxItemLength + delta;
                            var spaceAvailable = Application.Screen.Width - lblSelector.FrameToScreen().X;
                            return spaceAvailable - Math.Min(nbElements, spaceAvailable);
                        })),
            Height = Dim.Fill(
                         Dim.Func(() =>
                         {
                             var delta = (categoryList?.HorizontalScrollBar.Visible == true) ? 4 : 3;
                             var nbElements = ((this.items is null) ? 0 : this.items.Count) + delta;
                             var spaceAvailable = Application.Screen.Height - lblSelector.FrameToScreen().Y;
                             return spaceAvailable - Math.Min(nbElements, spaceAvailable);
                         })),
            AllowsMarking = false,
            CanFocus = true,
            BorderStyle = LineStyle.Rounded,
            SuperViewRendersLineCanvas = false,
            Source = new ListWrapper<Item>(null),
            ColorScheme = Tools.ColorSchemeBlueOnGray // /!\ Need to be set !!!
        };
        categoryList.VerticalScrollBar.AutoShow = true;
        categoryList.HorizontalScrollBar.AutoShow = true;

        categoryList.SelectedItemChanged += CategoryList_SelectedItemChanged;
        categoryList.SameItemSelected += CategoryList_SameItemSelected;

        // Create contextMenu
        contextMenu = new();
        contextMenu.Add(categoryList);

        this.itemSelected = itemSelected;
        SetItems(items);
    }

    protected override void OnViewportChanged(DrawEventArgs e)
    {
        HideViewSelection();
    }

    public void HideViewSelection()
    {
        Application.Popover?.Hide(contextMenu);
    }

    public void ShowViewSelection()
    {
        if (items?.Count > 0)
        {
            Point position = new Point(lblSelector.FrameToScreen().X, lblSelector.FrameToScreen().Y + 1);
            contextMenu.MakeVisible(position);
        }
    }

    private void View_MouseClick(object? sender, MouseEventArgs e)
    {
        //categoryList.SelectedItem = -1;
        e.Handled = true;
        ShowViewSelection();
    }

    private void CategoryList_SelectedItemChanged(object? sender, ListViewItemEventArgs e)
    {
        HideViewSelection();

        itemSelected = (Item)e.Value;

        Terminal.Gui.Application.Invoke(() =>
        {
            UpdateDisplay();
        });

        //if (SetItemSelected((Item) e.Value))
        SelectedItemUpdated?.Invoke(this, (Item)e.Value);
    }

    private void CategoryList_SameItemSelected(object? sender, EventArgs e)
    {
        HideViewSelection();
    }

    private void UpdateDisplay()
    {
        if (itemSelected is null)
        {
            lblSelector.Text = "NONE";
            lblSelector.Enabled = false;
            lblArrow.Width = 0;
        }
        else
        {
            lblSelector.Text = itemSelected.Text;
            lblSelector.Enabled = true;
            lblArrow.Width = 2;
        }
        HideViewSelection();
    }

    public void SetItems(List<Item>? items)
    {
        this.items = items;

        var previousItemSelected = itemSelected;

        // Check itemSelected value
        if(items?.Count > 0)
        {
            if ((itemSelected is null)
                || (items.FirstOrDefault(i => i.Id == itemSelected.Id) is null))
                itemSelected = items[0];
        }
        else
        {
            itemSelected = null;
        }

        if (items?.Count > 0)
        {
            var strings = items.Select(i => i.ToString());
            maxItemLength = strings.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length;

            categoryList.Source = new ListWrapper<Item>(new ObservableCollection<Item>(items));
            categoryList.SelectedItem = 0;
        }
        else
        {
            maxItemLength = 0;
            categoryList.Source = new ListWrapper<Item>(null);
        }

        // Do we need to update the display ?
        if (previousItemSelected?.Id == itemSelected?.Id)
            return;

        Terminal.Gui.Application.Invoke(() =>
        {
            UpdateDisplay();
        });
        
    }

    public void SetItems(List<Peer>? peers)
    {
        List<Item>? items = null;
        if (peers?.Count > 0)
        {
            items = [];
            foreach (var peer in peers)
                items.Add(new Item(peer));
        }
        SetItems(items);
    }

    public void SetItems(List<String>? elements)
    {
        List<Item>? items = null;
        if (elements?.Count > 0)
        {
            items = [];
            int count = 0;
            foreach(var elm in elements)
            {
                var item = new Item()
                {
                    Id = $"{count++}",
                    Text = elm,
                };
                items.Add(item);
            }
        }
        SetItems(items);
    }


    public Boolean SetItemSelected(Peer? peer)
    {
        if ((peer is not null) && (items is not null))
        {
            var index = items.FindIndex(x => x.Id == peer.Id);
            if (index > 0)
                return SetItemSelected(index);
        }
        return false;
    }

    public Boolean SetItemSelected(Item? item)
    {
        if ( (item is not null) && (items is not null))
        {
            var index = items.FindIndex(x => x.Id == item.Id);
            if (index > 0)
                return SetItemSelected(index);
        }
        return false;
    }

    public Boolean SetItemSelected(int index)
    {
        if((index >= 0) && (items?.Count > index))
        {
            var item = items[index];
            if (item.Id != itemSelected?.Id)
            {
                itemSelected = items[index];
                Terminal.Gui.Application.Invoke(() =>
                {
                    categoryList.SelectedItem = index;
                    UpdateDisplay();
                });
                return true;
            }
        }
        return false;
    }
}
