using Terminal.Gui;
using Rainbow.Console;

public interface IBotViewFactory
{
    public View CreateBotView(UserConfig account);

    public UserConfig? GetRainbowAccountFromBotView(View? botView);
}
