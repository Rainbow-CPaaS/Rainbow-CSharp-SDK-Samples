![Rainbow](../../logo_rainbow.png)

 
# Rainbow CSharp SDK - Bot Base example
---

This example is the base of all others Bot using [Rainbow SDK C#](https://developers.openrainbow.com/csharp).

This example is working on **Linux, MacOs or Windows**.

It's based on a state machine to simplify the complexity. See more info about this [here](../README.md#StateMachine)

This README is divided in several part:

- [Bot features](#BotFeatures)

- [Dot graph](#DotGraph)

- [Configuration and log file](#Configuration)

- [Implementation details](#ImplementationDetails)

<a name="BotFeatures"></a>
## Bot Features
---

This bot is used as base for all other bots.

It includes these features:

- Get dot graph of the [state machine](../README.md#StateMachine) (thanks to [stateless](https://github.com/dotnet-state-machine/stateless) third-party)

- Create a bot using a specific configuration (login, pwd, host, ini file location) - so it's possible to create several bot and used them in sametime.

- Connect to Rainbow server and manage auto-reconnection (at least once an authentication has succeeded) 

- Manage incoming invitations to share presence with another Rainbow user. They are all automatically accepted.

- Manage incoming invitations to be a member on a bubble. They are all automatically accepted.

- Manage incoming messages:

  - Message coming from ourself are not taken into account.
  
  - To avoid a big flod only X messages by second are read (value easily updatable). The state machine will read the next one when the delay is correct. In the meantime all messages are queued (so there is no loss).
  
  - A different process is used if the message is coming from a bubble or from a one to one conversation. The message is then answered by a default message which depends of the process.

<a name="DotGraph"></a>
## Dot graph
---

It's the dot graph of this bot 
![Dot graph](./images/RainbowBotBase.svg)


The image file has been created using this online tool https://dreampuf.github.io/GraphvizOnline/ using the dot graph generated wiht this line of code:
```cs 
StateMachine<State, Trigger> _machine; // The state machine
...
String dotGrpah = UmlDotGraph.Format(_machine.GetInfo()); // Create dot graph as String once the state machine has been totally defined   
```  

<a name="Configuration"></a>
## Configuration and log file
---

To use this sample you need to define several information in file **RainbowApplicationInfo.cs**:

- **APP_ID, APP_SECRET_KEY**: Id and Secret key of your Rainbow application - more details [here](https://developers.openrainbow.com/doc/hub/developer-journey).

- **HOST_NAME**: the hostname to use to reach Rainbow server - for example **openrainbow.com**.

- **LOGIN_MASTER_BOT**: the login (i.e. email address) of the user (the "master bot") which can stop the bot remotely using a IM message.

- **LOGIN_BOT, PASSWORD_BOT**: Login and password to use for the bot to connect to the server.

- **NLOG_CONFIG_FILE_PATH**: Valid path to a XML file used to configure NLog. Without it no log files will be generated. This XML file is available [here](https://github.com/Rainbow-CPaaS/Rainbow-CSharp-SDK-Samples/blob/master/NLogConfiguration.xml).

**NOTE**: APP_ID, APP_SECRET_KEY and HOST_NAME are linked. Generally APP_ID and APP_SECRET_KEY are different according the HOST_NAME.

<a name="ImplementationDetails"></a>
## Implementation details
---

This example is composed of only 3 files: **RainbowApplicationInfo.cs, Program.cs and RainbowBotBase.cs**

### RainbowApplicationInfo.cs

Used to store main information (see previous chapter)

### Program.cs

Contains the Main method. It checks if main information seems correct, initialize log configuration, create bot and configure it. 

It also check two specific states: **NotConnected** and **Created**

#### NotConnected state 

If we are in this state, we need to take a specific action (or perjpas nothing more) according the **trigger** used to reach this state

- **Disconnect trigger**: The bot has received the "stop message" from the "master bot". It will do nothing more. Needs to add more logic if we want to start it again.    

- **ServerNotReachable trigger**: The server has never been reached. Need to add more logic to try again

- **TooManyAttempts trigger**: The Bot was logged at least once but now even after several attempts it's no more possible. Needs to add more logic to try again.

#### Created state

We are in this state only because credentials provided are incorrect. It's necessary to provide new ones. Needs to add more logic to try again.

### RainbowBotBase.cs

All possible states and triggers of the bot using a state machine are defined in this file.

**Trigger's list:** Configure, StartLogin, Disconnect, ServerNotReachable, IncorrectCredentials, AuthenticationSucceeded, InitializationPerformed, Connect, TooManyAttempts, DequeueInvitationsAndMessages, MessageReceivedFromBubble, MessageReceivedFromPeer, StopMessage, MessageManaged, BubbleInvitationReceived, BubbleInvitationManaged, InvitationReceived, InvitationManaged,
 
**State's list:** Created, NotConnected, Connecting, ConnectionFailed, Authenticated, Initialized, Connected, AutoReconnection, StopMessageReceived, MessageFromBubble, MessageFromPeer, BubbleInvitationReceived, InvitationReceived,


**Main methods are:**

- **ConfigureStateMachine**: To create / configure all the state machine

- **DequeueInvitationsAndMessages**: To dequeue in this order: invitations to share presence, invitations to be a member of a bubble and message received (we avoid a flod here using MESSAGE_READ_BY_SECOND variable) 

- **CreateRainbowObjects**: To create all necessary objects from the Rainbow SDK C#

- **SetRainbowRestictions**: To specify restrictions to use in the Rainbow SDK - for example we want to use the AutoReconnection service and don' want to store message

- **SubscribeToRainbowEvents / UnsubscribeToRainbowEvents**: To subscribe / unsubscribe to all necessary events from the Rainbow SDK C#

 
