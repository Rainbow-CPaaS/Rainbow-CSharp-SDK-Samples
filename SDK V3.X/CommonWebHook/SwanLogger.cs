using EmbedIO;
using Microsoft.Extensions.Logging;
using Swan.Logging;

namespace Rainbow.Example.CommonWebHook
{
    public class SwanLogger : Swan.Logging.ILogger
    {
        public readonly Microsoft.Extensions.Logging.ILogger log;
        
        public Swan.Logging.LogLevel LogLevel { get; set; } = Swan.Logging.LogLevel.Debug;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void Log(LogMessageReceivedEventArgs logEvent)
        {
            if(logEvent.MessageType >= this.LogLevel)
                log.LogDebug(logEvent.Message);
        }

        public SwanLogger() : this(null)
        {
        }

        public SwanLogger(String? loggerPrefix = null)
        {
            log = Rainbow.LogFactory.CreateLogger<SwanLogger>(loggerPrefix);
        }
    }
}