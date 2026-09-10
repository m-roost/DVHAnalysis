using DVHAnalysis;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels.Metrics
{
    public class ColdVolumeWithDoseMetricViewModel : MetricViewModel
    {
        public double Dose
        {
            get { return (Metric as ColdVolumeWithDoseMetric).Dose; }
            set
            {
                (Metric as ColdVolumeWithDoseMetric).Dose = value;
                NotifyPropertyChanged("Dose");
            }
        }
    }
}
