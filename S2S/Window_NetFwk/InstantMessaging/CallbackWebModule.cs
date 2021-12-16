using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using Rainbow;
using Microsoft.Extensions.Logging;

namespace Sample_InstantMessaging
{
    public class CallbackWebModule : WebModuleBase
    {
        // Define log object - use same repository than the SDK
        private static readonly ILogger log = Rainbow.LogFactory.CreateLogger("CallbackWebModule");

        private Rainbow.Application application;
        private Rainbow.S2SEventPipe s2sEventPipe;

        public CallbackWebModule(string baseRoute, Rainbow.Application application) : base(baseRoute)
        {
            this.application = application;
            s2sEventPipe = application.GetS2SEventPipe();
        }

        protected override async Task OnRequestAsync(IHttpContext context)
        {
            Boolean result = false;

            log.LogDebug("[OnRequestAsync] RequestedPath:[{0}] - HttpVerb:[{1}]", context.Request.Url.AbsolutePath, context.Request.HttpVerb);

            if (context.Request.Headers["Content-Type"] == "application/json")
            {
                String body = await context.GetRequestBodyAsStringAsync();
                result = s2sEventPipe.ParseCallbackContent(context.Request.HttpVerb.ToString(), context.Request.Url.AbsolutePath, body);
            }

            if (!result)
                throw HttpException.NotAcceptable();
        }

        public override bool IsFinalHandler { get; } = true;
    }
}
