using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Rainbow.Medias;
using FFmpeg.AutoGen;
using Microsoft.Extensions.Logging;
using Rainbow.SimpleJSON;
using Rainbow;

namespace BotVideoCompositor
{
    /// <summary>
    /// Static class with some utility methods
    /// </summary>
    public class Util
    {
        internal static readonly String NamespaceResources = "BotVideoCompositor.Resources";
        internal static JSONNode? _basicAdaptiveCardsData = null;

        private static Boolean resourcesInit = false;
        private static List<String>? resources = null;

        private static Object consoleLockObject = new Object();

        private static ILogger log = null;

        static public void SetLogger()
        {
            log = Rainbow.LogFactory.CreateLogger<Util>();
        }

        // To write in RED to console
        static public void WriteRedToConsole(String message)
        {
            WriteToConsole(message, foregroundColor: ConsoleColor.Red);
            log?.LogInformation(message);
        }

        // To write ni GREEN to console
        static public void WriteGreenToConsole(String message)
        {
            WriteToConsole(message, foregroundColor: ConsoleColor.Green);
            log?.LogInformation(message);
        }

        // To write in BLUE to console
        static public void WriteBlueToConsole(String message)
        {
            WriteToConsole(message, foregroundColor: ConsoleColor.Blue);
            log?.LogInformation(message);
        }

        // To write in WHITE to console
        static public void WriteWhiteToConsole(String message)
        {
            WriteToConsole(message, foregroundColor: ConsoleColor.White);
            log?.LogInformation(message);
        }

        /// <summary>
        /// Get Stream from the specified file path
        /// </summary>
        /// <param name="pathFile"><see cref="String"/>File path</param>
        /// <returns><see cref="Stream"/> - Valid Stream (read only) object if file exists or NULL</returns>
        static public Stream? GetStreamFromFile(String pathFile)
        {
            if (File.Exists(pathFile))
                return File.OpenRead(pathFile);
            return null;
        }

        static public String GetJsonStringFromItemsList(List<Item>? items, Boolean indented = false)
        {
            if (items == null)
                return "null";

            JSONArray jsonArray = new JSONArray();
            foreach (var item in items)
            {
                var jsonNode = new JSONObject();

                UtilJson.AddNode(jsonNode, "title", item.title);
                UtilJson.AddNode(jsonNode, "value", item.value);

                jsonArray.Add(jsonNode);
            }

            return jsonArray.ToString();
        }

        static public int? GetHeightAccordingRatio(int width, Size ratio)
        {
            if ((width % ratio.Width) == 0)
                return width * ratio.Height / ratio.Width;
            return null;
        }

        private static av_log_set_callback_callback? _logCallback;

        static public unsafe JSONNode? InitVideoStreams()
        {
            List<Item> items = new List<Item>();

            

            //_logCallback = (p0, level, format, vl) =>
            //{
            //    return;
            //};
            //ffmpeg.av_log_set_callback(_logCallback);


            RainbowApplicationInfo.BroadcastConfiguration = new BroadcastConfiguration();

            items.Clear();
            List<String> streamsName = new List<string>();
            var jsonLabels = JSON.Parse(RainbowApplicationInfo.labels);
            if (jsonLabels != null)
            {
                if (jsonLabels["streamsName"]?.IsArray == true)
                {
                    var list = UtilJson.AsStringList(jsonLabels, "streamsName");
                    foreach (var stream in list)
                        streamsName.Add(stream.ToString());
                }
                else
                    return null;

                int index = 0;
                MediaInput mediaInput;
                int maxLength = 0;
                foreach (var video in RainbowApplicationInfo.videos)
                {
                    string val;
                    string key;
                    string info;
                    int fps;

                    key = video.Uri;
                    if (streamsName.Count > index)
                        val = streamsName[index];
                    else
                        val = $"Stream {index + 1}";

                    mediaInput = new Rainbow.Medias.MediaInput(new InputStreamDevice(index.ToString(), val, key, withVideo: true, withAudio: false, loop: true, options: video.Settings), loggerPrefix: "");
                    mediaInput.SetVideoBitRate(1024 * 1000);

                    Util.WriteBlueToConsole($"\r\nStart initialization of [{val}] using [{key}].");
                    // Init stream and start it if possible
                    if (mediaInput.Init(true))
                    {
                        // We stop the stream if it's not a live stream
                        if (!mediaInput.LiveStream)
                            mediaInput.Stop();

                        fps = (int)Math.Round(mediaInput.Fps);

                        Util.WriteGreenToConsole($"Init done: [{val} - {mediaInput.Width}x{mediaInput.Height} - {fps}fps] - LiveStream:{mediaInput.LiveStream}]");

                        info = $"[{mediaInput.Width}x{mediaInput.Height} - {fps}fps]";
                        items.Add(new Item(info, val));
                        RainbowApplicationInfo.BroadcastConfiguration.MediaInputCollections.Add(index.ToString(), mediaInput);

                        index++;

                        // Need to get max string length for display purpose
                        if (val.Length > maxLength)
                            maxLength = val.Length;
                    }
                    else
                    {
                        Util.WriteRedToConsole($"Cannot initialized [{val}] using [{key}].");
                    }
                }

                List<Item> itemsForDisplay = new List<Item>();
                index = 0;
                foreach (var item in items)
                {
                    string? title = item.title;
                    string? value = item.value;

                    value = Util.AddString(value, "&nbsp;", maxLength);
                    //value = "<p style=\"font-family: Courier\"><b>" + value + "</b>&nbsp;" + title;
                    value = "<b>" + value + "</b>&nbsp;" + title;

                    title = value;
                    value = index.ToString();
                    ;
                    itemsForDisplay.Add(new Item(title, value));
                    index++;
                }

                var str = Util.GetJsonStringFromItemsList(itemsForDisplay, true);
                var json = JSON.Parse(str);
                return json;

                //// Add fps data with labels
                //if (temp is JToken jtokenStreams)
                //    jsonLabels.Add("streamsCollection", jtokenStreams);
                //else
                //    return null;
            }
            return null;
        }

        static public JSONNode? CreateBasicAdaptiveCardsData()
        {
            if (_basicAdaptiveCardsData != null)
                return _basicAdaptiveCardsData;

            if ((RainbowApplicationInfo.labels == null)
                || (RainbowApplicationInfo.fps == null)
                || (RainbowApplicationInfo.outputs == null)
                || (RainbowApplicationInfo.vignettes == null)
                || (RainbowApplicationInfo.videos == null))
                return null;

            try
            {
                List<Item> items = new List<Item>();
                String str;

                var jsonLabels = JSON.Parse(RainbowApplicationInfo.labels);
                if (jsonLabels != null)
                {
                    // ---------------------------
                    // -- Create data about overlayDisplay
                    if (jsonLabels["overlayDisplay"]?.IsArray == true)
                    {
                        var list = jsonLabels["overlayDisplay"];
                        RainbowApplicationInfo.overlayDisplay = new Dictionary<string, string>();

                        foreach (var item in list)
                        {
                            String? title = UtilJson.AsString(item, "title");
                            String? value = UtilJson.AsString(item, "value");
                            if ((!String.IsNullOrEmpty(title)) && (!String.IsNullOrEmpty(value)))
                                RainbowApplicationInfo.overlayDisplay.Add(title, value);
                        }
                        if (RainbowApplicationInfo.overlayDisplay.Count == 0)
                            return null;
                    }
                    else
                        return null;

                    // ---------------------------
                    // -- Create data about mosaicDisplay
                    if (jsonLabels["mosaicDisplay"]?.IsArray == true)
                    {
                        var list = jsonLabels["mosaicDisplay"];
                        RainbowApplicationInfo.mosaicDisplay = new Dictionary<string, string>();

                        foreach (var item in list)
                        {
                            String? title = UtilJson.AsString(item, "title");
                            String? value = UtilJson.AsString(item, "value");
                            if ((!String.IsNullOrEmpty(title)) && (!String.IsNullOrEmpty(value)))
                                RainbowApplicationInfo.mosaicDisplay.Add(title, value);
                        }
                        if (RainbowApplicationInfo.mosaicDisplay.Count == 0)
                            return null;

                    }
                    else
                        return null;


                    // ---------------------------
                    // -- Create data about fps
                    items.Clear();
                    foreach (var fps in RainbowApplicationInfo.fps)
                        items.Add(new Item(fps, fps));

                    str = Util.GetJsonStringFromItemsList(items, true);
                    var json = JSON.Parse(str);
                    // Add fps data with labels
                    if (json != null)
                        jsonLabels.Add("fpsCollection", json);
                    else
                        return null;


                    // ---------------------------
                    // -- Create data about outputs size
                    items.Clear();
                    foreach (var output in RainbowApplicationInfo.outputs)
                        items.Add(new Item(output.Value, output.Key));

                    str = Util.GetJsonStringFromItemsList(items, true);
                    json = JSON.Parse(str);

                    // Add fps data with labels
                    if (json != null)
                        jsonLabels.Add("sizeCollection", json);
                    else
                        return null;


                    // ---------------------------
                    // -- Create data about vignettes size
                    items.Clear();
                    foreach (var output in RainbowApplicationInfo.vignettes)
                        items.Add(new Item(output.Value, output.Key));

                    str = Util.GetJsonStringFromItemsList(items, true);
                    json = JSON.Parse(str);

                    // Add fps data with labels
                    if (json != null)
                        jsonLabels.Add("vignetteSizeSelection", json);
                    else
                        return null;

                    // ---------------------------
                    // -- Create data about streams
                    var jtokenStreams = InitVideoStreams();
                    if (jtokenStreams == null)
                        return null;
                    jsonLabels.Add("streamsCollection", jtokenStreams);

                    // Remove unnecessary properties
                    jsonLabels.Remove("streamsName");

                    _basicAdaptiveCardsData = jsonLabels;
                    RainbowApplicationInfo.BroadcastConfiguration.Init();

                    return _basicAdaptiveCardsData;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        static public String? AddString(String? src, String strToadd, int maxLength)
        {
            if (src == null)
                return null;

            if (String.IsNullOrEmpty(strToadd))
                return src;

            if (src.Length < maxLength)
            {
                int nbSteps = maxLength - src.Length;
                for (int i = 0; i < nbSteps; i++)
                    src += strToadd;
            }
            return src;
        }

        static public String GetLayoutText(Boolean overlayContext, String value)
        {
            Dictionary<String, String> dico;
            if (overlayContext)
                dico = RainbowApplicationInfo.overlayDisplay;
            else
                dico = RainbowApplicationInfo.mosaicDisplay;

            var item = dico.First(item => item.Value == value);
            return item.Key;

        }

#region Methods to compute Ration Prime factors

        static public Size? GetMediaInputSize(String id)
        {
            var mediaInput = GetMediaInput(id);
            if (mediaInput != null)
                return new Size(mediaInput.Width, mediaInput.Height);
            return null;
        }

        static public MediaInput? GetMediaInput(String id)
        {
            if (RainbowApplicationInfo.BroadcastConfiguration.MediaInputCollections.ContainsKey(id))
                return RainbowApplicationInfo.BroadcastConfiguration.MediaInputCollections[id];
            return null;
        }

        static public String? FilterToMosaic(List<Size> srcVideoSize, Size vignette, string layout, int fps = 10)
        {
            List<String> filtersScaledList = new List<string>();
            int index = 0;
            Boolean isSimpleCase = (layout == "H") || (layout == "V") || (layout == "G");

            foreach (var ratio in srcVideoSize)
            {
                if ( (index == 0) && (!isSimpleCase))
                    filtersScaledList.Add(FilterToScaleSize(ratio, new Size(vignette.Width * 2, vignette.Height * 2), fps) ); 
                else
                    filtersScaledList.Add(FilterToScaleSize(ratio, vignette, fps));

                index++;
            }

            string result = "";
            string subpart = "";
            index = 0;
            foreach (var filerScaled in filtersScaledList)
            {
                result += $"[{index}]setpts=PTS-STARTPTS,{filerScaled}[g{index}];\r\n";

                subpart += $"[g{index}]";
                index++;
            }
            subpart += $"xstack=inputs={index}:";

            switch (layout)
            {
                case "H":
                    subpart += $"grid={index}x1";
                    break;
                case "V":
                    subpart += $"grid=1x{index}";
                    break;
                case "G":
                    var sqrt = (int)Math.Ceiling(Math.Sqrt(srcVideoSize.Count));

                    if (srcVideoSize.Count == sqrt * sqrt)
                    {
                        subpart += $"grid={sqrt}x{sqrt}";
                    }
                    else
                    {
                        subpart += $"layout=";
                        // Example: layout=0_0|0_h0|0_h0+h1|w0_0|w0_h0|w0_h0+h1|w0+w3_0|w0+w3_h0|w0+w3_h0+h1 => 3x3 grid

                        int nb = 0;
                        string h = "";
                        string v = "";

                        for (int v_index = 0; v_index < sqrt; v_index++)
                        {
                            if (v_index == 0)
                                v = "0";
                            else if (v_index == 1)
                                v = "h0";
                            else
                                v += $"+h{v_index - 1}";

                            for (int h_index = 0; h_index < sqrt; h_index++)
                            {
                                if (h_index == 0)
                                    h = "0";
                                else if (h_index == 1)
                                    h = "w0";
                                else
                                    h += $"+w{h_index - 1}";

                                subpart += $"{h}_{v}|";

                                nb++;
                                if (nb >= index)
                                    break;
                            }

                            if (nb >= index)
                                break;
                        }


                        subpart = subpart.Substring(0, subpart.Length - 1);
                        subpart += ":fill=black";
                    }
                    break;
                case "H-M+2v":
                    if (index != 3)
                        return null;
                    subpart += $"layout=0_0|w0_0|w0_h1";
                    break;

                case "V-M+2v":
                    if (index != 3)
                        return null;
                    subpart += $"layout=0_0|0_h0|w1_h0";
                    break;

                case "W":
                    if (index != 6)
                        return null;
                    subpart += $"layout=0_0|w0_0|w0_h1|0_h0|w1_h0|w1+w2_h1+h2";
                    break;
            }

            result += subpart;
            result += "[grid];\r\n";
            result += $"[grid]fps={fps}";

            return result;
        }

        static public String FilterToOverlay(Size srcMain, Size srcOverlay, Size dst, Size dstOverlay, string dstOverlayPosition, int fps = 10)
        {
            string result = "";

            var mainScaleFilter = FilterToScaleSize(srcMain, dst, fps);
            var overlayScaleFilter = FilterToScaleSize(srcOverlay, dstOverlay, fps);

            string overlayFilter;
            switch(dstOverlayPosition)
            {
                case "TR":
                default:
                    overlayFilter = "overlay=x=W-w:y=0";
                    break;

                case "TL":
                    overlayFilter = "overlay=x=0:y=0";
                    break;

                case "BL":
                    overlayFilter = "overlay=x=0:y=H-h";
                    break;

                case "BR":
                    overlayFilter = "overlay=x=W-w:y=H-h";
                    break;
            }

            result =  $"[0]setpts=PTS-STARTPTS,{mainScaleFilter}[o0];\r\n";
            result += $"[1]setpts=PTS-STARTPTS,{overlayScaleFilter}[o1];\r\n";
            result += $"[o0][o1]{overlayFilter}[overlay];\r\n";
            result += $"[overlay]fps={fps}";

            return result;
        }

        static public String FilterToScaleSize(Size srcSize, Size dstSize, int? fps = null)
        {
            string result;

            // Get ratio information
            var ratio_src = Ratio(srcSize);
            var ratio_dst = Ratio(dstSize);

            if ((ratio_src.Width == ratio_dst.Width) && (ratio_src.Height == ratio_dst.Height))
            {
                result = $"scale={dstSize.Width}:-2";
            }
            else
            {
                var scale_dst = ScaleSize(ratio_src, dstSize);
                result = $"scale={scale_dst.Width}:{scale_dst.Height},pad={dstSize.Width}:{dstSize.Height}:(ow-iw)/2:(oh-ih)/2";
            }

            if (fps != null)
                result += $",fps={fps}";

            return result;
        }

        static public Size ScaleSize(Size srcSize, Size dstSize)
        {
            // Get ratio information
            var r = Ratio(srcSize);

            // Check on Width first
            var factor = DivisionResultAsMultipleOf4(dstSize.Width, r.Width);
            if(factor * r.Height > dstSize.Height)
                factor = DivisionResultAsMultipleOf4(dstSize.Height, r.Height);

            return new Size(factor * r.Width, factor * r.Height);
        }

        static public int DivisionResultAsMultipleOf4(int dividend, int divisor)
        {
            var result = dividend / divisor;
            if (result % 2 != 0)
                result -= 1;

            while (result % 4 != 0)
                result -= 2;
            return result;
        }

        static public Size Ratio(Size size)
        {
            var primeFactors_w = PrimeFactors(size.Width);
            var primeFactors_h = PrimeFactors(size.Height);

            var w_prime = primeFactors_w.Keys.ToList();
            var h_prime = primeFactors_h.Keys.ToList();
            var primes = w_prime.Union(h_prime).ToList(); // Without duplicate get all prime factors used in width and height

            foreach(int prime in primes)
            {
                if(primeFactors_w.ContainsKey(prime) && primeFactors_h.ContainsKey(prime))
                {
                    if (primeFactors_w[prime] == primeFactors_h[prime])
                    {
                        primeFactors_w.Remove(prime);
                        primeFactors_h.Remove(prime);
                    }
                    else if (primeFactors_w[prime] > primeFactors_h[prime])
                    {
                        primeFactors_w[prime] -= primeFactors_h[prime];
                        primeFactors_h.Remove(prime);
                    }
                    else
                    {
                        primeFactors_h[prime] -= primeFactors_w[prime];
                        
                        primeFactors_w.Remove(prime);
                    }
                }
            }

            int rw = PrimeFactorsResult(primeFactors_w);
            int rh = PrimeFactorsResult(primeFactors_h);

            return new Size(rw, rh);
        }

        static public int PrimeFactorsResult(Dictionary<int, int> primeFactors)
        {
            int result = 1;

            foreach(var primeItem in primeFactors)
            {
                var prime = primeItem.Key;
                var nb = primeItem.Value;
                for (int i = 0; i < nb; i++)
                    result *= prime;
            }

            return result;
        }

        static public Dictionary<int, int> PrimeFactors(int n)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            int nb;
            
            // Prints all the numbers of 2  
            while (n % 2 == 0)
            {
                if (result.ContainsKey(2))
                    nb = result[2] + 1;
                else
                    nb = 1;
                result[2] = nb;
                n /= 2;
            }

            // As no 2 can be further divided, this probably means that n
            // is now an odd number
            for (int i = 3; i <= Math.Sqrt(n); i += 2)
            {
                while (n % i == 0)
                {
                    if (result.ContainsKey(i))
                        nb = result[i] + 1;
                    else
                        nb = 1;
                    result[i] = nb;
                    n /= i;
                }
            }

            // This is for case if n is greater than 2
            if (n > 2)
            {
                result[n] = 1;
            }
            return result;
        }

#endregion Methods to compute Ration Prime factors

        static public String GetDefaultLayoutValue(Boolean overlayContext)
        {
            Dictionary<String, String> dico;
            if (overlayContext)
                dico = RainbowApplicationInfo.overlayDisplay;
            else
                dico = RainbowApplicationInfo.mosaicDisplay;

            var item = dico.First();
            return item.Value;

        }

        static public String GetLayoutValue(Boolean overlayContext, String text)
        {
            Dictionary<String, String> dico;
            if (overlayContext)
                dico = RainbowApplicationInfo.overlayDisplay;
            else
                dico = RainbowApplicationInfo.mosaicDisplay;

            var item = dico.First(item => item.Value == text);
            return item.Key;

        }

        // Get the list of all resources embedded in this package based on the folder path provided (if any)
        static internal List<String>? GetEmbeddedResourcesList(String folderPath = "", String? extension = null)
        {
            InitResourcesList();

            if (String.IsNullOrEmpty(folderPath))
                return resources;
            else
            {
                List<String> result = new List<string>();

                folderPath = folderPath.Replace("/", ".");

                if (folderPath.StartsWith("."))
                    folderPath = folderPath.Substring(1);

                if (folderPath.EndsWith("."))
                    folderPath = folderPath.Substring(0, folderPath.Length - 1);

                String path = Util.NamespaceResources + "." + folderPath + ".";

                if (resources != null)
                {
                    // Loop to find files stored in this path
                    foreach (String resource in resources)
                    {
                        if (resource.StartsWith(path))
                        {
                            if (String.IsNullOrEmpty(extension))
                                result.Add(resource.Replace(path, ""));
                            else
                            {
                                if (resource.EndsWith("." + extension))
                                    result.Add(resource.Replace(path, ""));
                            }
                        }
                    }
                }

                return result;
            }
        }

        // Get content as string of the specifiec embedded resource using the specified encoding
        static internal String? GetContentOfEmbeddedResource(String resourceName, Encoding encoding)
        {
            String? result = null;
            Stream? stream = GetStreamFromEmbeddedResource(resourceName);
            
            if (stream != null)
            {
                stream.Position = 0;
                using (StreamReader reader = new StreamReader(stream, encoding))
                    result = reader.ReadToEnd();

                stream.Close();
                stream.Dispose();
            }

            return result;
        }

        // Get stream of the specified resourceName using "GetResourceFullPath"
        static internal Stream? GetStreamFromEmbeddedResource(String resourceName)
        {
            Stream? ms = null;
            String? resourcePath = GetEmbededResourceFullPath(resourceName);

            if (resourcePath == null)
                return null;

            Stream? streamResourceInfo = typeof(BotVideoCompositor.Util).Assembly.GetManifestResourceStream(resourcePath);

            if (streamResourceInfo != null)
            {
                // Go to the beginning of the stream
                streamResourceInfo.Position = 0;

                // Create Memory Stream
                ms = new MemoryStream();

                // Copy stream
                streamResourceInfo.CopyTo(ms);

                // Dispose older Stream
                streamResourceInfo.Dispose();

                // Go to the beginning of the new Memory Stream
                ms.Position = 0;
            }

            return ms;
        }

        // Look from all resources to find one endings with "resourceName" specified in parameter
        static internal String? GetEmbededResourceFullPath(String resourceName)
        {
            InitResourcesList();

            if (resources == null)
                return null;

            if (resourceName.StartsWith(NamespaceResources))
            {
                foreach (String resource in resources)
                {
                    if (resource == resourceName)
                        return resource;
                }
            }
            else
            {
                foreach (String resource in resources)
                {
                    if (resource.EndsWith("." + resourceName))
                        return resource;
                }
            }
            return null;
        }

        // Get resources from assembly
        static internal void InitResourcesList()
        {
            if (resources == null && !resourcesInit)
            {
                // Avoid to do it more than once
                resourcesInit = true;

                String[] rsc = typeof(BotVideoCompositor.Util).Assembly.GetManifestResourceNames();
                if ((rsc != null) && (rsc.Length > 0))
                {
                    // Now ensure to get ONLY wanted resources

                    resources = new List<String>();
                    foreach (String resource in rsc)
                    {
                        if (resource.StartsWith(NamespaceResources))
                            resources.Add(resource);
                    }
                }
            }
        }

        static internal void WriteToConsole(String message, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null)
        {
            if (String.IsNullOrEmpty(message))
                return;

            lock (consoleLockObject)
            {
                if (foregroundColor != null)
                    Console.ForegroundColor = foregroundColor.Value;

                if (backgroundColor != null)
                    Console.BackgroundColor = backgroundColor.Value;

                Console.WriteLine(message);
                Console.ResetColor();
            }
        }
    }
}