using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

internal class BotViewFactory : IBotViewFactory
{
    public View CreateBotView(RainbowAccount account)
    {
        return new BotView(account);
    }

    public RainbowAccount? GetRainbowAccountFromBotView(View? botView)
    {
        if (botView == null)
            return null;

        if (botView is BotView v)
            return v.rbAccount;
        return null;
    }

}
