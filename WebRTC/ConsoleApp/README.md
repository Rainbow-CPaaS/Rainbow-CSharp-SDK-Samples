![Rainbow](./../logo_rainbow.png)

# ConsoleApp Sample

This sample works on Linux, MacOs and Windows

It permits to do this:

- Select an Audio Input device: a Microphone (if any), an Audio file (local or remote) or no device

- Select an Video Input device: a Camera (if any), a Monitor, an Video file (local or remote) or no device

- Connect to Rainbow server

- Search an existing user

- Call this user using Audio or Audio+Video

Ensure to configure correctly **SDL2** and **FFmpeg** Libraries (please [read](../README.md) this)

Ensure to set **APP_ID, APP_SECRET_KEY** and **HOST_NAME** (in file **ApplicationInfo.cs**)

You must also ensure to have correct credentials: **LOGIN_USER, PASSWORD_USER** (in file **ApplicationInfo.cs**)
 
**NOTE:**
- On MacOS, access to Camera or Monitor needs specific permissions. Ensure to set them so the terminal/console can use them.

**More important details [here](../README.md)**
