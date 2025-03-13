using EmbedIO;
using Microsoft.Extensions.Logging;

public class CallbackWebModule : WebModuleBase
{
    // Define log object - use same repository than the SDK
    private static readonly ILogger log = Rainbow.LogFactory.CreateS2SLogger();

    private Rainbow.S2SEventPipe s2sEventPipe;

    public CallbackWebModule(string baseRoute) : base(baseRoute)
    {
        s2sEventPipe = Rainbow.S2SEventPipe.Instance;
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
        Boolean result = false;
        log.LogDebug("[OnRequestAsync] RequestedPath:[{Path}] - HttpVerb:[{HttpVerb}]", absolutePath, httpVerb);

        if (contentType == "application/json")
        {
            result = await s2sEventPipe.ParseCallbackContentAsync(httpVerb, absolutePath, body);
        }

        if (!result)
            throw HttpException.NotAcceptable();
    }

    public override bool IsFinalHandler { get; } = true;
}
