using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BotVideoOrchestratorAndBroadcaster
{
    /// <summary>
    /// Static class with some utility methods
    /// </summary>
    public class Util
    {
        internal static readonly String NamespaceResources = "BotVideoOrchestratorAndBroadcaster.Resources";

        private static Boolean resourcesInit = false;
        private static List<String>? resources = null;

        private static Object consoleLockObject = new Object();

        // To write error to console
        static public void WriteErrorToConsole(String message)
        {
            WriteToConsole(message, foregroundColor: ConsoleColor.Red);
        }

        // To write warning to console
        static public void WriteWarningToConsole(String message)
        {
            WriteToConsole(message, foregroundColor: ConsoleColor.Green);
        }

        // To write debug to console
        static public void WriteDebugToConsole(String message)
        {
            WriteToConsole(message, foregroundColor: ConsoleColor.Blue);
        }

        // To write info to console
        static public void WriteInfoToConsole(String message)
        {
            WriteToConsole(message);
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

        static internal void WriteToConsole(String message, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null)
        {
            if (String.IsNullOrEmpty(message))
                return;

            
            ConsoleColor color = new ConsoleColor();

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

            Stream? streamResourceInfo = typeof(BotVideoOrchestratorAndBroadcaster.Util).Assembly.GetManifestResourceStream(resourcePath);

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

                String[] rsc = typeof(BotVideoOrchestratorAndBroadcaster.Util).Assembly.GetManifestResourceNames();
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
    }
}