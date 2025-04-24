![Rainbow](../../../logo_rainbow.png)

# Rainbow CSharp SDK Samples - Terminal Application - Hybrid Telephony

This example permits to understand:
- how to manage login / logout features
- the AutoReconnection service
- Manage Hybrid Telephony settings:
	- Call Forward
	- Nomadic
- Manage Hybrid Telephony calls using 3PCC (Third Party Call Control):
	- Make Call (up to two in same time)
	- Release, Answer, Fwd to VM
	- Hold / UnHold
	- Transfer one call to another one
	- Create Conference call

You need first to set configuration files:
- **../TerminalAppLibrary/exeSettings.json**: see chapter [File exeSettings.json](./../../ConfigurationFiles.md#exeSettings.json) - no need here to use external dependencies.
- **../TerminalAppLibrary/credentials.json**: see chapter [File credentials.json](./../../ConfigurationFiles.md#credentials.json).

One loggued, if the current user as the right to use Hybrid Telephony, several panels permits to manage Hybrid Telephony settings and calls using 3PCC.

## How it works

**BotViewFactory** implements the **IBotViewFactory** in the easiest way.

**BotView** inherits from **View** and display the **LoginView** available from the **TerminallAppLibrary**.

Once loggued, the **LoginView** is hidden and replaced by several views available from the **TerminallAppLibrary**.
- **HybridTelephonyCallForwardView** 
- **HybridTelephonyMakeCallView** 
- **HybridTelephonyNomadicView** 
- **HybridTelephonyPanelCallView** which uses two **HybridTelephonyCallView**
- **HybridTelephonyServiceView** 
- **HybridTelephonyVoiceMailView** 

**Main.cs** specify the **IBotViewFactory** for the **BotWindow** and use this window as main for Terminal.Gui.Application.
