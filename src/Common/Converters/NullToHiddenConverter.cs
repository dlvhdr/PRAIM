using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Common.Converters
{
    public class NullToHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool invert;
            bool invert_valid = bool.TryParse(parameter as string, out invert);

            if (!invert_valid) {
                return (value != null) ? Visibility.Visible : Visibility.Hidden;
            }

            if (value != null) {
                return (invert) ? Visibility.Hidden : Visibility.Visible;
            }

            return (invert) ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
