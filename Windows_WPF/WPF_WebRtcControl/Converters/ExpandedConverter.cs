using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace SDK.WpfApp.Converters
{
   public class ExpandedConverter : IValueConverter
    {
        static string lastValue = "";

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Boolean result = false;

            if ((string)value == (string)parameter)
            {
                if (lastValue == (string)parameter)
                {
                    result = false;
                    lastValue = "";
                }
                else
                {
                    result = true;
                    lastValue = (string)parameter;
                }
                
            }
            else
                lastValue = (string)parameter;

            return result;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return parameter;
        }
    }

}
