![Rainbow](../../../logo_rainbow.png)

# Rainbow CSharp SDK Samples - Terminal Application - Presence Panel using S2S

Using S2S event pipe (and not XMPP event pipe), this example permits to understand:
- how to manage login / logout features
- the AutoReconnection service
- Presence and Aggregated Presences
- Add / Remove contacts from network / roster
- Add / Remove contacts from favorites

- You need first to set **../TerminalAppLibrary/Resources.json** according you environment / configuration:
	- server configuration with  **appId, appSecret and hostname**
	- account(s) with  **botName, login, password** and also **useS2S**
	- S2S Callback URL using the field **s2sCallbackURL**

One loggued, a Presence panel is displayed to see interactively Aggregated Presence of contacts in the roster.

A double click on one on them will open an dialog which list all Presences of the select Contact.

A right click, open a context menu: 
	- you can remove the selected contact form the user's network
	- you can add or remove it form the favorites

It's also possible to change the Presence of the contact connected to the Rainbow server using right click on his name.

About the S2S:
- check [Getting started guide](https://developers.openrainbow.com/doc/sdk/csharp/core/sts/guides/035_events_mode?isBeta=true)

Ngok local instance must be starte locally like this:
**ngrok http --host-header="localhost:9870" 9870**

## How it works

The code of this sample is 95% the same than the Presence Panel using XMPP Event.

Differences are:
- In **config.json** file, you have to define a S2S Callback URL using the field **s2sCallbackURL**
- In **config.json** file, for Rainbow Account which must use S2S, set the field **useS2S** to **true**
- in **Main.cs**, we create a web server which will listen to a valid port (here we use 9870) using the specified S2S Callback URL
- When a request is made to the web servern thanks to the callback web modeul (cf. **CallbackWebModule.cs**), we parse its content using ***S2SEventPipe.ParseCallbackContentAsync()*** method.

**BotViewFactory** implements the **IBotViewFactory** in the easiest way.

**BotView** inherits from **View** and display the **LoginView** available from the **TerminallAppLibrary**.

Once loggued, the **LoginView** is hidden and replaced by **PresencePaneView** available from the **TerminallAppLibrary**.

**Main.cs** specify the **IBotViewFactory** for the **BotWindow** and use this window as main for Terminal.Gui.Application.
