using System;
using System.Globalization;
using System.Windows.Data;

namespace UMRO.DvhAnalysis.Script.Presentation.Converters
{
    // Converts an enum to true/false, based on parameter passed
    // (see http://stackoverflow.com/questions/397556/how-to-bind-radiobuttons-to-an-enum)
    public class EnumToBoolConverter : IValueConverter
    {

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
}
