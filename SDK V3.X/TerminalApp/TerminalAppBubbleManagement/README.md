![Rainbow](../../../logo_rainbow.png)

# Rainbow CSharp SDK Samples - Terminal Application - Bubble Management

- WORK IN PROGRESS - 

This example permits to understand:
- how to list bubble members
- Promote / Demote members
- Invite new members

You need first to set configuration files:
- **../TerminalAppLibrary/exeSettings.json**: see chapter [File exeSettings.json](./../../ConfigurationFiles.md#exeSettings.json) - no need here to use external dependencies.
- **../TerminalAppLibrary/credentials.json**: see chapter [File credentials.json](./../../ConfigurationFiles.md#credentials.json).

One loggued, the first bubble found is used to display its members. You can select any existing bubble

From the members list, a right click open a context menu to manage them

## How it works

**BotViewFactory** implements the **IBotViewFactory** in the easiest way.

**BotView** inherits from **View** and display the **LoginView** available from the **TerminallAppLibrary**.

Once loggued, the **LoginView** is hidden and replaced by **PresencePaneView** available from the **TerminallAppLibrary**.

**Main.cs** specify the **IBotViewFactory** for the **BotWindow** and use this window as main for Terminal.Gui.Application.
