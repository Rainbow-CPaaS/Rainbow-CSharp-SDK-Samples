![Rainbow](./../../../logo_rainbow.png)

# Rainbow CSharp SDK v3 - Log Request as User

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
Permits to understand as user how to use the **CustomerCare** service of the SDK.

This example and the "Log Request as Admin" are related and must be used in same time.

This examples permits:
- to select how to login on Rainbow server:
	- Use a TV activation code
	- Use a Login / password set in **credentials.json** file
	- Use previous login information already set
- Once loggued, wait log request from administrator. They are automatically accepted.

## File exeSettings.json

You need to set correctly the file "exeSettings.json" like described in chapter [File exeSettings.json](./../../ConfigurationFiles.md#exeSettings.json).

## File credentials.json

You need to set correctly the file "credentials.json" like described in chapter [File credentials.json](./../../ConfigurationFiles.md#credentials.json).
