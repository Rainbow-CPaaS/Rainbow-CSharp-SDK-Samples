using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using System.Diagnostics;

namespace Rainbow.Example.Common
{
    public class LogConfigurator
    {
        private static Object _lock = new();

        private static Boolean _configSet = false;

        public static String Directory = "." + Path.DirectorySeparatorChar;

        public static Boolean OutputOnConsole = false;

#region NLOG code part

        public static String LoggerPrefix = "${scopeproperty:LoggerPrefix:whenEmpty=default}";

        public static String Layout = "layout=\"${longdate} [$LOGGERPREFIX]| ${level:format=FirstCharacter} | ${threadid:padding=-3} | ${callsite:className=True:includeNamespace=False:fileName=False:includeSourcePath=False:methodName=True:padding=40:fixedLength=True:alignmentOnTruncation=Right} | ${message}\"";

        public static String XmlContent =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<nlog xmlns=""http://www.nlog-project.org/schemas/NLog.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
<targets>
  <target xsi:type=""AsyncWrapper""
          name=""asyncFile""
          queueLimit=""50000""
          overflowAction=""Block""
          batchSize=""200""
          timeToSleepBetweenBatches=""1"">

    <target xsi:type=""File""
            encoding=""utf-8""
            name=""botFile""
            fileName=""[$DIRECTORY][$LOGGERPREFIX]_RainbowSdk.log""
            [$LAYOUT]

            keepFileOpen=""true""
            openFileCacheSize=""600""
            autoFlush=""true""
            concurrentWrites=""false""

            archiveFileName=""[$DIRECTORY][$LOGGERPREFIX]_RainbowSdk_{###}.log""
            archiveAboveSize=""5242880""
            maxArchiveFiles=""10""
            archiveNumbering=""Rolling"" />
  </target>
  
    <target
	    xsi:type=""ColoredConsole""
	    encoding=""utf-8""
	    name=""console""
	    [$LAYOUT]>

        <!-- one ligne = one LoggerPrefix value-->
        <highlight-row condition=""equals('${scopeproperty:LoggerPrefix}','USER1')"" foregroundColor=""Yellow"" />
        <highlight-row condition=""equals('${scopeproperty:LoggerPrefix}','USER2')"" foregroundColor=""Green"" />
    </target>
</targets>

<rules>
  <logger name=""*"" minlevel=""Trace"" writeTo=""[$TARGET]"">
  </logger>
</rules>
</nlog>";

        /// <summary>
        /// To configure NLog: if configuration has been set correctly once, it can't be changed.
        /// </summary>
        /// <param name="directory">Relative or absolute directory path where log files will be stored (useful only if log output are not on the console)</param>
        /// <param name="useConsole">True to output log entries on the console - False by default</param>
        /// <param name="deleteDirectory">True to delete first the directory specified - False by default</param>
        /// <returns></returns>
        public static Boolean Configure(String? directory = null, Boolean useConsole = false, Boolean deleteDirectory = false)
        {
            lock (_lock)
            {
                if (_configSet)
                {
                    ConsoleAbstraction.WriteDarkYellow($"Configuration has been already done");
                    return true;
                }
            
                if (directory is not null)
                {
                    if (!directory.EndsWith(Path.DirectorySeparatorChar))
                        directory += Path.DirectorySeparatorChar;

                    Directory = directory;
                }

                try
                {
                    if (deleteDirectory)
                        System.IO.Directory.Delete(Directory, true);
                } 
                catch { }


                OutputOnConsole = useConsole;

                var _layout = Layout;
                var _xmlContent = XmlContent;

                if (OutputOnConsole)
                {
                    _xmlContent = _xmlContent.Replace("[$TARGET]", "console");
                }
                else
                {
                    _layout = _layout.Replace("[$LOGGERPREFIX]", "");
                    _xmlContent = _xmlContent.Replace("[$DIRECTORY]", Directory);
                    _xmlContent = _xmlContent.Replace("[$TARGET]", "asyncFile");
                }

                _xmlContent = _xmlContent.Replace("[$LAYOUT]", _layout);
                _xmlContent = _xmlContent.Replace("[$LOGGERPREFIX]", LoggerPrefix);

                try
                {
                    // Set Logger factory to Rainbow SDK only once
                    if (Rainbow.LogFactory.Get() == NullLoggerFactory.Instance)
                    {
                        var factory = new NLog.Extensions.Logging.NLogLoggerFactory();
                        Rainbow.LogFactory.Set(factory);
                    }
                    // Create NLog configuration using XML file content
                    NLog.Config.XmlLoggingConfiguration config = NLog.Config.XmlLoggingConfiguration.CreateFromXmlString(_xmlContent);

                    NLog.LogManager.Configuration ??= new();

                    foreach (var rule in config.LoggingRules)
                    {
                        rule.RuleName = rule.LoggerNamePattern;
                        NLog.LogManager.Configuration.AddRule(rule);

                    }

                    NLog.LogManager.ReconfigExistingLoggers(); // => necessary

                    _configSet = true;
                    ConsoleAbstraction.WriteBlue($"Log settings used:\r\n{_xmlContent}");
                }
                catch (Exception ex)
                {
                    ConsoleAbstraction.WriteLine("NLogConfigurator.AddLogger failed: [{exception}]", args: ex);
                }

                return _configSet;
            }
        }
#endregion NLOG code part


#region SERILOG code part

        public static string SerilogOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.ffff} [$LOGGERPREFIX]| {Level:u1} | {ThreadId,-3} | {Caller,40} | {Message:l}{NewLine}{Exception}";

        public static String SerilogLoggerPrefix = "{LoggerPrefix,-8} |";

        public static Boolean ConfigureUsingSerilog(String? directory = null, Boolean useConsole = false, Boolean deleteDirectory = false)
        {
            lock (_lock)
            {
                if (_configSet)
                {
                    ConsoleAbstraction.WriteDarkYellow($"Configuration has been already done");
                    return true;
                }

                if (directory is not null)
                {
                    if (!directory.EndsWith(Path.DirectorySeparatorChar))
                        directory += Path.DirectorySeparatorChar;

                    Directory = directory;
                }

                try
                {
                    if (deleteDirectory)
                        System.IO.Directory.Delete(Directory, true);
                }
                catch { }


                OutputOnConsole = useConsole;

                var _serilogOutputTemplate = SerilogOutputTemplate;
                if (OutputOnConsole)
                    _serilogOutputTemplate = _serilogOutputTemplate.Replace("[$LOGGERPREFIX]", SerilogLoggerPrefix);
                else
                    _serilogOutputTemplate = _serilogOutputTemplate.Replace("[$LOGGERPREFIX]", "");

                try
                {
                    var loggerConfiguration = new Serilog.LoggerConfiguration()
                        .MinimumLevel.Debug() // ~ minlevel="Debug" in Log
                        .Enrich.FromLogContext()
                        .Enrich.With<SerilogCallerEnricher>();
                    loggerConfiguration = Serilog.ThreadLoggerConfigurationExtensions.WithThreadId(loggerConfiguration.Enrich);

                    if (OutputOnConsole)
                    {
                        Serilog.ConsoleLoggerConfigurationExtensions.Console(loggerConfiguration.WriteTo, outputTemplate: _serilogOutputTemplate);
                    }
                    else
                        Serilog.LoggerConfigurationAsyncExtensions.Async(loggerConfiguration.WriteTo, a => a.Map(
                                keyPropertyName: "LoggerPrefix",
                                defaultKey: "default",
                                configure: (prefix, wt) => wt.File(
                                    path: $"./logs/{prefix}_RainbowSdk_serilog.log",
                                    outputTemplate: _serilogOutputTemplate,
                                    fileSizeLimitBytes: 5_242_880,      // ~ archiveAboveSize in NLog
                                    rollOnFileSizeLimit: true,           // ~ archiveNumbering="Rolling" in NLog
                                    retainedFileCountLimit: 10,           // ~ maxArchiveFiles in NLog
                                    rollingInterval: Serilog.RollingInterval.Infinite,
                                    shared: false),
                                sinkMapCountLimit: 50),
                            bufferSize: 50000,          // ~ queueLimit in NLog
                            blockWhenFull: true);        // ~ overflowAction="Block" in NLog

                    Serilog.Log.Logger = loggerConfiguration.CreateLogger();

                    var factory = Serilog.SerilogLoggerFactoryExtensions.AddSerilog(new LoggerFactory(), Serilog.Log.Logger, dispose: true);
                    Rainbow.LogFactory.Set(factory);

                    _configSet = true;
                    ConsoleAbstraction.WriteBlue($"Log settings used using Serilog");
                }
                catch (Exception ex)
                {
                    ConsoleAbstraction.WriteLine("NLogConfigurator.AddLogger failed: [{exception}]", args: ex);
                }
                return _configSet;
            }
        }

        public class SerilogCallerEnricher : Serilog.Core.ILogEventEnricher
        {
            private static readonly HashSet<string> ExcludedTypeNames = new()
            {
                "SerilogLogger",       // Serilog.Extensions.Logging.SerilogLogger
                "ScopedLogger",        
                "Logger`1",            // Microsoft.Extensions.Logging.Logger<T>
            };

            public void Enrich(Serilog.Events.LogEvent logEvent, Serilog.Core.ILogEventPropertyFactory propertyFactory)
            {
                var stack = new StackTrace();
                for (int i = 2; i < stack.FrameCount; i++)
                {
                    var method = stack.GetFrame(i)?.GetMethod();
                    var declaringType = method?.DeclaringType;
                    if (declaringType is null) continue;

                    if (declaringType.Namespace?.StartsWith("Serilog") == true) continue;
                    if (declaringType.Namespace?.StartsWith("Microsoft.Extensions.Logging") == true) continue;
                    if (ExcludedTypeNames.Contains(declaringType.Name)) continue;

                    var caller = $"{declaringType.Name}.{method?.Name}";
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Caller", caller));
                    return;
                }
            }
        }
    }
#endregion SERILOG code part
}
