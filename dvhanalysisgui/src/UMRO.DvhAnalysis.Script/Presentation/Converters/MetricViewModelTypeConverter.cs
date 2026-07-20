using System;
using System.Globalization;
using System.Windows.Data;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Metrics;

namespace UMRO.DvhAnalysis.Script.Presentation.Converters
{
    // Converts an metric type to a string,
    // used to group metrics in metric lists
    public class MetricViewModelTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value is MinDoseMetricViewModel ||
                value is MaxDoseMetricViewModel ||
                value is MeanDoseMetricViewModel ||
                value is StdDevDoseMetricViewModel)
            {
                return "General";
            }
            else if (value is DoseToVolumeMetricViewModel)
            {
                return "Dose to Volume";
            }
            else if (value is VolumeWithDoseMetricViewModel)
            {
                return "Volume with Dose";
            }
            else if (value is ColdVolumeWithDoseMetricViewModel)
            {
                return "Cold Volume with Dose";
            }
            else if (value is DoseComplementToVolumeMetricViewModel)
            {
                return "Dose Complement to Volume";
            }
            else if (value is NTCPMetricViewModel)
            {
                return "NTCP";
            }
            else if (value is EUDMetricViewModel)
            {
                return "gEUD";
            }
            else if (value is SMPCMetricViewModel)
            {
                return "Point Dose";
            }
            else
            {
                return "Unknown";
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
