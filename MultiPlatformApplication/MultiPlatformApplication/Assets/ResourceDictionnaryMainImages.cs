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
    public class ResourceDictionnaryMainImages : ResourceDictionary
    {

        private List<String> svgList = new List<string> { "loading", "arrow_back",
                                            "burger-menu", "dialpad", 
                                            "chat", "newsfeed", "bubble", "contacts", "calllog", 
                                            "send", "exclamation", "siren", "problem-alert", "bulb", "chat--resized", "mic", "attach", "more_horiz", "more_vert",
                                            "profil", "phone_mobile", "cloud", "meeting", "settings", "help", "info", "logout"};

        public ResourceDictionnaryMainImages()
        {
            CreateSvgFilesAndAddThemAsImageSource();
        }


        private Boolean CreateSvgFile(String svgResourceName, int size)
        {


            String imageFilePath;
            String imagePoolFolderPath = Helper.GetImagesStorageFolderPath();

            // Get Svg with transparent background color and white color
            SkiaSharp.SKBitmap bitmap = Rainbow.Common.ImageTools.GetBitmapFromSvgResourceName(svgResourceName, size, "#00000000", "#ffffff");
            if (bitmap != null)
            {
                // Save image
                imageFilePath = Path.Combine(imagePoolFolderPath, svgResourceName + "_white.png");
                Rainbow.Common.ImageTools.SaveBitmapToFile(bitmap, imageFilePath);

                // Get Svg with transparent background color and black color
                bitmap = Rainbow.Common.ImageTools.GetBitmapFromSvgResourceName(svgResourceName, size, "#00000000", "#000000");
                if (bitmap != null)
                {
                    // Save image
                    imageFilePath = Path.Combine(imagePoolFolderPath, svgResourceName + "_black.png");
                    Rainbow.Common.ImageTools.SaveBitmapToFile(bitmap, imageFilePath);
                    return true;
                }
            }
            return false;
        }

        public void CreateSvgFilesAndAddThemAsImageSource()
        {
            String imagePoolFolderPath = Helper.GetImagesStorageFolderPath();
            Directory.CreateDirectory(imagePoolFolderPath);

            String name;
            ImageSource imageSource;

            foreach (String svgResourceName in svgList)
            {
                // Create SVG Files
                if (CreateSvgFile(svgResourceName, 36))
                {
                    // Add ImageSource to the resource dictionnary
                    name = svgResourceName + "_white";
                    imageSource = ImageSource.FromFile(Path.Combine(imagePoolFolderPath, name + ".png"));
                    Add("MainImage_" + name, imageSource);

                    name = svgResourceName + "_black";
                    imageSource = ImageSource.FromFile(Path.Combine(imagePoolFolderPath, name + ".png"));
                    Add("MainImage_" + name, imageSource);
                }
            }
        }
    }

    

}
