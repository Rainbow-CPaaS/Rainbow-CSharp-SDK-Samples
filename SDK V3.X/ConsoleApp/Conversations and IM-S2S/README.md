![Rainbow](./../../../logo_rainbow.png)

# Rainbow CSharp SDK v3 - Conversations and IM using S2S

This console application permits to understand how to use the package [Rainbow.CSharp.SDK](https://www.nuget.org/packages/Rainbow.CSharp.SDK).

Documentation is available [here](https://developers.openrainbow.com/doc/sdk/csharp/core/lts/guides/001_getting_started)

We use Server To Server (S2S) in this example - read this [guide](https://developers.openrainbow.com/doc/sdk/csharp/core/lts/guides/035_events_mode) for more info.

## Prerequisites

### Rainbow API HUB

This SDK is using the [Rainbow environment](https://developers.openrainbow.com/)
 
This environment is based on the [Rainbow service](https://www.openrainbow.com/) 

### Rainbow CSharp SDK

To have more info about the SDK:
- check [Getting started guide](https://developers.openrainbow.com/doc/sdk/csharp/core/lts/guides/001_getting_started)
- check [API documentation](https://developers.openrainbow.com/doc/sdk/csharp/core/lts/api/Rainbow.Application)

## Features

It's the same example available stored [here](https://github.com/Rainbow-CPaaS/Rainbow-CSharp-SDK-Samples/tree/master/SDK%20V3.X/ConsoleApp/Conversations%20and%20IM) but using S2S (Server To Server) and not XMPP event mode
- Conversations: list, delete, open
- Instant Messaging: send / receive messages, get older messages, manage receipts

Only differences are:
- S2S can be used instead of XMPP - read this [guide](https://developers.openrainbow.com/doc/sdk/csharp/core/lts/guides/035_events_mode) for more info.
- we use ngrok (or an equivalent) to simulate a public web site accessible from Rainbow server.
- We launch **ngrok** with this command line: **ngrok http --host-header="localhost:9870" 9870**
- It indicates to create a public HTTP/HTTPS web server on internet
- All requests performed on public web server will redirected locally on port 9870 whit an host-header equals to "localhost:9870"
- In the output of ngrok tools, we can see something like this:
**Forwarding https://8764-147-161-181-116.ngrok-free.app -> http://localhost:9870** 
- It means that the public web site URL is https://8764-147-161-181-116.ngrok-free.app
- This URL which must be used as S2SCallback URL when configuring Rainbow Application:
	- RbApplication.SetS2SCallbackUrl(exeSettings.S2SCallbackURL);
- Finally, we need to start a local web server using the 9870 (same port as previously set when launching ngrok)
- Each requests performed on the local web server must be parsed by the Rainbow S2SEventPipe instance
- Extract of file **CallbackWebModule.cs**:

```csharp
    protected override async Task OnRequestAsync(IHttpContext context)
    {
        Boolean result = false;

        log.LogDebug("[OnRequestAsync] RequestedPath:[{Path}] - HttpVerb:[{HttpVerb}]", context.Request.Url.AbsolutePath, context.Request.HttpVerb);

        if (context.Request.Headers["Content-Type"] == "application/json")
        {
            String body = await context.GetRequestBodyAsStringAsync();
            result = await s2sEventPipe.ParseCallbackContentAsync(context.Request.HttpVerb.ToString(), context.Request.Url.AbsolutePath, body);
        }

        if (!result)
            throw HttpException.NotAcceptable();
    }
```


## File exeSettings.json

You need to set correctly the file "exeSettings.json" like described in chapter [File exeSettings.json](./../../ConfigurationFiles.md#exeSettings.json) - no need here to use external dependencies.

## File credentials.json

You need to set correctly the file "credentials.json" like described in chapter [File credentials.json](./../../ConfigurationFiles.md#credentials.json).
