![Rainbow](../logo_rainbow.png)

 
# Rainbow CSharp SDK Samples - v3.x
---

You will find here several samples which illustrate how to use the Rainbow CSharp SDK v3.X

To have more info about the SDK:
- check [Getting started guide](https://developers.openrainbow.com/doc/sdk/csharp/core/sts/guides/001_getting_started?isBeta=true)
- check [API documentation](https://developers.openrainbow.com/doc/sdk/csharp/core/sts/api/Rainbow.Application?isBeta=true)


## Rainbow API HUB
---

This SDK is using the [Rainbow environment](https://developers.openrainbow.com/)
 
This environment is based on the [Rainbow service](https://www.openrainbow.com/) 


## Examples as Console Application
---

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
| InstantMessaging | Fetch all messages archived from a Peer | [Project](./ConsoleApp/Fetch%20messages%20from%20peer) |


## Examples as Terminal Application
---

This examples are more interactive and they are working on **Linux, MacOS and Windows** (right / left mouse click, UI)

A Visual Studio solution centralized all of them [here](./TerminalApp/)

| Project Name | Content | Description | |
| --- | --- | --- | --- |
| TerminAppLogin | Login, AutoReconnection | Simple Login form to understand basic features and switch between different accounts  | [README](./TerminalApp/TerminalAppLogin/README.md) |
| TerminAppLoginSplittedView | Login, AutoReconnection | Simple Login form with a splitted view to manage in same time two different accounts | [README](./TerminalApp/TerminalAppLoginSplittedView/README.md) |
| TerminAppPresencePanel | Login, AutoReconnection, (Aggregated) Presence, User and Bubble invitation | Panel to see Presence of all contatcs in the roster. Possibility to change it's own presence. Manage user and bubble invitations | [README](./TerminalApp/TerminalAppPresencePanel/README.md) | 

