using System;
using System.Globalization;
using System.Windows.Data;
using DVHAnalysis;

namespace UMRO.DvhAnalysis.Script.Presentation.Converters
{
    public class MetricResultConverter : IValueConverter
    {
        public object Convert_noUnit(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            MetricResult metricResult = value as MetricResult;
            string metricString = string.Empty;

            if (metricResult == null || metricResult.Unit == MetricUnit.Invalid)
            {
                return metricString;
            }

            metricString += string.Format("{0:F2}", metricResult.Value); 

            return metricString;
        }

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            MetricResult metricResult = value as MetricResult;
            string metricString = string.Empty;

            if (metricResult == null || metricResult.Unit == MetricUnit.Invalid)
            {
                return metricString;
            }

            switch (metricResult.Unit)
            {
                case MetricUnit.Gy:
                case MetricUnit.cGy:
                case MetricUnit.cc:
                    metricString += string.Format("{0:F2} {1}",
                        metricResult.Value, metricResult.Unit.ToString());
                    break;

                case MetricUnit.Percent:
                    metricString += string.Format("{0:F2}%", metricResult.Value);
                    break;

                case MetricUnit.Fraction:
                    metricString += string.Format("{0:F2}%", metricResult.Value * 100.0);
                    break;

                default:
                    metricString += string.Format("{0:F2}", metricResult.Value);
                    break;
                    //throw new ApplicationException("Unknown metric unit.");
            }

            if (metricResult.DVHModel is LQBioDoseDVHModel)
            {
                metricString += " (LQ2)";
            }
            else if (metricResult.DVHModel is LQLBioDoseDVHModel)
            {
                metricString += " (LQL2)";
            }

            return metricString;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
