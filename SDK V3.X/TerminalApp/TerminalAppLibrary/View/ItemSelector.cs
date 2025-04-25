using Rainbow.Model;
using System.Collections.Generic;
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

    List<Item>? items;
    Item? itemSelected;
    
    private PopoverMenu? contextMenu;
    private ListView? categoryList;

    public event EventHandler<Item>? SelectedItemUpdated;

    public ItemSelector(String label, List<Item>? items = null, Item? itemSelected = null) 
    {
        lbl = new()
        {
            X = 0,
            Y = 0,
            Height = 1,
            Width = (label is null) ? 1 : label.Length + 1,
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

        this.itemSelected = itemSelected;
        SetItems(items);
    }

    private void View_MouseClick(object? sender, MouseEventArgs e)
    {
        e.Handled = true;
        if (items?.Count > 0)
        {
            contextMenu = new();

            var finalItems = items;

            var strings = finalItems.Select(i => i.ToString());
            var maxLength = strings.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length;


            categoryList = new()
            {
                X = Pos.Func( () => lblSelector.FrameToScreen().X),
                Y = Pos.Func(() => lblSelector.FrameToScreen().Y + 1),
                Width = Dim.Fill(
                            Dim.Func(() =>
                                    {
                                        var delta = (categoryList?.VerticalScrollBar.Visible == true) ? 4 : 3;
                                        var nbElements = maxLength + delta;
                                        var spaceAvailable = Application.Screen.Width - lblSelector.FrameToScreen().X;
                                        if (nbElements > spaceAvailable)
                                            nbElements = spaceAvailable;
                                        return spaceAvailable - nbElements;
                                        //return 0;
                                    })),
                Height = Dim.Fill(
                             Dim.Func(() =>
                                    {
                                        var delta = (categoryList?.HorizontalScrollBar.Visible == true) ? 4 : 3;
                                        var nbElements = finalItems.Count + delta;
                                        var spaceAvailable = Application.Screen.Height - lblSelector.FrameToScreen().Y;
                                        if (nbElements > spaceAvailable)
                                            nbElements = spaceAvailable;
                                        return spaceAvailable - nbElements;
                                    })),
                AllowsMarking = false,
                CanFocus = true,
                BorderStyle = LineStyle.Rounded,
                SuperViewRendersLineCanvas = false,
                Source = new ListWrapper<Item>(new ObservableCollection<Item>(finalItems)),
                ColorScheme = Tools.ColorSchemeBlueOnGray // /!\ Need to be set !!!
            };
            categoryList.VerticalScrollBar.AutoShow = true;
            categoryList.HorizontalScrollBar.AutoShow = true;

            categoryList.SelectedItemChanged += CategoryList_SelectedItemChanged;

            contextMenu.Add(categoryList);

            Point position = new Point(lblSelector.FrameToScreen().X, lblSelector.FrameToScreen().Y + 1);
            contextMenu.MakeVisible(position);
        }
    }

    private void CategoryList_SelectedItemChanged(object? sender, ListViewItemEventArgs e)
    {
        Application.Popover?.Hide(contextMenu);

        if (SetItemSelected((Item) e.Value))
            SelectedItemUpdated?.Invoke(this, (Item)e.Value);
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
            //lblArrow.Y = Pos.Right(this) - 2;
        }



    }

    public void SetItems(List<Item>? items)
    {
        this.items = items;

        if(items?.Count > 0)
        {
            if ( (itemSelected is null)
                || (items.FirstOrDefault(i => i.Id == itemSelected.Id) is null) )
                itemSelected = items[0];
        }
        else
        {
            itemSelected = null;
        }

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

    public Boolean SetItemSelected(Item? item)
    {
        Boolean result = false;
        if ((item is not null) && (item.Id != itemSelected?.Id) && items?.Count > 0)
        {
            if (items.FirstOrDefault(i => i.Id == item.Id) is not null)
            {
                itemSelected = item;
                result = true;

                Terminal.Gui.Application.Invoke(() =>
                {
                    UpdateDisplay();
                });
            }
        }
        return result;
    }
}
