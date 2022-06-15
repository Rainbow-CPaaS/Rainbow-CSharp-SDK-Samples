![Rainbow](./../logo_rainbow.png)

# Rainbow-CSharp-SDK-Samples - WebRTC 

This sample demonstrates how to use the **Rainbow CSharp SDK WebRTC** 

This SDK provides additional services / tools for these platforms: **Windows, MacOs, and Linux**. 

**Only x64 platform is supported.**

**Android and iOS are not yet supported.**

It uses as core the **Rainbow CSharp SDK (core)** library. So you need first to understand how to use it.

## Nuget package

The SDK is provided as a Nuget package available [here](https://www.nuget.org/packages/Rainbow.CSharp.WebRTC/)

**Mostly all dependencies are managed by Nuget package system but, according our platform, it's necessary to install specifc libraries/binaries.** See the dedicated chapter for more details.

This package targets **.Net Standard 2.0**, **.Net Standard 2.1**, **.Net Core App 3.1** and **.Net 5.0**

## WebRTC service

Permits to manage WebRTC communications: **Peer To Peer** or in a **Conference** on **Linux, MacOS and Windows** using Rainbow environment (https://developers.openrainbow.com/home)

**Peer To Peer communication features**:

- Answer, Decline, Make, Retract, Hang up call
    
- Add / Remove Video or Sharing
    
- Mute / Unmute Audio, Video or Sharing
    
- Call status: Ringing incoming, ringing outgoing, in progress, ...
    
- Audio Stream: Output and Input are supported
    
- Video Stream: Output is supported. **Input is not supported** (work still in progress)

**Conference features**:

- Start, Join Conference
    
- Participants list
    
- Publishers list
    
- Mute / Unmute: current user but also other Peers if current user is a moderator of the conference

- Call status: Ringing incoming, ringing outgoing, in progress, ...
    
- Display Active Talker. **Current user of this SDK is never seen as Active Talker** (work still in progress)
    
- Add / Remove Video or Sharing
    
- Subscribe (automatically or manualy) to publication of Peers (Video or Sharing)
    
- Request / Refuse / Accept Sharing transfert
    
- Conference delegation
    
- Audio Stream: Output and Input are supported
    
- Video Stream: Ouput is supported. **Input is not supported** (work still in progress)

## Libraries specific for each platform

This SDK is based on [FFmpeg](https://www.ffmpeg.org/) libraries and [SDL2](https://www.libsdl.org) libraries. 

They are available for each platform (Windows, MacOs, and Linux) but you need to get them differently according your environement.

### Windows

#### SDL2 libraries:

Download X64 libraries from here: https://www.libsdl.org/download-2.0.php

Then they must be installed in the same folder that your executable using this SDK.  

#### FFmpeg libraries:

Download X64 libraries from here: https://github.com/GyanD/codexffmpeg/releases/download/4.4.1/ffmpeg-4.4.1-full_build-shared.zip

From this ZIP file only files in folder **bin** are used by the SDK. You can store them on your system where you want but you need to specify this folder when you create an instance of **WebRTCCommunications** object.

Example:
``` csharp
String FFMPEG_LIB_PATH = @"C:\ffmpeg-4.4.1-full_build-shared\bin";

Rainbow.Application rbApplication = new Rainbow.Application();
Rainbow.WebRTCCommunications rbWebRTCCommunications =  WebRTCCommunications.CreateInstance(rbApplication, FFMPEG_LIB_PATH); 
```

### MacOS

#### SDL2 libraries:

Download X64 libraries from here: https://www.libsdl.org/download-2.0.php

Then they must be installed in the same folder that your executable using this SDK.  

#### FFmpeg libraries:

Get x64 libraries using this command line:

`sudo apt install ffmpeg`

Then use these command line:

`ffmpeg --version`

You will have an ouput with some details about ffmpeg and about **libdir** path. 

For example something like this: **--libdir=/usr/lib/aarch64-linux-gnu**

Use this path to create a valide **WebRTCCommunications** object.

Example:
``` csharp
String FFMPEG_LIB_PATH = @"/usr/lib/aarch64-linux-gnu";

Rainbow.Application rbApplication = new Rainbow.Application();
Rainbow.WebRTCCommunications rbWebRTCCommunications =  WebRTCCommunications.CreateInstance(rbApplication, FFMPEG_LIB_PATH); 
```

### Linux

#### SDL2 libraries:

Get x64 libraries using this command line:

`sudo apt-get install libsdl2-dev`

Then they must be installed in the same folder that your executable using this SDK.

#### FFmpeg libraries:

Get x64 libraries using this command line:

`sudo apt install ffmpeg`

Then use these command line:

`ffmpeg --version`

You will have an ouput with some details about ffmpeg and about **libdir** path. 

For example something like this: **--libdir=/usr/lib/aarch64-linux-gnu**

Use this path to create a valide **WebRTCCommunications** object.

Example:
``` csharp
String FFMPEG_LIB_PATH = @"/usr/lib/aarch64-linux-gnu";

Rainbow.Application rbApplication = new Rainbow.Application();
Rainbow.WebRTCCommunications rbWebRTCCommunications =  WebRTCCommunications.CreateInstance(rbApplication, FFMPEG_LIB_PATH); 
```


