using EmbedIO;
using Microsoft.Extensions.Logging;

namespace Rainbow.Example.CommonWebHook
{
    /// <summary>
    /// Class used with Rainbow.Application object to manage S2S connection
    /// </summary>
    public class CallbackS2SModule : WebModuleBase
    {
        // Define log object - use same repository than the SDK
        private readonly ILogger log;

        public CallbackS2SModule(string baseRoute, Rainbow.Application application) : base(baseRoute)
        {
            // Create logger
            log = Rainbow.LogFactory.CreateLogger<CallbackS2SModule>(application.LoggerPrefix);
        }

        protected override async Task OnRequestAsync(IHttpContext context)
        {
            if (context?.Request is null) return;

            // We need the content-type of the body
            String? contentType = context.Request.Headers["Content-Type"];

            if (contentType == "application/json")
            {
                // We get the HTTP verb of the request (POST, GET, ...)
                String httpVerb = context.Request.HttpVerb.ToString();

                // We get the AbsolutePath of the request
                String absolutePath = context.Request.Url.AbsolutePath;

                // We get the body of the request
                String body = await context.GetRequestBodyAsStringAsync();

                // We use Rainbow.S2SEventPipe instance with all this info
                log.LogDebug("[OnRequestAsync] RequestedPath:[{Path}] - HttpVerb:[{HttpVerb}]", absolutePath, httpVerb);

                var sdkResult = await Rainbow.S2SEventPipe.ParseCallbackContentAsync(httpVerb, absolutePath, body);
                if (!sdkResult.Success)
                    log.LogWarning("SdkError:[{SdkError}]", sdkResult.Result);

                // /!\ Whatever the result, don't throw an exception, to avoid to unvalidate the S2S connection.
            }
        }

        public override bool IsFinalHandler { get; } = true;

        public static WebServer CreateWebServer(string url, Rainbow.Application application)
        {
            WebServer server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO)
                    )

                // First, we will configure our web server by adding Modules.
                .WithLocalSessionManager()
                .WithModule(new CallbackS2SModule("/", application));

            server.WithStaticFolder("/", "./", true, configure =>
            {

            });

            return server;
        }
    }
}
