![Rainbow](../logo_rainbow.png)

 
# Rainbow CSharp SDK - Binaries
---

This folder contains several binaries / libraries files usefull to test / use Rainbow C# SDK.

## SDL2 and FFmpeg libraries

They are mandatory if you use [Rainbow.CSharp.WebRTC](https://www.nuget.org/packages/Rainbow.CSharp.WebRTC/) or [Rainbow.CSharp.Medias](https://www.nuget.org/packages/Rainbow.CSharp.Medias/)  

Info about [FFmpeg](https://www.ffmpeg.org/) libraries and [SDL2](https://www.libsdl.org) libraries. 

For **FFmpeg** version 7.0.1 must be used.

For **SDL2** version 2.0.22 must be used.

They are available for each platform (Windows, MacOs, and Linux) but you need to get them differently according your environement.

### Windows

#### SDL2 libraries:

Download X64 libraries from here: https://www.libsdl.org/download-2.0.php

They are available also [here](./Windows/SDL2-2.0.22)

Then they must be installed **in the same folder** that your executable using this SDK.

#### FFmpeg libraries:

Download X64 libraries from here: https://github.com/GyanD/codexffmpeg/releases/download/7.0.1/ffmpeg-7.0.1-full_build-shared.7z

They are available also [here](./Windows/ffmpeg-7.0.1-full_build-shared)

Only files in folder **bin** are used by the SDK. You can store them on your system where you want but you need to specify this folder when you create an instance of **WebRTCCommunications** object.

Example:
``` csharp
String FFMPEG_LIB_PATH = @"C:\ffmpeg-7.0.1-full_build-shared";

Rainbow.Medias.Helper.InitExternalLibraries(FFMPEG_LIB_PATH);
```

### MacOS

#### SDL2 libraries:

Download X64 libraries from here: https://www.libsdl.org/download-2.0.php

Then they must be installed **in the same folder** that your executable using this SDK.

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

## Medias files

This [folder](./Medias) contains severals Audio/Video files usefull when using / testing [Rainbow.CSharp.WebRTC](https://www.nuget.org/packages/Rainbow.CSharp.WebRTC/) or [Rainbow.CSharp.Medias](https://www.nuget.org/packages/Rainbow.CSharp.Medias/)

