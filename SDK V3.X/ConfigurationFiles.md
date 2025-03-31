![Rainbow](./../logo_rainbow.png)

# Rainbow CSharp SDK v3 - Configuration Files

According the example, one or several JSON files are used to configure it.

This document explains the structure of each JSON files.

Once a file is modified, this [online tool](https://jsonlint.com/) can be used to ensure that a valid JSON is used. If it's not the case the file can not be parsed.

## Table of content

- [File exeSettings.json](#exeSettings.json)
- [File credentials.json](#credentials.json)
- [File streams.json](#streams.json)

<a name="exeSettings.json"></a>
## File exeSettings.json
This file must be stored in **./config** folder path.

The structure of this file is always the same. It looks like this:
```javascript
{
  "exeSettings": {
    "useAudioVideo": false,
    "ffmpegLibFolderPath": "C:\\ffmpeg-7.0.1-full_build-shared\\bin",
    "logFolderPath": ".\\logs",
    "s2sCallbackURL": "",
    "cultureInfo": "en-US"
  }
}
```
**Details:**
- **useAudioVideo**: Boolean (false by default) - True if the current example is using audio / video medias.
- **ffmpegLibFolderPath**: String (null/empty by default) - Must be set to a valid folder path to FFmpeg libraries if **useAudioVideo** is set to true.
- **logFolderPath**: String (".\\logs" by default) - Folder path where logs of the example will be stored.
- **s2sCallbackURL**: String (null/empty by default) - Must be set to a valid S2S callback URL if **S2S** is used in the example.
- **cultureInfo**: String (null/empty by default) - Used in TerminalApp context, to specify a specific culture info - see https://learn.microsoft.com/fr-fr/dotnet/api/system.globalization.cultureinfo.getcultureinfo


<a name="credentials.json"></a>
### File credentials.json
This file must be stored in **./config** folder path.

The structure of this file is always the same. It looks like this:
```javascript
{
  "credentials": {
    "serverConfig": {
      "appId": "01234567890123456789",
      "appSecret": "98765432109876543210",
      "hostname": "openrainbow.net",
      "oauthPorts": [ 80, 8080, 1234, 9876, 4567 ],
      "ssoRedirectUrl": "myappscheme://callback/"
    },

    "usersConfig": [
      {
        "iniFolderPath": ".\\logs",
        "prefix": "User1",
        "login": "my_account_1@my_domain.com",
        "password": "my_password1",
        "autoLogin": true
      },
      {
        "iniFolderPath": ".\\logs",
        "prefix": "User2",
        "login": "my_account_2@my_domain.com",
        "password": "my_password2",
        "autoLogin": true
      },
      {
        "iniFolderPath": ".\\logs",
        "prefix": "User3",
        "apiKey": "ad561fe5471gf3f56d7g413d"
      }
    ]
  }
}
```
**Details:**
- **credentials**: To define all credentials (and a little more) to connect to the Rainbow Server  
  - **serverConfig**: See [developer journey](https://developers.openrainbow.com/doc/hub/developer-journey) for more details
      - **appId**: Application Id (its value is dependent of the host name used)
      - **appSecret**: Application Secret(its value is dependent of the host name used)
      - **hostname**: Host name used to connect to the Rainbow server (examples: **openrainbow.com**, **openrainbow.net**, ...)
      - **oauthPorts**: Optional - used only in OAuth authentication context - List of port to use for the redirect Uri
      - **ssoRedirectUrl**: Optional - used only in SSO authentication context - SSO redirect Uri
      
  - **usersConfig**: Permits to define one or several accounts
      - **iniFolderPath**: String (".\\logs" by default) - Folder path where INI file of this account will be stored. If possible use the same folder where logs are store - see **logFolderPath** in [File exeSettings.json](#exeSettings.json)
      - **prefix**: String (must not be empty/null) - Prefix used for this account
      - **login**, **password**: Login (email address) Password used by this account to connect to Rainbow server
      - **apiKey**: Api Key used to connect to Rainbow server (instead of login / password)

<a name="streams.json"></a>
## File streams.json

This file must be stored in **./config** folder path.

The structure of this file is always the same. It looks like this:
```javascript
{
    "streams": [
      {
        "id": "id1",
        "media": "audio",
        "uri": "c:\\media\\myAudioFile.mp4",
        "connected": false
      },
      {
        "id": "id2",
        "media": "video",
        "uri": "c:\\media\\myVideoFile.mp4",
        "connected": false
      },
      {
        "id": "id3",
        "media": "video",
        "uri": "rtsp://rtsp.stream/movie",
        "uriSettings": {
          "rtsp_transport": "tcp",
          "max_delay": "0"
        },
        "forceLiveStream": false,
        "connected": false
      },
      {
        "id" : "screen1",
        "media" : "video",
        "uri" : "Composite GDI Display",
        "uriType" : "screen"
      },
      {
        "id" : "webcam1",
        "media" : "video",
        "uri" : "HD Pro Webcam C920",
        "uriType" : "webcam"
      },
      {
        "id" : "microphone1",
        "media" : "audio",
        "uri" : "Microphone (HD Pro Webcam C920)",
        "uriType" : "microphone"
      }
      ,
      {
        "id" : "composition1",
        "media" : "composition",
        "videoComposition" : [ "id2", "webcam1"],
        "videoFilter" : "[1]setpts=PTS-STARTPTS,scale=640:-1[scaled0];[0]setpts=PTS-STARTPTS,scale=250:-1[scaled1];[scaled0][scaled1]overlay=main_w-overlay_w-10:main_h-overlay_h-10"
      }
    ]
}
```
**Details:**
- **streams**: to define one of several streams and how to connect / use them
    - **id**: String (cannot be null/empty) - Unique identifier of the stream
    - **media**: String (possible values: "audio", "video", "audio+video", "composition")
        - Media to grab from this stream
        - If the stream contains audio and video but you want only to use it for video purpose use "video". It permits to avoid to decode audio part.
        - "composition" permits to create a video stream using one of several streams (see **videoComposition**) based on a **videoFilter**
    - **uriType**: String (possible values: "other" (default value if null or not provided), "screen", "webcam", "microphone")
        - to define the type of the uri
        - "screen" to grab stream from a plugged screen, "webcam" to grab stream from a plugged webcam, "microphone" to grab stream from a plugged microphone.
        - "other" (or null or not provided) to grab stream from any other source.
    - **uri**: String (cannot be null/empty) - Uri to use to connect to the stream
        - can be a path to a local or remote file - for example "c:\\media\\myVideoFile.mp4"
        - can be an URL (http, ftp, rtsp, ...). You can specify a login/pwd like this: rtsp://myLogin:myPwd@rtsp.stream/movie
        - can be the full name of the local device to use (when "screen", "webcam" or "microphone" are used for **uriType**)
        - can be null or not provided if a "composition" is used
    - **uriSettings**: Object (can be null or not provided)
        - More settings to enhance connection to the stream.
        - One or several settings can be set. It's related to the protocol used and options available in [ffmpeg](https://ffmpeg.org/ffmpeg-protocols.html).
    - **connected**: Boolean (can be null or not provided - false by default) - If True it means the example will connect and stay connected to the stream even if this stream is not used in the conference or if there is no conference.
    - **forceLiveStream**: Boolean (can be null or not provided - null by default) - Due to some internal restrictions, remote stream could be not grabbed well. Using true or false can help to resolve this situation.
    - **videoComposition**: Array of String (can be null or not provided - null by default)
        - Mandatory if **media** is set as "composition"
        - The array contains id of video stream used to create the composition. Order used is important and it's related to the **videoFilter**
    - **videoFilter**: String (can be null or not provided - null by default)
        - Mandatory if **media** is set as "composition"
        - Can be used also if **media** is set as "video", "audio+video" to use a filter of the original stream
        - If used a valid filter for ffmpeg must be used - see [ffmpeg video filters documentation](https://ffmpeg.org/ffmpeg-filters.html#Video-Filters)