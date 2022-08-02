![Rainbow](../logo_rainbow.png)

 
# Rainbow CSharp SDK - Bot examples
---

You will find here several samples which illustrate how to use the Rainbow CSharp SDK to create Bot.

They are all working in **Linux, MacOs or Windows**. 

They are listed in order of priority / difficulty if you just started to use the SDK and/or create Bot.

All bots are using a [state machine](#StateMachine) to simplify the complexity.

List of examples:

- [Bot - Base](#BotBase): **This example is the base of all others Bot**.

- [Bot - Adaptive Cards](#BotAdaptiveCards): This example demonstrates how to use [**Adaptive Cards**](https://adaptivecards.io/) for example to create a multiple choice question (MCQ) test.

- [Bot - CCTV](#BotCCTV): This example demonstrates how several Bots can **communicate** and use **WebRTC**. The bot master asks other bots to join specific conference to display a specific CCTV.

<a name="StateMachine"></a>
## State Machine
---

A third party library ([stateless](https://github.com/dotnet-state-machine/stateless) is used to create the state machine.

Several library exists. This one has been choosed because it's a **lightweight one** (no dependency) and **permits to visualize the state machine** using a dot graph like the one below.

The dot grpah can be direclty created from the state machine defined by code using only one line:

```cs 
StateMachine<State, Trigger> _machine; // The state machine
...
String dotGrpah = UmlDotGraph.Format(_machine.GetInfo()); // Create dot graph as String once the state machine has been totally defined   
```

This image represents the state machine of the [Bot - Base](#BotBase):

![Rainbow](./BotBase/images/RainbowBotBase.svg)

A dot graph is a plain text describing a graph. To visualize it a tool is necessary like these ones:

- [viz-js](http://viz-js.com/): online visualizer

- [GraphvizOnline](https://dreampuf.github.io/GraphvizOnline/): online visualizer

- [graphviz.org](http://www.graphviz.org/)

<a name="BotBase"></a>
## Bot - Base
---

[This bot](./BotBase/README.md) is used as base for all other bots.

It includes these features:

- Get dot graph of the [state machine](#StateMachine) (thanks to [stateless](https://github.com/dotnet-state-machine/stateless) third-party)

- Create a bot using a specific configuration (login, pwd, host, ini file location) - so it's possible to create several bot and used them in sametime.

- Connect to Rainbow server and manage auto-reconnection (at least once an authentication has succeeded) 

- Manage incoming invitations to share presence with another Rainbow user. They are all automatically accepted.

- Manage incoming invitations to be a member on a bubble. They are all automatically accepted.

- Manage incoming messages:

  - Message coming from ourself are not taken into account.
  
  - To avoid a big flod only X messages by second are read (value easily updatable). The state machine will read the next one when the delay is correct. In the meantime all messages are queued (so there is no loss).
  
  - A different process is used if the message is coming from a bubble or from a one to one conversation. The message is then answered by a default message which depends of the process.


<a name="BotAdaptiveCards"></a>
## Bot - Adaptive Cards
---

[This bot](./BotAdaptiveCards/README.md) is based on **Bot Base**.

The goal of this Bot is to create a multiple choice question (**MCQ**) test for a specific list of users defined at startup.

Questions to users are displayed using **Adaptive cards**.

So we need to properly manage the connection with the RB server, contact all the users who need to take the test and, when they are ready, run the test until the end.

**Features**:

- Get dot graph of the [state machine](../README.md#StateMachine) (thanks to [stateless](https://github.com/dotnet-state-machine/stateless) third-party)

- Accept a specific configuration (login, pwd, host, ini file location, user list who must take the test) - so it's possible to create several bot and used them in sametime. If a thousand users need to pass the test, we can imagine having several bots (for example one for 100 users) instead of a single bot...

- Connect to Rainbow server and manage auto-reconnection (at least once an authentication has succeeded)

- Once connected, we send a welcome message to all test participants one by one, explaining that a MCQ test is available and how to start it. 

- Manage incoming messages:

  - Message coming from ourself are not taken into account.
  
  - Message coming from bubble are not taken into account.
  
  - Message coming directly from user who doesn't need to take the test are not taken into account 
  
  - Message coming from users taking the test are analysed and only anser to the test are taken into account: answer aer stored and the next question is displayd until the end. Then a final message is sentd with the test result.

![Gif](./BotAdaptiveCards/images/Rainbow-MCQTest.gif)

<a name="BotCCTV"></a>
## Bot - CCTV
---

**IN PROGRESS**
