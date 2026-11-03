using EmbedIO;
using Microsoft.Extensions.Logging;
using Rainbow.Model;
using System.Security.Cryptography;
using System.Text;

namespace Rainbow.Example.CommonWebHook
{
    public class CallbackWebHookModule : WebModuleBase
    {
        // Define log object - use same repository than the SDK
        private readonly Microsoft.Extensions.Logging.ILogger log;

        private readonly Rainbow.WebHookEventPipe webHookEventPipe;

        private readonly CompanyEventWebHook companyEventWebHook;

        public CallbackWebHookModule(string baseRoute, Rainbow.Application application, CompanyEventWebHook companyEventWebHook) : base(baseRoute)
        {
            // Create logger
            log = Rainbow.LogFactory.CreateLogger<CallbackWebHookModule>(application.LoggerPrefix);

            webHookEventPipe = application.GetWebHookEventPipe();
            this.companyEventWebHook = companyEventWebHook;
        }

        public static string? ComputeWebHookSignature(string? token, string? subscriptionId)
        {
            if (String.IsNullOrEmpty(token) || String.IsNullOrEmpty(subscriptionId))
                return null;
            byte[] keyBytes = Encoding.UTF8.GetBytes(token);
            byte[] messageBytes = Encoding.UTF8.GetBytes(subscriptionId);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                byte[] hash = hmac.ComputeHash(messageBytes);
                return Convert.ToBase64String(hash);
            }
        }

        protected override async Task OnRequestAsync(IHttpContext context)
        {
            if (context?.Request is null) return;

            // We need the content-type of the body
            String? contentType = context.Request.Headers["Content-Type"];
            if (contentType == "application/json")
            {
                String? webHookSignature = context.Request.Headers["X-Rainbow-Wh-Signature"];
                String? subscriptionId = context.Request.Headers["X-Rainbow-SubscriptionId"];
                //String? webHookSignatureExpected = Rainbow.Util.ComputeWebHookSignature(companyEventWebHook.Token, subscriptionId);
                String? webHookSignatureExpected = ComputeWebHookSignature(companyEventWebHook.Token, subscriptionId);

                // We get the body of the request
                String body = await context.GetRequestBodyAsStringAsync();

                // We get the HTTP verb of the request (POST, GET, ...)
                String httpVerb = context.Request.HttpVerb.ToString();

                // We get the AbsolutePath of the request
                String absolutePath = context.Request.Url.AbsolutePath;

                log.LogDebug("[OnRequestAsync] RequestedPath:[{Path}] - CorrectWhSignature:[{correctWhSignature}] - HttpVerb:[{HttpVerb}] - Body:[{Body}]", absolutePath, webHookSignatureExpected?.Equals(webHookSignature) == true, httpVerb, body);
                var result = await webHookEventPipe.ParseCallbackContentAsync(httpVerb, body);

                // /!\ Whatever the result, don't throw an exception, to avoid to unvalidate the web hook.
            }
        }

        public override bool IsFinalHandler { get; } = true;

        public static WebServer CreateWebServer(string url, Rainbow.Application application, CompanyEventWebHook companyEventWebHook)
        {
            WebServer server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO)
                    )

                // First, we will configure our web server by adding Modules.
                .WithLocalSessionManager()
                .WithModule(new CallbackWebHookModule("/", application, companyEventWebHook));

            server.WithStaticFolder("/", "./", true, configure =>
            {

            });

            return server;
        }
    }
}
