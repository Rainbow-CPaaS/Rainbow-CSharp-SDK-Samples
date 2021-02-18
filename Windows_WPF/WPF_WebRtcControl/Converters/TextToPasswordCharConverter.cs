using System;
using System.Globalization;
using System.Windows.Data;

namespace SDK.WpfApp.Converters
{
    public class TextToPasswordCharConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new String('*', value?.ToString().Length ?? 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
