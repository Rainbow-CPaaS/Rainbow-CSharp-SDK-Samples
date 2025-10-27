![Rainbow](./../../../logo_rainbow.png)

# Rainbow CSharp SDK v3 - WebRTC

## Bugs / Restrictions known:
- limit automatically video fps/size according media (video vs sharing) when a file is used to avoid bad display for remote (already done when webcam or screen is used).
- Window to display Video / Sharing:
	- ratio is not changed according remote stream
- "Streams.json": 
	- Media supported: 'audio', 'video', 'audio+video'
	- UriType supported: 'other' (or null/empty value)

## [0.8.4]
- Fix: 
	- Ensure to use Main Thread to manage Window used for Video / Sharing display

## [0.8.3]
- Enhancement: 
	- allow to set RTP Audio options using "rtpAudioOptions.json"
	- allow to set RTP Video options using "rtpVideoOptions.json"

## [0.8.2]
- Enhancement: allow to select a stream from "streams.json" for any media input (audio, video or sharing)

## [0.8.1]
- Use official SDK:
	- Rainbow.CSharp.SDK.WebRTC: 2.0.0
	- Rainbow.CSharp.SDK.WebRTC.Desktop: 2.0.0 
- Enhancement: Allow to manage P2P calls (make, answer and take call)

## [0.8]
- Use SDK GLOBAL Version: 3.0.0-sts.4.2-20
- FIX: After some delay, incoming audio played is noisy
- Enhancement: Use windows to display Video (only once in same time) and/or Sharing subscribed as Media Publication
- Enhancement: Display info when DC (local or remote) is closed

## [0.7]
- Use SDK GLOBAL Version: 3.0.0-sts.4.2-12
- FIX: Subscription to a Video MediaPublication don't remove, if any, the previous one
- FIX: Cannot unsubcribe to Audio MediaPublication
- FIX: No OnTrack event about Audio / Video / Sharing when MediaPublication is unsubscribed
- FIX: No OnTrack event about Audio / Video / Sharing when it's removed or updated locally
- FIX: Can now select **No Audio Output** (even if conference is in progress)
- Enhancement: Display media using String and not Int value

## [0.6]
- Use SDK GLOBAL Version: 3.0.0-sts.4.2-11
- Conferences: start (searching a bubble), join or quit (with or without audio)
- Audio input: selectio: None, Emptry track or select a file / device
- Audio input: event when a device is added/removed
- Audio output: selection: None or select a device
- Audio output: event when a device is added/removed
- Video input: selection: None or select a file / device
- Video input: event when a device is added/removed
- Sharing input: selection: None or select a file / device
- Sharing input: event when a device is added/removed
- DataChannel: add / remove to conference a DC
- MediaPublication: event:  when a media is added/removed in the conf
- MediaPublicatios: subscribe/unsubscribe:  to audio, video or sharing (only one video can be subscribed in sametime, the previous one is then unsubscribe). Media subscription are save in associated files (except for Audio) 
- Menu permits to know devices available / used and current config