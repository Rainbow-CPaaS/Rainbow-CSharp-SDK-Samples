![Rainbow](../../logo_rainbow.png)

# Rainbow CSharp SDK Samples (v3.x) in an interactive terminal

You will find here several examples which illustrate how to use the Rainbow CSharp SDK v3.X in an interactive terminal.

All examples are working on **Linux, MacOS and Windows**

To have more info about the SDK:
- check [Getting started guide](https://developers.openrainbow.com/doc/sdk/csharp/core/sts/guides/001_getting_started?isBeta=true)
- check [API documentation](https://developers.openrainbow.com/doc/sdk/csharp/core/sts/api/Rainbow.Application?isBeta=true)


## Rainbow API HUB

This SDK is using the [Rainbow environment](https://developers.openrainbow.com/)
 
This environment is based on the [Rainbow service](https://www.openrainbow.com/) 

## Examples

They are listed in order of priority / complexity.


 | Project Name | Content | Description |
| --- | --- | --- | 
| [TerminAppLogin](./TerminalApp/TerminalAppLogin/README.md) | Login, AutoReconnection | Simple Login form to understand basic features and switch between different accounts |
| [TerminAppLoginSplittedView](./TerminalApp/TerminalAppLoginSplittedView/README.md) | Login, AutoReconnection | Simple Login form with a splitted view to manage in same time two different accounts |
| [TerminAppPresencePanel](./TerminalApp/TerminalAppPresencePanel/README.md) | Login, AutoReconnection, (Aggregated) Presence, User and Bubble invitation | Panel to see Presence of all contatcs in the roster. Possibility to change it's own presence. Manage user and bubble invitations | 
| [TerminalAppS2SPresencePanel](./TerminalApp/TerminalAppS2SPresencePanel/README.md) | Login, AutoReconnection, (Aggregated) Presence, User and Bubble invitation | Using S2S, Panel to see Presence of all contatcs in the roster. Possibility to change it's own presence. Manage user and bubble invitations |
| [TerminAppSearch](./TerminAppSearch/README.md) | Login, AutoReconnection, Search | Panel to search contact by name | 
| [TerminalAppHybridTelephony](./TerminalApp/TerminalAppHybridTelephony/README.md) | Login, AutoReconnection, Hybrip Telephony | To follow/update in 3PCC (trhid party remote control) Hybrid Telephony calls. Manage Call forward, nomadic and voicemail features |
| [TerminalAppBubbleManagement](./TerminalAppBubbleManagement/README.md) | Login, AutoReconnection, Conversation, Bubble, Bubble Link, Waiting Room / Lobby | To list bubble's members, to promote / demote them, to accept / deny contact in waiting romm / lobby |
 
## How to test them

### File exeSettings.json

Open the solution **TerminalApp.sln**

You need to set correctly the file "exeSettings.json" like described in chapter [File exeSettings.json](./../../ConfigurationFiles.md#exeSettings.json).

## File credentials.json

Open the solution **TerminalApp.sln**

You need to set correctly the file "credentials.json" like described in chapter [File credentials.json](./../../ConfigurationFiles.md#credentials.json).

Now select as **default project**, the one you want to test. Compile it and test it.

In each project a README.md describes features of the example.

## How it works

All projects are using common Interfaces, Windows, Views and Objects centralized in **TerminalAppLibrary**

Each project must create an object implementing the **IBotViewFactory**. 

This object permits to create for each sample a specific **BotView** to display different information

