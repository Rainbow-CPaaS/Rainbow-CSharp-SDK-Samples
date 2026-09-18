using EmbedIO;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.Example.CommonWebHook
{
    public class CallbackWebHookModule : WebModuleBase
    {
        // Define log object - use same repository than the SDK
        private readonly Microsoft.Extensions.Logging.ILogger log;

        private readonly Rainbow.WebHookEventPipe webHookEventPipe;

        public CallbackWebHookModule(string baseRoute, Rainbow.Application application) : base(baseRoute)
        {
            // Create logger
            log = Rainbow.LogFactory.CreateLogger<CallbackWebHookModule>(application.LoggerPrefix);

            webHookEventPipe = application.GetWebHookEventPipe();
        }

        protected override async Task OnRequestAsync(IHttpContext context)
        {
            if (context?.Request is null) return;

            Boolean result = false;

            // We need the content-type of the body
            String? contentType = context.Request.Headers["Content-Type"];
            if (contentType == "application/json")
            {
                // We get the body of the request
                String body = await context.GetRequestBodyAsStringAsync();

                // We get the HTTP verb of the request (POST, GET, ...)
                String httpVerb = context.Request.HttpVerb.ToString();

                // We get the AbsolutePath of the request
                String absolutePath = context.Request.Url.AbsolutePath;

                log.LogDebug("[OnRequestAsync] RequestedPath:[{Path}] - HttpVerb:[{HttpVerb}] - Body:[{Body}]", absolutePath, httpVerb, body);
                result = await webHookEventPipe.ParseCallbackContentAsync(httpVerb, body);

                // /!\ Whatever the result, don't throw an exception, to avoid to unvalidate the web hook.
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
                .WithModule(new CallbackWebHookModule("/", application));

            server.WithStaticFolder("/", "./", true, configure =>
            {

            });

            return server;
        }
    }
}
