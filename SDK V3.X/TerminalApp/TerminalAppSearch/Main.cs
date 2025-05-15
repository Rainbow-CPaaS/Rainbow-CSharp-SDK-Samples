
// Indicates configuration elements we need to use in this example
using System.Text;

Configuration.NeedsRainbowServerConfiguration = true;
Configuration.NeedsRainbowAccounts = true;

// Initialize the Configuration i.e. reads content of the file "config.json" defined in the "Resources" folder or "TerminalAppLibrary" project
if (!Configuration.Initialize())
    return;

// Specify the "BotViewFactory" to use in "BotWindow"
BotWindow.BotViewFactory = new BotViewFactory();

// Use "BotWindow" as main window for Terminal.Gui.Application
Terminal.Gui.Application.Run<BotWindow>().Dispose();

// Before the application exits, reset Terminal.Gui for clean shutdown
Terminal.Gui.Application.Shutdown();

