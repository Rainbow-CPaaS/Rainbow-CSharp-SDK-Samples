using System;
using System.IO;
using System.Globalization;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

using Rainbow;
using Rainbow.Model;

using NLog;


namespace WpfSSOSamples.Helpers
{
    public static class Helper
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(Helper));

        public static String GetTempFolder()
        {
            return System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
        }

        public static BitmapFrame GetBitmapFrameFromResource(String resourceName)
        {
            BitmapFrame bitmapFrame = null;
            Stream stream = GetMemoryStreamFromResource(resourceName);
            if (stream != null)
            {
                bitmapFrame = BitmapFrame.Create(stream);
                //stream.Dispose();
            }
            return bitmapFrame;
        }

        public static BitmapImage GetBitmapImageFromStream(Stream stream)
        {
            BitmapImage bitmapImage = null;
            if (stream != null)
            {
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream();
                stream.CopyTo(bitmapImage.StreamSource);
                bitmapImage.StreamSource.Position = 0;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        public static BitmapImage GetBitmapImageFromResource(String resourceName)
        {

            BitmapImage bitmapImage = null;
            Stream stream = GetMemoryStreamFromResource(resourceName);
            if(stream != null)
            {
                bitmapImage = GetBitmapImageFromStream(stream);
                stream.Dispose();
            }

            return bitmapImage;
        }

        public static Stream GetStreamFromFile(String path)
        {
            Stream result = null;
            if (File.Exists(path))
            {
                using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    result = new MemoryStream();
                    fs.CopyTo(result);
                    result.Position = 0;
                    fs.Close();
                }
            }
            return result;
        }

        public static BitmapImage BitmapImageFromFile(String filePath)
        {
            return GetBitmapImageFromStream(GetStreamFromFile(filePath));
        }

        public static Stream GetMemoryStreamFromResource(String resourceName)
        {
            Stream ms = null;
            StreamResourceInfo streamResourceInfo = System.Windows.Application.GetResourceStream(new Uri($"/Resources/{resourceName}", UriKind.RelativeOrAbsolute));
            
            if( (streamResourceInfo != null)
                && (streamResourceInfo.Stream != null) )
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
