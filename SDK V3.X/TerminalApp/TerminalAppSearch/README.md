![Rainbow](../../../logo_rainbow.png)

# Rainbow CSharp SDK Samples - Terminal Application - Search

This example permits to understand how to search contact by name.
The result is splitted in different parts:
	- from my network
	- from my company
	- from my personal directory
	- not from my company
	- and from other directories

You need first to set configuration files:
- **../TerminalAppLibrary/exeSettings.json**: see chapter [File exeSettings.json](./../../ConfigurationFiles.md#exeSettings.json) - no need here to use external dependencies.
- **../TerminalAppLibrary/credentials.json**: see chapter [File credentials.json](./../../ConfigurationFiles.md#credentials.json).

One loggued, a text field permits to start the search by name.

## How it works

**BotViewFactory** implements the **IBotViewFactory** in the easiest way.

**BotView** inherits from **View** and display the **LoginView** available from the **TerminallAppLibrary**.

Once loggued, the **LoginView** is hidden and replaced by :
- a view called **viewFieldSearch** which contains **lblFieldSearch** and **textFieldSearch**.
- a **SearchResultsView** available from the **TerminallAppLibrary**

When the text typed in **textFieldSearch** is updated, a new search is performed.

**Main.cs** specify the **IBotViewFactory** for the **BotWindow** and use this window as main for Terminal.Gui.Application.
