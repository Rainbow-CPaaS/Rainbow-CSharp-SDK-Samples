![Rainbow](./../../../logo_rainbow.png)

# Rainbow CSharp SDK v3 - Bot Broadcaster

##  [0.0.2] - 2025-02-XX
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
