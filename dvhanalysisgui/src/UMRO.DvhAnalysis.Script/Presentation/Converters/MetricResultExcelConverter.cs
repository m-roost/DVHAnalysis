using System;
using System.Globalization;
using System.Windows.Data;
using DVHAnalysis;

namespace UMRO.DvhAnalysis.Script.Presentation.Converters
{
    public class MetricResultExcelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            MetricResult metricResult = value as MetricResult;

            if (metricResult == null || metricResult.Unit == MetricUnit.Invalid)
            {
                return string.Empty;
            }
            else if (metricResult.Unit == MetricUnit.Fraction)
            {
                return string.Format("{0:F2}", metricResult.Value * 100.00);
            }
            else
            {
                return string.Format("{0:F2}", metricResult.Value);
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
