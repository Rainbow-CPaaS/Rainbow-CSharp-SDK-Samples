![Rainbow](./../../../logo_rainbow.png)

# Rainbow CSharp SDK v3 - MediaPlayer

This console application permits to understand how to use the package [Rainbow.CSharp.SDK.Medias](https://www.nuget.org/packages/Rainbow.CSharp.SDK.Medias/).

Documentation is available [here](https://developers.openrainbow.net/doc/sdk/csharp/medias/guides/001_getting_started)

## Features
- Provide a simple Media Player
- Can use audio and/or video file stored locally or remotely
- Can use audio and/or video stream using protocols like RTSP, HTTP, ...
- To define streams, the file "streamsSettings.json" must be used. 
- It's also possible to add to this file, streams using plugged devices like: "webcam", "screen" or "microphone"
- Using your keyboard (the console must have the focus), you can select / update at any time which streams to use.

## File exeSettings.json

You need to set correctly the file "exeSettings.json" like described in chapter [File exeSettings.json](./../../Bot/README.md#exeSettings.json) to use some dependencies listed in next chapter.

## File streamsSettings.json

You need to set correctly the file "streamsSettings.json". It uses exactly the same structure that the object **streams** defined in chapter [Extended BotConfiguration](./../../Bot/README.md#ExtendedBotConfiguration) of the [Bot Brocaster example](./../../Bot/BotBroadcaster/README.md).

## Restrictions
Not all configuration settings are yet available. 

Check [CHANGELOG](CHANGELOG.md)

## Dependencies

FFmpeg and SDL2 libraries are mandatory.

Please refer to this [README](https://github.com/Rainbow-CPaaS/Rainbow-CSharp-SDK-Samples/blob/master/Binaries/README.md) to know which version must be used and how to download them.