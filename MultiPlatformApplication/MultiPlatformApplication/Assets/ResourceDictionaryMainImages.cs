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

        private List<String> svgList = new List<string> { "loading", "arrow_back", "sort", "filter-list",
                                            "burger-menu", "dialpad", 
                                            "chat", "newsfeed", "bubble", "contacts", "calllog", 
                                            "send", "exclamation", "siren", "problem-alert", "bulb", "chat--resized", "mic", "attach", "more_horiz", "more_vert",
                                            "profil", "phone_mobile", "cloud", "meeting", "settings", "help", "info", "logout"};

        private List<String> svgFileTypeList = new List<string> { "ai-type", "audio-type", "archive-type", "code-type", "document-type", 
                                                    "excel-type", "image-type", "pdf-type", "powerpoint-type", "psd-type", "txt-type", 
                                                    "unknown-type", "video-type" };

        public ResourceDictionaryMainImages()
        {
            CreateBlackAndWhiteSvgFilesAndAddThemAsImageSource(false);

            CreateUrgencyFiles();

            CreateFileTypes();
        }

        private void CreateFileTypes()
        {
            int size = 60;
            String imageFilePath;
            String imagePoolFolderPath = Helper.GetImagesStorageFolderPath();

            foreach (String svgResourceName in svgFileTypeList)
            {
                imageFilePath = Path.Combine(imagePoolFolderPath, svgResourceName + ".png");
                if(CreateImageFile(imageFilePath, svgResourceName, size, "#00000000", null))
                {
                    ImageSource imageSource = ImageSource.FromFile(imageFilePath);
                    Add("MainImage_" + svgResourceName, imageSource);
                }
            }
        }

        private void CreateUrgencyFiles()
        {
            // TODO: need to update code when Theme switching will be supported
            ThemeLight themeLight = new ThemeLight();

            // Emergency:
            CreateUrgencyFile(themeLight, "ColorUrgencyEmergency", "siren");

            CreateUrgencyFile(themeLight, "ColorUrgencyImportant", "problem-alert");

            CreateUrgencyFile(themeLight, "ColorUrgencyInformation", "bulb");
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

        private Boolean CreateUrgencyFile(ResourceDictionary resourceDictionary, String colorKey, String svgResourceName)
        {
            if (resourceDictionary.ContainsKey(colorKey))
            {
                Color color = (Color)resourceDictionary[colorKey];
                String imagePoolFolderPath = Helper.GetImagesStorageFolderPath();

                String imageFilePath = Path.Combine(imagePoolFolderPath, svgResourceName + ".png");
                if (CreateImageFile(imageFilePath, svgResourceName, 36, "#00000000", color.ToHex()))
                {
                    ImageSource imageSource = ImageSource.FromFile(imageFilePath);
                    Add("MainImage_" + svgResourceName, imageSource);
                    return true;
                }
            }
            return false;
        }
    }

}
