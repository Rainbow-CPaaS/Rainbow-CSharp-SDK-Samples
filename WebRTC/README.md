![Rainbow](./../logo_rainbow.png)

# Rainbow-CSharp-SDK-Samples - WebRTC 

This folder contains samples which demonstrates how to use the **Rainbow CSharp SDK WebRTC** 

This SDK provides additional services / tools for these platforms: **Windows, MacOs, and Linux**. 

**Only x64 platform is supported.**

**Android and iOS are not yet supported.**

It uses the **Rainbow CSharp SDK (core)** library. Check a look [here](../README.md)

It uses the **Rainbow CSharp SDK Medias** library. Check a look [here](../Medias/README.md)

So you need first to understand how to use them.

Full Documentation of **Rainbow CSharp SDK (core)** is available **[here](https://developers.openrainbow.com/doc/sdk/csharp/core/lts/guides/001_getting_started)**

Full Documentation of **Rainbow CSharp SDK Medias** is available **[here](https://developers.openrainbow.com/doc/sdk/csharp/medias/sts/guides/001_getting_started?isBeta=true)**

Full Documentation of **Rainbow CSharp SDK WebRTC** is available **[here](https://developers.openrainbow.com/doc/sdk/csharp/webrtc/sts/guides/001_getting_started?isBeta=true)**

## Nuget package

The SDK is provided as a Nuget package available [here](https://www.nuget.org/packages/Rainbow.CSharp.WebRTC/)

**Mostly all dependencies are managed by Nuget package system but, according our platform, it's necessary to install specifc libraries/binaries.** See the dedicated chapter for more details.

This package targets **.Net Standard 2.0**, **.Net Standard 2.1**, **.Net Core App 3.1** and **.Net 5.0**

## WebRTC service

Permits to manage WebRTC communications: **Peer To Peer** or in a **Conference** on **Linux, MacOS and Windows** using [Rainbow environment](https://developers.openrainbow.com/home).

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
    
- Audio Stream: Output is supported. **Input is not supported** (work still in progress)
    
- Video Stream: Ouput is supported. **Input is not supported** (work still in progress)

## SDL2 and FFmpeg libraries

To use this samples you need to download and install SDL2 and FFmpeg libraries.

Please [refer](../Binaries/README.md) to this docmentation for all details.


