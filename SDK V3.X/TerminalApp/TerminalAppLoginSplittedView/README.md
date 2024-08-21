![Rainbow](../../../logo_rainbow.png)

# Rainbow CSharp SDK Samples - Terminal Application - Login with splitted view

This example permits to understand:
- how to manage login / logout features
- the AutoReconnection service

- You need first to set **../TerminalAppLibrary/Resources.json** according you environment / configuration:
	- server configuration with  **appId, appSecret and hostname**
	- account(s) with  **botName, login, password**

## How it works

**BotViewFactory** implements the **IBotViewFactory** in the easiest way.

**BotView** inherits from **View** and display the **LoginView** available from the **TerminallAppLibrary**

**Main.cs** specify the **IBotViewFactory** for the **BotWindow** and use this window as main for Terminal.Gui.Application.

The code is nearly the same than in the project **TerminalLogin** only this line has been added in **Main.cs**:
```
Configuration.UseSplittedView = true; // We want to display up to two Rainbow accounts in same time
```