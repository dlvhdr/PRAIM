using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Common.Converters
{
    /// <summary>
    /// Returns true if value == null and false otherwise.
    /// Invert parameter inverts the result.
    /// </summary>
    public class NullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool invert;
            bool invert_valid = bool.TryParse(parameter as string, out invert);

            if (!invert_valid) {
                return (value == null);
            }

            return (invert)? !(value == null) : value == null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
