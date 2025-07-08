using Rainbow.Example.Common;
using Terminal.Gui.ViewBase;


public interface IBotViewFactory
{
    public View CreateBotView(UserConfig account);

    public UserConfig? GetRainbowAccountFromBotView(View? botView);
}
