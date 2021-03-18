using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Resources;

namespace SDK.WpfApp
{
    public class WpfUtil
    {

        private static String[] resources = null;

        public static String GetContentFromResource(String resourceName)
        {
            return GetStringFromStream(GetStreamFromResource(resourceName));
        }

        static public String GetStringFromStream(Stream stream)
        {
            String result = null;
            if (stream != null)
            {
                StreamReader reader = new StreamReader(stream);
                result = reader.ReadToEnd();
                reader.Dispose();
            }
            return result;
        }

        static internal Stream GetStreamFromResource(String resourceName)
        {
            Stream ms = null;
            String resourcePath = GetResourceFullPath(resourceName);

            if (resourcePath == null)
                return null;


            Stream streamResourceInfo = System.Reflection.Assembly.GetCallingAssembly().GetManifestResourceStream(resourcePath);

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


        static internal String GetResourceFullPath(String resourceName)
        {
            if (resources == null)
                resources = System.Reflection.Assembly.GetCallingAssembly().GetManifestResourceNames();

            if (resources == null)
                return null;

            if (resources.Length == 0)
                return null;

            foreach (String resource in resources)
            {
                if (resource.EndsWith("." + resourceName))
                    return resource;
            }
            return null;
        }

    }
}
