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

| Project Name | Content | Description | |
| --- | --- | --- | --- |
| TerminAppLogin | Login, AutoReconnection | Simple Login form to understand basic features and switch between different accounts  | [README](./TerminalAppLogin/README.md) |
| TerminAppLoginSplittedView | Login, AutoReconnection | Simple Login form with a splitted view to manage in same time two different accounts | [README](./TerminalAppLoginSplittedView/README.md) |
| TerminAppPresencePanel | Login, AutoReconnection, (Aggregated) Presence, User and Bubble invitation | Panel to see Presence of all contatcs in the roster. Possibility to change it's own presence. Manage user and bubble invitations | [README](./TerminalAppPresencePanel/README.md) | 
| TerminalAppS2SPresencePanel | Login, AutoReconnection, (Aggregated) Presence, User and Bubble invitation | Using S2S, Panel to see Presence of all contatcs in the roster. Possibility to change it's own presence. Manage user and bubble invitations | [README](./TerminalAppS2SPresencePanel/README.md) | 

## How to test them

- Open the solution **TerminalApp.sln**
- Set the **./TerminalAppLibrary/Resources.json** according you environment / configuration:
	- server configuration with  **appId, appSecret and hostname**
	- account(s) with  **botName, login, password**
	- Eventually configurS2S Callback URL using the field **s2sCallbackURL**
- Select as **default project**, the one you want to test
- Compile it and test it.

In each project a README.md describes features of the example.


## How it works

All projects are using common Interfaces, Windows, Views and Objects centralized in **TerminalAppLibrary**

Each project must create an object implementing the **IBotViewFactory**. 

This object permits to create for each sample a specific **BotView** to display different information

