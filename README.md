![Rainbow](logo_rainbow.png)

 
# Rainbow-CSharp-SDK-Samples
---

Small samples (as `Desktop applications`) to understand how to use the SDK (available on on `Windows`, `MacOS`)

Advanced sample (as `Mobile application`) going deeper in SDK use for `IOS` and `MacOs`

S2S (Server to Server) sample (as 'BOT application') to understand how to use S2S event mode - full details about S2S versus XMPP event mode [here](https://hub.openrainbow.com/#/documentation/doc/sdk/csharp/guides/035_events_mode) 

## Rainbow API HUB
---

This SDK is using the Rainbow Hub environment.

Full details [here](https://hub.openrainbow.net/)
 
This environment is based on the [Rainbow service](https://www.openrainbow.com/) 


## Rainbow CSharp SDK
---

To have more info about the SDK:
- check [Getting started guide](https://hub.openrainbow.com/#/documentation/doc/sdk/csharp/guides/001_getting_started)
- check [API documentation](https://hub.openrainbow.com/#/documentation/doc/sdk/csharp/api/Rainbow.Application)


## Small samples
---

They are listed in order of priority if you just started to use the SDK - they are available for `Windows` and `MacOS`

| Content | Platform | Description |
| ------- | --------- | ----------- |
| Contacts | [Windows](https://github.com/Rainbow-CPaaS/Rainbow-CSharp-SDK-Samples/tree/master/Windows_NetFwk/Contacts) / [MacOs](https://github.com/Rainbow-CPaaS/Rainbow-CSharp-SDK-Samples/tree/master/Mac_Xamarin/Contacts) | Manage list of contacts - First sample to understand |
| Conversations / Favorites | [Windows](https://github.com/Rainbow-CPaaS/Rainbow-CSharp-SDK-Samples/tree/master/Windows_NetFwk/Conversations) / [MacOs](https://github.com/Rainbow-CPaaS/Rainbow-CSharp-SDK-Samples/tree/master/Mac_Xamarin/Conversations) | Manage list of conversations / favorites - Second sample to understand |
| Instant Messaging | [Windows](https://github.com/Rainbow-CPaaS/Rainbow-CSharp-SDK-Samples/tree/master/Windows_NetFwk/InstantMessaging) / [MacOs](https://github.com/Rainbow-CPaaS/Rainbow-CSharp-SDK-Samples/tree/master/Mac_Xamarin/InstantMessaging)| Send, receive simple IM messages, manage presence |
| Conferences | [Windows](https://github.com/Rainbow-CPaaS/Rainbow-CSharp-SDK-Samples/tree/master/Windows_NetFwk/Conferences) / [MacOs](https://github.com/Rainbow-CPaaS/Rainbow-CSharp-SDK-Samples/tree/master/Mac_Xamarin/Conferences) | Manage Conferences (PSTN or WebRTC): Start/Join/Stop, Mute/Unmute, Lock/Unlock, Drop particpant) |
| Telephony | [Windows](https://github.com/Rainbow-CPaaS/Rainbow-CSharp-SDK-Samples/tree/master/Windows_NetFwk/Telephony) | Manage PBX call: Make call, Hang Up, Hold/UnHold, Transfer, Conference |


## Advanced Mobile Application Sample 
---

It's an advanced example of Instant Messaging for `IOS` and `MacOs`: [Advanced sample](https://github.com/Rainbow-CPaaS/Rainbow-CSharp-SDK-Samples/tree/master/Windows_NetFwk/Contacts)

Some screenshoots of this sample:

| Login | Conversations List | Conversation Stream |
| ----- | ------------------ | ------------------- |
| ![Login screen](./images/MobileApp_Login.png) | ![Conversations List](./images/MobileApp_Conversations_List.png) | ![Conversation Stream](./images/MobileApp_Conversation_Stream.png) |


## Server to server sample 
---

It's important to read this [guide](https://hub.openrainbow.net/#/documentation/doc/sdk/csharp/guides/035_events_mode) first to understand what is a S2S application and the architecture associated.

This [sample](https://github.com/Rainbow-CPaaS/Rainbow-CSharp-SDK-Samples/tree/master/S2S) permits to understand how to use this SDK in Server to server context. It's better first to understand how the use SDK using small examples.

This [sample](https://github.com/Rainbow-CPaaS/Rainbow-CSharp-SDK-Samples/tree/master/S2S) is based on `Instant Messaging` sample using S2S event mode
