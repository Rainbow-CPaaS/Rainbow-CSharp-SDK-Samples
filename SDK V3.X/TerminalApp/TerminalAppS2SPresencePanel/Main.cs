
// Indicates configuration elements we need to use in this example
using Rainbow.Example.CommonWebHook;

Configuration.NeedsRainbowServerConfiguration = true;
Configuration.NeedsRainbowAccounts = true;

// Initialize the Configuration i.e. reads content of the file "config.json" defined in the "Resources" folder or "TerminalAppLibrary" project
if (!Configuration.Initialize())
    return;

// Check if a S2SCallbackURL has been defined
if (String.IsNullOrEmpty(Configuration.ExeSettings.S2SCallbackURL))
{
    ConsoleAbstraction.WriteLine("There is no S2SCallbackURL defined in config.json file ...");
    return;
}

// Specify the "BotViewFactory" to use in "BotWindow"
BotWindow.BotViewFactory = new BotViewFactory();

// Use "BotWindow" as main window
Tools.Application?.Run(new BotWindow());
