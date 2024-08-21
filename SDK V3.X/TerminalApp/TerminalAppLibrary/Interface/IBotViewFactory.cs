using Terminal.Gui;

public interface IBotViewFactory
{
    public View CreateBotView(RainbowAccount account);

    public RainbowAccount ? GetRainbowAccountFromBotView(View? botView);
}
