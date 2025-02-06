![Rainbow](./../../../logo_rainbow.png)

# Rainbow CSharp SDK v3 - Bot Broadcaster

##  [0.0.2]
- use **credentials.json** (renamed from **botCredentials.json**)
- Add **bot** object in JSON to specify which bot must use the configuration - see chapter [File botConfiguration.json](../README.md#botConfiguration.json) for details.
- **ApplicationMessage**, to update the bot configuration, must have the first **XmlElement** with **"botConfiguration"** as name - see chapter [Using ApplicationMessage](../README.md#UsingApplicationMessage) for details.

##  [0.0.1.3]
Configuration structure accepted - see chapter [Extended BotConfiguration](../README.md#ExtendedBotConfiguration) for details.

- **administrators** object, **bubbleInvitationAutoAccept** and **userInvitationAutoAccept**: 
- **streams**:
    - **id**: accepted
    - **media**: **ONLY** "video" is accepted
    - **uriType**: **ONLY** "other" (or null or not provided) is accepted
    - **uri**: accepted
    - **uriSettings**: accepted
    - **forceLiveStream**: accepted
    - **videoComposition**: **NOT** accepted
    - **videoFilter**: **NOT** accepted
- **conference**: 
    - **id**: accepted
    - **jid**: accepted
    - **name**: accepted
    - **audioStreamId**: **NOT** accepted
    - **videoStreamId**: accepted
    - **sharingStreamId**: accepted
