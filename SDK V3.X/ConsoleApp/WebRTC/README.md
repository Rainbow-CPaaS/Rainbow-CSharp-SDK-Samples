![Rainbow](./../../../logo_rainbow.png)

# Rainbow CSharp SDK v3 - WebRTC

This console application permits to understand how to use the package [Rainbow.CSharp.SDK.Desktop](https://www.nuget.org/packages/Rainbow.CSharp.SDK.WebRTC.Desktop).

Documentation is available [here](https://developers.openrainbow.com/doc/sdk/csharp/webrtc.desktop/lts/guides/001_getting_started)

## Prerequisites

### Rainbow API HUB

This SDK is using the [Rainbow environment](https://developers.openrainbow.com/)
 
This environment is based on the [Rainbow service](https://www.openrainbow.com/) 

### Rainbow CSharp SDK

To have more info about the SDK:
- check [Getting started guide](https://developers.openrainbow.com/doc/sdk/csharp/core/lts/guides/001_getting_started)
- check [API documentation](https://developers.openrainbow.com/doc/sdk/csharp/core/lts/api/Rainbow.Application)

## Features
- Join / Start a Conference
- Select / Change Medias input: Audio, Video, Sharing
- Select / Change Audio Output: to play audio of the conf in the selected device
- Subscribe to MediaPublications: Audio (necessary to hear audio of the conference), Video (only one in same time), Sharing

## File exeSettings.json

You need to set correctly the file "exeSettings.json" like described in chapter [File exeSettings.json](./../../ConfigurationFiles.md#exeSettings.json).

## File credentials.json

You need to set correctly the file "credentials.json" like described in chapter [File credentials.json](./../../ConfigurationFiles.md#credentials.json).

## File streams.json

You need to set correctly the file "streams.json" like described in chapter [File streams.json](./../../ConfigurationFiles.md#streams.json).

## Restrictions
Not all settings from **streams.json** are supported for code simplification: only **audio**, **video** or **audio+video** is supported, **videoFilter** is not supported

Check also [CHANGELOG](CHANGELOG.md)

## Dependencies

FFmpeg and SDL2 libraries are mandatory.

Please refer to this [README](https://github.com/Rainbow-CPaaS/Rainbow-CSharp-SDK-Samples/blob/master/Binaries/README.md) to know which version must be used and how to download them.