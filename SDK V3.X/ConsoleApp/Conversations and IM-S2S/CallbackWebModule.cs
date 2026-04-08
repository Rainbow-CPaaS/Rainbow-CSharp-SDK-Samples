using EmbedIO;
using Microsoft.Extensions.Logging;

public class CallbackWebModule : WebModuleBase
{
    // Define log object - use same repository than the SDK
    private readonly ILogger log;

    public CallbackWebModule(string baseRoute,string logPrefix) : base(baseRoute)
    {
        log = Rainbow.LogFactory.CreateLogger<CallbackWebModule>(logPrefix);
    }

    protected override async Task OnRequestAsync(IHttpContext context)
    {
        // We get the HTTP verb of the request (POST, GET, ...)
        String httpVerb = context.Request.HttpVerb.ToString();

        // We get the AbsolutePath of the request
        String absolutePath = context.Request.Url.AbsolutePath;

        // We get the body of the request
        String body = await context.GetRequestBodyAsStringAsync();

        // And finally we need the content-type of the body
        String? contentType = context.Request.Headers["Content-Type"];

        // We use Rainbow.S2SEventPipe instance with all this info
        log.LogDebug("[OnRequestAsync] RequestedPath:[{Path}] - HttpVerb:[{HttpVerb}]", absolutePath, httpVerb);

        if (contentType == "application/json")
        {
            var sdkResult = await Rainbow.S2SEventPipe.ParseCallbackContentAsync(httpVerb, absolutePath, body);
            if(!sdkResult.Success)
                log.LogWarning(sdkResult.Result.ToString());
        }
    }

    public override bool IsFinalHandler { get; } = true;
}
