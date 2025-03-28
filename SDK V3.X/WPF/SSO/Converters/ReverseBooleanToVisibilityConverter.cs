using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfSSOSamples.Converters
{
    public class ReverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility result = Visibility.Visible;
            if (value is Boolean)
            {
                Boolean b = (Boolean)value;
                result = b ? Visibility.Collapsed : Visibility.Visible;
            }
            else if (value == null)
                result = Visibility.Visible;

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
