![Rainbow](../logo_rainbow.png)
 
# Rainbow CSharp SDK examples - v3.x

You will find here several samples which illustrate how to use the Rainbow CSharp SDK v3.X


## Table of content

- [Prerequisites](#Prerequisites)
    - [Rainbow API HUB](#RainbowAPIHUB)
    - [Rainbow CSharp SDK](#RainbowCSharpSDK)
- [Console Application examples](#ConsoleApplicationexamples)
- [Bot examples](#Botexamples)
- [Terminal Application examples](#TerminalApplicationexamples)

<a name="Prerequisites"></a>
## Prerequisites

<a name="RainbowAPIHUB"></a>
### Rainbow API HUB

This SDK is using the [Rainbow environment](https://developers.openrainbow.com/)
 
This environment is based on the [Rainbow service](https://www.openrainbow.com/) 

<a name="RainbowCSharpSDK"></a>
### Rainbow CSharp SDK

To have more info about the SDK:
- check [Getting started guide](https://developers.openrainbow.com/doc/sdk/csharp/core/lts/guides/001_getting_started)
- check [API documentation](https://developers.openrainbow.com/doc/sdk/csharp/core/lts/api/Rainbow.Application)

<a name="ConsoleApplicationexamples"></a>
## Console Application examples

They are working on **Linux, MacOS and Windows**

They are listed in order of priority / complexity.

| Content | Description | Project |
| --- | --- | --- |
| Login | Simple Login using await / async | [Project](./ConsoleApp/Login%20simple) |
| Logs | Configure logger to have logs file | [Project](./ConsoleApp/Configure%20logger) | 
| Login | Login and first events management | [Project](./ConsoleApp/Login%20and%20events) |
| Login, AutoReconnection | Login and AutoReconnection management | [Project](./ConsoleApp/Login%20and%20autoreconnection) |
| Logs, Multiple Instances | Configure multiple loggers (to manage several Rainbow instance) | [Project](./ConsoleApp/Configure%20multiple%20loggers) |
| Bubbles, Contacts | Get information about Bubbles and Contacts | [Project](./ConsoleApp/Get%20basic%20contacts%20and%20bubbles%20info) |
| Contacts, Presence | Get the presence of your contacts and modify yours | [Project](./ConsoleApp/Get%20and%20set%20presence) |
| Contacts, Presence, Multi-Instance | Using presence, detect if users are connected using an UCaaS Rainbow Application or not | [Project](./ConsoleApp/Detect%20UCaaS%20connection) |
| InstantMessaging | Fetch all messages archived from a Peer | [Project](./ConsoleApp/Fetch%20messages%20from%20peer) |

<a name="Botexamples"></a>
## Bot examples

They are also console applications but developped to be used as bot

A Visual Studio solution centralized all of them [here](./Bot/).

A more detailed [README](./Bot/README.md) is available.

| Project Name | Content | Description |
| --- | --- | --- |
| BotLibrary | Base for all other Bots | Authentication, Connection,  Auto-reconnection, Bubble and User invitations, BotConfiguration updates |
| BotBasicMessages | Auto Answer | Small example to understand how to use BotLibrary.BotBase class |
| BotBroadcaster | Medias Broadcast in Conference | Advanced example and **ready to use** to have a Bot which can grab streams and broadcast them in a conference | 

<a name="TerminalApplicationexamples"></a>
## Terminal Application examples

This examples are more interactive and they are working on **Linux, MacOS and Windows** (right / left mouse click, UI)

A Visual Studio solution centralized all of them [here](./TerminalApp/)

| Project Name | Content | Description | |
| --- | --- | --- | --- |
| TerminAppLogin | Login, AutoReconnection | Simple Login form to understand basic features and switch between different accounts  | [README](./TerminalApp/TerminalAppLogin/README.md) |
| TerminAppLoginSplittedView | Login, AutoReconnection | Simple Login form with a splitted view to manage in same time two different accounts | [README](./TerminalApp/TerminalAppLoginSplittedView/README.md) |
| TerminAppPresencePanel | Login, AutoReconnection, (Aggregated) Presence, User and Bubble invitation | Panel to see Presence of all contatcs in the roster. Possibility to change it's own presence. Manage user and bubble invitations | [README](./TerminalApp/TerminalAppPresencePanel/README.md) | 
| TerminalAppS2SPresencePanel | Login, AutoReconnection, (Aggregated) Presence, User and Bubble invitation | Using S2S, Panel to see Presence of all contatcs in the roster. Possibility to change it's own presence. Manage user and bubble invitations | [README](./TerminalApp/TerminalAppS2SPresencePanel/README.md) | 

