using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Common.Converters
{
    public class NullToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool invert;
            bool invert_valid = bool.TryParse(parameter as string, out invert);

            if (!invert_valid) {
                return (value != null) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (value != null) {
                return (invert) ? Visibility.Collapsed : Visibility.Visible;
            }

            return (invert) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
