using Rainbow.Model;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Text;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

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
    public const String LBL_NONE = "NONE";

    private View lbl;
    private View lblSelector;
    private View lblArrow;
    private PopoverMenu contextMenu;

    private ListView categoryList;
    private List<Item>? categoryListItems;
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
            SchemeName = Tools.DEFAULT_SCHEME_NAME,
            HotKeySpecifier = (Rune)0xffff,
            Text = $"{label}:",
        };

        lblSelector = new()
        {
            X = Pos.Right(lbl) + 1,
            Y = 0,
            Height = 1,
            Width = Dim.Func((view) => 
                {
                    var textLength = (lblSelector is null)? 0 : lblSelector.Text.Length;
                    var spaceAvailable = Frame.Width - lbl.Frame.Width - ( (lblArrow is null) ? 0 : lblArrow.Frame.Width) - 2;
                    if (spaceAvailable < 0) spaceAvailable = 0;
                    return Math.Min(textLength, spaceAvailable); 
                }),
            CanFocus = true,
            HotKeySpecifier = (Rune)0xffff, // We don't want any HotKey
        };
        lblSelector.MouseEvent += View_MouseEvent;

        lblArrow = new()
        {
            X = Pos.Right(lblSelector) + 1,
            Y = 0,
            Height = 1,
            Width = 2,
            Text = Emojis.EXPAND,
        };
        lblArrow.MouseEvent += View_MouseEvent;

        Add(lbl, lblSelector, lblArrow);

        Height = 1;
        Width = Dim.Fill();

        // Create categoryList
        categoryList = new()
        {
            X = Pos.Func((view) => lblSelector.FrameToScreen().X),
            Y = Pos.Func((view) => lblSelector.FrameToScreen().Y + 1),
            Width = Dim.Fill(
                        Dim.Func((view) =>
                        {
                            var delta = (categoryList?.VerticalScrollBar.Visible == true) ? 4 : 3;
                            var nbElements = maxItemLength + delta;
                            var spaceAvailable = Tools.Application.Screen.Width - lblSelector.FrameToScreen().X;
                            return spaceAvailable - Math.Min(nbElements, spaceAvailable);
                        })),
            Height = Dim.Fill(
                         Dim.Func((view) =>
                         {
                             var delta = (categoryList?.HorizontalScrollBar.Visible == true) ? 4 : 3;
                             var nbElements = ((this.items is null) ? 0 : this.items.Count) + delta;
                             var spaceAvailable = Tools.Application.Screen.Height - lblSelector.FrameToScreen().Y;
                             return spaceAvailable - Math.Min(nbElements, spaceAvailable);
                         })),
            //AllowsMarking = false,
            CanFocus = true,
            BorderStyle = LineStyle.Rounded,
            SuperViewRendersLineCanvas = false,
        };
        categoryList.VerticalScrollBar.VisibilityMode = ScrollBarVisibilityMode.Auto;
        categoryList.HorizontalScrollBar.VisibilityMode = ScrollBarVisibilityMode.Auto;

        categoryList.ValueChanged += CategoryList_ValueChanged;
        categoryList.Activating += CategoryList_Activating; // To hide categoryList even if same item is selected

        // Create contextMenu
        contextMenu = new()
        {
            CanFocus = true,
            SchemeName = "BrightBlue"
        };
        contextMenu.Add(categoryList);
        contextMenu.VisibleChanged += ContextMenu_VisibleChanged;

        this.itemSelected = itemSelected;
        SetItems(items);
        CanFocus = true;
        SchemeName = "BrightBlue";
    }


    private void ContextMenu_VisibleChanged(object? sender, EventArgs e)
    {
        lblArrow.Text = contextMenu.Visible ? Emojis.COLLAPSE : Emojis.EXPAND;

        if(!contextMenu.Visible)
            Tools.Application.Popovers?.DeRegister(contextMenu);
    }

    protected override void OnViewportChanged(DrawEventArgs e)
    {
        HideViewSelection();
    }

    public void HideViewSelection() 
    {
        Tools.Application.Popovers?.Hide(contextMenu);
    }

    public void ShowViewSelection()
    {
        if (items?.Count > 0)
        {
            Point position = new Point(lblSelector.FrameToScreen().X, lblSelector.FrameToScreen().Y + 1);
            Tools.Application.Popovers?.Register(contextMenu);
            contextMenu.MakeVisible(position);
        }
    }

    private void View_MouseEvent(object? sender, Mouse e)
    {
        e.Handled = true;
        ShowViewSelection();
    }

    private void CategoryList_Activating(object? sender, CommandEventArgs e)
    {
        HideViewSelection();
    }

    private void CategoryList_ValueChanged(object? sender, Terminal.Gui.App.ValueChangedEventArgs<int?> e)
    {
        if (e.NewValue >= 0 && categoryListItems?.Count > e.NewValue)
        {
            var item = categoryListItems[e.NewValue.Value];
            if (itemSelected?.Id != item.Id)
            {
                itemSelected = item;

                Tools.Application.Invoke(() =>
                {
                    UpdateDisplay();
                });

                SelectedItemUpdated?.Invoke(this, item);
            }
        }
    }

    private void CategoryList_SameItemSelected(object? sender, EventArgs e)
    {
        HideViewSelection();
    }

    private void UpdateDisplay()
    {
        if (itemSelected is null)
        {
            lblSelector.Text = LBL_NONE;
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
            
            categoryListItems = items;
            categoryList.SetSource(new ObservableCollection<Item>(items));
            categoryList.SelectedItem = 0;
        }
        else
        {
            maxItemLength = 0;
            categoryListItems = [];
            categoryList.SetSource(new ObservableCollection<Item>());
            //categoryList.SelectedItem = -1;
        }

        // Do we need to update the display ?
        if ( (previousItemSelected is not null) &&  previousItemSelected?.Id == itemSelected?.Id)
            return;

        Tools.Application.Invoke(() =>
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

    public Boolean SetItemSelected(Item? item)
    {
        if ((item is not null) && (items is not null))
        {
            var index = items.FindIndex(x => x.Id == item.Id);
            if (index >= 0)
                return SetItemSelected(index);
        }
        return false;
    }

    public Boolean SetItemSelected(Peer? peer)
    {
        if ((peer is not null) && (items is not null))
        {
            var index = items.FindIndex(x => x.Id == peer.Id);
            if (index >= 0)
                return SetItemSelected(index);
        }
        return false;
    }

    public Boolean SetItemSelected(String? element)
    {
        if (!String.IsNullOrEmpty(element) && (items is not null))
        {
            var index = items.FindIndex(x => x.Text == element);
            if (index >= 0)
                return SetItemSelected(index);
        }
        return false;
    }

    public Boolean SetItemSelected(int index)
    {
        if((index >= 0) && (items?.Count > index))
        {
            var item = items[index];
            lblSelector.Text = item.Text;

            if (item.Id != itemSelected?.Id)
            {
                itemSelected = items[index];
                Tools.Application.Invoke(() =>
                {
                    if(categoryList.SelectedItem != index)
                        categoryList.SelectedItem = index;
                    UpdateDisplay();
                });
                return true;
            }
        }
        return false;
    }
}
