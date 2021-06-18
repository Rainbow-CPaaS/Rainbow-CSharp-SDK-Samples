using MultiPlatformApplication.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace MultiPlatformApplication.Converters
{
    public class IdToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ImageSource result = null;
            if (value is String)
            {
                String id = (String)value;

                if (!String.IsNullOrEmpty(id))
                {
                    String filePath = Helper.GetEmbededResourceFullPath(id);
                    if (!String.IsNullOrEmpty(filePath))
                        result = ImageSource.FromResource(filePath, typeof(IdToImageSourceConverter).Assembly);
                    else
                        result = Helper.GetImageSourceFromResourceDictionaryById(App.Current.Resources, id);
                }
            }
           
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }
}
