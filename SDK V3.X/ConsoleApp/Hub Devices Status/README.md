![Rainbow](./../../../logo_rainbow.png)

# Rainbow CSharp SDK v3 - Hub Device Status

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

### Web Hook / Callback URL

This example is using a Web Hook - also called Server To Server (S2S) used by the Rainbow server to reach a web server (using a callback URL) to provide information when an event is triggered.

More details about S2S / Web Hook can be found [here](https://developers.openrainbow.com/doc/sdk/csharp/core/lts/guides/035_events_mode)

This example create a local web server using "localhost" on port 9870 (i.e. "http://localhost:9870")

To have this local web server accessible from Rainbow Server with use the tool [ngrok](https://ngrok.com/). (An equivalent tool could be used)

This tool is launched using this command line:
**ngrok http --host-header="localhost:9870" 9870**

In the output of **ngrok tool**, we can see something like this:
- **Forwarding https://8764-147-161-181-116.ngrok-free.app -> http://localhost:9870** 
- It means that the public web site URL is https://8764-147-161-181-116.ngrok-free.app
- This URL must be used as Callback URL in file exeSettings.json (more info in a next chapter)

## Features

Using an admin account it permits to get Hub Telephony device state (in service / out of service) of all the company of the admin account.

It's necessary to perform this steps to have a static view of all Hub Devices State:
- Get all users of the company (to associate correctly a device to a user)
- Get all Cloud PBX used by this company
- Loop on each Cloud Pbx to get all Hub Devices used by this company
- Loop on each Hub Devices to get from server their state

Then to have a dynamic view of all Hub Devices State (i.e. event will be received when a Hub Device State has changed), we need to:
- Get list of Company Event Subscriptions and if any delete them
- Get list of Company Event WebHooks and if any delete them
- Create a Company Event WebHook using a Callback Url
- Create Company Event Subscription using the previously Company Event WebHook created using "device_state" as event type
- Now each time a Hub Device state is updated the Callback URL is used by the Rainbow server.


## File exeSettings.json

You need to set correctly the file "exeSettings.json" like described in chapter [File exeSettings.json](./../../ConfigurationFiles.md#exeSettings.json) - no need here to use external dependencies.

A callback URL must be set in this example using "s2sCallbackURL" property.

## File credentials.json

You need to set correctly the file "credentials.json" like described in chapter [File credentials.json](./../../ConfigurationFiles.md#credentials.json).
