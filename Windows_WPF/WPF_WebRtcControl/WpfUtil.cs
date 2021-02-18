using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Resources;

namespace SDK.WpfApp
{
    public class WpfUtil
    {
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

        static public Stream GetStreamFromResource(String resourceName)
        {
            Stream ms = null;
            StreamResourceInfo streamResourceInfo = System.Windows.Application.GetResourceStream(new Uri($"/Resources/{resourceName}", UriKind.RelativeOrAbsolute));

            if ((streamResourceInfo != null)
                && (streamResourceInfo.Stream != null))
            {
                // Go to the beginning of the stream
                streamResourceInfo.Stream.Position = 0;

                // Create Memory Stream
                ms = new MemoryStream();

                // Copy stream
                streamResourceInfo.Stream.CopyTo(ms);

                // Dispose older Stream
                streamResourceInfo.Stream.Dispose();

                // Go to the beginning of the new Memory Stream
                ms.Position = 0;
                //ms.Seek(0, SeekOrigin.Begin);
            }

            return ms;
        }
    }
}
