![Rainbow](./../../../logo_rainbow.png)

# Rainbow CSharp SDK v3 - Bot Broadcaster

Please refer first to this [README](../README.md): all details and JSON configuration files are fully explained.

## Restrictions
Not all configuration seetings are yet available. 

We list here (based on chapter [Extended BotConfiguration](../README.md#ExtendedBotConfiguration)) settings which are limited or not available for the moment.
- **streams**
    - **media**: only "video" is supported
    - **uriType**: only "other" (or null or not provided) is supported 
    - **videoComposition**: not supported
    - **videoFilter**: not supported
- **conference**: 
    - **audioStreamId**: not supported

## Dependencies

FFmpeg and SDL2 libraries are mandatory.

Please refer to [README](https://github.com/Rainbow-CPaaS/Rainbow-CSharp-SDK-Samples/blob/master/Binaries/README.md) to know which version must be used and how to download them.