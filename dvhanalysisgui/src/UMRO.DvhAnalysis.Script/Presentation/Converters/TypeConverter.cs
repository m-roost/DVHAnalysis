using System;
using System.Globalization;
using System.Windows.Data;

namespace UMRO.DvhAnalysis.Script.Presentation.Converters
{
    // Converts an object's type to a string,
    // used to test for type in WPF
    public class TypeConverter : IValueConverter
    {

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return (value != null) ? value.GetType() : null;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
