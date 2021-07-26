using MultiPlatformApplication.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Assets
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ResourceDictionaryMainImages : ResourceDictionary
    {

        private List<String> svgList = new List<string> { "dialpad", "bubble" };

        public ResourceDictionaryMainImages()
        {
            CreateBlackAndWhiteSvgFilesAndAddThemAsImageSource(false);
        }

        private void CreateBlackAndWhiteSvgFilesAndAddThemAsImageSource(bool blackVersion)
        {
            String imagePoolFolderPath = Helper.GetImagesStorageFolderPath();
            Directory.CreateDirectory(imagePoolFolderPath);

            String name;
            ImageSource imageSource;

            foreach (String svgResourceName in svgList)
            {
                // Create SVG Files
                if (CreateBlackAndWhiteSvgFile(svgResourceName, 36, blackVersion))
                {
                    // Add ImageSource to the resource dictionnary
                    name = svgResourceName + "_white";
                    imageSource = ImageSource.FromFile(Path.Combine(imagePoolFolderPath, name + ".png"));
                    Add("MainImage_" + name, imageSource);

                    if (blackVersion)
                    {
                        name = svgResourceName + "_black";
                        imageSource = ImageSource.FromFile(Path.Combine(imagePoolFolderPath, name + ".png"));
                        Add("MainImage_" + name, imageSource);
                    }
                }
            }
        }

        private Boolean CreateBlackAndWhiteSvgFile(String svgResourceName, int size, bool blackVersion)
        {
            String imageFilePath;
            String imagePoolFolderPath = Helper.GetImagesStorageFolderPath();

            Boolean result;

            // White version
            imageFilePath = Path.Combine(imagePoolFolderPath, svgResourceName + "_white.png");
            result = CreateImageFile(imageFilePath, svgResourceName, size, "#00000000", "#ffffff");

            if(result)
            {
                if (blackVersion)
                {
                    // Black version
                    imageFilePath = Path.Combine(imagePoolFolderPath, svgResourceName + "_black.png");
                    result = CreateImageFile(imageFilePath, svgResourceName, size, "#00000000", "#000000");
                }
            }
            
            return result;
        }

        private Boolean CreateImageFile(String path, String svgResourceName, int size, String backgroundColor, String color)
        {
            if (!File.Exists(path))
            {
                // Get Svg with transparent background color and white color
                SkiaSharp.SKBitmap bitmap = Rainbow.Common.ImageTools.GetBitmapFromSvgResourceName(svgResourceName, size, backgroundColor, color);
                if (bitmap != null)
                    return Rainbow.Common.ImageTools.SaveBitmapToFile(bitmap, path);
                else
                    return false;
            }
            return true;
        }
        
    }

}
