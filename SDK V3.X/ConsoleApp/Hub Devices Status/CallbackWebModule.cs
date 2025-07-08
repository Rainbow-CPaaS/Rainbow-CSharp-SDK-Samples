using EmbedIO;
using Microsoft.Extensions.Logging;
using Rainbow.Example.Common;
using Swan.Logging;


public class SwanLogger : Swan.Logging.ILogger
{
    public const String LOGGER_NAME = "WebHook";

    public Microsoft.Extensions.Logging.ILogger log = null;

    Swan.Logging.LogLevel Swan.Logging.ILogger.LogLevel => Swan.Logging.LogLevel.Debug;


    public void Dispose()
    {

    }

    public void Log(LogMessageReceivedEventArgs logEvent)
    {
        if(log is null)
        {
            NLogConfigurator.AddLogger(LOGGER_NAME);
            log = Rainbow.LogFactory.CreateLogger(LOGGER_NAME);
        }
        log.LogDebug(logEvent.Message);
    }
}

public class CallbackWebModule : WebModuleBase
{
    // Define log object - use same repository than the SDK
    private static readonly Microsoft.Extensions.Logging.ILogger log = Rainbow.LogFactory.CreateLogger(SwanLogger.LOGGER_NAME);

    private Rainbow.WebHookEventPipe webHookEventPipe;

    public CallbackWebModule(string baseRoute, Rainbow.Application application) : base(baseRoute)
    {
        webHookEventPipe = application.GetWebHookEventPipe();
    }

    protected override async Task OnRequestAsync(IHttpContext context)
    {
        if (context.Request.Headers["Content-Type"] == "application/json")
        {
            String body = await context.GetRequestBodyAsStringAsync();
            log.LogDebug("[OnRequestAsync] RequestedPath:[{Path}] - HttpVerb:[{HttpVerb}] - Body:[{Body}]", context.Request.Url.AbsolutePath, context.Request.HttpVerb, body);
            await webHookEventPipe.ParseCallbackContentAsync(context.Request.HttpVerb.ToString(), body);
        }
    }

    public override bool IsFinalHandler { get; } = true;
}
