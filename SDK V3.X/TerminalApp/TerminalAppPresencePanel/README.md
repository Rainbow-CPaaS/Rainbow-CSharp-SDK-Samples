![Rainbow](../../../logo_rainbow.png)

# Rainbow CSharp SDK Samples - Terminal Application - Presence Panel

This example permits to understand:
- how to manage login / logout features
- the AutoReconnection service
- Presence and Aggregated Presences
- Add / Remove contacts from network / roster
- Add / Remove contacts from favorites

You need first to set configuration files:
- **../TerminalAppLibrary/exeSettings.json**: see chapter [File exeSettings.json](./../../ConfigurationFiles.md#exeSettings.json) - no need here to use external dependencies.
- **../TerminalAppLibrary/credentials.json**: see chapter [File credentials.json](./../../ConfigurationFiles.md#credentials.json).

One loggued, a Presence panel is displayed to see interactively Aggregated Presence of contacts in the roster.

A double click on one on them will open an dialog which list all Presences of the select Contact.

A right click, open a context menu: 
	- you can remove the selected contact form the user's network
	- you can add or remove it form the favorites

It's also possible to change the Presence of the contact connected to the Rainbow server using right click on his name.

## How it works

**BotViewFactory** implements the **IBotViewFactory** in the easiest way.

**BotView** inherits from **View** and display the **LoginView** available from the **TerminallAppLibrary**.

Once loggued, the **LoginView** is hidden and replaced by **PresencePaneView** available from the **TerminallAppLibrary**.

**Main.cs** specify the **IBotViewFactory** for the **BotWindow** and use this window as main for Terminal.Gui.Application.
