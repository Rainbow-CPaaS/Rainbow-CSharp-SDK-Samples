using Rainbow.Console;
using Terminal.Gui;

internal class BotViewFactory : IBotViewFactory
{
    public View CreateBotView(UserConfig account)
    {
        return new BotView(account);
    }

    public UserConfig? GetRainbowAccountFromBotView(View? botView)
    {
        if (botView == null)
            return null;

        if (botView is BotView v)
            return v.rbAccount;
        return null;
    }

}
