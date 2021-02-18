using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SDK.WpfApp.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility result = Visibility.Visible;
            if(value is Boolean)
            {
                Boolean b = (Boolean)value;
                result = b ? Visibility.Visible : Visibility.Collapsed;
            }
            else if(value == null)
                result = Visibility.Collapsed;

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
