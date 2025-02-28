![Rainbow](./../../../logo_rainbow.png)

# Rainbow CSharp SDK v3 - MediaPlayer

This console application permits to understand how to use the package [Rainbow.CSharp.SDK.Medias](https://www.nuget.org/packages/Rainbow.CSharp.SDK.Medias/).

Documentation is available [here](https://developers.openrainbow.com/doc/sdk/csharp/medias/lts/guides/001_getting_started)

## Features
- Provide a simple Media Player
- Can use audio and/or video file stored locally or remotely
- Can use audio and/or video stream using protocols like RTSP, HTTP, ...
- To define streams, the file "streams.json" must be used. 
- It's also possible to add to this file, streams using plugged devices like: "webcam", "screen" or "microphone"
- Using your keyboard (the console must have the focus), you can select / update at any time which streams to use.

## File exeSettings.json

You need to set correctly the file "exeSettings.json" like described in chapter [File exeSettings.json](./../../ConfigurationFiles.md#exeSettings.json).

## File streams.json

You need to set correctly the file "streams.json" like described in chapter [File streams.json](./../../ConfigurationFiles.md#streams.json).

## Restrictions
Not all configuration settings in file **streams.json** are yet available. 

Check [CHANGELOG](CHANGELOG.md)

## Dependencies

FFmpeg and SDL2 libraries are mandatory.

Please refer to this [README](https://github.com/Rainbow-CPaaS/Rainbow-CSharp-SDK-Samples/blob/master/Binaries/README.md) to know which version must be used and how to download them.