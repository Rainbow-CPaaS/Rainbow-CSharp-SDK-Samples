![Rainbow](./../../../logo_rainbow.png)

# Rainbow CSharp SDK v3 - Manages users and Guests

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

Using an admin account which can create user or guests:
- List (if any) organisations
- List Compagnies
- Create, Delete Company Link
- Create User using Company Link
- Create / Get Bubble Link
- Create User as GuestMode using Bubble Link
- Create / List User
- Delete User (includes User created using Company Link)
- Create / List / Delete Guest

## File exeSettings.json

You need to set correctly the file "exeSettings.json" like described in chapter [File exeSettings.json](./../../ConfigurationFiles.md#exeSettings.json) - no need here to use external dependencies.

## File credentials.json

You need to set correctly the file "credentials.json" like described in chapter [File credentials.json](./../../ConfigurationFiles.md#credentials.json).
