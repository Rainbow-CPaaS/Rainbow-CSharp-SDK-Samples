![Rainbow](./../../../logo_rainbow.png)

# Rainbow CSharp SDK v3 - Login with OAuth

This console application permits to understand how to use the package [Rainbow.CSharp.SDK](https://www.nuget.org/packages/Rainbow.CSharp.SDK).

Documentation is available [here](https://developers.openrainbow.com/doc/sdk/csharp/core/lts/guides/001_getting_started)

## Prerequisites

### Rainbow API HUB

This SDK is using the [Rainbow environment](https://developers.openrainbow.com/)
 
This environment is based on the [Rainbow service](https://www.openrainbow.com/) 

### Rainbow CSharp SDK

To have more info about the SDK:
- check [Getting started guide](https://developers.openrainbow.com/doc/sdk/csharp/core/lts/guides/001_getting_started)
- check [API documentation](https://developers.openrainbow.com/doc/sdk/csharp/core/lts/api/Rainbow.Application)

## Features
- To understand how to connect to Rainbow server without the need to specify any user credentials in the config.
- It's necessary to define for you Application (Id / Secret Key) valid OAuth 2.0 information and redirect URI
- They are used to create a small web server with the first avaialble port to wait until OAuth authentication process is performed
- Events used from **Application** service: **AuthenticationFailed, AuthenticationSucceeded** and **ConnectionStateChanged** 
- Events used from **AutoReconnection** service: **Cancelled, Started, MaxNbAttemptsReached** and **TokenExpired** 

## File exeSettings.json

You need to set correctly the file "exeSettings.json" like described in chapter [File exeSettings.json](./../../ConfigurationFiles.md#exeSettings.json) - no need here to use external dependencies.

## File credentials.json

You need to set correctly the file "credentials.json" like described in chapter [File credentials.json](./../../ConfigurationFiles.md#credentials.json).
