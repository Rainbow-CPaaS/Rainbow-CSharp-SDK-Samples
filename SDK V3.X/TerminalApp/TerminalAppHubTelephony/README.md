![Rainbow](../../../logo_rainbow.png)

# Rainbow CSharp SDK Samples - Terminal Application - Hub Telephony

This example permits to understand some Hub Telephony service features:
- service available / operational
- PBX Id and PBX Agent version
- Set anonymous call on / off
- Select current phone: Office phone, Computer
- Set Auto-Accept call on Rainbow Web/Desktop application on / off
- 3PCC Make Call: with phone number, auto-accept, phone selection and potentially resource selection
- Follow up to 2 calls in progress:
	- info provided: id,  type [hg (Hunting Group), ccq (Call Queue), sfu], status, direction, device used, participant, called
	- actions possible: answer (using current phone), hold/unhold, release, transfer to VM, send DTMF, transfer a call to another one

You need first to set configuration files:
- **../TerminalAppLibrary/exeSettings.json**: see chapter [File exeSettings.json](./../../ConfigurationFiles.md#exeSettings.json) - no need here to use external dependencies.
- **../TerminalAppLibrary/credentials.json**: see chapter [File credentials.json](./../../ConfigurationFiles.md#credentials.json).

## How it works

**BotViewFactory** implements the **IBotViewFactory** in the easiest way.

**BotView** inherits from **View** and display the **LoginView** available from the **TerminallAppLibrary**.

Once loggued, the **LoginView** is hidden and replaced by several views available from the **TerminallAppLibrary**.
- **HubTelephonyMakeCallView** 
- **HubTelephonyPanelCallView** which uses two **HubTelephonyCallView**
- **HubTelephonyServiceView** 

**Main.cs** specify the **IBotViewFactory** for the **BotWindow** and use this window as main for Terminal.Gui.Application.
