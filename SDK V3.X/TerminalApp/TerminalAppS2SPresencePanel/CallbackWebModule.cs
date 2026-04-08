using EmbedIO;
using Microsoft.Extensions.Logging;
using Rainbow;
using Swan.Logging;

public class CallbackWebModule : WebModuleBase
{
    // Define log object - use same repository than the SDK
    private static readonly Microsoft.Extensions.Logging.ILogger log = Rainbow.LogFactory.CreateLogger("CallbackWebModule");

    public CallbackWebModule(string baseRoute) : base(baseRoute)
    {
    }

    protected override async Task OnRequestAsync(IHttpContext context)
    {
        log.LogDebug("[OnRequestAsync] RequestedPath:[{Path}] - HttpVerb:[{HttpVerb}]", context.Request.Url.AbsolutePath, context.Request.HttpVerb);

        if (context.Request.Headers["Content-Type"] == "application/json")
        {
            String body = await context.GetRequestBodyAsStringAsync();
            var sdkResult = await Rainbow.S2SEventPipe.ParseCallbackContentAsync(context.Request.HttpVerb.ToString(), context.Request.Url.AbsolutePath, body);
            if (!sdkResult.Success)
                log.LogWarning(sdkResult.Result.ToString());
        }
    }

    public override bool IsFinalHandler { get; } = true;
}

public class SwanLogger : Swan.Logging.ILogger
{
    const String LOGGER_NAME = "S2S";

    public Microsoft.Extensions.Logging.ILogger log = null;

    Swan.Logging.LogLevel Swan.Logging.ILogger.LogLevel => Swan.Logging.LogLevel.Debug;

    public void Dispose()
    {

    }

    public void Log(LogMessageReceivedEventArgs logEvent)
    {
        log ??= Rainbow.LogFactory.CreateLogger(LOGGER_NAME);
        log.LogDebug(logEvent.Message);


    }
}