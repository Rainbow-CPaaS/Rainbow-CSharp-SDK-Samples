using Microsoft.Extensions.Logging.Abstractions;
using NLog.Config;
using NLog.Extensions.Logging;

/// <summary>
/// To add loggers at any moment.
/// 
/// Useful when you manage several instance of Rainbow Application.
/// 
/// By default :
/// - loggers are created in the current folder "./" - see <see cref="NLogConfigurator.Directory"/>
/// - logs are stored in UTF8 files ix maximum 5242880 octets with up to 10 archives files - see <see cref="NLogConfigurator.TargetContent"/>
/// - logs are splitted in two files - once dedicated to WebRTC and the other one for all other things  - see <see cref="NLogConfigurator.LoggerContent"/>
/// </summary>
namespace Rainbow.Example.Common
{
    public static class NLogConfigurator
    {
        private static List<String> prefixUsed;
           
        static NLogConfigurator()
        {
            prefixUsed = [];
        }

        public static Boolean DeleteLogDirectory()
        {
            try
            {
                System.IO.Directory.Delete(Directory, true);
                return true;
            } catch { return false; }
        }

        /// <summary>
        /// To add one or several loggers at any time.
        /// 
        /// It you need to create new Rainbow Application(s), call first this method with correct prefix
        /// </summary>
        /// <param name="prefix"><see cref="String"/>prefix(s) to manage as new logger</param>
        /// <returns><see cref="Boolean"/> - True on success</returns>
        public static Boolean AddLogger(params String[] prefix)
        {
            lock(prefixUsed)
            {
                if ((prefix == null) || (prefix.Length == 0))
                    prefix = [""];

                // Check new prefix
                var newPrefix = prefix.ToList().Except(prefixUsed).ToList();
                if (!newPrefix.Any())
                    return true; // No new prefix added

                // Store new prefix
                prefixUsed.AddRange(newPrefix);

                // Ensure to have "" as prefix in the end
                newPrefix.Remove("");
                newPrefix.Add("");

                // Check Directory
                if (String.IsNullOrEmpty(Directory))
                    Directory = "." + Path.DirectorySeparatorChar;
                else if (!Directory.EndsWith(Path.DirectorySeparatorChar+""))
                    Directory += Path.DirectorySeparatorChar;

                // Get content of the log file configuration
                String logConfigContent = XmlContent;
                String logLoggerContent = LoggerContent;
                String logTargetContent = OutputOnConsole ? TargetContentOnConsole : TargetContent;
                
                string loggers = "";
                string targets = "";

                foreach (String prefixItem in newPrefix)
                {
                    String layout;
                    if (OutputOnConsole)
                        layout = Layout.Replace("[$PREFIX]", "| " + prefixItem);
                    else
                        layout = Layout.Replace("[$PREFIX]", "");

                    targets += logTargetContent.Replace("[$PREFIX]", prefixItem).Replace("[$LAYOUT]", layout) + "\r\n";

                    if (prefixItem == "")
                    {
                        loggers += logLoggerContent.Replace("[$PREFIX]", prefixItem)
                                        .Replace("final=\"true\"", "") + "\r\n";
                    }
                    else
                        loggers += logLoggerContent.Replace("[$PREFIX]", prefixItem) + "\r\n";
                }

                logConfigContent = logConfigContent.Replace("<!--RULES-->", loggers)
                                            .Replace("<!--TARGETS-->", targets)
                                            .Replace("[$DIRECTORY]", Directory);

                try
                {
                    // Set Logger factory to Rainbow SDK only once
                    if (Rainbow.LogFactory.Get() == NullLoggerFactory.Instance)
                    {
                        var factory = new NLogLoggerFactory();
                    
                        Rainbow.LogFactory.Set(factory);
                    }

                    //Util.WriteBlue($"Log settings used:\r\n{logConfigContent}");

                    // Create NLog configuration using XML file content
                    XmlLoggingConfiguration config = XmlLoggingConfiguration.CreateFromXmlString(logConfigContent);

                    NLog.LogManager.Configuration ??= new();

                    // Need to remove previous rules "*" and "WEBRTC" (if any)
                    NLog.LogManager.Configuration.RemoveRuleByName("*");
                    NLog.LogManager.Configuration.RemoveRuleByName("WEBRTC");

                    foreach (var rule in config.LoggingRules)
                    {
                        rule.RuleName = rule.LoggerNamePattern;
                        NLog.LogManager.Configuration.AddRule(rule);
                    }
                    
                    NLog.LogManager.ReconfigExistingLoggers();

                    return true;
                }
                catch (Exception ex) 
                { 
                    Console.WriteLine("NLogConfigurator.AddLogger failed: [{exception}]", ex);
                }

                return false;
            }
        }

        /// <summary>
        /// To specify where log files will be stored
        /// 
        /// Default value: "." + Path.DirectorySeparatorChar;
        /// </summary>
        public static String Directory = "." + Path.DirectorySeparatorChar;

        public static Boolean OutputOnConsole = false;

        /// <summary>
        /// Main structure of the XML configuration file for NLog
        /// </summary>
        public static String XmlContent =
    @"<?xml version=""1.0"" encoding=""utf-8""?>
<nlog xmlns=""http://www.nlog-project.org/schemas/NLog.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<rules>
<!--RULES-->
	</rules>

	<targets>
<!--TARGETS-->
	</targets>
</nlog>";

        /// <summary>
        /// By default define 2 loggers: once for WebRTC and another for all others things
        /// </summary>
        public static String LoggerContent =
    @"<logger
    name=""[$PREFIX]WEBRTC""
    minlevel=""Debug""
    writeTo=""[$PREFIX]RAINBOW_WEBRTC_TARGET""
    final=""true"" />
<logger
    name=""[$PREFIX]*""
    minlevel=""Debug""
    writeTo=""[$PREFIX]RAINBOW_DEFAULT_TARGET""
    final=""true"" />";

        /// <summary>
        /// By default define 2 targets (one for each logger - see <see cref="LoggerContent"/>)
        /// Each target, define a file with 5242880 octets maximum and up to 10 archived files
        /// </summary>
        public static String TargetContent =
@"<target
    xsi:type=""File""
    encoding=""utf-8""
    name=""[$PREFIX]RAINBOW_WEBRTC_TARGET""
    fileName=""[$DIRECTORY][$PREFIX]RainbowSdk_WebRTC.log""
    archiveFileName=""[$DIRECTORY][$PREFIX]RainbowSdk_WebRTC_{###}.log""
    archiveAboveSize=""5242880""
    maxArchiveFiles=""10""
    [$LAYOUT]
    />

<target
    xsi:type=""File""
    encoding=""utf-8""
    name=""[$PREFIX]RAINBOW_DEFAULT_TARGET""
    fileName=""[$DIRECTORY][$PREFIX]RainbowSdk.log""
    archiveFileName=""[$DIRECTORY][$PREFIX]RainbowSdk_{###}.log""
    archiveAboveSize=""5242880""
    maxArchiveFiles=""10""
    [$LAYOUT]
    />";

        /// <summary>
        /// By default define 2 targets (one for each logger - see <see cref="LoggerContent"/>)
        /// Each target, define a file with 5242880 octets maximum and up to 10 archived files
        /// </summary>
        public static String TargetContentOnConsole =
@"<target
    xsi:type=""Console""
    encoding=""utf-8""
    name=""[$PREFIX]RAINBOW_WEBRTC_TARGET""
    [$LAYOUT]
    />

<target
    xsi:type=""Console""
    encoding=""utf-8""
    name=""[$PREFIX]RAINBOW_DEFAULT_TARGET""
    [$LAYOUT]
    />";

        public static String Layout = "layout=\"${longdate} [$PREFIX]| ${level:format=FirstCharacter} | ${threadid:padding=-3} | ${callsite:className=True:includeNamespace=False:fileName=False:includeSourcePath=False:methodName=True:padding=40:fixedLength=True:alignmentOnTruncation=Right} | ${message}\"";
    }
}