using Microsoft.Extensions.Logging;
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
public static class NLogConfigurator
{
    private static List<String> prefixUsed;

    static NLogConfigurator()
    {
        prefixUsed = new ();
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
        if( (prefix == null) || (prefix.Length == 0) )
            prefix = [""];

        // Check new prefix
        var newPrefix = prefix.ToList().Except(prefixUsed);
        if(newPrefix.Count() == 0)
            return true; // No new prefix added

        // Add new ones
        prefixUsed.AddRange(newPrefix);

        // Set "" always at the end
        if(prefixUsed.Contains(""))
            prefixUsed.Remove("");
        prefixUsed.Add("");

        // Check Directory
        if (String.IsNullOrEmpty(Directory))
            Directory = "./";
        else if (!Directory.EndsWith("/"))
            Directory += "/";

        // Get content of the log file configuration
        String logConfigContent = XmlContent;
        String logLoggerContent = LoggerContent;
        String logTargetContent = TargetContent;

        string loggers = "";
        string targets = "";

        foreach (String prefixItem in prefixUsed)
        {
            targets += logTargetContent.Replace("[$PREFIX]", prefixItem) + "\r\n";

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
            // Create NLog configuration using XML file content
            XmlLoggingConfiguration config = XmlLoggingConfiguration.CreateFromXmlString(logConfigContent);
            if (config.InitializeSucceeded == true)
            {
                // Create Logger factory
                var factory = LoggerFactory.Create(builder => builder.AddNLog(config));

                // Set Logger factory to Rainbow SDK
                Rainbow.LogFactory.Set(factory);

                return true;
            }
        }
        catch { }

        return false;
    }

    /// <summary>
    /// To specify where log files will be stored
    /// 
    /// Default value: "./"
    /// </summary>
    public static String Directory = "./";

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
    layout=""${longdate} | ${level:uppercase=true:padding=-5} | ${threadid:padding=-3} | ${callsite:className=True:fileName=False:includeSourcePath=False:methodName=True:padding=70:fixedLength=True:alignmentOnTruncation=Right}: ${callsite-linenumber:padding=-4} | ${message}""
    />

<target
    xsi:type=""File""
    encoding=""utf-8""
    name=""[$PREFIX]RAINBOW_DEFAULT_TARGET""
    fileName=""[$DIRECTORY][$PREFIX]RainbowSdk.log""
    archiveFileName=""[$DIRECTORY][$PREFIX]RainbowSdk_{###}.log""
    archiveAboveSize=""5242880""
    maxArchiveFiles=""10""
    layout=""${longdate} | ${level:uppercase=true:padding=-5} | ${threadid:padding=-3} | ${callsite:className=True:fileName=False:includeSourcePath=False:methodName=True:padding=70:fixedLength=True:alignmentOnTruncation=Right}: ${callsite-linenumber:padding=-4} | ${message}""
    />";
}
