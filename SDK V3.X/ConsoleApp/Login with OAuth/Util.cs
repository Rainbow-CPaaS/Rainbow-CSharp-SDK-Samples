using System.Text;

namespace Rainbow.Console
{
    /// <summary>
    /// Static class with some utility methods
    /// </summary>
    public class Util
    {
        internal static readonly String NamespaceResources = "ConsoleWebRTC.Resources";

        private static Boolean resourcesInit = false;
        private static List<String>? resources = null;

        private static readonly Object consoleLockObject = new ();


#region CONSOLE UTILITY METHODS

        public static void WriteDateTime()
        {
            WriteDarkYellow($"[{DateTime.Now:HH:mm:ss.fff}]");
        }

        // Yellow => To display information with user's interactions
        public static void WriteYellow(String message)
        {
            WriteToConsole(message, ConsoleColor.Yellow);
        }

        // DarkYellow => To confirm user choice
        public static void WriteDarkYellow(String message)
        {
            WriteToConsole(message, ConsoleColor.DarkYellow);
        }

        public static void WriteWhite(String message)
        {
            WriteToConsole(message, ConsoleColor.White);
        }

        // Red => Error occurs or major event occurs
        public static void WriteRed(String message)
        {
            WriteToConsole(message, ConsoleColor.Red);
        }

        // WriteGreen => to indicate a process is in progress
        public static void WriteGreen(String message)
        {
            WriteToConsole(message, ConsoleColor.Green);
        }

        // Blue => To display event result
        public static void WriteBlue(String message)
        {
            WriteToConsole(message, ConsoleColor.Blue);
        }

        public static void WriteToConsole(String message, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = ConsoleColor.Black)
        {
            if (message == null)
                return;

            //lock (consoleLockObject)
            {
                if (foregroundColor != null)
                    System.Console.ForegroundColor = foregroundColor.Value;

                if (backgroundColor != null)
                    System.Console.BackgroundColor = backgroundColor.Value;

                System.Console.WriteLine(message);
                System.Console.ResetColor();
            }
        }
#endregion CONSOLE UTILITY METHODS

        /// <summary>
        /// Get Stream from the specified file path
        /// </summary>
        /// <param name="pathFile"><see cref="String"/>File path</param>
        /// <returns><see cref="System.IO.Stream"/> - Valid Stream (read only) object if file exists or NULL</returns>
        static public System.IO.Stream? GetStreamFromFile(String pathFile)
        {
            if (File.Exists(pathFile))
                return File.OpenRead(pathFile);
            return null;
        }

        // Get the list of all resources embedded in this package based on the folder path provided (if any)
        static internal List<String>? GetEmbeddedResourcesList(String folderPath = "", String? extension = null)
        {
            InitResourcesList();

            if (String.IsNullOrEmpty(folderPath))
                return resources;
            else
            {
                List<String> result = [];

                folderPath = folderPath.Replace("/", ".");

                if (folderPath.StartsWith('.'))
                    folderPath = folderPath[1..];

                if (folderPath.EndsWith('.'))
                    folderPath = folderPath[..^1];

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
            System.IO.Stream? stream = GetStreamFromEmbeddedResource(resourceName);
            
            if (stream != null)
            {
                stream.Position = 0;
                using (StreamReader reader = new(stream, encoding))
                    result = reader.ReadToEnd();

                stream.Close();
                stream.Dispose();
            }

            return result;
        }

        // Get stream of the specified resourceName using "GetResourceFullPath"
        static internal System.IO.Stream? GetStreamFromEmbeddedResource(String resourceName)
        {
            System.IO.Stream? ms = null;
            String? resourcePath = GetEmbededResourceFullPath(resourceName);

            if (resourcePath == null)
                return null;

            System.IO.Stream? streamResourceInfo = typeof(Util).Assembly.GetManifestResourceStream(resourcePath);

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

                String[] rsc = typeof(Util).Assembly.GetManifestResourceNames();
                if ((rsc != null) && (rsc.Length > 0))
                {
                    // Now ensure to get ONLY wanted resources

                    resources = [];
                    foreach (String resource in rsc)
                    {
                        if (resource.StartsWith(NamespaceResources))
                            resources.Add(resource);
                    }
                }
            }
        }
    }
}