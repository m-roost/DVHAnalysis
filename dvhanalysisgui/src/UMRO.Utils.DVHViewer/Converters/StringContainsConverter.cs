using System;
using System.Globalization;
using System.Windows.Data;

namespace UMRO.Utils.DVHViewer.Converters
{
    internal class StringContainsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value as string;
            string value2 = parameter as string;
            return text.Contains(value2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
